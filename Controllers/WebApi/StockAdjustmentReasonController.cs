using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class StockAdjustmentReasonController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllStockAdjustmentReasons(ClsStockAdjustmentReasonVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                StockAdjustmentReasonId = a.StockAdjustmentReasonId,
                StockAdjustmentReason = a.StockAdjustmentReason,
                a.Description,
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
                det = det.Where(a => a.StockAdjustmentReason.ToLower().Contains(obj.Search.ToLower()) ||
                (a.Description != null && a.Description.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockAdjustmentReasons = det.OrderByDescending(a => a.StockAdjustmentReasonId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockAdjustmentReason(ClsStockAdjustmentReason obj)
        {
            var det = oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.StockAdjustmentReasonId == obj.StockAdjustmentReasonId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                StockAdjustmentReasonId = a.StockAdjustmentReasonId,
                StockAdjustmentReason = a.StockAdjustmentReason,
                a.Description,
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
                    StockAdjustmentReason = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertStockAdjustmentReason(ClsStockAdjustmentReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.StockAdjustmentReason == null || obj.StockAdjustmentReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStockAdjustmentReason" });
                    isError = true;
                }

                if (obj.StockAdjustmentReason != null && obj.StockAdjustmentReason != "")
                {
                    if (oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.StockAdjustmentReason.ToLower() == obj.StockAdjustmentReason.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Stock Adjustment Reason exists", Id = "divStockAdjustmentReason" });
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
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsStockAdjustmentReason oStockAdjustmentReason = new ClsStockAdjustmentReason()
                {
                    StockAdjustmentReason = obj.StockAdjustmentReason,
                    Description= obj.Description,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsStockAdjustmentReason.Add(oStockAdjustmentReason);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Adjustment Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment Reason \"" + obj.StockAdjustmentReason + "\" created",
                    Id = oStockAdjustmentReason.StockAdjustmentReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment Reason created successfully",
                    Data = new
                    {
                        StockAdjustmentReason = oStockAdjustmentReason
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateStockAdjustmentReason(ClsStockAdjustmentReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.StockAdjustmentReason == null || obj.StockAdjustmentReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStockAdjustmentReason" });
                    isError = true;
                }

                if (obj.StockAdjustmentReason != null && obj.StockAdjustmentReason != "")
                {
                    if (oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.StockAdjustmentReason.ToLower() == obj.StockAdjustmentReason.ToLower() && a.StockAdjustmentReasonId != obj.StockAdjustmentReasonId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Stock Adjustment Reason exists", Id = "divStockAdjustmentReason" });
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
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsStockAdjustmentReason oStockAdjustmentReason = new ClsStockAdjustmentReason()
                {
                    StockAdjustmentReasonId = obj.StockAdjustmentReasonId,
                    StockAdjustmentReason = obj.StockAdjustmentReason,
                    Description = obj.Description,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsStockAdjustmentReason.Attach(oStockAdjustmentReason);
                oConnectionContext.Entry(oStockAdjustmentReason).Property(x => x.StockAdjustmentReasonId).IsModified = true;
                oConnectionContext.Entry(oStockAdjustmentReason).Property(x => x.StockAdjustmentReason).IsModified = true;
                oConnectionContext.Entry(oStockAdjustmentReason).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oStockAdjustmentReason).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oStockAdjustmentReason).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Adjustment Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment Reason \"" + obj.StockAdjustmentReason + "\" updated",
                    Id = oStockAdjustmentReason.StockAdjustmentReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment Reason updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockAdjustmentReasonActiveInactive(ClsStockAdjustmentReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsStockAdjustmentReason oClsRole = new ClsStockAdjustmentReason()
                {
                    StockAdjustmentReasonId = obj.StockAdjustmentReasonId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsStockAdjustmentReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.StockAdjustmentReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Adjustment Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment Reason \"" + oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.StockAdjustmentReasonId == obj.StockAdjustmentReasonId).Select(a => a.StockAdjustmentReason).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.StockAdjustmentReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment Reason " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockAdjustmentReasonDelete(ClsStockAdjustmentReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.StockAdjustmentReasonId == obj.StockAdjustmentReasonId && a.IsDeleted == false && a.IsCancelled == false).Count();
                //if (ItemCount > 0)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "Cannot delete as it is already in use",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsStockAdjustmentReason oClsRole = new ClsStockAdjustmentReason()
                {
                    StockAdjustmentReasonId = obj.StockAdjustmentReasonId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsStockAdjustmentReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.StockAdjustmentReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Adjustment Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment Reason \"" + oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.StockAdjustmentReasonId == obj.StockAdjustmentReasonId).Select(a => a.StockAdjustmentReason).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.StockAdjustmentReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment Reason deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveStockAdjustmentReasons(ClsStockAdjustmentReasonVm obj)
        {
            var det = oConnectionContext.DbClsStockAdjustmentReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                StockAdjustmentReasonId = a.StockAdjustmentReasonId,
                StockAdjustmentReason = a.StockAdjustmentReason,
                a.Description
            }).OrderBy(a => a.StockAdjustmentReason).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockAdjustmentReasons = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
