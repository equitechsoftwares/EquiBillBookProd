using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseReturn")]
    public class ClsPurchaseReturn
    {
        [Key]
        public long PurchaseReturnId { get; set; }
        public long PurchaseId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string InvoiceNo { get; set; }
        //public int PayTermNo { get; set; }
        //public int PayTerm { get; set; }
        public string AttachDocument { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long SmsSettingsId { get; set; }
        public long EmailSettingsId { get; set; }
        public long WhatsappSettingsId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public long BranchId { get; set; }
        public long SupplierId { get; set; }
        public int ReturnType { get; set; }
        public DateTime? ExpiredBefore { get; set; }
        public decimal QuantityLessThan { get; set; }
        public bool IsDirectReturn { get; set; }
        public string InvoiceId { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public long AccountId { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public long TaxAccountId { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public long PaymentTermId { get; set; }
        public DateTime DueDate { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public int IsReverseCharge { get; internal set; }
        public bool IsCancelled { get; set; }
        public long PrefixId { get; set; }
        public long PurchaseDebitNoteReasonId { get; set; }
        public decimal NetAmountReverseCharge { get; set; }
        public decimal RoundOffReverseCharge { get; set; }
        public decimal GrandTotalReverseCharge { get; set; }                
        public decimal TaxableAmount { get; set; }
        public decimal SpecialDiscount { get; set; }
        public long SpecialDiscountAccountId { get; set; }
    }

    public class ClsPurchaseReturnVm
    {
        public long PurchaseReturnId { get; set; }
        public long PurchaseId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string InvoiceNo { get; set; }
        //public int PayTermNo { get; set; }
        //public int PayTerm { get; set; }
        public string AttachDocument { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string FileExtensionAttachDocument { get; set; }
        public string FileExtensionShippingDocument { get; set; }
        public List<ClsPurchaseDetailsVm> PurchaseDetails { get; set; }
        public List<ClsPurchaseReturnDetailsVm> PurchaseReturnDetails { get; set; }
        public ClsSupplierPaymentVm Payment { get; set; }
        public string Branch { get; set; }
        public string PurchaseInvoiceNo { get; set; }
        public string SupplierName { get; set; }
        public string SupplierMobileNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseReturnAmount { get; set; }
        public decimal PurchaseReturnQuantity { get; set; }
        public long PurchaseReturnDetailsId { get; set; }
        public long BranchId { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public string Title { get; set; }
        public string UserType { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string InvoiceUrl { get; set; }
        public string CurrencySymbol { get; set; }
        public long SmsSettingsId { get; set; }
        public long EmailSettingsId { get; set; }
        public long WhatsappSettingsId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public int ReturnType { get; set; }
        public long SupplierId { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime? ExpiredBefore { get; set; }
        public decimal QuantityLessThan { get; set; }
        public bool IsDirectReturn { get; set; }
        public string InvoiceId { get; set; }
        public string Domain { get; set; }
        public decimal PaidQuantity { get; set; }
        public decimal FreeQuantity { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public long AccountId { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public long TaxAccountId { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public long PaymentTermId { get; set; }
        public DateTime DueDate { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public long SupplierPaymentId { get; set; }
        public int IsReverseCharge { get; set; }
        public long CountryId { get; set; }
        public bool IsCancelled { get; set; }
        public long PrefixId { get; set; }
        public decimal AmountRemaining { get; set; }
        public long PurchaseDebitNoteReasonId { get; set; }
        public int TotalItems { get; set; }
        public decimal NetAmountReverseCharge { get; set; }
        public decimal RoundOffReverseCharge { get; set; }
        public decimal GrandTotalReverseCharge { get; set; }
        public string ItemCode { get; set; }                
        public decimal TaxableAmount { get; set; }
        public List<ClsPurchaseReturnAdditionalChargesVm> PurchaseReturnAdditionalCharges { get; set; }
        public decimal SpecialDiscount { get; set; }
        public long SpecialDiscountAccountId { get; set; }
    }

}