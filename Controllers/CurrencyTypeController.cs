using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPAS2Model;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class CurrencyTypeController : BaseController
  {
    public CurrencyTypeController(): base()
    {
      ViewBag.currentMenuIndex = "MDM-CURRENCY-TYPE";
    }

    private OPAS2DbContext db = new OPAS2DbContext();

    // GET: CurrencyType
    [UserLogon]
    public ActionResult Index()
    {
      return View(db.currencyTypes);
    }

    // GET: CurrencyType/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: CurrencyType/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: CurrencyType/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.currencyTypes.Create();
        obj.name = collection["name"];
        obj.shortName = collection["shortName"];
        obj.symbol = collection["symbol"];
        db.currencyTypes.Add(obj);

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: CurrencyType/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      var obj = db.currencyTypes.Find(id);
      return View(obj);
    }

    // POST: CurrencyType/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection)
    {
      try
      {
        var obj = db.currencyTypes.Find(id);
        obj.name = collection["name"];
        obj.shortName = collection["shortName"];
        obj.symbol = collection["symbol"];

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: CurrencyType/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: CurrencyType/Delete/5
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
      var obj = db.currencyTypes.Find(id);
      return obj.name;
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}
