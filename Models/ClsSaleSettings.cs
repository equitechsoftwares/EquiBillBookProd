using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblSaleSettings")]
    public class ClsSaleSettings
    {
        [Key]
        public long SaleSettingsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal DefaultSaleDiscount { get; set; }
        public string DiscountType { get; set; }
        public long DefaultSaleTaxId { get; set; }
        public bool SalePriceIsMinSellingPrice { get; set; }
        public bool AllowOverSelling { get; set; }
        public bool IsPayTermRequired { get; set; }
        public int SalesCommissionAgent { get; set; }
        public int CommissionCalculationType { get; set; }
        public bool IsCommissionAgentRequired { get; set; }
        public bool AllowOnlinePayment { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableWhatsapp { get; set; }
        public bool EnableFreeQuantity { get; set; }
        public bool EnableSpecialDiscount { get; set; }
        public bool EnableRoundOff { get; set; }
        public int InvoiceType { get; set; }
        public bool AutoPrintInvoiceQuotation { get; set; }
        public bool AutoPrintInvoiceOrder { get; set; }
        public bool AutoPrintInvoiceDeliveryChallan { get; set; }
        public bool AutoPrintInvoiceProforma { get; set; }
        public bool AutoPrintInvoiceBill { get; set; }
        public bool AutoPrintInvoiceSalesReturn { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public bool EnableCustomerGroup { get; set; }
        public bool EnableSalesQuotation { get; set; }
        public bool EnableSalesOrder { get; set; }
        public bool EnableSalesProforma { get; set; }
        public bool EnableDeliveryChallan { get; set; }
        public long PaymentTermId { get; set; }
        public bool EnablePos { get; set; }
        public long SpecialDiscountAccountId { get; set; }
        public bool EnableNotes { get; set; }
        public bool EnableTerms { get; set; }
        public bool EnableRecurringSales { get; set; }
        [AllowHtml]
        public string DefaultNotes { get; set; }
        [AllowHtml]
        public string DefaultTerms { get; set; }
    }

    public class ClsSaleSettingsVm
    {
        public long SaleSettingsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal DefaultSaleDiscount { get; set; }
        public string DiscountType { get; set; }
        public long DefaultSaleTaxId { get; set; }
        public bool SalePriceIsMinSellingPrice { get; set; }
        public bool AllowOverSelling { get; set; }
        public bool IsPayTermRequired { get; set; }
        public int SalesCommissionAgent { get; set; }
        public int CommissionCalculationType { get; set; }
        public bool IsCommissionAgentRequired { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public bool AllowOnlinePayment { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableWhatsapp { get; set; }
        public bool EnableFreeQuantity { get; set; }
        public string Domain { get; set; }
        public bool EnableSpecialDiscount { get; set; }
        public bool EnableRoundOff { get; set; }
        public int InvoiceType { get; set; }
        public bool AutoPrintInvoiceQuotation { get; set; }
        public bool AutoPrintInvoiceOrder { get; set; }
        public bool AutoPrintInvoiceDeliveryChallan { get; set; }
        public bool AutoPrintInvoiceProforma { get; set; }
        public bool AutoPrintInvoiceBill { get; set; }
        public bool AutoPrintInvoiceSalesReturn { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public bool EnableCustomerGroup { get; set; }
        public bool EnableSalesQuotation { get; set; }
        public bool EnableSalesOrder { get; set; }
        public bool EnableSalesProforma { get; set; }
        public bool EnableDeliveryChallan { get; set; }
        public long PaymentTermId { get; set; }
        public bool EnablePos { get; set; }
        public long SpecialDiscountAccountId { get; set; }
        public bool EnableNotes { get; set; }
        public bool EnableTerms { get; set; }
        public bool EnableRecurringSales { get; set; }
        [AllowHtml]
        public string DefaultNotes { get; set; }
        [AllowHtml]
        public string DefaultTerms { get; set; }
    }

}