using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblBusinessSettings")]
    public class ClsBusinessSettings
    {
        [Key]
        public long BusinessSettingsId { get; set; }
        public string BusinessName { get; set; }
        public DateTime? StartDate { get; set; }
        public long CountryId { get; set; }
        public int CurrencySymbolPlacement { get; set; }
        public long TimeZoneId { get; set; }
        public bool EnableDaylightSavingTime { get; set; }
        public long IndustryTypeId { get; set; }
        public string BusinessTypeIds { get; set; }
        public int FinancialYearStartMonth { get; set; }
        public int TransactionEditDays { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string BusinessLogo { get; set; }
        public decimal CreditLimitForSupplier { get; set; }
        public decimal CreditLimitForCustomer { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int SetupStatus { get; set; }
        public int DatatablePageEntries { get; set; }
        public bool ShowHelpText { get; set; }
        public bool EnableDarkMode { get; set; }
        public bool FixedHeader { get; set; }
        public bool FixedFooter { get; set; }
        public bool EnableSound { get; set; }
        public string WebsiteUrl { get; set; }
        public long CompetitorId { get; set; }
        public bool EnableDefaultSmsBranding { get; set; }
        public bool EnableDefaultEmailBranding { get; set; }
        public string Favicon { get; set; }
        public string ParentBusinessName { get; set; }
        public string ParentWebsiteUrl { get; set; }
        public int FreeTrialDays { get; set; }
        public string ResellerTermsUrl { get; set; }
        public bool CollapseSidebar { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string BusinessRegistrationType { get; set; }
        public string BusinessIcon { get; set; }
    }

    public class ClsBusinessSettingsVm
    {
        public long BusinessSettingsId { get; set; }
        public string BusinessName { get; set; }
        public DateTime? StartDate { get; set; }
        public long CountryId { get; set; }
        public int CurrencySymbolPlacement { get; set; }
        public long TimeZoneId { get; set; }
        public bool EnableDaylightSavingTime { get; set; }
        public long IndustryTypeId { get; set; }
        public string BusinessTypeIds { get; set; }
        public string[] BusinessTypes { get; set; }
        public int FinancialYearStartMonth { get; set; }
        public int TransactionEditDays { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string BusinessLogo { get; set; }
        public decimal CreditLimitForSupplier { get; set; }
        public decimal CreditLimitForCustomer { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int SetupStatus { get; set; }
        public int DaysLeft { get; set; }
        public bool IsMainBranchUpdated { get; set; }
        public long BranchId { get; set; }
        public bool IsBusinessNameUpdated { get; set; }
        public bool IsFirstCustomerCreated { get; set; }
        public bool IsFirstItemCreated { get; set; }
        public bool IsFirstSaleCreated { get; set; }
        public string FileExtension { get; set; }
        public string FileExtensionFavicon { get; set; }
        public string FileExtensionIcon { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public int DatatablePageEntries { get; set; }
        public bool ShowHelpText { get; set; }
        public bool EnableDarkMode { get; set; }
        public string Country { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }
        public string TimeZoneDisplayName { get; set; }
        public bool FixedHeader { get; set; }
        public bool FixedFooter { get; set; }
        public bool EnableSound { get; set; }
        public string CurrencyCode { get; set; }
        public bool SupportsDaylightSavingTime { get; set; }
        public string WebsiteUrl { get; set; }
        public long CompetitorId { get; set; }
        public bool EnableDefaultSmsBranding { get; set; }
        public bool EnableDefaultEmailBranding { get; set; }
        public List<ClsShortCutKeySettingVm> ShortCutKeySettings { get; set; }
        public string Domain { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string AltMobileNo { get; set; }
        public string Email { get; set; }
        public string Favicon { get; set; }
        public string ParentBusinessName { get; set; }
        public string ParentWebsiteUrl { get; set; }
        public int FreeTrialDays { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string ResellerTermsUrl { get; set; }
        public bool CollapseSidebar { get; set; }
        public int IsBusinessRegistered { get; set; }
        public long StateId { get; set; }
        public string CustomerTaxPreference { get; set; }
        public string BusinessRegistrationType { get; set; }
        public string GstTreatment { get; set; }
        public string BusinessIcon { get; set; }
    }

}