using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using EnouFlowOrgMgmtLib;
using System.Dynamic;

namespace OPAS2.Api
{
  public class UserApiController : BaseApiController
  {
    // GET api/<controller>
    public IEnumerable<UserDTO> Get()
    {
      return OrgMgmtDBHelper.getAllUserDTOs(orgDb, true);
    }

    [ResponseType(typeof(UserDTO))]
    [HttpPost]
    [Route("api/User/Login/")]
    public IHttpActionResult Login()
    {
      dynamic bizObj = getPostedJsonObject();
      if (hasAttr((ExpandoObject)bizObj, "password") &&
        !string.IsNullOrWhiteSpace(bizObj.password) &&
        hasAttr((ExpandoObject)bizObj, "username") &&
        !string.IsNullOrWhiteSpace(bizObj.username))
      {
        User user = OrgMgmtDBHelper.logonUser(
          bizObj.username, bizObj.password, orgDb);
        if (user != null) // 成功登陆, 创建并返回Token用于未来访问其他API
        {
          var token = db.userAuthenticationTokens.Create();
          token.userGuid = user.guid;
          token.userId = user.userId;
          db.userAuthenticationTokens.Add(token);
          db.SaveChanges();

          return (Ok(new {
            status = "success",
            user = new
            {
              authenticationToken = token.guid,
              userId = token.userId,
              userGuid = token.userGuid,
              userLogonName = user.logonName,
              userDisplayName = user.name,
              
              authenticationTokenExpireTime = token.expireTime
            }
          }));
        }
      }

      return Unauthorized();
      //return Ok((UserDTO)null);
    }

    // GET api/<controller>/5
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<controller>
    public void Post([FromBody]string value)
    {
    }

    // PUT api/<controller>/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/<controller>/5
    public void Delete(int id)
    {
    }
  }
}