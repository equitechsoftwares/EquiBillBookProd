using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class RewardPointSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();

        public async Task<ActionResult> Index()
        {
            ClsRewardPointSettingsVm obj = new ClsRewardPointSettingsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            // Load tiers
            var getTiersResult = await rewardPointSettingsController.GetTiers(obj);
            ClsResponse oClsResponseTiers = await oCommonController.ExtractResponseFromActionResult(getTiersResult);

            ViewBag.RewardPointSetting = oClsResponse.Data.RewardPointSetting;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "reward point settings").FirstOrDefault();
            
            // Access Tiers dynamically since it's not in ClsData model
            ViewBag.RewardPointTiers = oClsResponseTiers.Data.RewardPointTiers;

            return View();
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> Update(ClsRewardPointSettingsVm obj)
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

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            var rewardPointSettingsUpdateResult = await rewardPointSettingsController.RewardPointSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingsUpdateResult);

            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SaveTier(ClsRewardPointTierVm obj)
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

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            var saveTierResult = await rewardPointSettingsController.SaveTier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saveTierResult);

            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> DeleteTier(ClsRewardPointTierVm obj)
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

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            var deleteTierResult = await rewardPointSettingsController.DeleteTier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(deleteTierResult);

            return Json(oClsResponse);
        }

    }
}

