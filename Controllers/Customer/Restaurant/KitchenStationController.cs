using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class KitchenStationController : Controller
    {
        CommonController oCommonController = new CommonController();

        public async Task<ActionResult> Index(long? BranchId)
        {
            ClsKitchenStationVm obj = new ClsKitchenStationVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.GetStations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Stations = oClsResponse.Data.Stations;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "kitchen stations").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ClsKitchenStationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = obj.PageIndex > 0 ? obj.PageIndex : 1;
                obj.BranchId = obj.BranchId > 0 ? obj.BranchId : 0;
                ViewBag.BranchId = obj.BranchId;
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.GetStations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Stations = oClsResponse.Data.Stations;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "kitchen stations").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        public async Task<ActionResult> Add()
        {
            ClsKitchenStationVm obj = new ClsKitchenStationVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoryResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponseCategories = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            ClsStationTypeVm stationTypeObj = new ClsStationTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stationTypeResult = await stationTypeController.ActiveStationTypes(stationTypeObj);
            ClsResponse oClsResponseStationTypes = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);

            ViewBag.Categories = oClsResponseCategories.Data.Categories;
            ViewBag.StationTypes = oClsResponseStationTypes.Data.StationTypes;

            return View();
        }

        public async Task<ActionResult> Edit(long id)
        {
            ClsKitchenStationVm obj = new ClsKitchenStationVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KitchenStationId = id;
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.KitchenStation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoryResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponseCategories = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            ClsStationTypeVm stationTypeObj = new ClsStationTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stationTypeResult = await stationTypeController.ActiveStationTypes(stationTypeObj);
            ClsResponse oClsResponseStationTypes = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);

            ViewBag.Station = oClsResponse.Data.Station;
            ViewBag.CategoryIds = oClsResponse.Data.CategoryIds;
            ViewBag.Categories = oClsResponseCategories.Data.Categories;
            ViewBag.StationTypes = oClsResponseStationTypes.Data.StationTypes;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> InsertKitchenStation(ClsKitchenStationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = obj.BranchId > 0 ? obj.BranchId : Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.InsertKitchenStation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateKitchenStation(ClsKitchenStationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = obj.BranchId > 0 ? obj.BranchId : Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.InsertKitchenStation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> KitchenStationDelete(ClsKitchenStationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.KitchenStationDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> KitchenStationActiveInactive(ClsKitchenStationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.KitchenStationActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetStations(ClsKitchenStationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = obj.BranchId > 0 ? obj.BranchId : Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            var kitchenStationResult = await kitchenStationController.GetStations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kitchenStationResult);
            return Json(oClsResponse);
        }
    }
}
