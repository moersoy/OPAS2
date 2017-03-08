using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EnouFlowInstanceLib;

namespace OPAS2.Models
{
  public static class TypeSelectListsInFlow
  {
    public static List<dynamic> FlowTaskTypeList = 
      new List<dynamic>() {
        new {id=(int) EnumFlowTaskType.normal ,
          name ="Normal/普通审批"},
        new {id=(int) EnumFlowTaskType.invitation,
          name ="Invite opinion/征询意见"},
        new {id=(int) EnumFlowTaskType.redraft,
          name ="Redraft/重新起草"},
        new {id=(int) EnumFlowTaskType.delegation,
          name ="Delegation/代理审批"},
        new {id=(int) EnumFlowTaskType.invitationFeedback,
          name ="Opinion feedback/征询意见回复"},
    };
  }
}