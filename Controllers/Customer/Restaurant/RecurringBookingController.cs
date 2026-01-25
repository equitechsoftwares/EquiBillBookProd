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
    public class RecurringBookingController : Controller
    {
        CommonController oCommonController = new CommonController();

        public async Task<ActionResult> Index(ClsRecurringBookingVm obj, string returnUrl = null)
        {
            if (obj == null)
            {
                obj = new ClsRecurringBookingVm();
            }
            // Ensure PageIndex defaults to 1 if not set or is 0
            if (obj.PageIndex <= 0)
            {
                obj.PageIndex = 1;
            }
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.GetRecurringBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.RecurringBookings = oClsResponse.Data.RecurringBookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RecurringBookingFetch(ClsRecurringBookingVm obj)
        {
            if (obj == null)
            {
                obj = new ClsRecurringBookingVm();
            }
            // Ensure PageIndex defaults to 1 if not set or is 0
            if (obj.PageIndex <= 0)
            {
                obj.PageIndex = 1;
            }
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.GetRecurringBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.RecurringBookings = oClsResponse.Data.RecurringBookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();

            return PartialView("PartialRecurringBooking");
        }

        public async Task<ActionResult> Add(long BookingId = 0)
        {
            ClsRecurringBookingVm obj = new ClsRecurringBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            ClsTableBookingVm bookingObj = new ClsTableBookingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var bookingResult = await tableBookingController.GetBookings(bookingObj);
            ClsResponse oClsResponseBookings = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponseUsers = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            ViewBag.MenuPermission = oClsResponse35?.Data?.MenuPermissions?.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35?.Data?.MenuPermissions?.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.Branchs = oClsResponseBranches?.Data?.Branchs ?? new System.Collections.Generic.List<ClsBranchVm>();
            
            var bookingsList = oClsResponseBookings?.Data?.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            
            // Log for debugging
            System.Diagnostics.Debug.WriteLine($"[RecurringBookingController.Add] BookingId parameter: {BookingId}");
            System.Diagnostics.Debug.WriteLine($"[RecurringBookingController.Add] Total bookings in list: {bookingsList.Count}");
            if (BookingId > 0)
            {
                var bookingExists = bookingsList.Any(b => b.BookingId == BookingId);
                System.Diagnostics.Debug.WriteLine($"[RecurringBookingController.Add] BookingId {BookingId} exists in list: {bookingExists}");
                if (!bookingExists && bookingsList.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[RecurringBookingController.Add] Available BookingIds: {string.Join(", ", bookingsList.Select(b => b.BookingId))}");
                }
            }
            
            ViewBag.Bookings = bookingsList;
            ViewBag.Users = oClsResponseUsers?.Data?.Users ?? new System.Collections.Generic.List<ClsUserVm>();
            ViewBag.Tables = oClsResponseTables?.Data?.Tables ?? new System.Collections.Generic.List<ClsRestaurantTableVm>();
            ViewBag.BookingId = BookingId; // Pass BookingId to view for pre-selection (0 if not provided)

            return View();
        }

        public async Task<ActionResult> Edit(long RecurringBookingId)
        {
            ClsRecurringBookingVm obj = new ClsRecurringBookingVm();
            obj.RecurringBookingId = RecurringBookingId;
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.GetRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            ClsTableBookingVm bookingObj = new ClsTableBookingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var bookingResult = await tableBookingController.GetBookings(bookingObj);
            ClsResponse oClsResponseBookings = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponseUsers = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            ViewBag.RecurringBooking = oClsResponse?.Data?.RecurringBooking;
            ViewBag.MenuPermission = oClsResponse35?.Data?.MenuPermissions?.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35?.Data?.MenuPermissions?.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.Branchs = oClsResponseBranches?.Data?.Branchs ?? new System.Collections.Generic.List<ClsBranchVm>();
            ViewBag.Bookings = oClsResponseBookings?.Data?.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.Users = oClsResponseUsers?.Data?.Users ?? new System.Collections.Generic.List<ClsUserVm>();
            ViewBag.Tables = oClsResponseTables?.Data?.Tables ?? new System.Collections.Generic.List<ClsRestaurantTableVm>();

            return View();
        }

        public async Task<ActionResult> Details(long RecurringBookingId)
        {
            ClsRecurringBookingVm obj = new ClsRecurringBookingVm();
            obj.RecurringBookingId = RecurringBookingId;
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.GetRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.RecurringBooking = oClsResponse.Data.RecurringBooking;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CreateRecurringBooking(ClsRecurringBookingVm obj)
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

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.InsertRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateRecurringBooking(ClsRecurringBookingVm obj)
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

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.UpdateRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> RecurringBookingActiveInactive(ClsRecurringBookingVm obj)
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

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.RecurringBookingActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteRecurringBooking(ClsRecurringBookingVm obj)
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

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.DeleteRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }
    }
}
