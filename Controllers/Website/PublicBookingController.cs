using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Data.Entity;

namespace EquiBillBook.Controllers.Website
{
    public class PublicBookingController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        /// <summary>
        /// Gets time slots from normalized table or generates them based on mode
        /// Also checks operating days and date overrides
        /// </summary>
        private async Task<List<string>> GetTimeSlotsAsync(long restaurantSettingsId, string bookingTimeSlotMode, TimeSpan? bookingStartTime, TimeSpan? bookingEndTime, int defaultBookingDuration, DateTime bookingDate)
        {
            var timeSlots = new List<string>();
            ConnectionContext oConnectionContext = new ConnectionContext();
            var bookingDateValue = bookingDate.Date;
            
            // Step 1: Check for date override first
            // Use DbFunctions.TruncateTime to ensure proper date comparison (ignoring time component)
            var dateOverride = await oConnectionContext.DbClsRestaurantBookingDateOverride
                .Where(d => d.RestaurantSettingsId == restaurantSettingsId &&
                           System.Data.Entity.DbFunctions.TruncateTime(d.OverrideDate) == bookingDateValue &&
                           d.IsActive &&
                           !d.IsDeleted)
                .FirstOrDefaultAsync();

            List<TimeSpan> timeSlotSpans = new List<TimeSpan>();

            // If date has override
            if (dateOverride != null)
            {
                // If date is closed, return empty slots
                if (dateOverride.IsClosed)
                {
                    timeSlotSpans = new List<TimeSpan>(); // Empty - restaurant is closed
                }
                else
                {
                    // Date is open but has override time slots - use those
                    timeSlotSpans = await oConnectionContext.DbClsRestaurantBookingDateOverrideSlot
                        .Where(s => s.BookingDateOverrideId == dateOverride.BookingDateOverrideId &&
                                   s.IsActive &&
                                   !s.IsDeleted)
                        .OrderBy(s => s.TimeSlot)
                        .Select(s => s.TimeSlot)
                        .ToListAsync();
                }
            }
            else
            {
                // No date override - check operating days
                var dayOfWeek = (int)bookingDateValue.DayOfWeek; // 0=Sunday, 1=Monday, etc.
                var isOperatingDay = await oConnectionContext.DbClsRestaurantOperatingDay
                    .AnyAsync(od => od.RestaurantSettingsId == restaurantSettingsId &&
                              od.DayOfWeek == dayOfWeek &&
                              od.IsActive &&
                              !od.IsDeleted);

                // If not an operating day, return empty slots
                if (!isOperatingDay)
                {
                    timeSlotSpans = new List<TimeSpan>(); // Empty - restaurant is closed on this day
                }
                else
                {
                    // It's an operating day - get regular time slots
                    timeSlotSpans = await GetRegularTimeSlotsAsync(oConnectionContext, restaurantSettingsId, bookingTimeSlotMode, bookingStartTime, bookingEndTime, defaultBookingDuration);
                }
            }
            
            // Convert TimeSpan list to string list
            foreach (var slot in timeSlotSpans)
            {
                timeSlots.Add(slot.ToString(@"hh\:mm"));
            }
            
            return timeSlots;
        }

        /// <summary>
        /// Gets regular time slots (without date/operating day checks)
        /// </summary>
        private async Task<List<TimeSpan>> GetRegularTimeSlotsAsync(ConnectionContext oConnectionContext, long restaurantSettingsId, string bookingTimeSlotMode, TimeSpan? bookingStartTime, TimeSpan? bookingEndTime, int defaultBookingDuration)
        {
            var timeSlots = new List<TimeSpan>();
            
            // Check if using Manual mode (normalized time slots) or Auto mode (generate from start/end time)
            // Use case-insensitive comparison and trim whitespace
            var bookingMode = !string.IsNullOrWhiteSpace(bookingTimeSlotMode) ? bookingTimeSlotMode.Trim() : "";
            
            if (string.Equals(bookingMode, "Manual", StringComparison.OrdinalIgnoreCase))
            {
                // Get time slots from normalized table - ONLY these slots, no fallback
                timeSlots = await oConnectionContext.DbClsRestaurantBookingTimeSlot
                    .Where(ts => ts.RestaurantSettingsId == restaurantSettingsId && 
                                ts.IsActive && 
                                !ts.IsDeleted)
                    .OrderBy(ts => ts.DisplayOrder)
                    .ThenBy(ts => ts.TimeSlot)
                    .Select(ts => ts.TimeSlot)
                    .ToListAsync();
                // In Manual mode, return only configured slots (even if empty) - NO FALLBACK TO AUTO
            }
            else if (string.Equals(bookingMode, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                // Auto mode - generate slots based on start/end time and duration
                // Only generate if start and end times are configured
                if (bookingStartTime.HasValue && bookingEndTime.HasValue && defaultBookingDuration > 0)
                {
                    var startTime = bookingStartTime.Value;
                    var endTime = bookingEndTime.Value;
                    var intervalMinutes = defaultBookingDuration;
                    
                    for (var time = startTime; time <= endTime; time = time.Add(new TimeSpan(0, intervalMinutes, 0)))
                    {
                        timeSlots.Add(time);
                    }
                }
                // If Auto mode but no start/end times or duration configured, return empty slots (no fallback)
            }
            // If mode is null, empty, or invalid - return empty slots (no fallback, no auto-generation)
            
            return timeSlots;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CreatePublicBooking(PublicBookingRequest request)
        {
            // Proxy the public booking request to the Web API controller
            serializer.MaxJsonLength = int.MaxValue;

            PublicTableBookingController publicTableBookingController = new PublicTableBookingController();
            var result = await publicTableBookingController.CreatePublicBooking(request);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Main booking page - /book/{slug}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> BookTable(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("BookingNotFound");
            }

            serializer.MaxJsonLength = 2147483644;

            // Resolve slug to get restaurant settings
            ConnectionContext oConnectionContext = new ConnectionContext();
            var settings = await oConnectionContext.DbClsRestaurantSettings
                .Where(s => s.PublicBookingSlug != null &&
                           s.PublicBookingSlug.ToLower() == slug.ToLower() && 
                           s.EnablePublicBooking == true && 
                           s.IsActive == true && 
                           s.IsDeleted == false)
                .Select(s => new
                {
                    s.CompanyId,
                    s.BranchId,
                    s.RestaurantSettingsId,
                    s.PublicBookingAdvanceDays,
                    s.BookingTimeSlotMode,
                    s.BookingStartTime,
                    s.BookingEndTime,
                    s.DefaultBookingDuration,
                    s.PublicBookingRequireDeposit,
                    s.PublicBookingDepositPercentage,
                    s.PublicBookingDepositMode,
                    s.PublicBookingDepositFixedAmount,
                    s.PublicBookingDepositPerGuestAmount,
                    s.PublicBookingAutoConfirm
                })
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                return RedirectToAction("BookingNotFound");
            }

            // Get business settings for display
            var businessSettings = await oConnectionContext.DbClsBusinessSettings
                .Where(b => b.CompanyId == settings.CompanyId)
                .Select(b => new
                {
                    b.BusinessName,
                    b.ParentBusinessName,
                    b.BusinessLogo
                })
                .FirstOrDefaultAsync();

            var branch = await oConnectionContext.DbClsBranch
                .Where(b => b.BranchId == settings.BranchId)
                .Select(b => new
                {
                    b.Branch,
                    b.Address,
                    b.Mobile,
                    b.Email
                })
                .FirstOrDefaultAsync();

            ViewBag.Slug = slug;
            ViewBag.CompanyId = settings.CompanyId;
            ViewBag.BranchId = settings.BranchId;
            ViewBag.BusinessName = businessSettings?.BusinessName ?? "";
            ViewBag.ParentBusinessName = businessSettings?.ParentBusinessName ?? "";
            ViewBag.BusinessLogo = businessSettings?.BusinessLogo ?? "";
            ViewBag.BranchName = branch?.Branch ?? "";
            ViewBag.BranchAddress = branch?.Address ?? "";
            ViewBag.BranchMobile = branch?.Mobile ?? "";
            ViewBag.BranchEmail = branch?.Email ?? "";
            ViewBag.AdvanceDays = settings.PublicBookingAdvanceDays > 0 ? settings.PublicBookingAdvanceDays : 30;
            ViewBag.DefaultDuration = settings.DefaultBookingDuration > 0 ? settings.DefaultBookingDuration : 120;
            ViewBag.RequireDeposit = settings.PublicBookingRequireDeposit;
            ViewBag.DepositMode = settings.PublicBookingDepositMode ?? "Fixed";
            ViewBag.DepositFixedAmount = settings.PublicBookingDepositFixedAmount;
            ViewBag.DepositPerGuestAmount = settings.PublicBookingDepositPerGuestAmount;
            ViewBag.AutoConfirm = settings.PublicBookingAutoConfirm;

            // Get time slots from normalized table or generate based on mode
            // Use today's date for initial page load (will be filtered by date selection on frontend)
            ViewBag.TimeSlots = await GetTimeSlotsAsync(
                settings.RestaurantSettingsId,
                settings.BookingTimeSlotMode,
                settings.BookingStartTime,
                settings.BookingEndTime,
                settings.DefaultBookingDuration,
                DateTime.Today
            );

            // SEO Meta Tags
            ViewBag.PageTitle = $"Book a Table - {businessSettings?.BusinessName ?? ""}";
            ViewBag.MetaDescription = $"Reserve your table at {businessSettings?.BusinessName ?? ""}. Book online now!";
            ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/book/" + slug;

            return View();
        }

        /// <summary>
        /// Direct table booking page - /booktable/{slug}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> BookTableBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("BookingNotFound");
            }

            serializer.MaxJsonLength = 2147483644;

            // Resolve slug to get table and restaurant settings
            ConnectionContext oConnectionContext = new ConnectionContext();
            var table = await oConnectionContext.DbClsRestaurantTable
                .Where(t => t.TableSlug != null &&
                           t.TableSlug.ToLower() == slug.ToLower() &&
                           t.IsActive == true &&
                           t.IsDeleted == false)
                .Select(t => new
                {
                    t.TableId,
                    t.TableNo,
                    t.TableName,
                    t.CompanyId,
                    t.BranchId,
                    t.FloorId,
                    t.Capacity
                })
                .FirstOrDefaultAsync();

            if (table == null)
            {
                return RedirectToAction("BookingNotFound");
            }

            // Get restaurant settings for this branch
            var settings = await oConnectionContext.DbClsRestaurantSettings
                .Where(s => s.CompanyId == table.CompanyId &&
                           s.BranchId == table.BranchId &&
                           s.EnablePublicBooking == true &&
                           s.IsActive == true &&
                           s.IsDeleted == false)
                .Select(s => new
                {
                    s.RestaurantSettingsId,
                    s.PublicBookingAdvanceDays,
                    s.BookingTimeSlotMode,
                    s.BookingStartTime,
                    s.BookingEndTime,
                    s.DefaultBookingDuration,
                    s.PublicBookingRequireDeposit,
                    s.PublicBookingDepositPercentage,
                    s.PublicBookingDepositMode,
                    s.PublicBookingDepositFixedAmount,
                    s.PublicBookingDepositPerGuestAmount,
                    s.PublicBookingAutoConfirm
                })
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                return RedirectToAction("BookingNotFound");
            }

            // Get business settings for display
            var businessSettings = await oConnectionContext.DbClsBusinessSettings
                .Where(b => b.CompanyId == table.CompanyId)
                .Select(b => new
                {
                    b.BusinessName,
                    b.ParentBusinessName,
                    b.BusinessLogo
                })
                .FirstOrDefaultAsync();

            var branch = await oConnectionContext.DbClsBranch
                .Where(b => b.BranchId == table.BranchId)
                .Select(b => new
                {
                    b.Branch,
                    b.Address,
                    b.Mobile,
                    b.Email
                })
                .FirstOrDefaultAsync();

            // Get floor name for display (optional)
            string floorName = "";
            if (table.FloorId > 0)
            {
                floorName = await oConnectionContext.DbClsRestaurantFloor
                    .Where(f => f.FloorId == table.FloorId)
                    .Select(f => f.FloorName)
                    .FirstOrDefaultAsync() ?? "";
            }

            ViewBag.Slug = slug;
            ViewBag.TableId = table.TableId;
            ViewBag.TableNo = table.TableNo;
            ViewBag.TableName = table.TableName;
            ViewBag.CompanyId = table.CompanyId;
            ViewBag.BranchId = table.BranchId;
            ViewBag.FloorId = table.FloorId;
            ViewBag.TableCapacity = table.Capacity;
            ViewBag.FloorName = floorName ?? "";
            ViewBag.BusinessName = businessSettings?.BusinessName ?? "";
            ViewBag.ParentBusinessName = businessSettings?.ParentBusinessName ?? "";
            ViewBag.BusinessLogo = businessSettings?.BusinessLogo ?? "";
            ViewBag.BranchName = branch?.Branch ?? "";
            ViewBag.BranchAddress = branch?.Address ?? "";
            ViewBag.BranchMobile = branch?.Mobile ?? "";
            ViewBag.BranchEmail = branch?.Email ?? "";
            ViewBag.AdvanceDays = settings.PublicBookingAdvanceDays > 0 ? settings.PublicBookingAdvanceDays : 30;
            ViewBag.DefaultDuration = settings.DefaultBookingDuration > 0 ? settings.DefaultBookingDuration : 120;
            ViewBag.RequireDeposit = settings.PublicBookingRequireDeposit;
            ViewBag.DepositMode = settings.PublicBookingDepositMode ?? "Fixed";
            ViewBag.DepositFixedAmount = settings.PublicBookingDepositFixedAmount;
            ViewBag.DepositPerGuestAmount = settings.PublicBookingDepositPerGuestAmount;
            ViewBag.AutoConfirm = settings.PublicBookingAutoConfirm;

            // Get time slots from normalized table or generate based on mode
            // Use today's date for initial page load (will be filtered by date selection on frontend)
            ViewBag.TimeSlots = await GetTimeSlotsAsync(
                settings.RestaurantSettingsId,
                settings.BookingTimeSlotMode,
                settings.BookingStartTime,
                settings.BookingEndTime,
                settings.DefaultBookingDuration,
                DateTime.Today
            );

            // SEO Meta Tags
            ViewBag.PageTitle = $"Book Table {table.TableNo} - {businessSettings?.BusinessName ?? ""}";
            ViewBag.MetaDescription = $"Reserve table {table.TableNo} at {businessSettings?.BusinessName ?? ""}. Book online now!";
            ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/booktable/" + slug;

            return View("BookTable");
        }

        /// <summary>
        /// Booking confirmation page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> BookingConfirmation(string bookingNo)
        {
            if (string.IsNullOrEmpty(bookingNo))
            {
                return RedirectToAction("BookingNotFound");
            }

            serializer.MaxJsonLength = 2147483644;

            // Get booking details via API
            PublicTableBookingController publicTableBookingController = new PublicTableBookingController();
            var result = await publicTableBookingController.GetBookingDetails(bookingNo);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            if (oClsResponse.Status == 0 || oClsResponse.Data == null)
            {
                return RedirectToAction("BookingNotFound");
            }

            dynamic booking = oClsResponse.Data.Booking;

            // Get booking entity directly from database to get CompanyId, BranchId, and token
            ConnectionContext oConnectionContext = new ConnectionContext();
            var bookingEntity = await oConnectionContext.DbClsTableBooking
                .Where(b => b.BookingNo == bookingNo && b.IsDeleted == false)
                .Select(b => new { 
                    b.PublicBookingToken, 
                    b.BranchId, 
                    b.CompanyId 
                })
                .FirstOrDefaultAsync();

            // Extract CompanyId and BranchId from booking entity
            long companyId = 0;
            long branchId = 0;
            if (bookingEntity != null)
            {
                companyId = bookingEntity.CompanyId;
                branchId = bookingEntity.BranchId;
            }

            // Get business settings
            var businessSettings = await oConnectionContext.DbClsBusinessSettings
                .Where(b => b.CompanyId == companyId)
                .Select(b => new
                {
                    b.BusinessName,
                    b.ParentBusinessName,
                    b.BusinessLogo
                })
                .FirstOrDefaultAsync();

            // Check if public cancellation is enabled and if cancellation after confirm is allowed
            bool enableCancellation = false;
            bool allowCancelAfterConfirm = false;
            bool cancelledDueToDaysBefore = false;
            int? cancellationDaysBefore = null;
            string bookingStatus = booking?.Status?.ToString() ?? "";
            
            // Query settings using companyId and branchId
            if (companyId > 0 && branchId > 0)
            {
                var settings = await oConnectionContext.DbClsRestaurantSettings
                    .Where(s => s.CompanyId == companyId && s.BranchId == branchId && s.IsDeleted == false)
                    .Select(s => new { 
                        s.EnablePublicBookingCancellation, 
                        s.AllowCancelAfterConfirm,
                        s.PublicBookingCancellationDaysBefore
                    })
                    .FirstOrDefaultAsync();
                    
                if (settings != null)
                {
                    enableCancellation = settings.EnablePublicBookingCancellation;
                    allowCancelAfterConfirm = settings.AllowCancelAfterConfirm;
                    cancellationDaysBefore = settings.PublicBookingCancellationDaysBefore;
                    
                    // If booking is confirmed and cancellation after confirm is not allowed, disable cancellation
                    if (enableCancellation && bookingStatus == "Confirmed" && !allowCancelAfterConfirm)
                    {
                        enableCancellation = false;
                    }
                    
                    // Check cancellation days before policy
                    if (enableCancellation && settings.PublicBookingCancellationDaysBefore > 0)
                    {
                        try
                        {
                            var bookingDate = (DateTime)booking.BookingDate;
                            var currentDate = oCommonController.CurrentDate(companyId);
                            var daysUntilBooking = (bookingDate.Date - currentDate.Date).Days;
                            
                            if (daysUntilBooking < settings.PublicBookingCancellationDaysBefore)
                            {
                                enableCancellation = false;
                                cancelledDueToDaysBefore = true;
                            }
                        }
                        catch
                        {
                            // If date parsing fails, allow API to handle validation
                        }
                    }
                }
            }

            ViewBag.Booking = booking;
            ViewBag.BookingNo = bookingNo;
            ViewBag.CancellationToken = (enableCancellation && bookingEntity != null) ? bookingEntity.PublicBookingToken : null;
            ViewBag.EnableCancellation = enableCancellation;
            ViewBag.BookingStatus = bookingStatus;
            ViewBag.AllowCancelAfterConfirm = allowCancelAfterConfirm;
            ViewBag.CancelledDueToDaysBefore = cancelledDueToDaysBefore;
            ViewBag.CancellationDaysBefore = cancellationDaysBefore;
            ViewBag.BusinessName = businessSettings?.BusinessName ?? "";
            ViewBag.ParentBusinessName = businessSettings?.ParentBusinessName ?? "";
            ViewBag.BusinessLogo = businessSettings?.BusinessLogo ?? "";

            ViewBag.PageTitle = $"Booking Confirmation - {booking.BookingNo}";
            ViewBag.MetaDescription = $"Your table booking confirmation for {booking.BookingNo}";

            return View();
        }

        /// <summary>
        /// Booking cancellation page
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> BookingCancellation(string bookingNo, string token)
        {
            if (string.IsNullOrEmpty(bookingNo) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("BookingNotFound");
            }

            serializer.MaxJsonLength = 2147483644;

            // Get booking details via API
            PublicTableBookingController publicTableBookingController = new PublicTableBookingController();
            var result = await publicTableBookingController.GetBookingDetails(bookingNo);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            if (oClsResponse.Status == 0 || oClsResponse.Data == null)
            {
                return RedirectToAction("BookingNotFound");
            }

            dynamic booking = oClsResponse.Data.Booking;

            // Get booking entity directly from database to get CompanyId, BranchId, and verify token
            ConnectionContext oConnectionContext = new ConnectionContext();
            var bookingEntity = await oConnectionContext.DbClsTableBooking
                .Where(b => b.BookingNo == bookingNo && b.IsDeleted == false)
                .Select(b => new { 
                    b.PublicBookingToken, 
                    b.BranchId, 
                    b.CompanyId,
                    b.Status
                })
                .FirstOrDefaultAsync();

            // Extract CompanyId and BranchId from booking entity
            long companyId = 0;
            long branchId = 0;
            if (bookingEntity != null)
            {
                companyId = bookingEntity.CompanyId;
                branchId = bookingEntity.BranchId;
            }

            // Verify token matches (basic check - full verification done in API)
            if (bookingEntity == null || bookingEntity.PublicBookingToken != token)
            {
                return RedirectToAction("BookingNotFound");
            }

            // Check if public cancellation is enabled and if cancellation after confirm is allowed
            bool enableCancellation = false;
            bool allowCancelAfterConfirm = false;
            string bookingStatus = booking?.Status?.ToString() ?? "";
            
            if (companyId > 0 && branchId > 0)
            {
                var settings = await oConnectionContext.DbClsRestaurantSettings
                    .Where(s => s.CompanyId == companyId && s.BranchId == branchId && s.IsDeleted == false)
                    .Select(s => new { 
                        s.EnablePublicBookingCancellation, 
                        s.AllowCancelAfterConfirm,
                        s.PublicBookingCancellationDaysBefore
                    })
                    .FirstOrDefaultAsync();
                    
                if (settings != null)
                {
                    enableCancellation = settings.EnablePublicBookingCancellation;
                    allowCancelAfterConfirm = settings.AllowCancelAfterConfirm;
                    
                    // If booking is confirmed and cancellation after confirm is not allowed, disable cancellation
                    if (enableCancellation && bookingStatus == "Confirmed" && !allowCancelAfterConfirm)
                    {
                        enableCancellation = false;
                    }
                    
                    // Check cancellation days before policy
                    if (enableCancellation && settings.PublicBookingCancellationDaysBefore > 0)
                    {
                        try
                        {
                            var bookingDate = (DateTime)booking.BookingDate;
                            var currentDate = oCommonController.CurrentDate(companyId);
                            var daysUntilBooking = (bookingDate.Date - currentDate.Date).Days;
                            
                            if (daysUntilBooking < settings.PublicBookingCancellationDaysBefore)
                            {
                                enableCancellation = false;
                            }
                        }
                        catch
                        {
                            // If date parsing fails, allow API to handle validation
                        }
                    }
                }
            }

            if (!enableCancellation)
            {
                return RedirectToAction("BookingNotFound");
            }

            // Verify token (basic check - full verification done in API)
            ViewBag.BookingNo = bookingNo;
            ViewBag.Token = token;
            ViewBag.Booking = booking;

            ViewBag.PageTitle = $"Cancel Booking - {bookingNo}";
            ViewBag.MetaDescription = $"Cancel your table booking {bookingNo}";

            return View();
        }

        /// <summary>
        /// Booking not found page
        /// </summary>
        public Task<ActionResult> BookingNotFound()
        {
            ViewBag.PageTitle = "Booking Not Found";
            return Task.FromResult<ActionResult>(View());
        }
    }
}

