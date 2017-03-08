using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

using OPAS2.Helpers;
using System.Web.Routing;
using EnouFlowInstanceLib;
using EnouFlowTemplateLib;
using EnouFlowOrgMgmtLib;
using OPAS2Model;

using Newtonsoft.Json;

using OPAS2.Models;
using OPAS2.Filters;

namespace OPAS2.Controllers
{
  public class BaseController : Controller
  {
    protected override IAsyncResult BeginExecuteCore(
      AsyncCallback callback, object state)
    {
      #region 处理多语言
      string cultureName = null;
      // Attempt to read the culture cookie from Request
      HttpCookie cultureCookie = Request.Cookies["_culture"];
      if (cultureCookie != null)
        cultureName = cultureCookie.Value;
      else
        cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
                null;
      // Validate culture name
      cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

      // Modify current thread's cultures            
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
      Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

      ViewBag.cultureName = cultureName;
      #endregion

      #region 初始化公用ViewBag对象, 将用于_Layout.cshtml
      if (Session["currentUserDTO"] != null)
      {
        ViewBag.currentUserDTO = Session["currentUserDTO"];
        ViewBag.functionPermissionCodes = Session["functionPermissionCodes"];
        var userGuid = (string)ViewBag.currentUserDTO.guid;
        using (var db = new EnouFlowInstanceContext())
        {
          ViewBag.flowTaskForUsers = FlowInstanceHelper.GetFlowTaskForUserListOfUser(userGuid, db);
        }
      }
      #endregion

      return base.BeginExecuteCore(callback, state);
    }

    #region base64编解码
    public string encodeToBase64(string str)
    {
      return Convert.ToBase64String(
        System.Text.Encoding.UTF8.GetBytes(str));
    }
    public string decodeFromBase64(string str)
    {
      return System.Text.Encoding.UTF8.GetString(
        Convert.FromBase64String(str));
    }
    #endregion

    public bool parseCheckboxValue(string formattedString)
    {
      return Convert.ToBoolean(formattedString.Split(',')[0]);
    }

    #region 流程相关
    protected FlowTemplate getFlowTemplateByCode(string flowTemplateCode)
    {
      var flowTemplates = FlowTemplateDBHelper.getAvailableFlowTemplatesByCode(flowTemplateCode).ToList();

      var flowTemplate = 
        flowTemplates.OrderByDescending(t => t.version).FirstOrDefault();

      if (flowTemplate == null)
      {
        throw new Exception("无可用的流程模板template");
      }

      return flowTemplate;
    }

    protected void fillFlowInitDataInViewBag(string flowTemplateCode)
    {
      var flowTemplate = getFlowTemplateByCode(flowTemplateCode);
      var flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowTemplate.flowTemplateJson);
      var firstStartNode = flowTemplateDefHelper.
        getNodesOfStartType().FirstOrDefault();
      if (firstStartNode != null)
      {
        var strJson = JsonConvert.SerializeObject(
         AvailableFlowAction.getNodeOutBoundAvailableFlowActions(
           firstStartNode, flowTemplateDefHelper));
        ViewBag.availableFlowConnectionsEncoded = encodeToBase64(strJson);
        ViewBag.flowTemplateGuid = flowTemplate.guid;
        ViewBag.currentActivityGuid = firstStartNode.guid;
        ViewBag.flowTemplateCode = flowTemplateCode;
      }
      else
      {
        throw new Exception("无可用的流程开始节点");
      }
    }

    protected void fillFlowContinuationDataInViewBag(
      FlowInstance flowInstance)
    {
      ViewBag.currentActivityGuid = flowInstance.currentActivityGuid;
      ViewBag.flowInstanceId = flowInstance.flowInstanceId;

      FlowTemplateDefHelper flowTemplateDefHelper = new FlowTemplateDefHelper(
        flowInstance.flowTemplateJson);

      ActivityNode fromNode = flowTemplateDefHelper.getNodeFromGuid(
        flowInstance.currentActivityGuid);
      if (fromNode != null)
      { // 获取可用的从当前状态节点出发的所有连接和目的状态节点
        var strJson = JsonConvert.SerializeObject(
         AvailableFlowAction.getNodeOutBoundAvailableFlowActions(
           fromNode, flowTemplateDefHelper));
        ViewBag.availableFlowConnectionsEncoded = encodeToBase64(strJson);
      }
      else
      {
        throw new Exception("未找到指定的当前流程实例节点");
      }
    }
    #endregion

    #region 为各主数据生成可选列表的JSON
    protected void PrepareSelectListOfDepartment(EnouFlowOrgMgmtContext orgDb)
    {
      ViewBag.departmentListJsonEncoded = encodeToBase64(
      JsonConvert.SerializeObject(
        OrgMgmtDBHelper.getAllDepartmentDTOs(orgDb).Select(
          obj => new {
            id = obj.departmentId,
            name = obj.name
          }
          ).ToList()
        ));
    }

    protected void PrepareSelectListOfCurrencyType(OPAS2DbContext db)
    {
      ViewBag.currencyTypeListJsonEncoded = encodeToBase64(
      JsonConvert.SerializeObject(
        db.currencyTypes.Select(
          obj => new {
            id = obj.currencyTypeId,
            name =  obj.name
          }
          ).ToList()
        ));
    }

    protected void PrepareSelectListOfCostCenter(OPAS2DbContext db)
    {
      ViewBag.costCenterListJsonEncoded = encodeToBase64(
      JsonConvert.SerializeObject(
        db.costCenters.Select(
          obj => new {
            id = obj.costCenterId,
            name = obj.chineseName + " / " + obj.englishName,
          }
          ).ToList()
        ));
    }

    protected void PrepareSelectListOfVendor(OPAS2DbContext db)
    {
      ViewBag.vendorListJsonEncoded = encodeToBase64(
      JsonConvert.SerializeObject(
        db.vendors.Select(
          obj => new {
            id = obj.vendorId,
            name = obj.chineseName,
          }
          ).ToList()
        ));
    }

    protected void PrepareSelectListOfPOType()
    {
      ViewBag.POTypeListJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(
          TypeSelectLists.POTypeList));
    }

    protected void PrepareSelectListOfUnitMeasure()
    {
      ViewBag.unitMeasureTypesJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(TypeSelectLists.UnitMeasureTypeList));
    }

    protected void PrepareSelectListOfPaymentAreaType()
    {
      ViewBag.paymentAreaTypesJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(TypeSelectLists.PaymentAreaTypeList));
    }

    protected void PrepareSelectListOfPaymentCurrencyType()
    {
      ViewBag.paymentCurrencyTypesJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(TypeSelectLists.PaymentCurrencyTypeList));
    }

    protected void PrepareSelectListOfPaymentMethodType()
    {
      ViewBag.paymentMethodTypesJsonEncoded = encodeToBase64(
        JsonConvert.SerializeObject(TypeSelectLists.PaymentMethodTypeList));
    }
    #endregion

    #region 流程任务相关
    protected List<FlowTask> getValidInviteOtherFeedbackTasks(
      int originFlowTaskForUserId, 
      EnouFlowInstanceContext flowInstDb,
      OPAS2DbContext oPAS2Db)
    {
      var _tasksInDb = flowInstDb.flowTaskForUsers.Where(
        t => t.relativeFlowTaskForUserId == originFlowTaskForUserId)
        .ToList();
      if(_tasksInDb != null && _tasksInDb.Count()>0)
      { 
        return _tasksInDb.Select(t => new FlowTask(t, oPAS2Db)).ToList();
      }
      else
      {
        return new List<FlowTask>();
      }
    }

    protected Tuple<bool, ActionResult> checkTaskValidity(
      FlowTaskForUser flowTaskForUser, FlowInstance flowInstance,
      EnouFlowInstanceContext flowInstDb)
    {
      #region 如果bizTimeStamp已过期,则返回错误
      if (flowTaskForUser == null ||
        flowTaskForUser.bizTimeStamp < flowInstance.bizTimeStamp)
      {
        if (flowTaskForUser != null) // 更新任务的状态
        {
          if(flowTaskForUser.taskState == EnumFlowTaskState.initial ||
            flowTaskForUser.taskState == EnumFlowTaskState.peeked ||
            flowTaskForUser.taskState == EnumFlowTaskState.taken)
          {
            flowTaskForUser.taskState = EnumFlowTaskState.obsoleted;
            flowInstDb.SaveChanges();
          }
        }
        return new Tuple<bool, ActionResult>(false,
          Content("该审批任务已过期 / This flow task is already expired."));
      }
      #endregion

      #region 检查任务状态是否可以进行处理(避免反复提交和任务被已用户自行删除的场景)
      if (!flowTaskForUser.isValidToProcess())
      {
        return new Tuple<bool, ActionResult>(false,
          Content("该审批任务状态已不能被处理 / This flow task state is not availabel for processing."));
      }
      #endregion

      return new Tuple<bool, ActionResult>(true, null);
    }

    protected Tuple<bool, ActionResult> fillInviteOtherFeedback(
      EnouFlowInstanceContext flowInstDb, int flowTaskForUserId)
    {
      FlowTaskForUser flowTaskForUser = flowInstDb.flowTaskForUsers.Find(
        flowTaskForUserId);
      FlowInstance flowInstance = flowTaskForUser.flowInstance;

      #region 检查timestamp是否已过期
      Tuple<bool, ActionResult> taskValidity =
        checkTaskValidity(flowTaskForUser, flowInstance, flowInstDb);
      if (!taskValidity.Item1)
      {
        return taskValidity;
      }
      #endregion

      ViewBag.taskGuid = flowTaskForUser.guid;

      #region 流程相关数据 被征询处理意见时还是填充流程信息,以便被顾问人给出后续处理意见
      fillFlowContinuationDataInViewBag(flowInstance);
      #endregion

      return new Tuple<bool, ActionResult>(true, null);
    }

    #endregion
  }
}