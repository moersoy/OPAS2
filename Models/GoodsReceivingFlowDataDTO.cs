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
  // 将包含收货的业务数据与流程操作相关的数据，目前用于移动端查看审批
  public class GoodsReceivingFlowDataDTO
  {
    public FlowDocumentDataDTO flowDocumentData { get; set; }
    #region Biz Data
    public int goodsReceivingId { get; set; }
    public string documentNo { get; set; }
    public string departmentName { get; set; }
    public string description { get; set; }
    public decimal? totalAmount { get; set; }
    public decimal? totalAmountInRMB { get; set; }
    public string vendorName { get; set; }
    public string submitor { get; set; }
    #endregion
    public List<GoodsReceivingDetailDataDTO> details { get; set; }
  }

  public class GoodsReceivingDetailDataDTO
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
    public GoodsReceivingDetailDataDTO(GoodsReceivingDetail detail)
    {
      id = detail.goodsReceivingDetailId;
      guid = detail.guid;
      lineNo = detail.lineNo;
      itemName = detail.itemName;
      price = detail.price;
      quantity = detail.quantity;
      amount = detail.amount;
      amountInRMB = detail.amountInRMB;
    }
  }

  public static class GoodsReceivingDataDTOBuilder
  {
    public static GoodsReceivingFlowDataDTO create(
      FlowInstance flowInstance, 
      FlowTaskForUser flowTaskForUser,
      GoodsReceiving bizDocumentObj)
    {
      FlowDocumentDataDTO flowDocumentData = 
        new FlowDocumentDataDTO(flowTaskForUser, flowInstance);

      List<GoodsReceivingDetailDataDTO> details =
        bizDocumentObj.details.Select(detail => 
          new GoodsReceivingDetailDataDTO(detail)).ToList();

      GoodsReceivingFlowDataDTO GoodsReceivingFlowDataDTO = 
        new GoodsReceivingFlowDataDTO() {
          flowDocumentData = flowDocumentData,
          #region Biz Data
          goodsReceivingId = bizDocumentObj.goodsReceivingId,
          documentNo = bizDocumentObj.documentNo,
          departmentName = OrgMgmtDBHelper.getDepartment(
            bizDocumentObj.departmentId)?.name,
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

      return GoodsReceivingFlowDataDTO;
    }
  }

}