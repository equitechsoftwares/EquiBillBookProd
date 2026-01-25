using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblStockTransfer")]
    public class ClsStockTransfer
    {
        [Key]
        public long StockTransferId { get; set; }
        public long FromBranchId { get; set; }
        public long ToBranchId { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        
        public string Notes { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        
        public decimal Subtotal { get; set; }
        public string InvoiceId { get; set; }
        public long StockTransferReasonId { get; set; }
    }

    public class ClsStockTransferVm
    {
        public long StockTransferId { get; set; }
        public long FromBranchId { get; set; }
        public string FromBranch { get; set; }
        public long ToBranchId { get; set; }
        public string ToBranch { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        
        public string Notes { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public List<ClsStockTransferDetailsVm> StockTransferDetails { get; set; }
        public string UserType { get; set; }
        public string Title { get; set; }
        public long BranchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        
        public string InvoiceUrl { get; set; }
        public ClsBranchVm FromBranchDetails { get; set; }
        public ClsBranchVm ToBranchDetails { get; set; }
        public decimal Subtotal { get; set; }
        public string InvoiceId { get; set; }
        public string Domain { get; set; }
        public long StockTransferReasonId { get; set; }
        public string StockTransferReason { get; set; }
        public int TotalItems { get; set; }
    }

}