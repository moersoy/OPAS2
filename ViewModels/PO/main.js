﻿const v = new Vue({
  el: '#app',
  data: {
    workingMode: initBag.workingMode, // backend will fill this global variable
    removeAttachPath: initBag.removeAttachPath, //删除附件的URL路径
    detailsName: "PODetails",
    dialogShowFlowChart: false,
    flowChartCached: false,
    flowTemplateDef: initBag.flowTemplateDef,
    newItem: { // 新建PO的数据
      // 流程部分
      flowTemplateGuid: initBag.flowTemplateGuid,
      currentActivityGuid: initBag.currentActivityGuid,
      selectedConnectionGuid: '',
      selectedPaticipantGuid: '',
      currentUserGuid: initBag.currentUserGuid,
      guid: initBag.guid, //DocumentId
      purchaseReqId: initBag.purchaseReqId, //前驱的PR
      purchaseOrderId: initBag.purchaseOrderId,
      PRDocumentNo: initBag.PRDocumentNo,
      taskGuid: initBag.taskGuid,
      flowInstanceId: initBag.flowInstanceId,

      // 业务数据
      contactOfficePhone: initBag.bizData.contactOfficePhone,
      contactMobile: initBag.bizData.contactMobile,
      departmentId: initBag.bizData.departmentId,
      costCenterId: initBag.bizData.costCenterId,
      currencyTypeId: initBag.bizData.currencyTypeId,
      mainCurrencyRate: initBag.bizData.mainCurrencyRate || 1,
      vendorId: initBag.bizData.vendorId,
      vendorContactPerson: initBag.bizData.vendorContactPerson,
      vendorTel: initBag.bizData.vendorTel,
      orderDate: initBag.bizData.orderDate || null,
      effectiveDate: initBag.bizData.effectiveDate || null,
      reason: initBag.bizData.reason || '',
      POType: initBag.bizData.POType,
      description: initBag.bizData.description || '',
      paymentTerm: initBag.bizData.paymentTerm || '',
      // 明细项子表部分
      PODetails: initBag.PODetails,
      // 仅在前台操作中用于辅助的临时数据,不属于业务实体
      sessionData:{
        availableFlowConnections: initBag.availableFlowConnections,
        potentialPaticipants: null,
        CreateWithFlowActionPath: initBag.POCreateWithFlowActionPath,
        NextFlowActionPath: initBag.PONextFlowActionPath,
        needChoosePaticipant: true,
      }
    },
    examineItem: {// 审批PO的数据
      currentUserGuid: initBag.currentUserGuid,
      guid: initBag.guid, //DocumentId
      purchaseOrderId: initBag.purchaseOrderId,
      taskGuid: initBag.taskGuid,
      PRDocumentNo: initBag.PRDocumentNo,
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
        NextFlowActionPath: initBag.PONextFlowActionPath,
        RejectToStartFlowActionPath: initBag.PORejectToStartFlowActionPath,
        InviteOtherFlowActionPath: initBag.POInviteOtherFlowActionPath,
        InviteOtherFeedbackFlowActionPath:
          initBag.POInviteOtherFeedbackFlowActionPath,
        needChoosePaticipant: true,
      }
    },
    masterData: {
      pRDetailItemNames: initBag.pRDetailItemNames,
      unitMeasureTypes: initBag.unitMeasureTypes,
      departmentList: initBag.departmentList,
      currencyTypeList: initBag.currencyTypeList,
      currencyRateList: initBag.currencyRateList,
      costCenterList: initBag.costCenterList,
      vendorList: initBag.vendorList,
      POTypes: initBag.POTypes,
    }
  },
  mixins: [opas_vue_public_mixin, opas_vue_biz_document_mixin],
  methods: {
    handleDeleteDetail(index, row) {
      this.newItem.PODetails.splice(index, 1);
    },
    handleAppendDetailNew(index, row) {
      if (row.guid) { // existed record
        console.error("不支持已有的记录被Append!");
      } else { // new record
        this.newItem.PODetails.splice(-1, 1,
          {
            id: 0,
            guid: Guid.raw(),
            lineNo: _.parseInt(row.lineNo),
            itemName: row.itemName,
            unitMeasure: row.unitMeasure,
            price: row.price,
            quantity: row.quantity,
            amount: row.amount,
            amountInRMB: row.amountInRMB,
            description: row.description,
          },
          {
            id: 0,
            guid: '',
            lineNo: _.parseInt(_.max(_.map(this.newItem.PODetails, 'lineNo'))) + 5 || 5,
            itemName: '',
            unitMeasure: '',
            price: 0,
            quantity: 0,
            amount: 0,
            amountInRMB: 0,
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
    currencyTypeChanged(e) {
      this.newItem.mainCurrencyRate = _.find(
        this.masterData.currencyRateList, (o) => {
          return o.id ==
            this.newItem.currencyTypeId
        }).currencyRate || 1;
    },
    vendorChanged() {
      var _vendor = _.find(this.masterData.vendorList,
        (o) => {
          return o.id ==
            this.newItem.vendorId
        });
      if (_vendor){
        this.newItem.vendorContactPerson = _vendor.contactPerson;
        this.newItem.vendorTel = _vendor.contactPersonTel;
      }
    }
  }
});
