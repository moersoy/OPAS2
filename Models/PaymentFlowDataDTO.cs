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
  // 将包含付款申请的业务数据与流程操作相关的数据，目前用于移动端查看审批
  public class PaymentFlowDataDTO
  {
    public FlowDocumentDataDTO flowDocumentData { get; set; }
    #region Biz Data
    public int paymentId { get; set; }
    public string documentNo { get; set; }
    public string departmentName { get; set; }
    public string reason { get; set; }
    public string description { get; set; }
    public decimal? totalAmount { get; set; }
    public decimal? totalAmountInRMB { get; set; }
    public string vendorName { get; set; }
    public string submitor { get; set; }
    #endregion
    public List<PaymentDetailDataDTO> details { get; set; }
  }

  public class PaymentDetailDataDTO
  {
    public int id { get; set; }
    public string guid { get; set; }
    public int lineNo { get; set; }
    public string itemName { get; set; }
    public string unitMeasure { get; set; }
    public decimal price { get; set; }
    public decimal quantity { get; set; }
    public decimal amount { get; set; } // should be quantity*price
    public decimal amountInRMB { get; set; }
    public PaymentDetailDataDTO(PaymentDetail detail)
    {
      id = detail.paymentDetailDetailId;
      guid = detail.guid;
      lineNo = detail.lineNo;
      itemName = detail.itemName;
      price = detail.price;
      quantity = detail.quantity;
      amount = detail.amount;
      amountInRMB = detail.amountInRMB;
    }
  }

  public static class PaymentDataDTOBuilder
  {
    public static PaymentFlowDataDTO create(
      FlowInstance flowInstance, 
      FlowTaskForUser flowTaskForUser,
      Payment bizDocumentObj)
    {
      FlowDocumentDataDTO flowDocumentData = 
        new FlowDocumentDataDTO(flowTaskForUser, flowInstance);

      List<PaymentDetailDataDTO> details =
        bizDocumentObj.details.Select(detail => 
          new PaymentDetailDataDTO(detail)).ToList();

      PaymentFlowDataDTO PaymentFlowDataDTO =
        new PaymentFlowDataDTO() {
          flowDocumentData = flowDocumentData,
          #region Biz Data
          paymentId = bizDocumentObj.paymentId,
          documentNo = bizDocumentObj.documentNo,
          departmentName = OrgMgmtDBHelper.getDepartment(
            bizDocumentObj.departmentId)?.name,
          reason = bizDocumentObj.reason,
          description = bizDocumentObj.description,
          totalAmount = bizDocumentObj.totalAmount,
          totalAmountInRMB = bizDocumentObj.totalAmountInRMB,
          vendorName = (bizDocumentObj.vendorId.HasValue ? 
            OPAS2ModelDBHelper.getVendor(
              bizDocumentObj.vendorId.Value).chineseName : ""),
          submitor = bizDocumentObj.submitor,
          #endregion
          details = details
        };

      return PaymentFlowDataDTO;
    }
  }

}