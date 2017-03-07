using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;

using OPAS2Model;

namespace OPAS2.Controllers
{
  public class AttachFileController : BaseController
  {
    private static string AttachFileStoredPath =
      ConfigurationManager.AppSettings["OPAS_AttachFileStoredPath"];
    private OPAS2DbContext db = new OPAS2DbContext();

    public ActionResult Download(string id)
    {
      var attachFile = db.attachFiles.Where(
        obj => obj.guid == id).FirstOrDefault();
      return File(
        Path.Combine(attachFile.serverPath, attachFile.serverName), 
        "application/octet-stream", 
        attachFile.originalName);
    }

    // GET: AttachFile
    public PartialViewResult Index(string bizDocumentGuid)
    {
      var attachFiles = db.attachFiles.Where(
        obj => obj.bizDocumentGuid == bizDocumentGuid);
      return PartialView(attachFiles);
    }

    // GET: AttachFile/Details/5
    public ActionResult Details(int id)
    {
      return View();
    }

    // GET: AttachFile/Create
    public ActionResult Create()
    {
      return View();
    }

    // POST: AttachFile/Create
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

    // GET: AttachFile/Edit/5
    public ActionResult Edit(int id)
    {
      return View();
    }

    // POST: AttachFile/Edit/5
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

    // GET: AttachFile/Delete/5
    public ActionResult Delete(int id)
    {
      return View();
    }

    // POST: AttachFile/Delete/5
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
