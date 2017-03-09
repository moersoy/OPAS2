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
  public class GRController : BaseController
  {
    private EnouFlowOrgMgmtContext orgDb = new EnouFlowOrgMgmtContext();
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb = new EnouFlowInstanceContext();

    private string flowTemplateCode = "GR";

    public GRController() : base()
    {
      ViewBag.currentMenuIndex = "GR-NEW";
    }

    // GET: GR/CreateFromPO/1
    [UserLogon]
    public ActionResult CreateFromPO(int purchaseOrderId)
    {
      ViewBag.currentMenuIndex = "GR-NEW";

      PurchaseOrder po = OPAS2ModelDBHelper.getPO(purchaseOrderId, db);
      ViewBag.PO = po;

      ViewBag.guid = Guid.NewGuid().ToString();
      User user = orgDb.users.Find(((UserDTO)Session["currentUserDTO"]).userId);
      Department department = orgDb.departments.Find(po.departmentId);

      #region 业务单据相关初始数据
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(new
        {
          receiver = user.name + " / " + user.englishName,
          checker = user.name + " / " + user.englishName,
          deliveryDate = DateTime.Now,
        }
      ));
      #endregion

      #region 新建子表项
      ViewBag.GRDetails = encodeToBase64(//只能使用PO的列表项生成对应的收货列表项
        JsonConvert.SerializeObject(
          po.details.Select(
            detail => new {
              id = detail.purchaseOrderDetailId,
              lineNo = detail.lineNo,
              itemName = detail.itemName,
              unitMeasure = detail.unitMeasure,
              price = detail.price,
              quantity = detail.quantity,
              receivingQuantity = 0.0,
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
      ViewBag.currentMenuIndex = "GR-NEW";

      GoodsReceiving gr = db.goodsReceivings.Where(
        obj => obj.guid == id).FirstOrDefault();

      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        gr.flowInstanceId.Value, flowInstDb);

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

      ViewBag.PO = gr.PurchaseOrder;
      ViewBag.taskGuid = flowTaskForUser?.guid;
      ViewBag.goodsReceivingId = gr.goodsReceivingId;
      ViewBag.guid = gr.guid;

      #region 业务数据基本字段
      dynamic bizData = new
      {
        description = gr.description,
        receiver = gr.receiver,
        checker = gr.checker,
        deliveryDate = gr.deliveryDate,
        deliveryLocation = gr.deliveryLocation,
        shippingInfo = gr.shippingInfo,
        trackingNo = gr.trackingNo,
        weight = gr.weight,
        packingSlipNo = gr.packingSlipNo,
      };
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(bizData));
      #endregion

      #region 下拉框列表
      PrepareSelectLists();
      #endregion

      #region 处理已有的子表项
      var detailList = new List<object>();
      if (gr.details.Count() > 0) // 已有的子表不为空
      {
        detailList.AddRange(gr.details.Select(
          detail => new
          {
            id = detail.goodsReceivingDetailId,
            guid = detail.guid,
            lineNo = detail.lineNo,
            itemName = detail.itemName,
            unitMeasure = detail.unitMeasure,
            price = detail.price,
            quantity = db.purchaseOrderDetails.Find(
              detail.purchaseOrderDetailId).quantity,
            receivingQuantity = detail.quantity,
            receivedQuantity =  db.purchaseOrderDetails.Find(
              detail.purchaseOrderDetailId).receivedQuantity,
            amount = detail.amount,
            paidAmount = db.purchaseOrderDetails.Find(
              detail.purchaseOrderDetailId).paidAmount,
          }).ToList());
       
      }
      ViewBag.GRDetails = encodeToBase64(
        JsonConvert.SerializeObject(detailList));
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(gr);
    }

    // GET: GR/Examine/5b354131-f2ea-489d-8fc6-119676fdcebe/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Examine(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      GoodsReceiving gr = db.goodsReceivings.Where(
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

      ViewBag.PO = gr.PurchaseOrder;
      ViewBag.goodsReceivingId = gr.goodsReceivingId;
      ViewBag.taskGuid = flowTaskForUser.guid;

      #region 获取可能的征询处理意见(InviteOther)的任务反馈意见内容
      ViewBag.inviteOtherFeedbackTasks =
        getValidInviteOtherFeedbackTasks(
          flowTaskForUser.flowTaskForUserId, flowInstDb, db);
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(gr);
    }

    // GET: GR/InviteOtherFeedback/5b354131-f2ea-489d-8fc6-119676fdcebe/5
    [UserLogon]
    [HttpGet]
    public ActionResult InviteOtherFeedback(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      GoodsReceiving gr = db.goodsReceivings.Where(
        obj => obj.guid == id).FirstOrDefault();
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Find(
        flowTaskForUserId);
      FlowInstance flowInstance = flowTaskForUser.FlowInstance;

      Tuple<bool, ActionResult> taskValidity = fillInviteOtherFeedback(
        flowInstDb, flowTaskForUserId);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }

      ViewBag.PO = gr.PurchaseOrder;
      ViewBag.goodsReceivingId = gr.goodsReceivingId;
      ViewBag.taskGuid = flowTaskForUser.guid;

      return View(gr);
    }

    private void PrepareSelectLists()
    { }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      orgDb.Dispose();
      flowInstDb.Dispose();
      base.Dispose(disposing);
    }
  }
}
