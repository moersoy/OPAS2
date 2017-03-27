const v = new Vue({
  el: '#app',
  data: {
    jumpTo: {},
    dataPackage:{
      selectedPaticipantGuid: null,
      flowInstanceId: 0,
      currentUserId: initBag.currentUserId,
      currentUserGuid: initBag.currentUserGuid,
      nextActivityGuid: null,
      remarkOfAprrover: null,
    },
    dialogJumpToActivity: false,
    availableFlowActivityNodes: null,
    allUserDTOs: null,
  },
  computed: {
    
  },
  mixins: [opas_vue_public_mixin],
  methods: {
    onShowJumpTo(flowInstanceId) {
      var that = this;
      var _parseError = this.parseErrorOfServerResponse;
      this.dataPackage.nextActivityGuid = null;
      this.dataPackage.flowInstanceId = flowInstanceId;

      axios.get(window.location.protocol + '//' +
        window.location.host + '/api/FlowInstance/GetFlowTemplateJson/' +
        flowInstanceId)
      .then(function (response) {
        that.jumpTo.flowDef = JSON.parse(response.data);
        if (!that.jumpTo.flowDef) return null;
        that.jumpTo.availableFlowActivityNodes =
          _.map(_.reject(that.jumpTo.flowDef.activityNodes.nodes,
              node => (node.type == "st-autoActivity" ||
                node.type == "st-end")),
            (node) => { return { name: node.name, guid: node.guid }; });
        that.dialogJumpToActivity = true;
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '获取数据失败,发生错误:' + _parseError(error),
          showClose: true
        });
        console.error(error);
        return false;
      });
      
    },
    onSubmitJumpToFlowAction() {
      console.log(this.dataPackage);
      this.submitToBackend(window.location.protocol + '//' +
        window.location.host + '/api/FlowInstance/JumpToActivity/',
        this.dataPackage,
        "成功执行跳转 / Jumped to the activity");
    },
    otherMountedActions() {
      var that = this;
      var _parseError = this.parseErrorOfServerResponse;

      axios.get(window.location.protocol + '//' +
        window.location.host + '/api/UserApi')
      .then(function (response) {
        // console.log(response.data);
        that.allUserDTOs = response.data;
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '获取数据失败,发生错误:' + _parseError(error),
          showClose: true
        });
        console.error(error);
        return false;
      });
    },
  }
});