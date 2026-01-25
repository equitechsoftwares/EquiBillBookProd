using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class VehicleController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllVehicles(ClsVehicleVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);


            var det = oConnectionContext.DbClsVehicle.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                VehicleId = a.VehicleId,
                VehicleName = a.VehicleName,
                Description = a.Description,
                MileageRate = a.MileageRate,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.VehicleName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Vehicles = det.OrderByDescending(a => a.VehicleId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Vehicle(ClsVehicle obj)
        {
            var det = oConnectionContext.DbClsVehicle.Where(a => a.VehicleId == obj.VehicleId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                VehicleId = a.VehicleId,
                VehicleName = a.VehicleName,
                Description = a.Description,
                MileageRate = a.MileageRate,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Vehicle = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertVehicle(ClsVehicleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.VehicleName == null || obj.VehicleName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divVehicleName" });
                    isError = true;
                }

                if (oConnectionContext.DbClsVehicle.Where(a => a.VehicleName.ToLower() == obj.VehicleName.ToLower()
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Vehicle Name exists", Id = "divVehicleName" });
                    isError = true;
                }

                if (obj.MileageRate == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMileageRate" });
                    isError = true;
                }

                if (isError == true)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "",
                        Errors = errors,
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsVehicle oVehicle = new ClsVehicle()
                {
                    VehicleName = obj.VehicleName,
                    Description = obj.Description,
                    MileageRate = obj.MileageRate,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsVehicle.Add(oVehicle);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Vehicle",
                    CompanyId = obj.CompanyId,
                    Description = "Vehicle \"" + obj.VehicleName + "\" created",
                    Id = oVehicle.VehicleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Vehicle created successfully",
                    Data = new
                    {
                        Vehicle = oVehicle
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateVehicle(ClsVehicleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.VehicleName == null || obj.VehicleName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divVehicleName" });
                    isError = true;
                }

                if (oConnectionContext.DbClsVehicle.Where(a => a.VehicleName.ToLower() == obj.VehicleName.ToLower() && a.VehicleId != obj.VehicleId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Vehicle Name exists", Id = "divVehicleName" });
                    isError = true;
                }

                if (obj.MileageRate == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMileageRate" });
                    isError = true;
                }

                if (isError == true)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "",
                        Errors = errors,
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsVehicle oVehicle = new ClsVehicle()
                {
                    VehicleId = obj.VehicleId,
                    VehicleName = obj.VehicleName,
                    Description = obj.Description,
                    MileageRate = obj.MileageRate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVehicle.Attach(oVehicle);
                oConnectionContext.Entry(oVehicle).Property(x => x.VehicleId).IsModified = true;
                oConnectionContext.Entry(oVehicle).Property(x => x.VehicleName).IsModified = true;
                oConnectionContext.Entry(oVehicle).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oVehicle).Property(x => x.MileageRate).IsModified = true;
                oConnectionContext.Entry(oVehicle).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oVehicle).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Vehicle",
                    CompanyId = obj.CompanyId,
                    Description = "Vehicle \"" + obj.VehicleName + "\" updated",
                    Id = oVehicle.VehicleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Vehicle updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> VehicleActiveInactive(ClsVehicleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsVehicle oClsRole = new ClsVehicle()
                {
                    VehicleId = obj.VehicleId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVehicle.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.VehicleId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Vehicle",
                    CompanyId = obj.CompanyId,
                    Description = "Vehicle \"" + oConnectionContext.DbClsVehicle.Where(a => a.VehicleId == obj.VehicleId).Select(a => a.VehicleName).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.VehicleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Vehicle " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> VehicleDelete(ClsVehicleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ExpenseCount = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                && a.VehicleId == obj.VehicleId).Count();

                if (ExpenseCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsVehicle oClsRole = new ClsVehicle()
                {
                    VehicleId = obj.VehicleId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVehicle.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.VehicleId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Vehicle",
                    CompanyId = obj.CompanyId,
                    Description = "Vehicle \"" + oConnectionContext.DbClsVehicle.Where(a => a.VehicleId == obj.VehicleId).Select(a => a.VehicleName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.VehicleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Vehicle deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveVehicles(ClsVehicleVm obj)
        {
           var det = oConnectionContext.DbClsVehicle.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true).Select(a => new ClsVehicleVm
                {
                    VehicleId = a.VehicleId,
                    VehicleName = a.VehicleName,
                }).OrderBy(a=>a.VehicleName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Vehicles = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> MileageUpdate(ClsVehicleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsVehicle oClsRole = new ClsVehicle()
                {
                    VehicleId = obj.VehicleId,
                    MileageRate = obj.MileageRate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVehicle.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.VehicleId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.MileageRate).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Vehicle",
                    CompanyId = obj.CompanyId,
                    Description = "Vehicle \"" + oConnectionContext.DbClsVehicle.Where(a => a.VehicleId == obj.VehicleId).Select(a => a.VehicleName).FirstOrDefault() + " mileage rate updated",
                    Id = oClsRole.VehicleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Vehicle " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
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
