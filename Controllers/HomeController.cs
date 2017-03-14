using OPAS2.Filters;
using OPAS2.Helpers;
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
  public class HomeController : BaseController
  {
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb =
      new EnouFlowInstanceContext();

    public HomeController() : base()
    {
      ViewBag.currentMenuIndex = "";
    }

    [UserLogon]
    public ActionResult Index()
    {
      var userDTO = (UserDTO)Session["currentUserDTO"];
      List<FlowTaskForUser> flowTaskForUsers = new List<FlowTaskForUser>();
      flowTaskForUsers = FlowInstanceHelper.GetFlowTaskForUserListOfUser(
        userDTO.guid, flowInstDb);

      List<Models.FlowTask> tasks = flowTaskForUsers.Select(
        task => new Models.FlowTask(task, db)).ToList();

      var myTasks = tasks.Where(obj => // 审批任务或重新提交任务
        obj.taskType == EnumFlowTaskType.normal ||
        obj.taskType == EnumFlowTaskType.redraft).ToList();
      if (myTasks != null && myTasks.Count() > 1)
      {
        myTasks = myTasks.OrderByDescending(
          obj => obj.flowTaskForUserId).ToList();
      }
      ViewBag.MyTasks = myTasks;

      var invitationTasks = tasks.Where(obj => // 被邀请提供意见任务
        obj.taskType == EnumFlowTaskType.invitation).ToList();
      if (invitationTasks != null && myTasks.Count() > 1)
      {
        invitationTasks = invitationTasks.OrderByDescending(
          obj => obj.flowTaskForUserId).ToList();
      }
      ViewBag.InvitationTasks = invitationTasks;

      int userId = ViewBag.currentUserDTO.userId;
      var myWaitingApplications =
        flowInstDb.flowInstances.Where(obj => // 本人提交等待审批的请求
          obj.lifeState == EnumFlowInstanceLifeState.middle &&
          obj.creatorId == userId).ToList();
      if(myWaitingApplications!=null && myWaitingApplications.Count() > 1)
      {
        myWaitingApplications = myWaitingApplications.OrderByDescending(
          obj => obj.flowInstanceId).ToList();
      }
      ViewBag.MyWaitingApplications = myWaitingApplications;

      return View();
    }

    public ActionResult SetCulture(string culture)
    {
      // Validate input
      culture = CultureHelper.GetImplementedCulture(culture);
      // Save culture in a cookie
      HttpCookie cookie = Request.Cookies["_culture"];

      if (cookie != null)
        cookie.Value = culture;   // update cookie value
      else
      {
        cookie = new HttpCookie("_culture");
        cookie.Value = culture;
        cookie.Expires = DateTime.Now.AddYears(1);
      }
      Response.Cookies.Add(cookie);

      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      flowInstDb.Dispose();
      base.Dispose(disposing);
    }
  }
}