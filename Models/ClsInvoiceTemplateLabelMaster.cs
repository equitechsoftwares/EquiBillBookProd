using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblInvoiceTemplateLabelsMaster")]
    public class ClsInvoiceTemplateLabelMaster
    {
        [Key]
        public long InvoiceTemplateLabelMasterId { get; set; }

        public long InvoiceTemplateMasterId { get; set; }

        public long InvoiceTemplateLabelCategoryMasterId { get; set; } // FK to ClsInvoiceTemplateLabelCategoryMaster

        [Required]
        [MaxLength(100)]
        public string LabelKey { get; set; } // e.g., "InvoiceNumber", "InvoiceDate", "QuotationNumber"

        [Required]
        [MaxLength(200)]
        public string LabelText { get; set; } // Display text shown in UI

        [MaxLength(20)]
        public string LabelColor { get; set; } // Label color hex value

        public bool IsVisibleByDefault { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsInvoiceTemplateLabelMasterVm
    {
        public long InvoiceTemplateLabelMasterId { get; set; }
        public long InvoiceTemplateMasterId { get; set; }
        public long InvoiceTemplateLabelCategoryMasterId { get; set; }
        public string CategoryKey { get; set; } // Populated from join with categories
        public string CategoryName { get; set; } // Populated from join with categories
        public string LabelKey { get; set; }
        public string LabelText { get; set; }
        public string LabelColor { get; set; }
        public bool IsVisibleByDefault { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        // Request metadata
        public long CompanyId { get; set; }
    }
}

