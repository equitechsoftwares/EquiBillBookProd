using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class OrganizationSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: OrganizationSettings
        public async Task<ActionResult> BusinessSettings()
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            obj.PaymentType = "all";
            obj.UnitType = "all";

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.TimeZoneController timeZoneController = new WebApi.TimeZoneController();
            ClsTimeZoneVm timeZoneObj = new ClsTimeZoneVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTimeZonesResult = await timeZoneController.ActiveTimeZones(timeZoneObj);
            ClsResponse oClsResponse16 = await oCommonController.ExtractResponseFromActionResult(activeTimeZonesResult);

            WebApi.IndustryTypeController industryTypeController = new WebApi.IndustryTypeController();
            ClsIndustryTypeVm industryTypeObj = new ClsIndustryTypeVm { AddedBy = obj.AddedBy };
            var activeIndustryTypesResult = await industryTypeController.ActiveIndustryTypes(industryTypeObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(activeIndustryTypesResult);

            WebApi.BusinessTypeController businessTypeController = new WebApi.BusinessTypeController();
            ClsBusinessTypeVm businessTypeObj = new ClsBusinessTypeVm { AddedBy = obj.AddedBy };
            var activeBusinessTypesResult = await businessTypeController.ActiveBusinessTypes(businessTypeObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(activeBusinessTypesResult);

            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.TimeZones = oClsResponse16.Data.TimeZones;
            ViewBag.IndustryTypes = oClsResponse19.Data.IndustryTypes;
            ViewBag.BusinessTypes = oClsResponse20.Data.BusinessTypes;

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> BusinessSettings(ClsBusinessSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsUpdateResult = await businessSettingsController.BusinessSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(businessSettingsUpdateResult);

            //Response.Cookies["BusinessSetting"]["DefaultProfitPercent"] = Convert.ToString(obj.DefaultProfitPercent);
            Response.Cookies["BusinessSetting"]["DateFormat"] = Convert.ToString(obj.DateFormat);
            Response.Cookies["BusinessSetting"]["TimeFormat"] = Convert.ToString(obj.TimeFormat);
            Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(obj.CurrencySymbolPlacement);
            Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(365);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> ProfileUpdate()
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Roles = oClsResponse.Data.Roles;
            ViewBag.Religions = oClsResponse.Data.Religions;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ProfileUpdate(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = arr[0];
            }
            WebApi.UserController userController = new WebApi.UserController();
            var profileUpdateResult = await userController.ProfileUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(profileUpdateResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ProfileUpdateCompany(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.UserController userController = new WebApi.UserController();
            var profileUpdateCompanyResult = await userController.ProfileUpdateCompany(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(profileUpdateCompanyResult);
            return Json(oClsResponse);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ChangePassword(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.LoginDetailsId = Convert.ToInt64(Request.Cookies["data"]["LoginDetailsId"]);
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.UserController userController = new WebApi.UserController();
            var changePasswordResult = await userController.ChangePassword(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(changePasswordResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ChangeLoginEmail()
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }

            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.User = oClsResponse.Data.User;
            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ValidateLoginEmail(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = arr[0];
            }
            WebApi.UserController userController = new WebApi.UserController();
            var validateLoginEmailResult = await userController.ValidateLoginEmail(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(validateLoginEmailResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UpdateLoginEmail(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = arr[0];
            }

            WebApi.UserController userController = new WebApi.UserController();
            var updateLoginEmailResult = await userController.UpdateLoginEmail(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateLoginEmailResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> OpeningBalance()
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            obj.PaymentType = "all";
            obj.UnitType = "all";

            WebApi.AccountOpeningBalanceController accountOpeningBalanceController = new WebApi.AccountOpeningBalanceController();
            ClsAccountOpeningBalanceVm accountOpeningBalanceObj = new ClsAccountOpeningBalanceVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var openingBalanceResult = await accountOpeningBalanceController.OpeningBalance(accountOpeningBalanceObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(openingBalanceResult);

            WebApi.AccountSettingsController accountSettingsController = new WebApi.AccountSettingsController();
            ClsAccountSettingsVm accountSettingsObj = new ClsAccountSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountSettingResult = await accountSettingsController.AccountSetting(accountSettingsObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountSettingResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            ViewBag.AccountTypes = oClsResponse27.Data.AccountTypes;
            ViewBag.BankPayments = oClsResponse27.Data.BankPayments;
            ViewBag.AccountSetting = oClsResponse28.Data.AccountSetting;
            ViewBag.Branchs = oClsResponse30.Data.Branchs;

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UpdateOpeningBalance(ClsAccountTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.AccountOpeningBalanceController accountOpeningBalanceController = new WebApi.AccountOpeningBalanceController();
            var updateOpeningBalanceResult = await accountOpeningBalanceController.UpdateOpeningBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateOpeningBalanceResult);
            return Json(oClsResponse);
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> DeleteOpeningBalance(ClsAccountOpeningBalanceVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.AccountOpeningBalanceController accountOpeningBalanceController = new WebApi.AccountOpeningBalanceController();
            var deleteOpeningBalanceResult = await accountOpeningBalanceController.DeleteOpeningBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(deleteOpeningBalanceResult);
            return Json(oClsResponse);
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> OpeningBalanceFetch(ClsAccountOpeningBalanceVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.AccountOpeningBalanceController accountOpeningBalanceController = new WebApi.AccountOpeningBalanceController();
            var openingBalanceResult = await accountOpeningBalanceController.OpeningBalance(obj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(openingBalanceResult);

            ViewBag.AccountTypes = oClsResponse27.Data.AccountTypes;
            ViewBag.BankPayments = oClsResponse27.Data.BankPayments;
            return PartialView("PartialOpeningBalance");
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> TimeZone(ClsTimeZoneVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TimeZoneController timeZoneController = new WebApi.TimeZoneController();
            var timeZoneResult = await timeZoneController.TimeZone(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(timeZoneResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> CheckTime(ClsBusinessSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var checkTimeResult = await businessSettingsController.CheckTime(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(checkTimeResult);
            return Json(oClsResponse);
        }

    }
}