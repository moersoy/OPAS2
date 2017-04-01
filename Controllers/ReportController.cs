using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using EnouFlowOrgMgmtLib;
using OPAS2Model;
using EnouFlowInstanceLib;

using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class ReportController : BaseController
  {
    private OPAS2DbContext db = new OPAS2DbContext();
    // GET: Report
    public ActionResult Index()
    {
      return View();
    }

    [UserLogon]
    public ActionResult BizDocOverview()
    {
      ViewBag.currentMenuIndex = "REPORT-BIZDOC-OVERVIEW";

      var departments = OrgMgmtDBHelper.getAllDepartmentDTOs(
        new EnouFlowOrgMgmtContext());
      ViewBag.departmentList = departments.Select(
        obj => "'" + obj.name + "'").Aggregate(
        (temp, next) => temp + "," + next );

      ViewBag.prCountList = departments.Select(obj => 
        db.purchaseReqs.Where(doc=>
          doc.departmentId==obj.departmentId).Count()
      ).ToList().Select(
        obj => obj.ToString()).ToList().Aggregate(
        (temp, next) => temp + "," + next);

      ViewBag.poCountList = departments.Select(obj =>
        db.purchaseOrders.Where(doc => 
          doc.departmentId == obj.departmentId).Count()
      ).ToList().Select(
        obj => obj.ToString()).ToList().Aggregate(
        (temp, next) => temp + "," + next);

      ViewBag.grCountList = departments.Select(obj =>
        db.goodsReceivings.Where(doc => 
          doc.departmentId == obj.departmentId).Count()
      ).ToList().Select(
        obj => obj.ToString()).ToList().Aggregate(
        (temp, next) => temp + "," + next);

      ViewBag.pmCountList = departments.Select(obj =>
        db.payments.Where(doc => 
          doc.departmentId == obj.departmentId).Count()
      ).ToList().Select(
        obj => obj.ToString()).ToList().Aggregate(
        (temp, next) => temp + "," + next);

      return View();
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}