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

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class TableBookingController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        TableStatusHelper tableStatusHelper = new TableStatusHelper();

        /// <summary>
        /// Lookup or create customer by mobile number
        /// </summary>
        private async Task<long> LookupOrCreateCustomer(string customerName, string customerMobile, string customerEmail, long companyId, long branchId, long addedBy)
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
                var userController = new UserController();
                var userVm = new ClsUserVm
                {
                    CompanyId = companyId,
                    AddedBy = addedBy,
                    UserType = "customer",
                    Name = customerName?.Trim() ?? "",
                    MobileNo = normalizedMobile,
                    EmailId = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim(),
                    IsActive = true,
                    IsDeleted = false,
                    JoiningDate = oCommonController.CurrentDate(companyId),
                    BranchId = branchId,
                    GstTreatment = "Consumer",
                    TaxPreference = "Taxable"
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
        [HttpPost]
        public async Task<IHttpActionResult> GetBookings(ClsTableBookingVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var query = oConnectionContext.DbClsTableBooking.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false);
            
            // Filter by BranchId only if provided
            if (obj.BranchId!= 0)
            {
                query = query.Where(a => a.BranchId == obj.BranchId);
            }

            if (obj.FromDate.HasValue)
            {
                query = query.Where(a => a.BookingDate >= obj.FromDate.Value.Date);
            }

            if (obj.ToDate.HasValue)
            {
                query = query.Where(a => a.BookingDate <= obj.ToDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(obj.Status))
            {
                query = query.Where(a => a.Status == obj.Status);
            }

            // Filter by TableId in junction table (if TableIds contains a single table ID, use it for filtering)
            // Note: For backward compatibility with existing filter, we can use TableIds[0] if only one table
            // This can be removed if filtering by single table is no longer needed
            // if (obj.TableIds != null && obj.TableIds.Count == 1)
            // {
            //     query = query.Where(a => 
            //         oConnectionContext.DbClsTableBookingTable.Any(bt => bt.BookingId == a.BookingId && bt.TableId == obj.TableIds[0]));
            // }

            var det = query.Select(a => new
            {
                BookingId = a.BookingId,
                BookingNo = a.BookingNo,
                CustomerId = a.CustomerId,
                CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                CustomerMobile = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                CustomerEmail = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.EmailId).FirstOrDefault(),
                BookingDate = a.BookingDate,
                BookingTime = a.BookingTime,
                Duration = a.Duration,
                NoOfGuests = a.NoOfGuests,
                Status = a.Status,
                BookingType = a.BookingType,
                SpecialRequest = a.SpecialRequest,
                DepositAmount = a.DepositAmount,
                DepositPaid = (oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == a.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Payment")
                    .Select(cp => cp.Amount)
                    .DefaultIfEmpty(0)
                    .Sum() -
                    oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == a.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Refund")
                    .Select(cp => cp.Amount)
                    .DefaultIfEmpty(0)
                    .Sum()) >= a.DepositAmount,
                ReminderSent = a.ReminderSent,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                ConfirmedOn = a.ConfirmedOn,
                CancelledOn = a.CancelledOn,
                CancellationReason = a.CancellationReason,
                SalesId = a.SalesId,
                WaiterId = a.WaiterId,
                WaiterName = a.WaiterId > 0 ? oConnectionContext.DbClsUser.Where(u => u.UserId == a.WaiterId).Select(u => u.Name).FirstOrDefault() : null,
                SalesNo = oConnectionContext.DbClsSales.Where(s => s.SalesId == a.SalesId).Select(s => s.InvoiceNo).FirstOrDefault(),
                KotNos = oConnectionContext.DbClsKotMaster.Where(k => k.BookingId == a.BookingId && !k.IsDeleted).Select(k => k.KotNo).ToList(),
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            // Enrich with multiple tables data
            var enrichedDet = det.Select(booking => 
            {
                var bookingId = booking.BookingId;
                var tableIds = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == bookingId)
                    .OrderBy(bt => bt.DisplayOrder)
                    .Select(bt => bt.TableId)
                    .ToList();
                
                var tableNos = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == bookingId)
                    .OrderBy(bt => bt.DisplayOrder)
                    .Join(oConnectionContext.DbClsRestaurantTable,
                        bt => bt.TableId,
                        t => t.TableId,
                        (bt, t) => t.TableNo)
                    .ToList();
                
                var tableNames = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == bookingId)
                    .OrderBy(bt => bt.DisplayOrder)
                    .Join(oConnectionContext.DbClsRestaurantTable,
                        bt => bt.TableId,
                        t => t.TableId,
                        (bt, t) => t.TableName ?? "")
                    .ToList();
                
                return new
                {
                    booking.BookingId,
                    booking.BookingNo,
                    TableId = tableIds.Any() ? (long?)tableIds.First() : null, // Primary table ID from junction table
                    booking.CustomerId,
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
                    booking.DepositPaid,
                    booking.ReminderSent,
                    booking.BranchId,
                    booking.CompanyId,
                    booking.IsActive,
                    booking.IsDeleted,
                    booking.AddedBy,
                    booking.AddedOn,
                    booking.ModifiedBy,
                    booking.ModifiedOn,
                    booking.ConfirmedOn,
                    booking.CancelledOn,
                    booking.CancellationReason,
                    booking.SalesId,
                    TableNo = tableNos.Any() ? tableNos.First() : null,
                    TableName = tableNames.Any() ? tableNames.First() : null,
                    booking.SalesNo,
                    KotNos = booking.KotNos,
                    booking.AddedByCode,
                    TableIds = tableIds,
                    TableNos = tableNos,
                    TableNames = tableNames
                };
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                enrichedDet = enrichedDet.Where(a => (a.BookingNo != null && a.BookingNo.ToLower().Contains(obj.Search.ToLower())) ||
                                     (a.CustomerName != null && a.CustomerName.ToLower().Contains(obj.Search.ToLower())) ||
                                     (a.CustomerMobile != null && a.CustomerMobile.Contains(obj.Search)) ||
                                     (a.TableNos != null && a.TableNos.Any() && string.Join(", ", a.TableNos).ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Bookings = enrichedDet.OrderByDescending(a => a.BookingDate).ThenByDescending(a => a.BookingTime).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = enrichedDet.Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TableBooking(ClsTableBookingVm obj)
        {
            var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == obj.BookingId && a.CompanyId == obj.CompanyId).FirstOrDefault();
            
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

            // Get tables from junction table
            var tableIds = oConnectionContext.DbClsTableBookingTable
                .Where(bt => bt.BookingId == booking.BookingId)
                .OrderBy(bt => bt.DisplayOrder)
                .Select(bt => bt.TableId)
                .ToList();

            var tableNos = oConnectionContext.DbClsTableBookingTable
                .Where(bt => bt.BookingId == booking.BookingId)
                .OrderBy(bt => bt.DisplayOrder)
                .Join(oConnectionContext.DbClsRestaurantTable,
                    bt => bt.TableId,
                    t => t.TableId,
                    (bt, t) => t.TableNo)
                .ToList();

            var tableNames = oConnectionContext.DbClsTableBookingTable
                .Where(bt => bt.BookingId == booking.BookingId)
                .OrderBy(bt => bt.DisplayOrder)
                .Join(oConnectionContext.DbClsRestaurantTable,
                    bt => bt.TableId,
                    t => t.TableId,
                    (bt, t) => t.TableName ?? "")
                .ToList();

            // Get FloorId from the first table (for pre-selecting floor in edit form)
            // Get the first table ID outside the query to avoid LINQ translation issues
            var firstTableId = tableIds.Any() ? tableIds.First() : 0;
            var floorId = firstTableId > 0 ? oConnectionContext.DbClsRestaurantTable
                .Where(t => t.TableId == firstTableId)
                .Select(t => t.FloorId)
                .FirstOrDefault() : 0;

            // Get customer info
            var customer = booking.CustomerId > 0 ? oConnectionContext.DbClsUser
                .Where(u => u.UserId == booking.CustomerId)
                .Select(u => new { u.Name, u.MobileNo, u.EmailId })
                .FirstOrDefault() : null;

            // Get waiter info
            var waiter = booking.WaiterId > 0 ? oConnectionContext.DbClsUser
                .Where(u => u.UserId == booking.WaiterId)
                .Select(u => u.Name)
                .FirstOrDefault() : null;

            // Calculate DepositPaid dynamically
            decimal totalPaid = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.BookingId == booking.BookingId &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.Type == "Booking Deposit Payment")
                .Select(cp => cp.Amount)
                .DefaultIfEmpty(0)
                .Sum();

            decimal totalRefunded = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.BookingId == booking.BookingId &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.Type == "Booking Deposit Refund")
                .Select(cp => cp.Amount)
                .DefaultIfEmpty(0)
                .Sum();

            bool depositPaid = (totalPaid - totalRefunded) >= booking.DepositAmount;

            var det = new
            {
                BookingId = booking.BookingId,
                BookingNo = booking.BookingNo,
                TableId = tableIds.Any() ? (long?)tableIds.First() : null,
                CustomerId = booking.CustomerId,
                CustomerName = customer != null ? customer.Name : null,
                CustomerMobile = customer != null ? customer.MobileNo : null,
                CustomerEmail = customer != null ? customer.EmailId : null,
                BookingDate = booking.BookingDate,
                BookingTime = booking.BookingTime,
                BookingTimeString = string.Format("{0:D2}:{1:D2}", booking.BookingTime.Hours, booking.BookingTime.Minutes),
                Duration = booking.Duration,
                NoOfGuests = booking.NoOfGuests,
                Status = booking.Status,
                BookingType = booking.BookingType,
                SpecialRequest = booking.SpecialRequest,
                DepositAmount = booking.DepositAmount,
                DepositPaid = depositPaid,
                PaymentTransactionId = booking.PaymentTransactionId,
                PaymentGatewayType = booking.PaymentGatewayType,
                PaymentDate = booking.PaymentDate,
                ReminderSent = booking.ReminderSent,
                BranchId = booking.BranchId,
                CompanyId = booking.CompanyId,
                IsActive = booking.IsActive,
                IsDeleted = booking.IsDeleted,
                AddedBy = booking.AddedBy,
                AddedOn = booking.AddedOn,
                ModifiedBy = booking.ModifiedBy,
                ModifiedOn = booking.ModifiedOn,
                ConfirmedOn = booking.ConfirmedOn,
                CancelledOn = booking.CancelledOn,
                CancellationReason = booking.CancellationReason,
                CancellationCharge = booking.CancellationCharge,
                SalesId = booking.SalesId,
                WaiterId = booking.WaiterId,
                WaiterName = waiter,
                TableNo = tableNos.Any() ? tableNos.First() : null,
                TableName = tableNames.Any() ? tableNames.First() : null,
                SalesNo = oConnectionContext.DbClsSales.Where(s => s.SalesId == booking.SalesId).Select(s => s.InvoiceNo).FirstOrDefault(),
                KotNos = oConnectionContext.DbClsKotMaster.Where(k => k.BookingId == booking.BookingId && !k.IsDeleted).Select(k => k.KotNo).ToList(),
                TableIds = tableIds,
                TableNos = tableNos,
                TableNames = tableNames,
                FloorId = floorId
            };

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Booking = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CreateStandalone(ClsTableBookingVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            
            // Parse BookingTime from BookingTimeString (JSON sends it as "HH:mm" string)
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
                                obj.BookingTime = new TimeSpan(hours, minutes, 0);
                            }
                        }
                    }
                }
                catch
                {
                    // If parsing fails, validation will catch if it's invalid
                }
            }

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.NoOfGuests <= 0)
                {
                    errors.Add(new ClsError { Message = "Number of guests must be greater than 0", Id = "divNoOfGuests" });
                    isError = true;
                }

                if (obj.BookingDate == default(DateTime))
                {
                    errors.Add(new ClsError { Message = "Booking Date is required", Id = "divBookingDate" });
                    isError = true;
                }
                else
                {
                    obj.BookingDate = obj.BookingDate.AddHours(5).AddMinutes(30);
                    //obj.BookingDate = obj.BookingDate.Date;
                }

                // Validate BookingTime - ensure it's not default (00:00:00) unless explicitly set to midnight
                // Note: We allow 00:00:00 as a valid time (midnight), but we check if parsing might have failed
                if (obj.BookingTime == default(TimeSpan))
                {
                    // This might be a parsing issue - the fallback parser above should have handled it
                    // But if it's still default, it could be an actual issue
                    // For now, we'll allow it and let the business logic handle it
                }

                // Validate that at least one table is selected
                if (obj.TableIds == null || !obj.TableIds.Any())
                {
                    errors.Add(new ClsError { Message = "Please select at least one table", Id = "divTableId" });
                    isError = true;
                }

                List<long> tablesToValidate = obj.TableIds != null ? obj.TableIds.Distinct().ToList() : new List<long>();

                // Validate table availability and capacity for all selected tables
                if (tablesToValidate.Any())
                {
                    var bookingStart = obj.BookingDate.Date.Add(obj.BookingTime);
                    var bookingEnd = bookingStart.AddMinutes(obj.Duration);
                    int totalCapacity = 0;
                    List<string> unavailableTables = new List<string>();
                    List<string> lowCapacityTables = new List<string>();

                    foreach (var tableId in tablesToValidate)
                    {
                        // Check availability - exclude current booking if updating
                        long excludeBookingId = obj.BookingId > 0 ? obj.BookingId : 0;
                        if (!tableStatusHelper.IsTableAvailable(tableId, bookingStart, bookingEnd, excludeBookingId))
                        {
                            var table = oConnectionContext.DbClsRestaurantTable
                                .Where(t => t.TableId == tableId && t.IsActive && !t.IsDeleted)
                                .Select(t => t.TableNo)
                                .FirstOrDefault();
                            unavailableTables.Add(table ?? $"Table {tableId}");
                        }
                        else
                        {
                            // Get table capacity
                            var tableCapacity = oConnectionContext.DbClsRestaurantTable
                                .Where(t => t.TableId == tableId && t.IsActive && !t.IsDeleted)
                                .Select(t => t.Capacity)
                                .FirstOrDefault();
                            
                            if (tableCapacity > 0)
                            {
                                totalCapacity += tableCapacity;
                                
                                // Allow 20% overflow per table, but warn if exceeded
                                if (obj.NoOfGuests > tableCapacity && obj.NoOfGuests > (tableCapacity * 1.20))
                                {
                                    var tableNo = oConnectionContext.DbClsRestaurantTable
                                        .Where(t => t.TableId == tableId)
                                        .Select(t => t.TableNo)
                                        .FirstOrDefault();
                                    lowCapacityTables.Add($"{tableNo} (Capacity: {tableCapacity})");
                                }
                            }
                        }
                    }

                    if (unavailableTables.Any())
                    {
                        errors.Add(new ClsError 
                        { 
                            Message = $"Table(s) not available for the selected date and time: {string.Join(", ", unavailableTables)}", 
                            Id = "divTableId" 
                        });
                        isError = true;
                    }

                    // Warn if total capacity is insufficient (but don't block - allow manual override)
                    if (totalCapacity > 0 && obj.NoOfGuests > totalCapacity && obj.NoOfGuests > (totalCapacity * 1.20))
                    {
                        // This is a warning, not an error - allow booking but inform user
                        var warningMessage = $"Total guests ({obj.NoOfGuests}) exceeds combined table capacity ({totalCapacity}). ";
                        if (lowCapacityTables.Any())
                        {
                            warningMessage += $"Consider larger tables or combining more tables.";
                        }
                        // Store warning in SpecialRequest or handle separately
                        if (string.IsNullOrEmpty(obj.SpecialRequest))
                        {
                            obj.SpecialRequest = $"⚠️ Capacity Warning: {warningMessage}";
                        }
                        else
                        {
                            obj.SpecialRequest += $" | ⚠️ Capacity Warning: {warningMessage}";
                        }
                    }
                }

                if (isError == true)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "",
                        Errors = errors,
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Generate BookingNo if not provided
                long PrefixUserMapId = 0;
                if (obj.BookingId == 0)
                {
                    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
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
                        obj.BookingNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    }
                    else
                    {
                        // Fallback if prefix not configured
                        obj.BookingNo = "BK" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsTableBooking.Where(a => a.CompanyId == obj.CompanyId).Count().ToString().PadLeft(4, '0');
                    }
                }

                // Lookup or create customer if CustomerId is 0 but customer info is provided
                if (obj.CustomerId == 0 && !string.IsNullOrWhiteSpace(obj.CustomerMobile))
                {
                    obj.CustomerId = await LookupOrCreateCustomer(obj.CustomerName, obj.CustomerMobile, obj.CustomerEmail, obj.CompanyId, obj.BranchId, obj.AddedBy);
                }

                // Calculate deposit amount based on restaurant settings if not provided or if settings require it
                decimal depositAmount = obj.DepositAmount;
                
                // Get restaurant settings to check deposit requirements
                var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                    .Where(rs => rs.CompanyId == obj.CompanyId && rs.BranchId == obj.BranchId && rs.IsDeleted == false)
                    .FirstOrDefault();
                
                if (restaurantSettings != null && restaurantSettings.RequireDeposit)
                {
                    // If deposit amount is not provided or is 0, calculate it based on mode
                    if (depositAmount <= 0)
                    {
                        var depositMode = restaurantSettings.DepositMode ?? "Fixed";
                        if (depositMode.Equals("PerGuest", StringComparison.OrdinalIgnoreCase))
                        {
                            depositAmount = obj.NoOfGuests * restaurantSettings.DepositPerGuestAmount;
                        }
                        else
                        {
                            depositAmount = restaurantSettings.DepositFixedAmount;
                        }
                    }
                    // If deposit amount was provided from frontend, use it (already set above)
                }
                else
                {
                    // If deposit is not required, ensure it's 0
                    depositAmount = 0;
                }

                ClsTableBooking oBooking = new ClsTableBooking()
                {
                    BookingId = obj.BookingId,
                    BookingNo = obj.BookingNo,
                    CustomerId = obj.CustomerId,
                    BookingDate = obj.BookingDate,
                    BookingTime = obj.BookingTime,
                    Duration = obj.Duration,
                    NoOfGuests = obj.NoOfGuests,
                    Status = obj.Status ?? "Pending",
                    BookingType = obj.BookingType ?? "WalkIn",
                    SpecialRequest = obj.SpecialRequest,
                    DepositAmount = depositAmount,
                    ReminderSent = false,
                    WaiterId = obj.WaiterId > 0 ? obj.WaiterId : 0,  // Industry standard: one waiter per booking
                    BranchId = obj.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    SalesId = 0,  // Standalone - no sales link initially
                };

                if (obj.BookingId == 0)
                {
                    oConnectionContext.DbClsTableBooking.Add(oBooking);
                }
                else
                {
                    oConnectionContext.DbClsTableBooking.Attach(oBooking);
                    oConnectionContext.Entry(oBooking).Property(x => x.BookingId).IsModified = true;
                    //oConnectionContext.Entry(oBooking).Property(x => x.BookingNo).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.CustomerId).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.BookingDate).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.BookingTime).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.Duration).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.NoOfGuests).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.Status).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.BookingType).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.SpecialRequest).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.DepositAmount).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.WaiterId).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oBooking).Property(x => x.ModifiedOn).IsModified = true;
                    oBooking.ModifiedOn = CurrentDate;
                    
                    // Remove existing table associations for updates
                    var existingBookingTables = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.BookingId == obj.BookingId)
                        .ToList();
                    foreach (var existingBt in existingBookingTables)
                    {
                        oConnectionContext.DbClsTableBookingTable.Remove(existingBt);
                    }
                }

                oConnectionContext.SaveChanges();

                // Handle multiple tables - create junction table entries
                if (tablesToValidate.Any())
                {
                    int displayOrder = 1;
                    foreach (var tableId in tablesToValidate)
                    {
                        oConnectionContext.DbClsTableBookingTable.Add(new ClsTableBookingTable
                        {
                            BookingId = oBooking.BookingId,
                            TableId = tableId,
                            IsPrimary = displayOrder == 1, // First table is primary
                            DisplayOrder = displayOrder++,
                            AddedOn = CurrentDate
                        });
                    }
                    oConnectionContext.SaveChanges();
                }

                // Update prefix counter if used
                if (PrefixUserMapId > 0 && obj.BookingId == 0)
                {
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                }

                // Emit booking status update
               // await SocketIoHelper.EmitBookingStatusUpdated(new { BookingId = oBooking.BookingId, Status = oBooking.Status }, obj.CompanyId, obj.BranchId);

                // Emit table status updates for all assigned tables
                foreach (var tableId in tablesToValidate)
                {
                    // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = tableId, Status = tableStatusHelper.GetTableStatus(tableId).ToString() }, obj.CompanyId, obj.BranchId);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "TableBooking",
                    CompanyId = obj.CompanyId,
                    Description = "Booking \"" + obj.BookingNo + "\" " + (obj.BookingId == 0 ? "created" : "updated"),
                    Id = oBooking.BookingId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.BookingId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Booking " + (obj.BookingId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        Booking = oBooking
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CheckAvailability(ClsTableBookingVm obj)
        {
            if (obj.TableIds == null || !obj.TableIds.Any())
            {
                data = new
                {
                    Status = 0,
                    Message = "At least one TableId is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            if (!obj.StartDateTime.HasValue || !obj.EndDateTime.HasValue)
            {
                data = new
                {
                    Status = 0,
                    Message = "StartDateTime and EndDateTime are required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Verify tables exist and belong to company/branch
            var tables = oConnectionContext.DbClsRestaurantTable
                .Where(t => obj.TableIds.Contains(t.TableId) && 
                           t.CompanyId == obj.CompanyId && 
                           t.BranchId == obj.BranchId && 
                           t.IsActive && 
                           !t.IsDeleted)
                .ToList();

            if (tables.Count != obj.TableIds.Count)
            {
                data = new
                {
                    Status = 0,
                    Message = "One or more tables not found",
                    Data = new
                    {
                        IsAvailable = false,
                        TableIds = obj.TableIds,
                        StartDateTime = obj.StartDateTime,
                        EndDateTime = obj.EndDateTime
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            // Check availability for all tables
            var unavailableTables = new List<long>();
            foreach (var tableId in obj.TableIds)
            {
                var isAvailable = tableStatusHelper.IsTableAvailable(tableId, obj.StartDateTime.Value, obj.EndDateTime.Value);
                if (!isAvailable)
                {
                    unavailableTables.Add(tableId);
                }
            }

            var allAvailable = unavailableTables.Count == 0;

            data = new
            {
                Status = 1,
                Message = allAvailable ? "All tables available" : "Some tables unavailable",
                Data = new
                {
                    IsAvailable = allAvailable,
                    TableIds = obj.TableIds,
                    UnavailableTableIds = unavailableTables,
                    StartDateTime = obj.StartDateTime,
                    EndDateTime = obj.EndDateTime
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ConfirmBooking(ClsTableBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsTableBooking oBooking = new ClsTableBooking()
                {
                    BookingId = obj.BookingId,
                    Status = "Confirmed",
                    ConfirmedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsTableBooking.Attach(oBooking);
                oConnectionContext.Entry(oBooking).Property(x => x.BookingId).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ConfirmedOn).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // Emit booking status update
                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == obj.BookingId).Select(a => new { a.CompanyId, a.BranchId }).FirstOrDefault();
                if (booking != null)
                {
                   // await SocketIoHelper.EmitBookingStatusUpdated(new { BookingId = obj.BookingId, Status = "Confirmed" }, booking.CompanyId, booking.BranchId);
                    // Emit table status updates for all tables in this booking
                    var bookingTableIds = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.BookingId == obj.BookingId)
                        .Select(bt => bt.TableId)
                        .ToList();
                    foreach (var tableId in bookingTableIds)
                    {
                       // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = tableId, Status = tableStatusHelper.GetTableStatus(tableId).ToString() }, booking.CompanyId, booking.BranchId);
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "Booking confirmed successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CancelBooking(ClsTableBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // 1) Mark booking as cancelled
                ClsTableBooking oBooking = new ClsTableBooking()
                {
                    BookingId = obj.BookingId,
                    Status = "Cancelled",
                    CancelledOn = CurrentDate,
                    CancellationReason = obj.CancellationReason,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsTableBooking.Attach(oBooking);
                oConnectionContext.Entry(oBooking).Property(x => x.BookingId).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.CancelledOn).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.CancellationReason).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // 2) Restore any credits that were applied to this booking's deposit
                //    (same pattern as BookingPaymentController.Delete for credit-based payments)
                var creditDepositPayments = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == obj.BookingId &&
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
                        creditDepRow.ModifiedBy = obj.AddedBy;
                        creditDepRow.ModifiedOn = CurrentDate;
                    }
                }
                oConnectionContext.SaveChanges();

                // Note: Deposits are now stored as unused credits. 
                // Manual refunds can be created from unused credits using the refund functionality in BookingPaymentController.
                // The refund will restore the credits back to the customer's advance balance.

                // Emit booking status update
                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == obj.BookingId).Select(a => new { a.CompanyId, a.BranchId }).FirstOrDefault();
                if (booking != null)
                {
                   // await SocketIoHelper.EmitBookingStatusUpdated(new { BookingId = obj.BookingId, Status = "Cancelled" }, booking.CompanyId, booking.BranchId);
                    // Emit table status updates for all tables in this booking
                    var bookingTableIds = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.BookingId == obj.BookingId)
                        .Select(bt => bt.TableId)
                        .ToList();
                    foreach (var tableId in bookingTableIds)
                    {
                       // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = tableId, Status = tableStatusHelper.GetTableStatus(tableId).ToString() }, booking.CompanyId, booking.BranchId);
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "Booking cancelled successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> DeleteBooking(ClsTableBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var booking = oConnectionContext.DbClsTableBooking
                    .Where(a => a.BookingId == obj.BookingId && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                    .FirstOrDefault();

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

                // Deposit rule - check for any deposit payments or refunds against this booking
                var bookingDepositPayments = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == booking.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Payment")
                    .ToList();

                var bookingDepositRefunds = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == booking.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Refund")
                    .ToList();

                bool hasDepositTransactions = bookingDepositPayments.Any() || bookingDepositRefunds.Any();

                // Always reverse deposit effects and soft-delete deposit records when deposits exist
                if (hasDepositTransactions)
                {
                    // Reverse any credit-based deposits (ParentId > 0 and not direct payment),
                    // same pattern as BookingPaymentController.Delete and CancelBooking.
                    var creditBasedDeposits = bookingDepositPayments
                        .Where(cp => cp.ParentId > 0 && cp.IsDirectPayment == false)
                        .Select(cp => new
                        {
                            cp.CustomerPaymentId,
                            cp.ParentId,
                            cp.Amount,
                            cp.CustomerId
                        })
                        .ToList();

                    foreach (var creditDep in creditBasedDeposits)
                    {
                        // Restore parent credit payment's AmountRemaining and AmountUsed
                        string restoreParentQuery = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + creditDep.Amount +
                            ",\"AmountUsed\"=\"AmountUsed\"-" + creditDep.Amount + " where \"CustomerPaymentId\"=" + creditDep.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(restoreParentQuery);

                        // Restore customer's advance balance
                        string restoreBalanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + creditDep.Amount + " where \"UserId\"=" + creditDep.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(restoreBalanceQuery);
                    }

                    // Soft-delete all booking deposit payments (both credit-based and others)
                    foreach (var dep in bookingDepositPayments)
                    {
                        dep.IsDeleted = true;
                        dep.ModifiedBy = obj.AddedBy;
                        dep.ModifiedOn = CurrentDate;
                    }

                    // Soft-delete all booking deposit refunds tied to this booking
                    foreach (var refund in bookingDepositRefunds)
                    {
                        refund.IsDeleted = true;
                        refund.ModifiedBy = obj.AddedBy;
                        refund.ModifiedOn = CurrentDate;
                    }

                    oConnectionContext.SaveChanges();
                }

                // Soft delete booking
                booking.IsDeleted = true;
                booking.ModifiedBy = obj.AddedBy;
                booking.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Remove any table mappings for this booking
                var bookingTables = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == booking.BookingId)
                    .ToList();

                if (bookingTables.Any())
                {
                    oConnectionContext.DbClsTableBookingTable.RemoveRange(bookingTables);
                    oConnectionContext.SaveChanges();
                }

                // Activity log
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser ?? "",
                    Category = "TableBooking",
                    CompanyId = obj.CompanyId,
                    Description = "Table booking deleted",
                    Id = obj.BookingId,
                    IpAddress = obj.IpAddress ?? "",
                    Platform = obj.Platform ?? "",
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Booking deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CompleteBooking(ClsTableBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsTableBooking oBooking = new ClsTableBooking()
                {
                    BookingId = obj.BookingId,
                    Status = "Completed",
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsTableBooking.Attach(oBooking);
                oConnectionContext.Entry(oBooking).Property(x => x.BookingId).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // Emit booking status update
                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == obj.BookingId).Select(a => new { a.CompanyId, a.BranchId }).FirstOrDefault();
                if (booking != null)
                {
                   // await SocketIoHelper.EmitBookingStatusUpdated(new { BookingId = obj.BookingId, Status = "Completed" }, booking.CompanyId, booking.BranchId);
                    // Emit table status updates for all tables in this booking
                    var bookingTableIds = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.BookingId == obj.BookingId)
                        .Select(bt => bt.TableId)
                        .ToList();
                    foreach (var tableId in bookingTableIds)
                    {
                       // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = tableId, Status = tableStatusHelper.GetTableStatus(tableId).ToString() }, booking.CompanyId, booking.BranchId);
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "Booking completed successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> MarkAsNoShow(ClsTableBookingVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsTableBooking oBooking = new ClsTableBooking()
                {
                    BookingId = obj.BookingId,
                    Status = "No Show",
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsTableBooking.Attach(oBooking);
                oConnectionContext.Entry(oBooking).Property(x => x.BookingId).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBooking).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // Emit booking status update
                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == obj.BookingId).Select(a => new { a.CompanyId, a.BranchId }).FirstOrDefault();
                if (booking != null)
                {
                   // await SocketIoHelper.EmitBookingStatusUpdated(new { BookingId = obj.BookingId, Status = "No Show" }, booking.CompanyId, booking.BranchId);
                    // Emit table status updates for all tables in this booking
                    var bookingTableIds = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.BookingId == obj.BookingId)
                        .Select(bt => bt.TableId)
                        .ToList();
                    foreach (var tableId in bookingTableIds)
                    {
                       // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = tableId, Status = tableStatusHelper.GetTableStatus(tableId).ToString() }, booking.CompanyId, booking.BranchId);
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "Booking marked as No-Show successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> LinkToKot([FromBody] ClsTableBookingVm obj)
        {
            long bookingId = obj.BookingId;
            if (obj.KotId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "KOT ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
            long kotId = obj.KotId;
            var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == bookingId).FirstOrDefault();
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
            var CurrentDate = oCommonController.CurrentDate(booking.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                booking.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Update KOT with booking link
                var kot = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == kotId).FirstOrDefault();
                if (kot != null)
                {
                    kot.BookingId = bookingId;
                    kot.ModifiedOn = CurrentDate;
                    oConnectionContext.SaveChanges();
                }

                // Emit linking event
               // await SocketIoHelper.EmitBookingLinkedToKot(new { BookingId = bookingId, KotId = kotId }, booking.CompanyId, booking.BranchId);

                data = new
                {
                    Status = 1,
                    Message = "Booking linked to KOT successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> LinkToSales([FromBody] ClsTableBookingVm obj)
        {
            long bookingId = obj.BookingId;
            if (obj.SalesId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Sales ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
            long salesId = obj.SalesId;
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == bookingId).FirstOrDefault();
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
                var CurrentDate = oCommonController.CurrentDate(booking.CompanyId);

                booking.SalesId = salesId;
                booking.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Automatically link all served KOTs for this booking to the same sales invoice
                var servedKots = oConnectionContext.DbClsKotMaster
                    .Where(a => a.BookingId == bookingId && 
                           a.OrderStatus == "Served" && 
                           a.SalesId == 0 && 
                           a.CompanyId == booking.CompanyId && 
                           a.IsDeleted == false)
                    .ToList();

                int linkedKotCount = 0;
                foreach (var kot in servedKots)
                {
                    kot.SalesId = salesId;
                    kot.ModifiedOn = CurrentDate;
                    linkedKotCount++;
                }

                if (linkedKotCount > 0)
                {
                    oConnectionContext.SaveChanges();
                }

                string message = linkedKotCount > 0 
                    ? $"Booking and {linkedKotCount} served KOT(s) linked to Sales successfully"
                    : "Booking linked to Sales successfully";

                data = new
                {
                    Status = 1,
                    Message = message,
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetCalendarEvents(ClsTableBookingVm obj)
        {
            var startDate = obj.FromDate ?? DateTime.Now.Date;
            var endDate = obj.ToDate ?? DateTime.Now.Date.AddDays(30);

            // First, get the raw data from the database
            var query = oConnectionContext.DbClsTableBooking
                .Where(a => a.CompanyId == obj.CompanyId && 
                       a.IsDeleted == false &&
                       a.BookingDate >= startDate &&
                       a.BookingDate <= endDate);
            
            // Filter by BranchId only if provided and greater than 0
            if (obj.BranchId > 0)
            {
                query = query.Where(a => a.BranchId == obj.BranchId);
            }
            
            var bookingsData = query
                .Select(a => new
                {
                    a.BookingId,
                    CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                    a.BookingDate,
                    a.BookingTime,
                    a.Duration,
                    a.Status,
                    a.BookingNo,
                    CustomerMobile = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                    a.NoOfGuests,
                    a.BookingType,
                    a.SpecialRequest
                })
                .ToList();

            // Get all table IDs from junction table
            var allBookingIds = bookingsData.Select(b => (long)b.BookingId).ToList();
            var bookingTableIds = oConnectionContext.DbClsTableBookingTable
                .Where(bt => allBookingIds.Contains((long)bt.BookingId))
                .Select(bt => (long)bt.TableId)
                .Distinct()
                .ToList();
            
            var allTableIdsList = bookingTableIds.Distinct().ToList();
            
            // Get table numbers for all tables in one query
            var tables = oConnectionContext.DbClsRestaurantTable
                .Where(t => allTableIdsList.Contains((long)t.TableId))
                .ToDictionary(t => (long)t.TableId, t => t.TableNo);

            // Get booking-table mappings
            var bookingTablesMap = oConnectionContext.DbClsTableBookingTable
                .Where(bt => allBookingIds.Contains((long)bt.BookingId))
                .GroupBy(bt => (long)bt.BookingId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(bt => bt.DisplayOrder)
                          .Select(bt => new { TableId = (long)bt.TableId, TableNo = tables.ContainsKey((long)bt.TableId) ? tables[(long)bt.TableId] : null })
                          .ToList()
                );

            // Now perform date calculations in memory
            var bookings = bookingsData.Select(a =>
            {
                // Get table info from junction table
                string tableNo = "No Table";
                long? primaryTableId = null;
                List<string> allTableNos = new List<string>();
                
                if (bookingTablesMap.ContainsKey(a.BookingId))
                {
                    var bookingTables = bookingTablesMap[a.BookingId];
                    allTableNos = bookingTables.Select(bt => bt.TableNo).Where(tn => !string.IsNullOrEmpty(tn)).ToList();
                    if (allTableNos.Any() && bookingTables.Any())
                    {
                        tableNo = string.Join(", ", allTableNos);
                        var firstTable = bookingTables.First();
                        primaryTableId = firstTable != null ? (long?)firstTable.TableId : (long?)null;
                    }
                }
                
                var bookingStart = a.BookingDate.Date.Add(a.BookingTime);
                var bookingEnd = bookingStart.AddMinutes(a.Duration);
                
                return new
                {
                    id = a.BookingId,
                    title = a.CustomerName + " - " + (tableNo ?? "No Table"),
                    start = bookingStart.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = bookingEnd.ToString("yyyy-MM-ddTHH:mm:ss"),
                    allDay = false,
                    color = a.Status == "Confirmed" ? "#28a745" : a.Status == "Pending" ? "#ffc107" : a.Status == "Cancelled" ? "#dc3545" : a.Status == "No Show" ? "#ff9800" : a.Status == "Completed" ? "#17a2b8" : "#6c757d",
                    extendedProps = new
                    {
                        bookingNo = a.BookingNo,
                        tableId = primaryTableId,
                        tableNo = tableNo,
                        tableNos = allTableNos,
                        customerName = a.CustomerName,
                        customerMobile = a.CustomerMobile,
                        noOfGuests = a.NoOfGuests,
                        status = a.Status,
                        bookingType = a.BookingType,
                        specialRequest = a.SpecialRequest
                    }
                };
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Events = bookings
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetTablePerformanceReport(ClsTableBookingVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                // Get all tables for the branch
                var tables = oConnectionContext.DbClsRestaurantTable
                    .Where(t => t.CompanyId == obj.CompanyId && 
                               (obj.BranchId == 0 || t.BranchId == obj.BranchId) && 
                               t.IsDeleted == false)
                    .Select(t => new { t.TableId, t.TableNo, t.TableName, t.Capacity, t.FloorId })
                    .ToList();

                // Get all bookings in the date range
                var bookings = oConnectionContext.DbClsTableBooking
                    .Where(b => b.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || b.BranchId == obj.BranchId) &&
                               b.IsDeleted == false &&
                               b.BookingDate >= obj.FromDate.Value &&
                               b.BookingDate <= obj.ToDate.Value)
                    .ToList();

                // Get booking-table mappings
                var bookingIds = bookings.Select(b => b.BookingId).ToList();
                var bookingTableMappings = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bookingIds.Contains(bt.BookingId))
                    .GroupBy(bt => bt.TableId)
                    .ToDictionary(g => g.Key, g => g.Select(bt => bt.BookingId).ToList());

                // Get sales linked to bookings for revenue calculation
                var bookingSalesIds = bookings.Where(b => b.SalesId > 0).Select(b => b.SalesId).Distinct().ToList();
                var salesRevenue = oConnectionContext.DbClsSales
                    .Where(s => bookingSalesIds.Contains(s.SalesId) && s.IsDeleted == false && s.IsCancelled == false)
                    .GroupBy(s => (from bt in oConnectionContext.DbClsTableBookingTable
                                   join b in oConnectionContext.DbClsTableBooking on bt.BookingId equals b.BookingId
                                   where b.SalesId == s.SalesId
                                   select bt.TableId).FirstOrDefault())
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.GrandTotal));

                // Calculate performance metrics for each table
                var tablePerformance = tables.Select(table =>
                {
                    var tableBookings = new List<long>();
                    if (bookingTableMappings.ContainsKey(table.TableId))
                    {
                        tableBookings = bookingTableMappings[table.TableId];
                    }

                    var bookingsForTable = bookings.Where(b => tableBookings.Contains(b.BookingId)).ToList();

                    var totalBookings = bookingsForTable.Count;
                    var confirmedBookings = bookingsForTable.Count(b => b.Status.ToLower() == "confirmed");
                    var completedBookings = bookingsForTable.Count(b => b.Status.ToLower() == "completed");
                    var cancelledBookings = bookingsForTable.Count(b => b.Status.ToLower() == "cancelled");

                    // Calculate total occupancy hours
                    var totalOccupancyMinutes = bookingsForTable
                        .Where(b => b.Status.ToLower() == "completed" || b.Status.ToLower() == "confirmed")
                        .Sum(b => (double?)b.Duration) ?? 0;
                    var totalOccupancyHours = Math.Round(totalOccupancyMinutes / 60.0, 2);

                    // Calculate revenue
                    var revenue = 0m;
                    if (salesRevenue.ContainsKey(table.TableId))
                    {
                        revenue = salesRevenue[table.TableId];
                    }

                    // Average guests per booking
                    var avgGuests = bookingsForTable.Any() 
                        ? Math.Round(bookingsForTable.Average(b => (double?)b.NoOfGuests) ?? 0, 1)
                        : 0;

                    // Average booking duration
                    var avgDuration = bookingsForTable.Any()
                        ? Math.Round(bookingsForTable.Average(b => (double?)b.Duration) ?? 0, 1)
                        : 0;

                    // Calculate occupancy rate (assuming 12 hours operation per day)
                    var daysInRange = (obj.ToDate.Value - obj.FromDate.Value).TotalDays + 1;
                    var totalAvailableHours = daysInRange * 12; // 12 hours per day
                    var occupancyRate = totalAvailableHours > 0 
                        ? Math.Round((totalOccupancyHours / totalAvailableHours) * 100, 2)
                        : 0;

                    return new
                    {
                        TableId = table.TableId,
                        TableNo = table.TableNo,
                        TableName = table.TableName ?? table.TableNo,
                        Capacity = table.Capacity,
                        TotalBookings = totalBookings,
                        ConfirmedBookings = confirmedBookings,
                        CompletedBookings = completedBookings,
                        CancelledBookings = cancelledBookings,
                        TotalRevenue = revenue,
                        TotalOccupancyHours = totalOccupancyHours,
                        AverageGuests = avgGuests,
                        AverageDuration = avgDuration,
                        OccupancyRate = occupancyRate
                    };
                })
                .Where(tp => tp.TotalBookings > 0 || tp.TotalRevenue > 0) // Only show tables with activity
                .OrderByDescending(tp => tp.TotalRevenue)
                .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        TablePerformance = tablePerformance,
                        TotalCount = tablePerformance.Count
                    }
                };
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new
                    {
                        TablePerformance = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetIndividualTableDetailReport(ClsTableBookingVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var tables = oConnectionContext.DbClsRestaurantTable
                    .Where(t => t.CompanyId == obj.CompanyId && 
                               (obj.BranchId == 0 || t.BranchId == obj.BranchId) && 
                               t.IsDeleted == false)
                    .ToList();

                var tableDetails = new List<dynamic>();

                foreach (var table in tables)
                {
                    // Filter by table if specified
                    if (obj.TableIds != null && obj.TableIds.Count > 0 && !obj.TableIds.Contains(table.TableId))
                        continue;

                    var tableBookings = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.TableId == table.TableId)
                        .Join(oConnectionContext.DbClsTableBooking,
                            bt => bt.BookingId,
                            b => b.BookingId,
                            (bt, b) => b)
                        .Where(b => b.CompanyId == obj.CompanyId &&
                                   b.IsDeleted == false &&
                                   b.BookingDate >= obj.FromDate.Value &&
                                   b.BookingDate <= obj.ToDate.Value)
                        .ToList();

                    var totalBookings = tableBookings.Count;
                    var confirmedBookings = tableBookings.Count(b => b.Status.ToLower() == "confirmed");
                    var completedBookings = tableBookings.Count(b => b.Status.ToLower() == "completed");
                    var cancelledBookings = tableBookings.Count(b => b.Status.ToLower() == "cancelled");

                    // Calculate revenue from linked sales
                    var bookingSalesIds = tableBookings.Where(b => b.SalesId > 0).Select(b => b.SalesId).Distinct().ToList();
                    var revenue = oConnectionContext.DbClsSales
                        .Where(s => bookingSalesIds.Contains(s.SalesId) && s.IsDeleted == false && s.IsCancelled == false)
                        .Sum(s => (decimal?)s.GrandTotal) ?? 0;

                    // Calculate occupancy hours
                    var totalOccupancyMinutes = tableBookings
                        .Where(b => b.Status.ToLower() == "completed" || b.Status.ToLower() == "confirmed")
                        .Sum(b => (double?)b.Duration) ?? 0;
                    var totalOccupancyHours = Math.Round(totalOccupancyMinutes / 60.0, 2);

                    // Get table type and floor info
                    var tableType = oConnectionContext.DbClsTableType
                        .Where(tt => tt.TableTypeId == table.TableTypeId)
                        .Select(tt => tt.TableTypeName)
                        .FirstOrDefault() ?? "N/A";

                    var floor = oConnectionContext.DbClsRestaurantFloor
                        .Where(f => f.FloorId == table.FloorId)
                        .Select(f => f.FloorName)
                        .FirstOrDefault() ?? "N/A";

                    tableDetails.Add(new
                    {
                        TableId = table.TableId,
                        TableNo = table.TableNo,
                        TableName = table.TableName ?? table.TableNo,
                        Capacity = table.Capacity,
                        TableType = tableType,
                        Floor = floor,
                        TotalBookings = totalBookings,
                        ConfirmedBookings = confirmedBookings,
                        CompletedBookings = completedBookings,
                        CancelledBookings = cancelledBookings,
                        TotalRevenue = revenue,
                        TotalOccupancyHours = totalOccupancyHours,
                        UtilizationPercentage = totalOccupancyHours > 0 ? Math.Round((totalOccupancyHours / (obj.ToDate.Value - obj.FromDate.Value).TotalDays / 24.0) * 100, 2) : 0
                    });
                }

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        TableDetails = tableDetails.OrderByDescending(t => t.TotalRevenue),
                        TotalCount = tableDetails.Count
                    }
                };
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new
                    {
                        TableDetails = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetBookingSourceReport(ClsTableBookingVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var bookings = oConnectionContext.DbClsTableBooking
                    .Where(b => b.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || b.BranchId == obj.BranchId) &&
                               b.IsDeleted == false &&
                               b.BookingDate >= obj.FromDate.Value &&
                               b.BookingDate <= obj.ToDate.Value)
                    .ToList();

                var sourceData = bookings
                    .GroupBy(b => 
                    {
                        // Determine source based on PublicBookingToken and other indicators
                        if (!string.IsNullOrEmpty(b.PublicBookingToken))
                            return "Online/Public";
                        else if (!string.IsNullOrEmpty(b.IpAddress))
                            return "Walk-in (System)";
                        else
                            return "Phone/Manual";
                    })
                    .Select(g => new
                    {
                        Source = g.Key,
                        TotalBookings = g.Count(),
                        ConfirmedBookings = g.Count(b => b.Status.ToLower() == "confirmed"),
                        CompletedBookings = g.Count(b => b.Status.ToLower() == "completed"),
                        CancelledBookings = g.Count(b => b.Status.ToLower() == "cancelled"),
                        TotalRevenue = oConnectionContext.DbClsSales
                            .Where(s => g.Where(b => b.SalesId > 0).Select(b => b.SalesId).Contains(s.SalesId) && 
                                       s.IsDeleted == false && s.IsCancelled == false)
                            .Sum(s => (decimal?)s.GrandTotal) ?? 0,
                        AverageGuests = g.Average(b => (double?)b.NoOfGuests) ?? 0
                    })
                    .OrderByDescending(s => s.TotalBookings)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        SourceData = sourceData,
                        TotalCount = sourceData.Count
                    }
                };
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new
                    {
                        SourceData = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetTableTypePerformanceReport(ClsTableBookingVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var bookings = oConnectionContext.DbClsTableBooking
                    .Where(b => b.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || b.BranchId == obj.BranchId) &&
                               b.IsDeleted == false &&
                               b.BookingDate >= obj.FromDate.Value &&
                               b.BookingDate <= obj.ToDate.Value)
                    .ToList();

                var bookingTableMappings = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bookings.Select(b => b.BookingId).Contains(bt.BookingId))
                    .Join(oConnectionContext.DbClsRestaurantTable,
                        bt => bt.TableId,
                        t => t.TableId,
                        (bt, t) => new { bt.BookingId, t.TableTypeId })
                    .Join(oConnectionContext.DbClsTableType,
                        bt => bt.TableTypeId,
                        tt => tt.TableTypeId,
                        (bt, tt) => new { bt.BookingId, TableTypeName = tt.TableTypeName })
                    .ToList();

                var typePerformance = bookings
                    .GroupBy(b => 
                    {
                        var typeMapping = bookingTableMappings.FirstOrDefault(m => m.BookingId == b.BookingId);
                        return typeMapping != null ? typeMapping.TableTypeName : "Unknown";
                    })
                    .Select(g => new
                    {
                        TableType = g.Key,
                        TotalBookings = g.Count(),
                        ConfirmedBookings = g.Count(b => b.Status.ToLower() == "confirmed"),
                        CompletedBookings = g.Count(b => b.Status.ToLower() == "completed"),
                        CancelledBookings = g.Count(b => b.Status.ToLower() == "cancelled"),
                        TotalRevenue = oConnectionContext.DbClsSales
                            .Where(s => g.Where(b => b.SalesId > 0).Select(b => b.SalesId).Contains(s.SalesId) && 
                                       s.IsDeleted == false && s.IsCancelled == false)
                            .Sum(s => (decimal?)s.GrandTotal) ?? 0,
                        AverageGuests = g.Average(b => (double?)b.NoOfGuests) ?? 0,
                        AverageDuration = g.Average(b => (double?)b.Duration) ?? 0
                    })
                    .OrderByDescending(t => t.TotalRevenue)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        TypePerformance = typePerformance,
                        TotalCount = typePerformance.Count
                    }
                };
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new
                    {
                        TypePerformance = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetCustomerVisitFrequencyReport(ClsTableBookingVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-90);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var bookings = oConnectionContext.DbClsTableBooking
                    .Where(b => b.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || b.BranchId == obj.BranchId) &&
                               b.IsDeleted == false &&
                               b.CustomerId > 0 &&
                               b.BookingDate >= obj.FromDate.Value &&
                               b.BookingDate <= obj.ToDate.Value)
                    .ToList();

                var customerFrequency = bookings
                    .GroupBy(b => b.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        CustomerName = oConnectionContext.DbClsUser
                            .Where(u => u.UserId == g.Key)
                            .Select(u => u.Name)
                            .FirstOrDefault() ?? "Unknown",
                        CustomerMobile = oConnectionContext.DbClsUser
                            .Where(u => u.UserId == g.Key)
                            .Select(u => u.MobileNo)
                            .FirstOrDefault() ?? "",
                        TotalVisits = g.Count(),
                        CompletedVisits = g.Count(b => b.Status.ToLower() == "completed"),
                        CancelledVisits = g.Count(b => b.Status.ToLower() == "cancelled"),
                        TotalSpent = oConnectionContext.DbClsSales
                            .Where(s => g.Where(b => b.SalesId > 0).Select(b => b.SalesId).Contains(s.SalesId) && 
                                       s.IsDeleted == false && s.IsCancelled == false)
                            .Sum(s => (decimal?)s.GrandTotal) ?? 0,
                        AverageSpent = oConnectionContext.DbClsSales
                            .Where(s => g.Where(b => b.SalesId > 0).Select(b => b.SalesId).Contains(s.SalesId) && 
                                       s.IsDeleted == false && s.IsCancelled == false)
                            .Average(s => (decimal?)s.GrandTotal) ?? 0,
                        LastVisitDate = g.Max(b => b.BookingDate),
                        FirstVisitDate = g.Min(b => b.BookingDate)
                    })
                    .Where(c => c.TotalVisits > 0)
                    .OrderByDescending(c => c.TotalVisits)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        CustomerFrequency = customerFrequency,
                        TotalCount = customerFrequency.Count
                    }
                };
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new
                    {
                        CustomerFrequency = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }
    }
}


