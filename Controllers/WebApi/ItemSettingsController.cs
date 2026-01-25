using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class ItemSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> ItemSetting(ClsItemSettingsVm obj)
        {
            var det = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.TaxType,
                ItemSettingsId = a.ItemSettingsId,
                EnableItemExpiry = a.EnableItemExpiry,
                ExpiryType = a.ExpiryType,
                OnItemExpiry = a.OnItemExpiry,
                StopSellingBeforeDays = a.StopSellingBeforeDays,
                EnableBrands = a.EnableBrands,
                EnableCategory = a.EnableCategory,
                EnableSubCategory = a.EnableSubCategory,
                EnableSubSubCategory = a.EnableSubSubCategory,
                EnableTax_PriceInfo = a.EnableTax_PriceInfo,
                DefaultUnitId = a.DefaultUnitId,
                EnableSecondaryUnit = a.EnableSecondaryUnit,
                EnableTertiaryUnit = a.EnableTertiaryUnit,
                EnableQuaternaryUnit = a.EnableQuaternaryUnit,
                EnableRacks = a.EnableRacks,
                EnableRow = a.EnableRow,
                EnablePosition = a.EnablePosition,
                EnableWarranty = a.EnableWarranty,
                a.DefaultProfitPercent,
                EnableLotNo = a.EnableLotNo,
                a.StockAccountingMethod,
                a.EnableProductVariation,
                a.EnableProductDescription,
                a.EnableComboProduct,
                //a.EnableImei,
                a.EnablePrintLabel,
                a.EnableStockAdjustment,
                a.ExpiryDateFormat,
                a.EnableMrp,
                a.EnableStockTransfer,
                a.EnableSellingPriceGroup,
                a.EnableSalt,
                a.EnableItemImage
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemSettingsUpdate(ClsItemSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long ItemSettingsId = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.ItemSettingsId).FirstOrDefault();

                ClsItemSettings oClsItemSettings = new ClsItemSettings()
                {
                    ItemSettingsId = ItemSettingsId,
                    EnableItemExpiry = obj.EnableItemExpiry,
                    ExpiryType = obj.ExpiryType,
                    OnItemExpiry = obj.OnItemExpiry,
                    StopSellingBeforeDays = obj.StopSellingBeforeDays,
                    EnableBrands = obj.EnableBrands,
                    EnableCategory = obj.EnableCategory,
                    EnableSubCategory = obj.EnableSubCategory,
                    EnableSubSubCategory = obj.EnableSubSubCategory,
                    EnableTax_PriceInfo = obj.EnableTax_PriceInfo,
                    DefaultUnitId = obj.DefaultUnitId,
                    EnableSecondaryUnit = obj.EnableSecondaryUnit,
                    EnableTertiaryUnit = obj.EnableTertiaryUnit,
                    EnableQuaternaryUnit = obj.EnableQuaternaryUnit,
                    EnableRacks = obj.EnableRacks,
                    EnableRow = obj.EnableRow,
                    EnablePosition = obj.EnablePosition,
                    EnableWarranty = obj.EnableWarranty,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    DefaultProfitPercent = obj.DefaultProfitPercent,
                    EnableLotNo = obj.EnableLotNo,
                    StockAccountingMethod = obj.StockAccountingMethod,
                    EnableProductVariation = obj.EnableProductVariation,
                    EnableProductDescription = obj.EnableProductDescription,
                    EnableComboProduct = obj.EnableComboProduct,
                    //EnableImei = obj.EnableImei,
                    EnablePrintLabel = obj.EnablePrintLabel,
                    EnableStockAdjustment = obj.EnableStockAdjustment,
                    ExpiryDateFormat = obj.ExpiryDateFormat,
                    EnableMrp = obj.EnableMrp,
                    EnableStockTransfer = obj.EnableStockTransfer,
                    EnableSellingPriceGroup = obj.EnableSellingPriceGroup,
                    TaxType = obj.TaxType,
                    EnableSalt = obj.EnableSalt,
                    EnableItemImage = obj.EnableItemImage,
                };

                oConnectionContext.DbClsItemSettings.Attach(oClsItemSettings);
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.ItemSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableItemExpiry).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.ExpiryType).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.OnItemExpiry).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.StopSellingBeforeDays).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableBrands).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableCategory).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableSubCategory).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableSubSubCategory).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableTax_PriceInfo).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.DefaultUnitId).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableSecondaryUnit).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableTertiaryUnit).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableQuaternaryUnit).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableRacks).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableRow).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnablePosition).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableWarranty).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.DefaultProfitPercent).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableLotNo).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.StockAccountingMethod).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableProductVariation).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableProductDescription).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableComboProduct).IsModified = true;
                //oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableImei).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnablePrintLabel).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableStockAdjustment).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.ExpiryDateFormat).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableMrp).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableStockTransfer).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableSellingPriceGroup).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.TaxType).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableSalt).IsModified = true;
                oConnectionContext.Entry(oClsItemSettings).Property(x => x.EnableItemImage).IsModified = true;

                oConnectionContext.SaveChanges();

                //disable free quantity 
                if (obj.EnableLotNo == false)
                {
                    //pos settings
                    long PosSettingsId = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.PosSettingsId).FirstOrDefault();
                    ClsPosSettings oClsPosSettings = new ClsPosSettings()
                    {
                        PosSettingsId = PosSettingsId,
                        EnableFreeQuantity = false
                    };

                    oConnectionContext.DbClsPosSettings.Attach(oClsPosSettings);
                    oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableFreeQuantity).IsModified = true;
                    oConnectionContext.SaveChanges();
                    //pos settings

                    //purchase settings
                    long PurchaseSettingsId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.PurchaseSettingsId).FirstOrDefault();

                    ClsPurchaseSettings oClsPurchaseSettings = new ClsPurchaseSettings()
                    {
                        PurchaseSettingsId = PurchaseSettingsId,
                        EnableFreeQuantity = false
                    };

                    oConnectionContext.DbClsPurchaseSettings.Attach(oClsPurchaseSettings);
                    oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableFreeQuantity).IsModified = true;
                    oConnectionContext.SaveChanges();
                    //purchase settings

                    //purchase settings
                    long SaleSettingsId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SaleSettingsId).FirstOrDefault();

                    ClsSaleSettings oClsSaleSettings = new ClsSaleSettings()
                    {
                        SaleSettingsId = SaleSettingsId,
                        EnableFreeQuantity = false
                    };

                    oConnectionContext.DbClsSaleSettings.Attach(oClsSaleSettings);
                    oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableFreeQuantity).IsModified = true;
                    oConnectionContext.SaveChanges();
                    //purchase settings
                }
                //disable free quantity 


                //if (obj.OnItemExpiry == 2)
                //{
                List<ClsAvailableLots> availableLots = (from a in oConnectionContext.DbClsPurchase
                                                        join b in oConnectionContext.DbClsPurchaseDetails
                                                         on a.PurchaseId equals b.PurchaseId
                                                        join c in oConnectionContext.DbClsItemBranchMap
                                                        on b.ItemDetailsId equals c.ItemDetailsId
                                                        join d in oConnectionContext.DbClsItem 
                                                        on c.ItemId equals d.ItemId
                                                        where a.CompanyId == obj.CompanyId && b.QuantityRemaining > 0
                                                        && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && b.IsActive == true
                                                        && b.IsDeleted == false && c.IsActive == true && c.IsDeleted == false
                                                        && d.IsActive == true && d.IsDeleted == false
                                                        && b.ExpiryDate != null
                                                        select new ClsAvailableLots
                                                        {
                                                            ExpiryDate = b.ExpiryDate,
                                                            Type = "purchase",
                                                            QuantityRemaining = b.QuantityRemaining,
                                                            Id = b.PurchaseDetailsId,
                                                            IsStopSelling = b.IsStopSelling,
                                                            ItemBranchMapId = c.ItemBranchMapId
                                                        }).Union(from a in oConnectionContext.DbClsOpeningStock
                                                                 join c in oConnectionContext.DbClsItemBranchMap
                                                         on a.ItemDetailsId equals c.ItemDetailsId
                                                                 join d in oConnectionContext.DbClsItem
                                                                on c.ItemId equals d.ItemId
                                                                 where a.CompanyId == obj.CompanyId && a.QuantityRemaining > 0
                                                                      && a.IsActive == true && a.IsDeleted == false && c.IsActive == true
                                                                      && c.IsDeleted == false 
                                                                      && d.IsActive == true && d.IsDeleted == false
                                                                      && a.ExpiryDate != null
                                                                 select new ClsAvailableLots
                                                                 {
                                                                     ExpiryDate = a.ExpiryDate,
                                                                     Type = "openingstock",
                                                                     QuantityRemaining = a.QuantityRemaining,
                                                                     Id = a.OpeningStockId,
                                                                     IsStopSelling = a.IsStopSelling,
                                                                     ItemBranchMapId = c.ItemBranchMapId
                                                                 }).Union(from a in oConnectionContext.DbClsStockTransfer
                                                                          join b in oConnectionContext.DbClsStockTransferDetails
                                                                           on a.StockTransferId equals b.StockTransferId
                                                                          join c in oConnectionContext.DbClsItemBranchMap
                                                                          on b.ItemDetailsId equals c.ItemDetailsId
                                                                          join d in oConnectionContext.DbClsItem
                                                                          on c.ItemId equals d.ItemId
                                                                          where a.CompanyId == obj.CompanyId && b.QuantityRemaining > 0
                                                                          && a.IsActive == true && a.IsDeleted == false && b.IsActive == true
                                                                          && b.IsDeleted == false && c.IsActive == true && c.IsDeleted == false
                                                                          && d.IsActive == true && d.IsDeleted == false
                                                                          //&& b.ExpiryDate != null
                                                                          select new ClsAvailableLots
                                                                          {
                                                                              ExpiryDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                    : b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                    : DateTime.MinValue,
                                                                              Type = "stocktransfer",
                                                                              QuantityRemaining = b.QuantityRemaining,
                                                                              Id = b.StockTransferDetailsId,
                                                                              IsStopSelling = b.IsStopSelling,
                                                                              ItemBranchMapId = c.ItemBranchMapId
                                                                          })
                                                        .ToList();

                foreach (var lot in availableLots)
                {
                    if (lot.IsStopSelling == true)
                    {
                        if (obj.OnItemExpiry == 1)
                        {
                            if (lot.Type == "purchase")
                            {
                                string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=False where \"PurchaseDetailsId\"=" + lot.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            else if (lot.Type == "openingstock")
                            {
                                string query = "update \"tblOpeningStock\" set \"IsStopSelling\"=False where \"OpeningStockId\"=" + lot.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            else if (lot.Type == "stocktransfer")
                            {
                                string query = "update \"tblStockTransferDetails\" set \"IsStopSelling\"=False where \"StockTransferDetailsId\"=" + lot.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            //else if (lot.Type == "stockadjustment")
                            //{
                            //    string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=False where \"StockAdjustmentDetailsId\"=" + lot.Id;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query);
                            //}

                            string query1 = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + lot.QuantityRemaining + " where \"ItemBranchMapId\"=" + lot.ItemBranchMapId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                        }
                        else
                        {
                            if ((lot.ExpiryDate != null))
                            {
                                if ((lot.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days > obj.StopSellingBeforeDays)
                            {
                                if (lot.Type == "purchase")
                                {
                                    string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=False where \"PurchaseDetailsId\"=" + lot.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (lot.Type == "openingstock")
                                {
                                    string query = "update \"tblOpeningStock\" set \"IsStopSelling\"=False where \"OpeningStockId\"=" + lot.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (lot.Type == "stocktransfer")
                                {
                                    string query = "update \"tblStockTransferDetails\" set \"IsStopSelling\"=False where \"StockTransferDetailsId\"=" + lot.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                    //else if (lot.Type == "stockadjustment")
                                    //{
                                    //    string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=0 where \"StockAdjustmentDetailsId\"=" + lot.Id;
                                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                    //}

                                    string query1 = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + lot.QuantityRemaining + " where \"ItemBranchMapId\"=" + lot.ItemBranchMapId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                            }
                            else
                            {
                                if (lot.Type == "purchase")
                                {
                                    string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=True where \"PurchaseDetailsId\"=" + lot.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (lot.Type == "openingstock")
                                {
                                    string query = "update \"tblOpeningStock\" set \"IsStopSelling\"=True where \"OpeningStockId\"=" + lot.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (lot.Type == "stocktransfer")
                                {
                                    string query = "update \"tblStockTransferDetails\" set \"IsStopSelling\"=True where \"StockTransferDetailsId\"=" + lot.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                    //else if (lot.Type == "stockadjustment")
                                    //{
                                    //    string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=True where \"StockAdjustmentDetailsId\"=" + lot.Id;
                                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                    //}

                                    //string query1 = "update tblItemBranchMap set Quantity=Quantity,0)-" + lot.QuantityRemaining + " where ItemBranchMapId=" + lot.ItemBranchMapId;
                                    //oConnectionContext.Database.ExecuteSqlCommand(query1);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (obj.OnItemExpiry != 1)
                        {
                            if(lot.ExpiryDate!= null)
                            {
                                if ((lot.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= obj.StopSellingBeforeDays)
                                {
                                    if (lot.Type == "purchase")
                                    {
                                        string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=True where \"PurchaseDetailsId\"=" + lot.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (lot.Type == "openingstock")
                                    {
                                        string query = "update \"tblOpeningStock\" set \"IsStopSelling\"=True where \"OpeningStockId\"=" + lot.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (lot.Type == "stocktransfer")
                                    {
                                        string query = "update \"tblStockTransferDetails\" set \"IsStopSelling\"=True where \"StockTransferDetailsId\"=" + lot.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    //else if (lot.Type == "stockadjustment")
                                    //{
                                    //    string query = "update \"tblPurchaseDetails\" set \"IsStopSelling\"=True where \"StockAdjustmentDetailsId\"=" + lot.Id;
                                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                    //}

                                    string query1 = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + lot.QuantityRemaining + " where \"ItemBranchMapId\"=" + lot.ItemBranchMapId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                }
                            }
                           
                        }
                    }
                }
                //}

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Item\" updated",
                    Id = oClsItemSettings.ItemSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Item Info updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
