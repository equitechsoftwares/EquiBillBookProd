using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPosSettings")]
    public class ClsPosSettings
    {
        [Key]
        public long PosSettingsId { get; set; }
        public bool DisableDraft { get; set; }
        public bool DisableQuotation { get; set; }
        public bool DisableProforma { get; set; }
        public bool DisableCreditSale { get; set; }
        public bool DisableHold { get; set; }
        public bool DisableMultiplePay { get; set; }       
        public bool DisableExpressCheckout { get; set; }
        public bool DontShowProductSuggestion { get; set; }
        public bool DontShowRecentTransactions { get; set; }
        public bool DisableDicount { get; set; }
        public bool DisableOrderTax { get; set; }
        public bool SubTotalEditable { get; set; }
        public bool EnableTransactionDate { get; set; }
        public bool EnableServiceStaff { get; set; }
        public bool IsServiceStaffRequired { get; set; }
        public bool EnableWeighingScale { get; set; }
        public bool ShowInvoiceScheme { get; set; }
        public bool ShowInvoiceLayoutDropdown { get; set; }
        public bool PrintInvoiceOnHold { get; set; }
        public bool ShowPricingOnProductSuggestionTooltip { get; set; }
        public string WeighingScaleBarcodePrefix { get; set; }
        public int ProductSkuLength { get; set; }
        public int QuantityIntegerPartLength { get; set; }
        public int QuantityFractionalPartLength { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool AllowOnlinePayment { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableWhatsapp { get; set; }
        public bool EnableNotes { get; set; }
        public bool EnableFreeQuantity { get; set; }
        public bool SalePriceIsMinSellingPrice { get; set; }
        public string Draft { get; set; }
        public string Quotation { get; set; }
        public string Proforma { get; set; }
        public string CreditSale { get; set; }
        public string Multiple { get; set; }
        public string Hold { get; set; }
        public string Cancel { get; set; }
        public string GoToProductQuantity { get; set; }
        public string WeighingScale { get; set; }
        public string EditDiscount { get; set; }
        public string EditOrderTax { get; set; }
        public string EditShippingCharge { get; set; }
        public string EditPackagingCharge { get; set; }
        public string AddPaymentRow { get; set; }
        public string FinalisePayment { get; set; }
        public string AddNewProduct { get; set; }
        public string RecentTransactions { get; set; }
        public string HoldList { get; set; }
        public string Calculator { get; set; }
        public string FullScreen { get; set; }
        public string RegisterDetails { get; set; }
        public string PosExit { get; set; }
        public bool EnableRoundOff { get; set; }
        public int InvoiceType { get; set; }
        public bool AutoPrintInvoiceFinal { get; set; }
        public bool AutoPrintInvoiceSalesReturn { get; set; }
        public bool EnableSpecialDiscount { get; set; }
        public string EditSpecialDiscount { get; set; }
        public bool EnablePlaceOfSupply { get; set; }
        public bool EnableSellingPriceGroup { get; set; }
        public bool ShowItemImage { get; set; }
        public bool ShowItemSellingPrice { get; set; }
        public bool ShowItemMrp { get; set; }
        // Restaurant/KOT Settings
        public bool EnableKot { get; set; }
        public bool EnableTableBooking { get; set; }
        public bool AutoCreateKot { get; set; }
        public bool AllowLinkExistingKot { get; set; }
    }

    public class ClsPosSettingsVm
    {
        public long PosSettingsId { get; set; }
        public bool DisableDraft { get; set; }
        public bool DisableQuotation { get; set; }
        public bool DisableProforma { get; set; }
        public bool DisableCreditSale { get; set; }
        public bool DisableHold { get; set; }
        public bool DisableMultiplePay { get; set; }
        public bool DisableExpressCheckout { get; set; }
        public bool DontShowProductSuggestion { get; set; }
        public bool DontShowRecentTransactions { get; set; }
        public bool DisableDicount { get; set; }
        public bool DisableOrderTax { get; set; }
        public bool SubTotalEditable { get; set; }
        public bool EnableTransactionDate { get; set; }
        public bool EnableServiceStaff { get; set; }
        public bool IsServiceStaffRequired { get; set; }
        public bool EnableWeighingScale { get; set; }
        public bool ShowInvoiceScheme { get; set; }
        public bool ShowInvoiceLayoutDropdown { get; set; }
        public bool PrintInvoiceOnHold { get; set; }
        public bool ShowPricingOnProductSuggestionTooltip { get; set; }
        public string WeighingScaleBarcodePrefix { get; set; }
        public int ProductSkuLength { get; set; }
        public int QuantityIntegerPartLength { get; set; }
        public int QuantityFractionalPartLength { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public bool AllowOnlinePayment { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableWhatsapp { get; set; }
        public bool EnableNotes { get; set; }
        public bool EnableFreeQuantity { get; set; }
        public bool SalePriceIsMinSellingPrice { get; set; }
        public string Draft { get; set; }
        public string Quotation { get; set; }
        public string Proforma { get; set; }
        public string CreditSale { get; set; }
        public string Multiple { get; set; }
        public string Hold { get; set; }
        public string Cancel { get; set; }
        public string GoToProductQuantity { get; set; }
        public string WeighingScale { get; set; }
        public string EditDiscount { get; set; }
        public string EditOrderTax { get; set; }
        public string EditShippingCharge { get; set; }
        public string EditPackagingCharge { get; set; }
        public string AddPaymentRow { get; set; }
        public string FinalisePayment { get; set; }
        public string AddNewProduct { get; set; }
        public List<ClsPaymentTypeVm> PaymentTypes { get; set; }
        public List<ClsAdditionalChargeVm> AdditionalCharges { get; set; }
        public string Domain { get; set; }
        public string RecentTransactions { get; set; }
        public string HoldList { get; set; }
        public string Calculator { get; set; }
        public string FullScreen { get; set; }
        public string RegisterDetails { get; set; }
        public string PosExit { get; set; }
        public bool EnableRoundOff { get; set; }
        public int InvoiceType { get; set; }
        public bool AutoPrintInvoiceFinal { get; set; }
        public bool AutoPrintInvoiceSalesReturn { get; set; }
        public bool EnableSpecialDiscount { get; set; }
        public string EditSpecialDiscount { get; set; }
        public bool EnablePlaceOfSupply { get; set; }
        public bool EnableSellingPriceGroup { get; set; }
        public bool ShowItemImage { get; set; }
        public bool ShowItemSellingPrice { get; set; }
        public bool ShowItemMrp { get; set; }
        // Restaurant/KOT Settings
        public bool EnableKot { get; set; }
        public bool EnableTableBooking { get; set; }
        public bool AutoCreateKot { get; set; }
        public bool AllowLinkExistingKot { get; set; }
    }

}