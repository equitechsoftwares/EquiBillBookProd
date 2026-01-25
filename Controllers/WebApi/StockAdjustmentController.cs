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
    public class StockAdjustmentController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllStockAdjustments(ClsStockAdjustmentVm obj)
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

            var det = oConnectionContext.DbClsStockAdjustment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            //&& a.BranchId == obj.BranchId
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new
                {
                    a.StockAdjustmentReasonId,
                    StockAdjustmentReason = oConnectionContext.DbClsStockAdjustmentReason.Where(b=>b.StockAdjustmentReasonId == a.StockAdjustmentReasonId).Select(b=>b.StockAdjustmentReason).FirstOrDefault(),
                    a.InvoiceId,
                    a.StockDeductionType,
                    StockAdjustmentId = a.StockAdjustmentId,
                    a.BranchId,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    a.ReferenceNo,
                    a.AdjustmentDate,
                    a.AdjustmentType,
                    a.TotalAmount,
                    a.TotalAmountRecovered,
                    a.Notes,
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    a.TotalQuantity,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    TotalItems = oConnectionContext.DbClsStockAdjustmentDetails.Where(c => c.StockAdjustmentId == a.StockAdjustmentId &&
                    c.IsDeleted == false).Count()
                }).ToList();

            if (obj.BranchId != 0)
            {
                det = det.Where(a => a.BranchId == obj.BranchId).Select(a => a).ToList();
            }

            if (obj.AdjustmentType != null && obj.AdjustmentType != "")
            {
                det = det.Where(a => a.AdjustmentType == obj.AdjustmentType).Select(a => a).ToList();
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
                    StockAdjustments = det.OrderByDescending(a => a.StockAdjustmentId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> StockAdjustment(ClsStockAdjustment obj)
        {
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
            var det = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.StockAdjustmentReasonId,
                a.AccountId,
                a.InvoiceId,
                a.StockDeductionType,
                StockAdjustmentId = a.StockAdjustmentId,
                a.BranchId,
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                a.ReferenceNo,
                a.AdjustmentDate,
                a.AdjustmentType,
                a.TotalAmount,
                a.TotalAmountRecovered,
                a.Notes,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                a.TotalQuantity,
                StockAdjustmentDetails = (from b in oConnectionContext.DbClsStockAdjustmentDetails
                                          join c in oConnectionContext.DbClsItemDetails
                                          on b.ItemDetailsId equals c.ItemDetailsId
                                          join d in oConnectionContext.DbClsItem
                                          on c.ItemId equals d.ItemId
                                          join e in oConnectionContext.DbClsItemBranchMap
                                          on b.ItemDetailsId equals e.ItemDetailsId
                                          where b.StockAdjustmentId == a.StockAdjustmentId && b.IsDeleted == false
                                          && e.BranchId == a.BranchId
                                          select new
                                          {
                                              b.AccountId,
                                              Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
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
                                              //          QuantityRemaining = a.AdjustmentType == "Debit" ? (b.QuantityRemaining + b.LotType == "purchase" ?
                                              //oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                              //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                              //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                              //: b.QuantityRemaining) : b.QuantityRemaining,
                                              QuantityRemaining = a.AdjustmentType.ToLower() == "debit" ? (b.QuantityRemaining + (b.LotType == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    : b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    : b.LotType == "stocktransfer" ? oConnectionContext.DbClsStockTransferDetails.Where(f => f.StockTransferDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                    : e.Quantity)) : e.Quantity,
                                              //b.QuantityRemaining,
                                              b.StockAdjustmentDetailsId,
                                              b.Amount,
                                              b.Quantity,
                                              b.UnitCost,
                                              d.ItemId,
                                              d.ProductType,
                                              c.ItemDetailsId,
                                              d.ItemName,
                                              SKU = d.ProductType == "Variable" ? c.SKU : d.SkuCode,
                                              c.VariationDetailsId,
                                              VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                              UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                              c.SalesExcTax,
                                              //b.SalesIncTax,
                                              c.TotalCost,
                                              d.ItemCode,
                                              //b.ManufacturingDate,
                                              //b.ExpiryDate,
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
                                              AllowDecimal = oConnectionContext.DbClsUnit.Where(c => c.UnitId == d.UnitId).Select(c => c.AllowDecimal).FirstOrDefault(),
                                              SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == d.SecondaryUnitId).Select(c => c.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                              TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == d.TertiaryUnitId).Select(c => c.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                              QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == d.QuaternaryUnitId).Select(c => c.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                              PriceAddedFor = b.PriceAddedFor,
                                          }).ToList(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockAdjustment = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertStockAdjustment(ClsStockAdjustmentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;

                if (obj.AdjustmentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAdjustmentDate" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.StockAdjustmentReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStockAdjustmentReason" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsStockAdjustment.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Reference No exists", Id = "divReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.StockAdjustmentDetails == null || obj.StockAdjustmentDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.StockAdjustmentDetails != null)
                {
                    foreach (var item in obj.StockAdjustmentDetails)
                    {
                        if(item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "This is required", Id = "divQuantity" + item.DivId });
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

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "stock adjustment"
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

                if (obj.AdjustmentType.ToLower() == "debit")
                {
                    var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                    if (EnableLotNo == true)
                    {
                        if (obj.StockAdjustmentDetails != null)
                        {
                            foreach (var Sales in obj.StockAdjustmentDetails)
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
                                    foreach (var inner in obj.StockAdjustmentDetails)
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
                        if (obj.StockAdjustmentDetails != null)
                        {
                            foreach (var Sales in obj.StockAdjustmentDetails)
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                                    decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();

                                    decimal convertedStock = 0;
                                    foreach (var inner in obj.StockAdjustmentDetails)
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
                }

                ClsStockAdjustment oClsStockAdjustment = new ClsStockAdjustment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    BranchId = obj.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    StockAdjustmentId = obj.StockAdjustmentId,
                    ReferenceNo = obj.ReferenceNo,
                    AdjustmentDate = obj.AdjustmentDate.AddHours(5).AddMinutes(30),
                    AdjustmentType = obj.AdjustmentType,
                    Notes = obj.Notes,
                    TotalQuantity = obj.TotalQuantity,
                    TotalAmount = obj.TotalAmount,
                    TotalAmountRecovered = obj.TotalAmountRecovered,
                    StockDeductionType = obj.StockDeductionType,
                    InvoiceId = oCommonController.CreateToken(),
                    AccountId = obj.AccountId,
                    PrefixId = PrefixId,
                    StockAdjustmentReasonId = obj.StockAdjustmentReasonId,
                };

                oConnectionContext.DbClsStockAdjustment.Add(oClsStockAdjustment);
                oConnectionContext.SaveChanges();

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays }).FirstOrDefault();

                if (obj.StockAdjustmentDetails != null)
                {
                    foreach (var StockAdjustment in obj.StockAdjustmentDetails)
                    {
                        decimal convertedStock = oCommonController.StockConversion(StockAdjustment.Quantity, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);

                        if (obj.AdjustmentType.ToLower() == "debit")
                        {
                            if (StockAdjustment.LotId == 0)
                            {
                                StockAdjustment.StockDeductionIds = oCommonController.deductStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                            }
                            else
                            {
                                StockAdjustment.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.LotId, StockAdjustment.LotType);
                            }

                            //StockAdjustment.StockDeductionIds = oCommonController.deductStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                        }
                        else
                        {
                            if (StockAdjustment.LotId == 0)
                            {
                                StockAdjustment.StockDeductionIds = oCommonController.addStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                            }
                            else
                            {
                                StockAdjustment.StockDeductionIds = oCommonController.addStockLot(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.LotId, StockAdjustment.LotType);
                            }
                        }

                        if (StockAdjustment.LotType == "stocktransfer")
                        {
                            StockAdjustment.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockAdjustment.LotId).Select(a => a.LotId).FirstOrDefault();
                            StockAdjustment.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockAdjustment.LotId).Select(a => a.LotType).FirstOrDefault();
                        }
                        else
                        {
                            StockAdjustment.LotIdForLotNoChecking = StockAdjustment.LotId;
                            StockAdjustment.LotTypeForLotNoChecking = StockAdjustment.LotType;
                        }

                        long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();

                        ClsStockAdjustmentDetails oClsStockAdjustmentDetails = new ClsStockAdjustmentDetails()
                        {
                            Amount = StockAdjustment.Amount,
                            ItemId = StockAdjustment.ItemId,
                            ItemDetailsId = StockAdjustment.ItemDetailsId,
                            StockAdjustmentId = oClsStockAdjustment.StockAdjustmentId,
                            Quantity = StockAdjustment.Quantity,
                            UnitCost = StockAdjustment.UnitCost,
                            IsActive = StockAdjustment.IsActive,
                            IsDeleted = StockAdjustment.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            QuantityRemaining = convertedStock,
                            QuantitySold = 0,
                            //StockDeductionIds = StockAdjustment.StockDeductionIds,
                            PriceAddedFor = StockAdjustment.PriceAddedFor,
                            LotId = StockAdjustment.LotId,
                            LotType = StockAdjustment.LotType,
                            //IsStopSelling = false,
                            UnitAddedFor = StockAdjustment.UnitAddedFor,
                            LotIdForLotNoChecking = StockAdjustment.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = StockAdjustment.LotTypeForLotNoChecking,
                            QuantityAdjusted = convertedStock,
                            AccountId = InventoryAccountId
                        };

                        oConnectionContext.DbClsStockAdjustmentDetails.Add(oClsStockAdjustmentDetails);
                        oConnectionContext.SaveChanges();

                        //string ll = "delete from tblStockAdjustmentDeductionId where StockAdjustmentDetailsId=" + oClsStockAdjustmentDetails.StockAdjustmentDetailsId;
                        //oConnectionContext.Database.ExecuteSqlCommand(ll);

                        foreach (var l in StockAdjustment.StockDeductionIds)
                        {
                            ClsStockAdjustmentDeductionId oClsStockAdjustmentDeductionId = new ClsStockAdjustmentDeductionId()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                Id = l.Id,
                                Type = l.Type,
                                Quantity = l.Quantity,
                                StockAdjustmentDetailsId = oClsStockAdjustmentDetails.StockAdjustmentDetailsId,
                                StockAdjustmentId = oClsStockAdjustment.StockAdjustmentId,
                            };
                            oConnectionContext.DbClsStockAdjustmentDeductionId.Add(oClsStockAdjustmentDeductionId);
                            oConnectionContext.SaveChanges();
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
                    Category = "Stock Adjust",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment \"" + obj.ReferenceNo+"\" created",
                    Id = oClsStockAdjustment.StockAdjustmentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockAdjustmentDelete(ClsStockAdjustmentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.AdjustmentType = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId).Select(a => a.AdjustmentType).FirstOrDefault();
                obj.BranchId = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId).Select(a => a.BranchId).FirstOrDefault();
                var details = oConnectionContext.DbClsStockAdjustmentDetails.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId && a.IsDeleted == false).Select(a => new
                {
                    //a.IsStopSelling,
                    a.StockAdjustmentDetailsId,
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    a.QuantityRemaining,
                    a.PriceAddedFor
                }).ToList();

                //foreach (var item in details)
                //{
                //    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                //    if (convertedStock != item.QuantityRemaining)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Cannot delete.. mismatch quantity",
                //            Data = new
                //            {
                //            }
                //        };
                //        return await Task.FromResult(Ok(data));
                //    }
                //}

                ClsStockAdjustment oClsStockAdjustment = new ClsStockAdjustment()
                {
                    StockAdjustmentId = obj.StockAdjustmentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsStockAdjustment.Attach(oClsStockAdjustment);
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.StockAdjustmentId).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == item.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    string ll = "delete from \"tblStockAdjustmentDeductionId\" where \"StockAdjustmentDetailsId\"=" + item.StockAdjustmentDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(ll);

                    if (obj.AdjustmentType.ToLower() == "credit")
                    {
                        //Release stock
                        //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                        //    Where(a => a.StockAdjustmentDetailsId == item.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                        //List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        //== item.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                        foreach (var res in _StockDeductionIds)
                        {
                            if (res.Type == "purchase")
                            {
                                query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            else if (res.Type == "openingstock")
                            {
                                query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            //else if (res.Type == "stockadjustment")
                            //{
                            //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockAdjustmentDetailsId=" + res.Id;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query);
                            //}
                            else if (res.Type == "stocktransfer")
                            {
                                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        };

                        //if (item.IsStopSelling == false)
                        //{
                        //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + obj.BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                        //}
                    }
                    else
                    {
                        //Release stock
                        //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                        //    Where(a => a.StockAdjustmentDetailsId == item.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                        //List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        //== item.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

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

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        };
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Adjust",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment \"" + oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId).Select(a => a.ReferenceNo).FirstOrDefault()+ "\" deleted",
                    Id = oClsStockAdjustment.StockAdjustmentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateStockAdjustment(ClsStockAdjustmentVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.AdjustmentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAdjustmentDate" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.StockAdjustmentReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStockAdjustmentReason" });
                    isError = true;
                }

                //if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                //{
                //    if (oConnectionContext.DbClsStockAdjustment.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.StockAdjustmentId!= obj.StockAdjustmentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Reference No exists", Id = "divReferenceNo" });
                //        isError = true;
                //    }
                //}

                if (obj.StockAdjustmentDetails == null || obj.StockAdjustmentDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.StockAdjustmentDetails != null)
                {
                    foreach (var item in obj.StockAdjustmentDetails)
                    {
                        if(item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "This is required", Id = "divQuantity" + item.DivId });
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

                //if (obj.AdjustmentType == "credit")
                //{
                //    if (obj.CheckStockPriceMismatch == true)
                //    {
                //        string CurrencySymbol = (from a in oConnectionContext.DbClsUserCurrencyMap
                //                                 join b in oConnectionContext.DbClsCurrency
                //          on a.CurrencyId equals b.CurrencyId
                //                                 where a.CompanyId == obj.CompanyId && a.IsMain == true
                //                                 select b.CurrencySymbol).FirstOrDefault();

                //        if (isError == true)
                //        {
                //            data = new
                //            {
                //                Status = 3,
                //                Message = "",
                //                Errors = errors,
                //                Data = new
                //                {
                //                }
                //            };
                //            return await Task.FromResult(Ok(data));
                //        }
                //    }
                //}

                //if (obj.AdjustmentType.ToLower() == "credit")
                //{
                //    if (obj.StockAdjustmentDetails != null)
                //    {
                //        foreach (var StockAdjustment in obj.StockAdjustmentDetails)
                //        {
                //            if (StockAdjustment.StockAdjustmentDetailsId != 0)
                //            {
                //                //decimal convertedStock = oCommonController.StockConversion(StockAdjustment.Quantity, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                //                decimal QuantitySold = oConnectionContext.DbClsStockAdjustmentDetails.Where(a => a.StockAdjustmentDetailsId == StockAdjustment.StockAdjustmentDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                //                if ((StockAdjustment.Quantity - QuantitySold) < 0)
                //                {
                //                    data = new
                //                    {
                //                        Status = 0,
                //                        Message = "Mismatch between purchase and sold quantity for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                //                        Data = new
                //                        {
                //                        }
                //                    };
                //                    return await Task.FromResult(Ok(data));
                //                }
                //                //else
                //                //{
                //                //    string query = "update tblItemBranchMap set Quantity = Quantity,0)-" + convertedStock + " where BranchId=" + obj.BranchId + " and ItemDetailsId=" + StockAdjustment.ItemDetailsId;
                //                //    oConnectionContext.Database.ExecuteSqlCommand(query);
                //                //}
                //            }
                //        }
                //    }
                //}               

                //Release stock
                if (obj.AdjustmentType.ToLower() == "debit")
                {
                    if (obj.StockAdjustmentDetails != null)
                    {
                        foreach (var StockAdjustment in obj.StockAdjustmentDetails)
                        {
                            if (StockAdjustment.StockAdjustmentDetailsId != 0)
                            {
                                //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                                //Where(a => a.StockAdjustmentDetailsId == StockAdjustment.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                                //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == StockAdjustment.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                foreach (var res in _StockDeductionIds)
                                {
                                    string query = "";
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

                                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + res.Quantity + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + StockAdjustment.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                };

                                //decimal convertedStock = oCommonController.StockConversion(StockAdjustment.Quantity, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                                //decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == StockAdjustment.ItemId && a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
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
                            }
                            if(StockAdjustment.IsDeleted == true)
                            {
                                string query = "update \"tblStockAdjustmentDetails\" set \"IsDeleted\"=True where \"StockAdjustmentDetailsId\"=" + StockAdjustment.StockAdjustmentDetailsId;
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }
                else
                {
                    if (obj.StockAdjustmentDetails != null)
                    {
                        foreach (var StockAdjustment in obj.StockAdjustmentDetails)
                        {
                            if (StockAdjustment.StockAdjustmentDetailsId != 0)
                            {
                                //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                                //Where(a => a.StockAdjustmentDetailsId == StockAdjustment.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                                //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == StockAdjustment.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                foreach (var res in _StockDeductionIds)
                                {
                                    string query = "";
                                    if (res.Type == "purchase")
                                    {
                                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "openingstock")
                                    {
                                        query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "stocktransfer")
                                    {
                                        query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + res.Quantity + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + StockAdjustment.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                };
                            }
                            if (StockAdjustment.IsDeleted == true)
                            {
                                string query = "update \"tblStockAdjustmentDetails\" set \"IsDeleted\"=True where \"StockAdjustmentDetailsId\"=" + StockAdjustment.StockAdjustmentDetailsId;
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }
                //Release stock

                obj.StockAdjustmentDetails.RemoveAll(r => r.IsDeleted == true);

                if (obj.AdjustmentType.ToLower() == "debit")
                {
                    var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                    if (EnableLotNo == true)
                    {
                        if (obj.StockAdjustmentDetails != null)
                        {
                            foreach (var Sales in obj.StockAdjustmentDetails)
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
                                        if (Sales.StockAdjustmentDetailsId != 0)
                                        {
                                            remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                        }
                                    }

                                    decimal convertedStock = 0;
                                    foreach (var inner in obj.StockAdjustmentDetails)
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
                        if (obj.StockAdjustmentDetails != null)
                        {
                            foreach (var Sales in obj.StockAdjustmentDetails)
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                                    decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();

                                    decimal convertedStock = 0;
                                    foreach (var inner in obj.StockAdjustmentDetails)
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
                }

                //int count = 1;
                //if (obj.StockAdjustmentDetails != null)
                //{
                //    foreach (var item in obj.StockAdjustmentDetails)
                //    {
                //        if (obj.AdjustmentType.ToLower() == "debit")
                //        {
                //            int innerCount = 1;

                //            foreach (var inner in obj.StockAdjustmentDetails)
                //            {
                //                if (item.SkuCode != "" && item.SkuCode != null)
                //                {
                //                    if (item.SkuCode.ToLower() == inner.SkuCode.ToLower() && count != innerCount)
                //                    {
                //                        errors.Add(new ClsError { Message = "Duplicate Sku Code exists in row no " + count, Id = "" });
                //                        isError = true;
                //                    }
                //                }

                //                innerCount++;
                //            }
                //            count++;
                //        }
                //    }
                //}

                ClsStockAdjustment oClsStockAdjustment = new ClsStockAdjustment()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    BranchId = obj.BranchId,
                    StockAdjustmentId = obj.StockAdjustmentId,
                    ReferenceNo = obj.ReferenceNo,
                    AdjustmentDate = obj.AdjustmentDate.AddHours(5).AddMinutes(30),
                    AdjustmentType = obj.AdjustmentType,
                    Notes = obj.Notes,
                    TotalQuantity = obj.TotalQuantity,
                    TotalAmount = obj.TotalAmount,
                    TotalAmountRecovered = obj.TotalAmountRecovered,
                    StockDeductionType = obj.StockDeductionType,
                    AccountId = obj.AccountId,
                    StockAdjustmentReasonId = obj.StockAdjustmentReasonId
                };

                oConnectionContext.DbClsStockAdjustment.Attach(oClsStockAdjustment);
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.StockAdjustmentId).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.ReferenceNo).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.AdjustmentDate).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.AdjustmentType).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.TotalAmount).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.TotalAmountRecovered).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.StockDeductionType).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsStockAdjustment).Property(x => x.StockAdjustmentReasonId).IsModified = true;
                oConnectionContext.SaveChanges();

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays }).FirstOrDefault();

                if (obj.StockAdjustmentDetails != null)
                {
                    foreach (var StockAdjustment in obj.StockAdjustmentDetails)
                    {
                        decimal convertedStock = oCommonController.StockConversion(StockAdjustment.Quantity, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                        if (StockAdjustment.StockAdjustmentDetailsId == 0)
                        {
                            if (obj.AdjustmentType.ToLower() == "debit")
                            {
                                if (StockAdjustment.LotId == 0)
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.deductStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                                }
                                else
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.LotId, StockAdjustment.LotType);
                                }
                            }
                            else
                            {
                                if (StockAdjustment.LotId == 0)
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.addStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                                }
                                else
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.addStockLot(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.LotId, StockAdjustment.LotType);
                                }
                            }

                            if (StockAdjustment.LotType == "stocktransfer")
                            {
                                StockAdjustment.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockAdjustment.LotId).Select(a => a.LotId).FirstOrDefault();
                                StockAdjustment.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockAdjustment.LotId).Select(a => a.LotType).FirstOrDefault();
                            }
                            else
                            {
                                StockAdjustment.LotIdForLotNoChecking = StockAdjustment.LotId;
                                StockAdjustment.LotTypeForLotNoChecking = StockAdjustment.LotType;
                            }

                            long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                            ClsStockAdjustmentDetails oClsStockAdjustmentDetails = new ClsStockAdjustmentDetails()
                            {
                                Amount = StockAdjustment.Amount,
                                ItemId = StockAdjustment.ItemId,
                                ItemDetailsId = StockAdjustment.ItemDetailsId,
                                StockAdjustmentId = oClsStockAdjustment.StockAdjustmentId,
                                Quantity = StockAdjustment.Quantity,
                                UnitCost = StockAdjustment.UnitCost,
                                IsActive = StockAdjustment.IsActive,
                                IsDeleted = StockAdjustment.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                QuantityRemaining = convertedStock,
                                QuantitySold = 0,
                                //StockDeductionIds = StockAdjustment.StockDeductionIds,
                                PriceAddedFor = StockAdjustment.PriceAddedFor,
                                LotId = StockAdjustment.LotId,
                                LotType = StockAdjustment.LotType,
                                //IsStopSelling = false,
                                UnitAddedFor = StockAdjustment.UnitAddedFor,
                                LotIdForLotNoChecking = StockAdjustment.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = StockAdjustment.LotTypeForLotNoChecking,
                                QuantityAdjusted = convertedStock,
                                AccountId = InventoryAccountId
                            };

                            oConnectionContext.DbClsStockAdjustmentDetails.Add(oClsStockAdjustmentDetails);
                            oConnectionContext.SaveChanges();

                            //string ll = "delete from tblStockAdjustmentDeductionId where StockAdjustmentDetailsId=" + oClsStockAdjustmentDetails.StockAdjustmentDetailsId;
                            //oConnectionContext.Database.ExecuteSqlCommand(ll);

                            foreach (var l in StockAdjustment.StockDeductionIds)
                            {
                                ClsStockAdjustmentDeductionId oClsStockAdjustmentDeductionId = new ClsStockAdjustmentDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    StockAdjustmentDetailsId = oClsStockAdjustmentDetails.StockAdjustmentDetailsId,
                                    StockAdjustmentId = oClsStockAdjustment.StockAdjustmentId,
                                };
                                oConnectionContext.DbClsStockAdjustmentDeductionId.Add(oClsStockAdjustmentDeductionId);
                                oConnectionContext.SaveChanges();
                            }
                        }
                        else
                        {
                            decimal QuantitySold = oConnectionContext.DbClsStockAdjustmentDetails.Where(a => a.StockAdjustmentDetailsId == StockAdjustment.StockAdjustmentDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                            if (obj.AdjustmentType.ToLower() == "debit")
                            {
                                if (StockAdjustment.LotId == 0)
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.deductStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                                }
                                else
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.LotId, StockAdjustment.LotType);
                                }
                            }
                            else
                            {
                                if (StockAdjustment.LotId == 0)
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.addStock(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.ItemId, StockAdjustment.PriceAddedFor);
                                }
                                else
                                {
                                    StockAdjustment.StockDeductionIds = oCommonController.addStockLot(obj.BranchId, StockAdjustment.ItemDetailsId, convertedStock, StockAdjustment.LotId, StockAdjustment.LotType);
                                }
                            }

                            if (StockAdjustment.LotType == "stocktransfer")
                            {
                                StockAdjustment.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockAdjustment.LotId).Select(a => a.LotId).FirstOrDefault();
                                StockAdjustment.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == StockAdjustment.LotId).Select(a => a.LotType).FirstOrDefault();
                            }
                            else
                            {
                                StockAdjustment.LotIdForLotNoChecking = StockAdjustment.LotId;
                                StockAdjustment.LotTypeForLotNoChecking = StockAdjustment.LotType;
                            }

                            long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == StockAdjustment.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();

                            ClsStockAdjustmentDetails oClsStockAdjustmentDetails = new ClsStockAdjustmentDetails()
                            {
                                StockAdjustmentDetailsId = StockAdjustment.StockAdjustmentDetailsId,
                                Amount = StockAdjustment.Amount,
                                ItemId = StockAdjustment.ItemId,
                                ItemDetailsId = StockAdjustment.ItemDetailsId,
                                StockAdjustmentId = oClsStockAdjustment.StockAdjustmentId,
                                Quantity = StockAdjustment.Quantity,
                                UnitCost = StockAdjustment.UnitCost,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                QuantityRemaining = convertedStock - QuantitySold,
                                //StockDeductionIds = StockAdjustment.StockDeductionIds,
                                PriceAddedFor = StockAdjustment.PriceAddedFor,
                                LotId = StockAdjustment.LotId,
                                LotType = StockAdjustment.LotType,
                                UnitAddedFor = StockAdjustment.UnitAddedFor,
                                LotIdForLotNoChecking = StockAdjustment.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = StockAdjustment.LotTypeForLotNoChecking,
                                QuantityAdjusted = convertedStock,
                                AccountId = InventoryAccountId
                            };
                            oConnectionContext.DbClsStockAdjustmentDetails.Attach(oClsStockAdjustmentDetails);
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.Amount).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.StockAdjustmentId).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            //oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.StockDeductionIds).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.LotId).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.LotType).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.QuantityAdjusted).IsModified = true;
                            oConnectionContext.Entry(oClsStockAdjustmentDetails).Property(x => x.AccountId).IsModified = true;

                            oConnectionContext.SaveChanges();

                            string ll = "delete from \"tblStockAdjustmentDeductionId\" where \"StockAdjustmentDetailsId\"=" + StockAdjustment.StockAdjustmentDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(ll);

                            foreach (var l in StockAdjustment.StockDeductionIds)
                            {
                                ClsStockAdjustmentDeductionId oClsStockAdjustmentDeductionId = new ClsStockAdjustmentDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    StockAdjustmentDetailsId = StockAdjustment.StockAdjustmentDetailsId,
                                    StockAdjustmentId = oClsStockAdjustment.StockAdjustmentId,
                                };
                                oConnectionContext.DbClsStockAdjustmentDeductionId.Add(oClsStockAdjustmentDeductionId);
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Stock Adjust",
                    CompanyId = obj.CompanyId,
                    Description = "Stock Adjustment \"" + oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId).Select(a => a.ReferenceNo).FirstOrDefault()+"\" updated",
                    Id = oClsStockAdjustment.StockAdjustmentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Stock Adjustment updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockAdjustmentDetailsDelete(ClsStockAdjustmentDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                string AdjustmentType = "";
                if (obj.StockAdjustmentId != 0)
                {
                    AdjustmentType = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId).Select(a => a.AdjustmentType).FirstOrDefault();
                    obj.BranchId = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsStockAdjustmentDetails.Where(a => a.StockAdjustmentId == obj.StockAdjustmentId && a.IsDeleted == false).Select(a => new
                    {
                        a.StockAdjustmentDetailsId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.QuantityRemaining,
                        a.PriceAddedFor,
                    }).ToList();

                    //foreach (var item in details)
                    //{
                    //    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    //    if (convertedStock != item.QuantityRemaining)
                    //    {
                    //        data = new
                    //        {
                    //            Status = 0,
                    //            Message = "Cannot delete.. mismatch quantity",
                    //            Data = new
                    //            {
                    //            }
                    //        };
                    //        return await Task.FromResult(Ok(data));
                    //    }
                    //}

                    string query = "update \"tblStockAdjustmentDetails\" set \"IsDeleted\"=True where \"StockAdjustmentId\"=" + obj.StockAdjustmentId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in details)
                    {
                        //decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                        if (AdjustmentType.ToLower() == "credit")
                        {
                            //Release stock
                            //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                            //    Where(a => a.StockAdjustmentDetailsId == item.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                            //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                            List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == item.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                            foreach (var res in _StockDeductionIds)
                            {
                                if (res.Type == "purchase")
                                {
                                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (res.Type == "openingstock")
                                {
                                    query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                //else if (res.Type == "stockadjustment")
                                //{
                                //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockAdjustmentDetailsId=" + res.Id;
                                //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                //}
                                else if (res.Type == "stocktransfer")
                                {
                                    query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }

                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            };

                            //if (item.IsStopSelling == false)
                            //{
                            //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + obj.BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query);
                            //}
                        }
                        else
                        {
                            //Release stock
                            //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                            //    Where(a => a.StockAdjustmentDetailsId == item.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                            //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                            List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == item.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

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

                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            };
                        }
                    }


                    //foreach (var item in details)
                    //{
                    //    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    //    if (AdjustmentType.ToLower() == "credit")
                    //    {
                    //        //if (item.IsStopSelling == false)
                    //        //{
                    //            query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                    //            oConnectionContext.Database.ExecuteSqlCommand(query);
                    //        //}
                    //    }
                    //    else
                    //    {
                    //        query = "update tblItemBranchMap set Quantity=Quantity,0)+" + convertedStock + " where BranchId=" + BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    }
                    //}
                }
                else
                {
                    AdjustmentType = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == oConnectionContext.DbClsStockAdjustmentDetails.Where(b => b.StockAdjustmentDetailsId == obj.StockAdjustmentDetailsId).Select(b => b.StockAdjustmentId).FirstOrDefault()).Select(a => a.AdjustmentType).FirstOrDefault();
                    obj.BranchId = oConnectionContext.DbClsStockAdjustment.Where(a => a.StockAdjustmentId == oConnectionContext.DbClsStockAdjustmentDetails.Where(b => b.StockAdjustmentDetailsId == obj.StockAdjustmentDetailsId).Select(b => b.StockAdjustmentId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                    var details = oConnectionContext.DbClsStockAdjustmentDetails.Where(a => a.StockAdjustmentDetailsId == obj.StockAdjustmentDetailsId).Select(a => new
                    {
                        a.StockAdjustmentDetailsId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.QuantityRemaining,
                        a.PriceAddedFor,
                    }).FirstOrDefault();

                    string query = "update \"tblStockAdjustmentDetails\" set \"IsDeleted\"=True where \"StockAdjustmentDetailsId\"=" + obj.StockAdjustmentDetailsId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    if (AdjustmentType.ToLower() == "credit")
                    {
                        //Release stock
                        //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                        //    Where(a => a.StockAdjustmentDetailsId == details.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                        List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == details.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                        foreach (var res in _StockDeductionIds)
                        {
                            if (res.Type == "purchase")
                            {
                                query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            else if (res.Type == "openingstock")
                            {
                                query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            //else if (res.Type == "stockadjustment")
                            //{
                            //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + res.Quantity + ",QuantitySold=QuantitySold,0)-" + res.Quantity + " where StockAdjustmentDetailsId=" + res.Id;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query);
                            //}
                            else if (res.Type == "stocktransfer")
                            {
                                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + details.ItemId + " and \"ItemDetailsId\"=" + details.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        };

                        //if (item.IsStopSelling == false)
                        //{
                        //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + obj.BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                        //}
                    }
                    else
                    {
                        //Release stock
                        //string StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDetails.
                        //    Where(a => a.StockAdjustmentDetailsId == details.StockAdjustmentDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                        List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(a => a.StockAdjustmentDetailsId
                        == details.StockAdjustmentDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

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

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + details.ItemId + " and \"ItemDetailsId\"=" + details.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        };
                    }

                    //if (AdjustmentType.ToLower() == "credit")
                    //{
                    //    //if (details.IsStopSelling == false)
                    //    //{
                    //        query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    //}
                    //}
                    //else
                    //{
                    //    query = "update tblItemBranchMap set Quantity=Quantity,0)+" + convertedStock + " where BranchId=" + BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
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

        public async Task<IHttpActionResult> StockAdjustmentJournal(ClsStockAdjustmentVm obj)
        {
            var allStockAdjust = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                               join b in oConnectionContext.DbClsStockAdjustment
                            on a.StockAdjustmentId equals b.StockAdjustmentId
                               where b.StockAdjustmentId == obj.StockAdjustmentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsActive == true
                               select new ClsBankPaymentVm
                               {
                                   //AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                   AccountId = a.AccountId,
                                   Debit = b.AdjustmentType.ToLower() == "debit" ? 0 : a.Amount,
                                   Credit = b.AdjustmentType.ToLower() == "credit" ? 0 : a.Amount
                               }).ToList();

            var journal = (from a in oConnectionContext.DbClsStockAdjustment
                        //   join b in oConnectionContext.DbClsStockAdjustmentDetails
                        //on a.StockAdjustmentId equals b.StockAdjustmentId
                           where a.StockAdjustmentId == obj.StockAdjustmentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = a.AdjustmentType.ToLower() == "debit" ? a.TotalAmount : 0,
                               Credit = a.AdjustmentType.ToLower() == "credit"? a.TotalAmount : 0
                           })
                           //(from a in oConnectionContext.DbClsStockAdjustmentDetails
                           //         join b in oConnectionContext.DbClsStockAdjustment
                           //      on a.StockAdjustmentId equals b.StockAdjustmentId
                           //         where b.StockAdjustmentId == obj.StockAdjustmentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           //         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //         select new ClsBankPaymentVm
                           //         {
                           //             AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                           //             Debit = b.AdjustmentType == "debit" ? 0 :a.Amount,
                           //             Credit = b.AdjustmentType == "credit" ? 0 :a.Amount
                           //         })
                           .ToList();

            journal = journal.Concat(from a in allStockAdjust
                                     group a by a.AccountId into stdGroup
                                     select new ClsBankPaymentVm
                                     {
                                         // sales account
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                         Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                     }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = journal
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
