using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OPAS2Model;
using EnouFlowOrgMgmtLib;
using EnouFlowTemplateLib;
using EnouFlowInstanceLib;
using Newtonsoft.Json;

using OPAS2.Models;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class PMController : BaseController
  {
    private EnouFlowOrgMgmtContext orgDb = new EnouFlowOrgMgmtContext();
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb = new EnouFlowInstanceContext();

    private string flowTemplateCode = "PM";

    public PMController() : base()
    {
      ViewBag.currentMenuIndex = "PM-NEW";
    }

    // GET: PM/CreateFromPO/1
    [UserLogon]
    public ActionResult CreateFromPO(int purchaseOrderId)
    {
      ViewBag.currentMenuIndex = "PM-NEW";

      PurchaseOrder po = OPAS2ModelDBHelper.getPO(purchaseOrderId, db);
      ViewBag.PO = po;

      ViewBag.guid = Guid.NewGuid().ToString();
      User user = orgDb.users.Find(((UserDTO)Session["currentUserDTO"]).userId);
      Department department = orgDb.departments.Find(po.departmentId);

      #region 业务单据相关初始数据
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(new
        {
          applicantName = user.name + " / " + user.englishName,
          applicantEmail = user.email,
          applicantPhone = user.officeTel,
          mainCurrencyRate = po.mainCurrencyRate,
          isDownPayment = false,
          isNormalPayment = true,
          isImmediatePayment = false,
          isAdvancePayment = false,
        }
      ));
      #endregion

      #region 新建子表项
      ViewBag.PMDetails = encodeToBase64(//只能使用PO的列表项生成对应的付款列表项
        JsonConvert.SerializeObject(
          po.details.Select(
            detail => new {
              id = detail.purchaseOrderDetailId,
              lineNo = detail.lineNo,
              itemName = detail.itemName,
              unitMeasure = detail.unitMeasure,
              price = detail.price,
              quantity = detail.quantity,
              payingQuantity = 0.0,
              receivedQuantity = detail.receivedQuantity,
              amount = detail.amount,
              paidAmount = detail.paidAmount,
            }).ToList()));
      #endregion

      #region 下拉框列表
      PrepareSelectLists();
      #endregion

      #region 流程相关初始数据
      fillFlowInitDataInViewBag(flowTemplateCode);
      #endregion

      return View();
    }

    [UserLogon]
    [HttpGet]
    public ActionResult UpdateAtStart(string id)
    {
      ViewBag.currentMenuIndex = "PM-NEW";

      Payment pm = db.payments.Where(
        obj => obj.guid == id).FirstOrDefault();

      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        pm.flowInstanceId.Value, flowInstDb);

      UserDTO currentUserDTO = (UserDTO)ViewBag.currentUserDTO;
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Where(
        task => task.FlowInstance.flowInstanceId == flowInstance.flowInstanceId &&
        task.bizTimeStamp == flowInstance.bizTimeStamp &&
        task.userId == currentUserDTO.userId
      ).FirstOrDefault();

      #region 检查timestamp是否已过期
      Tuple<bool, ActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance, flowInstDb);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      ViewBag.PO = pm.PurchaseOrder;
      ViewBag.taskGuid = flowTaskForUser?.guid;
      ViewBag.paymentId = pm.paymentId;
      ViewBag.guid = pm.guid;

      #region 业务数据基本字段
      dynamic bizData = new
      {
        reason = pm.reason,
        description = pm.description,
        mainCurrencyRate = pm.mainCurrencyRate,
        vendorBankName = pm.vendorBankName,
        vendorBankAccount = pm.vendorBankAccount,
        SWIFTCode = pm.SWIFTCode,
        applicantName = pm.applicantName,
        applicantEmail = pm.applicantEmail,
        applicantPhone = pm.applicantPhone,
        paymentAreaType = pm.paymentAreaType,
        paymentCurrencyType = pm.paymentCurrencyType,
        paymentMethodType = pm.paymentMethodType,
        payingDaysRequirement = pm.payingDaysRequirement,
        invoiceNo = pm.invoiceNo,
        isDownPayment = pm.isDownPayment,
        isNormalPayment = pm.isNormalPayment,
        isImmediatePayment = pm.isImmediatePayment,
        isAdvancePayment = pm.isAdvancePayment,
      };
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(bizData));
      #endregion

      #region 下拉框列表
      PrepareSelectLists();
      #endregion

      #region 处理已有的子表项
      var detailList = new List<object>();
      if (pm.details.Count() > 0) // 已有的子表不为空
      {
        detailList.AddRange(pm.details.Select(
          detail => new
          {
            id = detail.paymentDetailDetailId,
            guid = detail.guid,
            lineNo = detail.lineNo,
            itemName = detail.itemName,
            unitMeasure = detail.unitMeasure,
            price = detail.price,
            quantity = db.purchaseOrderDetails.Find(
              detail.purchaseOrderDetailId).quantity,
            payingQuantity = detail.quantity,
            receivedQuantity = db.purchaseOrderDetails.Find(
              detail.purchaseOrderDetailId).receivedQuantity,
            amount = detail.amount,
            paidAmount = db.purchaseOrderDetails.Find(
              detail.purchaseOrderDetailId).paidAmount,
          }).ToList());

      }
      ViewBag.PMDetails = encodeToBase64(
        JsonConvert.SerializeObject(detailList));
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(pm);
    }

    // GET: PM/Examine/5b354131-f2ea-489d-8fc6-119676fdcebe/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Examine(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      Payment pm = db.payments.Where(
        obj => obj.guid == id).FirstOrDefault();
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Find(
        flowTaskForUserId);
      FlowInstance flowInstance = flowTaskForUser.FlowInstance;

      #region 检查timestamp是否已过期
      Tuple<bool, ActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance,flowInstDb);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      ViewBag.PO = pm.PurchaseOrder;
      ViewBag.paymentId = pm.paymentId;
      ViewBag.taskGuid = flowTaskForUser.guid;

      #region 获取可能的征询处理意见(InviteOther)的任务反馈意见内容
      ViewBag.inviteOtherFeedbackTasks =
        getValidInviteOtherFeedbackTasks(
          flowTaskForUser.flowTaskForUserId, flowInstDb, db);
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(pm);
    }

    // GET: PM/Display/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Display(string guid)
    {
      ViewBag.currentMenuIndex = "";

      Payment pm = db.payments.Where(
        obj => obj.guid == guid).FirstOrDefault();
      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        pm.flowInstanceId.Value, flowInstDb);

      ViewBag.PO = pm.PurchaseOrder;
      ViewBag.paymentId = pm.paymentId;

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(pm);
    }

    // GET: PM/InviteOtherFeedback/5b354131-f2ea-489d-8fc6-119676fdcebe/5
    [UserLogon]
    [HttpGet]
    public ActionResult InviteOtherFeedback(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      Payment pm = db.payments.Where(
        obj => obj.guid == id).FirstOrDefault();
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Find(
        flowTaskForUserId);
      FlowInstance flowInstance = flowTaskForUser.FlowInstance;

      #region 检查timestamp是否已过期
      Tuple<bool, ActionResult> taskValidity = fillInviteOtherFeedback(
        flowInstDb, flowTaskForUserId);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      ViewBag.PO = pm.PurchaseOrder;
      ViewBag.paymentId = pm.paymentId;
      ViewBag.taskGuid = flowTaskForUser.guid;

      return View(pm);
    }

    private void PrepareSelectLists()
    {
      PrepareSelectListOfPaymentAreaType();
      PrepareSelectListOfPaymentCurrencyType();
      PrepareSelectListOfPaymentMethodType();
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      orgDb.Dispose();
      flowInstDb.Dispose();
      base.Dispose(disposing);
    }
  }
}