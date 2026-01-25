using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblItemDetails")]
    public class ClsItemDetails
    {
        [Key]
        public long ItemDetailsId { get; set; }
        public decimal PurchaseExcTax { get; set; }
        public decimal PurchaseIncTax { get; set; }
        public decimal DefaultProfitMargin { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public string ProductImage { get; set; }
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public long VariationId { get; set; }
        public long VariationDetailsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string SKU { get; set; }
        public long ComboItemDetailsId { get; set; }
        public decimal DefaultMrp { get; set; }
        public long SalesAccountId { get; set; }
        public long PurchaseAccountId { get; set; }
        public long InventoryAccountId { get; set; }
        public int PriceAddedFor { get; set; }
        public long PrefixId { get; set; }
    }
    public class ClsItemDetailsVm
    {
        public long ItemDetailsId { get; set; }
        public string ProductType { get; set; }
        public decimal PurchaseExcTax { get; set; }
        public decimal PurchaseIncTax { get; set; }
        public decimal DefaultProfitMargin { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public string ProductImage { get; set; }
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public long VariationId { get; set; }
        public long VariationDetailsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string FileExtensionProductImage { get; set; }
        public string SKU { get; set; }
        public string Variation { get; set; }
        public List<ClsVariationDetailsVm> VariationDetails { get; set; }
        public string VariationName { get; set; }
        public string ItemName { get; set; }
        public long ComboItemDetailsId { get; set; }
        public string UnitName { get; set; }
        public string Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public long TaxId { get; set; }
        public decimal StockValueByPurchasePrice { get; set; }
        public decimal StockValueBySalesPrice { get; set; }
        public decimal PotentialProfit { get; set; }
        public decimal TotalUnitSold { get; set; }
        public decimal TotalUnitTransferred { get; set; }
        public decimal TotalUnitAdjusted { get; set; }
        public decimal UnitCost { get; set; }
        public long BranchId { get; set; }
        public string UnitShortName { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public IList<ClsSellingPriceGroupVm> SellingPriceGroups { get; set; }
        public long SellingPriceGroupId { get; set; }
        public decimal SellingPrice { get; set; }
        public string SecondaryUnitShortName { get; set; }
        public string TertiaryUnitShortName { get; set; }
        public string QuaternaryUnitShortName { get; set; }
        public decimal UToSValue { get; set; }
        public decimal SToTValue { get; set; }
        public decimal TToQValue { get; set; }
        public long UnitId { get; set; }
        public long SecondaryUnitId { get; set; }
        public long TertiaryUnitId { get; set; }
        public long QuaternaryUnitId { get; set; }
        public int PriceAddedFor { get; set; }
        public bool AllowDecimal { get; set; }
        public bool SecondaryUnitAllowDecimal { get; set; }
        public bool TertiaryUnitAllowDecimal { get; set; }
        public bool QuaternaryUnitAllowDecimal { get; set; }
        public bool IsManageStock { get; set; }
        public string TaxType { get; set; }
        public long DivId { get; set; }
        public List<ClsAvailableLots> AvailableLots { get; set; }
        public bool EnableLotNo { get; set; }
        public bool EnableImei { get; set; }
        public long BrandId { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public bool EnableWarranty { get; set; }
        public string Domain { get; set; }
        public string BranchName { get; set; }
        public decimal Discount { get; set; }
        public decimal DefaultMrp { get; set; }
        public string DiscountType { get; set; }
        public long SalesAccountId { get; set; }
        public long PurchaseAccountId { get; set; }
        public long InventoryAccountId { get; set; }
        public long WarrantyId { get; set; }
        //public List<ClsBranchVm> Branchs { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long InterStateTaxId { get; set; }
        public string InterStateTax { get; set; }
        public decimal InterStateTaxPercent { get; set; }
        public long TaxExemptionId { get; set; }
        public long TaxPreferenceId { get; set; }
        public string ItemType { get; set; }
        public long ItemCodeId { get; set; }
        public long PrefixId { get; set; }
        public long SaltId { get; set; }
        public string SaltName { get; set; }
        public decimal QuantityRemaining { get; set; }
        public long PurchaseDetailsId { get; set; }
        public string LotNo { get; set; }
        public long LotId { get; set; }
        public string LotType { get; set; }
        public long SalesDetailsId { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public List<ClsItemDetailsVariationMapVm> AttributeMappings { get; set; }
        public List<ClsItemDetailsVm> Variants { get; set; }
        public string ItemCode { get; set; }
    }
}