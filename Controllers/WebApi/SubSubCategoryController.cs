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
    public class SubSubCategoryController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSubSubCategories(ClsSubSubCategoryVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSubSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                SubSubCategoryId = a.SubSubCategoryId,
                a.CategoryId,
                a.SubCategoryId,
                Category = oConnectionContext.DbClsCategory.Where(b => b.CategoryId == a.CategoryId).Select(b => b.Category).FirstOrDefault(),
                SubCategory = oConnectionContext.DbClsSubCategory.Where(b => b.SubCategoryId == a.SubCategoryId).Select(b => b.SubCategory).FirstOrDefault(),
                //a.SubSubCategoryCode,
                SubSubCategory = a.SubSubCategory,
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
                det = det.Where(a => a.Category.Contains(obj.Search.ToLower()) || a.SubCategory.Contains(obj.Search.ToLower()) ||
                a.SubSubCategory.Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SubSubCategories = det.OrderByDescending(a => a.SubSubCategoryId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubSubCategory(ClsSubSubCategory obj)
        {
            var det = oConnectionContext.DbClsSubSubCategory.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SubSubCategoryId = a.SubSubCategoryId,
                //a.SubSubCategoryCode,
                a.CategoryId,
                a.SubCategoryId,
                SubSubCategory = a.SubSubCategory,
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

            var SubCategories = oConnectionContext.DbClsSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.CategoryId == det.CategoryId).Select(a => new
            {
                SubCategoryId = a.SubCategoryId,
                //a.SubCategoryCode,
                SubCategory = a.SubCategory,
            }).OrderBy(a => a.SubCategory).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SubSubCategory = det,
                    Categories = Categories,
                    SubCategories = SubCategories
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSubSubCategory(ClsSubSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.SubSubCategory == null || obj.SubSubCategory == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubSubCategory" });
                    isError = true;
                }

                if (obj.CategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategory" });
                    isError = true;
                }

                if (obj.SubCategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubCategory" });
                    isError = true;
                }

                //if (obj.SubSubCategoryCode != "" && obj.SubSubCategoryCode != null)
                //{
                //    if (oConnectionContext.DbClsSubSubCategory.Where(a => a.CategoryId == obj.CategoryId && a.SubCategoryId == obj.SubCategoryId &&
                //    a.SubSubCategoryCode.ToLower() == obj.SubSubCategoryCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Sub Sub Category Code exists", Id = "divSubSubCategoryCode" });
                //        isError = true;
                //    }
                //}

                if (obj.SubSubCategory != null && obj.SubSubCategory != "")
                {
                    if (oConnectionContext.DbClsSubSubCategory.Where(a => a.CategoryId == obj.CategoryId && a.SubCategoryId == obj.SubCategoryId &&
                    a.SubSubCategory.ToLower() == obj.SubSubCategory.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sub Sub Category Name exists", Id = "divSubSubCategory" });
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

                //if (obj.SubSubCategoryCode == "" || obj.SubSubCategoryCode == null)
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Sub Sub Category"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.SubSubCategoryCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsSubSubCategory oSubSubCategory = new ClsSubSubCategory()
                {
                    SubSubCategory = obj.SubSubCategory,
                    //SubSubCategoryCode = obj.SubSubCategoryCode,
                    CategoryId = obj.CategoryId,
                    SubCategoryId = obj.SubCategoryId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsSubSubCategory.Add(oSubSubCategory);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Sub Category \"" + obj.SubSubCategory +"\" created",
                    Id = oSubSubCategory.SubSubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Sub Category created successfully",
                    Data = new
                    {
                        SubSubCategory = oSubSubCategory
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSubSubCategory(ClsSubSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.SubSubCategory == null || obj.SubSubCategory == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubSubCategory" });
                    isError = true;
                }

                if (obj.CategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategory" });
                    isError = true;
                }

                if (obj.SubCategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubCategory" });
                    isError = true;
                }

                if (obj.SubSubCategory != "" && obj.SubSubCategory != null)
                {
                    if (oConnectionContext.DbClsSubSubCategory.Where(a => a.CategoryId == obj.CategoryId && a.SubCategoryId == obj.SubCategoryId &&
                    a.SubSubCategory.ToLower() == obj.SubSubCategory.ToLower() && a.SubSubCategoryId != obj.SubSubCategoryId && a.CompanyId == obj.CompanyId &&
                    a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sub Sub Category Name exists", Id = "divSubSubCategory" });
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

                //if (obj.SubSubCategoryCode != "" && obj.SubSubCategoryCode != null)
                //{
                //    if (oConnectionContext.DbClsSubSubCategory.Where(a => a.SubSubCategoryCode == obj.SubSubCategoryCode && a.SubSubCategoryId != obj.SubSubCategoryId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Sub Sub Category code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}

                ClsSubSubCategory oSubSubCategory = new ClsSubSubCategory()
                {
                    SubSubCategoryId = obj.SubSubCategoryId,
                    SubCategoryId = obj.SubCategoryId,
                    //SubSubCategoryCode = obj.SubSubCategoryCode,
                    CategoryId = obj.CategoryId,
                    SubSubCategory = obj.SubSubCategory,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSubSubCategory.Attach(oSubSubCategory);
                oConnectionContext.Entry(oSubSubCategory).Property(x => x.SubSubCategoryId).IsModified = true;
                oConnectionContext.Entry(oSubSubCategory).Property(x => x.SubCategoryId).IsModified = true;
                oConnectionContext.Entry(oSubSubCategory).Property(x => x.SubSubCategory).IsModified = true;
                //oConnectionContext.Entry(oSubSubCategory).Property(x => x.SubSubCategoryCode).IsModified = true;
                oConnectionContext.Entry(oSubSubCategory).Property(x => x.CategoryId).IsModified = true;
                oConnectionContext.Entry(oSubSubCategory).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSubSubCategory).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Sub Category \"" + obj.SubSubCategory+"\" updated",
                    Id = oSubSubCategory.SubSubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Sub Category updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubSubCategoryActiveInactive(ClsSubSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSubSubCategory oClsRole = new ClsSubSubCategory()
                {
                    SubSubCategoryId = obj.SubSubCategoryId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSubSubCategory.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SubSubCategoryId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Sub Category \"" +oConnectionContext.DbClsSubSubCategory.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).Select(a => a.SubSubCategory).FirstOrDefault()+(obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsRole.SubSubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Sub Category " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubSubCategoryDelete(ClsSubSubCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.SubSubCategoryId == obj.SubSubCategoryId && a.IsDeleted == false).Count();
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
                ClsSubSubCategory oClsRole = new ClsSubSubCategory()
                {
                    SubSubCategoryId = obj.SubSubCategoryId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSubSubCategory.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SubSubCategoryId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Sub Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Sub Sub Category \"" + oConnectionContext.DbClsSubSubCategory.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).Select(a => a.SubSubCategory).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.SubSubCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Sub Category deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSubSubCategories(ClsSubSubCategoryVm obj)
        {
            var det = oConnectionContext.DbClsSubSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.SubCategoryId == obj.SubCategoryId).Select(a => new
            {
                SubSubCategoryId = a.SubSubCategoryId,
                //a.SubSubCategoryCode,
                SubSubCategory = a.SubSubCategory,
            }).OrderBy(a => a.SubSubCategory).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SubSubCategories = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SubSubCategoryPurchaseSalesReport(ClsPurchaseSales obj)
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

            List<ClsPurchaseSales> det = oConnectionContext.DbClsSubSubCategory.Where(a => a.IsActive == true && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
            {
                Name = a.SubSubCategory,
                TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                 where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                TotalPurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                     join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                     join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && e.Type.ToLower() == "purchase payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                     select e.Amount).DefaultIfEmpty().Sum(),
                TotalPurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                    join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                    join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                    where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                    select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                   join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                                   where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && e.Type.ToLower() == "purchase payment" &&
                                                                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                   select e.Amount).DefaultIfEmpty().Sum(),
                TotalSales = (from b in oConnectionContext.DbClsSales
                              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                              join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                              where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                              select b.GrandTotal).DefaultIfEmpty().Sum(),
                TotalSalesPaid = (from b in oConnectionContext.DbClsSales
                                  join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                  join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                  join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                  where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && (e.Type.ToLower() == "sales payment") &&
                                  b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && d.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                  select e.Amount).DefaultIfEmpty().Sum(),
                TotalSalesDue = (from b in oConnectionContext.DbClsSales
                                 join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                 where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                 select b.GrandTotal - b.WriteOffAmount).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsSales
                                                                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                                join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                                where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && (e.Type.ToLower() == "sales payment") &&
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

        public async Task<IHttpActionResult> SubSubCategoryProfitLossReport(ClsPurchaseSales obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsPurchaseSales> det = oConnectionContext.DbClsSubSubCategory.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new ClsPurchaseSales
            {
                Name = a.SubSubCategory,
                //TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                //                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                //                 join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //                 where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                //TotalSales = (from b in oConnectionContext.DbClsSales
                //              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                //              join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //              where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //              select b.GrandTotal).DefaultIfEmpty().Sum(),
                //TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                //                      join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                //                      join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                //                      where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                //                      select b.TotalQuantity).DefaultIfEmpty().Sum(),
                TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                    join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                    join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                    where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                    select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                   join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                   where b.BranchId == obj.BranchId && d.SubSubCategoryId == a.SubSubCategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
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
