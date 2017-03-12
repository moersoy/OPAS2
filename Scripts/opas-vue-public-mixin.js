var opas_vue_public_mixin = {
  data: {
  },
  methods: {
    alertErrorNotImplemented() {
      this.$notify.error({
        title: '错误',
        message: '本功能尚未实现'
      });
    },
    handleLeftNavMenuItemSelected(key, keyPath) {
      if (key == "MY-ACCOUNT" ||
        key == "MY-ALERT" ||
        key == "MY-DELEGATE" ||
        key == "REPORT-TODO") {
        this.alertErrorNotImplemented();
      }
    },
    drawFlowChart(flowTemplateDef, currentActivityNodeGuid) {
      console.log(flowTemplateDef);
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

  }
};