using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
    public class BrandController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllBrands(ClsBrandVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsBrand.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                BrandId = a.BrandId,
                Brand = a.Brand,
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
                det = det.Where(a => a.Brand.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Brands = det.OrderByDescending(a => a.BrandId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Brand(ClsBrand obj)
        {
            var det = oConnectionContext.DbClsBrand.Where(a => a.BrandId == obj.BrandId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                BrandId = a.BrandId,
                Brand = a.Brand,
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
                    Brand = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertBrand(ClsBrandVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                long PrefixUserMapId = 0;
                if (obj.Brand == null || obj.Brand == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBrand" });
                    isError = true;
                }

                //if (obj.BrandCode != "" && obj.BrandCode != null)
                //{
                //    if (oConnectionContext.DbClsBrand.Where(a => a.BrandCode.ToLower() == obj.BrandCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Brand Code exists", Id = "divBrandCode" });
                //        isError = true;
                //    }
                //}

                if (obj.Brand != null && obj.Brand != "")
                {
                    if (oConnectionContext.DbClsBrand.Where(a => a.Brand.ToLower() == obj.Brand.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Brand Name exists", Id = "divBrand" });
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

                //if (obj.BrandCode == "" || obj.BrandCode == null)
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Brand"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.BrandCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsBrand oBrand = new ClsBrand()
                {
                    Brand = obj.Brand,
                    //BrandCode = obj.BrandCode,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsBrand.Add(oBrand);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Brand",
                    CompanyId = obj.CompanyId,
                    Description = "Brand \"" + obj.Brand +"\" created",
                    Id = oBrand.BrandId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Brand created successfully",
                    Data = new
                    {
                        Brand = oBrand
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateBrand(ClsBrandVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Brand == null || obj.Brand == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBrand" });
                    isError = true;
                }

                //if (obj.BrandCode != "" && obj.BrandCode != null)
                //{
                //    if (oConnectionContext.DbClsBrand.Where(a => a.BrandCode == obj.BrandCode && a.BrandId != obj.BrandId && a.CompanyId == obj.CompanyId).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Brand Code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}

                if (obj.Brand != null && obj.Brand != "")
                {
                    if (oConnectionContext.DbClsBrand.Where(a => a.Brand.ToLower() == obj.Brand.ToLower() && a.BrandId != obj.BrandId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Brand Name exists", Id = "divBrand" });
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

                ClsBrand oBrand = new ClsBrand()
                {
                    BrandId = obj.BrandId,
                    Brand = obj.Brand,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBrand.Attach(oBrand);
                oConnectionContext.Entry(oBrand).Property(x => x.BrandId).IsModified = true;
                oConnectionContext.Entry(oBrand).Property(x => x.Brand).IsModified = true;
                oConnectionContext.Entry(oBrand).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBrand).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Brand",
                    CompanyId = obj.CompanyId,
                    Description = "Brand \"" + obj.Brand+"\" updated",
                    Id = oBrand.BrandId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Brand updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BrandActiveInactive(ClsBrandVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsBrand oClsRole = new ClsBrand()
                {
                    BrandId = obj.BrandId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBrand.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.BrandId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Brand",
                    CompanyId = obj.CompanyId,
                    Description = "Brand \""+oConnectionContext.DbClsBrand.Where(a => a.BrandId == obj.BrandId).Select(a => a.Brand).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.BrandId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Brand " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BrandDelete(ClsBrandVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.BrandId == obj.BrandId && a.IsDeleted == false).Count();
                if (ItemCount > 0)
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
                ClsBrand oClsRole = new ClsBrand()
                {
                    BrandId = obj.BrandId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBrand.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.BrandId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Brand",
                    CompanyId = obj.CompanyId,
                    Description = "Brand \"" + oConnectionContext.DbClsBrand.Where(a => a.BrandId == obj.BrandId).Select(a => a.Brand).FirstOrDefault()+"\" deleted",
                    Id = oClsRole.BrandId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Brand deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveBrands(ClsBrandVm obj)
        {
            var det = oConnectionContext.DbClsBrand.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                BrandId = a.BrandId,
                Brand = a.Brand,
            }).OrderBy(a => a.Brand).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Brands = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> PublicBrands(ClsBrandVm obj)
        {
            if (obj == null || obj.CompanyId <= 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "CompanyId is required",
                    Data = new
                    {
                        Brands = new List<object>()
                    }
                };

                return await Task.FromResult(Ok(data));
            }

            var det = oConnectionContext.DbClsBrand.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true)
                .Select(a => new
                {
                    BrandId = a.BrandId,
                    Brand = a.Brand,
                })
                .OrderBy(a => a.Brand)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Brands = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BrandPurchaseSalesReport(ClsPurchaseSales obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
                BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
              && b.IsDeleted == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
               oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            }).FirstOrDefault();

            if (obj.BranchId == 0)
            {
                obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            }

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsPurchaseSales> det = oConnectionContext.DbClsBrand.Where(a => a.IsActive == true && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
            {
                Name = a.Brand,
                TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                 where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                TotalPurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                     join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                     join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && e.Type.ToLower() == "purchase payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                     select e.Amount).DefaultIfEmpty().Sum(),
                TotalPurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                    join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                    join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                    where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                    select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                   join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                                   where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && e.Type.ToLower() == "purchase payment" &&
                                                                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                   select e.Amount).DefaultIfEmpty().Sum(),
                TotalSales = (from b in oConnectionContext.DbClsSales
                              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                              join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                              where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                              select b.GrandTotal).DefaultIfEmpty().Sum(),
                TotalSalesPaid = (from b in oConnectionContext.DbClsSales
                                  join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                  join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                  join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                  where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && (e.Type.ToLower() == "sales payment") &&
                                  b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                  select e.Amount).DefaultIfEmpty().Sum(),
                TotalSalesDue = (from b in oConnectionContext.DbClsSales
                                 join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                 where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                 select b.GrandTotal - b.WriteOffAmount).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsSales
                                                                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                                join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                                where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && (e.Type.ToLower() == "sales payment") &&
                                                                                b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                select e.Amount).DefaultIfEmpty().Sum(),
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSales = det,
                    Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BrandProfitLossReport(ClsUserVm obj)
        {
            List<ClsPurchaseSales> PurchaseSales = oConnectionContext.DbClsBrand.Where(a => a.IsActive == true && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
            {
                Name = a.Brand,
                //TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                //                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                //                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //                 where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //                 && DbFunctions.TruncateTime(b.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                //                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                //TotalSales = (from b in oConnectionContext.DbClsSales
                //              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                //              join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //              where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //              && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                //              select b.GrandTotal).DefaultIfEmpty().Sum(),
                //TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                //                      join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                //                      join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //                      where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //                      && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                //                      select b.TotalQuantity).DefaultIfEmpty().Sum(),
                TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                    join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                    join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                    where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                    && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                    select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                   where b.BranchId == obj.BranchId && d.BrandId == a.BrandId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                                                                   && DbFunctions.TruncateTime(b.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                                                                   select b.GrandTotal).DefaultIfEmpty().Sum()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSales = PurchaseSales
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesByBrandReport(ClsSalesDetailsVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            //}).FirstOrDefault();

            //if (obj.BranchId == 0)
            //{
            //    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            //}

            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsPurchaseSales> det = oConnectionContext.DbClsBrand.Where(a => a.IsActive == true && a.IsDeleted == false
            && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
            {
                BrandId = a.BrandId,
                Name = a.Brand,
                SalesDetails = (from b in oConnectionContext.DbClsSales
                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                where d.BrandId == a.BrandId && b.Status != "Draft"
                                && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                select new ClsSalesDetailsVm
                                {
                                    BranchId = b.BranchId,
                                    ItemId = c.ItemId,
                                    CustomerId = b.CustomerId,
                                    SubCategoryId = d.SubCategoryId,
                                    SubSubCategoryId = d.SubSubCategoryId,
                                    BrandId = d.BrandId,
                                    AmountIncTax = c.AmountIncTax,
                                    Quantity = c.QuantitySold
                                }).ToList(),
                SalesReturnDetails = (from b in oConnectionContext.DbClsSalesReturn
                                      join c in oConnectionContext.DbClsSalesReturnDetails on b.SalesReturnId equals c.SalesReturnId
                                      join d in oConnectionContext.DbClsSales on b.SalesId equals d.SalesId
                                      join e in oConnectionContext.DbClsItem on c.ItemId equals e.ItemId
                                      where e.BrandId == a.BrandId && b.Status != "Draft"
                                && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false
                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      select new ClsSalesReturnDetailsVm
                                      {
                                          BranchId = d.BranchId,
                                          ItemId = c.ItemId,
                                          CustomerId = d.CustomerId,
                                          CategoryId = e.CategoryId,
                                          SubCategoryId = e.SubCategoryId,
                                          SubSubCategoryId = e.SubSubCategoryId,
                                          BrandId = e.BrandId,
                                          AmountIncTax = c.AmountIncTax,
                                          Quantity = c.QuantityReturned
                                      }).ToList(),
            }).ToList();

            if (obj.BranchId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    BrandId = a.BrandId,
                    Name = a.Name,
                    SalesDetails = a.SalesDetails.Where(b => b.BranchId == obj.BranchId).Select(b => new ClsSalesDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                    SalesReturnDetails = a.SalesReturnDetails.Where(b => b.BranchId == obj.BranchId).Select(b => new ClsSalesReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                }).ToList();
            }

            //if (obj.CustomerId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        SalesDetails = a.SalesDetails.Where(b => b.CustomerId == obj.CustomerId).Select(b => new ClsSalesDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.CustomerGroupId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        SalesDetails = a.SalesDetails.Where(b => b.CustomerGroupId == obj.CustomerGroupId).Select(b => new ClsSalesDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.CategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        SalesDetails = a.SalesDetails.Where(b => b.CategoryId == obj.CategoryId).Select(b => new ClsSalesDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.SubCategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        SalesDetails = a.SalesDetails.Where(b => b.SubCategoryId == obj.SubCategoryId).Select(b => new ClsSalesDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.SubSubCategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        SalesDetails = a.SalesDetails.Where(b => b.SubSubCategoryId == obj.SubSubCategoryId).Select(b => new ClsSalesDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.ItemId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        SalesDetails = a.SalesDetails.Where(b => b.ItemId == obj.ItemId).Select(b => new ClsSalesDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            if (obj.BrandId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    BrandId = a.BrandId,
                    Name = a.Name,
                    SalesDetails = a.SalesDetails.Where(b => b.BrandId == obj.BrandId).Select(b => new ClsSalesDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                    SalesReturnDetails = a.SalesReturnDetails.Where(b => b.BrandId == obj.BrandId).Select(b => new ClsSalesReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                }).ToList();
            }

            var SalesDetails = det.Select(a => new ClsSalesDetailsVm
            {
                BrandId = a.BrandId,
                Name = a.Name,
                Quantity = a.SalesDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
                AmountIncTax = a.SalesDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                ReturnedAmountIncTax = a.SalesReturnDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                ReturnedQuantity = a.SalesReturnDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDetails = SalesDetails.Where(a => a.AmountIncTax > 0).OrderByDescending(a => a.BrandId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = SalesDetails.Where(a => a.AmountIncTax > 0).Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDetailsByBrand(ClsSalesDetailsVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesDetails
                                    join b in oConnectionContext.DbClsSales
                                        on a.SalesId equals b.SalesId
                                    join c in oConnectionContext.DbClsItem
                                on a.ItemId equals c.ItemId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                    && a.AmountIncTax != 0 && b.Status != "Draft"
                    && c.BrandId == obj.BrandId
                                    select new ClsBankPaymentVm
                                    {
                                        Id = b.SalesId,
                                        AddedOn = b.SalesDate,
                                        ReferenceNo = b.InvoiceNo,
                                        TransactionAmount = a.AmountIncTax
                                    }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesDetails
                                join b in oConnectionContext.DbClsSales
                                    on a.SalesId equals b.SalesId
                                join c in oConnectionContext.DbClsItem
                            on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                       && b.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && c.BrandId == obj.BrandId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.SalesId,
                                    AddedOn = b.SalesDate,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnDetailsByBrand(ClsSalesDetailsVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesReturnDetails
                                    join b in oConnectionContext.DbClsSalesReturn
                                 on a.SalesReturnId equals b.SalesReturnId
                                    join p in oConnectionContext.DbClsSales
                                    on b.SalesId equals p.SalesId
                                    join c in oConnectionContext.DbClsItem
                                    on a.ItemId equals c.ItemId
                                    where a.CompanyId == obj.CompanyId
                         && a.IsDeleted == false && a.IsActive == true
                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
               && a.AmountIncTax != 0 && c.BrandId == obj.BrandId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                    {
                                        Id = b.SalesReturnId,
                                        AddedOn = b.Date,
                                        ReferenceNo = b.InvoiceNo,
                                        TransactionAmount = a.AmountIncTax
                                    }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesReturnDetails
                                join b in oConnectionContext.DbClsSalesReturn
                             on a.SalesReturnId equals b.SalesReturnId
                                join p in oConnectionContext.DbClsSales
                                on b.SalesId equals p.SalesId
                                join c in oConnectionContext.DbClsItem
                                on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId
                     && a.IsDeleted == false && a.IsActive == true
                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                     && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                     && p.BranchId == obj.BranchId
       && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
           DbFunctions.TruncateTime(b.Date) <= obj.ToDate
           && a.AmountIncTax != 0 && c.BrandId == obj.BrandId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.SalesReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseByBrandReport(ClsSalesDetailsVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            //}).FirstOrDefault();

            //if (obj.BranchId == 0)
            //{
            //    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            //}

            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsPurchaseSales> det = oConnectionContext.DbClsBrand.Where(a => a.IsActive == true && a.IsDeleted == false
            && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
            {
                BrandId = a.BrandId,
                Name = a.Brand,
                PurchaseDetails = (from b in oConnectionContext.DbClsPurchase
                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                where d.BrandId == a.BrandId && b.Status != "Draft"
                                && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                select new ClsPurchaseDetailsVm
                                {
                                    BranchId = b.BranchId,
                                    ItemId = c.ItemId,
                                    SupplierId = b.SupplierId,
                                    SubCategoryId = d.SubCategoryId,
                                    SubSubCategoryId = d.SubSubCategoryId,
                                    BrandId = d.BrandId,
                                    AmountIncTax = c.AmountIncTax,
                                    Quantity = c.Quantity,
                                    QuantityRemaining = c.QuantityRemaining
                                }).ToList(),
                PurchaseReturnDetails = (from b in oConnectionContext.DbClsPurchaseReturn
                                         join c in oConnectionContext.DbClsPurchaseReturnDetails on b.PurchaseReturnId equals c.PurchaseReturnId
                                         join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                         where d.BrandId == a.BrandId && b.Status != "Draft"
                                   && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                   && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select new ClsPurchaseReturnDetailsVm
                                         {
                                             BranchId = b.BranchId,
                                             ItemId = c.ItemId,
                                             SupplierId = b.SupplierId,
                                             SubCategoryId = d.SubCategoryId,
                                             SubSubCategoryId = d.SubSubCategoryId,
                                             BrandId = d.BrandId,
                                             AmountIncTax = c.AmountIncTax,
                                             Quantity = c.Quantity,
                                             QuantityRemaining = c.QuantityRemaining
                                         }).ToList(),
            }).ToList();

            if (obj.BranchId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    BrandId = a.BrandId,
                    Name = a.Name,
                    PurchaseDetails = a.PurchaseDetails.Where(b => b.BranchId == obj.BranchId).Select(b => new ClsPurchaseDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                    PurchaseReturnDetails = a.PurchaseReturnDetails.Where(b => b.BranchId == obj.BranchId).Select(b => new ClsPurchaseReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                }).ToList();
            }

            //if (obj.CustomerId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.SupplierId == obj.SupplierId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.CustomerGroupId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.CustomerGroupId == obj.CustomerGroupId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            CustomerId = b.CustomerId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.CategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.CategoryId == obj.CategoryId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.SubCategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.SubCategoryId == obj.SubCategoryId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.SubSubCategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.SubSubCategoryId == obj.SubSubCategoryId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.ItemId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        BrandId = a.BrandId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.ItemId == obj.ItemId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            if (obj.BrandId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    BrandId = a.BrandId,
                    Name = a.Name,
                    PurchaseDetails = a.PurchaseDetails.Where(b => b.BrandId == obj.BrandId).Select(b => new ClsPurchaseDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining,
                    }).ToList(),
                    PurchaseReturnDetails = a.PurchaseReturnDetails.Where(b => b.BrandId == obj.BrandId).Select(b => new ClsPurchaseReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                }).ToList();
            }

            var PurchaseDetails = det.Select(a => new ClsPurchaseDetailsVm
            {
                ItemId = a.ItemId,
                BrandId = a.BrandId,
                Name = a.Name,
                AmountIncTax = a.PurchaseDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                Quantity = a.PurchaseDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
                QuantityRemaining = a.PurchaseDetails.Select(b => b.QuantityRemaining).DefaultIfEmpty().Sum(),
                ReturnedAmountIncTax = a.PurchaseReturnDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                ReturnedQuantity = a.PurchaseReturnDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDetails = PurchaseDetails.Where(a => a.AmountIncTax > 0).OrderByDescending(a => a.BrandId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = PurchaseDetails.Where(a => a.AmountIncTax > 0).Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDetailsByBrand(ClsSalesDetailsVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                             on a.PurchaseId equals b.PurchaseId
                                join c in oConnectionContext.DbClsItem
                                on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && c.BrandId == obj.BrandId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseId,
                                    AddedOn = b.PurchaseDate,
                                    ReferenceNo = b.ReferenceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                             on a.PurchaseId equals b.PurchaseId
                                join c in oConnectionContext.DbClsItem
                                on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                 && b.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && c.BrandId == obj.BrandId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseId,
                                    AddedOn = b.PurchaseDate,
                                    ReferenceNo = b.ReferenceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnDetailsByBrand(ClsSalesDetailsVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                join b in oConnectionContext.DbClsPurchaseReturn
                             on a.PurchaseReturnId equals b.PurchaseReturnId
                                join c in oConnectionContext.DbClsItem
                                on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId
                                && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                         DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                         && a.AmountIncTax != 0 && c.BrandId == obj.BrandId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                join b in oConnectionContext.DbClsPurchaseReturn
                             on a.PurchaseReturnId equals b.PurchaseReturnId
                                join c in oConnectionContext.DbClsItem
                                on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId
                                && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                && b.BranchId == obj.BranchId
                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                         DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                         && a.AmountIncTax != 0 && c.BrandId == obj.BrandId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }
        
    }
}
