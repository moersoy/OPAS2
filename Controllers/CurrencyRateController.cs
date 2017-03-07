using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPAS2Model;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class CurrencyRateController : BaseController
  {
    public CurrencyRateController() : base()
    {
      ViewBag.currentMenuIndex = "MDM-CURRENCY-RATE";
    }

    private OPAS2DbContext db = new OPAS2DbContext();

    // GET: CurrencyRate
    [UserLogon]
    public ActionResult Index()
    {
      return View(db.currencyHistoryRecords);
    }

    // GET: CurrencyRate/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: CurrencyRate/Create
    [UserLogon]
    public ActionResult Create()
    {
      var currencyTypes = db.currencyTypes;
      ViewBag.selectList = new SelectList(currencyTypes,
        "currencyTypeId", "name");

      return View();
    }

    // POST: CurrencyRate/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.currencyHistoryRecords.Create();
        obj.currencyTypeId = int.Parse( collection["currencyTypeId"]);
        obj.CurrencyType = db.currencyTypes.Find(obj.currencyTypeId);
        obj.effectiveDateFrom = DateTime.Parse(collection["effectiveDateFrom"]);
        obj.effectiveRate = decimal.Parse( collection["effectiveRate"]);
        db.currencyHistoryRecords.Add(obj);

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch(Exception ex)
      {
        return View();
      }
    }

    // GET: CurrencyRate/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      return View();
    }

    // POST: CurrencyRate/Edit/5
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

    // GET: CurrencyRate/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: CurrencyRate/Delete/5
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
