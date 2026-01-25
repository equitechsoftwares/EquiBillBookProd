using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesReturnDetails")]
    public class ClsSalesReturnDetails
    {
        [Key]
        public long SalesReturnDetailsId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesDetailsId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        //public string StockDeductionIds { get; set; }
        public int PriceAddedFor { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal FreeQuantity { get; set; }
        public int UnitAddedFor { get; set; }
        public decimal QuantityReturned { get; set; }
        public long TaxId { get; set; }
        public string DiscountType { get; set; }
        public decimal Discount { get; set; }
        public decimal UnitCost { get; set; }
        public decimal PriceIncTax { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PriceExcTax { get; set; }
        public decimal AmountIncTax { get; set; }
        public string ComboId { get; set; }
        public bool IsComboItems { get; set; }
        public long AccountId { get; set; }
        public long DiscountAccountId { get; set; }
        public long TaxAccountId { get; set; }
        public decimal ExtraDiscount { get; set; }
        public long ItemCodeId { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public bool IsCombo { get; set; }
    }

    public class ClsSalesReturnDetailsVm
    {
        public long SalesReturnDetailsId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesDetailsId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        //public string StockDeductionIds { get; set; }
        public IList<ClsStockDeductionIds> StockDeductionIds { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public int PriceAddedFor { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal FreeQuantity { get; set; }
        public int UnitAddedFor { get; set; }
        public long BranchId { get; set; }
        public string Domain { get; set; }
        public decimal QuantityReturned { get; set; }
        public long TaxId { get; set; }
        public string DiscountType { get; set; }
        public decimal Discount { get; set; }
        public decimal UnitCost { get; set; }
        public decimal PriceIncTax { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PriceExcTax { get; set; }
        public decimal AmountIncTax { get; set; }
        public string ComboId { get; set; }
        public bool IsComboItems { get; set; }
        public string ProductType { get; set; }
        public long AccountId { get; set; }
        public long DiscountAccountId { get; set; }
        public long TaxAccountId { get; set; }
        public long CustomerId { get; set; }
        public long SubCategoryId { get; set; }
        public long SubSubCategoryId { get; set; }
        public long BrandId { get; set; }
        public long CategoryId { get; set; }
        public int DivId { get; set; }
        public decimal ExtraDiscount { get; set; }
        public long ItemCodeId { get; set; }
        public DateTime? LotManufacturingDate { get; set; }
        public DateTime? LotExpiryDate { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public bool IsCombo { get; set; }
        public string ProductImage { get; set; }
        public string SKU { get; set; }
    }

}