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
  public class PurchaseOrderFlowDataDTO
  {
    public FlowDocumentDataDTO flowDocumentData { get; set; }
    #region Biz Data
    public int purchaseOrderId { get; set; }
    public string documentNo { get; set; }
    public string departmentName { get; set; }
    public string departmentNameBelongTo { get; set; }
    public string contactOfficePhone { get; set; }
    public string contactMobile { get; set; }
    public string costCenterName { get; set; }
    public DateTime? orderDate { get; set; }
    public DateTime? effectiveDate { get; set; }
    public string reason { get; set; }
    public string description { get; set; }
    public decimal? totalAmount { get; set; }
    public string currencyTypeName { get; set; }
    public decimal? totalAmountInRMB { get; set; }
    public string POTypeName { get; set; }
    public string vendorName { get; set; }
    public string transportTerm { get; set; }
    public string paymentTerm { get; set; }
    public string submitor { get; set; }
    #endregion
    public List<PurchaseOrderDetailDataDTO> details { get; set; }
  }

  public class PurchaseOrderDetailDataDTO
  {
    public int id { get; set; }
    public string guid { get; set; }
    public int lineNo { get; set; }
    public string itemName { get; set; }
    public string description { get; set; }
    public string unitMeasure { get; set; }
    public decimal price { get; set; }
    public decimal quantity { get; set; }
    public decimal amount { get; set; } // should be quantity*price
    public decimal amountInRMB { get; set; }
    public PurchaseOrderDetailDataDTO(PurchaseOrderDetail detail)
    {
      id = detail.purchaseOrderDetailId;
      guid = detail.guid;
      lineNo = detail.lineNo;
      itemName = detail.itemName;
      description = detail.description;
      price = detail.price;
      quantity = detail.quantity;
      amount = detail.amount;
      amountInRMB = detail.amountInRMB;
    }
  }

  public static class PurchaseOrderDataDTOBuilder
  {
    public static PurchaseOrderFlowDataDTO create(
      FlowInstance flowInstance, 
      FlowTaskForUser flowTaskForUser,
      PurchaseOrder bizDocumentObj)
    {
      FlowDocumentDataDTO flowDocumentData = 
        new FlowDocumentDataDTO(flowTaskForUser, flowInstance);

      List<PurchaseOrderDetailDataDTO> details =
        bizDocumentObj.details.Select(detail => 
          new PurchaseOrderDetailDataDTO(detail)).ToList();

      PurchaseOrderFlowDataDTO purchaseOrderFlowDataDTO = 
        new PurchaseOrderFlowDataDTO() {
          flowDocumentData = flowDocumentData,
          #region Biz Data
          purchaseOrderId = bizDocumentObj.purchaseOrderId,
          documentNo = bizDocumentObj.documentNo,
          departmentName = OrgMgmtDBHelper.getDepartment(
            bizDocumentObj.departmentId)?.name,
          departmentNameBelongTo = OrgMgmtDBHelper.getDepartment(
            bizDocumentObj.departmentIdBelongTo)?.name,
          contactOfficePhone = bizDocumentObj.contactOfficePhone,
          contactMobile = bizDocumentObj.contactMobile,
          costCenterName = OPAS2ModelDBHelper.getCostCenter(
            bizDocumentObj.costCenterId)?.chineseName,
          orderDate = bizDocumentObj.orderDate,
          effectiveDate = bizDocumentObj.effectiveDate,
          reason = bizDocumentObj.reason,
          description = bizDocumentObj.description,
          totalAmount = bizDocumentObj.totalAmount,
          currencyTypeName = (bizDocumentObj.currencyTypeId.HasValue ? 
            OPAS2ModelDBHelper.getCurrencyType(
              bizDocumentObj.currencyTypeId.Value).name : ""),
          totalAmountInRMB = bizDocumentObj.totalAmountInRMB,
          POTypeName = OPAS2EnumsHelper.getPOTypeName(bizDocumentObj.POType),
          vendorName = (bizDocumentObj.vendorId.HasValue ? 
            OPAS2ModelDBHelper.getVendor(
              bizDocumentObj.vendorId.Value).chineseName : ""),
          transportTerm = bizDocumentObj.transportTerm,
          paymentTerm = bizDocumentObj.paymentTerm,
          submitor = bizDocumentObj.submitor,
          #endregion
          details = details
        };

      return purchaseOrderFlowDataDTO;
    }
  }

}