using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesReturn")]
    public class ClsSalesReturn
    {
        [Key]
        public long SalesReturnId { get; set; }
        public long SalesId { get; set; }
        public long BranchId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string InvoiceNo { get; set; }
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
        public string InvoiceId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public long AccountId { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public long TaxAccountId { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public long PaymentTermId { get; set; }
        public DateTime DueDate { get; set; }
        public long PlaceOfSupplyId { get; set; }
        //public string Reason { get; set; }
        public long CustomerId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public bool IsCancelled { get; set; }
        public long PrefixId { get; set; }
        public long SalesCreditNoteReasonId { get; set; }               
        public decimal NetAmountReverseCharge { get; set; }
        public decimal RoundOffReverseCharge { get; set; }
        public decimal GrandTotalReverseCharge { get; set; }
        public decimal TaxableAmount { get; set; }
        public int IsReverseCharge { get; set; }
        public int PayTaxForExport { get; set; }
        public int TaxCollectedFromCustomer { get; set; }
        public decimal SpecialDiscount { get; set; }
        public long SpecialDiscountAccountId { get; set; }
        [AllowHtml]
        public string Terms { get; set; }
    }

    public class ClsSalesReturnVm
    {
        public long SalesReturnId { get; set; }
        public long SalesId { get; set; }
        public long BranchId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string InvoiceNo { get; set; }
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
        public List<ClsSalesDetailsVm> SalesDetails { get; set; }
        public List<ClsSalesReturnDetailsVm> SalesReturnDetails { get; set; }
        public ClsCustomerPaymentVm Payment { get; set; }
        public string Branch { get; set; }
        public string SalesInvoiceNo { get; set; }
        //public string Customer { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNo { get; set; }
        public DateTime SalesDate { get; set; }
        public decimal SalesReturnAmount { get; set; }
        public decimal SalesReturnQuantity { get; set; }
        public long SalesReturnDetailsId { get; set; }
        public string SalesType { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string InvoiceUrl { get; set; }
        public ClsBusinessSettingsVm BusinessSetting { get; set; }
        //public List<ClsAddress> Addresses { get; set; }
        public string BranchName { get; set; }
        public bool IsShippingAddressDifferent { get; set; }
        public ClsUserVm User { get; set; }
        public string CurrencySymbol { get; set; }
        public long SmsSettingsId { get; set; }
        public long EmailSettingsId { get; set; }
        public long WhatsappSettingsId { get; set; }
        public decimal SalesReturnUnitCost { get; set; }
        public long CustomerId { get; set; }
        public string InvoiceId { get; set; }
        public string Domain { get; set; }
        public decimal PaidQuantity { get; set; }
        public decimal FreeQuantity { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public long AccountId { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public long TaxAccountId { get; set; }
        public string Type { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public long PaymentTermId { get; set; }
        public DateTime DueDate { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public string Reason { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public long CustomerPaymentId { get; set; }
        public bool IsCancelled { get; set; }
        public long PrefixId { get; set; }
        public long BranchStateId { get; set; }
        public int IsReverseCharge { get; set; }
        public string UserType { get; set; }
        public decimal AmountRemaining { get; set; }
        public long SalesCreditNoteReasonId { get; set; }
        public int TotalItems { get; set; }
        public string ItemCode { get; set; }                
        public decimal NetAmountReverseCharge { get; set; }
        public decimal RoundOffReverseCharge { get; set; }
        public decimal GrandTotalReverseCharge { get; set; }
        public decimal TaxableAmount { get; set; }
        public int PayTaxForExport { get; set; }
        public int TaxCollectedFromCustomer { get; set; }
        public List<ClsSalesReturnAdditionalChargesVm> SalesReturnAdditionalCharges { get; set; }
        public decimal SpecialDiscount { get; set; }
        public long SpecialDiscountAccountId { get; set; }
        [AllowHtml]
        public string Terms { get; set; }
        public decimal PointsEarned { get; set; }
        public decimal RedeemPoints { get; set; }
        public decimal OriginalSaleGrandTotal { get; set; }
    }

}