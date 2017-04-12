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

    public static List<AvailableFlowAction> 
      getCurrentActivityOutBoundAvailableFlowActions(
      string flowTemplateJson, 
      string currentActivityGuid)
    {
      FlowTemplateDefHelper flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowTemplateJson);

      ActivityNode fromNode = flowTemplateDefHelper.getNodeFromGuid(
        currentActivityGuid);
      if (fromNode != null)
      { // 获取可用的从当前状态节点出发的所有连接和目的状态节点
        return getNodeOutBoundAvailableFlowActions(
           fromNode, flowTemplateDefHelper);
      }
      else
      {
        throw new Exception("未找到指定的当前流程实例节点");
      }
    }

    private AvailableFlowAction() { }
  }
}