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
    public EnumFlowTaskType taskType { get; set; }
    public EnumFlowTaskState taskState { get; set; }
    public string documentTypeName { get; set; }
    public string documentTypeCode { get; set; }
    public string documentNo { get; set; }
    public string documentSubject { get; set; }
    public int departmentId { get; set; }
    public int creatorUserId { get; set; }
    public int taskOperationUserId { get; set; }
    public DateTime createTime { get; set; }
    public int? intField_1 { get; set; }
    public int? intField_2 { get; set; }
    public int? intField_3 { get; set; }
    public int? intField_4 { get; set; }
    public int? intField_5 { get; set; }
    public int? intField_6 { get; set; }
    public int? intField_7 { get; set; }
    public int? intField_8 { get; set; }
    public int? intField_9 { get; set; }
    public int? intField_10 { get; set; }
    public string stringField_1 { get; set; }
    public string stringField_2 { get; set; }
    public string stringField_3 { get; set; }
    public string stringField_4 { get; set; }
    public string stringField_5 { get; set; }
    public string stringField_6 { get; set; }
    public string stringField_7 { get; set; }
    public string stringField_8 { get; set; }
    public string stringField_9 { get; set; }
    public string stringField_10 { get; set; }
    public decimal? decimalField_1 { get; set; }
    public decimal? decimalField_2 { get; set; }
    public decimal? decimalField_3 { get; set; }
    public decimal? decimalField_4 { get; set; }
    public decimal? decimalField_5 { get; set; }
    public decimal? decimalField_6 { get; set; }
    public decimal? decimalField_7 { get; set; }
    public decimal? decimalField_8 { get; set; }
    public decimal? decimalField_9 { get; set; }
    public decimal? decimalField_10 { get; set; }
    public DateTime? dateTimeField_1 { get; set; }
    public DateTime? dateTimeField_2 { get; set; }
    public DateTime? dateTimeField_3 { get; set; }
    public DateTime? dateTimeField_4 { get; set; }
    public DateTime? dateTimeField_5 { get; set; }

    public FlowTask(FlowTaskForUser flowTaskForUser, 
      OPAS2DbContext oPAS2Db)
    {
      #region 填充任务基本字段
      bizDocumentGuid = flowTaskForUser.bizDocumentGuid;
      flowTaskForUserId = flowTaskForUser.flowTaskForUserId;
      flowTaskForUserGuid = flowTaskForUser.guid;
      taskOperationUserId = flowTaskForUser.userId;
      taskType = flowTaskForUser.taskType;
      taskState = flowTaskForUser.taskState;
      documentTypeCode = flowTaskForUser.bizDocumentTypeCode;
      documentTypeName = flowTaskForUser.bizDocumentTypeCode;
      #endregion

      #region 填充任务的自定义字段
      intField_1 = flowTaskForUser.intField_1;
      intField_2 = flowTaskForUser.intField_2;
      intField_3 = flowTaskForUser.intField_3;
      intField_4 = flowTaskForUser.intField_4;
      intField_5 = flowTaskForUser.intField_5;
      intField_6 = flowTaskForUser.intField_6;
      intField_7 = flowTaskForUser.intField_7;
      intField_8 = flowTaskForUser.intField_8;
      intField_9 = flowTaskForUser.intField_9;
      intField_10 = flowTaskForUser.intField_10;
      stringField_1 = flowTaskForUser.stringField_1;
      stringField_2 = flowTaskForUser.stringField_2;
      stringField_3 = flowTaskForUser.stringField_3;
      stringField_4 = flowTaskForUser.stringField_4;
      stringField_5 = flowTaskForUser.stringField_5;
      stringField_6 = flowTaskForUser.stringField_6;
      stringField_7 = flowTaskForUser.stringField_7;
      stringField_8 = flowTaskForUser.stringField_8;
      stringField_9 = flowTaskForUser.stringField_9;
      stringField_10 = flowTaskForUser.stringField_10;
      decimalField_1 = flowTaskForUser.decimalField_1;
      decimalField_2 = flowTaskForUser.decimalField_2;
      decimalField_3 = flowTaskForUser.decimalField_3;
      decimalField_4 = flowTaskForUser.decimalField_4;
      decimalField_5 = flowTaskForUser.decimalField_5;
      decimalField_6 = flowTaskForUser.decimalField_6;
      decimalField_7 = flowTaskForUser.decimalField_7;
      decimalField_8 = flowTaskForUser.decimalField_8;
      decimalField_9 = flowTaskForUser.decimalField_9;
      decimalField_10 = flowTaskForUser.decimalField_10;
      dateTimeField_1 = flowTaskForUser.dateTimeField_1;
      dateTimeField_2 = flowTaskForUser.dateTimeField_2;
      dateTimeField_3 = flowTaskForUser.dateTimeField_3;
      dateTimeField_4 = flowTaskForUser.dateTimeField_4;
      dateTimeField_5 = flowTaskForUser.dateTimeField_5;
      #endregion

      #region 根据业务单据类型代码定位到对应的业务单据表填充审批任务的相关字段
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
      #endregion
    }

    private FlowTask() { }
  }
}