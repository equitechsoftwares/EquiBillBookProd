using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    public class BlogController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        // Public endpoint - Get all published blogs
        [HttpPost]
        public async Task<IHttpActionResult> WebBlogs(ClsBlogVm obj)
        {
            try
            {
                var blogs = (from a in oConnectionContext.DbClsBlog
                             join b in oConnectionContext.DbClsBlogCategory on a.BlogCategoryId equals b.BlogCategoryId into categoryJoin
                             from category in categoryJoin.DefaultIfEmpty()
                             where a.IsDeleted == false &&
                                   a.IsActive == true &&
                                   (obj.CompanyId == 0 || a.CompanyId == obj.CompanyId)
                             select new
                             {
                                 a.BlogId,
                                 a.Title,
                                 a.ShortDescription,
                                 a.Description,
                                 a.Image,
                                 CategoryName = category != null ? category.CategoryName : null,
                                 a.Taglist,
                                 a.UniqueSlug,
                                 a.ViewCount,
                                 AddedOn = a.PublishedDate ?? a.AddedOn,
                                 PublishedDate = a.PublishedDate ?? a.AddedOn
                             })
                             .OrderByDescending(a => a.PublishedDate)
                             .ToList();

                // Apply category filter if provided
                if (!string.IsNullOrWhiteSpace(obj.CategoryFilter))
                {
                    blogs = blogs.Where(a => a.CategoryName != null &&
                                           a.CategoryName.ToLower() == obj.CategoryFilter.ToLower()).ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(obj.Search))
                {
                    string searchLower = obj.Search.ToLower();
                    blogs = blogs.Where(a =>
                        (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                        (a.ShortDescription != null && a.ShortDescription.ToLower().Contains(searchLower)) ||
                        (a.CategoryName != null && a.CategoryName.ToLower().Contains(searchLower)) ||
                        (a.Taglist != null && a.Taglist.ToLower().Contains(searchLower))
                    ).ToList();
                }

                // Apply pagination if needed
                if (obj.PageSize > 0)
                {
                    int skip = obj.PageSize * (obj.PageIndex - 1);
                    blogs = blogs.Skip(skip).Take(obj.PageSize).ToList();
                }

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Blogs = blogs,
                        TotalCount = blogs.Count
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error fetching blogs: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        // Public endpoint - Get single blog by slug
        [HttpPost]
        public async Task<IHttpActionResult> WebBlog(ClsBlogVm obj)
        {
            if (string.IsNullOrWhiteSpace(obj.UniqueSlug) && string.IsNullOrWhiteSpace(obj.Title))
            {
                data = new
                {
                    Status = 0,
                    Message = "Blog slug or title is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Check conditions outside the query to avoid LINQ translation issues
            bool hasUniqueSlug = !string.IsNullOrWhiteSpace(obj.UniqueSlug);
            bool hasTitle = !string.IsNullOrWhiteSpace(obj.Title);
            string uniqueSlug = hasUniqueSlug ? obj.UniqueSlug : null;
            string title = hasTitle ? obj.Title : null;

            var blog = (from a in oConnectionContext.DbClsBlog
                        join b in oConnectionContext.DbClsBlogCategory on a.BlogCategoryId equals b.BlogCategoryId into categoryJoin
                        from category in categoryJoin.DefaultIfEmpty()
                        where a.IsDeleted == false &&
                              a.IsActive == true &&
                              (obj.CompanyId == 0 || a.CompanyId == obj.CompanyId) &&
                              ((hasUniqueSlug && a.UniqueSlug == uniqueSlug) ||
                               (hasTitle && a.Title == title))
                        select new
                        {
                            a.BlogId,
                            a.Title,
                            a.ShortDescription,
                            a.Description,
                            a.Image,
                            CategoryName = category != null ? category.CategoryName : null,
                            a.Taglist,
                            a.UniqueSlug,
                            a.MetaTitle,
                            a.MetaDescription,
                            a.MetaKeywords,
                            a.ViewCount,
                            a.BlogCategoryId,
                            AddedOn = a.PublishedDate ?? a.AddedOn,
                            PublishedDate = a.PublishedDate ?? a.AddedOn
                        })
                        .FirstOrDefault();

            if (blog == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Blog not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Get client IP address
            string clientIpAddress = GetClientIpAddress();
            DateTime currentDate = oCommonController.CurrentDate(obj.CompanyId > 0 ? obj.CompanyId : 1);
            DateTime todayStart = currentDate.Date; // Start of today (00:00:00)
            DateTime tomorrowStart = todayStart.AddDays(1); // Start of tomorrow

            // Store the current ViewCount - will be updated if view is tracked
            long updatedViewCount = blog.ViewCount;

            // Check if this IP has already viewed this blog today (calendar day)
            bool hasViewedToday = oConnectionContext.DbClsBlogView
                .Any(a => a.BlogId == blog.BlogId && 
                          a.IpAddress == clientIpAddress && 
                          a.ViewedOn >= todayStart && 
                          a.ViewedOn < tomorrowStart);

            // Only increment view count if this is a new view today
            if (!hasViewedToday)
            {
                try
                {
                    // Add view tracking record
                    ClsBlogView oClsBlogView = new ClsBlogView
                    {
                        BlogId = blog.BlogId,
                        IpAddress = clientIpAddress,
                        Browser = obj.Browser,
                        Platform = obj.Platform,
                        ViewedOn = currentDate
                    };
                    oConnectionContext.DbClsBlogView.Add(oClsBlogView);

                    // Increment view count
                    var blogEntity = oConnectionContext.DbClsBlog
                        .Where(a => a.BlogId == blog.BlogId)
                        .FirstOrDefault();
                    if (blogEntity != null)
                    {
                        blogEntity.ViewCount = blogEntity.ViewCount + 1;
                        oConnectionContext.SaveChanges();
                        // Get the updated view count
                        updatedViewCount = blogEntity.ViewCount;
                    }
                }
                catch
                {
                    // Handle unique constraint violation (race condition)
                    // If another request from same IP already added the view today, just continue
                    // This prevents errors if two requests come simultaneously
                }
            }

            // Create blog response with updated ViewCount
            var blogResponse = new
            {
                blog.BlogId,
                blog.Title,
                blog.ShortDescription,
                blog.Description,
                blog.Image,
                blog.CategoryName,
                blog.Taglist,
                blog.UniqueSlug,
                blog.MetaTitle,
                blog.MetaDescription,
                blog.MetaKeywords,
                ViewCount = updatedViewCount,
                blog.BlogCategoryId,
                blog.AddedOn,
                blog.PublishedDate
            };

            // Get related blogs (same category, excluding current)
            long? blogCategoryId = blogResponse != null ? blogResponse.BlogCategoryId : null;
            bool hasCategory = blogCategoryId.HasValue && blogCategoryId.Value > 0;

            var relatedBlogs = oConnectionContext.DbClsBlog
                .Where(a => a.IsDeleted == false &&
                            a.IsActive == true &&
                            a.BlogId != blogResponse.BlogId &&
                            (obj.CompanyId == 0 || a.CompanyId == obj.CompanyId) &&
                            (hasCategory && a.BlogCategoryId == blogCategoryId))
                .Select(a => new
                {
                    a.BlogId,
                    a.Title,
                    a.ShortDescription,
                    a.Image,
                    a.UniqueSlug,
                    AddedOn = a.PublishedDate ?? a.AddedOn
                })
                .OrderByDescending(a => a.AddedOn)
                .Take(5)
                .ToList();

            // Get recent blogs
            var recentBlogs = oConnectionContext.DbClsBlog
                .Where(a => a.IsDeleted == false &&
                            a.IsActive == true &&
                            a.BlogId != blogResponse.BlogId &&
                            (obj.CompanyId == 0 || a.CompanyId == obj.CompanyId))
                .Select(a => new
                {
                    a.BlogId,
                    a.Title,
                    a.Image,
                    a.UniqueSlug,
                    AddedOn = a.PublishedDate ?? a.AddedOn
                })
                .OrderByDescending(a => a.AddedOn)
                .Take(5)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Blog = blogResponse,
                    Blogs = recentBlogs,
                    RelatedBlogs = relatedBlogs
                }
            };

            return await Task.FromResult(Ok(data));
        }

        // Admin endpoint - Get all blogs (with authentication)
        [IdentityBasicAuthenticationAttribute]
        [HttpPost]
        public async Task<IHttpActionResult> AllBlogs(ClsBlogVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.CompanyId == obj.CompanyId)
                    .Select(a => a.DatatablePageEntries)
                    .FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = (from a in oConnectionContext.DbClsBlog
                       join b in oConnectionContext.DbClsBlogCategory on a.BlogCategoryId equals b.BlogCategoryId into categoryJoin
                       from category in categoryJoin.DefaultIfEmpty()
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                       select new
                       {
                           a.BlogId,
                           a.Title,
                           a.ShortDescription,
                           CategoryName = category != null ? category.CategoryName : null,
                           a.UniqueSlug,
                           a.IsActive,
                           a.ViewCount,
                           a.AddedOn,
                           a.PublishedDate,
                           a.ModifiedOn,
                           AddedByCode = oConnectionContext.DbClsUser
                               .Where(z => z.UserId == a.AddedBy)
                               .Select(z => z.Username)
                               .FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser
                               .Where(z => z.UserId == a.ModifiedBy)
                               .Select(z => z.Username)
                               .FirstOrDefault(),
                       })
                       .ToList();

            if (!string.IsNullOrWhiteSpace(obj.Search))
            {
                string searchLower = obj.Search.ToLower();
                det = det.Where(a =>
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.ShortDescription != null && a.ShortDescription.ToLower().Contains(searchLower)) ||
                    (a.CategoryName != null && a.CategoryName.ToLower().Contains(searchLower))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(obj.CategoryFilter))
            {
                det = det.Where(a => a.CategoryName != null &&
                                   a.CategoryName.ToLower() == obj.CategoryFilter.ToLower()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Blogs = det.OrderByDescending(a => a.BlogId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        // Admin endpoint - Get single blog
        [IdentityBasicAuthenticationAttribute]
        [HttpPost]
        public async Task<IHttpActionResult> Blog(ClsBlogVm obj)
        {
            var det = (from a in oConnectionContext.DbClsBlog
                       join b in oConnectionContext.DbClsBlogCategory on a.BlogCategoryId equals b.BlogCategoryId into categoryJoin
                       from category in categoryJoin.DefaultIfEmpty()
                       where a.BlogId == obj.BlogId && a.CompanyId == obj.CompanyId
                       select new
                       {
                           a.BlogId,
                           a.Title,
                           a.ShortDescription,
                           a.Description,
                           a.Image,
                           a.BlogCategoryId,
                           CategoryName = category != null ? category.CategoryName : null,
                           a.Taglist,
                           a.MetaTitle,
                           a.MetaDescription,
                           a.MetaKeywords,
                           a.UniqueSlug,
                           a.PublishedDate,
                           a.ViewCount,
                           a.IsActive,
                           a.IsDeleted,
                           a.AddedBy,
                           a.AddedOn,
                           a.ModifiedBy,
                           a.ModifiedOn,
                           a.CompanyId
                       })
                      .FirstOrDefault();

            data = new
            {
                Status = det != null ? 1 : 0,
                Message = det != null ? "found" : "not found",
                Data = new
                {
                    Blog = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        // Admin endpoint - Insert blog
        [IdentityBasicAuthenticationAttribute]
        [HttpPost]
        public async Task<IHttpActionResult> InsertBlog(ClsBlogVm obj)
        {
            using (System.Transactions.TransactionScope dbContextTransaction = new System.Transactions.TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (string.IsNullOrWhiteSpace(obj.Title))
                {
                    errors.Add(new ClsError { Message = "Title is required", Id = "divTitle" });
                    isError = true;
                }

                if (string.IsNullOrWhiteSpace(obj.Description))
                {
                    errors.Add(new ClsError { Message = "Description is required", Id = "divDescription" });
                    isError = true;
                }

                // Generate slug from title if not provided
                if (string.IsNullOrWhiteSpace(obj.UniqueSlug))
                {
                    obj.UniqueSlug = GenerateSlug(obj.Title);
                }
                else
                {
                    obj.UniqueSlug = GenerateSlug(obj.UniqueSlug);
                }

                // Check slug uniqueness
                if (oConnectionContext.DbClsBlog
                    .Any(a => a.UniqueSlug == obj.UniqueSlug &&
                             a.CompanyId == obj.CompanyId &&
                             a.IsDeleted == false))
                {
                    // Append number if slug exists
                    int counter = 1;
                    string baseSlug = obj.UniqueSlug;
                    while (oConnectionContext.DbClsBlog
                        .Any(a => a.UniqueSlug == obj.UniqueSlug &&
                                 a.CompanyId == obj.CompanyId &&
                                 a.IsDeleted == false))
                    {
                        obj.UniqueSlug = baseSlug + "-" + counter;
                        counter++;
                    }
                }

                if (isError)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Validation failed",
                        Data = new { Errors = errors }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Category is now managed via BlogCategoryId only

                string imagePath = obj.Image ?? "";

                // Handle image upload if base64 image is provided
                if (obj.Image != "" && obj.Image != null && !obj.Image.Contains("http"))
                {
                    string filepathPass = "";
                    filepathPass = "/ExternalContents/Images/BlogImage/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage;

                    string base64 = obj.Image.Replace(obj.Image.Substring(0, obj.Image.IndexOf(',') + 1), "");
                    byte[] imageBytes = Convert.FromBase64String(base64);
                    System.IO.Stream strm = new System.IO.MemoryStream(imageBytes);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BlogImage");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    oCommonController.GenerateThumbnails(imageBytes.Length, strm, targetFile);

                    imagePath = filepathPass;
                }

                ClsBlog oClsBlog = new ClsBlog
                {
                    Title = obj.Title,
                    ShortDescription = obj.ShortDescription ?? "",
                    Description = obj.Description,
                    Image = imagePath,
                    BlogCategoryId = obj.BlogCategoryId > 0 ? obj.BlogCategoryId : (long?)null,
                    Taglist = obj.Taglist ?? "",
                    MetaTitle = obj.MetaTitle ?? obj.Title,
                    MetaDescription = obj.MetaDescription ?? obj.ShortDescription ?? "",
                    MetaKeywords = obj.MetaKeywords ?? "",
                    UniqueSlug = obj.UniqueSlug,
                    PublishedDate = obj.IsActive ? (obj.PublishedDate ?? CurrentDate) : (DateTime?)null,
                    ViewCount = 0,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };

                oConnectionContext.DbClsBlog.Add(oClsBlog);
                oConnectionContext.SaveChanges();
                dbContextTransaction.Complete();

                data = new
                {
                    Status = 1,
                    Message = "Blog created successfully",
                    Data = new
                    {
                        BlogId = oClsBlog.BlogId
                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        // Admin endpoint - Update blog
        [IdentityBasicAuthenticationAttribute]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateBlog(ClsBlogVm obj)
        {
            using (System.Transactions.TransactionScope dbContextTransaction = new System.Transactions.TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var oClsBlog = oConnectionContext.DbClsBlog
                    .Where(a => a.BlogId == obj.BlogId && a.CompanyId == obj.CompanyId)
                    .FirstOrDefault();

                if (oClsBlog == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Blog not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (string.IsNullOrWhiteSpace(obj.Title))
                {
                    errors.Add(new ClsError { Message = "Title is required", Id = "divTitle" });
                    isError = true;
                }

                if (string.IsNullOrWhiteSpace(obj.Description))
                {
                    errors.Add(new ClsError { Message = "Description is required", Id = "divDescription" });
                    isError = true;
                }

                // Generate slug from title if not provided
                if (string.IsNullOrWhiteSpace(obj.UniqueSlug))
                {
                    obj.UniqueSlug = GenerateSlug(obj.Title);
                }
                else
                {
                    obj.UniqueSlug = GenerateSlug(obj.UniqueSlug);
                }

                // Check slug uniqueness (excluding current blog)
                if (oConnectionContext.DbClsBlog
                    .Any(a => a.UniqueSlug == obj.UniqueSlug &&
                             a.BlogId != obj.BlogId &&
                             a.CompanyId == obj.CompanyId &&
                             a.IsDeleted == false))
                {
                    // Append number if slug exists
                    int counter = 1;
                    string baseSlug = obj.UniqueSlug;
                    while (oConnectionContext.DbClsBlog
                        .Any(a => a.UniqueSlug == obj.UniqueSlug &&
                                 a.BlogId != obj.BlogId &&
                                 a.CompanyId == obj.CompanyId &&
                                 a.IsDeleted == false))
                    {
                        obj.UniqueSlug = baseSlug + "-" + counter;
                        counter++;
                    }
                }

                if (isError)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Validation failed",
                        Data = new { Errors = errors }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Category is now managed via BlogCategoryId only

                // Handle image upload if base64 image is provided
                string pic1 = oConnectionContext.DbClsBlog.Where(a => a.BlogId == obj.BlogId).Select(a => a.Image).FirstOrDefault();
                if (obj.Image != "" && obj.Image != null && !obj.Image.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/BlogImage/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage;

                    string base64 = obj.Image.Replace(obj.Image.Substring(0, obj.Image.IndexOf(',') + 1), "");
                    byte[] imageBytes = Convert.FromBase64String(base64);
                    System.IO.Stream strm = new System.IO.MemoryStream(imageBytes);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BlogImage");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    oCommonController.GenerateThumbnails(imageBytes.Length, strm, targetFile);

                    oClsBlog.Image = filepathPass;
                }
                else
                {
                    oClsBlog.Image = pic1;
                }

                oClsBlog.Title = obj.Title;
                oClsBlog.ShortDescription = obj.ShortDescription ?? "";
                oClsBlog.Description = obj.Description;
                oClsBlog.BlogCategoryId = obj.BlogCategoryId > 0 ? obj.BlogCategoryId : (long?)null;
                oClsBlog.Taglist = obj.Taglist ?? "";
                oClsBlog.MetaTitle = obj.MetaTitle ?? obj.Title;
                oClsBlog.MetaDescription = obj.MetaDescription ?? obj.ShortDescription ?? "";
                oClsBlog.MetaKeywords = obj.MetaKeywords ?? "";
                oClsBlog.UniqueSlug = obj.UniqueSlug;

                // Set published date if activating for the first time
                if (obj.IsActive && !oClsBlog.PublishedDate.HasValue)
                {
                    oClsBlog.PublishedDate = obj.PublishedDate ?? CurrentDate;
                }
                else if (obj.IsActive && obj.PublishedDate.HasValue)
                {
                    oClsBlog.PublishedDate = obj.PublishedDate;
                }
                else if (!obj.IsActive)
                {
                    oClsBlog.PublishedDate = null;
                }

                oClsBlog.IsActive = obj.IsActive;
                oClsBlog.ModifiedBy = obj.ModifiedBy;
                oClsBlog.ModifiedOn = CurrentDate;

                oConnectionContext.SaveChanges();
                dbContextTransaction.Complete();

                data = new
                {
                    Status = 1,
                    Message = "Blog updated successfully",
                    Data = new
                    {
                        BlogId = oClsBlog.BlogId
                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        // Admin endpoint - Active/Inactive blog
        [IdentityBasicAuthenticationAttribute]
        [HttpPost]
        public async Task<IHttpActionResult> BlogActiveInactive(ClsBlogVm obj)
        {
            using (System.Transactions.TransactionScope dbContextTransaction = new System.Transactions.TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                var oClsBlog = oConnectionContext.DbClsBlog
                    .Where(a => a.BlogId == obj.BlogId && a.CompanyId == obj.CompanyId)
                    .FirstOrDefault();

                if (oClsBlog == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Blog not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                oClsBlog.IsActive = obj.IsActive;
                oClsBlog.ModifiedBy = obj.AddedBy;
                oClsBlog.ModifiedOn = CurrentDate;

                // Set published date if activating for the first time
                if (obj.IsActive && !oClsBlog.PublishedDate.HasValue)
                {
                    oClsBlog.PublishedDate = CurrentDate;
                }
                else if (!obj.IsActive)
                {
                    oClsBlog.PublishedDate = null;
                }

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Blogs",
                    CompanyId = obj.CompanyId,
                    Description = "Blog \"" + oClsBlog.Title + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsBlog.BlogId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Blog " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        // Admin endpoint - Delete blog
        [IdentityBasicAuthenticationAttribute]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteBlog(ClsBlogVm obj)
        {
            using (System.Transactions.TransactionScope dbContextTransaction = new System.Transactions.TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                var oClsBlog = oConnectionContext.DbClsBlog
                    .Where(a => a.BlogId == obj.BlogId && a.CompanyId == obj.CompanyId)
                    .FirstOrDefault();

                if (oClsBlog == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Blog not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                oClsBlog.IsDeleted = true;
                oClsBlog.ModifiedBy = obj.ModifiedBy;
                oClsBlog.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.ModifiedBy,
                    Browser = obj.Browser,
                    Category = "Blogs",
                    CompanyId = obj.CompanyId,
                    Description = "Blog \"" + oClsBlog.Title + "\" deleted",
                    Id = oClsBlog.BlogId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Blog deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        // Public endpoint - Get active blog categories
        [HttpPost]
        public async Task<IHttpActionResult> WebBlogCategories(ClsBlogVm obj)
        {
            try
            {
                var categories = (from a in oConnectionContext.DbClsBlogCategory
                                 where a.IsDeleted == false &&
                                       a.IsActive == true &&
                                       (obj.CompanyId == 0 || a.CompanyId == obj.CompanyId)
                                 select new
                                 {
                                     BlogCategoryId = a.BlogCategoryId,
                                     CategoryName = a.CategoryName
                                 })
                                 .OrderBy(a => a.CategoryName)
                                 .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        BlogCategories = categories
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error fetching blog categories: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        // Helper method to generate URL-friendly slug
        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            // Convert to lowercase
            string slug = text.ToLower().Trim();

            // Replace spaces with hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

            // Remove special characters except hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Replace multiple hyphens with single hyphen
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Remove leading/trailing hyphens
            slug = slug.Trim('-');

            return slug;
        }

        // Helper method to get client IP address
        private string GetClientIpAddress()
        {
            try
            {
                if (Request.Properties.ContainsKey("MS_HttpContext"))
                {
                    var httpContext = Request.Properties["MS_HttpContext"] as System.Web.HttpContextWrapper;
                    if (httpContext != null && httpContext.Request != null)
                    {
                        // Check for forwarded IP (if behind proxy/load balancer)
                        string forwardedFor = httpContext.Request.Headers["X-Forwarded-For"];
                        if (!string.IsNullOrWhiteSpace(forwardedFor))
                        {
                            // X-Forwarded-For can contain multiple IPs, take the first one
                            string[] ips = forwardedFor.Split(',');
                            if (ips.Length > 0)
                            {
                                return ips[0].Trim();
                            }
                        }

                        // Check for X-Real-IP header
                        string realIp = httpContext.Request.Headers["X-Real-IP"];
                        if (!string.IsNullOrWhiteSpace(realIp))
                        {
                            return realIp.Trim();
                        }

                        return httpContext.Request.UserHostAddress;
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

    }
}

