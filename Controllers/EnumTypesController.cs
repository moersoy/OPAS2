using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPAS2Model;
using System.Dynamic;
using System.Reflection;

using EnouFlowInstanceLib;
using OPAS2.Models;

namespace OPAS2.Controllers
{
  public class EnumTypesController : Controller
  {
    public string displayCostElementAccountType(int id)
    {
      return getNameFromIdInList(
        TypeSelectLists.CostElementAccountTypeList, id);
    }

    public string displayPOType(int id)
    {
      return getNameFromIdInList(
        TypeSelectLists.POTypeList, id);
    }

    public string displayPaymentAreaType(int id)
    {
      return getNameFromIdInList(
        TypeSelectLists.PaymentAreaTypeList, id);
    }

    public string displayPaymentCurrencyType(int id)
    {
      return getNameFromIdInList(
        TypeSelectLists.PaymentCurrencyTypeList, id);
    }

    public string displayPaymentMethodType(int id)
    {
      return getNameFromIdInList(
        TypeSelectLists.PaymentMethodTypeList, id);
    }

    public string displayFlowTaskType(int id)
    {
      return getNameFromIdInList(
        TypeSelectListsInFlow.FlowTaskTypeList, id);
    }

    private string getNameFromIdInList(List<dynamic> list, int id)
    {
      var obj = list.Where(_obj => _obj.GetType().GetProperty("id").
         GetValue(_obj, null) == id).FirstOrDefault();

      return obj.GetType().GetProperty("name").
          GetValue(obj, null);
    }
  }
}