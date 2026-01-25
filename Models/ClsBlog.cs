using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblBlog")]
    public class ClsBlog
    {
        [Key]
        public long BlogId { get; set; }
        public string Title { get; set; }
        
        public string ShortDescription { get; set; }
        
        [AllowHtml]
        public string Description { get; set; }
        
        public string Image { get; set; }
        
        public long? BlogCategoryId { get; set; }
        
        public string Taglist { get; set; } // Comma-separated tags
        
        public string MetaTitle { get; set; }
        
        public string MetaDescription { get; set; }
        
        public string MetaKeywords { get; set; }
        
        public string UniqueSlug { get; set; } // URL-friendly slug
        
        public DateTime? PublishedDate { get; set; }
        public long ViewCount { get; set; }
        
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsBlogVm
    {
        public long BlogId { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string FileExtensionImage { get; set; }
        public long? BlogCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Taglist { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string UniqueSlug { get; set; }
        public DateTime? PublishedDate { get; set; }
        public long ViewCount { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
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
        public string CategoryFilter { get; set; }
    }
}

