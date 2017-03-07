using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPAS2Model;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class CostElementController : BaseController
  {
    private OPAS2DbContext db = new OPAS2DbContext();

    public CostElementController() : base()
    {
      ViewBag.currentMenuIndex = "MDM-COST-ELEMENT";
    }

    // GET: CostElement
    [UserLogon]
    public ActionResult Index()
    {
      return View(db.costElements);
    }

    // GET: CostElement/Details/5
    [UserLogon]
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: CostElement/Create
    [UserLogon]
    public ActionResult Create()
    {
      var costElementTypes = db.costElementTypes;
      ViewBag.selectList = new SelectList(costElementTypes, "costElementTypeId", "name");

      ViewBag.costElementAccountTypes = new SelectList(TypeSelectLists.CostElementAccountTypeList, "id", "name");

      return View();
    }

    // POST: CostElement/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.costElements.Create();
        obj.name = collection["name"];
        obj.englishName = collection["englishName"];
        obj.description = collection["description"];
        obj.costElementTypeId = int.Parse(
          collection["costElementTypeId"]);
        obj.costElementAccountType = 
          (EnumCostElementAccountType) int.Parse(
          collection["costElementAccountTypeId"]);

        db.costElements.Add(obj);
        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch(Exception ex)
      {

        return View();
      }
    }

    // GET: CostElement/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      var obj = db.costElements.Find(id);
      if (obj == null) // 未传入正确的id
      {
        return RedirectToAction("Index");
      }

      var costElementTypes = db.costElementTypes;
      ViewBag.selectList = new SelectList(costElementTypes, 
        "costElementTypeId", "name", obj.costElementTypeId);

      ViewBag.costElementAccountTypes = new SelectList(
        TypeSelectLists.CostElementAccountTypeList, 
        "id", "name", (int) obj.costElementAccountType);

      return View(obj);
    }

    // POST: CostElement/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection)
    {
      try
      {
        var obj = db.costElements.Find(id);
        if (obj == null) // 未传入正确的id
        {
          return RedirectToAction("Index");
        }

        obj.name = collection["name"];
        obj.englishName = collection["englishName"];
        obj.description = collection["description"];
        obj.costElementTypeId = int.Parse(
          collection["costElementTypeId"]);
        obj.costElementAccountType =
          (EnumCostElementAccountType)int.Parse(
          collection["costElementAccountTypeId"]);

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: CostElement/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: CostElement/Delete/5
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
    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}
