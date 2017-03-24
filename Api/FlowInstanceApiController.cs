using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using EnouFlowInstanceLib;

namespace OPAS2.Api
{
  public class FlowInstanceApiController : BaseApiController
  {
    // GET api/<controller>/5
    public IEnumerable<FlowInstanceFriendlyLogDTO> GetFriendlyLogs(
      int id) //FlowInstanceId
    {
      using (var db = new EnouFlowInstanceContext())
      {
        return FlowInstanceHelper.GetFlowInstanceFriendlyLogs(id,db).
          Select(obj=>obj.convertToDTO());
      }
    }
  }
}