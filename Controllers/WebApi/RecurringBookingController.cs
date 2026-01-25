using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Newtonsoft.Json;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class RecurringBookingController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        [HttpPost]
        public async Task<IHttpActionResult> InsertRecurringBooking(ClsRecurringBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsTableBooking templateBooking = null;
                
                // If BookingId is provided, validate and get template booking
                if (obj.BookingId > 0)
                {
                    templateBooking = oConnectionContext.DbClsTableBooking
                        .Where(a => a.BookingId == obj.BookingId && 
                               a.CompanyId == obj.CompanyId && 
                               a.IsDeleted == false)
                        .FirstOrDefault();

                    if (templateBooking == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Booking not found",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }
                else
                {
                    // Validate booking details are provided when no template booking
                    if (obj.CustomerId <= 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Customer is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (string.IsNullOrEmpty(obj.BookingTimeString) && !obj.BookingTime.HasValue)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Booking time is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (obj.NoOfGuests <= 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Number of guests is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (obj.TableIds == null || !obj.TableIds.Any())
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "At least one table is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // Parse booking time if provided as string
                TimeSpan? bookingTime = null;
                if (templateBooking != null)
                {
                    bookingTime = templateBooking.BookingTime;
                }
                else if (!string.IsNullOrEmpty(obj.BookingTimeString))
                {
                    if (TimeSpan.TryParse(obj.BookingTimeString, out TimeSpan parsedTime))
                    {
                        bookingTime = parsedTime;
                    }
                    else if (obj.BookingTime.HasValue)
                    {
                        bookingTime = obj.BookingTime;
                    }
                }
                else if (obj.BookingTime.HasValue)
                {
                    bookingTime = obj.BookingTime;
                }

                // Table IDs will be stored in junction table (tblRecurringBookingTable)

                // Apply timezone adjustment to StartDate and EndDate (same as TableBookingController)
                var adjustedStartDate = obj.StartDate.AddHours(5).AddMinutes(30);
                //adjustedStartDate = adjustedStartDate.Date;
                
                DateTime adjustedEndDate = DateTime.MinValue;
                if (!obj.IsNeverExpires && obj.EndDate != DateTime.MinValue)
                {
                    adjustedEndDate = obj.EndDate.AddHours(5).AddMinutes(30);
                    //adjustedEndDate = adjustedEndDate.Date;
                }

                ClsRecurringBooking oRecurringBooking = new ClsRecurringBooking()
                {
                    BookingId = obj.BookingId,
                    CustomerId = templateBooking != null ? templateBooking.CustomerId : obj.CustomerId,
                    BookingTime = bookingTime,
                    Duration = templateBooking != null ? templateBooking.Duration : (obj.Duration > 0 ? obj.Duration : 120),
                    NoOfGuests = templateBooking != null ? templateBooking.NoOfGuests : obj.NoOfGuests,
                    BranchId = templateBooking != null ? templateBooking.BranchId : obj.BranchId,
                    FloorId = obj.FloorId > 0 ? obj.FloorId : 0,
                    SpecialRequest = templateBooking != null ? templateBooking.SpecialRequest : obj.SpecialRequest,
                    RecurrenceType = obj.RecurrenceType ?? "Weekly",
                    RepeatEveryNumber = obj.RepeatEveryNumber > 0 ? obj.RepeatEveryNumber : 1,
                    RepeatEvery = obj.RepeatEvery,
                    DayOfMonth = obj.DayOfMonth,
                    StartDate = adjustedStartDate,
                    EndDate = adjustedEndDate,
                    IsNeverExpires = obj.IsNeverExpires,
                    IsActive = obj.IsActive,
                    CompanyId = obj.CompanyId,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate
                };

                oConnectionContext.DbClsRecurringBooking.Add(oRecurringBooking);
                oConnectionContext.SaveChanges();

                // Save table IDs to junction table if no template booking
                if (templateBooking == null && obj.TableIds != null && obj.TableIds.Any())
                {
                    // Add new table assignments
                    int displayOrder = 0;
                    foreach (var tableId in obj.TableIds)
                    {
                        var recurringBookingTable = new ClsRecurringBookingTable
                        {
                            RecurringBookingId = oRecurringBooking.RecurringBookingId,
                            TableId = tableId,
                            IsPrimary = displayOrder == 0,
                            DisplayOrder = displayOrder++,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsRecurringBookingTable.Add(recurringBookingTable);
                    }
                    oConnectionContext.SaveChanges();
                }

                // Save days of week to junction table for Weekly recurrence type
                if (obj.RecurrenceType == "Weekly" && obj.DaysOfWeek != null && obj.DaysOfWeek.Any())
                {
                    // Add new days
                    int displayOrder = 0;
                    foreach (var dayOfWeek in obj.DaysOfWeek.OrderBy(d => d))
                    {
                        // Validate day of week (0-6)
                        if (dayOfWeek >= 0 && dayOfWeek <= 6)
                        {
                            var recurringBookingDay = new ClsRecurringBookingDay
                            {
                                RecurringBookingId = oRecurringBooking.RecurringBookingId,
                                DayOfWeek = dayOfWeek,
                                DisplayOrder = displayOrder++,
                                AddedOn = CurrentDate
                            };
                            oConnectionContext.DbClsRecurringBookingDay.Add(recurringBookingDay);
                        }
                    }
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser ?? "",
                    Category = "RecurringBooking",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Booking created",
                    Id = oRecurringBooking.RecurringBookingId,
                    IpAddress = obj.IpAddress ?? "",
                    Platform = obj.Platform ?? "",
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Booking created successfully",
                    Data = new
                    {
                        //RecurringBooking = oRecurringBooking
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> UpdateRecurringBooking(ClsRecurringBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Check if recurring booking exists
                var existing = oConnectionContext.DbClsRecurringBooking
                    .Where(a => a.RecurringBookingId == obj.RecurringBookingId && a.CompanyId == obj.CompanyId)
                    .FirstOrDefault();

                if (existing == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Recurring Booking not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsTableBooking templateBooking = null;
                
                // If BookingId is provided, validate and get template booking
                if (obj.BookingId > 0)
                {
                    templateBooking = oConnectionContext.DbClsTableBooking
                        .Where(a => a.BookingId == obj.BookingId && 
                               a.CompanyId == obj.CompanyId && 
                               a.IsDeleted == false)
                        .FirstOrDefault();

                    if (templateBooking == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Booking not found",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }
                else
                {
                    // Validate booking details are provided when no template booking
                    if (obj.CustomerId <= 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Customer is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (string.IsNullOrEmpty(obj.BookingTimeString) && !obj.BookingTime.HasValue)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Booking time is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (obj.NoOfGuests <= 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Number of guests is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (obj.TableIds == null || !obj.TableIds.Any())
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "At least one table is required",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // Parse booking time if provided as string
                TimeSpan? bookingTime = null;
                if (templateBooking != null)
                {
                    bookingTime = templateBooking.BookingTime;
                }
                else if (!string.IsNullOrEmpty(obj.BookingTimeString))
                {
                    if (TimeSpan.TryParse(obj.BookingTimeString, out TimeSpan parsedTime))
                    {
                        bookingTime = parsedTime;
                    }
                    else if (obj.BookingTime.HasValue)
                    {
                        bookingTime = obj.BookingTime;
                    }
                }
                else if (obj.BookingTime.HasValue)
                {
                    bookingTime = obj.BookingTime;
                }

                // Apply timezone adjustment to StartDate and EndDate (same as TableBookingController)
                var adjustedStartDate = obj.StartDate.AddHours(5).AddMinutes(30);
                //adjustedStartDate = adjustedStartDate.Date;
                
                DateTime adjustedEndDate = DateTime.MinValue;
                if (!obj.IsNeverExpires && obj.EndDate != DateTime.MinValue)
                {
                    adjustedEndDate = obj.EndDate.AddHours(5).AddMinutes(30);
                    //adjustedEndDate = adjustedEndDate.Date;
                }

                // Update existing record
                existing.BookingId = obj.BookingId;
                existing.CustomerId = templateBooking != null ? templateBooking.CustomerId : obj.CustomerId;
                existing.BookingTime = bookingTime;
                existing.Duration = templateBooking != null ? templateBooking.Duration : (obj.Duration > 0 ? obj.Duration : 120);
                existing.NoOfGuests = templateBooking != null ? templateBooking.NoOfGuests : obj.NoOfGuests;
                existing.BranchId = templateBooking != null ? templateBooking.BranchId : obj.BranchId;
                existing.FloorId = obj.FloorId > 0 ? obj.FloorId : 0;
                existing.SpecialRequest = templateBooking != null ? templateBooking.SpecialRequest : obj.SpecialRequest;
                existing.RecurrenceType = obj.RecurrenceType ?? "Weekly";
                existing.RepeatEveryNumber = obj.RepeatEveryNumber > 0 ? obj.RepeatEveryNumber : 1;
                existing.RepeatEvery = obj.RepeatEvery;
                existing.DayOfMonth = obj.DayOfMonth;
                existing.StartDate = adjustedStartDate;
                existing.EndDate = adjustedEndDate;
                existing.IsNeverExpires = obj.IsNeverExpires;
                existing.IsActive = obj.IsActive;

                oConnectionContext.SaveChanges();

                // Save table IDs to junction table if no template booking
                if (templateBooking == null && obj.TableIds != null && obj.TableIds.Any())
                {
                    // Remove existing table assignments for this recurring booking
                    var existingTables = oConnectionContext.DbClsRecurringBookingTable
                        .Where(rt => rt.RecurringBookingId == existing.RecurringBookingId)
                        .ToList();
                    if (existingTables.Any())
                    {
                        oConnectionContext.DbClsRecurringBookingTable.RemoveRange(existingTables);
                        oConnectionContext.SaveChanges();
                    }

                    // Add new table assignments
                    int displayOrder = 0;
                    foreach (var tableId in obj.TableIds)
                    {
                        var recurringBookingTable = new ClsRecurringBookingTable
                        {
                            RecurringBookingId = existing.RecurringBookingId,
                            TableId = tableId,
                            IsPrimary = displayOrder == 0,
                            DisplayOrder = displayOrder++,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsRecurringBookingTable.Add(recurringBookingTable);
                    }
                    oConnectionContext.SaveChanges();
                }

                // Save days of week to junction table for Weekly recurrence type
                if (obj.RecurrenceType == "Weekly" && obj.DaysOfWeek != null && obj.DaysOfWeek.Any())
                {
                    // Remove existing days for this recurring booking
                    var existingDays = oConnectionContext.DbClsRecurringBookingDay
                        .Where(rd => rd.RecurringBookingId == existing.RecurringBookingId)
                        .ToList();
                    if (existingDays.Any())
                    {
                        oConnectionContext.DbClsRecurringBookingDay.RemoveRange(existingDays);
                        oConnectionContext.SaveChanges();
                    }

                    // Add new days
                    int displayOrder = 0;
                    foreach (var dayOfWeek in obj.DaysOfWeek.OrderBy(d => d))
                    {
                        // Validate day of week (0-6)
                        if (dayOfWeek >= 0 && dayOfWeek <= 6)
                        {
                            var recurringBookingDay = new ClsRecurringBookingDay
                            {
                                RecurringBookingId = existing.RecurringBookingId,
                                DayOfWeek = dayOfWeek,
                                DisplayOrder = displayOrder++,
                                AddedOn = CurrentDate
                            };
                            oConnectionContext.DbClsRecurringBookingDay.Add(recurringBookingDay);
                        }
                    }
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser ?? "",
                    Category = "RecurringBooking",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Booking updated",
                    Id = existing.RecurringBookingId,
                    IpAddress = obj.IpAddress ?? "",
                    Platform = obj.Platform ?? "",
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Booking updated successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetRecurringBookings(ClsRecurringBookingVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsRecurringBooking
                .Where(a => a.CompanyId == obj.CompanyId)
                .Select(a => new
                {
                    RecurringBookingId = a.RecurringBookingId,
                    BookingId = a.BookingId,
                    RecurrenceType = a.RecurrenceType,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    RepeatEvery = a.RepeatEvery,
                    DayOfMonth = a.DayOfMonth,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsNeverExpires = a.IsNeverExpires,
                    IsActive = a.IsActive,
                    CompanyId = a.CompanyId,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn
                })
                .ToList();

            // Apply search filter if provided
            var searchText = obj.SearchText ?? obj.Search ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                det = det.Where(a => 
                    (a.RecurrenceType != null && a.RecurrenceType.ToLower().Contains(searchText.ToLower()))
                ).ToList();
            }

            var totalCount = det.Count();
            var activeCount = det.Where(a => a.IsActive == true).Count();
            var inactiveCount = det.Where(a => a.IsActive == false).Count();
            var pagedDet = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList();

            // Load DaysOfWeek for Weekly recurrence types
            var recurringBookingIds = pagedDet.Select(a => a.RecurringBookingId).ToList();
            var daysOfWeekData = oConnectionContext.DbClsRecurringBookingDay
                .Where(rd => recurringBookingIds.Contains(rd.RecurringBookingId))
                .GroupBy(rd => rd.RecurringBookingId)
                .Select(g => new
                {
                    RecurringBookingId = g.Key,
                    DaysOfWeek = g.OrderBy(rd => rd.DisplayOrder).ThenBy(rd => rd.DayOfWeek).Select(rd => rd.DayOfWeek).ToList()
                })
                .ToDictionary(x => x.RecurringBookingId, x => x.DaysOfWeek);

            // Add DaysOfWeek to each item
            var result = pagedDet.Select(a => new
            {
                a.RecurringBookingId,
                a.BookingId,
                a.RecurrenceType,
                a.RepeatEveryNumber,
                a.RepeatEvery,
                a.DayOfMonth,
                a.StartDate,
                a.EndDate,
                a.IsNeverExpires,
                a.IsActive,
                a.CompanyId,
                a.AddedBy,
                a.AddedOn,
                DaysOfWeek = daysOfWeekData.ContainsKey(a.RecurringBookingId) ? daysOfWeekData[a.RecurringBookingId] : new List<int>()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RecurringBookings = result,
                    TotalCount = totalCount,
                    ActiveCount = activeCount,
                    InactiveCount = inactiveCount,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetRecurringBooking(ClsRecurringBookingVm obj)
        {
            var det = oConnectionContext.DbClsRecurringBooking
                .Where(a => a.RecurringBookingId == obj.RecurringBookingId && a.CompanyId == obj.CompanyId)
                .Select(a => new
                {
                    RecurringBookingId = a.RecurringBookingId,
                    BookingId = a.BookingId,
                    CustomerId = a.CustomerId,
                    BookingTime = a.BookingTime,
                    Duration = a.Duration,
                    NoOfGuests = a.NoOfGuests,
                    BranchId = a.BranchId,
                    FloorId = a.FloorId,
                    SpecialRequest = a.SpecialRequest,
                    RecurrenceType = a.RecurrenceType,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    RepeatEvery = a.RepeatEvery,
                    DayOfMonth = a.DayOfMonth,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsNeverExpires = a.IsNeverExpires,
                    IsActive = a.IsActive,
                    CompanyId = a.CompanyId,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    BookingNo = oConnectionContext.DbClsTableBooking
                        .Where(b => b.BookingId == a.BookingId)
                        .Select(b => b.BookingNo)
                        .FirstOrDefault(),
                    Booking = oConnectionContext.DbClsTableBooking
                        .Where(b => b.BookingId == a.BookingId && b.IsDeleted == false)
                        .Select(b => new
                        {
                            b.BookingId,
                            b.BookingNo,
                            b.BookingDate,
                            b.BookingTime,
                            b.Duration,
                            b.NoOfGuests,
                            b.Status,
                            b.SpecialRequest
                        })
                        .FirstOrDefault(),
                    TableIds = oConnectionContext.DbClsRecurringBookingTable
                        .Where(rt => rt.RecurringBookingId == a.RecurringBookingId)
                        .Select(rt => rt.TableId)
                        .ToList(),
                    DaysOfWeek = oConnectionContext.DbClsRecurringBookingDay
                        .Where(rd => rd.RecurringBookingId == a.RecurringBookingId)
                        .OrderBy(rd => rd.DisplayOrder)
                        .ThenBy(rd => rd.DayOfWeek)
                        .Select(rd => rd.DayOfWeek)
                        .ToList()
                })
                .FirstOrDefault();

            if (det == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Recurring Booking not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Convert TimeSpan? to string format (hh:mm) to avoid JSON deserialization issues
            var convertedDet = new
            {
                det.RecurringBookingId,
                det.BookingId,
                det.CustomerId,
                BookingTime = det.BookingTime.HasValue ? det.BookingTime.Value.ToString(@"hh\:mm") : (string)null,
                det.Duration,
                det.NoOfGuests,
                det.BranchId,
                det.FloorId,
                det.SpecialRequest,
                det.RecurrenceType,
                det.RepeatEveryNumber,
                det.RepeatEvery,
                det.DayOfMonth,
                det.StartDate,
                det.EndDate,
                det.IsNeverExpires,
                det.IsActive,
                det.CompanyId,
                det.AddedBy,
                det.AddedOn,
                det.BookingNo,
                Booking = det.Booking != null ? new
                {
                    det.Booking.BookingId,
                    det.Booking.BookingNo,
                    det.Booking.BookingDate,
                    BookingTime = det.Booking.BookingTime.ToString(@"hh\:mm"),
                    det.Booking.Duration,
                    det.Booking.NoOfGuests,
                    det.Booking.Status,
                    det.Booking.SpecialRequest
                } : (object)null,
                det.TableIds,
                det.DaysOfWeek
            };

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RecurringBooking = convertedDet
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> RecurringBookingActiveInactive(ClsRecurringBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var existing = oConnectionContext.DbClsRecurringBooking
                    .Where(a => a.RecurringBookingId == obj.RecurringBookingId && a.CompanyId == obj.CompanyId)
                    .FirstOrDefault();

                if (existing == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Recurring Booking not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                existing.IsActive = obj.IsActive;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser ?? "",
                    Category = "RecurringBooking",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Booking " + (obj.IsActive == true ? "activated" : "deactivated"),
                    Id = obj.RecurringBookingId,
                    IpAddress = obj.IpAddress ?? "",
                    Platform = obj.Platform ?? "",
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Booking " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> DeleteRecurringBooking(ClsRecurringBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var existing = oConnectionContext.DbClsRecurringBooking
                    .Where(a => a.RecurringBookingId == obj.RecurringBookingId && a.CompanyId == obj.CompanyId)
                    .FirstOrDefault();

                if (existing == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Recurring Booking not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                oConnectionContext.DbClsRecurringBooking.Remove(existing);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser ?? "",
                    Category = "RecurringBooking",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Booking deleted",
                    Id = obj.RecurringBookingId,
                    IpAddress = obj.IpAddress ?? "",
                    Platform = obj.Platform ?? "",
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Booking deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Generate bookings from recurring patterns (should be called by background service/job)
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GenerateRecurringBookings(ClsRecurringBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            var generatedCount = 0;
            var errors = new List<string>();

            // Get all active recurring bookings
            var recurringBookings = oConnectionContext.DbClsRecurringBooking
                .Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true)
                .ToList();

            foreach (var recurring in recurringBookings)
            {
                try
                {
                    // Check if we should generate bookings for this recurring pattern
                    if (!recurring.IsNeverExpires && recurring.EndDate < CurrentDate.Date)
                    {
                        continue; // Recurring pattern has ended
                    }

                    if (recurring.StartDate > CurrentDate.Date.AddDays(30))
                    {
                        continue; // Start date is too far in the future
                    }

                    // Get the template booking or use stored details
                    ClsTableBooking templateBooking = null;
                    List<long> storedTableIds = new List<long>();

                    if (recurring.BookingId > 0)
                    {
                        templateBooking = oConnectionContext.DbClsTableBooking
                            .Where(b => b.BookingId == recurring.BookingId && b.IsDeleted == false)
                            .FirstOrDefault();

                        if (templateBooking == null)
                        {
                            errors.Add($"Template booking {recurring.BookingId} not found for recurring booking {recurring.RecurringBookingId}");
                            continue;
                        }
                    }
                    else
                    {
                        // Get table IDs from junction table
                        storedTableIds = oConnectionContext.DbClsRecurringBookingTable
                            .Where(rt => rt.RecurringBookingId == recurring.RecurringBookingId)
                            .Select(rt => rt.TableId)
                            .ToList();
                    }

                    // Get days of week from junction table for Weekly recurrence type
                    List<int> daysOfWeek = null;
                    if (recurring.RecurrenceType == "Weekly")
                    {
                        daysOfWeek = oConnectionContext.DbClsRecurringBookingDay
                            .Where(rd => rd.RecurringBookingId == recurring.RecurringBookingId)
                            .OrderBy(rd => rd.DisplayOrder)
                            .ThenBy(rd => rd.DayOfWeek)
                            .Select(rd => rd.DayOfWeek)
                            .ToList();
                        
                        // If no days in table, use start date's day of week
                        if (!daysOfWeek.Any())
                        {
                            daysOfWeek = new List<int> { (int)recurring.StartDate.DayOfWeek };
                        }
                    }

                    // Generate bookings based on recurrence type using normalized columns
                    var datesToGenerate = GetRecurrenceDates(recurring.RecurrenceType, recurring.StartDate, recurring.EndDate, 
                        recurring.IsNeverExpires, recurring.RepeatEveryNumber, recurring.RepeatEvery, recurring.DayOfMonth, CurrentDate, daysOfWeek);

                    // Get booking time and other details
                    TimeSpan bookingTime = templateBooking != null ? templateBooking.BookingTime : (recurring.BookingTime ?? TimeSpan.FromHours(19));
                    long customerId = templateBooking != null ? templateBooking.CustomerId : recurring.CustomerId;
                    int duration = templateBooking != null ? templateBooking.Duration : (recurring.Duration > 0 ? recurring.Duration : 120);
                    int noOfGuests = templateBooking != null ? templateBooking.NoOfGuests : recurring.NoOfGuests;
                    long branchId = templateBooking != null ? templateBooking.BranchId : recurring.BranchId;
                    string specialRequest = templateBooking != null ? templateBooking.SpecialRequest : recurring.SpecialRequest;
                    
                    // Calculate deposit amount dynamically based on restaurant settings (not stored value)
                    // This ensures deposit is calculated with current settings and actual guest count
                    decimal depositAmount = 0;
                    var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                        .Where(rs => rs.CompanyId == recurring.CompanyId && rs.BranchId == branchId && rs.IsDeleted == false)
                        .FirstOrDefault();
                    
                    if (restaurantSettings != null && restaurantSettings.RequireDeposit)
                    {
                        var depositMode = restaurantSettings.DepositMode ?? "Fixed";
                        if (depositMode.Equals("PerGuest", StringComparison.OrdinalIgnoreCase))
                        {
                            depositAmount = noOfGuests * restaurantSettings.DepositPerGuestAmount;
                        }
                        else
                        {
                            depositAmount = restaurantSettings.DepositFixedAmount;
                        }
                    }
                    // If deposit is not required, depositAmount remains 0
                    
                    // DepositPaid is now calculated dynamically from payments, not stored
                    long addedBy = templateBooking != null ? templateBooking.AddedBy : recurring.AddedBy;

                    // Get table IDs
                    List<long> tableIds = new List<long>();
                    if (templateBooking != null)
                    {
                        var templateTables = oConnectionContext.DbClsTableBookingTable
                            .Where(bt => bt.BookingId == templateBooking.BookingId)
                            .Select(bt => bt.TableId)
                            .ToList();
                        tableIds = templateTables;
                    }
                    else
                    {
                        tableIds = storedTableIds;
                    }

                    if (customerId <= 0 || noOfGuests <= 0 || tableIds.Count == 0)
                    {
                        errors.Add($"Invalid booking details for recurring booking {recurring.RecurringBookingId}");
                        continue;
                    }

                    foreach (var bookingDate in datesToGenerate)
                    {
                        // Check if booking already exists for this date/time from this recurring booking
                        var existingBooking = oConnectionContext.DbClsTableBooking
                            .Where(b => b.RecurringBookingId == recurring.RecurringBookingId &&
                                   b.BookingDate.Date == bookingDate.Date &&
                                   b.BookingTime == bookingTime &&
                                   b.IsDeleted == false)
                            .FirstOrDefault();

                        if (existingBooking == null)
                        {
                            // Create new booking
                            var newBooking = new ClsTableBooking
                            {
                                BookingNo = GenerateBookingNo(recurring.CompanyId, branchId),
                                CustomerId = customerId,
                                BookingDate = bookingDate.Date,
                                BookingTime = bookingTime,
                                Duration = duration,
                                NoOfGuests = noOfGuests,
                                Status = "Pending",
                                BookingType = "Recurring",
                                SpecialRequest = specialRequest,
                                DepositAmount = depositAmount,
                                ReminderSent = false,
                                BranchId = branchId,
                                CompanyId = recurring.CompanyId,
                                RecurringBookingId = recurring.RecurringBookingId,
                                IsActive = true,
                                IsDeleted = false,
                                AddedBy = addedBy,
                                AddedOn = CurrentDate,
                                ModifiedBy = addedBy
                            };

                            oConnectionContext.DbClsTableBooking.Add(newBooking);
                            oConnectionContext.SaveChanges();

                            // Add table assignments
                            int displayOrder = 0;
                            foreach (var tableId in tableIds)
                            {
                                var newBookingTable = new ClsTableBookingTable
                                {
                                    BookingId = newBooking.BookingId,
                                    TableId = tableId,
                                    IsPrimary = displayOrder == 0,
                                    DisplayOrder = displayOrder++,
                                    AddedOn = CurrentDate
                                };
                                oConnectionContext.DbClsTableBookingTable.Add(newBookingTable);
                            }

                            oConnectionContext.SaveChanges();
                            generatedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing recurring booking {recurring.RecurringBookingId}: {ex.Message}");
                }
            }

            data = new
            {
                Status = 1,
                Message = $"Generated {generatedCount} bookings from recurring patterns",
                Data = new
                {
                    GeneratedCount = generatedCount,
                    Errors = errors
                }
            };

            return await Task.FromResult(Ok(data));
        }

        private List<DateTime> GetRecurrenceDates(string recurrenceType, DateTime startDate, DateTime endDate, 
            bool isNeverExpires, int repeatEveryNumber, string repeatEvery, int? dayOfMonth, DateTime currentDate, List<int> daysOfWeek = null)
        {
            var dates = new List<DateTime>();
            var checkEndDate = isNeverExpires ? currentDate.Date.AddDays(30) : endDate;
            var checkStartDate = currentDate.Date;

            // Ensure repeatEveryNumber is at least 1
            var interval = repeatEveryNumber;
            if (interval < 1) interval = 1;

            if (recurrenceType == "Daily")
            {
                // Start from the later of startDate or checkStartDate to avoid generating past dates
                var effectiveStartDate = startDate.Date > checkStartDate ? startDate.Date : checkStartDate;
                
                // Find the first valid date that matches the interval pattern
                if (effectiveStartDate <= checkEndDate)
                {
                    // Calculate days since original start date to determine if we're on an interval boundary
                    var daysSinceStart = (effectiveStartDate - startDate.Date).Days;
                    var remainder = daysSinceStart % interval;
                    
                    // If we're not on an interval boundary, move to the next valid date
                    if (remainder != 0)
                    {
                        effectiveStartDate = effectiveStartDate.AddDays(interval - remainder);
                    }
                    
                    // Generate dates from effective start date
                    for (var date = effectiveStartDate; date <= checkEndDate; date = date.AddDays(interval))
                    {
                        dates.Add(date);
                    }
                }
            }
            else if (recurrenceType == "Weekly")
            {
                // Use provided daysOfWeek list, or use start date's day
                if (daysOfWeek == null || !daysOfWeek.Any())
                {
                    daysOfWeek = new List<int> { (int)startDate.DayOfWeek };
                }

                var weekStart = startDate.Date;
                // Start from the later of startDate or checkStartDate to avoid generating past dates
                var effectiveStartDate = startDate.Date > checkStartDate ? startDate.Date : checkStartDate;

                for (var date = effectiveStartDate; date <= checkEndDate; date = date.AddDays(1))
                {
                    if (daysOfWeek.Contains((int)date.DayOfWeek))
                    {
                        // Check if this date falls within the interval weeks
                        var weeksSinceStart = (int)((date - weekStart).TotalDays / 7);
                        if (weeksSinceStart >= 0 && weeksSinceStart % interval == 0)
                        {
                            dates.Add(date);
                        }
                    }
                }
            }
            else if (recurrenceType == "Monthly")
            {
                // Use DayOfMonth from column, or fallback to start date's day
                var targetDayOfMonth = dayOfMonth ?? startDate.Day;

                // Start from the later of startDate or checkStartDate to avoid generating past dates
                var effectiveStartDate = startDate.Date > checkStartDate ? startDate.Date : checkStartDate;
                
                // Find the first month that's on or after effectiveStartDate and matches the interval
                var currentMonth = new DateTime(effectiveStartDate.Year, effectiveStartDate.Month, 1);
                var startMonth = new DateTime(startDate.Year, startDate.Month, 1);
                
                // Calculate months since start to find the first valid month in the interval
                var monthsSinceStart = (currentMonth.Year - startMonth.Year) * 12 + (currentMonth.Month - startMonth.Month);
                var remainder = monthsSinceStart % interval;
                
                // If we're not on an interval boundary, move to the next valid month
                if (remainder != 0)
                {
                    currentMonth = currentMonth.AddMonths(interval - remainder);
                }
                
                // Generate dates from the first valid month
                while (currentMonth <= checkEndDate)
                {
                    // Adjust day if month doesn't have that day
                    var targetDate = new DateTime(currentMonth.Year, currentMonth.Month, Math.Min(targetDayOfMonth, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month)));
                    if (targetDate >= checkStartDate && targetDate <= checkEndDate)
                    {
                        dates.Add(targetDate);
                    }
                    currentMonth = currentMonth.AddMonths(interval);
                }
            }
            else if (recurrenceType == "Yearly")
            {
                // Use DayOfMonth from column, or fallback to start date's day
                var targetDayOfMonth = dayOfMonth ?? startDate.Day;
                var targetMonth = startDate.Month;

                // Start from the later of startDate or checkStartDate to avoid generating past dates
                var effectiveStartDate = startDate.Date > checkStartDate ? startDate.Date : checkStartDate;
                
                // Find the first year that's on or after effectiveStartDate and matches the interval
                var currentYear = effectiveStartDate.Year;
                var startYear = startDate.Year;
                
                // Calculate years since start to find the first valid year in the interval
                var yearsSinceStart = currentYear - startYear;
                var remainder = yearsSinceStart % interval;
                
                // If we're not on an interval boundary, move to the next valid year
                if (remainder != 0)
                {
                    currentYear += (interval - remainder);
                }
                
                // Generate dates from the first valid year
                while (currentYear <= checkEndDate.Year)
                {
                    // Check if month and day match (handle cases where day doesn't exist in some years, e.g., Feb 29)
                    if (currentYear >= effectiveStartDate.Year)
                    {
                        var targetDate = new DateTime(currentYear, targetMonth, Math.Min(targetDayOfMonth, DateTime.DaysInMonth(currentYear, targetMonth)));
                        
                        // Only add if the date is on or after the effective start date and within the end date
                        if (targetDate >= checkStartDate && targetDate <= checkEndDate)
                        {
                            dates.Add(targetDate);
                        }
                    }
                    currentYear += interval;
                }
            }

            return dates.Distinct().OrderBy(d => d).ToList();
        }

        private string GenerateBookingNo(long companyId, long branchId)
        {
            long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == branchId).Select(a => a.PrefixId).FirstOrDefault();
            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                  join b in oConnectionContext.DbClsPrefixUserMap
                                   on a.PrefixMasterId equals b.PrefixMasterId
                                  where a.IsActive == true && a.IsDeleted == false &&
                                  b.CompanyId == companyId && b.IsActive == true
                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "booking"
                                  && b.PrefixId == PrefixId
                                  select new
                                  {
                                      b.PrefixUserMapId,
                                      b.Prefix,
                                      b.NoOfDigits,
                                      b.Counter
                                  }).FirstOrDefault();

            string bookingNo = "";
            long PrefixUserMapId = 0;
            if (prefixSettings != null)
            {
                PrefixUserMapId = prefixSettings.PrefixUserMapId;
                bookingNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                
                // Update counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
            }
            else
            {
                bookingNo = "BK" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsTableBooking.Where(a => a.CompanyId == companyId).Count().ToString().PadLeft(4, '0');
            }

            return bookingNo;
        }
    }
}

