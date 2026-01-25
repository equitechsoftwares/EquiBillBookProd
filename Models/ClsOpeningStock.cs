using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblOpeningStock")]
    public class ClsOpeningStock
    {
        [Key]
        public long OpeningStockId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal SubTotal { get; set; }
        public long BranchId { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal QuantitySold { get; set; }
        public int PriceAddedFor { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public string LotNo { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsStopSelling { get; set; }
        public int UnitAddedFor { get; set; }
        public decimal Mrp { get; set; }
        public long AccountId { get; set; }
        public long JournalAccountId { get; set; }
        public decimal QuantityPurchased { get; set; }
        public decimal DefaultProfitMargin { get; set; }
    }

    public class ClsOpeningStockVm
    {
        public long OpeningStockId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal SubTotal { get; set; }
        public long BranchId { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal QuantitySold { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string ItemName { get; set; }
        public string VariationName { get; set; }
        public string SKU { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public int PriceAddedFor { get; set; }
        public long UnitId { get; set; }
        public long SecondaryUnitId { get; set; }
        public long TertiaryUnitId { get; set; }
        public long QuaternaryUnitId { get; set; }
        public string UnitShortName { get; set; }
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
        public decimal PurchaseExcTax { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public string LotNo { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsStopSelling { get; set; }
        public long DivId { get; set; }
        public decimal OpeningStockSalesIncTax { get; set; }
        //public decimal PurchaseIncTax { get; set; }
        public int UnitAddedFor { get; set; }
        public string Domain { get; set; }
        public decimal Mrp { get; set; }
        public decimal DefaultMrp { get; set; }
        public List<ClsOpeningStockImport> OpeningStockImports { get; set; }
        public long AccountId { get; set; }
        public long JournalAccountId { get; set; }
        public decimal QuantityPurchased { get; set; }
        public decimal DefaultProfitMargin { get; set; }
    }

    public class ClsOpeningStockImport
    {
        public string SKU { get; set; }
        public string BranchName { get; set; }
        public string UnitType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string LotNo { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public decimal Mrp { get; set; }
        public decimal DefaultProfitMargin { get; set; }
    }

}