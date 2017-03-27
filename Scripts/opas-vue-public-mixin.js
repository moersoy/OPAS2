var opas_vue_public_mixin = {
  data: {
    messageBoxDurationConfig: { success: 30000, error: 60000 },
  },
  methods: {
    alertErrorNotImplemented() {
      this.$notify.error({
        title: '错误',
        message: '本功能尚未实现'
      });
    },
    handleLeftNavMenuItemSelected(key, keyPath) {
      if (key == "MY-ALERT" ||
        key == "REPORT-TODO") {
        this.alertErrorNotImplemented();
      }
    },
    drawFlowChart(flowTemplateDef, currentActivityNodeGuid) {
      if (flowTemplateDef) {
        let raphael = Raphael('FlowChart',
          $('FlowChart').width(), $('FlowChart').height());
        let drawingPaper = new DrawingPaper(raphael);
        drawingPaper.nodesData = flowTemplateDef.activityNodes.nodes;
        drawingPaper.connectionsData = flowTemplateDef.activityConnections.connections;
        drawingPaper.render();
        if (currentActivityNodeGuid) {
          drawingPaper.decorateSelectedNode(currentActivityNodeGuid);
        }
      }
    },
    alertBackendError() {
      if (this.backendError) {
        this.$message({
          showClose: true,
          message: this.backendError,
          type: 'error',
          duration: 0,
        });
      }
    },
    parseErrorOfServerResponse(error) {
      var errContent = error.toString();
      if (error.response && error.response.data &&
        error.response.data.Message) {
        errContent = error.response.data.Message;
      }
      return errContent;
    },
    jumpToUrl(url) {
      window.location = url;
    },
    submitToBackend(path, dataObj, successPrompt) {
      var _parseError = this.parseErrorOfServerResponse;
      var that = this;

      var loadingInstance = this.$loading({
        fullscreen: true,
        body: true,
        text: "正在提交 / Processsing"
      });
      axios.post(path,
        {
          docJson: JSON.stringify(dataObj),
        },
        {headers: {'X-OPAS2-UserToken': '1234567890chao'}}
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: (successPrompt || '提交表单成功') + ':' + response.data.toString(),
          showClose: true
        });
        loadingInstance.close();
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '提交表单失败,发生错误:' + _parseError(error),
          showClose: true
        });
        console.error(dataObj, error);
        loadingInstance.close();
        return false;
      });
    },
  },
  mounted () {
    this.alertBackendError();

    if (this.otherMountedActions) {
      this.otherMountedActions();
    }
  }
};