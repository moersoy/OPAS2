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
  public class PRController : BaseController
  {
    private EnouFlowOrgMgmtContext orgDb = new EnouFlowOrgMgmtContext();
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb = new EnouFlowInstanceContext();
    private string flowTemplateCode = "PR";

    public PRController() : base()
    {
      ViewBag.currentMenuIndex = "PR-NEW";
    }

    [UserLogon]
    public ActionResult ListEffective()
    {
      ViewBag.currentMenuIndex = "PR-OPEN";
      var purchaseReqs = db.purchaseReqs.Where(
        pr => pr.bizState == EnumBizState.Open);

      return View(purchaseReqs);
    }

    // GET: PR/Display/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Display(string guid)
    {
      var purchaseReq = db.purchaseReqs.Where(
        pr => pr.guid == guid).FirstOrDefault();
      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        purchaseReq.flowInstanceId.Value,flowInstDb);

      ViewBag.purchaseReqId = purchaseReq.purchaseReqId;

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(purchaseReq);
    }

    // GET: PR/Create
    [UserLogon]
    [HttpGet]
    public ActionResult Create()
    {
      ViewBag.currentMenuIndex = "PR-NEW";

      ViewBag.guid = Guid.NewGuid().ToString();
      User user = orgDb.users.Find(((UserDTO)Session["currentUserDTO"]).userId);
      Department department = user.getDepartmentsBelongTo(orgDb, true).FirstOrDefault();

      #region 业务数据基本字段
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(new
        {
          contactOfficePhone = user.officeTel,
          contactMobile = user.personalMobile,
          departmentId = department.departmentId,
          departmentIdBelongTo = department.departmentId,
          isFirstBuy = true,
          isBidingRequired = true,
          reason = "For: ",
          estimatedCostInRMB = 0,
        }));
      #endregion

      #region 下拉框列表
      PrepareSelectLists();
      #endregion

      #region 新建子表项
      ViewBag.PRDetails = encodeToBase64(
        JsonConvert.SerializeObject(
          new List<object>() {
            new {
              id = 0,
              guid = "",
              lineNo = 5,
              itemType =  EnumPRItemType.Goods,
              itemName = "",
              estimatedCost = 0,
              description = "",
            }
          }));
      #endregion

      #region 流程相关初始数据
      fillFlowInitDataInViewBag(flowTemplateCode);
      #endregion

      return View();
    }

    // GET: PR/Examine/5b354131-f2ea-489d-8fc6-119676fdcebe/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Examine(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      PurchaseReq purchaseReq = db.purchaseReqs.Where(
        pr => pr.guid == id).FirstOrDefault();
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

      ViewBag.purchaseReqId = purchaseReq.purchaseReqId;
      ViewBag.taskGuid = flowTaskForUser.guid;

      #region 获取可能的征询处理意见(InviteOther)的任务反馈意见内容
      ViewBag.inviteOtherFeedbackTasks = 
        getValidInviteOtherFeedbackTasks( 
          flowTaskForUser.flowTaskForUserId,flowInstDb,db);
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(purchaseReq);
    }

    [UserLogon]
    [HttpGet]
    public ActionResult UpdateAtStart(string id)
    {
      ViewBag.currentMenuIndex = "PR-NEW";

      PurchaseReq purchaseReq = db.purchaseReqs.Where(
        pr => pr.guid == id).FirstOrDefault();

      FlowInstance flowInstance = FlowInstanceHelper.GetFlowInstance(
        purchaseReq.flowInstanceId.Value, flowInstDb);

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

      ViewBag.taskGuid = flowTaskForUser?.guid;
      ViewBag.purchaseReqId = purchaseReq.purchaseReqId;
      ViewBag.guid = purchaseReq.guid;

      #region 业务数据基本字段
      dynamic bizData = new {
        contactOfficePhone = purchaseReq.contactOfficePhone,
        contactMobile = purchaseReq.contactMobile,
        WBSNo = purchaseReq.WBSNo,
        departmentId = purchaseReq.departmentId,
        departmentIdBelongTo = purchaseReq.departmentIdBelongTo,
        contactOtherMedia = purchaseReq.contactOtherMedia,
        costCenterId = purchaseReq.costCenterId,
        expectReceiveBeginTime = purchaseReq.expectReceiveBeginTime,
        expectReceiveEndTime = purchaseReq.expectReceiveEndTime,
        isFirstBuy = purchaseReq.isFirstBuy,
        firstBuyDate = purchaseReq.firstBuyDate,
        isBidingRequired = purchaseReq.isBidingRequired,
        noBiddingReason = purchaseReq.noBiddingReason,
        reason = purchaseReq.reason,
        description = purchaseReq.description,
        estimatedCostInRMB = purchaseReq.estimatedCostInRMB,
        averageBenchmark = purchaseReq.averageBenchmark,
        benchmarkDescription = purchaseReq.benchmarkDescription,
        firstCostAmount = purchaseReq.firstCostAmount,
        firstBuyDescription = purchaseReq.firstBuyDescription,
        otherVendorsNotInList = purchaseReq.otherVendorsNotInList,
      };
      ViewBag.bizDataJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(bizData));
      #endregion

      #region 下拉框列表
      PrepareSelectLists();
      #endregion

      #region 处理已有的子表与新建子表项
      var detailList = new List<object>();
      if (purchaseReq.details.Count() > 0)
      { 
        var maxDetailLineNo = purchaseReq.details.Max(detail => detail.lineNo);
        detailList.AddRange(purchaseReq.details.Select(
              detail => new
              {
                id = detail.purchaseReqDetailId,
                guid = detail.guid,
                lineNo = detail.lineNo,
                itemType = detail.itemType,
                itemName = detail.itemName,
                estimatedCost = detail.estimatedCost.HasValue ? detail.estimatedCost.Value : 0,
                description = detail.description,
              }).ToList());
        detailList.Add(new
        {
          id = 0,
          guid = "",
          lineNo = maxDetailLineNo + 5,
          itemType = EnumPRItemType.Goods,
          itemName = "",
          estimatedCost = decimal.Parse("0"),
          description = "",
        });
      }
      else
      {
        detailList.Add(new
        {
          id = 0,
          guid = "",
          lineNo = 5,
          itemType = EnumPRItemType.Goods,
          itemName = "",
          estimatedCost = decimal.Parse("0"),
          description = "",
        });
      }
      ViewBag.PRDetails = encodeToBase64(
        JsonConvert.SerializeObject(detailList));
      #endregion

      #region 流程相关数据
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return View(purchaseReq);
    }

    // GET: PR/InviteOtherFeedback/5b354131-f2ea-489d-8fc6-119676fdcebe/5
    [UserLogon]
    [HttpGet]
    public ActionResult InviteOtherFeedback(string id, int flowTaskForUserId)
    {
      ViewBag.currentMenuIndex = "";

      PurchaseReq purchaseReq = db.purchaseReqs.Where(
        pr => pr.guid == id).FirstOrDefault();
      ViewBag.purchaseReqId = purchaseReq.purchaseReqId;

      Tuple<bool, ActionResult> taskValidity = fillInviteOtherFeedback(
        flowInstDb, flowTaskForUserId);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }

      return View(purchaseReq);
    }

    private void PrepareSelectLists()
    {
      PrepareSelectListOfDepartment(orgDb);
      PrepareSelectListOfCurrencyType(db);
      PrepareSelectListOfCostCenter(db);

      ViewBag.pRItemTypesJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(TypeSelectLists.PRItemTypeList));
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
