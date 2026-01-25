using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblInvoiceTemplateColorCategoriesMaster")]
    public class ClsInvoiceTemplateColorCategoryMaster
    {
        [Key]
        public long InvoiceTemplateColorCategoryMasterId { get; set; }
        
        public long InvoiceTemplateMasterId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CategoryKey { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }
        
        public string Description { get; set; }
        
        [MaxLength(50)]
        public string Icon { get; set; }
        
        public int SortOrder { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
    }
}

