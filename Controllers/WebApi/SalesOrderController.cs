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
    public class SalesOrderController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllSalesOrders(ClsSalesOrderVm obj)
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
            List<ClsSalesOrderVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsSalesOrderVm
                {
                    IsInvoiced = a.IsInvoiced,
                    TotalTaxAmount = a.TotalTaxAmount,
                    TotalDiscount = a.TotalDiscount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    Status = a.Status,
                    InvoiceUrl = oCommonController.webUrl,//+ "/SalesOrders/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    SalesOrderId = a.SalesOrderId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    SalesOrderDate = a.SalesOrderDate,
                    //SalesOrderType = a.SalesOrderType,
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
                    TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesOrderDetails.Where(c=>c.SalesOrderId==a.SalesOrderId && c.IsDeleted==false).Count()
                    PaidQuantity = oConnectionContext.DbClsSalesOrderDetails.Where(c => c.SalesOrderId == a.SalesOrderId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsSalesOrderDetails.Where(c => c.SalesOrderId == a.SalesOrderId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                    ReferenceType = a.ReferenceType,
                    CanEdit = (oConnectionContext.DbClsSalesProforma.Where(c => c.IsDeleted == false && c.ReferenceId == a.SalesOrderId && c.ReferenceType == "sales order").Count() == 0 &&
                    oConnectionContext.DbClsSales.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesOrderId && c.ReferenceType == "sales order").Count() == 0 &&
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesOrderId && c.ReferenceType == "sales order").Count() == 0) ? true : false,
                    TotalItems = oConnectionContext.DbClsSalesOrderDetails.Where(c => c.SalesOrderId == a.SalesOrderId &&
                    c.IsDeleted == false).Count()
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                a.Status.ToLower() == obj.Status.ToLower()
&& a.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsSalesOrderVm
               {
                   IsInvoiced = a.IsInvoiced,
                   TotalTaxAmount = a.TotalTaxAmount,
                   TotalDiscount = a.TotalDiscount,
                   InvoiceId = a.InvoiceId,
                   BranchId = a.BranchId,
                   Status = a.Status,
                   InvoiceUrl = oCommonController.webUrl,//+ "/SalesOrders/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                   BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                   SalesOrderId = a.SalesOrderId,
                   GrandTotal = a.GrandTotal,
                   Notes = a.Notes,
                   SalesOrderDate = a.SalesOrderDate,
                   //SalesOrderType = a.SalesOrderType,
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
                   TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesOrderDetails.Where(c=>c.SalesOrderId==a.SalesOrderId && c.IsDeleted==false).Count()
                   PaidQuantity = oConnectionContext.DbClsSalesOrderDetails.Where(c => c.SalesOrderId == a.SalesOrderId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                   FreeQuantity = oConnectionContext.DbClsSalesOrderDetails.Where(c => c.SalesOrderId == a.SalesOrderId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                   Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                   ReferenceType = a.ReferenceType,
                   CanEdit = (oConnectionContext.DbClsSalesProforma.Where(c => c.IsDeleted == false && c.ReferenceId == a.SalesOrderId && c.ReferenceType == "sales order").Count() == 0 &&
                    oConnectionContext.DbClsSales.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesOrderId && c.ReferenceType == "sales order").Count() == 0 &&
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.IsDeleted == false && c.IsCancelled == false && c.ReferenceId == a.SalesOrderId && c.ReferenceType == "sales order").Count() == 0) ? true : false,
                   TotalItems = oConnectionContext.DbClsSalesOrderDetails.Where(c => c.SalesOrderId == a.SalesOrderId &&
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
                    SalesOrders = det.OrderByDescending(a => a.SalesOrderId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> SalesOrder(ClsSalesOrderVm obj)
        {
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
            //bool EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

            var det = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == obj.SalesOrderId && a.CompanyId == obj.CompanyId).Select(a => new ClsSalesOrderVm
            {
                Terms =a.Terms,
                GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                PayTaxForExport = a.PayTaxForExport,
                TaxCollectedFromCustomer = a.TaxCollectedFromCustomer,
                IsReverseCharge = a.IsReverseCharge,
                TaxableAmount = a.TaxableAmount,
                NetAmountReverseCharge = a.NetAmountReverseCharge,
                RoundOffReverseCharge = a.RoundOffReverseCharge,
                GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                SourceOfSupplyId = a.PlaceOfSupplyId,
                DestinationOfSupplyId = a.PlaceOfSupplyId,
                TaxExemptionId = a.TaxExemptionId,
                BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                PlaceOfSupplyId = a.PlaceOfSupplyId,
                //a.PaymentStatus,
                TotalTaxAmount = a.TotalTaxAmount,
                //a.SalesOrderType,
                InvoiceId = a.InvoiceId,
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
                Status = a.Status,
                PayTerm = a.PayTerm,
                PayTermNo = a.PayTermNo,
                AttachDocument = a.AttachDocument,
                SalesOrderId = a.SalesOrderId,
                GrandTotal = a.GrandTotal,
                TaxId = a.TaxId,
                TotalDiscount = a.TotalDiscount,
                TotalQuantity = a.TotalQuantity,
                Discount = a.Discount,
                DiscountType = a.DiscountType,
                Notes = a.Notes,
                SalesOrderDate = a.SalesOrderDate,
                ShippingAddress = a.ShippingAddress,
                ShippingDetails = a.ShippingDetails,
                ShippingDocument = a.ShippingDocument,
                ShippingStatus = a.ShippingStatus,
                DeliveredTo = a.DeliveredTo,
                InvoiceNo = a.InvoiceNo,
                Subtotal = a.Subtotal,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                SmsSettingsId = a.SmsSettingsId,
                EmailSettingsId = a.EmailSettingsId,
                WhatsappSettingsId = a.WhatsappSettingsId,
                TaxAmount = a.TaxAmount,
                ReferenceId = a.ReferenceId,
                //Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                //    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                //    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                //a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                ReferenceType = a.ReferenceType,
                SalesOrderDetails = (from b in oConnectionContext.DbClsSalesOrderDetails
                                     join c in oConnectionContext.DbClsItemBranchMap
                                     on b.ItemDetailsId equals c.ItemDetailsId
                                     join d in oConnectionContext.DbClsItemDetails
                                      on b.ItemDetailsId equals d.ItemDetailsId
                                     join e in oConnectionContext.DbClsItem
                                     on d.ItemId equals e.ItemId
                                     where b.SalesOrderId == a.SalesOrderId && b.IsDeleted == false && c.BranchId == a.BranchId
                                     && b.IsComboItems == false
                                     select new ClsSalesOrderDetailsVm
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
                                         QuantityRemaining = a.Status.ToLower() != "draft" ? ((b.LotType == "purchase" ?
                                         oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                         : b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                         : b.LotType == "stocktransfer" ? oConnectionContext.DbClsStockTransferDetails.Where(f => f.StockTransferDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                         //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.QuantityRemaining).FirstOrDefault()
                                         : c.Quantity)) : c.Quantity,
                                         DiscountType = b.DiscountType,
                                         SalesOrderDetailsId = b.SalesOrderDetailsId,
                                         OtherInfo = b.OtherInfo,
                                         Discount = b.Discount,
                                         SalesOrderId = b.SalesOrderId,
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
                                         //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
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
                SalesOrderAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                ).Select(b => new ClsSalesOrderAdditionalChargesVm
                {
                    SalesOrderAdditionalChargesId = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(c => c.SalesOrderId == a.SalesOrderId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c=>c.SalesOrderAdditionalChargesId).FirstOrDefault(),
                    Name = b.Name,
                    AdditionalChargeId = b.AdditionalChargeId,
                    SalesOrderId = a.SalesOrderId,
                    TaxId = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(c => c.SalesOrderId == a.SalesOrderId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                    AmountExcTax = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(c => c.SalesOrderId == a.SalesOrderId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                    AmountIncTax = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(c => c.SalesOrderId == a.SalesOrderId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                    TaxAmount = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(c => c.SalesOrderId == a.SalesOrderId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                    AccountId = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(c => c.SalesOrderId == a.SalesOrderId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                    ItemCodeId = b.ItemCodeId,
                    TaxExemptionId = b.TaxExemptionId,
                    TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                }).ToList(),
            }).FirstOrDefault();

            det.Reference = det.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == det.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                det.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == det.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                det.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == det.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                det.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == det.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                det.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == det.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                det.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == det.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                det.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == det.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == det.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault();

            var AllTaxs = oConnectionContext.DbClsSalesOrder.Where(a => a.IsDeleted == false && a.SalesOrderId == det.SalesOrderId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsSalesOrderDetails.Where(a => a.SalesOrderId == det.SalesOrderId && a.IsDeleted == false
                                && a.IsComboItems == false).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).Concat(oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(a => a.SalesOrderId == det.SalesOrderId
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
                    SalesOrder = det,
                    Taxs = finalTaxs,
                    //ItemDetails = ItemDetails
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSalesOrder(ClsSalesOrderVm obj)
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

                if (obj.SalesOrderDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesOrderDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.InvoiceNo != "" && obj.InvoiceNo != null)
                {
                    if (oConnectionContext.DbClsSalesOrder.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Order# exists", Id = "divInvoiceNo" });
                        isError = true;
                    }
                }

                if (obj.SalesOrderDetails == null || obj.SalesOrderDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.SalesOrderDetails != null)
                {
                    foreach (var item in obj.SalesOrderDetails)
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
                    
                    PrefixType = "Sales Order";
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

                List<ClsSalesOrderDetailsVm> _SalesOrdersDetails = new List<ClsSalesOrderDetailsVm>();
                if (obj.SalesOrderDetails != null)
                {
                    foreach (var SalesOrders in obj.SalesOrderDetails)
                    {
                        SalesOrders.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.ProductType).FirstOrDefault();
                        if (SalesOrders.ProductType.ToLower() == "combo")
                        {
                            SalesOrders.ComboId = oCommonController.CreateToken();
                            var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => new
                            {
                                ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                ItemDetailsId = a.ItemDetailsId,
                                ComboItemDetailsId = a.ComboItemDetailsId,
                                Quantity = a.Quantity,
                                a.PriceAddedFor
                            }).ToList();

                            foreach (var item in combo)
                            {
                                _SalesOrdersDetails.Add(new ClsSalesOrderDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesOrders.Quantity, Under = SalesOrders.ItemDetailsId, IsComboItems = true, ComboId = SalesOrders.ComboId, DivId = SalesOrders.DivId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                            }
                            _SalesOrdersDetails.Add(SalesOrders);
                        }
                        else
                        {
                            _SalesOrdersDetails.Add(SalesOrders);
                        }
                    }
                }

                obj.SalesOrderDetails = _SalesOrdersDetails;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                //if (EnableLotNo == true)
                //{
                //    if (obj.SalesOrderDetails != null)
                //    {
                //        foreach (var SalesOrders in obj.SalesOrderDetails)
                //        {
                //            if (SalesOrders.ProductType != "Combo")
                //            {
                //                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                if (IsManageStock == true)
                //                {
                //                    if (SalesOrders.IsComboItems == true)
                //                    {
                //                        //decimal convertedStock = oCommonController.StockConversion(SalesOrders.Quantity + SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                //                        decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == SalesOrders.ItemId && a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                //                        //if (remainingQty < convertedStock)
                //                        //{
                //                        //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                        //    isError = true;
                //                        //}

                //                        decimal convertedStock = 0;
                //                        foreach (var inner in obj.SalesOrderDetails)
                //                        {
                //                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                            if (IsManageStock_Inner == true)
                //                            {
                //                                if (SalesOrders.ItemId == inner.ItemId && SalesOrders.ItemDetailsId == inner.ItemDetailsId)
                //                                {
                //                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                //                                }
                //                            }

                //                        }
                //                        if (remainingQty < convertedStock)
                //                        {
                //                            //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+SalesOrders.DivId });
                //                            errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                            isError = true;
                //                        }

                //                    }
                //                    else
                //                    {
                //                        decimal remainingQty = 0;
                //                        //string LotNo = "";
                //                        if (SalesOrders.LotType == "openingstock")
                //                        {
                //                            remainingQty = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == SalesOrders.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                //                            //LotNo = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == SalesOrders.LotId).Select(a => a.LotNo).FirstOrDefault();
                //                        }
                //                        else if (SalesOrders.LotType == "purchase")
                //                        {
                //                            remainingQty = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == SalesOrders.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                //                            //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == SalesOrders.LotId).Select(a => a.LotNo).FirstOrDefault();
                //                        }
                //                        else if (SalesOrders.LotType == "stocktransfer")
                //                        {
                //                            remainingQty = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                //                            //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == SalesOrders.LotId).Select(a => a.LotNo).FirstOrDefault();
                //                        }

                //                        decimal convertedStock = 0;
                //                        foreach (var inner in obj.SalesOrderDetails)
                //                        {
                //                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                            if (IsManageStock_Inner == true)
                //                            {
                //                                if (SalesOrders.LotType == inner.LotType && SalesOrders.LotId == inner.LotId)
                //                                {
                //                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                //                                }
                //                            }

                //                        }
                //                        if (remainingQty < convertedStock)
                //                        {
                //                            //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+SalesOrders.DivId });
                //                            errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                            isError = true;
                //                        }
                //                    }

                //                }
                //            }
                //        }
                //        if (isError == true)
                //        {
                //            data = new
                //            {
                //                Status = 2,
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
                //else
                //{
                //    if (obj.SalesOrderDetails != null)
                //    {
                //        foreach (var SalesOrders in obj.SalesOrderDetails)
                //        {
                //            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //            if (IsManageStock == true)
                //            {
                //                //decimal convertedStock = oCommonController.StockConversion(SalesOrders.Quantity + SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                //                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == SalesOrders.ItemId && a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                //                //if (remainingQty < convertedStock)
                //                //{
                //                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                //    isError = true;
                //                //}

                //                decimal convertedStock = 0;
                //                foreach (var inner in obj.SalesOrderDetails)
                //                {
                //                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                    if (IsManageStock_Inner == true)
                //                    {
                //                        if (SalesOrders.ItemId == inner.ItemId && SalesOrders.ItemDetailsId == inner.ItemDetailsId)
                //                        {
                //                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                //                        }
                //                    }

                //                }
                //                if (remainingQty < convertedStock)
                //                {
                //                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+SalesOrders.DivId });
                //                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                    isError = true;
                //                }

                //            }
                //        }
                //        if (isError == true)
                //        {
                //            data = new
                //            {
                //                Status = 2,
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

                //if (obj.SalesOrderType == "Pos")
                //{
                //    obj.CashRegisterId = oConnectionContext.DbClsCashRegister.Where(a => a.AddedBy == obj.AddedBy && a.Status == 1).Select(a => a.CashRegisterId).FirstOrDefault();
                //}

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                ClsSalesOrder oClsSalesOrder = new ClsSalesOrder()
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
                    SalesOrderDate = obj.SalesOrderDate.AddHours(5).AddMinutes(30),
                    SalesOrderId = obj.SalesOrderId,
                    InvoiceNo = obj.InvoiceNo,
                    ShippingAddress = obj.ShippingAddress,
                    ShippingDetails = obj.ShippingDetails,
                    ShippingStatus = obj.ShippingStatus,
                    Subtotal = obj.Subtotal,
                    TaxId = obj.TaxId,
                    TotalQuantity = obj.TotalQuantity,
                    BranchId = obj.BranchId,
                    //SalesOrderType = obj.SalesOrderType,
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
                    ReferenceId = obj.ReferenceId,
                    ReferenceType = obj.ReferenceType,
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
                    Terms = obj.Terms
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SalesOrders/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesOrders/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesOrder.AttachDocument = filepathPass;
                }

                if (obj.ShippingDocument != "" && obj.ShippingDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SalesOrders/ShippingDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionShippingDocument;

                    string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesOrders/ShippingDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesOrder.ShippingDocument = filepathPass;
                }

                oConnectionContext.DbClsSalesOrder.Add(oClsSalesOrder);
                oConnectionContext.SaveChanges();

                if (obj.SalesOrderAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesOrderAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

                        ClsSalesOrderAdditionalCharges oClsSalesOrderAdditionalCharges = new ClsSalesOrderAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            SalesOrderId = oClsSalesOrder.SalesOrderId,
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
                        oConnectionContext.DbClsSalesOrderAdditionalCharges.Add(oClsSalesOrderAdditionalCharges);
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.SalesOrderDetails != null)
                {
                    foreach (var SalesOrders in obj.SalesOrderDetails)
                    {
                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        decimal convertedStock = 0, freeConvertedStock = 0;
                        if (SalesOrders.ProductType != "Combo")
                        {
                            convertedStock = oCommonController.StockConversion(SalesOrders.Quantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                            freeConvertedStock = oCommonController.StockConversion(SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            //if (IsManageStock == true)
                            //{
                            //    if (obj.Status.ToLower() == "sent")
                            //    {
                            //        if (SalesOrders.LotId == 0)
                            //        {
                            //            SalesOrders.StockDeductionIds = oCommonController.deductStock(obj.BranchId, SalesOrders.ItemDetailsId, (convertedStock + freeConvertedStock), SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                            //        }
                            //        else
                            //        {
                            //            SalesOrders.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, SalesOrders.ItemDetailsId, (convertedStock + freeConvertedStock), SalesOrders.LotId, SalesOrders.LotType);
                            //        }
                            //    }
                            //}

                            if (SalesOrders.LotType == "stocktransfer")
                            {
                                SalesOrders.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.LotId).FirstOrDefault();
                                SalesOrders.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.LotType).FirstOrDefault();
                            }
                            else
                            {
                                SalesOrders.LotIdForLotNoChecking = SalesOrders.LotId;
                                SalesOrders.LotTypeForLotNoChecking = SalesOrders.LotType;
                            }
                        }

                        DateTime? WarrantyExpiryDate = null;
                        if (SalesOrders.WarrantyId != 0)
                        {
                            var warranty = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == SalesOrders.WarrantyId).Select(a => new
                            {
                                a.Duration,
                                a.DurationNo
                            }).FirstOrDefault();

                            if (warranty.Duration == "Days")
                            {
                                WarrantyExpiryDate = obj.SalesOrderDate.AddDays(warranty.DurationNo);
                            }
                            else if (warranty.Duration == "Months")
                            {
                                WarrantyExpiryDate = obj.SalesOrderDate.AddMonths(Convert.ToInt32(warranty.DurationNo));
                            }
                            else if (warranty.Duration == "Years")
                            {
                                WarrantyExpiryDate = obj.SalesOrderDate.AddYears(Convert.ToInt32(warranty.DurationNo));
                            }
                        }

                        //long ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                        ClsSalesOrderDetails oClsSalesOrderDetails = new ClsSalesOrderDetails()
                        {
                            DiscountType = SalesOrders.DiscountType,
                            OtherInfo = SalesOrders.OtherInfo,
                            PriceIncTax = SalesOrders.PriceIncTax,
                            ItemId = SalesOrders.ItemId,
                            ItemDetailsId = SalesOrders.ItemDetailsId,
                            SalesOrderId = oClsSalesOrder.SalesOrderId,
                            TaxId = SalesOrders.TaxId,
                            Discount = SalesOrders.Discount,
                            Quantity = SalesOrders.Quantity,
                            UnitCost = SalesOrders.UnitCost,
                            IsActive = SalesOrders.IsActive,
                            IsDeleted = SalesOrders.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            //StockDeductionIds = SalesOrders.StockDeductionIds,
                            QuantityRemaining = SalesOrders.ProductType == "Combo" ? (SalesOrders.Quantity + SalesOrders.FreeQuantity) : (convertedStock + freeConvertedStock),
                            WarrantyId = SalesOrders.WarrantyId,
                            DefaultUnitCost = DefaultUnitCost,
                            DefaultAmount = SalesOrders.Quantity * DefaultUnitCost,
                            PriceAddedFor = SalesOrders.PriceAddedFor,
                            LotId = SalesOrders.LotId,
                            LotType = SalesOrders.LotType,
                            FreeQuantity = SalesOrders.FreeQuantity,
                            //FreeQuantityPriceAddedFor = SalesOrders.FreeQuantityPriceAddedFor,
                            AmountExcTax = SalesOrders.AmountExcTax,
                            TaxAmount = SalesOrders.TaxAmount,
                            PriceExcTax = SalesOrders.PriceExcTax,
                            AmountIncTax = SalesOrders.AmountIncTax,
                            Under = SalesOrders.Under,
                            UnitAddedFor = SalesOrders.UnitAddedFor,
                            LotIdForLotNoChecking = SalesOrders.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = SalesOrders.LotTypeForLotNoChecking,
                            ComboId = SalesOrders.ComboId,
                            IsComboItems = SalesOrders.IsComboItems,
                            QuantitySold = convertedStock + freeConvertedStock,
                            ComboPerUnitQuantity = SalesOrders.ComboPerUnitQuantity,
                            WarrantyExpiryDate = WarrantyExpiryDate,
                            ExtraDiscount = SalesOrders.ExtraDiscount,
                            ItemCodeId = SalesOrders.ItemCodeId,
                            TaxExemptionId = SalesOrders.TaxExemptionId,
                            TotalTaxAmount = SalesOrders.TotalTaxAmount,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsSalesOrderDetails.Add(oClsSalesOrderDetails);
                        oConnectionContext.SaveChanges();

                        //string ll = "delete from tblSalesOrdersDeductionId where SalesOrderDetailsId=" + SalesOrders.oClsSalesOrderDetails;
                        //oConnectionContext.Database.ExecuteSqlCommand(ll);

                        //if (SalesOrders.StockDeductionIds != null)
                        //{
                        //    foreach (var l in SalesOrders.StockDeductionIds)
                        //    {
                        //        ClsSalesOrderDeductionId oClsSalesOrderDeductionId = new ClsSalesOrderDeductionId()
                        //        {
                        //            AddedBy = obj.AddedBy,
                        //            AddedOn = CurrentDate,
                        //            CompanyId = obj.CompanyId,
                        //            Id = l.Id,
                        //            Type = l.Type,
                        //            Quantity = l.Quantity,
                        //            SalesOrderDetailsId = oClsSalesOrderDetails.SalesOrderDetailsId,
                        //            SalesOrderId = oClsSalesOrder.SalesOrderId,
                        //        };
                        //        oConnectionContext.DbClsSalesOrderDeductionId.Add(oClsSalesOrderDeductionId);
                        //        oConnectionContext.SaveChanges();
                        //    }
                        //}
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
                    Category = "Sales Order",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Order \"" + obj.InvoiceNo + "\" created",
                    Id = oClsSalesOrder.SalesOrderId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Order", obj.CompanyId, oClsSalesOrder.SalesOrderId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                //if(arr[0] == "1")
                //{
                //    oCommonController.InsertSmsUsed(oClsSalesOrder.SalesOrderId, obj.Status, obj.CompanyId, obj.AddedBy, CurrentDate);
                //}

                data = new
                {
                    Status = 1,
                    Message = "Sales Order created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        SalesOrder = new
                        {
                            SalesOrderId = oClsSalesOrder.SalesOrderId,
                            InvoiceId = oClsSalesOrder.InvoiceId
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceOrder }).FirstOrDefault(),
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesOrderDelete(ClsSalesOrderVm obj)
        {
            int SalesProformaCount = (from a in oConnectionContext.DbClsSalesProforma
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                       && a.ReferenceId == obj.SalesOrderId && a.ReferenceType == "sales order"
                                      select a.SalesProformaId).Count();

            int DeliveryChallanCount = (from a in oConnectionContext.DbClsDeliveryChallan
                                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                       && a.ReferenceId == obj.SalesOrderId && a.ReferenceType == "sales order"
                                        select a.DeliveryChallanId).Count();

            int SalesCount = (from a in oConnectionContext.DbClsSales
                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                              && a.ReferenceId == obj.SalesOrderId && a.ReferenceType == "sales order"
                              select a.SalesId).Count();

            if (SalesProformaCount > 0 || DeliveryChallanCount > 0 || SalesCount > 0)
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
                ClsSalesOrder oClsSalesOrder = new ClsSalesOrder()
                {
                    SalesOrderId = obj.SalesOrderId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesOrder.Attach(oClsSalesOrder);
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SalesOrderId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Order",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Order \"" + oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == obj.SalesOrderId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsSalesOrder.SalesOrderId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Order deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesOrder(ClsSalesOrderVm obj)
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

                if (obj.SalesOrderDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesOrderDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.SalesOrderDetails == null || obj.SalesOrderDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.SalesOrderDetails != null)
                {
                    foreach (var item in obj.SalesOrderDetails)
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

                List<ClsSalesOrderDetailsVm> _SalesOrdersDetails = new List<ClsSalesOrderDetailsVm>();
                if (obj.SalesOrderDetails != null)
                {
                    foreach (var SalesOrders in obj.SalesOrderDetails)
                    {
                        if (SalesOrders.SalesOrderDetailsId != 0)
                        {
                            SalesOrders.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesOrders.ProductType.ToLower() == "combo")
                            {
                                SalesOrders.ComboId = oConnectionContext.DbClsSalesOrderDetails.Where(a => a.SalesOrderDetailsId == SalesOrders.SalesOrderDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesOrderDetails.Where(a => a.ComboId == SalesOrders.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.SalesOrderDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesOrdersDetails.Add(new ClsSalesOrderDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesOrders.Quantity, Under = SalesOrders.ItemDetailsId, IsComboItems = true, ComboId = SalesOrders.ComboId, DivId = SalesOrders.DivId, SalesOrderDetailsId = item.SalesOrderDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1, IsDeleted = SalesOrders.IsDeleted });
                                }
                                _SalesOrdersDetails.Add(SalesOrders);
                            }
                            else
                            {
                                _SalesOrdersDetails.Add(SalesOrders);
                            }
                        }
                        else
                        {
                            _SalesOrdersDetails.Add(SalesOrders);
                        }
                    }
                }

                obj.SalesOrderDetails = _SalesOrdersDetails;

                obj.SalesOrderDetails.RemoveAll(r => r.IsComboItems == true);
                //obj.SalesOrderDetails.RemoveAll(r => r.IsDeleted == true);

                List<ClsSalesOrderDetailsVm> _SalesOrdersDetails1 = new List<ClsSalesOrderDetailsVm>();
                if (obj.SalesOrderDetails != null)
                {
                    foreach (var SalesOrders in obj.SalesOrderDetails)
                    {
                        if (SalesOrders.SalesOrderDetailsId == 0)
                        {
                            SalesOrders.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesOrders.ProductType.ToLower() == "combo")
                            {
                                SalesOrders.ComboId = oCommonController.CreateToken();
                                var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => new
                                {
                                    ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ComboItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesOrdersDetails1.Add(new ClsSalesOrderDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesOrders.Quantity, Under = SalesOrders.ItemDetailsId, IsComboItems = true, ComboId = SalesOrders.ComboId, DivId = SalesOrders.DivId, SalesOrderDetailsId = SalesOrders.SalesOrderDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesOrdersDetails1.Add(SalesOrders);
                            }
                            else
                            {
                                _SalesOrdersDetails1.Add(SalesOrders);
                            }
                        }
                        else
                        {
                            SalesOrders.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (SalesOrders.ProductType.ToLower() == "combo")
                            {
                                SalesOrders.ComboId = oConnectionContext.DbClsSalesOrderDetails.Where(a => a.SalesOrderDetailsId == SalesOrders.SalesOrderDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesOrderDetails.Where(a => a.ComboId == SalesOrders.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == SalesOrders.ItemId && b.ComboItemDetailsId == a.ItemDetailsId).Select(b => b.Quantity).FirstOrDefault(),
                                    a.SalesOrderDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesOrdersDetails1.Add(new ClsSalesOrderDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * SalesOrders.Quantity, Under = SalesOrders.ItemDetailsId, IsComboItems = true, ComboId = SalesOrders.ComboId, DivId = SalesOrders.DivId, SalesOrderDetailsId = item.SalesOrderDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesOrdersDetails1.Add(SalesOrders);
                            }
                            else
                            {
                                _SalesOrdersDetails1.Add(SalesOrders);
                            }
                        }
                    }
                }

                obj.SalesOrderDetails = _SalesOrdersDetails1;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                //if (EnableLotNo == true)
                //{
                //    if (obj.SalesOrderDetails != null)
                //    {
                //        foreach (var SalesOrders in obj.SalesOrderDetails)
                //        {
                //            if (SalesOrders.ProductType != "Combo")
                //            {
                //                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                if (IsManageStock == true)
                //                {
                //                    if (SalesOrders.IsComboItems == true)
                //                    {
                //                        //decimal convertedStock = oCommonController.StockConversion(SalesOrders.Quantity + SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                //                        decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == SalesOrders.ItemId && a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                //                        //if (remainingQty < convertedStock)
                //                        //{
                //                        //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                        //    isError = true;
                //                        //}
                //                        decimal convertedStock = 0;
                //                        foreach (var inner in obj.SalesOrderDetails)
                //                        {
                //                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                            if (IsManageStock_Inner == true)
                //                            {
                //                                if (SalesOrders.ItemId == inner.ItemId && SalesOrders.ItemDetailsId == inner.ItemDetailsId)
                //                                {
                //                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                //                                }
                //                            }

                //                        }
                //                        if (remainingQty < convertedStock)
                //                        {
                //                            //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+SalesOrders.DivId });
                //                            errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                            isError = true;
                //                        }
                //                    }
                //                    else
                //                    {
                //                        decimal remainingQty = 0;
                //                        //string LotNo = "";
                //                        if (SalesOrders.LotType == "openingstock")
                //                        {
                //                            remainingQty = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == SalesOrders.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                //                            //LotNo = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == SalesOrders.LotId).Select(a => a.LotNo).FirstOrDefault();
                //                        }
                //                        else if (SalesOrders.LotType == "purchase")
                //                        {
                //                            remainingQty = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == SalesOrders.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                //                            //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == SalesOrders.LotId).Select(a => a.LotNo).FirstOrDefault();
                //                        }
                //                        else if (SalesOrders.LotType == "stocktransfer")
                //                        {
                //                            remainingQty = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.QuantityRemaining).FirstOrDefault();
                //                            //LotNo = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == SalesOrders.LotId).Select(a => a.LotNo).FirstOrDefault();
                //                        }
                //                        else
                //                        {
                //                            if (SalesOrders.SalesOrderDetailsId != 0)
                //                            {
                //                                remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == SalesOrders.ItemId && a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                //                            }
                //                        }

                //                        decimal convertedStock = 0;
                //                        foreach (var inner in obj.SalesOrderDetails)
                //                        {
                //                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                            if (IsManageStock_Inner == true)
                //                            {
                //                                if (SalesOrders.LotType == inner.LotType && SalesOrders.LotId == inner.LotId)
                //                                {
                //                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                //                                }
                //                            }

                //                        }
                //                        if (remainingQty < convertedStock)
                //                        {
                //                            //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+SalesOrders.DivId });
                //                            errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                            isError = true;
                //                        }
                //                    }

                //                }
                //            }
                //        }
                //        if (isError == true)
                //        {
                //            data = new
                //            {
                //                Status = 2,
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
                //else
                //{
                //    if (obj.SalesOrderDetails != null)
                //    {
                //        foreach (var SalesOrders in obj.SalesOrderDetails)
                //        {
                //            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //            if (IsManageStock == true)
                //            {
                //                //decimal convertedStock = oCommonController.StockConversion(SalesOrders.Quantity + SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                //                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == SalesOrders.ItemId && a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                //                //if (remainingQty < convertedStock)
                //                //{
                //                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                //    isError = true;
                //                //}
                //                decimal convertedStock = 0;
                //                foreach (var inner in obj.SalesOrderDetails)
                //                {
                //                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                //                    if (IsManageStock_Inner == true)
                //                    {
                //                        if (SalesOrders.ItemId == inner.ItemId && SalesOrders.ItemDetailsId == inner.ItemDetailsId)
                //                        {
                //                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                //                        }
                //                    }

                //                }
                //                if (remainingQty < convertedStock)
                //                {
                //                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+SalesOrders.DivId });
                //                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + SalesOrders.DivId });
                //                    isError = true;
                //                }
                //            }
                //        }
                //        if (isError == true)
                //        {
                //            data = new
                //            {
                //                Status = 2,
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

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                ClsSalesOrder oClsSalesOrder = new ClsSalesOrder()
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
                    SalesOrderDate = obj.SalesOrderDate.AddHours(5).AddMinutes(30),
                    SalesOrderId = obj.SalesOrderId,
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
                    //SalesOrderType = obj.SalesOrderType,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxExemptionId = obj.TaxExemptionId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    Terms = obj.Terms
                };

                string pic1 = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == obj.SalesOrderId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/SalesOrders/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesOrders/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesOrder.AttachDocument = filepathPass;
                }
                else
                {
                    oClsSalesOrder.AttachDocument = pic1;
                }

                pic1 = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == obj.SalesOrderId).Select(a => a.ShippingDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/SalesOrders/ShippingDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionShippingDocument;

                    string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SalesOrders/ShippingDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSalesOrder.ShippingDocument = filepathPass;
                }
                else
                {
                    oClsSalesOrder.ShippingDocument = pic1;
                }

                oConnectionContext.DbClsSalesOrder.Attach(oClsSalesOrder);
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.CompanyId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.DeliveredTo).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PayTerm).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PayTermNo).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SalesOrderDate).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SalesOrderId).IsModified = true;
                //oConnectionContext.Entry(oClsSalesOrder).Property(x => x.InvoiceNo).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ShippingAddress).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ShippingDetails).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ShippingStatus).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ShippingDocument).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.OnlinePaymentSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ChangeReturn).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TotalPaying).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Balance).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.HoldReason).IsModified = true;
                //oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PaymentStatus).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PaymentType).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.NetAmount).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TotalTaxAmount).IsModified = true;
                //oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SalesOrderType).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PlaceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.TaxCollectedFromCustomer).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Terms).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.SalesOrderAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesOrderAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.SalesAccountId }).FirstOrDefault();

                        if (item.SalesOrderAdditionalChargesId == 0)
                        {
                            ClsSalesOrderAdditionalCharges oClsSalesOrderAdditionalCharges = new ClsSalesOrderAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesOrderId = oClsSalesOrder.SalesOrderId,
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
                            oConnectionContext.DbClsSalesOrderAdditionalCharges.Add(oClsSalesOrderAdditionalCharges);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsSalesOrderAdditionalCharges oClsSalesOrderAdditionalCharges = new ClsSalesOrderAdditionalCharges()
                            {
                                SalesOrderAdditionalChargesId = item.SalesOrderAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesOrderId = oClsSalesOrder.SalesOrderId,
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
                            oConnectionContext.DbClsSalesOrderAdditionalCharges.Attach(oClsSalesOrderAdditionalCharges);
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.SalesOrderId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSalesOrderAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.SalesOrderDetails != null)
                {
                    foreach (var SalesOrders in obj.SalesOrderDetails)
                    {
                        if (SalesOrders.IsDeleted == true)
                        {
                            string query = "update \"tblSalesOrderDetails\" set \"IsDeleted\"=True where \"SalesOrderDetailsId\"=" + SalesOrders.SalesOrderDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        else
                        {
                            var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                            DateTime? WarrantyExpiryDate = null;
                            if (SalesOrders.WarrantyId != 0)
                            {
                                var warranty = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == SalesOrders.WarrantyId).Select(a => new
                                {
                                    a.Duration,
                                    a.DurationNo
                                }).FirstOrDefault();

                                if (warranty.Duration == "Days")
                                {
                                    WarrantyExpiryDate = obj.SalesOrderDate.AddDays(warranty.DurationNo);
                                }
                                else if (warranty.Duration == "Months")
                                {
                                    WarrantyExpiryDate = obj.SalesOrderDate.AddMonths(Convert.ToInt32(warranty.DurationNo));
                                }
                                else if (warranty.Duration == "Years")
                                {
                                    WarrantyExpiryDate = obj.SalesOrderDate.AddYears(Convert.ToInt32(warranty.DurationNo));
                                }
                            }

                            SalesOrders.SalesOrderDetailsId = oConnectionContext.DbClsSalesOrderDetails.Where(a => a.CompanyId == obj.CompanyId
                                && a.IsDeleted == false && a.SalesOrderId == obj.SalesOrderId && a.ItemId == SalesOrders.ItemId
                                && a.ItemDetailsId == SalesOrders.ItemDetailsId).Select(a => a.SalesOrderDetailsId).FirstOrDefault();

                            if (SalesOrders.SalesOrderDetailsId == 0)
                            {
                                decimal convertedStock = 0, freeConvertedStock = 0;
                                if (SalesOrders.ProductType != "Combo")
                                {
                                    convertedStock = oCommonController.StockConversion(SalesOrders.Quantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    freeConvertedStock = oCommonController.StockConversion(SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    //if (IsManageStock == true)
                                    //{
                                    //    if (obj.Status.ToLower() == "sent")
                                    //    {
                                    //        if (SalesOrders.LotId == 0)
                                    //        {
                                    //            SalesOrders.StockDeductionIds = oCommonController.deductStock(obj.BranchId, SalesOrders.ItemDetailsId, (convertedStock + freeConvertedStock), SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    //        }
                                    //        else
                                    //        {
                                    //            SalesOrders.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, SalesOrders.ItemDetailsId, (convertedStock + freeConvertedStock), SalesOrders.LotId, SalesOrders.LotType);
                                    //        }
                                    //    }
                                    //}

                                    if (SalesOrders.LotType == "stocktransfer")
                                    {
                                        SalesOrders.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.LotId).FirstOrDefault();
                                        SalesOrders.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.LotType).FirstOrDefault();
                                    }
                                    else
                                    {
                                        SalesOrders.LotIdForLotNoChecking = SalesOrders.LotId;
                                        SalesOrders.LotTypeForLotNoChecking = SalesOrders.LotType;
                                    }
                                }

                                ClsSalesOrderDetails oClsSalesOrderDetails = new ClsSalesOrderDetails()
                                {
                                    DiscountType = SalesOrders.DiscountType,
                                    OtherInfo = SalesOrders.OtherInfo,
                                    PriceIncTax = SalesOrders.PriceIncTax,
                                    ItemId = SalesOrders.ItemId,
                                    ItemDetailsId = SalesOrders.ItemDetailsId,
                                    SalesOrderId = oClsSalesOrder.SalesOrderId,
                                    TaxId = SalesOrders.TaxId,
                                    Discount = SalesOrders.Discount,
                                    Quantity = SalesOrders.Quantity,
                                    UnitCost = SalesOrders.UnitCost,
                                    IsActive = SalesOrders.IsActive,
                                    IsDeleted = SalesOrders.IsDeleted,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    //StockDeductionIds = SalesOrders.StockDeductionIds,
                                    QuantityRemaining = SalesOrders.IsComboItems == true ? (SalesOrders.Quantity + SalesOrders.FreeQuantity) : (convertedStock + freeConvertedStock),
                                    WarrantyId = SalesOrders.WarrantyId,
                                    DefaultUnitCost = DefaultUnitCost,
                                    DefaultAmount = SalesOrders.Quantity * DefaultUnitCost,
                                    PriceAddedFor = SalesOrders.PriceAddedFor,
                                    LotId = SalesOrders.LotId,
                                    LotType = SalesOrders.LotType,
                                    FreeQuantity = SalesOrders.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = SalesOrders.FreeQuantityPriceAddedFor,
                                    AmountExcTax = SalesOrders.AmountExcTax,
                                    TaxAmount = SalesOrders.TaxAmount,
                                    PriceExcTax = SalesOrders.PriceExcTax,
                                    AmountIncTax = SalesOrders.AmountIncTax,
                                    UnitAddedFor = SalesOrders.UnitAddedFor,
                                    LotIdForLotNoChecking = SalesOrders.LotIdForLotNoChecking,
                                    LotTypeForLotNoChecking = SalesOrders.LotTypeForLotNoChecking,
                                    ComboId = SalesOrders.ComboId,
                                    IsComboItems = SalesOrders.IsComboItems,
                                    QuantitySold = convertedStock + freeConvertedStock,
                                    ComboPerUnitQuantity = SalesOrders.ComboPerUnitQuantity,
                                    WarrantyExpiryDate = WarrantyExpiryDate,
                                    ExtraDiscount = SalesOrders.ExtraDiscount,
                                    TaxExemptionId = SalesOrders.TaxExemptionId,
                                    ItemCodeId = SalesOrders.ItemCodeId,
                                    TotalTaxAmount = SalesOrders.TotalTaxAmount,
                                };

                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsSalesOrderDetails.Add(oClsSalesOrderDetails);
                                oConnectionContext.SaveChanges();

                                //string ll = "delete from tblSalesOrdersDeductionId where SalesOrderDetailsId=" + oClsSalesOrderDetails.SalesOrderDetailsId;
                                //oConnectionContext.Database.ExecuteSqlCommand(ll);

                                //if (SalesOrders.StockDeductionIds != null)
                                //{
                                //    foreach (var l in SalesOrders.StockDeductionIds)
                                //    {
                                //        ClsSalesOrderDeductionId oClsSalesOrderDeductionId = new ClsSalesOrderDeductionId()
                                //        {
                                //            AddedBy = obj.AddedBy,
                                //            AddedOn = CurrentDate,
                                //            CompanyId = obj.CompanyId,
                                //            Id = l.Id,
                                //            Type = l.Type,
                                //            Quantity = l.Quantity,
                                //            SalesOrderDetailsId = oClsSalesOrderDetails.SalesOrderDetailsId,
                                //            SalesOrderId = oClsSalesOrder.SalesOrderId,
                                //        };
                                //        oConnectionContext.DbClsSalesOrderDeductionId.Add(oClsSalesOrderDeductionId);
                                //        oConnectionContext.SaveChanges();
                                //    }
                                //}
                            }
                            else
                            {
                                decimal QuantityReturned = 0;
                                decimal convertedStock = 0, freeConvertedStock = 0;

                                if (SalesOrders.ProductType != "Combo")
                                {
                                    convertedStock = oCommonController.StockConversion(SalesOrders.Quantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    freeConvertedStock = oCommonController.StockConversion(SalesOrders.FreeQuantity, SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == SalesOrders.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    //if (IsManageStock == true)
                                    //{
                                    //    if (obj.Status.ToLower() == "sent")
                                    //    {
                                    //        if (SalesOrders.LotId == 0)
                                    //        {
                                    //            SalesOrders.StockDeductionIds = oCommonController.deductStock(obj.BranchId, SalesOrders.ItemDetailsId, (convertedStock + freeConvertedStock), SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    //        }
                                    //        else
                                    //        {
                                    //            SalesOrders.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, SalesOrders.ItemDetailsId, (convertedStock + freeConvertedStock), SalesOrders.LotId, SalesOrders.LotType);
                                    //        }
                                    //    }

                                    //    QuantityReturned = oCommonController.StockConversion((from a in oConnectionContext.DbClsSalesOrderReturn
                                    //                                                          join b in oConnectionContext.DbClsSalesOrderReturnDetails
                                    //                                                             on a.SalesOrdersReturnId equals b.SalesOrdersReturnId
                                    //                                                          where a.SalesOrderId == obj.SalesOrderId && b.ItemId == SalesOrders.ItemId &&
                                    //                                                          b.ItemDetailsId == SalesOrders.ItemDetailsId
                                    //                                                          select b.Quantity).FirstOrDefault(), SalesOrders.ItemId, SalesOrders.PriceAddedFor);
                                    //}
                                }
                                else
                                {
                                    //QuantityReturned = (from a in oConnectionContext.DbClsSalesOrderReturn
                                    //                    join b in oConnectionContext.DbClsSalesOrderReturnDetails
                                    //                       on a.SalesOrdersReturnId equals b.SalesOrdersReturnId
                                    //                    where a.SalesOrderId == obj.SalesOrderId && b.ItemId == SalesOrders.ItemId &&
                                    //                    b.ItemDetailsId == SalesOrders.ItemDetailsId
                                    //                    select b.Quantity).FirstOrDefault();
                                }

                                if (SalesOrders.LotType == "stocktransfer")
                                {
                                    SalesOrders.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.LotId).FirstOrDefault();
                                    SalesOrders.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == SalesOrders.LotId).Select(a => a.LotType).FirstOrDefault();
                                }
                                else
                                {
                                    SalesOrders.LotIdForLotNoChecking = SalesOrders.LotId;
                                    SalesOrders.LotTypeForLotNoChecking = SalesOrders.LotType;
                                }

                                ClsSalesOrderDetails oClsSalesOrderDetails = new ClsSalesOrderDetails()
                                {
                                    SalesOrderDetailsId = SalesOrders.SalesOrderDetailsId,
                                    DiscountType = SalesOrders.DiscountType,
                                    OtherInfo = SalesOrders.OtherInfo,
                                    PriceIncTax = SalesOrders.PriceIncTax,
                                    ItemId = SalesOrders.ItemId,
                                    ItemDetailsId = SalesOrders.ItemDetailsId,
                                    SalesOrderId = oClsSalesOrder.SalesOrderId,
                                    TaxId = SalesOrders.TaxId,
                                    Discount = SalesOrders.Discount,
                                    Quantity = SalesOrders.Quantity,
                                    UnitCost = SalesOrders.UnitCost,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    //StockDeductionIds = SalesOrders.StockDeductionIds,
                                    QuantityRemaining = SalesOrders.ProductType == "Combo" ? (SalesOrders.Quantity + SalesOrders.FreeQuantity) - QuantityReturned : (convertedStock + freeConvertedStock) - QuantityReturned,
                                    //QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityReturned,
                                    WarrantyId = SalesOrders.WarrantyId,
                                    DefaultUnitCost = DefaultUnitCost,
                                    DefaultAmount = SalesOrders.Quantity * DefaultUnitCost,
                                    PriceAddedFor = SalesOrders.PriceAddedFor,
                                    LotId = SalesOrders.LotId,
                                    LotType = SalesOrders.LotType,
                                    FreeQuantity = SalesOrders.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = SalesOrders.FreeQuantityPriceAddedFor,
                                    AmountExcTax = SalesOrders.AmountExcTax,
                                    TaxAmount = SalesOrders.TaxAmount,
                                    PriceExcTax = SalesOrders.PriceExcTax,
                                    AmountIncTax = SalesOrders.AmountIncTax,
                                    UnitAddedFor = SalesOrders.UnitAddedFor,
                                    LotIdForLotNoChecking = SalesOrders.LotIdForLotNoChecking,
                                    LotTypeForLotNoChecking = SalesOrders.LotTypeForLotNoChecking,
                                    ComboId = SalesOrders.ComboId,
                                    IsComboItems = SalesOrders.IsComboItems,
                                    QuantitySold = convertedStock + freeConvertedStock,
                                    ComboPerUnitQuantity = SalesOrders.ComboPerUnitQuantity,
                                    WarrantyExpiryDate = WarrantyExpiryDate,
                                    ExtraDiscount = SalesOrders.ExtraDiscount,
                                    TaxExemptionId = SalesOrders.TaxExemptionId,
                                    ItemCodeId = SalesOrders.ItemCodeId,
                                    TotalTaxAmount = SalesOrders.TotalTaxAmount,
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsSalesOrderDetails.Attach(oClsSalesOrderDetails);
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.DiscountType).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.OtherInfo).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.PriceIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ItemId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ItemDetailsId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.SalesOrderId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.TaxId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.Discount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.Quantity).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.UnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ModifiedOn).IsModified = true;
                                //oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.StockDeductionIds).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.QuantityRemaining).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.WarrantyId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.DefaultUnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.DefaultAmount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.PriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.LotId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.LotType).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.FreeQuantity).IsModified = true;
                                //oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.AmountExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.TaxAmount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.PriceExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.AmountIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.UnitAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ComboId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.IsComboItems).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.QuantitySold).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ComboPerUnitQuantity).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.WarrantyExpiryDate).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ExtraDiscount).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.TaxExemptionId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.ItemCodeId).IsModified = true;
                                oConnectionContext.Entry(oClsSalesOrderDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                                oConnectionContext.SaveChanges();

                                //if (SalesOrders.StockDeductionIds != null)
                                //{
                                //    string ll = "delete from tblSalesOrdersDeductionId where SalesOrderDetailsId=" + SalesOrders.SalesOrderDetailsId;
                                //    oConnectionContext.Database.ExecuteSqlCommand(ll);

                                //    foreach (var l in SalesOrders.StockDeductionIds)
                                //    {
                                //        ClsSalesOrderDeductionId oClsSalesOrderDeductionId = new ClsSalesOrderDeductionId()
                                //        {
                                //            AddedBy = obj.AddedBy,
                                //            AddedOn = CurrentDate,
                                //            CompanyId = obj.CompanyId,
                                //            Id = l.Id,
                                //            Type = l.Type,
                                //            Quantity = l.Quantity,
                                //            SalesOrderDetailsId = SalesOrders.SalesOrderDetailsId,
                                //            SalesOrderId = oClsSalesOrder.SalesOrderId,
                                //        };
                                //        oConnectionContext.DbClsSalesOrderDeductionId.Add(oClsSalesOrderDeductionId);
                                //        oConnectionContext.SaveChanges();
                                //    }
                                //}
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Order",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Order \"" + obj.InvoiceNo + "\" updated",
                    Id = oClsSalesOrder.SalesOrderId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Order", obj.CompanyId, oClsSalesOrder.SalesOrderId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Order updated successfully",
                    Data = new
                    {
                        SalesOrder = new
                        {
                            SalesOrderId = oClsSalesOrder.SalesOrderId,
                            InvoiceId = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == oClsSalesOrder.SalesOrderId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceOrder }).FirstOrDefault(),
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesOrderDetailsDelete(ClsSalesOrderDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.SalesOrderId != 0)
                {
                    string query = "update \"tblSalesOrdersDetails\" set \"IsDeleted\"=True where \"SalesOrderId\"=" + obj.SalesOrderId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblSalesOrdersDetails\" set \"IsDeleted\"=True where \"SalesOrderDetailsId\"=" + obj.SalesOrderDetailsId;
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
        public async Task<IHttpActionResult> Invoice(ClsSalesOrderVm obj)
        {
            var det = oConnectionContext.DbClsSalesOrder.Where(a => a.IsDeleted == false && a.InvoiceId == obj.InvoiceId).Select(a => new
            {
                a.Terms,
                a.OnlinePaymentSettingsId,
                a.SalesOrderId,
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
                a.SalesOrderDate,
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
                //a.SalesOrderType,
                SalesOrderDetails = (from b in oConnectionContext.DbClsSalesOrderDetails
                                     join c in oConnectionContext.DbClsItemDetails
                                     on b.ItemDetailsId equals c.ItemDetailsId
                                     join d in oConnectionContext.DbClsItem
                                     on c.ItemId equals d.ItemId
                                     where b.SalesOrderId == a.SalesOrderId && b.IsDeleted == false
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
                                         b.SalesOrderDetailsId,
                                         b.PriceIncTax,
                                         b.OtherInfo,
                                         b.AmountIncTax,
                                         b.Discount,
                                         b.SalesOrderId,
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
                                         d.ItemCode,
                                         ComboItems = (from bb in oConnectionContext.DbClsSalesOrderDetails
                                                       join cc in oConnectionContext.DbClsItemDetails
                                                       on bb.ItemDetailsId equals cc.ItemDetailsId
                                                       join dd in oConnectionContext.DbClsItem
                                                       on cc.ItemId equals dd.ItemId
                                                       where bb.SalesOrderId == a.SalesOrderId && bb.ComboId == b.ComboId && bb.IsDeleted == false
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
                SalesOrderAdditionalCharges = oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(b => b.SalesOrderId == a.SalesOrderId
&& b.IsDeleted == false && b.IsActive == true).Select(b => new ClsSalesOrderAdditionalChargesVm
{
   SalesOrderAdditionalChargesId = b.SalesOrderAdditionalChargesId,
   Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
   AdditionalChargeId = b.AdditionalChargeId,
   SalesOrderId = b.SalesOrderId,
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

            var AllTaxs = oConnectionContext.DbClsSalesOrder.Where(a => a.IsDeleted == false && a.SalesOrderId == det.SalesOrderId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsSalesOrderDetails.Where(a => a.SalesOrderId == det.SalesOrderId && a.IsDeleted == false
                                && a.IsComboItems == false).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).Concat(oConnectionContext.DbClsSalesOrderAdditionalCharges.Where(a => a.SalesOrderId == det.SalesOrderId
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

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesOrder = det,
                    Taxs = finalTaxs,
                    BusinessSetting = BusinessSetting,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesOrderStatus(ClsSalesOrderVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "Sales Order Status is required", Id = "divSalesOrderStatus" });
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

                ClsSalesOrder oClsSalesOrder = new ClsSalesOrder()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    SalesOrderId = obj.SalesOrderId,
                    Status = obj.Status,
                    PreviousStatus = obj.Status
                };

                oConnectionContext.DbClsSalesOrder.Attach(oClsSalesOrder);
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.SalesOrderId).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSalesOrder).Property(x => x.PreviousStatus).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Order",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Order \"" + oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == obj.SalesOrderId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" status changed to \"" + obj.Status + "\"",
                    Id = oClsSalesOrder.SalesOrderId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Order", obj.CompanyId, oClsSalesOrder.SalesOrderId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Sales Order status changed successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        SalesOrder = new
                        {
                            SalesOrderId = oClsSalesOrder.SalesOrderId,
                            InvoiceId = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId == oClsSalesOrder.SalesOrderId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceOrder }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
