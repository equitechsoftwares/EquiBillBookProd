using EquiBillBook.Filters;
using EquiBillBook.Helpers;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class KotController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        TableStatusHelper tableStatusHelper = new TableStatusHelper();
        [HttpPost]
        public async Task<IHttpActionResult> GetKots(ClsKotMasterVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var query = oConnectionContext.DbClsKotMaster.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false);
            
            // Filter by BranchId only if provided (not "All")
            if (obj.BranchId > 0)
            {
                query = query.Where(a => a.BranchId == obj.BranchId);
            }

            if (obj.FromDate.HasValue)
            {
                query = query.Where(a => a.OrderTime >= obj.FromDate.Value);
            }

            if (obj.ToDate.HasValue)
            {
                query = query.Where(a => a.OrderTime <= obj.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(obj.OrderStatus))
            {
                query = query.Where(a => a.OrderStatus == obj.OrderStatus);
            }

            if (obj.TableId > 0)
            {
                query = query.Where(a => a.TableId == obj.TableId);
            }

            if (obj.WithSales)
            {
                query = query.Where(a => a.SalesId > 0);
            }
            else
            {
                query = query.Where(a => a.SalesId == 0);
            }

            var det = query.Select(a => new
            {
                KotId = a.KotId,
                KotNo = a.KotNo,
                TableId = a.TableId,
                SalesId = a.SalesId,
                BookingId = a.BookingId,
                OrderType = a.OrderType,
                OrderStatus = a.OrderStatus,
                OrderTime = a.OrderTime,
                ExpectedTime = a.ExpectedTime,
                ReadyTime = a.ReadyTime,
                ServedTime = a.ServedTime,
                WaiterId = a.WaiterId,
                GuestCount = a.GuestCount,
                CustomerId = a.CustomerId,
                SpecialInstructions = a.SpecialInstructions,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                Printed = a.Printed,
                PrintedOn = a.PrintedOn,
                PrintedBy = a.PrintedBy,
                TableNo = a.TableId > 0 ? oConnectionContext.DbClsRestaurantTable.Where(t => t.TableId == a.TableId).Select(t => t.TableNo).FirstOrDefault() : null,
                TableName = a.TableId > 0 ? oConnectionContext.DbClsRestaurantTable.Where(t => t.TableId == a.TableId).Select(t => t.TableName).FirstOrDefault() : null,
                WaiterName = a.WaiterId > 0 ? oConnectionContext.DbClsUser.Where(u => u.UserId == a.WaiterId).Select(u => u.Name).FirstOrDefault() : null,
                CustomerName = a.CustomerId > 0 ? oConnectionContext.DbClsUser.Where(u => u.UserId == a.CustomerId).Select(u => u.Name).FirstOrDefault() : null,
                SalesNo = a.SalesId > 0 ? oConnectionContext.DbClsSales.Where(s => s.SalesId == a.SalesId).Select(s => s.InvoiceNo).FirstOrDefault() : null,
                BookingNo = a.BookingId > 0 ? oConnectionContext.DbClsTableBooking.Where(b => b.BookingId == a.BookingId).Select(b => b.BookingNo).FirstOrDefault() : null,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => (a.KotNo != null && a.KotNo.ToLower().Contains(obj.Search.ToLower())) ||
                                     (a.TableNo != null && a.TableNo.ToLower().Contains(obj.Search.ToLower())) ||
                                     (a.CustomerName != null && a.CustomerName.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Kots = det.OrderByDescending(a => a.OrderTime).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetStandaloneKots(ClsKotMasterVm obj)
        {
            var query = oConnectionContext.DbClsKotMaster
                .Where(a => a.CompanyId == obj.CompanyId && 
                       a.SalesId == 0 && 
                       a.IsDeleted == false);
            
            // Filter by BranchId only if provided (not "All")
            if (obj.BranchId > 0)
            {
                query = query.Where(a => a.BranchId == obj.BranchId);
            }
            
            var det = query
                .Select(a => new
                {
                    KotId = a.KotId,
                    KotNo = a.KotNo,
                    TableId = a.TableId,
                    OrderType = a.OrderType,
                    OrderStatus = a.OrderStatus,
                    OrderTime = a.OrderTime,
                    TableNo = a.TableId > 0 ? oConnectionContext.DbClsRestaurantTable.Where(t => t.TableId == a.TableId).Select(t => t.TableNo).FirstOrDefault() : null
                })
                .OrderByDescending(a => a.OrderTime)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Kots = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Kot(ClsKotMasterVm obj)
        {
            var det = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == obj.KotId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                KotId = a.KotId,
                KotNo = a.KotNo,
                TableId = a.TableId,
                SalesId = a.SalesId,
                BookingId = a.BookingId,
                OrderType = a.OrderType,
                OrderStatus = a.OrderStatus,
                OrderTime = a.OrderTime,
                ExpectedTime = a.ExpectedTime,
                ReadyTime = a.ReadyTime,
                ServedTime = a.ServedTime,
                WaiterId = a.WaiterId,
                GuestCount = a.GuestCount,
                CustomerId = a.CustomerId,
                SpecialInstructions = a.SpecialInstructions,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                Printed = a.Printed,
                PrintedOn = a.PrintedOn,
                PrintedBy = a.PrintedBy,
                TableNo = a.TableId > 0 ? oConnectionContext.DbClsRestaurantTable.Where(t => t.TableId == a.TableId).Select(t => t.TableNo).FirstOrDefault() : null,
                SalesNo = a.SalesId > 0 ? oConnectionContext.DbClsSales.Where(s => s.SalesId == a.SalesId).Select(s => s.InvoiceNo).FirstOrDefault() : null,
                BookingNo = a.BookingId > 0 ? oConnectionContext.DbClsTableBooking.Where(b => b.BookingId == a.BookingId).Select(b => b.BookingNo).FirstOrDefault() : null,
                WaiterName = a.WaiterId > 0 ? oConnectionContext.DbClsUser.Where(u => u.UserId == a.WaiterId).Select(u => u.Name).FirstOrDefault() : null
            }).FirstOrDefault();

            // Get KOT Details
            var kotDetails = oConnectionContext.DbClsKotDetails
                .Where(a => a.KotId == obj.KotId && a.IsDeleted == false)
                .Select(a => new
                {
                    KotDetailsId = a.KotDetailsId,
                    KotId = a.KotId,
                    ItemId = a.ItemId,
                    ItemDetailsId = a.ItemDetailsId,
                    ItemName = oConnectionContext.DbClsItem.Where(i => i.ItemId == a.ItemId).Select(i => i.ItemName).FirstOrDefault(),
                    Quantity = a.Quantity,
                    UnitId = a.UnitId,
                    Unit = oConnectionContext.DbClsUnit.Where(u => u.UnitId == a.UnitId).Select(u => u.UnitName).FirstOrDefault(),
                    CookingInstructions = a.CookingInstructions,
                    ItemStatus = a.ItemStatus,
                    KitchenStationId = a.KitchenStationId,
                    EstimatedTime = a.EstimatedTime,
                    StartedCookingAt = a.StartedCookingAt,
                    ReadyAt = a.ReadyAt,
                    ServedAt = a.ServedAt,
                    Priority = a.Priority,
                    KitchenStationName = a.KitchenStationId > 0 ? oConnectionContext.DbClsKitchenStation.Where(s => s.KitchenStationId == a.KitchenStationId).Select(s => s.StationName).FirstOrDefault() : null
                })
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Kot = det,
                    KotDetails = kotDetails
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CreateStandalone(ClsKotMasterVm obj)
        {
            // Creates a standalone KOT (can be with or without booking)
            // Industry Standard: 1 KOT per order/booking
            // If BookingId is provided, this still creates only 1 KOT for that booking
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Validate BranchId
                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "Business Location is required", Id = "divBranch" });
                    isError = true;
                }
                
                // Validate OrderType and set default
                if (string.IsNullOrEmpty(obj.OrderType))
                {
                    obj.OrderType = "DineIn";
                }
                
                // Validate TableId - mandatory for DineIn orders
                if (obj.OrderType == "DineIn" && (obj.TableId == null || obj.TableId == 0))
                {
                    errors.Add(new ClsError { Message = "Table is required for Dine In orders", Id = "divTableId" });
                    isError = true;
                }
                
                // Validate GuestCount
                if (obj.GuestCount == null || obj.GuestCount <= 0)
                {
                    errors.Add(new ClsError { Message = "Number of Guests is required and must be greater than 0", Id = "divGuestCount" });
                    isError = true;
                }
                
                // Validate Items
                if (obj.KotDetails == null || obj.KotDetails.Count == 0)
                {
                    errors.Add(new ClsError { Message = "At least one item is required", Id = "divKotDetails" });
                    isError = true;
                }
                else
                {
                    // Validate each item has valid quantity
                    foreach (var detail in obj.KotDetails)
                    {
                        if (detail.Quantity <= 0)
                        {
                            errors.Add(new ClsError { Message = "All items must have a quantity greater than 0", Id = "divKotDetails" });
                            isError = true;
                            break;
                        }
                    }
                }

                // Get existing KOT if editing
                ClsKotMaster existingKot = null;
                if (obj.KotId > 0)
                {
                    existingKot = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == obj.KotId && a.CompanyId == obj.CompanyId).FirstOrDefault();
                    if (existingKot == null)
                    {
                        errors.Add(new ClsError { Message = "KOT not found", Id = "divKotDetails" });
                        isError = true;
                    }
                    else
                    {
                        // Industry standard: Check if KOT can be edited
                        var restrictedStatuses = new[] { "Preparing", "Ready", "Served", "Cancelled" };
                        if (restrictedStatuses.Contains(existingKot.OrderStatus))
                        {
                            errors.Add(new ClsError { Message = $"Cannot edit KOT with status '{existingKot.OrderStatus}'. Only KOTs with 'Pending' or 'Printed' status can be edited.", Id = "divKotDetails" });
                            isError = true;
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

                // If BookingId is provided, fetch booking details to populate BranchId and CustomerId
                if (obj.BookingId > 0)
                {
                    var booking = oConnectionContext.DbClsTableBooking
                        .Where(a => a.BookingId == obj.BookingId && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                        .FirstOrDefault();
                    
                    if (booking != null)
                    {
                        // Populate BranchId and CustomerId from booking if not already set
                        if (obj.BranchId == 0)
                        {
                            obj.BranchId = booking.BranchId;
                        }
                        if (obj.CustomerId == 0)
                        {
                            obj.CustomerId = booking.CustomerId;
                        }
                    }
                }

                // Generate KotNo if not provided (only for new KOTs)
                long PrefixUserMapId = 0;
                if (obj.KotId == 0 && string.IsNullOrEmpty(obj.KotNo))
                {
                    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "kot"
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
                        obj.KotNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    }
                    else
                    {
                        // Fallback if prefix not configured
                        obj.KotNo = "KOT" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsKotMaster.Where(a => a.CompanyId == obj.CompanyId).Count().ToString().PadLeft(4, '0');
                    }
                }
                else if (obj.KotId > 0 && existingKot != null)
                {
                    // Preserve existing KotNo when editing
                    obj.KotNo = existingKot.KotNo;
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

                ClsKotMaster oKot;
                if (existingKot != null)
                {
                    // Update the tracked entity instead of attaching a new one to avoid PK conflicts
                    oKot = existingKot;
                    oKot.KotNo = obj.KotNo;
                    oKot.TableId = obj.TableId > 0 ? obj.TableId : existingKot.TableId;
                    oKot.BookingId = obj.BookingId > 0 ? obj.BookingId : existingKot.BookingId;
                    oKot.OrderType = obj.OrderType ?? "DineIn";
                    oKot.ExpectedTime = obj.ExpectedTime ?? existingKot.ExpectedTime;
                    oKot.WaiterId = obj.WaiterId > 0 ? obj.WaiterId : existingKot.WaiterId;
                    oKot.GuestCount = obj.GuestCount > 0 ? obj.GuestCount : existingKot.GuestCount;
                    oKot.CustomerId = obj.CustomerId > 0 ? obj.CustomerId : existingKot.CustomerId;
                    oKot.SpecialInstructions = obj.SpecialInstructions;
                    oKot.ModifiedBy = obj.AddedBy;
                    oKot.ModifiedOn = CurrentDate;
                    // Preserve OrderStatus/OrderTime/ReadyTime/ServedTime/Printed fields
                    oConnectionContext.SaveChanges();
                }
                else
                {
                    oKot = new ClsKotMaster()
                    {
                        KotId = obj.KotId,
                        KotNo = obj.KotNo,
                        TableId = obj.TableId > 0 ? obj.TableId : 0,
                        SalesId = obj.SalesId > 0 ? obj.SalesId : 0,
                        BookingId = obj.BookingId > 0 ? obj.BookingId : 0,
                        OrderType = obj.OrderType ?? "DineIn",
                        OrderStatus = obj.OrderStatus ?? "Pending",
                        OrderTime = CurrentDate,
                        ExpectedTime = obj.ExpectedTime,
                        ReadyTime = null,
                        ServedTime = null,
                        WaiterId = obj.WaiterId > 0 ? obj.WaiterId : 0,
                        GuestCount = obj.GuestCount > 0 ? obj.GuestCount : 0,
                        CustomerId = obj.CustomerId > 0 ? obj.CustomerId : 0,
                        SpecialInstructions = obj.SpecialInstructions,
                        BranchId = obj.BranchId,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        ModifiedBy = obj.AddedBy,
                        Printed = false,
                        PrintedOn = null,
                        PrintedBy = 0
                    };

                    oConnectionContext.DbClsKotMaster.Add(oKot);
                    oConnectionContext.SaveChanges();
                }

                // Save KOT Details
                if (obj.KotDetails != null && obj.KotDetails.Count > 0)
                {
                    // Delete existing details if updating (handled per-record below)
                    var existingDetails = obj.KotId > 0
                        ? oConnectionContext.DbClsKotDetails.Where(a => a.KotId == oKot.KotId).ToList()
                        : new System.Collections.Generic.List<ClsKotDetails>();

                    foreach (var detailVm in obj.KotDetails)
                    {
                        var item = oConnectionContext.DbClsItem.Where(a => a.ItemId == detailVm.ItemId).FirstOrDefault();
                        var itemDetails = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == detailVm.ItemDetailsId).FirstOrDefault();
                        
                        // Get UnitId from detailVm, or fallback to item's primary UnitId
                        var unitId = detailVm.UnitId > 0 ? detailVm.UnitId : (item?.UnitId ?? 0);

                        // Get kitchen station for item category (if not provided)
                        long kitchenStationId = detailVm.KitchenStationId;
                        if (kitchenStationId == 0 && item != null && item.CategoryId > 0)
                        {
                            var stationMap = oConnectionContext.DbClsKitchenStationCategoryMap
                                .Where(a => a.CategoryId == item.CategoryId && a.IsActive && a.CompanyId == obj.CompanyId)
                                .Select(a => a.KitchenStationId)
                                .FirstOrDefault();
                            if (stationMap > 0)
                            {
                                kitchenStationId = stationMap;
                            }
                        }

                        ClsKotDetails oDetail = new ClsKotDetails()
                        {
                            KotDetailsId = detailVm.KotDetailsId,
                            KotId = oKot.KotId,
                            ItemId = detailVm.ItemId,
                            ItemDetailsId = detailVm.ItemDetailsId,
                            Quantity = detailVm.Quantity,
                            UnitId = unitId,
                            CookingInstructions = detailVm.CookingInstructions,
                            ItemStatus = detailVm.ItemStatus ?? "Pending",
                            KitchenStationId = kitchenStationId,
                            EstimatedTime = detailVm.EstimatedTime,
                            Priority = detailVm.Priority,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy
                        };

                        if (detailVm.KotDetailsId == 0)
                        {
                            oConnectionContext.DbClsKotDetails.Add(oDetail);
                        }
                        else
                        {
                            var existingDetail = existingDetails.FirstOrDefault(d => d.KotDetailsId == detailVm.KotDetailsId);
                            if (existingDetail != null)
                            {
                                existingDetail.ItemId = oDetail.ItemId;
                                existingDetail.ItemDetailsId = oDetail.ItemDetailsId;
                                existingDetail.Quantity = oDetail.Quantity;
                                existingDetail.UnitId = oDetail.UnitId;
                                existingDetail.CookingInstructions = oDetail.CookingInstructions;
                                existingDetail.ItemStatus = oDetail.ItemStatus;
                                existingDetail.KitchenStationId = oDetail.KitchenStationId;
                                existingDetail.EstimatedTime = oDetail.EstimatedTime;
                                existingDetail.Priority = oDetail.Priority;
                                existingDetail.ModifiedBy = oDetail.ModifiedBy;
                                existingDetail.ModifiedOn = CurrentDate;
                                existingDetail.IsDeleted = false; // ensure restored when editing
                            }
                            else
                            {
                                oConnectionContext.DbClsKotDetails.Add(oDetail);
                            }
                        }
                    }
                    // Mark removed details as deleted
                    foreach (var existingDetail in existingDetails)
                    {
                        if (!obj.KotDetails.Any(d => d.KotDetailsId == existingDetail.KotDetailsId))
                        {
                            existingDetail.IsDeleted = true;
                            existingDetail.ModifiedOn = CurrentDate;
                        }
                    }
                    oConnectionContext.SaveChanges();
                }

                // Update prefix counter if used
                if (PrefixUserMapId > 0 && obj.KotId == 0)
                {
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                }

                // Emit KOT created event
                //await SocketIoHelper.EmitKotCreated(new { KotId = oKot.KotId, KotNo = oKot.KotNo, OrderStatus = oKot.OrderStatus }, obj.CompanyId, obj.BranchId);

                // Emit table status update if table is assigned
                if (oKot.TableId > 0)
                {
                   // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = oKot.TableId, Status = tableStatusHelper.GetTableStatus(oKot.TableId).ToString() }, obj.CompanyId, obj.BranchId);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "KOT",
                    CompanyId = obj.CompanyId,
                    Description = "KOT \"" + obj.KotNo + "\" " + (obj.KotId == 0 ? "created" : "updated"),
                    Id = oKot.KotId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.KotId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Check AutoPrintKot setting (only for new KOTs)
                bool shouldAutoPrint = false;
                if (obj.KotId == 0)
                {
                    var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                        .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false)
                        .FirstOrDefault();
                    
                    if (restaurantSettings != null && restaurantSettings.AutoPrintKot)
                    {
                        shouldAutoPrint = true;
                        // Mark as printed
                        oKot.Printed = true;
                        oKot.PrintedOn = CurrentDate;
                        oKot.PrintedBy = obj.AddedBy;
                        oConnectionContext.SaveChanges();
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "KOT " + (obj.KotId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        Kot = oKot,
                        ShouldAutoPrint = shouldAutoPrint
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CreateFromSales(long salesId, long tableId)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Get Sales order
                var sales = oConnectionContext.DbClsSales.Where(a => a.SalesId == salesId && a.IsDeleted == false).FirstOrDefault();
                if (sales == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Sales order not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var CurrentDate = oCommonController.CurrentDate(sales.CompanyId);

                // Check if KOT already exists for this sales
                var existingKot = oConnectionContext.DbClsKotMaster.Where(a => a.SalesId == salesId && a.IsDeleted == false).FirstOrDefault();
                if (existingKot != null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "KOT already exists for this sales order",
                        Data = new
                        {
                            KotId = existingKot.KotId,
                            KotNo = existingKot.KotNo
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Generate KotNo
                long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == sales.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                      join b in oConnectionContext.DbClsPrefixUserMap
                                       on a.PrefixMasterId equals b.PrefixMasterId
                                      where a.IsActive == true && a.IsDeleted == false &&
                                      b.CompanyId == sales.CompanyId && b.IsActive == true
                                      && b.IsDeleted == false && a.PrefixType.ToLower() == "kot"
                                      && b.PrefixId == PrefixId
                                      select new
                                      {
                                          b.PrefixUserMapId,
                                          b.Prefix,
                                          b.NoOfDigits,
                                          b.Counter
                                      }).FirstOrDefault();

                string kotNo = "";
                long PrefixUserMapId = 0;
                if (prefixSettings != null)
                {
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    kotNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }
                else
                {
                    kotNo = "KOT" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsKotMaster.Where(a => a.CompanyId == sales.CompanyId).Count().ToString().PadLeft(4, '0');
                }

                // Check for active booking on table
                long activeBookingId = 0;
                long kotTableId = tableId > 0 ? tableId : sales.TableId;
                if (kotTableId > 0)
                {
                    var activeBooking = (from b in oConnectionContext.DbClsTableBooking
                                        join bt in oConnectionContext.DbClsTableBookingTable on b.BookingId equals bt.BookingId
                                        where bt.TableId == kotTableId &&
                                              b.CompanyId == sales.CompanyId &&
                                              b.BranchId == sales.BranchId &&
                                              b.Status != "Cancelled" &&
                                              b.Status != "Completed" &&
                                              b.IsDeleted == false &&
                                              b.BookingDate.Date == CurrentDate.Date &&
                                              b.BookingTime <= CurrentDate.TimeOfDay &&
                                              (b.BookingTime.Add(TimeSpan.FromMinutes(b.Duration)) >= CurrentDate.TimeOfDay)
                                        orderby b.BookingDate descending
                                        select b).FirstOrDefault();
                    if (activeBooking != null)
                    {
                        activeBookingId = activeBooking.BookingId;
                    }
                }

                // Create KOT Master
                ClsKotMaster oKot = new ClsKotMaster()
                {
                    KotNo = kotNo,
                    TableId = kotTableId > 0 ? kotTableId : 0,
                    SalesId = salesId,
                    BookingId = activeBookingId > 0 ? activeBookingId : 0,
                    OrderType = kotTableId > 0 ? "DineIn" : "Takeaway",
                    OrderStatus = "Pending",
                    OrderTime = CurrentDate,
                    ExpectedTime = null,
                    ReadyTime = null,
                    ServedTime = null,
                    WaiterId = 0,
                    GuestCount = 0,
                    CustomerId = sales.CustomerId,
                    SpecialInstructions = sales.Notes,
                    BranchId = sales.BranchId,
                    CompanyId = sales.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = sales.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = sales.AddedBy,
                    Printed = false
                };

                oConnectionContext.DbClsKotMaster.Add(oKot);
                oConnectionContext.SaveChanges();

                // Create KOT Details from Sales Details
                var salesDetails = oConnectionContext.DbClsSalesDetails
                    .Where(a => a.SalesId == salesId && a.IsDeleted == false && !a.IsComboItems)
                    .ToList();

                foreach (var salesDetail in salesDetails)
                {
                    var item = oConnectionContext.DbClsItem.Where(a => a.ItemId == salesDetail.ItemId).FirstOrDefault();
                    if (item != null)
                    {
                        // Get kitchen station for item category
                        long? kitchenStationId = null;
                        if (item.CategoryId > 0)
                        {
                            var stationMap = oConnectionContext.DbClsKitchenStationCategoryMap
                                .Where(a => a.CategoryId == item.CategoryId && a.IsActive)
                                .Select(a => a.KitchenStationId)
                                .FirstOrDefault();
                            if (stationMap > 0)
                            {
                                kitchenStationId = stationMap;
                            }
                        }

                        var unitId = item.UnitId;
                        
                        ClsKotDetails oKotDetail = new ClsKotDetails()
                        {
                            KotId = oKot.KotId,
                            ItemId = salesDetail.ItemId,
                            ItemDetailsId = salesDetail.ItemDetailsId,
                            Quantity = salesDetail.Quantity,
                            UnitId = unitId,
                            CookingInstructions = salesDetail.OtherInfo,
                            ItemStatus = "Pending",
                            KitchenStationId = kitchenStationId ?? 0,
                            EstimatedTime = 0,
                            Priority = 0,
                            CompanyId = sales.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = sales.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = sales.AddedBy
                        };

                        oConnectionContext.DbClsKotDetails.Add(oKotDetail);
                    }
                }
                oConnectionContext.SaveChanges();

                // Update prefix counter
                if (PrefixUserMapId > 0)
                {
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                }

                // Link booking to KOT if booking exists
                // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                if (activeBookingId > 0)
                {
                    var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == activeBookingId).FirstOrDefault();
                    if (booking != null)
                    {
                        booking.ModifiedOn = CurrentDate;
                        oConnectionContext.SaveChanges();
                    }
                }

                // Emit events
               // await SocketIoHelper.EmitKotCreated(new { KotId = oKot.KotId, KotNo = oKot.KotNo, OrderStatus = oKot.OrderStatus }, sales.CompanyId, sales.BranchId);
               // await SocketIoHelper.EmitKotLinkedToSales(new { KotId = oKot.KotId, SalesId = salesId }, sales.CompanyId, sales.BranchId);

                if (oKot.TableId > 0)
                {
                   // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = oKot.TableId, Status = tableStatusHelper.GetTableStatus(oKot.TableId).ToString() }, sales.CompanyId, sales.BranchId);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = sales.AddedBy,
                    Browser = "",
                    Category = "KOT",
                    CompanyId = sales.CompanyId,
                    Description = "KOT \"" + kotNo + "\" created from Sales \"" + sales.InvoiceNo + "\"",
                    Id = oKot.KotId,
                    IpAddress = "",
                    Platform = "",
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Check AutoPrintKot setting
                bool shouldAutoPrint = false;
                var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                    .Where(a => a.CompanyId == sales.CompanyId && a.BranchId == sales.BranchId && a.IsDeleted == false)
                    .FirstOrDefault();
                
                if (restaurantSettings != null && restaurantSettings.AutoPrintKot)
                {
                    shouldAutoPrint = true;
                    // Mark as printed
                    oKot.Printed = true;
                    oKot.PrintedOn = CurrentDate;
                    oKot.PrintedBy = sales.AddedBy;
                    oConnectionContext.SaveChanges();
                }

                data = new
                {
                    Status = 1,
                    Message = "KOT created from Sales successfully",
                    Data = new
                    {
                        Kot = oKot,
                        ShouldAutoPrint = shouldAutoPrint
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CreateFromBooking(long bookingId)
        {
            // This method creates KOT from existing Booking
            // Industry Standard: 1 KOT per booking (not per table)
            // Even if booking has multiple tables, only 1 KOT is created
            // The KOT represents the order/items, not the seating arrangement
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

                // Generate KotNo
                long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == booking.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                      join b in oConnectionContext.DbClsPrefixUserMap
                                       on a.PrefixMasterId equals b.PrefixMasterId
                                      where a.IsActive == true && a.IsDeleted == false &&
                                      b.CompanyId == booking.CompanyId && b.IsActive == true
                                      && b.IsDeleted == false && a.PrefixType.ToLower() == "kot"
                                      && b.PrefixId == PrefixId
                                      select new
                                      {
                                          b.PrefixUserMapId,
                                          b.Prefix,
                                          b.NoOfDigits,
                                          b.Counter
                                      }).FirstOrDefault();

                string kotNo = "";
                long PrefixUserMapId = 0;
                if (prefixSettings != null)
                {
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    kotNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }
                else
                {
                    kotNo = "KOT" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsKotMaster.Where(a => a.CompanyId == booking.CompanyId).Count().ToString().PadLeft(4, '0');
                }

                // Get primary table ID from junction table
                // Note: Even if booking has multiple tables, we use only the primary table
                // for the KOT. This follows industry standard of 1 KOT per order/booking.
                var primaryTable = oConnectionContext.DbClsTableBookingTable
                    .Where(bt => bt.BookingId == bookingId && bt.IsPrimary)
                    .Select(bt => bt.TableId)
                    .FirstOrDefault();
                
                // If no primary table, get first table (by display order)
                if (primaryTable == 0)
                {
                    primaryTable = oConnectionContext.DbClsTableBookingTable
                        .Where(bt => bt.BookingId == bookingId)
                        .OrderBy(bt => bt.DisplayOrder)
                        .Select(bt => bt.TableId)
                        .FirstOrDefault();
                }

                ClsKotMaster oKot = new ClsKotMaster()
                {
                    KotNo = kotNo,
                    TableId = primaryTable > 0 ? primaryTable : 0,
                    BookingId = bookingId,
                    SalesId = 0,
                    OrderType = "DineIn",
                    OrderStatus = "Pending",
                    OrderTime = CurrentDate,
                    GuestCount = booking.NoOfGuests,
                    CustomerId = booking.CustomerId,
                    WaiterId = booking.WaiterId > 0 ? booking.WaiterId : 0,  // Auto-assign booking's waiter (industry standard)
                    BranchId = booking.BranchId,
                    CompanyId = booking.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = booking.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = booking.AddedBy,
                    Printed = false
                };

                oConnectionContext.DbClsKotMaster.Add(oKot);
                oConnectionContext.SaveChanges();

                // Link booking to KOT
                // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                booking.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Update prefix counter
                if (PrefixUserMapId > 0)
                {
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                }

                // Emit events
               // await SocketIoHelper.EmitKotCreated(new { KotId = oKot.KotId, KotNo = oKot.KotNo }, booking.CompanyId, booking.BranchId);
               // await SocketIoHelper.EmitBookingLinkedToKot(new { BookingId = bookingId, KotId = oKot.KotId }, booking.CompanyId, booking.BranchId);

                // Check AutoPrintKot setting
                bool shouldAutoPrint = false;
                var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                    .Where(a => a.CompanyId == booking.CompanyId && a.BranchId == booking.BranchId && a.IsDeleted == false)
                    .FirstOrDefault();
                
                if (restaurantSettings != null && restaurantSettings.AutoPrintKot)
                {
                    shouldAutoPrint = true;
                    // Mark as printed
                    oKot.Printed = true;
                    oKot.PrintedOn = CurrentDate;
                    oKot.PrintedBy = booking.AddedBy;
                    oConnectionContext.SaveChanges();
                }

                data = new
                {
                    Status = 1,
                    Message = "KOT created from booking successfully",
                    Data = new
                    {
                        Kot = oKot,
                        ShouldAutoPrint = shouldAutoPrint
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateKotStatus(ClsKotMasterVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsKotMaster oKot = new ClsKotMaster()
                {
                    KotId = obj.KotId,
                    OrderStatus = obj.OrderStatus,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };

                if (obj.OrderStatus == "Ready")
                {
                    oKot.ReadyTime = CurrentDate;
                }
                else if (obj.OrderStatus == "Served")
                {
                    oKot.ServedTime = CurrentDate;
                }

                oConnectionContext.DbClsKotMaster.Attach(oKot);
                oConnectionContext.Entry(oKot).Property(x => x.KotId).IsModified = true;
                oConnectionContext.Entry(oKot).Property(x => x.OrderStatus).IsModified = true;
                oConnectionContext.Entry(oKot).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oKot).Property(x => x.ModifiedOn).IsModified = true;
                if (obj.OrderStatus == "Ready")
                {
                    oConnectionContext.Entry(oKot).Property(x => x.ReadyTime).IsModified = true;
                }
                if (obj.OrderStatus == "Served")
                {
                    oConnectionContext.Entry(oKot).Property(x => x.ServedTime).IsModified = true;
                }

                // When the overall KOT status moves forward, mirror the same
                // status on individual KOT items so the KDS shows the latest state.
                if (obj.OrderStatus == "Preparing" || obj.OrderStatus == "Ready" || obj.OrderStatus == "Served")
                {
                    var kotDetails = oConnectionContext.DbClsKotDetails
                        .Where(d => d.KotId == obj.KotId && d.IsDeleted == false)
                        .ToList();

                    foreach (var detail in kotDetails)
                    {
                        detail.ItemStatus = obj.OrderStatus;
                        detail.ModifiedBy = obj.AddedBy;
                        detail.ModifiedOn = CurrentDate;

                        if (obj.OrderStatus == "Preparing")
                        {
                            // Track first time the item started cooking
                            detail.StartedCookingAt = detail.StartedCookingAt ?? CurrentDate;
                        }
                        else if (obj.OrderStatus == "Ready")
                        {
                            // Record first ready timestamp if not already set
                            detail.ReadyAt = detail.ReadyAt ?? CurrentDate;
                        }
                        else if (obj.OrderStatus == "Served")
                        {
                            detail.ServedAt = CurrentDate;
                        }
                    }
                }

                oConnectionContext.SaveChanges();

                // Emit status update
                var kot = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == obj.KotId).Select(a => new { a.CompanyId, a.BranchId, a.TableId }).FirstOrDefault();
                if (kot != null)
                {
                   // await SocketIoHelper.EmitKotStatusUpdated(new { KotId = obj.KotId, OrderStatus = obj.OrderStatus }, kot.CompanyId, kot.BranchId);
                    if (kot.TableId > 0)
                    {
                       // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = kot.TableId, Status = tableStatusHelper.GetTableStatus(kot.TableId).ToString() }, kot.CompanyId, kot.BranchId);
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "KOT status updated successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateItemStatus(ClsKotDetailsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsKotDetails oDetail = new ClsKotDetails()
                {
                    KotDetailsId = obj.KotDetailsId,
                    ItemStatus = obj.ItemStatus,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };

                if (obj.ItemStatus == "Preparing")
                {
                    oDetail.StartedCookingAt = CurrentDate;
                }
                else if (obj.ItemStatus == "Ready")
                {
                    oDetail.ReadyAt = CurrentDate;
                }
                else if (obj.ItemStatus == "Served")
                {
                    oDetail.ServedAt = CurrentDate;
                }

                oConnectionContext.DbClsKotDetails.Attach(oDetail);
                oConnectionContext.Entry(oDetail).Property(x => x.KotDetailsId).IsModified = true;
                oConnectionContext.Entry(oDetail).Property(x => x.ItemStatus).IsModified = true;
                oConnectionContext.Entry(oDetail).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oDetail).Property(x => x.ModifiedOn).IsModified = true;
                if (obj.ItemStatus == "Preparing")
                {
                    oConnectionContext.Entry(oDetail).Property(x => x.StartedCookingAt).IsModified = true;
                }
                if (obj.ItemStatus == "Ready")
                {
                    oConnectionContext.Entry(oDetail).Property(x => x.ReadyAt).IsModified = true;
                }
                if (obj.ItemStatus == "Served")
                {
                    oConnectionContext.Entry(oDetail).Property(x => x.ServedAt).IsModified = true;
                }
                oConnectionContext.SaveChanges();

                // Emit item status update
                var detail = oConnectionContext.DbClsKotDetails.Where(a => a.KotDetailsId == obj.KotDetailsId)
                    .Join(oConnectionContext.DbClsKotMaster, d => d.KotId, k => k.KotId, (d, k) => new { k.CompanyId, k.BranchId, d.KitchenStationId })
                    .FirstOrDefault();

                if (detail != null)
                {
                   // await SocketIoHelper.EmitItemStatusUpdated(new { KotDetailsId = obj.KotDetailsId, ItemStatus = obj.ItemStatus }, detail.CompanyId, detail.BranchId, detail.KitchenStationId);
                }

                data = new
                {
                    Status = 1,
                    Message = "Item status updated successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetActiveKots(ClsKotMasterVm obj)
        {
            var query = oConnectionContext.DbClsKotMaster
                .Where(a => a.CompanyId == obj.CompanyId && 
                       a.OrderStatus != "Served" && 
                       a.OrderStatus != "Cancelled" &&
                       a.IsDeleted == false);
            
            // Filter by BranchId only if provided (not "All")
            if (obj.BranchId > 0)
            {
                query = query.Where(a => a.BranchId == obj.BranchId);
            }
            
            // Filter by KitchenStationId if provided
            if (obj.KitchenStationId > 0)
            {
                // Only include KOTs that have at least one detail for the selected station
                var kotIdsWithStation = oConnectionContext.DbClsKotDetails
                    .Where(d => d.KitchenStationId == obj.KitchenStationId && d.IsDeleted == false)
                    .Select(d => d.KotId)
                    .Distinct()
                    .ToList();
                
                query = query.Where(a => kotIdsWithStation.Contains(a.KotId));
            }
            
            var det = query.Select(a => new
                {
                    KotId = a.KotId,
                    KotNo = a.KotNo,
                    TableId = a.TableId,
                    OrderStatus = a.OrderStatus,
                    OrderTime = a.OrderTime,
                    ExpectedTime = a.ExpectedTime,
                    GuestCount = a.GuestCount,
                    SpecialInstructions = a.SpecialInstructions,
                    TableNo = a.TableId > 0 ? oConnectionContext.DbClsRestaurantTable.Where(t => t.TableId == a.TableId).Select(t => t.TableNo).FirstOrDefault() : null,
                    KotDetails = oConnectionContext.DbClsKotDetails
                        .Where(d => d.KotId == a.KotId && d.IsDeleted == false && 
                               (obj.KitchenStationId == 0 || d.KitchenStationId == obj.KitchenStationId))
                        .Select(d => new
                        {
                            KotDetailsId = d.KotDetailsId,
                            ItemName = oConnectionContext.DbClsItem.Where(i => i.ItemId == d.ItemId).Select(i => i.ItemName).FirstOrDefault(),
                            Quantity = d.Quantity,
                            Unit = oConnectionContext.DbClsUnit.Where(u => u.UnitId == d.UnitId).Select(u => u.UnitName).FirstOrDefault(),
                            CookingInstructions = d.CookingInstructions,
                            ItemStatus = d.ItemStatus,
                            KitchenStationId = d.KitchenStationId,
                            KitchenStationName = d.KitchenStationId > 0 ? oConnectionContext.DbClsKitchenStation.Where(s => s.KitchenStationId == d.KitchenStationId).Select(s => s.StationName).FirstOrDefault() : null,
                            EstimatedTime = d.EstimatedTime,
                            Priority = d.Priority
                        })
                        .ToList()
                })
                .OrderByDescending(a => a.OrderTime)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Kots = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> LinkToSales([FromBody] ClsKotMasterVm obj)
        {
            long kotId = obj.KotId;
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
                var kot = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == kotId).FirstOrDefault();
                if (kot == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "KOT not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
                var CurrentDate = oCommonController.CurrentDate(kot.CompanyId);

                kot.SalesId = salesId;
                kot.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Emit linking event
               // await SocketIoHelper.EmitKotLinkedToSales(new { KotId = kotId, SalesId = salesId }, kot.CompanyId, kot.BranchId);

                data = new
                {
                    Status = 1,
                    Message = "KOT linked to Sales successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> LinkToBooking([FromBody] ClsKotMasterVm obj)
        {
            long kotId = obj.KotId;
            if (obj.BookingId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
            long bookingId = obj.BookingId;
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var kot = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == kotId).FirstOrDefault();
                if (kot == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "KOT not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
                var CurrentDate = oCommonController.CurrentDate(kot.CompanyId);

                kot.BookingId = bookingId;
                kot.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Link booking to KOT
                // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == bookingId).FirstOrDefault();
                if (booking != null)
                {
                    booking.ModifiedOn = CurrentDate;
                    oConnectionContext.SaveChanges();
                }

                // Emit linking event
               // await SocketIoHelper.EmitBookingLinkedToKot(new { BookingId = bookingId, KotId = kotId }, kot.CompanyId, kot.BranchId);

                data = new
                {
                    Status = 1,
                    Message = "KOT linked to Booking successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Converts multiple served KOTs to a single consolidated sales invoice
        /// Industry Standard: All served KOTs from a booking/table are consolidated into one final bill
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> ConvertKotsToSales([FromBody] ClsKotMasterVm obj)
        {
            // obj.KotIds should contain list of KotIds to convert
            if (obj.KotIds == null || obj.KotIds.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "At least one KOT ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Get all KOTs
                var kots = oConnectionContext.DbClsKotMaster
                    .Where(a => obj.KotIds.Contains(a.KotId) && 
                           a.CompanyId == obj.CompanyId && 
                           a.IsDeleted == false)
                    .ToList();

                if (kots.Count == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "No valid KOTs found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Validate all KOTs are served
                var notServedKots = kots.Where(k => k.OrderStatus != "Served").ToList();
                if (notServedKots.Any())
                {
                    data = new
                    {
                        Status = 0,
                        Message = $"Cannot convert KOTs that are not served. KOTs {string.Join(", ", notServedKots.Select(k => k.KotNo))} are not served.",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Check if any KOT is already linked to sales
                var alreadyLinkedKots = kots.Where(k => k.SalesId > 0).ToList();
                if (alreadyLinkedKots.Any())
                {
                    data = new
                    {
                        Status = 0,
                        Message = $"Some KOTs are already linked to sales invoices: {string.Join(", ", alreadyLinkedKots.Select(k => k.KotNo))}",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Get first KOT for common properties
                var firstKot = kots.First();
                long bookingId = firstKot.BookingId;
                long tableId = firstKot.TableId;
                long customerId = firstKot.CustomerId > 0 ? firstKot.CustomerId : 0;
                long branchId = firstKot.BranchId;
                long companyId = firstKot.CompanyId;

                // If booking exists, get customer from booking
                if (bookingId > 0)
                {
                    var booking = oConnectionContext.DbClsTableBooking
                        .Where(b => b.BookingId == bookingId && b.IsDeleted == false)
                        .FirstOrDefault();
                    if (booking != null && booking.CustomerId > 0)
                    {
                        customerId = booking.CustomerId;
                    }
                }

                if (customerId == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Customer information is required. Please ensure KOTs have customer assigned or linked to a booking.",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Get all KOT details and consolidate items
                var allKotDetails = oConnectionContext.DbClsKotDetails
                    .Where(d => obj.KotIds.Contains(d.KotId) && d.IsDeleted == false)
                    .ToList();

                if (allKotDetails.Count == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "No items found in selected KOTs",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Consolidate items: Group by ItemId + ItemDetailsId, sum quantities
                var consolidatedItems = allKotDetails
                    .GroupBy(d => new { d.ItemId, d.ItemDetailsId })
                    .Select(g => new
                    {
                        ItemId = g.Key.ItemId,
                        ItemDetailsId = g.Key.ItemDetailsId,
                        TotalQuantity = g.Sum(x => x.Quantity),
                        UnitId = g.First().UnitId,
                        CookingInstructions = string.Join("; ", g.Where(x => !string.IsNullOrEmpty(x.CookingInstructions)).Select(x => x.CookingInstructions).Distinct())
                    })
                    .ToList();

                // Get item pricing information
                var itemIds = consolidatedItems.Select(i => i.ItemId).Distinct().ToList();
                var items = oConnectionContext.DbClsItem
                    .Where(i => itemIds.Contains(i.ItemId) && i.IsDeleted == false)
                    .ToList();

                var itemDetailsIds = consolidatedItems.Select(i => i.ItemDetailsId).Distinct().ToList();
                var itemDetails = oConnectionContext.DbClsItemDetails
                    .Where(id => itemDetailsIds.Contains(id.ItemDetailsId) && id.IsDeleted == false)
                    .ToList();

                // Get customer
                var customer = oConnectionContext.DbClsUser
                    .Where(u => u.UserId == customerId && u.IsDeleted == false)
                    .FirstOrDefault();

                if (customer == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Customer not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Get customer's selling price group - check if customer has a price group assigned
                long sellingPriceGroupId = 0;
                var customerPriceGroup = oConnectionContext.DbClsSellingPriceGroup
                    .Where(spg => spg.CompanyId == companyId && spg.IsDeleted == false)
                    .OrderBy(spg => spg.SellingPriceGroupId)
                    .Select(spg => spg.SellingPriceGroupId)
                    .FirstOrDefault();
                
                if (customerPriceGroup > 0)
                {
                    sellingPriceGroupId = customerPriceGroup;
                }

                // Get default tax
                var defaultTax = oConnectionContext.DbClsTax
                    .Where(t => t.CompanyId == companyId && t.IsDeleted == false)
                    .OrderBy(t => t.TaxId)
                    .FirstOrDefault();

                long taxId = defaultTax != null ? defaultTax.TaxId : 0;

                // Build sales details with pricing
                List<ClsSalesDetails> salesDetailsList = new List<ClsSalesDetails>();
                decimal subtotal = 0;
                decimal totalQuantity = 0;
                decimal totalTaxAmount = 0;

                foreach (var consolidatedItem in consolidatedItems)
                {
                    var item = items.FirstOrDefault(i => i.ItemId == consolidatedItem.ItemId);
                    var itemDetail = itemDetails.FirstOrDefault(id => id.ItemDetailsId == consolidatedItem.ItemDetailsId);

                    if (item == null || itemDetail == null) continue;

                    // Get item branch map for stock quantity
                    var itemBranchMap = oConnectionContext.DbClsItemBranchMap
                        .Where(ibm => ibm.ItemDetailsId == consolidatedItem.ItemDetailsId && 
                               ibm.BranchId == branchId && ibm.IsDeleted == false)
                        .FirstOrDefault();

                    if (itemBranchMap == null) continue;

                    // Get selling price - use ItemDetails prices, or price group if available
                    decimal unitPriceIncTax = itemDetail.SalesIncTax;
                    decimal unitPriceExcTax = itemDetail.SalesExcTax;
                    
                    // If branch has override price, use that
                    if (itemBranchMap.SalesIncTax > 0)
                    {
                        unitPriceIncTax = itemBranchMap.SalesIncTax;
                        // Calculate exc tax from inc tax if needed
                        unitPriceExcTax = itemDetail.SalesExcTax;
                    }
                    
                    // Check price group pricing
                    if (sellingPriceGroupId > 0)
                    {
                        var priceGroupMap = oConnectionContext.DbClsItemSellingPriceGroupMap
                            .Where(ispgm => ispgm.ItemDetailsId == consolidatedItem.ItemDetailsId &&
                                   ispgm.SellingPriceGroupId == sellingPriceGroupId &&
                                   ispgm.IsDeleted == false)
                            .FirstOrDefault();
                        if (priceGroupMap != null && priceGroupMap.SellingPrice > 0)
                        {
                            // Price group has SellingPrice (which is typically inc tax)
                            unitPriceIncTax = priceGroupMap.SellingPrice;
                            // Calculate exc tax - approximate by using same ratio
                            if (itemDetail.SalesIncTax > 0)
                            {
                                decimal ratio = itemDetail.SalesExcTax / itemDetail.SalesIncTax;
                                unitPriceExcTax = unitPriceIncTax * ratio;
                            }
                            else
                            {
                                unitPriceExcTax = unitPriceIncTax;
                            }
                        }
                    }

                    // Get tax for item - from Item, not ItemBranchMap
                    long itemTaxId = taxId;
                    if (item.TaxId > 0)
                    {
                        itemTaxId = item.TaxId;
                    }

                    var tax = oConnectionContext.DbClsTax
                        .Where(t => t.TaxId == itemTaxId && t.IsDeleted == false)
                        .FirstOrDefault();

                    decimal taxPercent = tax != null ? tax.TaxPercent : 0;
                    decimal amountExcTax = unitPriceExcTax * consolidatedItem.TotalQuantity;
                    decimal taxAmount = amountExcTax * (taxPercent / 100);
                    decimal amountIncTax = amountExcTax + taxAmount;

                    // Derive per-unit exclusive/inclusive prices the same way Sales/Add does,
                    // so that (PriceIncTax - PriceExcTax) always reflects the correct per-unit tax.
                    decimal priceExcTaxPerUnit = consolidatedItem.TotalQuantity > 0
                        ? amountExcTax / consolidatedItem.TotalQuantity
                        : unitPriceExcTax;
                    decimal priceIncTaxPerUnit = consolidatedItem.TotalQuantity > 0
                        ? amountIncTax / consolidatedItem.TotalQuantity
                        : priceExcTaxPerUnit;

                    subtotal += amountIncTax;
                    totalQuantity += consolidatedItem.TotalQuantity;
                    totalTaxAmount += taxAmount;

                    decimal availableQuantity = itemBranchMap.Quantity;
                    decimal quantityRemaining = availableQuantity >= consolidatedItem.TotalQuantity ? 
                        (availableQuantity - consolidatedItem.TotalQuantity) : 0;

                    salesDetailsList.Add(new ClsSalesDetails
                    {
                        ItemId = consolidatedItem.ItemId,
                        ItemDetailsId = consolidatedItem.ItemDetailsId,
                        Quantity = consolidatedItem.TotalQuantity,
                        // Align KOT → Sales with Sales/Add: use per-unit selling price before tax as UnitCost
                        UnitCost = priceExcTaxPerUnit,
                        // These drive the per-unit tax column on the invoice (PriceIncTax - PriceExcTax)
                        PriceExcTax = priceExcTaxPerUnit,
                        PriceIncTax = priceIncTaxPerUnit,
                        AmountExcTax = amountExcTax,
                        TaxAmount = taxAmount,
                        AmountIncTax = amountIncTax,
                        TaxId = itemTaxId,
                        Discount = 0,
                        DiscountType = "Amount",
                        OtherInfo = consolidatedItem.CookingInstructions,
                        CompanyId = companyId,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        ModifiedBy = obj.AddedBy,
                        QuantityRemaining = quantityRemaining,
                        WarrantyId = 0,
                        // Keep purchase price only for costing, like normal sales
                        DefaultUnitCost = itemDetail.PurchaseIncTax,
                        DefaultAmount = itemDetail.PurchaseIncTax * consolidatedItem.TotalQuantity,
                        PriceAddedFor = 1,
                        LotId = 0,
                        LotType = "",
                        FreeQuantity = 0,
                        Under = 0,
                        UnitAddedFor = 1,
                        LotIdForLotNoChecking = 0,
                        LotTypeForLotNoChecking = "",
                        ComboId = "",
                        IsComboItems = false,
                        QuantitySold = consolidatedItem.TotalQuantity,
                        ComboPerUnitQuantity = 0,
                        AccountId = 0,
                        DiscountAccountId = 0,
                        TaxAccountId = 0,
                        PurchaseAccountId = 0,
                        InventoryAccountId = 0,
                        ExtraDiscount = 0,
                        ItemCodeId = 0,
                        TaxExemptionId = 0,
                        TotalTaxAmount = taxAmount,
                        IsCombo = false
                    });
                }

                if (salesDetailsList.Count == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "No valid items found to create sales invoice",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Calculate totals
                decimal totalDiscount = 0;
                decimal grandTotal = subtotal - totalDiscount;
                decimal netAmount = grandTotal;

                // Get sales prefix - Use same logic as InsertSales API
                // Hybrid approach: Check Customer PrefixId first, then fall back to Branch PrefixId
                long PrefixId = 0;
                long customerPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == customerId && a.CompanyId == companyId).Select(a => a.PrefixId).FirstOrDefault();
                
                if (customerPrefixId != 0)
                {
                    // Use Customer's PrefixId if set
                    PrefixId = customerPrefixId;
                }
                else
                {
                    // Fall back to Branch PrefixId
                    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == branchId).Select(a => a.PrefixId).FirstOrDefault();
                }

                string PrefixType = "Sales"; // SalesType - matching InsertSales API logic
                var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                      join b in oConnectionContext.DbClsPrefixUserMap
                                      on a.PrefixMasterId equals b.PrefixMasterId
                                      where a.IsActive == true && a.IsDeleted == false &&
                                      b.CompanyId == companyId && b.IsActive == true &&
                                      b.IsDeleted == false && a.PrefixType.ToLower() == PrefixType.ToLower() &&
                                      b.PrefixId == PrefixId
                                      select new
                                      {
                                          b.PrefixUserMapId,
                                          b.Prefix,
                                          b.NoOfDigits,
                                          b.Counter
                                      }).FirstOrDefault();

                string invoiceNo = "";
                long SalesPrefixUserMapId = 0;
                if (prefixSettings != null)
                {
                    SalesPrefixUserMapId = prefixSettings.PrefixUserMapId;
                    invoiceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                // Create sales invoice
                ClsSales oClsSales = new ClsSales
                {
                    BranchId = branchId,
                    CustomerId = customerId,
                    SellingPriceGroupId = sellingPriceGroupId,
                    SalesDate = CurrentDate,
                    Status = "Draft",
                    InvoiceNo = invoiceNo,
                    Subtotal = subtotal,
                    TaxId = taxId,
                    TaxAmount = totalTaxAmount,
                    TotalQuantity = totalQuantity,
                    Discount = 0,
                    DiscountType = "Amount",
                    TotalDiscount = totalDiscount,
                    GrandTotal = grandTotal,
                    NetAmount = netAmount,
                    TotalTaxAmount = totalTaxAmount,
                    Notes = $"Consolidated from KOTs: {string.Join(", ", kots.Select(k => k.KotNo))}",
                    SalesType = "Sales",
                    TotalPaying = 0,
                    Balance = grandTotal,
                    ChangeReturn = 0,
                    PaymentType = "",
                    CompanyId = companyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    PrefixId = PrefixId,
                    TableId = tableId,
                    BookingId = bookingId,
                    KotId = firstKot.KotId, // Store first KOT ID for reference
                    CashRegisterId = 0,
                    RoundOff = 0,
                    AccountId = 0,
                    DiscountAccountId = 0,
                    RoundOffAccountId = 0,
                    TaxAccountId = 0,
                    UserGroupId = 0,
                    ReferenceId = 0,
                    ReferenceType = "",
                    PaymentTermId = 0,
                    DueDate = CurrentDate,
                    PlaceOfSupplyId = 0,
                    TaxExemptionId = 0,
                    ParentId = 0,
                    IsWriteOff = false,
                    IsBusinessRegistered = 0,
                    GstTreatment = "",
                    BusinessRegistrationNameId = 0,
                    BusinessRegistrationNo = "",
                    BusinessLegalName = "",
                    BusinessTradeName = "",
                    PanNo = "",
                    IsReverseCharge = 0,
                    IsCancelled = false,
                    GstPayment = "",
                    SalesDebitNoteReasonId = 0,
                    NetAmountReverseCharge = 0,
                    RoundOffReverseCharge = 0,
                    GrandTotalReverseCharge = 0,
                    TaxableAmount = subtotal,
                    PayTaxForExport = 0,
                    TaxCollectedFromCustomer = 1,
                    SpecialDiscount = 0,
                    SpecialDiscountAccountId = 0,
                    Terms = "",
                    RecurringSalesId = 0,
                    RedeemPoints = 0,
                    PointsEarned = 0,
                    PointsDiscount = 0,
                    OnlinePaymentSettingsId = 0,
                    ExchangeRate = 1,
                    SmsSettingsId = 0,
                    EmailSettingsId = 0,
                    WhatsappSettingsId = 0,
                    InvoiceId = oCommonController.CreateToken()
                };

                oConnectionContext.DbClsSales.Add(oClsSales);
                oConnectionContext.SaveChanges();

                // Create sales details
                foreach (var salesDetail in salesDetailsList)
                {
                    salesDetail.SalesId = oClsSales.SalesId;
                    oConnectionContext.DbClsSalesDetails.Add(salesDetail);
                }
                oConnectionContext.SaveChanges();

                // Link all KOTs to the sales invoice
                foreach (var kot in kots)
                {
                    kot.SalesId = oClsSales.SalesId;
                    kot.ModifiedBy = obj.AddedBy;
                    kot.ModifiedOn = CurrentDate;
                }
                oConnectionContext.SaveChanges();

                // Update booking if exists
                if (bookingId > 0)
                {
                    var booking = oConnectionContext.DbClsTableBooking
                        .Where(b => b.BookingId == bookingId && b.IsDeleted == false)
                        .FirstOrDefault();
                    if (booking != null)
                    {
                        // Do not override the primary/first sales link for the booking.
                        // Only set SalesId if this is the first invoice linked to the booking.
                        if (booking.SalesId == 0)
                        {
                            booking.SalesId = oClsSales.SalesId;
                        }
                        // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                        booking.ModifiedBy = obj.AddedBy;
                        booking.ModifiedOn = CurrentDate;
                        oConnectionContext.SaveChanges();
                    }
                }

                // Update prefix counter
                if (SalesPrefixUserMapId > 0)
                {
                    string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + SalesPrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                }

                data = new
                {
                    Status = 1,
                    Message = $"Successfully converted {kots.Count} KOT(s) to sales invoice",
                    Data = new
                    {
                        SalesId = oClsSales.SalesId,
                        InvoiceNo = oClsSales.InvoiceNo,
                        GrandTotal = oClsSales.GrandTotal,
                        KotIds = kots.Select(k => k.KotId).ToList()
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Gets all KOTs for a specific booking
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GetKotsByBooking(ClsKotMasterVm obj)
        {
            if (obj.BookingId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var kots = oConnectionContext.DbClsKotMaster
                .Where(a => a.BookingId == obj.BookingId &&
                            a.CompanyId == obj.CompanyId &&
                            a.IsDeleted == false)
                .OrderByDescending(a => a.OrderTime)
                .Select(a => new ClsKotMasterVm
                {
                    KotId = a.KotId,
                    KotNo = a.KotNo,
                    OrderStatus = a.OrderStatus,
                    OrderTime = a.OrderTime,
                    TableId = a.TableId,
                    BookingId = a.BookingId,
                    SalesId = a.SalesId,
                    // Fetch Sales invoice number so UI can show Sales No beside each KOT
                    SalesNo = oConnectionContext.DbClsSales
                        .Where(s => s.SalesId == a.SalesId && s.IsDeleted == false)
                        .Select(s => s.InvoiceNo)
                        .FirstOrDefault(),
                    GuestCount = a.GuestCount,
                    OrderType = a.OrderType
                })
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Kots = kots
                }
            };
            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Converts all served KOTs from a booking to a single sales invoice
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> ConvertBookingKotsToSales([FromBody] ClsKotMasterVm obj)
        {
            if (obj.BookingId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Booking ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Get all served KOTs for this booking that are not yet linked to sales
            var servedKots = oConnectionContext.DbClsKotMaster
                .Where(k => k.BookingId == obj.BookingId &&
                       k.CompanyId == obj.CompanyId &&
                       k.IsDeleted == false &&
                       k.OrderStatus == "Served" &&
                       k.SalesId == 0)
                .Select(k => k.KotId)
                .ToList();

            if (servedKots.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No served KOTs found for this booking that are not already linked to sales",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Use the existing ConvertKotsToSales logic
            obj.KotIds = servedKots;
            return await ConvertKotsToSales(obj);
        }

        /// <summary>
        /// Soft deletes a KOT and its details
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> DeleteKot(ClsKotMasterVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var kot = oConnectionContext.DbClsKotMaster
                    .Where(a => a.KotId == obj.KotId && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                    .FirstOrDefault();

                if (kot == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "KOT not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Check if KOT is linked to sales - prevent deletion if linked
                if (kot.SalesId > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete KOT as it is linked to a sales invoice",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Soft delete all KOT details
                var kotDetails = oConnectionContext.DbClsKotDetails
                    .Where(d => d.KotId == obj.KotId && d.IsDeleted == false)
                    .ToList();

                foreach (var detail in kotDetails)
                {
                    detail.IsDeleted = true;
                    detail.ModifiedBy = obj.AddedBy;
                    detail.ModifiedOn = CurrentDate;
                }

                // Soft delete KOT master
                kot.IsDeleted = true;
                kot.ModifiedBy = obj.AddedBy;
                kot.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                // Activity log
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser ?? "",
                    Category = "Kot",
                    CompanyId = obj.CompanyId,
                    Description = "KOT deleted",
                    Id = obj.KotId,
                    IpAddress = obj.IpAddress ?? "",
                    Platform = obj.Platform ?? "",
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "KOT deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetCategoryWiseKotPerformance(ClsKotMasterVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var kots = oConnectionContext.DbClsKotMaster
                    .Where(k => k.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || k.BranchId == obj.BranchId) &&
                               k.IsDeleted == false &&
                               k.OrderTime >= obj.FromDate.Value &&
                               k.OrderTime <= obj.ToDate.Value)
                    .ToList();

                var kotIds = kots.Select(k => k.KotId).ToList();
                var kotDetails = oConnectionContext.DbClsKotDetails
                    .Where(kd => kotIds.Contains(kd.KotId) && kd.IsDeleted == false)
                    .ToList();

                var categoryPerformance = kotDetails
                    .GroupBy(kd => 
                    {
                        var item = oConnectionContext.DbClsItem
                            .Where(i => i.ItemId == kd.ItemId)
                            .FirstOrDefault();
                        if (item != null && item.CategoryId > 0)
                        {
                            var category = oConnectionContext.DbClsCategory
                                .Where(c => c.CategoryId == item.CategoryId)
                                .Select(c => c.Category)
                                .FirstOrDefault();
                            return category ?? "Uncategorized";
                        }
                        return "Uncategorized";
                    })
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalKots = g.Select(kd => kd.KotId).Distinct().Count(),
                        TotalQuantity = g.Sum(kd => kd.Quantity),
                        TotalItems = g.Count(),
                        AveragePreparationTime = g.Where(kd => kd.ReadyAt.HasValue && kd.StartedCookingAt.HasValue)
                            .Average(kd => (double?)(kd.ReadyAt.Value - kd.StartedCookingAt.Value).TotalMinutes) ?? 0,
                        CompletedItems = g.Count(kd => kd.ItemStatus.ToLower() == "served" || kd.ItemStatus.ToLower() == "completed"),
                        PendingItems = g.Count(kd => kd.ItemStatus.ToLower() == "pending" || kd.ItemStatus.ToLower() == "preparing")
                    })
                    .OrderByDescending(c => c.TotalItems)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        CategoryPerformance = categoryPerformance,
                        TotalCount = categoryPerformance.Count
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
                        CategoryPerformance = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetHourlyRevenueReport(ClsKotMasterVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var kots = oConnectionContext.DbClsKotMaster
                    .Where(k => k.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || k.BranchId == obj.BranchId) &&
                               k.IsDeleted == false &&
                               k.SalesId > 0 &&
                               k.OrderTime >= obj.FromDate.Value &&
                               k.OrderTime <= obj.ToDate.Value)
                    .ToList();

                var salesIds = kots.Select(k => k.SalesId).Distinct().ToList();
                var sales = oConnectionContext.DbClsSales
                    .Where(s => salesIds.Contains(s.SalesId) && s.IsDeleted == false && s.IsCancelled == false)
                    .ToList();

                var hourlyRevenue = kots
                    .GroupBy(k => k.OrderTime.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        HourLabel = $"{g.Key:00}:00 - {(g.Key + 1):00}:00",
                        TotalKots = g.Count(),
                        TotalRevenue = sales
                            .Where(s => g.Select(k => k.SalesId).Contains(s.SalesId))
                            .Sum(s => (decimal?)s.GrandTotal) ?? 0,
                        AverageOrderValue = sales
                            .Where(s => g.Select(k => k.SalesId).Contains(s.SalesId))
                            .Average(s => (decimal?)s.GrandTotal) ?? 0,
                        TotalOrders = sales
                            .Where(s => g.Select(k => k.SalesId).Contains(s.SalesId))
                            .Count()
                    })
                    .OrderBy(h => h.Hour)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        HourlyRevenue = hourlyRevenue,
                        TotalCount = hourlyRevenue.Count
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
                        HourlyRevenue = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetKotStatusTransitionReport(ClsKotMasterVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var kots = oConnectionContext.DbClsKotMaster
                    .Where(k => k.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || k.BranchId == obj.BranchId) &&
                               k.IsDeleted == false &&
                               k.OrderTime >= obj.FromDate.Value &&
                               k.OrderTime <= obj.ToDate.Value)
                    .ToList();

                var statusTransitions = kots
                    .Select(k => new
                    {
                        KotId = k.KotId,
                        KotNo = k.KotNo,
                        TableNo = oConnectionContext.DbClsRestaurantTable
                            .Where(t => t.TableId == k.TableId)
                            .Select(t => t.TableNo)
                            .FirstOrDefault() ?? "N/A",
                        OrderTime = k.OrderTime,
                        CurrentStatus = k.OrderStatus,
                        ReadyTime = k.ReadyTime,
                        ServedTime = k.ServedTime,
                        TimeToReady = k.ReadyTime.HasValue ? (k.ReadyTime.Value - k.OrderTime).TotalMinutes : (double?)null,
                        TimeToServed = k.ServedTime.HasValue ? (k.ServedTime.Value - k.OrderTime).TotalMinutes : (double?)null,
                        TotalTime = k.ServedTime.HasValue ? (k.ServedTime.Value - k.OrderTime).TotalMinutes : (double?)null
                    })
                    .OrderByDescending(s => s.OrderTime)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        StatusTransitions = statusTransitions,
                        TotalCount = statusTransitions.Count
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
                        StatusTransitions = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetStaffPerformanceReport(ClsKotMasterVm obj)
        {
            try
            {
                if (obj.FromDate == null)
                    obj.FromDate = DateTime.Now.Date.AddDays(-30);
                if (obj.ToDate == null)
                    obj.ToDate = DateTime.Now.Date;

                var kots = oConnectionContext.DbClsKotMaster
                    .Where(k => k.CompanyId == obj.CompanyId &&
                               (obj.BranchId == 0 || k.BranchId == obj.BranchId) &&
                               k.IsDeleted == false &&
                               k.WaiterId > 0 &&
                               k.OrderTime >= obj.FromDate.Value &&
                               k.OrderTime <= obj.ToDate.Value)
                    .ToList();

                var staffPerformance = kots
                    .GroupBy(k => k.WaiterId)
                    .Select(g => new
                    {
                        WaiterId = g.Key,
                        WaiterName = oConnectionContext.DbClsUser
                            .Where(u => u.UserId == g.Key)
                            .Select(u => u.Name)
                            .FirstOrDefault() ?? "Unknown",
                        TotalKots = g.Count(),
                        CompletedKots = g.Count(k => k.OrderStatus.ToLower() == "served" || k.OrderStatus.ToLower() == "completed"),
                        PendingKots = g.Count(k => k.OrderStatus.ToLower() == "pending" || k.OrderStatus.ToLower() == "preparing"),
                        CancelledKots = g.Count(k => k.OrderStatus.ToLower() == "cancelled"),
                        TotalRevenue = oConnectionContext.DbClsSales
                            .Where(s => g.Where(k => k.SalesId > 0).Select(k => k.SalesId).Contains(s.SalesId) && 
                                       s.IsDeleted == false && s.IsCancelled == false)
                            .Sum(s => (decimal?)s.GrandTotal) ?? 0,
                        AverageOrderValue = oConnectionContext.DbClsSales
                            .Where(s => g.Where(k => k.SalesId > 0).Select(k => k.SalesId).Contains(s.SalesId) && 
                                       s.IsDeleted == false && s.IsCancelled == false)
                            .Average(s => (decimal?)s.GrandTotal) ?? 0,
                        AverageServiceTime = g.Where(k => k.ServedTime.HasValue && k.OrderTime != null)
                            .Average(k => (double?)(k.ServedTime.Value - k.OrderTime).TotalMinutes) ?? 0
                    })
                    .Where(s => s.TotalKots > 0)
                    .OrderByDescending(s => s.TotalRevenue)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        StaffPerformance = staffPerformance,
                        TotalCount = staffPerformance.Count
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
                        StaffPerformance = new List<dynamic>(),
                        TotalCount = 0
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }
    }
}


