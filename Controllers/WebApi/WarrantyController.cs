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
    public class WarrantyController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllWarrantys(ClsWarrantyVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            var det = oConnectionContext.DbClsWarranty.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                WarrantyId = a.WarrantyId,
                Warranty = a.Warranty,
                a.Description,
                a.Duration,
                a.DurationNo,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Warranty.ToLower().Contains(obj.Search.ToLower()) || a.Description.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Warrantys = det.OrderByDescending(a => a.WarrantyId).Skip(skip).Take(obj.PageSize),
                    TotalCount = det.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count(),
                    ActiveCount = det.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Warranty(ClsWarranty obj)
        {
            var det = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == obj.WarrantyId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                WarrantyId = a.WarrantyId,
                Warranty = a.Warranty,
                a.Description,
                a.Duration,
                a.DurationNo,
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
                    Warranty = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertWarranty(ClsWarrantyVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Warranty == null || obj.Warranty == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWarranty" });
                    isError = true;
                }

                if (obj.DurationNo == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDurationNo" });
                    isError = true;
                }

                if (obj.Warranty != null && obj.Warranty != "")
                {
                    if (oConnectionContext.DbClsWarranty.Where(a => a.Warranty.ToLower() == obj.Warranty.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Warranty exists", Id = "divWarranty" });
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

                ClsWarranty oWarranty = new ClsWarranty()
                {
                    Warranty = obj.Warranty,
                    Description = obj.Description,
                    Duration = obj.Duration,
                    DurationNo = obj.DurationNo,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsWarranty.Add(oWarranty);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Warranties",
                    CompanyId = obj.CompanyId,
                    Description = "Warranty \"" + obj.Warranty + "\" created",
                    Id = oWarranty.WarrantyId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Warranty created successfully",
                    Data = new
                    {
                        Warranty = oWarranty
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateWarranty(ClsWarrantyVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Warranty == null || obj.Warranty == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWarranty" });
                    isError = true;
                }

                if (obj.DurationNo == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDurationNo" });
                    isError = true;
                }

                if (obj.Warranty != null && obj.Warranty != "")
                {
                    if (oConnectionContext.DbClsWarranty.Where(a => a.Warranty.ToLower() == obj.Warranty.ToLower() && a.WarrantyId != obj.WarrantyId && a.CompanyId == obj.CompanyId
                && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Warranty exists", Id = "divWarranty" });
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

                ClsWarranty oWarranty = new ClsWarranty()
                {
                    WarrantyId = obj.WarrantyId,
                    Warranty = obj.Warranty,
                    Description = obj.Description,
                    Duration = obj.Duration,
                    DurationNo = obj.DurationNo,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsWarranty.Attach(oWarranty);
                oConnectionContext.Entry(oWarranty).Property(x => x.WarrantyId).IsModified = true;
                oConnectionContext.Entry(oWarranty).Property(x => x.Warranty).IsModified = true;
                oConnectionContext.Entry(oWarranty).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oWarranty).Property(x => x.Duration).IsModified = true;
                oConnectionContext.Entry(oWarranty).Property(x => x.DurationNo).IsModified = true;
                oConnectionContext.Entry(oWarranty).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oWarranty).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Warranties",
                    CompanyId = obj.CompanyId,
                    Description = "Warranty \"" + obj.Warranty + "\" updated",
                    Id = oWarranty.WarrantyId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Warranty updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> WarrantyActiveInactive(ClsWarrantyVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsWarranty oClsRole = new ClsWarranty()
                {
                    WarrantyId = obj.WarrantyId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsWarranty.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.WarrantyId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Warranties",
                    CompanyId = obj.CompanyId,
                    Description = "Warranty \"" + oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == obj.WarrantyId).Select(a => a.Warranty).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.WarrantyId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Warranty " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> WarrantyDelete(ClsWarrantyVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.WarrantyId == obj.WarrantyId && a.IsDeleted == false).Count();
                int SalesQuotationCount = (from a in oConnectionContext.DbClsSalesQuotation
                                           join b in oConnectionContext.DbClsSalesQuotationDetails
                                           on a.SalesQuotationId equals b.SalesQuotationId
                                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.WarrantyId == obj.WarrantyId
                                           select a.SalesQuotationId).Count();

                int SalesOrderCount = (from a in oConnectionContext.DbClsSalesOrder
                                       join b in oConnectionContext.DbClsSalesOrderDetails
                                           on a.SalesOrderId equals b.SalesOrderId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.WarrantyId == obj.WarrantyId
                                       select a.SalesOrderId).Count();

                int SalesProformaCount = (from a in oConnectionContext.DbClsSalesProforma
                                          join b in oConnectionContext.DbClsSalesProformaDetails
                                           on a.SalesProformaId equals b.SalesProformaId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.WarrantyId == obj.WarrantyId
                                          select a.SalesProformaId).Count();

                int DeliveryChallanCount = (from a in oConnectionContext.DbClsDeliveryChallan
                                            join b in oConnectionContext.DbClsDeliveryChallanDetails
                                           on a.DeliveryChallanId equals b.DeliveryChallanId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                           && b.WarrantyId == obj.WarrantyId
                                            select a.DeliveryChallanId).Count();

                int SalesCount = (from a in oConnectionContext.DbClsSales
                                  join b in oConnectionContext.DbClsSalesDetails
                                   on a.SalesId equals b.SalesId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                   && b.WarrantyId == obj.WarrantyId
                                  select a.SalesId).Count();

                if (ItemCount > 0 || SalesQuotationCount > 0 || SalesOrderCount > 0 || SalesProformaCount > 0
                    || DeliveryChallanCount > 0 || SalesCount > 0)
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
                ClsWarranty oClsRole = new ClsWarranty()
                {
                    WarrantyId = obj.WarrantyId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsWarranty.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.WarrantyId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Warranties",
                    CompanyId = obj.CompanyId,
                    Description = "Warranty \"" + oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == obj.WarrantyId).Select(a => a.Warranty).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.WarrantyId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Warranty deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveWarrantys(ClsWarrantyVm obj)
        {
            var det = oConnectionContext.DbClsWarranty.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                WarrantyId = a.WarrantyId,
                Warranty = a.Warranty,
            }).OrderBy(a => a.Warranty).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Warrantys = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
