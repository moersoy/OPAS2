using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPAS2Model;

using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class CostCenterController : BaseController
  {
    public CostCenterController(): base()
    {
      ViewBag.currentMenuIndex = "MDM-COST-CENTER";
    }

    private OPAS2DbContext db = new OPAS2DbContext();

    // GET: CostCenter
    [UserLogon]
    public ActionResult Index()
    {
      return View(db.costCenters);
    }

    // GET: CostCenter/Details/5
    [UserLogon]
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: CostCenter/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: CostCenter/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.costCenters.Create();
        obj.costCenterNo = collection["costCenterNo"];
        obj.chineseName = collection["chineseName"];
        obj.englishName = collection["englishName"];
        obj.description = collection["description"];
        db.costCenters.Add(obj);

        db.SaveChanges();
        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: CostCenter/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    public ActionResult Edit(string id)
    {
      var obj = db.costCenters.Where(costCenter => 
        costCenter.guid == id
      ).FirstOrDefault();

      return View(obj);

    }

    // POST: CostCenter/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(string id, FormCollection collection)
    {
      try
      {
        var obj = db.costCenters.Where(costCenter =>
          costCenter.guid == id
        ).FirstOrDefault();

        obj.costCenterNo = collection["costCenterNo"];
        obj.chineseName = collection["chineseName"];
        obj.englishName = collection["englishName"];
        obj.description = collection["description"];

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    public ActionResult DisplayNameById(int id)
    {
      var result = "";
      var obj = db.costCenters.Find(id);
      result = obj.chineseName + "/" + obj.englishName;

      return Content(result, "text/html");
    }

    // GET: CostCenter/Delete/5b354131-f2ea-489d-8fc6-119676fdcebe
    public ActionResult Delete(string id)
    {
      return View();
    }

    // POST: CostCenter/Delete/5b354131-f2ea-489d-8fc6-119676fdcebe
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
