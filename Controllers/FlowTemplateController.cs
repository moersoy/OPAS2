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
  public class FlowTemplateController : BaseController
  {
    public FlowTemplateController() : base()
    {
      ViewBag.currentMenuIndex = "SYS-FLOW-TEMPLATE";
    }

    // GET: FlowTemplate
    [UserLogon]
    [HttpGet]
    public ActionResult Index()
    {
      return View(FlowTemplateDBHelper.
        getAllFlowTemplates().OrderBy(obj=>obj.code));
    }

    // GET: FlowTemplate/Details/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult Details(string guid)
    {
      var obj = FlowTemplateDBHelper.getFlowTemplate(guid);
      ViewBag.templateJsonEncoded = encodeToBase64(obj.flowTemplateJson);
      return View(obj);
    }

    // GET: FlowTemplate/TogglePublish/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    public ActionResult TogglePublish(string guid)
    {
      var obj = FlowTemplateDBHelper.getFlowTemplate(guid);
      if (obj != null)
      {
        FlowTemplateDBHelper.togglePublishOfFlowTemplate(obj.flowTemplateId);
      }

      return RedirectToAction("Index");
    }
  }
}