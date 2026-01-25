using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EquiBillBook.Helpers
{
    public enum TableStatus
    {
        Available,      // Table is free and ready
        Reserved,       // Table has booking starting within 30 minutes
        Booked,         // Table has active booking (current time within booking window)
        Occupied,       // Table is currently in use (active KOT)
        Maintenance     // Table is under maintenance
    }

    public class TableStatusHelper
    {
        private ConnectionContext _context;

        public TableStatusHelper()
        {
            _context = new ConnectionContext();
        }

        public TableStatusHelper(ConnectionContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get restaurant settings for a branch
        /// </summary>
        private ClsRestaurantSettings GetRestaurantSettings(long branchId)
        {
            return _context.DbClsRestaurantSettings
                .FirstOrDefault(s => s.BranchId == branchId && s.IsActive && !s.IsDeleted);
        }

        /// <summary>
        /// Check if a recurring booking would generate a booking for the given date/time
        /// </summary>
        private bool WouldRecurringBookingGenerateBooking(long recurringBookingId, DateTime checkDate, TimeSpan? checkTime, DateTime currentDate)
        {
            var recurring = _context.DbClsRecurringBooking
                .Where(r => r.RecurringBookingId == recurringBookingId && r.IsActive)
                .FirstOrDefault();
            
            if (recurring == null) return false;
            
            // Check if recurring booking is within valid date range
            if (checkDate < recurring.StartDate.Date) return false;
            if (!recurring.IsNeverExpires && recurring.EndDate != DateTime.MinValue && checkDate > recurring.EndDate.Date) return false;
            
            // If checkTime is provided, verify it matches the recurring booking time
            if (checkTime.HasValue && recurring.BookingTime.HasValue)
            {
                if (checkTime.Value != recurring.BookingTime.Value) return false;
            }
            
            // Check if date matches recurrence pattern
            if (recurring.RecurrenceType == "Daily")
            {
                var daysSinceStart = (checkDate - recurring.StartDate.Date).Days;
                if (daysSinceStart < 0) return false;
                if (daysSinceStart % recurring.RepeatEveryNumber != 0) return false;
                return true;
            }
            else if (recurring.RecurrenceType == "Weekly")
            {
                // Get days of week for this recurring booking
                var daysOfWeek = _context.DbClsRecurringBookingDay
                    .Where(rd => rd.RecurringBookingId == recurringBookingId)
                    .Select(rd => rd.DayOfWeek)
                    .ToList();
                
                if (!daysOfWeek.Any())
                {
                    // Fallback to start date's day of week
                    daysOfWeek = new List<int> { (int)recurring.StartDate.DayOfWeek };
                }
                
                if (!daysOfWeek.Contains((int)checkDate.DayOfWeek)) return false;
                
                // Calculate weeks since start, accounting for the week containing the start date
                // Find the first occurrence of the selected day of week on or after the start date
                var startDateDayOfWeek = (int)recurring.StartDate.DayOfWeek;
                var targetDayOfWeek = (int)checkDate.DayOfWeek;
                
                // Calculate days from start date to the check date
                var daysSinceStart = (checkDate - recurring.StartDate.Date).Days;
                
                // If check date is before start date, it can't match
                if (daysSinceStart < 0) return false;
                
                // Find the first occurrence of targetDayOfWeek on or after start date
                var daysToFirstOccurrence = 0;
                if (startDateDayOfWeek <= targetDayOfWeek)
                {
                    daysToFirstOccurrence = targetDayOfWeek - startDateDayOfWeek;
                }
                else
                {
                    daysToFirstOccurrence = (7 - startDateDayOfWeek) + targetDayOfWeek;
                }
                
                // If check date is before the first occurrence, it can't match
                if (daysSinceStart < daysToFirstOccurrence) return false;
                
                // Calculate which occurrence this is (0-based: 0 = first occurrence, 1 = second, etc.)
                var occurrenceNumber = (daysSinceStart - daysToFirstOccurrence) / 7;
                
                // Check if this occurrence matches the repeat interval
                if (occurrenceNumber % recurring.RepeatEveryNumber != 0) return false;
                
                return true;
            }
            else if (recurring.RecurrenceType == "Monthly")
            {
                var targetDayOfMonth = recurring.DayOfMonth ?? recurring.StartDate.Day;
                if (checkDate.Day != targetDayOfMonth) return false;
                
                var monthsSinceStart = (checkDate.Year - recurring.StartDate.Year) * 12 + (checkDate.Month - recurring.StartDate.Month);
                if (monthsSinceStart < 0) return false;
                if (monthsSinceStart % recurring.RepeatEveryNumber != 0) return false;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get table status - Calculates dynamically based on maintenance, KOTs, bookings, and manual status
        /// Priority: Maintenance > Occupied > Booked > Reserved > Available
        /// </summary>
        /// <param name="tableId">Table ID to check</param>
        /// <param name="forDateTime">DateTime to check status for (null = current time)</param>
        /// <param name="excludeBookingId">Booking ID to exclude from status check (0 = exclude none, useful when updating existing booking)</param>
        public TableStatus GetTableStatus(long tableId, DateTime? forDateTime = null, long excludeBookingId = 0)
        {
            DateTime checkTime;
            if (forDateTime.HasValue)
            {
                // If DateTime is in UTC, convert to local time (IST)
                // This ensures date comparisons work correctly
                if (forDateTime.Value.Kind == DateTimeKind.Utc)
                {
                    checkTime = forDateTime.Value.ToLocalTime();
                }
                else if (forDateTime.Value.Kind == DateTimeKind.Unspecified)
                {
                    // Treat unspecified as local time
                    checkTime = DateTime.SpecifyKind(forDateTime.Value, DateTimeKind.Local);
                }
                else
                {
                    checkTime = forDateTime.Value;
                }
            }
            else
            {
                checkTime = DateTime.Now;
            }
            
            var table = _context.DbClsRestaurantTable.FirstOrDefault(t => t.TableId == tableId && t.IsActive && !t.IsDeleted);

            if (table == null) return TableStatus.Available;

            // 1. Check Maintenance Mode (highest priority)
            if (table.IsMaintenanceMode &&
                table.MaintenanceFrom.HasValue &&
                table.MaintenanceTo.HasValue &&
                checkTime >= table.MaintenanceFrom.Value &&
                checkTime <= table.MaintenanceTo.Value)
            {
                return TableStatus.Maintenance;
            }

            // 2. Check Active KOT/Order
            var restaurantSettings = GetRestaurantSettings(table.BranchId);
            var lookbackMinutes = restaurantSettings != null && restaurantSettings.DefaultBookingDuration > 0
                ? restaurantSettings.DefaultBookingDuration
                : 240; // Default to 4 hours if not found
            var lookbackTime = checkTime.AddMinutes(-lookbackMinutes);
            
            var activeKot = _context.DbClsKotMaster
                .Where(k => k.TableId == tableId &&
                       k.OrderStatus != "Served" &&
                       k.OrderStatus != "Cancelled" &&
                       k.OrderTime < checkTime &&
                       k.OrderTime > lookbackTime &&
                       k.IsActive && !k.IsDeleted)
                .Any();

            if (activeKot)
            {
                return TableStatus.Occupied;
            }

            // 3. Check Current Booking (booking is active now or overlaps with checkTime)
            var checkTimeDate = checkTime.Date;
            var checkTimeValue = checkTime;
            // Fetch bookings for the date first, then filter by time in memory (to avoid LINQ to Entities issues with TimeSpan)
            var bookingsForDate = (from bt in _context.DbClsTableBookingTable
                                  join b in _context.DbClsTableBooking on bt.BookingId equals b.BookingId
                                  where bt.TableId == tableId &&
                                        System.Data.Entity.DbFunctions.TruncateTime(b.BookingDate) == checkTimeDate &&
                                        b.Status != "Cancelled" &&
                                        b.IsActive && !b.IsDeleted &&
                                        (excludeBookingId == 0 || b.BookingId != excludeBookingId)
                                  select new { bt, b.BookingDate, b.BookingTime, b.Duration }).ToList();
            
            // Check if checkTime falls within any booking's time window (Booked status)
            var currentBooking = bookingsForDate
                .Where(x => {
                    // Normalize BookingDate to ensure it's just a date (remove any time component)
                    var bookingDateOnly = x.BookingDate.Date;
                    var bookingStart = bookingDateOnly.Add(x.BookingTime);
                    var bookingEnd = bookingStart.AddMinutes(x.Duration);
                    // Check if checkTime is within the booking window (includes exact start time, excludes exact end time)
                    return checkTimeValue >= bookingStart && checkTimeValue < bookingEnd;
                })
                .Select(x => x.bt)
                .FirstOrDefault();

            if (currentBooking != null)
            {
                return TableStatus.Booked;
            }

            // 4. Check Reserved (booking starting within 30 minutes)
            // Standard Reserved check: booking is about to start (within 30 minutes)
            var checkTimePlus30 = checkTime.AddMinutes(30);
            var reservedBooking = bookingsForDate
                .Where(x => {
                    // Normalize BookingDate to ensure it's just a date (remove any time component)
                    var bookingDateOnly = x.BookingDate.Date;
                    var bookingStart = bookingDateOnly.Add(x.BookingTime);
                    
                    // Check if booking starts within 30 minutes after checkTime
                    return bookingStart > checkTimeValue && bookingStart <= checkTimePlus30;
                })
                .Select(x => x.bt)
                .FirstOrDefault();

            if (reservedBooking != null)
            {
                return TableStatus.Reserved;
            }

            // 3a. Check Recurring Bookings that would generate a booking for this date/time
            // Filter by CompanyId to ensure we only check recurring bookings for the same company
            var recurringBookingsForTable = (from rbt in _context.DbClsRecurringBookingTable
                                             join rb in _context.DbClsRecurringBooking on rbt.RecurringBookingId equals rb.RecurringBookingId
                                             where rbt.TableId == tableId &&
                                                   rb.IsActive &&
                                                   rb.CompanyId == table.CompanyId
                                             select new { 
                                                 rb.RecurringBookingId, 
                                                 rb.BookingTime, 
                                                 rb.Duration
                                             }).ToList();

            var currentDate = DateTime.Now;
            
            foreach (var recurring in recurringBookingsForTable)
            {
                // Check if this recurring booking would generate a booking on the check date
                // Pass null for checkTime to skip time matching - we'll check time overlap separately
                if (WouldRecurringBookingGenerateBooking(recurring.RecurringBookingId, checkTimeDate, null, currentDate))
                {
                    if (recurring.BookingTime.HasValue)
                    {
                        var recurringBookingStart = checkTimeDate.Add(recurring.BookingTime.Value);
                        var recurringBookingEnd = recurringBookingStart.AddMinutes(recurring.Duration > 0 ? recurring.Duration : 120);
                        
                        // Check if checkTime falls within this recurring booking window
                        if (checkTimeValue >= recurringBookingStart && checkTimeValue < recurringBookingEnd)
                        {
                            return TableStatus.Booked;
                        }
                        
                        // Check if booking starts within 30 minutes (Reserved status)
                        var recurringBookingStartPlus30 = checkTimeValue.AddMinutes(30);
                        if (recurringBookingStart > checkTimeValue && recurringBookingStart <= recurringBookingStartPlus30)
                        {
                            return TableStatus.Reserved;
                        }
                    }
                }
            }

            // 5. Check Manual Status (if set, use it; otherwise default to Available)
            if (!string.IsNullOrWhiteSpace(table.Status))
            {
                TableStatus manualStatus;
                if (Enum.TryParse<TableStatus>(table.Status, true, out manualStatus))
                {
                    // Only use manual status if it's not conflicting with dynamic status
                    // Manual status can override Available, but not Maintenance/Occupied/Booked/Reserved
                    if (manualStatus == TableStatus.Available || manualStatus == TableStatus.Maintenance)
                    {
                        return manualStatus;
                    }
                }
            }

            // 6. Default to Available
            return TableStatus.Available;
        }

        /// <summary>
        /// Check if table is available for time range
        /// </summary>
        /// <param name="tableId">Table ID to check</param>
        /// <param name="startDateTime">Start date/time of the booking</param>
        /// <param name="endDateTime">End date/time of the booking</param>
        /// <param name="excludeBookingId">Booking ID to exclude from availability check (0 = exclude none, useful when updating existing booking)</param>
        public bool IsTableAvailable(long tableId, DateTime startDateTime, DateTime endDateTime, long excludeBookingId = 0)
        {
            var table = _context.DbClsRestaurantTable.FirstOrDefault(t => t.TableId == tableId && t.IsActive && !t.IsDeleted);
            if (table == null) return false;

            // Check maintenance mode
            if (table.IsMaintenanceMode &&
                table.MaintenanceFrom.HasValue &&
                table.MaintenanceTo.HasValue &&
                !(endDateTime < table.MaintenanceFrom.Value || startDateTime > table.MaintenanceTo.Value))
            {
                return false;
            }

            // Check for active KOTs
            // Get lookback window from restaurant settings (DefaultBookingDuration in minutes)
            var restaurantSettings = GetRestaurantSettings(table.BranchId);
            var lookbackMinutes = restaurantSettings != null && restaurantSettings.DefaultBookingDuration > 0 
                ? restaurantSettings.DefaultBookingDuration 
                : 240; // Default to 4 hours (240 minutes) if not found
            var lookbackTime = startDateTime.AddMinutes(-lookbackMinutes);
            var activeKot = _context.DbClsKotMaster
                .Where(k => k.TableId == tableId &&
                       k.OrderStatus != "Served" &&
                       k.OrderStatus != "Cancelled" &&
                       k.OrderTime < endDateTime &&
                       k.OrderTime > lookbackTime &&
                       k.IsActive && !k.IsDeleted)
                .Any();

            if (activeKot) return false;

            // Check for existing bookings that overlap with the requested time range
            var startDateTimeDate = startDateTime.Date;
            // Fetch bookings for the date first, then filter by time overlap in memory
            var bookingsForOverlapCheck = (from bt in _context.DbClsTableBookingTable
                                         join b in _context.DbClsTableBooking on bt.BookingId equals b.BookingId
                                         where bt.TableId == tableId &&
                                               System.Data.Entity.DbFunctions.TruncateTime(b.BookingDate) == startDateTimeDate &&
                                               b.Status != "Cancelled" &&
                                               b.IsActive && !b.IsDeleted &&
                                               (excludeBookingId == 0 || b.BookingId != excludeBookingId)
                                         select new { bt, b.BookingDate, b.BookingTime, b.Duration }).ToList();
            
            var hasOverlappingBooking = bookingsForOverlapCheck
                .Any(x => {
                    var existingBookingStart = x.BookingDate.Add(x.BookingTime);
                    var existingBookingEnd = existingBookingStart.AddMinutes(x.Duration);
                    // Check for time overlap: bookings overlap if:
                    // newStart < existingEnd AND newEnd > existingStart
                    return existingBookingEnd > startDateTime && existingBookingStart < endDateTime;
                });

            if (hasOverlappingBooking) return false;

            // Check for recurring bookings that would generate overlapping bookings
            var currentDate = DateTime.Now;
            // Filter by CompanyId to ensure we only check recurring bookings for the same company
            var recurringBookingsForTable = (from rbt in _context.DbClsRecurringBookingTable
                                             join rb in _context.DbClsRecurringBooking on rbt.RecurringBookingId equals rb.RecurringBookingId
                                             where rbt.TableId == tableId &&
                                                   rb.IsActive &&
                                                   rb.CompanyId == table.CompanyId
                                             select new { 
                                                 rb.RecurringBookingId, 
                                                 rb.BookingTime, 
                                                 rb.Duration,
                                                 rb.RecurrenceType,
                                                 rb.StartDate,
                                                 rb.EndDate,
                                                 rb.IsNeverExpires,
                                                 rb.RepeatEveryNumber,
                                                 rb.DayOfMonth
                                             }).ToList();

            foreach (var recurring in recurringBookingsForTable)
            {
                // Check if this recurring booking would generate a booking for the requested date
                // We check if it would generate a booking at the recurring booking's scheduled time
                // Then check if that booking overlaps with the requested time range
                if (WouldRecurringBookingGenerateBooking(recurring.RecurringBookingId, startDateTimeDate, recurring.BookingTime, currentDate))
                {
                    if (recurring.BookingTime.HasValue)
                    {
                        var recurringBookingStart = startDateTimeDate.Add(recurring.BookingTime.Value);
                        var recurringBookingEnd = recurringBookingStart.AddMinutes(recurring.Duration > 0 ? recurring.Duration : 120);
                        
                        // Check for time overlap: bookings overlap if:
                        // newStart < existingEnd AND newEnd > existingStart
                        if (recurringBookingEnd > startDateTime && recurringBookingStart < endDateTime)
                        {
                            return false; // Table is not available due to recurring booking
                        }
                    }
                }
            }

            // Check manual status - only Available or Reserved tables can be booked
            var status = GetTableStatus(tableId, startDateTime, excludeBookingId);
            return status == TableStatus.Available || status == TableStatus.Reserved;
        }

        /// <summary>
        /// Get table status timeline for date range
        /// </summary>
        public Dictionary<DateTime, TableStatus> GetTableStatusForRange(
            long tableId, DateTime startDate, DateTime endDate, int intervalMinutes = 30)
        {
            var result = new Dictionary<DateTime, TableStatus>();
            var current = startDate;

            while (current <= endDate)
            {
                result[current] = GetTableStatus(tableId, current);
                current = current.AddMinutes(intervalMinutes);
            }

            return result;
        }

        /// <summary>
        /// Get table availability for specific date/time (quick check)
        /// </summary>
        public bool IsTableAvailable(long tableId, DateTime checkDateTime)
        {
            var status = GetTableStatus(tableId, checkDateTime);
            // Only Available or Reserved tables can accept new bookings
            return status == TableStatus.Available || status == TableStatus.Reserved;
        }
    }
}


