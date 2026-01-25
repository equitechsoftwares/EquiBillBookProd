using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class SettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: settings
        public async Task<ActionResult> Index()
        {
            ClsHeaderVm obj = new ClsHeaderVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.HeaderType = "settings";

            // Header permissions
            WebApi.HeaderController headerController = new WebApi.HeaderController();
            var headerPermissionsResult = await headerController.HeaderPermissions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(headerPermissionsResult);

            // Item settings
            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var itemSettingResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(itemSettingResult);

            // Sale settings
            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            // Business settings
            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            // Expense settings
            WebApi.ExpenseSettingsController expenseSettingsController = new WebApi.ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var expenseSettingResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingResult);

            ViewBag.Headers = oClsResponse.Data.Headers;
            ViewBag.ItemSetting = oClsResponse4.Data.ItemSetting;
            ViewBag.SaleSetting = oClsResponse5.Data.SaleSetting;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;

            return View();
        }        

    }

}