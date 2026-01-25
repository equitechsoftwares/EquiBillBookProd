using EquiBillBook.Filters;
using EquiBillBook.Helpers;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Data.Entity;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [AllowAnonymous] // Public access - no authentication required
    public class PublicTableBookingController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        TableStatusHelper tableStatusHelper = new TableStatusHelper();

        /// <summary>
        /// Resolve slug to CompanyId and BranchId from RestaurantSettings.
        ///
        /// Supports BOTH:
        ///  - Restaurant public booking slug (DbClsRestaurantSettings.PublicBookingSlug)
        ///  - Direct table booking slug (DbClsRestaurantTable.TableSlug)
        /// </summary>
        private (long CompanyId, long BranchId, ClsRestaurantSettings Settings) ResolveSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return (0, 0, null);
            }

            var normalizedSlug = slug.Trim().ToLower();

            // 1) Try to resolve as restaurant public booking slug (existing behaviour)
            var settings = oConnectionContext.DbClsRestaurantSettings
                .Where(s => s.PublicBookingSlug != null &&
                            s.PublicBookingSlug.ToLower() == normalizedSlug &&
                            s.EnablePublicBooking == true &&
                            s.IsActive == true &&
                            s.IsDeleted == false)
                .FirstOrDefault();

            if (settings != null)
            {
                return (settings.CompanyId, settings.BranchId, settings);
            }

            // 2) Fallback: resolve as direct table slug (used by /booktable/{slug})
            var table = oConnectionContext.DbClsRestaurantTable
                .Where(t => t.TableSlug != null &&
                            t.TableSlug.ToLower() == normalizedSlug &&
                            t.IsActive == true &&
                            t.IsDeleted == false)
                .Select(t => new { t.CompanyId, t.BranchId })
                .FirstOrDefault();

            if (table == null)
            {
                return (0, 0, null);
            }

            // Get restaurant settings for the table's branch
            settings = oConnectionContext.DbClsRestaurantSettings
                .Where(s => s.CompanyId == table.CompanyId &&
                            s.BranchId == table.BranchId &&
                            s.EnablePublicBooking == true &&
                            s.IsActive == true &&
                            s.IsDeleted == false)
                .FirstOrDefault();

            if (settings == null)
            {
                return (0, 0, null);
            }

            return (settings.CompanyId, settings.BranchId, settings);
        }

        /// <summary>
        /// Validate if a booking date and time is allowed based on normalized restaurant settings
        /// Returns: (isValid, errorMessage, validTimeSlots)
        /// </summary>
        private (bool IsValid, string ErrorMessage, List<TimeSpan> ValidTimeSlots) ValidateBookingDateTime(ClsRestaurantSettings settings, DateTime bookingDate, TimeSpan? bookingTime = null, long companyId = 0)
        {
            var bookingDateOnly = bookingDate.Date;

            // Step 0: Check booking advance days limit (if companyId is provided)
            if (companyId > 0 && settings.PublicBookingAdvanceDays > 0)
            {
                var currentDate = oCommonController.CurrentDate(companyId).Date;
                var maxBookingDate = currentDate.AddDays(settings.PublicBookingAdvanceDays);
                
                if (bookingDateOnly < currentDate)
                {
                    return (false, "Booking date cannot be in the past", new List<TimeSpan>());
                }
                
                if (bookingDateOnly > maxBookingDate)
                {
                    return (false, $"Bookings can only be made up to {settings.PublicBookingAdvanceDays} days in advance", new List<TimeSpan>());
                }
            }

            // Step 1: Check for date override first
            // Use DbFunctions.TruncateTime to ensure proper date comparison (ignoring time component)
            var dateOverride = oConnectionContext.DbClsRestaurantBookingDateOverride
                .Where(d => d.RestaurantSettingsId == settings.RestaurantSettingsId &&
                           System.Data.Entity.DbFunctions.TruncateTime(d.OverrideDate) == bookingDateOnly &&
                           d.IsActive &&
                           !d.IsDeleted)
                .FirstOrDefault();

            List<TimeSpan> validTimeSlots = new List<TimeSpan>();

            // If date has override
            if (dateOverride != null)
            {
                // If date is closed, return invalid
                if (dateOverride.IsClosed)
                {
                    return (false, "Restaurant is closed on this date", new List<TimeSpan>());
                }
                else
                {
                    // Date is open but has override time slots - use those
                    validTimeSlots = oConnectionContext.DbClsRestaurantBookingDateOverrideSlot
                        .Where(s => s.BookingDateOverrideId == dateOverride.BookingDateOverrideId &&
                                   s.IsActive &&
                                   !s.IsDeleted)
                        .OrderBy(s => s.TimeSlot)
                        .Select(s => s.TimeSlot)
                        .ToList();

                    // If specific time is provided, validate it against override slots
                    if (bookingTime.HasValue)
                    {
                        if (!validTimeSlots.Contains(bookingTime.Value))
                        {
                            return (false, "The selected time slot is not available on this date", validTimeSlots);
                        }
                    }
                }
            }
            else
            {
                // No date override - check operating days
                var dayOfWeek = (int)bookingDateOnly.DayOfWeek; // 0=Sunday, 1=Monday, etc.
                var isOperatingDay = oConnectionContext.DbClsRestaurantOperatingDay
                    .Any(od => od.RestaurantSettingsId == settings.RestaurantSettingsId &&
                              od.DayOfWeek == dayOfWeek &&
                              od.IsActive &&
                              !od.IsDeleted);

                // If not an operating day, return invalid
                if (!isOperatingDay)
                {
                    return (false, "Restaurant is closed on this day of the week", new List<TimeSpan>());
                }

                // It's an operating day - get regular time slots
                // Check if using Manual mode (normalized time slots) or Auto mode (generate from start/end time)
                // Use case-insensitive comparison and trim whitespace
                var bookingMode = settings.BookingTimeSlotMode != null ? settings.BookingTimeSlotMode.Trim() : "";
                
                if (string.Equals(bookingMode, "Manual", StringComparison.OrdinalIgnoreCase))
                {
                    // Get time slots from normalized table
                    validTimeSlots = oConnectionContext.DbClsRestaurantBookingTimeSlot
                        .Where(ts => ts.RestaurantSettingsId == settings.RestaurantSettingsId &&
                                    ts.IsActive &&
                                    !ts.IsDeleted)
                        .OrderBy(ts => ts.DisplayOrder)
                        .ThenBy(ts => ts.TimeSlot)
                        .Select(ts => ts.TimeSlot)
                        .ToList();

                    // If specific time is provided, validate it against manual slots
                    if (bookingTime.HasValue)
                    {
                        if (!validTimeSlots.Contains(bookingTime.Value))
                        {
                            return (false, "The selected time slot is not available", validTimeSlots);
                        }
                    }
                }
                else if (string.Equals(bookingMode, "Auto", StringComparison.OrdinalIgnoreCase))
                {
                    // Auto mode - validate against start/end time range
                    // Only validate if start and end times are configured
                    if (settings.BookingStartTime.HasValue && settings.BookingEndTime.HasValue)
                    {
                        var startTime = settings.BookingStartTime.Value;
                        var endTime = settings.BookingEndTime.Value;

                        // If specific time is provided, validate it's within range
                        if (bookingTime.HasValue)
                        {
                            if (bookingTime.Value < startTime || bookingTime.Value > endTime)
                            {
                                return (false, $"Booking time must be between {startTime.ToString(@"hh\:mm")} and {endTime.ToString(@"hh\:mm")}", new List<TimeSpan>());
                            }
                        }
                        // In auto mode, we don't have explicit slots, but we can validate the time is within range
                        // The validTimeSlots list will be empty for auto mode (caller can generate slots if needed)
                    }
                    else
                    {
                        // Auto mode but no start/end times configured - invalid configuration
                        if (bookingTime.HasValue)
                        {
                            return (false, "Booking time slots are not configured for this restaurant", new List<TimeSpan>());
                        }
                    }
                }
                // If mode is neither Manual nor Auto (or null/invalid), return invalid (no fallback)
                else
                {
                    return (false, "Booking time slot mode is not properly configured", new List<TimeSpan>());
                }
            }

            // All validations passed
            return (true, null, validTimeSlots);
        }

        /// <summary>
        /// Get available tables for a specific date and time
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GetAvailableTables(PublicBookingRequest obj)
        {
            if (string.IsNullOrEmpty(obj.Slug))
            {
                data = new
                {
                    Status = 0,
                    Message = "Slug is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var (companyId, branchId, settings) = ResolveSlug(obj.Slug);
            if (companyId == 0 || settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking not available or invalid slug",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            if (!obj.BookingDate.HasValue || string.IsNullOrEmpty(obj.BookingTimeString) || !obj.NoOfGuests.HasValue || obj.NoOfGuests.Value <= 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking date, time, and number of guests are required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Parse BookingTime string to TimeSpan
            if (!TimeSpan.TryParse(obj.BookingTimeString, out TimeSpan bookingTime))
            {
                data = new
                {
                    Status = 0,
                    Message = "Invalid booking time format. Expected format: HH:mm or HH:mm:ss",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var bookingDate = obj.BookingDate.Value.Date;
            var duration = obj.Duration > 0 ? obj.Duration : (settings.DefaultBookingDuration > 0 ? settings.DefaultBookingDuration : 120);
            var noOfGuests = obj.NoOfGuests.Value;

            // Validate booking date and time against normalized settings (operating days, date overrides, time slots)
            var (isValid, errorMessage, validTimeSlots) = ValidateBookingDateTime(settings, bookingDate, bookingTime, companyId);
            if (!isValid)
            {
                data = new
                {
                    Status = 0,
                    Message = errorMessage,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var bookingStart = bookingDate.Add(bookingTime);
            var bookingEnd = bookingStart.AddMinutes(duration);

            // Get all active tables for this branch
            var allTables = oConnectionContext.DbClsRestaurantTable
                .Where(t => t.CompanyId == companyId && 
                           t.BranchId == branchId && 
                           t.IsActive == true && 
                           t.IsDeleted == false)
                .ToList();

            // Get all tables with their status (not just available ones)
            // This allows showing the full layout with occupied/reserved tables visible
            var tablesWithStatus = allTables
                .Select(t => new
                {
                    TableId = t.TableId,
                    TableNo = t.TableNo,
                    TableName = t.TableName,
                    Capacity = t.Capacity,
                    FloorId = t.FloorId,
                    FloorName = oConnectionContext.DbClsRestaurantFloor
                        .Where(f => f.FloorId == t.FloorId)
                        .Select(f => f.FloorName)
                        .FirstOrDefault(),
                    PositionX = t.PositionX,
                    PositionY = t.PositionY,
                    Status = tableStatusHelper.GetTableStatus(t.TableId, bookingStart).ToString(),
                    IsAvailable = tableStatusHelper.IsTableAvailable(t.TableId, bookingStart, bookingEnd)
                })
                .OrderBy(t => t.Capacity)
                .ThenBy(t => t.TableNo)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AvailableTables = tablesWithStatus, // Return all tables with status
                    BookingDate = bookingDate,
                    BookingTime = bookingTime,
                    Duration = duration,
                    NoOfGuests = noOfGuests
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Get available time slots for a specific date
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GetAvailableTimeSlots(PublicBookingRequest obj)
        {
            if (string.IsNullOrEmpty(obj.Slug))
            {
                data = new
                {
                    Status = 0,
                    Message = "Slug is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var (companyId, branchId, settings) = ResolveSlug(obj.Slug);
            if (companyId == 0 || settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking not available or invalid slug",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            if (!obj.BookingDate.HasValue)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking date is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var bookingDate = obj.BookingDate.Value.Date;
            var duration = obj.Duration > 0 ? obj.Duration : (settings.DefaultBookingDuration > 0 ? settings.DefaultBookingDuration : 120);

            // DEBUG: Log initial parameters
            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Slug: {obj.Slug}, BookingDate: {bookingDate:yyyy-MM-dd}, RestaurantSettingsId: {settings.RestaurantSettingsId}");

            // Step 1: Check for date override first
            // Use EntityFunctions.TruncateTime or compare date parts to ensure proper date comparison
            var dateOverride = oConnectionContext.DbClsRestaurantBookingDateOverride
                .Where(d => d.RestaurantSettingsId == settings.RestaurantSettingsId &&
                           System.Data.Entity.DbFunctions.TruncateTime(d.OverrideDate) == bookingDate &&
                           d.IsActive &&
                           !d.IsDeleted)
                .FirstOrDefault();

            // DEBUG: Log date override check result
            if (dateOverride != null)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Date override FOUND - BookingDateOverrideId: {dateOverride.BookingDateOverrideId}, IsClosed: {dateOverride.IsClosed}, OverrideDate: {dateOverride.OverrideDate:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] No date override found for date: {bookingDate:yyyy-MM-dd}");
                // DEBUG: Check if any overrides exist for this restaurant
                var allOverrides = oConnectionContext.DbClsRestaurantBookingDateOverride
                    .Where(d => d.RestaurantSettingsId == settings.RestaurantSettingsId &&
                               d.IsActive &&
                               !d.IsDeleted)
                    .Select(d => new { d.OverrideDate, d.BookingDateOverrideId })
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Total active overrides for restaurant: {allOverrides.Count}");
                foreach (var ovr in allOverrides)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots]   - OverrideDate: {ovr.OverrideDate:yyyy-MM-dd HH:mm:ss}, BookingDateOverrideId: {ovr.BookingDateOverrideId}");
                }
            }

            var timeSlots = new List<TimeSpan>();

            // If date has override
            if (dateOverride != null)
            {
                // If date is closed, return empty slots
                if (dateOverride.IsClosed)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Date override is CLOSED - returning empty slots");
                    timeSlots = new List<TimeSpan>(); // Empty - restaurant is closed
                }
                else
                {
                    // Date is open but has override time slots - use those
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Date override is OPEN - fetching override slots for BookingDateOverrideId: {dateOverride.BookingDateOverrideId}");
                    
                    var overrideSlotsQuery = oConnectionContext.DbClsRestaurantBookingDateOverrideSlot
                        .Where(s => s.BookingDateOverrideId == dateOverride.BookingDateOverrideId &&
                                   s.IsActive &&
                                   !s.IsDeleted);
                    
                    // DEBUG: Log query details
                    var allOverrideSlots = overrideSlotsQuery.ToList();
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Found {allOverrideSlots.Count} override slots (before filtering)");
                    foreach (var slot in allOverrideSlots)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots]   - OverrideSlotId: {slot.OverrideSlotId}, TimeSlot: {slot.TimeSlot}, IsActive: {slot.IsActive}, IsDeleted: {slot.IsDeleted}");
                    }
                    
                    timeSlots = overrideSlotsQuery
                        .OrderBy(s => s.TimeSlot)
                        .Select(s => s.TimeSlot)
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Final override time slots count: {timeSlots.Count}");
                    foreach (var slot in timeSlots)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots]   - TimeSlot: {slot}");
                    }
                }
            }
            else
            {
                // No date override - check operating days
                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] No date override - checking operating days");
                var dayOfWeek = (int)bookingDate.DayOfWeek; // 0=Sunday, 1=Monday, etc.
                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Day of week: {dayOfWeek} ({bookingDate.DayOfWeek})");
                
                var isOperatingDay = oConnectionContext.DbClsRestaurantOperatingDay
                    .Any(od => od.RestaurantSettingsId == settings.RestaurantSettingsId &&
                              od.DayOfWeek == dayOfWeek &&
                              od.IsActive &&
                              !od.IsDeleted);

                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Is operating day: {isOperatingDay}");

                // If not an operating day, return empty slots
                if (!isOperatingDay)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Not an operating day - returning empty slots");
                    timeSlots = new List<TimeSpan>(); // Empty - restaurant is closed on this day
                }
                else
                {
                    // It's an operating day - get regular time slots
                    // Check if using Manual mode (normalized time slots) or Auto mode (generate from start/end time)
                    // Use case-insensitive comparison and trim whitespace
                    var bookingMode = !string.IsNullOrWhiteSpace(settings.BookingTimeSlotMode) ? settings.BookingTimeSlotMode.Trim() : "";
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Booking mode: '{bookingMode}'");
                    
                    if (string.Equals(bookingMode, "Manual", StringComparison.OrdinalIgnoreCase))
                    {
                        // Get time slots from normalized table - ONLY these slots, no fallback
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Manual mode - fetching time slots from normalized table");
                        timeSlots = oConnectionContext.DbClsRestaurantBookingTimeSlot
                            .Where(ts => ts.RestaurantSettingsId == settings.RestaurantSettingsId && 
                                        ts.IsActive && 
                                        !ts.IsDeleted)
                            .OrderBy(ts => ts.DisplayOrder)
                            .ThenBy(ts => ts.TimeSlot)
                            .Select(ts => ts.TimeSlot)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Manual mode - found {timeSlots.Count} time slots");
                        // In Manual mode, return only configured slots (even if empty) - NO FALLBACK TO AUTO
                    }
                    else if (string.Equals(bookingMode, "Auto", StringComparison.OrdinalIgnoreCase))
                    {
                        // Auto mode - generate slots based on start/end time and duration
                        // Only generate if start and end times are configured
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Auto mode - generating slots");
                        if (settings.BookingStartTime.HasValue && settings.BookingEndTime.HasValue && duration > 0)
                        {
                            var startTime = settings.BookingStartTime.Value;
                            var endTime = settings.BookingEndTime.Value;
                            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Auto mode - StartTime: {startTime}, EndTime: {endTime}, Duration: {duration}");
                            // Use booking duration as interval to prevent overlapping bookings on same table
                            // Slots will be every [duration] minutes (e.g., if duration is 120 minutes, slots will be every 120 minutes)
                            var intervalMinutes = duration;
                            timeSlots = new List<TimeSpan>(); // Initialize new list for auto-generated slots
                            for (var time = startTime; time <= endTime; time = time.Add(new TimeSpan(0, intervalMinutes, 0)))
                            {
                                timeSlots.Add(time);
                            }
                            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Auto mode - generated {timeSlots.Count} time slots");
                        }
                        else
                        {
                            // Auto mode but no start/end times or duration configured - return empty slots (no fallback)
                            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Auto mode - missing configuration (StartTime: {settings.BookingStartTime.HasValue}, EndTime: {settings.BookingEndTime.HasValue}, Duration: {duration})");
                            timeSlots = new List<TimeSpan>();
                        }
                    }
                    else
                    {
                        // Mode is null, empty, or invalid - return empty slots (no fallback, no auto-generation)
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Invalid booking mode - returning empty slots");
                        timeSlots = new List<TimeSpan>();
                    }
                }
            }

            // Filter available time slots based on table availability
            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Total time slots before filtering: {timeSlots.Count}");
            
            var availableSlots = new List<object>();
            var allTables = oConnectionContext.DbClsRestaurantTable
                .Where(t => t.CompanyId == companyId && 
                           t.BranchId == branchId && 
                           t.IsActive == true && 
                           t.IsDeleted == false)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Total active tables: {allTables.Count}");

            // Get number of guests (if provided) to filter by capacity
            var noOfGuests = obj.NoOfGuests ?? 0;
            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Number of guests: {noOfGuests}");

            foreach (var slot in timeSlots)
            {
                var slotStart = bookingDate.Add(slot);
                var slotEnd = slotStart.AddMinutes(duration);
                
                // Check if any table is available for this time slot
                // Also check capacity if number of guests is specified
                var hasAvailableTable = allTables.Any(t => 
                    tableStatusHelper.IsTableAvailable(t.TableId, slotStart, slotEnd) &&
                    (noOfGuests <= 0 || t.Capacity >= noOfGuests)); // If guests specified, check capacity

                System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Slot: {slot}, Start: {slotStart:yyyy-MM-dd HH:mm:ss}, End: {slotEnd:yyyy-MM-dd HH:mm:ss}, IsAvailable: {hasAvailableTable}");

                availableSlots.Add(new
                {
                    Time = slot.ToString(@"hh\:mm"),
                    TimeValue = slot.ToString(),
                    IsAvailable = hasAvailableTable
                });
            }

            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Final available slots count: {availableSlots.Count}");
            var slotDetails = string.Join(", ", availableSlots.Select(s => 
            {
                dynamic slot = s;
                return $"{slot.Time} ({slot.IsAvailable})";
            }));
            System.Diagnostics.Debug.WriteLine($"[GetAvailableTimeSlots] Available slots: {slotDetails}");

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TimeSlots = availableSlots,
                    BookingDate = bookingDate
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Check if specific tables are available
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> CheckTableAvailability(PublicBookingRequest obj)
        {
            if (string.IsNullOrEmpty(obj.Slug) || obj.TableIds == null || !obj.TableIds.Any() || !obj.BookingDate.HasValue || string.IsNullOrEmpty(obj.BookingTimeString))
            {
                data = new
                {
                    Status = 0,
                    Message = "Slug, TableIds, BookingDate, and BookingTimeString are required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Parse BookingTime string to TimeSpan
            if (!TimeSpan.TryParse(obj.BookingTimeString, out TimeSpan bookingTime))
            {
                data = new
                {
                    Status = 0,
                    Message = "Invalid booking time format. Expected format: HH:mm or HH:mm:ss",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var (companyId, branchId, settings) = ResolveSlug(obj.Slug);
            if (companyId == 0 || settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking not available or invalid slug",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var bookingDate = obj.BookingDate.Value.Date;
            var duration = obj.Duration > 0 ? obj.Duration : (settings.DefaultBookingDuration > 0 ? settings.DefaultBookingDuration : 120);

            // Validate booking date and time against normalized settings (operating days, date overrides, time slots)
            var (isValid, errorMessage, validTimeSlots) = ValidateBookingDateTime(settings, bookingDate, bookingTime, companyId);
            if (!isValid)
            {
                data = new
                {
                    Status = 0,
                    Message = errorMessage,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var bookingStart = bookingDate.Add(bookingTime);
            var bookingEnd = bookingStart.AddMinutes(duration);

            // Check if all tables exist and belong to this company/branch
            var tables = oConnectionContext.DbClsRestaurantTable
                .Where(t => obj.TableIds.Contains(t.TableId) && 
                           t.CompanyId == companyId && 
                           t.BranchId == branchId && 
                           t.IsActive == true && 
                           t.IsDeleted == false)
                .ToList();

            if (tables.Count != obj.TableIds.Count)
            {
                data = new
                {
                    Status = 0,
                    Message = "One or more tables not found or invalid",
                    Data = new
                    {
                        IsAvailable = false,
                        TableIds = obj.TableIds,
                        BookingDate = bookingDate,
                        BookingTime = bookingTime
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            // Check availability for all tables
            var isAvailable = true;
            var unavailableTables = new List<long>();
            foreach (var tableId in obj.TableIds)
            {
                if (!tableStatusHelper.IsTableAvailable(tableId, bookingStart, bookingEnd))
                {
                    isAvailable = false;
                    unavailableTables.Add(tableId);
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    IsAvailable = isAvailable,
                    TableIds = obj.TableIds,
                    UnavailableTableIds = unavailableTables,
                    BookingDate = bookingDate,
                    BookingTime = bookingTime
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Generate a unique token for public booking cancellation
        /// </summary>
        private string GenerateBookingToken(long bookingId, string bookingNo, DateTime bookingDate)
        {
            var input = $"{bookingId}|{bookingNo}|{bookingDate:yyyyMMdd}";
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").Replace("=", "").Substring(0, 16);
            }
        }

        /// <summary>
        /// Lookup or create customer by mobile number
        /// </summary>
        private async Task<long> LookupOrCreateCustomer(string customerName, string customerMobile, string customerEmail, long companyId, long branchId)
        {
            if (string.IsNullOrWhiteSpace(customerMobile))
            {
                return 0;
            }

            // Normalize mobile number (remove spaces, dashes, etc.)
            var normalizedMobile = customerMobile.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Try to find existing customer by mobile
            var existingCustomer = oConnectionContext.DbClsUser
                .Where(u => u.CompanyId == companyId &&
                           u.UserType.ToLower() == "customer" &&
                           u.MobileNo.Trim() == normalizedMobile &&
                           u.IsDeleted == false)
                .FirstOrDefault();

            if (existingCustomer != null)
            {
                // Update customer info if provided
                if (!string.IsNullOrWhiteSpace(customerName) && existingCustomer.Name != customerName)
                {
                    existingCustomer.Name = customerName;
                }
                if (!string.IsNullOrWhiteSpace(customerEmail) && existingCustomer.EmailId != customerEmail)
                {
                    existingCustomer.EmailId = customerEmail;
                }
                oConnectionContext.SaveChanges();
                return existingCustomer.UserId;
            }

            // Create new customer
            try
            {
                // Get CountryId and Branch StateId for PlaceOfSupplyId validation
                var countryId = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.CompanyId == companyId)
                    .Select(a => a.CountryId)
                    .FirstOrDefault();

                var branchStateId = oConnectionContext.DbClsBranch
                    .Where(a => a.BranchId == branchId)
                    .Select(a => a.StateId)
                    .FirstOrDefault();

                // Set PlaceOfSupplyId: required for India (CountryId == 2) when GstTreatment is not "Export of Goods / Services (Zero-Rated Supply)"
                // Default to branch's StateId for India, otherwise 0
                long placeOfSupplyId = 0;
                if (countryId == 2)
                {
                    placeOfSupplyId = branchStateId > 0 ? branchStateId : 0;
                }

                var userController = new UserController();
                var userVm = new ClsUserVm
                {
                    CompanyId = companyId,
                    AddedBy = 0, // System user for public bookings
                    UserType = "customer",
                    Name = customerName?.Trim() ?? "",
                    MobileNo = normalizedMobile,
                    EmailId = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim(),
                    IsActive = true,
                    IsDeleted = false,
                    JoiningDate = oCommonController.CurrentDate(companyId),
                    BranchId = branchId,
                    GstTreatment = "Consumer",
                    TaxPreference = "Taxable",
                    PlaceOfSupplyId = placeOfSupplyId,
                    IpAddress = GetClientIpAddress(),
                    Browser = Request?.Properties.ContainsKey("MS_HttpContext") != null ? 
                        ((System.Web.HttpContextWrapper)Request.Properties["MS_HttpContext"])?.Request?.Browser?.Browser ?? "Public Booking" : 
                        "Public Booking",
                    Platform = Request?.Properties.ContainsKey("MS_HttpContext") != null ? 
                        ((System.Web.HttpContextWrapper)Request.Properties["MS_HttpContext"])?.Request?.Browser?.Platform ?? "web" : 
                        "web"
                };

                var request = new System.Net.Http.HttpRequestMessage();
                var config = new System.Web.Http.HttpConfiguration();
                userController.ControllerContext = new System.Web.Http.Controllers.HttpControllerContext(
                    config,
                    new System.Web.Http.Routing.HttpRouteData(new System.Web.Http.Routing.HttpRoute()),
                    request);

                var actionResult = await userController.InsertUser(userVm);
                var responseMessage = await actionResult.ExecuteAsync(System.Threading.CancellationToken.None);
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var serializer = new JavaScriptSerializer();
                var response = serializer.Deserialize<ClsResponse>(responseContent);

                if (response != null && response.Status == 1 && response.Data != null && response.Data.User != null && response.Data.User.UserId > 0)
                {
                    return response.Data.User.UserId;
                }
            }
            catch
            {
                // If customer creation fails, return 0 - booking can still proceed
            }

            return 0;
        }

        /// <summary>
        /// Initialize payment gateway for booking deposit
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> InitBookingDepositPayment(PublicBookingRequest obj)
        {
            if (string.IsNullOrEmpty(obj.Slug))
            {
                data = new
                {
                    Status = 0,
                    Message = "Slug is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var (companyId, branchId, settings) = ResolveSlug(obj.Slug);
            if (companyId == 0 || settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking not available or invalid slug",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Calculate deposit amount
            decimal depositAmount = 0;
            if (settings.PublicBookingRequireDeposit)
            {
                var depositMode = settings.PublicBookingDepositMode ?? "Fixed";
                if (depositMode.Equals("PerGuest", StringComparison.OrdinalIgnoreCase))
                {
                    var perGuestAmount = settings.PublicBookingDepositPerGuestAmount;
                    var guestCount = obj.NoOfGuests ?? 0;
                    depositAmount = guestCount * perGuestAmount;
                }
                else
                {
                    depositAmount = settings.PublicBookingDepositFixedAmount;
                }
            }

            // If no deposit required, return success with DepositAmount = 0
            // This allows the JavaScript to proceed directly to booking creation
            if (depositAmount <= 0)
            {
                data = new
                {
                    Status = 1,
                    Message = "Deposit is not required for this booking",
                    Data = new
                    {
                        DepositAmount = 0,
                        OnlinePaymentSetting = (object)null,
                        BusinessSetting = (object)null,
                        BookingData = new
                        {
                            obj.Slug,
                            obj.BookingDate,
                            obj.BookingTimeString,
                            obj.NoOfGuests,
                            obj.TableIds,
                            obj.CustomerName,
                            obj.CustomerMobile,
                            obj.CustomerEmail,
                            obj.SpecialRequest,
                            obj.Duration
                        }
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            // Get payment gateway settings
            var onlinePaymentSetting = oConnectionContext.DbClsOnlinePaymentSettings
                .Where(a => a.CompanyId == companyId &&
                           a.IsDefault == true &&
                           a.IsActive == true &&
                           a.IsDeleted == false)
                .Select(a => new
                {
                    a.OnlinePaymentSettingsId,
                    a.RazorpayKey,
                    a.RazorpayCurrencyId,
                    a.OnlinePaymentService,
                    a.PaypalClientId,
                    a.PaypalCurrencyId,
                    PaypalCurrencyCode = oConnectionContext.DbClsCurrency
                        .Where(c => c.CurrencyId == a.PaypalCurrencyId)
                        .Select(c => c.CurrencyCode)
                        .FirstOrDefault(),
                    RazorpayCurrencyCode = oConnectionContext.DbClsCurrency
                        .Where(c => c.CurrencyId == a.RazorpayCurrencyId)
                        .Select(c => c.CurrencyCode)
                        .FirstOrDefault(),
                })
                .FirstOrDefault();

            if (onlinePaymentSetting == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Payment gateway is not configured",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Get business settings
            var businessSetting = oConnectionContext.DbClsBusinessSettings
                .Where(a => a.CompanyId == companyId)
                .Select(a => new
                {
                    a.BusinessLogo,
                    a.BusinessName,
                    CurrencyCode = oConnectionContext.DbClsCountry
                        .Where(b => b.CountryId == a.CountryId)
                        .Select(b => b.CurrencyCode)
                        .FirstOrDefault(),
                    CurrencySymbol = oConnectionContext.DbClsCountry
                        .Where(b => b.CountryId == a.CountryId)
                        .Select(b => b.CurrencySymbol)
                        .FirstOrDefault(),
                })
                .FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "Payment gateway initialized",
                Data = new
                {
                    DepositAmount = depositAmount,
                    OnlinePaymentSetting = onlinePaymentSetting,
                    BusinessSetting = businessSetting,
                    BookingData = new
                    {
                        obj.Slug,
                        obj.BookingDate,
                        obj.BookingTimeString,
                        obj.NoOfGuests,
                        obj.TableIds,
                        obj.CustomerName,
                        obj.CustomerMobile,
                        obj.CustomerEmail,
                        obj.SpecialRequest,
                        obj.Duration
                    }
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Create a public booking after successful payment
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> CreatePublicBooking(PublicBookingRequest obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            if (string.IsNullOrEmpty(obj.Slug))
            {
                errors.Add(new ClsError { Message = "Slug is required", Id = "divSlug" });
                isError = true;
            }

            if (string.IsNullOrEmpty(obj.CustomerName))
            {
                errors.Add(new ClsError { Message = "Customer name is required", Id = "divCustomerName" });
                isError = true;
            }

            if (string.IsNullOrEmpty(obj.CustomerMobile))
            {
                errors.Add(new ClsError { Message = "Customer mobile is required", Id = "divCustomerMobile" });
                isError = true;
            }

            if (string.IsNullOrEmpty(obj.CustomerEmail))
            {
                errors.Add(new ClsError { Message = "Customer email is required", Id = "divCustomerEmail" });
                isError = true;
            }

            if (!obj.BookingDate.HasValue)
            {
                errors.Add(new ClsError { Message = "Booking date is required", Id = "divBookingDate" });
                isError = true;
            }

            if (string.IsNullOrEmpty(obj.BookingTimeString))
            {
                errors.Add(new ClsError { Message = "Booking time is required", Id = "divBookingTime" });
                isError = true;
            }

            if (!obj.NoOfGuests.HasValue || obj.NoOfGuests.Value <= 0)
            {
                errors.Add(new ClsError { Message = "Number of guests must be greater than 0", Id = "divNoOfGuests" });
                isError = true;
            }

            // Validate table selection
            var tableIds = obj.TableIds != null ? obj.TableIds.Where(t => t > 0).Distinct().ToList() : new List<long>();

            if (tableIds.Count == 0)
            {
                errors.Add(new ClsError { Message = "At least one table selection is required", Id = "divTableId" });
                isError = true;
            }

            if (isError)
            {
                data = new
                {
                    Status = 2,
                    Message = "Validation failed",
                    Errors = errors,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var (companyId, branchId, settings) = ResolveSlug(obj.Slug);
            if (companyId == 0 || settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking not available or invalid slug",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var CurrentDate = oCommonController.CurrentDate(companyId);

            // Parse BookingTime from BookingTimeString (JSON sends it as "HH:mm" string)
            TimeSpan bookingTime = default(TimeSpan);
            if (!string.IsNullOrEmpty(obj.BookingTimeString))
            {
                try
                {
                    // Parse time string (format: HH:mm or HH:mm:ss)
                    var timeParts = obj.BookingTimeString.Split(':');
                    if (timeParts.Length >= 2)
                    {
                        int hours = 0, minutes = 0;
                        if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                        {
                            // Validate hours and minutes
                            if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                            {
                                bookingTime = new TimeSpan(hours, minutes, 0);
                            }
                        }
                    }
                }
                catch
                {
                    // If parsing fails, validation will catch if it's invalid
                }
            }

            // Validate BookingTime - ensure it's not default (00:00:00) unless explicitly set to midnight
            if (bookingTime == default(TimeSpan))
            {
                errors.Add(new ClsError { Message = "Invalid booking time format. Expected format: HH:mm or HH:mm:ss", Id = "divBookingTime" });
                data = new
                {
                    Status = 2,
                    Message = "Validation failed",
                    Errors = errors,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Apply timezone adjustment to booking date (same as CreateStandalone)
            var bookingDate = obj.BookingDate.Value;
            bookingDate = bookingDate.AddHours(5).AddMinutes(30);
            var bookingDateOnly = bookingDate.Date;

            var duration = obj.Duration > 0 ? obj.Duration : (settings.DefaultBookingDuration > 0 ? settings.DefaultBookingDuration : 120);

            // Validate booking date and time against normalized settings (operating days, date overrides, time slots)
            var (isValid, errorMessage, validTimeSlots) = ValidateBookingDateTime(settings, bookingDateOnly, bookingTime);
            if (!isValid)
            {
                errors.Add(new ClsError { Message = errorMessage, Id = "divBookingDate" });
                data = new
                {
                    Status = 2,
                    Message = "Validation failed",
                    Errors = errors,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var bookingStart = bookingDateOnly.Add(bookingTime);
            var bookingEnd = bookingStart.AddMinutes(duration);

            // Verify all tables exist and belong to company/branch
            var tables = oConnectionContext.DbClsRestaurantTable
                .Where(t => tableIds.Contains(t.TableId) && 
                           t.CompanyId == companyId && 
                           t.BranchId == branchId && 
                           t.IsActive == true && 
                           t.IsDeleted == false)
                .ToList();

            if (tables.Count != tableIds.Count)
            {
                data = new
                {
                    Status = 2,
                    Message = "One or more tables not found or invalid",
                    Errors = new[] { new ClsError { Message = "One or more tables not found or invalid", Id = "divTableId" } },
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Ensure total capacity of selected tables can accommodate the guests
            var totalCapacity = tables.Sum(t => t.Capacity);
            if (obj.NoOfGuests.Value > totalCapacity)
            {
                data = new
                {
                    Status = 2,
                    Message = "Number of guests exceeds the total capacity of the selected table(s).",
                    Errors = new[] { new ClsError { Message = "Number of guests exceeds the total capacity of the selected table(s).", Id = "divNoOfGuests" } },
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Final availability check for all tables
            var unavailableTables = new List<long>();
            foreach (var tableId in tableIds)
            {
                if (!tableStatusHelper.IsTableAvailable(tableId, bookingStart, bookingEnd))
                {
                    unavailableTables.Add(tableId);
                }
            }

            if (unavailableTables.Any())
            {
                data = new
                {
                    Status = 2,
                    Message = "One or more tables are no longer available for the selected date and time",
                    Errors = new[] { new ClsError { Message = "One or more tables are no longer available for the selected date and time", Id = "divTableId" } },
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Lookup or create customer OUTSIDE of the transaction scope to avoid
            // nested async TransactionScope issues when InsertUser also uses TransactionScope.
            long customerId = await LookupOrCreateCustomer(obj.CustomerName, obj.CustomerMobile, obj.CustomerEmail, companyId, branchId);

            // Declare booking variable outside transaction scope so it can be used after transaction completes
            ClsTableBooking oBooking = null;

            // Wrap only the booking / prefix work in a TransactionScope (no awaits inside)
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Generate BookingNo
                long PrefixUserMapId = 0;
                string bookingNo = string.Empty;
                long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == branchId).Select(a => a.PrefixId).FirstOrDefault();
                var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                      join b in oConnectionContext.DbClsPrefixUserMap
                                       on a.PrefixMasterId equals b.PrefixMasterId
                                      where a.IsActive == true && a.IsDeleted == false &&
                                      b.CompanyId == companyId && b.IsActive == true
                                      && b.IsDeleted == false && a.PrefixType.ToLower() == "table booking"
                                      && b.PrefixId == PrefixId
                                      select new
                                      {
                                          b.PrefixUserMapId,
                                          b.Prefix,
                                          b.NoOfDigits,
                                          b.Counter
                                      }).FirstOrDefault();

                if (prefixSettings != null)
                {
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    bookingNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }
                else
                {
                    bookingNo = "BK" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsTableBooking.Where(a => a.CompanyId == companyId).Count().ToString().PadLeft(4, '0');
                }

            // Calculate deposit if required (supports Fixed and PerGuest)
            decimal depositAmount = 0;
            bool hasPayment = false;
            if (settings.PublicBookingRequireDeposit)
            {
                var depositMode = settings.PublicBookingDepositMode ?? "Fixed";
                if (depositMode.Equals("PerGuest", StringComparison.OrdinalIgnoreCase))
                {
                    var perGuestAmount = settings.PublicBookingDepositPerGuestAmount;
                    var guestCount = obj.NoOfGuests ?? 0;
                    depositAmount = guestCount * perGuestAmount;
                }
                else
                {
                    depositAmount = settings.PublicBookingDepositFixedAmount;
                }

                // If deposit is required, payment transaction ID must be provided
                if (depositAmount > 0 && string.IsNullOrEmpty(obj.PaymentTransactionId))
                {
                    data = new
                    {
                        Status = 2,
                        Message = "Payment is required to complete this booking",
                        Errors = new[] { new ClsError { Message = "Payment is required to complete this booking", Id = "divPayment" } },
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // If payment transaction ID is provided, mark as having payment
                if (!string.IsNullOrEmpty(obj.PaymentTransactionId))
                {
                    hasPayment = true;
                }
            }

                // Determine status
                string status = settings.PublicBookingAutoConfirm ? "Confirmed" : "Pending";
                DateTime? confirmedOn = settings.PublicBookingAutoConfirm ? CurrentDate : (DateTime?)null;

                // Generate cancellation token
                var bookingToken = GenerateBookingToken(0, bookingNo, bookingDate); // Will update after save

                // Get IP address
                string ipAddress = GetClientIpAddress();

                oBooking = new ClsTableBooking()
                {
                    BookingNo = bookingNo,
                    CustomerId = customerId,
                    BookingDate = bookingDateOnly,
                    BookingTime = bookingTime,
                    Duration = duration,
                    NoOfGuests = obj.NoOfGuests.Value,
                    Status = status,
                    BookingType = "Online",
                    SpecialRequest = obj.SpecialRequest,
                    DepositAmount = depositAmount,
                    ReminderSent = false,
                    BranchId = branchId,
                    CompanyId = companyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = 0, // System user for public bookings
                    AddedOn = CurrentDate,
                    ModifiedBy = 0,
                    PublicBookingToken = bookingToken,
                    IpAddress = ipAddress,
                    ConfirmedOn = confirmedOn,
                    PaymentTransactionId = !string.IsNullOrEmpty(obj.PaymentTransactionId) ? obj.PaymentTransactionId : null,
                    PaymentGatewayType = !string.IsNullOrEmpty(obj.PaymentGatewayType) ? obj.PaymentGatewayType : null,
                    PaymentDate = hasPayment ? CurrentDate : (DateTime?)null
                };

                oConnectionContext.DbClsTableBooking.Add(oBooking);
                oConnectionContext.SaveChanges();

                // Create centralized customer payment record if deposit was paid
                if (hasPayment && depositAmount > 0 && !string.IsNullOrEmpty(obj.PaymentTransactionId))
                {
                    string referenceId = oCommonController.CreateToken();

                    // Get Deferred Income account for JournalAccountId (used for unused credits)
                    long journalAccountId = oConnectionContext.DbClsAccount
                        .Where(a => a.CompanyId == companyId && a.IsActive == true && a.IsDeleted == false && a.Type == "Deferred Income")
                        .Select(a => a.AccountId)
                        .FirstOrDefault();

                    ClsCustomerPayment oBookingPayment = new ClsCustomerPayment()
                    {
                        BookingId = oBooking.BookingId,
                        CustomerId = customerId,
                        PaymentDate = CurrentDate,
                        PaymentTypeId = 0, // Can be updated later when mapping public gateway types
                        Amount = depositAmount,
                        Notes = "Deposit payment via " + obj.PaymentGatewayType,
                        AccountId = 0, // Default account, can be configured
                        BranchId = branchId,
                        CompanyId = companyId,
                        ReferenceNo = oBooking.BookingNo + "-DEP",
                        Type = "Booking Deposit Payment",
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = 0, // System user for public bookings
                        AddedOn = CurrentDate,
                        ModifiedBy = 0,
                        ModifiedOn = CurrentDate,
                        AttachDocument = null,
                        IsDebit = 2,
                        ParentId = 0,
                        ReferenceId = referenceId,
                        JournalAccountId = journalAccountId,
                        AmountRemaining = depositAmount,  // Store as unused credit, similar to customer payments
                        IsDirectPayment = true,
                        AmountUsed = 0,
                        PaymentLinkId = 0,
                        PlaceOfSupplyId = 0,
                        TaxId = 0,
                        IsBusinessRegistered = 0,
                        GstTreatment = null,
                        BusinessRegistrationNameId = 0,
                        BusinessRegistrationNo = null,
                        BusinessLegalName = null,
                        BusinessTradeName = null,
                        PanNo = null,
                        TaxAccountId = 0,
                        AmountExcTax = depositAmount,
                        TaxAmount = 0,
                        IsCancelled = false,
                        PrefixId = 0,
                        IsAdvance = false
                    };

                    oConnectionContext.DbClsCustomerPayment.Add(oBookingPayment);
                    oConnectionContext.SaveChanges();

                    // Update customer's advance balance (unused credits)
                    string balanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + depositAmount + " where \"UserId\"=" + customerId;
                    oConnectionContext.Database.ExecuteSqlCommand(balanceQuery);
                }

                // Create junction table entries for all selected tables
                int displayOrder = 1;
                foreach (var tableId in tableIds)
                {
                    var bookingTable = new ClsTableBookingTable
                    {
                        BookingId = oBooking.BookingId,
                        TableId = tableId,
                        IsPrimary = (displayOrder == 1), // First table is primary
                        DisplayOrder = displayOrder,
                        AddedOn = CurrentDate
                    };
                    oConnectionContext.DbClsTableBookingTable.Add(bookingTable);
                    displayOrder++;
                }
                oConnectionContext.SaveChanges();

                // Update token with actual booking ID
                bookingToken = GenerateBookingToken(oBooking.BookingId, oBooking.BookingNo, bookingDateOnly);
                oBooking.PublicBookingToken = bookingToken;
                oConnectionContext.SaveChanges();

                // Update prefix counter if used
                if (PrefixUserMapId > 0)
                {
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                }

                dbContextTransaction.Complete();
            }

            // Get domain for email (outside transaction scope)
            string domain = GetDomainFromRequest();

            // Get customer info for emails (outside transaction scope)
            var customerInfo = customerId > 0 ? oConnectionContext.DbClsUser
                .Where(u => u.UserId == customerId)
                .Select(u => new { u.Name, u.MobileNo, u.EmailId })
                .FirstOrDefault() : null;

            // Send email notifications (async - don't wait)
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendBookingConfirmationEmail(oBooking, customerInfo?.Name ?? obj.CustomerName, customerInfo?.EmailId ?? obj.CustomerEmail, settings, domain);
                }
                catch
                {
                    // Log error but don't fail booking
                }
            });

            data = new
            {
                Status = 1,
                Message = "Booking created successfully",
                Data = new
                {
                    BookingId = oBooking.BookingId,
                    BookingNo = oBooking.BookingNo,
                    Status = oBooking.Status,
                    CancellationToken = oBooking.PublicBookingToken
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Get booking details by booking number
        /// </summary>
        [HttpGet]
        public async Task<IHttpActionResult> GetBookingDetails(string bookingNo)
        {
            if (string.IsNullOrEmpty(bookingNo))
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking number is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var booking = (from b in oConnectionContext.DbClsTableBooking
                          join c in oConnectionContext.DbClsUser on b.CustomerId equals c.UserId into customerJoin
                          from customer in customerJoin.DefaultIfEmpty()
                          where b.BookingNo == bookingNo && b.IsDeleted == false
                          select new
                          {
                              BookingId = b.BookingId,
                              BookingNo = b.BookingNo,
                              CustomerId = b.CustomerId,
                              CustomerName = customer != null ? customer.Name : null,
                              CustomerMobile = customer != null ? customer.MobileNo : null,
                              CustomerEmail = customer != null ? customer.EmailId : null,
                              BookingDate = b.BookingDate,
                              BookingTime = b.BookingTime,
                              Duration = b.Duration,
                              NoOfGuests = b.NoOfGuests,
                              Status = b.Status,
                              BookingType = b.BookingType,
                              SpecialRequest = b.SpecialRequest,
                              DepositAmount = b.DepositAmount,
                              DepositPaid = (oConnectionContext.DbClsCustomerPayment
                                  .Where(cp => cp.BookingId == b.BookingId &&
                                               cp.IsDeleted == false &&
                                               cp.IsCancelled == false &&
                                               cp.Type == "Booking Deposit Payment")
                                  .Select(cp => cp.Amount)
                                  .DefaultIfEmpty(0)
                                  .Sum() -
                                  oConnectionContext.DbClsCustomerPayment
                                  .Where(cp => cp.BookingId == b.BookingId &&
                                               cp.IsDeleted == false &&
                                               cp.IsCancelled == false &&
                                               cp.Type == "Booking Deposit Refund")
                                  .Select(cp => cp.Amount)
                                  .DefaultIfEmpty(0)
                                  .Sum()) >= b.DepositAmount,
                              CancellationCharge = b.CancellationCharge,
                              PaymentTransactionId = b.PaymentTransactionId,
                              PaymentGatewayType = b.PaymentGatewayType,
                              PaymentDate = b.PaymentDate
                          })
                          .FirstOrDefault();

            // Get all tables for this booking
            var bookingTables = new List<object>();
            if (booking != null)
            {
                var bookingTableMappings = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == booking.BookingId)
                    .OrderBy(bt => bt.DisplayOrder)
                    .Select(bt => new { bt.TableId, bt.DisplayOrder })
                    .ToList();

                var tableIds = bookingTableMappings.Select(bt => bt.TableId).ToList();
                var tableDict = oConnectionContext.DbClsRestaurantTable
                    .Where(t => tableIds.Contains(t.TableId))
                    .ToDictionary(t => t.TableId, t => new { t.TableId, t.TableNo, t.TableName });

                // Order by DisplayOrder from booking table mappings
                foreach (var mapping in bookingTableMappings)
                {
                    if (tableDict.ContainsKey(mapping.TableId))
                    {
                        var table = tableDict[mapping.TableId];
                        bookingTables.Add(new { TableId = table.TableId, TableNo = table.TableNo, TableName = table.TableName });
                    }
                }
            }

            if (booking == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Get primary table for backward compatibility
            var primaryTableId = booking != null ? oConnectionContext.DbClsTableBookingTable
                .Where(bt => bt.BookingId == booking.BookingId && bt.IsPrimary)
                .Select(bt => bt.TableId)
                .FirstOrDefault() : 0;
            
            if (primaryTableId == 0 && booking != null)
            {
                primaryTableId = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == booking.BookingId)
                    .OrderBy(bt => bt.DisplayOrder)
                    .Select(bt => bt.TableId)
                    .FirstOrDefault();
            }

            var primaryTable = primaryTableId > 0 ? oConnectionContext.DbClsRestaurantTable
                .Where(t => t.TableId == primaryTableId)
                .Select(t => new { t.TableNo, t.TableName })
                .FirstOrDefault() : null;

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Booking = booking != null ? new
                    {
                        booking.BookingId,
                        booking.BookingNo,
                        TableId = primaryTableId,
                        TableNo = primaryTable != null ? primaryTable.TableNo : null,
                        TableName = primaryTable != null ? primaryTable.TableName : null,
                        TableNos = bookingTables.Select(t => ((dynamic)t).TableNo).ToList(),
                        booking.CustomerName,
                        booking.CustomerMobile,
                        booking.CustomerEmail,
                        booking.BookingDate,
                        booking.BookingTime,
                        booking.Duration,
                        booking.NoOfGuests,
                        booking.Status,
                        booking.BookingType,
                        booking.SpecialRequest,
                        booking.DepositAmount,
                        booking.CancellationCharge,
                        DepositPaid = (oConnectionContext.DbClsCustomerPayment
                            .Where(cp => cp.BookingId == booking.BookingId &&
                                         cp.IsDeleted == false &&
                                         cp.IsCancelled == false &&
                                         cp.Type == "Booking Deposit Payment")
                            .Select(cp => cp.Amount)
                            .DefaultIfEmpty(0)
                            .Sum() -
                            oConnectionContext.DbClsCustomerPayment
                            .Where(cp => cp.BookingId == booking.BookingId &&
                                         cp.IsDeleted == false &&
                                         cp.IsCancelled == false &&
                                         cp.Type == "Booking Deposit Refund")
                            .Select(cp => cp.Amount)
                            .DefaultIfEmpty(0)
                            .Sum()) >= booking.DepositAmount,
                        booking.PaymentTransactionId,
                        booking.PaymentGatewayType,
                        booking.PaymentDate
                    } : null
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Cancel a public booking using token
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> CancelPublicBooking(CancelBookingRequest obj)
        {
            if (string.IsNullOrEmpty(obj.BookingNo) || string.IsNullOrEmpty(obj.Token))
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking number and token are required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var booking = oConnectionContext.DbClsTableBooking
                .Where(b => b.BookingNo == obj.BookingNo && 
                           b.PublicBookingToken == obj.Token && 
                           b.IsDeleted == false)
                .FirstOrDefault();

            if (booking == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Invalid booking number or token",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            if (booking.Status == "Cancelled")
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking is already cancelled",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var CurrentDate = oCommonController.CurrentDate(booking.CompanyId);

            // Check if public cancellation is enabled
            var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                .Where(s => s.CompanyId == booking.CompanyId && s.BranchId == booking.BranchId && s.IsDeleted == false)
                .Select(s => new { 
                    s.EnablePublicBookingCancellation, 
                    s.AllowCancelAfterConfirm,
                    s.PublicBookingCancellationDaysBefore,
                    s.PublicBookingCancellationChargeMode,
                    s.PublicBookingCancellationFixedCharge,
                    s.PublicBookingCancellationPercentage,
                    s.PublicBookingCancellationPerGuestCharge
                })
                .FirstOrDefault();

            if (restaurantSettings == null || !restaurantSettings.EnablePublicBookingCancellation)
            {
                data = new
                {
                    Status = 0,
                    Message = "Public booking cancellation is not enabled for this restaurant",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Check if booking is confirmed and cancellation after confirm is not allowed
            if (booking.Status == "Confirmed" && !restaurantSettings.AllowCancelAfterConfirm)
            {
                data = new
                {
                    Status = 0,
                    Message = "Cancellation is not allowed after booking confirmation. Please contact the restaurant directly.",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Check cancellation days before policy
            if (restaurantSettings.PublicBookingCancellationDaysBefore > 0)
            {
                var daysUntilBooking = (booking.BookingDate.Date - CurrentDate.Date).Days;
                if (daysUntilBooking < restaurantSettings.PublicBookingCancellationDaysBefore)
                {
                    data = new
                    {
                        Status = 0,
                        Message = $"Cancellation must be done at least {restaurantSettings.PublicBookingCancellationDaysBefore} day(s) before the booking date. Please contact the restaurant directly.",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
            }

            // Calculate cancellation charges if applicable
            decimal cancellationCharge = 0;
            if (!string.IsNullOrEmpty(restaurantSettings.PublicBookingCancellationChargeMode) &&
                restaurantSettings.PublicBookingCancellationChargeMode != "None")
            {
                if (restaurantSettings.PublicBookingCancellationChargeMode == "Fixed")
                {
                    cancellationCharge = restaurantSettings.PublicBookingCancellationFixedCharge;
                }
                else if (restaurantSettings.PublicBookingCancellationChargeMode == "Percentage" &&
                         booking.DepositAmount > 0)
                {
                    cancellationCharge = booking.DepositAmount * (restaurantSettings.PublicBookingCancellationPercentage / 100);
                }
                else if (restaurantSettings.PublicBookingCancellationChargeMode == "PerGuest")
                {
                    cancellationCharge = booking.NoOfGuests * restaurantSettings.PublicBookingCancellationPerGuestCharge;
                }
            }

            // 1) Mark booking as cancelled
            booking.Status = "Cancelled";
            booking.CancelledOn = CurrentDate;
            booking.CancellationReason = obj.Reason ?? "Cancelled by customer";
            booking.CancellationCharge = cancellationCharge;
            booking.ModifiedBy = 0;
            booking.ModifiedOn = CurrentDate;

            oConnectionContext.SaveChanges();

            // 2) Restore any credits that were applied to this booking's deposit
            //    (same pattern as BookingPaymentController.Delete for credit-based payments)
            var creditDepositPayments = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.BookingId == booking.BookingId &&
                             cp.Type == "Booking Deposit Payment" &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.ParentId > 0 &&
                             cp.IsDirectPayment == false)
                .Select(cp => new
                {
                    cp.CustomerPaymentId,
                    cp.ParentId,
                    cp.Amount,
                    cp.CustomerId
                })
                .ToList();

            foreach (var creditDep in creditDepositPayments)
            {
                // Restore parent credit payment's AmountRemaining and AmountUsed
                string restoreParentQuery = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + creditDep.Amount +
                    ",\"AmountUsed\"=\"AmountUsed\"-" + creditDep.Amount + " where \"CustomerPaymentId\"=" + creditDep.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(restoreParentQuery);

                // Restore customer's advance balance
                string restoreBalanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + creditDep.Amount + " where \"UserId\"=" + creditDep.CustomerId;
                oConnectionContext.Database.ExecuteSqlCommand(restoreBalanceQuery);

                // Soft-delete the credit-based booking deposit payment row
                var creditDepRow = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.CustomerPaymentId == creditDep.CustomerPaymentId)
                    .FirstOrDefault();
                if (creditDepRow != null)
                {
                    creditDepRow.IsDeleted = true;
                    creditDepRow.ModifiedBy = 0;
                    creditDepRow.ModifiedOn = CurrentDate;
                }
            }
            oConnectionContext.SaveChanges();

            // 3) Record cancellation charge as a separate customer payment row for reporting/journal purposes.
            //    Refunds are now handled manually; this row simply represents the fee.
            if (cancellationCharge > 0 && booking.CustomerId > 0)
            {
                string chargeReferenceId = oCommonController.CreateToken();
                string chargeReferenceNo = booking.BookingNo + "-CANC-" + CurrentDate.ToString("yyyyMMddHHmmss");

                // Reuse Deferred Income account (if configured) as journal account similar to deposit payments.
                long journalAccountId = oConnectionContext.DbClsAccount
                    .Where(a => a.CompanyId == booking.CompanyId &&
                                a.IsActive == true &&
                                a.IsDeleted == false &&
                                a.Type == "Deferred Income")
                    .Select(a => a.AccountId)
                    .FirstOrDefault();

                var cancellationPayment = new ClsCustomerPayment
                {
                    CustomerId = booking.CustomerId,
                    BookingId = booking.BookingId,
                    SalesId = 0,
                    SalesReturnId = 0,
                    PaymentDate = CurrentDate,
                    PaymentTypeId = 0,
                    Amount = cancellationCharge,
                    Notes = $"Booking cancellation charge for {booking.BookingNo}",
                    CompanyId = booking.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = 0,
                    AddedOn = CurrentDate,
                    ModifiedBy = 0,
                    ModifiedOn = CurrentDate,
                    AttachDocument = null,
                    Type = "Booking Cancellation Charge",
                    AccountId = 0,
                    BranchId = booking.BranchId,
                    ReferenceNo = chargeReferenceNo,
                    IsDebit = 0,
                    ParentId = 0,
                    ReferenceId = chargeReferenceId,
                    JournalAccountId = journalAccountId,
                    AmountRemaining = 0,
                    IsDirectPayment = true,
                    AmountUsed = 0,
                    PaymentLinkId = 0,
                    PlaceOfSupplyId = 0,
                    TaxId = 0,
                    IsBusinessRegistered = 0,
                    GstTreatment = null,
                    BusinessRegistrationNameId = 0,
                    BusinessRegistrationNo = null,
                    BusinessLegalName = null,
                    BusinessTradeName = null,
                    PanNo = null,
                    TaxAccountId = 0,
                    AmountExcTax = cancellationCharge,
                    TaxAmount = 0,
                    IsCancelled = false,
                    PrefixId = 0,
                    IsAdvance = false
                };

                oConnectionContext.DbClsCustomerPayment.Add(cancellationPayment);
                oConnectionContext.SaveChanges();
            }

            // Send cancellation email
            var settings = oConnectionContext.DbClsRestaurantSettings
                .Where(s => s.CompanyId == booking.CompanyId && s.BranchId == booking.BranchId)
                .FirstOrDefault();

            // Get domain for email
            string domain = GetDomainFromRequest();

            // Get customer info for email
            var customerInfo = booking.CustomerId > 0 ? oConnectionContext.DbClsUser
                .Where(u => u.UserId == booking.CustomerId)
                .Select(u => new { u.Name, u.EmailId })
                .FirstOrDefault() : null;

            _ = Task.Run(async () =>
            {
                try
                {
                    await SendBookingCancellationEmail(booking, customerInfo?.Name ?? "", customerInfo?.EmailId ?? "", settings, domain);
                }
                catch
                {
                    // Log error but don't fail cancellation
                }
            });

            data = new
            {
                Status = 1,
                Message = cancellationCharge > 0 ? 
                    $"Booking cancelled successfully. Cancellation charge: {cancellationCharge:F2}" : 
                    "Booking cancelled successfully",
                Data = new
                {
                    BookingNo = booking.BookingNo,
                    Status = booking.Status,
                    CancellationCharge = cancellationCharge,
                    HasCancellationCharge = cancellationCharge > 0
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Get domain from request
        /// </summary>
        private string GetDomainFromRequest()
        {
            try
            {
                if (Request?.Properties?.ContainsKey("MS_HttpContext") == true)
                {
                    var httpContext = Request.Properties["MS_HttpContext"] as System.Web.HttpContextWrapper;
                    if (httpContext != null && httpContext.Request != null && httpContext.Request.Url != null)
                    {
                        return httpContext.Request.Url.Host;
                    }
                }
                return "equibillbook.com"; // Default domain
            }
            catch
            {
                return "equibillbook.com";
            }
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                if (Request?.Properties?.ContainsKey("MS_HttpContext") == true)
                {
                    var httpContext = Request.Properties["MS_HttpContext"] as System.Web.HttpContextWrapper;
                    if (httpContext != null && httpContext.Request != null)
                    {
                        return httpContext.Request.UserHostAddress;
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Send booking confirmation email to customer
        /// </summary>
        private async Task SendBookingConfirmationEmail(ClsTableBooking booking, string customerName, string customerEmail, ClsRestaurantSettings settings, string domain = "")
        {
            try
            {
                var businessSettings = oConnectionContext.DbClsBusinessSettings
                    .Where(b => b.CompanyId == booking.CompanyId)
                    .Select(b => new
                    {
                        b.BusinessName,
                        b.ParentBusinessName,
                        b.BusinessLogo
                    })
                    .FirstOrDefault();

                if (businessSettings == null) return;

                var branch = oConnectionContext.DbClsBranch
                    .Where(b => b.BranchId == booking.BranchId)
                    .Select(b => new { b.Branch, b.Email, b.Mobile, b.Address })
                    .FirstOrDefault();

                // Get all tables for this booking (ordered by DisplayOrder)
                var bookingTableMappings = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == booking.BookingId)
                    .OrderBy(bt => bt.DisplayOrder)
                    .Select(bt => new { bt.TableId, bt.DisplayOrder })
                    .ToList();

                var tableIds = bookingTableMappings.Select(bt => bt.TableId).ToList();
                var tables = oConnectionContext.DbClsRestaurantTable
                    .Where(t => tableIds.Contains(t.TableId))
                    .Select(t => new { t.TableId, t.TableNo, t.TableName })
                    .ToList();

                // Create a dictionary for quick lookup
                var tableDict = tables.ToDictionary(t => t.TableId, t => t);

                // Order tables by DisplayOrder from booking table mappings
                var orderedTables = bookingTableMappings
                    .Where(m => tableDict.ContainsKey(m.TableId))
                    .Select(m => tableDict[m.TableId])
                    .ToList();

                // Format tables for email (comma-separated or single table)
                string tableNo = "";
                string tableName = "";
                if (orderedTables.Count > 0)
                {
                    if (orderedTables.Count == 1)
                    {
                        tableNo = orderedTables[0].TableNo ?? "";
                        tableName = orderedTables[0].TableName ?? "";
                    }
                    else
                    {
                        // Multiple tables: format as "Table 1, Table 2, Table 3"
                        tableNo = string.Join(", ", orderedTables.Select(t => t.TableNo ?? "").Where(t => !string.IsNullOrEmpty(t)));
                        tableName = string.Join(", ", orderedTables.Select(t => !string.IsNullOrEmpty(t.TableName) ? t.TableName : "").Where(t => !string.IsNullOrEmpty(t)));
                    }
                }

                var emailController = new EmailController();
                var subject = $"Table Booking Confirmation - {booking.BookingNo}";

                await Task.Run(() =>
                {
                    emailController.PublicBookingConfirmation(
                        customerEmail ?? "",
                        subject,
                        booking.BookingNo,
                        customerName ?? "",
                        booking.BookingDate,
                        booking.BookingTime,
                        booking.Duration,
                        booking.NoOfGuests,
                        tableNo,
                        tableName,
                        booking.SpecialRequest ?? "",
                        booking.PublicBookingToken ?? "",
                        businessSettings.BusinessName,
                        businessSettings.ParentBusinessName,
                        businessSettings.BusinessLogo ?? "",
                        branch?.Branch ?? "",
                        branch?.Address ?? "",
                        branch?.Mobile ?? "",
                        branch?.Email ?? "",
                        domain
                    );
                });
            }
            catch
            {
                // Log error
            }
        }

        /// <summary>
        /// Send cancellation email
        /// </summary>
        private async Task SendBookingCancellationEmail(ClsTableBooking booking, string customerName, string customerEmail, ClsRestaurantSettings settings, string domain = "")
        {
            try
            {
                var businessSettings = oConnectionContext.DbClsBusinessSettings
                    .Where(b => b.CompanyId == booking.CompanyId)
                    .Select(b => new { b.BusinessName, b.ParentBusinessName, b.BusinessLogo })
                    .FirstOrDefault();

                if (businessSettings == null) return;

                var emailController = new EmailController();
                var subject = $"Table Booking Cancelled - {booking.BookingNo}";

                await Task.Run(() =>
                {
                    emailController.PublicBookingCancellation(
                        customerEmail ?? "",
                        subject,
                        booking.BookingNo,
                        customerName ?? "",
                        booking.BookingDate,
                        booking.BookingTime,
                        businessSettings.BusinessName,
                        businessSettings.ParentBusinessName,
                        businessSettings.BusinessLogo ?? "",
                        domain
                    );
                });
            }
            catch
            {
                // Log error
            }
        }
    }

    /// <summary>
    /// Request model for public booking
    /// </summary>
    public class PublicBookingRequest
    {
        public string Slug { get; set; }
        public DateTime? BookingDate { get; set; }
        public string BookingTimeString { get; set; } // Accept as string, convert to TimeSpan in controller (same as CreateStandalone)
        public int? NoOfGuests { get; set; }
        public List<long> TableIds { get; set; } = new List<long>(); // Support multiple tables
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public string SpecialRequest { get; set; }
        public int Duration { get; set; }
        public string PaymentTransactionId { get; set; } // Payment gateway transaction ID (Razorpay payment_id or PayPal transaction_id)
        public string PaymentGatewayType { get; set; } // "Razorpay" or "PayPal"
    }

    /// <summary>
    /// Request model for cancelling booking
    /// </summary>
    public class CancelBookingRequest
    {
        public string BookingNo { get; set; }
        public string Token { get; set; }
        public string Reason { get; set; }
    }
}

