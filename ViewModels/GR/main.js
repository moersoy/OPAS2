const v = new Vue({
  el: '#app',
  data: {
    workingMode: initBag.workingMode, // backend will fill this variable
    removeAttachPath: initBag.removeAttachPath, //删除附件的URL路径
    detailsName: "GRDetails",
    newItem: { // 新建GR的数据
      // 流程部分
      currentUserGuid: initBag.currentUserGuid,
      guid: initBag.guid, //DocumentId
      purchaseOrderId: initBag.purchaseOrderId, //前驱的PO
      goodsReceivingId: initBag.goodsReceivingId,
      PODocumentNo: initBag.PODocumentNo,
      departmentId: initBag.departmentId,
      flowTemplateGuid: initBag.flowTemplateGuid,
      currentActivityGuid: initBag.currentActivityGuid,
      selectedConnectionGuid: '',
      selectedPaticipantGuid: '',
      taskGuid: initBag.taskGuid,
      flowInstanceId: initBag.flowInstanceId,
      // 业务数据
      description: initBag.bizData.description || '',
      receiver: initBag.bizData.receiver || '', // 收货人
      checker: initBag.bizData.checker || '', // 验货人
      deliveryDate: initBag.bizData.deliveryDate || null, // 交货日期
      deliveryLocation: initBag.bizData.deliveryLocation || '', // 交货地点
      shippingInfo: initBag.bizData.shippingInfo || '', // 运输信息
      trackingNo: initBag.bizData.trackingNo || '', // 运单号
      weight: initBag.bizData.weight || '0', // 重量
      packingSlipNo: initBag.bizData.packingSlipNo || '', // 包装单号
      // 子表明细项部分
      GRDetails: initBag.GRDetails,
      // 仅在前台操作中用于辅助的临时数据,不属于业务实体,无需回传
      sessionData: {
        availableFlowConnections: initBag.availableFlowConnections,
        potentialPaticipants: null,
        CreateWithFlowActionPath: initBag.GRCreateWithFlowActionPath,
        NextFlowActionPath: initBag.GRNextFlowActionPath,
        needChoosePaticipant: true,
      }
    },
    examineItem: {// 审批GR的数据
      currentUserGuid: initBag.currentUserGuid,
      guid: initBag.guid, //DocumentId
      purchaseOrderId: initBag.purchaseOrderId,
      goodsReceivingId: initBag.goodsReceivingId,
      taskGuid: initBag.taskGuid,
      PODocumentNo: initBag.PODocumentNo,
      flowInstanceId: initBag.flowInstanceId,
      remarkOfAprrover: "",
      // 流程部分
      currentActivityGuid: initBag.currentActivityGuid,
      selectedConnectionGuid: '',
      selectedPaticipantGuid: '',
      // 仅在前台操作中用于辅助的临时数据,不属于业务实体
      sessionData: {
        availableFlowConnections: initBag.availableFlowConnections,
        potentialPaticipants: null,
        NextFlowActionPath: initBag.GRNextFlowActionPath,
        RejectToStartFlowActionPath: initBag.GRRejectToStartFlowActionPath,
        needChoosePaticipant: true,
      }
    },
    masterData: {
    },
  },
  mixins: [opas_vue_biz_document_mixin],
  methods: {
  }
});
