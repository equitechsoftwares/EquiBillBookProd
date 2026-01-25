using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class TableBookingController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<ActionResult> Index()
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
            }

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;

            return View();
        }

        public async Task<ActionResult> List()
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
                obj.PageIndex = 1;
                // Don't set BranchId - load all bookings initially
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            // Calculate status counts
            int pendingCount = 0, confirmedCount = 0, cancelledCount = 0, completedCount = 0;
            if (oClsResponse.Data.Bookings != null)
            {
                pendingCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Pending");
                confirmedCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Confirmed");
                cancelledCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Cancelled");
                completedCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Completed");
            }

            ViewBag.Bookings = oClsResponse.Data.Bookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.ConfirmedCount = confirmedCount;
            ViewBag.CancelledCount = cancelledCount;
            ViewBag.CompletedCount = completedCount;
            ViewBag.Tables = oClsResponseTables.Data.Tables;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> List(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            // Calculate status counts
            int pendingCount = 0, confirmedCount = 0, cancelledCount = 0, completedCount = 0;
            if (oClsResponse.Data.Bookings != null)
            {
                pendingCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Pending");
                confirmedCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Confirmed");
                cancelledCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Cancelled");
                completedCount = oClsResponse.Data.Bookings.Count(b => b.Status == "Completed");
            }

            ViewBag.Bookings = oClsResponse.Data.Bookings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.ConfirmedCount = confirmedCount;
            ViewBag.CancelledCount = cancelledCount;
            ViewBag.CompletedCount = completedCount;
            ViewBag.Tables = oClsResponseTables.Data.Tables;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table booking").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> Add()
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
                obj.BookingDate = DateTime.Now.Date;
                obj.BookingTime = DateTime.Now.TimeOfDay;
                obj.Duration = 120;
            }
            obj.UserType = "customer";

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = "customer" };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponseUsers = await oCommonController.ExtractResponseFromActionResult(userResult);

            // Make separate call for waiters (users/employees) - explicitly set UserType = "user"
            ClsUserVm waiterObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = "user" };
            var waiterResult = await userController.AllActiveUsers(waiterObj);
            ClsResponse oClsResponseWaiters = await oCommonController.ExtractResponseFromActionResult(waiterResult);
            // Filter for users (employees/staff) only - exclude customers
            ViewBag.Waiters = oClsResponseWaiters.Data.Users != null 
                ? oClsResponseWaiters.Data.Users.Where(u => u.UserType != null && u.UserType.ToLower() == "user").ToList() 
                : new List<ClsUserVm>();

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            UserGroupController userGroupController = new UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            CurrencyController currencyController = new CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            CountryController countryController = new CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            PaymentTermController paymentTermController = new PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.Tables = oClsResponseTables.Data.Tables;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.BranchId = obj.BranchId;
            ViewBag.Users = oClsResponseUsers.Data.Users;
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> Edit(long id)
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
                obj.BookingId = id;
            }
            obj.UserType = "customer";

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.TableBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            // Fix BookingTime if it was lost during serialization/deserialization
            if (oClsResponse?.Data?.Booking != null)
            {
                try
                {
                    var booking = oClsResponse.Data.Booking;
                    
                    // Check if BookingTime is 00:00:00 and BookingTimeString exists
                    TimeSpan? bookingTime = null;
                    string bookingTimeString = null;
                    bool needsFix = false;
                    
                    if (booking is System.Collections.IDictionary bookingDict)
                    {
                        // Handle as Dictionary
                        if (bookingDict.Contains("BookingTime") && bookingDict["BookingTime"] != null)
                        {
                            var bookingTimeObj = bookingDict["BookingTime"];
                            
                            if (bookingTimeObj is TimeSpan ts)
                            {
                                bookingTime = ts;
                            }
                            else if (bookingTimeObj is System.Collections.IDictionary timeSpanDict)
                            {
                                // Handle TimeSpan serialized as object (Days, Hours, Minutes, Seconds)
                                int hours = 0, minutes = 0;
                                if (timeSpanDict.Contains("Hours") && timeSpanDict["Hours"] != null)
                                {
                                    int.TryParse(timeSpanDict["Hours"].ToString(), out hours);
                                }
                                if (timeSpanDict.Contains("Minutes") && timeSpanDict["Minutes"] != null)
                                {
                                    int.TryParse(timeSpanDict["Minutes"].ToString(), out minutes);
                                }
                                // Handle Days if present (add to hours)
                                if (timeSpanDict.Contains("Days") && timeSpanDict["Days"] != null)
                                {
                                    int days = 0;
                                    if (int.TryParse(timeSpanDict["Days"].ToString(), out days))
                                    {
                                        hours += days * 24;
                                    }
                                }
                                bookingTime = new TimeSpan(hours, minutes, 0);
                            }
                            else if (TimeSpan.TryParse(bookingTimeObj.ToString(), out TimeSpan parsedTime))
                            {
                                bookingTime = parsedTime;
                            }
                        }
                        
                        if (bookingDict.Contains("BookingTimeString") && bookingDict["BookingTimeString"] != null)
                        {
                            bookingTimeString = bookingDict["BookingTimeString"].ToString();
                        }
                        
                        // Check if time is zero and string exists
                        if ((!bookingTime.HasValue || bookingTime.Value == TimeSpan.Zero) && !string.IsNullOrEmpty(bookingTimeString))
                        {
                            needsFix = true;
                        }
                        
                        // Update if needed
                        if (needsFix && TimeSpan.TryParse(bookingTimeString, out TimeSpan fixedTime))
                        {
                            bookingDict["BookingTime"] = fixedTime;
                        }
                    }
                    else
                    {
                        // Handle as dynamic object - convert to dictionary for easier manipulation
                        string bookingJson = serializer.Serialize(booking);
                        var bookingDictDynamic = serializer.Deserialize<Dictionary<string, object>>(bookingJson);
                        
                        if (bookingDictDynamic != null && bookingDictDynamic.ContainsKey("BookingTime") && bookingDictDynamic.ContainsKey("BookingTimeString"))
                        {
                            var bookingTimeObj = bookingDictDynamic["BookingTime"];
                            
                            if (bookingTimeObj is System.Collections.IDictionary timeSpanDict)
                            {
                                // Handle TimeSpan serialized as object
                                int hours = 0, minutes = 0;
                                if (timeSpanDict.Contains("Hours") && timeSpanDict["Hours"] != null)
                                {
                                    int.TryParse(timeSpanDict["Hours"].ToString(), out hours);
                                }
                                if (timeSpanDict.Contains("Minutes") && timeSpanDict["Minutes"] != null)
                                {
                                    int.TryParse(timeSpanDict["Minutes"].ToString(), out minutes);
                                }
                                if (timeSpanDict.Contains("Days") && timeSpanDict["Days"] != null)
                                {
                                    int days = 0;
                                    if (int.TryParse(timeSpanDict["Days"].ToString(), out days))
                                    {
                                        hours += days * 24;
                                    }
                                }
                                bookingTime = new TimeSpan(hours, minutes, 0);
                            }
                            
                            bookingTimeString = bookingDictDynamic["BookingTimeString"]?.ToString();
                            
                            // Check if time is zero and string exists
                            if ((!bookingTime.HasValue || bookingTime.Value == TimeSpan.Zero) && !string.IsNullOrEmpty(bookingTimeString))
                            {
                                if (TimeSpan.TryParse(bookingTimeString, out TimeSpan fixedTime))
                                {
                                    bookingDictDynamic["BookingTime"] = fixedTime;
                                    // Re-serialize and deserialize to ClsTableBookingVm to maintain proper structure
                                    string fixedJson = serializer.Serialize(bookingDictDynamic);
                                    oClsResponse.Data.Booking = serializer.Deserialize<ClsTableBookingVm>(fixedJson);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If fix fails, continue with original data
                }
            }

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            ClsRestaurantTableVm tableObj = new ClsRestaurantTableVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var tableResult = await restaurantTableController.GetTables(tableObj);
            ClsResponse oClsResponseTables = await oCommonController.ExtractResponseFromActionResult(tableResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = "customer" };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponseUsers = await oCommonController.ExtractResponseFromActionResult(userResult);

            // Make separate call for waiters (users/employees) - explicitly set UserType = "user"
            ClsUserVm waiterObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = "user" };
            var waiterResult = await userController.AllActiveUsers(waiterObj);
            ClsResponse oClsResponseWaiters = await oCommonController.ExtractResponseFromActionResult(waiterResult);
            // Filter for users (employees/staff) only - exclude customers
            ViewBag.Waiters = oClsResponseWaiters.Data.Users != null 
                ? oClsResponseWaiters.Data.Users.Where(u => u.UserType != null && u.UserType.ToLower() == "user").ToList() 
                : new List<ClsUserVm>();

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            UserGroupController userGroupController = new UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            CurrencyController currencyController = new CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            CountryController countryController = new CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            PaymentTermController paymentTermController = new PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.Booking = oClsResponse.Data.Booking;
            ViewBag.Tables = oClsResponseTables.Data.Tables;
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;
            ViewBag.Users = oClsResponseUsers.Data.Users;
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> Details(long id)
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
                obj.BookingId = id;
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.TableBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            // Fix BookingTime if it was lost during serialization/deserialization
            if (oClsResponse?.Data?.Booking != null)
            {
                try
                {
                    var booking = oClsResponse.Data.Booking;
                    
                    // Check if BookingTime is 00:00:00 and BookingTimeString exists
                    TimeSpan? bookingTime = null;
                    string bookingTimeString = null;
                    bool needsFix = false;
                    
                    if (booking is System.Collections.IDictionary bookingDict)
                    {
                        // Handle as Dictionary
                        if (bookingDict.Contains("BookingTime") && bookingDict["BookingTime"] != null)
                        {
                            var bookingTimeObj = bookingDict["BookingTime"];
                            
                            if (bookingTimeObj is TimeSpan ts)
                            {
                                bookingTime = ts;
                            }
                            else if (bookingTimeObj is System.Collections.IDictionary timeSpanDict)
                            {
                                // Handle TimeSpan serialized as object (Days, Hours, Minutes, Seconds)
                                int hours = 0, minutes = 0;
                                if (timeSpanDict.Contains("Hours") && timeSpanDict["Hours"] != null)
                                {
                                    int.TryParse(timeSpanDict["Hours"].ToString(), out hours);
                                }
                                if (timeSpanDict.Contains("Minutes") && timeSpanDict["Minutes"] != null)
                                {
                                    int.TryParse(timeSpanDict["Minutes"].ToString(), out minutes);
                                }
                                // Handle Days if present (add to hours)
                                if (timeSpanDict.Contains("Days") && timeSpanDict["Days"] != null)
                                {
                                    int days = 0;
                                    if (int.TryParse(timeSpanDict["Days"].ToString(), out days))
                                    {
                                        hours += days * 24;
                                    }
                                }
                                bookingTime = new TimeSpan(hours, minutes, 0);
                            }
                            else if (TimeSpan.TryParse(bookingTimeObj.ToString(), out TimeSpan parsedTime))
                            {
                                bookingTime = parsedTime;
                            }
                        }
                        
                        if (bookingDict.Contains("BookingTimeString") && bookingDict["BookingTimeString"] != null)
                        {
                            bookingTimeString = bookingDict["BookingTimeString"].ToString();
                        }
                        
                        // Check if time is zero and string exists
                        if ((!bookingTime.HasValue || bookingTime.Value == TimeSpan.Zero) && !string.IsNullOrEmpty(bookingTimeString))
                        {
                            needsFix = true;
                        }
                        
                        // Update if needed
                        if (needsFix && TimeSpan.TryParse(bookingTimeString, out TimeSpan fixedTime))
                        {
                            bookingDict["BookingTime"] = fixedTime;
                            // Re-serialize and deserialize to ensure the fix is applied to the booking object
                            string fixedJson = serializer.Serialize(bookingDict);
                            oClsResponse.Data.Booking = serializer.Deserialize<ClsTableBookingVm>(fixedJson);
                        }
                    }
                    else
                    {
                        // Handle as dynamic object - convert to dictionary for easier manipulation
                        string bookingJson = serializer.Serialize(booking);
                        var bookingDictDynamic = serializer.Deserialize<Dictionary<string, object>>(bookingJson);
                        
                        if (bookingDictDynamic != null && bookingDictDynamic.ContainsKey("BookingTime") && bookingDictDynamic.ContainsKey("BookingTimeString"))
                        {
                            var bookingTimeObj = bookingDictDynamic["BookingTime"];
                            
                            if (bookingTimeObj is System.Collections.IDictionary timeSpanDict)
                            {
                                // Handle TimeSpan serialized as object
                                int hours = 0, minutes = 0;
                                if (timeSpanDict.Contains("Hours") && timeSpanDict["Hours"] != null)
                                {
                                    int.TryParse(timeSpanDict["Hours"].ToString(), out hours);
                                }
                                if (timeSpanDict.Contains("Minutes") && timeSpanDict["Minutes"] != null)
                                {
                                    int.TryParse(timeSpanDict["Minutes"].ToString(), out minutes);
                                }
                                if (timeSpanDict.Contains("Days") && timeSpanDict["Days"] != null)
                                {
                                    int days = 0;
                                    if (int.TryParse(timeSpanDict["Days"].ToString(), out days))
                                    {
                                        hours += days * 24;
                                    }
                                }
                                bookingTime = new TimeSpan(hours, minutes, 0);
                            }
                            
                            bookingTimeString = bookingDictDynamic["BookingTimeString"]?.ToString();
                            
                            // Check if time is zero and string exists
                            if ((!bookingTime.HasValue || bookingTime.Value == TimeSpan.Zero) && !string.IsNullOrEmpty(bookingTimeString))
                            {
                                if (TimeSpan.TryParse(bookingTimeString, out TimeSpan fixedTime))
                                {
                                    bookingDictDynamic["BookingTime"] = fixedTime;
                                    // Re-serialize and deserialize to ClsTableBookingVm to maintain proper structure
                                    string fixedJson = serializer.Serialize(bookingDictDynamic);
                                    oClsResponse.Data.Booking = serializer.Deserialize<ClsTableBookingVm>(fixedJson);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If fix fails, continue with original data
                }
            }

            // Get all KOTs for this booking
            ClsKotMasterVm kotObj = new ClsKotMasterVm();
            kotObj.BookingId = id;
            kotObj.CompanyId = obj.CompanyId;
            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKotsByBooking(kotObj);
            ClsResponse oClsResponseKots = await oCommonController.ExtractResponseFromActionResult(kotResult);

            // Load booking payments to calculate deposit status
            ClsCustomerPaymentVm paymentObj = new ClsCustomerPaymentVm();
            paymentObj.BookingId = id;
            paymentObj.CompanyId = obj.CompanyId;
            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var paymentResult = await bookingPaymentController.BookingPayments(paymentObj);
            ClsResponse oClsResponsePayments = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            // Calculate deposit status based on payments
            bool depositPaid = false;
            
            // First, try to get DepositPaid from the booking response (if API already calculated it)
            if (oClsResponse.Data?.Booking != null)
            {
                var booking = oClsResponse.Data.Booking;
                try
                {
                    if (booking is System.Collections.IDictionary bookingDict)
                    {
                        if (bookingDict.Contains("DepositPaid") && bookingDict["DepositPaid"] != null)
                        {
                            depositPaid = Convert.ToBoolean(bookingDict["DepositPaid"]);
                        }
                    }
                    else
                    {
                        dynamic dynBooking = booking;
                        if (dynBooking.DepositPaid != null)
                        {
                            depositPaid = Convert.ToBoolean(dynBooking.DepositPaid);
                        }
                    }
                }
                catch { }
            }
            
            // If payment data is available, recalculate based on actual payments (more accurate)
            if (oClsResponsePayments.Data?.Booking != null)
            {
                var booking = oClsResponse.Data?.Booking;
                var paymentData = oClsResponsePayments.Data.Booking;
                
                try
                {
                    decimal depositAmount = 0;
                    decimal netPaid = 0;
                    
                    // Get deposit amount from booking
                    if (booking is System.Collections.IDictionary bookingDict)
                    {
                        if (bookingDict.Contains("DepositAmount") && bookingDict["DepositAmount"] != null)
                        {
                            depositAmount = Convert.ToDecimal(bookingDict["DepositAmount"]);
                        }
                    }
                    else if (booking != null)
                    {
                        dynamic dynBooking = booking;
                        if (dynBooking.DepositAmount != null)
                        {
                            depositAmount = Convert.ToDecimal(dynBooking.DepositAmount);
                        }
                    }
                    
                    // Get net paid amount from payment data
                    if (paymentData is System.Collections.IDictionary paymentDict)
                    {
                        if (paymentDict.Contains("NetPaid") && paymentDict["NetPaid"] != null)
                        {
                            netPaid = Convert.ToDecimal(paymentDict["NetPaid"]);
                        }
                    }
                    else
                    {
                        dynamic dynPaymentData = paymentData;
                        if (dynPaymentData.NetPaid != null)
                        {
                            netPaid = Convert.ToDecimal(dynPaymentData.NetPaid);
                        }
                    }
                    
                    // Calculate deposit status: net paid should be >= deposit amount
                    if (depositAmount > 0)
                    {
                        depositPaid = netPaid >= depositAmount;
                    }
                }
                catch { }
            }

            ViewBag.Booking = oClsResponse.Data.Booking;
            ViewBag.Kots = oClsResponseKots.Data?.Kots ?? new List<ClsKotMasterVm>();
            ViewBag.DepositPaid = depositPaid;
            // Separate credits from regular payments (same pattern as Sales view)
            // Credits are identified by: PaymentType == "Advance" AND ParentId > 0
            var allPayments = oClsResponsePayments.Data?.BookingPayments ?? new List<ClsCustomerPaymentVm>();
            // Regular payments: exclude credits (PaymentType == "Advance" with ParentId > 0)
            ViewBag.BookingPayments = allPayments.Where(a => !(a.PaymentType == "Advance" && a.ParentId > 0)).ToList();
            // Credits applied: PaymentType == "Advance" AND ParentId > 0
            ViewBag.CreditsApplied = allPayments.Where(a => a.PaymentType == "Advance" && a.ParentId > 0).ToList();

            // Add InvoiceUrl and MenuPermission for payment actions
            if (Request.Cookies["data"] != null)
            {
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> LinkToKot(long id)
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
                obj.BookingId = id;
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.TableBooking(obj);
            ClsResponse oClsResponseBooking = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            ClsKotMasterVm kotObj = new ClsKotMasterVm();
            kotObj.AddedBy = obj.AddedBy;
            kotObj.CompanyId = obj.CompanyId;
            WebApi.KotController kotController = new WebApi.KotController();
            var kotResult = await kotController.GetStandaloneKots(kotObj);
            ClsResponse oClsResponseKots = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Booking = oClsResponseBooking.Data.Booking;
            ViewBag.Kots = oClsResponseKots.Data.Kots;

            return View();
        }

        public async Task<ActionResult> LinkToSales(long id)
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
                obj.BookingId = id;
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.TableBooking(obj);
            ClsResponse oClsResponseBooking = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            ViewBag.Booking = oClsResponseBooking.Data.Booking;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LinkToKotAction(long bookingId, long kotId)
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
                obj.BookingId = bookingId;
                obj.KotId = kotId;
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.LinkToKot(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> LinkToSalesAction(long bookingId, long salesId)
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
                obj.BookingId = bookingId;
                obj.SalesId = salesId;
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.LinkToSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

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

        public async Task<ActionResult> CreateKotFromBooking(long id)
        {
            return RedirectToAction("Add", "Kot", new { bookingId = id });
        }

        public async Task<ActionResult> CalendarEvents(DateTime start, DateTime end, long? branchId = null)
        {
            try
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
                    obj.FromDate = start;
                    obj.ToDate = end;
                    obj.BranchId = branchId.HasValue && branchId.Value > 0 ? branchId.Value : 0;
                }
                else
                {
                    // If no cookies, return empty array
                    return Json(new object[0], JsonRequestBehavior.AllowGet);
                }

                WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
                var bookingResult = await tableBookingController.GetCalendarEvents(obj);
                ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

                // Extract events directly from the response data
                if (oClsResponse != null && oClsResponse.Status == 1 && oClsResponse.Data != null)
                {
                    try
                    {
                        // The Data property may be a Dictionary or dynamic object
                        // Serialize and deserialize to ensure we can access the Events property
                        string dataJson = serializer.Serialize(oClsResponse.Data);
                        Dictionary<string, object> dataDict = serializer.Deserialize<Dictionary<string, object>>(dataJson);
                        
                        if (dataDict != null && dataDict.ContainsKey("Events") && dataDict["Events"] != null)
                        {
                            var events = dataDict["Events"];
                            return Json(events, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception if needed, but continue to return empty array
                        System.Diagnostics.Debug.WriteLine($"Error extracting events from response: {ex.Message}");
                    }
                }
                
                // Return empty array if no events found or extraction failed
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"Error in CalendarEvents: {ex.Message}");
                // Return empty array on any exception
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateStandalone(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.CreateStandalone(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> CheckAvailability(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.CheckAvailability(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmBooking(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.ConfirmBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> GetRestaurantSettings(ClsRestaurantSettingsVm obj)
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

            WebApi.RestaurantSettingsController restaurantSettingsController = new WebApi.RestaurantSettingsController();
            var settingsResult = await restaurantSettingsController.GetRestaurantSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(settingsResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> CancelBooking(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.CancelBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> CompleteBooking(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.CompleteBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> MarkAsNoShow(ClsTableBookingVm obj)
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

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.MarkAsNoShow(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteBooking(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Browser = Request.Browser.Browser;
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.DeleteBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            return Json(oClsResponse);
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
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.InsertRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetRecurringBookings(ClsRecurringBookingVm obj)
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

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.GetRecurringBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetRecurringBooking(ClsRecurringBookingVm obj)
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

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.GetRecurringBooking(obj);
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
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.RecurringBookingController recurringBookingController = new WebApi.RecurringBookingController();
            var recurringBookingResult = await recurringBookingController.DeleteRecurringBooking(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringBookingResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> BookingPayments(ClsCustomerPaymentVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var paymentResult = await bookingPaymentController.BookingPayments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            ViewBag.BookingPayments = oClsResponse.Data.BookingPayments;
            ViewBag.Booking = oClsResponse.Data.Booking;
            
            // Set ViewBag values to match Sales payment pattern
            if (oClsResponse.Data.Booking != null)
            {
                ViewBag.Due = oClsResponse.Data.Booking.DueAmount;
                ViewBag.AdvanceBalance = oClsResponse.Data.Booking.AdvanceBalance;
            }
            else
            {
                ViewBag.Due = 0;
                ViewBag.AdvanceBalance = 0;
            }

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "booking payment").FirstOrDefault();
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Accounts = oClsResponse11.Data.Accounts;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.IsAccountsAddon = oClsResponse35.Data.MenuPermissions.Any(a => a.Menu.ToLower() == "accounts");
            ViewBag.Title = obj.Title ?? "Booking Payment";

            return PartialView("PartialBookingPayments");
        }

        [HttpPost]
        public async Task<ActionResult> PaymentInsert(ClsCustomerPaymentVm obj)
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
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var paymentResult = await bookingPaymentController.Insert(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> UnusedAdvanceBalance(ClsCustomerPaymentVm obj)
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

            CustomerPaymentController customerPaymentController = new CustomerPaymentController();
            var paymentResult = await customerPaymentController.UnusedAdvanceBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> AvailableCredits(ClsCustomerPaymentVm obj)
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

            CustomerPaymentController customerPaymentController = new CustomerPaymentController();
            var paymentResult = await customerPaymentController.UnusedAdvanceBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            // Get booking details
            ClsTableBookingVm bookingObj = new ClsTableBookingVm();
            bookingObj.BookingId = obj.BookingId;
            bookingObj.CompanyId = obj.CompanyId;
            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var bookingResult = await tableBookingController.TableBooking(bookingObj);
            ClsResponse oClsResponseBooking = await oCommonController.ExtractResponseFromActionResult(bookingResult);

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.Booking = oClsResponseBooking.Data.Booking;

            if (oClsResponseBooking.Data.Booking != null)
            {
                ViewBag.BookingNo = oClsResponseBooking.Data.Booking.BookingNo ?? "";
                ViewBag.DepositAmount = oClsResponseBooking.Data.Booking.DepositAmount;
            }
            else
            {
                ViewBag.BookingNo = "";
                ViewBag.DepositAmount = 0;
            }
            ViewBag.Due = obj.Due;

            return PartialView("PartialAvailableCredits");
        }

        [HttpPost]
        public async Task<ActionResult> PaymentDelete(ClsCustomerPaymentVm obj)
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
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var paymentResult = await bookingPaymentController.Delete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> BookingRefunds(ClsCustomerPaymentVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var refundResult = await bookingPaymentController.BookingRefunds(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(refundResult);

            ViewBag.BookingRefunds = oClsResponse.Data.BookingRefunds;
            ViewBag.OriginalPayment = oClsResponse.Data.OriginalPayment;

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Accounts = oClsResponse11.Data.Accounts;
            ViewBag.Title = obj.Title ?? "Booking Refund";

            return PartialView("PartialBookingRefunds");
        }

        [HttpPost]
        public async Task<ActionResult> RefundInsert(ClsCustomerPaymentVm obj)
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
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var refundResult = await bookingPaymentController.InsertBookingRefund(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(refundResult);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> RefundDelete(ClsCustomerPaymentVm obj)
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
                obj.Platform = Request.Browser.Platform;
                obj.IpAddress = Request.UserHostAddress;
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            BookingPaymentController bookingPaymentController = new BookingPaymentController();
            var refundResult = await bookingPaymentController.RefundDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(refundResult);

            return Json(oClsResponse);
        }
    }
}
