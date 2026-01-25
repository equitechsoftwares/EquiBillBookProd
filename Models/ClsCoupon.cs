using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCoupon")]
    public class ClsCoupon
    {
        [Key]
        public long CouponId { get; set; }
        public string CouponCode { get; set; }
        public string CouponDescription { get; set; }
        public int UsageType { get; set; }
        public int NoOfTimes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsNeverExpires { get; set; }
        public int DiscountType { get; set; }
        public decimal Discount { get; set; }
        public decimal MinimumPurchaseAmount { get; set; }
        public decimal MaximumDiscountAmount { get; set; }
        public bool ApplyToBasePlan { get; set; }
        public bool ApplyToAddons { get; set; }
        public int OrderNo { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsCouponVm
    {
        public long CouponId { get; set; }
        public string CouponCode { get; set; }
        public string CouponDescription { get; set; }
        public int UsageType { get; set; }
        public int NoOfTimes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsNeverExpires { get; set; }
        public int DiscountType { get; set; }
        public decimal Discount { get; set; }
        public decimal MinimumPurchaseAmount { get; set; }
        public decimal MaximumDiscountAmount { get; set; }
        public bool ApplyToBasePlan { get; set; }
        public bool ApplyToAddons { get; set; }
        public int OrderNo { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public long CompanyId { get; set; }
        public string Domain { get; set; }
        public long Under { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public long TermLengthId { get; set; }
        public List<ClsCouponTermLengthVm> TermLengths { get; set; }
        public List<ClsCouponAddonVm> Addons { get; set; }
        public decimal SubTotal { get; set; }
        public decimal PlanSubTotal { get; set; }
        public decimal AddonsSubTotal { get; set; }
        public List<long> SelectedAddonIds { get; set; }
        public List<AddonSubTotalVm> AddonSubTotals { get; set; }
        public decimal CalculatedDiscount { get; set; }
    }

    public class AddonSubTotalVm
    {
        public long PlanAddonsId { get; set; }
        public decimal SubTotal { get; set; }
    }

}