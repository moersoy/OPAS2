using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EnouFlowTemplateLib;
using EnouFlowInstanceLib;
using OPAS2Model;

namespace OPAS2.Models
{
  public class FlowDocumentDataDTO
  {
    public string currentUserGuid { get; set; }
    public string guid { get; set; } //DocumentId
    public string taskGuid { get; set; }
    public int flowInstanceId { get; set; }
    public string remarkOfAprrover { get; set; }
    public string currentActivityGuid { get; set; }
    public FlowDocumentSessionData sessionData { get; set; }

    public FlowDocumentDataDTO(
      FlowTaskForUser flowTaskForUser,
      FlowInstance flowInstance)
    {
      currentUserGuid = flowTaskForUser.userGuid;
      guid = flowTaskForUser.bizDocumentGuid;
      taskGuid = flowTaskForUser.guid;
      flowInstanceId = flowTaskForUser.flowInstanceId;
      remarkOfAprrover = "";
      currentActivityGuid = flowTaskForUser.currentActivityGuid;
      sessionData = new FlowDocumentSessionData(flowInstance);
    }
  }

  public class FlowDocumentSessionData
  {
    public string flowTemplateDef { get; set; }
    public List<AvailableFlowAction> availableFlowConnections { get; set; }
    public List<FlowDocumentAttachFile> flowDocumentAttachFiles { get; set; }

    public FlowDocumentSessionData(FlowInstance flowInstance)
    {
      flowTemplateDef = flowInstance.flowTemplateJson;
      availableFlowConnections = AvailableFlowAction.
        getCurrentActivityOutBoundAvailableFlowActions(
          flowInstance.flowTemplateJson, flowInstance.currentActivityGuid);
      using(OPAS2DbContext db = new OPAS2DbContext())
      {
        flowDocumentAttachFiles = db.attachFiles.Where(obj => 
          obj.bizDocumentGuid == flowInstance.bizDocumentGuid).Select(
          obj => new FlowDocumentAttachFile() {
            name = obj.originalName,
            guid = obj.guid
          }).ToList();
      }
    }
  }

  public class FlowDocumentAttachFile
  {
    public string name { get; set; }
    public string guid { get; set; }
  }
}