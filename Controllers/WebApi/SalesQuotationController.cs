using EquiBillBook.Controllers.WebApi.Common;
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
    public class SalesQuotationController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllSalesQuotations(ClsSalesQuotationVm obj)
        {
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
            List<ClsSalesQuotationVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsSalesQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsSalesQuotationVm
                {
                    IsInvoiced = a.IsInvoiced,
                    TotalTaxAmount = a.TotalTaxAmount,
                    TotalDiscount = a.TotalDiscount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    Status = a.Status,
                    InvoiceUrl = oCommonController.webUrl,//+ "/SalesQuotations/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    SalesQuotationId = a.SalesQuotationId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    SalesQuotationDate = a.SalesQuotationDate,
                    //SalesQuotationType = a.SalesQuotationType,
                    InvoiceNo = a.InvoiceNo,
                    Subtotal = a.Subtotal,
                    CustomerId = a.CustomerId,
                    CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                    CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    //PaymentStatus = a.PaymentStatus,
                    TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesQuotationDetails.Where(c=>c.SalesQuotationId==a.SalesQuotationId && c.IsDeleted==false).Count()
                    PaidQuantity = oConnectionContext.DbClsSalesQuotationDetails.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsSalesQuotationDetails.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    CanEdit = (oConnectionContext.DbClsSalesOrder.Where(c => c.IsDeleted == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0 &&
                    oConnectionContext.DbClsSalesProforma.Where(c => c.IsDeleted == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0 &&
                    oConnectionContext.DbClsSales.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0 &&
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0) ? true : false,
                    TotalItems = oConnectionContext.DbClsSalesQuotationDetails.Where(c => c.SalesQuotationId == a.SalesQuotationId &&
                    c.IsDeleted == false).Count()
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsSalesQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                a.Status.ToLower() == obj.Status.ToLower()
&& a.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsSalesQuotationVm
               {
                   IsInvoiced = a.IsInvoiced,
                   TotalTaxAmount = a.TotalTaxAmount,
                   TotalDiscount = a.TotalDiscount,
                   InvoiceId = a.InvoiceId,
                   BranchId = a.BranchId,
                   Status = a.Status,
                   InvoiceUrl = oCommonController.webUrl,//+ "/SalesQuotations/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                   BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                   SalesQuotationId = a.SalesQuotationId,
                   GrandTotal = a.GrandTotal,
                   Notes = a.Notes,
                   SalesQuotationDate = a.SalesQuotationDate,
                   //SalesQuotationType = a.SalesQuotationType,
                   InvoiceNo = a.InvoiceNo,
                   Subtotal = a.Subtotal,
                   CustomerId = a.CustomerId,
                   CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                   CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                   CompanyId = a.CompanyId,
                   IsActive = a.IsActive,
                   IsDeleted = a.IsDeleted,
                   AddedBy = a.AddedBy,
                   AddedOn = a.AddedOn,
                   ModifiedBy = a.ModifiedBy,
                   ModifiedOn = a.ModifiedOn,
                   AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                   ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                   //PaymentStatus = a.PaymentStatus,
                   TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesQuotationDetails.Where(c=>c.SalesQuotationId==a.SalesQuotationId && c.IsDeleted==false).Count()
                   PaidQuantity = oConnectionContext.DbClsSalesQuotationDetails.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                   FreeQuantity = oConnectionContext.DbClsSalesQuotationDetails.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                   CanEdit = (oConnectionContext.DbClsSalesOrder.Where(c => c.IsDeleted == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0 &&
                    oConnectionContext.DbClsSalesProforma.Where(c => c.IsDeleted == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0 &&
                    oConnectionContext.DbClsSales.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0 &&
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesQuotationId && c.ReferenceType == "sales quotation").Count() == 0) ? true : false,
                   TotalItems = oConnectionContext.DbClsSalesQuotationDetails.Where(c => c.SalesQuotationId == a.SalesQuotationId &&
                    c.IsDeleted == false).Count()
               }).ToList();
            }

            if (obj.Status != null && obj.Status != "")
            {
                det = det.Where(a => a.Status.ToLower() == obj.Status.ToLower()).Select(a => a).ToList();
            }

            if (obj.InvoiceNo != null && obj.InvoiceNo != "")
            {
                det = det.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower()).Select(a => a).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.IsInvoiced != 0)
            {
                det = det.Where(a => a.IsInvoiced == obj.IsInvoiced).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesQuotations = det.OrderByDescending(a => a.SalesQuotationId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> SalesQuotation(ClsSalesQuotationVm obj)
        {
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
            //bool EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

            var det = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == obj.SalesQuotationId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.Terms,
                GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                PayTaxForExport = a.PayTaxForExport,
                TaxCollectedFromCustomer = a.TaxCollectedFromCustomer,
                a.IsReverseCharge,
                TaxableAmount = a.TaxableAmount,
                NetAmountReverseCharge = a.NetAmountReverseCharge,
                RoundOffReverseCharge = a.RoundOffReverseCharge,
                GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                a.TaxExemptionId,
                BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                PlaceOfSupplyId = a.PlaceOfSupplyId,
                //a.PaymentStatus,
                a.TotalTaxAmount,
                //a.SalesQuotationType,
                a.InvoiceId,
                a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                a.NetAmount,
                a.ExchangeRate,
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                DefaultCurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(c => c.IsMain == true && c.CompanyId == obj.CompanyId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                a.BranchId,
                PaymentType = a.PaymentType == null ? "" : a.PaymentType,
                a.HoldReason,
                TotalPaying = a.TotalPaying,
                a.Balance,
                a.ChangeReturn,
                a.CustomerId,
                a.SellingPriceGroupId,
                a.Status,
                a.PayTerm,
                a.PayTermNo,
                a.AttachDocument,
                SalesQuotationId = a.SalesQuotationId,
                a.GrandTotal,
                a.TaxId,
                a.TotalDiscount,
                a.TotalQuantity,
                a.Discount,
                a.DiscountType,
                a.Notes,
                a.SalesQuotationDate,
                a.ShippingAddress,
                a.ShippingDetails,
                a.ShippingDocument,
                a.ShippingStatus,
                a.DeliveredTo,
                a.InvoiceNo,
                a.Subtotal,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                a.OnlinePaymentSettingsId,
                a.SmsSettingsId,
                a.EmailSettingsId,
                a.WhatsappSettingsId,
                a.TaxAmount,
                SalesQuotationDetails = (from b in oConnectionContext.DbClsSalesQuotationDetails
                                         join c in oConnectionContext.DbClsItemBranchMap
                                         on b.ItemDetailsId equals c.ItemDetailsId
                                         join d in oConnectionContext.DbClsItemDetails
                                          on b.ItemDetailsId equals d.ItemDetailsId
                                         join e in oConnectionContext.DbClsItem
                                         on d.ItemId equals e.ItemId
                                         where b.SalesQuotationId == a.SalesQuotationId && b.IsDeleted == false && c.BranchId == a.BranchId
                                         && b.IsComboItems == false
                                         select new
                                         {
                                             b.TotalTaxAmount,
                                             ItemCodeId = b.ItemCodeId,
                                             ItemType = e.ItemType,
                                             b.TaxExemptionId,
                                             b.ExtraDiscount,
                                             e.IsManageStock,
                                             Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                                             : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                                             //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f=>f.CompanyId == obj.CompanyId).Select(f=>f.EnableWarranty).FirstOrDefault(),
                                             e.EnableImei,
                                             b.WarrantyId,
                                             b.FreeQuantity,
                                             //b.FreeQuantityPriceAddedFor,
                                             b.PriceExcTax,
                                             b.PriceIncTax,
                                             b.AmountExcTax,
                                             b.TaxAmount,
                                             b.AmountIncTax,
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
                                             QuantityRemaining = a.Status.ToLower() != "draft" ? ((b.LotType == "purchase" ?
                                             oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             : b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             : b.LotType == "stocktransfer" ? oConnectionContext.DbClsStockTransferDetails.Where(f => f.StockTransferDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                             : c.Quantity)) : c.Quantity,
                                             b.DiscountType,
                                             b.SalesQuotationDetailsId,
                                             b.OtherInfo,
                                             b.Discount,
                                             b.SalesQuotationId,
                                             b.Quantity,
                                             b.TaxId,
                                             b.UnitCost,
                                             d.ItemId,
                                             e.ProductType,
                                             d.ItemDetailsId,
                                             e.ItemName,
                                             SKU = d.SKU == null ? e.SkuCode : d.SKU,
                                             d.VariationDetailsId,
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == d.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == e.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                             SalesExcTax = d.SalesExcTax,
                                             SalesIncTax = b.LotTypeForLotNoChecking == "purchase" ?
                                             oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                             : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                             //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                             : d.SalesIncTax,
                                             d.TotalCost,
                                             Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                             TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                             e.TaxType,
                                             e.ItemCode,
                                             b.DefaultUnitCost,
                                             b.DefaultAmount,
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
                SalesQuotationAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                ).Select(b => new ClsSalesQuotationAdditionalChargesVm
                {
                    SalesQuotationAdditionalChargesId = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c=>c.SalesQuotationAdditionalChargesId).FirstOrDefault(),
                    Name = b.Name,
                    AdditionalChargeId = b.AdditionalChargeId,
                    SalesQuotationId = a.SalesQuotationId,
                    TaxId = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                    AmountExcTax = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                    AmountIncTax = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                    TaxAmount = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                    AccountId = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(c => c.SalesQuotationId == a.SalesQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                    ItemCodeId = b.ItemCodeId,
                    TaxExemptionId = b.TaxExemptionId,
                    TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                }).ToList(),
            }).FirstOrDefault();

            var AllTaxs = oConnectionContext.DbClsSalesQuotation.Where(a => a.IsDeleted == false && a.SalesQuotationId == det.SalesQuotationId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.SalesQuotationId == det.SalesQuotationId && a.IsDeleted == false
                                && a.IsComboItems == false).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).Concat(oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(a => a.SalesQuotationId == det.SalesQuotationId
                                && a.IsDeleted == false && a.AmountExcTax>0).Select(a => new
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
                    SalesQuotation = det,
                    Taxs = finalTaxs,
                    //ItemDetails = ItemDetails
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSalesQuotation(ClsSalesQuotationVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                string PrefixType = "";

                long PrefixUserMapId = 0;

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

                if (obj.SalesQuotationDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesQuotationDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.InvoiceNo != "" && obj.InvoiceNo != null)
                {
                    if (oConnectionContext.DbClsSalesQuotation.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Quotation# exists", Id = "divInvoiceNo" });
                        isError = true;
                    }
                }

                if (obj.SalesQuotationDetails == null || obj.SalesQuotationDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var item in obj.SalesQuotationDetails)
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

                long PrefixId = 0;
                if (obj.InvoiceNo == "" || obj.InvoiceNo == null)
                {
                    // Hybrid approach: Check Customer PrefixId first, then fall back to Branch PrefixId
                    long customerPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => a.PrefixId).FirstOrDefault();
                    
                    if (customerPrefixId != 0)
                    {
                        // Use Customer's PrefixId if set
                        PrefixId = customerPrefixId;
                    }
                    else
                    {
                        // Fall back to Branch PrefixId
                        PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    }
                    
                    PrefixType = "Sales Quotation";
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == PrefixType.ToLower()
                                          && b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.InvoiceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                List<ClsSalesQuotationDetailsVm> _SalesQuotationsDetails = new List<ClsSalesQuotationDetailsVm>();
                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var SalesQuotations in obj.SalesQuotationDetails)
                    {
                        SalesQuotations.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ProductType).FirstOrDefault();
                        if (SalesQuotations.ProductType.ToLower() == "combo")
                        {
                            SalesQuotations.ComboId = oCommonController.CreateToken();
                            var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => new
                            {
                                ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                ItemDetailsId = a.ItemDetailsId,
                                ComboItemDetailsId = a.ComboItemDetailsId,
                                Quantity = a.Quantity,
                                a.PriceAddedFor
                            }).ToList();

                            foreach (var item in combo)
                            {
                                _SalesQuotationsDetails.Add(new ClsSalesQuotationDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesQuotations.Quantity, Under = SalesQuotations.ItemDetailsId, IsComboItems = true, ComboId = SalesQuotations.ComboId, DivId = SalesQuotations.DivId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                            }
                            _SalesQuotationsDetails.Add(SalesQuotations);
                        }
                        else
                        {
                            _SalesQuotationsDetails.Add(SalesQuotations);
                        }
                    }
                }

                obj.SalesQuotationDetails = _SalesQuotationsDetails;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                ClsSalesQuotation oClsSalesQuotation = new ClsSalesQuotation()
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
                    DeliveredTo = obj.DeliveredTo,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    PayTerm = obj.PayTerm,
                    PayTermNo = obj.PayTermNo,
                    SalesQuotationDate = obj.SalesQuotationDate.AddHours(5).AddMinutes(30),
                    SalesQuotationId = obj.SalesQuotationId,
                    InvoiceNo = obj.InvoiceNo,
                    ShippingAddress = obj.ShippingAddress,
                    ShippingDetails = obj.ShippingDetails,
                    ShippingStatus = obj.ShippingStatus,
                    Subtotal = obj.Subtotal,
                    TaxId = obj.TaxId,
                    TotalQuantity = obj.TotalQuantity,
                    BranchId = obj.BranchId,
                    //SalesQuotationType = obj.SalesQuotationType,
                    //PaymentStatus = obj.Status == "Sent" ? obj.PaymentStatus : "",
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ExchangeRate = obj.ExchangeRate,
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    TaxAmount = obj.TaxAmount,
                    InvoiceId = oCommonController.CreateToken(),//DateTime.Now.ToFileTime(),
                    CashRegisterId = obj.CashRegisterId,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    UserGroupId = UserGroupId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxExemptionId = obj.TaxExemptionId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    PrefixId = PrefixId,
                    PreviousStatus = obj.Status,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    Terms= obj.Terms
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SalesQuotations/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesQuotations/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesQuotation.AttachDocument = filepathPass;
                }

                if (obj.ShippingDocument != "" && obj.ShippingDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SalesQuotations/ShippingDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionShippingDocument;

                    string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesQuotations/ShippingDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesQuotation.ShippingDocument = filepathPass;
                }

                oConnectionContext.DbClsSalesQuotation.Add(oClsSalesQuotation);
                oConnectionContext.SaveChanges();

                if (obj.SalesQuotationAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesQuotationAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

                        ClsSalesQuotationAdditionalCharges oClsSalesQuotationAdditionalCharges = new ClsSalesQuotationAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
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
                        oConnectionContext.DbClsSalesQuotationAdditionalCharges.Add(oClsSalesQuotationAdditionalCharges);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var SalesQuotations in obj.SalesQuotationDetails)
                    {
                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == SalesQuotations.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        decimal convertedStock = 0, freeConvertedStock = 0;
                        if (SalesQuotations.ProductType != "Combo")
                        {
                            convertedStock = oCommonController.StockConversion(SalesQuotations.Quantity, SalesQuotations.ItemId, SalesQuotations.PriceAddedFor);
                            freeConvertedStock = oCommonController.StockConversion(SalesQuotations.FreeQuantity, SalesQuotations.ItemId, SalesQuotations.PriceAddedFor);
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.IsManageStock).FirstOrDefault();

                            if (SalesQuotations.LotType == "stocktransfer")
                            {
                                SalesQuotations.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesQuotations.LotId).Select(a => a.LotId).FirstOrDefault();
                                SalesQuotations.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesQuotations.LotId).Select(a => a.LotType).FirstOrDefault();
                            }
                            else
                            {
                                SalesQuotations.LotIdForLotNoChecking = SalesQuotations.LotId;
                                SalesQuotations.LotTypeForLotNoChecking = SalesQuotations.LotType;
                            }
                        }

                        DateTime? WarrantyExpiryDate = null;
                        if (SalesQuotations.WarrantyId != 0)
                        {
                            var warranty = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == SalesQuotations.WarrantyId).Select(a => new
                            {
                                a.Duration,
                                a.DurationNo
                            }).FirstOrDefault();

                            if (warranty.Duration == "Days")
                            {
                                WarrantyExpiryDate = obj.SalesQuotationDate.AddDays(warranty.DurationNo);
                            }
                            else if (warranty.Duration == "Months")
                            {
                                WarrantyExpiryDate = obj.SalesQuotationDate.AddMonths(Convert.ToInt32(warranty.DurationNo));
                            }
                            else if (warranty.Duration == "Years")
                            {
                                WarrantyExpiryDate = obj.SalesQuotationDate.AddYears(Convert.ToInt32(warranty.DurationNo));
                            }
                        }

                        SalesQuotations.ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                        ClsSalesQuotationDetails oClsSalesQuotationDetails = new ClsSalesQuotationDetails()
                        {
                            DiscountType = SalesQuotations.DiscountType,
                            OtherInfo = SalesQuotations.OtherInfo,
                            PriceIncTax = SalesQuotations.PriceIncTax,
                            ItemId = SalesQuotations.ItemId,
                            ItemDetailsId = SalesQuotations.ItemDetailsId,
                            SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
                            TaxId = SalesQuotations.TaxId,
                            Discount = SalesQuotations.Discount,
                            Quantity = SalesQuotations.Quantity,
                            UnitCost = SalesQuotations.UnitCost,
                            IsActive = SalesQuotations.IsActive,
                            IsDeleted = SalesQuotations.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            //StockDeductionIds = SalesQuotations.StockDeductionIds,
                            QuantityRemaining = SalesQuotations.ProductType == "Combo" ? (SalesQuotations.Quantity + SalesQuotations.FreeQuantity) : (convertedStock + freeConvertedStock),
                            WarrantyId = SalesQuotations.WarrantyId,
                            DefaultUnitCost = DefaultUnitCost,
                            DefaultAmount = SalesQuotations.Quantity * DefaultUnitCost,
                            PriceAddedFor = SalesQuotations.PriceAddedFor,
                            LotId = SalesQuotations.LotId,
                            LotType = SalesQuotations.LotType,
                            FreeQuantity = SalesQuotations.FreeQuantity,
                            //FreeQuantityPriceAddedFor = SalesQuotations.FreeQuantityPriceAddedFor,
                            AmountExcTax = SalesQuotations.AmountExcTax,
                            TaxAmount = SalesQuotations.TaxAmount,
                            PriceExcTax = SalesQuotations.PriceExcTax,
                            AmountIncTax = SalesQuotations.AmountIncTax,
                            Under = SalesQuotations.Under,
                            UnitAddedFor = SalesQuotations.UnitAddedFor,
                            LotIdForLotNoChecking = SalesQuotations.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = SalesQuotations.LotTypeForLotNoChecking,
                            ComboId = SalesQuotations.ComboId,
                            IsComboItems = SalesQuotations.IsComboItems,
                            QuantitySold = convertedStock + freeConvertedStock,
                            ComboPerUnitQuantity = SalesQuotations.ComboPerUnitQuantity,
                            WarrantyExpiryDate = WarrantyExpiryDate,
                            ExtraDiscount = SalesQuotations.ExtraDiscount,
                            ItemCodeId = SalesQuotations.ItemCodeId,
                            TaxExemptionId = SalesQuotations.TaxExemptionId,
                            TotalTaxAmount = SalesQuotations.TotalTaxAmount,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsSalesQuotationDetails.Add(oClsSalesQuotationDetails);
                        oConnectionContext.SaveChanges();
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
                    Category = "Sales Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Quotation \"" + obj.InvoiceNo + "\" created",
                    Id = oClsSalesQuotation.SalesQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Quotation", obj.CompanyId, oClsSalesQuotation.SalesQuotationId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                //if (arr[0] == "1")
                //{
                //    oCommonController.InsertSmsUsed(oClsSalesQuotation.SalesQuotationId, obj.Status, obj.CompanyId, obj.AddedBy, CurrentDate);
                //}

                data = new
                {
                    Status = 1,
                    Message = "Sales Quotation created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        SalesQuotation = new
                        {
                            SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
                            InvoiceId = oClsSalesQuotation.InvoiceId
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceQuotation }).FirstOrDefault(),
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesQuotationDelete(ClsSalesQuotationVm obj)
        {
            int SalesOrderCount = (from a in oConnectionContext.DbClsSalesOrder
                                   where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                       && a.ReferenceId == obj.SalesQuotationId && a.ReferenceType == "sales quotation"
                                   select a.SalesOrderId).Count();

            int SalesProformaCount = (from a in oConnectionContext.DbClsSalesProforma
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                       && a.ReferenceId == obj.SalesQuotationId && a.ReferenceType == "sales quotation"
                                      select a.SalesProformaId).Count();

            int DeliveryChallanCount = (from a in oConnectionContext.DbClsDeliveryChallan
                                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                       && a.ReferenceId == obj.SalesQuotationId && a.ReferenceType == "sales quotation"
                                        select a.DeliveryChallanId).Count();

            int SalesCount = (from a in oConnectionContext.DbClsSales
                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                              && a.ReferenceId == obj.SalesQuotationId && a.ReferenceType == "sales quotation"
                              select a.SalesId).Count();

            if (SalesOrderCount > 0 || SalesProformaCount > 0 || DeliveryChallanCount > 0 || SalesCount > 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Cannot delete as it is already in use",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsSalesQuotation oClsSalesQuotation = new ClsSalesQuotation()
                {
                    SalesQuotationId = obj.SalesQuotationId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesQuotation.Attach(oClsSalesQuotation);
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SalesQuotationId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Quotation \"" + oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == obj.SalesQuotationId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsSalesQuotation.SalesQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Quotation deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesQuotation(ClsSalesQuotationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
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

                if (obj.SalesQuotationDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesQuotationDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.SalesQuotationDetails == null || obj.SalesQuotationDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var item in obj.SalesQuotationDetails)
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

                List<ClsSalesQuotationDetailsVm> _SalesQuotationsDetails = new List<ClsSalesQuotationDetailsVm>();
                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var SalesQuotations in obj.SalesQuotationDetails)
                    {
                        if (SalesQuotations.SalesQuotationDetailsId != 0)
                        {
                            SalesQuotations.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesQuotations.ProductType.ToLower() == "combo")
                            {
                                SalesQuotations.ComboId = oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.SalesQuotationDetailsId == SalesQuotations.SalesQuotationDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.ComboId == SalesQuotations.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.SalesQuotationDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesQuotationsDetails.Add(new ClsSalesQuotationDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesQuotations.Quantity, Under = SalesQuotations.ItemDetailsId, IsComboItems = true, ComboId = SalesQuotations.ComboId, DivId = SalesQuotations.DivId, SalesQuotationDetailsId = item.SalesQuotationDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1, IsDeleted = SalesQuotations.IsDeleted });
                                }
                                _SalesQuotationsDetails.Add(SalesQuotations);
                            }
                            else
                            {
                                _SalesQuotationsDetails.Add(SalesQuotations);
                            }
                        }
                        else
                        {
                            _SalesQuotationsDetails.Add(SalesQuotations);
                        }
                    }
                }

                obj.SalesQuotationDetails = _SalesQuotationsDetails;

                obj.SalesQuotationDetails.RemoveAll(r => r.IsComboItems == true);
                //obj.SalesQuotationDetails.RemoveAll(r => r.IsDeleted == true);

                List<ClsSalesQuotationDetailsVm> _SalesQuotationsDetails1 = new List<ClsSalesQuotationDetailsVm>();
                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var SalesQuotations in obj.SalesQuotationDetails)
                    {
                        if (SalesQuotations.SalesQuotationDetailsId == 0)
                        {
                            SalesQuotations.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesQuotations.ProductType.ToLower() == "combo")
                            {
                                SalesQuotations.ComboId = oCommonController.CreateToken();
                                var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => new
                                {
                                    ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ComboItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesQuotationsDetails1.Add(new ClsSalesQuotationDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesQuotations.Quantity, Under = SalesQuotations.ItemDetailsId, IsComboItems = true, ComboId = SalesQuotations.ComboId, DivId = SalesQuotations.DivId, SalesQuotationDetailsId = SalesQuotations.SalesQuotationDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesQuotationsDetails1.Add(SalesQuotations);
                            }
                            else
                            {
                                _SalesQuotationsDetails1.Add(SalesQuotations);
                            }
                        }
                        else
                        {
                            SalesQuotations.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesQuotations.ProductType.ToLower() == "combo")
                            {
                                SalesQuotations.ComboId = oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.SalesQuotationDetailsId == SalesQuotations.SalesQuotationDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.ComboId == SalesQuotations.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == SalesQuotations.ItemId && b.ComboItemDetailsId == a.ItemDetailsId).Select(b => b.Quantity).FirstOrDefault(),
                                    a.SalesQuotationDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesQuotationsDetails1.Add(new ClsSalesQuotationDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesQuotations.Quantity, Under = SalesQuotations.ItemDetailsId, IsComboItems = true, ComboId = SalesQuotations.ComboId, DivId = SalesQuotations.DivId, SalesQuotationDetailsId = item.SalesQuotationDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesQuotationsDetails1.Add(SalesQuotations);
                            }
                            else
                            {
                                _SalesQuotationsDetails1.Add(SalesQuotations);
                            }
                        }
                    }
                }

                obj.SalesQuotationDetails = _SalesQuotationsDetails1;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                ClsSalesQuotation oClsSalesQuotation = new ClsSalesQuotation()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    Status = obj.Status,
                    CustomerId = obj.CustomerId,
                    TotalDiscount = obj.TotalDiscount,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    DeliveredTo = obj.DeliveredTo,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    Notes = obj.Notes,
                    PayTerm = obj.PayTerm,
                    PayTermNo = obj.PayTermNo,
                    SalesQuotationDate = obj.SalesQuotationDate.AddHours(5).AddMinutes(30),
                    SalesQuotationId = obj.SalesQuotationId,
                    //InvoiceNo = obj.InvoiceNo,
                    ShippingAddress = obj.ShippingAddress,
                    ShippingDetails = obj.ShippingDetails,
                    ShippingStatus = obj.ShippingStatus,
                    Subtotal = obj.Subtotal,
                    TaxId = obj.TaxId,
                    TotalQuantity = obj.TotalQuantity,
                    BranchId = obj.BranchId,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ExchangeRate = obj.ExchangeRate,
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    TaxAmount = obj.TaxAmount,
                    ChangeReturn = obj.ChangeReturn,
                    TotalPaying = obj.TotalPaying,
                    Balance = obj.Balance,
                    HoldReason = obj.HoldReason,
                    //PaymentStatus = obj.Status == "Sent" ? obj.PaymentStatus : "",
                    PaymentType = obj.PaymentType,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    UserGroupId = UserGroupId,
                    //SalesQuotationType = obj.SalesQuotationType,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxExemptionId = obj.TaxExemptionId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    Terms = obj.Terms
                };

                string pic1 = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == obj.SalesQuotationId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/SalesQuotations/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesQuotations/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesQuotation.AttachDocument = filepathPass;
                }
                else
                {
                    oClsSalesQuotation.AttachDocument = pic1;
                }

                pic1 = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == obj.SalesQuotationId).Select(a => a.ShippingDocument).FirstOrDefault();
                if (obj.ShippingDocument != "" && obj.ShippingDocument != null)
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/SalesQuotations/ShippingDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionShippingDocument;

                    string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesQuotations/ShippingDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesQuotation.ShippingDocument = filepathPass;
                }
                else
                {
                    oClsSalesQuotation.ShippingDocument = pic1;
                }

                oConnectionContext.DbClsSalesQuotation.Attach(oClsSalesQuotation);
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.CompanyId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.DeliveredTo).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PayTerm).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PayTermNo).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SalesQuotationDate).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SalesQuotationId).IsModified = true;
                //oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.InvoiceNo).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ShippingAddress).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ShippingDetails).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ShippingStatus).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ShippingDocument).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.OnlinePaymentSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ChangeReturn).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TotalPaying).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Balance).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.HoldReason).IsModified = true;
                //oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PaymentStatus).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PaymentType).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.NetAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TotalTaxAmount).IsModified = true;
                //oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SalesQuotationType).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PlaceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.TaxCollectedFromCustomer).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Terms).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.SalesQuotationAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesQuotationAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

                        if (item.SalesQuotationAdditionalChargesId == 0)
                        {
                            ClsSalesQuotationAdditionalCharges oClsSalesQuotationAdditionalCharges = new ClsSalesQuotationAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
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
                            oConnectionContext.DbClsSalesQuotationAdditionalCharges.Add(oClsSalesQuotationAdditionalCharges);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsSalesQuotationAdditionalCharges oClsSalesQuotationAdditionalCharges = new ClsSalesQuotationAdditionalCharges()
                            {
                                SalesQuotationAdditionalChargesId = item.SalesQuotationAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
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
                            oConnectionContext.DbClsSalesQuotationAdditionalCharges.Attach(oClsSalesQuotationAdditionalCharges);
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.SalesQuotationId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSalesQuotationAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.SalesQuotationDetails != null)
                {
                    foreach (var SalesQuotations in obj.SalesQuotationDetails)
                    {
                        if (SalesQuotations.IsDeleted == true)
                        {
                            string query = "update \"tblSalesQuotationDetails\" set \"IsDeleted\"=True where \"SalesQuotationDetailsId\"=" + SalesQuotations.SalesQuotationDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        else
                        {
                            var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == SalesQuotations.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                            DateTime? WarrantyExpiryDate = null;
                            if (SalesQuotations.WarrantyId != 0)
                            {
                                var warranty = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == SalesQuotations.WarrantyId).Select(a => new
                                {
                                    a.Duration,
                                    a.DurationNo
                                }).FirstOrDefault();

                                if (warranty.Duration == "Days")
                                {
                                    WarrantyExpiryDate = obj.SalesQuotationDate.AddDays(warranty.DurationNo);
                                }
                                else if (warranty.Duration == "Months")
                                {
                                    WarrantyExpiryDate = obj.SalesQuotationDate.AddMonths(Convert.ToInt32(warranty.DurationNo));
                                }
                                else if (warranty.Duration == "Years")
                                {
                                    WarrantyExpiryDate = obj.SalesQuotationDate.AddYears(Convert.ToInt32(warranty.DurationNo));
                                }
                            }

                            SalesQuotations.SalesQuotationDetailsId = oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.CompanyId == obj.CompanyId
                                && a.IsDeleted == false && a.SalesQuotationId == obj.SalesQuotationId && a.ItemId == SalesQuotations.ItemId
                                && a.ItemDetailsId == SalesQuotations.ItemDetailsId).Select(a => a.SalesQuotationDetailsId).FirstOrDefault();

                            if (SalesQuotations.SalesQuotationDetailsId == 0)
                            {
                                decimal convertedStock = 0, freeConvertedStock = 0;
                                if (SalesQuotations.ProductType != "Combo")
                                {
                                    convertedStock = oCommonController.StockConversion(SalesQuotations.Quantity, SalesQuotations.ItemId, SalesQuotations.PriceAddedFor);
                                    freeConvertedStock = oCommonController.StockConversion(SalesQuotations.FreeQuantity, SalesQuotations.ItemId, SalesQuotations.PriceAddedFor);
                                    bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.IsManageStock).FirstOrDefault();

                                    if (SalesQuotations.LotType == "stocktransfer")
                                    {
                                        SalesQuotations.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesQuotations.LotId).Select(a => a.LotId).FirstOrDefault();
                                        SalesQuotations.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesQuotations.LotId).Select(a => a.LotType).FirstOrDefault();
                                    }
                                    else
                                    {
                                        SalesQuotations.LotIdForLotNoChecking = SalesQuotations.LotId;
                                        SalesQuotations.LotTypeForLotNoChecking = SalesQuotations.LotType;
                                    }
                                }

                                SalesQuotations.ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                                ClsSalesQuotationDetails oClsSalesQuotationDetails = new ClsSalesQuotationDetails()
                                {
                                    DiscountType = SalesQuotations.DiscountType,
                                    OtherInfo = SalesQuotations.OtherInfo,
                                    PriceIncTax = SalesQuotations.PriceIncTax,
                                    ItemId = SalesQuotations.ItemId,
                                    ItemDetailsId = SalesQuotations.ItemDetailsId,
                                    SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
                                    TaxId = SalesQuotations.TaxId,
                                    Discount = SalesQuotations.Discount,
                                    Quantity = SalesQuotations.Quantity,
                                    UnitCost = SalesQuotations.UnitCost,
                                    IsActive = SalesQuotations.IsActive,
                                    IsDeleted = SalesQuotations.IsDeleted,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    //StockDeductionIds = SalesQuotations.StockDeductionIds,
                                    QuantityRemaining = SalesQuotations.IsComboItems == true ? (SalesQuotations.Quantity + SalesQuotations.FreeQuantity) : (convertedStock + freeConvertedStock),
                                    WarrantyId = SalesQuotations.WarrantyId,
                                    DefaultUnitCost = DefaultUnitCost,
                                    DefaultAmount = SalesQuotations.Quantity * DefaultUnitCost,
                                    PriceAddedFor = SalesQuotations.PriceAddedFor,
                                    LotId = SalesQuotations.LotId,
                                    LotType = SalesQuotations.LotType,
                                    FreeQuantity = SalesQuotations.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = SalesQuotations.FreeQuantityPriceAddedFor,
                                    AmountExcTax = SalesQuotations.AmountExcTax,
                                    TaxAmount = SalesQuotations.TaxAmount,
                                    PriceExcTax = SalesQuotations.PriceExcTax,
                                    AmountIncTax = SalesQuotations.AmountIncTax,
                                    UnitAddedFor = SalesQuotations.UnitAddedFor,
                                    LotIdForLotNoChecking = SalesQuotations.LotIdForLotNoChecking,
                                    LotTypeForLotNoChecking = SalesQuotations.LotTypeForLotNoChecking,
                                    ComboId = SalesQuotations.ComboId,
                                    IsComboItems = SalesQuotations.IsComboItems,
                                    QuantitySold = convertedStock + freeConvertedStock,
                                    ComboPerUnitQuantity = SalesQuotations.ComboPerUnitQuantity,
                                    WarrantyExpiryDate = WarrantyExpiryDate,
                                    ExtraDiscount = SalesQuotations.ExtraDiscount,
                                    TaxExemptionId = SalesQuotations.TaxExemptionId,
                                    ItemCodeId = SalesQuotations.ItemCodeId,
                                    TotalTaxAmount = SalesQuotations.TotalTaxAmount,
                                };

                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsSalesQuotationDetails.Add(oClsSalesQuotationDetails);
                                oConnectionContext.SaveChanges();
                            }
                            else
                            {
                                decimal QuantityReturned = 0;
                                decimal convertedStock = 0, freeConvertedStock = 0;

                                if (SalesQuotations.ProductType != "Combo")
                                {
                                    convertedStock = oCommonController.StockConversion(SalesQuotations.Quantity, SalesQuotations.ItemId, SalesQuotations.PriceAddedFor);
                                    freeConvertedStock = oCommonController.StockConversion(SalesQuotations.FreeQuantity, SalesQuotations.ItemId, SalesQuotations.PriceAddedFor);
                                    bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                }
                                else
                                {
                                    //QuantityReturned = (from a in oConnectionContext.DbClsSalesQuotationReturn
                                    //                    join b in oConnectionContext.DbClsSalesQuotationReturnDetails
                                    //                       on a.SalesQuotationsReturnId equals b.SalesQuotationsReturnId
                                    //                    where a.SalesQuotationId == obj.SalesQuotationId && b.ItemId == SalesQuotations.ItemId &&
                                    //                    b.ItemDetailsId == SalesQuotations.ItemDetailsId
                                    //                    select b.Quantity).FirstOrDefault();
                                }

                                if (SalesQuotations.LotType == "stocktransfer")
                                {
                                    SalesQuotations.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesQuotations.LotId).Select(a => a.LotId).FirstOrDefault();
                                    SalesQuotations.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesQuotations.LotId).Select(a => a.LotType).FirstOrDefault();
                                }
                                else
                                {
                                    SalesQuotations.LotIdForLotNoChecking = SalesQuotations.LotId;
                                    SalesQuotations.LotTypeForLotNoChecking = SalesQuotations.LotType;
                                }

                                SalesQuotations.ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesQuotations.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                                ClsSalesQuotationDetails oClsSalesQuotationDetails = new ClsSalesQuotationDetails()
                                {
                                    SalesQuotationDetailsId = SalesQuotations.SalesQuotationDetailsId,
                                    DiscountType = SalesQuotations.DiscountType,
                                    OtherInfo = SalesQuotations.OtherInfo,
                                    PriceIncTax = SalesQuotations.PriceIncTax,
                                    ItemId = SalesQuotations.ItemId,
                                    ItemDetailsId = SalesQuotations.ItemDetailsId,
                                    SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
                                    TaxId = SalesQuotations.TaxId,
                                    Discount = SalesQuotations.Discount,
                                    Quantity = SalesQuotations.Quantity,
                                    UnitCost = SalesQuotations.UnitCost,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    //StockDeductionIds = SalesQuotations.StockDeductionIds,
                                    QuantityRemaining = SalesQuotations.ProductType == "Combo" ? (SalesQuotations.Quantity + SalesQuotations.FreeQuantity) - QuantityReturned : (convertedStock + freeConvertedStock) - QuantityReturned,
                                    //QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityReturned,
                                    WarrantyId = SalesQuotations.WarrantyId,
                                    DefaultUnitCost = DefaultUnitCost,
                                    DefaultAmount = SalesQuotations.Quantity * DefaultUnitCost,
                                    PriceAddedFor = SalesQuotations.PriceAddedFor,
                                    LotId = SalesQuotations.LotId,
                                    LotType = SalesQuotations.LotType,
                                    FreeQuantity = SalesQuotations.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = SalesQuotations.FreeQuantityPriceAddedFor,
                                    AmountExcTax = SalesQuotations.AmountExcTax,
                                    TaxAmount = SalesQuotations.TaxAmount,
                                    PriceExcTax = SalesQuotations.PriceExcTax,
                                    AmountIncTax = SalesQuotations.AmountIncTax,
                                    UnitAddedFor = SalesQuotations.UnitAddedFor,
                                    LotIdForLotNoChecking = SalesQuotations.LotIdForLotNoChecking,
                                    LotTypeForLotNoChecking = SalesQuotations.LotTypeForLotNoChecking,
                                    ComboId = SalesQuotations.ComboId,
                                    IsComboItems = SalesQuotations.IsComboItems,
                                    QuantitySold = convertedStock + freeConvertedStock,
                                    ComboPerUnitQuantity = SalesQuotations.ComboPerUnitQuantity,
                                    WarrantyExpiryDate = WarrantyExpiryDate,
                                    ExtraDiscount = SalesQuotations.ExtraDiscount,
                                    TaxExemptionId = SalesQuotations.TaxExemptionId,
                                    ItemCodeId = SalesQuotations.ItemCodeId,
                                    TotalTaxAmount = SalesQuotations.TotalTaxAmount,
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsSalesQuotationDetails.Attach(oClsSalesQuotationDetails);
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.DiscountType).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.OtherInfo).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.PriceIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ItemId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ItemDetailsId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.SalesQuotationId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.TaxId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.Discount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.Quantity).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.UnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ModifiedOn).IsModified = true;
                                //oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.StockDeductionIds).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.QuantityRemaining).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.WarrantyId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.DefaultUnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.DefaultAmount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.PriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.LotId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.LotType).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.FreeQuantity).IsModified = true;
                                //oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.AmountExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.TaxAmount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.PriceExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.AmountIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.UnitAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ComboId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.IsComboItems).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.QuantitySold).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ComboPerUnitQuantity).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.WarrantyExpiryDate).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ExtraDiscount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.TaxExemptionId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.ItemCodeId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesQuotationDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Quotation \"" + obj.InvoiceNo + "\" updated",
                    Id = oClsSalesQuotation.SalesQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Quotation", obj.CompanyId, oClsSalesQuotation.SalesQuotationId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Quotation updated successfully",
                    Data = new
                    {
                        SalesQuotation = new
                        {
                            SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
                            InvoiceId = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == oClsSalesQuotation.SalesQuotationId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceQuotation }).FirstOrDefault(),
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesQuotationDetailsDelete(ClsSalesQuotationDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.SalesQuotationId != 0)
                {
                    string query = "update \"tblSalesQuotationsDetails\" set \"IsDeleted\"=True where \"SalesQuotationId\"=" + obj.SalesQuotationId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblSalesQuotationsDetails\" set \"IsDeleted\"=True where \"SalesQuotationDetailsId\"=" + obj.SalesQuotationDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
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
        public async Task<IHttpActionResult> Invoice(ClsSalesQuotationVm obj)
        {
            var det = oConnectionContext.DbClsSalesQuotation.Where(a => a.IsDeleted == false && a.InvoiceId == obj.InvoiceId).Select(a => new
            {
                a.Terms,
                a.OnlinePaymentSettingsId,
                a.SalesQuotationId,
                User = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => new
                {
                    c.Name,
                    c.MobileNo,
                    c.EmailId,
                    TaxNo = c.BusinessRegistrationNo,
                    Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == a.CustomerId).Select(b => new
                    {
                        b.MobileNo,
                        b.Name,
                        b.EmailId,
                        b.Address,
                        City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                        State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                        Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                        b.Zipcode
                    }).ToList(),
                }).FirstOrDefault(),
                IsShippingAddressDifferent = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.IsShippingAddressDifferent).FirstOrDefault(),
                Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => new
                {
                    b.Branch,
                    Mobile = b.Mobile,
                    b.Email,
                    //b.TaxNo,
                    //Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == b.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                    TaxNo = oConnectionContext.DbClsTaxSetting.Where(c => c.IsDeleted == false
                   && c.TaxSettingId == b.TaxSettingId).Select(c => c.BusinessRegistrationNo).FirstOrDefault(),
                    Tax = oConnectionContext.DbClsBusinessRegistrationName.Where(d => d.BusinessRegistrationNameId ==
                    oConnectionContext.DbClsTaxSetting.Where(c => c.IsDeleted == false
                    && c.TaxSettingId == b.TaxSettingId).Select(c => c.BusinessRegistrationNameId).FirstOrDefault()).Select(d => d.Name).FirstOrDefault(),
                    b.Address,
                    b.AltMobileNo,
                    City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                    State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                    Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                    b.Zipcode
                }).FirstOrDefault(),
                a.Notes,
                a.InvoiceNo,
                a.SalesQuotationDate,
                a.Subtotal,
                a.Discount,
                a.DiscountType,
                a.TotalDiscount,
                a.GrandTotal,
                a.Status,
                a.TaxAmount,
                //PaymentStatus = a.PaymentStatus,
                a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                a.NetAmount,
                Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.Tax).FirstOrDefault(),
                a.TotalQuantity,
                a.CompanyId,
                //a.SalesQuotationType,
                SalesQuotationDetails = (from b in oConnectionContext.DbClsSalesQuotationDetails
                                         join c in oConnectionContext.DbClsItemDetails
                                         on b.ItemDetailsId equals c.ItemDetailsId
                                         join d in oConnectionContext.DbClsItem
                                         on c.ItemId equals d.ItemId
                                         where b.SalesQuotationId == a.SalesQuotationId && b.IsDeleted == false
                                         && b.IsComboItems == false
                                         select new
                                         {
                                             d.ProductImage,
                                             b.TotalTaxAmount,
                                             b.PriceExcTax,
                                             Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                             : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                             b.DiscountType,
                                             b.SalesQuotationDetailsId,
                                             b.PriceIncTax,
                                             b.OtherInfo,
                                             b.AmountIncTax,
                                             b.Discount,
                                             b.SalesQuotationId,
                                             b.FreeQuantity,
                                             b.Quantity,
                                             b.TaxId,
                                             b.UnitCost,
                                             d.ItemId,
                                             d.ProductType,
                                             c.ItemDetailsId,
                                             d.ItemName,
                                             SKU = c.SKU == null ? d.SkuCode : c.SKU,
                                             c.VariationDetailsId,
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                             c.SalesExcTax,
                                             c.SalesIncTax,
                                             c.TotalCost,
                                             Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                             TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                             d.TaxType,
                                             ItemCode = oConnectionContext.DbClsItemCode.Where(e => e.ItemCodeId == d.ItemCodeId && e.IsDeleted == false).Select(e => e.Code).FirstOrDefault(),
                                             ComboItems = (from bb in oConnectionContext.DbClsSalesQuotationDetails
                                                           join cc in oConnectionContext.DbClsItemDetails
                                                           on bb.ItemDetailsId equals cc.ItemDetailsId
                                                           join dd in oConnectionContext.DbClsItem
                                                           on cc.ItemId equals dd.ItemId
                                                           where bb.SalesQuotationId == a.SalesQuotationId && bb.ComboId == b.ComboId && bb.IsDeleted == false
                                                           && bb.IsComboItems == true
                                                           select new
                                                           {
                                                               bb.Quantity,
                                                               SKU = cc.SKU == null ? dd.SkuCode : cc.SKU,
                                                               Unit = bb.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == dd.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                                  : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == dd.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                                  : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == dd.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                                  : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == dd.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                                               dd.ItemName,
                                                               VariationName = oConnectionContext.DbClsVariationDetails.Where(ccc => ccc.VariationDetailsId == cc.VariationDetailsId).Select(ccc => ccc.VariationDetails).FirstOrDefault(),
                                                           })
                                         }).ToList(),
                SalesQuotationAdditionalCharges = oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(b => b.SalesQuotationId == a.SalesQuotationId
&& b.IsDeleted == false && b.IsActive == true).Select(b => new ClsSalesQuotationAdditionalChargesVm
{
SalesQuotationAdditionalChargesId = b.SalesQuotationAdditionalChargesId,
Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
AdditionalChargeId = b.AdditionalChargeId,
SalesQuotationId = b.SalesQuotationId,
TaxId = b.TaxId,
AmountExcTax = b.AmountExcTax,
AmountIncTax = b.AmountIncTax,
TaxAmount = b.AmountIncTax - b.AmountExcTax,
AccountId = b.AccountId,
ItemCodeId = b.ItemCodeId,
TaxExemptionId = b.TaxExemptionId,
TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
}).ToList(),
            }).FirstOrDefault();

            var AllTaxs = oConnectionContext.DbClsSalesQuotation.Where(a => a.IsDeleted == false && a.SalesQuotationId == det.SalesQuotationId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsSalesQuotationDetails.Where(a => a.SalesQuotationId == det.SalesQuotationId && a.IsDeleted == false
                                && a.IsComboItems == false).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).Concat(oConnectionContext.DbClsSalesQuotationAdditionalCharges.Where(a => a.SalesQuotationId == det.SalesQuotationId
                                && a.IsDeleted == false && a.AmountExcTax>0).Select(a => new
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

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                a.BusinessLogo,
                a.BusinessName,
                a.DateFormat,
                a.TimeFormat,
                a.CurrencySymbolPlacement,
                a.CountryId,
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesQuotation = det,
                    Taxs = finalTaxs,
                    BusinessSetting = BusinessSetting,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesQuotationStatus(ClsSalesQuotationVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Status == "" || obj.Status == null)
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

                ClsSalesQuotation oClsSalesQuotation = new ClsSalesQuotation()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    SalesQuotationId = obj.SalesQuotationId,
                    Status = obj.Status,
                    PreviousStatus = obj.Status
                };

                oConnectionContext.DbClsSalesQuotation.Attach(oClsSalesQuotation);
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.SalesQuotationId).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSalesQuotation).Property(x => x.PreviousStatus).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Quotation \"" + oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == obj.SalesQuotationId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" status changed to \"" + obj.Status + "\"",
                    Id = oClsSalesQuotation.SalesQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Quotation", obj.CompanyId, oClsSalesQuotation.SalesQuotationId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Quotation status changed successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        SalesQuotation = new
                        {
                            SalesQuotationId = oClsSalesQuotation.SalesQuotationId,
                            InvoiceId = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId == oClsSalesQuotation.SalesQuotationId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceQuotation }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
