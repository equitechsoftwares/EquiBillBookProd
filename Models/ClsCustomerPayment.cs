using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCustomerPayment")]
    public class ClsCustomerPayment
    {
        [Key]
        public long CustomerPaymentId { get; set; }
        public long CustomerId { get; set; }
        public long BookingId { get; set; }  // 0 for non-booking payments
        public long SalesId { get; set; }
        public long SalesReturnId { get; set; }
        public DateTime PaymentDate { get; set; }
        public long PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string Type { get; set; }
        public long AccountId { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public int IsDebit { get; set; }
        //public string PaymentIds { get; set; }
        public long ParentId { get; set; }
        public string ReferenceId { get; set; }
        public long JournalAccountId { get; set; }
        public decimal AmountRemaining { get; set; }
        public bool IsDirectPayment { get; set; }
        public decimal AmountUsed { get; set; }
        public long PaymentLinkId { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public long TaxId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public long TaxAccountId { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public bool IsCancelled { get; set; }
        public long PrefixId { get; set; }
        public bool IsAdvance { get; set; }
    }

    public class ClsCustomerPaymentVm
    {
        public long CustomerPaymentId { get; set; }
        public long CustomerId { get; set; }
        public long BookingId { get; set; }
        public long SalesId { get; set; }
        public long SalesReturnId { get; set; }
        public DateTime PaymentDate { get; set; }
        public long PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string Type { get; set; }
        public long AccountId { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public int IsDebit { get; set; }
        //public string PaymentIds { get; set; }
        public long ParentId { get; set; }
        public string Domain { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string FileExtensionAttachDocument { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Title { get; set; }
        public string UserType { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public long PaymentId { get; set; }
        public string InvoiceNo { get; set; }
        public decimal GrandTotal { get; set; }
        public string OtherRemarks { get; set; }
        public string PaymentType { get; set; }
        public string AddedByCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SalesReferenceNo { get; set; }
        public long UserGroupId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNo { get; set; }
        public decimal TotalQuantity { get; set; }
        public string SalesReturnReferenceNo { get; set; }
        public long UserId { get; set; }
        public long SupplierPaymentId { get; set; }
        public long AccountsPaymentId { get; set; }
        public string ReferenceId { get; set; }
        public string InvoiceUrl { get; set; }
        public bool IsAdvance { get; set; }
        public long JournalAccountId { get; set; }
        public decimal Due { get; set; }
        public decimal AmountRemaining { get; set; }
        public string ModifiedByCode { get; set; }
        public bool IsDirectPayment { get; set; }
        public string AccountName { get; set; }
        public string ParentReferenceNo { get; set; }
        public decimal AmountUsed { get; set; }
        public DateTime SalesDate { get; set; }
        public long PaymentLinkId { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public long TaxId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public long TaxAccountId { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public List<ClsCustomerPaymentIds> CustomerPaymentIds { get; set; }
        public bool IsActivityLog { get; set; }
        public List<ClsTaxTypeVm> TaxTypes { get; set; }
        public string TaxType { get; set; }
        public bool IsCancelled { get; set; }
        public string SalesReturnInvoiceNo { get; set; }
        public bool IsTaxAccount { get; set; }
        public string StateCode { get; set; }
        public string PlaceOfSupply { get; set; }
        public decimal TaxPercent { get; set; }
        
        // Additional properties for booking payments (derived from Type)
        public bool IsRefund { get; set; }
        public string PaymentStatus { get; set; }
        public long BookingPaymentId { get; set; } // Maps to CustomerPaymentId for backward compatibility
        public long RefundOfPaymentId { get; set; } // Maps to ParentId for backward compatibility
    }

    public class ClsCustomerPaymentIds
    {
        public long CustomerPaymentId { get; set; }
        public long SalesId { get; set; }
        public long BookingId { get; set; }
        public long CustomerId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Due { get; set; }
        public decimal AmountRemaining { get; set; }
        public long PrefixId { get; set; }
        public DateTime SalesDate { get; set; }
        public DateTime BookingDate { get; set; }
        public string InvoiceNo { get; set; }
        public string BookingNo { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal DepositAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; }
        public decimal OpeningBalance { get; set; }
        public long DivId { get; set; }
        public string ReferenceNo { get; set; }
        public long SalesReturnId { get; set; }
        public bool IsCancelled { get; set; }
        public long ParentId { get; set; }
    }

}