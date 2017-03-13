using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OPAS2Model;

using OPAS2.Models;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class VendorBankController : BaseController
  {
    private OPAS2DbContext db = new OPAS2DbContext();

    public VendorBankController() : base()
    {
      ViewBag.currentMenuIndex = "MDM-VENDOR-BANK";
    }

    // GET: VendorBank/5
    public ActionResult Index(int vendorId)
    {
      return View(db.vendorBanks.Where(obj => obj.vendorId == vendorId));
    }

    // GET: VendorBank/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: VendorBank/Create
    public ActionResult Create()
    {
      return View();
    }

    // POST: VendorBank/Create
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        // TODO: Add insert logic here

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: VendorBank/Edit/5
    public ActionResult Edit(int id)
    {
      return View();
    }

    // POST: VendorBank/Edit/5
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

    // GET: VendorBank/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: VendorBank/Delete/5
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
