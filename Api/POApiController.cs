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
  public class POApiController : BaseApiController
  {
    private string flowTemplateCode = "PO";

    [HttpPost]
    [Route("api/PO/CreateWithFlowAction/")]
    public IHttpActionResult CreateWithFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      UserDTO userDTO = OrgMgmtDBHelper.getUserDTO(bizObj.currentUserGuid, orgDb);
      PurchaseReq pr = db.purchaseReqs.Find(bizObj.purchaseReqId);
      var poGuid = bizObj.guid;
      #endregion

      #region 检查是否为重复提交的单据,如果重复提交则拒绝
      if (OPAS2ModelDBHelper.getPO(poGuid, db) != null)
      {
        return BadRequest("不能重复提交创建表单 / Cannot create same business document.");
      }
      #endregion

      #region 业务数据: 创建PO与Detail, 回填附件列表
      // 获取部门代码,用于生成表单序列号
      string departmentCode = orgDb.departments.Find((int)bizObj.departmentId)?.code;
      string documentNo = new OPAS2ModelDBHelper().generateDocumentSerialNo(
        EnumBizDocumentType.PO,
        OrgCode, // OrgCode
        LocationCode, // Location code
        DateTime.Now.Year.ToString(),
        departmentCode);

      PurchaseOrder po = db.purchaseOrders.Create();
      po.guid = bizObj.guid;
      po.documentNo = documentNo;
      po.PurchaseReq = pr;
      AssignBasicFields(po, bizObj, userDTO);

      decimal totalAmount = 0;
      decimal totalAmountInRMB = 0;
      foreach (var _detail in bizObj.PODetails)
      {
        var poDtl = InsertDetailRecord(_detail, po, userDTO);
        totalAmount += poDtl.amount;
        totalAmountInRMB += poDtl.amountInRMB;
      }
      po.totalAmount = totalAmount;
      po.totalAmountInRMB = totalAmountInRMB;

      db.purchaseOrders.Add(po);

      db.SaveChanges();

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == po.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = po.purchaseOrderId;
        att.bizDocumentType = EnumBizDocumentType.PO;
      });
      db.SaveChanges();

      #endregion

      #region 执行公用的CreateWithFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        CreateWithFlowActionGeneral(
          bizObj, po, flowTemplateCode,
          generateBizDataPayloadJson(po),
          generateOptionalFlowActionDataJson(po), 
          userDTO, po.reason);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok(documentNo);
    }

    [HttpPost]
    [Route("api/PO/UpdateAtStartFlowAction/")]
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

      #region 同步业务数据: 更新PO与Detail, 回填附件列表
      PurchaseOrder po = db.purchaseOrders.Find(bizObj.purchaseOrderId);
      AssignBasicFields(po, bizObj, userDTO);

#warning TODO 被用户删除的detail record需要进行标记
      decimal totalAmount = 0;
      decimal totalAmountInRMB = 0;
      foreach (var _detail in bizObj.PODetails)
      {
        if ((int)_detail.id == 0)
        { //新加入的detail record
          var poDtl = InsertDetailRecord(_detail, po, userDTO);
          totalAmount += poDtl.amount;
          totalAmountInRMB += poDtl.amountInRMB;
        }
        else // 属于可能被修改detail record, 只需要同步更新业务数据部分
        {
          var poDtl = db.purchaseOrderDetails.Find((int)_detail.id);
          poDtl.itemName = _detail.itemName;
          poDtl.unitMeasure = _detail.unitMeasure;
          poDtl.price = tryParseToDecimal(_detail.price, 0);
          poDtl.quantity = tryParseToDecimal(_detail.quantity, 0);
          poDtl.amount = poDtl.price * poDtl.quantity;
          poDtl.amountInRMB = poDtl.amount * po.mainCurrencyRate;
          poDtl.description = _detail.description;
          totalAmount += poDtl.amount;
          totalAmountInRMB += poDtl.amountInRMB;
        }
      }
      po.totalAmount = totalAmount;
      po.totalAmountInRMB = totalAmountInRMB;

      // save attachFiles uploaded before the whole document submitted
      db.attachFiles.Where(att => att.bizDocumentGuid == po.guid)
      .ToList().ForEach(att =>
      {
        att.bizDocumentId = po.purchaseOrderId;
        att.bizDocumentType = EnumBizDocumentType.PO;
      });
      db.SaveChanges();

      #endregion

      #region 执行公用的UpdateAtStartFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        UpdateAtStartFlowActionGeneral(
          bizObj, po, flowTemplateCode, flowInstance,
          generateBizDataPayloadJson(po),
          generateOptionalFlowActionDataJson(po),
          userDTO, po.reason);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PO/ExamineFlowAction/")]
    public IHttpActionResult ExamineFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      PurchaseOrder po = db.purchaseOrders.Find(bizObj.purchaseOrderId);

      #region 执行公用的ExamineFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        ExamineFlowActionGeneral(
          bizObj, po, flowTemplateCode,
          generateBizDataPayloadJson(po),
          generateOptionalFlowActionDataJson(po));
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok("成功提交 / Sucessfully submitted.");
    }

    [HttpPost]
    [Route("api/PO/RejectToStartFlowAction/")]
    public IHttpActionResult RejectToStartFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      PurchaseOrder po = db.purchaseOrders.Find(bizObj.purchaseOrderId);
      #endregion

      #region 流程数据操作: 创建FlowActionRejectToStart, 更改任务状态
      Tuple<bool, IHttpActionResult> generalResult =
        RejectToStartFlowActionGeneral(bizObj, flowTemplateCode,
        flowInstance, po, userDTO);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/PO/InviteOtherFlowAction/")]
    public IHttpActionResult InviteOtherFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      PurchaseOrder po = db.purchaseOrders.Find(bizObj.purchaseOrderId);

      return InviteOtherFlowActionGeneral(
        bizObj, flowTemplateCode, po.documentNo);
    }

    [HttpPost]
    [Route("api/PO/InviteOtherFeedbackFlowAction/")]
    public IHttpActionResult InviteOtherFeedbackFlowAction()
    {
      dynamic bizObj = getPostedJsonObject();
      PurchaseOrder po = db.purchaseOrders.Find(bizObj.purchaseOrderId);

      return InviteOtherFeedbackFlowActionGeneral(
        bizObj, flowTemplateCode, po.documentNo);
    }

    private void AssignBasicFields(
      PurchaseOrder po, dynamic bizObj, UserDTO userDTO)
    {
      po.contactOfficePhone = bizObj.contactOfficePhone;
      po.contactMobile = bizObj.contactMobile;
      po.departmentId = (int)bizObj.departmentId;
      po.costCenterId = (int)bizObj.costCenterId;

      if (hasAttr(bizObj, "orderDate"))
      {
        po.orderDate = (DateTime)bizObj.orderDate;
      }
      if (hasAttr(bizObj, "effectiveDate"))
      {
        po.orderDate = (DateTime)bizObj.effectiveDate;
      }
      po.Vendor = db.vendors.Find((int)bizObj.vendorId);
      po.vendorContactPerson = bizObj.vendorContactPerson;
      po.vendorTel = bizObj.vendorTel;
      po.reason = bizObj.reason;
      po.description = bizObj.description;
      po.CurrencyType = db.currencyTypes.Find((int)bizObj.currencyTypeId);
      po.mainCurrencyRate = tryParseToDecimal(
        bizObj.mainCurrencyRate, (decimal)1.0);
      po.paymentTerm = bizObj.paymentTerm;
      po.submitTime = DateTime.Now;
      po.submitor = userDTO.name;
      po.submitorUserId = userDTO.userId;
      po.creator = userDTO.name;
      po.creatorUserId = userDTO.userId;
    }

    private PurchaseOrderDetail InsertDetailRecord(
      dynamic detailData, PurchaseOrder po, UserDTO userDTO)
    {
      var poDtl = db.purchaseOrderDetails.Create();
      poDtl.PurchaseOrder = po;
      poDtl.lineNo = (int)detailData.lineNo;
      poDtl.itemName = detailData.itemName;
      poDtl.unitMeasure = detailData.unitMeasure;
      poDtl.price = tryParseToDecimal(detailData.price, 0);
      poDtl.quantity = tryParseToDecimal(detailData.quantity, 0);
      poDtl.amount = poDtl.price * poDtl.quantity;
      poDtl.amountInRMB = poDtl.amount * po.mainCurrencyRate;
      poDtl.description = detailData.description;
      poDtl.creator = userDTO.name;
      poDtl.creatorUserId = userDTO.userId;
      db.purchaseOrderDetails.Add(poDtl);

      return poDtl;
    }

    private string generateBizDataPayloadJson(PurchaseOrder po)
    {
      dynamic bizDataPayload = new ExpandoObject();
      bizDataPayload.Document = po;
      bizDataPayload.AmountTotal =
        po.details.Aggregate<PurchaseOrderDetail, decimal>(
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

    private string generateOptionalFlowActionDataJson(PurchaseOrder po)
    {
      return null;
    }
  }
}