using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPAS2Model;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class CostElementTypeController : BaseController
  {
    public CostElementTypeController(): base()
    {
      ViewBag.currentMenuIndex = "MDM-COST-ELEMENT-TYPE";
    }

    private OPAS2DbContext db = new OPAS2DbContext();
    // GET: CostElementType
    public ActionResult Index()
    {
      return View(db.costElementTypes);
    }

    // GET: CostElementType/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: CostElementType/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: CostElementType/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.costElementTypes.Create();
        obj.name = collection["name"];
        obj.englishName = collection["englishName"];
        obj.description = collection["description"];
        db.costElementTypes.Add(obj);

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: CostElementType/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      var obj = db.costElementTypes.Find(id);
      return View(obj);
    }

    // POST: CostElementType/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection)
    {
      try
      {
        var obj = db.costElementTypes.Find(id);
        obj.name = collection["name"];
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

    // GET: CostElementType/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: CostElementType/Delete/5
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

    public string ShowNameOfId(int id)
    {
      var obj = db.costElementTypes.Find(id);
      return obj.name;
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}
