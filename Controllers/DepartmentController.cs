using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnouFlowOrgMgmtLib;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class DepartmentController : BaseController
  {
    private EnouFlowOrgMgmtContext db = new EnouFlowOrgMgmtContext();

    public DepartmentController() : base()
    {
      ViewBag.currentMenuIndex = "SYS-DEPARTMENT";
    }

    // GET: Department
    [UserLogon]
    public ActionResult Index()
    {
      return View(OrgMgmtDBHelper.getAllDepartmentDTOs(db));
    }

    // GET: Department/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: Department/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: Department/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = OrgMgmtDBHelper.createDepartment(db);
        obj.name = collection["name"];
        obj.shortName = collection["shortName"];
        obj.displayName = collection["displayName"];
        obj.englishName = collection["englishName"];
        obj.code = collection["code"];
        obj.indexNumber = collection["indexNumber"];
        OrgMgmtDBHelper.saveCreatedDepartment(
          db.bizEntitySchemas.FirstOrDefault(), obj, null, db);

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: Department/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    public ActionResult Edit(string id)
    {
      var obj = OrgMgmtDBHelper.convertDepartment2DTO(
        db.departments.Where(dep =>
        dep.guid == id).FirstOrDefault(),
        db);

      return View(obj);
    }

    // POST: Department/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(string id, FormCollection collection)
    {
      try
      {
        var obj = db.departments.Where(dep =>
          dep.guid == id
        ).FirstOrDefault();

        obj.name = collection["name"];
        obj.shortName = collection["shortName"];
        obj.displayName = collection["displayName"];
        obj.englishName = collection["englishName"];
        obj.code = collection["code"];
        obj.indexNumber = collection["indexNumber"];

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
      result = db.departments.Find(id).name;

      return Content(result, "text/html");
    }

    public ActionResult DisplayManagers(int id)
    {
      var result = "";

      List<UserDTO> users = OrgMgmtDBHelper.getUserDTOsOfPositionInDepartment(
        id, db, UserPositionToDepartment.manager);
      if (users != null && users.Count() > 0)
      {
        result = users.Aggregate("", (total, current) => 
          { return total + current.name + ";"; });
      }

      return Content(result, "text/html");
    }

    // GET: Department/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: Department/Delete/5
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
