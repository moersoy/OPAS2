using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EnouFlowInstanceLib;
using EnouFlowTemplateLib;
using EnouFlowOrgMgmtLib;
using System.Dynamic;
using OPAS2Model;

namespace OPAS2.Models
{
  // 将包含采购申请的业务数据与流程操作相关的数据，目前用于移动端查看审批
  public class PurchaseReqFlowDataDTO
  {
    public FlowDocumentDataDTO flowDocumentData { get; set; }
    #region Biz Data
    public int purchaseReqId { get; set; }
    public string documentNo { get; set; }
    public string WBSNo { get; set; }
    //departmentId: number;
    public string departmentName { get; set; }
    //departmentIdBelongTo: number;
    public string departmentNameBelongTo { get; set; }
    public string contactOfficePhone { get; set; }
    public string contactMobile { get; set; }
    public string contactOtherMedia { get; set; }
    //costCenterId: number;
    public string costCenterName { get; set; }
    public DateTime? expectReceiveBeginTime { get; set; }
    public DateTime? expectReceiveEndTime { get; set; }
    public bool isFirstBuy { get; set; }
    public DateTime? firstBuyDate { get; set; }
    public bool isBidingRequired { get; set; }
    public string noBiddingReason { get; set; }
    public string reason { get; set; }
    public string description { get; set; }
    public decimal? estimatedCostInRMB { get; set; }
    public decimal? averageBenchmark { get; set; }
    public string benchmarkDescription { get; set; }
    public decimal? firstCostAmount { get; set; }
    public string firstBuyDescription { get; set; }
    public string otherVendorsNotInList { get; set; }
    #endregion
    public List<PurchaseReqDetailDataDTO> details { get; set; }
  }

  public class PurchaseReqDetailDataDTO
  {
    public int id { get; set; }
    public string guid { get; set; }
    public int lineNo { get; set; }
    public string itemTypeName { get; set; }
    public string itemName { get; set; }
    public decimal? estimatedCost { get; set; }
    public string description { get; set; }

    public PurchaseReqDetailDataDTO(PurchaseReqDetail detail)
    {
      id = detail.purchaseReqDetailId;
      guid = detail.guid;
      lineNo = detail.lineNo;
      itemTypeName = OPAS2EnumsHelper.getPRItemTypeName(detail.itemType);
      itemName = detail.itemName;
      estimatedCost = detail.estimatedCost;
      description = detail.description;
    }
  }

  public static class PurchaseReqDataDTOBuilder
  {
    public static PurchaseReqFlowDataDTO create(
      FlowInstance flowInstance, 
      FlowTaskForUser flowTaskForUser,
      PurchaseReq bizDocumentObj)
    {
      FlowDocumentDataDTO flowDocumentData = 
        new FlowDocumentDataDTO(flowTaskForUser, flowInstance);

      List<PurchaseReqDetailDataDTO> details =
        bizDocumentObj.details.Select(detail => 
          new PurchaseReqDetailDataDTO(detail)).ToList();

      PurchaseReqFlowDataDTO purchaseReqFlowDataDTO = 
        new PurchaseReqFlowDataDTO() {
          flowDocumentData = flowDocumentData,
          #region Biz Data
          purchaseReqId = bizDocumentObj.purchaseReqId,
          documentNo = bizDocumentObj.documentNo,
          WBSNo = bizDocumentObj.WBSNo,
          departmentName = OrgMgmtDBHelper.getDepartment(
            bizDocumentObj.departmentId).name,
          departmentNameBelongTo = OrgMgmtDBHelper.getDepartment(
            bizDocumentObj.departmentIdBelongTo).name,
          contactOfficePhone = bizDocumentObj.contactOfficePhone,
          contactMobile = bizDocumentObj.contactMobile,
          contactOtherMedia = bizDocumentObj.contactOtherMedia,
          costCenterName = OPAS2ModelDBHelper.getCostCenter(
            bizDocumentObj.costCenterId).chineseName,
          expectReceiveBeginTime = bizDocumentObj.expectReceiveBeginTime,
          expectReceiveEndTime = bizDocumentObj.expectReceiveEndTime,
          isFirstBuy = bizDocumentObj.isFirstBuy,
          firstBuyDate = bizDocumentObj.firstBuyDate,
          isBidingRequired = bizDocumentObj.isBidingRequired,
          noBiddingReason = bizDocumentObj.noBiddingReason,
          reason = bizDocumentObj.reason,
          description = bizDocumentObj.description,
          estimatedCostInRMB = bizDocumentObj.estimatedCostInRMB,
          averageBenchmark = bizDocumentObj.averageBenchmark,
          benchmarkDescription = bizDocumentObj.benchmarkDescription,
          firstCostAmount = bizDocumentObj.firstCostAmount,
          firstBuyDescription = bizDocumentObj.firstBuyDescription,
          otherVendorsNotInList = bizDocumentObj.otherVendorsNotInList,
          #endregion
          details = details
        };

      return purchaseReqFlowDataDTO;
    }
  }

}