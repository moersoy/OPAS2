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
        // 以下为流程实例跳转操作所需数据
        dialogJumpToActivity: false,
        jumpTo: {},
        jumpToDataPackage: {
          selectedPaticipantGuid: null,
          flowInstanceId: 0,
          currentUserId: initBag.currentUserId,
          currentUserGuid: initBag.currentUserGuid,
          nextActivityGuid: null,
          remarkOfAprrover: null
        },
        allUserDTOs: null,
        // 以下为流程实例终止操作所需数据
        dialogTerminate: false,
        terminate: {},
        terminateDataPackage: {
          flowInstanceId: 0,
          currentUserId: initBag.currentUserId,
          currentUserGuid: initBag.currentUserGuid,
          nextActivityGuid: null,
          remarkOfAprrover: null
        }
      },
      computed: {},
      mixins: [opas_vue_public_mixin],
      methods: {
        onShowJumpTo: function onShowJumpTo(flowInstanceId) {
          var that = this;
          var _parseError = this.parseErrorOfServerResponse;
          this.jumpToDataPackage.nextActivityGuid = null;
          this.jumpToDataPackage.flowInstanceId = flowInstanceId;

          axios.get(window.location.protocol + '//' + window.location.host + '/api/FlowInstance/GetFlowTemplateJson/' + flowInstanceId).then(function (response) {
            that.jumpTo.flowDef = JSON.parse(response.data);
            if (!that.jumpTo.flowDef) return null;
            that.jumpTo.availableFlowActivityNodes = _.map(_.reject(that.jumpTo.flowDef.activityNodes.nodes, function (node) {
              return node.type == "st-autoActivity" || node.type == "st-end";
            }), function (node) {
              return { name: node.name, guid: node.guid };
            });
            that.dialogJumpToActivity = true;
          }).catch(function (error) {
            that.$message.error({
              duration: that.messageBoxDurationConfig.error,
              message: '获取数据失败,发生错误:' + _parseError(error),
              showClose: true
            });
            console.error(error);
            return false;
          });
        },
        onSubmitJumpToFlowAction: function onSubmitJumpToFlowAction() {
          console.log(this.jumpToDataPackage);
          this.submitToBackend(window.location.protocol + '//' + window.location.host + '/api/FlowInstance/JumpToActivity/', this.jumpToDataPackage, "成功执行跳转 / Jumped to the activity");
        },
        onShowTerminate: function onShowTerminate(flowInstanceId) {
          var that = this;
          var _parseError = this.parseErrorOfServerResponse;
          this.terminateDataPackage.nextActivityGuid = null;
          this.terminateDataPackage.flowInstanceId = flowInstanceId;

          axios.get(window.location.protocol + '//' + window.location.host + '/api/FlowInstance/GetFlowTemplateJson/' + flowInstanceId).then(function (response) {
            that.terminate.flowDef = JSON.parse(response.data);
            if (!that.terminate.flowDef) return null;
            that.terminate.availableFlowActivityNodes = _.map(_.filter(that.terminate.flowDef.activityNodes.nodes, function (node) {
              return node.type == "st-end";
            }), function (node) {
              return { name: node.name, guid: node.guid };
            });
            that.dialogTerminate = true;
          }).catch(function (error) {
            that.$message.error({
              duration: that.messageBoxDurationConfig.error,
              message: '获取数据失败,发生错误:' + _parseError(error),
              showClose: true
            });
            console.error(error);
            return false;
          });
        },
        onSubmitTerminateFlowAction: function onSubmitTerminateFlowAction() {
          console.log(this.terminateDataPackage);
          this.submitToBackend(window.location.protocol + '//' + window.location.host + '/api/FlowInstance/Terminate/', this.terminateDataPackage, "成功执行终止 / Flow instance terminated");
        },
        otherMountedActions: function otherMountedActions() {
          var that = this;
          var _parseError = this.parseErrorOfServerResponse;

          axios.get(window.location.protocol + '//' + window.location.host + '/api/UserApi').then(function (response) {
            // console.log(response.data);
            that.allUserDTOs = response.data;
          }).catch(function (error) {
            that.$message.error({
              duration: that.messageBoxDurationConfig.error,
              message: '获取数据失败,发生错误:' + _parseError(error),
              showClose: true
            });
            console.error(error);
            return false;
          });
        }
      }
    });
  }, {}] }, {}, [1]);