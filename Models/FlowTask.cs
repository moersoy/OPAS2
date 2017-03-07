using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OPAS2Model;
using EnouFlowInstanceLib;

namespace OPAS2.Models
{
  public class FlowTask
  {
    public string bizDocumentGuid { get; set; }
    public int flowTaskForUserId { get; set; }
    public string flowTaskForUserGuid { get; set; }
    public string documentTypeName { get; set; }
    public string documentTypeCode { get; set; }
    public string documentNo { get; set; }
    public string documentSubject { get; set; }
    public int departmentId { get; set; }
    public int creatorUserId { get; set; }
    public DateTime createTime { get; set; }

    public FlowTask(FlowTaskForUser flowTaskForUser, 
      OPAS2DbContext oPAS2Db)
    {
      bizDocumentGuid = flowTaskForUser.bizDocumentGuid;
      flowTaskForUserId = flowTaskForUser.flowTaskForUserId;
      flowTaskForUserGuid = flowTaskForUser.guid;
      documentTypeCode = flowTaskForUser.bizDocumentTypeCode;
      documentTypeName = flowTaskForUser.bizDocumentTypeCode;

      // 根据业务单据类型代码定位到对应的业务单据表填充审批任务的相关字段
      switch (documentTypeCode)
      {
        case "PR":
          var bizDocumentPR = oPAS2Db.purchaseReqs.Where(
            obj => obj.guid == bizDocumentGuid).FirstOrDefault();
          documentNo = bizDocumentPR.documentNo;
          documentSubject = bizDocumentPR.reason;
          departmentId = bizDocumentPR.departmentId;
          creatorUserId = bizDocumentPR.creatorUserId;
          createTime = bizDocumentPR.createTime;
          break;
        case "PO":
          var bizDocumentPO = oPAS2Db.purchaseOrders.Where(
            obj => obj.guid == bizDocumentGuid).FirstOrDefault();
          documentNo = bizDocumentPO.documentNo;
          documentSubject = bizDocumentPO.reason;
          departmentId = bizDocumentPO.departmentId;
          creatorUserId = bizDocumentPO.creatorUserId;
          createTime = bizDocumentPO.createTime;
          break;
        case "GR":
          var bizDocumentGR = oPAS2Db.goodsReceivings.Where(
            obj => obj.guid == bizDocumentGuid).FirstOrDefault();
          documentNo = bizDocumentGR.documentNo;
          documentSubject = bizDocumentGR.PurchaseOrder.reason; //使用对应PO的主题
          departmentId = bizDocumentGR.departmentId;
          creatorUserId = bizDocumentGR.creatorUserId;
          createTime = bizDocumentGR.createTime;
          break;
        case "PM":
          var bizDocumentPM = oPAS2Db.payments.Where(
            obj => obj.guid == bizDocumentGuid).FirstOrDefault();
          documentNo = bizDocumentPM.documentNo;
          documentSubject = bizDocumentPM.reason; 
          departmentId = bizDocumentPM.departmentId;
          creatorUserId = bizDocumentPM.creatorUserId;
          createTime = bizDocumentPM.createTime;
          break;
        default:
          throw new Exception(
            "Unexpected documentTypeCode: " + documentTypeCode);
      }

    }

    private FlowTask() { }
  }
}