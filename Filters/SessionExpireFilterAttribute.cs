using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using EnouFlowOrgMgmtLib;

namespace OPAS2.Filters
{
  public class UserLogonAttribute : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      HttpContext ctx = HttpContext.Current;

      // check if session is supported
      UserDTO currentUserDTO = (UserDTO)ctx.Session["currentUserDTO"];
      if (currentUserDTO == null)
      {
        filterContext.Result = new RedirectResult("~/User/Logon");
        return;
      }

      base.OnActionExecuting(filterContext);
    }
  }
}