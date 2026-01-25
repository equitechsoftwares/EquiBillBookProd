using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblBillOfEntry")]
    public class ClsBillOfEntry
    {
        [Key]
        public long BillOfEntryId { get; set; }
        public long SupplierId { get; set; }
        public long BranchId { get; set; }
        public long PurchaseId { get; set; }
        public string BillOfEntryNo { get; set; }
        public string PortCode { get; set; }
        public DateTime BillOfEntryDate { get; set; }
        public long PaidThrough { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalCustomDuty { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
    }

    public class ClsBillOfEntryVm
    {
        public long BillOfEntryId { get; set; }
        public long SupplierId { get; set; }
        public long BranchId { get; set; }
        public long PurchaseId { get; set; }
        public string BillOfEntryNo { get; set; }
        public string PortCode { get; set; }
        public DateTime BillOfEntryDate { get; set; }
        public long PaidThrough { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalCustomDuty { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string PurchaseInvoiceNo { get; set; }
        public List<ClsBillOfEntryDetailsVm> BillOfEntryDetails { get; set; }
        public string FileExtensionAttachDocument { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Domain { get; set; }
        public string Platform { get; set; }
    }

}