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
using System.Web.Http.Results;
using System.Web.UI;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class PurchaseController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();

        public async Task<IHttpActionResult> AllPurchases(ClsPurchaseVm obj)
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

            List<ClsPurchaseVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
             //&& a.BranchId == obj.BranchId
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsPurchaseVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    NetAmountReverseCharge = a.NetAmountReverseCharge,
                    RoundOffReverseCharge = a.RoundOffReverseCharge,
                    GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                    PurchaseType = a.PurchaseType,
                    BillOfEntryId = oConnectionContext.DbClsBillOfEntry.Where(b => b.PurchaseId == a.PurchaseId && b.IsDeleted == false && b.IsActive == true).Select(b => b.BillOfEntryId).FirstOrDefault(),
                    GstTreatment = a.GstTreatment,
                    IsCancelled = a.IsCancelled,
                    TotalTaxAmount = a.TotalTaxAmount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    InvoiceUrl = oCommonController.webUrl,// + "/purchase/invoice?InvoiceNo=" + a.ReferenceNo+"&Id="+a.CompanyId,
                    TotalQuantity = a.TotalQuantity,
                    PaidQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PurchaseId = a.PurchaseId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    PurchaseDate = a.PurchaseDate,
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
                    Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
                oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    //PaymentStatus = a.PaymentStatus,
                    Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                    //     PurchaseReturnDue = oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false
                    //     && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                    //     .Select(c => c.PurchaseReturnId).FirstOrDefault()).Count() == 0 ?
                    // oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault() :
                    //oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault()
                    //- oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                    // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                    ReferenceType = a.ReferenceType,
                    TotalItems = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId &&
                    c.IsDeleted == false).Count()
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
             && a.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsPurchaseVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    NetAmountReverseCharge = a.NetAmountReverseCharge,
                    RoundOffReverseCharge = a.RoundOffReverseCharge,
                    GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                    PurchaseType = a.PurchaseType,
                    BillOfEntryId = oConnectionContext.DbClsBillOfEntry.Where(b => b.PurchaseId == a.PurchaseId && b.IsDeleted == false && b.IsActive == true).Select(b => b.BillOfEntryId).FirstOrDefault(),
                    GstTreatment = a.GstTreatment,
                    IsCancelled = a.IsCancelled,
                    TotalTaxAmount = a.TotalTaxAmount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    InvoiceUrl = oCommonController.webUrl,// + "/purchase/invoice?InvoiceNo=" + a.ReferenceNo + "&Id=" + a.CompanyId,
                    TotalQuantity = a.TotalQuantity,
                    PaidQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PurchaseId = a.PurchaseId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    PurchaseDate = a.PurchaseDate,
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
                    Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
                oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    //PaymentStatus = a.PaymentStatus,
                    Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false).Count() == 0 ? false : true,
                    PurchaseReturnDue = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false
                    && b.PurchaseId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                    .Select(c => c.PurchaseReturnId).FirstOrDefault()).Count() == 0 ?
                oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault() :
               oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault()
               - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                .Select(c => c.PurchaseReturnId).FirstOrDefault()).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                    ReferenceType = a.ReferenceType,
                    TotalItems = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId &&
                    c.IsDeleted == false).Count()
                }).ToList();
            }

            //if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            //{
            //    if (obj.PaymentStatus.ToLower() == "due")
            //    {
            //        det = det.Where(a => a.PaymentStatus.ToLower() == "partially paid" || a.PaymentStatus.ToLower() == "due" ||
            //        a.PaymentStatus.ToLower() == "overdue").Select(a => a).ToList();
            //    }
            //    else if (obj.PaymentStatus.ToLower() == "payment")
            //    {
            //        det = det.Where(a => a.PaymentStatus.ToLower() == "partially paid" || a.PaymentStatus.ToLower() == "paid").Select(a => a).ToList();
            //    }
            //    else
            //    {
            //        det = det.Where(a => a.PaymentStatus.ToLower() == obj.PaymentStatus.ToLower()).Select(a => a).ToList();
            //    }
            //}

            if (obj.From == "dashboard")
            {
                det = det.Where(a => a.Status != "Draft").Select(a => a).ToList();
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
                    Purchases = det.OrderByDescending(a => a.PurchaseId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> Purchase(ClsPurchaseVm obj)
        {
            var det = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseVm
            {
                BillOfEntryId = oConnectionContext.DbClsBillOfEntry.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.BillOfEntryId).FirstOrDefault(),
                TaxableAmount = a.TaxableAmount,
                NetAmountReverseCharge = a.NetAmountReverseCharge,
                RoundOffReverseCharge = a.RoundOffReverseCharge,
                GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                IsCancelled = a.IsCancelled,
                IsReverseCharge = a.IsReverseCharge,
                BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                SourceOfSupplyId = a.SourceOfSupplyId,
                DestinationOfSupplyId = a.DestinationOfSupplyId,
                TotalTaxAmount = a.TotalTaxAmount,
                InvoiceId = a.InvoiceId,
                //BranchName = oConnectionContext.DbClsBranch.Where(b=>b.BranchId == a.BranchId).Select(b=>b.Branch).FirstOrDefault(),
                //SupplierName = oConnectionContext.DbClsUser.Where(b=>b.UserId == a.SupplierId).Select(b=>b.Name).FirstOrDefault(),
                //Tax = oConnectionContext.DbClsTax.Where(b=>b.TaxId == a.TaxId).Select(b=>b.Tax).FirstOrDefault(),
                //TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                ExchangeRate = a.ExchangeRate,
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.SupplierId).Select(e => e.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                DefaultCurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(c => c.IsMain == true && c.CompanyId == obj.CompanyId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                TotalDiscount = a.TotalDiscount,
                AttachDocument = a.AttachDocument,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                BranchId = a.BranchId,
                CompanyId = a.CompanyId,
                DeliveredTo = a.DeliveredTo,
                Discount = a.Discount,
                DiscountType = a.DiscountType,
                GrandTotal = a.GrandTotal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                Notes = a.Notes,
                //PayTerm = a.PayTerm,
                //PayTermNo = a.PayTermNo,
                PaymentTermId = a.PaymentTermId,
                DueDate = a.DueDate,
                Due = a.GrandTotal - (oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                PurchaseDate = a.PurchaseDate,
                PurchaseId = a.PurchaseId,
                Status = a.Status,
                ReferenceNo = a.ReferenceNo,
                ShippingAddress = a.ShippingAddress,
                ShippingDetails = a.ShippingDetails,
                ShippingStatus = a.ShippingStatus,
                Subtotal = a.Subtotal,
                SupplierId = a.SupplierId,
                TaxId = a.TaxId,
                TaxAmount = a.TaxAmount,
                TotalQuantity = a.TotalQuantity,
                SmsSettingsId = a.SmsSettingsId,
                EmailSettingsId = a.EmailSettingsId,
                WhatsappSettingsId = a.WhatsappSettingsId,
                RoundOff = a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                NetAmount = a.NetAmount,
                Supplier = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.GstTreatment).FirstOrDefault(),
                ReferenceId = a.ReferenceId,
                //Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                //    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                //    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                ReferenceType = a.ReferenceType,
                PurchaseDetails = (from b in oConnectionContext.DbClsPurchaseDetails
                                   join c in oConnectionContext.DbClsItemDetails
                                   on b.ItemDetailsId equals c.ItemDetailsId
                                   join d in oConnectionContext.DbClsItem
                                   on c.ItemId equals d.ItemId
                                   where b.PurchaseId == a.PurchaseId && b.IsDeleted == false
                                   select new ClsPurchaseDetailsVm
                                   {
                                       TotalTaxAmount = b.TotalTaxAmount,
                                       TaxExemptionId = b.TaxExemptionId,
                                       ITCType = b.ITCType,
                                       ItemCodeId = b.ItemCodeId,
                                       ItemType = d.ItemType,
                                       ExtraDiscount = b.ExtraDiscount,
                                       Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                       FreeQuantity = b.FreeQuantity,
                                       //b.FreeQuantityPriceAddedFor,
                                       PurchaseExcTax = b.PurchaseExcTax,
                                       PurchaseIncTax = b.PurchaseIncTax,
                                       LotNo = b.LotNo,
                                       ExpiryDate = b.ExpiryDate,
                                       ManufacturingDate = b.ManufacturingDate,
                                       QuantitySold = b.QuantitySold,
                                       QuantityRemaining = b.QuantityRemaining,
                                       PurchaseDetailsId = b.PurchaseDetailsId,
                                       AmountExcTax = b.AmountExcTax,
                                       AmountIncTax = b.AmountIncTax,
                                       Discount = b.Discount,
                                       Quantity = b.Quantity,
                                       TaxId = b.TaxId,
                                       UnitCost = b.UnitCost,
                                       ItemId = d.ItemId,
                                       ProductType = d.ProductType,
                                       ItemDetailsId = c.ItemDetailsId,
                                       ItemName = d.ItemName,
                                       SKU = c.SKU == null ? d.SkuCode : c.SKU,
                                       VariationDetailsId = c.VariationDetailsId,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                       SalesExcTax = b.SalesExcTax,
                                       SalesIncTax = b.SalesIncTax,
                                       TotalCost = c.TotalCost,
                                       Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                       TaxType = d.TaxType,
                                       ItemCode = d.ItemCode,
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
                                       TaxAmount = b.TaxAmount,
                                       DiscountType = b.DiscountType,
                                       DefaultProfitMargin = b.DefaultProfitMargin,
                                       Mrp = b.Mrp
                                   }).ToList(),
               PurchaseAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                    ).Select(b => new ClsPurchaseAdditionalChargesVm
                    {
                        ITCType = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ITCType).FirstOrDefault(),
                        PurchaseAdditionalChargesId = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.PurchaseAdditionalChargesId).FirstOrDefault(),
                        Name = b.Name,
                        AdditionalChargeId = b.AdditionalChargeId,
                        PurchaseId = a.PurchaseId,
                        TaxId = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                        AmountExcTax = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                        AmountIncTax = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                        TaxAmount = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                        AccountId = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == a.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
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

            var AllTaxs = oConnectionContext.DbClsPurchase.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId == det.PurchaseId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == det.PurchaseId && a.IsDeleted == false).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.AmountExcTax
            })).Concat(oConnectionContext.DbClsPurchaseAdditionalCharges.Where(a => a.PurchaseId == det.PurchaseId
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
                    Purchase = det,
                    Taxs = finalTaxs,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPurchase(ClsPurchaseVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;

                long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2).Select(a => a.TransactionId).FirstOrDefault();

                var Transaction = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => new ClsTransactionVm
                {
                    StartDate = a.StartDate,
                    ExpiryDate = a.ExpiryDate,
                }).FirstOrDefault();

                int TotalPurchaseBillUsed = oConnectionContext.DbClsPurchase.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();

                int TotalExpenseUsed = oConnectionContext.DbClsExpense.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();

                int TotalOrder = oCommonController.fetchPlanQuantity(obj.CompanyId, "Bill");
                if ((TotalPurchaseBillUsed + TotalExpenseUsed) >= TotalOrder)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Purchase Bill/ Expense quota already used. Please upgrade addons from My Plan Menu",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.SupplierId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
                    isError = true;
                }

                if (obj.PurchaseDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseDate" });
                    isError = true;
                }

                if (obj.DueDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDueDate" });
                    isError = true;
                }

                if (obj.PurchaseType == "Credit Note")
                {
                    if (obj.ParentId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseInvoice" });
                        isError = true;
                    }
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseStatus" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsPurchase.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Purchase Bill# exists", Id = "divPurchaseReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.PurchaseDetails == null || obj.PurchaseDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                //if (obj.Payment != null)
                //{
                //    if (obj.Payment.Amount != 0 || obj.Payment.PaymentDate != DateTime.MinValue || obj.Payment.PaymentTypeId != 0)
                //    {
                //        if (obj.Payment.Amount == 0)
                //        {
                //            errors.Add(new ClsError { Message = "This field is required", Id = "divPAmount" });
                //            isError = true;
                //        }
                //        if (obj.Payment.PaymentDate == DateTime.MinValue)
                //        {
                //            errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
                //            isError = true;
                //        }
                //        if (obj.Payment.PaymentTypeId == 0)
                //        {
                //            errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                //            isError = true;
                //        }
                //        if (obj.Payment.PaymentTypeId != 0)
                //        {
                //            string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.Payment.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                //            if (_paymentType.ToLower() == "advance")
                //            {
                //                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.AdvanceBalance).FirstOrDefault() < obj.Payment.Amount)
                //                {
                //                    data = new
                //                    {
                //                        Status = 0,
                //                        Message = "Not enough advance balance",
                //                        Data = new
                //                        {
                //                        }
                //                    };
                //                    return await Task.FromResult(Ok(data));
                //                }
                //            }
                //        }
                //    }
                //}

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays, a.EnableLotNo }).FirstOrDefault();

                if (obj.PurchaseDetails != null)
                {
                    foreach (var item in obj.PurchaseDetails)
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
                                    //else
                                    //{
                                    //    int lotCount = (from a in oConnectionContext.DbClsPurchase
                                    //                    join b in oConnectionContext.DbClsPurchaseDetails
                                    //                    on a.PurchaseId equals b.PurchaseId
                                    //                    where a.SupplierId == obj.SupplierId
                                    //                    && b.ItemDetailsId == item.ItemDetailsId
                                    //                    && a.IsDeleted == false && a.IsCancelled == false
                                    //                    && b.IsDeleted == false && b.LotNo == item.LotNo
                                    //                    select b.LotNo).Count();
                                    //    if (lotCount > 0)
                                    //    {
                                    //        errors.Add(new ClsError { Message = "Lot No already exists", Id = "divLotNo" + item.DivId });
                                    //        isError = true;
                                    //    }

                                    //    // Check for duplicate LotNo within the same list
                                    //    int duplicateCount = obj.PurchaseDetails
                                    //        .Where(x => x.IsDeleted == false
                                    //                    && x.ItemId == item.ItemId && x.ItemDetailsId == item.ItemDetailsId
                                    //                    && x.LotNo == item.LotNo
                                    //                    && x.DivId != item.DivId) // Exclude self
                                    //        .Count();

                                    //    if (duplicateCount > 0)
                                    //    {
                                    //        errors.Add(new ClsError { Message = "Duplicate Lot No in this purchase", Id = "divLotNo" + item.DivId });
                                    //        isError = true;
                                    //    }
                                    //}
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
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "purchase"
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

                decimal due = 0;
                if (obj.Payment != null)
                {
                    if (obj.Payment.Amount != 0 && obj.Payment.PaymentDate != DateTime.MinValue && obj.Payment.PaymentTypeId != 0)
                    {
                        if (obj.GrandTotal == obj.Payment.Amount)
                        {
                            obj.Status = "Paid";
                        }
                        else if (obj.GrandTotal > obj.Payment.Amount)
                        {
                            obj.Status = "Partially Paid";
                            due = obj.GrandTotal - obj.Payment.Amount;
                        }
                        else
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "Payment amount cannot be greater than Grand Total",
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                    }
                    else if (obj.Payment.Amount == 0 && obj.Payment.PaymentDate == DateTime.MinValue || obj.Payment.PaymentTypeId == 0)
                    {
                        due = obj.GrandTotal;
                    }
                    else
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Payment Amount, Paid On and Payment Method is required",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // check credit limit
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalPurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.SupplierId == obj.SupplierId
                                            ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsPurchase
                                             join b in oConnectionContext.DbClsSupplierPayment
                                         on a.PurchaseId equals b.PurchaseId
                                             where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.SupplierId == obj.SupplierId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if (obj.Status != "Draft")
                        {
                            if ((TotalPurchaseDue + due) > creditLimit)
                            {
                                data = new
                                {
                                    Status = 4,
                                    Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalPurchaseDue)),
                                    Data = new
                                    {
                                        User = new
                                        {
                                            CreditLimit = creditLimit,
                                            TotalPurchaseDue = TotalPurchaseDue,
                                            TotalPurchase = due,
                                            UserId = obj.SupplierId
                                        }
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }

                    }
                }
                // check credit limit

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                ClsPurchase oClsPurchase = new ClsPurchase()
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
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.DueDate,
                    PurchaseDate = obj.PurchaseDate.AddHours(5).AddMinutes(30),
                    PurchaseId = obj.PurchaseId,
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
                    ExchangeRate = obj.ExchangeRate,
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    InvoiceId = oCommonController.CreateToken(),
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //ShippingChargesAccountId = ShippingChargesAccountId,
                    //PackagingChargesAccountId = PackagingChargesAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
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
                    IsCancelled = false,
                    PrefixId = PrefixId,
                    PurchaseType = obj.PurchaseType,
                    ParentId = obj.ParentId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Purchase/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Purchase/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPurchase.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsPurchase.Add(oClsPurchase);
                oConnectionContext.SaveChanges();

                if (obj.PurchaseAdditionalCharges != null)
                {
                    foreach (var item in obj.PurchaseAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.PurchaseAccountId }).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        //decimal AmountExcTax = obj.IsReverseCharge == 1 ? Purchase.AmountIncTax : Purchase.AmountExcTax;
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

                        if (obj.CountryId == 2)
                        {
                            //if (Purchase.ITCType == "Eligible For ITC")
                            //{
                            //    if (obj.IsReverseCharge == 1)
                            //    {
                            //        AccountType = "Reverse Charge Tax Input but not due";

                            //        taxList = finalTaxs.Select(a => new ClsTaxVm
                            //        {
                            //            TaxType = "Reverse Charge",
                            //            TaxId = a.TaxId,
                            //            TaxPercent = a.TaxPercent,
                            //            TaxAmount = a.TaxAmount,
                            //            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                            //        }).ToList();
                            //    }
                            //    else
                            //    {
                            //        taxList = finalTaxs.Select(a => new ClsTaxVm
                            //        {
                            //            TaxType = "Normal",
                            //            TaxId = a.TaxId,
                            //            TaxPercent = a.TaxPercent,
                            //            TaxAmount = a.TaxAmount,
                            //            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                            //        }).ToList();
                            //    }
                            //}
                            //else
                            //{
                            if (obj.IsReverseCharge == 1)
                            {
                                AccountType = "Tax Paid Expense";

                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                {
                                    TaxType = "Reverse Charge",
                                    TaxId = a.TaxId,
                                    TaxPercent = a.TaxPercent,
                                    TaxAmount = a.TaxAmount,
                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                }).ToList();
                            }
                            else
                            {
                                AccountType = "Tax Paid Expense";
                            }
                            //}
                        }
                        else
                        {
                            taxList = finalTaxs.Select(a => new ClsTaxVm
                            {
                                TaxType = "Normal",
                                TaxId = a.TaxId,
                                TaxPercent = a.TaxPercent,
                                TaxAmount = a.TaxAmount,
                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                            }).ToList();
                        }

                        ClsPurchaseAdditionalCharges oClsPurchaseAdditionalCharges = new ClsPurchaseAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            PurchaseId = oClsPurchase.PurchaseId,
                            TaxId = item.TaxId,
                            AmountExcTax = item.AmountExcTax,
                            AmountIncTax = item.AmountIncTax,
                            TaxAmount = item.AmountIncTax - item.AmountExcTax,
                            AccountId = AdditionalCharge.PurchaseAccountId,
                            ItemCodeId = AdditionalCharge.ItemCodeId,
                            ITCType = item.ITCType,
                            TaxExemptionId = item.TaxExemptionId,
                            IsActive = item.IsActive,
                            IsDeleted = item.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsPurchaseAdditionalCharges.Add(oClsPurchaseAdditionalCharges);
                        oConnectionContext.SaveChanges();

                        if (AccountType != "")
                        {
                            AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                            ClsPurchaseAdditionalTaxJournal oClsPurchaseAdditionalTaxJournal = new ClsPurchaseAdditionalTaxJournal()
                            {
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseAdditionalChargesId = oClsPurchaseAdditionalCharges.PurchaseAdditionalChargesId,
                                TaxId = item.TaxId,
                                TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : (item.AmountIncTax - item.AmountExcTax),
                                AccountId = AccountId,
                                PurchaseTaxJournalType = "Normal"
                            };
                            oConnectionContext.DbClsPurchaseAdditionalTaxJournal.Add(oClsPurchaseAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsPurchaseAdditionalTaxJournal oClsPurchaseAdditionalTaxJournal = new ClsPurchaseAdditionalTaxJournal()
                            {
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseAdditionalChargesId = oClsPurchaseAdditionalCharges.PurchaseAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                PurchaseTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsPurchaseAdditionalTaxJournal.Add(oClsPurchaseAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.PurchaseDetails != null)
                {
                    foreach (var Purchase in obj.PurchaseDetails)
                    {
                        bool IsStopSelling = false, flag = false;
                        decimal convertedStock = oCommonController.StockConversion(Purchase.Quantity, Purchase.ItemId, Purchase.PriceAddedFor);
                        decimal freeConvertedStock = oCommonController.StockConversion(Purchase.FreeQuantity, Purchase.ItemId, Purchase.PriceAddedFor);

                        if (Purchase.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                        {
                            if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                            {
                                IsStopSelling = true;
                            }
                        }

                        long PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Purchase.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                        //long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Purchase.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == Purchase.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                        //long ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == Purchase.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == Purchase.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        //decimal AmountExcTax = obj.IsReverseCharge == 1 ? Purchase.AmountIncTax : Purchase.AmountExcTax;
                        decimal AmountExcTax = Purchase.AmountExcTax;
                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Purchase.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == Purchase.TaxId
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

                        if (obj.CountryId == 2)
                        {
                            if (Purchase.ITCType == "Eligible For ITC")
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Reverse Charge Tax Input but not due";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Normal",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                            }
                            else
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Tax Paid Expense";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    AccountType = "Tax Paid Expense";
                                }
                            }
                        }
                        else
                        {
                            taxList = finalTaxs.Select(a => new ClsTaxVm
                            {
                                TaxType = "Normal",
                                TaxId = a.TaxId,
                                TaxPercent = a.TaxPercent,
                                TaxAmount = a.TaxAmount,
                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                            }).ToList();
                        }

                        Purchase.DefaultProfitMargin = ((Purchase.SalesExcTax - Purchase.UnitCost) / Purchase.UnitCost) * 100;

                        ClsPurchaseDetails oClsPurchaseDetails = new ClsPurchaseDetails()
                        {
                            ITCType = Purchase.ITCType,
                            AmountExcTax = Purchase.AmountExcTax,
                            AmountIncTax = Purchase.AmountIncTax,
                            ItemId = Purchase.ItemId,
                            ItemDetailsId = Purchase.ItemDetailsId,
                            PurchaseId = oClsPurchase.PurchaseId,
                            PurchaseExcTax = Purchase.PurchaseExcTax,
                            PurchaseIncTax = Purchase.PurchaseIncTax,
                            //Tax = Purchase.Tax,
                            TaxId = Purchase.TaxId,
                            Discount = Purchase.Discount,
                            Quantity = Purchase.Quantity,
                            UnitCost = Purchase.UnitCost,
                            IsActive = Purchase.IsActive,
                            IsDeleted = Purchase.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            QuantityRemaining = convertedStock + freeConvertedStock,
                            QuantitySold = 0,
                            LotNo = Purchase.LotNo,
                            ExpiryDate = Purchase.ExpiryDate != null ? Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) : Purchase.ExpiryDate,
                            ManufacturingDate = Purchase.ManufacturingDate != null ? Purchase.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : Purchase.ManufacturingDate,
                            PriceAddedFor = Purchase.PriceAddedFor,
                            SalesExcTax = Purchase.SalesExcTax,
                            SalesIncTax = Purchase.SalesIncTax,
                            FreeQuantity = Purchase.FreeQuantity,
                            //FreeQuantityPriceAddedFor = Purchase.FreeQuantityPriceAddedFor,
                            IsStopSelling = IsStopSelling,
                            TaxAmount = Purchase.TaxAmount,
                            DiscountType = Purchase.DiscountType,
                            DefaultProfitMargin = Purchase.DefaultProfitMargin,
                            UnitAddedFor = Purchase.UnitAddedFor,
                            QuantityPurchased = convertedStock + freeConvertedStock,
                            Mrp = Purchase.Mrp,
                            AccountId = PurchaseAccountId,
                            DiscountAccountId = DiscountAccountId,
                            TaxAccountId = TaxAccountId,
                            ExtraDiscount = Purchase.ExtraDiscount,
                            ItemCodeId = Purchase.ItemCodeId,
                            TaxExemptionId = Purchase.TaxExemptionId,
                            TotalTaxAmount = Purchase.TotalTaxAmount
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsPurchaseDetails.Add(oClsPurchaseDetails);
                        oConnectionContext.SaveChanges();

                        if (AccountType != "")
                        {
                            AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                            ClsPurchaseTaxJournal oClsPurchaseTaxJournal = new ClsPurchaseTaxJournal()
                            {
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseDetailsId = oClsPurchaseDetails.PurchaseDetailsId,
                                TaxId = Purchase.TaxId,
                                TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : Purchase.TaxAmount,
                                AccountId = AccountId,
                                PurchaseTaxJournalType = "Normal"
                            };
                            oConnectionContext.DbClsPurchaseTaxJournal.Add(oClsPurchaseTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsPurchaseTaxJournal oClsPurchaseTaxJournal = new ClsPurchaseTaxJournal()
                            {
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseDetailsId = oClsPurchaseDetails.PurchaseDetailsId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                PurchaseTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsPurchaseTaxJournal.Add(oClsPurchaseTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        if (IsStopSelling == false)
                        {
                            if (obj.Status.ToLower() != "draft")
                            {
                                string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }

                if (obj.Payment != null)
                {
                    if (obj.Payment.Amount != 0)
                    {
                        List<ClsSupplierPaymentVm> oAdvanceBalances = new List<ClsSupplierPaymentVm>();

                        string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.Payment.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                        if (_paymentType == "Advance")
                        {
                            //if (obj.Type.ToLower() == "purchase payment")
                            //{
                            decimal AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.AdvanceBalance).FirstOrDefault();
                            if (AdvanceBalance < obj.Payment.Amount)
                            {
                                errors.Add(new ClsError { Message = "Insuffcient Advance Balance", Id = "divAmount" });
                                isError = true;
                            }
                            else
                            {
                                var availableAdvances = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierId == obj.SupplierId && a.IsDeleted == false && a.IsCancelled == false &&
                            a.IsActive == true && a.Type.ToLower() == "supplier advance payment" && a.AmountRemaining > 0).Select(a => new
                            {
                                a.ParentId,
                                a.SupplierPaymentId,
                                a.AmountRemaining
                            }).ToList();

                                decimal amountRemaininToDeduct = obj.Payment.Amount;
                                foreach (var item in availableAdvances)
                                {
                                    if (amountRemaininToDeduct != 0)
                                    {
                                        decimal availableAmount = item.AmountRemaining;
                                        decimal amount = 0;
                                        if (availableAmount >= amountRemaininToDeduct)
                                        {
                                            amount = amountRemaininToDeduct;
                                        }
                                        else if (availableAmount < amountRemaininToDeduct)
                                        {
                                            amount = availableAmount;
                                        }

                                        string _query1 = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"SupplierPaymentId\"=" + item.SupplierPaymentId;
                                        oConnectionContext.Database.ExecuteSqlCommand(_query1);

                                        amountRemaininToDeduct = amountRemaininToDeduct - amount;

                                        oAdvanceBalances.Add(new ClsSupplierPaymentVm { PurchaseId = obj.PurchaseId, SupplierPaymentId = item.SupplierPaymentId, Amount = amount, ParentId = item.ParentId });
                                    }
                                }

                                //long SupplierId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.Id).Select(a => a.SupplierId).FirstOrDefault();
                                string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Payment.Amount + " where \"UserId\"=" + obj.SupplierId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                                //}
                            }
                        }

                        //long PrefixId1 = 0;
                        if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                        {
                            //long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                  join b in oConnectionContext.DbClsPrefixUserMap
                                                   on a.PrefixMasterId equals b.PrefixMasterId
                                                  where a.IsActive == true && a.IsDeleted == false &&
                                                  b.CompanyId == obj.CompanyId && b.IsActive == true
                                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "payment"
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

                        long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                        && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

                        if (oAdvanceBalances != null && oAdvanceBalances.Count > 0)
                        {
                            obj.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                       && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                            foreach (var l in oAdvanceBalances)
                            {
                                ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = obj.Notes,
                                    Amount = l.Amount,
                                    PaymentDate = obj.Payment.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.Payment.PaymentTypeId,
                                    PurchaseId = oClsPurchase.PurchaseId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = "Purchase Payment",
                                    BranchId = obj.BranchId,
                                    AccountId = obj.AccountId,
                                    ReferenceNo = obj.ReferenceNo,
                                    IsDebit = 1,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ReferenceId = oCommonController.CreateToken(),
                                    SupplierId = obj.SupplierId,
                                    IsDirectPayment = true,
                                    JournalAccountId = JournalAccountId,
                                    ParentId = l.ParentId,
                                    PrefixId = PrefixId
                                };
                                oConnectionContext.DbClsSupplierPayment.Add(oClsPayment);
                                oConnectionContext.SaveChanges();

                                //ClsSupplierPaymentDeductionId _oClsSupplierPaymentDeductionId = new ClsSupplierPaymentDeductionId()
                                //{
                                //    AddedBy = obj.AddedBy,
                                //    AddedOn = CurrentDate,
                                //    CompanyId = obj.CompanyId,
                                //    DeductedFromId = l.SupplierPaymentId,
                                //    Amount = l.Amount,
                                //    PurchaseId = l.PurchaseId,
                                //    SupplierPaymentId = oClsPayment.SupplierPaymentId,
                                //    SupplierId = obj.SupplierId
                                //};
                                //oConnectionContext.DbClsSupplierPaymentDeductionId.Add(_oClsSupplierPaymentDeductionId);
                                //oConnectionContext.SaveChanges();
                            }
                        }
                        else
                        {
                            ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                IsActive = obj.IsActive,
                                IsDeleted = obj.IsDeleted,
                                Notes = obj.Notes,
                                Amount = obj.Payment.Amount,
                                PaymentDate = obj.Payment.PaymentDate.AddHours(5).AddMinutes(30),
                                PaymentTypeId = obj.Payment.PaymentTypeId,
                                PurchaseId = oClsPurchase.PurchaseId,
                                AttachDocument = obj.AttachDocument,
                                Type = "Purchase Payment",
                                BranchId = obj.BranchId,
                                AccountId = obj.AccountId,
                                ReferenceNo = obj.ReferenceNo,
                                IsDebit = 1,
                                //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                ReferenceId = oCommonController.CreateToken(),
                                SupplierId = obj.SupplierId,
                                IsDirectPayment = true,
                                JournalAccountId = JournalAccountId,
                                PrefixId = PrefixId
                            };
                            if (obj.AttachDocument != "" && obj.AttachDocument != null)
                            {
                                string filepathPass = "";

                                filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                                string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                                byte[] imageCheque = Convert.FromBase64String(base64);
                                Stream strm = new MemoryStream(imageCheque);
                                var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                                var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Payment/AttachDocument");
                                if (!Directory.Exists(folder))
                                {
                                    Directory.CreateDirectory(folder);
                                }

                                //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                                //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                                oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                                oClsPayment.AttachDocument = filepathPass;
                            }
                            oConnectionContext.DbClsSupplierPayment.Add(oClsPayment);
                            oConnectionContext.SaveChanges();
                        }

                        //increase counter
                        string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                        //increase counter

                        //ClsActivityLogVm oClsActivityLogVm1 = new ClsActivityLogVm()
                        //{
                        //    AddedBy = obj.AddedBy,
                        //    Browser = obj.Browser,
                        //    Category = "Purchase Payment",
                        //    CompanyId = obj.CompanyId,
                        //    Description = "added payment of " + obj.Payment.Amount + " for " + obj.ReferenceNo,
                        //    Id = oClsPayment.SupplierPaymentId,
                        //    IpAddress = obj.IpAddress,
                        //    Platform = obj.Platform,
                        //    Type = "Insert"
                        //};
                        //oCommonController.InsertActivityLog(oClsActivityLogVm1, CurrentDate);
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
                    Category = "Purchase",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Bill \"" + obj.ReferenceNo + "\" created",
                    Id = oClsPurchase.PurchaseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (obj.ReferenceType != "" && obj.ReferenceType != null)
                {
                    if (obj.ReferenceType == "purchase quotation")
                    {
                        string qq = "update \"tblPurchaseQuotation\" set \"Status\"='Invoiced' where \"PurchaseQuotationId\"=" + obj.ReferenceId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                    }
                    else if (obj.ReferenceType == "purchase order")
                    {
                        string qq = "update \"tblPurchaseOrder\" set \"Status\"='Invoiced' where \"PurchaseOrderId\"=" + obj.ReferenceId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);

                        qq = "update \"tblPurchaseQuotation\" set \"Status\"='Invoiced' where \"PurchaseQuotationId\"=(select \"ReferenceId\" from \"tblPurchaseOrder\" where \"PurchaseOrderId\"=" + obj.ReferenceId + ")";
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                    }
                }

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill", obj.CompanyId, oClsPurchase.PurchaseId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Purchase = new
                        {
                            PurchaseId = oClsPurchase.PurchaseId,
                            InvoiceId = oClsPurchase.InvoiceId
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseBill }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDelete(ClsPurchaseVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int PurchaseReturnCount = oConnectionContext.DbClsPurchaseReturn.Where(a => a.CompanyId == obj.CompanyId && a.PurchaseId == obj.PurchaseId && a.IsDeleted == false && a.IsCancelled == false && a.IsCancelled == false).Count();
                if (PurchaseReturnCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Return exist for the transaction, edit the return instead",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.BranchId).FirstOrDefault();
                var details = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == obj.PurchaseId && a.IsDeleted == false).Select(a => new
                {
                    a.IsStopSelling,
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    a.FreeQuantity,
                    a.QuantityRemaining,
                    a.PriceAddedFor,
                    ReturnedQuantity = oConnectionContext.DbClsPurchaseReturnDetails.Where(b => b.PurchaseDetailsId == a.PurchaseDetailsId).Select(b => b.Quantity).DefaultIfEmpty().FirstOrDefault(),
                    IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                }).ToList();

                foreach (var item in details)
                {
                    if (item.IsPurchaseReturn == true)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Return exist for the transaction, edit the return instead",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    decimal freeConvertedStock = oCommonController.StockConversion(item.FreeQuantity, item.ItemId, item.PriceAddedFor);

                    if ((convertedStock + freeConvertedStock) != item.QuantityRemaining)
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

                ClsPurchase oClsPurchase = new ClsPurchase()
                {
                    PurchaseId = obj.PurchaseId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchase.Attach(oClsPurchase);
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PurchaseId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    if (item.IsStopSelling == false)
                    {
                        decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                        decimal freeConvertedStock = oCommonController.StockConversion(item.FreeQuantity, item.ItemId, item.PriceAddedFor);
                        query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                var paymentDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == obj.CompanyId && a.PurchaseId == obj.PurchaseId &&
                 a.Type.ToLower() == "purchase payment" && a.IsDeleted == false && a.IsCancelled == false).Select(a => new
                 {
                     a.SupplierPaymentId,
                     a.PurchaseId,
                     a.Type,
                     a.Amount,
                     PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault(),
                     SupplierId = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == obj.PurchaseId).Select(b => b.SupplierId).FirstOrDefault()
                 }).ToList();

                foreach (var item in paymentDetails)
                {
                    ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                    {
                        SupplierPaymentId = item.SupplierPaymentId,
                        IsDeleted = true,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
                    oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    if (item.PaymentType == "Advance")
                    {
                        //var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => new
                        //{
                        //    a.SupplierPaymentDeductionId,
                        //    a.SupplierPaymentId,
                        //    a.DeductedFromId,
                        //    a.Amount
                        //}).ToList();

                        var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => new
                        {
                            a.ParentId,
                            a.Amount
                        }).ToList();

                        foreach (var inner in SupplierPaymentDeductionIds)
                        {
                            string q = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"SupplierPaymentId\"=" + inner.ParentId;
                            oConnectionContext.Database.ExecuteSqlCommand(q);

                            //q = "update \"tblSupplierPaymentDeductionId\" set \"isdeleted\"=True where \"SupplierPaymentDeductionId\"=" + inner.ParentId;
                            //oConnectionContext.Database.ExecuteSqlCommand(q);
                        }
                        string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + item.Amount + " where \"UserId\"=" + item.SupplierId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                var Purchase = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new
                {
                    a.ReferenceId,
                    a.ReferenceType
                }).FirstOrDefault();

                if (Purchase.ReferenceType != "" && Purchase.ReferenceType != null)
                {
                    if (Purchase.ReferenceType == "purchase quotation")
                    {
                        if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Purchase.ReferenceId
                        && a.ReferenceType == Purchase.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId != obj.PurchaseId).Count() == 0)
                        {
                            if (oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Purchase.ReferenceId && a.ReferenceType == "purchase quotation").Count() == 0)
                            {
                                string qq = "update \"tblPurchaseQuotation\" set \"Status\"='Accepted' where \"PurchaseQuotationId\"=" + Purchase.ReferenceId;
                                oConnectionContext.Database.ExecuteSqlCommand(qq);
                            }
                        }
                    }
                    else if (Purchase.ReferenceType == "purchase order")
                    {
                        if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Purchase.ReferenceId
                        && a.ReferenceType == Purchase.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId != obj.PurchaseId).Count() == 0)
                        {
                            string qq = "update \"tblPurchaseOrder\" set \"Status\"='Confirmed' where \"PurchaseOrderId\"=" + Purchase.ReferenceId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);

                            long PurchaseOrderRefId = oConnectionContext.DbClsPurchaseOrder.Where(b =>
                            b.PurchaseOrderId == Purchase.ReferenceId && b.ReferenceType == Purchase.ReferenceType).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == PurchaseOrderRefId
                        && a.ReferenceType == "purchase quotation" && a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId != obj.PurchaseId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceType == "purchase quotation" && a.ReferenceId == PurchaseOrderRefId && a.PurchaseOrderId != Purchase.ReferenceId).Count() == 0)
                                {
                                    qq = "update \"tblPurchaseQuotation\" set \"Status\"='Accepted' where \"PurchaseQuotationId\"=" + PurchaseOrderRefId;
                                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Bill \"" + oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPurchase.PurchaseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseCancel(ClsPurchaseVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //if (oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseId == obj.PurchaseId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "Return exist for the transaction, edit the return instead",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}

                obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.BranchId).FirstOrDefault();
                var details = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == obj.PurchaseId && a.IsDeleted == false).Select(a => new
                {
                    a.IsStopSelling,
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    a.FreeQuantity,
                    a.QuantityRemaining,
                    a.PriceAddedFor,
                    ReturnedQuantity = oConnectionContext.DbClsPurchaseReturnDetails.Where(b => b.PurchaseDetailsId == a.PurchaseDetailsId).Select(b => b.Quantity).DefaultIfEmpty().FirstOrDefault(),
                    IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                }).ToList();

                foreach (var item in details)
                {
                    if (item.IsPurchaseReturn == true)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Return exist for the transaction, edit the return instead",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    decimal freeConvertedStock = oCommonController.StockConversion(item.FreeQuantity, item.ItemId, item.PriceAddedFor);

                    if ((convertedStock + freeConvertedStock) != item.QuantityRemaining)
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

                ClsPurchase oClsPurchase = new ClsPurchase()
                {
                    PurchaseId = obj.PurchaseId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchase.Attach(oClsPurchase);
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PurchaseId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    if (item.IsStopSelling == false)
                    {
                        decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                        decimal freeConvertedStock = oCommonController.StockConversion(item.FreeQuantity, item.ItemId, item.PriceAddedFor);
                        query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                var paymentDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == obj.CompanyId && a.PurchaseId == obj.PurchaseId &&
                 a.Type.ToLower() == "purchase payment" && a.IsDeleted == false && a.IsCancelled == false).Select(a => new
                 {
                     a.SupplierPaymentId,
                     a.PurchaseId,
                     a.Type,
                     a.Amount,
                     PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault(),
                     SupplierId = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == obj.PurchaseId).Select(b => b.SupplierId).FirstOrDefault()
                 }).ToList();

                foreach (var item in paymentDetails)
                {
                    ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                    {
                        SupplierPaymentId = item.SupplierPaymentId,
                        IsDeleted = true,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
                    oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    if (item.PaymentType == "Advance")
                    {
                        //var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => new
                        //{
                        //    a.SupplierPaymentDeductionId,
                        //    a.SupplierPaymentId,
                        //    a.DeductedFromId,
                        //    a.Amount
                        //}).ToList();

                        var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => new
                        {
                            a.ParentId,
                            a.Amount
                        }).ToList();

                        foreach (var inner in SupplierPaymentDeductionIds)
                        {
                            string q = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"SupplierPaymentId\"=" + inner.ParentId;
                            oConnectionContext.Database.ExecuteSqlCommand(q);

                            //q = "update \"tblSupplierPaymentDeductionId\" set \"isdeleted\"=True where \"SupplierPaymentDeductionId\"=" + inner.ParentId;
                            //oConnectionContext.Database.ExecuteSqlCommand(q);
                        }
                        string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + item.Amount + " where \"UserId\"=" + item.SupplierId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                var Purchase = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new
                {
                    a.ReferenceId,
                    a.ReferenceType
                }).FirstOrDefault();

                if (Purchase.ReferenceType != "" && Purchase.ReferenceType != null)
                {
                    if (Purchase.ReferenceType == "purchase quotation")
                    {
                        if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Purchase.ReferenceId
                        && a.ReferenceType == Purchase.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId != obj.PurchaseId).Count() == 0)
                        {
                            if (oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Purchase.ReferenceId && a.ReferenceType == "purchase quotation").Count() == 0)
                            {
                                string qq = "update \"tblPurchaseQuotation\" set \"Status\"='Accepted' where \"PurchaseQuotationId\"=" + Purchase.ReferenceId;
                                oConnectionContext.Database.ExecuteSqlCommand(qq);
                            }
                        }
                    }
                    else if (Purchase.ReferenceType == "purchase order")
                    {
                        if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Purchase.ReferenceId
                        && a.ReferenceType == Purchase.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId != obj.PurchaseId).Count() == 0)
                        {
                            string qq = "update \"tblPurchaseOrder\" set \"Status\"='Confirmed' where \"PurchaseOrderId\"=" + Purchase.ReferenceId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);

                            long PurchaseOrderRefId = oConnectionContext.DbClsPurchaseOrder.Where(b =>
                            b.PurchaseOrderId == Purchase.ReferenceId && b.ReferenceType == Purchase.ReferenceType).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == PurchaseOrderRefId
                        && a.ReferenceType == "purchase quotation" && a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId != obj.PurchaseId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceType == "purchase quotation" && a.ReferenceId == PurchaseOrderRefId && a.PurchaseOrderId != Purchase.ReferenceId).Count() == 0)
                                {
                                    qq = "update \"tblPurchaseQuotation\" set \"Status\"='Accepted' where \"PurchaseQuotationId\"=" + PurchaseOrderRefId;
                                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Bill \"" + oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" updated",
                    Id = oClsPurchase.PurchaseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill", obj.CompanyId, oClsPurchase.PurchaseId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchase(ClsPurchaseVm obj)
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

                if (obj.PurchaseDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseDate" });
                    isError = true;
                }

                if (obj.DueDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDueDate" });
                    isError = true;
                }

                if (obj.PurchaseType == "Credit Note")
                {
                    if (obj.ParentId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseInvoice" });
                        isError = true;
                    }
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseStatus" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.PurchaseDetails == null || obj.PurchaseDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays, a.EnableLotNo }).FirstOrDefault();

                if (obj.PurchaseDetails != null)
                {
                    foreach (var item in obj.PurchaseDetails)
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
                                    //else
                                    //{
                                    //    int lotCount = (from a in oConnectionContext.DbClsPurchase
                                    //                    join b in oConnectionContext.DbClsPurchaseDetails
                                    //                    on a.PurchaseId equals b.PurchaseId
                                    //                    where a.SupplierId == obj.SupplierId
                                    //                    && b.ItemDetailsId == item.ItemDetailsId
                                    //                    && a.PurchaseId != obj.PurchaseId
                                    //                    && a.IsDeleted == false && a.IsCancelled == false
                                    //                    && b.IsDeleted == false && b.LotNo == item.LotNo
                                    //                    select b.LotNo).Count();
                                    //    if (lotCount > 0)
                                    //    {
                                    //        errors.Add(new ClsError { Message = "Lot No already exists", Id = "divLotNo" + item.DivId });
                                    //        isError = true;
                                    //    }

                                    //    // Check for duplicate LotNo within the same list
                                    //    int duplicateCount = obj.PurchaseDetails
                                    //        .Where(x => x.IsDeleted == false
                                    //                    && x.ItemId == item.ItemId && x.ItemDetailsId == item.ItemDetailsId
                                    //                    && x.LotNo == item.LotNo
                                    //                    && x.DivId != item.DivId) // Exclude self
                                    //        .Count();

                                    //    if (duplicateCount > 0)
                                    //    {
                                    //        errors.Add(new ClsError { Message = "Duplicate Lot No in this purchase", Id = "divLotNo" + item.DivId });
                                    //        isError = true;
                                    //    }
                                    //}
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

                var PrevPurchase = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new
                {
                    a.Status
                }).FirstOrDefault();

                //if (PrevPurchase.Status.ToLower() != "draft")
                //{
                if (obj.PurchaseDetails != null)
                {
                    foreach (var Purchase in obj.PurchaseDetails)
                    {
                        if (Purchase.PurchaseDetailsId != 0)
                        {
                            decimal convertedStock = oCommonController.StockConversion(Purchase.Quantity + Purchase.FreeQuantity, Purchase.ItemId, Purchase.PriceAddedFor);
                            decimal QuantitySold = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                            //                     decimal QuantityReturned = (from a in oConnectionContext.DbClsPurchaseReturn
                            //                                                 join b in oConnectionContext.DbClsPurchaseReturnDetails
                            //on a.PurchaseReturnId equals b.PurchaseReturnId
                            //                                                 where a.PurchaseId == obj.PurchaseId && b.ItemId == Purchase.ItemId &&
                            //                                                 b.ItemDetailsId == Purchase.ItemDetailsId
                            //                                                 select b.Quantity).FirstOrDefault();

                            decimal QuantityReturned = (from b in oConnectionContext.DbClsPurchaseReturnDetails
                                                        where b.PurchaseDetailsId == Purchase.PurchaseDetailsId && b.ItemId == Purchase.ItemId &&
                                                        b.ItemDetailsId == Purchase.ItemDetailsId
                                                        select b.QuantityRemaining).FirstOrDefault();

                            if (convertedStock - (QuantitySold + QuantityReturned) < 0)
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = "Mismatch between purchase and sold quantity for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == Purchase.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                            if (QuantitySold > 0 && obj.Status.ToLower() == "draft")
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = "Mismatch between purchase and sold quantity for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == Purchase.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                    }
                }
                //}

                decimal Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum();
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
                //else
                //{
                //    obj.PaymentStatus = "Due";
                //}

                decimal due = obj.GrandTotal - Paid;
                // check credit limit
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalPurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.SupplierId == obj.SupplierId
                                            ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsPurchase
                                             join b in oConnectionContext.DbClsSupplierPayment
                                         on a.PurchaseId equals b.PurchaseId
                                             where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.SupplierId == obj.SupplierId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if (obj.Status != "Draft")
                        {
                            if ((TotalPurchaseDue + due) > creditLimit)
                            {
                                data = new
                                {
                                    Status = 4,
                                    //Message = "Only " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + (creditLimit - TotalPurchaseDue) + " credit is available out of " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + creditLimit,
                                    Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalPurchaseDue)),
                                    Data = new
                                    {
                                        User = new
                                        {
                                            CreditLimit = creditLimit,
                                            TotalPurchaseDue = TotalPurchaseDue,
                                            TotalPurchase = due,
                                            UserId = obj.SupplierId
                                        }
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }

                    }
                }
                // check credit limit

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                ClsPurchase oClsPurchase = new ClsPurchase()
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
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.DueDate,
                    PurchaseDate = obj.PurchaseDate.AddHours(5).AddMinutes(30),
                    PurchaseId = obj.PurchaseId,
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
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //ShippingChargesAccountId = ShippingChargesAccountId,
                    //PackagingChargesAccountId = PackagingChargesAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
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
                    ParentId = obj.ParentId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId,
                };

                string pic1 = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/Purchase/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Purchase/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPurchase.AttachDocument = filepathPass;
                }
                else
                {
                    oClsPurchase.AttachDocument = pic1;
                }

                oConnectionContext.DbClsPurchase.Attach(oClsPurchase);
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.DeliveredTo).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.DueDate).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PurchaseDate).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PurchaseId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.Status).IsModified = true;
                //oConnectionContext.Entry(oClsPurchase).Property(x => x.ReferenceNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ShippingAddress).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ShippingDetails).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ShippingStatus).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.SupplierId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.NetAmount).IsModified = true;
                //oConnectionContext.Entry(oClsPurchase).Property(x => x.PaymentStatus).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.RoundOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.TaxAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.SourceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.DestinationOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.IsBusinessRegistered).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.GstTreatment).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.BusinessRegistrationNameId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.BusinessRegistrationNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.BusinessLegalName).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.BusinessTradeName).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PanNo).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.IsReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ParentId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.PurchaseAdditionalCharges != null)
                {
                    foreach (var item in obj.PurchaseAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.PurchaseAccountId }).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        //decimal AmountExcTax = obj.IsReverseCharge == 1 ? Purchase.AmountIncTax : Purchase.AmountExcTax;
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

                        if (obj.CountryId == 2)
                        {
                            if (item.ITCType == "Eligible For ITC")
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Reverse Charge Tax Input but not due";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Normal",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                            }
                            else
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Tax Paid Expense";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    AccountType = "Tax Paid Expense";
                                }
                            }
                        }
                        else
                        {
                            taxList = finalTaxs.Select(a => new ClsTaxVm
                            {
                                TaxType = "Normal",
                                TaxId = a.TaxId,
                                TaxPercent = a.TaxPercent,
                                TaxAmount = a.TaxAmount,
                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                            }).ToList();
                        }

                        long PurchaseAdditionalChargesId = 0;
                        if (item.PurchaseAdditionalChargesId == 0)
                        {
                            ClsPurchaseAdditionalCharges oClsPurchaseAdditionalCharges = new ClsPurchaseAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                PurchaseId = oClsPurchase.PurchaseId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.PurchaseAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                ITCType = item.ITCType,
                                TaxExemptionId = item.TaxExemptionId,
                                IsActive = item.IsActive,
                                IsDeleted = item.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId
                            };
                            oConnectionContext.DbClsPurchaseAdditionalCharges.Add(oClsPurchaseAdditionalCharges);
                            oConnectionContext.SaveChanges();

                            PurchaseAdditionalChargesId = oClsPurchaseAdditionalCharges.PurchaseAdditionalChargesId;
                        }
                        else
                        {
                            ClsPurchaseAdditionalCharges oClsPurchaseAdditionalCharges = new ClsPurchaseAdditionalCharges()
                            {
                                PurchaseAdditionalChargesId = item.PurchaseAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                PurchaseId = oClsPurchase.PurchaseId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.PurchaseAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                ITCType = item.ITCType,
                                TaxExemptionId = item.TaxExemptionId,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsPurchaseAdditionalCharges.Attach(oClsPurchaseAdditionalCharges);
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.PurchaseId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.ITCType).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();

                            PurchaseAdditionalChargesId = oClsPurchaseAdditionalCharges.PurchaseAdditionalChargesId;
                        }

                        string query = "delete from \"tblPurchaseAdditionalTaxJournal\" where \"PurchaseId\"=" + oClsPurchase.PurchaseId + " and \"PurchaseAdditionalChargesId\"=" + PurchaseAdditionalChargesId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        if (AccountType != "")
                        {
                            AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                            ClsPurchaseAdditionalTaxJournal oClsPurchaseAdditionalTaxJournal = new ClsPurchaseAdditionalTaxJournal()
                            {
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseAdditionalChargesId = PurchaseAdditionalChargesId,
                                TaxId = item.TaxId,
                                TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : (item.AmountIncTax - item.AmountExcTax),
                                AccountId = AccountId,
                                PurchaseTaxJournalType = "Normal"
                            };
                            oConnectionContext.DbClsPurchaseAdditionalTaxJournal.Add(oClsPurchaseAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsPurchaseAdditionalTaxJournal oClsPurchaseAdditionalTaxJournal = new ClsPurchaseAdditionalTaxJournal()
                            {
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseAdditionalChargesId = PurchaseAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                PurchaseTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsPurchaseAdditionalTaxJournal.Add(oClsPurchaseAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.PurchaseDetails != null)
                {
                    foreach (var Purchase in obj.PurchaseDetails)
                    {
                        if (Purchase.IsDeleted == true)
                        {
                            string query = "update \"tblPurchaseDetails\" set \"IsDeleted\"=True where \"PurchaseDetailsId\"=" + Purchase.PurchaseDetailsId;
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            if (Purchase.IsStopSelling == false)
                            {
                                decimal convertedStock = oCommonController.StockConversion(oConnectionContext.DbClsPurchaseDetails.Where(o => o.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(o => o.Quantity).FirstOrDefault(), Purchase.ItemId, Purchase.PriceAddedFor);
                                decimal freeConvertedStock = oCommonController.StockConversion(oConnectionContext.DbClsPurchaseDetails.Where(o => o.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(o => o.FreeQuantity).FirstOrDefault(), Purchase.ItemId, Purchase.PriceAddedFor);

                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + Purchase.ItemId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                        else
                        {
                            long PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Purchase.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                            //long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Purchase.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                            TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == Purchase.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                            bool IsStopSelling = false, flag = false;
                            decimal convertedStock = oCommonController.StockConversion(Purchase.Quantity, Purchase.ItemId, Purchase.PriceAddedFor);
                            decimal freeConvertedStock = oCommonController.StockConversion(Purchase.FreeQuantity, Purchase.ItemId, Purchase.PriceAddedFor);

                            Purchase.PurchaseDetailsId = oConnectionContext.DbClsPurchaseDetails.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && a.PurchaseId == obj.PurchaseId && a.ItemId == Purchase.ItemId
                            && a.ItemDetailsId == Purchase.ItemDetailsId).Select(a => a.PurchaseDetailsId).FirstOrDefault();

                            if (Purchase.PurchaseDetailsId == 0)
                            {
                                if (Purchase.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                                {
                                    if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                    {
                                        IsStopSelling = true;
                                    }
                                }

                                string AccountType = "";

                                var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == Purchase.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                                List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                                //decimal AmountExcTax = obj.IsReverseCharge == 1 ? Purchase.AmountIncTax : Purchase.AmountExcTax;
                                decimal AmountExcTax = Purchase.AmountExcTax;
                                var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Purchase.TaxId).Select(a => new
                                {
                                    a.TaxId,
                                    a.Tax,
                                    a.TaxPercent,
                                }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                               where a.TaxId == Purchase.TaxId
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

                                if (obj.CountryId == 2)
                                {
                                    if (Purchase.ITCType == "Eligible For ITC")
                                    {
                                        if (obj.IsReverseCharge == 1)
                                        {
                                            AccountType = "Reverse Charge Tax Input but not due";

                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Reverse Charge",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                        else
                                        {
                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Normal",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                    }
                                    else
                                    {
                                        if (obj.IsReverseCharge == 1)
                                        {
                                            AccountType = "Tax Paid Expense";

                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Reverse Charge",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                        else
                                        {
                                            AccountType = "Tax Paid Expense";
                                        }
                                    }
                                }
                                else
                                {
                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Normal",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                    }).ToList();
                                }

                                Purchase.DefaultProfitMargin = ((Purchase.SalesExcTax - Purchase.UnitCost) / Purchase.UnitCost) * 100;

                                ClsPurchaseDetails oClsPurchaseDetails = new ClsPurchaseDetails()
                                {
                                    AmountExcTax = Purchase.AmountExcTax,
                                    AmountIncTax = Purchase.AmountIncTax,
                                    ItemDetailsId = Purchase.ItemDetailsId,
                                    PurchaseId = oClsPurchase.PurchaseId,
                                    PurchaseExcTax = Purchase.PurchaseExcTax,
                                    PurchaseIncTax = Purchase.PurchaseIncTax,
                                    //Tax = Purchase.Tax,
                                    TaxId = Purchase.TaxId,
                                    Discount = Purchase.Discount,
                                    Quantity = Purchase.Quantity,
                                    UnitCost = Purchase.UnitCost,
                                    IsActive = Purchase.IsActive,
                                    IsDeleted = Purchase.IsDeleted,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    QuantityRemaining = convertedStock + freeConvertedStock,
                                    QuantitySold = 0,
                                    LotNo = Purchase.LotNo,
                                    ExpiryDate = Purchase.ExpiryDate != null ? Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) : Purchase.ExpiryDate,
                                    ManufacturingDate = Purchase.ManufacturingDate != null ? Purchase.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : Purchase.ManufacturingDate,
                                    PriceAddedFor = Purchase.PriceAddedFor,
                                    SalesExcTax = Purchase.SalesExcTax,
                                    SalesIncTax = Purchase.SalesIncTax,
                                    FreeQuantity = Purchase.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = Purchase.FreeQuantityPriceAddedFor,
                                    IsStopSelling = IsStopSelling,
                                    TaxAmount = Purchase.TaxAmount,
                                    DiscountType = Purchase.DiscountType,
                                    DefaultProfitMargin = Purchase.DefaultProfitMargin,
                                    UnitAddedFor = Purchase.UnitAddedFor,
                                    QuantityPurchased = convertedStock + freeConvertedStock,
                                    Mrp = Purchase.Mrp,
                                    AccountId = PurchaseAccountId,
                                    DiscountAccountId = DiscountAccountId,
                                    TaxAccountId = TaxAccountId,
                                    ItemId = Purchase.ItemId,
                                    ExtraDiscount = Purchase.ExtraDiscount,
                                    ItemCodeId = Purchase.ItemCodeId,
                                    ITCType = Purchase.ITCType,
                                    TaxExemptionId = Purchase.TaxExemptionId,
                                    TotalTaxAmount = Purchase.TotalTaxAmount
                                };

                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsPurchaseDetails.Add(oClsPurchaseDetails);
                                oConnectionContext.SaveChanges();

                                if (AccountType != "")
                                {
                                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                    && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                    ClsPurchaseTaxJournal oClsPurchaseTaxJournal = new ClsPurchaseTaxJournal()
                                    {
                                        PurchaseId = oClsPurchase.PurchaseId,
                                        PurchaseDetailsId = oClsPurchaseDetails.PurchaseDetailsId,
                                        TaxId = Purchase.TaxId,
                                        TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : Purchase.TaxAmount,
                                        AccountId = AccountId,
                                        PurchaseTaxJournalType = "Normal"
                                    };
                                    oConnectionContext.DbClsPurchaseTaxJournal.Add(oClsPurchaseTaxJournal);
                                    oConnectionContext.SaveChanges();
                                }

                                foreach (var taxJournal in taxList)
                                {
                                    ClsPurchaseTaxJournal oClsPurchaseTaxJournal = new ClsPurchaseTaxJournal()
                                    {
                                        PurchaseId = oClsPurchase.PurchaseId,
                                        PurchaseDetailsId = oClsPurchaseDetails.PurchaseDetailsId,
                                        TaxId = taxJournal.TaxId,
                                        TaxAmount = taxJournal.TaxAmount,
                                        AccountId = taxJournal.AccountId,
                                        PurchaseTaxJournalType = taxJournal.TaxType
                                    };
                                    oConnectionContext.DbClsPurchaseTaxJournal.Add(oClsPurchaseTaxJournal);
                                    oConnectionContext.SaveChanges();
                                }

                                if (IsStopSelling == false)
                                {
                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                }
                            }
                            else
                            {
                                decimal QuantityOut = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                                decimal QuantityReturned = (from b in oConnectionContext.DbClsPurchaseReturnDetails
                                                            where b.PurchaseDetailsId == Purchase.PurchaseDetailsId && b.ItemId == Purchase.ItemId &&
                                                            b.ItemDetailsId == Purchase.ItemDetailsId
                                                            select b.QuantityRemaining).FirstOrDefault();

                                bool previousIsStopSelling = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.IsStopSelling).FirstOrDefault();
                                if (previousIsStopSelling == true)
                                {
                                    if (ItemSetting.OnItemExpiry == 1)
                                    {
                                        flag = true;
                                        IsStopSelling = false;
                                        if (obj.Status.ToLower() != "draft")
                                        {
                                            string query1 = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                                        }
                                    }
                                    else
                                    {
                                        if ((Purchase.ExpiryDate != null))
                                        {
                                            if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days > ItemSetting.StopSellingBeforeDays)
                                            {
                                                flag = true;
                                                IsStopSelling = false;
                                                if (obj.Status.ToLower() != "draft")
                                                {
                                                    string query1 = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                                }
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
                                            if (obj.Status.ToLower() != "draft")
                                            {
                                                string query1 = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (ItemSetting.OnItemExpiry != 1)
                                    {
                                        if ((Purchase.ExpiryDate != null))
                                        {
                                            if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                            {
                                                flag = true;
                                                IsStopSelling = true;
                                                if (obj.Status.ToLower() != "draft")
                                                {
                                                    string query1 = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"-(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (flag == false)
                                {
                                    decimal Quantity = oCommonController.StockConversion(oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.Quantity + a.FreeQuantity).FirstOrDefault(), Purchase.ItemId, oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.PriceAddedFor).FirstOrDefault());

                                    if (PrevPurchase.Status.ToLower() != "draft")
                                    {
                                        string query1 = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"-(" + (Quantity - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                                    }

                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        string query1 = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" =\"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                                    }
                                    //else
                                    //{
                                    //    if (PrevPurchase.Status.ToLower() != "draft")
                                    //    {
                                    //        string query = "update tblItemBranchMap set SalesIncTax=" + Purchase.SalesIncTax + ",Quantity = Quantity,0)-(" + Quantity + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + Purchase.ItemDetailsId;
                                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    //    }
                                    //}
                                }

                                string AccountType = "";

                                var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == Purchase.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                                List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                                //decimal AmountExcTax = obj.IsReverseCharge == 1 ? Purchase.AmountIncTax : Purchase.AmountExcTax;
                                decimal AmountExcTax = Purchase.AmountExcTax;
                                var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Purchase.TaxId).Select(a => new
                                {
                                    a.TaxId,
                                    a.Tax,
                                    a.TaxPercent,
                                }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                               where a.TaxId == Purchase.TaxId
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

                                if (obj.CountryId == 2)
                                {
                                    if (Purchase.ITCType == "Eligible For ITC")
                                    {
                                        if (obj.IsReverseCharge == 1)
                                        {
                                            AccountType = "Reverse Charge Tax Input but not due";

                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Reverse Charge",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                        else
                                        {
                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Normal",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                    }
                                    else
                                    {
                                        if (obj.IsReverseCharge == 1)
                                        {
                                            AccountType = "Tax Paid Expense";

                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Reverse Charge",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                        else
                                        {
                                            AccountType = "Tax Paid Expense";
                                        }
                                    }
                                }
                                else
                                {
                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Normal",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                    }).ToList();
                                }

                                Purchase.DefaultProfitMargin = ((Purchase.SalesExcTax - Purchase.UnitCost) / Purchase.UnitCost) * 100;

                                ClsPurchaseDetails oClsPurchaseDetails = new ClsPurchaseDetails()
                                {
                                    PurchaseDetailsId = Purchase.PurchaseDetailsId,
                                    AmountExcTax = Purchase.AmountExcTax,
                                    AmountIncTax = Purchase.AmountIncTax,
                                    ItemId = Purchase.ItemId,
                                    ItemDetailsId = Purchase.ItemDetailsId,
                                    PurchaseId = oClsPurchase.PurchaseId,
                                    PurchaseExcTax = Purchase.PurchaseExcTax,
                                    PurchaseIncTax = Purchase.PurchaseIncTax,
                                    //Tax = Purchase.Tax,
                                    TaxId = Purchase.TaxId,
                                    Discount = Purchase.Discount,
                                    Quantity = Purchase.Quantity,
                                    UnitCost = Purchase.UnitCost,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityOut,
                                    LotNo = Purchase.LotNo,
                                    ExpiryDate = Purchase.ExpiryDate != null ? Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) : Purchase.ExpiryDate,
                                    ManufacturingDate = Purchase.ManufacturingDate != null ? Purchase.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : Purchase.ManufacturingDate,
                                    PriceAddedFor = Purchase.PriceAddedFor,
                                    SalesExcTax = Purchase.SalesExcTax,
                                    SalesIncTax = Purchase.SalesIncTax,
                                    FreeQuantity = Purchase.FreeQuantity,
                                    //FreeQuantityPriceAddedFor = Purchase.FreeQuantityPriceAddedFor,
                                    IsStopSelling = IsStopSelling,
                                    TaxAmount = Purchase.TaxAmount,
                                    DiscountType = Purchase.DiscountType,
                                    DefaultProfitMargin = Purchase.DefaultProfitMargin,
                                    UnitAddedFor = Purchase.UnitAddedFor,
                                    QuantityPurchased = convertedStock + freeConvertedStock,
                                    Mrp = Purchase.Mrp,
                                    AccountId = PurchaseAccountId,
                                    DiscountAccountId = DiscountAccountId,
                                    TaxAccountId = TaxAccountId,
                                    ExtraDiscount = Purchase.ExtraDiscount,
                                    ItemCodeId = Purchase.ItemCodeId,
                                    ITCType = Purchase.ITCType,
                                    TaxExemptionId = Purchase.TaxExemptionId,
                                    TotalTaxAmount = Purchase.TotalTaxAmount
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsPurchaseDetails.Attach(oClsPurchaseDetails);
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.AmountExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.AmountIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ItemId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ItemDetailsId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PurchaseId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PurchaseExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PurchaseIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TaxId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.Discount).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.Quantity).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.UnitCost).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.QuantityRemaining).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.LotNo).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ExpiryDate).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ManufacturingDate).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.SalesExcTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.SalesIncTax).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.FreeQuantity).IsModified = true;
                                //oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.IsStopSelling).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TaxAmount).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.DiscountType).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.DefaultProfitMargin).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.UnitAddedFor).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.QuantityPurchased).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.Mrp).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.AccountId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.DiscountAccountId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TaxAccountId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ExtraDiscount).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ItemCodeId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ITCType).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TaxExemptionId).IsModified = true;
                                oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                                oConnectionContext.SaveChanges();

                                string query = "delete from \"tblPurchaseTaxJournal\" where \"PurchaseId\"=" + oClsPurchase.PurchaseId + " and \"PurchaseDetailsId\"=" + oClsPurchaseDetails.PurchaseDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                if (AccountType != "")
                                {
                                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                    && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                    ClsPurchaseTaxJournal oClsPurchaseTaxJournal = new ClsPurchaseTaxJournal()
                                    {
                                        PurchaseId = oClsPurchase.PurchaseId,
                                        PurchaseDetailsId = oClsPurchaseDetails.PurchaseDetailsId,
                                        TaxId = Purchase.TaxId,
                                        TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : Purchase.TaxAmount,
                                        AccountId = AccountId,
                                        PurchaseTaxJournalType = "Normal"
                                    };
                                    oConnectionContext.DbClsPurchaseTaxJournal.Add(oClsPurchaseTaxJournal);
                                    oConnectionContext.SaveChanges();
                                }

                                foreach (var taxJournal in taxList)
                                {
                                    ClsPurchaseTaxJournal oClsPurchaseTaxJournal = new ClsPurchaseTaxJournal()
                                    {
                                        PurchaseId = oClsPurchase.PurchaseId,
                                        PurchaseDetailsId = oClsPurchaseDetails.PurchaseDetailsId,
                                        TaxId = taxJournal.TaxId,
                                        TaxAmount = taxJournal.TaxAmount,
                                        AccountId = taxJournal.AccountId,
                                        PurchaseTaxJournalType = taxJournal.TaxType
                                    };
                                    oConnectionContext.DbClsPurchaseTaxJournal.Add(oClsPurchaseTaxJournal);
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
                    Category = "Purchase",
                    CompanyId = obj.CompanyId,
                    //Description = "modified " + obj.ReferenceNo,
                    Description = "Purchase Bill \"" + obj.ReferenceNo + "\" updated",
                    Id = oClsPurchase.PurchaseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill", obj.CompanyId, oClsPurchase.PurchaseId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Purchase = new
                        {
                            PurchaseId = oClsPurchase.PurchaseId,
                            InvoiceId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == oClsPurchase.PurchaseId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseBill }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDetailsDelete(ClsPurchaseDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.PurchaseId != 0)
                {
                    obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == obj.PurchaseId && a.IsDeleted == false).Select(a => new
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

                    //string query = "update \"tblPurchaseDetails\" set \"IsDeleted\"=True where \"PurchaseId\"=" + obj.PurchaseId;
                    ////ConnectionContext ocon = new ConnectionContext();
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    //foreach (var item in details)
                    //{
                    //    if (item.IsStopSelling == false)
                    //    {
                    //        decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);

                    //        query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + obj.BranchId + " and ItemId=" + item.ItemId + " and ItemDetailsId=" + item.ItemDetailsId;
                    //        oConnectionContext.Database.ExecuteSqlCommand(query);
                    //    }
                    //}
                }
                else
                {
                    obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == oConnectionContext.DbClsPurchaseDetails.Where(b => b.PurchaseDetailsId == obj.PurchaseDetailsId).Select(b => b.PurchaseId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == obj.PurchaseDetailsId).Select(a => new
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

                    //string query = "update \"tblPurchaseDetails\" set \"IsDeleted\"=True where \"PurchaseDetailsId\"=" + obj.PurchaseDetailsId;
                    ////ConnectionContext ocon = new ConnectionContext();
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    //if (details.IsStopSelling == false)
                    //{
                    //    query = "update tblItemBranchMap set Quantity=Quantity,0)-" + convertedStock + " where BranchId=" + obj.BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
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

        public async Task<IHttpActionResult> PurchaseReport(ClsPurchaseVm obj)
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

            var det = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Select(a => new
            {
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                PurchaseId = a.PurchaseId,
                a.GrandTotal,
                a.Notes,
                a.PurchaseDate,
                a.Status,
                a.ReferenceNo,
                a.Subtotal,
                a.SupplierId,
                SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
                    oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                //PaymentStatus = a.PaymentStatus,
                Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                    a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                TotalQuantity = a.TotalQuantity//oConnectionContext.DbClsPurchaseDetails.Where(c=>c.PurchaseId==a.PurchaseId && c.IsDeleted==false).Count()
            }).ToList();


            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.PurchaseDate.Date >= obj.FromDate && a.PurchaseDate.Date <= obj.ToDate).ToList();
            }

            //if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            //{
            //    det = det.Where(a => a.PaymentStatus.ToLower() == obj.PaymentStatus.ToLower()).Select(a => a).ToList();
            //}

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
                    Purchases = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDetailsReport(ClsPurchaseVm obj)
        {
            var det = (from b in oConnectionContext.DbClsPurchaseDetails
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.PurchaseId == obj.PurchaseId && b.IsDeleted == false
                       select new
                       {
                           b.QuantitySold,
                           b.QuantityRemaining,
                           b.PurchaseDetailsId,
                           b.AmountExcTax,
                           b.AmountIncTax,
                           b.Discount,
                           b.PurchaseIncTax,
                           b.PurchaseExcTax,
                           b.Quantity,
                           b.TaxId,
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
                           Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                           TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                           d.TaxType,
                           d.ItemCode
                       }).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDetails = det.OrderByDescending(a => a.PurchaseDetailsId).ToList(),
                    TotalCount = det.Count(),

                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemPurchaseReport(ClsPurchaseVm obj)
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

            List<ClsPurchaseDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from b in oConnectionContext.DbClsPurchaseDetails
                       join a in oConnectionContext.DbClsPurchase
                       on b.PurchaseId equals a.PurchaseId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false &&
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                && a.Status != "Draft"
                       select new ClsPurchaseDetailsVm
                       {
                           PurchaseId = a.PurchaseId,
                           QuantityPurchased = b.QuantityPurchased,
                           //AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                           AdjustedQuantityDebit = (from x in oConnectionContext.DbClsStockAdjustmentDeductionId
                                                    join y in oConnectionContext.DbClsStockAdjustment
                                                    on x.StockAdjustmentId equals y.StockAdjustmentId
                                                    where x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId
                                                    && y.AdjustmentType == "Debit"
                                                    select x.Quantity
                                               ).DefaultIfEmpty().Sum(),
                           AdjustedQuantityCredit = (from x in oConnectionContext.DbClsStockAdjustmentDeductionId
                                                     join y in oConnectionContext.DbClsStockAdjustment
                                                     on x.StockAdjustmentId equals y.StockAdjustmentId
                                                     where x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId
                                                     && y.AdjustmentType == "Credit"
                                                     select x.Quantity
                                               ).DefaultIfEmpty().Sum(),
                           BranchId = a.BranchId,
                           CategoryId = d.CategoryId,
                           SubCategoryId = d.SubCategoryId,
                           SubSubCategoryId = d.SubSubCategoryId,
                           BrandId = d.BrandId,
                           PurchaseDate = a.PurchaseDate,
                           //PaymentStatus = a.PaymentStatus,
                           //Status = a.Status,
                           SupplierId = a.SupplierId,
                           ReferenceNo = a.ReferenceNo,
                           QuantitySold = b.QuantitySold,
                           QuantityRemaining = b.QuantityRemaining,
                           PurchaseDetailsId = b.PurchaseDetailsId,
                           AmountExcTax = b.AmountExcTax,
                           AmountIncTax = b.AmountIncTax,
                           Discount = b.Discount,
                           PurchaseExcTax = b.PurchaseExcTax,
                           PurchaseIncTax = b.PurchaseIncTax,
                           Quantity = b.Quantity,
                           FreeQuantity = b.FreeQuantity,
                           TaxId = b.TaxId,
                           UnitCost = b.UnitCost,
                           ItemId = d.ItemId,
                           ProductType = d.ProductType,
                           ItemDetailsId = c.ItemDetailsId,
                           ItemName = d.ItemName,
                           SKU = c.SKU,
                           //VariationDetailsId = c.VariationDetailsId,
                           Name = oConnectionContext.DbClsUser.Where(x => x.UserId == a.SupplierId).Select(x => x.Name).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.SupplierId).Select(x => x.EmailId).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.SupplierId).Select(x => x.MobileNo).FirstOrDefault(),
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                           SalesExcTax = c.SalesExcTax,
                           SalesIncTax = c.SalesIncTax,
                           TotalCost = c.TotalCost,
                           Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                           TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                           //TaxType = d.TaxType,
                           //ItemCode = d.ItemCode
                       }).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsPurchaseDetails
                       join a in oConnectionContext.DbClsPurchase
                       on b.PurchaseId equals a.PurchaseId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false &&
                       a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                && a.Status != "Draft"
                       select new ClsPurchaseDetailsVm
                       {
                           PurchaseId = a.PurchaseId,
                           QuantityPurchased = b.QuantityPurchased,
                           //AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                           AdjustedQuantityDebit = (from x in oConnectionContext.DbClsStockAdjustmentDeductionId
                                                    join y in oConnectionContext.DbClsStockAdjustment
                                                    on x.StockAdjustmentId equals y.StockAdjustmentId
                                                    where x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId
                                                    && y.AdjustmentType == "Debit"
                                                    select x.Quantity
                                               ).DefaultIfEmpty().Sum(),
                           AdjustedQuantityCredit = (from x in oConnectionContext.DbClsStockAdjustmentDeductionId
                                                     join y in oConnectionContext.DbClsStockAdjustment
                                                     on x.StockAdjustmentId equals y.StockAdjustmentId
                                                     where x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId
                                                     && y.AdjustmentType == "Credit"
                                                     select x.Quantity
                                               ).DefaultIfEmpty().Sum(),
                           BranchId = a.BranchId,
                           CategoryId = d.CategoryId,
                           SubCategoryId = d.SubCategoryId,
                           SubSubCategoryId = d.SubSubCategoryId,
                           BrandId = d.BrandId,
                           PurchaseDate = a.PurchaseDate,
                           //PaymentStatus = a.PaymentStatus,
                           //Status = a.Status,
                           SupplierId = a.SupplierId,
                           ReferenceNo = a.ReferenceNo,
                           QuantitySold = b.QuantitySold,
                           QuantityRemaining = b.QuantityRemaining,
                           PurchaseDetailsId = b.PurchaseDetailsId,
                           AmountExcTax = b.AmountExcTax,
                           AmountIncTax = b.AmountIncTax,
                           Discount = b.Discount,
                           PurchaseExcTax = b.PurchaseExcTax,
                           PurchaseIncTax = b.PurchaseIncTax,
                           Quantity = b.Quantity,
                           FreeQuantity = b.FreeQuantity,
                           TaxId = b.TaxId,
                           UnitCost = b.UnitCost,
                           ItemId = d.ItemId,
                           ProductType = d.ProductType,
                           ItemDetailsId = c.ItemDetailsId,
                           ItemName = d.ItemName,
                           SKU = c.SKU,
                           //VariationDetailsId = c.VariationDetailsId,
                           Name = oConnectionContext.DbClsUser.Where(x => x.UserId == a.SupplierId).Select(x => x.Name).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.SupplierId).Select(x => x.EmailId).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.SupplierId).Select(x => x.MobileNo).FirstOrDefault(),
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                           SalesExcTax = c.SalesExcTax,
                           SalesIncTax = c.SalesIncTax,
                           TotalCost = c.TotalCost,
                           Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                           TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                           //TaxType = d.TaxType,
                           //ItemCode = d.ItemCode
                       }).ToList();
            }

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }
            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).ToList();
            }
            if (obj.ItemDetailsId != 0)
            {
                det = det.Where(a => a.ItemDetailsId == obj.ItemDetailsId).Select(a => a).ToList();
            }

            if (obj.ReferenceNo != null && obj.ReferenceNo != "")
            {
                det = det.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower()).Select(a => a).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            List<ClsPurchaseDetailsVm> _det1 = new List<ClsPurchaseDetailsVm>();
            List<ClsPurchaseDetailsVm> _det2 = new List<ClsPurchaseDetailsVm>();

            _det1 = det.OrderByDescending(a => a.PurchaseDate).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.QuantityPurchased;
                //decimal TotalUnitAdjusted = item.AdjustedQuantity;
                decimal TotalUnitAdjustedDebit = item.AdjustedQuantityDebit;
                decimal TotalUnitAdjustedCredit = item.AdjustedQuantityCredit;

                if (obj.PriceAddedFor == 0) { obj.PriceAddedFor = 1; }

                //TotalCurrentStock Stock
                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    //TotalCurrentStock = TotalCurrentStock;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue;
                        TotalUnitAdjustedDebit = TotalUnitAdjustedDebit / conversionRates.UToSValue;
                        TotalUnitAdjustedCredit = TotalUnitAdjustedCredit / conversionRates.UToSValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        //TotalCurrentStock = TotalCurrentStock;
                    }
                }
                else if (conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalUnitAdjustedDebit = TotalUnitAdjustedDebit / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalUnitAdjustedCredit = TotalUnitAdjustedCredit / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                        TotalUnitAdjustedDebit = TotalUnitAdjustedDebit / conversionRates.SToTValue;
                        TotalUnitAdjustedCredit = TotalUnitAdjustedCredit / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        //TotalCurrentStock = TotalCurrentStock;
                    }
                }
                else
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjustedDebit = TotalUnitAdjustedDebit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjustedCredit = TotalUnitAdjustedCredit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjustedDebit = TotalUnitAdjustedDebit / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjustedCredit = TotalUnitAdjustedCredit / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                        TotalUnitAdjustedDebit = TotalUnitAdjustedDebit / conversionRates.TToQValue;
                        TotalUnitAdjustedCredit = TotalUnitAdjustedCredit / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                _det2.Add(new ClsPurchaseDetailsVm
                {
                    PurchaseId = item.PurchaseId,
                    QuantityPurchased = item.QuantityPurchased,
                    AdjustedQuantityDebit = TotalUnitAdjustedDebit,
                    AdjustedQuantityCredit = TotalUnitAdjustedCredit,
                    BranchId = item.BranchId,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    SubSubCategoryId = item.SubSubCategoryId,
                    BrandId = item.BrandId,
                    PurchaseDate = item.PurchaseDate,
                    SupplierId = item.SupplierId,
                    ReferenceNo = item.ReferenceNo,
                    QuantitySold = item.QuantitySold,
                    QuantityRemaining = item.QuantityRemaining,
                    PurchaseDetailsId = item.PurchaseDetailsId,
                    AmountExcTax = item.AmountExcTax,
                    AmountIncTax = item.AmountIncTax,
                    Discount = item.Discount,
                    PurchaseExcTax = item.PurchaseExcTax,
                    PurchaseIncTax = item.PurchaseIncTax,
                    Quantity = TotalCurrentStock,
                    FreeQuantity = item.FreeQuantity,
                    TaxId = item.TaxId,
                    UnitCost = item.UnitCost,
                    ItemId = item.ItemId,
                    ProductType = item.ProductType,
                    ItemDetailsId = item.ItemDetailsId,
                    ItemName = item.ItemName,
                    SKU = item.SKU,
                    Name = item.Name,
                    EmailId = item.EmailId,
                    MobileNo = item.MobileNo,
                    VariationName = item.VariationName,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                    SalesExcTax = item.SalesExcTax,
                    SalesIncTax = item.SalesIncTax,
                    TotalCost = item.TotalCost,
                    Tax = item.Tax,
                    TaxPercent = item.TaxPercent
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDetails = _det2,
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Invoice(ClsPurchaseVm obj)
        {
            var det = oConnectionContext.DbClsPurchase.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.InvoiceId == obj.InvoiceId).Select(a => new
            {
                a.IsCancelled,
                a.PurchaseId,
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
                a.PurchaseDate,
                a.DueDate,
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
                Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                  a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                Payments = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseId == a.PurchaseId && b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
                {
                    b.PaymentDate,
                    ReferenceNo = b.ParentId == 0 ? b.ReferenceNo : oConnectionContext.DbClsSupplierPayment.Where(c => c.SupplierPaymentId == b.ParentId).Select(c => c.ReferenceNo).FirstOrDefault(),
                    b.Notes,
                    b.Amount,
                    b.PaymentTypeId,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                }),
                PurchaseDetails = (from b in oConnectionContext.DbClsPurchaseDetails
                                   join c in oConnectionContext.DbClsItemDetails
                                   on b.ItemDetailsId equals c.ItemDetailsId
                                   join d in oConnectionContext.DbClsItem
                                   on c.ItemId equals d.ItemId
                                   where b.PurchaseId == a.PurchaseId && b.IsDeleted == false
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
                                       b.PurchaseId,
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
                PurchaseAdditionalCharges = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(b => b.PurchaseId == a.PurchaseId
                    && b.IsDeleted == false && b.IsActive == true).Select(b => new ClsPurchaseAdditionalChargesVm
                    {
                        PurchaseAdditionalChargesId = b.PurchaseAdditionalChargesId,
                        Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
                        AdditionalChargeId = b.AdditionalChargeId,
                        PurchaseId = b.PurchaseId,
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

            var AllTaxs = oConnectionContext.DbClsPurchase.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId == det.PurchaseId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == det.PurchaseId && a.IsDeleted == false).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.AmountExcTax
            })).Concat(oConnectionContext.DbClsPurchaseAdditionalCharges.Where(a => a.PurchaseId == det.PurchaseId
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
                    Purchase = det,
                    BusinessSetting = BusinessSetting,
                    Taxs = finalTaxs,
                    ItemSetting = ItemSetting
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchaseStatus(ClsPurchaseVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "Purchase Status is required", Id = "divPurchaseStatus" });
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

                //      if (obj.CheckStockPriceMismatch == true)
                //      {
                //          string CurrencySymbol = (from a in oConnectionContext.DbClsUserCurrencyMap
                //                                   join b in oConnectionContext.DbClsCurrency
                //            on a.CurrencyId equals b.CurrencyId
                //                                   where a.CompanyId == obj.CompanyId && a.IsMain == true
                //                                   select b.CurrencySymbol).FirstOrDefault();

                //          foreach (var Purchase in obj.PurchaseDetails)
                //          {
                //              decimal PreviousSellingPrice = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemDetailsId ==
                //              Purchase.ItemDetailsId && a.IsActive == true &&
                //              a.IsDeleted == false && a.IsCancelled == false).Select(a => a.SalesIncTax).DefaultIfEmpty().FirstOrDefault();

                //              if (PreviousSellingPrice != 0)
                //              {
                //                  if (PreviousSellingPrice != Purchase.SalesIncTax)
                //                  {
                //                      errors.Add(new ClsError
                //                      {
                //                          Message = oConnectionContext.DbClsItem.Where(a => a.ItemId ==
                //Purchase.ItemId).Select(a => a.ItemName).FirstOrDefault() +
                //                      " (Previous Selling price-" + CurrencySymbol + PreviousSellingPrice + " / New Selling Price-" + CurrencySymbol + Purchase.SalesIncTax + ')',
                //                          Id = ""
                //                      });
                //                      isError = true;
                //                  }
                //              }
                //          }

                //          if (isError == true)
                //          {
                //              data = new
                //              {
                //                  Status = 3,
                //                  Message = "",
                //                  Errors = errors,
                //                  Data = new
                //                  {
                //                  }
                //              };
                //              return await Task.FromResult(Ok(data));
                //          }
                //      }

                var PrevPurchase = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new
                {
                    a.Status
                }).FirstOrDefault();

                obj = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new ClsPurchaseVm
                {
                    SupplierId = a.SupplierId,
                    GrandTotal = a.GrandTotal,
                    BranchId = a.BranchId,
                    CompanyId = obj.CompanyId,
                    AddedBy = obj.AddedBy,
                    PurchaseId = a.PurchaseId,
                    Status = obj.Status,
                    PurchaseDetails = oConnectionContext.DbClsPurchaseDetails.Where(b => b.PurchaseId == a.PurchaseId).Select(b => new ClsPurchaseDetailsVm
                    {
                        PurchaseDetailsId = b.PurchaseDetailsId,
                        PurchaseId = b.PurchaseId,
                        ItemId = b.ItemId,
                        ItemDetailsId = b.ItemDetailsId,
                        Quantity = b.Quantity,
                        PurchaseExcTax = b.PurchaseExcTax,
                        PurchaseIncTax = b.PurchaseIncTax,
                        TaxId = b.TaxId,
                        TaxAmount = b.TaxAmount,
                        Discount = b.Discount,
                        DiscountType = b.DiscountType,
                        UnitCost = b.UnitCost,
                        AmountExcTax = b.AmountExcTax,
                        AmountIncTax = b.AmountIncTax,
                        CompanyId = b.CompanyId,
                        IsActive = b.IsActive,
                        IsDeleted = b.IsDeleted,
                        AddedBy = b.AddedBy,
                        AddedOn = a.AddedOn,
                        ModifiedBy = a.ModifiedBy,
                        ModifiedOn = a.ModifiedOn,
                        QuantityRemaining = b.QuantityRemaining,
                        QuantitySold = b.QuantitySold,
                        LotNo = b.LotNo,
                        ManufacturingDate = b.ManufacturingDate,
                        ExpiryDate = b.ExpiryDate,
                        PriceAddedFor = b.PriceAddedFor,
                        SalesExcTax = b.SalesExcTax,
                        SalesIncTax = b.SalesIncTax,
                        FreeQuantity = b.FreeQuantity,
                        IsStopSelling = b.IsStopSelling,
                        DefaultProfitMargin = b.DefaultProfitMargin,
                        UnitAddedFor = b.UnitAddedFor,
                    }).ToList()
                }).FirstOrDefault();


                if (obj.PurchaseDetails != null)
                {
                    foreach (var Purchase in obj.PurchaseDetails)
                    {
                        if (Purchase.PurchaseDetailsId != 0)
                        {
                            decimal QuantitySold = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                            //                     decimal QuantityReturned = (from a in oConnectionContext.DbClsPurchaseReturn
                            //                                                 join b in oConnectionContext.DbClsPurchaseReturnDetails
                            //on a.PurchaseReturnId equals b.PurchaseReturnId
                            //                                                 where a.PurchaseId == obj.PurchaseId && b.ItemId == Purchase.ItemId &&
                            //                                                 b.ItemDetailsId == Purchase.ItemDetailsId
                            //                                                 select b.Quantity).FirstOrDefault();

                            decimal QuantityReturned = (from b in oConnectionContext.DbClsPurchaseReturnDetails
                                                        where b.PurchaseDetailsId == Purchase.PurchaseDetailsId && b.ItemId == Purchase.ItemId &&
                                                        b.ItemDetailsId == Purchase.ItemDetailsId
                                                        select b.QuantityRemaining).FirstOrDefault();

                            if (Purchase.Quantity - (QuantitySold + QuantityReturned) < 0)
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = "Mismatch between purchase and sold quantity for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == Purchase.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                            if (QuantitySold > 0 && obj.Status.ToLower() == "draft")
                            {
                                data = new
                                {
                                    Status = 0,
                                    Message = "Mismatch between purchase and sold quantity for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == Purchase.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                    Data = new
                                    {
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                    }
                }

                decimal due = obj.GrandTotal;
                // check credit limit
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalPurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.SupplierId == obj.SupplierId
                                            ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsPurchase
                                             join b in oConnectionContext.DbClsSupplierPayment
                                         on a.PurchaseId equals b.PurchaseId
                                             where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.SupplierId == obj.SupplierId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if (obj.Status != "Draft")
                        {
                            if ((TotalPurchaseDue + due) > creditLimit)
                            {
                                data = new
                                {
                                    Status = 4,
                                    //Message = "Only " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + (creditLimit - TotalPurchaseDue) + " credit is available out of " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + creditLimit,
                                    Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalPurchaseDue)),
                                    Data = new
                                    {
                                        User = new
                                        {
                                            CreditLimit = creditLimit,
                                            TotalPurchaseDue = TotalPurchaseDue,
                                            TotalPurchase = due,
                                            UserId = obj.SupplierId
                                        }
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }

                    }
                }
                // check credit limit

                ClsPurchase oClsPurchase = new ClsPurchase()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    PurchaseId = obj.PurchaseId,
                    Status = obj.Status,
                };

                oConnectionContext.DbClsPurchase.Attach(oClsPurchase);
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.PurchaseId).IsModified = true;
                oConnectionContext.Entry(oClsPurchase).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                { a.OnItemExpiry, a.StopSellingBeforeDays }).FirstOrDefault();

                if (obj.PurchaseDetails != null)
                {
                    foreach (var Purchase in obj.PurchaseDetails)
                    {
                        bool IsStopSelling = false, flag = false;
                        decimal convertedStock = oCommonController.StockConversion(Purchase.Quantity, Purchase.ItemId, Purchase.PriceAddedFor);
                        decimal freeConvertedStock = oCommonController.StockConversion(Purchase.FreeQuantity, Purchase.ItemId, Purchase.PriceAddedFor);

                        if (Purchase.PurchaseDetailsId == 0)
                        {
                            if (Purchase.ExpiryDate != null && ItemSetting.OnItemExpiry == 2)
                            {
                                if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                {
                                    IsStopSelling = true;
                                }
                            }

                            ClsPurchaseDetails oClsPurchaseDetails = new ClsPurchaseDetails()
                            {
                                AmountExcTax = Purchase.AmountExcTax,
                                AmountIncTax = Purchase.AmountIncTax,
                                ItemDetailsId = Purchase.ItemDetailsId,
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseExcTax = Purchase.PurchaseExcTax,
                                PurchaseIncTax = Purchase.PurchaseIncTax,
                                //Tax = Purchase.Tax,
                                TaxId = Purchase.TaxId,
                                Discount = Purchase.Discount,
                                Quantity = Purchase.Quantity,
                                UnitCost = Purchase.UnitCost,
                                IsActive = Purchase.IsActive,
                                IsDeleted = Purchase.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                QuantityRemaining = convertedStock + freeConvertedStock,
                                QuantitySold = 0,
                                LotNo = Purchase.LotNo,
                                ExpiryDate = Purchase.ExpiryDate != null ? Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) : Purchase.ExpiryDate,
                                ManufacturingDate = Purchase.ManufacturingDate != null ? Purchase.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : Purchase.ManufacturingDate,
                                PriceAddedFor = Purchase.PriceAddedFor,
                                SalesIncTax = Purchase.SalesIncTax,
                                FreeQuantity = Purchase.FreeQuantity,
                                //FreeQuantityPriceAddedFor = Purchase.FreeQuantityPriceAddedFor,
                                IsStopSelling = IsStopSelling,
                                TaxAmount = Purchase.TaxAmount,
                                DiscountType = Purchase.DiscountType,
                                DefaultProfitMargin = Purchase.DefaultProfitMargin,
                                UnitAddedFor = Purchase.UnitAddedFor
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsPurchaseDetails.Add(oClsPurchaseDetails);
                            oConnectionContext.SaveChanges();

                            if (IsStopSelling == false)
                            {
                                if (obj.Status.ToLower() != "draft")
                                {
                                    string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                            }
                        }
                        else
                        {
                            decimal QuantityOut = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.QuantitySold).FirstOrDefault();

                            decimal QuantityReturned = (from b in oConnectionContext.DbClsPurchaseReturnDetails
                                                        where b.PurchaseDetailsId == Purchase.PurchaseDetailsId && b.ItemId == Purchase.ItemId &&
                                                        b.ItemDetailsId == Purchase.ItemDetailsId
                                                        select b.QuantityRemaining).FirstOrDefault();

                            bool previousIsStopSelling = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.IsStopSelling).FirstOrDefault();
                            if (previousIsStopSelling == true)
                            {
                                if (ItemSetting.OnItemExpiry == 1)
                                {
                                    flag = true;
                                    IsStopSelling = false;
                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }
                                }
                                else
                                {
                                    if ((Purchase.ExpiryDate != null))
                                    {
                                        if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days > ItemSetting.StopSellingBeforeDays)
                                        {
                                            flag = true;
                                            IsStopSelling = false;
                                            if (obj.Status.ToLower() != "draft")
                                            {
                                                string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
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
                                        if (obj.Status.ToLower() != "draft")
                                        {
                                            string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                            oConnectionContext.Database.ExecuteSqlCommand(query);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ItemSetting.OnItemExpiry != 1)
                                {
                                    if ((Purchase.ExpiryDate != null))
                                    {
                                        if ((Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) - DateTime.Now.Date).Days <= ItemSetting.StopSellingBeforeDays)
                                        {
                                            flag = true;
                                            IsStopSelling = true;
                                            if (obj.Status.ToLower() != "draft")
                                            {
                                                string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"-(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                                oConnectionContext.Database.ExecuteSqlCommand(query);
                                            }
                                        }
                                    }
                                }
                            }

                            if (flag == false)
                            {
                                decimal Quantity = oCommonController.StockConversion(oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.Quantity + a.FreeQuantity).FirstOrDefault(), Purchase.ItemId, oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == Purchase.PurchaseDetailsId).Select(a => a.PriceAddedFor).FirstOrDefault());

                                if (PrevPurchase.Status.ToLower() != "draft")
                                {
                                    string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" = \"Quantity\"-(" + (Quantity - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }

                                if (obj.Status.ToLower() != "draft")
                                {
                                    string query = "update \"tblItemBranchMap\" set \"SalesIncTax\"=" + Purchase.SalesIncTax + ",\"Quantity\" =\"Quantity\"+(" + ((convertedStock + freeConvertedStock) - (QuantityOut + QuantityReturned)) + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemDetailsId\"=" + Purchase.ItemDetailsId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                                //else
                                //{
                                //    if (PrevPurchase.Status.ToLower() != "draft")
                                //    {
                                //        string query = "update tblItemBranchMap set SalesIncTax=" + Purchase.SalesIncTax + ",Quantity = Quantity,0)-(" + Quantity + ") where BranchId=" + obj.BranchId + " and ItemDetailsId=" + Purchase.ItemDetailsId;
                                //        oConnectionContext.Database.ExecuteSqlCommand(query);
                                //    }
                                //}
                            }

                            ClsPurchaseDetails oClsPurchaseDetails = new ClsPurchaseDetails()
                            {
                                PurchaseDetailsId = Purchase.PurchaseDetailsId,
                                AmountExcTax = Purchase.AmountExcTax,
                                AmountIncTax = Purchase.AmountIncTax,
                                ItemId = Purchase.ItemId,
                                ItemDetailsId = Purchase.ItemDetailsId,
                                PurchaseId = oClsPurchase.PurchaseId,
                                PurchaseExcTax = Purchase.PurchaseExcTax,
                                PurchaseIncTax = Purchase.PurchaseIncTax,
                                //Tax = Purchase.Tax,
                                TaxId = Purchase.TaxId,
                                Discount = Purchase.Discount,
                                Quantity = Purchase.Quantity,
                                UnitCost = Purchase.UnitCost,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityOut,
                                LotNo = Purchase.LotNo,
                                ExpiryDate = Purchase.ExpiryDate != null ? Purchase.ExpiryDate.Value.AddHours(5).AddMinutes(30) : Purchase.ExpiryDate,
                                ManufacturingDate = Purchase.ManufacturingDate != null ? Purchase.ManufacturingDate.Value.AddHours(5).AddMinutes(30) : Purchase.ManufacturingDate,
                                PriceAddedFor = Purchase.PriceAddedFor,
                                SalesIncTax = Purchase.SalesIncTax,
                                FreeQuantity = Purchase.FreeQuantity,
                                //FreeQuantityPriceAddedFor = Purchase.FreeQuantityPriceAddedFor,
                                IsStopSelling = IsStopSelling,
                                TaxAmount = Purchase.TaxAmount,
                                DiscountType = Purchase.DiscountType,
                                DefaultProfitMargin = Purchase.DefaultProfitMargin,
                                UnitAddedFor = Purchase.UnitAddedFor
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsPurchaseDetails.Attach(oClsPurchaseDetails);
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PurchaseId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PurchaseExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PurchaseIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.Discount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.LotNo).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ExpiryDate).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ManufacturingDate).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.SalesIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.FreeQuantity).IsModified = true;
                            //oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.IsStopSelling).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.DiscountType).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.DefaultProfitMargin).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Bill \"" + oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" status changed to \"" + obj.Status + "\"",
                    Id = oClsPurchase.PurchaseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill", obj.CompanyId, oClsPurchase.PurchaseId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Bill status changed successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Purchase = new
                        {
                            PurchaseId = oClsPurchase.PurchaseId,
                            InvoiceId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == oClsPurchase.PurchaseId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseBill }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseSalesReport(ClsPurchaseSales obj)
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

            ClsPurchaseSales det;

            if (obj.BranchId == 0)
            {
                det = new ClsPurchaseSales
                {
                    PurchaseExcTax = (from b in oConnectionContext.DbClsPurchase
                                      join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                      where
                                      //b.BranchId == obj.BranchId && 
                                      oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                      select b.Subtotal).DefaultIfEmpty().Sum(),
                    PurchaseIncTax = (from b in oConnectionContext.DbClsPurchase
                                      join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                      where
                                                                           //b.BranchId == obj.BranchId && 
                                                                           oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                      select b.GrandTotal).DefaultIfEmpty().Sum(),
                    PurchaseDiscount = (from b in oConnectionContext.DbClsPurchase
                                        join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                        where
                                                                             //b.BranchId == obj.BranchId && 
                                                                             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                        b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                        select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    PurchaseTax = (from b in oConnectionContext.DbClsPurchase
                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                   where
                                                                        //b.BranchId == obj.BranchId && 
                                                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                   select b.GrandTotal).DefaultIfEmpty().Sum(),
                    PurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                    join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                    where
                                                                         //b.BranchId == obj.BranchId && 
                                                                         oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                    e.Type.ToLower() == "purchase payment" &&
                                    b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                    && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                    select e.Amount).DefaultIfEmpty().Sum(),
                    PurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                   where
                                                                        //b.BranchId == obj.BranchId && 
                                                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                   b.IsDeleted == false && b.IsCancelled == false
                                   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                   select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsPurchase
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where
                                                                          //b.BranchId == obj.BranchId && 
                                                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                     e.Type.ToLower() == "purchase payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                    PurchaseReturnExcTax = (from b in oConnectionContext.DbClsPurchaseReturn
                                            join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                            where
                                                                                 //c.BranchId == obj.BranchId && 
                                                                                 oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                            select b.Subtotal).DefaultIfEmpty().Sum(),
                    PurchaseReturnIncTax = (from b in oConnectionContext.DbClsPurchaseReturn
                                            join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                            where
                                                                                 //c.BranchId == obj.BranchId && 
                                                                                 oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                            select b.GrandTotal).DefaultIfEmpty().Sum(),
                    PurchaseReturnDiscount = (from b in oConnectionContext.DbClsPurchaseReturn
                                              join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                              where
                                                                                   //c.BranchId == obj.BranchId && 
                                                                                   oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                              b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                              select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    PurchaseReturnTax = (from b in oConnectionContext.DbClsPurchaseReturn
                                         join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                         where
                                                                              //c.BranchId == obj.BranchId && 
                                                                              oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                         b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.TaxAmount).DefaultIfEmpty().Sum(),
                    PurchaseReturnPaid = (from b in oConnectionContext.DbClsPurchaseReturn
                                          join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseReturnId equals e.PurchaseId
                                          where
                                                                               //b.BranchId == obj.BranchId && 
                                                                               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
             l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                          e.Type.ToLower() == "supplier refund" &&
                                          b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                          select e.Amount).DefaultIfEmpty().Sum(),
                    PurchaseReturnDue = (from b in oConnectionContext.DbClsPurchaseReturn
                                         where
                                                                              //b.BranchId == obj.BranchId && 
                                                                              oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                         b.IsDeleted == false && b.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsPurchaseReturn
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where
                                                                          //b.BranchId == obj.BranchId && 
                                                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                     e.Type.ToLower() == "supplier refund" &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                    SalesExcTax = (from b in oConnectionContext.DbClsSales
                                   join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                   where
                                      //b.BranchId == obj.BranchId && 
                                      oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                   select b.Subtotal).DefaultIfEmpty().Sum(),
                    SalesIncTax = (from b in oConnectionContext.DbClsSales
                                   join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                   where
                                                                           //b.BranchId == obj.BranchId && 
                                                                           oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                   select b.GrandTotal).DefaultIfEmpty().Sum(),
                    SalesDiscount = (from b in oConnectionContext.DbClsSales
                                     join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                     where
                                                                             //b.BranchId == obj.BranchId && 
                                                                             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                        b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                     select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    SalesTax = (from b in oConnectionContext.DbClsSales
                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                where
                                                                        //b.BranchId == obj.BranchId && 
                                                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                select b.GrandTotal).DefaultIfEmpty().Sum(),
                    SalesPaid = (from b in oConnectionContext.DbClsSales
                                 join e in oConnectionContext.DbClsSupplierPayment on b.SalesId equals e.PurchaseId
                                 where
                                                                      //b.BranchId == obj.BranchId && 
                                                                      oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                 e.Type.ToLower() == "sales payment" &&
                                 b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                 && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                 select e.Amount).DefaultIfEmpty().Sum(),
                    SalesDue = (from b in oConnectionContext.DbClsSales
                                where
                                                                     //b.BranchId == obj.BranchId && 
                                                                     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                b.IsDeleted == false && b.IsCancelled == false
                                && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsSales
                                     join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                     where
                                                                          //b.BranchId == obj.BranchId && 
                                                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                     e.Type.ToLower() == "sales payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                    SalesReturnExcTax = (from b in oConnectionContext.DbClsSalesReturn
                                         join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                         where
                                                                                 //c.BranchId == obj.BranchId && 
                                                                                 oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.Subtotal).DefaultIfEmpty().Sum(),
                    SalesReturnIncTax = (from b in oConnectionContext.DbClsSalesReturn
                                         join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                         where
                                                                              //c.BranchId == obj.BranchId && 
                                                                              oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                         b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.GrandTotal).DefaultIfEmpty().Sum(),
                    SalesReturnDiscount = (from b in oConnectionContext.DbClsSalesReturn
                                           join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                           where
                                                                                //c.BranchId == obj.BranchId && 
                                                                                oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                           b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                         DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                           select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    SalesReturnTax = (from b in oConnectionContext.DbClsSalesReturn
                                      join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                      where
                                                                           //c.BranchId == obj.BranchId && 
                                                                           oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                      && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      select b.TaxAmount).DefaultIfEmpty().Sum(),
                    SalesReturnPaid = (from b in oConnectionContext.DbClsSalesReturn
                                       join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                       join e in oConnectionContext.DbClsCustomerPayment on b.SalesReturnId equals e.SalesId
                                       where
                                                                            //b.BranchId == obj.BranchId && 
                                                                            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                       (e.Type.ToLower() == "customer refund") &&
                                       b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                       && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                   DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                       select e.Amount).DefaultIfEmpty().Sum(),
                    SalesReturnDue = (from b in oConnectionContext.DbClsSalesReturn
                                      join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                      where
                                                                              //b.BranchId == obj.BranchId && 
                                                                              oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                         b.IsDeleted == false && b.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsSalesReturn
                                     join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                     join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                     where
                                                                          //b.BranchId == obj.BranchId && 
                                                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                                     (e.Type.ToLower() == "customer refund") &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),

                };
            }
            else
            {
                det = new ClsPurchaseSales
                {
                    PurchaseExcTax = (from b in oConnectionContext.DbClsPurchase
                                      join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                      where
                                      b.BranchId == obj.BranchId &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                      select b.Subtotal).DefaultIfEmpty().Sum(),
                    PurchaseIncTax = (from b in oConnectionContext.DbClsPurchase
                                      join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                      where
                                                                           b.BranchId == obj.BranchId &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                      select b.GrandTotal).DefaultIfEmpty().Sum(),
                    PurchaseDiscount = (from b in oConnectionContext.DbClsPurchase
                                        join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                        where
                                                                             b.BranchId == obj.BranchId &&
                                        b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                        select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    PurchaseTax = (from b in oConnectionContext.DbClsPurchase
                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                   where
                                                                        b.BranchId == obj.BranchId &&
                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                   select b.GrandTotal).DefaultIfEmpty().Sum(),
                    PurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                    join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                    where
                                                                         b.BranchId == obj.BranchId &&
                                    e.Type.ToLower() == "purchase payment" &&
                                    b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                    && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                    select e.Amount).DefaultIfEmpty().Sum(),
                    PurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                   where
                                                                        b.BranchId == obj.BranchId &&
                                   b.IsDeleted == false && b.IsCancelled == false
                                   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                   select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsPurchase
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where
                                                                          b.BranchId == obj.BranchId &&
                                     e.Type.ToLower() == "purchase payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                    PurchaseReturnExcTax = (from b in oConnectionContext.DbClsPurchaseReturn
                                            join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                            where
                                                                                 c.BranchId == obj.BranchId &&
                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                            select b.Subtotal).DefaultIfEmpty().Sum(),
                    PurchaseReturnIncTax = (from b in oConnectionContext.DbClsPurchaseReturn
                                            join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                            where
                                                                                 c.BranchId == obj.BranchId &&
                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                            select b.GrandTotal).DefaultIfEmpty().Sum(),
                    PurchaseReturnDiscount = (from b in oConnectionContext.DbClsPurchaseReturn
                                              join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                              where
                                                                                   c.BranchId == obj.BranchId &&
                                              b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                              select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    PurchaseReturnTax = (from b in oConnectionContext.DbClsPurchaseReturn
                                         join c in oConnectionContext.DbClsPurchase on b.PurchaseId equals c.PurchaseId
                                         where
                                                                              c.BranchId == obj.BranchId &&
                                         b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.TaxAmount).DefaultIfEmpty().Sum(),
                    PurchaseReturnPaid = (from b in oConnectionContext.DbClsPurchaseReturn
                                          join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseReturnId equals e.PurchaseId
                                          where
                                                                               b.BranchId == obj.BranchId &&
                                          e.Type.ToLower() == "supplier refund" &&
                                          b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                          select e.Amount).DefaultIfEmpty().Sum(),
                    PurchaseReturnDue = (from b in oConnectionContext.DbClsPurchaseReturn
                                         where
                                                                              b.BranchId == obj.BranchId &&
                                         b.IsDeleted == false && b.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsPurchaseReturn
                                     join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                     where
                                                                          b.BranchId == obj.BranchId &&
                                     e.Type.ToLower() == "supplier refund" &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                    SalesExcTax = (from b in oConnectionContext.DbClsSales
                                   join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                   where
                                      b.BranchId == obj.BranchId &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                   select b.Subtotal).DefaultIfEmpty().Sum(),
                    SalesIncTax = (from b in oConnectionContext.DbClsSales
                                   join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                   where
                                                                           b.BranchId == obj.BranchId &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                      && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                   select b.GrandTotal).DefaultIfEmpty().Sum(),
                    SalesDiscount = (from b in oConnectionContext.DbClsSales
                                     join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                     where
                                                                             b.BranchId == obj.BranchId &&
                                        b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                     select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    SalesTax = (from b in oConnectionContext.DbClsSales
                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                where
                                                                        b.BranchId == obj.BranchId &&
                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                select b.GrandTotal).DefaultIfEmpty().Sum(),
                    SalesPaid = (from b in oConnectionContext.DbClsSales
                                 join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                 where
                                                                      b.BranchId == obj.BranchId &&
                                 e.Type.ToLower() == "sales payment" &&
                                 b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                 && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                 select e.Amount).DefaultIfEmpty().Sum(),
                    SalesDue = (from b in oConnectionContext.DbClsSales
                                where
                                                                     b.BranchId == obj.BranchId &&
                                b.IsDeleted == false && b.IsCancelled == false
                                && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsSales
                                     join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                     where
                                                                          b.BranchId == obj.BranchId &&
                                     e.Type.ToLower() == "sales payment" &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                    SalesReturnExcTax = (from b in oConnectionContext.DbClsSalesReturn
                                         join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                         where
                                                                                 c.BranchId == obj.BranchId &&
                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.Subtotal).DefaultIfEmpty().Sum(),
                    SalesReturnIncTax = (from b in oConnectionContext.DbClsSalesReturn
                                         join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                         where
                                                                              c.BranchId == obj.BranchId &&
                                         b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                         select b.GrandTotal).DefaultIfEmpty().Sum(),
                    SalesReturnDiscount = (from b in oConnectionContext.DbClsSalesReturn
                                           join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                           where
                                                                                c.BranchId == obj.BranchId &&
                                           b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                         DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                           select b.TotalDiscount).DefaultIfEmpty().Sum(),
                    SalesReturnTax = (from b in oConnectionContext.DbClsSalesReturn
                                      join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                      where
                                                                           c.BranchId == obj.BranchId &&
                                      b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                      && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      select b.TaxAmount).DefaultIfEmpty().Sum(),
                    SalesReturnPaid = (from b in oConnectionContext.DbClsSalesReturn
                                       join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                       join e in oConnectionContext.DbClsCustomerPayment on b.SalesReturnId equals e.SalesId
                                       where
                                                                            c.BranchId == obj.BranchId &&
                                       (e.Type.ToLower() == "customer refund") &&
                                       b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                       && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                   DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                       select e.Amount).DefaultIfEmpty().Sum(),
                    SalesReturnDue = (from b in oConnectionContext.DbClsSalesReturn
                                      join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                      where
                                                                              c.BranchId == obj.BranchId &&
                                         b.IsDeleted == false && b.IsCancelled == false
                                         && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      select b.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from b in oConnectionContext.DbClsSalesReturn
                                     join c in oConnectionContext.DbClsSales on b.SalesId equals c.SalesId
                                     join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                     where
                                                                          c.BranchId == obj.BranchId &&
                                     (e.Type.ToLower() == "customer refund") &&
                                     b.IsDeleted == false && b.IsCancelled == false && e.IsDeleted == false && e.IsCancelled == false
                                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                     select e.Amount).DefaultIfEmpty().Sum(),
                };
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSale = det,
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseInvoices(ClsPurchaseVm obj)
        {
            List<ClsPurchaseVm> det;
            if (obj.PurchaseType == "Credit Note")
            {
                det = (from b in oConnectionContext.DbClsPurchase
                       where b.IsDeleted == false && b.IsCancelled == false
                       && b.BranchId == obj.BranchId
                       && b.SupplierId == obj.SupplierId
                       && b.PurchaseType == "Purchase"
                       && b.Status.ToLower() != "draft"
                       select new ClsPurchaseVm
                       {
                           PurchaseId = b.PurchaseId,
                           ReferenceNo = b.ReferenceNo
                       }).Distinct().OrderByDescending(a => a.PurchaseId).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsPurchase
                       join c in oConnectionContext.DbClsPurchaseDetails
                       on b.PurchaseId equals c.PurchaseId
                       where b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                       //&& c.QuantityRemaining > 0 
                       && b.BranchId == obj.BranchId
                       && b.SupplierId == obj.SupplierId
                       && b.Status.ToLower() != "draft"
                       select new ClsPurchaseVm
                       {
                           PurchaseId = b.PurchaseId,
                           ReferenceNo = b.ReferenceNo
                       }).Distinct().OrderByDescending(a => a.PurchaseId).ToList();
            }

            var user = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => new
            {
                a.GstTreatment,
                a.CurrencyId,
                CurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                ExchangeRate = oConnectionContext.DbClsUserCurrencyMap.Where(c => c.CurrencyId == a.CurrencyId && c.CompanyId == obj.CompanyId).Select(c => c.ExchangeRate).FirstOrDefault(),
                DefaultCurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(c => c.IsMain == true && c.CompanyId == obj.CompanyId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                a.AdvanceBalance,
                //a.PayTerm,
                //a.PayTermNo,
                a.PlaceOfSupplyId,
                a.SourceOfSupplyId,
                a.PaymentTermId,
                a.TaxExemptionId
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Purchases = det,
                    User = user
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDetails(ClsPurchaseVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            var det = (from bb in oConnectionContext.DbClsPurchase
                       where bb.PurchaseId == obj.PurchaseId && bb.CompanyId == obj.CompanyId && bb.IsActive == true && bb.IsDeleted == false && bb.IsCancelled == false
                       select new
                       {
                           bb.IsReverseCharge,
                           bb.IsCancelled,
                           bb.SourceOfSupplyId,
                           bb.DestinationOfSupplyId,
                           bb.TotalTaxAmount,
                           CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == bb.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                           bb.PurchaseId,
                           bb.PurchaseDate,
                           PurchaseInvoiceNo = bb.ReferenceNo,
                           bb.SupplierId,
                           bb.BranchId,
                           Branch = oConnectionContext.DbClsBranch.Where(z => z.BranchId == bb.BranchId).Select(z => z.Branch).FirstOrDefault(),
                           SupplierName = oConnectionContext.DbClsUser.Where(z => z.UserId == bb.SupplierId).Select(z => z.Name).FirstOrDefault(),
                           PurchaseReturnId = 0,
                           SmsSettingsId = bb.SmsSettingsId,
                           EmailSettingsId = bb.EmailSettingsId,
                           WhatsappSettingsId = bb.WhatsappSettingsId,
                           DueDate = CurrentDate,
                           PaymentTermId = oConnectionContext.DbClsPaymentTerm.Where(z => z.CompanyId == obj.CompanyId && z.IsDueUponReceipt == true).Select(z => z.PaymentTermId).FirstOrDefault(),
                           PurchaseDetails = (from b in oConnectionContext.DbClsPurchaseDetails
                                              join c in oConnectionContext.DbClsItemDetails
                                              on b.ItemDetailsId equals c.ItemDetailsId
                                              join d in oConnectionContext.DbClsItem
                                              on c.ItemId equals d.ItemId
                                              where b.PurchaseId == obj.PurchaseId && b.IsDeleted == false
                                              select new
                                              {
                                                  d.IsManageStock,
                                                  b.ExtraDiscount,
                                                  b.PurchaseDetailsId,
                                                  QuantityReturned = 0,
                                                  QuantityReturnedPriceAddedFor = b.PriceAddedFor,
                                                  PurchaseReturnUnitCost = b.PurchaseIncTax,
                                                  FreeQuantityReturned = 0,
                                                  //FreeQuantityReturnedPriceAddedFor = b.FreeQuantityPriceAddedFor,
                                                  b.QuantityRemaining,
                                                  PurchaseReturnPrice = b.PurchaseIncTax,
                                                  AmountExcTax = 0,
                                                  AmountIncTax = 0,
                                                  b.Discount,
                                                  Quantity = b.Quantity,
                                                  b.FreeQuantity,
                                                  //b.FreeQuantityPriceAddedFor,
                                                  b.TaxId,
                                                  b.UnitCost,
                                                  d.ItemId,
                                                  d.ProductType,
                                                  c.ItemDetailsId,
                                                  d.ItemName,
                                                  SKU = d.ProductType == "Single" ? d.SkuCode : c.SKU,
                                                  c.VariationDetailsId,
                                                  VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                  b.PurchaseExcTax,
                                                  b.PurchaseIncTax,
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
                                              }).ToList(),
                       }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseReturn = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseJournal(ClsPurchaseVm obj)
        {
            var taxList = (from q in oConnectionContext.DbClsPurchaseTaxJournal
                           join a in oConnectionContext.DbClsPurchaseDetails
                           on q.PurchaseDetailsId equals a.PurchaseDetailsId
                           join b in oConnectionContext.DbClsPurchase
                        on a.PurchaseId equals b.PurchaseId
                           //join c in oConnectionContext.DbClsTax
                           //  on q.TaxId equals c.TaxId
                           where q.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //&& a.TaxAmount != 0
                           //&& c.TaxTypeId != 0
                           select new
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = (q.PurchaseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                               Credit = (q.PurchaseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                               AccountId = q.AccountId
                           }).Concat(from q in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                     join a in oConnectionContext.DbClsPurchaseAdditionalCharges
                                     on q.PurchaseAdditionalChargesId equals a.PurchaseAdditionalChargesId
                                     join b in oConnectionContext.DbClsPurchase
                                  on a.PurchaseId equals b.PurchaseId
                                     //join c in oConnectionContext.DbClsTax
                                     //  on q.TaxId equals c.TaxId
                                     where q.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                     //&& a.TaxAmount != 0
                                     //&& c.TaxTypeId != 0
                                     && a.AmountExcTax > 0
                                     select new
                                     {
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = (q.PurchaseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                         Credit = (q.PurchaseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                         AccountId = q.AccountId
                                     }).ToList();

            var purchaseAccount = (from a in oConnectionContext.DbClsPurchaseDetails
                                   join b in oConnectionContext.DbClsPurchase
                                on a.PurchaseId equals b.PurchaseId
                                   where a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                   && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   select new ClsBankPaymentVm
                                   {
                                       AccountId = a.AccountId,
                                       Debit = a.UnitCost * a.Quantity, //b.IsReverseCharge == 1 ? a.Amount : a.AmountExcTax,
                                       Credit = 0
                                   }).ToList();

            var journal = (from a in oConnectionContext.DbClsPurchase
                           where a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               ////Account Payable
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = 0,
                               Credit = a.IsReverseCharge == 1 ? a.GrandTotalReverseCharge : a.GrandTotal
                           })
                           .Concat(from a in oConnectionContext.DbClsPurchase
                                   where a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                   select new ClsBankPaymentVm
                                   {
                                       // Round off charge
                                       AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.RoundOffAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                       Debit = a.RoundOff > 0 ? a.RoundOff : 0,
                                       Credit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0
                                   }).Concat(from a in oConnectionContext.DbClsPurchase
                                             where a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                             select new ClsBankPaymentVm
                                             {
                                                 // Special discount 
                                                 AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.SpecialDiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                 Debit = 0,
                                                 Credit = a.SpecialDiscount,
                                             }).Concat(from a in oConnectionContext.DbClsPurchase
                                             where a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                             select new ClsBankPaymentVm
                                             {
                                                 // discount 
                                                 AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.DiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                 Debit = 0,
                                                 Credit = a.TotalDiscount,
                                             })
                                                .Concat(from a in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                        where a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId
                                                        && a.IsDeleted == false && a.IsActive == true
                                                        select new ClsBankPaymentVm
                                                        {
                                                            // Write Off journal account 
                                                            AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                            Debit = a.AmountExcTax,
                                                            Credit = 0,
                                                        }).ToList();

            journal = journal.Concat(from a in purchaseAccount
                                     group a by a.AccountId into stdGroup
                                     //orderby stdgroup.key descending
                                     select new ClsBankPaymentVm
                                     {
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                         Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                     }).Concat(from a in taxList
                                               group a by a.AccountId into taxGroup
                                               select new ClsBankPaymentVm
                                               {
                                                   // tax 
                                                   AccountName = taxGroup.FirstOrDefault()?.AccountName,
                                                   Debit = taxGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                                   Credit = taxGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                                   IsTaxAccount = true
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

        public async Task<IHttpActionResult> PurchaseInvoicesByReference(ClsPurchaseVm obj)
        {
            List<ClsPurchaseVm> det = new List<ClsPurchaseVm>();
            List<ClsPurchaseVm> det1 = new List<ClsPurchaseVm>();
            List<ClsPurchaseVm> det2 = new List<ClsPurchaseVm>();

            if (obj.ReferenceType == "purchase quotation")
            {
                det1 = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && a.ReferenceId == obj.ReferenceId && a.ReferenceType == obj.ReferenceType).Select(a => new ClsPurchaseVm
            {
                TotalTaxAmount = a.TotalTaxAmount,
                InvoiceId = a.InvoiceId,
                BranchId = a.BranchId,
                InvoiceUrl = oCommonController.webUrl,// + "/purchase/invoice?InvoiceNo=" + a.ReferenceNo+"&Id="+a.CompanyId,
                TotalQuantity = a.TotalQuantity,
                PaidQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                FreeQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                PurchaseId = a.PurchaseId,
                GrandTotal = a.GrandTotal,
                Notes = a.Notes,
                PurchaseDate = a.PurchaseDate,
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
                Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
                oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                //PaymentStatus = a.PaymentStatus,
                Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                // PurchaseReturnDue = oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false
                // && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Count() == 0 ?
                // oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault() :
                //oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault()
                //- oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                ReferenceType = a.ReferenceType
            }).ToList();

                long PurchaseOrderRefId = oConnectionContext.DbClsPurchaseOrder.Where(b =>
                            b.ReferenceId == obj.ReferenceId && b.ReferenceType == obj.ReferenceType).Select(b => b.PurchaseOrderId).FirstOrDefault();

                det2 = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == PurchaseOrderRefId && a.ReferenceType == "purchase order").Select(a => new ClsPurchaseVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               InvoiceUrl = oCommonController.webUrl,// + "/purchase/invoice?InvoiceNo=" + a.ReferenceNo+"&Id="+a.CompanyId,
               TotalQuantity = a.TotalQuantity,
               PaidQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               PurchaseId = a.PurchaseId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               PurchaseDate = a.PurchaseDate,
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
               Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
               oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //PaymentStatus = a.PaymentStatus,
               Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
               a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               // PurchaseReturnDue = oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false
               // && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
               // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Count() == 0 ?
               // oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault() :
               //oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault()
               //- oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
               // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                   a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                   a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                   a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                   a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                   a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                   a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                   oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
               ReferenceType = a.ReferenceType
           }).ToList();
                det = det1.Concat(det2).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && a.ReferenceId == obj.ReferenceId && a.ReferenceType == obj.ReferenceType).Select(a => new ClsPurchaseVm
            {
                TotalTaxAmount = a.TotalTaxAmount,
                InvoiceId = a.InvoiceId,
                BranchId = a.BranchId,
                InvoiceUrl = oCommonController.webUrl,// + "/purchase/invoice?InvoiceNo=" + a.ReferenceNo+"&Id="+a.CompanyId,
                TotalQuantity = a.TotalQuantity,
                PaidQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                FreeQuantity = oConnectionContext.DbClsPurchaseDetails.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                PurchaseId = a.PurchaseId,
                GrandTotal = a.GrandTotal,
                Notes = a.Notes,
                PurchaseDate = a.PurchaseDate,
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
                Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
                oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                //PaymentStatus = a.PaymentStatus,
                Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                IsPurchaseReturn = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                // PurchaseReturnDue = oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false
                // && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Count() == 0 ?
                // oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault() :
                //oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).FirstOrDefault()
                //- oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseId == a.PurchaseId && c.IsDeleted == false && c.IsCancelled == false)
                // .Select(c => c.PurchaseReturnId).FirstOrDefault()).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                ReferenceType = a.ReferenceType
            }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Purchases = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UnpaidPurchaseInvoices(ClsPurchase obj)
        {
            //var det = (from b in oConnectionContext.DbClsPurchase
            //           where b.Status != "Draft" && b.IsDeleted == false && b.IsCancelled == false
            //           && b.SupplierId == obj.SupplierId
            //           select new
            //           {
            //               b.PurchaseId,
            //               b.ReferenceNo,
            //               b.PurchaseDate,
            //               b.GrandTotal,
            //               Due = b.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(bb => bb.Type.ToLower() == "Purchase"
            //               && b.IsDeleted == false && b.IsCancelled == false && bb.PurchaseId == b.PurchaseId).Select(bb => bb.Amount).DefaultIfEmpty().Sum()
            //           }).Distinct().OrderByDescending(a => a.PurchaseId).ToList();

            var Dues = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && a.SupplierId == obj.SupplierId && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                ).OrderBy(a => a.PurchaseId).Select(a => new ClsPurchaseVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    PurchaseDate = a.PurchaseDate,
                    ReferenceNo = a.ReferenceNo,
                    GrandTotal = a.GrandTotal,
                    Type = "Purchase Payment",
                    BranchId = a.BranchId,
                    PurchaseId = a.PurchaseId,
                    SupplierId = a.SupplierId,
                    Due = a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                }).ToList();

            if ((oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.OpeningBalance).FirstOrDefault() -
            oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier opening balance payment" &&
            b.IsDeleted == false && b.IsCancelled == false && b.SupplierId == obj.SupplierId).Select(b => b.Amount).DefaultIfEmpty().Sum()) > 0)
            {
                Dues = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => new ClsPurchaseVm
                {
                    IsReverseCharge = 2,
                    PurchaseDate = a.AddedOn,
                    InvoiceNo = "",
                    GrandTotal = a.OpeningBalance,
                    Type = "Supplier Opening Balance Payment",
                    BranchId = 0,
                    SupplierId = a.UserId,
                    PurchaseId = 0,
                    Due = a.OpeningBalance - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() ==
                    "supplier opening balance payment" && b.IsDeleted == false && b.IsCancelled == false && b.SupplierId == obj.SupplierId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                }).ToList().Union(Dues).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Purchases = Dues,
                }
            };
            return await Task.FromResult(Ok(data));

        }

        public async Task<IHttpActionResult> ImportPurchase(ClsPurchaseVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.PurchaseImports == null || obj.PurchaseImports.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No data",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Plan quota check
            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
a.StartDate != null && a.Status == 2).Select(a => a.TransactionId).FirstOrDefault();

            var Transaction = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => new ClsTransactionVm
            {
                StartDate = a.StartDate,
                ExpiryDate = a.ExpiryDate,
            }).FirstOrDefault();

            int TotalPurchaseBillUsed = oConnectionContext.DbClsPurchase.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
&& (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();

            int TotalExpenseUsed = oConnectionContext.DbClsExpense.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
&& (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();

            int TotalOrder = oCommonController.fetchPlanQuantity(obj.CompanyId, "Bill");
            if ((TotalPurchaseBillUsed + TotalExpenseUsed + obj.PurchaseImports.Count) >= TotalOrder)
            {
                data = new
                {
                    Status = 0,
                    Message = "Purchase Bill/ Expense quota already used. Please upgrade addons from My Plan Menu",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            int count = 1;
            foreach (var purchase in obj.PurchaseImports)
            {
                // Supplier required
                if (string.IsNullOrEmpty(purchase.SupplierName))
                {
                    errors.Add(new ClsError { Message = $"SupplierName is required in row no {count}", Id = "" });
                    isError = true;
                }

                // Purchase Date required
                if (purchase.PurchaseDate == null || purchase.PurchaseDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = $"PurchaseDate is required in row no {count}", Id = "" });
                    isError = true;
                }

                // Due Date required
                if (purchase.DueDate == null || purchase.DueDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = $"DueDate is required in row no {count}", Id = "" });
                    isError = true;
                }

                // Branch required
                if (string.IsNullOrEmpty(purchase.BranchName))
                {
                    errors.Add(new ClsError { Message = $"BranchName is required in row no {count}", Id = "" });
                    isError = true;
                }

                // Status required
                if (string.IsNullOrEmpty(purchase.Status))
                {
                    errors.Add(new ClsError { Message = $"Status is required in row no {count}", Id = "" });
                    isError = true;
                }

                // ReferenceNo duplicate check
                if (!string.IsNullOrEmpty(purchase.ReferenceNo))
                {
                    if (oConnectionContext.DbClsPurchase.Any(a => a.ReferenceNo.ToLower() == purchase.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false))
                    {
                        errors.Add(new ClsError { Message = $"Duplicate Purchase Bill# exists in row no {count}", Id = "" });
                        isError = true;
                    }
                }

                // PurchaseDetails required
                if (purchase.PurchaseDetails == null || purchase.PurchaseDetails.Count == 0)
                {
                    errors.Add(new ClsError { Message = $"PurchaseDetails are required in row no {count}", Id = "" });
                    isError = true;
                }
                else
                {
                    int detailCount = 1;
                    foreach (var item in purchase.PurchaseDetails)
                    {
                        if (item.Quantity <= 0)
                        {
                            errors.Add(new ClsError { Message = $"Quantity must be greater than 0 in row {count}, item {detailCount}", Id = "" });
                            isError = true;
                        }
                        if (item.UnitCost <= 0)
                        {
                            errors.Add(new ClsError { Message = $"UnitCost must be greater than 0 in row {count}, item {detailCount}", Id = "" });
                            isError = true;
                        }
                        // Add more per-item validations as needed (e.g., LotNo, SalesExcTax, etc.)
                        detailCount++;
                    }
                }

                // Add payment validation if your import supports it
                // if (purchase.Payment != null) { ... }

                count++;
            }

            if (isError)
            {
                data = new
                {
                    Status = 2,
                    Message = "",
                    Errors = errors,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            foreach (var purchase in obj.PurchaseImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    // Supplier
                    long SupplierId = oConnectionContext.DbClsUser
                        .Where(a => a.Name.ToLower() == purchase.SupplierName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                        .Select(a => a.UserId).FirstOrDefault();
                    if (SupplierId == 0)
                    {
                        var supplier = new ClsUser
                        {
                            Name = purchase.SupplierName,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsUser.Add(supplier);
                        oConnectionContext.SaveChanges();
                        SupplierId = supplier.UserId;
                    }

                    // Branch
                    long BranchId = oConnectionContext.DbClsBranch
                        .Where(a => a.Branch.ToLower() == purchase.BranchName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                        .Select(a => a.BranchId).FirstOrDefault();
                    if (BranchId == 0)
                    {
                        var branch = new ClsBranch
                        {
                            Branch = purchase.BranchName,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsBranch.Add(branch);
                        oConnectionContext.SaveChanges();
                        BranchId = branch.BranchId;
                    }

                    // Purchase
                    var oClsPurchase = new ClsPurchase
                    {
                        SupplierId = SupplierId,
                        BranchId = BranchId,
                        CompanyId = obj.CompanyId,
                        PurchaseDate = purchase.PurchaseDate,
                        ReferenceNo = purchase.ReferenceNo,
                        Subtotal = purchase.Subtotal,
                        TaxId = purchase.TaxId,
                        TaxAmount = purchase.TaxAmount,
                        TotalQuantity = purchase.TotalQuantity,
                        Discount = purchase.Discount,
                        DiscountType = purchase.DiscountType,
                        GrandTotal = purchase.GrandTotal,
                        ShippingDetails = purchase.ShippingDetails,
                        ShippingAddress = purchase.ShippingAddress,
                        ShippingStatus = purchase.ShippingStatus,
                        DeliveredTo = purchase.DeliveredTo,
                        Notes = purchase.Notes,
                        Status = purchase.Status,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate
                    };
                    oConnectionContext.DbClsPurchase.Add(oClsPurchase);
                    oConnectionContext.SaveChanges();

                    // Purchase Details
                    foreach (var detail in purchase.PurchaseDetails)
                    {
                        var oClsPurchaseDetails = new ClsPurchaseDetails
                        {
                            PurchaseId = oClsPurchase.PurchaseId,
                            ItemId = detail.ItemId,
                            ItemDetailsId = detail.ItemDetailsId,
                            Quantity = detail.Quantity,
                            UnitCost = detail.UnitCost,
                            AmountExcTax = detail.AmountExcTax,
                            AmountIncTax = detail.AmountIncTax,
                            TaxId = detail.TaxId,
                            Discount = detail.Discount,
                            DiscountType = detail.DiscountType,
                            FreeQuantity = detail.FreeQuantity,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsPurchaseDetails.Add(oClsPurchaseDetails);
                        oConnectionContext.SaveChanges();

                        // Update stock (tblItemBranchMap)
                        string query = $"update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+{detail.Quantity + detail.FreeQuantity} where \"BranchId\"={BranchId} and \"ItemId\"={detail.ItemId} and \"ItemDetailsId\"={detail.ItemDetailsId}";
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }

                    // Activity log
                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Purchase",
                        CompanyId = obj.CompanyId,
                        Description = $"Purchase imported: {purchase.ReferenceNo}",
                        Id = 0,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                    dbContextTransaction.Complete();
                }
            }

            data = new
            {
                Status = 1,
                Message = "Purchases imported successfully",
                Data = new { }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
