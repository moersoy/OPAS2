using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using EnouFlowOrgMgmtLib;
using OPAS2Model;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class RoleController : BaseController
  {
    private EnouFlowOrgMgmtContext db =
      new EnouFlowOrgMgmtContext();
    public RoleController() : base()
    {
      ViewBag.currentMenuIndex = "SYS-ROLE";
    }

    // GET: Role
    [UserLogon]
    public ActionResult Index()
    {
      return View(db.roles);
    }

    // GET: Role/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: Role/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: Role/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = OrgMgmtDBHelper.createRole(db);
        obj.name = collection["name"];
        obj.displayName = collection["displayName"];
        obj.englishName = collection["englishName"];

        OrgMgmtDBHelper.saveCreatedRole(obj, db);

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: Role/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    public ActionResult Edit(string id)
    {
      var obj = db.roles.Where(
        f => f.guid == id).FirstOrDefault();

      return View(obj);
    }

    // POST: Role/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(string id, FormCollection collection)
    {
      try
      {
        var obj = db.roles.Where(
        f => f.guid == id).FirstOrDefault();

        obj.name = collection["name"];
        obj.displayName = collection["displayName"];
        obj.englishName = collection["englishName"];

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    [UserLogon]
    public ActionResult AddBelongedUser(string id)
    {
      var obj = db.roles.Where(
        f => f.guid == id).FirstOrDefault();

      var userDTOs = OrgMgmtDBHelper.getAllUserDTOs(db);

      ViewBag.userDTOs = new SelectList(
        userDTOs, "guid", "name");

      return View(obj);
    }

    [UserLogon]
    [HttpPost]
    public ActionResult AddBelongedUser(int roleId, string userGuid)
    {
      var user = OrgMgmtDBHelper.getUser(userGuid, db);
      var role = db.roles.Find(roleId);
      if (user != null && role != null)
      {
        OrgMgmtDBHelper.setUserRole(user.userId, role, db);
      }

      return RedirectToAction("Index");
    }

    public ActionResult ListUsers(string id)
    {
      var obj = db.roles.Where(
        f => f.guid == id).FirstOrDefault();

      var users = obj.getUsersBelongTo(db);

      var result = "";
      if (users.Count() > 0)
      {
        result = users.Select(user => user.name).ToList().Aggregate
          ((total, next) => { return total + ", " + next; });
      }

      return Content(result, "text/html");
    }

    // GET: Role/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: Role/Delete/5
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
