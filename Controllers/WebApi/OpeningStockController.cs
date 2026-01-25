using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class OpeningStockController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        //public async Task<IHttpActionResult> AllOpeningStocks(ClsOpeningStockVm obj)
        //{
        //                if (obj.PageSize == 0)
        //        {
        //            obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
        //}

        //    int skip = obj.PageSize * (obj.PageIndex - 1);
        //    if (obj.Search == "" || obj.Search == null)
        //    {
        //        var det = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new
        //        {
        //            OpeningStockId = a.OpeningStockId,
        //            a.ItemId,
        //            a.BranchId,
        //            a.Date,
        //            CompanyId = a.CompanyId,
        //            IsActive = a.IsActive,
        //            IsDeleted = a.IsDeleted,
        //            AddedBy = a.AddedBy,
        //            AddedOn = a.AddedOn,
        //            ModifiedBy = a.ModifiedBy,
        //            ModifiedOn = a.ModifiedOn,
        //        }).OrderByDescending(a => a.OpeningStockId).Skip(skip).Take(obj.PageSize).ToList();
        //        data = new
        //        {
        //            Status = 1,
        //            Message = "found",
        //            Data = new
        //            {
        //                OpeningStocks = det,
        //                TotalCount = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count(),
        //                ActiveCount = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Count(),
        //                InactiveCount = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == false).Count(),
        //PageSize = obj.PageSize
        //            }
        //        };
        //    }
        //    else
        //    {

        //    }
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> OpeningStock(ClsOpeningStock obj)
        {
            //obj.Date = obj.Date.AddHours(5).AddMinutes(30);
            //var OpeningStockIds = oConnectionContext.DbClsOpeningStock.Where(a => 
            //a.Date.Year == obj.Date.Year && a.Date.Month == obj.Date.Month && a.Date.Day == obj.Date.Day &&
            //a.BranchId == obj.BranchId && a.ItemId == obj.ItemId && a.ItemDetailsId == obj.ItemDetailsId).Select(a => a.OpeningStockId).ToList();


            var OpeningStocks = (from c in oConnectionContext.DbClsOpeningStock
                                 join b in oConnectionContext.DbClsItemDetails
                                 on c.ItemDetailsId equals b.ItemDetailsId
                                 join d in oConnectionContext.DbClsItem
                                 on c.ItemId equals d.ItemId
                                 where c.BranchId == obj.BranchId && c.ItemId == obj.ItemId &&
                c.ItemDetailsId == obj.ItemDetailsId && c.IsDeleted == false
                                 select new
                                 {
                                     c.DefaultProfitMargin,
                                     c.Mrp,
                                     //b.PurchaseIncTax,
                                     c.LotNo,
                                     c.ExpiryDate,
                                     c.ManufacturingDate,
                                     c.SalesExcTax,
                                     c.SalesIncTax,
                                     //SalesIncTax = b.SalesIncTax,
                                     //OpeningStockSalesIncTax = c.SalesIncTax,
                                     c.Date,
                                     c.QuantitySold,
                                     c.QuantityRemaining,
                                     c.Notes,
                                     c.OpeningStockId,
                                     c.Quantity,
                                     c.UnitCost,
                                     d.ItemId,
                                     c.SubTotal,
                                     d.ProductType,
                                     c.ItemDetailsId,
                                     d.ItemName,
                                     SKU = d.ProductType == "Single" ? d.SkuCode : b.SKU,
                                     b.VariationDetailsId,
                                     VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                     UnitId = d.UnitId,
                                     SecondaryUnitId = d.SecondaryUnitId,
                                     TertiaryUnitId = d.TertiaryUnitId,
                                     QuaternaryUnitId = d.QuaternaryUnitId,
                                     UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == d.UnitId).Select(c => c.UnitShortName).FirstOrDefault(),
                                     SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == d.SecondaryUnitId).Select(c => c.SecondaryUnitShortName).FirstOrDefault(),
                                     TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == d.TertiaryUnitId).Select(c => c.TertiaryUnitShortName).FirstOrDefault(),
                                     QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == d.QuaternaryUnitId).Select(c => c.QuaternaryUnitShortName).FirstOrDefault(),
                                     AllowDecimal = oConnectionContext.DbClsUnit.Where(c => c.UnitId == d.UnitId).Select(c => c.AllowDecimal).FirstOrDefault(),
                                     SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == d.SecondaryUnitId).Select(c => c.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                     TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == d.TertiaryUnitId).Select(c => c.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                     QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == d.QuaternaryUnitId).Select(c => c.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                     UToSValue = d.UToSValue,
                                     SToTValue = d.SToTValue,
                                     TToQValue = d.TToQValue,
                                     PriceAddedFor = c.PriceAddedFor,
                                     b.TotalCost,
                                     d.ItemCode,
                                 }).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OpeningStocks = OpeningStocks,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> InsertOpeningStock(ClsOpeningStockVm obj)
        //{
        //    using (TransactionScope dbContextTransaction = new TransactionScope())
        //    {
        //        ClsOpeningStock oClsOpeningStock = new ClsOpeningStock()
        //        {
        //            AddedBy = obj.AddedBy,
        //            AddedOn = CurrentDate,
        //            BranchId = obj.BranchId,
        //            CompanyId = obj.CompanyId,
        //            IsActive = true,
        //            IsDeleted = obj.IsDeleted,
        //            OpeningStockId = obj.OpeningStockId,
        //            Date = obj.Date,
        //            ItemId = obj.ItemId,
        //            Notes = obj.Notes
        //        };

        //        oConnectionContext.DbClsOpeningStock.Add(oClsOpeningStock);
        //        oConnectionContext.SaveChanges();

        //        //if (obj.OpeningStockDetails != null)
        //        //{
        //        //    foreach (var OpeningStock in obj.OpeningStockDetails)
        //        //    {
        //        //        ClsOpeningStockDetails oClsOpeningStockDetails = new ClsOpeningStockDetails()
        //        //        {
        //        //            ItemDetailsId = OpeningStock.ItemDetailsId,
        //        //            OpeningStockId = oClsOpeningStock.OpeningStockId,
        //        //            Quantity = OpeningStock.Quantity,
        //        //            UnitCost = OpeningStock.UnitCost,
        //        //            IsActive = true,
        //        //            IsDeleted = OpeningStock.IsDeleted,
        //        //            AddedBy = obj.AddedBy,
        //        //            AddedOn = CurrentDate,
        //        //            CompanyId = obj.CompanyId,
        //        //            SubTotal = OpeningStock.SubTotal,
        //        //            Notes = OpeningStock.Notes,
        //        //            QuantityRemaining = OpeningStock.Quantity,
        //        //            QuantitySold = 0
        //        //        };

        //        //        //ConnectionContext ocon = new ConnectionContext();
        //        //        oConnectionContext.DbClsOpeningStockDetails.Add(oClsOpeningStockDetails);
        //        //        oConnectionContext.SaveChanges();

        //        //        string query = "update tblItemBranchMap set Quantity = Quantity,0)+" + OpeningStock.Quantity + " where BranchId=" + obj.BranchId + " and ItemDetailsId=" + OpeningStock.ItemDetailsId;
        //        //        oConnectionContext.Database.ExecuteSqlCommand(query);
        //        //    }
        //        //}

        //        data = new
        //        {
        //            Status = 1,
        //            Message = "Opening Stock created successfully",
        //            Data = new
        //            {
        //            }
        //        };
        //        dbContextTransaction.Complete();
        //    }
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> OpeningStockDelete(ClsOpeningStockVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long BranchId = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == obj.OpeningStockId).Select(a => a.BranchId).FirstOrDefault();

                if (obj.OpeningStockId != 0)
                {
                    var details = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == obj.OpeningStockId).Select(a => new
                    {
                        a.IsStopSelling,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.QuantityRemaining,
                        a.PriceAddedFor,
                    }).FirstOrDefault();

                    decimal convertedStock = oCommonController.StockConversion(details.Quantity, details.ItemId, details.PriceAddedFor);
                    if (convertedStock != details.QuantityRemaining)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Cannot delete.. mismatch quantity",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    //string query = "update tblopeningstock set IsDeleted=1 where OpeningStockId=" + obj.OpeningStockId;
                    ////ConnectionContext ocon = new ConnectionContext();
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    //if (details.IsStopSelling == false)
                    //{
                    //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                    //}
                }

                data = new
                {
                    Status = 1,
                    Message = "Deleted successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateOpeningStock(ClsItemVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays, a.EnableLotNo }).FirstOrDefault();

                if (obj.OpeningStocks != null)
                {
                    foreach (var OpeningStock in obj.OpeningStocks)
                    {
                        if(OpeningStock.IsDeleted == false)
                        {
                            if (OpeningStock.Quantity == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divQuantity" + OpeningStock.DivId });
                                isError = true;
                            }

                            if (OpeningStock.UnitCost == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divUnitCost" + OpeningStock.DivId });
                                isError = true;
                            }

                            if (OpeningStock.Date == DateTime.MinValue)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divDate" + OpeningStock.DivId });
                                isError = true;
                            }

                            if (OpeningStock.SalesExcTax == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSalesExcTax" + OpeningStock.DivId });
                                isError = true;
                            }

                            if (OpeningStock.OpeningStockId != 0)
                            {
                                decimal convertedStock = oCommonController.StockConversion(OpeningStock.Quantity, obj.ItemId, OpeningStock.PriceAddedFor);
                                decimal QuantityOut = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == OpeningStock.OpeningStockId).Select(a => a.QuantitySold).FirstOrDefault();

                                if (convertedStock - QuantityOut < 0)
                                {
                                    errors.Add(new ClsError { Message = "Mismatch between purchase and sold quantity for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.ItemName).FirstOrDefault(), Id = "divQuantity" + OpeningStock.DivId });
                                    isError = true;
                                }
                            }

                            if (ItemSetting.EnableLotNo == true)
                            {
                                if (OpeningStock.LotNo == null || OpeningStock.LotNo == "")
                                {
                                    errors.Add(new ClsError { Message = "This field is required", Id = "divLotNo" + OpeningStock.DivId });
                                    isError = true;
                                }
                            }
                        }                                         
                    }

                    if (isError == true)
                    {
                        data = new
                        {
                            Status = 2,
                            Message = "",
                            Errors = errors,
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.OpeningStocks != null)
                {
                    foreach (var OpeningStock in obj.OpeningStocks)
                    {
                        if(OpeningStock.IsDeleted == true)
                        {
                            string query = "update \"tblOpeningStock\" set \"IsDeleted\"=True where \"OpeningStockId\"=" + OpeningStock.OpeningStockId;
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            if (OpeningStock.IsStopSelling == false)
                            {
                                decimal convertedStock = oCommonController.StockConversion(OpeningStock.Quantity, obj.ItemId, OpeningStock.PriceAddedFor);
                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + convertedStock + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + obj.ItemId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                        else
                        {
                            bool IsStopSelling = false, flag = false;

                            long AccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == OpeningStock.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                            long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault();

                            if (OpeningStock.OpeningStockId == 0)
                            {
                                decimal convertedStock = oCommonController.StockConversion(OpeningStock.Quantity, obj.ItemId, OpeningStock.PriceAddedFor);

                                if (OpeningStock.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                                {
                                    if ((OpeningStock.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                    {
                                        IsStopSelling = true;
                                    }
                                }

                                decimal DefaultProfitMargin = ((OpeningStock.SalesExcTax - OpeningStock.UnitCost)/OpeningStock.UnitCost)*100;

                                ClsOpeningStock oClsOpeningStock = new ClsOpeningStock()
                                {
                                    ItemDetailsId = OpeningStock.ItemDetailsId,
                                    OpeningStockId = OpeningStock.OpeningStockId,
                                    Quantity = OpeningStock.Quantity,
                                    UnitCost = OpeningStock.UnitCost,
                                    IsActive = true,
                                    IsDeleted = OpeningStock.IsDeleted,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    SubTotal = OpeningStock.SubTotal,
                                    Notes = OpeningStock.Notes,
                                    QuantitySold = 0,
                                    QuantityRemaining = convertedStock,
                                    BranchId = obj.BranchId,
                                    Date = OpeningStock.Date.AddHours(5).AddMinutes(30),
                                    ItemId = obj.ItemId,
                                    PriceAddedFor = OpeningStock.PriceAddedFor,
                                    SalesExcTax = OpeningStock.SalesExcTax,
                                    SalesIncTax = OpeningStock.SalesIncTax,
                                    LotNo = OpeningStock.LotNo,
                                    ExpiryDate = OpeningStock.ExpiryDate != null ? OpeningStock.ExpiryDate.Value.AddHours(5).AddMinutes(30) : OpeningStock.ExpiryDate,
                                    ManufacturingDate = OpeningStock.ManufacturingDate != null ? OpeningStock.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : OpeningStock.ManufacturingDate,
                                    IsStopSelling = IsStopSelling,
                                    UnitAddedFor = OpeningStock.UnitAddedFor,
                                    Mrp = OpeningStock.Mrp,
                                    AccountId = AccountId,
                                    JournalAccountId = JournalAccountId,
                                    QuantityPurchased = convertedStock,
                                    DefaultProfitMargin = DefaultProfitMargin
                                };

                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsOpeningStock.Add(oClsOpeningStock);
                                oConnectionContext.SaveChanges();

                                if (IsStopSelling == false)
                                {
                                    string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + OpeningStock.SalesIncTax + ",\"Quantity\" = \"Quantity\"+" + convertedStock + " where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                            }
                            else
                            {
                                decimal convertedStock = oCommonController.StockConversion(OpeningStock.Quantity, obj.ItemId, OpeningStock.PriceAddedFor);
                                decimal QuantityOut = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == OpeningStock.OpeningStockId).Select(a => a.QuantitySold).FirstOrDefault();

                                bool previousIsStopSelling = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == OpeningStock.OpeningStockId).Select(a => a.IsStopSelling).FirstOrDefault();
                                if (previousIsStopSelling == true)
                                {
                                    if (ItemSetting.OnItemExpiry == 1)
                                    {
                                        flag = true;
                                        IsStopSelling = false;
                                        string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + OpeningStock.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + (convertedStock - QuantityOut) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else
                                    {
                                        if ((OpeningStock.ExpiryDate != null))
                                        {
                                            if ((OpeningStock.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days > ItemSetting.StopSellingBeforeDays)
                                            {
                                                flag = true;
                                                IsStopSelling = false;
                                                string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + OpeningStock.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + (convertedStock - QuantityOut) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                            else
                                            {
                                                flag = true;
                                                IsStopSelling = true;
                                            }
                                        }
                                        else
                                        {
                                            flag = true;
                                            IsStopSelling = false;
                                            string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + OpeningStock.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + (convertedStock - QuantityOut) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                            oConnectionContext.Database.ExecuteSqlCommand(query);
                                        }
                                    }
                                }
                                else
                                {
                                    if (ItemSetting.OnItemExpiry != 1)
                                    {
                                        if ((OpeningStock.ExpiryDate != null))
                                        {
                                            if ((OpeningStock.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                            {
                                                flag = true;
                                                IsStopSelling = true;
                                                string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + OpeningStock.SalesIncTax + ",\"Quantity\" = \"Quantity\"-(" + (convertedStock - QuantityOut) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                        }
                                    }
                                }

                                if (flag == false)
                                {
                                    decimal Quantity = oCommonController.StockConversion(oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == OpeningStock.OpeningStockId).Select(a => a.Quantity).FirstOrDefault(), obj.ItemId, oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == OpeningStock.OpeningStockId).Select(a => a.PriceAddedFor).FirstOrDefault());
                                    string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + OpeningStock.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + (convertedStock - Quantity) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + OpeningStock.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }

                                decimal DefaultProfitMargin = ((OpeningStock.SalesExcTax - OpeningStock.UnitCost) / OpeningStock.UnitCost) * 100;

                                ClsOpeningStock oClsOpeningStock = new ClsOpeningStock()
                                {
                                    OpeningStockId = OpeningStock.OpeningStockId,
                                    ItemDetailsId = OpeningStock.ItemDetailsId,
                                    Quantity = OpeningStock.Quantity,
                                    UnitCost = OpeningStock.UnitCost,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    SubTotal = OpeningStock.SubTotal,
                                    Notes = OpeningStock.Notes,
                                    QuantityRemaining = convertedStock - QuantityOut,
                                    PriceAddedFor = OpeningStock.PriceAddedFor,
                                    Date = OpeningStock.Date.AddHours(5).AddMinutes(30),
                                    SalesExcTax = OpeningStock.SalesExcTax,
                                    SalesIncTax = OpeningStock.SalesIncTax,
                                    LotNo = OpeningStock.LotNo,
                                    ExpiryDate = OpeningStock.ExpiryDate != null ? OpeningStock.ExpiryDate.Value.AddHours(5).AddMinutes(30) : OpeningStock.ExpiryDate,
                                    ManufacturingDate = OpeningStock.ManufacturingDate != null ? OpeningStock.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : OpeningStock.ManufacturingDate,
                                    IsStopSelling = IsStopSelling,
                                    UnitAddedFor = OpeningStock.UnitAddedFor,
                                    Mrp = OpeningStock.Mrp,
                                    AccountId = AccountId,
                                    JournalAccountId = JournalAccountId,
                                    QuantityPurchased = convertedStock, //convertedStock - QuantityOut,
                                    DefaultProfitMargin = DefaultProfitMargin
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsOpeningStock.Attach(oClsOpeningStock);
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.ItemDetailsId).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.OpeningStockId).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.Quantity).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.UnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.SubTotal).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.Notes).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.QuantityRemaining).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.PriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.Date).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.SalesIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.SalesExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.LotNo).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.ExpiryDate).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.ManufacturingDate).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.IsStopSelling).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.UnitAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.Mrp).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.AccountId).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.JournalAccountId).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.QuantityPurchased).IsModified = true;
                                oConnectionContext.Entry(oClsOpeningStock).Property(x => x.DefaultProfitMargin).IsModified = true;
                                oConnectionContext.SaveChanges();

                                //string query = "update tblItemBranchMap set SalesIncTax=" + OpeningStock.SalesIncTax + ",Quantity = Quantity,0)+(" + (convertedStock - Quantity) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + OpeningStock.ItemDetailsId;
                                //oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Opening Stock",
                    CompanyId = obj.CompanyId,
                    Description = "Opening Stock for \"" + oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.SkuCode).FirstOrDefault()+"\" updated",
                    Id = obj.ItemId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Opening Stock updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> OpeningStockDetailsDelete(ClsOpeningStockVm obj)
        //{
        //    using (TransactionScope dbContextTransaction = new TransactionScope())
        //    {
        //        long BranchId = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == obj.OpeningStockId).Select(a => a.BranchId).FirstOrDefault();

        //        if (obj.OpeningStockId != 0)
        //        {
        //            var details = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == obj.OpeningStockId).Select(a => new
        //            {
        //                a.IsStopSelling,
        //                a.ItemDetailsId,
        //                a.ItemId,
        //                a.Quantity,
        //                a.QuantityRemaining,
        //                a.PriceAddedFor,
        //            }).FirstOrDefault();

        //            decimal convertedStock = oCommonController.StockConversion(details.Quantity, details.ItemId, details.PriceAddedFor);
        //            if (convertedStock != details.QuantityRemaining)
        //            {
        //                data = new
        //                {
        //                    Status = 0,
        //                    Message = "Cannot delete.. mismatch quantity",
        //                    Data = new
        //                    {
        //                    }
        //                };
        //                return await Task.FromResult(Ok(data));
        //            }

        //            string query = "update tblopeningstock set IsDeleted=1 where OpeningStockId=" + obj.OpeningStockId;
        //            //ConnectionContext ocon = new ConnectionContext();
        //            oConnectionContext.Database.ExecuteSqlCommand(query);

        //            if (details.IsStopSelling == false)
        //            {
        //                query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
        //                oConnectionContext.Database.ExecuteSqlCommand(query);
        //            }
        //        }

        //        data = new
        //        {
        //            Status = 1,
        //            Message = "Deleted successfully",
        //            Data = new
        //            {

        //            }
        //        };
        //        dbContextTransaction.Complete();
        //    }
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> ItemDetailsForOpeningStock(ClsItemVm obj)
        {
            var OpeningStocks = (from b in oConnectionContext.DbClsItemDetails
                                 join d in oConnectionContext.DbClsItem
                                 on b.ItemId equals d.ItemId
                                 where b.ItemId == obj.ItemId && b.IsDeleted == false && b.ItemDetailsId == obj.ItemDetailsId
                                 select new
                                 {
                                     b.DefaultProfitMargin,
                                     b.DefaultMrp,
                                     b.SalesExcTax,
                                     b.SalesIncTax,
                                     Notes = "",
                                     OpeningStockDetailsId = 0,
                                     Quantity = 0,
                                     UnitCost = d.TaxId == 0 ? b.PurchaseIncTax : b.PurchaseExcTax,
                                     b.PurchaseExcTax,
                                     d.ItemId,
                                     SubTotal = 0,
                                     d.ProductType,
                                     b.ItemDetailsId,
                                     d.ItemName,
                                     SKU = d.ProductType == "Variable" ? b.SKU : d.SkuCode,
                                     b.VariationDetailsId,
                                     VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                     UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                     b.TotalCost,
                                     d.ItemCode,
                                     UnitId = d.UnitId,
                                     SecondaryUnitId = d.SecondaryUnitId,
                                     TertiaryUnitId = d.TertiaryUnitId,
                                     QuaternaryUnitId = d.QuaternaryUnitId,
                                     UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == d.UnitId).Select(c => c.UnitShortName).FirstOrDefault(),
                                     SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == d.SecondaryUnitId).Select(c => c.SecondaryUnitShortName).FirstOrDefault(),
                                     TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == d.TertiaryUnitId).Select(c => c.TertiaryUnitShortName).FirstOrDefault(),
                                     QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == d.QuaternaryUnitId).Select(c => c.QuaternaryUnitShortName).FirstOrDefault(),
                                     UToSValue = d.UToSValue,
                                     SToTValue = d.SToTValue,
                                     TToQValue = d.TToQValue,
                                     d.PriceAddedFor,
                                 }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OpeningStocks = OpeningStocks,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportOpeningStock(ClsOpeningStockVm obj)
        {
            //using (TransactionScope dbContextTransaction = new TransactionScope())
            //{
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.OpeningStockImports == null || obj.OpeningStockImports.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No data",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            List<ClsItemImport> oClsItemImports = new List<ClsItemImport>();
            int count = 1;
            if (obj.OpeningStockImports != null)
            {
                foreach (var item in obj.OpeningStockImports)
                {
                    long ItemId = 0, ItemDetailsId = 0;
                    if (item.SKU == "" || item.SKU == null)
                    {
                        errors.Add(new ClsError { Message = "SKU is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.SKU != "" && item.SKU != null)
                    {
                       var _Item = (from a in oConnectionContext.DbClsItemDetails
                                  join b in oConnectionContext.DbClsItem
                                     on a.ItemId equals b.ItemId
                                  where (a.SKU.ToLower().Trim() == item.SKU.ToLower().Trim() || b.SkuCode.ToLower().Trim() == item.SKU.ToLower().Trim()) && a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                     && b.IsDeleted == false
                                  select new { a.ItemId,a.ItemDetailsId }).DefaultIfEmpty().FirstOrDefault();

                        if( _Item != null )
                        {
                            ItemId = _Item.ItemId;
                            ItemDetailsId = _Item.ItemDetailsId;
                        }

                        if (ItemDetailsId == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid SKU in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.BranchName != "" && item.BranchName != null)
                    {
                        long BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                     a.Branch.ToLower().Trim() == item.BranchName.ToLower().Trim()).Select(a => a.BranchId).DefaultIfEmpty().FirstOrDefault();
                        if (BranchId == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid BranchName in row no " + count, Id = "" });
                            isError = true;
                        }

                        if (oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == BranchId && a.ItemDetailsId == ItemDetailsId
                        && a.IsDeleted == false).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = item.SKU + " is not available in " + item.BranchName + " in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    var Units = oConnectionContext.DbClsItem.Where(b => b.ItemId == ItemId).Select(b => new
                    {
                        b.UnitId,
                        b.SecondaryUnitId,
                        b.TertiaryUnitId,
                        b.QuaternaryUnitId
                    }).FirstOrDefault();

                    if (item.UnitType != "" && item.UnitType != null)
                    {
                        if (item.UnitType.ToLower() != "primary unit" && item.UnitType.ToLower() != "secondary unit"
                            && item.UnitType.ToLower() != "tertiary unit" && item.UnitType.ToLower() != "quaternary unit")
                        {
                            errors.Add(new ClsError { Message = "Invalid Unit Type in row no " + count, Id = "" });
                            isError = true;
                        }

                        if (item.UnitType.ToLower() == "secondary unit")
                        {
                            if (Units.SecondaryUnitId == 0)
                            {
                                errors.Add(new ClsError { Message = "Secondary Unit is not available for " + item.SKU + " in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        if (item.UnitType.ToLower() == "tertiary unit")
                        {
                            if (Units.TertiaryUnitId == 0)
                            {
                                errors.Add(new ClsError { Message = "Tertiary Unit is not available for " + item.SKU + " in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        if (item.UnitType.ToLower() == "quaternary unit")
                        {
                            if (Units.QuaternaryUnitId == 0)
                            {
                                errors.Add(new ClsError { Message = "Quaternary Unit is not available for " + item.SKU + " in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.Quantity == 0)
                    {
                        errors.Add(new ClsError { Message = "Quantity is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.UnitCost == 0)
                    {
                        errors.Add(new ClsError { Message = "UnitCost is required in row no " + count, Id = "" });
                        isError = true;
                    }
                    count++;
                }
            }

            if (isError == true)
            {
                data = new
                {
                    Status = 2,
                    Message = "",
                    Errors = errors,
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            foreach (var item in obj.OpeningStockImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    int UnitAddedFor = 0, PriceAddedFor = 0;
                    long ItemId = 0, ItemDetailsId = 0;

                    var _Item = (from a in oConnectionContext.DbClsItemDetails
                              join b in oConnectionContext.DbClsItem
                                 on a.ItemId equals b.ItemId
                              where (a.SKU.ToLower().Trim() == item.SKU.ToLower().Trim() || b.SkuCode.ToLower().Trim() == item.SKU.ToLower().Trim()) && a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && b.IsDeleted == false
                              select new { a.ItemId ,a.ItemDetailsId}).DefaultIfEmpty().FirstOrDefault(); 
                    
                    if (_Item != null)
                    {
                        ItemId = _Item.ItemId;
                        ItemDetailsId = _Item.ItemDetailsId;
                    }

                    long BranchId = 0;
                    if (item.BranchName == "" || item.BranchName == null)
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BranchId).FirstOrDefault();
                    }
                    else
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.Branch == item.BranchName && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.BranchId).FirstOrDefault();
                    }

                    decimal SalesExcTax = 0;
                    if (item.SalesExcTax == 0)
                    {
                        SalesExcTax = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();
                    }
                    else
                    {
                        SalesExcTax = item.SalesExcTax;
                    }

                    decimal SalesIncTax = 0;
                    if (item.SalesIncTax == 0)
                    {
                        SalesIncTax = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == ItemDetailsId).Select(a => a.SalesIncTax).FirstOrDefault();
                    }
                    else
                    {
                        SalesIncTax = item.SalesIncTax;
                    }

                    decimal Mrp = 0;
                    if (item.Mrp == 0)
                    {
                        Mrp = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == ItemDetailsId).Select(a => a.DefaultMrp).FirstOrDefault();
                    }
                    else
                    {
                        Mrp = item.Mrp;
                    }

                    var Units = oConnectionContext.DbClsItem.Where(b => b.ItemId == ItemId).Select(b => new
                    {
                        b.UnitId,
                        b.SecondaryUnitId,
                        b.TertiaryUnitId,
                        b.QuaternaryUnitId
                    }).FirstOrDefault();

                    string UnitType = "";
                    if (item.UnitType == "" || item.UnitType == null)
                    {
                        UnitType = "Primary Unit";
                    }
                    else
                    {
                        UnitType = item.UnitType;
                    }

                    if (Units.QuaternaryUnitId != 0)
                    {
                        if (item.UnitType == "Primary Unit")
                        {
                            PriceAddedFor = 1;
                            UnitAddedFor = 1;
                        }
                        else if (item.UnitType == "Secondary Unit")
                        {
                            PriceAddedFor = 2;
                            UnitAddedFor = 2;
                        }
                        else if (item.UnitType == "Tertiary Unit")
                        {
                            PriceAddedFor = 3;
                            UnitAddedFor = 3;
                        }
                        else
                        {
                            PriceAddedFor = 4;
                            UnitAddedFor = 4;
                        }
                    }
                    else if (Units.TertiaryUnitId != 0)
                    {
                        if (item.UnitType == "Primary Unit")
                        {
                            PriceAddedFor = 2;
                            UnitAddedFor = 1;
                        }
                        else if (item.UnitType == "Secondary Unit")
                        {
                            PriceAddedFor = 3;
                            UnitAddedFor = 2;
                        }
                        else
                        {
                            PriceAddedFor = 4;
                            UnitAddedFor = 3;
                        }
                    }
                    else if (Units.SecondaryUnitId != 0)
                    {
                        if (item.UnitType == "Primary Unit")
                        {
                            PriceAddedFor = 3;
                            UnitAddedFor = 1;
                        }
                        else
                        {
                            PriceAddedFor = 4;
                            UnitAddedFor = 2;
                        }
                    }
                    else
                    {
                        PriceAddedFor = 4;
                        UnitAddedFor = 1;
                    }

                    var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                    { a.OnItemExpiry, a.StopSellingBeforeDays }).FirstOrDefault();


                    bool IsStopSelling = false, flag = false;
                    decimal convertedStock = oCommonController.StockConversion(item.Quantity, ItemId, PriceAddedFor);

                    if (item.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                    {
                        if ((item.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                        {
                            IsStopSelling = true;
                        }
                    }

                    long AccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                    long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
       && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault();

                    ClsOpeningStock oClsOpeningStock = new ClsOpeningStock()
                    {
                        ItemDetailsId = ItemDetailsId,
                        Quantity = item.Quantity,
                        UnitCost = item.UnitCost,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        SubTotal = item.UnitCost * item.Quantity,
                        Notes = item.Notes,
                        QuantitySold = 0,
                        QuantityRemaining = convertedStock,
                        BranchId = BranchId,
                        Date = item.Date != DateTime.MinValue ? item.Date.AddHours(5).AddMinutes(30) : CurrentDate.Date,
                        ItemId = ItemId,
                        PriceAddedFor = PriceAddedFor,
                        SalesIncTax = SalesIncTax,
                        SalesExcTax = SalesExcTax,
                        LotNo = item.LotNo,
                        ExpiryDate = item.ExpiryDate != null ? item.ExpiryDate.Value.AddHours(5).AddMinutes(30) : item.ExpiryDate,
                        ManufacturingDate = item.ManufacturingDate != null ? item.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : item.ManufacturingDate,
                        IsStopSelling = IsStopSelling,
                        UnitAddedFor = UnitAddedFor,
                        Mrp = Mrp,
                        AccountId  = AccountId,
                        JournalAccountId = JournalAccountId,
                        QuantityPurchased = convertedStock
                    };
                    oConnectionContext.DbClsOpeningStock.Add(oClsOpeningStock);
                    oConnectionContext.SaveChanges();

                    if (IsStopSelling == false)
                    {
                        string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + SalesIncTax + ",\"Quantity\" = \"Quantity\"+" + convertedStock + " where \"BranchId\"=" + BranchId + " and \"ItemDetailsId\"=" + ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }

                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Opening Stock",
                CompanyId = obj.CompanyId,
                Description = "Opening Stock imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Opening Stock imported successfully",
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
