using EquiBillBook.Filters;
using EquiBillBook.Helpers;
using EquiBillBook.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class RestaurantDashboardController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        [HttpPost]
        public async Task<IHttpActionResult> GetRestaurantDashboard(ClsRestaurantSettingsVm obj)
        {
            if (obj.BranchId <= 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Branch ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var next7Days = today.AddDays(7);

            // Get restaurant settings for the branch
            var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false)
                .FirstOrDefault();

            // Today's Bookings Statistics
            var todayBookings = oConnectionContext.DbClsTableBooking
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && 
                           a.IsDeleted == false && a.BookingDate == today);

            var totalTodayBookings = todayBookings.Count();
            var pendingBookings = todayBookings.Where(a => a.Status.ToLower() == "pending").Count();
            var confirmedBookings = todayBookings.Where(a => a.Status.ToLower() == "confirmed").Count();
            var activeBookings = todayBookings.Where(a => a.Status.ToLower() == "confirmed" || a.Status.ToLower() == "pending").Count();
            var completedBookings = todayBookings.Where(a => a.Status.ToLower() == "completed").Count();
            var cancelledBookings = todayBookings.Where(a => a.Status.ToLower() == "cancelled").Count();

            // Upcoming Bookings (next 7 days)
            var upcomingBookings = oConnectionContext.DbClsTableBooking
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && 
                           a.IsDeleted == false && 
                           a.BookingDate >= today && a.BookingDate <= next7Days &&
                           (a.Status.ToLower() == "pending" || a.Status.ToLower() == "confirmed"))
                .Count();

            // Active Recurring Bookings
            var activeRecurringBookings = oConnectionContext.DbClsRecurringBooking
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && 
                           a.IsActive == true)
                .Count();

            // KOT Statistics (Today's KOTs)
            var todayKots = oConnectionContext.DbClsKotMaster
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && 
                           a.IsDeleted == false && 
                           DbFunctions.TruncateTime(a.OrderTime) == today);

            var totalTodayKots = todayKots.Count();
            var pendingKots = todayKots.Where(a => a.OrderStatus.ToLower() == "pending").Count();
            var preparingKots = todayKots.Where(a => a.OrderStatus.ToLower() == "preparing").Count();
            var readyKots = todayKots.Where(a => a.OrderStatus.ToLower() == "ready").Count();
            var completedKots = todayKots.Where(a => a.OrderStatus.ToLower() == "served" || a.OrderStatus.ToLower() == "completed").Count();

            // Table Statistics
            var totalTables = oConnectionContext.DbClsRestaurantTable
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && 
                           a.IsDeleted == false && a.IsActive == true)
                .Count();

            // Get all tables with their manual status (if set) or calculated status
            var allTables = oConnectionContext.DbClsRestaurantTable
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && 
                           a.IsDeleted == false && a.IsActive == true)
                .ToList();

            // Count tables by status (using manual status if set, otherwise calculate)
            TableStatusHelper tableStatusHelper = new TableStatusHelper(oConnectionContext);
            int availableTables = 0;
            int occupiedTables = 0;
            int reservedTables = 0;
            int bookedTables = 0;
            int maintenanceTables = 0;

            foreach (var table in allTables)
            {
                // Calculate status dynamically based on bookings, KOTs, etc.
                TableStatus status = tableStatusHelper.GetTableStatus(table.TableId);

                switch (status)
                {
                    case TableStatus.Available:
                        availableTables++;
                        break;
                    case TableStatus.Occupied:
                        occupiedTables++;
                        break;
                    case TableStatus.Reserved:
                        reservedTables++;
                        break;
                    case TableStatus.Booked:
                        bookedTables++;
                        break;
                    case TableStatus.Maintenance:
                        maintenanceTables++;
                        break;
                }
            }

            // Today's Restaurant Revenue (from Sales linked to bookings or KOTs)
            var todayRevenue = oConnectionContext.DbClsSales
                .Where(s => s.CompanyId == obj.CompanyId && s.BranchId == obj.BranchId && 
                           s.IsDeleted == false && s.IsCancelled == false &&
                           DbFunctions.TruncateTime(s.AddedOn) == today &&
                           (oConnectionContext.DbClsTableBooking.Any(b => b.SalesId == s.SalesId && b.CompanyId == obj.CompanyId) ||
                            oConnectionContext.DbClsKotMaster.Any(k => k.SalesId == s.SalesId && k.CompanyId == obj.CompanyId)))
                .Select(s => s.GrandTotal)
                .DefaultIfEmpty(0)
                .Sum();

            // Booking Conversion Rate (bookings that converted to sales)
            var totalBookingsWithSales = todayBookings.Where(a => a.SalesId > 0).Count();
            var bookingConversionRate = totalTodayBookings > 0 
                ? Math.Round((decimal)totalBookingsWithSales * 100 / totalTodayBookings, 2) 
                : 0;

            // Average Booking Duration
            var avgBookingDuration = todayBookings
                .Where(a => a.Duration > 0)
                .Select(a => a.Duration)
                .DefaultIfEmpty(0)
                .Average();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RestaurantSettings = restaurantSettings != null ? new
                    {
                        EnableKitchenDisplay = restaurantSettings.EnableKitchenDisplay,
                        EnableTableBooking = restaurantSettings.EnableTableBooking,
                        EnableRecurringBooking = restaurantSettings.EnableRecurringBooking,
                        AutoPrintKot = restaurantSettings.AutoPrintKot
                    } : new
                    {
                        EnableKitchenDisplay = false,
                        EnableTableBooking = false,
                        EnableRecurringBooking = false,
                        AutoPrintKot = false
                    },
                    BookingStats = new
                    {
                        TotalTodayBookings = totalTodayBookings,
                        PendingBookings = pendingBookings,
                        ConfirmedBookings = confirmedBookings,
                        ActiveBookings = activeBookings,
                        CompletedBookings = completedBookings,
                        CancelledBookings = cancelledBookings,
                        UpcomingBookings = upcomingBookings,
                        ActiveRecurringBookings = activeRecurringBookings,
                        BookingConversionRate = bookingConversionRate,
                        AverageBookingDuration = Math.Round(avgBookingDuration, 0)
                    },
                    KotStats = new
                    {
                        TotalTodayKots = totalTodayKots,
                        PendingKots = pendingKots,
                        PreparingKots = preparingKots,
                        ReadyKots = readyKots,
                        CompletedKots = completedKots
                    },
                    TableStats = new
                    {
                        TotalTables = totalTables,
                        AvailableTables = availableTables,
                        OccupiedTables = occupiedTables,
                        ReservedTables = reservedTables,
                        BookedTables = bookedTables,
                        MaintenanceTables = maintenanceTables
                    },
                    RevenueStats = new
                    {
                        TodayRevenue = todayRevenue
                    }
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}

