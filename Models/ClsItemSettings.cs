using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblItemSettings")]
    public class ClsItemSettings
    {
        [Key]
        public long ItemSettingsId { get; set; }
        public bool EnableItemExpiry { get; set; }
        public int ExpiryType { get; set; }
        public int OnItemExpiry { get; set; }
        public int StopSellingBeforeDays { get; set; }
        public bool EnableBrands { get; set; }
        public bool EnableCategory { get; set; }
        public bool EnableSubCategory { get; set; }
        public bool EnableSubSubCategory { get; set; }
        public bool EnableTax_PriceInfo { get; set; }
        public long DefaultUnitId { get; set; }
        public bool EnableSecondaryUnit { get; set; }
        public bool EnableTertiaryUnit { get; set; }
        public bool EnableQuaternaryUnit { get; set; }
        public bool EnableRacks { get; set; }
        public bool EnableRow { get; set; }
        public bool EnablePosition { get; set; }
        public bool EnableWarranty { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal DefaultProfitPercent { get; set; }
        public bool EnableLotNo { get; set; }
        public int StockAccountingMethod { get; set; }
        public bool EnableProductVariation { get; set; }
        public bool EnableProductDescription { get; set; }
        public bool EnableComboProduct { get; set; }
        //public bool EnableImei { get; set; }
        public bool EnableBarcode { get; set; }
        public bool EnablePrintLabel { get; set; }
        public bool EnableStockAdjustment { get; set; }
        public string ExpiryDateFormat { get; set; }
        public bool EnableMrp { get; set; }
        public bool EnableStockTransfer { get; set; }
        public bool EnableSellingPriceGroup { get; set; }
        public string TaxType { get; set; }
        public bool EnableSalt { get; set; }
        public bool EnableItemImage { get; set; }
    }

    public class ClsItemSettingsVm
    {
        public long ItemSettingsId { get; set; }
        public bool EnableItemExpiry { get; set; }
        public int ExpiryType { get; set; }
        public int OnItemExpiry { get; set; }
        public int StopSellingBeforeDays { get; set; }
        public bool EnableBrands { get; set; }
        public bool EnableCategory { get; set; }
        public bool EnableSubCategory { get; set; }
        public bool EnableSubSubCategory { get; set; }
        public bool EnableTax_PriceInfo { get; set; }
        public long DefaultUnitId { get; set; }
        public bool EnableSecondaryUnit { get; set; }
        public bool EnableTertiaryUnit { get; set; }
        public bool EnableQuaternaryUnit { get; set; }
        public bool EnableRacks { get; set; }
        public bool EnableRow { get; set; }
        public bool EnablePosition { get; set; }
        public bool EnableWarranty { get; set; }
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
        public decimal DefaultProfitPercent { get; set; }
        public bool EnableLotNo { get; set; }
        public int StockAccountingMethod { get; set; }
        public bool EnableProductVariation { get; set; }
        public bool EnableProductDescription { get; set; }
        public bool EnableComboProduct { get; set; }
        //public bool EnableImei { get; set; }
        public bool EnableBarcode { get; set; }
        public bool EnablePrintLabel { get; set; }
        public bool EnableStockAdjustment { get; set; }
        public string Domain { get; set; }
        public string ExpiryDateFormat { get; set; }
        public bool EnableMrp { get; set; }
        //public bool EnableEditingProductPrice { get; set; }
        public bool EnableStockTransfer { get; set; }
        public bool EnableSellingPriceGroup { get; set; }
        public string TaxType { get; set; }
        public bool EnableSalt { get; set; }
        public bool EnableItemImage { get; set; }
    }
}