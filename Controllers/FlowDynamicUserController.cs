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
  public class FlowDynamicUserController : BaseController
  {
    public FlowDynamicUserController() : base()
    {
      ViewBag.currentMenuIndex = "SYS-FLOW-DYNAMIC-USER";
    }

    // GET: FlowDynamicUser
    [UserLogon]
    [HttpGet]
    public ActionResult Index()
    {
      return View(
        FlowTemplateDBHelper.getAllFlowDynamicUsers());
    }

    // GET: FlowDynamicUser/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: FlowDynamicUser/Create
    [UserLogon]
    [HttpPost, ValidateInput(false)]
    public ActionResult Create(FormCollection collection)
    {
      var obj = new FlowDynamicUser();
      obj.name = collection["name"];
      obj.displayName = collection["displayName"];
      obj.script = collection["script"];
      obj.memo = collection["memo"];
      try
      {
        Tuple<bool, FlowDynamicUser, List<string>> result =
          FlowTemplateDBHelper.createFlowDynamicUser(
            null, collection["name"], collection["displayName"],
            collection["script"], collection["memo"], 
            bool.Parse(collection["isPublished"]));
        if (result.Item1)
        {
          return RedirectToAction("Index");
        }
        else
        {
          ViewBag.backendError = result.Item3.Aggregate(
            "", (err, all) => all + err );
          return View(obj);
        }

      }
      catch (Exception ex)
      {
        ViewBag.backendError = ex.Message;
        return View(obj);
      }
    }

    // GET: FlowDynamicUser/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      EnouFlowTemplateDbContext db = new EnouFlowTemplateDbContext();
      var flowDynamicUser = db.flowDynamicUsers.Find(id);
      return View(flowDynamicUser);
    }

    // POST: FlowDynamicUser/Edit/5
    [UserLogon]
    [HttpPost, ValidateInput(false)]
    public ActionResult Edit(int id, FormCollection collection)
    {
      EnouFlowTemplateDbContext db = new EnouFlowTemplateDbContext();
      var flowDynamicUser = db.flowDynamicUsers.Find(id);

      try
      {
        Tuple<bool, FlowDynamicUser, List<string>> result = 
          FlowTemplateDBHelper.updateFlowDynamicUser(
            flowDynamicUser.guid, collection["name"],
            collection["displayName"], collection["script"],
            collection["memo"], parseCheckboxValue(collection["isPublished"]));

        if (result.Item1)
        {
          return RedirectToAction("Index");
        }
        else
        {
          ViewBag.backendError = result.Item3.Aggregate(
            "", (err, all) => all + err);
          return View(flowDynamicUser);
        }
      }
      catch (Exception ex)
      {
        ViewBag.backendError = ex.Message;
        return View(flowDynamicUser);
      }
    }
  }
}
