using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
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
    public class RecurringSalesController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllRecurringSales(ClsRecurringSalesVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
            }).FirstOrDefault();

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
            List<ClsRecurringSalesVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsRecurringSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsRecurringSalesVm
                {
                    RecurringSalesType = a.RecurringSalesType,
                    CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                    CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                    DisplayName = a.DisplayName,
                    RepeatEvery = a.RepeatEvery,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsNeverExpires = a.IsNeverExpires,
                    Status = a.Status,
                    LastInvoiceDate = a.LastInvoiceDate,
                    NextInvoiceDate = a.NextInvoiceDate,
                    TotalTaxAmount = a.TotalTaxAmount,
                    TotalDiscount = a.TotalDiscount,
                    BranchId = a.BranchId,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    RecurringSalesId = a.RecurringSalesId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    Subtotal = a.Subtotal,
                    CustomerId = a.CustomerId,
                    GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsRecurringSalesDetails.Where(c=>c.RecurringSalesId==a.RecurringSalesId && c.IsDeleted==false).Count()
                    PaidQuantity = oConnectionContext.DbClsRecurringSalesDetails.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsRecurringSalesDetails.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    TotalItems = oConnectionContext.DbClsRecurringSalesDetails.Where(c => c.RecurringSalesId == a.RecurringSalesId &&
                    c.IsDeleted == false && c.IsComboItems == false).Count()
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsRecurringSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
//&& a.Status.ToLower() == obj.Status.ToLower()
&& a.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsRecurringSalesVm
               {
                   RecurringSalesType = a.RecurringSalesType,
                   CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                   CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                   DisplayName = a.DisplayName,
                   RepeatEvery = a.RepeatEvery,
                   RepeatEveryNumber = a.RepeatEveryNumber,
                   StartDate = a.StartDate,
                   EndDate = a.EndDate,
                   IsNeverExpires = a.IsNeverExpires,
                   Status = a.Status,
                   LastInvoiceDate = a.LastInvoiceDate,
                   NextInvoiceDate = a.NextInvoiceDate,
                   TotalTaxAmount = a.TotalTaxAmount,
                   TotalDiscount = a.TotalDiscount,
                   BranchId = a.BranchId,
                   BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                   RecurringSalesId = a.RecurringSalesId,
                   GrandTotal = a.GrandTotal,
                   Notes = a.Notes,
                   Subtotal = a.Subtotal,
                   CustomerId = a.CustomerId,
                   GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                   CompanyId = a.CompanyId,
                   IsActive = a.IsActive,
                   IsDeleted = a.IsDeleted,
                   AddedBy = a.AddedBy,
                   AddedOn = a.AddedOn,
                   ModifiedBy = a.ModifiedBy,
                   ModifiedOn = a.ModifiedOn,
                   AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                   ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                   TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsRecurringSalesDetails.Where(c=>c.RecurringSalesId==a.RecurringSalesId && c.IsDeleted==false).Count()
                   PaidQuantity = oConnectionContext.DbClsRecurringSalesDetails.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                   FreeQuantity = oConnectionContext.DbClsRecurringSalesDetails.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                   TotalItems = oConnectionContext.DbClsRecurringSalesDetails.Where(c => c.RecurringSalesId == a.RecurringSalesId &&
                   c.IsDeleted == false && c.IsComboItems == false).Count()
               }).ToList();
            }

            if (obj.From == "dashboard")
            {
                det = det.Where(a => a.Status != "Draft" && a.Status != "Hold").Select(a => a).ToList();
            }

            if (obj.Status != null && obj.Status != "")
            {
                det = det.Where(a => a.Status.ToLower() == obj.Status.ToLower()).Select(a => a).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RecurringSales = det.OrderByDescending(a => a.RecurringSalesId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> RecurringSale(ClsRecurringSalesVm obj)
        {
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
            //bool EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

            var det = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == obj.RecurringSalesId && a.CompanyId == obj.CompanyId).Select(a => new ClsRecurringSalesVm
            {
                DisplayName = a.DisplayName,
                RepeatEveryNumber = a.RepeatEveryNumber,
                RepeatEvery = a.RepeatEvery,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                IsNeverExpires = a.IsNeverExpires,
                Terms = a.Terms,
                AdvanceBalance = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.AdvanceBalance).FirstOrDefault(),
                GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                PayTaxForExport = a.PayTaxForExport,
                TaxCollectedFromCustomer = a.TaxCollectedFromCustomer,
                IsReverseCharge = a.IsReverseCharge,
                TaxableAmount = a.TaxableAmount,
                NetAmountReverseCharge = a.NetAmountReverseCharge,
                RoundOffReverseCharge = a.RoundOffReverseCharge,
                GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                IsCancelled = a.IsCancelled,
                TaxExemptionId = a.TaxExemptionId,
                BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                PlaceOfSupplyId = a.PlaceOfSupplyId,
                Status = a.Status,
                TotalTaxAmount = a.TotalTaxAmount,
                RecurringSalesType = a.RecurringSalesType,
                RoundOff = a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                NetAmount = a.NetAmount,
                ExchangeRate = a.ExchangeRate,
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                DefaultCurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(c => c.IsMain == true && c.CompanyId == obj.CompanyId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                BranchId = a.BranchId,
                PaymentType = a.PaymentType == null ? "" : a.PaymentType,
                HoldReason = a.HoldReason,
                TotalPaying = a.TotalPaying,
                Balance = a.Balance,
                ChangeReturn = a.ChangeReturn,
                CustomerId = a.CustomerId,
                SellingPriceGroupId = a.SellingPriceGroupId,
                //a.Status,
                //PayTerm=a.PayTerm,
                //PayTermNo=a.PayTermNo,
                PaymentTermId = a.PaymentTermId,
                AttachDocument = a.AttachDocument,
                RecurringSalesId = a.RecurringSalesId,
                GrandTotal = a.GrandTotal,
                TaxId = a.TaxId,
                TotalDiscount = a.TotalDiscount,
                TotalQuantity = a.TotalQuantity,
                Discount = a.Discount,
                DiscountType = a.DiscountType,
                Notes = a.Notes,
                Subtotal = a.Subtotal,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                TaxAmount = a.TaxAmount,
                RecurringSalesDetails = (from b in oConnectionContext.DbClsRecurringSalesDetails
                                         join c in oConnectionContext.DbClsItemBranchMap
                                         on b.ItemDetailsId equals c.ItemDetailsId
                                         join d in oConnectionContext.DbClsItemDetails
                                          on b.ItemDetailsId equals d.ItemDetailsId
                                         join e in oConnectionContext.DbClsItem
                                         on d.ItemId equals e.ItemId
                                         where b.RecurringSalesId == a.RecurringSalesId && b.IsDeleted == false && c.BranchId == a.BranchId
                                         && b.IsComboItems == false
                                         select new ClsRecurringSalesDetailsVm
                                         {
                                             TotalTaxAmount = b.TotalTaxAmount,
                                             ItemCodeId = b.ItemCodeId,
                                             ItemType = e.ItemType,
                                             TaxExemptionId = b.TaxExemptionId,
                                             ExtraDiscount = b.ExtraDiscount,
                                             IsManageStock = e.IsManageStock,
                                             Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                                             : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                                             //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f=>f.CompanyId == obj.CompanyId).Select(f=>f.EnableWarranty).FirstOrDefault(),
                                             EnableImei = e.EnableImei,
                                             WarrantyId = b.WarrantyId,
                                             FreeQuantity = b.FreeQuantity,
                                             //b.FreeQuantityPriceAddedFor,
                                             PriceExcTax = b.PriceExcTax,
                                             PriceIncTax = b.PriceIncTax,
                                             AmountExcTax = b.AmountExcTax,
                                             TaxAmount = b.TaxAmount,
                                             AmountIncTax = b.AmountIncTax,
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
                                             LotId = b.LotId,
                                             LotType = b.LotType,
                                             QuantityRemaining = a.Status.ToLower() != "draft" ? (b.QuantityRemaining + (b.LotType == "purchase" ?
                                             oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             : b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             : b.LotType == "stocktransfer" ? oConnectionContext.DbClsStockTransferDetails.Where(f => f.StockTransferDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             : c.Quantity)) : c.Quantity,
                                             DiscountType = b.DiscountType,
                                             RecurringSalesDetailsId = b.RecurringSalesDetailsId,
                                             OtherInfo = b.OtherInfo,
                                             Discount = b.Discount,
                                             RecurringSalesId = b.RecurringSalesId,
                                             Quantity = b.Quantity,
                                             TaxId = b.TaxId,
                                             UnitCost = b.UnitCost,
                                             ItemId = d.ItemId,
                                             ProductType = e.ProductType,
                                             ItemDetailsId = d.ItemDetailsId,
                                             ItemName = e.ItemName,
                                             SKU = d.SKU == null ? e.SkuCode : d.SKU,
                                             VariationDetailsId = d.VariationDetailsId,
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == d.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == e.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                             SalesExcTax = d.SalesExcTax,
                                             SalesIncTax = b.LotTypeForLotNoChecking == "purchase" ?
                                             oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                             : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                             //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.RecurringSalesIncTax).FirstOrDefault()
                                             : d.SalesIncTax,
                                             TotalCost = d.TotalCost,
                                             Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                             TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                             TaxType = e.TaxType,
                                             ItemCode = e.ItemCode,
                                             DefaultUnitCost = b.DefaultUnitCost,
                                             DefaultAmount = b.DefaultAmount,
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
                                             AllowDecimal = oConnectionContext.DbClsUnit.Where(c => c.UnitId == e.UnitId).Select(c => c.AllowDecimal).FirstOrDefault(),
                                             SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == e.SecondaryUnitId).Select(c => c.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                             TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == e.TertiaryUnitId).Select(c => c.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                             QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == e.QuaternaryUnitId).Select(c => c.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                             PriceAddedFor = b.PriceAddedFor,
                                         }).ToList(),

                RecurringSalesAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                ).Select(b => new ClsRecurringSalesAdditionalChargesVm
                {
                    InterStateTaxId = b.InterStateTaxId,
                    IntraStateTaxId = b.IntraStateTaxId,
                    RecurringSalesAdditionalChargesId = oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.RecurringSalesAdditionalChargesId).FirstOrDefault(),
                    Name = b.Name,
                    AdditionalChargeId = b.AdditionalChargeId,
                    RecurringSalesId = a.RecurringSalesId,
                    TaxId = oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                    AmountExcTax = oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                    AmountIncTax = oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                    TaxAmount = oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                    AccountId = oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(c => c.RecurringSalesId == a.RecurringSalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                    ItemCodeId = b.ItemCodeId,
                    TaxExemptionId = b.TaxExemptionId,
                    TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                }).ToList(),
            }).FirstOrDefault();

            if (det.RecurringSalesDetails != null && det.RecurringSalesDetails.Count > 0)
            {
                foreach (var _comboStock in det.RecurringSalesDetails)
                {
                    if (_comboStock.ProductType == "Combo")
                    {
                        var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == _comboStock.ItemId).Select(a => new
                        {
                            ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                            IsManageStock = false,
                            Quantity = a.Quantity,
                            QtyForSell = 0,
                            AvailableQuantity = oConnectionContext.DbClsItemBranchMap.Where(b => b.ItemDetailsId == a.ComboItemDetailsId && b.BranchId == det.BranchId && b.IsActive == true && b.IsDeleted == false).Select(b => b.Quantity).FirstOrDefault(),
                        }).ToList();

                        var itemsWithQty = combo.Select(item => new
                        {
                            IsManageStock = oConnectionContext.DbClsItem.Where(b => b.ItemId == item.ItemId).Select(b => b.IsManageStock).FirstOrDefault(),
                            item.Quantity,
                            item.AvailableQuantity,
                            QtyForSell = item.AvailableQuantity != 0 ? (int)(item.AvailableQuantity / item.Quantity) : 0
                        }).ToList();

                        //var minQty = itemsWithQty.Where(a=>a.IsManageStock == true).DefaultIfEmpty().Min(item => item.QtyForSell);

                        var minQty = itemsWithQty
                           .Where(a => a != null && a.IsManageStock == true)
                           .Select(item => item.QtyForSell)
                           .DefaultIfEmpty()
                           .Min();

                        bool IsManageStock = itemsWithQty.Where(a => a.IsManageStock == true).Count() > 0 ? true : false;

                        if (IsManageStock == true)
                        {
                            //if (minQty > 0)
                            //{
                            _comboStock.QuantityRemaining = _comboStock.QuantityRemaining + minQty;
                            _comboStock.IsManageStock = true;
                            //}
                        }
                        //else
                        //{
                        //    ComboStock1.Add(_comboStock.ItemName + " ~ " + _comboStock.SkuCode);
                        //}
                    }
                }
            }

            var AllTaxs = oConnectionContext.DbClsRecurringSales.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.RecurringSalesId == det.RecurringSalesId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsRecurringSalesDetails.Where(a => a.RecurringSalesId == det.RecurringSalesId && a.IsDeleted == false
                                && a.IsComboItems == false).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                }
                                )).Concat(oConnectionContext.DbClsRecurringSalesAdditionalCharges.Where(a => a.RecurringSalesId == det.RecurringSalesId
                                && a.IsDeleted == false && a.AmountExcTax > 0).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).ToList();

            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
            foreach (var item in AllTaxs)
            {
                bool CanDelete = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.CanDelete).FirstOrDefault();
                if (CanDelete == true)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                        });
                    }
                }
            }

            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                     (k, c) => new
                     {
                         //TaxId = k,
                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                     }
                    ).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RecurringSale = det,
                    Taxs = finalTaxs,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertRecurringSales(ClsRecurringSalesVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                long CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                long PrefixUserMapId = 0;

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.CustomerId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCustomer" });
                    isError = true;
                }
                else
                {
                    if (CountryId == 2)
                    {
                        if (userDet.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.PlaceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divPlaceOfSupply" });
                                isError = true;
                            }
                        }
                    }
                }

                if (obj.RecurringSalesType == "" || obj.RecurringSalesType == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRecurringSalesType" });
                    isError = true;
                }

                if (obj.StartDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStartDate" });
                    isError = true;
                }

                if(obj.IsNeverExpires == false)
                {
                    if (obj.EndDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divEndDate" });
                        isError = true;
                    }
                }

                if (obj.DisplayName == "" || obj.DisplayName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDisplayName" });
                    isError = true;
                }

                if (obj.RepeatEvery == "" || obj.RepeatEvery == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRepeatEvery" });
                    isError = true;
                }

                if (obj.RepeatEveryNumber == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRepeatEveryNumber" });
                    isError = true;
                }

                if (obj.RecurringSalesDetails == null || obj.RecurringSalesDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.RecurringSalesDetails != null)
                {
                    foreach (var item in obj.RecurringSalesDetails)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.UnitCost <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divUnitCost" + item.DivId });
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

                List<ClsRecurringSalesDetailsVm> _RecurringSalesDetails = new List<ClsRecurringSalesDetailsVm>();
                if (obj.RecurringSalesDetails != null)
                {
                    foreach (var RecurringSales in obj.RecurringSalesDetails)
                    {
                        RecurringSales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == RecurringSales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                        if (RecurringSales.ProductType.ToLower() == "combo")
                        {
                            RecurringSales.ComboId = oCommonController.CreateToken();
                            var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == RecurringSales.ItemId).Select(a => new
                            {
                                ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                ItemDetailsId = a.ItemDetailsId,
                                ComboItemDetailsId = a.ComboItemDetailsId,
                                Quantity = a.Quantity,
                                a.PriceAddedFor
                            }).ToList();

                            foreach (var item in combo)
                            {
                                _RecurringSalesDetails.Add(new ClsRecurringSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * RecurringSales.Quantity, Under = RecurringSales.ItemDetailsId, IsComboItems = true, ComboId = RecurringSales.ComboId, DivId = RecurringSales.DivId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                            }
                            _RecurringSalesDetails.Add(RecurringSales);
                        }
                        else
                        {
                            _RecurringSalesDetails.Add(RecurringSales);
                        }
                    }
                }

                obj.RecurringSalesDetails = _RecurringSalesDetails;

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                ClsRecurringSales oClsRecurringSales = new ClsRecurringSales()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    PaymentType = obj.PaymentType,
                    HoldReason = obj.HoldReason,
                    TotalPaying = obj.TotalPaying,
                    Balance = obj.Balance,
                    ChangeReturn = obj.ChangeReturn,
                    CustomerId = obj.CustomerId,
                    Status = obj.Status,
                    TotalDiscount = obj.TotalDiscount,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    RecurringSalesId = obj.RecurringSalesId,
                    Subtotal = obj.Subtotal,
                    TaxId = obj.TaxId,
                    TotalQuantity = obj.TotalQuantity,
                    BranchId = obj.BranchId,
                    RecurringSalesType = obj.RecurringSalesType,
                    //Status = obj.Status ,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    ExchangeRate = obj.ExchangeRate,
                    TaxAmount = obj.TaxAmount,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    UserGroupId = UserGroupId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxExemptionId = obj.TaxExemptionId,
                    //IsDebitNote = obj.IsDebitNote,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    IsReverseCharge = obj.IsReverseCharge,
                    IsCancelled = false,
                    GstPayment = obj.GstPayment,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    Terms = obj.Terms,
                    DisplayName = obj.DisplayName,
                    RepeatEveryNumber = obj.RepeatEveryNumber,
                    RepeatEvery = obj.RepeatEvery,
                    StartDate = obj.StartDate,
                    EndDate = obj.EndDate,
                    IsNeverExpires = obj.IsNeverExpires,
                    //LastInvoiceDate = obj.StartDate,
                    //NextInvoiceDate = obj.StartDate
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/RecurringSales/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/RecurringSales/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsRecurringSales.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsRecurringSales.Add(oClsRecurringSales);
                oConnectionContext.SaveChanges();

                if (obj.RecurringSalesAdditionalCharges != null)
                {
                    foreach (var item in obj.RecurringSalesAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        decimal AmountExcTax = item.AmountExcTax;
                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == item.TaxId
                                       select new
                                       {
                                           TaxId = a.SubTaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                       }).ToList();

                        foreach (var tax in taxs)
                        {
                            oClsTaxVm.Add(new ClsTaxVm
                            {
                                TaxId = tax.TaxId,
                                Tax = tax.Tax,
                                TaxPercent = tax.TaxPercent,
                                TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                            });
                        }

                        var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                 (k, c) => new
                                 {
                                     TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                     Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                     TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                     TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                 }
                                ).ToList();

                        List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                        taxList = finalTaxs.Select(a => new ClsTaxVm
                        {
                            TaxType = "Normal",
                            TaxId = a.TaxId,
                            TaxPercent = a.TaxPercent,
                            TaxAmount = a.TaxAmount,
                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                        }).ToList();

                        ClsRecurringSalesAdditionalCharges oClsRecurringSalesAdditionalCharges = new ClsRecurringSalesAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                            TaxId = item.TaxId,
                            AmountExcTax = item.AmountExcTax,
                            AmountIncTax = item.AmountIncTax,
                            TaxAmount = item.AmountIncTax - item.AmountExcTax,
                            AccountId = AdditionalCharge.SalesAccountId,
                            ItemCodeId = AdditionalCharge.ItemCodeId,
                            TaxExemptionId = item.TaxExemptionId,
                            IsActive = item.IsActive,
                            IsDeleted = item.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsRecurringSalesAdditionalCharges.Add(oClsRecurringSalesAdditionalCharges);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.RecurringSalesDetails != null)
                {
                    foreach (var RecurringSales in obj.RecurringSalesDetails)
                    {
                        if (obj.RecurringSalesType == "Bill Of Supply")
                        {
                            RecurringSales.TaxId = oConnectionContext.DbClsTax.Where(b => b.Tax == "Non-Taxable").Select(b => b.TaxId).FirstOrDefault();
                            RecurringSales.TaxExemptionId = oConnectionContext.DbClsTaxExemption.Where(b => b.CanDelete == false).Select(b => b.TaxExemptionId).FirstOrDefault();
                        }
                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == RecurringSales.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        decimal convertedStock = 0, freeConvertedStock = 0;
                        if (RecurringSales.ProductType != "Combo")
                        {
                            convertedStock = oCommonController.StockConversion(RecurringSales.Quantity, RecurringSales.ItemId, RecurringSales.PriceAddedFor);
                            freeConvertedStock = oCommonController.StockConversion(RecurringSales.FreeQuantity, RecurringSales.ItemId, RecurringSales.PriceAddedFor);                            
                        }

                        long PurchaseAccountId = 0;
                        long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == RecurringSales.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                        if (InventoryAccountId != 0)
                        {
                            PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == RecurringSales.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                        }

                        ClsRecurringSalesDetails oClsRecurringSalesDetails = new ClsRecurringSalesDetails()
                        {
                            DiscountType = RecurringSales.DiscountType,
                            OtherInfo = RecurringSales.OtherInfo,
                            PriceIncTax = RecurringSales.PriceIncTax,
                            ItemId = RecurringSales.ItemId,
                            ItemDetailsId = RecurringSales.ItemDetailsId,
                            RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                            TaxId = RecurringSales.TaxId,
                            Discount = RecurringSales.Discount,
                            Quantity = RecurringSales.Quantity,
                            UnitCost = RecurringSales.UnitCost,
                            IsActive = RecurringSales.IsActive,
                            IsDeleted = RecurringSales.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            //StockDeductionIds = RecurringSales.StockDeductionIds,
                            QuantityRemaining = RecurringSales.ProductType == "Combo" ? (RecurringSales.Quantity + RecurringSales.FreeQuantity) : (convertedStock + freeConvertedStock),
                            WarrantyId = RecurringSales.WarrantyId,
                            DefaultUnitCost = DefaultUnitCost,
                            DefaultAmount = RecurringSales.Quantity * DefaultUnitCost,
                            PriceAddedFor = RecurringSales.PriceAddedFor,
                            LotId = RecurringSales.LotId,
                            LotType = RecurringSales.LotType,
                            FreeQuantity = RecurringSales.FreeQuantity,
                            //FreeQuantityPriceAddedFor = RecurringSales.FreeQuantityPriceAddedFor,
                            AmountExcTax = RecurringSales.AmountExcTax,
                            TaxAmount = RecurringSales.TaxAmount,
                            PriceExcTax = RecurringSales.PriceExcTax,
                            AmountIncTax = RecurringSales.AmountIncTax,
                            Under = RecurringSales.Under,
                            UnitAddedFor = RecurringSales.UnitAddedFor,
                            LotIdForLotNoChecking = RecurringSales.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = RecurringSales.LotTypeForLotNoChecking,
                            ComboId = RecurringSales.ComboId,
                            IsComboItems = RecurringSales.IsComboItems,
                            QuantitySold = 0,
                            ComboPerUnitQuantity = RecurringSales.ComboPerUnitQuantity,
                            AccountId = 0,
                            DiscountAccountId = 0,
                            TaxAccountId = 0,
                            PurchaseAccountId = PurchaseAccountId,
                            InventoryAccountId = InventoryAccountId,
                            //WarrantyExpiryDate = WarrantyExpiryDate,
                            ExtraDiscount = RecurringSales.ExtraDiscount,
                            ItemCodeId = RecurringSales.ItemCodeId,
                            TaxExemptionId = RecurringSales.TaxExemptionId,
                            TotalTaxAmount = RecurringSales.TotalTaxAmount,
                            IsCombo = RecurringSales.ProductType == "Combo" ? true : false,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsRecurringSalesDetails.Add(oClsRecurringSalesDetails);
                        oConnectionContext.SaveChanges();
                    }
                }

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                oCommonController.CreateRecurringSalesInvoices(oClsRecurringSales.RecurringSalesId);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Recurring Sales",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Sales" + " \"" + obj.DisplayName + "\" created",
                    Id = oClsRecurringSales.RecurringSalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Invoice created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            RecurringSalesId = oClsRecurringSales.RecurringSalesId
                        }
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RecurringSalesDelete(ClsRecurringSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.BranchId = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == obj.RecurringSalesId).Select(a => a.BranchId).FirstOrDefault();

                ClsRecurringSales oClsRecurringSales = new ClsRecurringSales()
                {
                    RecurringSalesId = obj.RecurringSalesId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsRecurringSales.Attach(oClsRecurringSales);
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RecurringSalesId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Recurring Sales",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Sales" + " \"" + oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == obj.RecurringSalesId).Select(a => a.DisplayName).FirstOrDefault() + "\" deleted",
                    Id = oClsRecurringSales.RecurringSalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Invoice deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RecurringSalesDetailsDelete(ClsRecurringSalesDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.RecurringSalesId != 0)
                {
                    string query = "update \"tblRecurringSalesDetails\" set \"IsDeleted\"=True where \"RecurringSalesId\"=" + obj.RecurringSalesId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblRecurringSalesDetails\" set \"IsDeleted\"=True where \"RecurringSalesDetailsId\"=" + obj.RecurringSalesDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                data = new
                {
                    Status = 1,
                    Message = "Deleted successfully",
                    Data = new { }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RecurringSalesStop(ClsRecurringSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsRecurringSales oClsRecurringSales = new ClsRecurringSales()
                {
                    RecurringSalesId = obj.RecurringSalesId,
                    Status= "Stopped",
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsRecurringSales.Attach(oClsRecurringSales);
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RecurringSalesId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Recurring Sales",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Sales" + " \"" + oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == obj.RecurringSalesId).Select(a => a.DisplayName).FirstOrDefault() + "\" stopped",
                    Id = oClsRecurringSales.RecurringSalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Invoice stopped successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateRecurringSales(ClsRecurringSalesVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsBusinessRegistered,
                a.GstTreatment,
                a.BusinessRegistrationNameId,
                a.BusinessRegistrationNo,
                a.BusinessLegalName,
                a.BusinessTradeName,
                a.PanNo
            }).FirstOrDefault();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                long PrefixUserMapId = 0;

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.CustomerId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCustomer" });
                    isError = true;
                }
                else
                {
                    if (CountryId == 2)
                    {
                        if (userDet.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.PlaceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divPlaceOfSupply" });
                                isError = true;
                            }
                        }
                    }
                }

                if (obj.RecurringSalesType == "" || obj.RecurringSalesType == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRecurringSalesType" });
                    isError = true;
                }

                if (obj.StartDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStartDate" });
                    isError = true;
                }

                if (obj.IsNeverExpires == false)
                {
                    if (obj.EndDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divEndDate" });
                        isError = true;
                    }
                }

                if (obj.DisplayName == "" || obj.DisplayName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDisplayName" });
                    isError = true;
                }

                if (obj.RepeatEvery == "" || obj.RepeatEvery == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRepeatEvery" });
                    isError = true;
                }

                if (obj.RepeatEveryNumber == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRepeatEveryNumber" });
                    isError = true;
                }

                if (obj.RecurringSalesDetails == null || obj.RecurringSalesDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.RecurringSalesDetails != null)
                {
                    foreach (var item in obj.RecurringSalesDetails)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.UnitCost <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divUnitCost" + item.DivId });
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

                List<ClsRecurringSalesDetailsVm> _RecurringSalesDetails = new List<ClsRecurringSalesDetailsVm>();
                if (obj.RecurringSalesDetails != null)
                {
                    foreach (var RecurringSales in obj.RecurringSalesDetails)
                    {
                        if (RecurringSales.RecurringSalesDetailsId != 0)
                        {
                            RecurringSales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == RecurringSales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (RecurringSales.ProductType.ToLower() == "combo")
                            {
                                RecurringSales.ComboId = oConnectionContext.DbClsRecurringSalesDetails.Where(a => a.RecurringSalesDetailsId == RecurringSales.RecurringSalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsRecurringSalesDetails.Where(a => a.ComboId == RecurringSales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.RecurringSalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _RecurringSalesDetails.Add(new ClsRecurringSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * RecurringSales.Quantity, Under = RecurringSales.ItemDetailsId, IsComboItems = true, ComboId = RecurringSales.ComboId, DivId = RecurringSales.DivId, RecurringSalesDetailsId = item.RecurringSalesDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1, IsDeleted = RecurringSales.IsDeleted });
                                }
                                _RecurringSalesDetails.Add(RecurringSales);
                            }
                            else
                            {
                                _RecurringSalesDetails.Add(RecurringSales);
                            }
                        }
                        else
                        {
                            _RecurringSalesDetails.Add(RecurringSales);
                        }
                    }
                }

                obj.RecurringSalesDetails = _RecurringSalesDetails;

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                ClsRecurringSales oClsRecurringSales = new ClsRecurringSales()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    Status = obj.Status,
                    CustomerId = obj.CustomerId,
                    TotalDiscount = obj.TotalDiscount,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    Notes = obj.Notes,
                    PaymentTermId = obj.PaymentTermId,
                    RecurringSalesId = obj.RecurringSalesId,
                    //InvoiceNo = obj.InvoiceNo,
                    Subtotal = obj.Subtotal,
                    TaxId = obj.TaxId,
                    TotalQuantity = obj.TotalQuantity,
                    BranchId = obj.BranchId,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    ExchangeRate = obj.ExchangeRate,
                    TaxAmount = obj.TaxAmount,
                    ChangeReturn = obj.ChangeReturn,
                    TotalPaying = obj.TotalPaying,
                    Balance = obj.Balance,
                    HoldReason = obj.HoldReason,
                    //Status = obj.Status,
                    PaymentType = obj.PaymentType,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    UserGroupId = UserGroupId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxExemptionId = obj.TaxExemptionId,
                    GstPayment = obj.GstPayment,
                    IsReverseCharge = obj.IsReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    Terms = obj.Terms,
                    DisplayName = obj.DisplayName,
                    RepeatEveryNumber = obj.RepeatEveryNumber,
                    RepeatEvery = obj.RepeatEvery,
                    StartDate = obj.StartDate,
                    EndDate = obj.EndDate,
                    IsNeverExpires = obj.IsNeverExpires,
                    //LastInvoiceDate = obj.StartDate,
                    //NextInvoiceDate = obj.StartDate
                };

                string pic1 = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == obj.RecurringSalesId).Select(a => a.AttachDocument).FirstOrDefault();
                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/RecurringSales/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/RecurringSales/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsRecurringSales.AttachDocument = filepathPass;
                }
                else
                {
                    oClsRecurringSales.AttachDocument = pic1;
                }

                oConnectionContext.DbClsRecurringSales.Attach(oClsRecurringSales);
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.CompanyId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RecurringSalesId).IsModified = true;
                //oConnectionContext.Entry(oClsRecurringSales).Property(x => x.InvoiceNo).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.ChangeReturn).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TotalPaying).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Balance).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.HoldReason).IsModified = true;
                //oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.PaymentType).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.NetAmount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RoundOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TaxAccountId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.PlaceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.GstPayment).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.IsReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.TaxCollectedFromCustomer).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.Terms).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.DisplayName).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RepeatEveryNumber).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.RepeatEvery).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.StartDate).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.EndDate).IsModified = true;
                oConnectionContext.Entry(oClsRecurringSales).Property(x => x.IsNeverExpires).IsModified = true;
                //oConnectionContext.Entry(oClsRecurringSales).Property(x => x.LastInvoiceDate).IsModified = true;
                //oConnectionContext.Entry(oClsRecurringSales).Property(x => x.NextInvoiceDate).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.RecurringSalesAdditionalCharges != null)
                {
                    foreach (var item in obj.RecurringSalesAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();


                        long RecurringSalesAdditionalChargesId = 0;
                        if (item.RecurringSalesAdditionalChargesId == 0)
                        {
                            ClsRecurringSalesAdditionalCharges oClsRecurringSalesAdditionalCharges = new ClsRecurringSalesAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.SalesAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                TaxExemptionId = item.TaxExemptionId,
                                IsActive = item.IsActive,
                                IsDeleted = item.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId
                            };
                            oConnectionContext.DbClsRecurringSalesAdditionalCharges.Add(oClsRecurringSalesAdditionalCharges);
                            oConnectionContext.SaveChanges();

                            RecurringSalesAdditionalChargesId = oClsRecurringSalesAdditionalCharges.RecurringSalesAdditionalChargesId;
                        }
                        else
                        {
                            ClsRecurringSalesAdditionalCharges oClsRecurringSalesAdditionalCharges = new ClsRecurringSalesAdditionalCharges()
                            {
                                RecurringSalesAdditionalChargesId = item.RecurringSalesAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.SalesAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                TaxExemptionId = item.TaxExemptionId,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsRecurringSalesAdditionalCharges.Attach(oClsRecurringSalesAdditionalCharges);
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.RecurringSalesId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();

                            RecurringSalesAdditionalChargesId = oClsRecurringSalesAdditionalCharges.RecurringSalesAdditionalChargesId;
                        }
                    }
                }

                if (obj.RecurringSalesDetails != null)
                {
                    foreach (var RecurringSales in obj.RecurringSalesDetails)
                    {
                        if (obj.RecurringSalesType == "Bill Of Supply")
                        {
                            RecurringSales.TaxId = oConnectionContext.DbClsTax.Where(b => b.Tax == "Non-Taxable").Select(b => b.TaxId).FirstOrDefault();
                            RecurringSales.TaxExemptionId = oConnectionContext.DbClsTaxExemption.Where(b => b.CanDelete == false).Select(b => b.TaxExemptionId).FirstOrDefault();
                        }

                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == RecurringSales.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        if (RecurringSales.RecurringSalesDetailsId == 0)
                        {
                            decimal convertedStock = 0, freeConvertedStock = 0;                                                        

                            ClsRecurringSalesDetails oClsRecurringSalesDetails = new ClsRecurringSalesDetails()
                            {
                                DiscountType = RecurringSales.DiscountType,
                                OtherInfo = RecurringSales.OtherInfo,
                                PriceIncTax = RecurringSales.PriceIncTax,
                                ItemId = RecurringSales.ItemId,
                                ItemDetailsId = RecurringSales.ItemDetailsId,
                                RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                                TaxId = RecurringSales.TaxId,
                                Discount = RecurringSales.Discount,
                                Quantity = RecurringSales.Quantity,
                                UnitCost = RecurringSales.UnitCost,
                                IsActive = RecurringSales.IsActive,
                                IsDeleted = RecurringSales.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                //StockDeductionIds = RecurringSales.StockDeductionIds,
                                //QuantityRemaining = RecurringSales.IsComboItems == true ? (RecurringSales.Quantity + RecurringSales.FreeQuantity) : (convertedStock + freeConvertedStock),
                                QuantityRemaining = RecurringSales.ProductType == "Combo" ? (RecurringSales.Quantity + RecurringSales.FreeQuantity) : (convertedStock + freeConvertedStock),
                                WarrantyId = RecurringSales.WarrantyId,
                                DefaultUnitCost = DefaultUnitCost,
                                DefaultAmount = RecurringSales.Quantity * DefaultUnitCost,
                                PriceAddedFor = RecurringSales.PriceAddedFor,
                                LotId = RecurringSales.LotId,
                                LotType = RecurringSales.LotType,
                                FreeQuantity = RecurringSales.FreeQuantity,
                                //FreeQuantityPriceAddedFor = RecurringSales.FreeQuantityPriceAddedFor,
                                AmountExcTax = RecurringSales.AmountExcTax,
                                TaxAmount = RecurringSales.TaxAmount,
                                PriceExcTax = RecurringSales.PriceExcTax,
                                AmountIncTax = RecurringSales.AmountIncTax,
                                UnitAddedFor = RecurringSales.UnitAddedFor,
                                LotIdForLotNoChecking = RecurringSales.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = RecurringSales.LotTypeForLotNoChecking,
                                ComboId = RecurringSales.ComboId,
                                IsComboItems = RecurringSales.IsComboItems,
                                QuantitySold = convertedStock + freeConvertedStock,
                                ComboPerUnitQuantity = RecurringSales.ComboPerUnitQuantity,
                                ExtraDiscount = RecurringSales.ExtraDiscount,
                                TaxExemptionId = RecurringSales.TaxExemptionId,
                                ItemCodeId = RecurringSales.ItemCodeId,
                                TotalTaxAmount = RecurringSales.TotalTaxAmount,
                                IsCombo = RecurringSales.ProductType == "Combo" ? true : false,
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsRecurringSalesDetails.Add(oClsRecurringSalesDetails);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            decimal QuantityReturned = 0;
                            decimal convertedStock = 0, freeConvertedStock = 0;

                            ClsRecurringSalesDetails oClsRecurringSalesDetails = new ClsRecurringSalesDetails()
                            {
                                RecurringSalesDetailsId = RecurringSales.RecurringSalesDetailsId,
                                DiscountType = RecurringSales.DiscountType,
                                OtherInfo = RecurringSales.OtherInfo,
                                PriceIncTax = RecurringSales.PriceIncTax,
                                ItemId = RecurringSales.ItemId,
                                ItemDetailsId = RecurringSales.ItemDetailsId,
                                RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                                TaxId = RecurringSales.TaxId,
                                Discount = RecurringSales.Discount,
                                Quantity = RecurringSales.Quantity,
                                UnitCost = RecurringSales.UnitCost,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                //StockDeductionIds = RecurringSales.StockDeductionIds,
                                QuantityRemaining = RecurringSales.ProductType == "Combo" ? (RecurringSales.Quantity + RecurringSales.FreeQuantity) - QuantityReturned : (convertedStock + freeConvertedStock) - QuantityReturned,
                                //QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityReturned,
                                WarrantyId = RecurringSales.WarrantyId,
                                DefaultUnitCost = DefaultUnitCost,
                                DefaultAmount = RecurringSales.Quantity * DefaultUnitCost,
                                PriceAddedFor = RecurringSales.PriceAddedFor,
                                LotId = RecurringSales.LotId,
                                LotType = RecurringSales.LotType,
                                FreeQuantity = RecurringSales.FreeQuantity,
                                //FreeQuantityPriceAddedFor = RecurringSales.FreeQuantityPriceAddedFor,
                                AmountExcTax = RecurringSales.AmountExcTax,
                                TaxAmount = RecurringSales.TaxAmount,
                                PriceExcTax = RecurringSales.PriceExcTax,
                                AmountIncTax = RecurringSales.AmountIncTax,
                                UnitAddedFor = RecurringSales.UnitAddedFor,
                                LotIdForLotNoChecking = RecurringSales.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = RecurringSales.LotTypeForLotNoChecking,
                                ComboId = RecurringSales.ComboId,
                                IsComboItems = RecurringSales.IsComboItems,
                                QuantitySold = convertedStock + freeConvertedStock,
                                ComboPerUnitQuantity = RecurringSales.ComboPerUnitQuantity,
                                ExtraDiscount = RecurringSales.ExtraDiscount,
                                TaxExemptionId = RecurringSales.TaxExemptionId,
                                ItemCodeId = RecurringSales.ItemCodeId,
                                TotalTaxAmount = RecurringSales.TotalTaxAmount,
                                IsCombo = RecurringSales.ProductType == "Combo" ? true : false,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsRecurringSalesDetails.Attach(oClsRecurringSalesDetails);
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.DiscountType).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.OtherInfo).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.PriceIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.RecurringSalesId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.Discount).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ModifiedOn).IsModified = true;
                            //oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.StockDeductionIds).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.WarrantyId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.DefaultUnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.DefaultAmount).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.LotId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.LotType).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.FreeQuantity).IsModified = true;
                            //oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.PriceExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ComboId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.IsComboItems).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.QuantitySold).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ComboPerUnitQuantity).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.DiscountAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.TaxAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.PurchaseAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.InventoryAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.WarrantyExpiryDate).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ExtraDiscount).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsRecurringSalesDetails).Property(x => x.IsCombo).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                oCommonController.CreateRecurringSalesInvoices(oClsRecurringSales.RecurringSalesId);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Recurring Sales",
                    CompanyId = obj.CompanyId,
                    Description = "Recurring Sales" + " \"" + oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == obj.RecurringSalesId).Select(a => a.DisplayName).FirstOrDefault() + "\" updated",
                    Id = oClsRecurringSales.RecurringSalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Recurring Sales updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            RecurringSalesId = oClsRecurringSales.RecurringSalesId,
                        },
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
