using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EnouFlowTemplateLib;

namespace OPAS2.Models
{
  public class AvailableFlowAction
  {
    public ActivityConnection connection { get;  }
    public ActivityNode toNode { get;  }
    public bool isValidInDefinition { get; } = false;

    public AvailableFlowAction(ActivityConnection connection, ActivityNode toNode)
    {
      this.connection = connection;
      this.toNode = toNode;
      if (connection?.toGuid == toNode?.guid)
      {
        isValidInDefinition = true;
      }
    }

    public static List<AvailableFlowAction> getNodeOutBoundAvailableFlowActions(
      ActivityNode fromNode, FlowTemplateDefHelper flowTemplateDefHelper)
    {
      var outConns = flowTemplateDefHelper.getOutboundConnectionsOfNode(fromNode);

      return outConns.Select(conn => new AvailableFlowAction(
        conn, flowTemplateDefHelper.getNodeFromGuid(conn.toGuid))).ToList();
    }

    private AvailableFlowAction() { }
  }
}