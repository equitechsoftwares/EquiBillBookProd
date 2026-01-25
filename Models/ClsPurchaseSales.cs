using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsPurchaseSales
    {
        public long ItemId { get; set; }
        public long ItemDetailId { get; set; }
        public long CategoryId { get; set; }
        public long BrandId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalPurchasePaid { get; set; }
        public decimal TotalPurchaseDue { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalSalesPaid { get; set; }
        public decimal TotalSalesDue { get; set; }
        public long AddedBy { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public decimal TotalSalesQuantity { get; set; }
        public decimal TotalGrossProfit { get; set; }
        public string Title { get; set; }
        public decimal PurchaseIncTax { get; set; }
        public decimal PurchaseReturnIncTax { get; set; }
        public decimal PurchaseDue { get; set; }
        public decimal SalesIncTax { get; set; }
        public decimal SalesReturnIncTax { get; set; }
        public decimal SalesDue { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ClsSalesDetailsVm> SalesDetails { get; set; }
        public List<ClsSalesReturnDetailsVm> SalesReturnDetails { get; set; }
        public List<ClsPurchaseDetailsVm> PurchaseDetails { get; set; }
        public List<ClsPurchaseReturnDetailsVm> PurchaseReturnDetails { get; set; }
        public decimal PurchaseExcTax { get; set; }
        public decimal PurchaseDiscount { get; set; }
        public decimal PurchaseTax { get; set; }
        public decimal PurchasePaid { get; set; }
        public decimal PurchaseReturnExcTax { get; set; }
        public decimal PurchaseReturnDiscount { get; set; }
        public decimal PurchaseReturnTax { get; set; }
        public decimal PurchaseReturnPaid { get; set; }
        public decimal PurchaseReturnDue { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesDiscount { get; set; }
        public decimal SalesTax { get; set; }
        public decimal SalesPaid { get; set; }
        public decimal SalesReturnExcTax { get; set; }
        public decimal SalesReturnDiscount { get; set; }
        public decimal SalesReturnTax { get; set; }
        public decimal SalesReturnPaid { get; set; }
        public decimal SalesReturnDue { get; set; }
        public string VariationName { get; set; }
    }
}