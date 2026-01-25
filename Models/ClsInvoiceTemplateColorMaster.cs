using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblInvoiceTemplateColorsMaster")]
    public class ClsInvoiceTemplateColorMaster
    {
        [Key]        
        public long InvoiceTemplateColorMasterId { get; set; }
        public long InvoiceTemplateMasterId { get; set; }
        public string ColorKey { get; set; }
        public string ColorName { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        
        // Foreign key to Color Categories Master
        public long? InvoiceTemplateColorCategoryMasterId { get; set; }
        
        // Keep Category string for backward compatibility (will be removed after migration)
        public string Category { get; set; }
        
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsInvoiceTemplateColorMasterVm
    {
        public long InvoiceTemplateColorMasterId { get; set; }
        public long InvoiceTemplateMasterId { get; set; }
        public string ColorKey { get; set; }
        public string ColorName { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        
        // Foreign key to Color Categories Master
        public long? InvoiceTemplateColorCategoryMasterId { get; set; }
        
        // Category info (populated via join/lookup, for display)
        public string Category { get; set; }
        public string CategoryName { get; set; }
        public string CategoryKey { get; set; }
        
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
        // Additional properties for ViewModel
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public long AddedBy { get; set; }
        public long ModifiedBy { get; set; }
    }
}

