using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{

    public class ClsGstResult
    {
        public List<ClsGstSale> Sales { get; set; }
        public List<ClsGstSale> GroupedSales { get; set; }
    }

    public class ClsGstSale
    {
        public string InvoiceType { get; set; }
        public int IsReverseCharge { get; set; }
        public string CustomerName { get; set; }
        public string StateCode { get; set; }
        public long SalesId { get; set; }
        public DateTime SalesDate { get; set; }
        public string InvoiceNo { get; set; }
        public decimal GrandTotal { get; set; }
        public string PlaceOfSupply { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string ParentInvoiceNo { get; set; }
        public DateTime ParentSalesDate { get; set; }
        public string SalesType { get; set; }
        public string Reason { get; set; }
        public decimal AmountExcTax { get; set; }
        public string PaymentType { get; set; }
        public decimal ParentGrandTotal { get; set; }
        public string PortCode { get; set; }
        public string ShippingBillNo { get; set; }
        public DateTime ShippingBillDate { get; set; }
        public decimal TotalCgstValue { get; set; }
        public decimal TotalSgstValue { get; set; }
        public decimal TotalUtgstValue { get; set; }
        public decimal TotalIgstValue { get; set; }
        public decimal TotalCessValue { get; set; }
        public List<ClsGstSaleDetail> SalesDetails { get; set; }
        public List<ClsGstSaleDetail> NilRated { get; set; }
        public List<ClsGstSaleDetail> Exempted { get; set; }
        public List<ClsGstSaleDetail> NonGst { get; set; }
        public List<ClsGstSaleDetail> GroupedSalesDetails { get; set; }
    }

    public class ClsGstSaleDetail
    {
        public decimal TaxPercent { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal ParentGrandTotal { get; set; }
        public string StateCode { get; set; }
        public decimal GrandTotal { get; set; }
        public string PlaceOfSupply { get; set; }
        public long TaxId { get; set; }
        public decimal AmountRemaining { get; set; }
        public string SupplyType { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
        public long SalesId { get; set; }
        public DateTime SalesDate { get; set; }
        public string InvoiceNo { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public string GstTreatment { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public decimal NilRatedAmount { get; set; }
        public decimal ExemptedAmount { get; set; }
        public decimal NonGstAmount { get; set; }
        public string PaymentType { get; set; }
        public string InvoiceType { get; set; }
        public List<ClsGstTaxType> TaxTypes { get; set; }
    }

    public class ClsGstTaxType
    {
        public long TaxTypeId { get; set; }
        public string TaxType { get; set; }
        public decimal TaxAmount { get; set; }
    }


}