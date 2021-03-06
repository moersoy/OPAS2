﻿using System;
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
using OPAS2.Models;
using OPAS2.ApiFilters;
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
      dynamic bizObj = getPostedJsonDocObject();

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

      #region 执行公用的CreateWithFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        CreateWithFlowActionGeneral(
          bizObj, pm, flowTemplateCode,
          generateBizDataPayloadJson(pm),
          generateOptionalFlowActionDataJson(pm),
          userDTO, pm.reason); // 这里沿用PO的采购原因作为收货原因
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok(documentNo);
    }

    [HttpPost]
    [Route("api/PM/UpdateAtStartFlowAction/")]
    public IHttpActionResult UpdateAtStartFlowAction()
    {
      dynamic bizObj = getPostedJsonDocObject();

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

      #region 执行公用的UpdateAtStartFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        UpdateAtStartFlowActionGeneral(
          bizObj, pm, flowTemplateCode, flowInstance,
          generateBizDataPayloadJson(pm),
          generateOptionalFlowActionDataJson(pm),
          userDTO, pm.reason);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok("成功提交 / Sucessfully submitted.");
    }

    [HttpPost]
    [Route("api/PM/ExamineFlowAction/")]
    public IHttpActionResult ExamineFlowAction()
    {
      dynamic bizObj = getPostedJsonDocObject();
      Payment pm = db.payments.Find(bizObj.paymentId);

      #region 执行公用的ExamineFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        ExamineFlowActionGeneral(
          bizObj, pm, flowTemplateCode,
          generateBizDataPayloadJson(pm),
          generateOptionalFlowActionDataJson(pm));
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      if (pm.bizDocumentFlowState == EnumBizDocumentFlowState.StoppedValid)
      { // 收货流程审批完毕,需要更新对应的PO明细表的已收货数量
        pm.details.ForEach(pmDtl => {
          pmDtl.PurchaseOrderDetail.paidQuantity += pmDtl.quantity ;
        });
      }
      db.SaveChanges();

      return Ok("成功提交 / Sucessfully submitted.");
    }

    [UserAuthentication]
    [HttpGet]
    [Route("api/PM/Examine/{id}/{flowTaskForUserId}")]
    public IHttpActionResult ExamineMobile(
  string id, int flowTaskForUserId)
    {
      Payment payment = db.payments.Where(
        pm => pm.guid == id).FirstOrDefault();
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Find(
        flowTaskForUserId);
      FlowInstance flowInstance = flowTaskForUser.FlowInstance;

      #region 检查timestamp是否已过期
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 获取可能的征询处理意见(InviteOther)的任务反馈意见内容
      //ViewBag.inviteOtherFeedbackTasks =
      //  getValidInviteOtherFeedbackTasks(
      //    flowTaskForUser.flowTaskForUserId, flowInstDb, db);
#warning 暂不实现
      #endregion

      #region 填充流程与业务相关数据
      PaymentFlowDataDTO paymentFlowDataDTO =
        PaymentDataDTOBuilder.create(
          flowInstance, flowTaskForUser, payment);
      #endregion

      return Ok(paymentFlowDataDTO);
    }

    [HttpPost]
    [Route("api/PM/RejectToStartFlowAction/")]
    public IHttpActionResult RejectToStartFlowAction()
    {
      dynamic bizObj = getPostedJsonDocObject();

      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      Payment pm = db.payments.Find(bizObj.paymentId);
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
      dynamic bizObj = getPostedJsonDocObject();
      Payment pm = db.payments.Find(bizObj.paymentId);

      return InviteOtherFlowActionGeneral(
        bizObj, flowTemplateCode, pm.documentNo);
    }

    [HttpPost]
    [Route("api/PM/InviteOtherFeedbackFlowAction/")]
    public IHttpActionResult InviteOtherFeedbackFlowAction()
    {
      dynamic bizObj = getPostedJsonDocObject();
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

    private string generateOptionalFlowActionDataJson(Payment pm)
    {
      return null;
    }
  }
}
