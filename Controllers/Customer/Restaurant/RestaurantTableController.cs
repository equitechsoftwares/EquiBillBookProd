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
    public class RestaurantTableController : Controller
    {
        CommonController oCommonController = new CommonController();

        [HttpGet]
        public async Task<ActionResult> Index(long? BranchId, string filter, string returnUrl = null)
        {
            ClsRestaurantTableVm obj = new ClsRestaurantTableVm();
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
            
            // Set filter status based on filter parameter
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                if (filter == "available")
                {
                    ViewBag.FilterStatus = "Available";
                    obj.Status = "Available";
                }
                else if (filter == "occupied")
                {
                    ViewBag.FilterStatus = "Occupied";
                    obj.Status = "Occupied";
                }
                else if (filter == "all")
                {
                    ViewBag.FilterStatus = "";
                    obj.Status = "";
                }
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.GetTables(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm floorObj = new ClsRestaurantFloorVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var floorResult = await restaurantFloorController.ActiveFloors(floorObj);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorResult);

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            ClsTableTypeVm typeObj = new ClsTableTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var typeResult = await tableTypeController.ActiveTableTypes(typeObj);
            ClsResponse oClsResponseTypes = await oCommonController.ExtractResponseFromActionResult(typeResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Tables = oClsResponse.Data.Tables;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.Floors = oClsResponseFloors.Data.Floors;
            ViewBag.TableTypes = oClsResponseTypes.Data.TableTypes;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tables").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ClsRestaurantTableVm obj)
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
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.GetTables(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm floorObj = new ClsRestaurantFloorVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var floorResult = await restaurantFloorController.ActiveFloors(floorObj);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorResult);

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            ClsTableTypeVm typeObj = new ClsTableTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var typeResult = await tableTypeController.ActiveTableTypes(typeObj);
            ClsResponse oClsResponseTypes = await oCommonController.ExtractResponseFromActionResult(typeResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Tables = oClsResponse.Data.Tables;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.Floors = oClsResponseFloors.Data.Floors;
            ViewBag.TableTypes = oClsResponseTypes.Data.TableTypes;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tables").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;
            ViewBag.FilterStatus = obj.Status;

            return View();
        }

        public async Task<ActionResult> Add()
        {
            ClsRestaurantTableVm obj = new ClsRestaurantTableVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm floorObj = new ClsRestaurantFloorVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var floorResult = await restaurantFloorController.ActiveFloors(floorObj);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorResult);

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            ClsTableTypeVm typeObj = new ClsTableTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var typeResult = await tableTypeController.ActiveTableTypes(typeObj);
            ClsResponse oClsResponseTypes = await oCommonController.ExtractResponseFromActionResult(typeResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Floors = oClsResponseFloors.Data.Floors;
            ViewBag.TableTypes = oClsResponseTypes.Data.TableTypes;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        public async Task<ActionResult> Edit(long id)
        {
            ClsRestaurantTableVm obj = new ClsRestaurantTableVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TableId = id;
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.RestaurantTable(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm floorObj = new ClsRestaurantFloorVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var floorResult = await restaurantFloorController.ActiveFloors(floorObj);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorResult);

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            ClsTableTypeVm typeObj = new ClsTableTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var typeResult = await tableTypeController.ActiveTableTypes(typeObj);
            ClsResponse oClsResponseTypes = await oCommonController.ExtractResponseFromActionResult(typeResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Table = oClsResponse.Data.Table;
            ViewBag.Floors = oClsResponseFloors.Data.Floors;
            ViewBag.TableTypes = oClsResponseTypes.Data.TableTypes;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        public async Task<ActionResult> TableLayout(long? FloorId)
        {
            ClsRestaurantTableVm obj = new ClsRestaurantTableVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                if (FloorId != null)
                {
                    obj.FloorId = FloorId.Value;
                }
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.GetTables(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);

            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm floorObj = new ClsRestaurantFloorVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var floorResult = await restaurantFloorController.ActiveFloors(floorObj);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorResult);

            ViewBag.Tables = oClsResponse.Data.Tables;
            ViewBag.Floors = oClsResponseFloors.Data.Floors;
            ViewBag.FloorId = obj.FloorId;

            return View();
        }

        public async Task<ActionResult> TableStatus()
        {
            ClsRestaurantTableVm obj = new ClsRestaurantTableVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.GetTables(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);

            ViewBag.Tables = oClsResponse.Data.Tables;

            // Load floors for filter dropdown
            WebApi.RestaurantFloorController restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm floorObj = new ClsRestaurantFloorVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var floorResult = await restaurantFloorController.ActiveFloors(floorObj);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorResult);
            if (oClsResponseFloors != null && oClsResponseFloors.Data != null && oClsResponseFloors.Data.Floors != null)
            {
                ViewBag.Floors = oClsResponseFloors.Data.Floors;
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> InsertRestaurantTable(ClsRestaurantTableVm obj)
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

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.InsertRestaurantTable(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GenerateQrCode(ClsRestaurantTableVm obj)
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

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.GenerateQrCode(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> RestaurantTableDelete(ClsRestaurantTableVm obj)
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

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.RestaurantTableDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> RestaurantTableActiveInactive(ClsRestaurantTableVm obj)
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

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.RestaurantTableActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetTables(ClsRestaurantTableVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.GetTables(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> SetTableStatus(ClsRestaurantTableVm obj)
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

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var tableResult = await restaurantTableController.SetTableStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableResult);
            return Json(oClsResponse);
        }
    }
}
