using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblVariationDetails")]
    public class ClsVariationDetails
    {
        [Key]
        public long VariationDetailsId { get; set; }
        public long VariationId { get; set; }
        public string VariationDetails { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsVariationDetailsVm
    {
        public long VariationDetailsId { get; set; }
        public long VariationId { get; set; }
        public string VariationDetails { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal DefaultProfitMargin { get; set; }
        public long ItemDetailsId { get; set; }
        public long ItemId { get; set; }
        public string ProductImage { get; set; }
        public decimal PurchaseExcTax { get; set; }
        public decimal PurchaseIncTax { get; set; }
        public decimal Quantity { get; set; }
        public decimal SalesExcTax { get; set; }
        public decimal SalesIncTax { get; set; }
        public string SKU { get; set; }
        public string ComboProductName { get; set; }
        public decimal TotalCost { get; set; }
        public decimal ComboPurchaseExcTax { get; set; }
        public string ComboSku { get; set; }
        public string ComboVariationName { get; set; }
        public string ComboUnitName { get; set; }
        public long ComboItemDetailsId { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public decimal DefaultMrp { get; set; }
        public long InventoryAccountId { get; set; }
        public long PurchaseAccountId { get; set; }
        public long SalesAccountId { get; set; }
        public long UnitId { get; set; }
        public long SecondaryUnitId { get; set; }
        public long TertiaryUnitId { get; set; }
        public long QuaternaryUnitId { get; set; }
        public string UnitShortName { get; set; }
        public string SecondaryUnitShortName { get; set; }
        public string TertiaryUnitShortName { get; set; }
        public string QuaternaryUnitShortName { get; set; }
        public int PriceAddedFor { get; set; }
        public decimal UToSValue { get; set; }
        public decimal SToTValue { get; set; }
        public decimal TToQValue { get; set; }
        public bool AllowDecimal { get; set; }
        public bool SecondaryUnitAllowDecimal { get; set; }
        public bool TertiaryUnitAllowDecimal { get; set; }
        public bool QuaternaryUnitAllowDecimal { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Mrp { get; set; }
        public decimal ComboPurchaseIncTax { get; set; }
        //public int ComboPriceAddedFor { get; set; }
        public List<ClsItemDetailsVariationMapVm> AttributeMappings { get; set; }
    }

}