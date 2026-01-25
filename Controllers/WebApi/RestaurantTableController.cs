using EquiBillBook.Filters;
using EquiBillBook.Helpers;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class RestaurantTableController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        TableStatusHelper tableStatusHelper = new TableStatusHelper();
        [HttpPost]
        public async Task<IHttpActionResult> GetTables(ClsRestaurantTableVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            if (obj.PageIndex == 0)
            {
                obj.PageIndex = 1;
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var detQuery = oConnectionContext.DbClsRestaurantTable.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false);

            if (obj.BranchId > 0)
            {
                detQuery = detQuery.Where(a => a.BranchId == obj.BranchId);
            }

            if (obj.FloorId > 0)
            {
                detQuery = detQuery.Where(a => a.FloorId == obj.FloorId);
            }

            var det = detQuery.Select(a => new
            {
                TableId = a.TableId,
                TableNo = a.TableNo,
                TableName = a.TableName,
                Capacity = a.Capacity,
                FloorId = a.FloorId,
                TableTypeId = a.TableTypeId,
                IsMaintenanceMode = a.IsMaintenanceMode,
                MaintenanceFrom = a.MaintenanceFrom,
                MaintenanceTo = a.MaintenanceTo,
                MaintenanceReason = a.MaintenanceReason,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                PositionX = a.PositionX,
                PositionY = a.PositionY,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                QRCodeImage = a.QRCodeImage,
                TableSlug = a.TableSlug,
                Status = a.Status,
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                FloorName = oConnectionContext.DbClsRestaurantFloor.Where(f => f.FloorId == a.FloorId).Select(f => f.FloorName).FirstOrDefault(),
                TableTypeName = oConnectionContext.DbClsTableType.Where(t => t.TableTypeId == a.TableTypeId).Select(t => t.TableTypeName).FirstOrDefault(),
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => (a.TableNo != null && a.TableNo.ToLower().Contains(obj.Search.ToLower())) ||
                                     (a.TableName != null && a.TableName.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            // Calculate status dynamically using TableStatusHelper (includes bookings, KOTs, maintenance)
            TableStatusHelper tableStatusHelper = new TableStatusHelper(oConnectionContext);
            
            // Debug: Log ForDateTime if provided
            if (obj.ForDateTime.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"[GetTables] ForDateTime received: {obj.ForDateTime.Value} (Kind: {obj.ForDateTime.Value.Kind})");
            }
            
            var tablesWithStatus = det.Select(t => new
            {
                t.TableId,
                t.TableNo,
                t.TableName,
                t.Capacity,
                t.FloorId,
                t.TableTypeId,
                t.IsMaintenanceMode,
                t.MaintenanceFrom,
                t.MaintenanceTo,
                t.MaintenanceReason,
                t.BranchId,
                t.CompanyId,
                t.IsActive,
                t.IsDeleted,
                t.PositionX,
                t.PositionY,
                t.AddedBy,
                t.AddedOn,
                t.ModifiedBy,
                t.ModifiedOn,
                t.QRCodeImage,
                t.TableSlug,
                t.BranchName,
                t.FloorName,
                t.TableTypeName,
                t.AddedByCode,
                t.ModifiedByCode,
                Status = tableStatusHelper.GetTableStatus(t.TableId, obj.ForDateTime).ToString()
            }).ToList();

            // Filter by Status if provided (case-insensitive comparison)
            if (!string.IsNullOrEmpty(obj.Status))
            {
                tablesWithStatus = tablesWithStatus.Where(a => 
                    !string.IsNullOrEmpty(a.Status) && 
                    a.Status.Equals(obj.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Tables = tablesWithStatus.OrderByDescending(a => a.TableId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = tablesWithStatus.Count(),
                    ActiveCount = tablesWithStatus.Where(a => a.IsActive == true).Count(),
                    InactiveCount = tablesWithStatus.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> RestaurantTable(ClsRestaurantTableVm obj)
        {
            var det = oConnectionContext.DbClsRestaurantTable.Where(a => a.TableId == obj.TableId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                TableId = a.TableId,
                TableNo = a.TableNo,
                TableName = a.TableName,
                Capacity = a.Capacity,
                FloorId = a.FloorId,
                TableTypeId = a.TableTypeId,
                IsMaintenanceMode = a.IsMaintenanceMode,
                MaintenanceFrom = a.MaintenanceFrom,
                MaintenanceTo = a.MaintenanceTo,
                MaintenanceReason = a.MaintenanceReason,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                PositionX = a.PositionX,
                PositionY = a.PositionY,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                TableSlug = a.TableSlug,
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                FloorName = oConnectionContext.DbClsRestaurantFloor.Where(f => f.FloorId == a.FloorId).Select(f => f.FloorName).FirstOrDefault(),
                TableTypeName = oConnectionContext.DbClsTableType.Where(t => t.TableTypeId == a.TableTypeId).Select(t => t.TableTypeName).FirstOrDefault()
            }).FirstOrDefault();

            // Calculate current status
            object detWithStatus = null;
            if (det != null)
            {
                var status = tableStatusHelper.GetTableStatus(det.TableId);
                detWithStatus = new
                {
                    det.TableId,
                    det.TableNo,
                    det.TableName,
                    det.Capacity,
                    det.FloorId,
                    det.TableTypeId,
                    det.IsMaintenanceMode,
                    det.MaintenanceFrom,
                    det.MaintenanceTo,
                    det.MaintenanceReason,
                    det.BranchId,
                    det.CompanyId,
                    det.IsActive,
                    det.IsDeleted,
                    det.PositionX,
                    det.PositionY,
                    det.AddedBy,
                    det.AddedOn,
                    det.ModifiedBy,
                    det.ModifiedOn,
                    det.TableSlug,
                    det.BranchName,
                    det.FloorName,
                    det.TableTypeName,
                    Status = status.ToString()
                };
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Table = detWithStatus ?? det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertRestaurantTable(ClsRestaurantTableVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (string.IsNullOrEmpty(obj.TableNo))
                {
                    errors.Add(new ClsError { Message = "Table Number is required", Id = "divTableNo" });
                    isError = true;
                }

                if (obj.BranchId <= 0)
                {
                    errors.Add(new ClsError { Message = "Branch is required", Id = "divBranchId" });
                    isError = true;
                }

                if (string.IsNullOrWhiteSpace(obj.TableSlug))
                {
                    errors.Add(new ClsError { Message = "Table Slug is required", Id = "divTableSlug" });
                    isError = true;
                }

                if (obj.Capacity <= 0)
                {
                    errors.Add(new ClsError { Message = "Capacity must be greater than 0", Id = "divCapacity" });
                    isError = true;
                }

                if (!string.IsNullOrEmpty(obj.TableNo))
                {
                    if (oConnectionContext.DbClsRestaurantTable.Where(a => a.TableNo.ToLower() == obj.TableNo.ToLower() && a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false && a.TableId != obj.TableId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Table Number exists", Id = "divTableNo" });
                        isError = true;
                    }
                }

                // Validate TableSlug uniqueness (globally unique)
                if (!string.IsNullOrWhiteSpace(obj.TableSlug))
                {
                    string normalizedSlug = obj.TableSlug.Trim().ToLower();
                    var existingSlug = oConnectionContext.DbClsRestaurantTable
                        .Where(a => a.TableSlug != null &&
                                   a.TableSlug.ToLower() == normalizedSlug &&
                                   a.IsDeleted == false &&
                                   a.TableId != obj.TableId)
                        .FirstOrDefault();
                    
                    if (existingSlug != null)
                    {
                        errors.Add(new ClsError { Message = "This table slug is already in use. Please choose a different slug.", Id = "divTableSlug" });
                        isError = true;
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

                // Get existing QR path (for updates) so we can reuse/delete old file similar to Catalogue
                string existingQrPath = null;
                if (obj.TableId != 0)
                {
                    existingQrPath = oConnectionContext.DbClsRestaurantTable
                        .Where(a => a.TableId == obj.TableId && a.CompanyId == obj.CompanyId)
                        .Select(a => a.QRCodeImage)
                        .FirstOrDefault();
                }

                ClsRestaurantTable oTable = new ClsRestaurantTable()
                {
                    TableId = obj.TableId,
                    TableNo = obj.TableNo,
                    TableName = obj.TableName,
                    Capacity = obj.Capacity,
                    FloorId = obj.FloorId,
                    TableTypeId = obj.TableTypeId,
                    IsMaintenanceMode = false,
                    MaintenanceFrom = null,
                    MaintenanceTo = null,
                    MaintenanceReason = null,
                    BranchId = obj.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    PositionX = obj.PositionX,
                    PositionY = obj.PositionY,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = obj.TableId == 0 ? (DateTime?)null : CurrentDate,
                    QRCodeImage = existingQrPath,
                    TableSlug = obj.TableSlug
                };

                if (obj.TableId == 0)
                {
                    oConnectionContext.DbClsRestaurantTable.Add(oTable);
                }
                else
                {
                    oConnectionContext.DbClsRestaurantTable.Attach(oTable);
                    oConnectionContext.Entry(oTable).Property(x => x.TableId).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.TableNo).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.TableName).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.Capacity).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.FloorId).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.TableTypeId).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.BranchId).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.PositionX).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.PositionY).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.TableSlug).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oTable).Property(x => x.ModifiedOn).IsModified = true;
                }

                oConnectionContext.SaveChanges();

                // Generate and save QR code for the table (similar to catalogue)
                try
                {
                    // Get domain from request (similar to catalogue - can be passed from view model if needed)
                    string domain = null;
                    var request = HttpContext.Current?.Request;
                    if (request != null)
                    {
                        domain = request.Url?.GetLeftPart(UriPartial.Authority);
                    }
                    var qrPath = GenerateTableQrCode(oTable.TableId, oTable.TableNo, domain, oTable.QRCodeImage, oTable.TableSlug);
                    if (!string.IsNullOrWhiteSpace(qrPath))
                    {
                        oTable.QRCodeImage = qrPath;
                        // For updates, mark property as modified explicitly
                        if (obj.TableId != 0)
                        {
                            oConnectionContext.Entry(oTable).Property(x => x.QRCodeImage).IsModified = true;
                        }
                        oConnectionContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the save operation
                    System.Diagnostics.Debug.WriteLine("Error generating QR code for table: " + ex.Message);
                }

                // Emit table status update
               // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = oTable.TableId, Status = tableStatusHelper.GetTableStatus(oTable.TableId).ToString() }, obj.CompanyId, obj.BranchId);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "RestaurantTable",
                    CompanyId = obj.CompanyId,
                    Description = "Table \"" + obj.TableNo + "\" " + (obj.TableId == 0 ? "created" : "updated"),
                    Id = oTable.TableId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.TableId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Table " + (obj.TableId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        Table = oTable
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetTableStatus(long tableId, DateTime? forDateTime)
        {
            var status = tableStatusHelper.GetTableStatus(tableId, forDateTime);
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TableId = tableId,
                    Status = status.ToString(),
                    ForDateTime = forDateTime ?? DateTime.Now
                }
            };
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetTableAvailability(ClsRestaurantTableVm obj)
        {
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

            var isAvailable = tableStatusHelper.IsTableAvailable(obj.TableId, obj.StartDateTime.Value, obj.EndDateTime.Value);
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TableId = obj.TableId,
                    IsAvailable = isAvailable,
                    StartDateTime = obj.StartDateTime,
                    EndDateTime = obj.EndDateTime
                }
            };
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetAvailableTables(ClsRestaurantTableVm obj)
        {
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

            var allTables = oConnectionContext.DbClsRestaurantTable
                .Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsActive && !a.IsDeleted)
                .ToList();

            var availableTables = allTables
                .Where(t => tableStatusHelper.IsTableAvailable(t.TableId, obj.StartDateTime.Value, obj.EndDateTime.Value))
                .Select(t => new
                {
                    TableId = t.TableId,
                    TableNo = t.TableNo,
                    TableName = t.TableName,
                    Capacity = t.Capacity,
                    FloorId = t.FloorId,
                    TableTypeId = t.TableTypeId,
                    FloorName = oConnectionContext.DbClsRestaurantFloor.Where(f => f.FloorId == t.FloorId).Select(f => f.FloorName).FirstOrDefault(),
                    TableTypeName = oConnectionContext.DbClsTableType.Where(tt => tt.TableTypeId == t.TableTypeId).Select(tt => tt.TableTypeName).FirstOrDefault()
                })
                .OrderBy(t => t.TableNo)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Tables = availableTables
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SetMaintenanceMode(ClsRestaurantTableVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsRestaurantTable oTable = new ClsRestaurantTable()
                {
                    TableId = obj.TableId,
                    IsMaintenanceMode = obj.IsMaintenanceMode,
                    MaintenanceFrom = obj.MaintenanceFrom,
                    MaintenanceTo = obj.MaintenanceTo,
                    MaintenanceReason = obj.MaintenanceReason,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsRestaurantTable.Attach(oTable);
                oConnectionContext.Entry(oTable).Property(x => x.TableId).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.IsMaintenanceMode).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.MaintenanceFrom).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.MaintenanceTo).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.MaintenanceReason).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // Emit table status update
               // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = obj.TableId, Status = tableStatusHelper.GetTableStatus(obj.TableId).ToString() }, obj.CompanyId, obj.BranchId);

                data = new
                {
                    Status = 1,
                    Message = "Maintenance mode " + (obj.IsMaintenanceMode ? "enabled" : "disabled") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RestaurantTableActiveInactive(ClsRestaurantTableVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                ClsRestaurantTable oTable = new ClsRestaurantTable()
                {
                    TableId = obj.TableId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsRestaurantTable.Attach(oTable);
                oConnectionContext.Entry(oTable).Property(x => x.TableId).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // Emit table status update
               // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = obj.TableId, Status = tableStatusHelper.GetTableStatus(obj.TableId).ToString() }, obj.CompanyId, obj.BranchId);

                data = new
                {
                    Status = 1,
                    Message = "Table " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> SetTableStatus(ClsRestaurantTableVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Validate status
                var validStatuses = new[] { "Available", "Occupied", "Reserved", "Maintenance" };
                if (!string.IsNullOrEmpty(obj.Status) && !validStatuses.Contains(obj.Status, StringComparer.OrdinalIgnoreCase))
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Invalid status. Valid statuses are: Available, Occupied, Reserved, Maintenance",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsRestaurantTable oTable = new ClsRestaurantTable()
                {
                    TableId = obj.TableId,
                    Status = obj.Status,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsRestaurantTable.Attach(oTable);
                oConnectionContext.Entry(oTable).Property(x => x.TableId).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                // Emit table status update
               // await SocketIoHelper.EmitTableStatusUpdated(new { TableId = obj.TableId, Status = tableStatusHelper.GetTableStatus(obj.TableId).ToString() }, obj.CompanyId, obj.BranchId);

                data = new
                {
                    Status = 1,
                    Message = "Table status updated successfully",
                    Data = new
                    {
                        TableId = obj.TableId,
                        Status = tableStatusHelper.GetTableStatus(obj.TableId).ToString()
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RestaurantTableDelete(ClsRestaurantTableVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                // Check if table is in use
                int bookingCount = (from b in oConnectionContext.DbClsTableBooking
                                   join bt in oConnectionContext.DbClsTableBookingTable on b.BookingId equals bt.BookingId
                                   where bt.TableId == obj.TableId && b.IsDeleted == false
                                   select b).Count();
                int kotCount = oConnectionContext.DbClsKotMaster.Where(a => a.TableId == obj.TableId && a.IsDeleted == false).Count();

                if (bookingCount > 0 || kotCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsRestaurantTable oTable = new ClsRestaurantTable()
                {
                    TableId = obj.TableId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsRestaurantTable.Attach(oTable);
                oConnectionContext.Entry(oTable).Property(x => x.TableId).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTable).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Table deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GenerateQrCode(ClsRestaurantTableVm obj)
        {
            if (obj.TableId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Table ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            var table = oConnectionContext.DbClsRestaurantTable
                .Where(a => a.TableId == obj.TableId && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                .FirstOrDefault();

            if (table == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Table not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            try
            {
                // Get domain from request (similar to catalogue)
                string domain = null;
                var request = HttpContext.Current?.Request;
                if (request != null)
                {
                    domain = request.Url?.GetLeftPart(UriPartial.Authority);
                }

                // Build QR code URL using BuildTableUrl method (handles domain resolution like catalogue)
                // This will use slug if available, otherwise fall back to tableId
                string qrCodeUrl = BuildTableUrl(table.TableId, domain, table.TableSlug);

                // Regenerate QR code to ensure it always uses the current URL with slug (if available)
                // This ensures the QR code image matches the displayed URL, especially if slug was added after initial QR generation
                string qrPath = GenerateTableQrCode(table.TableId, table.TableNo, domain, table.QRCodeImage, table.TableSlug);
                
                string savedQrPath = !string.IsNullOrWhiteSpace(qrPath) ? qrPath : GetTableQrCodeFilePath(table.TableId, table.TableNo);

                if (string.IsNullOrWhiteSpace(savedQrPath) || !File.Exists(HostingEnvironment.MapPath(savedQrPath)))
                {
                    data = new
                    {
                        Status = 0,
                        Message = "QR code image not available for this table. Please edit & save the table to regenerate QR code.",
                        Data = new { QrCodeUrl = qrCodeUrl, TableId = table.TableId, TableNo = table.TableNo }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Return the image URL directly instead of converting to base64
                // The browser can load the image directly from the server
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
                        TableId = table.TableId,
                        TableNo = table.TableNo
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
        /// Generates and saves QR code for a restaurant table (similar to GenerateCatalogueQrCode)
        /// </summary>
        private string GenerateTableQrCode(long tableId, string tableNo, string domain, string existingRelativePath = null, string tableSlug = null)
        {
            if (tableId <= 0 || string.IsNullOrWhiteSpace(tableNo))
            {
                return existingRelativePath;
            }

            var tableUrl = BuildTableUrl(tableId, domain, tableSlug);
            if (string.IsNullOrWhiteSpace(tableUrl))
            {
                return existingRelativePath;
            }

            var directoryPath = HostingEnvironment.MapPath("~/ExternalContents/Images/RestaurantTableQRCode/");
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return existingRelativePath;
            }

            Directory.CreateDirectory(directoryPath);

            var sanitizedTableNo = SanitizeFileNameSegment(tableNo);
            if (string.IsNullOrWhiteSpace(sanitizedTableNo))
            {
                sanitizedTableNo = "table";
            }

            var fileName = string.Format("table-{0}-{1}.png", sanitizedTableNo, tableId);
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
            using (var qrCodeData = qrGenerator.CreateQrCode(tableUrl, QRCodeGenerator.ECCLevel.Q))
            using (var pngQrCode = new PngByteQRCode(qrCodeData))
            {
                var qrBytes = pngQrCode.GetGraphic(20);
                File.WriteAllBytes(filePath, qrBytes);
            }

            return "/ExternalContents/Images/RestaurantTableQRCode/" + fileName;
        }

        /// <summary>
        /// Gets the expected file path for a table's QR code
        /// </summary>
        private string GetTableQrCodeFilePath(long tableId, string tableNo)
        {
            if (tableId <= 0 || string.IsNullOrWhiteSpace(tableNo))
            {
                return null;
            }

            var sanitizedTableNo = SanitizeFileNameSegment(tableNo);
            if (string.IsNullOrWhiteSpace(sanitizedTableNo))
            {
                sanitizedTableNo = "table";
            }

            var fileName = string.Format("table-{0}-{1}.png", sanitizedTableNo, tableId);
            return "/ExternalContents/Images/RestaurantTableQRCode/" + fileName;
        }

        /// <summary>
        /// Builds the URL for the table booking page (similar to BuildCatalogueUrl in CatalogueController)
        /// Uses slug if available, otherwise falls back to tableId
        /// </summary>
        private string BuildTableUrl(long tableId, string domain, string tableSlug = null)
        {
            if (tableId <= 0)
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

            // Use slug if available, otherwise fall back to tableId
            string tablePath = !string.IsNullOrWhiteSpace(tableSlug) 
                ? "/booktable/" + tableSlug 
                : "/publicbooking/booktable?tableId=" + tableId;

            if (string.IsNullOrWhiteSpace(resolvedDomain))
            {
                return tablePath;
            }

            return resolvedDomain + tablePath;
        }

        /// <summary>
        /// Normalizes a relative path (similar to CatalogueController)
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
        /// Sanitizes a string to be used as a file name segment (similar to CatalogueController)
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
    }
}


