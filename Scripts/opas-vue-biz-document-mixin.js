﻿var opas_vue_biz_document_mixin = {
  data:{
    dialogChooseUserVisible: false,
    queryUserInfo: null,
    allUserDTOs: null,
    selectedUserDTO: null,
    dialogChooseUserConfirmHandle: null,
    messageBoxDurationConfig: { success: 30000, error :60000},
  },
  computed: {
    filteredUserDTOs: function () {
      if (this.queryUserInfo) {
        return _.filter(this.allUserDTOs, userDTO => {
          return (userDTO.name &&
            userDTO.name.indexOf(this.queryUserInfo) >= 0) ||
            (userDTO.englishName &&
            userDTO.englishName.indexOf(this.queryUserInfo) >= 0) ||
            (userDTO.defaultDepartmentName &&
            userDTO.defaultDepartmentName.indexOf(this.queryUserInfo)) >= 0;
        });
      } else {
        return this.allUserDTOs;
      }
    }
  },
  methods: {
    getCorrenspondingDataItem(workingMode) {
      switch (workingMode) {
        case "new":
          return this.newItem;
          break;
        case "updateAtStart":
          return this.newItem;
          break;
        case "examine":
          return this.examineItem;
          break;
        default:
          console.error("未处理的workingMode:" + workingMode);
          return null;
      }
    },
    onSubmitCreate() {
      var that = this;
      // TODO: 检查业务对象合法性

      // 检查流程操作合法性
      if (!this.isFlowOperationDataValid(this.newItem)) {
        return false;
      }

      // 克隆业务对象并删除无需上传的部分
      var newItemClone = _.cloneDeep(this.newItem) // 不能用Object.assign的浅拷贝
      newItemClone.sessionData = undefined;
      //如果需要删除无效的detail记录,则调用进行处理
      if (this.eraseInvalidDetail) {
        this.eraseInvalidDetail(newItemClone[this.detailsName]);
      }
      var _parseError = this.parseErrorOfServerResponse;

      // 提交到后端
      axios.post(this.newItem.sessionData.CreateWithFlowActionPath,
        {
          docJson: JSON.stringify(newItemClone),
        }
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: '提交表单成功:' + response.data.toString(),
          showClose: true
        });
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '提交表单失败,发生错误:' + _parseError(error),
          showClose: true
        });
        console.error(error);
        return false;
      });
    },
    onSubmitExamineFlowAction() {
      // TODO: 检查业务对象合法性
      // 检查流程操作合法性
      if (!this.isFlowOperationDataValid(this.examineItem)) {
        return false;
      }
      // 克隆业务对象并删除无需上传的部分
      var examineItemClone = Object.assign({}, this.examineItem);
      examineItemClone.sessionData = undefined;

      var that = this;
      var _parseError = this.parseErrorOfServerResponse;
      // 提交
      axios.post(this.examineItem.sessionData.NextFlowActionPath,
        {
          docJson: JSON.stringify(examineItemClone),
        }
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: '完成审批:' + response.data.toString(),
          showClose: true
        });
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '错误:' + _parseError(error),
          showClose: true
        });
        return false;
      });
    },
    onSubmitRejectToStartFlowAction() {
      // 克隆业务对象并删除无需上传的部分
      var examineItemClone = Object.assign({}, this.examineItem);
      examineItemClone.sessionData = undefined;

      var that = this;
      var _parseError = this.parseErrorOfServerResponse;
      // 提交
      axios.post(this.examineItem.sessionData.RejectToStartFlowActionPath,
        {
          docJson: JSON.stringify(examineItemClone),
        }
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: '成功提交退回申请人 / Reject to creator successfully.',
          showClose: true
        });
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '错误:' + _parseError(error),
          showClose: true
        });
        return false;
      });
    },
    onSubmitUpdateAtStartFlowAction() {
      var that = this;
      // TODO: 检查业务对象合法性

      // 检查流程操作合法性
      if (!this.isFlowOperationDataValid(this.newItem)) {
        return false;
      }

      // 克隆业务对象并删除无需上传的部分
      var newItemClone = _.cloneDeep(this.newItem);
      newItemClone.sessionData = undefined;
      //如果需要删除无效的detail记录,则调用进行处理
      if (this.eraseInvalidDetail) {
        this.eraseInvalidDetail(newItemClone[this.detailsName]);
      }
      //console.log(newItemClone);

      // 提交到后端
      var _parseError = this.parseErrorOfServerResponse;
      axios.post(this.newItem.sessionData.NextFlowActionPath,
        {
          docJson: JSON.stringify(newItemClone),
        }
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: '成功重新提交:' + response.data.toString(),
          showClose: true
        });
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '错误:' + _parseError(error),
          showClose: true
        });
        console.error(error);
        return false;
      });
    },
    onSubmitInviteOtherToExamine() {
      // 克隆业务对象并删除无需上传的部分
      var examineItemClone = Object.assign({}, this.examineItem);
      examineItemClone.sessionData = undefined;
      console.log(this.selectedUserDTO);
      examineItemClone.selectedPaticipantGuid =
        this.selectedUserDTO.guid;

      var that = this;
      var _parseError = this.parseErrorOfServerResponse;
      // 提交
      axios.post(this.examineItem.sessionData.InviteOtherFlowActionPath,
        {
          docJson: JSON.stringify(examineItemClone),
        }
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: '成功提交',
          showClose: true
        });
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '错误:' + _parseError(error),
          showClose: true
        });
        return false;
      });

      this.dialogChooseUserVisible = false;
    },
    onInviteOtherToExamine() {
      var that = this;
      if (!this.allUserDTOs) {
        axios.get("/api/UserApi")
        .then(function (response) {
          that.allUserDTOs = response.data;
          that.dialogChooseUserVisible = true;
          that.dialogChooseUserConfirmHandle =
            that.onSubmitInviteOtherToExamine;
        })
        .catch(function (error) {
          that.$message.error({
            duration: that.messageBoxDurationConfig.error,
            message: '获取用户列表错误:' + _parseError(error),
            showClose: true
          });
          return false;
        });
      } else {
        that.dialogChooseUserVisible = true;
        that.dialogChooseUserConfirmHandle =
          that.onSubmitInviteOtherToExamine;
      }
    },
    onSubmitInviteOtherFeedback() {
      // 克隆业务对象并删除无需上传的部分
      var examineItemClone = Object.assign({}, this.examineItem);
      examineItemClone.sessionData = undefined;

      var that = this;
      var _parseError = this.parseErrorOfServerResponse;
      // 提交
      axios.post(this.examineItem.sessionData.InviteOtherFeedbackFlowActionPath,
        {
          docJson: JSON.stringify(examineItemClone),
        }
      )
      .then(function (response) {
        that.$message.success({
          duration: that.messageBoxDurationConfig.success,
          message: '成功提交',
          showClose: true
        });
        console.info(response);
      })
      .catch(function (error) {
        that.$message.error({
          duration: that.messageBoxDurationConfig.error,
          message: '错误:' + _parseError(error),
          showClose: true
        });
        return false;
      });
    },
    onDialogChooseUserConfirmed(){
      this.dialogChooseUserConfirmHandle();
    },
    handleSelectedUserDTOChange(val) {
      this.selectedUserDTO = val;
    },
    handleRemoveAttachment(file, fileList) {
      //console.log(file, fileList);
      axios.post(this.removeAttachPath,
                {
                  fileName: file.name,
                })
      .catch(function (error) {
        console.error(error);
        return false;
      });
    },
    handleBeforeUploadAttachment(file) {
      if (_.some( // 不允许多次上传同一文件
          this.$refs.upload.fileList,
          { name: file.name })) {
        return false;
      }
    },
    handleFlowSelectedConnection(connectionGuid) {
      var dataItem = this.getCorrenspondingDataItem(this.workingMode);
      var destNode;
      if (connectionGuid) {
        destNode = _.find(
          dataItem.sessionData.availableFlowConnections,
          obj => {
            return obj.connection.guid == connectionGuid;
          }).toNode;
        dataItem.sessionData.potentialPaticipants = destNode.roles;
        dataItem.sessionData.needChoosePaticipant =
          this.isDestNodePaticipantRequired(destNode.type);
      } else {
        dataItem.sessionData.potentialPaticipants = null;
      }
    },
    showFlowChart() {
      this.dialogShowFlowChart = true;
      var that = this;
      if (!this.flowChartCached) {
        setTimeout(function () {
          that.drawFlowChart(that.flowTemplateDef,
            that.getCorrenspondingDataItem(that.workingMode).currentActivityGuid);
          that.flowChartCached = true;
        }, 0);
      }
    },
    isDestNodePaticipantRequired(destNodeType) {
      return destNodeType != "st-autoActivity" &&
        destNodeType != "st-end" &&
        destNodeType != "st-start";
    },
    isFlowOperationDataValid(dataItem) { // 检查流程操作合法性
      if (!dataItem.selectedConnectionGuid) {
        this.$message.error({
          duration: 10000,
          message: '错误:需要选择流程操作',
          showClose: true
        });
        return false;
      }

      if (dataItem.sessionData.needChoosePaticipant &&
        !dataItem.selectedPaticipantGuid) {
        this.$message.error({
          duration: 10000,
          message: '错误:需要选择下一步操作执行人',
          showClose: true
        });
        return false;
      }

      return true;
    },
    parseErrorOfServerResponse(error) {
      var errContent = error.toString();
      if (error.response && error.response.data &&
        error.response.data.Message) {
        errContent = error.response.data.Message;
      }
      return errContent;
    },
    customJSONstringify(obj) {
      return JSON.stringify(obj).replace(/\/Date/g, "\\\/Date").replace(/\)\//g, "\)\\\/")
    }
  }
};