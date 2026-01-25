using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblCustomerRewardPoints")]
    public class ClsCustomerRewardPoints
    {
        [Key]
        public long CustomerRewardPointsId { get; set; }
        public long CustomerId { get; set; }
        public long CompanyId { get; set; }
        public decimal TotalPointsEarned { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public decimal AvailablePoints { get; set; }
        public decimal ExpiredPoints { get; set; }
        public DateTime? LastEarnedDate { get; set; }
        public DateTime? LastRedeemedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsCustomerRewardPointsVm
    {
        public long CustomerRewardPointsId { get; set; }
        public long CustomerId { get; set; }
        public long CompanyId { get; set; }
        public decimal TotalPointsEarned { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public decimal AvailablePoints { get; set; }
        public decimal ExpiredPoints { get; set; }
        public DateTime? LastEarnedDate { get; set; }
        public DateTime? LastRedeemedDate { get; set; }
        public string CustomerName { get; set; }
        public string DisplayName { get; set; }
        // For reports
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public long AddedBy { get; set; }
    }

    /// <summary>
    /// ViewModel for Points Summary Report
    /// </summary>
    public class ClsPointsSummaryVm
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPointsEarned { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public decimal ExpiredPoints { get; set; }
        public decimal PointsReversed { get; set; }
        public decimal AvailablePoints { get; set; }
    }

    /// <summary>
    /// ViewModel for Customer Info in Detailed Statement
    /// </summary>
    public class ClsCustomerInfoVm
    {
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
    }
}

