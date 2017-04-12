using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using EnouFlowInstanceLib;
using EnouFlowOrgMgmtLib;
using OPAS2.ApiFilters;
using OPAS2.Models;

namespace OPAS2.Api
{
  public class FlowTaskApiController : BaseApiController
  {
    // GET api/<controller>
    [UserAuthentication]
    [HttpGet]
    [Route("api/FlowTask/")]
    public IEnumerable<FlowTask> Get()
    {
      string userGuid = (string)(Request.Properties["userGuid"]);
      var tasks = FlowInstanceHelper.GetFlowTaskForUserListOfUser(
        userGuid, flowInstDb).Where(task => 
          task.taskType == EnumFlowTaskType.normal ||
          task.taskType == EnumFlowTaskType.delegation || 
          task.taskType == EnumFlowTaskType.invitation).ToList();
      return tasks.Select(t => new FlowTask(t, db));
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