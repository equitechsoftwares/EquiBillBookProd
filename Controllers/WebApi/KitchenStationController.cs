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
    public class KitchenStationController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        [HttpPost]
        public async Task<IHttpActionResult> GetStations(ClsKitchenStationVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsKitchenStation.Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false).Select(a => new
            {
                KitchenStationId = a.KitchenStationId,
                StationName = a.StationName,
                StationType = a.StationType,
                PrinterId = a.PrinterId,
                PrinterName = a.PrinterName,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => a.StationName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Stations = det.OrderByDescending(a => a.KitchenStationId).ThenBy(a => a.StationName).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> KitchenStation(ClsKitchenStationVm obj)
        {
            var det = oConnectionContext.DbClsKitchenStation.Where(a => a.KitchenStationId == obj.KitchenStationId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                KitchenStationId = a.KitchenStationId,
                StationName = a.StationName,
                StationType = a.StationType,
                PrinterId = a.PrinterId,
                PrinterName = a.PrinterName,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn
            }).FirstOrDefault();

            // Get category mappings
            var categoryMappings = oConnectionContext.DbClsKitchenStationCategoryMap
                .Where(a => a.KitchenStationId == obj.KitchenStationId && a.IsActive)
                .Select(a => a.CategoryId)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Station = det,
                    CategoryIds = categoryMappings
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertKitchenStation(ClsKitchenStationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (string.IsNullOrEmpty(obj.StationName))
                {
                    errors.Add(new ClsError { Message = "Station Name is required", Id = "divStationName" });
                    isError = true;
                }

                if (!string.IsNullOrEmpty(obj.StationName))
                {
                    if (oConnectionContext.DbClsKitchenStation.Where(a => a.StationName.ToLower() == obj.StationName.ToLower() && a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false && a.KitchenStationId != obj.KitchenStationId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Station Name exists", Id = "divStationName" });
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

                ClsKitchenStation oStation = new ClsKitchenStation()
                {
                    KitchenStationId = obj.KitchenStationId,
                    StationName = obj.StationName,
                    StationType = obj.StationType,
                    PrinterId = obj.PrinterId,
                    PrinterName = obj.PrinterName,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    BranchId = obj.BranchId,
                    CompanyId = obj.CompanyId,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy
                };

                if (obj.KitchenStationId == 0)
                {
                    oConnectionContext.DbClsKitchenStation.Add(oStation);
                }
                else
                {
                    oConnectionContext.DbClsKitchenStation.Attach(oStation);
                    oConnectionContext.Entry(oStation).Property(x => x.KitchenStationId).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.StationName).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.StationType).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.PrinterId).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.PrinterName).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oStation).Property(x => x.ModifiedOn).IsModified = true;
                    oStation.ModifiedOn = CurrentDate;
                }

                oConnectionContext.SaveChanges();

                // Update category mappings
                if (obj.CategoryIdList != null)
                {
                    // Delete existing mappings
                    var existingMappings = oConnectionContext.DbClsKitchenStationCategoryMap
                        .Where(a => a.KitchenStationId == oStation.KitchenStationId)
                        .ToList();
                    foreach (var mapping in existingMappings)
                    {
                        mapping.IsActive = false;
                    }
                    oConnectionContext.SaveChanges();

                    // Add new mappings
                    foreach (var categoryId in obj.CategoryIdList)
                    {
                        var existing = oConnectionContext.DbClsKitchenStationCategoryMap
                            .Where(a => a.KitchenStationId == oStation.KitchenStationId && a.CategoryId == categoryId)
                            .FirstOrDefault();

                        if (existing != null)
                        {
                            existing.IsActive = true;
                        }
                        else
                        {
                            ClsKitchenStationCategoryMap oMapping = new ClsKitchenStationCategoryMap()
                            {
                                KitchenStationId = oStation.KitchenStationId,
                                CategoryId = categoryId,
                                CompanyId = obj.CompanyId,
                                IsActive = true
                            };
                            oConnectionContext.DbClsKitchenStationCategoryMap.Add(oMapping);
                        }
                    }
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "KitchenStation",
                    CompanyId = obj.CompanyId,
                    Description = "Kitchen Station \"" + obj.StationName + "\" " + (obj.KitchenStationId == 0 ? "created" : "updated"),
                    Id = oStation.KitchenStationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.KitchenStationId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Kitchen Station " + (obj.KitchenStationId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        Station = oStation
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> MapCategories(ClsKitchenStationVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Delete existing mappings
                var existingMappings = oConnectionContext.DbClsKitchenStationCategoryMap
                    .Where(a => a.KitchenStationId == obj.KitchenStationId)
                    .ToList();
                foreach (var mapping in existingMappings)
                {
                    mapping.IsActive = false;
                }
                oConnectionContext.SaveChanges();

                // Add new mappings
                if (obj.CategoryIdList != null && obj.CategoryIdList.Count > 0)
                {
                    foreach (var categoryId in obj.CategoryIdList)
                    {
                        var existing = oConnectionContext.DbClsKitchenStationCategoryMap
                            .Where(a => a.KitchenStationId == obj.KitchenStationId && a.CategoryId == categoryId)
                            .FirstOrDefault();

                        if (existing != null)
                        {
                            existing.IsActive = true;
                        }
                        else
                        {
                            ClsKitchenStationCategoryMap oMapping = new ClsKitchenStationCategoryMap()
                            {
                                KitchenStationId = obj.KitchenStationId,
                                CategoryId = categoryId,
                                CompanyId = obj.CompanyId,
                                IsActive = true
                            };
                            oConnectionContext.DbClsKitchenStationCategoryMap.Add(oMapping);
                        }
                    }
                    oConnectionContext.SaveChanges();
                }

                data = new
                {
                    Status = 1,
                    Message = "Categories mapped successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> KitchenStationActiveInactive(ClsKitchenStationVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                ClsKitchenStation oStation = new ClsKitchenStation()
                {
                    KitchenStationId = obj.KitchenStationId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsKitchenStation.Attach(oStation);
                oConnectionContext.Entry(oStation).Property(x => x.KitchenStationId).IsModified = true;
                oConnectionContext.Entry(oStation).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oStation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oStation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Kitchen Station " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> KitchenStationDelete(ClsKitchenStationVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                // Check if station is in use
                int kotDetailsCount = oConnectionContext.DbClsKotDetails.Where(a => a.KitchenStationId == obj.KitchenStationId && a.IsDeleted == false).Count();

                if (kotDetailsCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsKitchenStation oStation = new ClsKitchenStation()
                {
                    KitchenStationId = obj.KitchenStationId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsKitchenStation.Attach(oStation);
                oConnectionContext.Entry(oStation).Property(x => x.KitchenStationId).IsModified = true;
                oConnectionContext.Entry(oStation).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oStation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oStation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Kitchen Station deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveStations(ClsKitchenStationVm obj)
        {
            var stations = oConnectionContext.DbClsKitchenStation.Where(a => a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsActive == true && a.IsDeleted == false)
                .Select(a => new
                {
                    KitchenStationId = a.KitchenStationId,
                    StationName = a.StationName
                }).OrderBy(a => a.StationName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Stations = stations
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}


