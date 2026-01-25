using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseSettings")]
    public class ClsPurchaseSettings
    {
        [Key]
        public long PurchaseSettingsId { get; set; }
        public bool EnableEditingProductPrice { get; set; }
        public bool EnablePurchaseStatus { get; set; }
        public bool EnablePurchaseQuotation { get; set; }
        public bool EnablePurchaseOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableWhatsapp { get; set; }
        public bool EnableFreeQuantity { get; set; }
        public bool EnableSpecialDiscount { get; set; }
        public bool EnableRoundOff { get; set; }
        public bool AutoPrintInvoicePurchaseQuotation { get; set; }
        public bool AutoPrintInvoicePurchaseOrder { get; set; }
        public bool AutoPrintInvoicePurchaseBill { get; set; }
        public bool AutoPrintInvoicePurchaseReturn { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public long PaymentTermId { get; set; }
        public long SpecialDiscountAccountId { get; set; }
    }

    public class ClsPurchaseSettingsVm
    {
        public long PurchaseSettingsId { get; set; }
        public bool EnableEditingProductPrice { get; set; }
        public bool EnablePurchaseStatus { get; set; }
        public bool EnablePurchaseQuotation { get; set; }
        public bool EnablePurchaseOrder { get; set; }
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
        public bool EnableSms { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableWhatsapp { get; set; }
        public bool EnableFreeQuantity { get; set; }
        public string Domain { get; set; }
        public bool EnableSpecialDiscount { get; set; }
        public bool EnableRoundOff { get; set; }
        public bool AutoPrintInvoicePurchaseQuotation { get; set; }
        public bool AutoPrintInvoicePurchaseOrder { get; set; }
        public bool AutoPrintInvoicePurchaseBill { get; set; }
        public bool AutoPrintInvoicePurchaseReturn { get; set; }
        public long DiscountAccountId { get; set; }
        public long RoundOffAccountId { get; set; }
        public long PaymentTermId { get; set; }
        public long SpecialDiscountAccountId { get; set; }
    }

}