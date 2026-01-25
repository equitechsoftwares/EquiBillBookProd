using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsStock
    {
        public string ItemName { get; set; }
        public string VariationName { get; set; }
        public string SKU { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalOpeningStock { get; set; }
        public decimal TotalSalesReturn { get; set; }
        public decimal TotalStockTransferIn { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalStockAdjustment { get; set; }
        public decimal TotalStockTransferOut { get; set; }
        public decimal TotalPurchaseReturn { get; set; }
        public decimal TotalCurrentStock { get; set; }
        public IList<ClsStockDetails> StockDetails { get; set; }
        public decimal TotalWastage { get; set; }
        public string UnitName { get; set; }
        //public long UnitId { get; set; }
        //public long SecondaryUnitId { get; set; }
        //public long TertiaryUnitId { get; set; }
        //public long QuaternaryUnitId { get; set; }
    }

    public class ClsStockDetails
    {
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long Id { get; set; }
        public decimal PrimaryCredit { get; set; }
        public decimal SecondaryCredit { get; set; }
        public decimal TertiaryCredit { get; set; }
        public decimal QuaternaryCredit { get; set; }
        public decimal PrimaryDebit { get; set; }
        public decimal SecondaryDebit { get; set; }
        public decimal TertiaryDebit { get; set; }
        public decimal QuaternaryDebit { get; set; }
        public decimal PrimaryBalance { get; set; }
        public decimal SecondaryBalance { get; set; }
        public decimal TertiaryBalance { get; set; }
        public decimal QuaternaryBalance { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit{ get; set; }
        public decimal Balance { get; set; }
        public long ItemId { get; set; }
        public string PrimaryUnit { get; set; }
        public string SecondaryUnit { get; set; }
        public string TertiaryUnit { get; set; }
        public string QuaternaryUnit { get; set; }
        public int PriceAddedFor { get; set; }
        public bool PrimaryUnitAllowDecimal { get; set; }
        public bool SecondaryUnitAllowDecimal { get; set; }
        public bool TertiaryUnitAllowDecimal { get; set; }
        public bool QuaternaryUnitAllowDecimal { get; set; }
        public decimal FreeQuantity { get; set; }
        public int FreeQuantityPriceAddedFor { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal AmountIncTax { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public long AddedBy { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string ItemName { get; set; }
        public string SKU { get; set; }
        public string VariationName { get; set; }
        public decimal OpeningBalanceQty { get; set; }
        public decimal OpeningBalanceAmount { get; set; }
        public decimal QuantityIn { get; set; }
        public decimal AmountIn { get; set; }
        public decimal QuantityOut { get; set; }
        public decimal AmountOut { get; set; }
        public decimal PrimaryOpeningBalanceQty { get; set; }
        public decimal SecondaryOpeningBalanceQty { get; set; }
        public decimal TertiaryOpeningBalanceQty { get; set; }
        public decimal QuaternaryOpeningBalanceQty { get; set; }
        public decimal PrimaryQuantityIn { get; set; }
        public decimal SecondaryQuantityIn { get; set; }
        public decimal TertiaryQuantityIn { get; set; }
        public decimal QuaternaryQuantityIn { get; set; }
        public decimal PrimaryQuantityOut { get; set; }
        public decimal SecondaryQuantityOut { get; set; }
        public decimal TertiaryQuantityOut { get; set; }
        public decimal QuaternaryQuantityOut { get; set; }
        public string UnitName { get; set; }
        public DateTime DetailsDate { get; set; }
    }

    public class ClsStockDeductionIds
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
    }

}