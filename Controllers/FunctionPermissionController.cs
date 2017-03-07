using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OPAS2Model;
using OPAS2.Filters;
using EnouFlowOrgMgmtLib;

namespace OPAS2.Controllers
{
  public class FunctionPermissionController : BaseController
  {
    private OPAS2DbContext db = new OPAS2DbContext();
    private EnouFlowOrgMgmtContext orgDb = 
      new EnouFlowOrgMgmtContext();

    public FunctionPermissionController(): base()
    {
      ViewBag.currentMenuIndex = "SYS-PERMISSION";
    }

    // GET: FunctionPermission
    [UserLogon]
    public ActionResult Index()
    {
      return View(db.functionPermissions);
    }

    // GET: FunctionPermission/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: FunctionPermission/Create
    [UserLogon]
    public ActionResult Create()
    {
      return View();
    }

    // POST: FunctionPermission/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = db.functionPermissions.Create();
        obj.code = collection["code"];
        obj.memo = collection["memo"];
        obj.availableToEveryOne = parseCheckboxValue(
          collection["availableToEveryOne"]);

        db.functionPermissions.Add(obj);
        db.SaveChanges();
        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    // GET: FunctionPermission/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    public ActionResult Edit(string id)
    {
      var obj = db.functionPermissions.Where(
        f =>f.guid == id).FirstOrDefault();

      return View(obj);
    }

    // POST: FunctionPermission/Edit/5b354131-f2ea-489d-8fc6-119676fdcebe
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(string id, FormCollection collection)
    {
      try
      {
        var obj = db.functionPermissions.Where(
          f => f.guid == id).FirstOrDefault();

        obj.code = collection["code"];
        obj.memo = collection["memo"];
        obj.availableToEveryOne = parseCheckboxValue(
          collection["availableToEveryOne"]);

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    [UserLogon]
    public ActionResult AddUser(int id)
    {
      var obj = db.functionPermissions.Find(id);

      var userDTOs = OrgMgmtDBHelper.getAllUserDTOs(orgDb);

      ViewBag.selectList = new SelectList(
        userDTOs, "guid", "name");

      return View(obj);
    }

    [UserLogon]
    [HttpPost]
    public ActionResult AddUser(int id, string userGuid)
    {
      var user = OrgMgmtDBHelper.getUser(userGuid, orgDb);

      var functionPermission = db.functionPermissions.Find(id);
      if (user != null && functionPermission != null)
      {
        OPAS2ModelDBHelper.setFunctionPermissionUser(
          id, user.userId, user.guid, db);
      }

      return RedirectToAction("Index");
    }

    [UserLogon]
    [HttpPost]
    public ActionResult RemoveUser(int id, string userGuid)
    {
      var user = OrgMgmtDBHelper.getUser(userGuid, orgDb);

      var functionPermission = db.functionPermissions.Find(id);
      if (user != null && functionPermission != null)
      {
        OPAS2ModelDBHelper.unsetFunctionPermissionUser(
          id, user.userId, db);
      }

      return RedirectToAction("Index");
    }

    public ActionResult ListUsers(int id)
    {
      var obj = db.functionPermissions.Find(id);

      var users = obj.getUserIdsBelongTo(db).Select(
        userId => { return orgDb.users.Find(userId); }).ToList();

      var result = "";
      if (users.Count() > 0)
      {
        result = users.Select(user => user.name).ToList().Aggregate
          ((total, next) => { return total + ", " + next; });
      }

      return Content(result, "text/html");
    }

    [UserLogon]
    public ActionResult AddRole(int id)
    {
      var obj = db.functionPermissions.Find(id);

      var roleDTOs = OrgMgmtDBHelper.getAllRoleDTOs(orgDb);

      ViewBag.selectList = new SelectList(
        roleDTOs, "guid", "name");

      return View(obj);
    }

    [UserLogon]
    [HttpPost]
    public ActionResult AddRole(int id, string roleGuid)
    {
      var role = OrgMgmtDBHelper.getRole(roleGuid, orgDb);

      var functionPermission = db.functionPermissions.Find(id);
      if (role != null && functionPermission != null)
      {
        OPAS2ModelDBHelper.setFunctionPermissionRole(
          id, role.roleId, role.guid, db);
      }

      return RedirectToAction("Index");
    }

    [UserLogon]
    [HttpPost]
    public ActionResult RemoveRole(int id, string roleGuid)
    {
      var role = OrgMgmtDBHelper.getRole(roleGuid, orgDb);

      var functionPermission = db.functionPermissions.Find(id);
      if (role != null && functionPermission != null)
      {
        OPAS2ModelDBHelper.unsetFunctionPermissionRole(
          id, role.roleId, db);
      }

      return RedirectToAction("Index");
    }

    public ActionResult ListRoles(int id)
    {
      var obj = db.functionPermissions.Find(id);

      var roles = obj.getRoleIdsBelongTo(db).Select(
        roleId => { return orgDb.roles.Find(roleId); }).ToList();

      var result = "";
      if (roles.Count() > 0)
      {
        result = roles.Select(role => role.name).ToList().Aggregate
          ((total, next) => { return total + ", " + next; });
      }

      return Content(result, "text/html");
    }








    // GET: FunctionPermission/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: FunctionPermission/Delete/5
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
      orgDb.Dispose();
      base.Dispose(disposing);
    }
  }
}
