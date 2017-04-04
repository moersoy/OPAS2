using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using EnouFlowInstanceLib;
using OPAS2.ApiFilters;

namespace OPAS2.Api
{
  public class FlowTaskForUserApiController : BaseApiController
  {
    // GET api/<controller>
    [UserAuthentication]
    [HttpGet]
    [Route("api/FlowTaskForUser/")]
    public IEnumerable<FlowTaskForUser> Get()
    {
      return null;
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