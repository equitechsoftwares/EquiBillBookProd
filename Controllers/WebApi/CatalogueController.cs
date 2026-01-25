using EquiBillBook.Filters;
using EquiBillBook.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class CatalogueController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllCatalogues(ClsCatalogueVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsCatalogue.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.CatalogueId,
                a.CatalogueName,
                a.Tagline,
                a.QRCodeImage,
                a.UniqueSlug,
                a.CatalogueType,
                CatalogueTypeText = a.CatalogueType == 1 ? "All Items" : a.CatalogueType == 2 ? "Selected Items" : a.CatalogueType == 3 ? "By Category" : "By Brand",
                a.BranchId,
                BranchName = oConnectionContext.DbClsBranch
                    .Where(c => c.BranchId == a.BranchId)
                    .Select(c => c.Branch)
                    .FirstOrDefault(),
                a.ValidFrom,
                a.ValidTo,
                a.NeverExpires,
                a.TopBarColor,
                a.TopBarTextColor,
                a.BodyBackgroundColor,
                a.BodyTextColor,
                a.HeaderTextColor,
                a.LinkIconAccentColor,
                a.AddToCartButtonColor,
                a.WhatsappSupportButtonColor,
                a.FilterBadgeColor,
                a.IsHeaderFixed,
                a.IsActive,
                a.ShowInInvoices,
                a.IsDeleted,
                a.AddedBy,
                a.AddedOn,
                a.ModifiedBy,
                a.ModifiedOn,
                a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.CatalogueName.ToLower().Contains(obj.Search.ToLower()) ||
                                     a.UniqueSlug.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Catalogues = det.OrderByDescending(a => a.CatalogueId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Catalogue(ClsCatalogue obj)
        {
            var det = oConnectionContext.DbClsCatalogue.Where(a => a.CatalogueId == obj.CatalogueId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CatalogueId,
                a.CatalogueName,
                a.Tagline,
                a.UniqueSlug,
                a.CatalogueType,
                ItemIds = oConnectionContext.DbClsCatalogueItem.Where(b => b.CatalogueId == a.CatalogueId && b.IsDeleted == false)
                                                                   .OrderBy(b => b.DisplayOrder)
                                                                   .Select(b => b.ItemId)
                                                                   .ToList(),
                ItemDetailIds = oConnectionContext.DbClsCatalogueItem.Where(b => b.CatalogueId == a.CatalogueId && b.IsDeleted == false)
                                                                       .OrderBy(b => b.DisplayOrder)
                                                                       .Select(b => b.ItemDetailsId)
                                                                       .ToList(),
                CategoryIds = oConnectionContext.DbClsCatalogueCategory.Where(b => b.CatalogueId == a.CatalogueId && b.IsDeleted == false)
                                                                        .Select(b => b.CategoryId)
                                                                        .ToList(),
                BrandIds = oConnectionContext.DbClsCatalogueBrand.Where(b => b.CatalogueId == a.CatalogueId && b.IsDeleted == false)
                                                                  .Select(b => b.BrandId)
                                                                  .ToList(),
                a.ValidFrom,
                a.ValidTo,
                a.NeverExpires,
                a.ShowPrices,
                a.ShowMRP,
                a.ShowDiscount,
                a.ShowStock,
                a.ShowOutOfStock,
                a.ShowBusinessLogo,
                a.ShowBusinessName,
                a.ShowAddress,
                a.ShowContact,
                a.ShowWhatsApp,
                a.ShowEmail,
                a.ShowGstin,
                a.EnableWhatsAppEnquiry,
                a.EnableWhatsAppEnquiryForItems,
                a.EnableAddToCart,
                a.AddToCartQuotationMode,
                a.ShowProductCode,
                a.ShowImages,
                a.ItemsPerPage,
                a.DefaultSortOrder,
                a.SellingPriceGroupId,
                a.Theme,
                a.HeaderColor,
                a.HeaderTextColor,
                a.LinkIconAccentColor,
                a.AddToCartButtonColor,
                a.WhatsappSupportButtonColor,
                a.FilterBadgeColor,
                a.TopBarColor,
                a.TopBarTextColor,
                a.BodyBackgroundColor,
                a.BodyTextColor,
                a.IsHeaderFixed,
                a.IsPasswordProtected,
                a.Password,
                a.MetaTitle,
                a.MetaDescription,
                a.GoogleAnalyticsId,
                a.GoogleReviewUrl,
                a.QRCodeImage,
                a.IsActive,
                a.ShowInInvoices,
                a.IsDeleted,
                a.AddedBy,
                a.AddedOn,
                a.ModifiedBy,
                a.ModifiedOn,
                a.CompanyId,
                a.BranchId,
                a.FacebookUrl,
                a.InstagramUrl,
                a.TwitterUrl,
                a.LinkedInUrl,
                a.YouTubeUrl,
                a.TikTokUrl,
                a.PinterestUrl,
                Branch = oConnectionContext.DbClsBranch
                    .Where(c => c.BranchId == a.BranchId)
                    .Select(c => new
                    {
                        c.BranchId,
                        TaxNo = oConnectionContext.DbClsTaxSetting
                            .Where(t => t.IsDeleted == false && t.TaxSettingId == c.TaxSettingId)
                            .Select(t => t.BusinessRegistrationNo)
                            .FirstOrDefault(),
                        Tax = oConnectionContext.DbClsBusinessRegistrationName
                            .Where(d => d.BusinessRegistrationNameId ==
                                oConnectionContext.DbClsTaxSetting
                                    .Where(t => t.IsDeleted == false && t.TaxSettingId == c.TaxSettingId)
                                    .Select(t => t.BusinessRegistrationNameId)
                                    .FirstOrDefault())
                            .Select(d => d.Name)
                            .FirstOrDefault(),
                        c.WhatsappNo,
                        c.Branch,
                        c.Address,
                        City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == c.CityId).Select(cc => cc.City).FirstOrDefault(),
                        State = oConnectionContext.DbClsState.Where(cc => cc.StateId == c.StateId).Select(cc => cc.State).FirstOrDefault(),
                        Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == c.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                        c.Zipcode,
                        c.Mobile,
                        c.Email,
                    })
                    .FirstOrDefault(),
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Catalogue = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCatalogue(ClsCatalogueVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                int TotalCatalogueUsed = oConnectionContext.DbClsCatalogue.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count();
                int TotalCatalogue = oCommonController.fetchPlanQuantity(obj.CompanyId, "Catalogue");
                if (TotalCatalogueUsed >= TotalCatalogue)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Catalogue quota already used. Please upgrade addons from My Plan Menu",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var branchId = obj.BranchId > 0 ? obj.BranchId : 0;
                obj.BranchId = branchId;

                if (obj.CatalogueName == null || obj.CatalogueName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCatalogueName" });
                    isError = true;
                }

                if (obj.UniqueSlug == null || obj.UniqueSlug == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUniqueSlug" });
                    isError = true;
                }
                else
                {
                    // Validate UniqueSlug uniqueness (globally unique)
                    string normalizedSlug = obj.UniqueSlug.Trim().ToLower();
                    if (oConnectionContext.DbClsCatalogue.Where(a => a.UniqueSlug != null && a.UniqueSlug.ToLower() == normalizedSlug && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "This catalogue slug is already in use. Please choose a different slug.", Id = "divUniqueSlug" });
                        isError = true;
                    }
                }

                if (obj.CatalogueType == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCatalogueType" });
                    isError = true;
                }
                else if (obj.CatalogueType == 2)
                {
                    if (obj.ItemIds == null || obj.ItemIds.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Select at least one item", Id = "divCatalogueItems" });
                        isError = true;
                    }
                    if (obj.ItemDetailIds == null || obj.ItemDetailIds.Count != obj.ItemIds.Count)
                    {
                        errors.Add(new ClsError { Message = "Unable to process selected items. Please try again.", Id = "divCatalogueItems" });
                        isError = true;
                    }
                }
                else if (obj.CatalogueType == 3)
                {
                    if (obj.CategoryIds == null || obj.CategoryIds.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Select at least one category", Id = "divCatalogueCategories" });
                        isError = true;
                    }
                }
                else if (obj.CatalogueType == 4)
                {
                    if (obj.BrandIds == null || obj.BrandIds.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Select at least one brand", Id = "divCatalogueBrands" });
                        isError = true;
                    }
                }

                if (branchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.EnableAddToCart && obj.AddToCartQuotationMode <= 0)
                {
                    errors.Add(new ClsError { Message = "Select how quotations should be handled", Id = "divAddToCartMode" });
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

                var linkIconAccentColor = string.IsNullOrWhiteSpace(obj.LinkIconAccentColor) ? obj.AddToCartButtonColor : obj.LinkIconAccentColor;
                var headerTextColor = string.IsNullOrWhiteSpace(obj.HeaderTextColor) ? null : obj.HeaderTextColor;
                var addToCartButtonColor = string.IsNullOrWhiteSpace(obj.AddToCartButtonColor) ? linkIconAccentColor : obj.AddToCartButtonColor;
                var whatsappSupportButtonColor = string.IsNullOrWhiteSpace(obj.WhatsappSupportButtonColor) ? null : obj.WhatsappSupportButtonColor;
                var filterBadgeColor = string.IsNullOrWhiteSpace(obj.FilterBadgeColor) ? null : obj.FilterBadgeColor;

                ClsCatalogue oCatalogue = new ClsCatalogue()
                {
                    CatalogueName = obj.CatalogueName,
                    Tagline = obj.Tagline,
                    UniqueSlug = obj.UniqueSlug,
                    CatalogueType = obj.CatalogueType,
                    BranchId = branchId,
                    ValidFrom = obj.ValidFrom,
                    ValidTo = obj.ValidTo,
                    NeverExpires = obj.NeverExpires,
                    ShowPrices = obj.ShowPrices,
                    ShowMRP = obj.ShowMRP,
                    ShowDiscount = obj.ShowDiscount,
                    ShowStock = obj.ShowStock,
                    ShowOutOfStock = obj.ShowOutOfStock,
                    ShowBusinessLogo = obj.ShowBusinessLogo,
                    ShowBusinessName = obj.ShowBusinessName,
                    ShowAddress = obj.ShowAddress,
                    ShowContact = obj.ShowContact,
                    ShowWhatsApp = obj.ShowWhatsApp,
                    ShowEmail = obj.ShowEmail,
                    ShowGstin = obj.ShowGstin,
                    EnableWhatsAppEnquiry = obj.EnableWhatsAppEnquiry,
                    EnableWhatsAppEnquiryForItems = obj.EnableWhatsAppEnquiryForItems,
                    EnableAddToCart = obj.EnableAddToCart,
                    AddToCartQuotationMode = obj.EnableAddToCart ? obj.AddToCartQuotationMode : 0,
                    ShowProductCode = obj.ShowProductCode,
                    ShowImages = obj.ShowImages,
                    ItemsPerPage = obj.ItemsPerPage > 0 ? obj.ItemsPerPage : 20,
                    DefaultSortOrder = obj.DefaultSortOrder > 0 ? obj.DefaultSortOrder : 1,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    Theme = obj.Theme,
                    HeaderColor = obj.HeaderColor,
                    HeaderTextColor = headerTextColor,
                    LinkIconAccentColor = linkIconAccentColor,
                    AddToCartButtonColor = addToCartButtonColor,
                    WhatsappSupportButtonColor = whatsappSupportButtonColor,
                    FilterBadgeColor = filterBadgeColor,
                    TopBarColor = obj.TopBarColor,
                    TopBarTextColor = obj.TopBarTextColor,
                    BodyBackgroundColor = obj.BodyBackgroundColor,
                    BodyTextColor = obj.BodyTextColor,
                    IsHeaderFixed = obj.IsHeaderFixed,
                    IsPasswordProtected = obj.IsPasswordProtected,
                    Password = obj.Password,
                    MetaTitle = obj.MetaTitle,
                    MetaDescription = obj.MetaDescription,
                    GoogleAnalyticsId = string.IsNullOrWhiteSpace(obj.GoogleAnalyticsId) ? null : obj.GoogleAnalyticsId.Trim(),
                    GoogleReviewUrl = string.IsNullOrWhiteSpace(obj.GoogleReviewUrl) ? null : obj.GoogleReviewUrl.Trim(),
                    FacebookUrl = string.IsNullOrWhiteSpace(obj.FacebookUrl) ? null : obj.FacebookUrl.Trim(),
                    InstagramUrl = string.IsNullOrWhiteSpace(obj.InstagramUrl) ? null : obj.InstagramUrl.Trim(),
                    TwitterUrl = string.IsNullOrWhiteSpace(obj.TwitterUrl) ? null : obj.TwitterUrl.Trim(),
                    LinkedInUrl = string.IsNullOrWhiteSpace(obj.LinkedInUrl) ? null : obj.LinkedInUrl.Trim(),
                    YouTubeUrl = string.IsNullOrWhiteSpace(obj.YouTubeUrl) ? null : obj.YouTubeUrl.Trim(),
                    TikTokUrl = string.IsNullOrWhiteSpace(obj.TikTokUrl) ? null : obj.TikTokUrl.Trim(),
                    PinterestUrl = string.IsNullOrWhiteSpace(obj.PinterestUrl) ? null : obj.PinterestUrl.Trim(),
                    IsActive = obj.IsActive,
                    ShowInInvoices = obj.ShowInInvoices,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsCatalogue.Add(oCatalogue);
                oConnectionContext.SaveChanges();

                // If this catalogue is marked to show in invoices, unmark all other catalogues for this branch
                if (obj.ShowInInvoices == true && branchId > 0)
                {
                    var otherCatalogues = oConnectionContext.DbClsCatalogue
                        .Where(a => a.BranchId == branchId && 
                                    a.CompanyId == obj.CompanyId && 
                                    a.CatalogueId != oCatalogue.CatalogueId && 
                                    a.IsDeleted == false);
                    
                    foreach (var cat in otherCatalogues)
                    {
                        cat.ShowInInvoices = false;
                        oConnectionContext.Entry(cat).Property(x => x.ShowInInvoices).IsModified = true;
                    }
                    oConnectionContext.SaveChanges();
                }

                var qrPath = GenerateCatalogueQrCode(oCatalogue.UniqueSlug, obj.Domain, null, oCatalogue.CatalogueId);
                if (!string.IsNullOrWhiteSpace(qrPath))
                {
                    oCatalogue.QRCodeImage = qrPath;
                    oConnectionContext.SaveChanges();
                }

                if (obj.CatalogueType == 2 && obj.ItemIds != null && obj.ItemDetailIds != null)
                {
                    var pairedItems = obj.ItemIds
                        .Select((itemId, index) => new
                        {
                            ItemId = itemId,
                            ItemDetailId = obj.ItemDetailIds.Count > index ? obj.ItemDetailIds[index] : 0
                        })
                        .Where(p => p.ItemId > 0 && p.ItemDetailId > 0)
                        .GroupBy(p => new { p.ItemId, p.ItemDetailId })
                        .Select(g => g.Key)
                        .ToList();

                    if (pairedItems.Count > 0)
                    {
                        var itemMaps = pairedItems.Select((pair, idx) => new ClsCatalogueItem
                        {
                            CatalogueId = oCatalogue.CatalogueId,
                            ItemId = pair.ItemId,
                            ItemDetailsId = pair.ItemDetailId,
                            DisplayOrder = idx + 1,
                            IsFeatured = false,
                            CustomPrice = 0,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        }).ToList();

                        oConnectionContext.DbClsCatalogueItem.AddRange(itemMaps);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.CatalogueType == 3 && obj.CategoryIds != null)
                {
                    var categoryMaps = obj.CategoryIds
                        .Where(categoryId => categoryId > 0)
                        .Distinct()
                        .Select(categoryId => new ClsCatalogueCategory
                        {
                            CatalogueId = oCatalogue.CatalogueId,
                            CategoryId = categoryId,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        })
                        .ToList();

                    if (categoryMaps.Count > 0)
                    {
                        oConnectionContext.DbClsCatalogueCategory.AddRange(categoryMaps);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.CatalogueType == 4 && obj.BrandIds != null)
                {
                    var brandMaps = obj.BrandIds
                        .Where(brandId => brandId > 0)
                        .Distinct()
                        .Select(brandId => new ClsCatalogueBrand
                        {
                            CatalogueId = oCatalogue.CatalogueId,
                            BrandId = brandId,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        })
                        .ToList();

                    if (brandMaps.Count > 0)
                    {
                        oConnectionContext.DbClsCatalogueBrand.AddRange(brandMaps);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.EnableAddToCart == true)
                {
                    if (obj.AddToCartQuotationMode == 1 || obj.AddToCartQuotationMode == 3)
                    {
                        var saleSettings = oConnectionContext.DbClsSaleSettings
                            .Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                            .FirstOrDefault();

                        if (saleSettings != null && saleSettings.EnableSalesQuotation == false)
                        {
                            saleSettings.EnableSalesQuotation = true;
                            saleSettings.ModifiedBy = obj.AddedBy;
                            saleSettings.ModifiedOn = CurrentDate;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Catalogues",
                    CompanyId = obj.CompanyId,
                    Description = "Catalogue \"" + obj.CatalogueName + "\" created",
                    Id = oCatalogue.CatalogueId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Catalogue created successfully",
                    Data = new
                    {
                        Catalogue = oCatalogue
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCatalogue(ClsCatalogueVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var branchId = obj.BranchId > 0 ? obj.BranchId : 0;
                obj.BranchId = branchId;

                if (obj.CatalogueName == null || obj.CatalogueName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCatalogueName" });
                    isError = true;
                }

                if (obj.UniqueSlug == null || obj.UniqueSlug == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUniqueSlug" });
                    isError = true;
                }
                else
                {
                    // Validate UniqueSlug uniqueness (globally unique)
                    string normalizedSlug = obj.UniqueSlug.Trim().ToLower();
                    if (oConnectionContext.DbClsCatalogue.Where(a => a.UniqueSlug != null && a.UniqueSlug.ToLower() == normalizedSlug && a.CatalogueId != obj.CatalogueId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "This catalogue slug is already in use. Please choose a different slug.", Id = "divUniqueSlug" });
                        isError = true;
                    }
                }

                if (obj.CatalogueType == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCatalogueType" });
                    isError = true;
                }
                else if (obj.CatalogueType == 2)
                {
                    if (obj.ItemIds == null || obj.ItemIds.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Select at least one item", Id = "divCatalogueItems" });
                        isError = true;
                    }
                    if (obj.ItemDetailIds == null || obj.ItemDetailIds.Count != obj.ItemIds.Count)
                    {
                        errors.Add(new ClsError { Message = "Unable to process selected items. Please try again.", Id = "divCatalogueItems" });
                        isError = true;
                    }
                }
                else if (obj.CatalogueType == 3)
                {
                    if (obj.CategoryIds == null || obj.CategoryIds.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Select at least one category", Id = "divCatalogueCategories" });
                        isError = true;
                    }
                }
                else if (obj.CatalogueType == 4)
                {
                    if (obj.BrandIds == null || obj.BrandIds.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Select at least one brand", Id = "divCatalogueBrands" });
                        isError = true;
                    }
                }

                if (branchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.EnableAddToCart && obj.AddToCartQuotationMode <= 0)
                {
                    errors.Add(new ClsError { Message = "Select how quotations should be handled", Id = "divAddToCartMode" });
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

                var existingQrPath = oConnectionContext.DbClsCatalogue
                    .Where(a => a.CatalogueId == obj.CatalogueId && a.CompanyId == obj.CompanyId)
                    .Select(a => a.QRCodeImage)
                    .FirstOrDefault();

                var linkIconAccentColor = string.IsNullOrWhiteSpace(obj.LinkIconAccentColor) ? obj.AddToCartButtonColor : obj.LinkIconAccentColor;
                var headerTextColor = string.IsNullOrWhiteSpace(obj.HeaderTextColor) ? null : obj.HeaderTextColor;
                var addToCartButtonColor = string.IsNullOrWhiteSpace(obj.AddToCartButtonColor) ? linkIconAccentColor : obj.AddToCartButtonColor;
                var whatsappSupportButtonColor = string.IsNullOrWhiteSpace(obj.WhatsappSupportButtonColor) ? null : obj.WhatsappSupportButtonColor;
                var filterBadgeColor = string.IsNullOrWhiteSpace(obj.FilterBadgeColor) ? null : obj.FilterBadgeColor;

                ClsCatalogue oCatalogue = new ClsCatalogue()
                {
                    CatalogueId = obj.CatalogueId,
                    CatalogueName = obj.CatalogueName,
                    Tagline = obj.Tagline,
                    UniqueSlug = obj.UniqueSlug,
                    CatalogueType = obj.CatalogueType,
                    BranchId = branchId,
                    ValidFrom = obj.ValidFrom,
                    ValidTo = obj.ValidTo,
                    NeverExpires = obj.NeverExpires,
                    ShowPrices = obj.ShowPrices,
                    ShowMRP = obj.ShowMRP,
                    ShowDiscount = obj.ShowDiscount,
                    ShowStock = obj.ShowStock,
                    ShowOutOfStock = obj.ShowOutOfStock,
                    ShowBusinessLogo = obj.ShowBusinessLogo,
                    ShowBusinessName = obj.ShowBusinessName,
                    ShowAddress = obj.ShowAddress,
                    ShowContact = obj.ShowContact,
                    ShowWhatsApp = obj.ShowWhatsApp,
                    ShowEmail = obj.ShowEmail,
                    ShowGstin = obj.ShowGstin,
                    EnableWhatsAppEnquiry = obj.EnableWhatsAppEnquiry,
                    EnableWhatsAppEnquiryForItems = obj.EnableWhatsAppEnquiryForItems,
                    EnableAddToCart = obj.EnableAddToCart,
                    AddToCartQuotationMode = obj.EnableAddToCart ? obj.AddToCartQuotationMode : 0,
                    ShowProductCode = obj.ShowProductCode,
                    ShowImages = obj.ShowImages,
                    ItemsPerPage = obj.ItemsPerPage > 0 ? obj.ItemsPerPage : 20,
                    DefaultSortOrder = obj.DefaultSortOrder > 0 ? obj.DefaultSortOrder : 1,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    Theme = obj.Theme,
                    HeaderColor = obj.HeaderColor,
                    HeaderTextColor = headerTextColor,
                    LinkIconAccentColor = linkIconAccentColor,
                    AddToCartButtonColor = addToCartButtonColor,
                    WhatsappSupportButtonColor = whatsappSupportButtonColor,
                    FilterBadgeColor = filterBadgeColor,
                    TopBarColor = obj.TopBarColor,
                    TopBarTextColor = obj.TopBarTextColor,
                    BodyBackgroundColor = obj.BodyBackgroundColor,
                    BodyTextColor = obj.BodyTextColor,
                    IsHeaderFixed = obj.IsHeaderFixed,
                    IsPasswordProtected = obj.IsPasswordProtected,
                    Password = obj.Password,
                    MetaTitle = obj.MetaTitle,
                    MetaDescription = obj.MetaDescription,
                    GoogleAnalyticsId = string.IsNullOrWhiteSpace(obj.GoogleAnalyticsId) ? null : obj.GoogleAnalyticsId.Trim(),
                    GoogleReviewUrl = string.IsNullOrWhiteSpace(obj.GoogleReviewUrl) ? null : obj.GoogleReviewUrl.Trim(),
                    FacebookUrl = string.IsNullOrWhiteSpace(obj.FacebookUrl) ? null : obj.FacebookUrl.Trim(),
                    InstagramUrl = string.IsNullOrWhiteSpace(obj.InstagramUrl) ? null : obj.InstagramUrl.Trim(),
                    TwitterUrl = string.IsNullOrWhiteSpace(obj.TwitterUrl) ? null : obj.TwitterUrl.Trim(),
                    LinkedInUrl = string.IsNullOrWhiteSpace(obj.LinkedInUrl) ? null : obj.LinkedInUrl.Trim(),
                    YouTubeUrl = string.IsNullOrWhiteSpace(obj.YouTubeUrl) ? null : obj.YouTubeUrl.Trim(),
                    TikTokUrl = string.IsNullOrWhiteSpace(obj.TikTokUrl) ? null : obj.TikTokUrl.Trim(),
                    PinterestUrl = string.IsNullOrWhiteSpace(obj.PinterestUrl) ? null : obj.PinterestUrl.Trim(),
                    IsActive = obj.IsActive,
                    ShowInInvoices = obj.ShowInInvoices,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };

                var qrPath = GenerateCatalogueQrCode(obj.UniqueSlug, obj.Domain, existingQrPath, obj.CatalogueId);
                if (!string.IsNullOrWhiteSpace(qrPath) && !string.Equals(qrPath, existingQrPath, StringComparison.OrdinalIgnoreCase))
                {
                    oCatalogue.QRCodeImage = qrPath;
                }
                else
                {
                    oCatalogue.QRCodeImage = existingQrPath;
                }

                oConnectionContext.DbClsCatalogue.Attach(oCatalogue);
                oConnectionContext.Entry(oCatalogue).Property(x => x.CatalogueId).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.CatalogueName).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.Tagline).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.UniqueSlug).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.CatalogueType).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ValidFrom).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ValidTo).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.NeverExpires).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowPrices).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowMRP).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowDiscount).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowStock).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowOutOfStock).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowBusinessLogo).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowBusinessName).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowAddress).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowContact).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowWhatsApp).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowEmail).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowGstin).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.EnableWhatsAppEnquiry).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.EnableWhatsAppEnquiryForItems).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.EnableAddToCart).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.AddToCartQuotationMode).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowProductCode).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowImages).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ItemsPerPage).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.DefaultSortOrder).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.Theme).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.HeaderColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.HeaderTextColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.LinkIconAccentColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.AddToCartButtonColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.WhatsappSupportButtonColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.FilterBadgeColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.TopBarColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.TopBarTextColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.BodyBackgroundColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.BodyTextColor).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.IsHeaderFixed).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.IsPasswordProtected).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.Password).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.MetaTitle).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.MetaDescription).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.GoogleAnalyticsId).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.GoogleReviewUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.FacebookUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.InstagramUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.TwitterUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.LinkedInUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.YouTubeUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.TikTokUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.PinterestUrl).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ShowInInvoices).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oCatalogue).Property(x => x.QRCodeImage).IsModified = true;
                oConnectionContext.SaveChanges();

                // If this catalogue is marked to show in invoices, unmark all other catalogues for this branch
                if (obj.ShowInInvoices == true && branchId > 0)
                {
                    var otherCatalogues = oConnectionContext.DbClsCatalogue
                        .Where(a => a.BranchId == branchId && 
                                    a.CompanyId == obj.CompanyId && 
                                    a.CatalogueId != obj.CatalogueId && 
                                    a.IsDeleted == false);
                    
                    foreach (var cat in otherCatalogues)
                    {
                        cat.ShowInInvoices = false;
                        oConnectionContext.Entry(cat).Property(x => x.ShowInInvoices).IsModified = true;
                    }
                    oConnectionContext.SaveChanges();
                }

                var existingCatalogueItems = oConnectionContext.DbClsCatalogueItem.Where(b => b.CatalogueId == obj.CatalogueId).ToList();
                if (existingCatalogueItems.Count > 0)
                {
                    oConnectionContext.DbClsCatalogueItem.RemoveRange(existingCatalogueItems);
                    oConnectionContext.SaveChanges();
                }

                var existingCatalogueCategories = oConnectionContext.DbClsCatalogueCategory.Where(b => b.CatalogueId == obj.CatalogueId).ToList();
                if (existingCatalogueCategories.Count > 0)
                {
                    oConnectionContext.DbClsCatalogueCategory.RemoveRange(existingCatalogueCategories);
                    oConnectionContext.SaveChanges();
                }

                var existingCatalogueBrands = oConnectionContext.DbClsCatalogueBrand.Where(b => b.CatalogueId == obj.CatalogueId).ToList();
                if (existingCatalogueBrands.Count > 0)
                {
                    oConnectionContext.DbClsCatalogueBrand.RemoveRange(existingCatalogueBrands);
                    oConnectionContext.SaveChanges();
                }

                if (obj.CatalogueType == 2 && obj.ItemIds != null && obj.ItemDetailIds != null)
                {
                    var pairedItems = obj.ItemIds
                        .Select((itemId, index) => new
                        {
                            ItemId = itemId,
                            ItemDetailId = obj.ItemDetailIds.Count > index ? obj.ItemDetailIds[index] : 0
                        })
                        .Where(p => p.ItemId > 0 && p.ItemDetailId > 0)
                        .GroupBy(p => new { p.ItemId, p.ItemDetailId })
                        .Select(g => g.Key)
                        .ToList();

                    if (pairedItems.Count > 0)
                    {
                        var itemMaps = pairedItems.Select((pair, idx) => new ClsCatalogueItem
                        {
                            CatalogueId = oCatalogue.CatalogueId,
                            ItemId = pair.ItemId,
                            ItemDetailsId = pair.ItemDetailId,
                            DisplayOrder = idx + 1,
                            IsFeatured = false,
                            CustomPrice = 0,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        }).ToList();

                        oConnectionContext.DbClsCatalogueItem.AddRange(itemMaps);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.CatalogueType == 3 && obj.CategoryIds != null)
                {
                    var categoryMaps = obj.CategoryIds
                        .Where(categoryId => categoryId > 0)
                        .Distinct()
                        .Select(categoryId => new ClsCatalogueCategory
                        {
                            CatalogueId = oCatalogue.CatalogueId,
                            CategoryId = categoryId,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        })
                        .ToList();

                    if (categoryMaps.Count > 0)
                    {
                        oConnectionContext.DbClsCatalogueCategory.AddRange(categoryMaps);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.CatalogueType == 4 && obj.BrandIds != null)
                {
                    var brandMaps = obj.BrandIds
                        .Where(brandId => brandId > 0)
                        .Distinct()
                        .Select(brandId => new ClsCatalogueBrand
                        {
                            CatalogueId = oCatalogue.CatalogueId,
                            BrandId = brandId,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        })
                        .ToList();

                    if (brandMaps.Count > 0)
                    {
                        oConnectionContext.DbClsCatalogueBrand.AddRange(brandMaps);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.EnableAddToCart == true)
                {
                    if (obj.AddToCartQuotationMode == 1 || obj.AddToCartQuotationMode == 3)
                    {
                        var saleSettings = oConnectionContext.DbClsSaleSettings
                            .Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                            .FirstOrDefault();

                        if (saleSettings != null && saleSettings.EnableSalesQuotation == false)
                        {
                            saleSettings.EnableSalesQuotation = true;
                            saleSettings.ModifiedBy = obj.AddedBy;
                            saleSettings.ModifiedOn = CurrentDate;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Catalogues",
                    CompanyId = obj.CompanyId,
                    Description = "Catalogue \"" + obj.CatalogueName + "\" updated",
                    Id = oCatalogue.CatalogueId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Catalogue updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CatalogueActiveInactive(ClsCatalogueVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsCatalogue oClsCatalogue = new ClsCatalogue()
                {
                    CatalogueId = obj.CatalogueId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCatalogue.Attach(oClsCatalogue);
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.CatalogueId).IsModified = true;
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Catalogues",
                    CompanyId = obj.CompanyId,
                    Description = "Catalogue \"" + oConnectionContext.DbClsCatalogue.Where(a => a.CatalogueId == obj.CatalogueId).Select(a => a.CatalogueName).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsCatalogue.CatalogueId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Catalogue " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CatalogueDelete(ClsCatalogueVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsCatalogue oClsCatalogue = new ClsCatalogue()
                {
                    CatalogueId = obj.CatalogueId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCatalogue.Attach(oClsCatalogue);
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.CatalogueId).IsModified = true;
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsCatalogue).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Catalogues",
                    CompanyId = obj.CompanyId,
                    Description = "Catalogue \"" + oConnectionContext.DbClsCatalogue.Where(a => a.CatalogueId == obj.CatalogueId).Select(a => a.CatalogueName).FirstOrDefault() + "\" deleted",
                    Id = oClsCatalogue.CatalogueId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Catalogue deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> PublicCatalogue(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                data = new { Status = 0, Message = "Slug is required" };
                return await Task.FromResult(Ok(data));
            }

           

            var catalogue = oConnectionContext.DbClsCatalogue
            .Where(a => a.UniqueSlug != null &&
                       a.UniqueSlug.ToLower() == slug.ToLower() &&
                       a.IsActive == true &&
                       a.IsDeleted == false)
            .Select(a => new
            {
                a.FacebookUrl,
                a.InstagramUrl,
                a.TwitterUrl,
                a.LinkedInUrl,
                a.YouTubeUrl,
                a.TikTokUrl,
                a.PinterestUrl,
                a.GoogleAnalyticsId,
                a.GoogleReviewUrl,
                a.CatalogueId,
                a.AddedBy,
                a.CatalogueName,
                a.Tagline,
                a.UniqueSlug,
                a.Theme,
                a.HeaderColor,
                a.HeaderTextColor,
                a.LinkIconAccentColor,
                a.AddToCartButtonColor,
                a.WhatsappSupportButtonColor,
                a.FilterBadgeColor,
                a.TopBarColor,
                a.TopBarTextColor,
                a.BodyBackgroundColor,
                a.BodyTextColor,
                a.IsHeaderFixed,
                a.ShowPrices,
                a.ShowMRP,
                a.ShowDiscount,
                a.ShowStock,
                a.ShowOutOfStock,
                a.ShowBusinessLogo,
                a.ShowBusinessName,
                a.ShowAddress,
                a.ShowContact,
                a.ShowWhatsApp,
                a.ShowEmail,
                a.ShowGstin,
                a.ShowProductCode,
                a.ShowImages,
                a.EnableWhatsAppEnquiry,
                a.EnableWhatsAppEnquiryForItems,
                a.EnableAddToCart,
                a.AddToCartQuotationMode,
                a.ItemsPerPage,
                a.DefaultSortOrder,
                a.IsPasswordProtected,
                a.ValidFrom,
                a.ValidTo,
                a.NeverExpires,
                a.CatalogueType,
                a.CompanyId,
                CategoryIds = oConnectionContext.DbClsCatalogueCategory
                    .Where(cc => cc.CatalogueId == a.CatalogueId && cc.IsDeleted == false && cc.IsActive == true)
                    .Select(cc => cc.CategoryId)
                    .Distinct()
                    .ToList(),
                BrandIds = oConnectionContext.DbClsCatalogueBrand
                    .Where(cb => cb.CatalogueId == a.CatalogueId && cb.IsDeleted == false && cb.IsActive == true)
                    .Select(cb => cb.BrandId)
                    .Distinct()
                    .ToList(),
                a.SellingPriceGroupId,
                a.MetaTitle,
                a.MetaDescription,
                a.BranchId,
                Branch = oConnectionContext.DbClsBranch
                    .Where(b => b.BranchId == a.BranchId)
                    .Select(b => new
                    {
                        DialingCode = oConnectionContext.DbClsCountry.Where(bb => bb.CountryId == b.CountryId).Select(bb => bb.DialingCode).FirstOrDefault(),
                        b.BranchId,
                        TaxNo = oConnectionContext.DbClsTaxSetting.Where(c => c.IsDeleted == false
                    && c.TaxSettingId == b.TaxSettingId).Select(c => c.BusinessRegistrationNo).FirstOrDefault(),
                        Tax = oConnectionContext.DbClsBusinessRegistrationName.Where(d => d.BusinessRegistrationNameId ==
                        oConnectionContext.DbClsTaxSetting.Where(c => c.IsDeleted == false
                        && c.TaxSettingId == b.TaxSettingId).Select(c => c.BusinessRegistrationNameId).FirstOrDefault()).Select(d => d.Name).FirstOrDefault(),
                        b.WhatsappNo,
                        b.Branch,
                        b.Address,
                        City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                        State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                        Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                        b.Zipcode,
                        b.Mobile,
                        b.Email,
                    })
                    .FirstOrDefault(),
                BusinessSetting = oConnectionContext.DbClsBusinessSettings
                    .Where(c => c.CompanyId == a.CompanyId)
                    .Select(c => new
                    {
                        c.BusinessName,
                        c.BusinessLogo
                    }).FirstOrDefault()
            }).FirstOrDefault();

            if (catalogue == null)
            {
                data = new { Status = 0, Message = "Catalogue not found" };
                return await Task.FromResult(Ok(data));
            }

            if (oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == catalogue.CompanyId &&
            a.StartDate != null && a.Status == 2 && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault() == 0)
            {
                data = new { Status = 0, Message = "Catalogue not found" };
                return await Task.FromResult(Ok(data));
            }

            // Check validity period
            if (!catalogue.NeverExpires)
            {
                var currentDate = DateTime.Now;
                if (catalogue.ValidFrom.HasValue && currentDate < catalogue.ValidFrom.Value)
                {
                    data = new { Status = 0, Message = "Catalogue not yet active" };
                    return await Task.FromResult(Ok(data));
                }
                if (catalogue.ValidTo.HasValue && currentDate > catalogue.ValidTo.Value)
                {
                    data = new { Status = 0, Message = "Catalogue has expired" };
                    return await Task.FromResult(Ok(data));
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new { Catalogue = catalogue }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> VerifyCataloguePassword(ClsCataloguePasswordVm obj)
        {
            if (string.IsNullOrEmpty(obj.UniqueSlug))
            {
                data = new { Status = 0, Message = "Slug is required" };
                return await Task.FromResult(Ok(data));
            }

            var catalogue = oConnectionContext.DbClsCatalogue
                .Where(a => a.UniqueSlug.ToLower() == obj.UniqueSlug.ToLower() &&
                           a.IsActive == true &&
                           a.IsDeleted == false)
                .FirstOrDefault();

            if (catalogue == null)
            {
                data = new { Status = 0, Message = "Catalogue not found" };
                return await Task.FromResult(Ok(data));
            }

            if (catalogue.Password == obj.Password)
            {
                data = new { Status = 1, Message = "Password verified" };
            }
            else
            {
                data = new { Status = 0, Message = "Incorrect password" };
            }

            return await Task.FromResult(Ok(data));
        }

        private string GenerateCatalogueQrCode(string slug, string domain, string existingRelativePath = null, long? catalogueId = null)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return existingRelativePath;
            }

            var catalogueUrl = BuildCatalogueUrl(slug, domain);
            if (string.IsNullOrWhiteSpace(catalogueUrl))
            {
                return existingRelativePath;
            }

            var directoryPath = HostingEnvironment.MapPath("~/ExternalContents/Images/CatalogueQRCode/");
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return existingRelativePath;
            }

            Directory.CreateDirectory(directoryPath);

            var sanitizedSlug = SanitizeFileNameSegment(slug);
            if (string.IsNullOrWhiteSpace(sanitizedSlug))
            {
                sanitizedSlug = "catalogue";
            }

            var fileName = catalogueId.HasValue && catalogueId.Value > 0
                ? string.Format("{0}-{1}.png", sanitizedSlug, catalogueId.Value)
                : sanitizedSlug + ".png";

            var filePath = Path.Combine(directoryPath, fileName);

            var normalizedExistingPath = NormalizeRelativePath(existingRelativePath);
            if (!string.IsNullOrWhiteSpace(normalizedExistingPath))
            {
                var existingPhysicalPath = HostingEnvironment.MapPath(normalizedExistingPath);
                if (!string.IsNullOrWhiteSpace(existingPhysicalPath) &&
                    !existingPhysicalPath.Equals(filePath, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(existingPhysicalPath))
                {
                    File.Delete(existingPhysicalPath);
                }
            }

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(catalogueUrl, QRCodeGenerator.ECCLevel.Q))
            using (var pngQrCode = new PngByteQRCode(qrCodeData))
            {
                var qrBytes = pngQrCode.GetGraphic(20);
                File.WriteAllBytes(filePath, qrBytes);
            }

            return "/ExternalContents/Images/CatalogueQRCode/" + fileName;
        }

        private static string NormalizeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var trimmed = relativePath.Trim();

            if (trimmed.StartsWith("~", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(1);
            }

            trimmed = trimmed.TrimStart('/');

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            return "~/" + trimmed;
        }

        private static string SanitizeFileNameSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray();
            var sanitized = new string(chars);

            while (sanitized.Contains("--"))
            {
                sanitized = sanitized.Replace("--", "-");
            }

            return sanitized.Trim('-');
        }

        private string BuildCatalogueUrl(string slug, string domain)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            string resolvedDomain = domain;

            if (string.IsNullOrWhiteSpace(resolvedDomain))
            {
                var request = HttpContext.Current?.Request;
                if (request != null)
                {
                    resolvedDomain = request.Url?.GetLeftPart(UriPartial.Authority);
                }
            }
            else
            {
                resolvedDomain = resolvedDomain.Trim();
                resolvedDomain = resolvedDomain.TrimEnd('/');

                if (!resolvedDomain.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (resolvedDomain.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ||
                        resolvedDomain.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                        resolvedDomain.StartsWith("::1", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedDomain = "http://" + resolvedDomain;
                    }
                    else
                    {
                        resolvedDomain = "https://" + resolvedDomain;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(resolvedDomain))
            {
                return "/c/" + slug;
            }

            return resolvedDomain + "/c/" + slug;
        }
    }
}
