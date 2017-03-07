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
  public class VendorController : BaseController
  {

    private OPAS2DbContext db = new OPAS2DbContext();

    public VendorController() : base()
    {
      ViewBag.currentMenuIndex = "MDM-VENDOR";
    }

    [UserLogon]
    public ActionResult Index()
    {
      return View(db.vendors);
    }


    // GET: Vendor/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: Vendor/Create
    [HttpPost]
    [UserLogon]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.vendors.Create();
        obj.vendorOrgNo = collection["vendorOrgNo"];
        obj.vendorNo = collection["vendorNo"];
        obj.chineseName = collection["chineseName"];
        obj.englishName = collection["englishName"];
        obj.chineseAddress = collection["chineseAddress"];
        obj.englishAddress = collection["englishAddress"];
        obj.telphone = collection["telphone"];
        obj.email = collection["email"];
        obj.fax = collection["fax"];
        obj.contactPerson = collection["contactPerson"];
        obj.contactPersonTel = collection["contactPersonTel"];
        obj.contactPersonEmail = collection["contactPersonEmail"];
        obj.contactPersonTitle = collection["contactPersonTitle"];
        obj.duns = collection["duns"];
        obj.remark = collection["remark"];

        db.vendors.Add(obj);
        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: Vendor/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      var obj = db.vendors.Find(id);
      if (obj == null) // 未传入正确的id
      {
        return RedirectToAction("Index");
      }

      return View(obj);
    }

    // POST: Vendor/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection)
    {
      try
      {
        var obj = db.vendors.Find(id);
        if (obj == null) // 未传入正确的id
        {
          return RedirectToAction("Index");
        }

        obj.vendorOrgNo = collection["vendorOrgNo"];
        obj.vendorNo = collection["vendorNo"];
        obj.chineseName = collection["chineseName"];
        obj.englishName = collection["englishName"];
        obj.chineseAddress = collection["chineseAddress"];
        obj.englishAddress = collection["englishAddress"];
        obj.telphone = collection["telphone"];
        obj.email = collection["email"];
        obj.fax = collection["fax"];
        obj.contactPerson = collection["contactPerson"];
        obj.contactPersonTel = collection["contactPersonTel"];
        obj.contactPersonEmail = collection["contactPersonEmail"];
        obj.contactPersonTitle = collection["contactPersonTitle"];
        obj.duns = collection["duns"];
        obj.remark = collection["remark"];

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    public string ShowNameOfId(int id)
    {
      var obj = db.vendors.Find(id);
      return obj.chineseName;
    }

    #region 暂未实现
    // GET: Vendor/Details/5
    [UserLogon]
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: Vendor/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: Vendor/Delete/5
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
    #endregion

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}
