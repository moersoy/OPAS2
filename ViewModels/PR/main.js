const v = new Vue({
  el: '#app',
  data: {
    workingMode: initBag.workingMode, // backend will fill this global variable
    removeAttachPath: initBag.removeAttachPath, //删除附件的URL路径
    detailsName: "PRDetails",
    newItem: { // 新建PR的数据
      // 流程数据
      flowTemplateGuid: initBag.flowTemplateGuid,
      currentActivityGuid: initBag.currentActivityGuid,
      selectedConnectionGuid: '',
      selectedPaticipantGuid: '',
      currentUserGuid: initBag.currentUserGuid,
      guid: initBag.guid, //DocumentId
      purchaseReqId: initBag.purchaseReqId,
      taskGuid: initBag.taskGuid,
      flowInstanceId: initBag.flowInstanceId,
      // 业务数据
      WBSNo: initBag.bizData.WBSNo || '',
      departmentId: initBag.bizData.departmentId,
      departmentIdBelongTo: initBag.bizData.departmentIdBelongTo,
      contactOfficePhone: initBag.bizData.contactOfficePhone,
      contactMobile: initBag.bizData.contactMobile,
      contactOtherMedia: initBag.bizData.contactMobile || '',
      costCenterId: initBag.bizData.costCenterId,
      expectReceiveBeginTime: initBag.bizData.expectReceiveBeginTime || null,
      expectReceiveEndTime: initBag.bizData.expectReceiveEndTime || null,
      isFirstBuy: initBag.bizData.isFirstBuy || true,
      firstBuyDate: initBag.bizData.firstBuyDate || null,
      isBidingRequired: initBag.bizData.isBidingRequired || true,
      noBiddingReason: initBag.bizData.noBiddingReason || '',
      reason: initBag.bizData.reason || '',
      description: initBag.bizData.description || '',
      estimatedCostInRMB: initBag.bizData.estimatedCostInRMB || 0,
      averageBenchmark: initBag.bizData.averageBenchmark || 0,
      benchmarkDescription: initBag.bizData.benchmarkDescription || '',
      firstCostAmount: initBag.bizData.firstCostAmount || 0,
      firstBuyDescription: initBag.bizData.firstBuyDescription || '',
      otherVendorsNotInList: initBag.bizData.otherVendorsNotInList || '',
      // 明细项子表部分
      PRDetails: initBag.PRDetails,
      // 仅在前台操作中用于辅助的临时数据,不属于业务实体
      sessionData:{
        availableFlowConnections: initBag.availableFlowConnections,
        potentialPaticipants: null,
        CreateWithFlowActionPath: initBag.PRCreateWithFlowActionPath,
        NextFlowActionPath: initBag.PRNextFlowActionPath,
        needChoosePaticipant: true,
      }
    },
    examineItem: {// 审批PR的数据
      currentUserGuid: initBag.currentUserGuid,
      guid: initBag.guid, //DocumentId
      purchaseReqId: initBag.purchaseReqId,
      taskGuid: initBag.taskGuid,
      flowInstanceId: initBag.flowInstanceId,
      // 业务数据
      remarkOfAprrover: "",
      // 流程部分
      currentActivityGuid: initBag.currentActivityGuid,
      selectedConnectionGuid: '',
      selectedPaticipantGuid: '',
      // 仅在前台操作中用于辅助的临时数据,不属于业务实体
      sessionData: {
        availableFlowConnections: initBag.availableFlowConnections,
        potentialPaticipants: null,
        NextFlowActionPath: initBag.PRNextFlowActionPath,
        RejectToStartFlowActionPath: initBag.PRRejectToStartFlowActionPath,
        needChoosePaticipant: true,
      }
    },
    masterData: {
      pRItemTypes: initBag.pRItemTypes,
      departmentList: initBag.departmentList,
      currencyTypeList: initBag.currencyTypeList,
      costCenterList: initBag.costCenterList,
    }
  },
  mixins: [opas_vue_biz_document_mixin],
  methods: {
    getPRItemTypeName(id) {
      return _.find(
        this.masterData.pRItemTypes,
        { id: id }).name;
    },
    handleDeleteDetail(index, row) {
      this.newItem.PRDetails.splice(index, 1);
    },
    handleAppendDetailNew(index, row) {
      if (row.guid) { // existed record
        console.error("不支持已有的记录被Append!");
      } else { // new record
        this.newItem.PRDetails.splice(-1, 1,
          {
            id: 0,
            guid: Guid.raw(),
            lineNo: _.parseInt(row.lineNo),
            itemType: row.itemType,
            itemName: row.itemName,
            estimatedCost: row.estimatedCost,
            description: row.description,
          },
          {
            id: 0,
            guid: '',
            lineNo: _.parseInt(_.max(_.map(this.newItem.PRDetails, 'lineNo'))) + 5 || 5,
            itemType: 1,
            itemName: '',
            estimatedCost: 0,
            description: '',
          }
        );
      }
    },
    eraseInvalidDetail(details) {
      if (!details[details.length - 1].guid) {
        details.splice(-1, 1);
      }
    },
  }
});
