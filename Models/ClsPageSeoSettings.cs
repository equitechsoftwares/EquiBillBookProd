using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblPageSeoSettings")]
    public class ClsPageSeoSettings
    {
        [Key]
        public long PageSeoSettingsId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PageIdentifier { get; set; }
        
        [StringLength(200)]
        public string PageTitle { get; set; }
        
        [StringLength(500)]
        public string MetaDescription { get; set; }
        
        [StringLength(1000)]
        public string MetaKeywords { get; set; }
        
        // Open Graph Tags
        [StringLength(200)]
        public string OgTitle { get; set; }
        
        [StringLength(500)]
        public string OgDescription { get; set; }
        
        [StringLength(500)]
        public string OgImage { get; set; }
        
        [StringLength(500)]
        public string OgUrl { get; set; }
        
        // Twitter Card Tags
        [StringLength(200)]
        public string TwitterTitle { get; set; }
        
        [StringLength(500)]
        public string TwitterDescription { get; set; }
        
        [StringLength(500)]
        public string TwitterImage { get; set; }
        
        // Canonical URL
        [StringLength(500)]
        public string CanonicalUrl { get; set; }
        
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsPageSeoSettingsVm
    {
        public long PageSeoSettingsId { get; set; }
        public string PageIdentifier { get; set; }
        public string PageTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string OgUrl { get; set; }
        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterImage { get; set; }
        public string CanonicalUrl { get; set; }
        public bool IsActive { get; set; }
        public long AddedBy { get; set; }
        public long CompanyId { get; set; }
        public string Domain { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
    }
}

