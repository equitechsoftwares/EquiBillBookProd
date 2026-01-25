using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class KotController : Controller
    {
        CommonController oCommonController = new CommonController();

        public async Task<ActionResult> Index(long? BranchId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
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

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Calculate status counts
            int pendingCount = 0, preparingCount = 0, readyCount = 0, servedCount = 0, cancelledCount = 0;
            if (oClsResponse.Data.Kots != null)
            {
                pendingCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Pending" || b.OrderStatus == "Printed");
                preparingCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Preparing");
                readyCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Ready");
                servedCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Served");
                cancelledCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Cancelled");
            }

            ViewBag.Kots = oClsResponse.Data.Kots;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.PreparingCount = preparingCount;
            ViewBag.ReadyCount = readyCount;
            ViewBag.ServedCount = servedCount;
            ViewBag.CancelledCount = cancelledCount;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "kot").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                if (obj.PageIndex == 0) obj.PageIndex = 1;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Calculate status counts
            int pendingCount = 0, preparingCount = 0, readyCount = 0, servedCount = 0, cancelledCount = 0;
            if (oClsResponse.Data.Kots != null)
            {
                pendingCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Pending" || b.OrderStatus == "Printed");
                preparingCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Preparing");
                readyCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Ready");
                servedCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Served");
                cancelledCount = oClsResponse.Data.Kots.Count(b => b.OrderStatus == "Cancelled");
            }

            ViewBag.Kots = oClsResponse.Data.Kots;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.PreparingCount = preparingCount;
            ViewBag.ReadyCount = readyCount;
            ViewBag.ServedCount = servedCount;
            ViewBag.CancelledCount = cancelledCount;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "kot").FirstOrDefault();
            ViewBag.BranchId = obj.BranchId;

            return View();
        }

        public async Task<ActionResult> Add(long? bookingId, long? kotId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                if (bookingId.HasValue)
                {
                    obj.BookingId = bookingId.Value;
                }
                if (kotId.HasValue)
                {
                    obj.KotId = kotId.Value;
                }
            }

            // Load existing KOT data if editing
            ClsKotMasterVm existingKot = null;
            if (kotId.HasValue && kotId.Value > 0)
            {
                WebApi.KotController kotController = new WebApi.KotController();
                var kotResult = await kotController.Kot(obj);
                ClsResponse oClsResponseKot = await oCommonController.ExtractResponseFromActionResult(kotResult);
                if (oClsResponseKot.Status == 1 && oClsResponseKot.Data.Kot != null)
                {
                    existingKot = oClsResponseKot.Data.Kot;
                    ViewBag.KotDetails = oClsResponseKot.Data.KotDetails;
                    
                    // Check if KOT can be edited (industry standard: only Pending or Printed status)
                    var restrictedStatuses = new[] { "Preparing", "Ready", "Served", "Cancelled" };
                    if (restrictedStatuses.Contains(existingKot.OrderStatus))
                    {
                        TempData["ErrorMessage"] = $"Cannot edit KOT with status '{existingKot.OrderStatus}'. Only KOTs with 'Pending' or 'Printed' status can be edited.";
                        return RedirectToAction("Details", new { id = kotId.Value });
                    }
                    
                    // Set bookingId from existing KOT if not provided
                    if (bookingId == 0 && existingKot.BookingId != 0)
                    {
                        obj.BookingId = existingKot.BookingId;
                    }
                }
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            ClsKitchenStationVm stationObj = new ClsKitchenStationVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stationResult = await kitchenStationController.ActiveStations(stationObj);
            ClsResponse oClsResponseStations = await oCommonController.ExtractResponseFromActionResult(stationResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Load active users for waiter selection (users/employees only, not customers)
            ClsUserVm waiterObj = new ClsUserVm();
            waiterObj.AddedBy = obj.AddedBy;
            waiterObj.CompanyId = obj.CompanyId;
            waiterObj.UserType = "user";
            UserController userController = new UserController();
            var waiterResult = await userController.AllActiveUsers(waiterObj);
            ClsResponse oClsResponseWaiters = await oCommonController.ExtractResponseFromActionResult(waiterResult);
            // Filter for users (employees/staff) only - exclude customers
            ViewBag.Waiters = oClsResponseWaiters.Data.Users != null 
                ? oClsResponseWaiters.Data.Users.Where(u => u.UserType != null && u.UserType.ToLower() == "user").ToList() 
                : new List<ClsUserVm>();

            if (bookingId.HasValue || (existingKot != null && existingKot.BookingId != 0))
            {
                var bookingIdToLoad = bookingId ?? existingKot.BookingId;
                obj.BookingId = bookingIdToLoad;
                WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
                ClsTableBookingVm bookingObj = new ClsTableBookingVm { BookingId = bookingIdToLoad, CompanyId = obj.CompanyId };
                var bookingResult = await tableBookingController.TableBooking(bookingObj);
                ClsResponse oClsResponseBooking = await oCommonController.ExtractResponseFromActionResult(bookingResult);
                ViewBag.Booking = oClsResponseBooking.Data.Booking;
            }

            ViewBag.Tables = oClsResponseTables.Data.Tables;
            ViewBag.Stations = oClsResponseStations.Data.Stations;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.BookingId = bookingId ?? (existingKot != null ? existingKot.BookingId : 0);
            ViewBag.ExistingKot = existingKot;
            ViewBag.IsEditMode = kotId.HasValue && kotId.Value > 0;

            return View();
        }

        public async Task<ActionResult> Edit(long id)
        {
            // Redirect to Add action with kotId parameter
            return RedirectToAction("Add", new { kotId = id });
        }

        public async Task<ActionResult> Details(long id, string returnUrl = null)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = id;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.Kot(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kot = oClsResponse.Data.Kot;
            ViewBag.KotDetails = oClsResponse.Data.KotDetails;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        public async Task<ActionResult> LinkToSales(long id)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = id;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.Kot(obj);
            ClsResponse oClsResponseKot = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kot = oClsResponseKot.Data.Kot;

            return View();
        }

        public async Task<ActionResult> LinkToBooking(long id)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = id;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.Kot(obj);
            ClsResponse oClsResponseKot = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kot = oClsResponseKot.Data.Kot;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LinkToSalesAction(long kotId, long salesId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = kotId;
                obj.SalesId = salesId;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.LinkToSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> LinkToBookingAction(long kotId, long bookingId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = kotId;
                obj.BookingId = bookingId;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.LinkToBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> SearchSalesForLinking(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesResult = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> SearchBookingsForLinking(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> CreateFromBooking(long bookingId)
        {
            return RedirectToAction("Add", "Kot", new { bookingId = bookingId });
        }

        public async Task<ActionResult> KitchenDisplay(long? StationId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                if (StationId.HasValue)
                {
                    obj.KitchenStationId = StationId.Value;
                }
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.GetActiveKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            WebApi.KitchenStationController kitchenStationController = new WebApi.KitchenStationController();
            ClsKitchenStationVm stationObj = new ClsKitchenStationVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stationResult = await kitchenStationController.ActiveStations(stationObj);
            ClsResponse oClsResponseStations = await oCommonController.ExtractResponseFromActionResult(stationResult);

            ViewBag.Kots = oClsResponse.Data.Kots;
            ViewBag.Stations = oClsResponseStations.Data.Stations;
            ViewBag.StationId = StationId;

            return View();
        }

        public async Task<ActionResult> Print(long id)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = id;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.Kot(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kot = oClsResponse.Data.Kot;
            ViewBag.KotDetails = oClsResponse.Data.KotDetails;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateStandalone(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.CreateStandalone(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateKotStatus(ClsKotMasterVm obj)
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

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.UpdateKotStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateItemStatus(ClsKotDetailsVm obj)
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

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.UpdateItemStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> GetActiveKots(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                // Set BranchId from cookies if not already set in the request
                if (obj.BranchId == 0 && Request.Cookies["data"]["BranchId"] != null)
                {
                    obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                }
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.GetActiveKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> ConvertKotsToSalesAction(List<long> kotIds)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotIds = kotIds;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.ConvertKotsToSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> ConvertBookingKotsToSalesAction(long bookingId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BookingId = bookingId;
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.ConvertBookingKotsToSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(long kotId)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.KotId = kotId;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.DeleteKot(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            return Json(oClsResponse);
        }
    }
}
