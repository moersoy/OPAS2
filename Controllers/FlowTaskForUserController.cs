using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using EnouFlowInstanceLib;
using EnouFlowOrgMgmtLib;
using OPAS2.Filters;
using OPAS2Model;

namespace OPAS2.Controllers
{
  public class FlowTaskForUserController : BaseController
  {
    public FlowTaskForUserController(): base()
    {
      ViewBag.currentMenuIndex = "";
    }

    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowDb = 
      new EnouFlowInstanceContext();

    // GET: FlowTaskForUser
    [UserLogon]
    public ActionResult Index()
    {
      var userDTO = (UserDTO)Session["currentUserDTO"];
      List<FlowTaskForUser> flowTaskForUsers = ViewBag.flowTaskForUsers;
      List<Models.FlowTask> tasks = flowTaskForUsers.Select(
        task => new Models.FlowTask(task, db)).ToList();

      return View(tasks);
    }

    // GET: FlowTaskForUser/DelegatedIndex
    [UserLogon]
    public ActionResult DelegatedIndex()
    {
      List<Models.FlowTask> results = new List<Models.FlowTask>();

      var list = (List<Tuple<UserDTO, List<FlowTaskForUser>>>)
        ViewBag.delegatedFlowTaskForUsers;

      list.ForEach(turple => {
        results.AddRange(
          turple.Item2.Select(
            task => new Models.FlowTask(task, db)).ToList());
      });

      return View(results);
    }

    // GET: FlowTaskForUser/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    public ActionResult Edit(
      string id, string documentTypeCode, int flowTaskForUserId)
    {
      string actionName = "View";
      int flowTaskForUserIdToShow = flowTaskForUserId;

      using (var flowDb = new EnouFlowInstanceContext())
      {
        var task = FlowInstanceHelper.GetFlowTaskForUser(
          flowTaskForUserId, flowDb);
        switch (task.taskType)
        {
          case EnumFlowTaskType.normal:
            actionName = "Examine";
            break;
          case EnumFlowTaskType.invitationFeedback:
            // 审批用户获得征询意见的反馈任务,需要改为显示的是原审批任务, 通过:
            // 反馈任务->征询任务->原任务, 使用relativeFlowTaskForUserId
            var _taskInviteFeedback = FlowInstanceHelper.GetFlowTaskForUser(flowTaskForUserId, flowDb);
            if (_taskInviteFeedback.relativeFlowTaskForUserId.HasValue)
            {
              var _taskInvite = FlowInstanceHelper.GetFlowTaskForUser(
                _taskInviteFeedback.relativeFlowTaskForUserId.Value, flowDb);
              if (_taskInvite.relativeFlowTaskForUserId.HasValue)
              {
                flowTaskForUserIdToShow = _taskInvite.relativeFlowTaskForUserId.Value;
              }
            }
            actionName = "Examine";
            break;
          case EnumFlowTaskType.redraft:
            actionName = "UpdateAtStart";
            break;
          case EnumFlowTaskType.invitation:
            actionName = "InviteOtherFeedback";
            break;
          default:
            actionName = "View";
            break;
        }
      }
      
      switch (documentTypeCode)
      {
        case "PR":
          return RedirectToAction(actionName, "PR", 
            new { id = id, flowTaskForUserId = flowTaskForUserIdToShow });
        case "PO":
          return RedirectToAction(actionName, "PO",
            new { id = id, flowTaskForUserId = flowTaskForUserIdToShow });
        case "GR":
          return RedirectToAction(actionName, "GR",
            new { id = id, flowTaskForUserId = flowTaskForUserIdToShow });
        case "PM":
          return RedirectToAction(actionName, "PM",
            new { id = id, flowTaskForUserId = flowTaskForUserIdToShow });
        default:
          throw new Exception("Unexpected documentTypeCode: " + 
            documentTypeCode);
      }
    }

    public ActionResult DisplayCurrentOperatorsOfFlowInstance(int id)
    {
      var result = "";
      using (var orgDb = new EnouFlowOrgMgmtContext())
      {
        var flowTaskForUsers = FlowInstanceHelper.
          GetWaitingFlowTaskForUserListOfFlowInstance(id, flowDb);
        if(flowTaskForUsers != null && flowTaskForUsers.Count() > 0)
        {
          result = flowTaskForUsers.Aggregate("", (names, task) => {
            return names + orgDb.users.Find(task.userId).name + ";";
          });
        }
      }

      return Content(result, "text/html");
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      flowDb.Dispose();
      base.Dispose(disposing);
    }
  }
}
