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
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class OnlinePaymentSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: OnlinePaymentSettings
        public async Task<ActionResult> OnlinePaymentSettings(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var allOnlinePaymentSettingsResult = await onlinePaymentSettingsController.AllOnlinePaymentSettings(obj);
            ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(allOnlinePaymentSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Currencys = oClsResponse17.Data.Currencys;
            ViewBag.OnlinePaymentSettings = oClsResponse18.Data.OnlinePaymentSettings;
            ViewBag.TotalCount = oClsResponse18.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse18.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse18.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse18.Data.TotalCount / oClsResponse18.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse18.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "online payment settings").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> OnlinePaymentSettingsFetch(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Categories";
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var allOnlinePaymentSettingsResult = await onlinePaymentSettingsController.AllOnlinePaymentSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allOnlinePaymentSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.OnlinePaymentSettings = oClsResponse.Data.OnlinePaymentSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "online payment settings").FirstOrDefault();

            return PartialView("PartialOnlinePaymentSettings");
        }
        public async Task<ActionResult> OnlinePaymentSettingEdit(long OnlinePaymentSettingsId)
        {
            ClsOnlinePaymentSettingsVm obj = new ClsOnlinePaymentSettingsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.OnlinePaymentSettingsId = OnlinePaymentSettingsId;
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingResult = await onlinePaymentSettingsController.OnlinePaymentSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var activeCurrencysResult2 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult2);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.OnlinePaymentSetting = oClsResponse.Data.OnlinePaymentSetting;
            ViewBag.Currencys = oClsResponse17.Data.Currencys;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> OnlinePaymentSettingAdd(ClsOnlinePaymentSettingsVm obj)
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
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var allOnlinePaymentSettingsResult = await onlinePaymentSettingsController.AllOnlinePaymentSettings(obj);
            ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(allOnlinePaymentSettingsResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var activeCurrencysResult2 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult2);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Currencys = oClsResponse17.Data.Currencys;
            ViewBag.ActiveCount = oClsResponse18.Data.ActiveCount;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> OnlinePaymentSettingInsert(ClsOnlinePaymentSettingsVm obj)
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
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var insertOnlinePaymentSettingResult = await onlinePaymentSettingsController.InsertOnlinePaymentSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertOnlinePaymentSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OnlinePaymentSettingUpdate(ClsOnlinePaymentSettingsVm obj)
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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var updateOnlinePaymentSettingResult = await onlinePaymentSettingsController.UpdateOnlinePaymentSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateOnlinePaymentSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OnlinePaymentSettingActiveInactive(ClsOnlinePaymentSettingsVm obj)
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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingActiveInactiveResult = await onlinePaymentSettingsController.OnlinePaymentSettingActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OnlinePaymentSettingDelete(ClsOnlinePaymentSettingsVm obj)
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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingDeleteResult = await onlinePaymentSettingsController.OnlinePaymentSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingDeleteResult);
            return Json(oClsResponse);
        }
    }
}