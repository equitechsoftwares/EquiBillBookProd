using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblItem")]
    public class ClsItem
    {
        [Key]
        public long ItemId { get; set; }
        public string ItemType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public string SkuCode { get; set; }
        public string BarcodeType { get; set; }
        public long UnitId { get; set; }
        public long SecondaryUnitId { get; set; }
        public long TertiaryUnitId { get; set; }
        public long QuaternaryUnitId { get; set; }
        public decimal UToSValue { get; set; }
        public decimal SToTValue { get; set; }
        public decimal TToQValue { get; set; }
        public long BrandId { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public string BranchIds { get; set; }
        public bool IsManageStock { get; set; }
        public decimal AlertQuantity { get; set; }
        public string ProductImage { get; set; }
        public string ProductBrochure { get; set; }
        public long TaxId { get; set; }
        public string TaxType { get; set; }
        public string ProductType { get; set; }
        
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int ExpiryPeriod { get; set; }
        public string ExpiryPeriodType { get; set; }
        public long WarrantyId { get; set; }
        public int PriceAddedFor { get; set; }
        public bool EnableImei { get; set; }
        public string BatchNo { get; set; }
        public long ItemCodeId { get; set; }
        public long InterStateTaxId { get; set; }
        public long TaxExemptionId { get; set; }
        public long TaxPreferenceId { get; set; }
        public long PrefixId { get; set; }
        public long SaltId { get; set; }
    }
    public class ClsItemVm
    {
        public long ItemId { get; set; }
        public string ItemType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public string SkuCode { get; set; }
        public string HsnCode { get; set; }
        public string BarcodeType { get; set; }
        public long UnitId { get; set; }
        public long SecondaryUnitId { get; set; }
        public long TertiaryUnitId { get; set; }
        public long QuaternaryUnitId { get; set; }
        public decimal UToSValue { get; set; }
        public decimal SToTValue { get; set; }
        public decimal TToQValue { get; set; }
        public long BrandId { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public string BranchIds { get; set; }
        public bool IsManageStock { get; set; }
        public decimal AlertQuantity { get; set; }
        public string ProductImage { get; set; }
        public string ProductBrochure { get; set; }
        public long TaxId { get; set; }
        public string TaxType { get; set; }
        public string ProductType { get; set; }
        
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
        public List<ClsItemDetailsVm> ItemDetails { get; set; }
        public string FileExtensionProductImage { get; set; }
        public string FileExtensionProductBrochure { get; set; }
        public string Category { get; set; }
        //public string[] Branchs { get; set; }
        public List<ClsItemBranchMapVm> ItemBranchMaps { get; set; }
        public List<ClsBranchVm> Branchs { get; set; }
        public string Type { get; set; }
        public string VariationDetails { get; set; }
        public string UnitName { get; set; }
        public decimal TotalCost { get; set; }
        public string Tax { get; set; }
        public string TaxPercent { get; set; }
        public long BranchId { get; set; }
        public string SubCategory { get; set; }
        public string SubSubCategory { get; set; }
        public string Brand { get; set; }
        public string Title { get; set; }
        public string[] ItemsArray { get; set; }
        public string VariationName { get; set; }
        public long ItemDetailsId { get; set; }
        public string SKU { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int ExpiryPeriod { get; set; }
        public string ExpiryPeriodType { get; set; }
        public decimal TotalSales { get; set; }
        public decimal Quantity { get; set; }
        public string LotNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalSalesQuantity { get; set; }
        public decimal TotalGrossProfit { get; set; }
        public DateTime Date { get; set; }
        public IList<ClsOpeningStockVm> OpeningStocks { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public long WarrantyId { get; set; }
        public long CustomerId { get; set; }
        public long SellingPriceGroupId { get; set; }
        public string SecondaryUnitName { get; set; }
        public string TertiaryUnitName { get; set; }
        public string QuaternaryUnitName { get; set; }
        public int PriceAddedFor { get; set; }
        public string Warranty { get; set; }
        public string MenuType { get; set; }
        public List<ClsItemImport> ItemImports { get; set; }
        public bool CheckStockPriceMismatch { get; set; }
        public bool EnableImei { get; set; }
        public string Domain { get; set; }
        public string BatchNo { get; set; }
        public long UserId { get; set; }
        public long ItemCodeId { get; set; }
        public long InterStateTaxId { get; set; }
        public long TaxExemptionId { get; set; }
        public long TaxPreferenceId { get; set; }
        public string TaxPreference { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public long SupplierId { get; set; }
        public string TaxExemptionType { get; set; }
        public bool IsBillOfSupply { get; set; }
        public long PrefixId { get; set; }
        public long SaltId { get; set; }
        public string ItemCodeType { get; set; }
        public string UnitShortName { get; set; }
        public string SecondaryUnitShortName { get; set; }
        public string TertiaryUnitShortName { get; set; }
        public string QuaternaryUnitShortName { get; set; }
    }

    public class ClsItemImport
    {
        public long ItemId { get; set; }
        public string ItemType { get; set; }
        public string SkuCode { get; set; }
        public string HsnSacCode { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public string SecondaryUnitName { get; set; }
        public decimal UToSValue { get; set; }
        public string TertiaryUnitName { get; set; }
        public decimal SToTValue { get; set; }
        public string QuaternaryUnitName { get; set; }
        public decimal TToQValue { get; set; }
        public string BarcodeType { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubSubCategoryName { get; set; }
        public bool IsManageStock { get; set; }
        public decimal AlertQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int ExpiryPeriod { get; set; }
        public string ExpiryPeriodType { get; set; }
        public string WarrantyName { get; set; }

        [AllowHtml]
        public string Description { get; set; }
        public string BranchNames { get; set; }
        public string Rack { get; set; }
        public string Row { get; set; }
        public string Position { get; set; }
        public string TaxName { get; set; }
        public string IntraStateTaxName { get; set; }
        public string InterStateTaxName { get; set; }
        public string TaxType { get; set; }
        public string ProductType { get; set; }
        public int PriceAddedFor { get; set; }
        public string VariationName { get; set; }
        public string VariationValues { get; set; }
        public string VariationSKUs { get; set; }
        public string PurchasePrice { get; set; }
        public string ProfitMargin { get; set; }
        public string SellingPrice { get; set; }
        //public string Category { get; set; }
        //public string SubCategory { get; set; }
        //public string SubSubCategory { get; set; }
        public string MRP { get; set; }
        public int RowNo { get; set; }
        public string VariationGroupName { get; set; }
        public bool EnableImei { get; set; }
        public string InventoryAccount { get; set; }
        public string PurchaseAccount { get; set; }
        public string SalesAccount { get; set; }
        public string TaxPreference { get; set; }
        public string TaxExemptionReason { get; set; }
        public int WarrantyDuration    { get; set; }
        public string WarrantyDurationType { get; set; }
        public string SaltName { get; set; }
    }

}