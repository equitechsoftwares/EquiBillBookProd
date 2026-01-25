using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCatalogue")]
    public class ClsCatalogue
    {
        [Key]
        public long CatalogueId { get; set; }
        public string CatalogueName { get; set; }
        public string Tagline { get; set; }
        public int CatalogueType { get; set; } // 1=All, 2=Selected, 3=Category, 4=Brand
        public long BranchId { get; set; }
        public bool ShowPrices { get; set; }
        public bool ShowMRP { get; set; }
        public bool ShowDiscount { get; set; }
        public bool ShowStock { get; set; }
        public bool ShowOutOfStock { get; set; }
        public bool ShowBusinessLogo { get; set; }
        public bool ShowBusinessName { get; set; }
        public bool ShowAddress { get; set; }
        public bool ShowContact { get; set; }
        public bool ShowWhatsApp { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowGstin { get; set; }
        public bool EnableWhatsAppEnquiry { get; set; }
        public bool EnableWhatsAppEnquiryForItems { get; set; }
        public bool EnableAddToCart { get; set; }
        public string CartDisclaimerText { get; set; }
        public int AddToCartQuotationMode { get; set; }
        public long SellingPriceGroupId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Validity Period
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool NeverExpires { get; set; }

        // Display Preferences
        public bool ShowProductCode { get; set; }
        public bool ShowImages { get; set; }
        public int ItemsPerPage { get; set; }
        public int DefaultSortOrder { get; set; } // 1=Popularity, 2=Price Low-High, 3=Price High-Low, 4=Newest

        // Display Settings
        public string Theme { get; set; }
        public string HeaderColor { get; set; }
        public string HeaderTextColor { get; set; }
        public string LinkIconAccentColor { get; set; }        
        public string AddToCartButtonColor { get; set; }
        public string WhatsappSupportButtonColor { get; set; }
        public string FilterBadgeColor { get; set; }
        public string TopBarColor { get; set; }
        public string TopBarTextColor { get; set; }
        public string BodyBackgroundColor { get; set; }
        public string BodyTextColor { get; set; }
        public bool IsHeaderFixed { get; set; }

        // Advanced Features
        public bool IsPasswordProtected { get; set; }
        public string Password { get; set; }

        // SEO
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }

        // Analytics
        public string GoogleAnalyticsId { get; set; }

        // Google Review
        public string GoogleReviewUrl { get; set; }

        // Social Media Links
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string YouTubeUrl { get; set; }
        public string TikTokUrl { get; set; }
        public string PinterestUrl { get; set; }

        // QR Code
        public string QRCodeImage { get; set; }
        public string UniqueSlug { get; set; }

        // Invoice Display
        public bool ShowInInvoices { get; set; }
    }

    public class ClsCatalogueVm
    {
        public long CatalogueId { get; set; }
        public string CatalogueName { get; set; }
        public string Tagline { get; set; }
        public int CatalogueType { get; set; } // 1=All, 2=Selected, 3=Category, 4=Brand
        public bool ShowPrices { get; set; }
        public bool ShowMRP { get; set; }
        public bool ShowDiscount { get; set; }
        public bool ShowStock { get; set; }
        public bool ShowOutOfStock { get; set; }
        public bool ShowBusinessLogo { get; set; }
        public bool ShowBusinessName { get; set; }
        public bool ShowAddress { get; set; }
        public bool ShowContact { get; set; }
        public bool ShowWhatsApp { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowGstin { get; set; }
        public bool EnableWhatsAppEnquiry { get; set; }
        public bool EnableWhatsAppEnquiryForItems { get; set; }
        public bool EnableAddToCart { get; set; }
        public string CartDisclaimerText { get; set; }
        public int AddToCartQuotationMode { get; set; }
        public long SellingPriceGroupId { get; set; }
        private long _branchId;
        public long BranchId
        {
            get
            {
                return _branchId;
            }
            set
            {
                _branchId = value > 0 ? value : 0;
            }
        }

        public List<long> BranchIds
        {
            get
            {
                if (_branchId > 0)
                {
                    return new List<long> { _branchId };
                }

                return new List<long>();
            }
            set
            {
                if (value != null && value.Count > 0)
                {
                    var first = value.FirstOrDefault();
                    _branchId = first > 0 ? first : 0;
                }
                else
                {
                    _branchId = 0;
                }
            }
        }
        public List<long> ItemIds { get; set; } = new List<long>();
        public List<long> ItemDetailIds { get; set; } = new List<long>();
        public List<long> CategoryIds { get; set; } = new List<long>();
        public List<long> BrandIds { get; set; } = new List<long>();
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Validity Period
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool NeverExpires { get; set; }

        // Display Preferences
        public bool ShowProductCode { get; set; }
        public bool ShowImages { get; set; }
        public int ItemsPerPage { get; set; }
        public int DefaultSortOrder { get; set; } // 1=Popularity, 2=Price Low-High, 3=Price High-Low, 4=Newest

        // Display Settings
        public string Theme { get; set; }
        public string HeaderColor { get; set; }
        public string HeaderTextColor { get; set; }
        public string LinkIconAccentColor { get; set; }
        public string AddToCartButtonColor { get; set; }
        public string WhatsappSupportButtonColor { get; set; }
        public string FilterBadgeColor { get; set; }
        public string TopBarColor { get; set; }
        public string TopBarTextColor { get; set; }
        public string BodyBackgroundColor { get; set; }
        public string BodyTextColor { get; set; }
        public bool IsHeaderFixed { get; set; }

        // Advanced Features
        public bool IsPasswordProtected { get; set; }
        public string Password { get; set; }

        // SEO
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }

        // Analytics
        public string GoogleAnalyticsId { get; set; }

        // Google Review
        public string GoogleReviewUrl { get; set; }

        // Social Media Links
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string YouTubeUrl { get; set; }
        public string TikTokUrl { get; set; }
        public string PinterestUrl { get; set; }

        // QR Code
        public string QRCodeImage { get; set; }
        public string UniqueSlug { get; set; }

        // Invoice Display
        public bool ShowInInvoices { get; set; }

        // Pagination & Search Properties
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string BranchName { get; set; }
        public string CatalogueTypeText { get; set; }
        public ClsBusinessSettingsVm BusinessSetting { get; set; }
        public ClsBranchVm Branch { get; set; }
    }

    public class ClsCataloguePasswordVm
    {
        public string UniqueSlug { get; set; }
        public string Password { get; set; }
    }

    public class PublicCatalogueCategoryVm
    {
        public long? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public IList<PublicCatalogueSubCategoryVm> SubCategories { get; set; }
    }

    public class PublicCatalogueSubCategoryVm
    {
        public long? SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public IList<PublicCatalogueSubSubCategoryVm> SubSubCategories { get; set; }
    }

    public class PublicCatalogueSubSubCategoryVm
    {
        public long? SubSubCategoryId { get; set; }
        public string SubSubCategoryName { get; set; }
    }

    public class PublicCatalogueProductVm
    {
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public string ProductCode { get; set; }
        public string ProductImage { get; set; }
        public decimal SalesIncTax { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal DefaultMrp { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Quantity { get; set; }
        public string AttributesJson { get; set; }
        public string VariationName { get; set; }
        public string ProductType { get; set; }
        public List<PublicCatalogueProductVariantVm> Variants { get; set; } = new List<PublicCatalogueProductVariantVm>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class PublicCatalogueProductVariantVm
    {
        public long ItemDetailsId { get; set; }
        public string VariationName { get; set; }
        public string ProductImage { get; set; }
        public decimal SalesIncTax { get; set; }
        public decimal DefaultMrp { get; set; }
        public decimal Quantity { get; set; }
        public string SKU { get; set; }
        public string AttributesJson { get; set; }
    }

    public class PublicCatalogueProductGridViewModel
    {
        public PublicCatalogueProductGridViewModel()
        {
            Products = new List<PublicCatalogueProductVm>();
        }

        public IList<PublicCatalogueProductVm> Products { get; set; }
        public bool ShowPrices { get; set; }
        public bool ShowMRP { get; set; }
        public bool ShowDiscount { get; set; }
        public bool ShowStock { get; set; }
        public bool ShowOutOfStock { get; set; }
        public bool ShowProductCode { get; set; }
        public bool ShowImages { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
        public int NextPageIndex { get; set; }
    }

    public class PublicCatalogueFilterRequest
    {
        public string Slug { get; set; }
        public List<long> BrandIds { get; set; } = new List<long>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool IncludeOutOfStock { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public int SortOrder { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public long? SubSubCategoryId { get; set; }
        public long? PlaceOfSupplyId { get; set; }
    }

    public class PublicCatalogueSearchSuggestionsRequest
    {
        public string Slug { get; set; }
        public string Term { get; set; }
        public int Limit { get; set; } = 8;
        public bool? IncludeOutOfStock { get; set; }
        public List<long> BrandIds { get; set; } = new List<long>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public long? PlaceOfSupplyId { get; set; }
    }

    public class PublicCatalogueQuotationRequest
    {
        public string Slug { get; set; }
        public int Mode { get; set; }
        public List<PublicCatalogueCartItem> Items { get; set; } = new List<PublicCatalogueCartItem>();
        public PublicCatalogueCustomerPayload Customer { get; set; }
        public string Notes { get; set; }
    }

    public class PublicCatalogueCartItem
    {
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public string Code { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Mrp { get; set; }
    }

    public class PublicCatalogueCustomerPayload
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string MobileRaw { get; set; }
        public string Email { get; set; }
        public bool IsShippingDifferent { get; set; }
        public PublicCatalogueAddressPayload Billing { get; set; }
        public PublicCatalogueAddressPayload Shipping { get; set; }
    }

    public class PublicCatalogueAddressPayload
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string AltMobile { get; set; }
        public string Email { get; set; }
        public long CountryId { get; set; }
        public string Country { get; set; }
        public long StateId { get; set; }
        public string State { get; set; }
        public long CityId { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Landmark { get; set; }
        public string Pincode { get; set; }
    }
}