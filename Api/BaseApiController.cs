﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using EnouFlowTemplateLib;
using EnouFlowInstanceLib;
using EnouFlowOrgMgmtLib;
using EnouFlowEngine;
using OPAS2Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPAS2.Api
{
  public class BaseApiController : ApiController
  {
    protected string OrgCode = "QOROS";
    protected string LocationCode = "SH";

    protected OPAS2DbContext db = new OPAS2DbContext();
    protected EnouFlowOrgMgmtContext orgDb = new EnouFlowOrgMgmtContext();
    protected EnouFlowInstanceContext flowInstDb = new EnouFlowInstanceContext();

    protected FlowActionRequestDispatcher flowActionRequestDispatcher =
      new FlowActionRequestDispatcher();

    protected BaseApiController() : base()
    {
    }

    protected void UpdateDocumentStateFields(
      dynamic bizDocument, FlowInstance flowInstance)
    {
      #region 需要从数据库里读到最新的流程实例信息,因为之前的引擎调用可能已经导致其状态字段有变化
      flowInstDb.flowInstances.Attach(flowInstance); //此处需要Attach,因为CreateWithFlowAction调用时生成的flowInstance并不是当前flowInstDb所生成的
      var refreshedFlowInstance =
        flowInstDb.Entry(flowInstance).GetDatabaseValues();

      EnumFlowInstanceLifeState lifeState =
        (EnumFlowInstanceLifeState)(int)
          refreshedFlowInstance["lifeState"];
      #endregion

      switch (lifeState)
      {
        case EnumFlowInstanceLifeState.start:
          bizDocument.bizDocumentFlowState = EnumBizDocumentFlowState.InProcess;
          break;
        case EnumFlowInstanceLifeState.middle:
          bizDocument.bizDocumentFlowState = EnumBizDocumentFlowState.InProcess;
          break;
        case EnumFlowInstanceLifeState.end:
          bizDocument.bizDocumentFlowState = EnumBizDocumentFlowState.StoppedValid;
          bizDocument.bizState = EnumBizState.Open; //正常结束,可以用于创建后续单据了
          break;
        default:
          throw new Exception("遇到未处理的EnumFlowInstanceLifeState");
      }

      db.SaveChanges();
    }

    protected IHttpActionResult processAutoGeneratedActionRequest(int flowInstanceId)
    {
      FlowActionResult resultAuto = null;
      EnumFlowActionRequestType[] types = new EnumFlowActionRequestType[] { EnumFlowActionRequestType.moveToAutoGenerated };
      // 先尝试执行一次
      resultAuto = flowActionRequestDispatcher.
          processNextActionOfSpecifiedInstance(flowInstanceId, types);
      while (resultAuto != null && resultAuto.succeed)
      { // 有产生的自动型ActionRequest, 并且引擎处理成功过,则继续处理可能继续自动生成的ActionRequest
        resultAuto = flowActionRequestDispatcher.
          processNextActionOfSpecifiedInstance(flowInstanceId, types);
      }
      // 此时或者已无产生的自动型ActionRequest或者引擎处理过程中出错
      if (resultAuto != null && !resultAuto.succeed)
      {// 出错状态
        return BadRequest("流程引擎处理错误:" + resultAuto.failReason);
      }
      // 已无产生的自动型ActionRequest
      return null;
    }

    protected IHttpActionResult InviteOtherFlowActionGeneral(
      dynamic bizObj, string flowTemplateCode, string code)
    {
      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      #region 任务失效则退出
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 流程数据操作: 创建FlowActionInviteOther
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionInviteOther = FlowInstanceHelper.PostFlowActionInviteOther(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        flowTaskForUser.bizTimeStamp, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        flowInstance.bizDataPayloadJson, // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        code, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid) },
        flowTaskForUser.flowTaskForUserId //relativeFlowTaskForUserId
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionInviteOther.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }
      #endregion

      return Ok();
    }

    protected IHttpActionResult InviteOtherFeedbackFlowActionGeneral(
      dynamic bizObj, string flowTemplateCode,string code)
    {
      #region 从提交的JSON参数中初始化基本变量
      Tuple<UserDTO, FlowInstance, FlowTaskForUser> tupleBasic =
        getBasicTaskInfoFromBizObj(bizObj);
      UserDTO userDTO = tupleBasic.Item1;
      FlowInstance flowInstance = tupleBasic.Item2;
      FlowTaskForUser flowTaskForUser = tupleBasic.Item3;
      #endregion

      #region 任务失效则退出
      Tuple<bool, IHttpActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance);
      if (!taskValidity.Item1)
      {
        return taskValidity.Item2;
      }
      #endregion

      #region 流程数据操作: 创建FlowActionInviteOtherFeedback并处理
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      var actionInviteOtherFeedback =
        FlowInstanceHelper.PostFlowActionInviteOtherFeedback(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        flowInstance.bizDataPayloadJson, // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        code, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        bizObj.selectedConnectionGuid, //connectionGuid
        new List<Paticipant>() { //roles
          FlowTemplateDefHelper.getPaticipantFromGuid(
            bizObj.selectedPaticipantGuid)
        },
        flowTaskForUser.flowTaskForUserId //relativeFlowTaskForUserId
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionInviteOtherFeedback.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return BadRequest("流程引擎处理错误:" + flowResult.failReason);
      }
      #endregion

      return Ok();
    }

    protected dynamic getPostedJsonObject()
    {
      var result = Request.Content.ReadAsStringAsync().Result;
      dynamic docObj = JsonConvert.DeserializeObject(result);
      var docJson = (string)docObj.docJson;

      dynamic bizObj = parseJsonToDynamicObject(docJson);
      return bizObj;
    }

    protected Tuple<UserDTO, FlowInstance, FlowTaskForUser> getBasicTaskInfoFromBizObj(dynamic bizObj)
    {
      return new Tuple<UserDTO, FlowInstance, FlowTaskForUser>(
        OrgMgmtDBHelper.getUserDTO(bizObj.currentUserGuid, orgDb),
        FlowInstanceHelper.GetFlowInstance(
          (int)bizObj.flowInstanceId, flowInstDb),
        FlowInstanceHelper.GetFlowTaskForUser(
          (string)(bizObj.taskGuid), flowInstDb));
    }

    protected Tuple<bool, IHttpActionResult> checkTaskValidity(
      FlowTaskForUser flowTaskForUser, FlowInstance flowInstance)
    {
      #region 如果bizTimeStamp已过期,则返回错误
      if (flowTaskForUser.bizTimeStamp < flowInstance.bizTimeStamp)
      {
        return new Tuple<bool, IHttpActionResult>(false,
          BadRequest("该审批任务已过期 / This flow task is already expired."));
      }
      #endregion

      #region 检查任务状态是否可以进行处理
      // 避免1.反复提交;2.任务被其他用户的处理而过时;3.用户自行删除的场景
      if (!flowTaskForUser.isValidToProcess())
      {
        return new Tuple<bool, IHttpActionResult>(false,
          BadRequest("该审批任务状态已不能被处理 / This flow task state is not availabel for processing."));
      }
      #endregion

      return new Tuple<bool, IHttpActionResult>(true, null);
    }

    protected Tuple<bool, IHttpActionResult> RejectToStartFlowActionGeneral(
      dynamic bizObj, string flowTemplateCode, FlowInstance flowInstance,
      dynamic bizDocument, UserDTO userDTO)
    {
      var actionRejectToStart = FlowInstanceHelper.PostFlowActionRejectToStart(
        Guid.NewGuid().ToString(), // clientRequestGuid
        bizObj.guid, //bizDocumentGuid
        flowTemplateCode, //bizDocumentTypeCode
        DateTime.Now, // bizTimeStamp
        bizObj.remarkOfAprrover, // userMemo
        flowInstance.bizDataPayloadJson, // bizDataPayloadJson
        null, // optionalFlowActionDataJson
        userDTO.userId,  // userId
        userDTO.guid, // userGuid
        flowInstance.flowInstanceId, //flowInstanceId
        flowInstance.guid, //flowInstanceGuid
        bizDocument.documentNo, //code
        bizObj.currentActivityGuid, //currentActivityGuid
        null, //startActivityGuid
        new List<Paticipant>()
        { //roles
        }
      );

      var flowResult = flowActionRequestDispatcher.processSpecifiedAction(
        actionRejectToStart.flowActionRequestId);
      if (!flowResult.succeed)
      {
        return new Tuple<bool, IHttpActionResult>(
          false,BadRequest("流程引擎处理错误:" + flowResult.failReason));
      }

      #region 继续处理可能自动生成的ActionRequest
      IHttpActionResult resultAuto = processAutoGeneratedActionRequest(
        flowInstance.flowInstanceId);
      if (resultAuto != null)
      {
        return new Tuple<bool, IHttpActionResult>(
          false, resultAuto); // 出错,将返回非空值
      }
      #endregion

      #region 根据流程实例状态更新PR的bizDocumentFlowState字段,反映正确的单据流程状态
      UpdateDocumentStateFields(bizDocument, flowInstance);
      #endregion

      return  new Tuple<bool, IHttpActionResult>(true, null); // 完成处理,将返回空值
    }

    protected void appendRemarkOfAprroversOfDocument(
      dynamic bizObj, UserDTO userDTO, dynamic bizDocument)
    {
      if (!string.IsNullOrWhiteSpace(bizObj.remarkOfAprrover))
      {
        bizDocument.remarkOfAprrovers =
          bizDocument.remarkOfAprrovers + Environment.NewLine +
            bizObj.remarkOfAprrover + " - [" + userDTO.name + "/" +
            userDTO.englishName + "] " + DateTime.Now.ToString();
        db.SaveChanges();
      }
    }

    protected static dynamic parseJsonToDynamicObject(string json)
    {
      if (string.IsNullOrWhiteSpace(json)) return null;

      dynamic temp = JsonConvert.DeserializeObject<ExpandoObject>(
          json, new ExpandoObjectConverter());
      return temp;
    }

    protected static bool hasAttr(ExpandoObject expando, string key)
    {
      return ((IDictionary<string, Object>)expando).ContainsKey(key);
    }

    protected static decimal tryParseToDecimal(object value, decimal defaultValue)
    {
      decimal result;

      if (!decimal.TryParse(value.ToString(), out result))
      {
        result = defaultValue;
      }

      return result;
    }

    protected static DateTime convertJsonTime(double timestamp)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return origin.AddMilliseconds(timestamp);
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      orgDb.Dispose();
      flowInstDb.Dispose();
      base.Dispose(disposing);
    }

  }
}