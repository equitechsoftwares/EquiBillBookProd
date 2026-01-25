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
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class PageSeoSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllPageSeoSettings(ClsPageSeoSettingsVm obj)
        {
            var det = oConnectionContext.DbClsPageSeoSettings
                .Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                .Select(a => new
                {
                    a.PageSeoSettingsId,
                    a.PageIdentifier,
                    a.PageTitle,
                    a.IsActive,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    a.AddedOn,
                    a.ModifiedOn
                })
                .OrderBy(a => a.PageIdentifier)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PageSeoSettingsList = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PageSeoSettings(ClsPageSeoSettingsVm obj)
        {
            var det = oConnectionContext.DbClsPageSeoSettings
                .Where(a => a.PageSeoSettingsId == obj.PageSeoSettingsId && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                .Select(a => new
                {
                    a.PageSeoSettingsId,
                    a.PageIdentifier,
                    a.PageTitle,
                    a.MetaDescription,
                    a.MetaKeywords,
                    a.OgTitle,
                    a.OgDescription,
                    a.OgImage,
                    a.OgUrl,
                    a.TwitterTitle,
                    a.TwitterDescription,
                    a.TwitterImage,
                    a.CanonicalUrl,
                    a.IsActive
                })
                .FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PageSeoSettings = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPageSeoSettings(ClsPageSeoSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (string.IsNullOrWhiteSpace(obj.PageIdentifier))
                {
                    errors.Add(new ClsError { Message = "Page Identifier is required", Id = "divPageIdentifier" });
                    isError = true;
                }
                else
                {
                    // Check for duplicate PageIdentifier for the same company
                    if (oConnectionContext.DbClsPageSeoSettings
                        .Any(s => s.PageIdentifier.ToLower() == obj.PageIdentifier.ToLower() 
                            && s.CompanyId == obj.CompanyId 
                            && !s.IsDeleted))
                    {
                        errors.Add(new ClsError { Message = "Page Identifier already exists for this company", Id = "divPageIdentifier" });
                        isError = true;
                    }
                }

                // Validate field lengths
                if (!string.IsNullOrWhiteSpace(obj.PageTitle) && obj.PageTitle.Length > 200)
                {
                    errors.Add(new ClsError { Message = "Page Title cannot exceed 200 characters", Id = "divPageTitle" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.MetaDescription) && obj.MetaDescription.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Meta Description cannot exceed 500 characters", Id = "divMetaDescription" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.MetaKeywords) && obj.MetaKeywords.Length > 1000)
                {
                    errors.Add(new ClsError { Message = "Meta Keywords cannot exceed 1000 characters", Id = "divMetaKeywords" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgTitle) && obj.OgTitle.Length > 200)
                {
                    errors.Add(new ClsError { Message = "OG Title cannot exceed 200 characters", Id = "divOgTitle" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgDescription) && obj.OgDescription.Length > 500)
                {
                    errors.Add(new ClsError { Message = "OG Description cannot exceed 500 characters", Id = "divOgDescription" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgImage) && obj.OgImage.Length > 500)
                {
                    errors.Add(new ClsError { Message = "OG Image URL cannot exceed 500 characters", Id = "divOgImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgUrl) && obj.OgUrl.Length > 500)
                {
                    errors.Add(new ClsError { Message = "OG URL cannot exceed 500 characters", Id = "divOgUrl" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterTitle) && obj.TwitterTitle.Length > 200)
                {
                    errors.Add(new ClsError { Message = "Twitter Title cannot exceed 200 characters", Id = "divTwitterTitle" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterDescription) && obj.TwitterDescription.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Twitter Description cannot exceed 500 characters", Id = "divTwitterDescription" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterImage) && obj.TwitterImage.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Twitter Image URL cannot exceed 500 characters", Id = "divTwitterImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.CanonicalUrl) && obj.CanonicalUrl.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Canonical URL cannot exceed 500 characters", Id = "divCanonicalUrl" });
                    isError = true;
                }

                // Validate URL formats
                if (!string.IsNullOrWhiteSpace(obj.OgImage) && !Uri.IsWellFormedUriString(obj.OgImage, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "OG Image URL must be a valid URL", Id = "divOgImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgUrl) && !Uri.IsWellFormedUriString(obj.OgUrl, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "OG URL must be a valid URL", Id = "divOgUrl" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterImage) && !Uri.IsWellFormedUriString(obj.TwitterImage, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "Twitter Image URL must be a valid URL", Id = "divTwitterImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.CanonicalUrl) && !Uri.IsWellFormedUriString(obj.CanonicalUrl, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "Canonical URL must be a valid URL", Id = "divCanonicalUrl" });
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

                ClsPageSeoSettings oClsPageSeoSettings = new ClsPageSeoSettings();
                oClsPageSeoSettings.CompanyId = obj.CompanyId;
                oClsPageSeoSettings.PageIdentifier = obj.PageIdentifier;
                oClsPageSeoSettings.PageTitle = obj.PageTitle;
                oClsPageSeoSettings.MetaDescription = obj.MetaDescription;
                oClsPageSeoSettings.MetaKeywords = obj.MetaKeywords;
                oClsPageSeoSettings.OgTitle = obj.OgTitle;
                oClsPageSeoSettings.OgDescription = obj.OgDescription;
                oClsPageSeoSettings.OgImage = obj.OgImage;
                oClsPageSeoSettings.OgUrl = obj.OgUrl;
                oClsPageSeoSettings.TwitterTitle = obj.TwitterTitle;
                oClsPageSeoSettings.TwitterDescription = obj.TwitterDescription;
                oClsPageSeoSettings.TwitterImage = obj.TwitterImage;
                oClsPageSeoSettings.CanonicalUrl = obj.CanonicalUrl;
                oClsPageSeoSettings.IsActive = obj.IsActive;
                oClsPageSeoSettings.AddedOn = CurrentDate;
                oClsPageSeoSettings.AddedBy = obj.AddedBy;
                oClsPageSeoSettings.IsDeleted = false;
                oConnectionContext.DbClsPageSeoSettings.Add(oClsPageSeoSettings);
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    CompanyId = obj.CompanyId,
                //    AddedBy = obj.AddedBy,
                //    AddedOn = CurrentDate,
                //    ActivityType = "Insert",
                //    Module = "PageSeoSettings",
                //    Description = "Page SEO Settings inserted: " + obj.PageIdentifier,
                //    IpAddress = obj.IpAddress,
                //    Browser = obj.Browser,
                //    Platform = obj.Platform
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm);

                dbContextTransaction.Complete();

                data = new
                {
                    Status = 1,
                    Message = "Page SEO Settings inserted successfully.",
                    Data = new
                    {
                        PageSeoSettingsId = oClsPageSeoSettings.PageSeoSettingsId
                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        public async Task<IHttpActionResult> UpdatePageSeoSettings(ClsPageSeoSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (string.IsNullOrWhiteSpace(obj.PageIdentifier))
                {
                    errors.Add(new ClsError { Message = "Page Identifier is required", Id = "divPageIdentifier" });
                    isError = true;
                }
                else
                {
                    // Check for duplicate PageIdentifier for the same company (excluding current record)
                    if (oConnectionContext.DbClsPageSeoSettings
                        .Any(s => s.PageIdentifier.ToLower() == obj.PageIdentifier.ToLower() 
                            && s.CompanyId == obj.CompanyId 
                            && s.PageSeoSettingsId != obj.PageSeoSettingsId
                            && !s.IsDeleted))
                    {
                        errors.Add(new ClsError { Message = "Page Identifier already exists for this company", Id = "divPageIdentifier" });
                        isError = true;
                    }
                }

                // Validate field lengths (same as Insert)
                if (!string.IsNullOrWhiteSpace(obj.PageTitle) && obj.PageTitle.Length > 200)
                {
                    errors.Add(new ClsError { Message = "Page Title cannot exceed 200 characters", Id = "divPageTitle" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.MetaDescription) && obj.MetaDescription.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Meta Description cannot exceed 500 characters", Id = "divMetaDescription" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.MetaKeywords) && obj.MetaKeywords.Length > 1000)
                {
                    errors.Add(new ClsError { Message = "Meta Keywords cannot exceed 1000 characters", Id = "divMetaKeywords" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgTitle) && obj.OgTitle.Length > 200)
                {
                    errors.Add(new ClsError { Message = "OG Title cannot exceed 200 characters", Id = "divOgTitle" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgDescription) && obj.OgDescription.Length > 500)
                {
                    errors.Add(new ClsError { Message = "OG Description cannot exceed 500 characters", Id = "divOgDescription" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgImage) && obj.OgImage.Length > 500)
                {
                    errors.Add(new ClsError { Message = "OG Image URL cannot exceed 500 characters", Id = "divOgImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgUrl) && obj.OgUrl.Length > 500)
                {
                    errors.Add(new ClsError { Message = "OG URL cannot exceed 500 characters", Id = "divOgUrl" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterTitle) && obj.TwitterTitle.Length > 200)
                {
                    errors.Add(new ClsError { Message = "Twitter Title cannot exceed 200 characters", Id = "divTwitterTitle" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterDescription) && obj.TwitterDescription.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Twitter Description cannot exceed 500 characters", Id = "divTwitterDescription" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterImage) && obj.TwitterImage.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Twitter Image URL cannot exceed 500 characters", Id = "divTwitterImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.CanonicalUrl) && obj.CanonicalUrl.Length > 500)
                {
                    errors.Add(new ClsError { Message = "Canonical URL cannot exceed 500 characters", Id = "divCanonicalUrl" });
                    isError = true;
                }

                // Validate URL formats
                if (!string.IsNullOrWhiteSpace(obj.OgImage) && !Uri.IsWellFormedUriString(obj.OgImage, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "OG Image URL must be a valid URL", Id = "divOgImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.OgUrl) && !Uri.IsWellFormedUriString(obj.OgUrl, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "OG URL must be a valid URL", Id = "divOgUrl" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.TwitterImage) && !Uri.IsWellFormedUriString(obj.TwitterImage, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "Twitter Image URL must be a valid URL", Id = "divTwitterImage" });
                    isError = true;
                }

                if (!string.IsNullOrWhiteSpace(obj.CanonicalUrl) && !Uri.IsWellFormedUriString(obj.CanonicalUrl, UriKind.Absolute))
                {
                    errors.Add(new ClsError { Message = "Canonical URL must be a valid URL", Id = "divCanonicalUrl" });
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

                var oClsPageSeoSettings = oConnectionContext.DbClsPageSeoSettings
                    .FirstOrDefault(a => a.PageSeoSettingsId == obj.PageSeoSettingsId && a.CompanyId == obj.CompanyId && a.IsDeleted == false);

                if (oClsPageSeoSettings == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Page SEO Settings not found.",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                oClsPageSeoSettings.PageIdentifier = obj.PageIdentifier;
                oClsPageSeoSettings.PageTitle = obj.PageTitle;
                oClsPageSeoSettings.MetaDescription = obj.MetaDescription;
                oClsPageSeoSettings.MetaKeywords = obj.MetaKeywords;
                oClsPageSeoSettings.OgTitle = obj.OgTitle;
                oClsPageSeoSettings.OgDescription = obj.OgDescription;
                oClsPageSeoSettings.OgImage = obj.OgImage;
                oClsPageSeoSettings.OgUrl = obj.OgUrl;
                oClsPageSeoSettings.TwitterTitle = obj.TwitterTitle;
                oClsPageSeoSettings.TwitterDescription = obj.TwitterDescription;
                oClsPageSeoSettings.TwitterImage = obj.TwitterImage;
                oClsPageSeoSettings.CanonicalUrl = obj.CanonicalUrl;
                oClsPageSeoSettings.IsActive = obj.IsActive;
                oClsPageSeoSettings.ModifiedOn = CurrentDate;
                oClsPageSeoSettings.ModifiedBy = obj.AddedBy;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    CompanyId = obj.CompanyId,
                //    AddedBy = obj.AddedBy,
                //    AddedOn = CurrentDate,
                //    ActivityType = "Update",
                //    Module = "PageSeoSettings",
                //    Description = "Page SEO Settings updated: " + obj.PageIdentifier,
                //    IpAddress = obj.IpAddress,
                //    Browser = obj.Browser,
                //    Platform = obj.Platform
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm);

                dbContextTransaction.Complete();

                data = new
                {
                    Status = 1,
                    Message = "Page SEO Settings updated successfully.",
                    Data = new
                    {
                        PageSeoSettingsId = oClsPageSeoSettings.PageSeoSettingsId
                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        public async Task<IHttpActionResult> PageSeoSettingsDelete(ClsPageSeoSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var oClsPageSeoSettings = oConnectionContext.DbClsPageSeoSettings
                    .FirstOrDefault(a => a.PageSeoSettingsId == obj.PageSeoSettingsId && a.CompanyId == obj.CompanyId && a.IsDeleted == false);

                if (oClsPageSeoSettings == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Page SEO Settings not found.",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                oClsPageSeoSettings.IsDeleted = true;
                oClsPageSeoSettings.ModifiedOn = CurrentDate;
                oClsPageSeoSettings.ModifiedBy = obj.AddedBy;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    CompanyId = obj.CompanyId,
                //    AddedBy = obj.AddedBy,
                //    AddedOn = CurrentDate,
                //    ActivityType = "Delete",
                //    Module = "PageSeoSettings",
                //    Description = "Page SEO Settings deleted: " + oClsPageSeoSettings.PageIdentifier,
                //    IpAddress = obj.IpAddress,
                //    Browser = obj.Browser,
                //    Platform = obj.Platform
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm);

                dbContextTransaction.Complete();

                data = new
                {
                    Status = 1,
                    Message = "Page SEO Settings deleted successfully.",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }
    }
}

