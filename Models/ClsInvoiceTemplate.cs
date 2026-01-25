using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblInvoiceTemplates")]
    public class ClsInvoiceTemplate
    {
        [Key]
        public long InvoiceTemplateId { get; set; }
        public long CompanyId { get; set; }
        public string InvoiceType { get; set; } // "Sales", "SalesQuotation", "Purchase", etc.
        public string TemplateName { get; set; } // User-friendly name
        public string TemplateKey { get; set; } // "modern", "classic", "minimal", etc.
        public string Description { get; set; } // Short description shown on cards
        
        // Template Configuration (stored as JSON for flexibility)
        public string TemplateConfig { get; set; }
        
        // Default flag - only one default per invoice type per company
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        
        // Audit fields
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsInvoiceTemplateVm
    {
        public long InvoiceTemplateId { get; set; }
        public long CompanyId { get; set; }
        public string InvoiceType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateKey { get; set; }
        public InvoiceTemplateConfig TemplateConfig { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Description { get; set; }
        public string PreviewColor { get; set; }
        public string TemplateHtmlPath { get; set; }
    }

    // Invoice Type Configuration
    public class InvoiceTypeConfig
    {
        public string InvoiceTypeKey { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public int SortOrder { get; set; }
        public string Category { get; set; } // "Customer" or "Supplier"
    }

    // Template Configuration Classes
    public class InvoiceTemplateConfig
    {
        // Color Theme
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string HeaderTextColor { get; set; }
        public string BodyBackgroundColor { get; set; }
        public string BodyTextColor { get; set; }
        public string BorderColor { get; set; }
        public string TableHeaderColor { get; set; }
        public string TotalRowColor { get; set; }
        public string DueRowColor { get; set; }
        
        // Layout Options
        public string LayoutStyle { get; set; } // "Standard", "Compact", "Detailed"
        public bool ShowLogo { get; set; }
        public string LogoPosition { get; set; } // "Left", "Center", "Right"
        public bool ShowCompanyDetails { get; set; }
        public bool ShowQRCode { get; set; }
        public string QRCodePosition { get; set; }
        
        // Type-Specific Label Visibility
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Print Options
        public PrintOptions PrintSettings { get; set; }
        
        // Additional Customizations
        public bool ShowTaxBreakdown { get; set; }
        public bool ShowPaymentInfo { get; set; }
        public bool ShowShippingAddress { get; set; }
        public bool ShowBillingAddress { get; set; }
        public bool ShowFooterNote { get; set; }
        public string FooterNoteText { get; set; }
        public bool ShowItemImages { get; set; }
        public bool ShowItemDescription { get; set; }
        public bool ShowBarcode { get; set; }

        public InvoiceTemplateConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            PrintSettings = new PrintOptions();
        }
    }

    public class PrintOptions
    {
        public bool ShowPrintButton { get; set; }
        public bool ShowExportPdfButton { get; set; }
        public bool ShowPayNowButton { get; set; }
        public bool PrintLogo { get; set; }
        public bool PrintCompanyDetails { get; set; }
        public bool PrintWatermark { get; set; }
        public string WatermarkText { get; set; }
        public string PageSize { get; set; } // "A4", "Letter", "Custom"
        public string PageOrientation { get; set; } // "Portrait", "Landscape"
        public decimal MarginTop { get; set; }
        public decimal MarginBottom { get; set; }
        public decimal MarginLeft { get; set; }
        public decimal MarginRight { get; set; }
        public string FooterText { get; set; } // HTML footer text
    }

    // ========== ENHANCED TEMPLATE CONFIGURATION ==========
    // Section-based configuration with clear segregation
    
    public class ClsEnhancedInvoiceTemplateConfig
    {
        // ========== GLOBAL STYLING ==========
        public ClsColorTheme Colors { get; set; }
        public ClsFontTheme Fonts { get; set; }
        
        // ========== SECTION CONFIGURATIONS ==========
        public ClsHeaderSectionConfig Header { get; set; }
        public ClsCompanySectionConfig Company { get; set; }
        public ClsCustomerSectionConfig Customer { get; set; }
        public ClsItemsSectionConfig Items { get; set; }
        public ClsSummarySectionConfig Summary { get; set; }
        public ClsFooterSectionConfig Footer { get; set; }
        
        // ========== LAYOUT & PRINT ==========
        public ClsLayoutConfig Layout { get; set; }
        public PrintOptions PrintSettings { get; set; }

        public ClsEnhancedInvoiceTemplateConfig()
        {
            Colors = new ClsColorTheme();
            Fonts = new ClsFontTheme();
            Header = new ClsHeaderSectionConfig();
            Company = new ClsCompanySectionConfig();
            Customer = new ClsCustomerSectionConfig();
            Items = new ClsItemsSectionConfig();
            Summary = new ClsSummarySectionConfig();
            Footer = new ClsFooterSectionConfig();
            Layout = new ClsLayoutConfig();
            PrintSettings = new PrintOptions();
        }
    }

    // ========== COLOR THEME ==========
    public class ClsColorTheme
    {
        public string PrimaryColor { get; set; } = "#3b82f6";
        public string SecondaryColor { get; set; } = "#1e40af";
        public string HeaderBackgroundColor { get; set; } // Uses PrimaryColor if null
        public string HeaderTextColor { get; set; } = "#ffffff";
        public string BodyBackgroundColor { get; set; } = "#ffffff";
        public string BodyTextColor { get; set; } = "#1f2937";
        public string BorderColor { get; set; } = "#e5e7eb";
        public string TableHeaderColor { get; set; } = "#f9fafb";
        public string TotalRowColor { get; set; } = "#eff6ff";
        public string DueRowColor { get; set; } = "#fef2f2";
        public string AccentColor { get; set; } // Optional accent color
    }

    // ========== FONT THEME ==========
    public class ClsFontTheme
    {
        public string PrimaryFontFamily { get; set; } = "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif";
        public string SecondaryFontFamily { get; set; } // Optional secondary font
        public int BaseFontSize { get; set; } = 13; // in pixels
        public ClsFontWeightConfig FontWeights { get; set; }
        public ClsFontSizeConfig FontSizes { get; set; }

        public ClsFontTheme()
        {
            FontWeights = new ClsFontWeightConfig();
            FontSizes = new ClsFontSizeConfig();
        }
    }

    public class ClsFontWeightConfig
    {
        public int Normal { get; set; } = 400;
        public int Medium { get; set; } = 500;
        public int SemiBold { get; set; } = 600;
        public int Bold { get; set; } = 700;
    }

    public class ClsFontSizeConfig
    {
        public int Small { get; set; } = 11;
        public int Base { get; set; } = 13;
        public int Large { get; set; } = 16;
        public int ExtraLarge { get; set; } = 22;
        public int Title { get; set; } = 28;
    }

    // ========== LABEL STYLE ==========
    // Individual label styling (color, font weight, font size)
    public class ClsLabelStyle
    {
        public string Color { get; set; } // Label text color
        public int FontWeight { get; set; } = 400; // Font weight (300-800)
        public int FontSize { get; set; } = 13; // Font size in pixels
    }

    // ========== HEADER SECTION ==========
    public class ClsHeaderSectionConfig
    {
        public bool ShowHeader { get; set; } = true;
        public bool ShowLogo { get; set; } = true;
        public string LogoPosition { get; set; } = "Left"; // Left, Center, Right
        public bool ShowInvoiceTitle { get; set; } = true; // "INVOICE", "QUOTATION", etc.
        public bool ShowInvoiceNumber { get; set; } = true;
        public bool ShowDate { get; set; } = true;
        public bool ShowStatus { get; set; } = true;
        public bool ShowQRCode { get; set; } = false;
        public string QRCodePosition { get; set; } = "Right";
        
        // Header-specific labels (invoice type aware)
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Individual label styling (color, font weight, font size per label)
        public Dictionary<string, ClsLabelStyle> LabelStyles { get; set; }
        
        // Header styling
        public string BackgroundColor { get; set; } // Uses ColorTheme.PrimaryColor if null
        public string TextColor { get; set; } // Uses ColorTheme.HeaderTextColor if null
        public int PaddingTop { get; set; } = 24;
        public int PaddingBottom { get; set; } = 24;
        public int PaddingLeft { get; set; } = 32;
        public int PaddingRight { get; set; } = 32;

        public ClsHeaderSectionConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            LabelStyles = new Dictionary<string, ClsLabelStyle>();
        }
    }

    // ========== COMPANY SECTION ==========
    public class ClsCompanySectionConfig
    {
        public bool ShowCompanySection { get; set; } = true;
        public bool ShowCompanyName { get; set; } = true;
        public bool ShowCompanyAddress { get; set; } = true;
        public bool ShowCompanyPhone { get; set; } = true;
        public bool ShowCompanyEmail { get; set; } = true;
        public bool ShowCompanyWebsite { get; set; } = false;
        public bool ShowCompanyGST { get; set; } = true;
        public bool ShowCompanyPAN { get; set; } = false;
        public bool ShowCompanyBankDetails { get; set; } = false;
        
        // Company-specific labels
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Individual label styling (color, font weight, font size per label)
        public Dictionary<string, ClsLabelStyle> LabelStyles { get; set; }
        
        // Company section styling
        public string SectionTitle { get; set; } = "From"; // "From", "Company", "Seller", etc.
        public string TextColor { get; set; }
        public string BackgroundColor { get; set; }

        public ClsCompanySectionConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            LabelStyles = new Dictionary<string, ClsLabelStyle>();
        }
    }

    // ========== CUSTOMER SECTION ==========
    public class ClsCustomerSectionConfig
    {
        public bool ShowCustomerSection { get; set; } = true;
        public bool ShowCustomerName { get; set; } = true;
        public bool ShowCustomerAddress { get; set; } = true;
        public bool ShowCustomerPhone { get; set; } = true;
        public bool ShowCustomerEmail { get; set; } = true;
        public bool ShowCustomerGST { get; set; } = true;
        public bool ShowCustomerPAN { get; set; } = false;
        public bool ShowShippingAddress { get; set; } = false;
        public bool ShowBillingAddress { get; set; } = true;
        
        // Customer-specific labels (invoice type aware)
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Individual label styling (color, font weight, font size per label)
        public Dictionary<string, ClsLabelStyle> LabelStyles { get; set; }
        
        // Customer section styling
        public string SectionTitle { get; set; } = "Bill To"; // "Bill To", "Ship To", "Customer", "Buyer", etc.
        public string TextColor { get; set; }
        public string BackgroundColor { get; set; }

        public ClsCustomerSectionConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            LabelStyles = new Dictionary<string, ClsLabelStyle>();
        }
    }

    // ========== ITEMS SECTION ==========
    public class ClsItemsSectionConfig
    {
        public bool ShowItemsTable { get; set; } = true;
        public bool ShowItemImages { get; set; } = false;
        public bool ShowItemDescription { get; set; } = true;
        public bool ShowItemSKU { get; set; } = true;
        public bool ShowItemHSN { get; set; } = false;
        public bool ShowItemQuantity { get; set; } = true;
        public bool ShowItemUnit { get; set; } = true;
        public bool ShowItemRate { get; set; } = true;
        public bool ShowItemDiscount { get; set; } = true;
        public bool ShowItemTax { get; set; } = true;
        public bool ShowItemTotal { get; set; } = true;
        public bool ShowItemBarcode { get; set; } = false;
        
        // Items-specific labels (invoice type aware)
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Individual label styling (color, font weight, font size per label)
        public Dictionary<string, ClsLabelStyle> LabelStyles { get; set; }
        
        // Items table styling
        public string TableHeaderBackgroundColor { get; set; }
        public string TableHeaderTextColor { get; set; }
        public string TableBorderColor { get; set; }
        public bool ShowAlternateRowColors { get; set; } = true;
        public string AlternateRowColor { get; set; } = "#f9fafb";

        public ClsItemsSectionConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            LabelStyles = new Dictionary<string, ClsLabelStyle>();
        }
    }

    // ========== SUMMARY SECTION ==========
    public class ClsSummarySectionConfig
    {
        public bool ShowSummarySection { get; set; } = true;
        public bool ShowTotalQuantity { get; set; } = true;
        public bool ShowGrossAmount { get; set; } = true;
        public bool ShowTotalDiscount { get; set; } = true;
        public bool ShowSpecialDiscount { get; set; } = false;
        public bool ShowTaxBreakdown { get; set; } = true;
        public bool ShowNetAmount { get; set; } = true;
        public bool ShowRoundOff { get; set; } = true;
        public bool ShowGrandTotal { get; set; } = true;
        public bool ShowDueAmount { get; set; } = false; // Only for invoices with payment terms
        public bool ShowPaidAmount { get; set; } = false;
        public bool ShowBalanceAmount { get; set; } = false;
        
        // Summary-specific labels (invoice type aware)
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Individual label styling (color, font weight, font size per label)
        public Dictionary<string, ClsLabelStyle> LabelStyles { get; set; }
        
        // Summary styling
        public string TotalRowBackgroundColor { get; set; }
        public string TotalRowTextColor { get; set; }
        public string SummaryPosition { get; set; } = "Right"; // Left, Right, Bottom

        public ClsSummarySectionConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            LabelStyles = new Dictionary<string, ClsLabelStyle>();
        }
    }

    // ========== FOOTER SECTION ==========
    public class ClsFooterSectionConfig
    {
        public bool ShowFooter { get; set; } = true;
        public bool ShowFooterNote { get; set; } = true;
        public string FooterNoteText { get; set; } = "This is a computer generated invoice. No signature is required.";
        public bool ShowTermsAndConditions { get; set; } = false;
        public bool ShowPaymentInformation { get; set; } = false;
        public bool ShowBankDetails { get; set; } = false;
        public bool ShowSignature { get; set; } = false;
        public bool ShowValidityPeriod { get; set; } = false; // For quotations
        
        // Footer-specific labels
        public Dictionary<string, bool> LabelVisibility { get; set; }
        
        // Individual label styling (color, font weight, font size per label)
        public Dictionary<string, ClsLabelStyle> LabelStyles { get; set; }
        
        // Footer styling
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public int PaddingTop { get; set; } = 16;
        public int PaddingBottom { get; set; } = 16;

        public ClsFooterSectionConfig()
        {
            LabelVisibility = new Dictionary<string, bool>();
            LabelStyles = new Dictionary<string, ClsLabelStyle>();
        }
    }

    // ========== LAYOUT CONFIGURATION ==========
    public class ClsLayoutConfig
    {
        public string LayoutStyle { get; set; } = "Standard"; // Standard, Compact, Detailed
        public string PageWidth { get; set; } = "800px";
        public bool ShowBorders { get; set; } = true;
        public int BorderRadius { get; set; } = 6;
        public string Spacing { get; set; } = "normal"; // compact, normal, spacious
    }

    // ========== SECTION LABEL CONFIGURATION ==========
    // Helper class to organize labels by section for each invoice type
    public class ClsSectionLabelConfig
    {
        public Dictionary<string, bool> HeaderLabels { get; set; }
        public Dictionary<string, bool> CompanyLabels { get; set; }
        public Dictionary<string, bool> CustomerLabels { get; set; }
        public Dictionary<string, bool> ItemsLabels { get; set; }
        public Dictionary<string, bool> SummaryLabels { get; set; }
        public Dictionary<string, bool> FooterLabels { get; set; }

        public ClsSectionLabelConfig()
        {
            HeaderLabels = new Dictionary<string, bool>();
            CompanyLabels = new Dictionary<string, bool>();
            CustomerLabels = new Dictionary<string, bool>();
            ItemsLabels = new Dictionary<string, bool>();
            SummaryLabels = new Dictionary<string, bool>();
            FooterLabels = new Dictionary<string, bool>();
        }
    }
}

