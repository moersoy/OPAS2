"use strict";

(function e(t, n, r) {
  function s(o, u) {
    if (!n[o]) {
      if (!t[o]) {
        var a = typeof require == "function" && require;if (!u && a) return a(o, !0);if (i) return i(o, !0);var f = new Error("Cannot find module '" + o + "'");throw f.code = "MODULE_NOT_FOUND", f;
      }var l = n[o] = { exports: {} };t[o][0].call(l.exports, function (e) {
        var n = t[o][1][e];return s(n ? n : e);
      }, l, l.exports, e, t, n, r);
    }return n[o].exports;
  }var i = typeof require == "function" && require;for (var o = 0; o < r.length; o++) {
    s(r[o]);
  }return s;
})({ 1: [function (require, module, exports) {
    var v = new Vue({
      el: '#app',
      data: {
        workingMode: initBag.workingMode, // backend will fill this variable
        removeAttachPath: initBag.removeAttachPath, //删除附件的URL路径
        detailsName: "PMDetails",
        newItem: { // 新建PM的数据
          // 流程部分
          currentUserGuid: initBag.currentUserGuid,
          guid: initBag.guid, //DocumentId
          purchaseOrderId: initBag.purchaseOrderId, //前驱的PO
          paymentId: initBag.paymentId,
          PODocumentNo: initBag.PODocumentNo,
          departmentId: initBag.departmentId,
          flowTemplateGuid: initBag.flowTemplateGuid,
          currentActivityGuid: initBag.currentActivityGuid,
          selectedConnectionGuid: '',
          selectedPaticipantGuid: '',
          taskGuid: initBag.taskGuid,
          flowInstanceId: initBag.flowInstanceId,
          // 业务数据
          reason: initBag.bizData.reason || '',
          description: initBag.bizData.description || '',
          mainCurrencyRate: initBag.bizData.mainCurrencyRate || 1,
          vendorBankName: initBag.bizData.vendorBankName || '',
          vendorBankAccount: initBag.bizData.vendorBankName || '',
          SWIFTCode: initBag.bizData.SWIFTCode || '',
          applicantName: initBag.bizData.applicantName || '',
          applicantEmail: initBag.bizData.applicantEmail || '',
          applicantPhone: initBag.bizData.applicantPhone || '',
          paymentAreaType: initBag.bizData.paymentAreaType || 1,
          paymentCurrencyType: initBag.bizData.paymentCurrencyType || 1,
          paymentMethodType: initBag.bizData.paymentMethodType || 1,
          payingDaysRequirement: initBag.bizData.payingDaysRequirement || 0,
          invoiceNo: initBag.bizData.invoiceNo || '',
          isDownPayment: initBag.bizData.isDownPayment || false,
          isNormalPayment: initBag.bizData.isNormalPayment || true,
          isImmediatePayment: initBag.bizData.isImmediatePayment || false,
          isAdvancePayment: initBag.bizData.isAdvancePayment || false,

          // 子表明细项部分
          PMDetails: initBag.PMDetails,
          // 仅在前台操作中用于辅助的临时数据,不属于业务实体,无需回传
          sessionData: {
            availableFlowConnections: initBag.availableFlowConnections,
            potentialPaticipants: null,
            CreateWithFlowActionPath: initBag.PMCreateWithFlowActionPath,
            NextFlowActionPath: initBag.PMNextFlowActionPath,
            needChoosePaticipant: true
          }
        },
        examineItem: { // 审批PM的数据
          currentUserGuid: initBag.currentUserGuid,
          guid: initBag.guid, //DocumentId
          purchaseOrderId: initBag.purchaseOrderId,
          paymentId: initBag.paymentId,
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
            NextFlowActionPath: initBag.PMNextFlowActionPath,
            RejectToStartFlowActionPath: initBag.PMRejectToStartFlowActionPath,
            needChoosePaticipant: true
          }
        },
        masterData: {
          paymentAreaTypes: initBag.paymentAreaTypes,
          paymentCurrencyTypes: initBag.paymentCurrencyTypes,
          paymentMethodTypes: initBag.paymentMethodTypes
        }
      },
      mixins: [opas_vue_biz_document_mixin],
      methods: {}
    });
  }, {}] }, {}, [1]);