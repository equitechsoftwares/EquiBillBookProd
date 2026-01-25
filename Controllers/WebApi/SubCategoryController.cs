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
    public class SubCategoryController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSubCategories(ClsSubCategoryVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                SubCategoryId = a.SubCategoryId,
                a.CategoryId,
                Category = oConnectionContext.DbClsCategory.Where(b => b.CategoryId == a.CategoryId).Select(b => b.Category).FirstOrDefault(),
                //a.SubCategoryCode,
                SubCategory = a.SubCategory,
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
                det = det.Where(a => a.Category.ToLower().Contains(obj.Search.ToLower()) ||
                a.SubCategory.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SubCategories = det.OrderByDescending(a => a.SubCategoryId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubCategory(ClsSubCategory obj)
        {
            var det = oConnectionContext.DbClsSubCategory.Where(a => a.SubCategoryId == obj.SubCategoryId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SubCategoryId = a.SubCategoryId,
                //a.SubCategoryCode,
                a.CategoryId,
                SubCategory = a.SubCategory,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).FirstOrDefault();

            var Categories = oConnectionContext.DbClsCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CategoryId = a.CategoryId,
                //a.CategoryCode,
                Category = a.Category,
            }).OrderBy(a => a.Category).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SubCategory = det,
                    Categories = Categories
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSubCategory(ClsSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.CategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategory" });
                    isError = true;
                }

                if (obj.SubCategory == "" || obj.SubCategory == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubCategory" });
                    isError = true;
                }

                //if (obj.SubCategoryCode != "" && obj.SubCategoryCode != null)
                //{
                //    if (oConnectionContext.DbClsSubCategory.Where(a => a.CategoryId == obj.CategoryId && a.SubCategoryCode.ToLower() == obj.SubCategoryCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Sub Category Code exists", Id = "divSubCategoryCode" });
                //        isError = true;
                //    }
                //}

                if (obj.SubCategory != "" && obj.SubCategory != null)
                {
                    if (oConnectionContext.DbClsSubCategory.Where(a => a.CategoryId == obj.CategoryId && a.SubCategory.ToLower() == obj.SubCategory.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sub Category Name exists", Id = "divSubCategory" });
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

                //if (obj.SubCategoryCode == "" || obj.SubCategoryCode == null)
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Sub Category"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.SubCategoryCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsSubCategory oSubCategory = new ClsSubCategory()
                {
                    SubCategory = obj.SubCategory,
                    //SubCategoryCode = obj.SubCategoryCode,
                    CategoryId = obj.CategoryId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsSubCategory.Add(oSubCategory);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Category \"" + obj.SubCategory+"\" created",
                    Id = oSubCategory.SubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Category created successfully",
                    Data = new
                    {
                        SubCategory = oSubCategory
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSubCategory(ClsSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.CategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategory" });
                    isError = true;
                }

                if (obj.SubCategory == "" || obj.SubCategory == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubCategory" });
                    isError = true;
                }

                //if (obj.SubCategoryCode != "" && obj.SubCategoryCode != null)
                //{
                //    if (oConnectionContext.DbClsSubCategory.Where(a => a.SubCategoryCode == obj.SubCategoryCode && a.SubCategoryId != obj.SubCategoryId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Sub Category code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}

                if (obj.SubCategory != null && obj.SubCategory != "")
                {
                    if (oConnectionContext.DbClsSubCategory.Where(a => a.CategoryId == obj.CategoryId && a.SubCategory.ToLower() == obj.SubCategory.ToLower() && a.SubCategoryId != obj.SubCategoryId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sub Category Name exists", Id = "divSubCategory" });
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

                ClsSubCategory oSubCategory = new ClsSubCategory()
                {
                    SubCategoryId = obj.SubCategoryId,
                    //SubCategoryCode = obj.SubCategoryCode,
                    CategoryId = obj.CategoryId,
                    SubCategory = obj.SubCategory,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSubCategory.Attach(oSubCategory);
                oConnectionContext.Entry(oSubCategory).Property(x => x.SubCategoryId).IsModified = true;
                oConnectionContext.Entry(oSubCategory).Property(x => x.SubCategory).IsModified = true;
                //oConnectionContext.Entry(oSubCategory).Property(x => x.SubCategoryCode).IsModified = true;
                oConnectionContext.Entry(oSubCategory).Property(x => x.CategoryId).IsModified = true;
                oConnectionContext.Entry(oSubCategory).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSubCategory).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Category \""+oConnectionContext.DbClsSubCategory.Where(a => a.SubCategoryId == obj.SubCategoryId).Select(a => a.SubCategory).FirstOrDefault()+"\" updated",
                    Id = oSubCategory.SubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Category updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubCategoryActiveInactive(ClsSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSubCategory oClsRole = new ClsSubCategory()
                {
                    SubCategoryId = obj.SubCategoryId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSubCategory.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SubCategoryId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Category \"" + oConnectionContext.DbClsSubCategory.Where(a => a.SubCategoryId == obj.SubCategoryId).Select(a => a.SubCategory).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsRole.SubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Category " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubCategoryDelete(ClsSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int SubSubCategoryCount = oConnectionContext.DbClsSubSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.SubCategoryId == obj.SubCategoryId && a.IsDeleted == false).Count();
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.SubCategoryId == obj.SubCategoryId && a.IsDeleted == false).Count();
                if (SubSubCategoryCount > 0 || ItemCount > 0)
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
                ClsSubCategory oClsRole = new ClsSubCategory()
                {
                    SubCategoryId = obj.SubCategoryId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSubCategory.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SubCategoryId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Category \"" + oConnectionContext.DbClsSubCategory.Where(a => a.SubCategoryId == obj.SubCategoryId).Select(a => a.SubCategory).FirstOrDefault() +"\" deleted",
                    Id = oClsRole.SubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                data = new
                {
                    Status = 1,
                    Message = "Sub Category deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSubCategories(ClsSubCategoryVm obj)
        {
            var det = oConnectionContext.DbClsSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.CategoryId == obj.CategoryId).Select(a => new
            {
                SubCategoryId = a.SubCategoryId,
                SubCategory = a.SubCategory,
                SubSubCategoryCount = oConnectionContext.DbClsSubSubCategory.Where(b => b.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsActive == true).Count(),
            }).OrderBy(a => a.SubCategory).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SubCategories = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubCategoryPurchaseSalesReport(ClsPurchaseSales obj)
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

            List<ClsPurchaseSales> det = oConnectionContext.DbClsSubCategory.Where(a => a.IsActive == true && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
            {
                Name = a.SubCategory,
                TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                 where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                TotalPurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                     join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                     join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && e.Type.ToLower() == "purchase payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                     select e.Amount).DefaultIfEmpty().Sum(),
                TotalPurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                    join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                    join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                    where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                    select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                   join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                                   where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && e.Type.ToLower() == "purchase payment" &&
                                                                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                   select e.Amount).DefaultIfEmpty().Sum(),
                TotalSales = (from b in oConnectionContext.DbClsSales
                              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                              join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                              where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                              select b.GrandTotal).DefaultIfEmpty().Sum(),
                TotalSalesPaid = (from b in oConnectionContext.DbClsSales
                                  join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                  join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                  join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                  where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && (e.Type.ToLower() == "sales payment") &&
                                  b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                  select e.Amount).DefaultIfEmpty().Sum(),
                TotalSalesDue = (from b in oConnectionContext.DbClsSales
                                 join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                 where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                 select b.GrandTotal - b.WriteOffAmount).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsSales
                                                                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                                join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                                where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && (e.Type.ToLower() == "sales payment") &&
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

        public async Task<IHttpActionResult> SubCategoryProfitLossReport(ClsPurchaseSales obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsPurchaseSales> det = oConnectionContext.DbClsSubCategory.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new ClsPurchaseSales
            {
                Name = a.SubCategory,
                //TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                //                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                //                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //                 where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                //TotalSales = (from b in oConnectionContext.DbClsSales
                //              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                //              join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //              where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //              select b.GrandTotal).DefaultIfEmpty().Sum(),
                //TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                //                      join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                //                      join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //                      where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //                      select b.TotalQuantity).DefaultIfEmpty().Sum(),
                TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                    join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                    join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                    where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                    select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                   where b.BranchId == obj.BranchId && d.SubCategoryId == a.SubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                                                                   select b.GrandTotal).DefaultIfEmpty().Sum()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSales = det,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
