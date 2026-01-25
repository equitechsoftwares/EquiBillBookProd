using EquiBillBook.Filters;
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
    public class StationTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        [HttpPost]
        public async Task<IHttpActionResult> GetStationTypes(ClsStationTypeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsStationType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                StationTypeId = a.StationTypeId,
                StationTypeName = a.StationTypeName,
                Description = a.Description,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => a.StationTypeName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StationTypes = det.OrderByDescending(a => a.StationTypeId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StationType(ClsStationTypeVm obj)
        {
            var det = oConnectionContext.DbClsStationType.Where(a => a.StationTypeId == obj.StationTypeId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                StationTypeId = a.StationTypeId,
                StationTypeName = a.StationTypeName,
                Description = a.Description,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StationType = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertStationType(ClsStationTypeVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (string.IsNullOrEmpty(obj.StationTypeName))
                {
                    errors.Add(new ClsError { Message = "Station Type Name is required", Id = "divStationTypeName" });
                    isError = true;
                }

                if (!string.IsNullOrEmpty(obj.StationTypeName))
                {
                    if (oConnectionContext.DbClsStationType.Where(a => a.StationTypeName.ToLower() == obj.StationTypeName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.StationTypeId != obj.StationTypeId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Station Type Name exists", Id = "divStationTypeName" });
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

                ClsStationType oStationType = new ClsStationType()
                {
                    StationTypeId = obj.StationTypeId,
                    StationTypeName = obj.StationTypeName,
                    Description = obj.Description,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy
                };

                if (obj.StationTypeId == 0)
                {
                    oConnectionContext.DbClsStationType.Add(oStationType);
                }
                else
                {
                    oConnectionContext.DbClsStationType.Attach(oStationType);
                    oConnectionContext.Entry(oStationType).Property(x => x.StationTypeId).IsModified = true;
                    oConnectionContext.Entry(oStationType).Property(x => x.StationTypeName).IsModified = true;
                    oConnectionContext.Entry(oStationType).Property(x => x.Description).IsModified = true;
                    oConnectionContext.Entry(oStationType).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(oStationType).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oStationType).Property(x => x.ModifiedOn).IsModified = true;
                    oStationType.ModifiedOn = CurrentDate;
                }

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "StationType",
                    CompanyId = obj.CompanyId,
                    Description = "Station Type \"" + obj.StationTypeName + "\" " + (obj.StationTypeId == 0 ? "created" : "updated"),
                    Id = oStationType.StationTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.StationTypeId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Station Type " + (obj.StationTypeId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        StationType = oStationType
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StationTypeActiveInactive(ClsStationTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                ClsStationType oStationType = new ClsStationType()
                {
                    StationTypeId = obj.StationTypeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsStationType.Attach(oStationType);
                oConnectionContext.Entry(oStationType).Property(x => x.StationTypeId).IsModified = true;
                oConnectionContext.Entry(oStationType).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oStationType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oStationType).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Station Type " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StationTypeDelete(ClsStationTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                // Check if station type is in use
                int stationCount = oConnectionContext.DbClsKitchenStation.Where(a => a.StationType == obj.StationTypeName && a.IsDeleted == false).Count();

                if (stationCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsStationType oStationType = new ClsStationType()
                {
                    StationTypeId = obj.StationTypeId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsStationType.Attach(oStationType);
                oConnectionContext.Entry(oStationType).Property(x => x.StationTypeId).IsModified = true;
                oConnectionContext.Entry(oStationType).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oStationType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oStationType).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Station Type deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveStationTypes(ClsStationTypeVm obj)
        {
            var stationTypes = oConnectionContext.DbClsStationType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false)
                .Select(a => new
                {
                    StationTypeId = a.StationTypeId,
                    StationTypeName = a.StationTypeName
                }).OrderBy(a => a.StationTypeName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StationTypes = stationTypes
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}

