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
    private EnouFlowOrgMgmtContext orgDb = 
      new EnouFlowOrgMgmtContext();
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowInstanceContext flowInstDb = 
      new EnouFlowInstanceContext();

    public FlowInstanceController() : base()
    {
      ViewBag.currentMenuIndex = "SYS-FLOW-INSTANCES-MGMT";
    }

    // Running: FlowInstance
    [UserLogon]
    [HttpGet]
    public ActionResult Running()
    {
      var flowInstances = flowInstDb.flowInstances.Where(obj =>
       obj.lifeState == EnumFlowInstanceLifeState.start ||
       obj.lifeState == EnumFlowInstanceLifeState.middle);
      return View(flowInstances);
    }

    // Stopped: FlowInstance
    [UserLogon]
    [HttpGet]
    public ActionResult Stopped()
    {
      return View();
    }

    // GET: FlowInstance/DisplayBizDocument/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpGet]
    public ActionResult DisplayBizDocument(string guid)
    {
      var flowInstance = FlowInstanceHelper.GetFlowInstance(
        guid,flowInstDb);

      #region 判断流程实例数据合法性
      if (flowInstance == null)
      {
        return new HttpNotFoundResult("未找到流程实例:"+ guid);
      }
      else if (string.IsNullOrWhiteSpace(flowInstance.code))
      {
        return new HttpNotFoundResult("流程实例bizDocumentTypeCode字段为空:" + guid);
      }
      else if (string.IsNullOrWhiteSpace(flowInstance.bizDocumentGuid))
      {
        return new HttpNotFoundResult("流程实例bizDocumentGuid字段为空:" + guid);
      }
      #endregion

      return RedirectToAction("Display", flowInstance.bizDocumentTypeCode, 
        new { guid = flowInstance.bizDocumentGuid });
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