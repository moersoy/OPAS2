using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

using EnouFlowInstanceLib;

namespace OPAS2.Api
{
  public class FlowInstanceApiController : BaseApiController
  {
    // GET api/<controller>/5
    [HttpGet]
    [Route("api/FlowInstance/GetFriendlyLogs/{id}")]
    public IEnumerable<FlowInstanceFriendlyLogDTO> GetFriendlyLogs(
      int id) //FlowInstanceId
    {
      return FlowInstanceHelper.GetFlowInstanceFriendlyLogs(
        id, flowInstDb).Select(obj => obj.convertToDTO());
    }

    [HttpGet]
    [Route("api/FlowInstance/GetFlowTemplateJson/{id}")]
    public string GetFlowTemplateJson(int id)
    {
      return FlowInstanceHelper.GetFlowInstance(
        id, flowInstDb).flowTemplateJson;
    }

    [HttpPost]
    [Route("api/FlowInstance/JumpToActivity/")]
    public IHttpActionResult JumpToActivity()
    {
      dynamic bizObj = getPostedJsonObject();

      var flowInstance = FlowInstanceHelper.GetFlowInstance(
        (int)bizObj.flowInstanceId, flowInstDb);
      dynamic bizDocument = getBizDocumentFromFlowInstance(
        flowInstance.bizDocumentTypeCode, flowInstance.bizDocumentGuid);

      #region 执行公用的JumpToFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        JumpToFlowActionGeneral(
          bizObj, bizDocument, flowInstance.bizDocumentTypeCode,
          null, null, 
          (int)bizObj.currentUserId, bizObj.currentUserGuid, true);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok();
    }

    [HttpPost]
    [Route("api/FlowInstance/Terminate/")]
    public IHttpActionResult Terminate()
    {
      dynamic bizObj = getPostedJsonObject();

      var flowInstance = FlowInstanceHelper.GetFlowInstance(
        (int)bizObj.flowInstanceId, flowInstDb);
      dynamic bizDocument = getBizDocumentFromFlowInstance(
        flowInstance.bizDocumentTypeCode, flowInstance.bizDocumentGuid);

      #region 执行公用的TerminateFlowAction处理
      Tuple<bool, IHttpActionResult> generalResult =
        TerminateFlowActionGeneral(
          bizObj, bizDocument, flowInstance.bizDocumentTypeCode,
          null, null,
          (int)bizObj.currentUserId, bizObj.currentUserGuid);
      if (!generalResult.Item1)
      {
        return generalResult.Item2;
      }
      #endregion

      return Ok();
    }

    private dynamic getBizDocumentFromFlowInstance(
      string bizDocumentTypeCode, string bizDocumentGuid)
    {
      
      switch (bizDocumentTypeCode)
      {
        case "PR":
          return db.purchaseReqs.Where(obj => 
            obj.guid == bizDocumentGuid).FirstOrDefault();
        case "PO":
          return db.purchaseOrders.Where(obj =>
            obj.guid == bizDocumentGuid).FirstOrDefault();
        case "GR":
          return db.goodsReceivings.Where(obj =>
            obj.guid == bizDocumentGuid).FirstOrDefault();
        case "PM":
          return db.payments.Where(obj =>
            obj.guid == bizDocumentGuid).FirstOrDefault();
        default:
          throw new Exception("Unknown Document Type");
      }
    }
  }
}