using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Newtonsoft.Json;

using EnouFlowTemplateLib;
using EnouFlowInstanceLib;
using EnouFlowOrgMgmtLib;
using EnouFlowEngine;
using OPAS2Model;
using System.Dynamic;

namespace OPAS2.Api
{
  public class PMApiController : BaseApiController
  {
    private string flowTemplateCode = "PM";

    [HttpPost]
    [Route("api/PM/CreateWithFlowAction/")]
    public IHttpActionResult CreateWithFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      UserDTO userDTO = OrgMgmtDBHelper.getUserDTO(bizObj.currentUserGuid, orgDb);
      PurchaseOrder po = OPAS2ModelDBHelper.getPO((int)bizObj.purchaseOrderId, db);
      var pmGuid = bizObj.guid;
      #endregion

      #region 检查是否为重复提交的单据,如果重复提交则拒绝
      if (OPAS2ModelDBHelper.getPM(pmGuid, db) != null)
      {
        return BadRequest("不能重复提交创建表单 / Cannot create same business document.");
      }
      #endregion

      #region 业务数据: 创建PM与Detail, 回填附件列表
      // 获取部门代码,用于生成表单序列号
      string departmentCode = orgDb.departments.Find(po.departmentId).code;
      string documentNo = new OPAS2ModelDBHelper().generateDocumentSerialNo(
        EnumBizDocumentType.PM,
        OrgCode, // OrgCode
        LocationCode, // Location code
        DateTime.Now.Year.ToString(),
        departmentCode);

      Payment pm = db.payments.Create();
      pm.guid = pmGuid;
      pm.documentNo = documentNo;
      pm.PurchaseOrder = po;
      pm.Vendor = po.Vendor;
      pm.departmentId = po.departmentId;
      pm.mainCurrencyRate = po.mainCurrencyRate;
      pm.CurrencyType = po.CurrencyType;

      AssignBasicFields(pm, bizObj, userDTO);

      decimal totalAmount = 0;
      decimal totalAmountInRMB = 0;
      foreach (var _detail in bizObj.PMDetails)
      {
        var poDtl = db.purchaseOrderDetails.Find((int)_detail.id);
        var pmDtl = db.paymentDetails.Create();
        pmDtl.Payment = pm;
        pmDtl.PurchaseOrderDetail = poDtl;
        pmDtl.lineNo = poDtl.lineNo;
        pmDtl.itemName = poDtl.itemName;
        pmDtl.unitMeasure = poDtl.unitMeasure;
        pmDtl.price = poDtl.price;
        pmDtl.quantity = tryParseToDecimal(_detail.payingQuantity, 0);
        pmDtl.amount = poDtl.price * pmDtl.quantity;
        totalAmount += pmDtl.amount;
        pmDtl.amountInRMB = pmDtl.amount * po.mainCurrencyRate;
        totalAmountInRMB += pmDtl.amountInRMB;
        pmDtl.creator = userDTO.name;
        pmDtl.creatorUserId = userDTO.userId;

        db.paymentDetails.Add(pmDtl);
      }
      pm.totalAmount = totalAmount;
      pm.totalAmountInRMB = totalAmountInRMB;

      db.payments.Add(pm);
      db.SaveChanges(); // 这里需要进行一次保存同步,获取到表单的id给下面附件绑定使用

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == pm.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = pm.paymentId;
        att.bizDocumentType = EnumBizDocumentType.PM;
      });
      db.SaveChanges();

      #endregion

      #region 流程数据: 分别创建FlowActionStart和FlowActionMoveTo
      var flowTemplate = FlowTemplateDBHelper.getFlowTemplate(
        (string)bizObj.flowTemplateGuid);

      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowTemplate.flowTemplateJson);

      FlowInstance flowInstance = null;

      // 创建FlowActionStart
      FlowActionRequest actionStart = FlowInstanceHelper.PostFlowActionStart(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        po.reason, // userMemo
        generateBizDataPayloadJson(pm),  // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowTemplate.flowTemplateId, // flowTemplateId
        flowTemplate.guid, // flowTemplateGuid
        flowTemplate.flowTemplateJson, // flowTemplateJson
        documentNo, // code
        bizObj.currentActivityGuid // currentActivityGuid
      );

      // 处理该FlowActionStart
      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionStart.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }
      else
      {
        flowInstance = flowResult.flowInstance;
        // 回填流程实例的信息到PM记录中
        pm.flowInstanceGuid = flowInstance.guid;
        pm.flowInstanceId = flowInstance.flowInstanceId;
        db.SaveChanges();
      }

      //流程实例已在数据库中建立,获取实例相关信息
      var flowInstanceId = flowInstance.flowInstanceId;
      var flowInstanceGuid = flowInstance.guid;

      // 创建FlowActionMoveTo
      var actionMove = FlowInstanceHelper.PostFlowActionMoveTo(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now.AddSeconds(1), // bizTimeStamp
        po.reason, // userMemo
        generateBizDataPayloadJson(pm),  // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstanceId, //flowInstanceId
        flowInstanceGuid, //flowInstanceGuid
        documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        bizObj.selectedConnectionGuid, //connectionGuid
        flowTemplateDefHelper.getNodesOfConnection(
          (string)bizObj.selectedConnectionGuid)
          .Item2.guid, //nextActivityGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid)
        }
      );

      flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionMove.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }

      #endregion

      #region 继续处理可能自动生成的ActionRequest(可能会持续生成)
      IHttpActionResult resultAuto = processAutoGeneratedActionRequest(
        flowInstanceId);
      if (resultAuto != null) return resultAuto; // 出错,将返回非空值
      #endregion

      #region 根据流程实例状态更新PM的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(pm, flowInstance);
      #endregion

      return Ok(documentNo);
    }

    [HttpPost]
    [Route("api/PM/UpdateAtStartFlowAction/")]
    public IHttpActionResult UpdateAtStartFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      #region 验证任务有效性
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 同步业务数据: 更新GR与Detail, 回填附件列表
      Payment pm = db.payments.Find(bizObj.paymentId);
      AssignBasicFields(pm, bizObj, userDTO);

      decimal totalAmount = 0;
      decimal totalAmountInRMB = 0;
      foreach (var _detail in bizObj.PMDetails)
      {
        var pmDtl = db.paymentDetails.Find((int)_detail.id);
        pmDtl.quantity = tryParseToDecimal(_detail.quantity, 0);
        pmDtl.amount = pmDtl.price * pmDtl.quantity;
        pmDtl.amountInRMB = pmDtl.amount * pm.PurchaseOrder.mainCurrencyRate;
        totalAmount += pmDtl.amount;
        totalAmountInRMB += pmDtl.amountInRMB;
      }
      pm.totalAmount = totalAmount;
      pm.totalAmountInRMB = totalAmountInRMB;

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == pm.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = pm.paymentId;
        att.bizDocumentType = EnumBizDocumentType.PM;
      });
      db.SaveChanges();

      #endregion

      #region 流程数据: 创建FlowActionMoveTo并处理
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var flowInstanceId = flowInstance.flowInstanceId;
      var flowInstanceGuid = flowInstance.guid;

      // 创建FlowActionMoveTo
      var actionMove = FlowInstanceHelper.PostFlowActionMoveTo(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now.AddSeconds(1), // bizTimeStamp
        pm.reason, // userMemo
        generateBizDataPayloadJson(pm),  // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstanceId, //flowInstanceId
        flowInstanceGuid, //flowInstanceGuid
        pm.documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        bizObj.selectedConnectionGuid, //connectionGuid
        flowTemplateDefHelper.getNodesOfConnection(
          (string)bizObj.selectedConnectionGuid)
          .Item2.guid, //nextActivityGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid)
        }
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionMove.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }

      #endregion

      #region 继续处理可能自动生成的ActionRequest
      IHttpActionResult resultAuto = processAutoGeneratedActionRequest(
        flowInstanceId);
      if (resultAuto != null) return resultAuto; // 出错,将返回非空值
      #endregion

      #region 根据流程实例状态更新PM的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(pm, flowInstance);
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PM/ExamineFlowAction/")]
    public IHttpActionResult ExamineFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      UserDTO userDTO = OrgMgmtDBHelper.getUserDTO(bizObj.currentUserGuid, orgDb);
      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        (int)bizObj.flowInstanceId, flowInstDb);
      FlowTaskForUser flowTaskForUser = FlowInstanceHelper.GetFlowTaskForUser(
        (string)(bizObj.taskGuid), flowInstDb);
      #endregion

      #region 如果bizTimeStamp已过期,则返回错误
      if (flowTaskForUser.bizTimeStamp < flowInstance.bizTimeStamp)
      {
        return BadRequest("该审批任务已过期 / This flow task is already expired.");
      }
      #endregion

      #region 检查任务状态是否可以进行处理(避免反复提交和任务被已用户自行删除的场景)
      if (!flowTaskForUser.isValidToProcess())
      {
        return BadRequest("该审批任务状态已不能被处理 / This flow task state is not availabel for processing.");
      }
      #endregion

      #region 业务数据: 更新增加GR的remark字段内容
      Payment pm;

      pm = db.payments.Find(bizObj.paymentId);
      if (!string.IsNullOrWhiteSpace(bizObj.remarkOfAprrover))
        pm.remarkOfAprrovers = pm.remarkOfAprrovers + Environment.NewLine +
            bizObj.remarkOfAprrover + " - [" + userDTO.name + "/" +
            userDTO.englishName + "] " + DateTime.Now.ToString();
      db.SaveChanges();

      decimal totalAmountInRMB = 0;
      pm.details.ForEach(detail => { totalAmountInRMB += detail.amountInRMB; });
      #endregion

      #region 流程数据操作: 创建FlowActionMoveTo, 更改任务状态
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionMove = FlowInstanceHelper.PostFlowActionMoveTo(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        generateBizDataPayloadJson(pm),  // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        pm.documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        bizObj.selectedConnectionGuid, //connectionGuid
        flowTemplateDefHelper.getNodesOfConnection(
          (string)bizObj.selectedConnectionGuid)
          .Item2.guid, //nextActivityGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid)
        }
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionMove.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }

      #region 继续处理可能自动生成的ActionRequest
      IHttpActionResult resultAuto = processAutoGeneratedActionRequest(
        flowInstance.flowInstanceId);
      if (resultAuto != null) return resultAuto; // 出错,将返回非空值
      #endregion

      #region 根据流程实例状态更新PR的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(pm, flowInstance);
      #endregion

      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PM/RejectToStartFlowAction/")]
    public IHttpActionResult RejectToStartFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }

      #region 业务数据: 更新增加PM的remark字段内容
      Payment pm = db.payments.Find(bizObj.paymentId);
      appendRemarkOfAprroversOfDocument(bizObj, userDTO, pm);
      #endregion

      #region 流程数据操作: 创建FlowActionRejectToStart, 更改任务状态
      Tuple<bool, IHttpActionResult> generalResult =
        RejectToStartFlowActionGeneral(bizObj, flowTemplateCode,
        flowInstance, pm, userDTO);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PM/InviteOtherFlowAction/")]
    public IHttpActionResult InviteOtherFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      Payment pm = db.payments.Find(bizObj.paymentId);

      return InviteOtherFlowActionGeneral(
        bizObj, flowTemplateCode, pm.documentNo);
    }

    [HttpPost]
    [Route("api/PM/InviteOtherFeedbackFlowAction/")]
    public IHttpActionResult InviteOtherFeedbackFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      Payment pm = db.payments.Find(bizObj.paymentId);

      return InviteOtherFeedbackFlowActionGeneral(
        bizObj, flowTemplateCode, pm.documentNo);
    }

    private void AssignBasicFields(
      Payment pm, dynamic bizObj, UserDTO userDTO)
    {
      pm.vendorBankName = bizObj.vendorBankName;
      pm.vendorBankAccount = bizObj.vendorBankAccount;
      pm.SWIFTCode = bizObj.SWIFTCode;
      pm.applicantName = bizObj.applicantName;
      pm.applicantEmail = bizObj.applicantEmail;
      pm.applicantPhone = bizObj.applicantPhone;
      pm.paymentAreaType = (EnumPaymentAreaType)(int)
        bizObj.paymentAreaType;
      pm.paymentCurrencyType = (EnumPaymentCurrencyType)(int)
        bizObj.paymentCurrencyType;
      pm.paymentMethodType = (EnumPaymentMethodType)(int)
        bizObj.paymentMethodType;
      pm.payingDaysRequirement = (int)bizObj.payingDaysRequirement;
      pm.invoiceNo = bizObj.invoiceNo;
      pm.isDownPayment = bizObj.isDownPayment;
      pm.isNormalPayment = bizObj.isNormalPayment;
      pm.isImmediatePayment = bizObj.isImmediatePayment;
      pm.isAdvancePayment = bizObj.isAdvancePayment;
      pm.reason = bizObj.reason;
      pm.description = bizObj.description;
      pm.submitTime = DateTime.Now;
      pm.submitor = userDTO.name;
      pm.submitorUserId = userDTO.userId;
      pm.creator = userDTO.name;
      pm.creatorUserId = userDTO.userId;
    }

    private string generateBizDataPayloadJson(Payment pm)
    {
      dynamic bizDataPayload = new ExpandoObject();
      bizDataPayload.Document = pm;
      bizDataPayload.AmountTotal =
        pm.details.Aggregate<PaymentDetail, decimal>(
          0, (total, detail) =>
          {
            return total + detail.amountInRMB;
          });
      return JsonConvert.SerializeObject(bizDataPayload, Formatting.Indented,
        new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }
  }
}
