using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class BookingReportsController : Controller
    {
        CommonController oCommonController = new CommonController();

        // GET: BookingReports/BookingSummary
        public async Task<ActionResult> BookingSummary(string returnUrl = null)
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // POST: BookingReports/BookingSummaryFetch
        [HttpPost]
        public async Task<ActionResult> BookingSummaryFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialBookingSummary");
        }

        // GET: BookingReports/TableUtilization
        public async Task<ActionResult> TableUtilization()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/TableUtilizationFetch
        [HttpPost]
        public async Task<ActionResult> TableUtilizationFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialTableUtilization");
        }

        // GET: BookingReports/CustomerBookingHistory
        public async Task<ActionResult> CustomerBookingHistory()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/CustomerBookingHistoryFetch
        [HttpPost]
        public async Task<ActionResult> CustomerBookingHistoryFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialCustomerBookingHistory");
        }

        // GET: BookingReports/UnlinkedBookingsReport
        public async Task<ActionResult> UnlinkedBookingsReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            // Filter unlinked bookings (no SalesId and no KotId)
            if (oClsResponse.Data.Bookings != null)
            {
                var bookings = oClsResponse.Data.Bookings as System.Collections.Generic.List<dynamic>;
                if (bookings != null)
                {
                    bookings = bookings.Where(b => 
                        (b.SalesId == null || b.SalesId == 0) && 
                        (b.KotNos == null || ((System.Collections.Generic.List<string>)b.KotNos).Count == 0)
                    ).ToList();
                    oClsResponse.Data.TotalCount = bookings.Count;
                }
            }

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/UnlinkedBookingsReportFetch
        [HttpPost]
        public async Task<ActionResult> UnlinkedBookingsReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            // Filter unlinked bookings
            if (oClsResponse.Data.Bookings != null)
            {
                var bookings = oClsResponse.Data.Bookings as System.Collections.Generic.List<dynamic>;
                if (bookings != null)
                {
                    bookings = bookings.Where(b => 
                        (b.SalesId == null || b.SalesId == 0) && 
                        (b.KotNos == null || ((System.Collections.Generic.List<string>)b.KotNos).Count == 0)
                    ).ToList();
                    oClsResponse.Data.TotalCount = bookings.Count;
                }
            }

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialUnlinkedBookingsReport");
        }

        // GET: BookingReports/BookingConversionReport
        public async Task<ActionResult> BookingConversionReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/BookingConversionReportFetch
        [HttpPost]
        public async Task<ActionResult> BookingConversionReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Bookings = oClsResponse.Data.Bookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialBookingConversionReport");
        }

        // GET: BookingReports/BookingCancellationReport
        public async Task<ActionResult> BookingCancellationReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.Status = "Cancelled";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = oClsResponse.Data.Bookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/BookingCancellationReportFetch
        [HttpPost]
        public async Task<ActionResult> BookingCancellationReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.Status = "Cancelled";
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Bookings = oClsResponse.Data.Bookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialBookingCancellationReport");
        }

        // GET: BookingReports/NoShowReport
        public async Task<ActionResult> NoShowReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.Status = "Confirmed";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            // Filter for no-shows: confirmed bookings with past booking date/time and no sales
            object filteredBookings = null;
            long filteredCount = 0;
            if (oClsResponse?.Data != null && oClsResponse.Data.Bookings != null)
            {
                var bookings = oClsResponse.Data.Bookings as System.Collections.Generic.List<dynamic>;
                if (bookings != null)
                {
                    var now = DateTime.Now;
                    var filtered = bookings.Where(b =>
                    {
                        try
                        {
                            var bookingDate = (DateTime)b.BookingDate;
                            var bookingTime = (TimeSpan)b.BookingTime;
                            var bookingDateTime = bookingDate.Add(bookingTime);

                            var isPast = bookingDateTime < now;
                            var hasNoSales = b.SalesId == null || (long)b.SalesId == 0;
                            var isConfirmed = b.Status != null && b.Status.ToString().ToLower() == "confirmed";

                            return isPast && hasNoSales && isConfirmed;
                        }
                        catch
                        {
                            return false;
                        }
                    }).ToList();
                    filteredCount = filtered.Count;
                    filteredBookings = filtered;
                }
            }

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Bookings = filteredBookings ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = filteredCount;
            ViewBag.Branchs = oClsResponse25?.Data?.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(filteredCount > 0 ? filteredCount : 0) / (oClsResponse?.Data?.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse?.Data?.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35?.Data?.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/NoShowReportFetch
        [HttpPost]
        public async Task<ActionResult> NoShowReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.Status = "Confirmed";
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            // Filter for no-shows
            object filteredBookings2 = null;
            long filteredCount2 = 0;
            if (oClsResponse?.Data != null && oClsResponse.Data.Bookings != null)
            {
                var bookings = oClsResponse.Data.Bookings as System.Collections.Generic.List<dynamic>;
                if (bookings != null)
                {
                    var now = DateTime.Now;
                    var filtered = bookings.Where(b =>
                    {
                        try
                        {
                            var bookingDate = (DateTime)b.BookingDate;
                            var bookingTime = (TimeSpan)b.BookingTime;
                            var bookingDateTime = bookingDate.Add(bookingTime);

                            var isPast = bookingDateTime < now;
                            var hasNoSales = b.SalesId == null || (long)b.SalesId == 0;
                            var isConfirmed = b.Status != null && b.Status.ToString().ToLower() == "confirmed";

                            return isPast && hasNoSales && isConfirmed;
                        }
                        catch
                        {
                            return false;
                        }
                    }).ToList();
                    filteredCount2 = filtered.Count;
                    filteredBookings2 = filtered;
                }
            }

            ViewBag.Bookings = filteredBookings2 ?? new System.Collections.Generic.List<ClsTableBookingVm>();
            ViewBag.TotalCount = filteredCount2;
            ViewBag.PageCount = (int)Math.Ceiling((double)(filteredCount2 > 0 ? filteredCount2 : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialNoShowReport");
        }

        // GET: BookingReports/RecurringBookingReport
        public async Task<ActionResult> RecurringBookingReport()
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
                obj.PageIndex = 1;
            }
            var recurringBookingController = new WebApi.RecurringBookingController();
            var result = await recurringBookingController.GetRecurringBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.RecurringBookings = oClsResponse.Data.RecurringBookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            try 
            { 
                var activeCountVal = oClsResponse.Data.ActiveCount;
                ViewBag.ActiveCount = activeCountVal != null ? Convert.ToInt64(activeCountVal) : 0;
            } 
            catch { ViewBag.ActiveCount = 0; }
            try 
            { 
                var inactiveCountVal = oClsResponse.Data.InactiveCount;
                ViewBag.InactiveCount = inactiveCountVal != null ? Convert.ToInt64(inactiveCountVal) : 0;
            } 
            catch { ViewBag.InactiveCount = 0; }
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/RecurringBookingReportFetch
        [HttpPost]
        public async Task<ActionResult> RecurringBookingReportFetch(ClsRecurringBookingVm obj)
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
            var recurringBookingController = new WebApi.RecurringBookingController();
            var result = await recurringBookingController.GetRecurringBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.RecurringBookings = oClsResponse.Data.RecurringBookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            try 
            { 
                var activeCountVal = oClsResponse.Data.ActiveCount;
                ViewBag.ActiveCount = activeCountVal != null ? Convert.ToInt64(activeCountVal) : 0;
            } 
            catch { ViewBag.ActiveCount = 0; }
            try 
            { 
                var inactiveCountVal = oClsResponse.Data.InactiveCount;
                ViewBag.InactiveCount = inactiveCountVal != null ? Convert.ToInt64(inactiveCountVal) : 0;
            } 
            catch { ViewBag.InactiveCount = 0; }
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;

            return PartialView("PartialRecurringBookingReport");
        }

        // GET: BookingReports/TablePerformanceReport
        public async Task<ActionResult> TablePerformanceReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetTablePerformanceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            // Handle dynamic data from API response
            var tablePerformanceData = oClsResponse.Data as dynamic;
            ViewBag.TablePerformance = tablePerformanceData?.TablePerformance;
            ViewBag.TotalCount = tablePerformanceData?.TotalCount != null ? (long)tablePerformanceData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/TablePerformanceReportFetch
        [HttpPost]
        public async Task<ActionResult> TablePerformanceReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetTablePerformanceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            // Handle dynamic data from API response
            var tablePerformanceData = oClsResponse.Data as dynamic;
            ViewBag.TablePerformance = tablePerformanceData?.TablePerformance;
            ViewBag.TotalCount = tablePerformanceData?.TotalCount != null ? (long)tablePerformanceData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialTablePerformanceReport");
        }

        // GET: BookingReports/IndividualTableDetailReport
        public async Task<ActionResult> IndividualTableDetailReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetIndividualTableDetailReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var restaurantTableController = new WebApi.RestaurantTableController();
            var restaurantTableObj = new ClsRestaurantTableVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var resultTables = await restaurantTableController.GetTables(restaurantTableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(resultTables);

            var tableDetailData = oClsResponse.Data as dynamic;
            ViewBag.TableDetails = tableDetailData?.TableDetails;
            ViewBag.TotalCount = tableDetailData?.TotalCount != null ? (long)tableDetailData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.Tables = oClsResponseTables.Data?.Tables ?? new System.Collections.Generic.List<ClsRestaurantTableVm>();
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/IndividualTableDetailReportFetch
        [HttpPost]
        public async Task<ActionResult> IndividualTableDetailReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetIndividualTableDetailReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var tableDetailData = oClsResponse.Data as dynamic;
            ViewBag.TableDetails = tableDetailData?.TableDetails;
            ViewBag.TotalCount = tableDetailData?.TotalCount != null ? (long)tableDetailData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.TableId = obj.TableIds != null && obj.TableIds.Count > 0 ? obj.TableIds[0] : 0;

            return PartialView("PartialIndividualTableDetailReport");
        }

        // GET: BookingReports/BookingSourceReport
        public async Task<ActionResult> BookingSourceReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookingSourceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var sourceData = oClsResponse.Data as dynamic;
            ViewBag.SourceData = sourceData?.SourceData;
            ViewBag.TotalCount = sourceData?.TotalCount != null ? (long)sourceData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/BookingSourceReportFetch
        [HttpPost]
        public async Task<ActionResult> BookingSourceReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookingSourceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var sourceData = oClsResponse.Data as dynamic;
            ViewBag.SourceData = sourceData?.SourceData;
            ViewBag.TotalCount = sourceData?.TotalCount != null ? (long)sourceData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialBookingSourceReport");
        }

        // GET: BookingReports/TableTypePerformanceReport
        public async Task<ActionResult> TableTypePerformanceReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetTableTypePerformanceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var typeData = oClsResponse.Data as dynamic;
            ViewBag.TypePerformance = typeData?.TypePerformance;
            ViewBag.TotalCount = typeData?.TotalCount != null ? (long)typeData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/TableTypePerformanceReportFetch
        [HttpPost]
        public async Task<ActionResult> TableTypePerformanceReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetTableTypePerformanceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var typeData = oClsResponse.Data as dynamic;
            ViewBag.TypePerformance = typeData?.TypePerformance;
            ViewBag.TotalCount = typeData?.TotalCount != null ? (long)typeData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialTableTypePerformanceReport");
        }

        // GET: BookingReports/CustomerVisitFrequencyReport
        public async Task<ActionResult> CustomerVisitFrequencyReport()
        {
            ClsTableBookingVm obj = new ClsTableBookingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-90);
            obj.ToDate = DateTime.Now.Date;

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetCustomerVisitFrequencyReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var frequencyData = oClsResponse.Data as dynamic;
            ViewBag.CustomerFrequency = frequencyData?.CustomerFrequency;
            ViewBag.TotalCount = frequencyData?.TotalCount != null ? (long)frequencyData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "booking reports").FirstOrDefault();

            return View();
        }

        // POST: BookingReports/CustomerVisitFrequencyReportFetch
        [HttpPost]
        public async Task<ActionResult> CustomerVisitFrequencyReportFetch(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            obj.UserType = "customer";

            var tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetCustomerVisitFrequencyReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var frequencyData = oClsResponse.Data as dynamic;
            ViewBag.CustomerFrequency = frequencyData?.CustomerFrequency;
            ViewBag.TotalCount = frequencyData?.TotalCount != null ? (long)frequencyData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialCustomerVisitFrequencyReport");
        }
    }
}

