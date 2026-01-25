using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblStockAdjustment")]
    public class ClsStockAdjustment
    {
        [Key]
        public long StockAdjustmentId { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string AdjustmentType { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountRecovered { get; set; }
        public string Notes { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal TotalQuantity { get; set; }
        public string StockDeductionType { get; set; }
        public string InvoiceId { get; set; }
        public long AccountId { get; set; }
        public long PrefixId { get; set; }
        public long StockAdjustmentReasonId { get; set; }
    }

    public class ClsStockAdjustmentVm
    {
        public long StockAdjustmentId { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string AdjustmentType { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountRecovered { get; set; }
        public string Notes { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string UserType { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public decimal TotalQuantity { get; set; }
        public List<ClsStockAdjustmentDetailsVm> StockAdjustmentDetails { get; set; }
        public string Branch { get; set; }
        public string Title { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public bool CheckStockPriceMismatch { get; set; }
        public string CurrencySymbol { get; set; }
        public string StockDeductionType { get; set; }
        public string Domain { get; set; }
        public string InvoiceId { get; set; }
        public long AccountId { get; set; }
        public long PrefixId { get; set; }
        public long StockAdjustmentReasonId { get; set; }
        public string StockAdjustmentReason { get; set; }
        public int TotalItems { get; set; }
    }

}