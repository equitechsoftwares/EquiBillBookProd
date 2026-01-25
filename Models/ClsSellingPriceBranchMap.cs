using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSellingPriceBranchMap")]
    public class ClsSellingPriceBranchMap
    {
        [Key]
        public long SellingPriceBranchMapId { get; set; }
        public long SellingPriceGroupId { get; set; }
        public long BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsSellingPriceBranchMapVm
    {
        public long SellingPriceBranchMapId { get; set; }
        public long SellingPriceGroupId { get; set; }
        public long BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

}