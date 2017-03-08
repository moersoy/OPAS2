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
  public class PRApiController : BaseApiController
  {
    private string flowTemplateCode = "PR";

    [HttpPost]
    [Route("api/PR/CreateWithFlowAction/")]
    public IHttpActionResult CreateWithFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      UserDTO userDTO = OrgMgmtDBHelper.getUserDTO(bizObj.currentUserGuid, orgDb);
      var prGuid = bizObj.guid;
      #endregion

      #region 检查是否为重复提交的单据,如果重复提交则拒绝
      if (OPAS2ModelDBHelper.getPR(prGuid,db)!=null)
      {
        return BadRequest("不能重复提交创建表单 / Cannot create same business document.");
      }
      #endregion

      #region 业务数据: 创建PR与Detail, 回填附件列表
      // 获取部门代码,用于生成表单序列号
      string departmentCode = orgDb.departments.Find((int)bizObj.departmentId)?.code;
      string documentNo = new OPAS2ModelDBHelper().generateDocumentSerialNo(
        EnumBizDocumentType.PR,
        OrgCode, // OrgCode
        LocationCode, // Location code
        DateTime.Now.Year.ToString(),
        departmentCode);

      PurchaseReq pr = db.purchaseReqs.Create();
      pr.guid = bizObj.guid;
      pr.documentNo = documentNo;
      AssignBasicFields(pr, bizObj, userDTO);
      foreach (var _detail in bizObj.PRDetails)
      {
        var prDtl = db.purchaseReqDetails.Create();
        prDtl.PurchaseReq = pr;
        prDtl.estimatedCost = tryParseToDecimal(_detail.estimatedCost, 0);
        prDtl.lineNo = (int)_detail.lineNo;
        prDtl.itemName = _detail.itemName;
        prDtl.itemType = (EnumPRItemType)_detail.itemType;
        prDtl.description = _detail.description;
        prDtl.creator = userDTO.name;
        prDtl.creatorUserId = userDTO.userId;
        db.purchaseReqDetails.Add(prDtl);
      }
      db.purchaseReqs.Add(pr);

      db.SaveChanges();

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == pr.guid)
      .ToList().ForEach(att =>
        {
          att.bizDocumentId = pr.purchaseReqId;
          att.bizDocumentType = EnumBizDocumentType.PR;
        });
      db.SaveChanges();

      #endregion

      #region 流程数据: 分别创建FlowActionStart和FlowActionMoveTo
      var pRFlowTemplate = FlowTemplateDBHelper.getFlowTemplate(
        (string)bizObj.flowTemplateGuid);

      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        pRFlowTemplate.flowTemplateJson);

      FlowInstance flowInstance = null;

      // 创建FlowActionStart
      FlowActionRequest actionStart = FlowInstanceHelper.PostFlowActionStart(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        bizObj.reason, // userMemo
        generateBizDataPayloadJson(pr), // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        pRFlowTemplate.flowTemplateId, // flowTemplateId
        pRFlowTemplate.guid, // flowTemplateGuid
        pRFlowTemplate.flowTemplateJson, // flowTemplateJson
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
        // 回填流程实例的信息到PR记录中
        pr.flowInstanceGuid = flowInstance.guid;
        pr.flowInstanceId = flowInstance.flowInstanceId;
        db.SaveChanges();
      }
      var flowInstanceId = flowInstance.flowInstanceId;
      var flowInstanceGuid = flowInstance.guid;

      // 创建FlowActionMoveTo
      var actionMove = FlowInstanceHelper.PostFlowActionMoveTo(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now.AddSeconds(1), // bizTimeStamp
        bizObj.reason, // userMemo
        generateBizDataPayloadJson(pr), // bizDataPayloadJson
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

      #region 继续处理可能自动生成的ActionRequest
      IHttpActionResult resultAuto = processAutoGeneratedActionRequest(
        flowInstanceId);
      if (resultAuto != null) return resultAuto; // 出错,将返回非空值
      #endregion

      #region 根据流程实例状态更新PR的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(pr, flowInstance);
      #endregion

      return Ok(documentNo);
    }

    [HttpPost]
    [Route("api/PR/UpdateAtStartFlowAction/")]
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

      #region 同步业务数据: 更新PR与Detail, 回填附件列表
      PurchaseReq pr = db.purchaseReqs.Find(bizObj.purchaseReqId);
      AssignBasicFields(pr, bizObj, userDTO);

#warning TODO 被用户删除的detail record需要进行标记
      foreach (var _detail in bizObj.PRDetails)
      {
        if ((int)_detail.id == 0) { //新加入的detail record
          var prDtl = db.purchaseReqDetails.Create();
          prDtl.PurchaseReq = pr;
          prDtl.estimatedCost = tryParseToDecimal(_detail.estimatedCost, 0);
          prDtl.lineNo = (int)_detail.lineNo;
          prDtl.itemName = _detail.itemName;
          prDtl.itemType = (EnumPRItemType)_detail.itemType;
          prDtl.description = _detail.description;
          prDtl.creator = userDTO.name;
          prDtl.creatorUserId = userDTO.userId;
          db.purchaseReqDetails.Add(prDtl);
        }
        else // 属于可能被修改detail record, 只需要同步更新业务数据部分
        {
          var prDtl = db.purchaseReqDetails.Find((int)_detail.id);
          prDtl.estimatedCost = tryParseToDecimal(_detail.estimatedCost, 0);
          prDtl.itemName = _detail.itemName;
          prDtl.itemType = (EnumPRItemType)_detail.itemType;
          prDtl.description = _detail.description;
        }
      }

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == pr.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = pr.purchaseReqId;
        att.bizDocumentType = EnumBizDocumentType.PR;
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
        bizObj.reason, // userMemo
        generateBizDataPayloadJson(pr), // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstanceId, //flowInstanceId
        flowInstanceGuid, //flowInstanceGuid
        pr.documentNo, //code
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

      #region 根据流程实例状态更新PR的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(pr, flowInstance);
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PR/ExamineFlowAction/")]
    public IHttpActionResult ExamineFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      #region 任务失效则退出
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 业务数据: 更新增加PR的remark字段内容
      PurchaseReq pr = db.purchaseReqs.Find(bizObj.purchaseReqId);
      appendRemarkOfAprroversOfDocument(bizObj, userDTO, pr);
      #endregion

      #region 流程数据操作: 创建FlowActionMoveTo并处理, 更改任务状态
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionMove = FlowInstanceHelper.PostFlowActionMoveTo(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        generateBizDataPayloadJson(pr), // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        pr.documentNo, //code
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
      UpdateDocumentStateFields(pr, flowInstance);
      #endregion

      #endregion

      return Ok("成功提交 / Sucessfully submitted.");
    }

    [HttpPost]
    [Route("api/PR/RejectToStartFlowAction/")]
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

      #region 业务数据: 更新增加PR的remark字段内容
      PurchaseReq pr = db.purchaseReqs.Find(bizObj.purchaseReqId);
      appendRemarkOfAprroversOfDocument(bizObj, userDTO, pr);
      #endregion

      #region 流程数据操作: 创建FlowActionRejectToStart, 更改任务状态
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionRejectToStart = FlowInstanceHelper.PostFlowActionRejectToStart(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        flowInstance.bizDataPayloadJson, // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        pr.documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        null, //startActivityGuid
        new List<Paticipant>() { //roles
        }
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionRejectToStart.flowActionRequestId);
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
      UpdateDocumentStateFields(pr, flowInstance);
      #endregion

      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PR/InviteOtherFlowAction/")]
    public IHttpActionResult InviteOtherFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      PurchaseReq pr = db.purchaseReqs.Find(bizObj.purchaseReqId);

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      #region 任务失效则退出
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 流程数据操作: 创建FlowActionInviteOther
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionInviteOther = FlowInstanceHelper.PostFlowActionInviteOther(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        flowTaskForUser.bizTimeStamp, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        flowInstance.bizDataPayloadJson, // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        pr.documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid) },
        flowTaskForUser.flowTaskForUserId //relativeFlowTaskForUserId
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionInviteOther.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PR/InviteOtherFeedbackFlowAction/")]
    public IHttpActionResult InviteOtherFeedbackFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      PurchaseReq pr = db.purchaseReqs.Find(bizObj.purchaseReqId);

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      #region 任务失效则退出
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 流程数据操作: 创建FlowActionInviteOtherFeedback并处理
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionInviteOtherFeedback = 
        FlowInstanceHelper.PostFlowActionInviteOtherFeedback(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        generateBizDataPayloadJson(pr), // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        pr.documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        bizObj.selectedConnectionGuid, //connectionGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid)
        },
        flowTaskForUser.flowTaskForUserId //relativeFlowTaskForUserId
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionInviteOtherFeedback.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }
      #endregion

      return Ok();
    }

    private void AssignBasicFields(
      PurchaseReq pr, dynamic bizObj, UserDTO userDTO)
    {
      pr.WBSNo = bizObj.WBSNo;
      pr.contactOfficePhone = bizObj.contactOfficePhone;
      pr.contactMobile = bizObj.contactMobile;
      pr.contactOtherMedia = bizObj.contactOtherMedia;
      pr.departmentId = (int)bizObj.departmentId;
      pr.departmentIdBelongTo = (int)bizObj.departmentIdBelongTo;
      pr.costCenterId = (int)bizObj.costCenterId;

      if (hasAttr(bizObj, "expectReceiveBeginTime"))
      {
        pr.expectReceiveBeginTime =
          (DateTime)bizObj.expectReceiveBeginTime;
      }
      if (hasAttr(bizObj, "expectReceiveEndTime"))
      {
        pr.expectReceiveEndTime = (DateTime)bizObj.expectReceiveEndTime;
        //  convertJsonTime((double)bizObj.expectReceiveEndTime);
      }
      pr.isBidingRequired = bizObj.isBidingRequired;
      pr.noBiddingReason = bizObj.noBiddingReason;
      pr.reason = bizObj.reason;
      pr.description = bizObj.description;
#warning TODO: add these PR extra fields
      //pr.estimatedCost = (decimal)bizObj;
      //pr.currencyTypeId = 1;
      //pr.mainCurrencyRate = (decimal)1.0;
      pr.estimatedCostInRMB = tryParseToDecimal(bizObj.estimatedCostInRMB, 0);
      pr.averageBenchmark = tryParseToDecimal(bizObj.averageBenchmark, 0);
      pr.benchmarkDescription = bizObj.benchmarkDescription;
      pr.isFirstBuy = bizObj.isFirstBuy;
      if (hasAttr(bizObj, "firstBuyDate"))
      {
        pr.firstBuyDate = bizObj.firstBuyDate;
      }
      pr.firstCostAmount = tryParseToDecimal(bizObj.firstCostAmount, 0);
      pr.firstBuyDescription = bizObj.firstBuyDescription;
      pr.otherVendorsNotInList = bizObj.otherVendorsNotInList;
      pr.submitTime = DateTime.Now;
      pr.submitor = userDTO.name;
      pr.submitorUserId = userDTO.userId;
      pr.creator = userDTO.name;
      pr.creatorUserId = userDTO.userId;
    }

    private string generateBizDataPayloadJson(PurchaseReq pr)
    {
      dynamic bizDataPayload = new ExpandoObject();
      bizDataPayload.Document = pr;
      bizDataPayload.AmountTotal = 
        pr.details.Aggregate<PurchaseReqDetail, decimal>(
          0, (total, detail) =>
          {
            return total + detail.estimatedCost.Value;
          });
      return JsonConvert.SerializeObject(bizDataPayload, Formatting.Indented,
        new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }
  }
}