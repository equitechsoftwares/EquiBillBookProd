using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRewardPointTransaction")]
    public class ClsRewardPointTransaction
    {
        [Key]
        public long RewardPointTransactionId { get; set; }
        public long CustomerId { get; set; }
        public long SalesId { get; set; }
        public long SalesReturnId { get; set; }
        public string TransactionType { get; set; } // "Earn", "Redeem", "Expire", "Reverse"
        public decimal Points { get; set; }
        public decimal OrderAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public long CompanyId { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsRewardPointTransactionVm
    {
        public long RewardPointTransactionId { get; set; }
        public long CustomerId { get; set; }
        public long SalesId { get; set; }
        public long SalesReturnId { get; set; }
        public string TransactionType { get; set; }
        public decimal Points { get; set; }
        public decimal OrderAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNo { get; set; }
        public string Notes { get; set; }
        // For reports
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public long AddedBy { get; set; }
        public long CompanyId { get; set; }
        public int ExpiryPeriod { get; set; }
    }
}

