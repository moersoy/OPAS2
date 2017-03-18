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

    // GET: FlowDynamicUser/Details/5
    [UserLogon]
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: FlowDynamicUser/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: FlowDynamicUser/Create
    [UserLogon]
    [HttpPost]
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
            collection["script"], collection["memo"]);
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
      return View();
    }

    // POST: FlowDynamicUser/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection)
    {
      try
      {
        // TODO: Add update logic here

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: FlowDynamicUser/Delete/5
    [UserLogon]
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: FlowDynamicUser/Delete/5
    [UserLogon]
    [HttpPost]
    public ActionResult Delete(int id, FormCollection collection)
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
  }
}
