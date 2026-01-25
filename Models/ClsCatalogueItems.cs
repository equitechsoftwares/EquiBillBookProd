using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCatalogueItem")]
    public class ClsCatalogueItem
    {
        [Key]
        public long CatalogueItemId { get; set; }
        public long CatalogueId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public decimal CustomPrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsCatalogueItemVm
    {
        public long CatalogueItemId { get; set; }
        public long CatalogueId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; }
        public decimal CustomPrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public string UniqueSlug { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int SortOrder { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public long? SubSubCategoryId { get; set; }
        public List<long> BrandIds { get; set; } = new List<long>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool IncludeOutOfStock { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
        public long? PlaceOfSupplyId { get; set; }
    }

}