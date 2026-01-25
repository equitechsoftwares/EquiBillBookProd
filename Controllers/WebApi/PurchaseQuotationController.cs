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

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class PurchaseQuotationController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();

        public async Task<IHttpActionResult> AllPurchaseQuotations(ClsPurchaseQuotationVm obj)
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

            List<ClsPurchaseQuotationVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
             //&& a.BranchId == obj.BranchId
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsPurchaseQuotationVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    NetAmountReverseCharge = a.NetAmountReverseCharge,
                    RoundOffReverseCharge = a.RoundOffReverseCharge,
                    GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                    TotalTaxAmount = a.TotalTaxAmount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    InvoiceUrl = oCommonController.webUrl,// + "/PurchaseQuotation/invoice?InvoiceNo=" + a.ReferenceNo+"&Id="+a.CompanyId,
                    TotalQuantity = a.TotalQuantity,
                    PaidQuantity = oConnectionContext.DbClsPurchaseQuotationDetails.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsPurchaseQuotationDetails.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PurchaseQuotationId = a.PurchaseQuotationId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    PurchaseQuotationDate = a.PurchaseQuotationDate,
                    Status = a.Status,
                    ReferenceNo = a.ReferenceNo,
                    Subtotal = a.Subtotal,
                    SupplierId = a.SupplierId,
                    SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                    SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                    ReferenceType = a.ReferenceType,
                    CanEdit = (oConnectionContext.DbClsPurchaseOrder.Where(c => c.IsDeleted == false && c.ReferenceId == a.PurchaseQuotationId && c.ReferenceType == "purchase quotation").Count() == 0 &&
                    oConnectionContext.DbClsPurchase.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.PurchaseQuotationId && c.ReferenceType == "purchase quotation").Count() == 0) ? true : false,
                    TotalItems = oConnectionContext.DbClsPurchaseQuotationDetails.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId &&
                    c.IsDeleted == false).Count()
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
             && a.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsPurchaseQuotationVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    NetAmountReverseCharge = a.NetAmountReverseCharge,
                    RoundOffReverseCharge = a.RoundOffReverseCharge,
                    GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                    TotalTaxAmount = a.TotalTaxAmount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    InvoiceUrl = oCommonController.webUrl,// + "/PurchaseQuotation/invoice?InvoiceNo=" + a.ReferenceNo+"&Id="+a.CompanyId,
                    TotalQuantity = a.TotalQuantity,
                    PaidQuantity = oConnectionContext.DbClsPurchaseQuotationDetails.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsPurchaseQuotationDetails.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PurchaseQuotationId = a.PurchaseQuotationId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    PurchaseQuotationDate = a.PurchaseQuotationDate,
                    Status = a.Status,
                    ReferenceNo = a.ReferenceNo,
                    Subtotal = a.Subtotal,
                    SupplierId = a.SupplierId,
                    SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                    SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                    CompanyId = a.CompanyId,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                    ReferenceType = a.ReferenceType,
                    CanEdit = (oConnectionContext.DbClsPurchaseOrder.Where(c => c.IsDeleted == false && c.ReferenceId == a.PurchaseQuotationId && c.ReferenceType == "purchase quotation").Count() == 0 &&
                    oConnectionContext.DbClsPurchase.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.PurchaseQuotationId && c.ReferenceType == "purchase quotation").Count() == 0) ? true : false,
                    TotalItems = oConnectionContext.DbClsPurchaseQuotationDetails.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId &&
                    c.IsDeleted == false).Count()
                }).ToList();
            }

            if (obj.Status != null && obj.Status != "")
            {
                det = det.Where(a => a.Status.ToLower() == obj.Status.ToLower()).Select(a => a).ToList();
            }
            if (obj.ReferenceNo != null && obj.ReferenceNo != "")
            {
                det = det.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower()).Select(a => a).ToList();
            }
            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseQuotations = det.OrderByDescending(a => a.PurchaseQuotationId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> PurchaseQuotation(ClsPurchaseQuotationVm obj)
        {
            var det = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == obj.PurchaseQuotationId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.TaxableAmount,
                NetAmountReverseCharge = a.NetAmountReverseCharge,
                RoundOffReverseCharge = a.RoundOffReverseCharge,
                GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                a.IsReverseCharge,
                BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                SourceOfSupplyId = a.SourceOfSupplyId,
                DestinationOfSupplyId = a.DestinationOfSupplyId,
                a.TotalTaxAmount,
                a.InvoiceId,
                a.ExchangeRate,
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.SupplierId).Select(e => e.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                DefaultCurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(c => c.IsMain == true && c.CompanyId == obj.CompanyId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                a.TotalDiscount,
                AttachDocument = a.AttachDocument,
                a.AddedBy,
                a.AddedOn,
                a.BranchId,
                a.CompanyId,
                a.DeliveredTo,
                a.Discount,
                a.DiscountType,
                a.GrandTotal,
                a.IsActive,
                a.IsDeleted,
                a.Notes,
                a.PayTerm,
                a.PayTermNo,
                a.PurchaseQuotationDate,
                a.PurchaseQuotationId,
                a.Status,
                a.ReferenceNo,
                a.ShippingAddress,
                a.ShippingDetails,
                a.ShippingStatus,
                a.Subtotal,
                a.SupplierId,
                a.TaxId,
                TaxAmount = a.TaxAmount,
                a.TotalQuantity,
                a.SmsSettingsId,
                a.EmailSettingsId,
                a.WhatsappSettingsId,
                a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                a.NetAmount,
                Supplier = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.GstTreatment).FirstOrDefault(),
                Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                ReferenceType = a.ReferenceType,
                PurchaseQuotationDetails = (from b in oConnectionContext.DbClsPurchaseQuotationDetails
                                            join c in oConnectionContext.DbClsItemDetails
                                            on b.ItemDetailsId equals c.ItemDetailsId
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where b.PurchaseQuotationId == a.PurchaseQuotationId && b.IsDeleted == false
                                            select new
                                            {
                                                b.TotalTaxAmount,
                                                b.ITCType,
                                                b.TaxExemptionId,
                                                ItemCodeId = b.ItemCodeId,
                                                ItemType = d.ItemType,
                                                b.ExtraDiscount,
                                                Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                             : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                                b.FreeQuantity,
                                                //b.FreeQuantityPriceAddedFor,
                                                b.PurchaseExcTax,
                                                b.PurchaseIncTax,
                                                b.LotNo,
                                                b.ExpiryDate,
                                                b.ManufacturingDate,
                                                b.QuantitySold,
                                                b.QuantityRemaining,
                                                b.PurchaseQuotationDetailsId,
                                                b.AmountExcTax,
                                                b.AmountIncTax,
                                                b.Discount,
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
                                                b.SalesExcTax,
                                                b.SalesIncTax,
                                                c.TotalCost,
                                                Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                                TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                                d.TaxType,
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
                                                AllowDecimal = oConnectionContext.DbClsUnit.Where(c => c.UnitId == d.UnitId).Select(c => c.AllowDecimal).FirstOrDefault(),
                                                SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == d.SecondaryUnitId).Select(c => c.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                                TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == d.TertiaryUnitId).Select(c => c.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                                QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == d.QuaternaryUnitId).Select(c => c.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                                PriceAddedFor = b.PriceAddedFor,
                                                b.TaxAmount,
                                                b.DiscountType,
                                                b.DefaultProfitMargin,
                                                b.Mrp
                                            }).ToList(),
                PurchaseQuotationAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                    ).Select(b => new ClsPurchaseQuotationAdditionalChargesVm
                    {
                        ITCType = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ITCType).FirstOrDefault(),
                        PurchaseQuotationAdditionalChargesId = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.PurchaseQuotationAdditionalChargesId).FirstOrDefault(),
                        Name = b.Name,
                        AdditionalChargeId = b.AdditionalChargeId,
                        PurchaseQuotationId = a.PurchaseQuotationId,
                        TaxId = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                        AmountExcTax = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                        AmountIncTax = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                        TaxAmount = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                        AccountId = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(c => c.PurchaseQuotationId == a.PurchaseQuotationId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                        ItemCodeId = b.ItemCodeId,
                        TaxExemptionId = b.TaxExemptionId,
                        TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                    }).ToList(),
            }).FirstOrDefault();

            var AllTaxs = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.IsDeleted == false && a.PurchaseQuotationId == det.PurchaseQuotationId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationId == det.PurchaseQuotationId && a.IsDeleted == false).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.AmountExcTax
            })).Concat(oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(a => a.PurchaseQuotationId == det.PurchaseQuotationId
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
                    PurchaseQuotation = det,
                    Taxs = finalTaxs,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPurchaseQuotation(ClsPurchaseQuotationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;

                if (obj.SupplierId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
                    isError = true;
                }

                if (obj.PurchaseQuotationDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseQuotationDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseQuotationStatus" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsPurchaseQuotation.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Purchase Quotation# exists", Id = "divPurchaseQuotationReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.PurchaseQuotationDetails == null || obj.PurchaseQuotationDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays, a.EnableLotNo }).FirstOrDefault();

                if (obj.PurchaseQuotationDetails != null)
                {
                    foreach (var item in obj.PurchaseQuotationDetails)
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
                            if (ItemSetting.EnableLotNo == true)
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == item.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (item.LotNo == "" || item.LotNo == null)
                                    {
                                        errors.Add(new ClsError { Message = "", Id = "divLotNo" + item.DivId });
                                        isError = true;
                                    }
                                }
                            }
                            if (item.SalesExcTax <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divSalesExcTax" + item.DivId });
                                isError = true;
                            }
                        }
                    }
                }

                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                if (obj.SupplierId != 0)
                {
                    if (obj.CountryId == 2)
                    {
                        if (userDet.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }

                        if (obj.DestinationOfSupplyId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDestinationOfSupply" });
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

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    // Hybrid approach: Check Supplier PrefixId first, then fall back to Branch PrefixId
                    long supplierPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => a.PrefixId).FirstOrDefault();
                    
                    if (supplierPrefixId != 0)
                    {
                        // Use Supplier's PrefixId if set
                        PrefixId = supplierPrefixId;
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
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "purchase quotation"
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

                ClsPurchaseQuotation oClsPurchaseQuotation = new ClsPurchaseQuotation()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    TotalDiscount = obj.TotalDiscount,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    BranchId = obj.BranchId,
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
                    PurchaseQuotationDate = obj.PurchaseQuotationDate.AddHours(5).AddMinutes(30),
                    PurchaseQuotationId = obj.PurchaseQuotationId,
                    Status = obj.Status,
                    ReferenceNo = obj.ReferenceNo,
                    ShippingAddress = obj.ShippingAddress,
                    ShippingDetails = obj.ShippingDetails,
                    ShippingStatus = obj.ShippingStatus,
                    Subtotal = obj.Subtotal,
                    SupplierId = obj.SupplierId,
                    TaxId = obj.TaxId,
                    TaxAmount = obj.TaxAmount,
                    TotalQuantity = obj.TotalQuantity,
                    //PaymentStatus = obj.PaymentStatus,
                    ExchangeRate = obj.ExchangeRate,
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    InvoiceId = oCommonController.CreateToken(),//DateTime.Now.ToFileTime(),
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    ReferenceId = obj.ReferenceId,
                    ReferenceType = obj.ReferenceType,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    IsReverseCharge = obj.IsReverseCharge,
                    PrefixId = PrefixId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/PurchaseQuotation/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/PurchaseQuotation/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPurchaseQuotation.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsPurchaseQuotation.Add(oClsPurchaseQuotation);
                oConnectionContext.SaveChanges();

                if (obj.PurchaseQuotationAdditionalCharges != null)
                {
                    foreach (var item in obj.PurchaseQuotationAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.PurchaseAccountId }).FirstOrDefault();

                        ClsPurchaseQuotationAdditionalCharges oClsPurchaseQuotationAdditionalCharges = new ClsPurchaseQuotationAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                            TaxId = item.TaxId,
                            AmountExcTax = item.AmountExcTax,
                            AmountIncTax = item.AmountIncTax,
                            TaxAmount = item.AmountIncTax - item.AmountExcTax,
                            AccountId = AdditionalCharge.PurchaseAccountId,
                            ItemCodeId = AdditionalCharge.ItemCodeId,
                            TaxExemptionId = item.TaxExemptionId,
                            IsActive = item.IsActive,
                            IsDeleted = item.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Add(oClsPurchaseQuotationAdditionalCharges);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.PurchaseQuotationDetails != null)
                {
                    foreach (var PurchaseQuotation in obj.PurchaseQuotationDetails)
                    {
                        bool IsStopSelling = false, flag = false;
                        decimal convertedStock = oCommonController.StockConversion(PurchaseQuotation.Quantity, PurchaseQuotation.ItemId, PurchaseQuotation.PriceAddedFor);
                        decimal freeConvertedStock = oCommonController.StockConversion(PurchaseQuotation.FreeQuantity, PurchaseQuotation.ItemId, PurchaseQuotation.PriceAddedFor);

                        if (PurchaseQuotation.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                        {
                            if ((PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                            {
                                IsStopSelling = true;
                            }
                        }
                        //long ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseQuotation.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                        PurchaseQuotation.DefaultProfitMargin = ((PurchaseQuotation.SalesExcTax - PurchaseQuotation.UnitCost) / PurchaseQuotation.UnitCost) * 100;

                        ClsPurchaseQuotationDetails oClsPurchaseQuotationDetails = new ClsPurchaseQuotationDetails()
                        {
                            AmountExcTax = PurchaseQuotation.AmountExcTax,
                            AmountIncTax = PurchaseQuotation.AmountIncTax,
                            ItemId = PurchaseQuotation.ItemId,
                            ItemDetailsId = PurchaseQuotation.ItemDetailsId,
                            PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                            PurchaseExcTax = PurchaseQuotation.PurchaseExcTax,
                            PurchaseIncTax = PurchaseQuotation.PurchaseIncTax,
                            //Tax = PurchaseQuotation.Tax,
                            TaxId = PurchaseQuotation.TaxId,
                            Discount = PurchaseQuotation.Discount,
                            Quantity = PurchaseQuotation.Quantity,
                            UnitCost = PurchaseQuotation.UnitCost,
                            IsActive = PurchaseQuotation.IsActive,
                            IsDeleted = PurchaseQuotation.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            QuantityRemaining = convertedStock + freeConvertedStock,
                            QuantitySold = 0,
                            LotNo = PurchaseQuotation.LotNo,
                            ExpiryDate = PurchaseQuotation.ExpiryDate != null ? PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) : PurchaseQuotation.ExpiryDate,
                            ManufacturingDate = PurchaseQuotation.ManufacturingDate != null ? PurchaseQuotation.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : PurchaseQuotation.ManufacturingDate,
                            PriceAddedFor = PurchaseQuotation.PriceAddedFor,
                            SalesExcTax = PurchaseQuotation.SalesExcTax,
                            SalesIncTax = PurchaseQuotation.SalesIncTax,
                            FreeQuantity = PurchaseQuotation.FreeQuantity,
                            //FreeQuantityPriceAddedFor = PurchaseQuotation.FreeQuantityPriceAddedFor,
                            IsStopSelling = IsStopSelling,
                            TaxAmount = PurchaseQuotation.TaxAmount,
                            DiscountType = PurchaseQuotation.DiscountType,
                            DefaultProfitMargin = PurchaseQuotation.DefaultProfitMargin,
                            UnitAddedFor = PurchaseQuotation.UnitAddedFor,
                            QuantityPurchased = convertedStock + freeConvertedStock,
                            Mrp = PurchaseQuotation.Mrp,
                            ExtraDiscount = PurchaseQuotation.ExtraDiscount,
                            ItemCodeId = PurchaseQuotation.ItemCodeId,
                            ITCType = PurchaseQuotation.ITCType,
                            TaxExemptionId = PurchaseQuotation.TaxExemptionId,
                            TotalTaxAmount = PurchaseQuotation.TotalTaxAmount,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsPurchaseQuotationDetails.Add(oClsPurchaseQuotationDetails);
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
                    Category = "Purchase Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Quotation \"" + obj.ReferenceNo + "\" created",
                    Id = oClsPurchaseQuotation.PurchaseQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Quotation", obj.CompanyId, oClsPurchaseQuotation.PurchaseQuotationId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Quotation created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        PurchaseQuotation = new
                        {
                            PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                            InvoiceId = oClsPurchaseQuotation.InvoiceId
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseQuotation }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseQuotationDelete(ClsPurchaseQuotationVm obj)
        {
            int PurchaseOrderCount = (from a in oConnectionContext.DbClsPurchaseOrder
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                       && a.ReferenceId == obj.PurchaseQuotationId && a.ReferenceType == "purchase quotation"
                                      select a.PurchaseOrderId).Count();

            int PurchaseCount = (from a in oConnectionContext.DbClsPurchase
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                               && a.ReferenceId == obj.PurchaseQuotationId && a.ReferenceType == "purchase quotation"
                                 select a.PurchaseId).Count();

            if (PurchaseOrderCount > 0 || PurchaseCount > 0)
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
                ClsPurchaseQuotation oClsPurchaseQuotation = new ClsPurchaseQuotation()
                {
                    PurchaseQuotationId = obj.PurchaseQuotationId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchaseQuotation.Attach(oClsPurchaseQuotation);
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PurchaseQuotationId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "PurchaseQuotation",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Quotation \"" + oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == obj.PurchaseQuotationId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPurchaseQuotation.PurchaseQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Quotation deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchaseQuotation(ClsPurchaseQuotationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.SupplierId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
                    isError = true;
                }

                if (obj.PurchaseQuotationDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseQuotationDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseQuotationStatus" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.PurchaseQuotationDetails == null || obj.PurchaseQuotationDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays, a.EnableLotNo }).FirstOrDefault();

                if (obj.PurchaseQuotationDetails != null)
                {
                    foreach (var item in obj.PurchaseQuotationDetails)
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
                            if (ItemSetting.EnableLotNo == true)
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == item.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (item.LotNo == "" || item.LotNo == null)
                                    {
                                        errors.Add(new ClsError { Message = "", Id = "divLotNo" + item.DivId });
                                        isError = true;
                                    }
                                }
                            }
                            if (item.SalesExcTax <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divSalesExcTax" + item.DivId });
                                isError = true;
                            }
                        }
                    }
                }

                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                if (obj.SupplierId != 0)
                {
                    if (obj.CountryId == 2)
                    {
                        if (userDet.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }

                        if (obj.DestinationOfSupplyId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDestinationOfSupply" });
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

                ClsPurchaseQuotation oClsPurchaseQuotation = new ClsPurchaseQuotation()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    TotalDiscount = obj.TotalDiscount,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    BranchId = obj.BranchId,
                    DeliveredTo = obj.DeliveredTo,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    Notes = obj.Notes,
                    PayTerm = obj.PayTerm,
                    PayTermNo = obj.PayTermNo,
                    PurchaseQuotationDate = obj.PurchaseQuotationDate.AddHours(5).AddMinutes(30),
                    PurchaseQuotationId = obj.PurchaseQuotationId,
                    Status = obj.Status,
                    //ReferenceNo = obj.ReferenceNo,
                    ShippingAddress = obj.ShippingAddress,
                    ShippingDetails = obj.ShippingDetails,
                    ShippingStatus = obj.ShippingStatus,
                    Subtotal = obj.Subtotal,
                    SupplierId = obj.SupplierId,
                    TaxId = obj.TaxId,
                    TaxAmount = obj.TaxAmount,
                    TotalQuantity = obj.TotalQuantity,
                    ExchangeRate = obj.ExchangeRate,
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    //PaymentStatus = obj.PaymentStatus,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    IsReverseCharge = obj.IsReverseCharge,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                };

                string pic1 = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == obj.PurchaseQuotationId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/PurchaseQuotation/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/PurchaseQuotation/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPurchaseQuotation.AttachDocument = filepathPass;
                }
                else
                {
                    oClsPurchaseQuotation.AttachDocument = pic1;
                }

                oConnectionContext.DbClsPurchaseQuotation.Attach(oClsPurchaseQuotation);
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.DeliveredTo).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PayTerm).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PayTermNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PurchaseQuotationDate).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PurchaseQuotationId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.Status).IsModified = true;
                //oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ReferenceNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ShippingAddress).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ShippingDetails).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ShippingStatus).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.SupplierId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.NetAmount).IsModified = true;
                //oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PaymentStatus).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.SourceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.DestinationOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.IsBusinessRegistered).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.GstTreatment).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.BusinessRegistrationNameId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.BusinessRegistrationNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.BusinessLegalName).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.BusinessTradeName).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PanNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.IsReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.PurchaseQuotationAdditionalCharges != null)
                {
                    foreach (var item in obj.PurchaseQuotationAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.PurchaseAccountId }).FirstOrDefault();

                        if (item.PurchaseQuotationAdditionalChargesId == 0)
                        {
                            ClsPurchaseQuotationAdditionalCharges oClsPurchaseQuotationAdditionalCharges = new ClsPurchaseQuotationAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.PurchaseAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                TaxExemptionId = item.TaxExemptionId,
                                IsActive = item.IsActive,
                                IsDeleted = item.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId
                            };
                            oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Add(oClsPurchaseQuotationAdditionalCharges);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsPurchaseQuotationAdditionalCharges oClsPurchaseQuotationAdditionalCharges = new ClsPurchaseQuotationAdditionalCharges()
                            {
                                PurchaseQuotationAdditionalChargesId = item.PurchaseQuotationAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.PurchaseAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                TaxExemptionId = item.TaxExemptionId,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Attach(oClsPurchaseQuotationAdditionalCharges);
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.PurchaseQuotationId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseQuotationAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.PurchaseQuotationDetails != null)
                {
                    foreach (var PurchaseQuotation in obj.PurchaseQuotationDetails)
                    {
                        if (PurchaseQuotation.IsDeleted == true)
                        {
                            string query = "update \"tblPurchaseQuotationDetails\" set \"IsDeleted\"=True where \"PurchaseQuotationDetailsId\"=" + PurchaseQuotation.PurchaseQuotationDetailsId;
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        else
                        {
                            bool IsStopSelling = false, flag = false;
                            decimal convertedStock = oCommonController.StockConversion(PurchaseQuotation.Quantity, PurchaseQuotation.ItemId, PurchaseQuotation.PriceAddedFor);
                            decimal freeConvertedStock = oCommonController.StockConversion(PurchaseQuotation.FreeQuantity, PurchaseQuotation.ItemId, PurchaseQuotation.PriceAddedFor);

                            PurchaseQuotation.PurchaseQuotationDetailsId = oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.CompanyId == obj.CompanyId
                            && a.IsDeleted == false && a.PurchaseQuotationId == obj.PurchaseQuotationId && a.ItemId == PurchaseQuotation.ItemId
                            && a.ItemDetailsId == PurchaseQuotation.ItemDetailsId).Select(a => a.PurchaseQuotationDetailsId).FirstOrDefault();

                            if (PurchaseQuotation.PurchaseQuotationDetailsId == 0)
                            {
                                if (PurchaseQuotation.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                                {
                                    if ((PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                    {
                                        IsStopSelling = true;
                                    }
                                }

                                PurchaseQuotation.DefaultProfitMargin = ((PurchaseQuotation.SalesExcTax - PurchaseQuotation.UnitCost) / PurchaseQuotation.UnitCost) * 100;

                                ClsPurchaseQuotationDetails oClsPurchaseQuotationDetails = new ClsPurchaseQuotationDetails()
                                {
                                    AmountExcTax = PurchaseQuotation.AmountExcTax,
                                    AmountIncTax = PurchaseQuotation.AmountIncTax,
                                    ItemDetailsId = PurchaseQuotation.ItemDetailsId,
                                    PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                                    PurchaseExcTax = PurchaseQuotation.PurchaseExcTax,
                                    PurchaseIncTax = PurchaseQuotation.PurchaseIncTax,
                                    //Tax = PurchaseQuotation.Tax,
                                    TaxId = PurchaseQuotation.TaxId,
                                    Discount = PurchaseQuotation.Discount,
                                    Quantity = PurchaseQuotation.Quantity,
                                    UnitCost = PurchaseQuotation.UnitCost,
                                    IsActive = PurchaseQuotation.IsActive,
                                    IsDeleted = PurchaseQuotation.IsDeleted,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    QuantityRemaining = convertedStock + freeConvertedStock,
                                    QuantitySold = 0,
                                    LotNo = PurchaseQuotation.LotNo,
                                    ExpiryDate = PurchaseQuotation.ExpiryDate != null ? PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) : PurchaseQuotation.ExpiryDate,
                                    ManufacturingDate = PurchaseQuotation.ManufacturingDate != null ? PurchaseQuotation.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : PurchaseQuotation.ManufacturingDate,
                                    PriceAddedFor = PurchaseQuotation.PriceAddedFor,
                                    SalesExcTax = PurchaseQuotation.SalesExcTax,
                                    SalesIncTax = PurchaseQuotation.SalesIncTax,
                                    FreeQuantity = PurchaseQuotation.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = PurchaseQuotation.FreeQuantityPriceAddedFor,
                                    IsStopSelling = IsStopSelling,
                                    TaxAmount = PurchaseQuotation.TaxAmount,
                                    DiscountType = PurchaseQuotation.DiscountType,
                                    DefaultProfitMargin = PurchaseQuotation.DefaultProfitMargin,
                                    UnitAddedFor = PurchaseQuotation.UnitAddedFor,
                                    QuantityPurchased = convertedStock + freeConvertedStock,
                                    Mrp = PurchaseQuotation.Mrp,
                                    ItemId = PurchaseQuotation.ItemId,
                                    ExtraDiscount = PurchaseQuotation.ExtraDiscount,
                                    ItemCodeId = PurchaseQuotation.ItemCodeId,
                                    ITCType = PurchaseQuotation.ITCType,
                                    TaxExemptionId = PurchaseQuotation.TaxExemptionId,
                                    TotalTaxAmount = PurchaseQuotation.TotalTaxAmount,
                                };

                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsPurchaseQuotationDetails.Add(oClsPurchaseQuotationDetails);
                                oConnectionContext.SaveChanges();

                                //if (IsStopSelling == false)
                                //{
                                //    if (obj.Status.ToLower() != "draft")
                                //    {
                                //        string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity = Quantity,0)+" + (convertedStock + freeConvertedStock) + " where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                //        oConnectionContext.Database.ExecuteSqlCommand(query);
                                //    }
                                //}
                            }
                            else
                            {
                                decimal QuantityOut = oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationDetailsId == PurchaseQuotation.PurchaseQuotationDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                                //decimal QuantityReturned = (from b in oConnectionContext.DbClsPurchaseQuotationReturnDetails
                                //                            where b.PurchaseQuotationDetailsId == PurchaseQuotation.PurchaseQuotationDetailsId && b.ItemId == PurchaseQuotation.ItemId &&
                                //                            b.ItemDetailsId == PurchaseQuotation.ItemDetailsId
                                //                            select b.QuantityRemaining).FirstOrDefault();

                                bool previousIsStopSelling = oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationDetailsId == PurchaseQuotation.PurchaseQuotationDetailsId).Select(a => a.IsStopSelling).FirstOrDefault();
                                if (previousIsStopSelling == true)
                                {
                                    if (ItemSetting.OnItemExpiry == 1)
                                    {
                                        flag = true;
                                        IsStopSelling = false;
                                        //if (obj.Status.ToLower() != "draft")
                                        //{
                                        //    string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity = Quantity,0)+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                        //}
                                    }
                                    else
                                    {
                                        if ((PurchaseQuotation.ExpiryDate != null))
                                        {
                                            if ((PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days > ItemSetting.StopSellingBeforeDays)
                                            {
                                                flag = true;
                                                IsStopSelling = false;
                                                //if (obj.Status.ToLower() != "draft")
                                                //{
                                                //    string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity = Quantity,0)+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                                //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                //}
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
                                            //if (obj.Status.ToLower() != "draft")
                                            //{
                                            //    string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity = Quantity,0)+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                            //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                            //}
                                        }
                                    }
                                }
                                else
                                {
                                    if (ItemSetting.OnItemExpiry != 1)
                                    {
                                        if ((PurchaseQuotation.ExpiryDate != null))
                                        {
                                            if ((PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                            {
                                                flag = true;
                                                IsStopSelling = true;
                                                //if (obj.Status.ToLower() != "draft")
                                                //{
                                                //    string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity = Quantity,0)-(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                                //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                                //}
                                            }
                                        }
                                    }
                                }

                                if (flag == false)
                                {
                                    decimal Quantity = oCommonController.StockConversion(oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationDetailsId == PurchaseQuotation.PurchaseQuotationDetailsId).Select(a => a.Quantity + a.FreeQuantity).FirstOrDefault(), PurchaseQuotation.ItemId, oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationDetailsId == PurchaseQuotation.PurchaseQuotationDetailsId).Select(a => a.PriceAddedFor).FirstOrDefault());

                                    //if (PrevPurchaseQuotation.Status.ToLower() != "draft")
                                    //{
                                    //    string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity = Quantity,0)-(" + (Quantity - (QuantityOut + QuantityReturned)) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                    //}

                                    //if (obj.Status.ToLower() != "draft")
                                    //{
                                    //    string query = "update tblItemBranchMap set SalesIncTax=" + PurchaseQuotation.SalesIncTax + ",Quantity =Quantity,0)+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + PurchaseQuotation.ItemDetailsId;
                                    //    oConnectionContext.Database.ExecuteSqlCommand(query);
                                    //}
                                }

                                PurchaseQuotation.DefaultProfitMargin = ((PurchaseQuotation.SalesExcTax - PurchaseQuotation.UnitCost) / PurchaseQuotation.UnitCost) * 100;

                                ClsPurchaseQuotationDetails oClsPurchaseQuotationDetails = new ClsPurchaseQuotationDetails()
                                {
                                    PurchaseQuotationDetailsId = PurchaseQuotation.PurchaseQuotationDetailsId,
                                    AmountExcTax = PurchaseQuotation.AmountExcTax,
                                    AmountIncTax = PurchaseQuotation.AmountIncTax,
                                    ItemId = PurchaseQuotation.ItemId,
                                    ItemDetailsId = PurchaseQuotation.ItemDetailsId,
                                    PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                                    PurchaseExcTax = PurchaseQuotation.PurchaseExcTax,
                                    PurchaseIncTax = PurchaseQuotation.PurchaseIncTax,
                                    //Tax = PurchaseQuotation.Tax,
                                    TaxId = PurchaseQuotation.TaxId,
                                    Discount = PurchaseQuotation.Discount,
                                    Quantity = PurchaseQuotation.Quantity,
                                    UnitCost = PurchaseQuotation.UnitCost,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityOut,
                                    LotNo = PurchaseQuotation.LotNo,
                                    ExpiryDate = PurchaseQuotation.ExpiryDate != null ? PurchaseQuotation.ExpiryDate.Value.AddHours(5).AddMinutes(30) : PurchaseQuotation.ExpiryDate,
                                    ManufacturingDate = PurchaseQuotation.ManufacturingDate != null ? PurchaseQuotation.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : PurchaseQuotation.ManufacturingDate,
                                    PriceAddedFor = PurchaseQuotation.PriceAddedFor,
                                    SalesExcTax = PurchaseQuotation.SalesExcTax,
                                    SalesIncTax = PurchaseQuotation.SalesIncTax,
                                    FreeQuantity = PurchaseQuotation.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = PurchaseQuotation.FreeQuantityPriceAddedFor,
                                    IsStopSelling = IsStopSelling,
                                    TaxAmount = PurchaseQuotation.TaxAmount,
                                    DiscountType = PurchaseQuotation.DiscountType,
                                    DefaultProfitMargin = PurchaseQuotation.DefaultProfitMargin,
                                    UnitAddedFor = PurchaseQuotation.UnitAddedFor,
                                    QuantityPurchased = convertedStock + freeConvertedStock,
                                    Mrp = PurchaseQuotation.Mrp,
                                    ExtraDiscount = PurchaseQuotation.ExtraDiscount,
                                    ItemCodeId = PurchaseQuotation.ItemCodeId,
                                    ITCType = PurchaseQuotation.ITCType,
                                    TaxExemptionId = PurchaseQuotation.TaxExemptionId,
                                    TotalTaxAmount = PurchaseQuotation.TotalTaxAmount
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsPurchaseQuotationDetails.Attach(oClsPurchaseQuotationDetails);
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.AmountExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.AmountIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ItemId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ItemDetailsId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.PurchaseQuotationId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.PurchaseExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.PurchaseIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.TaxId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.Discount).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.Quantity).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.UnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.QuantityRemaining).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.LotNo).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ExpiryDate).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ManufacturingDate).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.PriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.SalesExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.SalesIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.FreeQuantity).IsModified = true;
                                //oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.IsStopSelling).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.TaxAmount).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.DiscountType).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.DefaultProfitMargin).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.UnitAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.QuantityPurchased).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.Mrp).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ExtraDiscount).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ItemCodeId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.ITCType).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.TaxExemptionId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseQuotationDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }

                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Quotation \"" + obj.ReferenceNo + "\" updated",
                    Id = oClsPurchaseQuotation.PurchaseQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Quotation", obj.CompanyId, oClsPurchaseQuotation.PurchaseQuotationId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Quotation updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        PurchaseQuotation = new
                        {
                            PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                            InvoiceId = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == oClsPurchaseQuotation.PurchaseQuotationId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseQuotation }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseQuotationDetailsDelete(ClsPurchaseQuotationDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.PurchaseQuotationId != 0)
                {
                    obj.BranchId = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == obj.PurchaseQuotationId).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationId == obj.PurchaseQuotationId && a.IsDeleted == false).Select(a => new
                    {
                        a.IsStopSelling,
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
                }
                else
                {
                    obj.BranchId = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == oConnectionContext.DbClsPurchaseQuotationDetails.Where(b => b.PurchaseQuotationDetailsId == obj.PurchaseQuotationDetailsId).Select(b => b.PurchaseQuotationId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationDetailsId == obj.PurchaseQuotationDetailsId).Select(a => new
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
        public async Task<IHttpActionResult> Invoice(ClsPurchaseQuotationVm obj)
        {
            var det = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.IsDeleted == false && a.InvoiceId == obj.InvoiceId).Select(a => new
            {
                a.PurchaseQuotationId,
                User = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => new
                {
                    c.Name,
                    c.MobileNo,
                    c.EmailId,
                    //c.TaxNo,
                    //Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == c.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                    TaxNo = c.BusinessRegistrationNo,
                    Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == a.SupplierId).Select(b => new
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
                Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => new
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
                a.Notes,
                a.ReferenceNo,
                a.PurchaseQuotationDate,
                a.Subtotal,
                a.Discount,
                a.DiscountType,
                a.TotalDiscount,
                a.GrandTotal,
                a.Status,
                Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.Tax).FirstOrDefault(),
                TaxAmount = a.TaxAmount,
                a.TotalQuantity,
                a.CompanyId,
                //a.PaymentStatus,
                a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                a.NetAmount,
                PurchaseQuotationDetails = (from b in oConnectionContext.DbClsPurchaseQuotationDetails
                                            join c in oConnectionContext.DbClsItemDetails
                                            on b.ItemDetailsId equals c.ItemDetailsId
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where b.PurchaseQuotationId == a.PurchaseQuotationId && b.IsDeleted == false
                                            select new
                                            {
                                                d.ProductImage,
                                                b.TotalTaxAmount,
                                                b.PurchaseExcTax,
                                                b.PurchaseIncTax,
                                                Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                             : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                             : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                                //b.DiscountType,
                                                //b.SalesDetailsId,
                                                //b.PriceIncTax,
                                                //b.OtherInfo,
                                                b.AmountExcTax,
                                                b.AmountIncTax,
                                                b.Discount,
                                                b.PurchaseQuotationId,
                                                b.Quantity,
                                                b.FreeQuantity,
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
                                                d.ItemCode
                                            }).ToList(),
                PurchaseQuotationAdditionalCharges = oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(b => b.PurchaseQuotationId == a.PurchaseQuotationId
                    && b.IsDeleted == false && b.IsActive == true).Select(b => new ClsPurchaseQuotationAdditionalChargesVm
                    {
                        PurchaseQuotationAdditionalChargesId = b.PurchaseQuotationAdditionalChargesId,
                        Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
                        AdditionalChargeId = b.AdditionalChargeId,
                        PurchaseQuotationId = b.PurchaseQuotationId,
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

            var AllTaxs = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.IsDeleted == false && a.PurchaseQuotationId == det.PurchaseQuotationId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsPurchaseQuotationDetails.Where(a => a.PurchaseQuotationId == det.PurchaseQuotationId && a.IsDeleted == false).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.AmountExcTax
            })).Concat(oConnectionContext.DbClsPurchaseQuotationAdditionalCharges.Where(a => a.PurchaseQuotationId == det.PurchaseQuotationId
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
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
            }).FirstOrDefault();

            var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                a.EnableMrp,
                a.EnableItemExpiry,
                a.ExpiryType,
                a.ExpiryDateFormat
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseQuotation = det,
                    BusinessSetting = BusinessSetting,
                    Taxs = finalTaxs,
                    ItemSetting = ItemSetting
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchaseQuotationStatus(ClsPurchaseQuotationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "Purchase Quotation Status is required", Id = "divPurchaseQuotationStatus" });
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

                ClsPurchaseQuotation oClsPurchaseQuotation = new ClsPurchaseQuotation()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    PurchaseQuotationId = obj.PurchaseQuotationId,
                    Status = obj.Status,
                };

                oConnectionContext.DbClsPurchaseQuotation.Attach(oClsPurchaseQuotation);
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.PurchaseQuotationId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseQuotation).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Quotation",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Quotation \"" + oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == obj.PurchaseQuotationId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" status changed to \"" + obj.Status + "\"",
                    Id = oClsPurchaseQuotation.PurchaseQuotationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Quotation", obj.CompanyId, oClsPurchaseQuotation.PurchaseQuotationId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Quotation status changed successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        PurchaseQuotation = new
                        {
                            PurchaseQuotationId = oClsPurchaseQuotation.PurchaseQuotationId,
                            InvoiceId = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.PurchaseQuotationId == oClsPurchaseQuotation.PurchaseQuotationId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseQuotation }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
