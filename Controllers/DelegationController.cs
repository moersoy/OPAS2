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
  public class DelegationController : BaseController
  {
    public DelegationController() : base()
    {
      ViewBag.currentMenuIndex = "MY-DELEGATION";
    }

    private OPAS2DbContext db = new OPAS2DbContext();

    // GET: Delegation
    [UserLogon]
    public ActionResult Index()
    {
      var userDTO = (UserDTO)Session["currentUserDTO"];
      return View(DelegationHistoryRecord.
        getDelegationHistoryRecords(
          userDTO.userId, db));
    }

    // GET: Delegation/Create
    [UserLogon]
    public ActionResult Create()
    {
      var obj = db.delegationHistoryRecords.Create();
      //obj.effectiveTimeFrom = DateTime.Now.AddDays(1);
      //obj.effectiveTimeTo = DateTime.Now.AddDays(2);
      PrepareSelectLists();
      return View(obj);
    }

    // POST: Delegation/Create
    [UserLogon]
    [HttpPost]
    public ActionResult Create(FormCollection collection)
    {
      DelegationHistoryRecord obj = null;
      try
      {
        BizSettings.BizDocumentTypeList.ForEach(bizDocumentType => {
          obj = db.delegationHistoryRecords.Create();
          obj.bizDocumentType = bizDocumentType.Item1;
          obj.delegateeUserId = int.Parse(collection["delegateeUserId"]);
          var delegateeUserDTO = OrgMgmtDBHelper.getUserDTO(obj.delegateeUserId);
          obj.delegatorUserGuid = delegateeUserDTO.guid;

          var userDTO = (UserDTO)Session["currentUserDTO"];
          obj.delegatorUserId = userDTO.userId;
          obj.delegatorUserGuid = userDTO.guid;
          obj.creatorUserId = userDTO.userId;
          obj.creatorUserGuid = userDTO.guid;

          obj.effectiveTimeFrom = DateTime.Parse(collection["effectiveTimeFrom"]);
          obj.effectiveTimeTo = DateTime.Parse(collection["effectiveTimeTo"]);

          db.delegationHistoryRecords.Add(obj);
        });

        db.SaveChanges();

        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        ViewBag.backendError = ex.Message;
        PrepareSelectLists();
        return View(obj);
      }
    }

    // GET: Delegation/Edit/5
    [UserLogon]
    public ActionResult Edit(int id)
    {
      DelegationHistoryRecord obj = 
        db.delegationHistoryRecords.Find(id);
      PrepareSelectLists();

      return View(obj);
    }

    // POST: Delegation/Edit/5
    [UserLogon]
    [HttpPost]
    public ActionResult Edit(int id, FormCollection collection)
    {
      DelegationHistoryRecord obj =
        db.delegationHistoryRecords.Find(id);
      try
      {
        obj.delegateeUserId = int.Parse(collection["delegateeUserId"]);
        var delegateeUserDTO = OrgMgmtDBHelper.getUserDTO(obj.delegateeUserId);
        obj.delegatorUserGuid = delegateeUserDTO.guid;
        obj.effectiveTimeFrom = DateTime.Parse(collection["effectiveTimeFrom"]);
        obj.effectiveTimeTo = DateTime.Parse(collection["effectiveTimeTo"]);
        db.SaveChanges();
        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        ViewBag.backendError = ex.Message;
        PrepareSelectLists();
        return View(obj);
      }
    }

    // GET: Delegation/Delete/5
    public ActionResult Delete(int id)
    {
      DelegationHistoryRecord obj =
        db.delegationHistoryRecords.Find(id);
      try
      {
        obj.isVisible = false;
        db.SaveChanges();
      }
      catch (Exception ex)
      {
        ViewBag.backendError = ex.Message;
      }
      return RedirectToAction("Index");
    }

    private void PrepareSelectLists()
    {
      using (var orgDb = new EnouFlowOrgMgmtContext())
      {
        SetSelectListOfUser(orgDb);
      }
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}