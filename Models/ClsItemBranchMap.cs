using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblItemBranchMap")]
    public class ClsItemBranchMap
    {
        [Key]
        public long ItemBranchMapId { get; set; }
        public long BranchId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Rack { get; set; }
        public string Row { get; set; }
        public string Position { get; set; }
        public decimal SalesIncTax { get; set; }
    }

    public class ClsItemBranchMapVm
    {
        public long ItemBranchMapId { get; set; }
        public long BranchId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long BrandId { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public string Rack { get; set; }
        public string Row { get; set; }
        public string Position { get; set; }
        public decimal SalesIncTax { get; set; }
        public string Domain { get; set; }
        public List<long> BrandIds { get; set; } = new List<long>();
        public string Search { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int SortOrder { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool IncludeOutOfStock { get; set; }
        public string UniqueSlug { get; set; }
        public long PlaceOfSupplyId { get; set; }
    }

}