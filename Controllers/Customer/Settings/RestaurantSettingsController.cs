using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class RestaurantSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();

        public async Task<ActionResult> Index(ClsRestaurantSettingsVm obj)
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
            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var getAllRestaurantSettingsResult = await restaurantSettingsController.GetAllRestaurantSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getAllRestaurantSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.RestaurantSettingsList = oClsResponse.Data.RestaurantSettingsList;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "restaurant settings").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> RestaurantSettingsFetch(ClsRestaurantSettingsVm obj)
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
            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var getAllRestaurantSettingsResult = await restaurantSettingsController.GetAllRestaurantSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getAllRestaurantSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.RestaurantSettingsList = oClsResponse.Data.RestaurantSettingsList;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "restaurant settings").FirstOrDefault();

            return PartialView("PartialRestaurantSettings");
        }

        public async Task<ActionResult> Edit(long RestaurantSettingsId)
        {
            ClsRestaurantSettingsVm obj = new ClsRestaurantSettingsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.RestaurantSettingsId = RestaurantSettingsId;
            }
            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var getRestaurantSettingsResult = await restaurantSettingsController.GetRestaurantSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getRestaurantSettingsResult);

            if (oClsResponse != null && oClsResponse.Data != null && oClsResponse.Data.RestaurantSettings != null)
            {
                ViewBag.RestaurantSettings = oClsResponse.Data.RestaurantSettings;
            }
            else
            {
                ViewBag.RestaurantSettings = null;
            }
            return View();
        }

        public async Task<ActionResult> RestaurantSettingsUpdate(ClsRestaurantSettingsVm obj)
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
            }
            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var updateRestaurantSettingsResult = await restaurantSettingsController.UpdateRestaurantSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateRestaurantSettingsResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> GenerateQrCode(ClsRestaurantSettingsVm obj)
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

            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var generateQrCodeResult = await restaurantSettingsController.GenerateQrCode(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(generateQrCodeResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> RestaurantSettingsActiveInactive(ClsRestaurantSettingsVm obj)
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
            }

            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var restaurantSettingsActiveInactiveResult = await restaurantSettingsController.RestaurantSettingsActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(restaurantSettingsActiveInactiveResult);
            return Json(oClsResponse);
        }
    }
}


