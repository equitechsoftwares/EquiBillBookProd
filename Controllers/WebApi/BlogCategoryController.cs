using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class BlogCategoryController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllBlogCategories(ClsBlogCategoryVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsBlogCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.BlogCategoryId,
                a.CategoryName,
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
                det = det.Where(a => a.CategoryName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BlogCategories = det.OrderByDescending(a => a.BlogCategoryId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BlogCategory(ClsBlogCategory obj)
        {
            var det = oConnectionContext.DbClsBlogCategory.Where(a => a.BlogCategoryId == obj.BlogCategoryId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.BlogCategoryId,
                a.CategoryName,
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
                    BlogCategory = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertBlogCategory(ClsBlogCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.CategoryName == null || obj.CategoryName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategoryName" });
                    isError = true;
                }

                if (obj.CategoryName != "" && obj.CategoryName != null)
                {
                    if (oConnectionContext.DbClsBlogCategory.Where(a => a.CategoryName.ToLower() == obj.CategoryName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Category Name exists", Id = "divCategoryName" });
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

                ClsBlogCategory oBlogCategory = new ClsBlogCategory()
                {
                    CategoryName = obj.CategoryName,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsBlogCategory.Add(oBlogCategory);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Blog Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Blog Category \"" + obj.CategoryName + "\" created",
                    Id = oBlogCategory.BlogCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Blog Category created successfully",
                    Data = new
                    {
                        BlogCategory = oBlogCategory
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateBlogCategory(ClsBlogCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.CategoryName == null || obj.CategoryName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategoryName" });
                    isError = true;
                }

                if (obj.CategoryName != "" && obj.CategoryName != null)
                {
                    if (oConnectionContext.DbClsBlogCategory.Where(a => a.CategoryName.ToLower() == obj.CategoryName.ToLower() && a.BlogCategoryId != obj.BlogCategoryId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Category Name exists", Id = "divCategoryName" });
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

                ClsBlogCategory oBlogCategory = new ClsBlogCategory()
                {
                    BlogCategoryId = obj.BlogCategoryId,
                    CategoryName = obj.CategoryName,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsBlogCategory.Attach(oBlogCategory);
                oConnectionContext.Entry(oBlogCategory).Property(x => x.BlogCategoryId).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.CategoryName).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.ModifiedBy,
                    Browser = obj.Browser,
                    Category = "Blog Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Blog Category \"" + obj.CategoryName + "\" updated",
                    Id = oBlogCategory.BlogCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Blog Category updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BlogCategoryActiveInactive(ClsBlogCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsBlogCategory oBlogCategory = new ClsBlogCategory()
                {
                    BlogCategoryId = obj.BlogCategoryId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBlogCategory.Attach(oBlogCategory);
                oConnectionContext.Entry(oBlogCategory).Property(x => x.BlogCategoryId).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Blog Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Blog Category \"" + oConnectionContext.DbClsBlogCategory.Where(a => a.BlogCategoryId == obj.BlogCategoryId).Select(a => a.CategoryName).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oBlogCategory.BlogCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Blog Category " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BlogCategoryDelete(ClsBlogCategoryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int BlogCount = oConnectionContext.DbClsBlog.Where(a => a.CompanyId == obj.CompanyId && a.BlogCategoryId == obj.BlogCategoryId && a.IsDeleted == false).Count();
                if (BlogCount > 0)
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
                ClsBlogCategory oBlogCategory = new ClsBlogCategory()
                {
                    BlogCategoryId = obj.BlogCategoryId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBlogCategory.Attach(oBlogCategory);
                oConnectionContext.Entry(oBlogCategory).Property(x => x.BlogCategoryId).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBlogCategory).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Blog Categories",
                    CompanyId = obj.CompanyId,
                    Description = "Blog Category \"" + oConnectionContext.DbClsBlogCategory.Where(a => a.BlogCategoryId == obj.BlogCategoryId).Select(a => a.CategoryName).FirstOrDefault() + "\" deleted",
                    Id = oBlogCategory.BlogCategoryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Blog Category deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveBlogCategories(ClsBlogCategoryVm obj)
        {
            var det = oConnectionContext.DbClsBlogCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                BlogCategoryId = a.BlogCategoryId,
                CategoryName = a.CategoryName,
            }).OrderBy(a => a.CategoryName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BlogCategories = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}

