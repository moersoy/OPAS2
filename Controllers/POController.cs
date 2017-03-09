using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;

using OPAS2Model;
using EnouFlowOrgMgmtLib;
using EnouFlowTemplateLib;
using EnouFlowInstanceLib;
using Newtonsoft.Json;

using OPAS2.Models;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class POController : BaseController
  {
    private EnouFlowOrgMgmtContext orgDb = new EnouFlowOrgMgmtContext();
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb = new EnouFlowInstanceContext();
    private string flowTemplateCode = "PO";

    public POController() : base()
    {
      ViewBag.currentMenuIndex = "PO-NEW";
    }

    [UserLogon]
    public ActionResult ListEffective()
    {
      ViewBag.currentMenuIndex = "PO-OPEN";

      var purchaseOrders = db.purchaseOrders.Where(
        po => po.bizState == EnumBizState.Open);

      return View(purchaseOrders);
    }

    // GET: PO/CreateFromPR/1
    [UserLogon]
    public ActionResult CreateFromPR(int purchaseReqId)
    {
      ViewBag.currentMenuIndex = "PO-NEW";

      PurchaseReq pr = OPAS2ModelDBHelper.getPR(purchaseReqId, db);
      ViewBag.PR = pr;

      ViewBag.guid = Guid.NewGuid().ToString();
      User user = orgDb.users.Find(((UserDTO)Session["currentUserDTO"]).userId);
      Department department = orgDb.departments.Find(pr.departmentId);

      #region 业务单据相关初始数据
      ViewBag.departmentId = department.departmentId;
      ViewBag.contactOfficePhone = user.officeTel;
      ViewBag.contactMobile = user.personalMobile;
      ViewBag.costCenterId = pr.costCenterId;

      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(new
        {
          departmentId = department.departmentId,
          contactOfficePhone = user.officeTel,
          contactMobile = user.personalMobile,
          costCenterId = pr.costCenterId,
          departmentIdBelongTo = department.departmentId,
          currencyTypeId = 0,
          mainCurrencyRate = 1,
          vendorId = 0,
          vendorContactPerson = "",
          vendorTel = "",
          orderDate = "",
          effectiveDate = "",
          reason = "For: ",
          description = "",
          paymentTerm = "Payment terms / 支付条款:\n 1. xxxxxxxxxxxx \n 2. XXXXXXXX",
        }
      ));
      #endregion

      #region 下拉框列表
      PrepareSelectLists(pr);
      #endregion

      #region 新建子表项
      ViewBag.PODetails = encodeToBase64(
        JsonConvert.SerializeObject(
          new List<object>() {
            new {
              id = 0,
              guid = "",
              lineNo = 5,
              itemName = "",
              unitMeasure = "",
              price = 0,
              quantity = 0,
              amount = 0,
              amountInRMB = 0,
              description = "",
              WBSNo = "",
              costElementId = 0,
            }
          }));
      #endregion

      #region 流程相关初始数据
      fillFlowInitDataInViewBag(flowTemplateCode);
      #endregion

      return View();
    }

    // GET: PO/Examine/5b354131-f2ea-489d-8fc6-119676fdcebe/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Examine(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      PurchaseOrder po = db.purchaseOrders.Where(
        obj => obj.guid == id).FirstOrDefault();
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Find(
        flowTaskForUserId);
      FlowInstance flowInstance = flowTaskForUser.FlowInstance;

      #region 检查timestamp是否已过期
      Tuple<bool, ActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance, flowInstDb);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      ViewBag.PR = po.PurchaseReq;
      ViewBag.purchaseOrderId = po.purchaseOrderId;
      ViewBag.taskGuid = flowTaskForUser.guid;

      #region 获取可能的征询处理意见(InviteOther)的任务反馈意见内容
      ViewBag.inviteOtherFeedbackTasks =
        getValidInviteOtherFeedbackTasks(
          flowTaskForUser.flowTaskForUserId, flowInstDb, db);
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(po);
    }

    [UserLogon]
    [HttpGet]
    public ActionResult UpdateAtStart(string id)
    {
      ViewBag.currentMenuIndex = "PO-NEW";

      PurchaseOrder po = db.purchaseOrders.Where(
        obj => obj.guid == id).FirstOrDefault();

      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        po.flowInstanceId.Value, flowInstDb);

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

      ViewBag.PR = po.PurchaseReq;
      ViewBag.taskGuid = flowTaskForUser?.guid;
      ViewBag.purchaseOrderId = po.purchaseOrderId;
      ViewBag.guid = po.guid;

      #region 业务数据基本字段
      dynamic bizData = new
      {
        contactOfficePhone = po.contactOfficePhone,
        contactMobile = po.contactMobile,
        WBSNo = po.WBSNo,
        departmentId = po.departmentId,
        departmentIdBelongTo = po.departmentIdBelongTo,
        costCenterId = po.costCenterId,
        orderDate = po.orderDate,
        effectiveDate = po.effectiveDate,
        totalAmount = po.totalAmount,
        currencyTypeId = po.currencyTypeId,
        mainCurrencyRate = po.mainCurrencyRate,
        totalAmountInRMB = po.totalAmountInRMB,
        reason = po.reason,
        description = po.description,
        POType = (int)po.POType,
        vendorId = po.vendorId,
        vendorTel = po.vendorTel,
        vendorContactPerson = po.vendorContactPerson,
        receiverTel = po.receiverTel,
        receiverContactPerson = po.receiverContactPerson,
        invoiceToTel = po.invoiceToTel,
        invoiceToContactPerson = po.invoiceToContactPerson,
        transportTerm = po.transportTerm,
        paymentTerm = po.paymentTerm,
      };
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(bizData));
      #endregion

      #region 下拉框列表
      PrepareSelectLists(po.PurchaseReq);
      #endregion

      #region 处理已有的子表与新建子表项
      var detailList = new List<object>();
      if (po.details.Count() > 0) // 已有的子表不为空
      {
        var maxDetailLineNo = po.details.Max(detail => detail.lineNo);
        detailList.AddRange(po.details.Select(
          detail => new
          {
            id = detail.purchaseOrderDetailId,
            guid = detail.guid,
            lineNo = detail.lineNo,
            itemName = detail.itemName,
            unitMeasure = detail.unitMeasure,
            price = detail.price,
            quantity = detail.quantity,
            amount = detail.amount,
            amountInRMB = detail.amountInRMB,
            description = detail.description,
            WBSNo = detail.WBSNo,
            costElementId = detail.costElementId,
          }).ToList());
        detailList.Add(new
        {
          id = 0,
          guid = "",
          lineNo = maxDetailLineNo + 5,
          itemName = "",
          unitMeasure = "",
          price = 0,
          quantity = 0,
          amount = 0,
          amountInRMB = 0,
          description = "",
          WBSNo = "",
          costElementId = 0,
        });
      }
      else // 已有的子表为空
      {
        detailList.Add(new
        {
          id = 0,
          guid = "",
          lineNo = 5,
          itemName = "",
          unitMeasure = "",
          price = 0,
          quantity = 0,
          amount = 0,
          amountInRMB = 0,
          description = "",
          WBSNo = "",
          costElementId = 0,
        });
      }
      ViewBag.PODetails = encodeToBase64(
        JsonConvert.SerializeObject(detailList));
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(po);
    }

    // GET: PO/InviteOtherFeedback/5b354131-f2ea-489d-8fc6-119676fdcebe/5
    [UserLogon]
    [HttpGet]
    public ActionResult InviteOtherFeedback(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      PurchaseOrder po = db.purchaseOrders.Where(
        obj => obj.guid == id).FirstOrDefault();
      ViewBag.purchaseOrderId = po.purchaseOrderId;
      ViewBag.PR = po.PurchaseReq;

      Tuple<bool, ActionResult> taskValidity = fillInviteOtherFeedback(
        flowInstDb, flowTaskForUserId);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }

      return View(po);
    }

    private void PrepareSelectLists(PurchaseReq pr)
    {
      ViewBag.pRDetailItemNames = encodeToBase64(
        JsonConvert.SerializeObject(
          pr.details.Select(
            detail => new
            {
              id = detail.itemName,
              name = detail.itemName
            })
              .ToList()));

      PrepareSelectListOfUnitMeasure();
      PrepareSelectListOfDepartment(orgDb);
      PrepareSelectListOfCurrencyType(db);
      PrepareSelectListOfCostCenter(db);
      PrepareSelectListOfVendor(db);
      PrepareSelectListOfPOType();
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
