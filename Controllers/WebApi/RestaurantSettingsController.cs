using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using QRCoder;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class RestaurantSettingsController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        [HttpPost]
        public async Task<IHttpActionResult> GetRestaurantSettings(ClsRestaurantSettingsVm obj)
        {
            ClsRestaurantSettings det = null;
            if (obj.RestaurantSettingsId > 0)
            {
                det = oConnectionContext.DbClsRestaurantSettings.Where(a => a.CompanyId == obj.CompanyId && a.RestaurantSettingsId == obj.RestaurantSettingsId && a.IsDeleted == false).FirstOrDefault();
            }
            else
            {
                det = oConnectionContext.DbClsRestaurantSettings.Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false).FirstOrDefault();
            }

            if (det == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Restaurant Settings not found",
                    Data = new
                    {
                        RestaurantSettings = (object)null
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            // Load normalized booking time slots (for list display)
            var timeSlotsList = oConnectionContext.DbClsRestaurantBookingTimeSlot
                .Where(ts => ts.RestaurantSettingsId == det.RestaurantSettingsId && 
                            ts.IsActive && 
                            !ts.IsDeleted)
                .OrderBy(ts => ts.DisplayOrder)
                .ThenBy(ts => ts.TimeSlot)
                .Select(ts => ts.TimeSlot)
                .ToList()
                .Select(ts => ts.ToString(@"hh\:mm"))
                .ToList();

            // Load normalized booking time slots (full objects for edit view)
            // Return only active time slots (since we removed the Active toggle - all manual slots are active)
            // First materialize the query, then convert TimeSpan to string format (LINQ to Objects)
            var bookingTimeSlotsNormalized = oConnectionContext.DbClsRestaurantBookingTimeSlot
                .Where(ts => ts.RestaurantSettingsId == det.RestaurantSettingsId && 
                            ts.IsActive && 
                            !ts.IsDeleted)
                .OrderBy(ts => ts.DisplayOrder)
                .ThenBy(ts => ts.TimeSlot)
                .Select(ts => new
                {
                    BookingTimeSlotId = ts.BookingTimeSlotId,
                    RestaurantSettingsId = ts.RestaurantSettingsId,
                    TimeSlot = ts.TimeSlot,
                    DisplayOrder = ts.DisplayOrder,
                    IsActive = ts.IsActive,
                    IsDeleted = ts.IsDeleted,
                    AddedBy = ts.AddedBy,
                    AddedOn = ts.AddedOn,
                    ModifiedBy = ts.ModifiedBy,
                    ModifiedOn = ts.ModifiedOn,
                    CompanyId = ts.CompanyId
                })
                .ToList()
                .Select(ts => new
                {
                    BookingTimeSlotId = ts.BookingTimeSlotId,
                    RestaurantSettingsId = ts.RestaurantSettingsId,
                    TimeSlot = ts.TimeSlot.ToString(@"hh\:mm"),
                    TimeSlotString = ts.TimeSlot.ToString(@"hh\:mm"),
                    DisplayOrder = ts.DisplayOrder,
                    IsActive = ts.IsActive,
                    IsDeleted = ts.IsDeleted,
                    AddedBy = ts.AddedBy,
                    AddedOn = ts.AddedOn,
                    ModifiedBy = ts.ModifiedBy,
                    ModifiedOn = ts.ModifiedOn,
                    CompanyId = ts.CompanyId
                })
                .ToList();

            // Load Operating Days from database (only active and not deleted)
            var operatingDaysList = oConnectionContext.DbClsRestaurantOperatingDay
                .Where(od => od.RestaurantSettingsId == det.RestaurantSettingsId && 
                            od.IsActive && 
                            !od.IsDeleted)
                .OrderBy(od => od.DisplayOrder)
                .ThenBy(od => od.DayOfWeek)
                .Select(od => new
                {
                    RestaurantOperatingDayId = od.RestaurantOperatingDayId,
                    RestaurantSettingsId = od.RestaurantSettingsId,
                    DayOfWeek = od.DayOfWeek,
                    DisplayOrder = od.DisplayOrder,
                    AddedOn = od.AddedOn
                })
                .ToList();

            // Load Date Overrides from database (only active and not deleted)
            // First materialize the query, then add TimeSlotString conversion
            var dateOverridesList = oConnectionContext.DbClsRestaurantBookingDateOverride
                .Where(d => d.RestaurantSettingsId == det.RestaurantSettingsId && 
                           d.IsActive && 
                           !d.IsDeleted)
                .OrderBy(d => d.OverrideDate)
                .Select(d => new
                {
                    BookingDateOverrideId = d.BookingDateOverrideId,
                    RestaurantSettingsId = d.RestaurantSettingsId,
                    OverrideDate = d.OverrideDate,
                    IsClosed = d.IsClosed,
                    Reason = d.Reason,
                    IsActive = d.IsActive,
                    IsDeleted = d.IsDeleted,
                    AddedBy = d.AddedBy,
                    AddedOn = d.AddedOn,
                    ModifiedBy = d.ModifiedBy,
                    ModifiedOn = d.ModifiedOn,
                    CompanyId = d.CompanyId,
                    TimeSlots = oConnectionContext.DbClsRestaurantBookingDateOverrideSlot
                        .Where(s => s.BookingDateOverrideId == d.BookingDateOverrideId && 
                                   s.IsActive && 
                                   !s.IsDeleted)
                        .OrderBy(s => s.TimeSlot)
                        .Select(s => new
                        {
                            OverrideSlotId = s.OverrideSlotId,
                            BookingDateOverrideId = s.BookingDateOverrideId,
                            TimeSlot = s.TimeSlot,
                            IsActive = s.IsActive,
                            IsDeleted = s.IsDeleted,
                            AddedBy = s.AddedBy,
                            AddedOn = s.AddedOn,
                            ModifiedBy = s.ModifiedBy,
                            ModifiedOn = s.ModifiedOn,
                            CompanyId = s.CompanyId
                        })
                        .ToList()
                })
                .ToList()
                .Select(d => new
                {
                    BookingDateOverrideId = d.BookingDateOverrideId,
                    RestaurantSettingsId = d.RestaurantSettingsId,
                    OverrideDate = d.OverrideDate,
                    OverrideDateString = d.OverrideDate.ToString("yyyy-MM-dd"),
                    IsClosed = d.IsClosed,
                    Reason = d.Reason,
                    IsActive = d.IsActive,
                    IsDeleted = d.IsDeleted,
                    AddedBy = d.AddedBy,
                    AddedOn = d.AddedOn,
                    ModifiedBy = d.ModifiedBy,
                    ModifiedOn = d.ModifiedOn,
                    CompanyId = d.CompanyId,
                    TimeSlots = d.TimeSlots.Select(s => new
                    {
                        OverrideSlotId = s.OverrideSlotId,
                        BookingDateOverrideId = s.BookingDateOverrideId,
                        TimeSlot = s.TimeSlot,
                        TimeSlotString = s.TimeSlot.ToString(@"hh\:mm"),
                        IsActive = s.IsActive,
                        IsDeleted = s.IsDeleted,
                        AddedBy = s.AddedBy,
                        AddedOn = s.AddedOn,
                        ModifiedBy = s.ModifiedBy,
                        ModifiedOn = s.ModifiedOn,
                        CompanyId = s.CompanyId
                    }).ToList()
                })
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RestaurantSettings = new
                    {
                        RestaurantSettingsId = det.RestaurantSettingsId,
                        EnableKitchenDisplay = det.EnableKitchenDisplay,
                        AutoPrintKot = det.AutoPrintKot,
                        EnableTableBooking = det.EnableTableBooking,
                        EnableRecurringBooking = det.EnableRecurringBooking,
                        BookingAdvanceDays = det.BookingAdvanceDays,
                        BookingStartTime = det.BookingStartTime.HasValue ? det.BookingStartTime.Value.ToString(@"hh\:mm") : (string)null,
                        BookingEndTime = det.BookingEndTime.HasValue ? det.BookingEndTime.Value.ToString(@"hh\:mm") : (string)null,
                        BookingTimeSlotMode = det.BookingTimeSlotMode,
                        BookingTimeSlotsList = timeSlotsList,
                        DefaultBookingDuration = det.DefaultBookingDuration,
                        RequireDeposit = det.RequireDeposit,
                        DepositMode = det.DepositMode,
                        DepositFixedAmount = det.DepositFixedAmount,
                        DepositPerGuestAmount = det.DepositPerGuestAmount,
                        EnablePublicBooking = det.EnablePublicBooking,
                        PublicBookingSlug = det.PublicBookingSlug,
                        PublicBookingAdvanceDays = det.PublicBookingAdvanceDays,
                        PublicBookingRequireDeposit = det.PublicBookingRequireDeposit,
                        PublicBookingDepositPercentage = det.PublicBookingDepositPercentage,
                        PublicBookingDepositMode = det.PublicBookingDepositMode,
                        PublicBookingDepositFixedAmount = det.PublicBookingDepositFixedAmount,
                        PublicBookingDepositPerGuestAmount = det.PublicBookingDepositPerGuestAmount,
                        PublicBookingAutoConfirm = det.PublicBookingAutoConfirm,
                        EnablePublicBookingCancellation = det.EnablePublicBookingCancellation,
                        AllowCancelAfterConfirm = det.AllowCancelAfterConfirm,
                        PublicBookingCancellationDaysBefore = det.PublicBookingCancellationDaysBefore,
                        PublicBookingCancellationChargeMode = det.PublicBookingCancellationChargeMode,
                        PublicBookingCancellationFixedCharge = det.PublicBookingCancellationFixedCharge,
                        PublicBookingCancellationPercentage = det.PublicBookingCancellationPercentage,
                        PublicBookingCancellationPerGuestCharge = det.PublicBookingCancellationPerGuestCharge,
                        QRCodeImage = det.QRCodeImage,
                        OperatingDays = operatingDaysList,
                        BookingTimeSlotsNormalized = bookingTimeSlotsNormalized,
                        DateOverrides = dateOverridesList,
                        BranchId = det.BranchId,
                        Branch = oConnectionContext.DbClsBranch
                            .Where(b => b.BranchId == det.BranchId && b.CompanyId == obj.CompanyId)
                            .Select(b => b.Branch)
                            .FirstOrDefault(),
                        CompanyId = det.CompanyId,
                        IsActive = det.IsActive
                    }
                }
            };
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetAllRestaurantSettings(ClsRestaurantSettingsVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var settingsList = oConnectionContext.DbClsRestaurantSettings
                .Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                .Select(a => new
                {
                    RestaurantSettingsId = a.RestaurantSettingsId,
                    EnableKitchenDisplay = a.EnableKitchenDisplay,
                    AutoPrintKot = a.AutoPrintKot,
                    EnableTableBooking = a.EnableTableBooking,
                    EnableRecurringBooking = a.EnableRecurringBooking,
                    BookingAdvanceDays = a.BookingAdvanceDays,
                    DefaultBookingDuration = a.DefaultBookingDuration,
                    RequireDeposit = a.RequireDeposit,
                    DepositMode = a.DepositMode,
                    DepositFixedAmount = a.DepositFixedAmount,
                    DepositPerGuestAmount = a.DepositPerGuestAmount,
                    EnablePublicBooking = a.EnablePublicBooking,
                    PublicBookingSlug = a.PublicBookingSlug,
                    EnablePublicBookingCancellation = a.EnablePublicBookingCancellation,
                    AllowCancelAfterConfirm = a.AllowCancelAfterConfirm,
                    PublicBookingCancellationDaysBefore = a.PublicBookingCancellationDaysBefore,
                    PublicBookingCancellationChargeMode = a.PublicBookingCancellationChargeMode,
                    PublicBookingCancellationFixedCharge = a.PublicBookingCancellationFixedCharge,
                    PublicBookingCancellationPercentage = a.PublicBookingCancellationPercentage,
                    PublicBookingCancellationPerGuestCharge = a.PublicBookingCancellationPerGuestCharge,
                    QRCodeImage = a.QRCodeImage,
                    BranchId = a.BranchId,
                    Branch = oConnectionContext.DbClsBranch
                        .Where(b => b.BranchId == a.BranchId && b.CompanyId == obj.CompanyId)
                        .Select(b => b.Branch)
                        .FirstOrDefault(),
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive
                })
                .ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                settingsList = settingsList.Where(a => 
                    (a.Branch != null && a.Branch.ToLower().Contains(obj.Search.ToLower()))
                ).ToList();
            }

            // Load normalized booking time slots for each setting
            var settingsWithTimeSlots = settingsList.Select(s => new
            {
                s.RestaurantSettingsId,
                s.EnableKitchenDisplay,
                s.AutoPrintKot,
                s.EnableTableBooking,
                s.EnableRecurringBooking,
                s.BookingAdvanceDays,
                BookingTimeSlotsList = oConnectionContext.DbClsRestaurantBookingTimeSlot
                    .Where(ts => ts.RestaurantSettingsId == s.RestaurantSettingsId && 
                                ts.IsActive && 
                                !ts.IsDeleted)
                    .OrderBy(ts => ts.DisplayOrder)
                    .ThenBy(ts => ts.TimeSlot)
                    .Select(ts => ts.TimeSlot)
                    .ToList()
                    .Select(ts => ts.ToString(@"hh\:mm"))
                    .ToList(),
                s.DefaultBookingDuration,
                    s.RequireDeposit,
                    s.EnablePublicBooking,
                    s.PublicBookingSlug,
                    s.EnablePublicBookingCancellation,
                    s.AllowCancelAfterConfirm,
                    s.PublicBookingCancellationDaysBefore,
                    s.PublicBookingCancellationChargeMode,
                    s.PublicBookingCancellationFixedCharge,
                    s.PublicBookingCancellationPercentage,
                    s.PublicBookingCancellationPerGuestCharge,
                    s.QRCodeImage,
                    s.BranchId,
                s.Branch,
                s.CompanyId,
                s.IsActive
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RestaurantSettingsList = settingsWithTimeSlots.OrderByDescending(a => a.RestaurantSettingsId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = settingsWithTimeSlots.Count(),
                    ActiveCount = settingsWithTimeSlots.Where(a => a.IsActive == true).Count(),
                    InactiveCount = settingsWithTimeSlots.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RestaurantSettings(ClsRestaurantSettingsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsRestaurantSettings existing = null;
                if (obj.RestaurantSettingsId > 0)
                {
                    existing = oConnectionContext.DbClsRestaurantSettings.Where(a => a.CompanyId == obj.CompanyId && a.RestaurantSettingsId == obj.RestaurantSettingsId && a.IsDeleted == false).FirstOrDefault();
                }
                else
                {
                    existing = oConnectionContext.DbClsRestaurantSettings.Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false).FirstOrDefault();
                }

                // Get existing QR path (for updates) so we can reuse/delete old file similar to RestaurantTable
                string existingQrPath = null;
                if (existing != null)
                {
                    existingQrPath = existing.QRCodeImage;
                }

                // Validate PublicBookingSlug uniqueness (globally unique)
                if (obj.EnablePublicBooking && !string.IsNullOrWhiteSpace(obj.PublicBookingSlug))
                {
                    // Normalize slug to lowercase for comparison
                    string normalizedSlug = obj.PublicBookingSlug.Trim().ToLower();
                    
                    // Check if slug already exists globally (any company, any branch)
                    var existingSlug = oConnectionContext.DbClsRestaurantSettings
                        .Where(s => s.PublicBookingSlug != null &&
                                   s.PublicBookingSlug.ToLower() == normalizedSlug &&
                                   s.IsDeleted == false &&
                                   s.EnablePublicBooking == true &&
                                   // Exclude current record if updating
                                   (existing == null || s.RestaurantSettingsId != existing.RestaurantSettingsId))
                        .FirstOrDefault();
                    
                    if (existingSlug != null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "This booking slug is already in use. Please choose a different slug.",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // Parse BookingStartTime and BookingEndTime from string properties following table booking pattern
                TimeSpan? bookingStartTime = null;
                TimeSpan? bookingEndTime = null;
                
                if (obj.BookingTimeSlotMode == "Auto")
                {
                    // Parse BookingStartTime from BookingStartTimeString (JSON sends it as "HH:mm" string)
                    if (!string.IsNullOrEmpty(obj.BookingStartTimeString))
                    {
                        try
                        {
                            // Parse time string (format: HH:mm or HH:mm:ss)
                            var timeParts = obj.BookingStartTimeString.Split(':');
                            if (timeParts.Length >= 2)
                            {
                                int hours = 0, minutes = 0;
                                if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                {
                                    // Validate hours and minutes
                                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                    {
                                        bookingStartTime = new TimeSpan(hours, minutes, 0);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If parsing fails, leave as null
                        }
                    }
                    
                    // Parse BookingEndTime from BookingEndTimeString (JSON sends it as "HH:mm" string)
                    if (!string.IsNullOrEmpty(obj.BookingEndTimeString))
                    {
                        try
                        {
                            // Parse time string (format: HH:mm or HH:mm:ss)
                            var timeParts = obj.BookingEndTimeString.Split(':');
                            if (timeParts.Length >= 2)
                            {
                                int hours = 0, minutes = 0;
                                if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                {
                                    // Validate hours and minutes
                                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                    {
                                        bookingEndTime = new TimeSpan(hours, minutes, 0);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If parsing fails, leave as null
                        }
                    }
                }
                else
                {
                    // When mode is Manual, set to null (not used)
                    bookingStartTime = null;
                    bookingEndTime = null;
                }

                ClsRestaurantSettings oSettings = new ClsRestaurantSettings()
                {
                    RestaurantSettingsId = existing != null ? existing.RestaurantSettingsId : (obj.RestaurantSettingsId > 0 ? obj.RestaurantSettingsId : 0),
                    EnableKitchenDisplay = obj.EnableKitchenDisplay,
                    AutoPrintKot = obj.AutoPrintKot,
                    EnableTableBooking = obj.EnableTableBooking,
                    EnableRecurringBooking = obj.EnableRecurringBooking,
                    BookingAdvanceDays = obj.BookingAdvanceDays,
                    BookingStartTime = bookingStartTime,
                    BookingEndTime = bookingEndTime,
                    BookingTimeSlotMode = obj.BookingTimeSlotMode,
                    DefaultBookingDuration = obj.DefaultBookingDuration,
                    RequireDeposit = obj.RequireDeposit,
                    DepositMode = obj.DepositMode,
                    DepositFixedAmount = obj.DepositFixedAmount,
                    DepositPerGuestAmount = obj.DepositPerGuestAmount,
                    PublicBookingDepositMode = obj.PublicBookingDepositMode,
                    PublicBookingDepositFixedAmount = obj.PublicBookingDepositFixedAmount,
                    PublicBookingDepositPerGuestAmount = obj.PublicBookingDepositPerGuestAmount,
                    EnablePublicBooking = obj.EnablePublicBooking,
                    PublicBookingSlug = obj.PublicBookingSlug,
                    PublicBookingAdvanceDays = obj.PublicBookingAdvanceDays,
                    PublicBookingRequireDeposit = obj.PublicBookingRequireDeposit,
                    PublicBookingDepositPercentage = obj.PublicBookingDepositPercentage,
                    PublicBookingAutoConfirm = obj.PublicBookingAutoConfirm,
                    EnablePublicBookingCancellation = obj.EnablePublicBookingCancellation,
                    AllowCancelAfterConfirm = obj.AllowCancelAfterConfirm,
                    PublicBookingCancellationDaysBefore = obj.PublicBookingCancellationDaysBefore,
                    PublicBookingCancellationChargeMode = obj.PublicBookingCancellationChargeMode,
                    PublicBookingCancellationFixedCharge = obj.PublicBookingCancellationFixedCharge,
                    PublicBookingCancellationPercentage = obj.PublicBookingCancellationPercentage,
                    PublicBookingCancellationPerGuestCharge = obj.PublicBookingCancellationPerGuestCharge,
                    QRCodeImage = existingQrPath,
                    BranchId = obj.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = existing != null ? existing.AddedOn : CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };

                if (existing == null)
                {
                    oConnectionContext.DbClsRestaurantSettings.Add(oSettings);
                }
                else
                {
                    oConnectionContext.DbClsRestaurantSettings.Attach(oSettings);
                    oConnectionContext.Entry(oSettings).Property(x => x.RestaurantSettingsId).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.EnableKitchenDisplay).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.AutoPrintKot).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.EnableTableBooking).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.EnableRecurringBooking).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.BookingAdvanceDays).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.BookingStartTime).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.BookingEndTime).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.BookingTimeSlotMode).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.DefaultBookingDuration).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.RequireDeposit).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.DepositMode).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.DepositFixedAmount).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.DepositPerGuestAmount).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingDepositMode).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingDepositFixedAmount).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingDepositPerGuestAmount).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.EnablePublicBooking).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingSlug).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingAdvanceDays).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingRequireDeposit).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingDepositPercentage).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingAutoConfirm).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.EnablePublicBookingCancellation).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.AllowCancelAfterConfirm).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingCancellationDaysBefore).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingCancellationChargeMode).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingCancellationFixedCharge).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingCancellationPercentage).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.PublicBookingCancellationPerGuestCharge).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.QRCodeImage).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oSettings).Property(x => x.ModifiedOn).IsModified = true;
                }

                oConnectionContext.SaveChanges();

                // Generate and save QR code for public booking page (if enabled and slug is set)
                try
                {
                    if (oSettings.EnablePublicBooking && !string.IsNullOrWhiteSpace(oSettings.PublicBookingSlug))
                    {
                        // Get domain from request
                        string domain = null;
                        var request = HttpContext.Current?.Request;
                        if (request != null)
                        {
                            domain = request.Url?.GetLeftPart(UriPartial.Authority);
                        }
                        var qrPath = GeneratePublicBookingQrCode(oSettings.RestaurantSettingsId, oSettings.PublicBookingSlug, domain, oSettings.QRCodeImage);
                        if (!string.IsNullOrWhiteSpace(qrPath))
                        {
                            oSettings.QRCodeImage = qrPath;
                            if (existing != null)
                            {
                                existing.QRCodeImage = qrPath;
                                oConnectionContext.Entry(existing).Property(x => x.QRCodeImage).IsModified = true;
                            }
                            else
                            {
                                oConnectionContext.Entry(oSettings).Property(x => x.QRCodeImage).IsModified = true;
                            }
                            oConnectionContext.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the save operation
                    System.Diagnostics.Debug.WriteLine("Error generating QR code for restaurant settings: " + ex.Message);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "RestaurantSettings",
                    CompanyId = obj.CompanyId,
                    Description = "Restaurant Settings updated",
                    Id = oSettings.RestaurantSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Restaurant Settings saved successfully",
                    Data = new
                    {
                        RestaurantSettings = new
                        {
                            oSettings.RestaurantSettingsId,
                            oSettings.EnableKitchenDisplay,
                            oSettings.AutoPrintKot,
                            oSettings.EnableTableBooking,
                            oSettings.EnableRecurringBooking,
                            oSettings.BookingAdvanceDays,
                            BookingStartTime = oSettings.BookingStartTime.HasValue ? oSettings.BookingStartTime.Value.ToString(@"hh\:mm") : (string)null,
                            BookingEndTime = oSettings.BookingEndTime.HasValue ? oSettings.BookingEndTime.Value.ToString(@"hh\:mm") : (string)null,
                            oSettings.BookingTimeSlotMode,
                            oSettings.DefaultBookingDuration,
                            oSettings.RequireDeposit,
                            oSettings.DepositMode,
                            oSettings.DepositFixedAmount,
                            oSettings.DepositPerGuestAmount,
                            oSettings.PublicBookingDepositMode,
                            oSettings.PublicBookingDepositFixedAmount,
                            oSettings.PublicBookingDepositPerGuestAmount,
                            oSettings.EnablePublicBooking,
                            oSettings.PublicBookingSlug,
                            oSettings.PublicBookingAdvanceDays,
                            oSettings.PublicBookingRequireDeposit,
                            oSettings.PublicBookingDepositPercentage,
                            oSettings.PublicBookingAutoConfirm,
                            oSettings.EnablePublicBookingCancellation,
                            oSettings.AllowCancelAfterConfirm,
                            oSettings.PublicBookingCancellationDaysBefore,
                            oSettings.PublicBookingCancellationChargeMode,
                            oSettings.PublicBookingCancellationFixedCharge,
                            oSettings.PublicBookingCancellationPercentage,
                            oSettings.PublicBookingCancellationPerGuestCharge,
                            oSettings.QRCodeImage,
                            oSettings.BranchId,
                            oSettings.CompanyId,
                            oSettings.IsActive,
                            oSettings.IsDeleted,
                            oSettings.AddedBy,
                            oSettings.AddedOn,
                            oSettings.ModifiedBy,
                            oSettings.ModifiedOn
                        }
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> UpdateRestaurantSettings(ClsRestaurantSettingsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            
            // Handle OperatingDays - convert from integers if needed
            // JSON may send OperatingDays as array of integers, which won't deserialize to List<ClsRestaurantOperatingDay>
            // Check if we have OperatingDaysInt (integers) and convert to OperatingDays (objects)
            if ((obj.OperatingDays == null || obj.OperatingDays.Count == 0) && 
                obj.OperatingDaysInt != null && obj.OperatingDaysInt.Count > 0)
            {
                obj.OperatingDays = obj.OperatingDaysInt
                    .Where(d => d >= 0 && d <= 6)
                    .Select(d => new ClsRestaurantOperatingDay { DayOfWeek = d })
                    .ToList();
            }
            
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var existing = oConnectionContext.DbClsRestaurantSettings.FirstOrDefault(a =>
                    a.CompanyId == obj.CompanyId &&
                    a.RestaurantSettingsId == obj.RestaurantSettingsId &&
                    a.IsDeleted == false);

                if (existing == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Restaurant Settings not found",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Validate PublicBookingSlug uniqueness (globally unique)
                if (obj.EnablePublicBooking && !string.IsNullOrWhiteSpace(obj.PublicBookingSlug))
                {
                    // Normalize slug to lowercase for comparison
                    string normalizedSlug = obj.PublicBookingSlug.Trim().ToLower();
                    
                    // Check if slug already exists globally (any company, any branch)
                    var existingSlug = oConnectionContext.DbClsRestaurantSettings
                        .Where(s => s.PublicBookingSlug != null &&
                                   s.PublicBookingSlug.ToLower() == normalizedSlug &&
                                   s.IsDeleted == false &&
                                   s.EnablePublicBooking == true &&
                                   // Exclude current record
                                   s.RestaurantSettingsId != existing.RestaurantSettingsId)
                        .FirstOrDefault();
                    
                    if (existingSlug != null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "This booking slug is already in use. Please choose a different slug.",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // Parse BookingStartTime and BookingEndTime from string properties following table booking pattern
                TimeSpan? bookingStartTime = null;
                TimeSpan? bookingEndTime = null;
                
                if (obj.BookingTimeSlotMode == "Auto")
                {
                    // Parse BookingStartTime from BookingStartTimeString (JSON sends it as "HH:mm" string)
                    if (!string.IsNullOrEmpty(obj.BookingStartTimeString))
                    {
                        try
                        {
                            // Parse time string (format: HH:mm or HH:mm:ss)
                            var timeParts = obj.BookingStartTimeString.Split(':');
                            if (timeParts.Length >= 2)
                            {
                                int hours = 0, minutes = 0;
                                if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                {
                                    // Validate hours and minutes
                                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                    {
                                        bookingStartTime = new TimeSpan(hours, minutes, 0);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If parsing fails, leave as null
                        }
                    }
                    
                    // Parse BookingEndTime from BookingEndTimeString (JSON sends it as "HH:mm" string)
                    if (!string.IsNullOrEmpty(obj.BookingEndTimeString))
                    {
                        try
                        {
                            // Parse time string (format: HH:mm or HH:mm:ss)
                            var timeParts = obj.BookingEndTimeString.Split(':');
                            if (timeParts.Length >= 2)
                            {
                                int hours = 0, minutes = 0;
                                if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                {
                                    // Validate hours and minutes
                                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                    {
                                        bookingEndTime = new TimeSpan(hours, minutes, 0);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If parsing fails, leave as null
                        }
                    }
                }
                else
                {
                    // When mode is Manual, set to null (not used)
                    bookingStartTime = null;
                    bookingEndTime = null;
                }

                // Update the existing tracked entity to avoid attach conflicts
                existing.EnableKitchenDisplay = obj.EnableKitchenDisplay;
                existing.AutoPrintKot = obj.AutoPrintKot;
                existing.EnableTableBooking = obj.EnableTableBooking;
                existing.EnableRecurringBooking = obj.EnableRecurringBooking;
                existing.BookingAdvanceDays = obj.BookingAdvanceDays;
                existing.BookingStartTime = bookingStartTime;
                existing.BookingEndTime = bookingEndTime;
                existing.BookingTimeSlotMode = obj.BookingTimeSlotMode;
                existing.DefaultBookingDuration = obj.DefaultBookingDuration;
                existing.RequireDeposit = obj.RequireDeposit;
                existing.DepositMode = obj.DepositMode;
                existing.DepositFixedAmount = obj.DepositFixedAmount;
                existing.DepositPerGuestAmount = obj.DepositPerGuestAmount;
                existing.EnablePublicBooking = obj.EnablePublicBooking;
                existing.PublicBookingSlug = obj.PublicBookingSlug;
                existing.PublicBookingAdvanceDays = obj.PublicBookingAdvanceDays;
                existing.PublicBookingRequireDeposit = obj.PublicBookingRequireDeposit;
                existing.PublicBookingDepositPercentage = obj.PublicBookingDepositPercentage;
                existing.PublicBookingDepositMode = obj.PublicBookingDepositMode;
                existing.PublicBookingDepositFixedAmount = obj.PublicBookingDepositFixedAmount;
                existing.PublicBookingDepositPerGuestAmount = obj.PublicBookingDepositPerGuestAmount;
                existing.PublicBookingAutoConfirm = obj.PublicBookingAutoConfirm;
                existing.EnablePublicBookingCancellation = obj.EnablePublicBookingCancellation;
                existing.AllowCancelAfterConfirm = obj.AllowCancelAfterConfirm;
                existing.PublicBookingCancellationDaysBefore = obj.PublicBookingCancellationDaysBefore;
                existing.PublicBookingCancellationChargeMode = obj.PublicBookingCancellationChargeMode;
                existing.PublicBookingCancellationFixedCharge = obj.PublicBookingCancellationFixedCharge;
                existing.PublicBookingCancellationPercentage = obj.PublicBookingCancellationPercentage;
                existing.PublicBookingCancellationPerGuestCharge = obj.PublicBookingCancellationPerGuestCharge;
                // IsActive is managed from the list view via ActiveInactive endpoint, preserve existing value here
                // Only update IsActive if the property was explicitly set in the update request
                // (Since it's removed from edit form, we preserve the existing value)
                existing.ModifiedBy = obj.AddedBy;
                existing.ModifiedOn = CurrentDate;

                oConnectionContext.SaveChanges();

                // Handle normalized booking time slots
                // Get all existing time slots (including soft-deleted ones)
                var existingTimeSlots = oConnectionContext.DbClsRestaurantBookingTimeSlot
                    .Where(ts => ts.RestaurantSettingsId == existing.RestaurantSettingsId)
                    .ToList();
                
                if (obj.BookingTimeSlotsNormalized != null && obj.BookingTimeSlotsNormalized.Count > 0)
                {
                    // Mark existing time slots as deleted (soft delete)
                    foreach (var existingSlot in existingTimeSlots)
                    {
                        existingSlot.IsDeleted = true;
                        existingSlot.IsActive = false;
                        existingSlot.ModifiedBy = obj.AddedBy;
                        existingSlot.ModifiedOn = CurrentDate;
                        oConnectionContext.Entry(existingSlot).Property(x => x.IsDeleted).IsModified = true;
                        oConnectionContext.Entry(existingSlot).Property(x => x.IsActive).IsModified = true;
                        oConnectionContext.Entry(existingSlot).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(existingSlot).Property(x => x.ModifiedOn).IsModified = true;
                    }
                    oConnectionContext.SaveChanges();

                    // Add new time slots
                    int slotDisplayOrder = 0;
                    foreach (var timeSlot in obj.BookingTimeSlotsNormalized)
                    {
                        TimeSpan? slotTime = null;
                        long bookingTimeSlotId = 0;
                        bool isActive = true;
                        
                        // Parse TimeSlotString from string (JSON sends it as "HH:mm" string)
                        // Check if it's a ClsRestaurantBookingTimeSlotVm object with TimeSlotString
                        if (timeSlot is ClsRestaurantBookingTimeSlotVm)
                        {
                            var slotVm = (ClsRestaurantBookingTimeSlotVm)timeSlot;
                            bookingTimeSlotId = slotVm.BookingTimeSlotId;
                            isActive = slotVm.IsActive;
                            
                            if (!string.IsNullOrEmpty(slotVm.TimeSlotString))
                            {
                                // Parse time string (format: HH:mm or HH:mm:ss) - same approach as BookingStartTimeString
                                try
                                {
                                    var timeString = slotVm.TimeSlotString;
                                    var timeParts = timeString.Split(':');
                                    if (timeParts.Length >= 2)
                                    {
                                        int hours = 0, minutes = 0;
                                        if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                        {
                                            // Validate hours and minutes
                                            if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                            {
                                                slotTime = new TimeSpan(hours, minutes, 0);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // If parsing fails, skip this time slot
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            // Fallback: Get properties using reflection (for anonymous objects)
                            var bookingTimeSlotIdProperty = timeSlot.GetType().GetProperty("BookingTimeSlotId");
                            if (bookingTimeSlotIdProperty != null)
                            {
                                var idValue = bookingTimeSlotIdProperty.GetValue(timeSlot);
                                if (idValue is long)
                                    bookingTimeSlotId = (long)idValue;
                                else if (idValue != null && long.TryParse(idValue.ToString(), out long parsedId))
                                    bookingTimeSlotId = parsedId;
                            }
                            
                            var isActiveProperty = timeSlot.GetType().GetProperty("IsActive");
                            if (isActiveProperty != null)
                            {
                                var activeValue = isActiveProperty.GetValue(timeSlot);
                                if (activeValue is bool)
                                    isActive = (bool)activeValue;
                            }
                            
                            // Get TimeSlotString property using reflection
                            var timeSlotStringProperty = timeSlot.GetType().GetProperty("TimeSlotString");
                            if (timeSlotStringProperty != null)
                            {
                                var timeSlotStringValue = timeSlotStringProperty.GetValue(timeSlot);
                                if (timeSlotStringValue is string && !string.IsNullOrEmpty((string)timeSlotStringValue))
                                {
                                    // Parse time string (format: HH:mm or HH:mm:ss) - same approach as BookingStartTimeString
                                    try
                                    {
                                        var timeString = (string)timeSlotStringValue;
                                        var timeParts = timeString.Split(':');
                                        if (timeParts.Length >= 2)
                                        {
                                            int hours = 0, minutes = 0;
                                            if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                            {
                                                // Validate hours and minutes
                                                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                                {
                                                    slotTime = new TimeSpan(hours, minutes, 0);
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // If parsing fails, skip this time slot
                                        continue;
                                    }
                                }
                            }
                            
                            // Also check TimeSlot property as fallback (for backward compatibility)
                            if (!slotTime.HasValue)
                            {
                                var timeSlotProperty = timeSlot.GetType().GetProperty("TimeSlot");
                                if (timeSlotProperty != null)
                                {
                                    var timeSlotValue = timeSlotProperty.GetValue(timeSlot);
                                    if (timeSlotValue is TimeSpan && (TimeSpan)timeSlotValue != default(TimeSpan))
                                    {
                                        slotTime = (TimeSpan)timeSlotValue;
                                    }
                                }
                            }
                        }

                        if (slotTime.HasValue)
                        {
                            // Check if this time slot already exists (for update scenario)
                            var existingSlot = existingTimeSlots
                                .FirstOrDefault(ts => ts.BookingTimeSlotId == bookingTimeSlotId && bookingTimeSlotId > 0);
                            
                            if (existingSlot != null)
                            {
                                // Update existing time slot
                                existingSlot.TimeSlot = slotTime.Value;
                                existingSlot.DisplayOrder = slotDisplayOrder++;
                                existingSlot.IsActive = isActive;
                                existingSlot.IsDeleted = false;
                                existingSlot.ModifiedBy = obj.AddedBy;
                                existingSlot.ModifiedOn = CurrentDate;
                                oConnectionContext.Entry(existingSlot).Property(x => x.TimeSlot).IsModified = true;
                                oConnectionContext.Entry(existingSlot).Property(x => x.DisplayOrder).IsModified = true;
                                oConnectionContext.Entry(existingSlot).Property(x => x.IsActive).IsModified = true;
                                oConnectionContext.Entry(existingSlot).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(existingSlot).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(existingSlot).Property(x => x.ModifiedOn).IsModified = true;
                            }
                            else
                            {
                                // Create new time slot
                                var newTimeSlot = new ClsRestaurantBookingTimeSlot
                                {
                                    RestaurantSettingsId = existing.RestaurantSettingsId,
                                    TimeSlot = slotTime.Value,
                                    DisplayOrder = slotDisplayOrder++,
                                    IsActive = isActive,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    CompanyId = obj.CompanyId
                                };
                                oConnectionContext.DbClsRestaurantBookingTimeSlot.Add(newTimeSlot);
                            }
                        }
                    }
                    oConnectionContext.SaveChanges();
                }
                else
                {
                    // If no time slots provided, mark all existing as deleted (soft delete)
                    foreach (var existingSlot in existingTimeSlots.Where(ts => !ts.IsDeleted))
                    {
                        existingSlot.IsDeleted = true;
                        existingSlot.IsActive = false;
                        existingSlot.ModifiedBy = obj.AddedBy;
                        existingSlot.ModifiedOn = CurrentDate;
                        oConnectionContext.Entry(existingSlot).Property(x => x.IsDeleted).IsModified = true;
                        oConnectionContext.Entry(existingSlot).Property(x => x.IsActive).IsModified = true;
                        oConnectionContext.Entry(existingSlot).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(existingSlot).Property(x => x.ModifiedOn).IsModified = true;
                    }
                    oConnectionContext.SaveChanges();
                }

                // Handle Operating Days with soft delete
                // Get all existing operating days (including soft-deleted/inactive ones)
                var existingOperatingDays = oConnectionContext.DbClsRestaurantOperatingDay
                    .Where(od => od.RestaurantSettingsId == existing.RestaurantSettingsId)
                    .ToList();

                // Get the list of days that should be active (from incoming data)
                var activeDays = obj.OperatingDays != null && obj.OperatingDays.Count > 0
                    ? obj.OperatingDays
                        .Select(od => od.DayOfWeek)
                        .Where(d => d >= 0 && d <= 6)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList()
                    : new List<int>();

                // Process each day that should be active
                int displayOrder = 0;
                foreach (var dayOfWeek in activeDays)
                {
                    // Check if a record already exists (even if deleted/inactive)
                    var existingDay = existingOperatingDays
                        .FirstOrDefault(od => od.DayOfWeek == dayOfWeek);

                    if (existingDay != null)
                    {
                        // Record exists - reactivate it and update fields
                        existingDay.IsActive = true;
                        existingDay.IsDeleted = false;
                        existingDay.DisplayOrder = displayOrder++;
                        existingDay.ModifiedBy = obj.AddedBy;
                        existingDay.ModifiedOn = CurrentDate;
                        // Mark the entity as modified
                        oConnectionContext.Entry(existingDay).Property(x => x.IsActive).IsModified = true;
                        oConnectionContext.Entry(existingDay).Property(x => x.IsDeleted).IsModified = true;
                        oConnectionContext.Entry(existingDay).Property(x => x.DisplayOrder).IsModified = true;
                        oConnectionContext.Entry(existingDay).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(existingDay).Property(x => x.ModifiedOn).IsModified = true;
                    }
                    else
                    {
                        // Record doesn't exist - create new one
                        var newOperatingDay = new ClsRestaurantOperatingDay
                        {
                            RestaurantSettingsId = existing.RestaurantSettingsId,
                            DayOfWeek = dayOfWeek,
                            DisplayOrder = displayOrder++,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsRestaurantOperatingDay.Add(newOperatingDay);
                    }
                }

                // Mark days that should not be active as soft deleted
                foreach (var existingDay in existingOperatingDays.Where(od => 
                    !activeDays.Contains(od.DayOfWeek) && 
                    od.IsActive && 
                    !od.IsDeleted))
                {
                    existingDay.IsActive = false;
                    existingDay.IsDeleted = true;
                    existingDay.ModifiedBy = obj.AddedBy;
                    existingDay.ModifiedOn = CurrentDate;
                    oConnectionContext.Entry(existingDay).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(existingDay).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(existingDay).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(existingDay).Property(x => x.ModifiedOn).IsModified = true;
                }

                oConnectionContext.SaveChanges();

                // Handle Date Overrides
                // Remove existing date overrides (cascade will remove time slots)
                // Only remove non-deleted ones to maintain data integrity
                var existingDateOverrides = oConnectionContext.DbClsRestaurantBookingDateOverride
                    .Where(d => d.RestaurantSettingsId == existing.RestaurantSettingsId && !d.IsDeleted)
                    .ToList();
                oConnectionContext.DbClsRestaurantBookingDateOverride.RemoveRange(existingDateOverrides);
                oConnectionContext.SaveChanges();

                // Add new date overrides
                if (obj.DateOverrides != null && obj.DateOverrides.Count > 0)
                {
                    foreach (var dateOverride in obj.DateOverrides)
                    {
                        // Parse the date - prefer OverrideDateString if available (from form input)
                        // Otherwise use OverrideDate (from JSON deserialization)
                        DateTime overrideDateValue;
                        
                        if (!string.IsNullOrEmpty(dateOverride.OverrideDateString))
                        {
                            // Parse from string (format: yyyy-MM-dd from date input)
                            if (!DateTime.TryParse(dateOverride.OverrideDateString, out overrideDateValue))
                            {
                                // Fallback to OverrideDate if parsing fails
                                overrideDateValue = dateOverride.OverrideDate;
                            }
                        }
                        else
                        {
                            overrideDateValue = dateOverride.OverrideDate;
                        }
                        
                        // Apply timezone adjustment to OverrideDate (same as TableBookingController)
                        // Only adjust if the date has a time component (midnight might indicate it came from string)
                        var adjustedOverrideDate = overrideDateValue;
                        if (overrideDateValue.TimeOfDay == TimeSpan.Zero)
                        {
                            // Date came as string, add timezone offset then take date only
                            adjustedOverrideDate = overrideDateValue.AddHours(5).AddMinutes(30);
                        }
                        adjustedOverrideDate = adjustedOverrideDate.Date;
                        
                        var newDateOverride = new ClsRestaurantBookingDateOverride
                        {
                            RestaurantSettingsId = existing.RestaurantSettingsId,
                            OverrideDate = adjustedOverrideDate,
                            IsClosed = dateOverride.IsClosed,
                            Reason = dateOverride.Reason,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsRestaurantBookingDateOverride.Add(newDateOverride);
                        oConnectionContext.SaveChanges(); // Save to get the ID

                        // Add time slots if the override is open (not closed)
                        if (!dateOverride.IsClosed && dateOverride.TimeSlots != null && dateOverride.TimeSlots.Count > 0)
                        {
                            foreach (var timeSlot in dateOverride.TimeSlots)
                            {
                                TimeSpan? slotTime = null;
                                
                                // Check if it's a ClsRestaurantBookingDateOverrideSlotVm object with TimeSlotString
                                if (timeSlot is ClsRestaurantBookingDateOverrideSlotVm)
                                {
                                    var slotVm = (ClsRestaurantBookingDateOverrideSlotVm)timeSlot;
                                    if (!string.IsNullOrEmpty(slotVm.TimeSlotString))
                                    {
                                        // Parse time string (format: HH:mm or HH:mm:ss) - same approach as BookingStartTimeString
                                        try
                                        {
                                            var timeString = slotVm.TimeSlotString;
                                            var timeParts = timeString.Split(':');
                                            if (timeParts.Length >= 2)
                                            {
                                                int hours = 0, minutes = 0;
                                                if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                                {
                                                    // Validate hours and minutes
                                                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                                    {
                                                        slotTime = new TimeSpan(hours, minutes, 0);
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            // If parsing fails, leave as null
                                        }
                                    }
                                }
                                else
                                {
                                    // Fallback: Get TimeSlotString property using reflection (for anonymous objects)
                                    var timeSlotStringProperty = timeSlot.GetType().GetProperty("TimeSlotString");
                                    if (timeSlotStringProperty != null)
                                    {
                                        var timeSlotStringValue = timeSlotStringProperty.GetValue(timeSlot);
                                        if (timeSlotStringValue is string && !string.IsNullOrEmpty((string)timeSlotStringValue))
                                        {
                                            // Parse time string (format: HH:mm or HH:mm:ss) - same approach as BookingStartTimeString
                                            try
                                            {
                                                var timeString = (string)timeSlotStringValue;
                                                var timeParts = timeString.Split(':');
                                                if (timeParts.Length >= 2)
                                                {
                                                    int hours = 0, minutes = 0;
                                                    if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes))
                                                    {
                                                        // Validate hours and minutes
                                                        if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                                        {
                                                            slotTime = new TimeSpan(hours, minutes, 0);
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                // If parsing fails, leave as null
                                            }
                                        }
                                    }
                                }

                                if (slotTime.HasValue)
                                {
                                    var newOverrideSlot = new ClsRestaurantBookingDateOverrideSlot
                                    {
                                        BookingDateOverrideId = newDateOverride.BookingDateOverrideId,
                                        TimeSlot = slotTime.Value,
                                        IsActive = true,
                                        IsDeleted = false,
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        ModifiedBy = obj.AddedBy,
                                        ModifiedOn = CurrentDate,
                                        CompanyId = obj.CompanyId
                                    };
                                    oConnectionContext.DbClsRestaurantBookingDateOverrideSlot.Add(newOverrideSlot);
                                }
                            }
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                // Generate and save QR code for public booking page (if enabled and slug is set)
                try
                {
                    if (existing.EnablePublicBooking && !string.IsNullOrWhiteSpace(existing.PublicBookingSlug))
                    {
                        // Get domain from request
                        string domain = null;
                        var request = HttpContext.Current?.Request;
                        if (request != null)
                        {
                            domain = request.Url?.GetLeftPart(UriPartial.Authority);
                        }
                        var qrPath = GeneratePublicBookingQrCode(existing.RestaurantSettingsId, existing.PublicBookingSlug, domain, existing.QRCodeImage);
                        if (!string.IsNullOrWhiteSpace(qrPath))
                        {
                            existing.QRCodeImage = qrPath;
                            oConnectionContext.Entry(existing).Property(x => x.QRCodeImage).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the save operation
                    System.Diagnostics.Debug.WriteLine("Error generating QR code for restaurant settings: " + ex.Message);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "RestaurantSettings",
                    CompanyId = obj.CompanyId,
                    Description = "Restaurant Settings updated",
                    Id = existing.RestaurantSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Restaurant Settings updated successfully",
                    Data = new
                    {
                        RestaurantSettings = new
                        {
                            existing.RestaurantSettingsId,
                            existing.EnableKitchenDisplay,
                            existing.AutoPrintKot,
                            existing.EnableTableBooking,
                            existing.EnableRecurringBooking,
                            existing.BookingAdvanceDays,
                            BookingStartTime = existing.BookingStartTime.HasValue ? existing.BookingStartTime.Value.ToString(@"hh\:mm") : (string)null,
                            BookingEndTime = existing.BookingEndTime.HasValue ? existing.BookingEndTime.Value.ToString(@"hh\:mm") : (string)null,
                            existing.BookingTimeSlotMode,
                            existing.DefaultBookingDuration,
                            existing.RequireDeposit,
                            existing.DepositMode,
                            existing.DepositFixedAmount,
                            existing.DepositPerGuestAmount,
                            existing.PublicBookingDepositMode,
                            existing.PublicBookingDepositFixedAmount,
                            existing.PublicBookingDepositPerGuestAmount,
                            existing.EnablePublicBooking,
                            existing.PublicBookingSlug,
                            existing.PublicBookingAdvanceDays,
                            existing.PublicBookingRequireDeposit,
                            existing.PublicBookingDepositPercentage,
                            existing.PublicBookingAutoConfirm,
                            existing.EnablePublicBookingCancellation,
                            existing.AllowCancelAfterConfirm,
                            existing.PublicBookingCancellationDaysBefore,
                            existing.PublicBookingCancellationChargeMode,
                            existing.PublicBookingCancellationFixedCharge,
                            existing.PublicBookingCancellationPercentage,
                            existing.PublicBookingCancellationPerGuestCharge,
                            existing.QRCodeImage,
                            existing.BranchId,
                            existing.CompanyId,
                            existing.IsActive,
                            existing.IsDeleted,
                            existing.AddedBy,
                            existing.AddedOn,
                            existing.ModifiedBy,
                            existing.ModifiedOn
                        }
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Generates QR code for restaurant public booking page
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GenerateQrCode(ClsRestaurantSettingsVm obj)
        {
            if (obj.RestaurantSettingsId <= 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Restaurant Settings ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var settings = oConnectionContext.DbClsRestaurantSettings
                .Where(a => a.CompanyId == obj.CompanyId && 
                           a.RestaurantSettingsId == obj.RestaurantSettingsId && 
                           a.IsDeleted == false)
                .FirstOrDefault();

            if (settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Restaurant Settings not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            if (!settings.EnablePublicBooking || string.IsNullOrWhiteSpace(settings.PublicBookingSlug))
            {
                data = new
                {
                    Status = 0,
                    Message = "Public booking is not enabled or slug is not set",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            try
            {
                // Get domain from request
                string domain = null;
                var request = HttpContext.Current?.Request;
                if (request != null)
                {
                    domain = request.Url?.GetLeftPart(UriPartial.Authority);
                }

                // Build QR code URL
                string qrCodeUrl = BuildPublicBookingUrl(settings.PublicBookingSlug, domain);

                // Only use the already-generated QR code image from disk.
                // We do NOT regenerate here; generation happens when the settings are saved.
                string savedQrPath = !string.IsNullOrWhiteSpace(settings.QRCodeImage)
                    ? settings.QRCodeImage
                    : GetPublicBookingQrCodeFilePath(settings.RestaurantSettingsId, settings.PublicBookingSlug);

                if (string.IsNullOrWhiteSpace(savedQrPath) || !File.Exists(HostingEnvironment.MapPath(savedQrPath)))
                {
                    data = new
                    {
                        Status = 0,
                        Message = "QR code image not available. Please save the restaurant settings to generate QR code.",
                        Data = new { QrCodeUrl = qrCodeUrl, RestaurantSettingsId = settings.RestaurantSettingsId, PublicBookingSlug = settings.PublicBookingSlug }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Return the image URL directly instead of converting to base64
                string qrCodeImageUrl = savedQrPath.StartsWith("~") ? savedQrPath.Substring(1) : savedQrPath;
                if (!qrCodeImageUrl.StartsWith("/"))
                {
                    qrCodeImageUrl = "/" + qrCodeImageUrl;
                }

                data = new
                {
                    Status = 1,
                    Message = "QR code generated successfully",
                    Data = new
                    {
                        QrCodeImageUrl = qrCodeImageUrl,
                        QrCodeUrl = qrCodeUrl,
                        RestaurantSettingsId = settings.RestaurantSettingsId,
                        PublicBookingSlug = settings.PublicBookingSlug
                    }
                };
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error generating QR code: " + ex.Message,
                    Data = new { }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Generates and saves QR code for restaurant public booking page (similar to GenerateTableQrCode)
        /// </summary>
        private string GeneratePublicBookingQrCode(long restaurantSettingsId, string publicBookingSlug, string domain, string existingRelativePath = null)
        {
            if (restaurantSettingsId <= 0 || string.IsNullOrWhiteSpace(publicBookingSlug))
            {
                return existingRelativePath;
            }

            var bookingUrl = BuildPublicBookingUrl(publicBookingSlug, domain);
            if (string.IsNullOrWhiteSpace(bookingUrl))
            {
                return existingRelativePath;
            }

            var directoryPath = HostingEnvironment.MapPath("~/ExternalContents/Images/RestaurantSettingsQRCode/");
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return existingRelativePath;
            }

            Directory.CreateDirectory(directoryPath);

            var sanitizedSlug = SanitizeFileNameSegment(publicBookingSlug);
            if (string.IsNullOrWhiteSpace(sanitizedSlug))
            {
                sanitizedSlug = "booking";
            }

            var fileName = string.Format("restaurant-{0}-{1}.png", sanitizedSlug, restaurantSettingsId);
            var filePath = Path.Combine(directoryPath, fileName);

            var normalizedExistingPath = NormalizeRelativePath(existingRelativePath);
            if (!string.IsNullOrWhiteSpace(normalizedExistingPath))
            {
                var existingPhysicalPath = HostingEnvironment.MapPath(normalizedExistingPath);
                if (!string.IsNullOrWhiteSpace(existingPhysicalPath) &&
                    !existingPhysicalPath.Equals(filePath, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(existingPhysicalPath))
                {
                    File.Delete(existingPhysicalPath);
                }
            }

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(bookingUrl, QRCodeGenerator.ECCLevel.Q))
            using (var pngQrCode = new PngByteQRCode(qrCodeData))
            {
                var qrBytes = pngQrCode.GetGraphic(20);
                File.WriteAllBytes(filePath, qrBytes);
            }

            return "/ExternalContents/Images/RestaurantSettingsQRCode/" + fileName;
        }

        /// <summary>
        /// Gets the expected file path for a restaurant's public booking QR code
        /// </summary>
        private string GetPublicBookingQrCodeFilePath(long restaurantSettingsId, string publicBookingSlug)
        {
            if (restaurantSettingsId <= 0 || string.IsNullOrWhiteSpace(publicBookingSlug))
            {
                return null;
            }

            var sanitizedSlug = SanitizeFileNameSegment(publicBookingSlug);
            if (string.IsNullOrWhiteSpace(sanitizedSlug))
            {
                sanitizedSlug = "booking";
            }

            var fileName = string.Format("restaurant-{0}-{1}.png", sanitizedSlug, restaurantSettingsId);
            return "/ExternalContents/Images/RestaurantSettingsQRCode/" + fileName;
        }

        /// <summary>
        /// Builds the URL for the public booking page
        /// </summary>
        private string BuildPublicBookingUrl(string publicBookingSlug, string domain)
        {
            if (string.IsNullOrWhiteSpace(publicBookingSlug))
            {
                return null;
            }

            string resolvedDomain = domain;

            if (string.IsNullOrWhiteSpace(resolvedDomain))
            {
                var request = HttpContext.Current?.Request;
                if (request != null)
                {
                    resolvedDomain = request.Url?.GetLeftPart(UriPartial.Authority);
                }
            }
            else
            {
                resolvedDomain = resolvedDomain.Trim();
                resolvedDomain = resolvedDomain.TrimEnd('/');

                if (!resolvedDomain.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (resolvedDomain.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ||
                        resolvedDomain.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                        resolvedDomain.StartsWith("::1", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedDomain = "http://" + resolvedDomain;
                    }
                    else
                    {
                        resolvedDomain = "https://" + resolvedDomain;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(resolvedDomain))
            {
                return "/book/" + publicBookingSlug;
            }

            return resolvedDomain + "/book/" + publicBookingSlug;
        }

        /// <summary>
        /// Normalizes a relative path
        /// </summary>
        private static string NormalizeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var trimmed = relativePath.Trim();

            if (trimmed.StartsWith("~", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(1);
            }

            trimmed = trimmed.TrimStart('/');

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            return "~/" + trimmed;
        }

        /// <summary>
        /// Sanitizes a string to be used as a file name segment
        /// </summary>
        private static string SanitizeFileNameSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray();
            var sanitized = new string(chars);

            while (sanitized.Contains("--"))
            {
                sanitized = sanitized.Replace("--", "-");
            }

            return sanitized.Trim('-');
        }

        /// <summary>
        /// Parses a time string (HH:MM format) to TimeSpan?
        /// </summary>
        private static TimeSpan? ParseTimeSpan(object timeValue)
        {
            if (timeValue == null)
            {
                return null;
            }

            // Handle TimeSpan objects directly
            if (timeValue is TimeSpan)
            {
                return (TimeSpan)timeValue;
            }

            string timeString = timeValue.ToString();
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return null;
            }

            // Try parsing as TimeSpan (handles "HH:MM:SS" format)
            if (TimeSpan.TryParse(timeString, out TimeSpan result))
            {
                return result;
            }

            // Try parsing "HH:MM" format explicitly
            var parts = timeString.Split(':');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
                {
                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                    {
                        return new TimeSpan(hours, minutes, 0);
                    }
                }
            }

            return null;
        }

        public async Task<IHttpActionResult> RestaurantSettingsActiveInactive(ClsRestaurantSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                
                // Get the existing entity and update it directly
                var restaurantSetting = oConnectionContext.DbClsRestaurantSettings
                    .Where(rs => rs.RestaurantSettingsId == obj.RestaurantSettingsId && rs.CompanyId == obj.CompanyId)
                    .FirstOrDefault();
                
                if (restaurantSetting == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Restaurant Settings not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
                
                // Get branch name for activity log
                var branchName = oConnectionContext.DbClsBranch
                    .Where(b => b.BranchId == restaurantSetting.BranchId && b.CompanyId == obj.CompanyId)
                    .Select(b => b.Branch)
                    .FirstOrDefault() ?? "Restaurant";
                
                // Update the tracked entity
                restaurantSetting.IsActive = obj.IsActive;
                restaurantSetting.ModifiedBy = obj.AddedBy;
                restaurantSetting.ModifiedOn = CurrentDate;
                
                oConnectionContext.Entry(restaurantSetting).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(restaurantSetting).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(restaurantSetting).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Restaurant Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Restaurant Settings for \"" + branchName + "\" " + (obj.IsActive == true ? "activated" : "deactivated"),
                    Id = restaurantSetting.RestaurantSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Restaurant Settings " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}


