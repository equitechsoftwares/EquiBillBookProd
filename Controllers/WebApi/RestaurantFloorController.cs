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
    public class RestaurantFloorController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        [HttpPost]
        public async Task<IHttpActionResult> GetFloors(ClsRestaurantFloorVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsRestaurantFloor.Where(a => a.CompanyId == obj.CompanyId && (obj.BranchId == 0 || a.BranchId == obj.BranchId) && a.IsDeleted == false).Select(a => new
            {
                FloorId = a.FloorId,
                FloorName = a.FloorName,
                FloorNumber = a.FloorNumber,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => a.FloorName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Floors = det.OrderByDescending(a => a.FloorId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RestaurantFloor(ClsRestaurantFloorVm obj)
        {
            var det = oConnectionContext.DbClsRestaurantFloor.Where(a => a.FloorId == obj.FloorId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                FloorId = a.FloorId,
                FloorName = a.FloorName,
                FloorNumber = a.FloorNumber,
                BranchId = a.BranchId,
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
                    Floor = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertRestaurantFloor(ClsRestaurantFloorVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (string.IsNullOrEmpty(obj.FloorName))
                {
                    errors.Add(new ClsError { Message = "Floor Name is required", Id = "divFloorName" });
                    isError = true;
                }

                if (!string.IsNullOrEmpty(obj.FloorName))
                {
                    if (oConnectionContext.DbClsRestaurantFloor.Where(a => a.FloorName.ToLower() == obj.FloorName.ToLower() && a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Floor Name exists", Id = "divFloorName" });
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

                ClsRestaurantFloor oFloor = new ClsRestaurantFloor()
                {
                    FloorId = obj.FloorId,
                    FloorName = obj.FloorName,
                    FloorNumber = obj.FloorNumber,
                    BranchId = obj.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy
                };

                if (obj.FloorId == 0)
                {
                    oConnectionContext.DbClsRestaurantFloor.Add(oFloor);
                }
                else
                {
                    oConnectionContext.DbClsRestaurantFloor.Attach(oFloor);
                    oConnectionContext.Entry(oFloor).Property(x => x.FloorId).IsModified = true;
                    oConnectionContext.Entry(oFloor).Property(x => x.FloorName).IsModified = true;
                    oConnectionContext.Entry(oFloor).Property(x => x.FloorNumber).IsModified = true;
                    oConnectionContext.Entry(oFloor).Property(x => x.BranchId).IsModified = true;
                    oConnectionContext.Entry(oFloor).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(oFloor).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oFloor).Property(x => x.ModifiedOn).IsModified = true;
                    oFloor.ModifiedOn = CurrentDate;
                }

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "RestaurantFloor",
                    CompanyId = obj.CompanyId,
                    Description = "Floor \"" + obj.FloorName + "\" " + (obj.FloorId == 0 ? "created" : "updated"),
                    Id = oFloor.FloorId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.FloorId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Floor " + (obj.FloorId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        Floor = oFloor
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RestaurantFloorActiveInactive(ClsRestaurantFloorVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                ClsRestaurantFloor oFloor = new ClsRestaurantFloor()
                {
                    FloorId = obj.FloorId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsRestaurantFloor.Attach(oFloor);
                oConnectionContext.Entry(oFloor).Property(x => x.FloorId).IsModified = true;
                oConnectionContext.Entry(oFloor).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oFloor).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oFloor).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Floor " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RestaurantFloorDelete(ClsRestaurantFloorVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                // Check if floor is in use
                int tableCount = oConnectionContext.DbClsRestaurantTable.Where(a => a.FloorId == obj.FloorId && a.IsDeleted == false).Count();

                if (tableCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsRestaurantFloor oFloor = new ClsRestaurantFloor()
                {
                    FloorId = obj.FloorId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsRestaurantFloor.Attach(oFloor);
                oConnectionContext.Entry(oFloor).Property(x => x.FloorId).IsModified = true;
                oConnectionContext.Entry(oFloor).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oFloor).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oFloor).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Floor deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveFloors(ClsRestaurantFloorVm obj)
        {
            var floors = oConnectionContext.DbClsRestaurantFloor.Where(a => a.CompanyId == obj.CompanyId && (obj.BranchId == 0 || a.BranchId == obj.BranchId) && a.IsActive == true && a.IsDeleted == false)
                .Select(a => new
                {
                    FloorId = a.FloorId,
                    FloorName = a.FloorName,
                    FloorNumber = a.FloorNumber
                }).OrderBy(a => a.FloorNumber).ThenBy(a => a.FloorName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Floors = floors
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}


