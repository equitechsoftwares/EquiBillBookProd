using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Customer.Restaurant
{
    [AuthorizationPrivilegeFilter]
    public class RestaurantDashboardController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<ActionResult> Index()
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
            }

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "restaurant").FirstOrDefault();
            ViewBag.Branchs = oClsResponseBranches.Data.Branchs;

            return View();
        }

        public async Task<ActionResult> RestaurantDashboardFetch(ClsRestaurantSettingsVm obj)
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

            if (obj.BranchId <= 0)
            {
                // If no branch selected, try to get first branch or default
                BranchController branchController = new BranchController();
                ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var branchResult = await branchController.ActiveBranchs(branchObj);
                ClsResponse oClsResponseBranches = await oCommonController.ExtractResponseFromActionResult(branchResult);
                if (oClsResponseBranches.Data.Branchs != null && oClsResponseBranches.Data.Branchs.Count > 0)
                {
                    obj.BranchId = oClsResponseBranches.Data.Branchs[0].BranchId;
                }
            }

            WebApi.RestaurantDashboardController restaurantDashboardController = new WebApi.RestaurantDashboardController();
            var dashboardResult = await restaurantDashboardController.GetRestaurantDashboard(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(dashboardResult);

            if (oClsResponse != null && oClsResponse.Status == 1 && oClsResponse.Data != null)
            {
                // ExtractResponseFromActionResult returns ClsResponse with Data property
                // The Data property contains the nested objects, but they may be Dictionary<string, object>
                // So we need to convert them to strongly-typed models
                var data = oClsResponse.Data;
                
                // Use serializer to convert the Data object to a dictionary for extraction
                string json = serializer.Serialize(data);
                Dictionary<string, object> dataDict = serializer.Deserialize<Dictionary<string, object>>(json);
                
                if (dataDict != null)
                {
                    // Extract nested objects from the Data dictionary
                    object bookingStatsObj = null;
                    object kotStatsObj = null;
                    object tableStatsObj = null;
                    object revenueStatsObj = null;
                    object restaurantSettingsObj = null;
                    
                    dataDict.TryGetValue("BookingStats", out bookingStatsObj);
                    dataDict.TryGetValue("KotStats", out kotStatsObj);
                    dataDict.TryGetValue("TableStats", out tableStatsObj);
                    dataDict.TryGetValue("RevenueStats", out revenueStatsObj);
                    dataDict.TryGetValue("RestaurantSettings", out restaurantSettingsObj);
                    
                    // Convert Dictionary to model classes
                    ViewBag.BookingStats = ConvertToBookingStats(bookingStatsObj);
                    ViewBag.KotStats = ConvertToKotStats(kotStatsObj);
                    ViewBag.TableStats = ConvertToTableStats(tableStatsObj);
                    ViewBag.RevenueStats = ConvertToRevenueStats(revenueStatsObj);
                    ViewBag.RestaurantSettings = ConvertToRestaurantSettings(restaurantSettingsObj);
                }
            }

            return PartialView("PartialRestaurantDashboard");
        }

        private ClsBookingStatsVm ConvertToBookingStats(object bookingStats)
        {
            if (bookingStats == null) return null;

            Dictionary<string, object> dict = null;
            
            // Try direct cast first
            dict = bookingStats as Dictionary<string, object>;
            
            // If direct cast fails, try to convert via JSON serialization (handles JavaScriptSerializer's object wrapping)
            if (dict == null)
            {
                try
                {
                    string json = serializer.Serialize(bookingStats);
                    dict = serializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    return null;
                }
            }
            
            if (dict == null) return null;

            return new ClsBookingStatsVm
            {
                TotalTodayBookings = GetIntValue(dict, "TotalTodayBookings"),
                PendingBookings = GetIntValue(dict, "PendingBookings"),
                ConfirmedBookings = GetIntValue(dict, "ConfirmedBookings"),
                ActiveBookings = GetIntValue(dict, "ActiveBookings"),
                CompletedBookings = GetIntValue(dict, "CompletedBookings"),
                CancelledBookings = GetIntValue(dict, "CancelledBookings"),
                UpcomingBookings = GetIntValue(dict, "UpcomingBookings"),
                ActiveRecurringBookings = GetIntValue(dict, "ActiveRecurringBookings"),
                BookingConversionRate = GetDecimalValue(dict, "BookingConversionRate"),
                AverageBookingDuration = GetDoubleValue(dict, "AverageBookingDuration")
            };
        }

        private ClsKotStatsVm ConvertToKotStats(object kotStats)
        {
            if (kotStats == null) return null;

            Dictionary<string, object> dict = null;
            
            // Try direct cast first
            dict = kotStats as Dictionary<string, object>;
            
            // If direct cast fails, try to convert via JSON serialization (handles JavaScriptSerializer's object wrapping)
            if (dict == null)
            {
                try
                {
                    string json = serializer.Serialize(kotStats);
                    dict = serializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    return null;
                }
            }
            
            if (dict == null) return null;

            return new ClsKotStatsVm
            {
                TotalTodayKots = GetIntValue(dict, "TotalTodayKots"),
                PendingKots = GetIntValue(dict, "PendingKots"),
                PreparingKots = GetIntValue(dict, "PreparingKots"),
                ReadyKots = GetIntValue(dict, "ReadyKots"),
                CompletedKots = GetIntValue(dict, "CompletedKots")
            };
        }

        private ClsTableStatsVm ConvertToTableStats(object tableStats)
        {
            if (tableStats == null) return null;

            Dictionary<string, object> dict = null;
            
            // Try direct cast first
            dict = tableStats as Dictionary<string, object>;
            
            // If direct cast fails, try to convert via JSON serialization (handles JavaScriptSerializer's object wrapping)
            if (dict == null)
            {
                try
                {
                    string json = serializer.Serialize(tableStats);
                    dict = serializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    return null;
                }
            }
            
            if (dict == null) return null;

            return new ClsTableStatsVm
            {
                TotalTables = GetIntValue(dict, "TotalTables"),
                AvailableTables = GetIntValue(dict, "AvailableTables"),
                OccupiedTables = GetIntValue(dict, "OccupiedTables"),
                ReservedTables = GetIntValue(dict, "ReservedTables"),
                MaintenanceTables = GetIntValue(dict, "MaintenanceTables")
            };
        }

        private ClsRevenueStatsVm ConvertToRevenueStats(object revenueStats)
        {
            if (revenueStats == null) return null;

            Dictionary<string, object> dict = null;
            
            // Try direct cast first
            dict = revenueStats as Dictionary<string, object>;
            
            // If direct cast fails, try to convert via JSON serialization (handles JavaScriptSerializer's object wrapping)
            if (dict == null)
            {
                try
                {
                    string json = serializer.Serialize(revenueStats);
                    dict = serializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    return null;
                }
            }
            
            if (dict == null) return null;

            return new ClsRevenueStatsVm
            {
                TodayRevenue = GetDecimalValue(dict, "TodayRevenue")
            };
        }

        private ClsRestaurantSettingsVm ConvertToRestaurantSettings(object restaurantSettings)
        {
            if (restaurantSettings == null) return new ClsRestaurantSettingsVm
            {
                EnableKitchenDisplay = false,
                EnableTableBooking = false,
                EnableRecurringBooking = false,
                AutoPrintKot = false
            };

            Dictionary<string, object> dict = null;
            
            // Try direct cast first
            dict = restaurantSettings as Dictionary<string, object>;
            
            // If direct cast fails, try to convert via JSON serialization (handles JavaScriptSerializer's object wrapping)
            if (dict == null)
            {
                try
                {
                    string json = serializer.Serialize(restaurantSettings);
                    dict = serializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    return new ClsRestaurantSettingsVm
                    {
                        EnableKitchenDisplay = false,
                        EnableTableBooking = false,
                        EnableRecurringBooking = false,
                        AutoPrintKot = false
                    };
                }
            }
            
            if (dict == null) return new ClsRestaurantSettingsVm
            {
                EnableKitchenDisplay = false,
                EnableTableBooking = false,
                EnableRecurringBooking = false,
                AutoPrintKot = false
            };

            return new ClsRestaurantSettingsVm
            {
                EnableKitchenDisplay = GetBoolValue(dict, "EnableKitchenDisplay"),
                EnableTableBooking = GetBoolValue(dict, "EnableTableBooking"),
                EnableRecurringBooking = GetBoolValue(dict, "EnableRecurringBooking"),
                AutoPrintKot = GetBoolValue(dict, "AutoPrintKot")
            };
        }

        private bool GetBoolValue(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
                return false;

            var value = dict[key];
            if (value is bool) return (bool)value;
            if (bool.TryParse(value.ToString(), out bool result)) return result;
            return false;
        }

        private int GetIntValue(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
                return 0;

            var value = dict[key];
            if (value is int) return (int)value;
            if (value is long) return (int)(long)value;
            if (int.TryParse(value.ToString(), out int result)) return result;
            return 0;
        }

        private decimal GetDecimalValue(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
                return 0;

            var value = dict[key];
            if (value is decimal) return (decimal)value;
            if (value is double) return (decimal)(double)value;
            if (value is float) return (decimal)(float)value;
            if (decimal.TryParse(value.ToString(), out decimal result)) return result;
            return 0;
        }

        private double GetDoubleValue(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
                return 0;

            var value = dict[key];
            if (value is double) return (double)value;
            if (value is decimal) return (double)(decimal)value;
            if (value is float) return (double)(float)value;
            if (double.TryParse(value.ToString(), out double result)) return result;
            return 0;
        }
    }
}
