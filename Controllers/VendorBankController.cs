using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OPAS2Model;
using EnouFlowOrgMgmtLib;

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

    // GET: VendorBank?vendorId=1
    [UserLogon]
    public ActionResult Index(int vendorId)
    {
      ViewBag.vendorId = vendorId;
      return View(db.vendorBanks.Where(
        obj => obj.vendorId == vendorId));
    }

    // GET: VendorBank/Details/5
    [UserLogon]
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: VendorBank/Create?vendorId=1
    [UserLogon]
    public ActionResult Create(int vendorId)
    {
      var obj = db.vendorBanks.Create();
      obj.Vendor = db.vendors.Find(vendorId);

      return View(obj);
    }

    // POST: VendorBank/Create
    [HttpPost]
    [UserLogon]
    public ActionResult Create(FormCollection collection)
    {
      var obj = db.vendorBanks.Create();
      try
      {
        var vendor = db.vendors.Find(int.Parse(collection["vendorId"]));
        var existingBankCount = vendor.banks.Count();
        obj.Vendor = vendor;
        obj.bankName = collection["bankName"]; 
        obj.bankAccount = collection["bankAccount"];
        obj.branchName = collection["branchName"];
        obj.SWIFTCode = collection["SWIFTCode"];
        obj.remark = collection["remark"];
        obj.creatorUserId = ((UserDTO)ViewBag.currentUserDTO).userId;
        obj.creator = ((UserDTO)ViewBag.currentUserDTO).name;
        obj.isDefaultBank = (existingBankCount == 0); // 第一个银行则为默认银行

        db.vendorBanks.Add(obj);
        db.SaveChanges();

        return RedirectToAction("Index",
          new { vendorId = int.Parse(collection["vendorId"])});
      }
      catch(Exception ex)
      {
        ViewBag.backendError = ex.Message;
        return View(obj);
      }
    }

    // GET: VendorBank/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      var obj = db.vendorBanks.Find(id);
      return View(obj);
    }

    // POST: VendorBank/Edit/5
    [HttpPost]
    [UserLogon]
    public ActionResult Edit(int id, FormCollection collection)
    {
      var obj = db.vendorBanks.Find(id);
      try
      {
        obj.bankName = collection["bankName"];
        obj.bankAccount = collection["bankAccount"];
        obj.branchName = collection["branchName"];
        obj.SWIFTCode = collection["SWIFTCode"];
        obj.remark = collection["remark"];
        obj.updateUserId = ((UserDTO)ViewBag.currentUserDTO).userId;
        obj.updateUser = ((UserDTO)ViewBag.currentUserDTO).name;
        obj.updateTime = DateTime.Now;

        db.SaveChanges();

        return RedirectToAction("Index",
          new { vendorId = obj.vendorId });
      }
      catch (Exception ex)
      {
        ViewBag.backendError = ex.Message;
        return View(obj);
      }
    }

    [UserLogon]
    public ActionResult SetDefault(int id)
    {
      var obj = db.vendorBanks.Find(id);
      var vendor = obj.Vendor;
      vendor.banks.ForEach(bank => bank.isDefaultBank = false);
      obj.isDefaultBank = true;
      db.SaveChanges();

      return RedirectToAction("Index",
          new { vendorId = vendor.vendorId });
    }


    // GET: VendorBank/Delete/5
    [UserLogon]
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: VendorBank/Delete/5
    [HttpPost]
    [UserLogon]
    public ActionResult Delete(int id, FormCollection collection)
    {
      try
      {
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
