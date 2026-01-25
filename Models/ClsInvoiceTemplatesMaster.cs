using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblInvoiceTemplatesMaster")]
    public class ClsInvoiceTemplatesMaster
    {
        [Key]
        public long InvoiceTemplateMasterId { get; set; }
        
        // Template Identity
        public string InvoiceType { get; set; } // "Sales", "SalesQuotation", "Purchase", etc.
        public string TemplateKey { get; set; } // "modern", "professional-quote", etc.
        public string TemplateName { get; set; } // User-friendly name
        public string Description { get; set; }
        
        // Visual Properties
        public string PreviewColor { get; set; } // CSS gradient string
        public string Icon { get; set; } // Font Awesome icon class
        public string PreviewImageUrl { get; set; } // Optional image URL
        public string TemplateHtmlPath { get; set; } // Path to HTML template file
        
        // Configuration
        public string TemplateConfig { get; set; } // JSON with default template settings
        public int SortOrder { get; set; }
        
        // Feature flags
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string RequiredAddons { get; set; } // JSON array of required addon names
        
        // Audit fields
        public DateTime AddedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsInvoiceTemplatesMasterVm
    {
        public long InvoiceTemplateMasterId { get; set; }
        public string InvoiceType { get; set; }
        public string TemplateKey { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string PreviewColor { get; set; }
        public string Icon { get; set; }
        public string PreviewImageUrl { get; set; }
        public string TemplateHtmlPath { get; set; }
        public string TemplateConfig { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string RequiredAddons { get; set; }
        
        // For request
        public long CompanyId { get; set; }
        
        // For response with HTML content
        public string TemplateHtml { get; set; }
    }
}

