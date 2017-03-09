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
  public class GRApiController : BaseApiController
  {
    private string flowTemplateCode = "GR";

    [HttpPost]
    [Route("api/GR/CreateWithFlowAction/")]
    public IHttpActionResult CreateWithFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      UserDTO userDTO = OrgMgmtDBHelper.getUserDTO(bizObj.currentUserGuid, orgDb);
      PurchaseOrder po = OPAS2ModelDBHelper.getPO((int)bizObj.purchaseOrderId, db);
      var grGuid = bizObj.guid;
      #endregion

      #region 检查是否为重复提交的单据,如果重复提交则拒绝
      if (OPAS2ModelDBHelper.getGR(grGuid, db) != null)
      {
        return BadRequest("不能重复提交创建表单 / Cannot create same business document.");
      }
      #endregion

      #region 业务数据: 创建GR与Detail, 回填附件列表
      // 获取部门代码,用于生成表单序列号
      string departmentCode = orgDb.departments.Find(po.departmentId).code;
      string documentNo = new OPAS2ModelDBHelper().generateDocumentSerialNo(
        EnumBizDocumentType.GR,
        OrgCode, // OrgCode
        LocationCode, // Location code
        DateTime.Now.Year.ToString(),
        departmentCode);

      GoodsReceiving gr = db.goodsReceivings.Create();
      gr.guid = bizObj.guid;
      gr.documentNo = documentNo;
      gr.PurchaseOrder = po;
      gr.Vendor = po.Vendor;
      gr.departmentId = po.departmentId;

      AssignBasicFields(gr, bizObj, userDTO);

      decimal totalAmount = 0;
      decimal totalAmountInRMB = 0;
      foreach (var _detail in bizObj.GRDetails)
      {
        var poDtl = db.purchaseOrderDetails.Find((int)_detail.id);
        var grDtl = db.goodsReceivingDetails.Create();
        grDtl.GoodsReceiving = gr;
        grDtl.PurchaseOrderDetail = poDtl;
        grDtl.lineNo = poDtl.lineNo;
        grDtl.itemName = poDtl.itemName;
        grDtl.unitMeasure = poDtl.unitMeasure;
        grDtl.price = poDtl.price;
        grDtl.quantity = tryParseToDecimal(_detail.receivingQuantity, 0);
        grDtl.amount = poDtl.price * grDtl.quantity;
        totalAmount += grDtl.amount;
        grDtl.amountInRMB = grDtl.amount * po.mainCurrencyRate;
        totalAmountInRMB += grDtl.amountInRMB;
        grDtl.creator = userDTO.name;
        grDtl.creatorUserId = userDTO.userId;
        db.goodsReceivingDetails.Add(grDtl);
      }

      gr.totalAmount = totalAmount;
      gr.totalAmountInRMB = totalAmountInRMB;
      db.goodsReceivings.Add(gr);
      db.SaveChanges();

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == gr.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = gr.goodsReceivingId;
        att.bizDocumentType = EnumBizDocumentType.GR;
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
        generateBizDataPayloadJson(gr),  // bizDataPayloadJson
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
        // 回填流程实例的信息到GR记录中
        gr.flowInstanceGuid = flowInstance.guid;
        gr.flowInstanceId = flowInstance.flowInstanceId;
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
        po.reason, // userMemo
        generateBizDataPayloadJson(gr),  // bizDataPayloadJson
        null, // optionalFlowActionDataJsonoptionalFlowActionDataJson
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

      #region 根据流程实例状态更新GR的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(gr, flowInstance);
      #endregion

      return Ok(documentNo);
    }

    [HttpPost]
    [Route("api/GR/UpdateAtStartFlowAction/")]
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
      GoodsReceiving gr = db.goodsReceivings.Find(bizObj.goodsReceivingId);
      AssignBasicFields(gr, bizObj, userDTO);

      decimal totalAmount = 0;
      decimal totalAmountInRMB = 0;
      foreach (var _detail in bizObj.GRDetails)
      {
          var grDtl = db.goodsReceivingDetails.Find((int)_detail.id);
          grDtl.quantity = tryParseToDecimal(_detail.quantity, 0);
          grDtl.amount = grDtl.price * grDtl.quantity;
          grDtl.amountInRMB = grDtl.amount * gr.PurchaseOrder.mainCurrencyRate;
          totalAmount += grDtl.amount;
          totalAmountInRMB += grDtl.amountInRMB;
      }
      gr.totalAmount = totalAmount;
      gr.totalAmountInRMB = totalAmountInRMB;

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == gr.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = gr.goodsReceivingId;
        att.bizDocumentType = EnumBizDocumentType.GR;
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
        gr.PurchaseOrder.reason, // userMemo
        generateBizDataPayloadJson(gr),  // bizDataPayloadJson
        null, // optionalFlowActionDataJsonoptionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstanceId, //flowInstanceId
        flowInstanceGuid, //flowInstanceGuid
        gr.documentNo, //code
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

      #region 根据流程实例状态更新GR的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(gr, flowInstance);
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/GR/ExamineFlowAction/")]
    public IHttpActionResult ExamineFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      GoodsReceiving gr = db.goodsReceivings.Find(bizObj.goodsReceivingId);

      #region 执行公用的ExamineFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        ExamineFlowActionGeneral(
          bizObj, gr, flowTemplateCode,
          generateBizDataPayloadJson(gr),
          generateOptionalFlowActionDataJson(gr));
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok("成功提交 / Sucessfully submitted.");
    }

    [HttpPost]
    [Route("api/GR/RejectToStartFlowAction/")]
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

      #region 业务数据: 更新增加PO的remark字段内容
      GoodsReceiving gr = db.goodsReceivings.Find(bizObj.goodsReceivingId);
      appendRemarkOfAprroversOfDocument(bizObj, userDTO, gr);
      #endregion

      #region 流程数据操作: 创建FlowActionRejectToStart, 更改任务状态
      Tuple<bool, IHttpActionResult> generalResult =
        RejectToStartFlowActionGeneral(bizObj, flowTemplateCode,
        flowInstance, gr, userDTO);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/GR/InviteOtherFlowAction/")]
    public IHttpActionResult InviteOtherFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      GoodsReceiving gr = db.goodsReceivings.Find(bizObj.goodsReceivingId);

      return InviteOtherFlowActionGeneral(
        bizObj, flowTemplateCode, gr.documentNo);
    }

    [HttpPost]
    [Route("api/GR/InviteOtherFeedbackFlowAction/")]
    public IHttpActionResult InviteOtherFeedbackFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      GoodsReceiving gr = db.goodsReceivings.Find(bizObj.goodsReceivingId);

      return InviteOtherFeedbackFlowActionGeneral(
        bizObj, flowTemplateCode, gr.documentNo);
    }

    private void AssignBasicFields(
      GoodsReceiving gr, dynamic bizObj, UserDTO userDTO)
    {
      if (hasAttr(bizObj, "deliveryDate"))
      {
        gr.deliveryDate = (DateTime)bizObj.deliveryDate;
      }
      else
      {
        gr.deliveryDate = null;
      }
      gr.receiver = bizObj.receiver;
      gr.checker = bizObj.checker;
      gr.deliveryLocation = bizObj.deliveryLocation;
      gr.shippingInfo = bizObj.shippingInfo;
      gr.trackingNo = bizObj.trackingNo;
      gr.weight = bizObj.weight;
      gr.packingSlipNo = bizObj.packingSlipNo;
      gr.description = bizObj.description;

      gr.submitTime = DateTime.Now;
      gr.submitor = userDTO.name;
      gr.submitorUserId = userDTO.userId;
      gr.creator = userDTO.name;
      gr.creatorUserId = userDTO.userId;
    }

    private string generateBizDataPayloadJson(GoodsReceiving gr)
    {
      dynamic bizDataPayload = new ExpandoObject();
      bizDataPayload.Document = gr;
      bizDataPayload.AmountTotal =
        gr.details.Aggregate<GoodsReceivingDetail, decimal>(
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

    private string generateOptionalFlowActionDataJson(GoodsReceiving gr)
    {
      return null;
    }
  }
}
