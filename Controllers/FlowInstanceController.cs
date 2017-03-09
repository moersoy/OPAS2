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
  public class FlowInstanceController : BaseController
  {
    private EnouFlowOrgMgmtContext orgDb = new EnouFlowOrgMgmtContext();
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb = new EnouFlowInstanceContext();

    public FlowInstanceController() : base()
    {
      ViewBag.currentMenuIndex = "FLOWINSTANCE-RUNNING";
    }

    // Running: FlowInstance
    [UserLogon]
    public ActionResult Running()
    {
      var flowInstances = flowInstDb.flowInstances.Where(obj =>
       obj.lifeState == EnumFlowInstanceLifeState.start ||
       obj.lifeState == EnumFlowInstanceLifeState.middle);
      return View(flowInstances);
    }

    // Stopped: FlowInstance
    [UserLogon]
    public ActionResult Stopped()
    {
      return View();
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