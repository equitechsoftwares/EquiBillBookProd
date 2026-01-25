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
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class SalesReturnController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllSalesReturn(ClsSalesVm obj)
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

            List<ClsSalesReturnVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsSalesReturn
                           //                       join b in oConnectionContext.DbClsSales
                           //on a.SalesId equals b.SalesId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
 //&& b.BranchId == obj.BranchId
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                       select new ClsSalesReturnVm
                       {
                           SalesCreditNoteReasonId = a.SalesCreditNoteReasonId,
                           IsCancelled = a.IsCancelled,
                           CustomerPaymentId = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false && b.IsCancelled == false
                           && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.CustomerPaymentId).FirstOrDefault(),
                           AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false && b.IsCancelled == false
                           && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.AmountRemaining).FirstOrDefault(),
                           TotalTaxAmount = a.TotalTaxAmount,
                           InvoiceId = a.InvoiceId,
                           BranchId = a.BranchId,
                           InvoiceUrl = oCommonController.webUrl,//+ "/sales/SalesReturnInvoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                           SalesType = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.SalesType).FirstOrDefault(),
                           SalesId = a.SalesId,
                           Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                           SalesReturnId = a.SalesReturnId,
                           GrandTotal = a.GrandTotal,
                           Notes = a.Notes,
                           Date = a.Date,
                           SalesInvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.InvoiceNo).FirstOrDefault(),
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
                           //        Paid = oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Count() == 0 ? 0 :
                           //oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Select(b => b.Amount).Sum(),
                           Status = a.Status,
                           //        Due = oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           TotalQuantity = a.TotalQuantity,
                           PaidQuantity = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == a.SalesReturnId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                           FreeQuantity = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == a.SalesReturnId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                           TotalItems = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == a.SalesReturnId &&
                    c.IsDeleted == false && c.IsComboItems == false).Count()
                       }).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsSalesReturn
                           //                       join b in oConnectionContext.DbClsSales
                           //on a.SalesId equals b.SalesId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
 && a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                       select new ClsSalesReturnVm
                       {
                           SalesCreditNoteReasonId = a.SalesCreditNoteReasonId,
                           IsCancelled = a.IsCancelled,
                           CustomerPaymentId = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false && b.IsCancelled == false
&& b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.CustomerPaymentId).FirstOrDefault(),
                           AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false && b.IsCancelled == false
                           && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.AmountRemaining).FirstOrDefault(),
                           TotalTaxAmount = a.TotalTaxAmount,
                           InvoiceId = a.InvoiceId,
                           BranchId = a.BranchId,
                           InvoiceUrl = oCommonController.webUrl,//+ "/sales/SalesReturnInvoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                           SalesType = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.SalesType).FirstOrDefault(),
                           SalesId = a.SalesId,
                           Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                           SalesReturnId = a.SalesReturnId,
                           GrandTotal = a.GrandTotal,
                           Notes = a.Notes,
                           Date = a.Date,
                           SalesInvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.InvoiceNo).FirstOrDefault(),
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
                           //        Paid = oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Count() == 0 ? 0 :
                           //oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Select(b => b.Amount).Sum(),
                           Status = a.Status,
                           //        Due = oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           TotalQuantity = a.TotalQuantity,
                           PaidQuantity = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == a.SalesReturnId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                           FreeQuantity = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == a.SalesReturnId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                           TotalItems = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == a.SalesReturnId &&
                    c.IsDeleted == false).Count()
                       }).ToList();
            }

            if (obj.InvoiceNo != null && obj.InvoiceNo != "")
            {
                det = det.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower()).Select(a => a).ToList();
            }

            if (obj.Status != "" && obj.Status != null)
            {
                det = det.Where(a => a.Status == obj.Status).Select(a => a).ToList();
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
                    SalesReturns = det.OrderByDescending(a => a.SalesReturnId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> SaleReturn(ClsSalesReturn obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();

            //obj.SalesReturnId = oConnectionContext.DbClsSalesReturn.Where(a => a.SalesId == obj.SalesId && a.IsDeleted == false && a.IsCancelled == false).Select(a => a.SalesReturnId).FirstOrDefault();

            if (obj.SalesReturnId == 0)
            {
                var det = (from bb in oConnectionContext.DbClsSales
                           where bb.SalesId == obj.SalesId && bb.CompanyId == obj.CompanyId
                           select new
                           {
                               bb.Terms,
                               GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == bb.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                               PayTaxForExport = bb.PayTaxForExport,
                               TaxCollectedFromCustomer = bb.TaxCollectedFromCustomer,
                               IsReverseCharge = bb.IsReverseCharge,
                               TaxableAmount = bb.TaxableAmount,
                               NetAmountReverseCharge = bb.NetAmountReverseCharge,
                               RoundOffReverseCharge = bb.RoundOffReverseCharge,
                               GrandTotalReverseCharge = bb.GrandTotalReverseCharge,
                               bb.Discount,
                               bb.DiscountType,
                               bb.TotalDiscount,
                               bb.SalesId,
                               IsCancelled = bb.IsCancelled,
                               BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == bb.BranchId).Select(b => b.StateId).FirstOrDefault(),
                               bb.PlaceOfSupplyId,
                               bb.TotalTaxAmount,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == bb.CustomerId).Select(e => e.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                               bb.SalesDate,
                               SalesInvoiceNo = bb.InvoiceNo,
                               bb.CustomerId,
                               bb.BranchId,
                               Branch = oConnectionContext.DbClsBranch.Where(z => z.BranchId == bb.BranchId).Select(z => z.Branch).FirstOrDefault(),
                               CustomerName = oConnectionContext.DbClsUser.Where(z => z.UserId == bb.CustomerId).Select(z => z.Name).FirstOrDefault(),
                               SalesReturnId = 0,
                               bb.SmsSettingsId,
                               bb.EmailSettingsId,
                               bb.WhatsappSettingsId,
                               bb.PointsEarned,
                               bb.RedeemPoints,
                               bb.PointsDiscount,
                               bb.GrandTotal,
                               DueDate = CurrentDate,
                               PaymentTermId = oConnectionContext.DbClsPaymentTerm.Where(z => z.CompanyId == obj.CompanyId && z.IsDueUponReceipt == true).Select(z => z.PaymentTermId).FirstOrDefault(),
                               SalesDetails = (from b in oConnectionContext.DbClsSalesDetails
                                               join c in oConnectionContext.DbClsItemDetails
                                               on b.ItemDetailsId equals c.ItemDetailsId
                                               join d in oConnectionContext.DbClsItem
                                               on c.ItemId equals d.ItemId
                                               where b.SalesId == obj.SalesId && b.IsDeleted == false
                                               && b.IsComboItems == false
                                               select new
                                               {
                                                   b.SalesDetailsId,
                                                   b.ExtraDiscount,
                                                   b.PriceExcTax,
                                                   AmountExcTax = 0,
                                                   //d.IsManageStock,
                                                   IsManageStock = true,
                                                   FreeQuantity = 0,
                                                   PurchaseReturnUnitCost = b.UnitCost,
                                                   b.QuantityRemaining,
                                                   SalesReturnUnitCost = b.PriceIncTax,
                                                   SalesReturnAmount = 0,
                                                   b.DiscountType,
                                                   b.PriceIncTax,
                                                   b.OtherInfo,
                                                   AmountIncTax = 0,
                                                   b.Discount,
                                                   Quantity = 0,
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
                                                   d.ItemCode,
                                                   UnitId = d.UnitId,
                                                   SecondaryUnitId = d.SecondaryUnitId,
                                                   TertiaryUnitId = d.TertiaryUnitId,
                                                   QuaternaryUnitId = d.QuaternaryUnitId,
                                                   UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                                                   SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                                                   TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                                                   QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                                                   UToSValue = d.UToSValue,
                                                   SToTValue = d.SToTValue,
                                                   TToQValue = d.TToQValue,
                                                   AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                                                   SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                                   TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                                   QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                                   PriceAddedFor = b.PriceAddedFor,
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
                                               }).ToList(),
                            SalesReturnAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                                ).Select(b => new ClsSalesReturnAdditionalChargesVm
                                {
                                    SalesReturnAdditionalChargesId =0,
                                    Name = b.Name,
                                    AdditionalChargeId = b.AdditionalChargeId,
                                    SalesReturnId = 0,
                                    TaxId = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == bb.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                                    AmountExcTax = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == bb.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                                    AmountIncTax = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == bb.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                                    TaxAmount = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == bb.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                                    AccountId = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == bb.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                                    ItemCodeId = b.ItemCodeId,
                                    TaxExemptionId = b.TaxExemptionId,
                                    TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                                }).ToList(),
                           }).FirstOrDefault();

                var AllTaxs = oConnectionContext.DbClsSales.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.SalesId == det.SalesId).Select(a => new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    a.TaxId,
                    AmountExcTax = a.Subtotal - a.TotalDiscount
                }).Concat(oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == det.SalesId && a.IsDeleted == false
                                    && a.IsComboItems == false).Select(a => new
                                    {
                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                        a.TaxId,
                                        AmountExcTax = a.AmountExcTax
                                    }
                                )).Concat(oConnectionContext.DbClsSalesAdditionalCharges.Where(a => a.SalesId == det.SalesId
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
                        SalesReturn = det,
                        Taxs = finalTaxs,
                    }
                };
            }
            else
            {
                var det = (from a in oConnectionContext.DbClsSalesReturn
                               //                       join b in oConnectionContext.DbClsSales
                               //on a.SalesId equals b.SalesId
                           where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
                           select new
                           {
                               a.Terms,
                               GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                               PayTaxForExport = a.PayTaxForExport,
                               TaxCollectedFromCustomer = a.TaxCollectedFromCustomer,
                               IsReverseCharge = a.IsReverseCharge,
                               TaxableAmount = a.TaxableAmount,
                               NetAmountReverseCharge = a.NetAmountReverseCharge,
                               RoundOffReverseCharge = a.RoundOffReverseCharge,
                               GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                               SalesCreditNoteReasonId = a.SalesCreditNoteReasonId,
                               IsCancelled = a.IsCancelled,
                               BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                               a.PlaceOfSupplyId,
                               Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == a.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                               a.TotalTaxAmount,
                               SalesType = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.SalesType).FirstOrDefault(),
                               a.InvoiceId,
                               a.RoundOff,
                               SpecialDiscount = a.SpecialDiscount,
                               a.NetAmount,
                               a.TaxId,
                               a.TaxAmount,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(bb => bb.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(bb => bb.CurrencySymbol).FirstOrDefault(),
                               a.Date,
                               a.SalesReturnId,
                               SalesInvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.InvoiceNo).FirstOrDefault(),
                               a.CustomerId,
                               a.BranchId,
                               Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == a.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                               CustomerName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.CustomerId).Select(bb => bb.Name).FirstOrDefault(),
                               a.Status,
                               //a.PayTerm,
                               //a.PayTermNo,
                               PaymentTermId = a.PaymentTermId,
                               DueDate = a.DueDate,
                               a.AttachDocument,
                               SalesId = a.SalesId,
                               PointsEarned = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.PointsEarned).FirstOrDefault(),
                               RedeemPoints = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.RedeemPoints).FirstOrDefault(),
                               OriginalSaleGrandTotal = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.GrandTotal).FirstOrDefault(),
                               a.GrandTotal,
                               a.TotalDiscount,
                               a.TotalQuantity,
                               a.Discount,
                               a.DiscountType,
                               a.Notes,
                               SalesDate = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.SalesDate).FirstOrDefault(),
                               a.InvoiceNo,
                               a.Subtotal,
                               CompanyId = a.CompanyId,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               a.SmsSettingsId,
                               a.EmailSettingsId,
                               a.WhatsappSettingsId,
                               SalesDetails = (from b in oConnectionContext.DbClsSalesReturnDetails
                                                   //join q in oConnectionContext.DbClsSalesDetails
                                                   //on b.SalesDetailsId equals q.SalesDetailsId
                                               join c in oConnectionContext.DbClsItemDetails
                                               on b.ItemDetailsId equals c.ItemDetailsId
                                               join d in oConnectionContext.DbClsItem
                                               on c.ItemId equals d.ItemId
                                               where b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false
                                               && b.IsComboItems == false //&& z.SalesReturnId == a.SalesReturnId
                                               select new
                                               {
                                                   b.TotalTaxAmount,
                                                   b.ExtraDiscount,
                                                   b.PriceExcTax,
                                                   b.AmountExcTax,
                                                   //d.IsManageStock,
                                                   IsManageStock = true,
                                                   b.FreeQuantity,
                                                   b.PriceIncTax,
                                                   QuantityRemaining = a.Status.ToLower() != "draft" ?
                                                   b.QuantityRemaining + oConnectionContext.DbClsSalesDetails.Where(f => f.SalesDetailsId ==
                                                   b.SalesDetailsId && f.IsComboItems == false).Select(f => f.QuantityRemaining).FirstOrDefault() :
                                                   oConnectionContext.DbClsSalesDetails.Where(f => f.SalesDetailsId ==
                                                   b.SalesDetailsId).Select(f => f.QuantityRemaining).FirstOrDefault(),
                                                   SalesReturnDetailsId = b.SalesReturnDetailsId,
                                                   b.DiscountType,
                                                   b.SalesDetailsId,
                                                   //b.OtherInfo,
                                                   b.AmountIncTax,
                                                   b.Discount,
                                                   Quantity = b.Quantity,
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
                                                   d.ItemCode,
                                                   UnitId = d.UnitId,
                                                   SecondaryUnitId = d.SecondaryUnitId,
                                                   TertiaryUnitId = d.TertiaryUnitId,
                                                   QuaternaryUnitId = d.QuaternaryUnitId,
                                                   UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                                                   SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                                                   TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                                                   QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                                                   UToSValue = d.UToSValue,
                                                   SToTValue = d.SToTValue,
                                                   TToQValue = d.TToQValue,
                                                   AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                                                   SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                                   TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                                   QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                                   PriceAddedFor = b.PriceAddedFor,
                                                   LotNo = oConnectionContext.DbClsSalesDetails.Where(f => f.SalesDetailsId ==
                                                   b.SalesDetailsId).Select(f => f.LotTypeForLotNoChecking).FirstOrDefault() == "purchase" ?
                                             oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == oConnectionContext.DbClsSalesDetails.Where(ff => ff.SalesDetailsId ==
                                                   b.SalesDetailsId).Select(ff => ff.LotIdForLotNoChecking).FirstOrDefault()).Select(f => f.LotNo + (EnableItemExpiry == true ? "- Exp Date: " + f.ExpiryDate : "")).FirstOrDefault()
                                             : oConnectionContext.DbClsSalesDetails.Where(f => f.SalesDetailsId ==
                                                   b.SalesDetailsId).Select(f => f.LotTypeForLotNoChecking).FirstOrDefault() == "openingstock" ?
                                                   oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == oConnectionContext.DbClsSalesDetails.Where(ff => ff.SalesDetailsId ==
                                                   b.SalesDetailsId).Select(ff => ff.LotIdForLotNoChecking).FirstOrDefault()).Select(f => f.LotNo + (EnableItemExpiry == true ? "- Exp Date: " + f.ExpiryDate : "")).FirstOrDefault()
                                             : "Default Stock Accounting Method",
                                                   LotId = oConnectionContext.DbClsSalesDetails.Where(f => f.SalesDetailsId == b.SalesDetailsId).Select(f => f.LotId).FirstOrDefault(),
                                                   LotType = oConnectionContext.DbClsSalesDetails.Where(f => f.SalesDetailsId == b.SalesDetailsId).Select(f => f.LotType).FirstOrDefault(),
                                               }).ToList(),
                               SalesReturnAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                                ).Select(b => new ClsSalesReturnAdditionalChargesVm
                                {
                                    SalesReturnAdditionalChargesId = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(c => c.SalesReturnId == a.SalesReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c=>c.SalesReturnAdditionalChargesId).FirstOrDefault(),
                                    Name = b.Name,
                                    AdditionalChargeId = b.AdditionalChargeId,
                                    SalesReturnId = a.SalesReturnId,
                                    TaxId = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(c => c.SalesReturnId == a.SalesReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                                    AmountExcTax = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(c => c.SalesReturnId == a.SalesReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                                    AmountIncTax = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(c => c.SalesReturnId == a.SalesReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                                    TaxAmount = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(c => c.SalesReturnId == a.SalesReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                                    AccountId = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(c => c.SalesReturnId == a.SalesReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                                    ItemCodeId = b.ItemCodeId,
                                    TaxExemptionId = b.TaxExemptionId,
                                    TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                                }).ToList(),
                           }).FirstOrDefault();

                var AllTaxs = oConnectionContext.DbClsSalesReturn.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.SalesReturnId == det.SalesReturnId).Select(a => new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    a.TaxId,
                    AmountExcTax = a.Subtotal - a.TotalDiscount
                }).Concat(oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == det.SalesReturnId && a.IsDeleted == false
                                    && a.IsComboItems == false).Select(a => new
                                    {
                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                        a.TaxId,
                                        AmountExcTax = a.AmountExcTax
                                    })).Concat(oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(a => a.SalesReturnId == det.SalesReturnId
                                && a.IsDeleted == false).Select(a => new
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
                        SalesReturn = det,
                        Taxs = finalTaxs,
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSalesReturn(ClsSalesReturnVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
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

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.SalesId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesInvoice" });
                    isError = true;
                }

                if (obj.SalesCreditNoteReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesCreditNoteReason" });
                    isError = true;
                }

                if (obj.InvoiceNo != "" && obj.InvoiceNo != null)
                {
                    if (oConnectionContext.DbClsSalesReturn.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Return# exists", Id = "divInvoiceNo" });
                        isError = true;
                    }
                }

                if (obj.SalesReturnDetails == null || obj.SalesReturnDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var item in obj.SalesReturnDetails)
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

                //if (obj.SalesId!= 0)
                //{
                //    obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId).Select(a => a.BranchId).FirstOrDefault();
                //}

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
                    
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "credit note"
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

                List<ClsSalesReturnDetailsVm> _SalesReturnDetails = new List<ClsSalesReturnDetailsVm>();
                if (obj.SalesReturnDetails != null)
                {
                    foreach (var Sales in obj.SalesReturnDetails)
                    {
                        //string _comboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                        //if (_comboId != "" && _comboId != null)
                        Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                        if (Sales.ProductType.ToLower() == "combo")
                        {
                            string _comboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                            Sales.ComboId = oCommonController.CreateToken();
                            var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == _comboId && a.IsComboItems == true).Select(a => new
                            {
                                ItemId = a.ItemId,
                                ItemDetailsId = a.ItemDetailsId,
                                ComboItemDetailsId = a.ItemDetailsId,
                                Quantity = a.ComboPerUnitQuantity,
                                SalesDetailsId = a.SalesDetailsId,
                            }).ToList();

                            foreach (var item in combo)
                            {
                                _SalesReturnDetails.Add(new ClsSalesReturnDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, IsComboItems = true, ComboId = Sales.ComboId, IsActive = true, PriceAddedFor = 4, UnitAddedFor = 1, SalesDetailsId = Sales.SalesDetailsId });
                            }
                            _SalesReturnDetails.Add(Sales);
                        }
                        else
                        {
                            _SalesReturnDetails.Add(Sales);
                        }
                    }
                }

                obj.SalesReturnDetails = _SalesReturnDetails;

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        if (SalesReturn.ProductType != "Combo")
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                decimal QuantityRemaining = 0;
                                if (SalesReturn.IsComboItems == true)
                                {
                                    QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == true).Select(a => a.QuantityRemaining).FirstOrDefault();
                                }
                                else
                                {
                                    QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == false).Select(a => a.QuantityRemaining).FirstOrDefault();
                                }

                                decimal convertedStock = oCommonController.StockConversion(SalesReturn.Quantity + SalesReturn.FreeQuantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);

                                if (convertedStock > QuantityRemaining)
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Only " + QuantityRemaining + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                        Data = new
                                        {
                                        }
                                    };
                                    return await Task.FromResult(Ok(data));
                                }
                            }
                        }
                    }
                }

                //obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                ClsSalesReturn oClsSalesReturn = new ClsSalesReturn()
                {
                    BranchId = obj.BranchId,
                    TotalTaxAmount = obj.TotalTaxAmount,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    SalesId = obj.SalesId,
                    Status = obj.Status,//"Due",
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
                    DueDate = obj.Date.AddHours(5).AddMinutes(30),//obj.DueDate,
                    SalesReturnId = obj.SalesReturnId,
                    InvoiceNo = obj.InvoiceNo,
                    Subtotal = obj.Subtotal,
                    TotalQuantity = obj.TotalQuantity,
                    //PaymentStatus = "Due",
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    InvoiceId = oCommonController.CreateToken(),//DateTime.Now.ToFileTime(),
                    TaxId = obj.TaxId,
                    TaxAmount = obj.TaxAmount,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    CustomerId = obj.CustomerId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    IsCancelled = false,
                    PrefixId = PrefixId,
                    SalesCreditNoteReasonId = obj.SalesCreditNoteReasonId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    IsReverseCharge = obj.IsReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId,
                    Terms = obj.Terms
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SalesReturn/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesReturn/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesReturn.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsSalesReturn.Add(oClsSalesReturn);
                oConnectionContext.SaveChanges();

                if (obj.SalesReturnAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesReturnAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

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

                        ClsSalesReturnAdditionalCharges oClsSalesReturnAdditionalCharges = new ClsSalesReturnAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            SalesReturnId = oClsSalesReturn.SalesReturnId,
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
                        oConnectionContext.DbClsSalesReturnAdditionalCharges.Add(oClsSalesReturnAdditionalCharges);
                        oConnectionContext.SaveChanges();

                        foreach (var taxJournal in taxList)
                        {
                            ClsSalesReturnAdditionalTaxJournal oClsSalesReturnAdditionalTaxJournal = new ClsSalesReturnAdditionalTaxJournal()
                            {
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                                SalesReturnAdditionalChargesId = oClsSalesReturnAdditionalCharges.SalesReturnAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                SalesReturnTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsSalesReturnAdditionalTaxJournal.Add(oClsSalesReturnAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        decimal convertedStock = 0, freeConvertedStock = 0;
                        string query = "", StockDeductionIds = "", _json = "";
                        List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();
                        bool IsManageStock = false;
                        if (SalesReturn.ProductType != "Combo")
                        {
                            IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                convertedStock = oCommonController.StockConversion(SalesReturn.Quantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);
                                freeConvertedStock = oCommonController.StockConversion(SalesReturn.FreeQuantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);

                                //StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                                //        Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId)
                                //        .Select(a => a.StockDeductionIds).FirstOrDefault();

                                decimal SalesDetailsId = 0;
                                if (SalesReturn.IsComboItems == true)
                                {
                                    SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                       Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                       && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == true)
                                       .Select(a => a.SalesDetailsId).FirstOrDefault();
                                }
                                else
                                {
                                    SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                       Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                       && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == false)
                                       .Select(a => a.SalesDetailsId).FirstOrDefault();
                                }

                                if (obj.Status.ToLower() != "draft")
                                {
                                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                           == SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                    decimal qtyRemaininToDeduct = convertedStock + freeConvertedStock;
                                    foreach (var res in _StockDeductionIds)
                                    {
                                        if (qtyRemaininToDeduct != 0)
                                        {
                                            decimal qty = 0;
                                            if (res.Quantity >= qtyRemaininToDeduct)
                                            {
                                                qty = qtyRemaininToDeduct;
                                            }
                                            else if (res.Quantity < qtyRemaininToDeduct)
                                            {
                                                qty = res.Quantity;
                                            }

                                            if (res.Type == "purchase")
                                            {
                                                query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"PurchaseDetailsId\"=" + res.Id;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                            else if (res.Type == "openingstock")
                                            {
                                                query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"OpeningStockId\"=" + res.Id;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                            else if (res.Type == "stocktransfer")
                                            {
                                                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }

                                            oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = res.Id, Type = res.Type, Quantity = qty });

                                            query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + qty + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                            oConnectionContext.Database.ExecuteSqlCommand(query);

                                            qtyRemaininToDeduct = qtyRemaininToDeduct - qty;
                                        }
                                    }
                                    ;

                                    //serializer.MaxJsonLength = 2147483644;
                                    //_json = serializer.Serialize(oClsStockDeductionIds);
                                }
                            }
                        }

                        long SalesAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == SalesReturn.ItemDetailsId).Select(a => a.SalesAccountId).FirstOrDefault();
                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == SalesReturn.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == SalesReturn.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        decimal AmountExcTax = SalesReturn.AmountExcTax;
                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == SalesReturn.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == SalesReturn.TaxId
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

                        SalesReturn.ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                        ClsSalesReturnDetails oClsSalesReturnDetails = new ClsSalesReturnDetails()
                        {
                            ItemId = SalesReturn.ItemId,
                            ItemDetailsId = SalesReturn.ItemDetailsId,
                            SalesReturnId = oClsSalesReturn.SalesReturnId,
                            Quantity = SalesReturn.Quantity,
                            IsActive = SalesReturn.IsActive,
                            IsDeleted = SalesReturn.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            //StockDeductionIds = _json,
                            PriceAddedFor = SalesReturn.PriceAddedFor,
                            QuantityRemaining = SalesReturn.ProductType == "Combo" ? SalesReturn.Quantity + SalesReturn.FreeQuantity : convertedStock + freeConvertedStock,
                            //QuantityRemaining = convertedStock + freeConvertedStock,
                            //Amount = SalesReturn.Amount,
                            FreeQuantity = SalesReturn.FreeQuantity,
                            SalesDetailsId = SalesReturn.SalesDetailsId,
                            UnitAddedFor = SalesReturn.UnitAddedFor,
                            QuantityReturned = SalesReturn.ProductType == "Combo" ? SalesReturn.Quantity + SalesReturn.FreeQuantity : convertedStock + freeConvertedStock,
                            TaxId = SalesReturn.TaxId,
                            DiscountType = SalesReturn.DiscountType,
                            Discount = SalesReturn.Discount,
                            UnitCost = SalesReturn.UnitCost,
                            AmountExcTax = SalesReturn.AmountExcTax,
                            TaxAmount = SalesReturn.TaxAmount,
                            AmountIncTax = SalesReturn.AmountIncTax,
                            PriceExcTax = SalesReturn.PriceExcTax,
                            PriceIncTax = SalesReturn.PriceIncTax,
                            ComboId = SalesReturn.ComboId,
                            IsComboItems = SalesReturn.IsComboItems,
                            AccountId = SalesAccountId,
                            DiscountAccountId = DiscountAccountId,
                            TaxAccountId = TaxAccountId,
                            ExtraDiscount = SalesReturn.ExtraDiscount,
                            ItemCodeId = SalesReturn.ItemCodeId,
                            TotalTaxAmount = SalesReturn.TotalTaxAmount,
                            IsCombo = SalesReturn.ProductType == "Combo" ? true : false,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsSalesReturnDetails.Add(oClsSalesReturnDetails);
                        oConnectionContext.SaveChanges();

                        //string ll = "delete from tblSalesReturnDeductionId where SalesReturnDetailsId=" + oClsSalesReturnDetails.SalesReturnDetailsId;
                        //oConnectionContext.Database.ExecuteSqlCommand(ll);

                        foreach (var l in oClsStockDeductionIds)
                        {
                            ClsSalesReturnDeductionId oClsSalesReturnDeductionId = new ClsSalesReturnDeductionId()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                Id = l.Id,
                                Type = l.Type,
                                Quantity = l.Quantity,
                                SalesReturnDetailsId = oClsSalesReturnDetails.SalesReturnDetailsId,
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                            };
                            oConnectionContext.DbClsSalesReturnDeductionId.Add(oClsSalesReturnDeductionId);
                            oConnectionContext.SaveChanges();
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsSalesReturnTaxJournal oClsSalesReturnTaxJournal = new ClsSalesReturnTaxJournal()
                            {
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                                SalesReturnDetailsId = oClsSalesReturnDetails.SalesReturnDetailsId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                SalesReturnTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsSalesReturnTaxJournal.Add(oClsSalesReturnTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        if (obj.Status.ToLower() != "draft")
                        {
                            if (SalesReturn.ProductType == "Combo")
                            {
                                query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (SalesReturn.Quantity + SalesReturn.FreeQuantity) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            else if (IsManageStock == true)
                            {
                                if (SalesReturn.IsComboItems == true)
                                {
                                    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else
                                {
                                    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                //    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                //oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }
                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                if (obj.Status.ToLower() != "draft")
                {
                    decimal SalesDue = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                    (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                    && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                    ClsCustomerPaymentVm oClsCustomerPayment = new ClsCustomerPaymentVm
                    {
                        SalesId = obj.SalesId,
                        SalesReturnId = oClsSalesReturn.SalesReturnId,
                        CompanyId = obj.CompanyId,
                        BranchId = obj.BranchId,
                        AddedBy = obj.AddedBy,
                        IsActive = true,
                        IsDeleted = false,
                        Amount = obj.GrandTotal,
                        PaymentDate = CurrentDate,
                        CustomerId = obj.CustomerId,
                        Type = "Customer Payment",
                        AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault(),
                        AmountExcTax = obj.GrandTotal,
                        TaxAccountId = 0,
                        TaxAmount = 0,
                        TaxId = 0,
                    };

                    if (SalesDue >= obj.GrandTotal)
                    {
                        oClsCustomerPayment.CustomerPaymentIds = new List<ClsCustomerPaymentIds> { new ClsCustomerPaymentIds()
                    {
                        Due = SalesDue,
                        Amount= obj.GrandTotal,
                        SalesId = obj.SalesId,
                        Type = oConnectionContext.DbClsSales.Where(a=>
                        a.CompanyId == obj.CompanyId && a.SalesId ==obj.SalesId && a.IsActive == true && a.IsDeleted==false).Select(a=>a.SalesType).FirstOrDefault() + " Payment"
                    }
                    };
                    }
                    else if (SalesDue < obj.GrandTotal)
                    {
                        oClsCustomerPayment.CustomerPaymentIds = new List<ClsCustomerPaymentIds> { new ClsCustomerPaymentIds()
                    {
                        Due = SalesDue,
                        Amount= SalesDue,
                        SalesId = obj.SalesId,
                        Type = oConnectionContext.DbClsSales.Where(a=>
                        a.CompanyId == obj.CompanyId && a.SalesId ==obj.SalesId && a.IsActive == true && a.IsDeleted==false).Select(a=>a.SalesType).FirstOrDefault() + " Payment"
                    }
                    };
                    }

                    oCommonController.CreditNoteCreditsApply(oClsCustomerPayment);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Return",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Return \"" + obj.InvoiceNo + "\" created",
                    Id = oClsSalesReturn.SalesReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Process Reward Points on Sales Return
                ProcessRewardPointsOnReturn(oClsSalesReturn.SalesReturnId, obj.SalesId, obj.CustomerId, obj.CompanyId, obj.GrandTotal, CurrentDate, obj.AddedBy);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Credit Note", obj.CompanyId, oClsSalesReturn.SalesReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Return created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            InvoiceId = oClsSalesReturn.InvoiceId,
                            SalesType = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.SalesType).FirstOrDefault()
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceSalesReturn }).FirstOrDefault(),
                        PosSetting = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceSalesReturn }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnDelete(ClsSalesReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.SalesId = oConnectionContext.DbClsSalesReturn.
                   Where(b => b.SalesReturnId == obj.SalesReturnId).Select(b => b.SalesId).FirstOrDefault();

                obj.BranchId = oConnectionContext.DbClsSales.
                   Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();

                List<ClsSalesReturnDetailsVm> details = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                && a.IsDeleted == false).Select(a => new ClsSalesReturnDetailsVm
                {
                    IsComboItems = a.IsComboItems,
                    ProductType = oConnectionContext.DbClsItem.Where(b => b.ItemId == a.ItemId).Select(b => b.ProductType).FirstOrDefault(),
                    SalesReturnDetailsId = a.SalesReturnDetailsId,
                    ItemDetailsId = a.ItemDetailsId,
                    ItemId = a.ItemId,
                    Quantity = a.Quantity,
                    //a.PriceAddedFor
                }).ToList();

                ClsSalesReturn oClsSalesReturn = new ClsSalesReturn()
                {
                    SalesReturnId = obj.SalesReturnId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesReturn.Attach(oClsSalesReturn);
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SalesReturnId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                //Release stock
                if (details != null)
                {
                    foreach (var SalesReturn in details)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesReturnDeductionId.Where(a => a.SalesReturnDetailsId
                    == SalesReturn.SalesReturnDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                            string ll = "delete from \"tblSalesReturnDeductionId\" where \"SalesReturnDetailsId\"=" + SalesReturn.SalesReturnDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(ll);

                            if (_StockDeductionIds != null)
                            {
                                foreach (var res in _StockDeductionIds)
                                {
                                    string query = "";
                                    if (res.Type == "purchase")
                                    {
                                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "openingstock")
                                    {
                                        query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "stocktransfer")
                                    {
                                        query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + res.Quantity + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);

                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + res.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + res.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                }
                            }
                        }
                        else if (SalesReturn.ProductType == "Combo")
                        {
                            string comboQuery = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + SalesReturn.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(comboQuery);
                        }
                    }
                }
                //Release stock

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Return",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Return \"" + oConnectionContext.DbClsSalesReturn.Where(a => a.SalesReturnId == obj.SalesReturnId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsSalesReturn.SalesReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Reverse reward points adjustments when return is deleted
                ReverseRewardPointsOnReturnDelete(obj.SalesReturnId, obj.CompanyId, CurrentDate, obj.AddedBy);

                data = new
                {
                    Status = 1,
                    Message = "Sales Return deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnCancel(ClsSalesReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.SalesId = oConnectionContext.DbClsSalesReturn.
                   Where(b => b.SalesReturnId == obj.SalesReturnId).Select(b => b.SalesId).FirstOrDefault();

                obj.BranchId = oConnectionContext.DbClsSales.
                   Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();

                List<ClsSalesReturnDetailsVm> details = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                && a.IsDeleted == false).Select(a => new ClsSalesReturnDetailsVm
                {
                    IsComboItems = a.IsComboItems,
                    ProductType = oConnectionContext.DbClsItem.Where(b => b.ItemId == a.ItemId).Select(b => b.ProductType).FirstOrDefault(),
                    SalesReturnDetailsId = a.SalesReturnDetailsId,
                    ItemDetailsId = a.ItemDetailsId,
                    ItemId = a.ItemId,
                    Quantity = a.Quantity,
                    //a.PriceAddedFor
                }).ToList();

                ClsSalesReturn oClsSalesReturn = new ClsSalesReturn()
                {
                    SalesReturnId = obj.SalesReturnId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesReturn.Attach(oClsSalesReturn);
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SalesReturnId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                //Release stock
                if (details != null)
                {
                    foreach (var SalesReturn in details)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesReturnDeductionId.Where(a => a.SalesReturnDetailsId
                    == SalesReturn.SalesReturnDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                            string ll = "delete from \"tblSalesReturnDeductionId\" where \"SalesReturnDetailsId\"=" + SalesReturn.SalesReturnDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(ll);

                            if (_StockDeductionIds != null)
                            {
                                foreach (var res in _StockDeductionIds)
                                {
                                    string query = "";
                                    if (res.Type == "purchase")
                                    {
                                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "openingstock")
                                    {
                                        query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "stocktransfer")
                                    {
                                        query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + res.Quantity + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);

                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + res.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + res.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                }
                            }
                        }
                        else if (SalesReturn.ProductType == "Combo")
                        {
                            string comboQuery = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + SalesReturn.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(comboQuery);
                        }
                    }
                }
                //Release stock

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Return",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Return \"" + oConnectionContext.DbClsSalesReturn.Where(a => a.SalesReturnId == obj.SalesReturnId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" updated",
                    Id = oClsSalesReturn.SalesReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Credit Note", obj.CompanyId, oClsSalesReturn.SalesReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Return cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesReturn(ClsSalesReturnVm obj)
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

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                //if (obj.Status == "" || obj.Status == null)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                //    isError = true;
                //}

                if (obj.SalesId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesInvoice" });
                    isError = true;
                }

                if (obj.SalesCreditNoteReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesCreditNoteReason" });
                    isError = true;
                }

                if (obj.SalesReturnDetails == null || obj.SalesReturnDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var item in obj.SalesReturnDetails)
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

                //  decimal CreditAmountUsed = oConnectionContext.DbClsCustomerPayment.Where(a => a.SalesReturnId == obj.SalesReturnId
                //&& a.CompanyId == obj.CompanyId).Select(a => a.AmountUsed).Sum();
                //  if (CreditAmountUsed < obj.GrandTotal)
                //  {
                //      data = new
                //      {
                //          Status = 0,
                //          Message = "Please make sure that the Credit Note amount is not lesser than " + CreditAmountUsed + " because as many credits have been either refunded or applied to bills",
                //          Errors = errors,
                //          Data = new
                //          {
                //          }
                //      };
                //      return await Task.FromResult(Ok(data));
                //  }

                List<ClsSalesReturnDetailsVm> _SalesReturnDetails = new List<ClsSalesReturnDetailsVm>();
                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        if (SalesReturn.SalesReturnDetailsId != 0)
                        {
                            SalesReturn.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesReturn.ProductType.ToLower() == "combo")
                            {
                                SalesReturn.ComboId = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnDetailsId == SalesReturn.SalesReturnDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.ComboId == SalesReturn.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.SalesReturnDetailsId
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesReturnDetails.Add(new ClsSalesReturnDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesReturn.Quantity, IsComboItems = true, ComboId = SalesReturn.ComboId, SalesReturnDetailsId = item.SalesReturnDetailsId, IsActive = true, PriceAddedFor = 4, UnitAddedFor = 4 });
                                }
                                _SalesReturnDetails.Add(SalesReturn);
                            }
                            else
                            {
                                _SalesReturnDetails.Add(SalesReturn);
                            }
                        }
                        else
                        {
                            _SalesReturnDetails.Add(SalesReturn);
                        }
                    }
                }

                obj.SalesReturnDetails = _SalesReturnDetails;

                //Release stock
                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            if (SalesReturn.SalesReturnDetailsId != 0)
                            {
                                //Release stock
                                //string StockDeductionIds = oConnectionContext.DbClsSalesReturnDetails.
                                //    Where(a => a.SalesReturnDetailsId == SalesReturn.SalesReturnDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                                //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesReturnDeductionId.Where(a => a.SalesReturnDetailsId
                        == SalesReturn.SalesReturnDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                foreach (var res in _StockDeductionIds)
                                {
                                    string query = "";

                                    //        decimal qty = oCommonController.StockConversion(res.Quantity, SalesReturn.ItemId, oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                                    //&& a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId).Select(a => a.PriceAddedFor).FirstOrDefault());

                                    if (res.Type == "purchase")
                                    {
                                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"+" + res.Quantity + " where \"PurchaseDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "openingstock")
                                    {
                                        query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"+" + res.Quantity + " where \"OpeningStockId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else if (res.Type == "stocktransfer")
                                    {
                                        query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + res.Quantity + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);

                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + res.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + res.Quantity + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                }
                            }
                        }
                        else if (SalesReturn.ProductType == "Combo")
                        {
                            decimal _comboQty = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId && a.IsCombo == true && a.IsDeleted == false).Select(a => a.Quantity).FirstOrDefault();
                            string comboQuery = "update \"tblSalesDetails\" set \"QuantityRemaining\"=(\"QuantityRemaining\"+" + _comboQty + ") where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(comboQuery);
                        }
                        if (SalesReturn.IsDeleted == true)
                        {
                            string query = "update \"tblSalesReturnDetails\" set \"IsDeleted\"=True where \"SalesReturnDetailsId\"=" + SalesReturn.SalesReturnDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                    }
                }
                //Release stock

                obj.SalesReturnDetails.RemoveAll(r => r.IsComboItems == true);
                obj.SalesReturnDetails.RemoveAll(r => r.IsDeleted == true);

                List<ClsSalesReturnDetailsVm> _SalesReturnDetails1 = new List<ClsSalesReturnDetailsVm>();
                if (obj.SalesReturnDetails != null)
                {
                    foreach (var Sales in obj.SalesReturnDetails)
                    {
                        if (Sales.SalesReturnDetailsId == 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                string _comboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                Sales.ComboId = oCommonController.CreateToken();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == _comboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.ComboPerUnitQuantity,
                                    SalesDetailsId = a.SalesDetailsId,
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesReturnDetails1.Add(new ClsSalesReturnDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, IsComboItems = true, ComboId = Sales.ComboId, IsActive = true, PriceAddedFor = 4, UnitAddedFor = 1, SalesDetailsId = Sales.SalesDetailsId });
                                }
                                _SalesReturnDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesReturnDetails1.Add(Sales);
                            }
                        }
                        else
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnDetailsId == Sales.SalesReturnDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                long SalesId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.SalesId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = oConnectionContext.DbClsSalesDetails.Where(b => b.SalesId == SalesId && b.ItemDetailsId == a.ItemDetailsId && b.IsComboItems == true).Select(b => b.ComboPerUnitQuantity).FirstOrDefault(),
                                    SalesReturnDetailsId = a.SalesReturnDetailsId,
                                    a.SalesDetailsId
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesReturnDetails1.Add(new ClsSalesReturnDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, IsComboItems = true, ComboId = Sales.ComboId, IsActive = true, PriceAddedFor = 4, UnitAddedFor = 1, SalesDetailsId = item.SalesDetailsId, SalesReturnDetailsId = item.SalesReturnDetailsId });
                                }
                                _SalesReturnDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesReturnDetails1.Add(Sales);
                            }
                        }
                    }
                }

                obj.SalesReturnDetails = _SalesReturnDetails1;

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            //decimal QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                            //                        && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                            decimal QuantityRemaining = 0, PreviousConvertedStock = 0;
                            if (SalesReturn.IsComboItems == true)
                            {
                                QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                        && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                        && a.IsComboItems == true).Select(a => a.QuantityRemaining).FirstOrDefault();

                                PreviousConvertedStock = oCommonController.StockConversion(
                                oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == true).Select(a => a.Quantity + a.FreeQuantity).
                            FirstOrDefault(), SalesReturn.ItemId, oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == true).Select(a => a.PriceAddedFor).FirstOrDefault());
                            }
                            else
                            {
                                QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                        && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                        && a.IsComboItems == false).Select(a => a.QuantityRemaining).FirstOrDefault();

                                PreviousConvertedStock = oCommonController.StockConversion(
                                oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == false).Select(a => a.Quantity + a.FreeQuantity).
                            FirstOrDefault(), SalesReturn.ItemId, oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == false).Select(a => a.PriceAddedFor).FirstOrDefault());
                            }

                            decimal convertedStock = oCommonController.StockConversion(SalesReturn.Quantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);

                            if (convertedStock > (QuantityRemaining + PreviousConvertedStock))
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = "Only " + (QuantityRemaining + PreviousConvertedStock) + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                    }
                }

              
                decimal Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum();
                if (Paid == obj.GrandTotal)
                {
                    obj.Status = "Paid";
                }
                else if (Paid > obj.GrandTotal)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "More amount is already paid",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }
                else if (Paid != 0 && Paid < obj.GrandTotal)
                {
                    obj.Status = "Partially Paid";
                }
                else
                {
                    obj.Status = "Due";
                }

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                ClsSalesReturn oClsSalesReturn = new ClsSalesReturn()
                {
                    BranchId = obj.BranchId,
                    TotalTaxAmount = obj.TotalTaxAmount,
                    //Status = obj.Status,
                    TotalDiscount = obj.TotalDiscount,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    Notes = obj.Notes,
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.DueDate,
                    SalesReturnId = obj.SalesReturnId,
                    //InvoiceNo = obj.InvoiceNo,
                    Subtotal = obj.Subtotal,
                    TotalQuantity = obj.TotalQuantity,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    TaxId = obj.TaxId,
                    TaxAmount = obj.TaxAmount,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    SalesCreditNoteReasonId = obj.SalesCreditNoteReasonId,
                    CustomerId = obj.CustomerId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    IsReverseCharge = obj.IsReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId,
                    Terms = obj.Terms
                };

                string pic1 = oConnectionContext.DbClsSalesReturn.Where(a => a.SalesReturnId == obj.SalesReturnId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/SalesReturn/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesReturn/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesReturn.AttachDocument = filepathPass;
                }
                else
                {
                    oClsSalesReturn.AttachDocument = pic1;
                }

                oConnectionContext.DbClsSalesReturn.Attach(oClsSalesReturn);
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.CompanyId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.DueDate).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SalesReturnId).IsModified = true;
                //oConnectionContext.Entry(oClsSalesReturn).Property(x => x.InvoiceNo).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Date).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.NetAmount).IsModified = true;
                //oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.RoundOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TaxAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.PlaceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SalesCreditNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.TaxCollectedFromCustomer).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Terms).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.SalesReturnAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesReturnAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

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

                        long SalesReturnAdditionalChargesId = 0;
                        if (item.SalesReturnAdditionalChargesId == 0)
                        {
                            ClsSalesReturnAdditionalCharges oClsSalesReturnAdditionalCharges = new ClsSalesReturnAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
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
                            oConnectionContext.DbClsSalesReturnAdditionalCharges.Add(oClsSalesReturnAdditionalCharges);
                            oConnectionContext.SaveChanges();
                            SalesReturnAdditionalChargesId = oClsSalesReturnAdditionalCharges.SalesReturnAdditionalChargesId;
                        }
                        else
                        {
                            ClsSalesReturnAdditionalCharges oClsSalesReturnAdditionalCharges = new ClsSalesReturnAdditionalCharges()
                            {
                                SalesReturnAdditionalChargesId = item.SalesReturnAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
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
                            oConnectionContext.DbClsSalesReturnAdditionalCharges.Attach(oClsSalesReturnAdditionalCharges);
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.SalesReturnId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                            SalesReturnAdditionalChargesId = oClsSalesReturnAdditionalCharges.SalesReturnAdditionalChargesId;
                        }

                        string query = "delete from \"tblSalesReturnAdditionalTaxJournal\" where \"SalesReturnId\"=" + oClsSalesReturn.SalesReturnId + " and \"SalesReturnAdditionalChargesId\"=" + SalesReturnAdditionalChargesId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        foreach (var taxJournal in taxList)
                        {
                            ClsSalesReturnAdditionalTaxJournal oClsSalesReturnAdditionalTaxJournal = new ClsSalesReturnAdditionalTaxJournal()
                            {
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                                SalesReturnAdditionalChargesId = SalesReturnAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                SalesReturnTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsSalesReturnAdditionalTaxJournal.Add(oClsSalesReturnAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        long SalesAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == SalesReturn.ItemDetailsId).Select(a => a.SalesAccountId).FirstOrDefault();
                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == SalesReturn.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                        if (SalesReturn.SalesReturnDetailsId == 0)
                        {
                            string query = "", StockDeductionIds = "", _json = "";
                            List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();

                            decimal convertedStock = 0, freeConvertedStock = 0;
                            bool IsManageStock = false;
                            if (SalesReturn.ProductType != "Combo")
                            {
                                IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    convertedStock = oCommonController.StockConversion(SalesReturn.Quantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);
                                    freeConvertedStock = oCommonController.StockConversion(SalesReturn.FreeQuantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);

                                    //StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                                    //        Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId)
                                    //        .Select(a => a.StockDeductionIds).FirstOrDefault();

                                    decimal SalesDetailsId = 0;

                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                            Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                            && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == true)
                                            .Select(a => a.SalesDetailsId).FirstOrDefault();
                                    }
                                    else
                                    {
                                        SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                                Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                                && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == false)
                                                .Select(a => a.SalesDetailsId).FirstOrDefault();
                                    }

                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                            == SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);                                

                                        decimal qtyRemaininToDeduct = convertedStock + freeConvertedStock;
                                        foreach (var res in _StockDeductionIds)
                                        {
                                            if (qtyRemaininToDeduct != 0)
                                            {
                                                decimal qty = 0;
                                                if (res.Quantity >= qtyRemaininToDeduct)
                                                {
                                                    qty = qtyRemaininToDeduct;
                                                }
                                                else if (res.Quantity < qtyRemaininToDeduct)
                                                {
                                                    qty = res.Quantity;
                                                }

                                                if (res.Type == "purchase")
                                                {
                                                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"PurchaseDetailsId\"=" + res.Id;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                }
                                                else if (res.Type == "openingstock")
                                                {
                                                    query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"OpeningStockId\"=" + res.Id;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                }
                                                else if (res.Type == "stocktransfer")
                                                {
                                                    query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                }

                                                oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = res.Id, Type = res.Type, Quantity = qty });

                                                query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + qty + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                                qtyRemaininToDeduct = qtyRemaininToDeduct - qty;
                                            }
                                        }
                                        ;

                                        //serializer.MaxJsonLength = 2147483644;
                                        //_json = serializer.Serialize(oClsStockDeductionIds);
                                    }
                                }
                            }

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == SalesReturn.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            decimal AmountExcTax = SalesReturn.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == SalesReturn.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == SalesReturn.TaxId
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

                            SalesReturn.ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                            ClsSalesReturnDetails oClsSalesReturnDetails = new ClsSalesReturnDetails()
                            {
                                ItemId = SalesReturn.ItemId,
                                ItemDetailsId = SalesReturn.ItemDetailsId,
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                                Quantity = SalesReturn.Quantity,
                                IsActive = SalesReturn.IsActive,
                                IsDeleted = SalesReturn.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                //StockDeductionIds = _json,
                                PriceAddedFor = SalesReturn.PriceAddedFor,
                                QuantityRemaining = SalesReturn.ProductType == "Combo" ? SalesReturn.Quantity + SalesReturn.FreeQuantity : convertedStock + freeConvertedStock,
                                //Amount = SalesReturn.Amount,
                                FreeQuantity = SalesReturn.FreeQuantity,
                                UnitAddedFor = SalesReturn.UnitAddedFor,
                                QuantityReturned = SalesReturn.ProductType == "Combo" ? SalesReturn.Quantity + SalesReturn.FreeQuantity : convertedStock + freeConvertedStock,
                                TaxId = SalesReturn.TaxId,
                                DiscountType = SalesReturn.DiscountType,
                                Discount = SalesReturn.Discount,
                                UnitCost = SalesReturn.UnitCost,
                                AmountExcTax = SalesReturn.AmountExcTax,
                                TaxAmount = SalesReturn.TaxAmount,
                                AmountIncTax = SalesReturn.AmountIncTax,
                                PriceExcTax = SalesReturn.PriceExcTax,
                                PriceIncTax = SalesReturn.PriceIncTax,
                                ComboId = SalesReturn.ComboId,
                                IsComboItems = SalesReturn.IsComboItems,
                                AccountId = SalesAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                ExtraDiscount = SalesReturn.ExtraDiscount,
                                TotalTaxAmount = SalesReturn.TotalTaxAmount,
                                IsCombo = SalesReturn.ProductType == "Combo" ? true : false,
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsSalesReturnDetails.Add(oClsSalesReturnDetails);
                            oConnectionContext.SaveChanges();

                            //string ll = "delete from tblSalesReturnDeductionId where SalesReturnDetailsId=" + oClsSalesReturnDetails.SalesReturnDetailsId;
                            //oConnectionContext.Database.ExecuteSqlCommand(ll);

                            foreach (var l in oClsStockDeductionIds)
                            {
                                ClsSalesReturnDeductionId oClsSalesReturnDeductionId = new ClsSalesReturnDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    SalesReturnDetailsId = oClsSalesReturnDetails.SalesReturnDetailsId,
                                    SalesReturnId = oClsSalesReturn.SalesReturnId,
                                };
                                oConnectionContext.DbClsSalesReturnDeductionId.Add(oClsSalesReturnDeductionId);
                                oConnectionContext.SaveChanges();
                            }

                            foreach (var taxJournal in taxList)
                            {
                                ClsSalesReturnTaxJournal oClsSalesReturnTaxJournal = new ClsSalesReturnTaxJournal()
                                {
                                    SalesReturnId = oClsSalesReturn.SalesReturnId,
                                    SalesReturnDetailsId = oClsSalesReturnDetails.SalesReturnDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    SalesReturnTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsSalesReturnTaxJournal.Add(oClsSalesReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            if (obj.Status.ToLower() != "draft")
                            {
                                if (SalesReturn.ProductType == "Combo")
                                {
                                    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (SalesReturn.Quantity + SalesReturn.FreeQuantity) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (IsManageStock == true)
                                {
                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    //query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    //oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                            }
                        }
                        else
                        {
                            string query = "", StockDeductionIds = "", _json = "";
                            List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();

                            decimal convertedStock = 0, freeConvertedStock = 0;
                            bool IsManageStock = false;
                            if (SalesReturn.ProductType != "Combo")
                            {
                                IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    convertedStock = oCommonController.StockConversion(SalesReturn.Quantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);
                                    freeConvertedStock = oCommonController.StockConversion(SalesReturn.FreeQuantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);

                                    //StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                                    //        Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId)
                                    //        .Select(a => a.StockDeductionIds).FirstOrDefault();

                                    decimal SalesDetailsId = 0;
                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                              Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                              && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == true)
                                              .Select(a => a.SalesDetailsId).FirstOrDefault();
                                    }
                                    else
                                    {
                                        SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                              Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                              && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == false)
                                              .Select(a => a.SalesDetailsId).FirstOrDefault();
                                    }

                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                            == SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                        decimal qtyRemaininToDeduct = convertedStock + freeConvertedStock;
                                        foreach (var res in _StockDeductionIds)
                                        {
                                            if (qtyRemaininToDeduct != 0)
                                            {
                                                decimal qty = 0;
                                                if (res.Quantity >= qtyRemaininToDeduct)
                                                {
                                                    qty = qtyRemaininToDeduct;
                                                }
                                                else if (res.Quantity < qtyRemaininToDeduct)
                                                {
                                                    qty = res.Quantity;
                                                }

                                                if (res.Type == "purchase")
                                                {
                                                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"PurchaseDetailsId\"=" + res.Id;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                }
                                                else if (res.Type == "openingstock")
                                                {
                                                    query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"OpeningStockId\"=" + res.Id;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                }
                                                else if (res.Type == "stocktransfer")
                                                {
                                                    query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                }

                                                oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = res.Id, Type = res.Type, Quantity = qty });

                                                query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + qty + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                                qtyRemaininToDeduct = qtyRemaininToDeduct - qty;
                                            }
                                        }
                                        ;

                                        //serializer.MaxJsonLength = 2147483644;
                                        //_json = serializer.Serialize(oClsStockDeductionIds);
                                    }
                                }
                            }
                            
                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == SalesReturn.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            decimal AmountExcTax = SalesReturn.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == SalesReturn.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == SalesReturn.TaxId
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

                            SalesReturn.ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();


                            ClsSalesReturnDetails oClsSalesReturnDetails = new ClsSalesReturnDetails()
                            {
                                SalesReturnDetailsId = SalesReturn.SalesReturnDetailsId,
                                ItemId = SalesReturn.ItemId,
                                ItemDetailsId = SalesReturn.ItemDetailsId,
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                                Quantity = SalesReturn.Quantity,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                //StockDeductionIds = _json,
                                PriceAddedFor = SalesReturn.PriceAddedFor,
                                QuantityRemaining = SalesReturn.ProductType == "Combo" ? SalesReturn.Quantity + SalesReturn.FreeQuantity : convertedStock + freeConvertedStock,
                                //Amount = SalesReturn.Amount,
                                FreeQuantity = SalesReturn.FreeQuantity,
                                UnitAddedFor = SalesReturn.UnitAddedFor,
                                QuantityReturned = SalesReturn.ProductType == "Combo" ? SalesReturn.Quantity + SalesReturn.FreeQuantity : convertedStock + freeConvertedStock,
                                TaxId = SalesReturn.TaxId,
                                DiscountType = SalesReturn.DiscountType,
                                Discount = SalesReturn.Discount,
                                UnitCost = SalesReturn.UnitCost,
                                AmountExcTax = SalesReturn.AmountExcTax,
                                TaxAmount = SalesReturn.TaxAmount,
                                AmountIncTax = SalesReturn.AmountIncTax,
                                PriceExcTax = SalesReturn.PriceExcTax,
                                PriceIncTax = SalesReturn.PriceIncTax,
                                ComboId = SalesReturn.ComboId,
                                IsComboItems = SalesReturn.IsComboItems,
                                AccountId = SalesAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                ExtraDiscount = SalesReturn.ExtraDiscount,
                                TotalTaxAmount = SalesReturn.TotalTaxAmount,
                                IsCombo = SalesReturn.ProductType == "Combo" ? true : false,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsSalesReturnDetails.Attach(oClsSalesReturnDetails);
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.SalesReturnId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.ModifiedOn).IsModified = true;
                            //oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.StockDeductionIds).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            //oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.Amount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.FreeQuantity).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.QuantityReturned).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.DiscountType).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.Discount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.PriceExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.PriceIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.ComboId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.IsComboItems).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.DiscountAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.TaxAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.ExtraDiscount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesReturnDetails).Property(x => x.IsCombo).IsModified = true;
                            oConnectionContext.SaveChanges();

                            string ll = "delete from \"tblSalesReturnDeductionId\" where \"SalesReturnDetailsId\"=" + SalesReturn.SalesReturnDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(ll);

                            foreach (var l in oClsStockDeductionIds)
                            {
                                ClsSalesReturnDeductionId oClsSalesReturnDeductionId = new ClsSalesReturnDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    SalesReturnDetailsId = SalesReturn.SalesReturnDetailsId,
                                    SalesReturnId = oClsSalesReturn.SalesReturnId,
                                };
                                oConnectionContext.DbClsSalesReturnDeductionId.Add(oClsSalesReturnDeductionId);
                                oConnectionContext.SaveChanges();
                            }

                            query = "delete from \"tblSalesReturnTaxJournal\" where \"SalesReturnId\"=" + oClsSalesReturn.SalesReturnId + " and \"SalesReturnDetailsId\"=" + oClsSalesReturnDetails.SalesReturnDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            foreach (var taxJournal in taxList)
                            {
                                ClsSalesReturnTaxJournal oClsSalesReturnTaxJournal = new ClsSalesReturnTaxJournal()
                                {
                                    SalesReturnId = oClsSalesReturn.SalesReturnId,
                                    SalesReturnDetailsId = oClsSalesReturnDetails.SalesReturnDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    SalesReturnTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsSalesReturnTaxJournal.Add(oClsSalesReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            if (obj.Status.ToLower() != "draft")
                            {
                                if (SalesReturn.ProductType == "Combo")
                                {
                                    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (SalesReturn.Quantity + SalesReturn.FreeQuantity) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else if (IsManageStock == true)
                                {
                                    if (SalesReturn.IsComboItems == true)
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                    else
                                    {
                                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    //query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                    //oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                            }
                        }
                    }
                }

                if (obj.Status.ToLower() != "draft")
                {
                    decimal SalesDue = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                   (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                   oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                   && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                    ClsCustomerPaymentVm oClsCustomerPayment = new ClsCustomerPaymentVm
                    {
                        SalesId = obj.SalesId,
                        SalesReturnId = oClsSalesReturn.SalesReturnId,
                        CompanyId = obj.CompanyId,
                        BranchId = obj.BranchId,
                        AddedBy = obj.AddedBy,
                        IsActive = true,
                        IsDeleted = false,
                        Amount = obj.GrandTotal,
                        PaymentDate = CurrentDate,
                        CustomerId = obj.CustomerId,
                        Type = "Customer Payment",
                        AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault(),
                        AmountExcTax = obj.GrandTotal,
                        TaxAccountId = 0,
                        TaxAmount = 0,
                        TaxId = 0,
                    };

                    if (SalesDue >= obj.GrandTotal)
                    {
                        oClsCustomerPayment.CustomerPaymentIds = new List<ClsCustomerPaymentIds> { new ClsCustomerPaymentIds()
                    {
                        Due= SalesDue,
                        Amount= obj.GrandTotal,
                        SalesId = obj.SalesId,
                        Type = oConnectionContext.DbClsSales.Where(a=>
                        a.CompanyId == obj.CompanyId && a.SalesId ==obj.SalesId && a.IsActive == true && a.IsDeleted==false).Select(a=>a.SalesType).FirstOrDefault() + " Payment"
                    }
                    };
                    }
                    else if (SalesDue < obj.GrandTotal)
                    {
                        oClsCustomerPayment.CustomerPaymentIds = new List<ClsCustomerPaymentIds> { new ClsCustomerPaymentIds()
                    {
                        Due= SalesDue,
                        Amount= SalesDue,
                        SalesId = obj.SalesId,
                        Type = oConnectionContext.DbClsSales.Where(a=>
                        a.CompanyId == obj.CompanyId && a.SalesId ==obj.SalesId && a.IsActive == true && a.IsDeleted==false).Select(a=>a.SalesType).FirstOrDefault() + " Payment"
                    }
                    };
                    }
                    oCommonController.CreditNoteCreditsApply(oClsCustomerPayment);
                }


                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Return",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Return \"" + obj.InvoiceNo + "\" updated",
                    Id = oClsSalesReturn.SalesReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Credit Note", obj.CompanyId, oClsSalesReturn.SalesReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Return updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            InvoiceId = oConnectionContext.DbClsSalesReturn.Where(a => a.SalesReturnId == oClsSalesReturn.SalesReturnId).Select(a => a.InvoiceId).FirstOrDefault(),
                            SalesType = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.SalesType).FirstOrDefault()
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceSalesReturn }).FirstOrDefault(),
                        PosSetting = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceSalesReturn }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnDetailsDelete(ClsSalesReturnDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.SalesReturnId != 0)
                {
                    obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == oConnectionContext.DbClsSalesReturn.
                    Where(b => b.SalesReturnId == obj.SalesReturnId).Select(b => b.SalesId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                    var details = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == obj.SalesReturnId && a.IsDeleted == false).Select(a => new
                    {
                        a.SalesReturnDetailsId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        SalesId = oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.SalesReturnId).Select(b => b.SalesId).FirstOrDefault()
                    }).ToList();

                    string query = "update \"tblSalesReturnDetails\" set \"IsDeleted\"=True where \"SalesReturnId\"=" + obj.SalesReturnId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in details)
                    {
                        //decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                        //string StockDeductionIds = oConnectionContext.DbClsSalesReturnDetails.
                        //          Where(a => a.SalesReturnDetailsId == item.SalesReturnDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                        List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesReturnDeductionId.Where(a => a.SalesReturnDetailsId
                        == item.SalesReturnDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                        foreach (var res in _StockDeductionIds)
                        {
                            //decimal convertedQty = oCommonController.StockConversion(res.Quantity, item.ItemId, item.PriceAddedFor);

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
                            //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + convertedQty + ",QuantitySold=QuantitySold,0)-" + convertedQty + " where StockAdjustmentDetailsId=" + res.Id;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query);
                            //}
                            else if (res.Type == "stocktransfer")
                            {
                                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + " where \"SalesId\"=" + item.SalesId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\" = " + item.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        ;
                    }

                    //foreach (var item in details)
                    //{
                    //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + item.Quantity + " where BranchId=" + BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                    //}

                }
                else
                {
                    obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == oConnectionContext.DbClsSalesReturn.
                    Where(b => b.SalesReturnId == oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnDetailsId == obj.SalesReturnDetailsId).Select(c => c.SalesReturnId).FirstOrDefault()).Select(b => b.SalesId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                    var details = oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnDetailsId == obj.SalesReturnDetailsId).Select(a => new
                    {
                        a.SalesReturnDetailsId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        SalesId = oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.SalesReturnId).Select(b => b.SalesId).FirstOrDefault()
                    }).FirstOrDefault();

                    string query = "update \"tblSalesReturnDetails\" set \"IsDeleted\"=True where \"SalesReturnDetailsId\"=" + obj.SalesReturnDetailsId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    //string StockDeductionIds = oConnectionContext.DbClsSalesReturnDetails.
                    //          Where(a => a.SalesReturnDetailsId == details.SalesReturnDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);
                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesReturnDeductionId.Where(a => a.SalesReturnDetailsId
                        == details.SalesReturnDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    foreach (var res in _StockDeductionIds)
                    {
                        //decimal convertedQty = oCommonController.StockConversion(res.Quantity, item.ItemId, item.PriceAddedFor);

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
                        //    query = "update tblStockAdjustmentDetails set QuantityRemaining=QuantityRemaining,0)+" + convertedQty + ",QuantitySold=QuantitySold,0)-" + convertedQty + " where StockAdjustmentDetailsId=" + res.Id;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                        //}
                        else if (res.Type == "stocktransfer")
                        {
                            query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }

                        query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + res.Quantity + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + details.ItemId + " and \"ItemDetailsId\"=" + details.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + res.Quantity + " where \"SalesId\"=" + details.SalesId + " and \"ItemId\"=" + details.ItemId + " and \"ItemDetailsId\" = " + details.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                    ;

                    //query = "update tblItemBranchMap set Quantity=Quantity,0)-" + details.Quantity + " where BranchId=" + BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
                    //oConnectionContext.Database.ExecuteSqlCommand(query);
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

        public async Task<IHttpActionResult> UpdateSalesReturnStatus(ClsSalesReturnVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "Sales return Status is required", Id = "divSalesReturnStatus" });
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

                obj = (from a in oConnectionContext.DbClsSalesReturn
                       join b in oConnectionContext.DbClsSales
on a.SalesId equals b.SalesId
                       where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
                       select new ClsSalesReturnVm
                       {
                           CustomerId = a.CustomerId,
                           CompanyId = a.CompanyId,
                           Status = obj.Status,
                           SalesReturnId = obj.SalesReturnId,
                           InvoiceNo = a.InvoiceNo,
                           SalesId = b.SalesId,
                           BranchId = b.BranchId,
                           AddedBy = a.AddedBy,
                           GrandTotal = a.GrandTotal,
                           SalesReturnDetails = (from b in oConnectionContext.DbClsSalesReturnDetails
                                                     //join b in oConnectionContext.DbClsSalesDetails
                                                     //on z.SalesDetailsId equals b.SalesDetailsId
                                                 join c in oConnectionContext.DbClsItemDetails
                                                 on b.ItemDetailsId equals c.ItemDetailsId
                                                 join d in oConnectionContext.DbClsItem
                                                 on c.ItemId equals d.ItemId
                                                 where b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false
                                                 && b.IsComboItems == false //&& z.SalesReturnId == a.SalesReturnId
                                                 select new ClsSalesReturnDetailsVm
                                                 {
                                                     IsComboItems = b.IsComboItems,
                                                     SalesReturnDetailsId = b.SalesReturnDetailsId,
                                                     ProductType = d.ProductType,
                                                     ItemId = b.ItemId,
                                                     ItemDetailsId = b.ItemDetailsId,
                                                     QuantityReturned = b.QuantityReturned
                                                 }).ToList(),
                       }).FirstOrDefault();

                List<ClsSalesReturnDetailsVm> _SalesReturnDetails = new List<ClsSalesReturnDetailsVm>();
                if (obj.SalesReturnDetails != null)
                {
                    foreach (var Sales in obj.SalesReturnDetails)
                    {
                        Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                        if (Sales.ProductType.ToLower() == "combo")
                        {
                            string _comboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                            Sales.ComboId = oCommonController.CreateToken();
                            var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == _comboId && a.IsComboItems == true).Select(a => new
                            {
                                ItemId = a.ItemId,
                                ItemDetailsId = a.ItemDetailsId,
                                ComboItemDetailsId = a.ItemDetailsId,
                                Quantity = a.ComboPerUnitQuantity,
                                SalesDetailsId = a.SalesDetailsId,
                            }).ToList();

                            foreach (var item in combo)
                            {
                                _SalesReturnDetails.Add(new ClsSalesReturnDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, IsComboItems = true, ComboId = Sales.ComboId, IsActive = true, PriceAddedFor = 4, UnitAddedFor = 1, SalesDetailsId = Sales.SalesDetailsId });
                            }
                            _SalesReturnDetails.Add(Sales);
                        }
                        else
                        {
                            _SalesReturnDetails.Add(Sales);
                        }
                    }
                }

                obj.SalesReturnDetails = _SalesReturnDetails;

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        if (SalesReturn.ProductType != "Combo")
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                //    decimal QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                                //&& a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                                decimal QuantityRemaining = 0;
                                if (SalesReturn.IsComboItems == true)
                                {
                                    QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == true).Select(a => a.QuantityRemaining).FirstOrDefault();
                                }
                                else
                                {
                                    QuantityRemaining = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId
                            && a.ItemId == SalesReturn.ItemId && a.ItemDetailsId == SalesReturn.ItemDetailsId
                            && a.IsComboItems == false).Select(a => a.QuantityRemaining).FirstOrDefault();
                                }

                                decimal convertedStock = oCommonController.StockConversion(SalesReturn.Quantity + SalesReturn.FreeQuantity, SalesReturn.ItemId, SalesReturn.PriceAddedFor);

                                if (QuantityRemaining == 0 || (convertedStock > QuantityRemaining))
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Only " + QuantityRemaining + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                        Data = new
                                        {
                                        }
                                    };
                                    return await Task.FromResult(Ok(data));
                                }
                            }
                        }
                    }
                }

                ClsSalesReturn oClsSalesReturn = new ClsSalesReturn()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    SalesReturnId = obj.SalesReturnId,
                    Status = obj.Status,
                };

                oConnectionContext.DbClsSalesReturn.Attach(oClsSalesReturn);
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.SalesReturnId).IsModified = true;
                oConnectionContext.Entry(oClsSalesReturn).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.SalesReturnDetails != null)
                {
                    foreach (var SalesReturn in obj.SalesReturnDetails)
                    {
                        string query = "", StockDeductionIds = "", _json = "";
                        List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();

                        bool IsManageStock = false;
                        if (SalesReturn.ProductType != "Combo")
                        {
                            IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                decimal SalesDetailsId = 0;
                                if (SalesReturn.IsComboItems == true)
                                {
                                    SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                             Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                             && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == true)
                                             .Select(a => a.SalesDetailsId).FirstOrDefault();
                                }
                                else
                                {
                                    SalesDetailsId = oConnectionContext.DbClsSalesDetails.
                                             Where(a => a.SalesId == obj.SalesId && a.ItemId == SalesReturn.ItemId
                                             && a.ItemDetailsId == SalesReturn.ItemDetailsId && a.IsComboItems == false)
                                             .Select(a => a.SalesDetailsId).FirstOrDefault();
                                }

                                if (obj.Status.ToLower() != "draft")
                                {
                                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                    decimal qtyRemaininToDeduct = SalesReturn.QuantityReturned;
                                    foreach (var res in _StockDeductionIds)
                                    {
                                        if (qtyRemaininToDeduct != 0)
                                        {
                                            decimal qty = 0;
                                            if (res.Quantity >= qtyRemaininToDeduct)
                                            {
                                                qty = qtyRemaininToDeduct;
                                            }
                                            else if (res.Quantity < qtyRemaininToDeduct)
                                            {
                                                qty = res.Quantity;
                                            }

                                            if (res.Type == "purchase")
                                            {
                                                query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"PurchaseDetailsId\"=" + res.Id;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                            else if (res.Type == "openingstock")
                                            {
                                                query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + qty + " where \"OpeningStockId\"=" + res.Id;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                            else if (res.Type == "stocktransfer")
                                            {
                                                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + qty + ",\"QuantitySold\"=\"QuantitySold\"-" + res.Quantity + " where \"StockTransferDetailsId\"=" + res.Id;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }

                                            oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = res.Id, Type = res.Type, Quantity = qty });

                                            query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + qty + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                            oConnectionContext.Database.ExecuteSqlCommand(query);

                                            qtyRemaininToDeduct = qtyRemaininToDeduct - qty;
                                        }
                                    }
                                    ;

                                    //serializer.MaxJsonLength = 2147483644;
                                    //_json = serializer.Serialize(oClsStockDeductionIds);
                                }
                            }
                        }

                        string ll = "delete from \"tblSalesReturnDeductionId\" where \"SalesReturnDetailsId\"=" + SalesReturn.SalesReturnDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(ll);

                        foreach (var l in oClsStockDeductionIds)
                        {
                            ClsSalesReturnDeductionId oClsSalesReturnDeductionId = new ClsSalesReturnDeductionId()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                Id = l.Id,
                                Type = l.Type,
                                Quantity = l.Quantity,
                                SalesReturnDetailsId = SalesReturn.SalesReturnDetailsId,
                                SalesReturnId = oClsSalesReturn.SalesReturnId,
                            };
                            oConnectionContext.DbClsSalesReturnDeductionId.Add(oClsSalesReturnDeductionId);
                            oConnectionContext.SaveChanges();
                        }

                        if (obj.Status.ToLower() != "draft")
                        {
                            if (SalesReturn.ProductType == "Combo")
                            {
                                query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (SalesReturn.QuantityReturned) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                            else if (IsManageStock == true)
                            {
                                if (SalesReturn.IsComboItems == true)
                                {
                                    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + SalesReturn.QuantityReturned + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=true";
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                else
                                {
                                    query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + SalesReturn.QuantityReturned + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId + " and \"IsComboItems\"=false";
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }

                                //query = "update \"tblSalesDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (SalesReturn.QuantityReturned) + " where \"SalesId\"=" + obj.SalesId + " and \"ItemId\"=" + SalesReturn.ItemId + " and \"ItemDetailsId\"=" + SalesReturn.ItemDetailsId;
                                //oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }

                    }
                }

                if (obj.Status.ToLower() != "draft")
                {
                    decimal SalesDue = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                    (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                    && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                    ClsCustomerPaymentVm oClsCustomerPayment = new ClsCustomerPaymentVm
                    {
                        SalesReturnId = oClsSalesReturn.SalesReturnId,
                        CompanyId = obj.CompanyId,
                        BranchId = obj.BranchId,
                        AddedBy = obj.AddedBy,
                        IsActive = true,
                        IsDeleted = false,
                        Amount = obj.GrandTotal,
                        PaymentDate = CurrentDate,
                        CustomerId = obj.CustomerId,
                        Type = "Customer Payment",
                        AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault(),
                        AmountExcTax = obj.GrandTotal,
                        TaxAccountId = 0,
                        TaxAmount = 0,
                        TaxId = 0,
                    };

                    if (SalesDue >= obj.GrandTotal)
                    {
                        oClsCustomerPayment.CustomerPaymentIds = new List<ClsCustomerPaymentIds> { new ClsCustomerPaymentIds()
                    {
                        Due = SalesDue,
                        Amount= obj.GrandTotal,
                        SalesId = obj.SalesId,
                        Type = oConnectionContext.DbClsSales.Where(a=>
                        a.CompanyId == obj.CompanyId && a.SalesId ==obj.SalesId && a.IsActive == true && a.IsDeleted==false).Select(a=>a.SalesType).FirstOrDefault() + " Payment"
                    }
                    };
                    }
                    else if (SalesDue < obj.GrandTotal)
                    {
                        oClsCustomerPayment.CustomerPaymentIds = new List<ClsCustomerPaymentIds> { new ClsCustomerPaymentIds()
                    {
                        Due = SalesDue,
                        Amount= SalesDue,
                        SalesId = obj.SalesId,
                        Type = oConnectionContext.DbClsSales.Where(a=>
                        a.CompanyId == obj.CompanyId && a.SalesId ==obj.SalesId && a.IsActive == true && a.IsDeleted==false).Select(a=>a.SalesType).FirstOrDefault() + " Payment"
                    }
                    };
                    }

                    oCommonController.CreditNoteCreditsApply(oClsCustomerPayment);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Return",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Return \"" + obj.InvoiceNo + "\" updated",
                    Id = oClsSalesReturn.SalesReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Credit Note", obj.CompanyId, oClsSalesReturn.SalesReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Return updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            InvoiceId = oConnectionContext.DbClsSalesReturn.Where(a => a.SalesReturnId == oClsSalesReturn.SalesReturnId).Select(a => a.InvoiceId).FirstOrDefault(),
                            SalesType = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.SalesType).FirstOrDefault()
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceSalesReturn }).FirstOrDefault(),
                        PosSetting = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceSalesReturn }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnReport(ClsSalesVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
                BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
              && b.IsDeleted == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
               oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            }).FirstOrDefault();

            if (obj.BranchId == 0)
            {
                obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            }

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = (from a in oConnectionContext.DbClsSalesReturn
                       join b in oConnectionContext.DbClsSales
   on a.SalesId equals b.SalesId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.BranchId == obj.BranchId
                       && b.Status != "Draft"
                       select new
                       {
                           a.SalesId,
                           b.BranchId,
                           Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                           SalesReturnId = a.SalesReturnId,
                           a.GrandTotal,
                           a.Notes,
                           a.Date,
                           a.InvoiceNo,
                           a.Subtotal,
                           b.CustomerId,
                           Customer = oConnectionContext.DbClsUser.Where(c => c.UserId == b.CustomerId).Select(c => c.Name).FirstOrDefault(),
                           CompanyId = a.CompanyId,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,

                           ModifiedOn = a.ModifiedOn,
                           Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesReturnId).Count() == 0 ? 0 :
                       oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           a.Status,
                           Due = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesReturnId).Count() == 0 ? a.GrandTotal :
                       a.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           a.TotalQuantity,

                       }).ToList();

            if (obj.InvoiceNo != null && obj.InvoiceNo != "")
            {
                det = det.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower()).Select(a => a).ToList();
            }
            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.Date.Date >= obj.FromDate && a.Date.Date <= obj.ToDate).ToList();
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
                    Sales = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Invoice(ClsSalesVm obj)
        {
            var det = (from a in oConnectionContext.DbClsSalesReturn
                       join z in oConnectionContext.DbClsSales
on a.SalesId equals z.SalesId
                       where a.IsDeleted == false && a.IsCancelled == false && a.InvoiceId == obj.InvoiceId
                       select new
                       {

                           a.Terms,
                           a.TotalTaxAmount,
                           a.IsCancelled,
                           a.SalesReturnId,
                           a.BranchId,
                           User = oConnectionContext.DbClsUser.Where(c => c.UserId == z.CustomerId).Select(c => new
                           {
                               c.Name,
                               c.MobileNo,
                               c.EmailId,
                               TaxNo = c.BusinessRegistrationNo,
                               Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == z.CustomerId).Select(b => new
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
                           IsShippingAddressDifferent = oConnectionContext.DbClsUser.Where(b => b.UserId == z.CustomerId).Select(b => b.IsShippingAddressDifferent).FirstOrDefault(),
                           Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == z.BranchId).Select(b => new
                           {
                               b.Branch,
                               b.Mobile,
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
                           z.SalesDate,
                           a.Notes,
                           a.InvoiceNo,
                           a.Date,
                           a.DueDate,
                           a.Subtotal,
                           a.Discount,
                           a.DiscountType,
                           a.TotalDiscount,
                           a.TaxId,
                           a.TaxAmount,
                           a.GrandTotal,
                           //a.OtherCharges,
                           a.Status,
                           a.CompanyId,
                           a.TotalQuantity,
                           //a.Status,
                           a.RoundOff,
                           SpecialDiscount = a.SpecialDiscount,
                           a.NetAmount,
                           //         Due = oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsCustomerRefund.Where(b => (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == a.SalesReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           //         Payments = oConnectionContext.DbClsCustomerRefund.Where(b => b.SalesReturnId == a.SalesReturnId && (b.Type.ToLower() == "customer refund") && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
                           //         {
                           //             b.PaymentDate,
                           //             b.ReferenceNo,
                           //             b.Amount,
                           //             b.PaymentTypeId,
                           //             PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                           //         }),
                           SalesDetails = (from b in oConnectionContext.DbClsSalesReturnDetails
                                           join c in oConnectionContext.DbClsItemDetails
                                           on b.ItemDetailsId equals c.ItemDetailsId
                                           join d in oConnectionContext.DbClsItem
                                           on c.ItemId equals d.ItemId
                                           where b.SalesReturnId == a.SalesReturnId && b.IsDeleted == false
                                           && b.IsComboItems == false
                                           select new
                                           {
                                               d.ProductImage,
                                               b.TotalTaxAmount,
                                               Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                               //b.DiscountType,
                                               //b.SalesDetailsId,
                                               //b.PriceIncTax,
                                               //b.OtherInfo,
                                               //b.Amount,
                                               //b.Discount,
                                               //b.SalesId,
                                               b.Quantity,
                                               b.FreeQuantity,
                                               //b.TaxId,
                                               //b.UnitCost,
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
                                               //Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                               //TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                               d.TaxType,
                                               d.ItemCode,
                                               b.PriceExcTax,
                                               b.PriceIncTax,
                                               //b.Amount,
                                               b.DiscountType,
                                               b.Discount,
                                               b.UnitCost,
                                               b.AmountExcTax,
                                               b.TaxAmount,
                                               b.AmountIncTax,
                                               ComboItems = (from bb in oConnectionContext.DbClsSalesReturnDetails
                                                             join cc in oConnectionContext.DbClsItemDetails
                                                             on bb.ItemDetailsId equals cc.ItemDetailsId
                                                             join dd in oConnectionContext.DbClsItem
                                                             on cc.ItemId equals dd.ItemId
                                                             where bb.SalesReturnId == a.SalesReturnId && bb.ComboId == b.ComboId && bb.IsDeleted == false
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
                           SalesAdditionalCharges = oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(b => b.SalesReturnId == a.SalesReturnId
                 && b.IsDeleted == false && b.IsActive == true).Select(b => new ClsSalesReturnAdditionalChargesVm
                 {
                     SalesReturnAdditionalChargesId = b.SalesReturnAdditionalChargesId,
                     Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
                     AdditionalChargeId = b.AdditionalChargeId,
                     SalesReturnId = b.SalesReturnId,
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

            var AllTaxs = oConnectionContext.DbClsSalesReturn.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.SalesReturnId == det.SalesReturnId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsSalesReturnDetails.Where(a => a.SalesReturnId == det.SalesReturnId && a.IsDeleted == false
                                && a.IsComboItems == false).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).Concat(oConnectionContext.DbClsSalesReturnAdditionalCharges.Where(a => a.SalesReturnId == det.SalesReturnId
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

            // Get catalogue for this branch that is marked to show in invoices
            //var branchId = (det != null) ? oConnectionContext.DbClsSalesReturn
            //    .Where(a => a.SalesReturnId == det.SalesReturnId)
            //    .Join(oConnectionContext.DbClsSales, sr => sr.SalesId, s => s.SalesId, (sr, s) => s.BranchId)
            //    .FirstOrDefault() : 0;
            
            var catalogue = (det != null) ? oConnectionContext.DbClsCatalogue
                .Where(a => a.BranchId == det.BranchId &&
                            a.CompanyId == det.CompanyId &&
                            a.IsActive == true &&
                            a.IsDeleted == false &&
                            a.ShowInInvoices == true &&
                            (a.NeverExpires == true || 
                             ((a.ValidFrom == null || a.ValidFrom <= DateTime.Now) && 
                              (a.ValidTo == null || a.ValidTo >= DateTime.Now))))
                .Select(a => new
                {
                    a.QRCodeImage,
                    a.UniqueSlug,
                    a.CatalogueName
                })
                .FirstOrDefault() : null;

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sale = det,
                    Taxs = finalTaxs,
                    BusinessSetting = BusinessSetting,
                    Catalogue = catalogue
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnJournal(ClsSalesReturnVm obj)
        {
            //  obj.SalesReturnId = oConnectionContext.DbClsSalesReturn.Where(a => a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsActive == true &&
            //a.IsDeleted == false && a.IsCancelled == false).Select(a => a.SalesReturnId).FirstOrDefault();

            var taxList = (from q in oConnectionContext.DbClsSalesReturnTaxJournal
                           join a in oConnectionContext.DbClsSalesReturnDetails
                           on q.SalesReturnDetailsId equals a.SalesReturnDetailsId
                           join b in oConnectionContext.DbClsSalesReturn
                        on a.SalesReturnId equals b.SalesReturnId
                           join c in oConnectionContext.DbClsTax
                           on q.TaxId equals c.TaxId
                           where q.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //&& a.TaxAmount != 0
                           && c.TaxTypeId != 0
                           select new
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = q.TaxAmount,
                               Credit = 0,
                               AccountId = q.AccountId
                           }).Concat(from q in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                     join a in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                     on q.SalesReturnAdditionalChargesId equals a.SalesReturnAdditionalChargesId
                                     join b in oConnectionContext.DbClsSalesReturn
                                  on a.SalesReturnId equals b.SalesReturnId
                                     join c in oConnectionContext.DbClsTax
                                     on q.TaxId equals c.TaxId
                                     where q.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                     //&& a.TaxAmount != 0
                                     && c.TaxTypeId != 0
                                     && a.AmountExcTax > 0
                                     select new
                                     {
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = q.TaxAmount,
                                         Credit = 0,
                                         AccountId = q.AccountId
                                     }).ToList();

            var journal = (from a in oConnectionContext.DbClsSalesReturn
                           where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               //Account Receivable
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = 0,
                               Credit = a.GrandTotal
                           }).Concat(from a in oConnectionContext.DbClsSalesReturnDetails
                                     join b in oConnectionContext.DbClsSalesReturn
                                  on a.SalesReturnId equals b.SalesReturnId
                                     //join p in oConnectionContext.DbClsSales
                                     //on b.SalesId equals p.SalesId
                                     where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsComboItems == false
                                     //&& p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                     group a by a.AccountId into stdGroup
                                     select new ClsBankPaymentVm
                                     {
                                         // sales account
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                         Credit = 0,
                                     }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                   //         join p in oConnectionContext.DbClsSales
                                                   //on a.SalesId equals p.SalesId
                                               where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
                                               && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                               //&& p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                               select new ClsBankPaymentVm
                                               {
                                                   // Round off charge
                                                   AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.RoundOffAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                   Debit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                   Credit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                               }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                             //join p in oConnectionContext.DbClsSales
                                                             //on a.SalesId equals p.SalesId
                                                         where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
                                                         && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                         //&& p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                         select new ClsBankPaymentVm
                                                         {
                                                             // discount 
                                                             AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.SpecialDiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                             Debit = 0,
                                                             Credit = a.SpecialDiscount,
                                                         }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                             //join p in oConnectionContext.DbClsSales
                                                             //on a.SalesId equals p.SalesId
                                                         where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
                                                         && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                         //&& p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                         select new ClsBankPaymentVm
                                                         {
                                                             // discount 
                                                             AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.DiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                             Debit = 0,
                                                             Credit = a.TotalDiscount,
                                                         }).Concat(from a in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                                                   where a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
                                                                   && a.IsDeleted == false && a.IsActive == true
                                                                   select new ClsBankPaymentVm
                                                                   {
                                                                       // Write Off journal account 
                                                                       AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                       Debit = a.AmountExcTax,
                                                                       Credit = 0,
                                                                   }).ToList();

            var sale = oConnectionContext.DbClsSalesReturn.Where(a => a.SalesReturnId == obj.SalesReturnId && a.CompanyId == obj.CompanyId
           && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new { a.IsReverseCharge }).FirstOrDefault();

            if (sale.IsReverseCharge == 2)
            {
                journal = journal.Concat(from a in taxList
                                         select new ClsBankPaymentVm
                                         {
                                             // tax 
                                             AccountName = a.AccountName,
                                             Debit = a.Debit,
                                             Credit = a.Credit,
                                             IsTaxAccount = true
                                         }).ToList();
            }

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

        public async Task<IHttpActionResult> SalesReturnSearchItems(ClsSalesReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            var Item = oConnectionContext.DbClsItem.Where(a => a.SkuCode == obj.ItemCode.Trim() && a.IsActive == true
            && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                ItemDetailsId = 0L,
                //a.ProductType,
                a.ItemType,
            }).FirstOrDefault();

            if (Item == null)
            {
                Item = oConnectionContext.DbClsItemDetails.Where(a => a.SKU == obj.ItemCode.Trim() && a.IsActive == true
            && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                a.ItemDetailsId,
                //a.ProductType,
                ItemType = oConnectionContext.DbClsItem.Where(b => b.ItemId == a.ItemId).Select(b => b.ItemType).FirstOrDefault(),
            }).FirstOrDefault();
            }

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId
            }).FirstOrDefault();

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.StateId).FirstOrDefault();

            //long SalesDetailsId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId && a.IsDeleted == false
            //&& a.ItemId == Item.ItemId && a.IsComboItems ==false).Select(a => a.SalesDetailsId).FirstOrDefault();

            //if (SalesDetailsId != 0)
            if (Item.ItemType.ToLower() == "product")
            {
                //var det = (from b in oConnectionContext.DbClsSalesDetails
                //           join c in oConnectionContext.DbClsItemDetails
                //           on b.ItemDetailsId equals c.ItemDetailsId
                //           join d in oConnectionContext.DbClsItem
                //           on c.ItemId equals d.ItemId
                //           where b.SalesId == obj.SalesId && b.IsDeleted == false
                //           && d.ItemId == Item.ItemId && b.QuantityRemaining > 0
                //           && b.IsComboItems == false
                //           select new
                //           {
                //               b.ExtraDiscount,
                //               b.PriceExcTax,
                //               AmountExcTax = 0,
                //               IsManageStock = d.IsManageStock,
                //               FreeQuantity = 0,
                //               PurchaseReturnUnitCost = b.UnitCost,
                //               b.QuantityRemaining,
                //               SalesReturnUnitCost = b.PriceIncTax,
                //               SalesReturnAmount = 0,
                //               b.DiscountType,
                //               b.SalesDetailsId,
                //               b.PriceIncTax,
                //               b.OtherInfo,
                //               AmountIncTax = 0,
                //               b.Discount,
                //               Quantity = 0,
                //               b.TaxId,
                //               InterStateTaxId = b.TaxId,
                //               b.UnitCost,
                //               d.ItemId,
                //               d.ProductType,
                //               c.ItemDetailsId,
                //               d.ItemName,
                //               SKU = c.SKU == null ? d.SkuCode : c.SKU,
                //               c.VariationDetailsId,
                //               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                //               UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                //               //c.SalesExcTax,
                //               SalesExcTax = b.UnitCost,
                //               c.SalesIncTax,
                //               c.TotalCost,
                //               Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                //               TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                //               d.TaxType,
                //               d.ItemCode,
                //               UnitId = d.UnitId,
                //               SecondaryUnitId = d.SecondaryUnitId,
                //               TertiaryUnitId = d.TertiaryUnitId,
                //               QuaternaryUnitId = d.QuaternaryUnitId,
                //               UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                //               SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                //               TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                //               QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                //               UToSValue = d.UToSValue,
                //               SToTValue = d.SToTValue,
                //               TToQValue = d.TToQValue,
                //               AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                //               SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                //               TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                //               QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                //               PriceAddedFor = b.PriceAddedFor,
                //               LotNo = b.LotTypeForLotNoChecking == "purchase" ?
                //                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                //                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                //                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                //                    : "Default Stock Accounting Method",
                //               LotManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ?
                //                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                //                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                //                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                //                    : null,
                //               LotExpiryDate = b.LotTypeForLotNoChecking == "purchase" ?
                //                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                //                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                //                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                //                    : null,
                //               b.LotId,
                //               b.LotType,
                //           }).ToList();

                var det = (from b in oConnectionContext.DbClsSalesDetails
                           join c in oConnectionContext.DbClsItemDetails
                           on b.ItemDetailsId equals c.ItemDetailsId
                           join d in oConnectionContext.DbClsItem
                           on c.ItemId equals d.ItemId
                           where b.SalesId == obj.SalesId && b.IsDeleted == false
                           && d.ItemId == Item.ItemId && b.QuantityRemaining > 0
                           && b.IsComboItems == false
                           select new ClsItemDetailsVm
                           {
                               //ExtraDiscount =b.ExtraDiscount,
                               //PriceExcTax = b.PriceExcTax,
                               //AmountExcTax = 0,
                               IsManageStock = d.IsManageStock,
                               //FreeQuantity = 0,
                               //PurchaseReturnUnitCost = b.UnitCost,
                               QuantityRemaining = b.QuantityRemaining,
                               //SalesReturnUnitCost = b.PriceIncTax,
                               //SalesReturnAmount = 0,
                               DiscountType = b.DiscountType,
                               SalesDetailsId = b.SalesDetailsId,
                               //PriceIncTax= b.PriceIncTax,
                               //OtherInfo = b.OtherInfo,
                               //AmountIncTax = 0,
                               Discount = b.Discount,
                               Quantity = 0,
                               TaxId = b.TaxId,
                               InterStateTaxId = b.TaxId,
                               UnitCost = b.UnitCost,
                               ItemId = d.ItemId,
                               ProductType = d.ProductType,
                               ItemDetailsId = c.ItemDetailsId,
                               ItemName = d.ItemName,
                               SKU = c.SKU == null ? d.SkuCode : c.SKU,
                               VariationDetailsId = c.VariationDetailsId,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                               //c.SalesExcTax,
                               SalesExcTax = b.UnitCost,
                               SalesIncTax = c.SalesIncTax,
                               TotalCost = c.TotalCost,
                               Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                               TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                               TaxType = d.TaxType,
                               //ItemCode=d.ItemCode,
                               UnitId = d.UnitId,
                               SecondaryUnitId = d.SecondaryUnitId,
                               TertiaryUnitId = d.TertiaryUnitId,
                               QuaternaryUnitId = d.QuaternaryUnitId,
                               UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                               SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                               TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                               QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                               UToSValue = d.UToSValue,
                               SToTValue = d.SToTValue,
                               TToQValue = d.TToQValue,
                               AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                               SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                               TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                               QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                               PriceAddedFor = b.PriceAddedFor,
                               LotNo = b.LotTypeForLotNoChecking == "purchase" ?
                                    oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                    : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                                    : "Default Stock Accounting Method",
                               //LotManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ?
                               //     oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                               //     : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                               //     //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                               //     : null,
                               //LotExpiryDate = b.LotTypeForLotNoChecking == "purchase" ?
                               //     oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                               //     : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                               //     //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.LotId).FirstOrDefault()
                               //     : null,
                               //b.LotId,
                               //b.LotType,
                           }).ToList();

                if (det == null || det.Count == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Sorry this item is not associated with this Sales Invoice",
                        Data = new
                        {

                        }
                    };
                }
                else
                {
                    foreach (var _comboStock in det)
                    {
                        if (_comboStock.ProductType == "Combo")
                        {
                            var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == _comboStock.ItemId).Select(a => new
                            {
                                ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                IsManageStock = false,
                            }).ToList();

                            var itemsWithQty = combo.Select(item => new
                            {
                                IsManageStock = oConnectionContext.DbClsItem.Where(b => b.ItemId == item.ItemId).Select(b => b.IsManageStock).FirstOrDefault(),
                            }).ToList();

                            bool IsManageStock = itemsWithQty.Where(a => a.IsManageStock == true).Count() > 0 ? true : false;

                            if (IsManageStock == true)
                            {

                                _comboStock.IsManageStock = true;
                            }
                        }
                    }

                    if (Item.ItemDetailsId != 0)
                    {
                        det = det.Where(a => a.ItemDetailsId == Item.ItemDetailsId).ToList();
                    }

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            ItemDetails = det,
                            BusinessSetting = new
                            {
                                CountryId = BusinessSetting.CountryId,
                                StateId = BranchStateId
                            }
                        }
                    };
                }
            }
            else
            {
                var det = (from c in oConnectionContext.DbClsItemBranchMap
                           join a in oConnectionContext.DbClsItemDetails
                           on c.ItemDetailsId equals a.ItemDetailsId
                           join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                           where c.ItemId == Item.ItemId && a.ItemId == Item.ItemId
                           && c.BranchId == obj.BranchId
                           && c.IsActive == true && b.IsActive == true && b.IsDeleted == false
                           select new
                           {
                               ItemCodeId = b.ItemCodeId,
                               ItemType = b.ItemType,
                               WarrantyId = b.WarrantyId,
                               //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f => f.CompanyId == obj.CompanyId).Select(f => f.EnableWarranty).FirstOrDefault(),
                               DefaultProfitMargin = a.DefaultProfitMargin,
                               TaxType = b.TaxType,
                               IsManageStock = b.IsManageStock,
                               //ItemBranchMapId=c.ItemBranchMapId,
                               //Quantity = c.Quantity,
                               QuantityRemaining = c.Quantity,
                               ItemId = b.ItemId,
                               ProductType = b.ProductType,
                               ItemDetailsId = a.ItemDetailsId,
                               ItemName = b.ItemName,
                               SKU = a.SKU == null ? b.SkuCode : a.SKU,
                               VariationDetailsId = a.VariationDetailsId,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(d => d.VariationDetailsId == a.VariationDetailsId).Select(d => d.VariationDetails).FirstOrDefault(),
                               UnitId = b.UnitId,
                               SecondaryUnitId = b.SecondaryUnitId,
                               TertiaryUnitId = b.TertiaryUnitId,
                               QuaternaryUnitId = b.QuaternaryUnitId,
                               UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                               SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                               TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                               QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                               UToSValue = b.UToSValue,
                               SToTValue = b.SToTValue,
                               TToQValue = b.TToQValue,
                               PriceAddedFor = b.PriceAddedFor,
                               PurchaseExcTax = a.PurchaseExcTax,
                               PurchaseIncTax = a.PurchaseIncTax,
                               SalesExcTax = a.SalesExcTax,
                               //SalesIncTax = obj.MenuType == "sale" ? c.SalesIncTax + (customerGroupDiscountPercentage * c.SalesIncTax) : a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                               SalesIncTax = a.SalesIncTax,
                               TotalCost = a.TotalCost,
                               TaxId = b.TaxId,
                               Tax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.Tax).FirstOrDefault(),
                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                               InterStateTaxId = b.InterStateTaxId,
                               InterStateTax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.Tax).FirstOrDefault(),
                               InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                               TaxExemptionId = b.TaxExemptionId,
                               TaxPreferenceId = b.TaxPreferenceId,
                               AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                               SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                               TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                               QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                               //EnableLotNo = EnableLotNo,
                               EnableImei = b.EnableImei,
                               DefaultMrp = a.DefaultMrp
                           }).ToList();

                if (Item.ItemDetailsId != 0)
                {
                    det = det.Where(a => a.ItemDetailsId == Item.ItemDetailsId).ToList();
                }

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        ItemDetails = det,
                        BusinessSetting = new
                        {
                            CountryId = BusinessSetting.CountryId,
                            StateId = BranchStateId
                        }
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        #region Reward Points Processing Methods

        /// <summary>
        /// Process reward points reversal on sales return (proportional)
        /// </summary>
        private void ProcessRewardPointsOnReturn(long salesReturnId, long salesId, long customerId, long companyId, decimal returnAmount, DateTime currentDate, long addedBy)
        {
            var settings = oConnectionContext.DbClsRewardPointSettings
                .Where(a => a.CompanyId == companyId && a.EnableRewardPoint && !a.IsDeleted)
                .FirstOrDefault();

            if (settings == null) return;

            // Get original sale details
            var originalSale = oConnectionContext.DbClsSales
                .Where(a => a.SalesId == salesId && a.CompanyId == companyId && !a.IsDeleted)
                .Select(a => new
                {
                    a.GrandTotal,
                    a.PointsEarned,
                    a.RedeemPoints
                })
                .FirstOrDefault();

            if (originalSale == null) return;

            // Calculate return ratio
            decimal returnRatio = 0;
            if (originalSale.GrandTotal > 0)
            {
                returnRatio = returnAmount / originalSale.GrandTotal;
            }

            // Get customer record (reward points are now stored in tblUser)
            var customer = oConnectionContext.DbClsUser
                .Where(a => a.UserId == customerId && a.CompanyId == companyId && !a.IsDeleted)
                .FirstOrDefault();

            if (customer == null) return;

            // Reverse earned points proportionally
            if (originalSale.PointsEarned > 0)
            {
                decimal pointsToReverse = Math.Floor(originalSale.PointsEarned * returnRatio);
                if (pointsToReverse > 0)
                {
                    customer.AvailableRewardPoints -= pointsToReverse;
                    if (customer.AvailableRewardPoints < 0) customer.AvailableRewardPoints = 0;
                    customer.TotalRewardPointsEarned -= pointsToReverse;
                    if (customer.TotalRewardPointsEarned < 0) customer.TotalRewardPointsEarned = 0;

                    customer.ModifiedBy = addedBy;
                    customer.ModifiedOn = currentDate;

                    // Create reverse transaction
                    var reverseTransaction = new ClsRewardPointTransaction
                    {
                        CustomerId = customerId,
                        SalesId = salesId,
                        SalesReturnId = salesReturnId,
                        TransactionType = "Return Reversal",
                        Points = pointsToReverse,
                        OrderAmount = returnAmount,
                        TransactionDate = currentDate,
                        CompanyId = companyId,
                        Notes = $"Points reversed due to sales return (proportional)",
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = addedBy,
                        AddedOn = currentDate
                    };
                    oConnectionContext.DbClsRewardPointTransaction.Add(reverseTransaction);
                }
            }

            // Restore redeemed points proportionally
            if (originalSale.RedeemPoints > 0)
            {
                decimal pointsToRestore = Math.Floor(originalSale.RedeemPoints * returnRatio);
                if (pointsToRestore > 0)
                {
                    customer.AvailableRewardPoints += pointsToRestore;
                    customer.TotalRewardPointsRedeemed -= pointsToRestore;
                    if (customer.TotalRewardPointsRedeemed < 0) customer.TotalRewardPointsRedeemed = 0;

                    customer.ModifiedBy = addedBy;
                    customer.ModifiedOn = currentDate;

                    // Create restore transaction
                    var restoreTransaction = new ClsRewardPointTransaction
                    {
                        CustomerId = customerId,
                        SalesId = salesId,
                        SalesReturnId = salesReturnId,
                        TransactionType = "Return Restore",
                        Points = pointsToRestore,
                        OrderAmount = returnAmount,
                        TransactionDate = currentDate,
                        CompanyId = companyId,
                        Notes = $"Redeemed points restored due to sales return (proportional)",
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = addedBy,
                        AddedOn = currentDate
                    };
                    oConnectionContext.DbClsRewardPointTransaction.Add(restoreTransaction);
                }
            }

            oConnectionContext.SaveChanges();
        }

        /// <summary>
        /// Reverse reward points adjustments when sales return is deleted/cancelled
        /// </summary>
        private void ReverseRewardPointsOnReturnDelete(long salesReturnId, long companyId, DateTime currentDate, long modifiedBy)
        {
            // Get all reverse transactions for this return
            var reverseTransactions = oConnectionContext.DbClsRewardPointTransaction
                .Where(a => a.SalesReturnId == salesReturnId && a.CompanyId == companyId && !a.IsDeleted && (a.TransactionType == "Return Reversal" || a.TransactionType == "Return Restore"))
                .ToList();

            foreach (var txn in reverseTransactions)
            {
                var customer = oConnectionContext.DbClsUser
                    .Where(a => a.UserId == txn.CustomerId && a.CompanyId == companyId && !a.IsDeleted)
                    .FirstOrDefault();

                if (customer == null) continue;

                // Reverse the reversal (restore original state)
                if (txn.TransactionType == "Return Reversal")
                {
                    // Restore the points that were reversed
                    customer.AvailableRewardPoints += txn.Points;
                    customer.TotalRewardPointsEarned += txn.Points;
                }
                else if (txn.TransactionType == "Return Restore")
                {
                    // Remove the points that were restored
                    customer.AvailableRewardPoints -= txn.Points;
                    if (customer.AvailableRewardPoints < 0) customer.AvailableRewardPoints = 0;
                    customer.TotalRewardPointsRedeemed += txn.Points;
                }

                customer.ModifiedBy = modifiedBy;
                customer.ModifiedOn = currentDate;

                // Mark transaction as deleted
                txn.IsDeleted = true;
                txn.ModifiedBy = modifiedBy;
                txn.ModifiedOn = currentDate;
            }

            oConnectionContext.SaveChanges();
        }

        #endregion

    }
}
