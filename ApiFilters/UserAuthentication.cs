using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using OPAS2Model;

namespace OPAS2.ApiFilters
{
  public class UserAuthentication : Attribute, IActionFilter
  {
    public Task<HttpResponseMessage> ExecuteActionFilterAsync(
      HttpActionContext actionContext,
      CancellationToken cancellationToken,
      Func<Task<HttpResponseMessage>> continuation)
    {
      var headers = actionContext.Request.Headers;
      if (headers.Contains("Authentication-Info") &&
        isValidUserToken(actionContext,
          headers.GetValues("Authentication-Info").FirstOrDefault()))
      { 
        return continuation();
      }
      else
      {
        HttpResponseMessage response = actionContext.Request.
        CreateErrorResponse(HttpStatusCode.Unauthorized,
          "Need login or login again.");
        return Task.FromResult<HttpResponseMessage>(response);
      }
    }

    private bool isValidUserToken(
      HttpActionContext actionContext, string token)
    {
      using (OPAS2DbContext db = new OPAS2DbContext())
      {
        var userAuthenticationToken =
          db.userAuthenticationTokens.Where(
            obj => obj.guid == token && obj.expireTime>DateTime.Now
            ).FirstOrDefault();
        if (userAuthenticationToken != null)
        {
          actionContext.Request.Properties["userId"] = userAuthenticationToken.userId;
          actionContext.Request.Properties["userGuid"] = userAuthenticationToken.userGuid;
          return true;
        }
      }

      return false;
    }

    public bool AllowMultiple
    {
      get { return true; }
    }
  }
}