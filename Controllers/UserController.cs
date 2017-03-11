using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnouFlowOrgMgmtLib;
using OPAS2.Filters;
using OPAS2Model;

namespace OPAS2.Controllers
{
  public class UserController : BaseController
  {
    private EnouFlowOrgMgmtContext db = new EnouFlowOrgMgmtContext();
    private OPAS2DbContext OPASDb = new OPAS2DbContext();

    public UserController() : base()
    {
      ViewBag.currentMenuIndex = "SYS-USER";
    }

    // GET: User
    [UserLogon]
    public ActionResult Index()
    {
      return View(OrgMgmtDBHelper.getAllUserDTOs(db));
    }

    // GET: User/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: User/Create
    [UserLogon]
    public ActionResult Create()
    {
      //var departments = OrgMgmtDBHelper.getAllDepartmentDTOs(db);
      //ViewBag.selectList = new SelectList(departments, "departmentId", "name");
      SetSelectListOfDepartment(db);
      return View();
    }

    // POST: User/Create
    [HttpPost]
    [UserLogon]
    public ActionResult Create(FormCollection collection)
    {
      try
      {
        var obj = OrgMgmtDBHelper.createUser(db);
        obj.name = collection["name"];
        obj.code = collection["code"];
        obj.indexNumber = collection["indexNumber"];
        obj.displayName = collection["displayName"];
        obj.englishName = collection["englishName"];
        if (!string.IsNullOrWhiteSpace(collection["email"]))
        {
          obj.email = collection["email"];
        }
        obj.logonName = collection["logonName"];
        #region password processing
        string logonPasswordOrigin = collection["logonPassword"];
        var PasswordHashAndSalt = OrgMgmtDBHelper.generatePasswordHashAndSalt(logonPasswordOrigin);
        obj.logonPasswordHash = PasswordHashAndSalt.Item1;
        obj.logonSalt = PasswordHashAndSalt.Item2;
        #endregion
        obj.officeTel = collection["officeTel"];
        obj.personalTel = collection["personalTel"];
        obj.personalMobile = collection["personalMobile"];

        OrgMgmtDBHelper.saveCreatedUser(obj, db);

        var department = db.departments.Find(int.Parse(collection["departmentId"]));
        if (department != null)
        {
          OrgMgmtDBHelper.saveDepartmentUserRelation(department, obj, db);
        }

        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {

        return View();
      }
    }

    // GET: User/Edit/5
    [UserLogon]
    public ActionResult Edit(string id)
    {
      var departments = OrgMgmtDBHelper.getAllDepartmentDTOs(db);
      ViewBag.selectList = new SelectList(departments, "departmentId", "name");

      var user = OrgMgmtDBHelper.convertUser2DTO(db.users.Where(_user =>
        _user.guid == id).FirstOrDefault(), db);

      return View(user);
    }

    [HttpGet]
    public ActionResult Logon()
    {
      ViewBag.logonName = "";
      return View();
    }

    [HttpPost]
    public ActionResult Logon(FormCollection collection)
    {
      string logonName = collection["logonName"];
      ViewBag.logonName = logonName; // 如果登陆不成功,暂存用户名再次显示
      string logonPassword = collection["logonPassword"];

      if (!string.IsNullOrWhiteSpace(logonPassword) && !string.IsNullOrWhiteSpace(logonName))
      {
        var user = OrgMgmtDBHelper.logonUser(logonName, logonPassword, db);
        if (user != null) // 成功登陆, 转向首页
        {
          //用户Profile
          Session["currentUserDTO"] = OrgMgmtDBHelper.convertUser2DTO(
            user, db, true);

          var userFunctionPermissions = OPAS2ModelDBHelper.getDirectFunctionPermissionsOfUser(
            user.userId, OPASDb);
          List<Role> roles = user.getRolesBelongTo(db, true);
          var roleFunctionPermissions = roles.ConvertAll(role =>
            OPAS2ModelDBHelper.getDirectFunctionPermissionsOfRole(role.roleId, OPASDb)).
            Aggregate(new List<FunctionPermission>(), 
              (list, next) => { list.AddRange(next); return list; });
          userFunctionPermissions.AddRange(roleFunctionPermissions);
          //计算用户权限列表并存入Session用于前端功能权限判定
          Session["functionPermissionCodes"] = 
            userFunctionPermissions.Distinct().Select(f => f.code).ToList();

          return RedirectToAction("Index","Home",null);
        }
      }
      return View(); // 继续显示登陆界面
    }

    [HttpGet]
    public ActionResult Logout()
    {
      Session.RemoveAll();
      return RedirectToAction("Logon");
    }

    // POST: User/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(string id, FormCollection collection)
    {
      try
      {
        var obj = db.users.Where(_user =>
          _user.guid == id).FirstOrDefault();
        obj.name = collection["name"];
        obj.code = collection["code"];
        obj.indexNumber = collection["indexNumber"];
        obj.displayName = collection["displayName"];
        obj.englishName = collection["englishName"];
        if (!string.IsNullOrWhiteSpace(collection["email"]))
        {
          obj.email = collection["email"];
        }
        obj.logonName = collection["logonName"];
        #region password processing
        string logonPasswordOrigin = collection["logonPassword"];
        if (!string.IsNullOrWhiteSpace(logonPasswordOrigin))
        { 
          var PasswordHashAndSalt = OrgMgmtDBHelper.generatePasswordHashAndSalt(logonPasswordOrigin);
          obj.logonPasswordHash = PasswordHashAndSalt.Item1;
          obj.logonSalt = PasswordHashAndSalt.Item2;
        }
        #endregion
        obj.officeTel = collection["officeTel"];
        obj.personalTel = collection["personalTel"];
        obj.personalMobile = collection["personalMobile"];

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch
      {
        return View();
      }
    }

    public ActionResult ShowFirstDepartment(int id)
    {
      var departmentNames = OrgMgmtDBHelper.convertUser2DTO(
        db.users.Find(id), db, true).departmentNames;

      var result = "";

      if (departmentNames.Count() > 0)
      {
        result = departmentNames[0];
      }

      return Content(result, "text/html");

    }

    public ActionResult DisplayNameById(int id)
    {
      var result = "";
      result = db.users.Find(id).name;

      return Content(result, "text/html");
    }







    // GET: User/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: User/Delete/5
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
      OPASDb.Dispose();
      base.Dispose(disposing);
    }
  }
}
