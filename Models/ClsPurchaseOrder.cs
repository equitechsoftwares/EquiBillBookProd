using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseOrder")]
    public class ClsPurchaseOrder
    {
        [Key]
        public long PurchaseOrderId { get; set; }
        public long SupplierId { get; set; }
        public DateTime PurchaseOrderDate { get; set; }
        public string ReferenceNo { get; set; }
        public long BranchId { get; set; }
        public int PayTermNo { get; set; }
        public int PayTerm { get; set; }
        public decimal Subtotal { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal GrandTotal { get; set; }
        public string ShippingDetails { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingStatus { get; set; }
        public string DeliveredTo { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal ExchangeRate { get; set; }
        public long SmsSettingsId { get; set; }
        public long EmailSettingsId { get; set; }
        public long WhatsappSettingsId { get; set; }
        public string InvoiceId { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public long ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public int IsReverseCharge { get; set; }
        public long PrefixId { get; set; }
        public decimal NetAmountReverseCharge { get; set; }
        public decimal RoundOffReverseCharge { get; set; }
        public decimal GrandTotalReverseCharge { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal SpecialDiscount { get; set; }

    }
    public class ClsPurchaseOrderVm
    {
        public long PurchaseOrderId { get; set; }
        public long SupplierId { get; set; }
        public DateTime PurchaseOrderDate { get; set; }
        public string ReferenceNo { get; set; }
        public long BranchId { get; set; }
        public int PayTermNo { get; set; }
        public int PayTerm { get; set; }
        public decimal Subtotal { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal GrandTotal { get; set; }
        public string ShippingDetails { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingStatus { get; set; }
        public string DeliveredTo { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<ClsPurchaseOrderDetailsVm> PurchaseOrderDetails { get; set; }
        public List<ClsPurchaseDetailsVm> PurchaseDetails { get; set; }
        public List<ClsSalesOrderDetailsVm> SalesOrderDetails { get; set; }
        public List<ClsPurchaseQuotationDetailsVm> PurchaseQuotationDetails { get; set; }
        public List<ClsSalesOrderAdditionalChargesVm> SalesOrderAdditionalCharges { get; set; }
        public List<ClsPurchaseQuotationAdditionalChargesVm> PurchaseQuotationAdditionalCharges { get; set; }
        public List<ClsPurchaseAdditionalChargesVm> PurchaseAdditionalCharges { get; set; }
        public string Search { get; set; }
        public string UserType { get; set; }
        public string SupplierName { get; set; }
        public ClsSupplierPaymentVm Payment { get; set; }
        public string AttachDocument { get; set; }
        public string FileExtensionAttachDocument { get; set; }
        public decimal TotalDiscount { get; set; }
        public string Title { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public bool IsPurchaseReturn { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public decimal PurchaseReturnDue { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string InvoiceUrl { get; set; }
        public ClsBusinessSettingsVm BusinessSetting { get; set; }
        public ClsBranchVm Branch { get; set; }
        //public List<ClsAddress> Addresses { get; set; }
        public string BranchName { get; set; }
        public List<ClsSupplierPaymentVm> Payments { get; set; }
        public ClsUserVm User { get; set; }
        public bool CheckStockPriceMismatch { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CurrencySymbol { get; set; }
        public string DefaultCurrencySymbol { get; set; }
        public long SmsSettingsId { get; set; }
        public long EmailSettingsId { get; set; }
        public long WhatsappSettingsId { get; set; }
        public string TaxNo { get; set; }
        public string PaymentType { get; set; }
        public List<ClsTaxVm> Taxs { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public long BrandId { get; set; }
        public bool IsAdvance { get; set; }
        public DateTime ExpiredBefore { get; set; }
        public decimal QuantityLessThan { get; set; }
        public string ItemCode { get; set; }
        public DateTime Date { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceId { get; set; }
        public string Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public string Domain { get; set; }
        public decimal PaidQuantity { get; set; }
        public decimal FreeQuantity { get; set; }
        public string SupplierMobileNo { get; set; }
        public int PriceAddedFor { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public string Type { get; set; }
        public long ItemDetailsId { get; set; }
        public string Page { get; set; }
        public long PurchaseQuotationId { get; set; }
        public long SalesOrderId { get; set; }
        public long ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string Reference { get; set; }
        public bool CanEdit { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public long CountryId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public int IsReverseCharge { get; set; }
        public long PrefixId { get; set; }
        public int TotalItems { get; set; }
        public decimal NetAmountReverseCharge { get; set; }
        public decimal RoundOffReverseCharge { get; set; }
        public decimal GrandTotalReverseCharge { get; set; }
        public decimal TaxableAmount { get; set; }
        public List<ClsPurchaseOrderAdditionalChargesVm> PurchaseOrderAdditionalCharges { get; set; }
        public decimal SpecialDiscount { get; set; }
    }
}