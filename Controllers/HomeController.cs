using OPAS2.Filters;
using OPAS2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPAS2.Controllers
{
  public class HomeController : BaseController
  {
    public HomeController(): base()
    {
      ViewBag.currentMenuIndex = "SYS-USER";
    }

    [UserLogon]
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult SetCulture(string culture)
    {
      // Validate input
      culture = CultureHelper.GetImplementedCulture(culture);
      // Save culture in a cookie
      HttpCookie cookie = Request.Cookies["_culture"];

      if (cookie != null)
        cookie.Value = culture;   // update cookie value
      else
      {
        cookie = new HttpCookie("_culture");
        cookie.Value = culture;
        cookie.Expires = DateTime.Now.AddYears(1);
      }
      Response.Cookies.Add(cookie);

      return RedirectToAction("Index");
    }

    public ActionResult About()
    {
      ViewBag.Message = "Your application description page.";

      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Your contact page.";

      return View();
    }
  }
}