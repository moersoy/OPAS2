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

    // GET: FlowTaskForUser
    [UserLogon]
    public ActionResult Index()
    {
      var userDTO = (UserDTO)Session["currentUserDTO"];
      List<FlowTaskForUser> flowTaskForUsers = new List<FlowTaskForUser>();
      using (var flowDb = new EnouFlowInstanceContext())
      {
        flowTaskForUsers = FlowInstanceHelper.GetFlowTaskForUserListOfUser(userDTO.guid, flowDb);
      }

      List<Models.FlowTask> tasks = flowTaskForUsers.Select(
        task => new Models.FlowTask(task, db)).ToList();

      return View(tasks);
    }

    // GET: FlowTaskForUser/Details/5b354131-f2ea-489d-8fc6-119676fdcebe
    public ActionResult Details(string id)
    {
      return View();
    }

    // GET: FlowTaskForUser/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    public ActionResult Edit(
      string id, string documentTypeCode, int flowTaskForUserId)
    {
      string actionName = "Examine";
      using (var flowDb = new EnouFlowInstanceContext())
      {
        var task = FlowInstanceHelper.GetFlowTaskForUser(
          flowTaskForUserId, flowDb);
        switch (task.taskType)
        {
          case EnumFlowTaskType.redraft:
            actionName = "UpdateAtStart";
            break;
          default:
            actionName = "Examine";
            break;
        }
      }
      
      switch (documentTypeCode)
      {
        case "PR":
          return RedirectToAction(actionName, "PR", 
            new { id = id, flowTaskForUserId = flowTaskForUserId });
        case "PO":
          return RedirectToAction(actionName, "PO",
            new { id = id, flowTaskForUserId = flowTaskForUserId });
        case "GR":
          return RedirectToAction(actionName, "GR",
            new { id = id, flowTaskForUserId = flowTaskForUserId });
        case "PM":
          return RedirectToAction(actionName, "PM",
            new { id = id, flowTaskForUserId = flowTaskForUserId });
        default:
          throw new Exception("Unexpected documentTypeCode: " + 
            documentTypeCode);
      }
    }

    // POST: FlowTaskForUser/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [HttpPost]
    public ActionResult Edit(string id, FormCollection collection)
    {
      try
      {
        

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: FlowTaskForUser/Delete/5b354131-f2ea-489d-8fc6-119676fdcebe
    public ActionResult Delete(string id)
    {
      return View();
    }

    // POST: FlowTaskForUser/Delete/5b354131-f2ea-489d-8fc6-119676fdcebe
    [HttpPost]
    public ActionResult Delete(string id, FormCollection collection)
    {
      try
      {
        // TODO: Add delete logic here

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}
