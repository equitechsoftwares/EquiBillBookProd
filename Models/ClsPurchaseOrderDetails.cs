using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseOrderDetails")]
    public class ClsPurchaseOrderDetails
    {
        [Key]
        public long PurchaseOrderDetailsId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchaseExcTax { get; set; }
        public decimal PurchaseIncTax { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal UnitCost { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal AmountIncTax { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal QuantitySold { get; set; }
        public string LotNo { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int PriceAddedFor { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public decimal FreeQuantity { get; set; }
        public bool IsStopSelling { get; set; }
        public decimal DefaultProfitMargin { get; set; }
        public int UnitAddedFor { get; set; }
        public decimal QuantityPurchased { get; set; }
        public decimal Mrp { get; set; }
        public decimal ExtraDiscount { get; set; }
        public string ITCType { get; set; }
        public long TaxExemptionId { get; set; }
        public long ItemCodeId { get; set; }
        public decimal TotalTaxAmount { get; set; }
    }
    public class ClsPurchaseOrderDetailsVm
    {
        public long PurchaseOrderDetailsId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchaseExcTax { get; set; }
        public decimal PurchaseIncTax { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal UnitCost { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal AmountIncTax { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Tax { get; set; }
        public string ItemName { get; set; }
        public string VariationName { get; set; }
        public string SKU { get; set; }
        public decimal TaxPercent { get; set; }
        public long PurchaseReturnDetailsId { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal PurchaseReturnAmount { get; set; }
        public decimal PurchaseReturnPrice { get; set; }
        public decimal QuantitySold { get; set; }
        public string ProductType { get; set; }
        public string LotNo { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string UnitShortName { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public int PriceAddedFor { get; set; }
        public long UnitId { get; set; }
        public long SecondaryUnitId { get; set; }
        public long TertiaryUnitId { get; set; }
        public long QuaternaryUnitId { get; set; }
        public string SecondaryUnitShortName { get; set; }
        public string TertiaryUnitShortName { get; set; }
        public string QuaternaryUnitShortName { get; set; }
        public decimal UToSValue { get; set; }
        public decimal SToTValue { get; set; }
        public decimal TToQValue { get; set; }
        public bool AllowDecimal { get; set; }
        public bool SecondaryUnitAllowDecimal { get; set; }
        public bool TertiaryUnitAllowDecimal { get; set; }
        public bool QuaternaryUnitAllowDecimal { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public decimal FreeQuantity { get; set; }
        public bool IsStopSelling { get; set; }
        public decimal DefaultProfitMargin { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public long BrandId { get; set; }
        public int QuantityReturnedPriceAddedFor { get; set; }
        public decimal FreeQuantityReturned { get; set; }
        public int FreeQuantityReturnedPriceAddedFor { get; set; }
        public decimal PurchaseReturnUnitCost { get; set; }
        public long BranchId { get; set; }
        public long SupplierId { get; set; }
        public string UnitName { get; set; }
        public decimal TotalCost { get; set; }
        public int UnitAddedFor { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
        public string Domain { get; set; }
        public decimal AdjustedQuantity { get; set; }
        public decimal QuantityPurchased { get; set; }
        public decimal Mrp { get; set; }
        public decimal ReturnedAmountIncTax { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal AdjustedQuantityDebit { get; set; }
        public decimal AdjustedQuantityCredit { get; set; }
        public int DivId { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal ExtraDiscount { get; set; }
        public long PurchaseDetailsId { get; set; }
        public long SalesOrderDetailsId { get; set; }
        public long PurchaseQuotationDetailsId { get; set; }
        public string ITCType { get; set; }
        public long TaxExemptionId { get; set; }
        public long ItemCodeId { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public string ProductImage { get; set; }
    }
}