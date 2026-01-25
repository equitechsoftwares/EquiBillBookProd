using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class StockTransferController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllStockTransfers(ClsStockTransferVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            //}).FirstOrDefault();

            //if (obj.BranchId == 0)
            //{
            //    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            //}

            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsStockTransfer.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.FromBranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new
                {
                    a.StockTransferReasonId,
                    StockTransferReason = oConnectionContext.DbClsStockTransferReason.Where(b => b.StockTransferReasonId == a.StockTransferReasonId).Select(b => b.StockTransferReason).FirstOrDefault(),
                    a.InvoiceId,
                    StockTransferId = a.StockTransferId,
                    a.FromBranchId,
                    InvoiceUrl = oCommonController.webUrl,//+ "/stocktransfer/invoice?InvoiceNo=" + a.ReferenceNo + "&Id=" + a.CompanyId,
                    FromBranch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.FromBranchId).Select(b => b.Branch).FirstOrDefault(),
                    a.ToBranchId,
                    ToBranch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.ToBranchId).Select(b => b.Branch).FirstOrDefault(),
                    a.ReferenceNo,
                    a.Date,
                    a.Status,
                    a.Subtotal,
                    a.TotalAmount,
                    a.Notes,
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    a.TotalQuantity,
                    TotalItems = oConnectionContext.DbClsStockTransferDetails.Where(c => c.StockTransferId == a.StockTransferId &&
                    c.IsDeleted == false).Count()
                }).ToList();

            //if (obj.Date != DateTime.MinValue && obj.Date != DateTime.MinValue)
            //{
            //    det = det.Where(a => a.Date.Date >= obj.FromDate.AddHours(5).AddMinutes(30) && a.Date.Date <= obj.ToDate.AddHours(5).AddMinutes(30)).ToList();
            //}

            if (obj.FromBranchId != 0)
            {
                det = det.Where(a => a.FromBranchId == obj.FromBranchId).Select(a => a).ToList();
            }

            if (obj.ToBranchId != 0)
            {
                det = det.Where(a => a.ToBranchId == obj.ToBranchId).Select(a => a).ToList();
            }

            if (obj.Status != 0)
            {
                det = det.Where(a => a.Status == obj.Status).Select(a => a).ToList();
            }

            if (obj.ReferenceNo != null && obj.ReferenceNo != "")
            {
                det = det.Where(a => a.ReferenceNo.ToLower().Contains(obj.ReferenceNo.ToLower())).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockTransfers = det.OrderByDescending(a => a.StockTransferId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockTransfer(ClsStockTransfer obj)
        {
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
            var det = oConnectionContext.DbClsStockTransfer.Where(a => a.StockTransferId == obj.StockTransferId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.StockTransferReasonId,
                a.InvoiceId,
                a.Subtotal,
                StockTransferId = a.StockTransferId,
                a.FromBranchId,
                FromBranch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.FromBranchId).Select(b => b.Branch).FirstOrDefault(),
                a.ToBranchId,
                ToBranch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.ToBranchId).Select(b => b.Branch).FirstOrDefault(),
                a.ReferenceNo,
                a.Date,
                a.Status,
                a.TotalAmount,
                a.Notes,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                a.TotalQuantity,
                StockTransferDetails = (from b in oConnectionContext.DbClsStockTransferDetails
                                        join c in oConnectionContext.DbClsItemBranchMap
                                        on b.ItemDetailsId equals c.ItemDetailsId
                                        join d in oConnectionContext.DbClsItemDetails
                                         on b.ItemDetailsId equals d.ItemDetailsId
                                        join e in oConnectionContext.DbClsItem
                                        on d.ItemId equals e.ItemId
                                        where b.StockTransferId == a.StockTransferId && b.IsDeleted == false && c.BranchId == a.FromBranchId
                                        select new
                                        {
                                            Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                                            LotNo = b.LotTypeForLotNoChecking == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                                    : "Default Stock Accounting Method",
                                            LotManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                                    : null,
                                            LotExpiryDate = b.LotTypeForLotNoChecking == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                                    : null,
                                            b.LotId,
                                            b.LotType,
                                            //        QuantityRemaining = b.QuantityRemaining + b.LotType == "purchase" ?
                                            //oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                            //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                            //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                            //: c.Quantity,
                                            QuantityRemaining = a.Status == 3 ? (b.QuantityRemaining + (b.LotType == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    : b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    : b.LotType == "stocktransfer" ? oConnectionContext.DbClsStockTransferDetails.Where(f => f.StockTransferDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    : c.Quantity)) : c.Quantity,
                                            b.StockTransferDetailsId,
                                            b.Amount,
                                            b.Quantity,
                                            b.UnitCost,
                                            d.ItemId,
                                            e.ProductType,
                                            c.ItemDetailsId,
                                            e.ItemName,
                                            d.SKU,
                                            d.VariationDetailsId,
                                            VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == d.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                            UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == e.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                            //d.SalesExcTax,
                                            //d.SalesIncTax,
                                            SalesIncTax = b.LotTypeForLotNoChecking == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                    : d.SalesIncTax,
                                            d.TotalCost,
                                            e.ItemCode,
                                            UnitId = e.UnitId,
                                            SecondaryUnitId = e.SecondaryUnitId,
                                            TertiaryUnitId = e.TertiaryUnitId,
                                            QuaternaryUnitId = e.QuaternaryUnitId,
                                            UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == e.UnitId).Select(c => c.UnitShortName).FirstOrDefault(),
                                            SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == e.SecondaryUnitId).Select(c => c.SecondaryUnitShortName).FirstOrDefault(),
                                            TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == e.TertiaryUnitId).Select(c => c.TertiaryUnitShortName).FirstOrDefault(),
                                            QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == e.QuaternaryUnitId).Select(c => c.QuaternaryUnitShortName).FirstOrDefault(),
                                            UToSValue = e.UToSValue,
                                            SToTValue = e.SToTValue,
                                            TToQValue = e.TToQValue,
                                            PriceAddedFor = b.PriceAddedFor,
                                        }).ToList(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockTransfer = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertStockTransfer(ClsStockTransferVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.FromBranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFromBranch" });
                    isError = true;
                }

                if (obj.ToBranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divToBranch" });
                    isError = true;
                }

                if (obj.FromBranchId != 0 && obj.FromBranchId == obj.ToBranchId)
                {
                    errors.Add(new ClsError { Message = "From Business Location & To Business Location cannot be same", Id = "divToBranch" });
                    isError = true;
                }

                if (obj.StockTransferReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStockTransferReason" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsStockTransfer.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Reference No exists", Id = "divReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.StockTransferDetails == null || obj.StockTransferDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.StockTransferDetails != null)
                {
                    foreach (var item in obj.StockTransferDetails)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "Quantity is required", Id = "divQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.UnitCost <= 0)
                            {
                                errors.Add(new ClsError { Message = "This is required", Id = "divUnitCost" + item.DivId });
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

                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.FromBranchId).Select(a => a.PrefixId).FirstOrDefault();
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "stock transfer"
                                          && b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                if (obj.StockTransferDetails != null)
                {
                    foreach (var StockAdjustment in obj.StockTransferDetails)
                    {
                        //decimal convertedStock = oCommonController.StockConversion(StockAdjustment.Quantity, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                        //decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.FromBranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                        //if (remainingQty < convertedStock)
                        //{
                        //    data = new
                        //    {
                        //        Status = 0,
                        //        Message = "Only " + remainingQty + " quantity is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId)
                        //        .Select(a => a.ItemName).FirstOrDefault(),
                        //        Data = new
                        //        {
                        //        }
                        //    };
                        //    return await Task.FromResult(Ok(data));
                        //}

                        if (oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.ToBranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Count() == 0)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId)
                                                            .Select(a => a.ItemName).FirstOrDefault() + " is not available in Branch " +
                                                            oConnectionContext.DbClsBranch.Where(d => d.BranchId == obj.ToBranchId).Select(d => d.Branch).FirstOrDefault(),
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                    }
                }

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                if (EnableLotNo == true)
                {
                    if (obj.StockTransferDetails != null)
                    {
                        foreach (var Sales in obj.StockTransferDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                decimal remainingQty = 0;
                                //string LotNo = "";
                                if (Sales.LotType == "openingstock")
                                {
                                    remainingQty = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == Sales.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                                    //LotNo = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == Sales.LotId).Select(a => a.LotNo).FirstOrDefault();
                                }
                                else if (Sales.LotType == "purchase")
                                {
                                    remainingQty = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Sales.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                                    //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Sales.LotId).Select(a => a.LotNo).FirstOrDefault();
                                }
                                else if (Sales.LotType == "stocktransfer")
                                {
                                    remainingQty = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                                    //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Sales.LotId).Select(a => a.LotNo).FirstOrDefault();
                                }

                                decimal convertedStock = 0;
                                foreach (var inner in obj.StockTransferDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.LotType == inner.LotType && Sales.LotId == inner.LotId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity, inner.ItemId, inner.PriceAddedFor);
                                        }
                                    }

                                }
                                if (remainingQty < convertedStock)
                                {
                                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+Sales.DivId });
                                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                    isError = true;
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
                }
                else
                {
                    if (obj.StockTransferDetails != null)
                    {
                        foreach (var Sales in obj.StockTransferDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.FromBranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();

                                decimal convertedStock = 0;
                                foreach (var inner in obj.StockTransferDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity, inner.ItemId, inner.PriceAddedFor);
                                        }
                                    }
                                }
                                if (remainingQty < convertedStock)
                                {
                                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+Sales.DivId });
                                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                    isError = true;
                                }

                                //if (remainingQty < convertedStock)
                                //{
                                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                //    isError = true;

                                //    //data = new
                                //    //{
                                //    //    Status = 0,
                                //    //    Message = "Only " + remainingQty + " quantity is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId)
                                //    //    .Select(a => a.ItemName).FirstOrDefault(),
                                //    //    Data = new
                                //    //    {
                                //    //    }
                                //    //};
                                //    //return await Task.FromResult(Ok(data));
                                //}
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
                }

                ClsStockTransfer oClsStockTransfer = new ClsStockTransfer()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    FromBranchId = obj.FromBranchId,
                    ToBranchId = obj.ToBranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    StockTransferId = obj.StockTransferId,
                    ReferenceNo = obj.ReferenceNo,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    Status = obj.Status,
                    Notes = obj.Notes,
                    TotalQuantity = obj.TotalQuantity,
                    TotalAmount = obj.TotalAmount,
                    Subtotal = obj.Subtotal,
                    InvoiceId = oCommonController.CreateToken(),
                    StockTransferReasonId = obj.StockTransferReasonId
                };

                oConnectionContext.DbClsStockTransfer.Add(oClsStockTransfer);
                oConnectionContext.SaveChanges();

                string query = "";
                if (obj.StockTransferDetails != null)
                {
                    foreach (var StockTransfer in obj.StockTransferDetails)
                    {
                        decimal convertedStock = oCommonController.StockConversion(StockTransfer.Quantity, StockTransfer.ItemId, StockTransfer.PriceAddedFor);

                        if (obj.Status == 3)
                        {
                            if (StockTransfer.LotId == 0)
                            {
                                StockTransfer.StockDeductionIds = oCommonController.deductStock(obj.FromBranchId, StockTransfer.ItemDetailsId, convertedStock, StockTransfer.ItemId, StockTransfer.PriceAddedFor);
                            }
                            else
                            {
                                StockTransfer.StockDeductionIds = oCommonController.deductStockLot(obj.FromBranchId, StockTransfer.ItemDetailsId, convertedStock, StockTransfer.LotId, StockTransfer.LotType);
                            }

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + convertedStock + " where \"BranchId\"=" + obj.ToBranchId + " and \"ItemId\"=" + StockTransfer.ItemId + " and \"ItemDetailsId\"=" + StockTransfer.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }

                        if (StockTransfer.LotType == "stocktransfer")
                        {
                            StockTransfer.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.LotId).Select(a => a.LotIdForLotNoChecking).FirstOrDefault();
                            StockTransfer.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.LotId).Select(a => a.LotTypeForLotNoChecking).FirstOrDefault();
                        }
                        else
                        {
                            StockTransfer.LotIdForLotNoChecking = StockTransfer.LotId;
                            StockTransfer.LotTypeForLotNoChecking = StockTransfer.LotType;
                        }

                        ClsStockTransferDetails oClsStockTransferDetails = new ClsStockTransferDetails()
                        {
                            Amount = StockTransfer.Amount,
                            ItemId = StockTransfer.ItemId,
                            ItemDetailsId = StockTransfer.ItemDetailsId,
                            StockTransferId = oClsStockTransfer.StockTransferId,
                            Quantity = StockTransfer.Quantity,
                            UnitCost = StockTransfer.UnitCost,
                            IsActive = StockTransfer.IsActive,
                            IsDeleted = StockTransfer.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            QuantityRemaining = convertedStock,
                            QuantitySold = 0,
                            //StockDeductionIds = StockTransfer.StockDeductionIds,
                            PriceAddedFor = StockTransfer.PriceAddedFor,
                            LotId = StockTransfer.LotId,
                            LotType = StockTransfer.LotType,
                            IsStopSelling = false,
                            UnitAddedFor = StockTransfer.UnitAddedFor,
                            LotIdForLotNoChecking = StockTransfer.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = StockTransfer.LotTypeForLotNoChecking,
                            QuantityTransferred= convertedStock,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsStockTransferDetails.Add(oClsStockTransferDetails);
                        oConnectionContext.SaveChanges();

                        //string ll = "delete from tblStockTransferDeductionId where StockTransferDetailsId=" + oClsStockTransferDetails.StockTransferDetailsId;
                        //oConnectionContext.Database.ExecuteSqlCommand(ll);

                        if (StockTransfer.StockDeductionIds != null)
                        {
                            foreach (var l in StockTransfer.StockDeductionIds)
                            {
                                ClsStockTransferDeductionId oClsStockTransferDeductionId = new ClsStockTransferDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    StockTransferDetailsId = oClsStockTransferDetails.StockTransferDetailsId,
                                    StockTransferId = oClsStockTransfer.StockTransferId,
                                };
                                oConnectionContext.DbClsStockTransferDeductionId.Add(oClsStockTransferDeductionId);
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Transfer",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Transfer \"" + obj.ReferenceNo+"\" created",
                    Id = oClsStockTransfer.StockTransferId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Transfer created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockTransferDelete(ClsStockTransferVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var stockTransferBranches = oConnectionContext.DbClsStockTransfer.Where(a => a.StockTransferId == obj.StockTransferId)
                    .Select(a => new { a.FromBranchId, a.ToBranchId }).FirstOrDefault();

                var details = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferId == obj.StockTransferId && a.IsDeleted == false).Select(a => new
                {
                    a.StockTransferDetailsId,
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    a.QuantityRemaining,
                    a.PriceAddedFor,
                }).ToList();

                foreach (var item in details)
                {
                    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    if (convertedStock != item.QuantityRemaining)
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
                }

                ClsStockTransfer oClsStockTransfer = new ClsStockTransfer()
                {
                    StockTransferId = obj.StockTransferId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsStockTransfer.Attach(oClsStockTransfer);
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.StockTransferId).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    //Release stock
                    //string StockDeductionIds = oConnectionContext.DbClsStockTransferDetails.
                    //    Where(a => a.StockTransferDetailsId == item.StockTransferDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockTransferDeductionId.Where(a => a.StockTransferDetailsId
                        == item.StockTransferDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    foreach (var res in _StockDeductionIds)
                    {

                        if (res.Type == "purchase")
                        {
                            query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        else if (res.Type == "openingstock")
                        {
                            query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        //else if (res.Type == "stockadjustment")
                        //{
                        //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockAdjustmentDetailsId=" + res.Id;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                        //}
                        else if (res.Type == "stocktransfer")
                        {
                            query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }

                        query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + res.Quantity + " where \"BranchId\"=" + stockTransferBranches.FromBranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + res.Quantity + " where \"BranchId\"=" + stockTransferBranches.ToBranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    };
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Transfer",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Transfer \"" + oConnectionContext.DbClsStockTransfer.Where(a => a.StockTransferId == obj.StockTransferId).Select(a => a.ReferenceNo).FirstOrDefault()+"\" deleted",
                    Id = oClsStockTransfer.StockTransferId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Transfer deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateStockTransfer(ClsStockTransferVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.FromBranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFromBranch" });
                    isError = true;
                }

                if (obj.ToBranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divToBranch" });
                    isError = true;
                }

                if (obj.FromBranchId != 0 && obj.FromBranchId == obj.ToBranchId)
                {
                    errors.Add(new ClsError { Message = "From Business Location & To Business Location cannot be same", Id = "divToBranch" });
                    isError = true;
                }

                if (obj.StockTransferReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStockTransferReason" });
                    isError = true;
                }

                //if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                //{
                //    if (oConnectionContext.DbClsStockTransfer.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.StockTransferId== obj.StockTransferId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Reference No exists", Id = "divReferenceNo" });
                //        isError = true;
                //    }
                //}

                if (obj.StockTransferDetails == null || obj.StockTransferDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.StockTransferDetails != null)
                {
                    foreach (var item in obj.StockTransferDetails)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "Quantity is required", Id = "divQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.UnitCost <= 0)
                            {
                                errors.Add(new ClsError { Message = "This is required", Id = "divUnitCost" + item.DivId });
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

                string query = "";

                if (obj.StockTransferDetails != null)
                {
                    foreach (var StockAdjustment in obj.StockTransferDetails)
                    {
                        if (StockAdjustment.StockTransferDetailsId == 0)
                        {
                            //decimal convertedStock = oCommonController.StockConversion(StockAdjustment.Quantity, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                            //decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.FromBranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                            //if (remainingQty < convertedStock)
                            //{
                            //    data = new
                            //    {
                            //        Status = 0,
                            //        Message = "Only " + remainingQty + " quantity is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId)
                            //        .Select(a => a.ItemName).FirstOrDefault(),
                            //        Data = new
                            //        {
                            //        }
                            //    };
                            //    return await Task.FromResult(Ok(data));
                            //}

                            if (oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.ToBranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Count() == 0)
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId)
                                                                .Select(a => a.ItemName).FirstOrDefault() + " is not available in Branch " +
                                                                oConnectionContext.DbClsBranch.Where(d => d.BranchId == obj.ToBranchId).Select(d => d.Branch).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                        else
                        {
                            //Release stock
                            //string StockDeductionIds = oConnectionContext.DbClsStockTransferDetails.
                            //Where(a => a.StockTransferDetailsId == StockAdjustment.StockTransferDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault() ?? "[]";
                            //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                            List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockTransferDeductionId.Where(a => a.StockTransferDetailsId
                        == StockAdjustment.StockTransferDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                            foreach (var res in _StockDeductionIds)
                            {
                                if (res.Type == "purchase")
                                {
                                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (res.Type == "openingstock")
                                {
                                    query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (res.Type == "stocktransfer")
                                {
                                    query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }

                                query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + res.Quantity + ") where \"BranchId\"=" + obj.FromBranchId + " and \"ItemDetailsId\"=" + StockAdjustment.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + res.Quantity + ") where \"BranchId\"=" + obj.ToBranchId + " and \"ItemDetailsId\"=" + StockAdjustment.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            };

                            if(StockAdjustment.IsDeleted == true)
                            {
                                query = "update \"tblStockTransferDetails\" set \"IsDeleted\"=True where \"StockTransferDetailsId\"=" + StockAdjustment.StockTransferDetailsId;
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }

                            //decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.FromBranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                            //if (remainingQty < StockAdjustment.Quantity)
                            //{
                            //    data = new
                            //    {
                            //        Status = 0,
                            //        Message = "Only " + remainingQty + " quantity is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId)
                            //        .Select(a => a.ItemName).FirstOrDefault(),
                            //        Data = new
                            //        {
                            //        }
                            //    };
                            //    return await Task.FromResult(Ok(data));
                            //}

                            if (oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.ToBranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Count() == 0)
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId)
                                                                .Select(a => a.ItemName).FirstOrDefault() + " is not available in Branch " +
                                                                oConnectionContext.DbClsBranch.Where(d => d.BranchId == obj.ToBranchId).Select(d => d.Branch).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                    }
                }

                obj.StockTransferDetails.RemoveAll(r => r.IsDeleted == true);

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                if (EnableLotNo == true)
                {
                    if (obj.StockTransferDetails != null)
                    {
                        foreach (var Sales in obj.StockTransferDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                decimal remainingQty = 0;
                                //string LotNo = "";
                                if (Sales.LotType == "openingstock")
                                {
                                    remainingQty = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == Sales.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                                    //LotNo = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == Sales.LotId).Select(a => a.LotNo).FirstOrDefault();
                                }
                                else if (Sales.LotType == "purchase")
                                {
                                    remainingQty = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Sales.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                                    //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Sales.LotId).Select(a => a.LotNo).FirstOrDefault();
                                }
                                else if (Sales.LotType == "stocktransfer")
                                {
                                    remainingQty = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                                    //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Sales.LotId).Select(a => a.LotNo).FirstOrDefault();
                                }
                                else
                                {
                                    if(Sales.StockTransferDetailsId !=0)
                                    {
                                        remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.FromBranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                    }
                                }

                                decimal convertedStock = 0;
                                foreach (var inner in obj.StockTransferDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.LotType == inner.LotType && Sales.LotId == inner.LotId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity, inner.ItemId, inner.PriceAddedFor);
                                        }
                                    }

                                }
                                if (remainingQty < convertedStock)
                                {
                                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+Sales.DivId });
                                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                    isError = true;
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
                }
                else
                {
                    if (obj.StockTransferDetails != null)
                    {
                        foreach (var Sales in obj.StockTransferDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.FromBranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();

                                decimal convertedStock = 0;
                                foreach (var inner in obj.StockTransferDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity, inner.ItemId, inner.PriceAddedFor);
                                        }
                                    }
                                }
                                if (remainingQty < convertedStock)
                                {
                                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+Sales.DivId });
                                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                    isError = true;
                                }

                                //if (remainingQty < convertedStock)
                                //{
                                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                //    isError = true;

                                //    //data = new
                                //    //{
                                //    //    Status = 0,
                                //    //    Message = "Only " + remainingQty + " quantity is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId)
                                //    //    .Select(a => a.ItemName).FirstOrDefault(),
                                //    //    Data = new
                                //    //    {
                                //    //    }
                                //    //};
                                //    //return await Task.FromResult(Ok(data));
                                //}
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
                }

                ClsStockTransfer oClsStockTransfer = new ClsStockTransfer()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    FromBranchId = obj.FromBranchId,
                    ToBranchId = obj.ToBranchId,
                    StockTransferId = obj.StockTransferId,
                    ReferenceNo = obj.ReferenceNo,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    Status = obj.Status,
                    Notes = obj.Notes,
                    TotalQuantity = obj.TotalQuantity,
                    TotalAmount = obj.TotalAmount,
                    Subtotal = obj.Subtotal,
                    StockTransferReasonId=obj.StockTransferReasonId
                };

                oConnectionContext.DbClsStockTransfer.Attach(oClsStockTransfer);
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.FromBranchId).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ToBranchId).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ReferenceNo).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.Date).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.TotalAmount).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.StockTransferReasonId).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.StockTransferDetails != null)
                {
                    foreach (var StockTransfer in obj.StockTransferDetails)
                    {
                        decimal convertedStock = oCommonController.StockConversion(StockTransfer.Quantity, StockTransfer.ItemId, StockTransfer.PriceAddedFor);

                        if (StockTransfer.StockTransferDetailsId == 0)
                        {
                            if (obj.Status == 3)
                            {
                                if (StockTransfer.LotId == 0)
                                {
                                    StockTransfer.StockDeductionIds = oCommonController.deductStock(obj.FromBranchId, StockTransfer.ItemDetailsId, convertedStock, StockTransfer.ItemId, StockTransfer.PriceAddedFor);
                                }
                                else
                                {
                                    StockTransfer.StockDeductionIds = oCommonController.deductStockLot(obj.FromBranchId, StockTransfer.ItemDetailsId, convertedStock, StockTransfer.LotId, StockTransfer.LotType);
                                }

                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + convertedStock + " where \"BranchId\"=" + obj.ToBranchId + " and \"ItemId\"=" + StockTransfer.ItemId + " and \"ItemDetailsId\"=" + StockTransfer.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }

                            if (StockTransfer.LotType == "stocktransfer")
                            {
                                StockTransfer.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.LotId).Select(a => a.LotIdForLotNoChecking).FirstOrDefault();
                                StockTransfer.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.LotId).Select(a => a.LotTypeForLotNoChecking).FirstOrDefault();
                            }
                            else
                            {
                                StockTransfer.LotIdForLotNoChecking = StockTransfer.LotId;
                                StockTransfer.LotTypeForLotNoChecking = StockTransfer.LotType;
                            }

                            long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == StockTransfer.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();

                            ClsStockTransferDetails oClsStockTransferDetails = new ClsStockTransferDetails()
                            {
                                Amount = StockTransfer.Amount,
                                ItemId = StockTransfer.ItemId,
                                ItemDetailsId = StockTransfer.ItemDetailsId,
                                StockTransferId = oClsStockTransfer.StockTransferId,
                                Quantity = StockTransfer.Quantity,
                                UnitCost = StockTransfer.UnitCost,
                                IsActive = StockTransfer.IsActive,
                                IsDeleted = StockTransfer.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                QuantityRemaining = convertedStock,
                                QuantitySold = 0,
                                //StockDeductionIds = StockTransfer.StockDeductionIds,
                                PriceAddedFor = StockTransfer.PriceAddedFor,
                                LotId = StockTransfer.LotId,
                                LotType = StockTransfer.LotType,
                                IsStopSelling = false,
                                UnitAddedFor = StockTransfer.UnitAddedFor,
                                LotIdForLotNoChecking = StockTransfer.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = StockTransfer.LotTypeForLotNoChecking,
                                QuantityTransferred = convertedStock,
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsStockTransferDetails.Add(oClsStockTransferDetails);
                            oConnectionContext.SaveChanges();

                            //string ll = "delete from tblStockTransferDeductionId where StockTransferDetailsId=" + oClsStockTransferDetails.StockTransferDetailsId;
                            //oConnectionContext.Database.ExecuteSqlCommand(ll);

                            if (StockTransfer.StockDeductionIds != null)
                            {
                                foreach (var l in StockTransfer.StockDeductionIds)
                                {
                                    ClsStockTransferDeductionId oClsStockTransferDeductionId = new ClsStockTransferDeductionId()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        Id = l.Id,
                                        Type = l.Type,
                                        Quantity = l.Quantity,
                                        StockTransferDetailsId = oClsStockTransferDetails.StockTransferDetailsId,
                                        StockTransferId = oClsStockTransfer.StockTransferId,
                                    };
                                    oConnectionContext.DbClsStockTransferDeductionId.Add(oClsStockTransferDeductionId);
                                    oConnectionContext.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            decimal QuantitySold = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.StockTransferDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                            if (obj.Status == 3)
                            {
                                if (StockTransfer.LotId == 0)
                                {
                                    StockTransfer.StockDeductionIds = oCommonController.deductStock(obj.FromBranchId, StockTransfer.ItemDetailsId, convertedStock, StockTransfer.ItemId, StockTransfer.PriceAddedFor);
                                }
                                else
                                {
                                    StockTransfer.StockDeductionIds = oCommonController.deductStockLot(obj.FromBranchId, StockTransfer.ItemDetailsId, convertedStock, StockTransfer.LotId, StockTransfer.LotType);
                                }

                                //if(obj.Status == 3)
                                //{
                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + convertedStock + " where \"BranchId\"=" + obj.ToBranchId + " and \"ItemId\"=" + StockTransfer.ItemId + " and \"ItemDetailsId\"=" + StockTransfer.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                //}
                            }

                            if (StockTransfer.LotType == "stocktransfer")
                            {
                                StockTransfer.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.LotId).Select(a => a.LotIdForLotNoChecking).FirstOrDefault();
                                StockTransfer.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockTransfer.LotId).Select(a => a.LotTypeForLotNoChecking).FirstOrDefault();
                            }
                            else
                            {
                                StockTransfer.LotIdForLotNoChecking = StockTransfer.LotId;
                                StockTransfer.LotTypeForLotNoChecking = StockTransfer.LotType;
                            }

                            long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == StockTransfer.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();

                            ClsStockTransferDetails oClsStockTransferDetails = new ClsStockTransferDetails()
                            {
                                StockTransferDetailsId = StockTransfer.StockTransferDetailsId,
                                Amount = StockTransfer.Amount,
                                ItemId = StockTransfer.ItemId,
                                ItemDetailsId = StockTransfer.ItemDetailsId,
                                StockTransferId = oClsStockTransfer.StockTransferId,
                                Quantity = StockTransfer.Quantity,
                                UnitCost = StockTransfer.UnitCost,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                QuantityRemaining = convertedStock - QuantitySold,
                                //StockDeductionIds = StockTransfer.StockDeductionIds,
                                PriceAddedFor = StockTransfer.PriceAddedFor,
                                LotId = StockTransfer.LotId,
                                LotType = StockTransfer.LotType,
                                UnitAddedFor = StockTransfer.UnitAddedFor,
                                LotIdForLotNoChecking = StockTransfer.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = StockTransfer.LotTypeForLotNoChecking,
                                QuantityTransferred = convertedStock,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsStockTransferDetails.Attach(oClsStockTransferDetails);
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.Amount).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.StockTransferId).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            //oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.StockDeductionIds).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.LotId).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.LotType).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsStockTransferDetails).Property(x => x.QuantityTransferred).IsModified = true;
                            oConnectionContext.SaveChanges();

                            string ll = "delete from \"tblStockTransferDeductionId\" where \"StockTransferDetailsId\"=" + StockTransfer.StockTransferDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(ll);

                            if (StockTransfer.StockDeductionIds != null)
                            {
                                foreach (var l in StockTransfer.StockDeductionIds)
                                {
                                    ClsStockTransferDeductionId oClsStockTransferDeductionId = new ClsStockTransferDeductionId()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        Id = l.Id,
                                        Type = l.Type,
                                        Quantity = l.Quantity,
                                        StockTransferDetailsId = StockTransfer.StockTransferDetailsId,
                                        StockTransferId = oClsStockTransfer.StockTransferId,
                                    };
                                    oConnectionContext.DbClsStockTransferDeductionId.Add(oClsStockTransferDeductionId);
                                    oConnectionContext.SaveChanges();
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Transfer",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Transfer \"" + obj.ReferenceNo+"\" updated",
                    Id = oClsStockTransfer.StockTransferId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Transfer updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockTransferDetailsDelete(ClsStockTransferDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.StockTransferId != 0)
                {
                    var stockTransferBranches = oConnectionContext.DbClsStockTransfer.Where(a => a.StockTransferId == obj.StockTransferId).Select(a => new
                    {
                        a.FromBranchId,
                        a.ToBranchId
                    }).FirstOrDefault();

                    var details = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferId == obj.StockTransferId && a.IsDeleted == false).Select(a => new
                    {
                        a.StockTransferDetailsId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.QuantityRemaining,
                        a.PriceAddedFor
                    }).ToList();

                    foreach (var item in details)
                    {
                        decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                        if (convertedStock != item.QuantityRemaining)
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
                    }

                    //string query = "update tblStockTransferDetails set IsDeleted=1 where StockTransferId=" + obj.StockTransferId;
                    ////ConnectionContext ocon = new ConnectionContext();
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    //foreach (var item in details)
                    //{
                    //    //Release stock
                    //    //string StockDeductionIds = oConnectionContext.DbClsStockTransferDetails.
                    //    //    Where(a => a.StockTransferDetailsId == item.StockTransferDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                    //    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                    //    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockTransferDeductionId.Where(a => a.StockTransferDetailsId
                    //    == item.StockTransferDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    //    foreach (var res in _StockDeductionIds)
                    //    {

                    //        if (res.Type == "purchase")
                    //        {
                    //            query = "update tblPurchaseDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where PurchaseDetailsId=" + res.Id;
                    //            oConnectionContext.Database.ExecuteSqlCommand(query);
                    //        }
                    //        else if (res.Type == "openingstock")
                    //        {
                    //            query = "update tblOpeningStock set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where OpeningStockId=" + res.Id;
                    //            oConnectionContext.Database.ExecuteSqlCommand(query);
                    //        }
                    //        //else if (res.Type == "stockadjustment")
                    //        //{
                    //        //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockAdjustmentDetailsId=" + res.Id;
                    //        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                    //        //}
                    //        else if (res.Type == "stocktransfer")
                    //        {
                    //            query = "update tblStockTransferDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockTransferDetailsId=" + res.Id;
                    //            oConnectionContext.Database.ExecuteSqlCommand(query);
                    //        }

                    //        query = "update tblItemBranchMap set Quantity=Quantity,0)+" + res.Quantity + " where BranchId=" + stockTransferBranches.FromBranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);

                    //        query = "update tblItemBranchMap set Quantity=Quantity,0)-" + res.Quantity + " where BranchId=" + stockTransferBranches.ToBranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    };
                    //}
                }
                else
                {
                    var stockTransferBranches = oConnectionContext.DbClsStockTransfer.Where(a => a.StockTransferId == oConnectionContext.DbClsStockTransferDetails.Where(b=>b.StockTransferDetailsId == obj.StockTransferDetailsId).Select(b=>b.StockTransferId).FirstOrDefault()).Select(a => new
                    {
                        a.FromBranchId,
                        a.ToBranchId
                    }).FirstOrDefault();

                    var details = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == obj.StockTransferDetailsId).Select(a => new
                    {
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.QuantityRemaining
                    }).FirstOrDefault();

                    //string query = "update tblStockTransferDetails set IsDeleted=1 where StockTransferDetailsId=" + obj.StockTransferDetailsId;
                    ////ConnectionContext ocon = new ConnectionContext();
                    //oConnectionContext.Database.ExecuteSqlCommand(query);


                    ////Release stock
                    ////string StockDeductionIds = oConnectionContext.DbClsStockTransferDetails.
                    ////    Where(a => a.StockTransferDetailsId == obj.StockTransferDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                    ////List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                    //List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockTransferDeductionId.Where(a => a.StockTransferDetailsId
                    //    == obj.StockTransferDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    //foreach (var res in _StockDeductionIds)
                    //{

                    //    if (res.Type == "purchase")
                    //    {
                    //        query = "update tblPurchaseDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where PurchaseDetailsId=" + res.Id;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    }
                    //    else if (res.Type == "openingstock")
                    //    {
                    //        query = "update tblOpeningStock set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where OpeningStockId=" + res.Id;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    }
                    //    //else if (res.Type == "stockadjustment")
                    //    //{
                    //    //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockAdjustmentDetailsId=" + res.Id;
                    //    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    //}
                    //    else if (res.Type == "stocktransfer")
                    //    {
                    //        query = "update tblStockTransferDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockTransferDetailsId=" + res.Id;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    }

                    //    query = "update tblItemBranchMap set Quantity=Quantity,0)+" + res.Quantity + " where BranchId=" + stockTransferBranches.FromBranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
                    //    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + res.Quantity + " where BranchId=" + stockTransferBranches.ToBranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                    //};
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

        [AllowAnonymous]
        public async Task<IHttpActionResult> Invoice(ClsStockTransferVm obj)
        {
            var det = oConnectionContext.DbClsStockTransfer.Where(a => a.IsDeleted == false && a.InvoiceId == obj.InvoiceId).Select(a => new
            {
                a.StockTransferId,
                FromBranchDetails = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.FromBranchId).Select(b => new
                {
                    b.Branch,
                    Mobile = b.Mobile,
                    b.Email,
                    b.TaxNo,
                    Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == b.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                    b.Address
                }).FirstOrDefault(),
                ToBranchDetails = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.ToBranchId).Select(b => new
                {
                    b.Branch,
                    Mobile = b.Mobile,
                    b.Email,
                    b.TaxNo,
                    Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == b.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                    b.Address
                }).FirstOrDefault(),
                a.Notes,
                a.ReferenceNo,
                a.Date,
                a.TotalAmount,
                a.Subtotal,
                a.TotalQuantity,
                a.Status,
                a.CompanyId,
                StockTransferDetails = (from b in oConnectionContext.DbClsStockTransferDetails
                                        join c in oConnectionContext.DbClsItemDetails
                                on b.ItemDetailsId equals c.ItemDetailsId
                                        join d in oConnectionContext.DbClsItem
                                        on c.ItemId equals d.ItemId
                                        where b.StockTransferId == a.StockTransferId && b.IsDeleted == false
                                        select new
                                        {
                                            d.ProductImage,
                                            Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                            : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                            : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                            : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                            b.StockTransferDetailsId,
                                            b.Amount,
                                            b.StockTransferId,
                                            b.Quantity,
                                            b.UnitCost,
                                            d.ItemId,
                                            d.ProductType,
                                            c.ItemDetailsId,
                                            d.ItemName,
                                            c.SKU,
                                            c.VariationDetailsId,
                                            VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                            UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                            c.SalesExcTax,
                                            c.SalesIncTax,
                                            c.TotalCost,
                                            d.TaxType,
                                            d.ItemCode
                                        }).ToList(),
            }).FirstOrDefault();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                a.BusinessLogo,
                a.BusinessName,
                a.DateFormat,
                a.TimeFormat,
                a.CurrencySymbolPlacement,
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockTransfer = det,
                    BusinessSetting = BusinessSetting,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateStockTransferStatus(ClsStockTransferVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Status == 0)
                {
                    errors.Add(new ClsError { Message = "Status is required", Id = "divStatus_M" });
                    isError = true;
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

                ClsStockTransfer oClsStockTransfer = new ClsStockTransfer()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    StockTransferId = obj.StockTransferId,
                    Status = obj.Status,
                };

                oConnectionContext.DbClsStockTransfer.Attach(oClsStockTransfer);
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.StockTransferId).IsModified = true;
                oConnectionContext.Entry(oClsStockTransfer).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Transfer",
                    CompanyId = obj.CompanyId,
                    Description = "modified " + oConnectionContext.DbClsStockTransfer.Where(a => a.StockTransferId == obj.StockTransferId).Select(a => a.ReferenceNo).FirstOrDefault(),
                    Id = oClsStockTransfer.StockTransferId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                //string[] arr = oCommonController.SendNotifications(obj.Status, obj.CompanyId, oClsSales.SalesId, obj.SmsSettingsId, obj.EmailSettingsId, obj.WhatsappSettingsId, obj.AddedBy, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Status updated successfully",
                    //WhatsappUrl = arr[2],
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
