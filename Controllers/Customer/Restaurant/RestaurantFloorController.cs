using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class RestaurantFloorController : Controller
    {
        CommonController oCommonController = new CommonController();

        [HttpGet]
        public async Task<ActionResult> Index(long? BranchId)
        {
            ClsRestaurantFloorVm obj = new ClsRestaurantFloorVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                obj.BranchId = BranchId != null ? Convert.ToInt64(BranchId) : 0;
                ViewBag.BranchId = obj.BranchId;
            }

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.GetFloors(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Floors = oClsResponse.Data.Floors;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "restaurant floors").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ClsRestaurantFloorVm obj)
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

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.GetFloors(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Floors = oClsResponse.Data.Floors;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "restaurant floors").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        public async Task<ActionResult> Add()
        {
            ClsRestaurantFloorVm obj = new ClsRestaurantFloorVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;

            return View();
        }

        public async Task<ActionResult> Edit(long id)
        {
            ClsRestaurantFloorVm obj = new ClsRestaurantFloorVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.FloorId = id;
            }

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.RestaurantFloor(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Floor = oClsResponse.Data.Floor;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> InsertRestaurantFloor(ClsRestaurantFloorVm obj)
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

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.InsertRestaurantFloor(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateRestaurantFloor(ClsRestaurantFloorVm obj)
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

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.InsertRestaurantFloor(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> RestaurantFloorDelete(ClsRestaurantFloorVm obj)
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

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.RestaurantFloorDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> RestaurantFloorActiveInactive(ClsRestaurantFloorVm obj)
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

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.RestaurantFloorActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetFloors(ClsRestaurantFloorVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = obj.BranchId; // allow all/branch from caller
            }

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            var floorResult = await restaurantFloorController.GetFloors(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(floorResult);
            return Json(oClsResponse);
        }
    }
}
