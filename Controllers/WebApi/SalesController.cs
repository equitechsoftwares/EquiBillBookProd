using EquiBillBook.Filters;
using EquiBillBook.Helpers;
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
using Vonage.Pricing;
using Vonage.SubAccounts;
//using Twilio.TwiML.Voice;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class SalesController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllSales(ClsSalesVm obj)
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
            List<ClsSalesVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
 //&& a.Status.ToLower() == obj.Status.ToLower()
 //&& a.BranchId == obj.BranchId 
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsSalesVm
                {
                    RecurringSalesId = a.RecurringSalesId,
                    SalesDebitNoteReason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == a.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                    SalesDebitNoteReasonId = a.SalesDebitNoteReasonId,
                    PayTaxForExport = a.PayTaxForExport,
                    ShippingBillId = oConnectionContext.DbClsShippingBill.Where(b => b.SalesId == a.SalesId && b.IsDeleted == false && b.IsActive == true).Select(b => b.ShippingBillId).FirstOrDefault(),
                    //IsDebitNote = a.IsDebitNote,
                    IsWriteOff = a.IsWriteOff,
                    WriteOffAmount = a.WriteOffAmount,
                    TotalTaxAmount = a.TotalTaxAmount,
                    TotalDiscount = a.TotalDiscount,
                    InvoiceId = a.InvoiceId,
                    BranchId = a.BranchId,
                    Status = a.Status,
                    InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    SalesId = a.SalesId,
                    GrandTotal = a.GrandTotal,
                    Notes = a.Notes,
                    SalesDate = a.SalesDate,
                    SalesType = a.SalesType,
                    InvoiceNo = a.InvoiceNo,
                    Subtotal = a.Subtotal,
                    CustomerId = a.CustomerId,
                    GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
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
                    Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    //Status = a.Status,
                    Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                    IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                    TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
                    PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                    FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                    //                    ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
                    //(from c in oConnectionContext.DbClsSalesReturn
                    // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
                    // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
                    // select e.Amount).DefaultIfEmpty().Sum(),
                    Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                    ReferenceType = a.ReferenceType,
                    IsCancelled = a.IsCancelled,
                    TotalItems = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId &&
                    c.IsDeleted == false && c.IsComboItems == false).Count()
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
//&& a.Status.ToLower() == obj.Status.ToLower()
&& a.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new ClsSalesVm
               {
                   RecurringSalesId = a.RecurringSalesId,
                   SalesDebitNoteReason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == a.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                   SalesDebitNoteReasonId = a.SalesDebitNoteReasonId,
                   PayTaxForExport = a.PayTaxForExport,
                   ShippingBillId = oConnectionContext.DbClsShippingBill.Where(b => b.SalesId == a.SalesId && b.IsDeleted == false && b.IsActive == true).Select(b => b.ShippingBillId).FirstOrDefault(),
                   //IsDebitNote = a.IsDebitNote,
                   IsWriteOff = a.IsWriteOff,
                   WriteOffAmount = a.WriteOffAmount,
                   TotalTaxAmount = a.TotalTaxAmount,
                   TotalDiscount = a.TotalDiscount,
                   InvoiceId = a.InvoiceId,
                   BranchId = a.BranchId,
                   Status = a.Status,
                   InvoiceUrl = oCommonController.webUrl + "/sales/invoice?InvoiceNo=" + a.InvoiceNo + "&Id=" + a.CompanyId,
                   BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                   SalesId = a.SalesId,
                   GrandTotal = a.GrandTotal,
                   Notes = a.Notes,
                   SalesDate = a.SalesDate,
                   SalesType = a.SalesType,
                   InvoiceNo = a.InvoiceNo,
                   Subtotal = a.Subtotal,
                   CustomerId = a.CustomerId,
                   GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
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
                   Paid = obj.SalesType == null ? (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? 0 :
               oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()) :
               (oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType.ToLower() && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? 0 :
               oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType.ToLower() && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                   //Status = a.Status,
                   Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                   IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                   TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
                   PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                   FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                   //                   ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
                   //(from c in oConnectionContext.DbClsSalesReturn
                   // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
                   // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
                   // select e.Amount).DefaultIfEmpty().Sum(),
                   Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                   ReferenceType = a.ReferenceType,
                   IsCancelled = a.IsCancelled,
                   TotalItems = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId &&
                    c.IsDeleted == false).Count()
               }).ToList();
            }

            if (obj.From == "dashboard")
            {
                det = det.Where(a => a.Status != "Draft" && a.Status != "Hold").Select(a => a).ToList();
            }

            if (obj.SalesType != null && obj.SalesType != "")
            {
                det = det.Where(a => a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "debit note" || a.SalesType.ToLower() == "bill of supply").Select(a => a).ToList();
            }

            if (obj.Status != null && obj.Status != "")
            {
                if (obj.Status == "Write Off")
                {
                    det = det.Where(a => a.IsWriteOff == true).Select(a => a).ToList();
                }
                else
                {
                    det = det.Where(a => a.Status.ToLower() == obj.Status.ToLower()).Select(a => a).ToList();
                }
            }

            if (obj.InvoiceNo != null && obj.InvoiceNo != "")
            {
                det = det.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower()).Select(a => a).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.RecurringSalesId != 0)
            {
                det = det.Where(a => a.RecurringSalesId== obj.RecurringSalesId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> Sale(ClsSalesVm obj)
        {
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
            //bool EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

            var det = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId).Select(a => new ClsSalesVm
            {
                PointsDiscount = a.PointsDiscount,
                PointsEarned = a.PointsEarned,
                RedeemPoints = a.RedeemPoints,
                Terms = a.Terms,
                AdvanceBalance = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.AdvanceBalance).FirstOrDefault(),
                ShippingBillId = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false).Select(c => c.ShippingBillId).FirstOrDefault(),
                GstTreatment = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.GstTreatment).FirstOrDefault(),
                PayTaxForExport = a.PayTaxForExport,
                TaxCollectedFromCustomer = a.TaxCollectedFromCustomer,
                IsReverseCharge = a.IsReverseCharge,
                TaxableAmount = a.TaxableAmount,
                NetAmountReverseCharge = a.NetAmountReverseCharge,
                RoundOffReverseCharge = a.RoundOffReverseCharge,
                GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                IsWriteOff = a.IsWriteOff,
                WriteOffAmount = a.WriteOffAmount,
                SalesDebitNoteReasonId = a.SalesDebitNoteReasonId,
                IsCancelled = a.IsCancelled,
                ParentInvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.ParentId).Select(b => b.InvoiceNo).FirstOrDefault(),
                ParentId = a.ParentId,
                //IsDebitNote = a.IsDebitNote,
                TaxExemptionId = a.TaxExemptionId,
                BranchStateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                PlaceOfSupplyId = a.PlaceOfSupplyId,
                Status = a.Status,
                TotalTaxAmount = a.TotalTaxAmount,
                SalesType = a.SalesType,
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
                //a.Status,
                //PayTerm=a.PayTerm,
                //PayTermNo=a.PayTermNo,
                PaymentTermId = a.PaymentTermId,
                DueDate = a.DueDate,
                AttachDocument = a.AttachDocument,
                SalesId = a.SalesId,
                GrandTotal = a.GrandTotal,
                TaxId = a.TaxId,
                TotalDiscount = a.TotalDiscount,
                TotalQuantity = a.TotalQuantity,
                Discount = a.Discount,
                Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                DiscountType = a.DiscountType,
                Notes = a.Notes,
                SalesDate = a.SalesDate,
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
                //    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
                //    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
                ReferenceType = a.ReferenceType,
                Payments = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesId == a.SalesId &&
                (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false).Select(b => new ClsCustomerPaymentVm
                {
                    AccountId = b.AccountId,
                    CustomerPaymentId = b.CustomerPaymentId,
                    Amount = b.Amount,
                    AttachDocument = b.AttachDocument,
                    Notes = b.Notes,
                    PaymentDate = b.PaymentDate,
                    PaymentTypeId = b.PaymentTypeId,
                    Type = b.Type
                }).ToList(),
                ChangeReturnPayment = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesId == a.SalesId &&
                b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false).Select(b => new ClsCustomerPaymentVm
                {
                    AccountId = b.AccountId,
                    CustomerPaymentId = b.CustomerPaymentId,
                    Amount = b.Amount,
                    AttachDocument = b.AttachDocument,
                    Notes = b.Notes,
                    PaymentDate = b.PaymentDate,
                    PaymentTypeId = b.PaymentTypeId,
                    Type = b.Type
                }).FirstOrDefault(),
                SalesDetails = (from b in oConnectionContext.DbClsSalesDetails
                                join c in oConnectionContext.DbClsItemBranchMap
                                on b.ItemDetailsId equals c.ItemDetailsId
                                join d in oConnectionContext.DbClsItemDetails
                                 on b.ItemDetailsId equals d.ItemDetailsId
                                join e in oConnectionContext.DbClsItem
                                on d.ItemId equals e.ItemId
                                where b.SalesId == a.SalesId && b.IsDeleted == false && c.BranchId == a.BranchId
                                && b.IsComboItems == false
                                select new ClsSalesDetailsVm
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
                                    SalesDetailsId = b.SalesDetailsId,
                                    OtherInfo = b.OtherInfo,
                                    Discount = b.Discount,
                                    SalesId = b.SalesId,
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
                                    //SalesExcTax = b.LotType == "purchase" ?
                                    //oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                    //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                    //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                    //: d.SalesExcTax,
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

                SalesAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                ).Select(b => new ClsSalesAdditionalChargesVm
                {
                    InterStateTaxId = b.InterStateTaxId,
                    IntraStateTaxId = b.IntraStateTaxId,
                    SalesAdditionalChargesId = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == a.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.SalesAdditionalChargesId).FirstOrDefault(),
                    Name = b.Name,
                    AdditionalChargeId = b.AdditionalChargeId,
                    SalesId = a.SalesId,
                    TaxId = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == a.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                    AmountExcTax = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == a.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                    AmountIncTax = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == a.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                    TaxAmount = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == a.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                    AccountId = oConnectionContext.DbClsSalesAdditionalCharges.Where(c => c.SalesId == a.SalesId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
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

            if (det.SalesDetails != null && det.SalesDetails.Count > 0)
            {
                foreach (var _comboStock in det.SalesDetails)
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
                    Sale = det,
                    Taxs = finalTaxs,
                    //ItemDetails = ItemDetails
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSales(ClsSalesVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                string PrefixType = "";

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

                long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2).Select(a => a.TransactionId).FirstOrDefault();

                var Transaction = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => new ClsTransactionVm
                {
                    StartDate = a.StartDate,
                    ExpiryDate = a.ExpiryDate,
                }).FirstOrDefault();

                int TotalOrderUsed = oConnectionContext.DbClsSales.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();
                int TotalOrder = oCommonController.fetchPlanQuantity(obj.CompanyId, "Order");
                if (TotalOrderUsed >= TotalOrder)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Order quota already used. Please upgrade addons from My Plan Menu",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

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

                if (obj.SalesDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesDate" });
                    isError = true;
                }

                if (obj.DueDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDueDate" });
                    isError = true;
                }

                if (obj.SalesType == "Debit Note")
                {
                    if (obj.SalesDebitNoteReasonId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesDebitNoteReason" });
                        isError = true;
                    }

                    if (obj.ParentId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesInvoice" });
                        isError = true;
                    }
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.InvoiceNo != "" && obj.InvoiceNo != null)
                {
                    if (oConnectionContext.DbClsSales.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Invoice# exists", Id = "divInvoiceNo" });
                        isError = true;
                    }
                }

                if (obj.SalesDetails == null || obj.SalesDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.Payment != null)
                {
                    if (obj.Payment.Amount != 0 || obj.Payment.PaymentDate != DateTime.MinValue || obj.Payment.PaymentTypeId != 0)
                    {
                        if (obj.Payment.Amount == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPAmount" });
                            isError = true;
                        }
                        if (obj.Payment.PaymentDate == DateTime.MinValue)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
                            isError = true;
                        }
                        if (obj.Payment.PaymentTypeId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                            isError = true;
                        }
                        if (obj.Payment.PaymentTypeId != 0)
                        {
                            string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.Payment.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                            if (_paymentType.ToLower() == "advance")
                            {
                                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault() < obj.Payment.Amount)
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Not enough advance balance",
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

                if (obj.SalesDetails != null)
                {
                    foreach (var item in obj.SalesDetails)
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
                    
                    //PrefixType = obj.SalesType == "pos" ? "Pos" : "Sales";
                    PrefixType = obj.SalesType;
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

                decimal PayableAmount = 0;
                //obj.Status = "Due";

                //if (obj.PaymentType != null && obj.PaymentType.ToLower() == "multiple")
                //{
                if (obj.Payments != null)
                {
                    foreach (var item in obj.Payments)
                    {
                        string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == item.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                        if (item.Amount == 0)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "Add amount first",
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                        else
                        {
                            PayableAmount = PayableAmount + item.Amount;

                            if (_paymentType.ToLower() == "advance")
                            {
                                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault() < item.Amount)
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Not enough advance balance",
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
                //}

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
                        //obj.Status = "Due";
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

                if (obj.Payments != null)
                {
                    if (PayableAmount == 0)
                    {
                        //obj.PaymentStatus = "Due";
                        due = obj.GrandTotal;
                    }
                    else
                    {
                        if (obj.GrandTotal <= PayableAmount)
                        {
                            obj.Status = "Paid";
                        }
                        else if (obj.GrandTotal > PayableAmount)
                        {
                            obj.Status = "Partially Paid";
                            due = obj.GrandTotal - PayableAmount;
                        }
                    }
                }

                // check credit limit
                //if (obj.Status.ToLower() == "credit" || obj.Payment == null)
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsSales
                                             join b in oConnectionContext.DbClsCustomerPayment
                                         on a.SalesId equals b.SalesId
                                             where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if (obj.Status != "Draft")
                        {
                            if ((TotalSalesDue + due) > creditLimit)
                            {
                                data = new
                                {
                                    Status = 4,
                                    //Message = "Only " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + (creditLimit - TotalSalesDue) + " credit is available out of " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + creditLimit,
                                    Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalSalesDue)),
                                    Data = new
                                    {
                                        User = new
                                        {
                                            CreditLimit = creditLimit,
                                            TotalSalesDue = TotalSalesDue,
                                            TotalSales = due,
                                            UserId = obj.CustomerId
                                        }
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                    }
                }
                // check credit limit

                List<ClsSalesDetailsVm> _SalesDetails = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                        if (Sales.ProductType.ToLower() == "combo")
                        {
                            Sales.ComboId = oCommonController.CreateToken();
                            var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == Sales.ItemId).Select(a => new
                            {
                                ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                ItemDetailsId = a.ItemDetailsId,
                                ComboItemDetailsId = a.ComboItemDetailsId,
                                Quantity = a.Quantity,
                                a.PriceAddedFor
                            }).ToList();

                            foreach (var item in combo)
                            {
                                _SalesDetails.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                            }
                            _SalesDetails.Add(Sales);
                        }
                        else
                        {
                            _SalesDetails.Add(Sales);
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                if (EnableLotNo == true)
                {
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            if (Sales.ProductType != "Combo")
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (Sales.IsComboItems == true)
                                    {
                                        //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                        decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                        //if (remainingQty < convertedStock)
                                        //{
                                        //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                        //    isError = true;
                                        //}

                                        decimal convertedStock = 0;
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                                    else
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
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.LotType == inner.LotType && Sales.LotId == inner.LotId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                //if (remainingQty < convertedStock)
                                //{
                                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                //    isError = true;
                                //}

                                decimal convertedStock = 0;
                                foreach (var inner in obj.SalesDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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

                //if (obj.SalesType == "Pos")
                //{
                obj.CashRegisterId = oConnectionContext.DbClsCashRegister.Where(a => a.AddedBy == obj.AddedBy && a.Status == 1).Select(a => a.CashRegisterId).FirstOrDefault();
                //}

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                if (obj.DueDate == DateTime.MinValue)
                {
                    obj.DueDate = obj.SalesDate;
                }

                ClsSales oClsSales = new ClsSales()
                {
                    RecurringSalesId = obj.RecurringSalesId,
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
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.DueDate,
                    SalesDate = obj.SalesDate.AddHours(5).AddMinutes(30),
                    SalesId = obj.SalesId,
                    InvoiceNo = obj.InvoiceNo,
                    ShippingAddress = obj.ShippingAddress,
                    ShippingDetails = obj.ShippingDetails,
                    ShippingStatus = obj.ShippingStatus,
                    Subtotal = obj.Subtotal,
                    TaxId = obj.TaxId,
                    TotalQuantity = obj.TotalQuantity,
                    BranchId = obj.BranchId,
                    SalesType = obj.SalesType,
                    //Status = obj.Status ,
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
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //ShippingChargesAccountId = ShippingChargesAccountId,
                    //PackagingChargesAccountId = PackagingChargesAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
                    UserGroupId = UserGroupId,
                    ReferenceId = obj.ReferenceId,
                    ReferenceType = obj.ReferenceType,
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
                    ParentId = obj.ParentId,
                    IsCancelled = false,
                    GstPayment = obj.GstPayment,
                    PrefixId = PrefixId,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    SalesDebitNoteReasonId = obj.SalesDebitNoteReasonId,
                    IsWriteOff = false,
                    WriteOffAmount = 0,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId,
                    Terms = obj.Terms,
                    RedeemPoints = obj.RedeemPoints,
                    PointsDiscount = obj.PointsDiscount,
                    PointsEarned = 0 // Will be updated after processing reward points
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Sales/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Sales/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSales.AttachDocument = filepathPass;
                }

                if (obj.ShippingDocument != "" && obj.ShippingDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Sales/ShippingDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionShippingDocument;

                    string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Sales/ShippingDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSales.ShippingDocument = filepathPass;
                }

                oConnectionContext.DbClsSales.Add(oClsSales);
                oConnectionContext.SaveChanges();

                // Validate and Process Reward Points - Redeem first (before calculating earned points)
                if (obj.RedeemPoints > 0)
                {
                    // Get customer's available points
                    var customer = oConnectionContext.DbClsUser
                        .Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId && !a.IsDeleted)
                        .Select(a => new { a.AvailableRewardPoints })
                        .FirstOrDefault();

                    if (customer != null && customer.AvailableRewardPoints < obj.RedeemPoints)
                    {
                        errors.Add(new ClsError { Message = "Insufficient reward points. Customer has only " + customer.AvailableRewardPoints + " points available, but trying to redeem " + obj.RedeemPoints + " points.", Id = "txtRedeemPoints" });
                        isError = true;
                    }
                    else
                    {
                        bool redemptionSuccess = ProcessRewardPointsRedeemed(oClsSales.SalesId, obj.CustomerId, obj.CompanyId, obj.RedeemPoints, obj.GrandTotal, CurrentDate, obj.AddedBy);
                        if (!redemptionSuccess)
                        {
                            errors.Add(new ClsError { Message = "Failed to process reward points redemption. Insufficient points available.", Id = "txtRedeemPoints" });
                            isError = true;
                        }
                    }
                }

                // Check for errors after reward points validation
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

                // Process Reward Points - Earn (only if status is Final, not Hold/Draft)
                if (obj.Status != null && obj.Status.ToLower() != "hold" && obj.Status.ToLower() != "draft")
                {
                    decimal pointsEarned = ProcessRewardPointsEarned(oClsSales.SalesId, obj.CustomerId, obj.CompanyId, obj.GrandTotal, obj.RedeemPoints, CurrentDate, obj.AddedBy);
                    // Update the sale record with points earned
                    oClsSales.PointsEarned = pointsEarned;
                    oConnectionContext.SaveChanges();
                }

                if (obj.SalesAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesAdditionalCharges)
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

                        ClsSalesAdditionalCharges oClsSalesAdditionalCharges = new ClsSalesAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            SalesId = oClsSales.SalesId,
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
                        oConnectionContext.DbClsSalesAdditionalCharges.Add(oClsSalesAdditionalCharges);
                        oConnectionContext.SaveChanges();

                        foreach (var taxJournal in taxList)
                        {
                            ClsSalesAdditionalTaxJournal oClsSalesAdditionalTaxJournal = new ClsSalesAdditionalTaxJournal()
                            {
                                SalesId = oClsSales.SalesId,
                                SalesAdditionalChargesId = oClsSalesAdditionalCharges.SalesAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                SalesTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsSalesAdditionalTaxJournal.Add(oClsSalesAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (obj.SalesType == "Bill Of Supply")
                        {
                            Sales.TaxId = oConnectionContext.DbClsTax.Where(b => b.Tax == "Non-Taxable").Select(b => b.TaxId).FirstOrDefault();
                            Sales.TaxExemptionId = oConnectionContext.DbClsTaxExemption.Where(b => b.CanDelete == false).Select(b => b.TaxExemptionId).FirstOrDefault();
                        }
                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        decimal convertedStock = 0, freeConvertedStock = 0;
                        if (Sales.ProductType != "Combo")
                        {
                            convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                            freeConvertedStock = oCommonController.StockConversion(Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                if (obj.Status.ToLower() != "draft")
                                {
                                    if (Sales.LotId == 0)
                                    {
                                        Sales.StockDeductionIds = oCommonController.deductStock(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.ItemId, Sales.PriceAddedFor);
                                    }
                                    else
                                    {
                                        Sales.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.LotId, Sales.LotType);
                                    }
                                }
                            }

                            if (Sales.LotType == "stocktransfer")
                            {
                                Sales.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotId).FirstOrDefault();
                                Sales.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotType).FirstOrDefault();
                            }
                            else
                            {
                                Sales.LotIdForLotNoChecking = Sales.LotId;
                                Sales.LotTypeForLotNoChecking = Sales.LotType;
                            }
                        }

                        long SalesAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.SalesAccountId).FirstOrDefault();
                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == Sales.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                        long PurchaseAccountId = 0;
                        long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                        if (InventoryAccountId != 0)
                        {
                            PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                        }

                        DateTime? WarrantyExpiryDate = null;
                        if (Sales.WarrantyId != 0)
                        {
                            var warranty = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == Sales.WarrantyId).Select(a => new
                            {
                                a.Duration,
                                a.DurationNo
                            }).FirstOrDefault();

                            if (warranty.Duration == "Days")
                            {
                                WarrantyExpiryDate = obj.SalesDate.AddDays(warranty.DurationNo);
                            }
                            else if (warranty.Duration == "Months")
                            {
                                WarrantyExpiryDate = obj.SalesDate.AddMonths(Convert.ToInt32(warranty.DurationNo));
                            }
                            else if (warranty.Duration == "Years")
                            {
                                WarrantyExpiryDate = obj.SalesDate.AddYears(Convert.ToInt32(warranty.DurationNo));
                            }
                        }

                        //long ItemCodeId = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ItemCodeId).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == Sales.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        decimal AmountExcTax = Sales.AmountExcTax;
                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Sales.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == Sales.TaxId
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

                        ClsSalesDetails oClsSalesDetails = new ClsSalesDetails()
                        {
                            DiscountType = Sales.DiscountType,
                            OtherInfo = Sales.OtherInfo,
                            PriceIncTax = Sales.PriceIncTax,
                            ItemId = Sales.ItemId,
                            ItemDetailsId = Sales.ItemDetailsId,
                            SalesId = oClsSales.SalesId,
                            TaxId = Sales.TaxId,
                            Discount = Sales.Discount,
                            Quantity = Sales.Quantity,
                            UnitCost = Sales.UnitCost,
                            IsActive = Sales.IsActive,
                            IsDeleted = Sales.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            //StockDeductionIds = Sales.StockDeductionIds,
                            QuantityRemaining = Sales.ProductType == "Combo" ? (Sales.Quantity + Sales.FreeQuantity) : (convertedStock + freeConvertedStock),
                            WarrantyId = Sales.WarrantyId,
                            DefaultUnitCost = DefaultUnitCost,
                            DefaultAmount = Sales.Quantity * DefaultUnitCost,
                            PriceAddedFor = Sales.PriceAddedFor,
                            LotId = Sales.LotId,
                            LotType = Sales.LotType,
                            FreeQuantity = Sales.FreeQuantity,
                            //FreeQuantityPriceAddedFor = Sales.FreeQuantityPriceAddedFor,
                            AmountExcTax = Sales.AmountExcTax,
                            TaxAmount = Sales.TaxAmount,
                            PriceExcTax = Sales.PriceExcTax,
                            AmountIncTax = Sales.AmountIncTax,
                            Under = Sales.Under,
                            UnitAddedFor = Sales.UnitAddedFor,
                            LotIdForLotNoChecking = Sales.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = Sales.LotTypeForLotNoChecking,
                            ComboId = Sales.ComboId,
                            IsComboItems = Sales.IsComboItems,
                            QuantitySold = convertedStock + freeConvertedStock,
                            ComboPerUnitQuantity = Sales.ComboPerUnitQuantity,
                            AccountId = SalesAccountId,
                            DiscountAccountId = DiscountAccountId,
                            TaxAccountId = TaxAccountId,
                            PurchaseAccountId = PurchaseAccountId,
                            InventoryAccountId = InventoryAccountId,
                            WarrantyExpiryDate = WarrantyExpiryDate,
                            ExtraDiscount = Sales.ExtraDiscount,
                            ItemCodeId = Sales.ItemCodeId,
                            TaxExemptionId = Sales.TaxExemptionId,
                            TotalTaxAmount = Sales.TotalTaxAmount,
                            IsCombo = Sales.ProductType == "Combo" ? true : false,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsSalesDetails.Add(oClsSalesDetails);
                        oConnectionContext.SaveChanges();

                        //string ll = "delete from tblSalesDeductionId where SalesDetailsId=" + Sales.oClsSalesDetails;
                        //oConnectionContext.Database.ExecuteSqlCommand(ll);

                        if (Sales.StockDeductionIds != null)
                        {
                            foreach (var l in Sales.StockDeductionIds)
                            {
                                ClsSalesDeductionId oClsSalesDeductionId = new ClsSalesDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    SalesDetailsId = oClsSalesDetails.SalesDetailsId,
                                    SalesId = oClsSales.SalesId,
                                };
                                oConnectionContext.DbClsSalesDeductionId.Add(oClsSalesDeductionId);
                                oConnectionContext.SaveChanges();
                            }
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsSalesTaxJournal oClsSalesTaxJournal = new ClsSalesTaxJournal()
                            {
                                SalesId = oClsSales.SalesId,
                                SalesDetailsId = oClsSalesDetails.SalesDetailsId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                SalesTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsSalesTaxJournal.Add(oClsSalesTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.ChangeReturn != 0)
                {
                    //long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    long PaymentPrefixUserMapId = 0;
                    var paymentPrefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                 join b in oConnectionContext.DbClsPrefixUserMap
                                                  on a.PrefixMasterId equals b.PrefixMasterId
                                                 where a.IsActive == true && a.IsDeleted == false &&
                                                 b.CompanyId == obj.CompanyId && b.IsActive == true
                                                 && b.IsDeleted == false && a.PrefixType == "Payment"
                                                 && b.PrefixId == PrefixId
                                                 select new
                                                 {
                                                     b.PrefixUserMapId,
                                                     b.Prefix,
                                                     b.NoOfDigits,
                                                     b.Counter
                                                 }).FirstOrDefault();
                    PaymentPrefixUserMapId = paymentPrefixSettings.PrefixUserMapId;
                    string ReferenceNo = paymentPrefixSettings.Prefix + paymentPrefixSettings.Counter.ToString().PadLeft(paymentPrefixSettings.NoOfDigits, '0');

                    //long ChangeReturnAccountId = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.ChangeReturnAccountId).FirstOrDefault();

                    ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        Notes = "",
                        Amount = obj.ChangeReturn,
                        PaymentDate = obj.SalesDate.AddHours(5).AddMinutes(30),//CurrentDate,
                        PaymentTypeId = obj.PaymentTypeId,
                        SalesId = oClsSales.SalesId,
                        AttachDocument = "",
                        Type = "Change Return",
                        BranchId = obj.BranchId,
                        AccountId = obj.AccountId,
                        ReferenceNo = ReferenceNo,
                        IsDebit = 1,
                        //OnlinePaymentSettingsId = 0,
                        ReferenceId = oCommonController.CreateToken(),
                        IsDirectPayment = true,
                        JournalAccountId = AccountId,
                        PrefixId = PrefixId
                    };

                    oConnectionContext.DbClsCustomerPayment.Add(oClsPayment);
                    oConnectionContext.SaveChanges();
                    //increase counter
                    string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PaymentPrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                    //increase counter
                }

                if (obj.Payment != null)
                {
                    if (obj.Payment.Amount != 0)
                    {
                        string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.Payment.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();

                        List<ClsCustomerPaymentVm> oAdvanceBalances = new List<ClsCustomerPaymentVm>();
                        if (_paymentType == "Advance")
                        {
                            //decimal AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault();
                            //if (AdvanceBalance < obj.Amount)
                            //{
                            //    errors.Add(new ClsError { Message = "Insuffcient Advance Balance", Id = "divAmount" });
                            //    isError = true;
                            //}
                            //else
                            //{
                            var availableAdvances = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerId == obj.CustomerId && a.IsDeleted == false && a.IsCancelled == false &&
                            a.IsActive == true && a.Type.ToLower() == "customer payment" && a.AmountRemaining > 0).Select(a => new
                            {
                                a.ParentId,
                                a.CustomerPaymentId,
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

                                    string _query1 = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"CustomerPaymentId\"=" + item.CustomerPaymentId;
                                    oConnectionContext.Database.ExecuteSqlCommand(_query1);

                                    amountRemaininToDeduct = amountRemaininToDeduct - amount;

                                    oAdvanceBalances.Add(new ClsCustomerPaymentVm { SalesId = obj.SalesId, CustomerPaymentId = item.CustomerPaymentId, Amount = amount, ParentId = item.ParentId });
                                }
                            }

                            string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Payment.Amount + " where \"UserId\"=" + obj.CustomerId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                            //}
                        }

                        long PaymentPrefixUserMapId = 0;
                        if (obj.Payment.ReferenceNo == "" || obj.Payment.ReferenceNo == null)
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
                            PaymentPrefixUserMapId = prefixSettings.PrefixUserMapId;
                            obj.Payment.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                        }

                        long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                        && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                        if (obj.Payment.AccountId == 0)
                        {
                            obj.Payment.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                        && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault();
                        }

                        if (obj.Payment.PaymentDate == DateTime.MinValue)
                        {
                            obj.Payment.PaymentDate = CurrentDate;
                        }

                        if (oAdvanceBalances != null && oAdvanceBalances.Count > 0)
                        {
                            obj.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                       && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                            foreach (var l in oAdvanceBalances)
                            {
                                ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = obj.Payment.Notes,
                                    Amount = l.Amount,
                                    PaymentDate = obj.Payment.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.Payment.PaymentTypeId,
                                    SalesId = oClsSales.SalesId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = obj.SalesType + " Payment",
                                    BranchId = obj.BranchId,
                                    AccountId = obj.Payment.AccountId,
                                    ReferenceNo = obj.Payment.ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ReferenceId = oCommonController.CreateToken(),
                                    JournalAccountId = JournalAccountId,
                                    CustomerId = obj.CustomerId,
                                    IsDirectPayment = true,
                                    ParentId = l.ParentId,
                                    PrefixId = PrefixId
                                };
                                oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                                oConnectionContext.SaveChanges();

                                //ClsCustomerPaymentDeductionId _oClsCustomerPaymentDeductionId = new ClsCustomerPaymentDeductionId()
                                //{
                                //    AddedBy = obj.AddedBy,
                                //    AddedOn = CurrentDate,
                                //    CompanyId = obj.CompanyId,
                                //    DeductedFromId = l.CustomerPaymentId,
                                //    Amount = l.Amount,
                                //    SalesId = l.SalesId,
                                //    CustomerPaymentId = oClsPayment1.CustomerPaymentId,
                                //    CustomerId = obj.CustomerId
                                //};
                                //oConnectionContext.DbClsCustomerPaymentDeductionId.Add(_oClsCustomerPaymentDeductionId);
                                //oConnectionContext.SaveChanges();
                            }
                        }
                        else
                        {
                            ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                IsActive = obj.IsActive,
                                IsDeleted = obj.IsDeleted,
                                Notes = obj.Payment.Notes,
                                Amount = obj.Payment.Amount,
                                PaymentDate = obj.Payment.PaymentDate.AddHours(5).AddMinutes(30),
                                PaymentTypeId = obj.Payment.PaymentTypeId,
                                SalesId = oClsSales.SalesId,
                                AttachDocument = obj.AttachDocument,
                                Type = obj.SalesType + " Payment",
                                BranchId = obj.BranchId,
                                AccountId = obj.Payment.AccountId,
                                ReferenceNo = obj.Payment.ReferenceNo,
                                IsDebit = 2,
                                //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                ReferenceId = oCommonController.CreateToken(),
                                JournalAccountId = JournalAccountId,
                                CustomerId = obj.CustomerId,
                                IsDirectPayment = true,
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

                                oClsPayment1.AttachDocument = filepathPass;
                            }

                            oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                            oConnectionContext.SaveChanges();
                        }

                        //increase counter
                        string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PaymentPrefixUserMapId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                        //increase counter

                        //ClsActivityLogVm oClsActivityLogVm1 = new ClsActivityLogVm()
                        //{
                        //    AddedBy = obj.AddedBy,
                        //    Browser = obj.Browser,
                        //    Category = PrefixType + " Payment",
                        //    CompanyId = obj.CompanyId,
                        //    Description = "added payment of " + obj.Payment.Amount + " for " + obj.InvoiceNo,
                        //    Id = oClsPayment.CustomerPaymentId,
                        //    IpAddress = obj.IpAddress,
                        //    Platform = obj.Platform,
                        //    Type = "Insert"
                        //};
                        //oCommonController.InsertActivityLog(oClsActivityLogVm1, CurrentDate);
                    }
                }

                if (obj.Payments != null)
                {
                    foreach (var item in obj.Payments)
                    {
                        if (item.Amount != 0)
                        {
                            string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == item.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();

                            List<ClsCustomerPaymentVm> oAdvanceBalances = new List<ClsCustomerPaymentVm>();
                            if (_paymentType == "Advance")
                            {
                                var availableAdvances = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerId == obj.CustomerId && a.IsDeleted == false && a.IsCancelled == false &&
                                a.IsActive == true && a.Type.ToLower() == "customer payment" && a.AmountRemaining > 0).Select(a => new
                                {
                                    a.ParentId,
                                    a.CustomerPaymentId,
                                    a.AmountRemaining
                                }).ToList();

                                decimal amountRemaininToDeduct = item.Amount;
                                foreach (var item1 in availableAdvances)
                                {
                                    if (amountRemaininToDeduct != 0)
                                    {
                                        decimal availableAmount = item1.AmountRemaining;
                                        decimal amount = 0;
                                        if (availableAmount >= amountRemaininToDeduct)
                                        {
                                            amount = amountRemaininToDeduct;
                                        }
                                        else if (availableAmount < amountRemaininToDeduct)
                                        {
                                            amount = availableAmount;
                                        }

                                        string _query1 = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"CustomerPaymentId\"=" + item1.CustomerPaymentId;
                                        oConnectionContext.Database.ExecuteSqlCommand(_query1);

                                        amountRemaininToDeduct = amountRemaininToDeduct - amount;

                                        oAdvanceBalances.Add(new ClsCustomerPaymentVm { SalesId = obj.SalesId, CustomerPaymentId = item1.CustomerPaymentId, Amount = amount, ParentId = item1.ParentId });
                                    }
                                }

                                string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + item.Amount + " where \"UserId\"=" + obj.CustomerId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                            }

                            long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                            && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                            if (item.AccountId == 0)
                            {
                                item.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                            && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();
                            }

                            if (item.PaymentDate == DateTime.MinValue)
                            {
                                item.PaymentDate = CurrentDate;
                            }

                            if (oAdvanceBalances != null && oAdvanceBalances.Count > 0)
                            {
                                item.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                           && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                                foreach (var l in oAdvanceBalances)
                                {
                                    ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        Notes = item.Notes,
                                        Amount = l.Amount,
                                        PaymentDate = item.PaymentDate.AddHours(5).AddMinutes(30),
                                        PaymentTypeId = item.PaymentTypeId,
                                        CustomerId = obj.CustomerId,
                                        SalesId = oClsSales.SalesId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = "Sales Payment",
                                        BranchId = obj.BranchId,
                                        AccountId = item.AccountId,
                                        //ReferenceNo = item.ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = l.CustomerPaymentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = JournalAccountId,
                                        PaymentLinkId = item.PaymentLinkId,
                                        PlaceOfSupplyId = obj.PlaceOfSupplyId,
                                        TaxId = 0,
                                        TaxAccountId = 0,
                                        AmountExcTax = l.Amount,
                                        TaxAmount = 0,
                                        IsBusinessRegistered = userDet.IsBusinessRegistered,
                                        GstTreatment = userDet.GstTreatment,
                                        BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                                        BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                                        BusinessLegalName = userDet.BusinessLegalName,
                                        BusinessTradeName = userDet.BusinessTradeName,
                                        PanNo = userDet.PanNo
                                    };
                                    oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                                    oConnectionContext.SaveChanges();
                                }
                            }
                            else
                            {
                                long PaymentPrefixUserMapId = 0;
                                if (item.ReferenceNo == "" || item.ReferenceNo == null)
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
                                    PaymentPrefixUserMapId = prefixSettings.PrefixUserMapId;
                                    item.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                                }

                                ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = item.Notes,
                                    Amount = item.Amount,
                                    PaymentDate = item.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = item.PaymentTypeId,
                                    SalesId = oClsSales.SalesId,
                                    Type = obj.SalesType + " Payment",
                                    BranchId = obj.BranchId,
                                    AccountId = item.AccountId,
                                    ReferenceNo = item.ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ReferenceId = oCommonController.CreateToken(),
                                    JournalAccountId = JournalAccountId,
                                    CustomerId = obj.CustomerId,
                                    IsDirectPayment = true,
                                    TaxId = 0,
                                    TaxAccountId = 0,
                                    AmountExcTax = item.Amount,
                                    TaxAmount = 0,
                                    PrefixId = PrefixId,
                                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                                };

                                if (item.AttachDocument != "" && item.AttachDocument != null)
                                {
                                    string filepathPass = "";

                                    filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + item.FileExtensionAttachDocument;

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

                                    oClsPayment1.AttachDocument = filepathPass;
                                }

                                oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                                oConnectionContext.SaveChanges();

                                //increase counter
                                string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PaymentPrefixUserMapId;
                                oConnectionContext.Database.ExecuteSqlCommand(qq);
                                //increase counter
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
                    Category = obj.SalesType.ToLower() == "sales" ? "Sales" : "POS",
                    CompanyId = obj.CompanyId,
                    Description = (obj.SalesType.ToLower() == "sales" ? "Sales Invoice" : "POS") + " \"" + obj.InvoiceNo + "\" created",
                    Id = oClsSales.SalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (obj.ReferenceType != "" && obj.ReferenceType != null)
                {
                    if (obj.ReferenceType == "sales quotation")
                    {
                        string qq = "update \"tblSalesQuotation\" set \"Status\"='Invoiced' where \"SalesQuotationId\"=" + obj.ReferenceId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                    }
                    else if (obj.ReferenceType == "sales order")
                    {
                        string qq = "update \"tblSalesOrder\" set \"Status\"='Invoiced' where \"SalesOrderId\"=" + obj.ReferenceId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);

                        qq = "update \"tblSalesQuotation\" set \"Status\"='Invoiced' where \"SalesQuotationId\"=(select \"ReferenceId\" from \"tblSalesOrder\" where \"SalesOrderId\"=" + obj.ReferenceId + ")";
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                    }
                    else if (obj.ReferenceType == "sales proforma")
                    {
                        string qq = "update \"tblSalesProforma\" set \"Status\"='Invoiced' where \"SalesProformaId\"=" + obj.ReferenceId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);

                        qq = "update \"tblSalesOrder\" set \"Status\"='Invoiced' where \"SalesOrderId\"=(select \"ReferenceId\" from \"tblSalesProforma\" where \"SalesProformaId\"=" + obj.ReferenceId + ")";
                        oConnectionContext.Database.ExecuteSqlCommand(qq);

                        qq = "update \"tblSalesQuotation\" set \"Status\"='Invoiced' where \"SalesQuotationId\"=(select \"ReferenceId\" from \"tblSalesOrder\" where \"SalesOrderId\"=(select \"ReferenceId\" from \"tblSalesProforma\" where \"SalesProformaId\"=" + obj.ReferenceId + "))";
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                    }
                    else if (obj.ReferenceType == "delivery challan")
                    {
                        string qq = "update \"tblDeliveryChallan\" set \"Status\"='Invoiced' where \"DeliveryChallanId\"=" + obj.ReferenceId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);

                        qq = "update \"tblSalesOrder\" set \"Status\"='Invoiced' where \"SalesOrderId\"=(select \"ReferenceId\" from \"tblDeliveryChallan\" where \"DeliveryChallanId\"=" + obj.ReferenceId + ")";
                        oConnectionContext.Database.ExecuteSqlCommand(qq);

                        qq = "update \"tblSalesQuotation\" set \"Status\"='Invoiced' where \"SalesQuotationId\"=(select \"ReferenceId\" from \"tblSalesOrder\" where \"SalesOrderId\"=(select \"ReferenceId\" from \"tblDeliveryChallan\" where \"DeliveryChallanId\"=" + obj.ReferenceId + "))";
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                    }
                }

                if (obj.SalesType == "Sales")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Pos")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Pos", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Debit Note")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Debit Note", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Bill Of Supply")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Bill Of Supply", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                //if(arr[0] == "1")
                //{
                //    oCommonController.InsertSmsUsed(oClsSales.SalesId, obj.Status, obj.CompanyId, obj.AddedBy, CurrentDate);
                //}

                // Create KOT if POS sales and AutoCreateKot is enabled
                long createdKotId = 0;
                if (obj.SalesType == "Pos")
                {
                    var posSettings = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).FirstOrDefault();
                    if (posSettings != null && posSettings.EnableKot && posSettings.AutoCreateKot)
                    {
                        // Check if linking to existing KOT
                        if (obj.KotId != 0)
                        {
                            // Link existing KOT to sales
                            var existingKot = oConnectionContext.DbClsKotMaster.Where(a => a.KotId == obj.KotId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).FirstOrDefault();
                            if (existingKot != null)
                            {
                                existingKot.SalesId = oClsSales.SalesId;
                                existingKot.ModifiedBy = obj.AddedBy;
                                existingKot.ModifiedOn = CurrentDate;
                                oConnectionContext.SaveChanges();
                                createdKotId = existingKot.KotId;

                                // Update booking if exists
                                if (existingKot.BookingId != 0)
                                {
                                    var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == existingKot.BookingId && a.IsDeleted == false).FirstOrDefault();
                                    if (booking != null)
                                    {
                                        booking.SalesId = oClsSales.SalesId;
                                        // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                                        booking.ModifiedBy = obj.AddedBy;
                                        booking.ModifiedOn = CurrentDate;
                                        oConnectionContext.SaveChanges();
                                    }
                                }
                            }
                        }
                        else if (obj.TableId !=0 && obj.TableId > 0)
                        {
                            // Create new KOT from sales
                            long kotPrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                  join b in oConnectionContext.DbClsPrefixUserMap
                                                   on a.PrefixMasterId equals b.PrefixMasterId
                                                  where a.IsActive == true && a.IsDeleted == false &&
                                                  b.CompanyId == obj.CompanyId && b.IsActive == true
                                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "kot"
                                                  && b.PrefixId == kotPrefixId
                                                  select new
                                                  {
                                                      b.PrefixUserMapId,
                                                      b.Prefix,
                                                      b.NoOfDigits,
                                                      b.Counter
                                                  }).FirstOrDefault();

                            string kotNo = "";
                            long KotPrefixUserMapId = 0;
                            if (prefixSettings != null)
                            {
                                KotPrefixUserMapId = prefixSettings.PrefixUserMapId;
                                kotNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                            }
                            else
                            {
                                kotNo = "KOT" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsKotMaster.Where(a => a.CompanyId == obj.CompanyId).Count().ToString().PadLeft(4, '0');
                            }

                            // Check for active booking on table
                            long activeBookingId = 0;
                            if (obj.BookingId!=0 && obj.BookingId> 0)
                            {
                                activeBookingId = obj.BookingId;
                            }
                            else
                            {
                                var activeBooking = (from b in oConnectionContext.DbClsTableBooking
                                                    join bt in oConnectionContext.DbClsTableBookingTable on b.BookingId equals bt.BookingId
                                                    where bt.TableId == obj.TableId &&
                                                          b.CompanyId == obj.CompanyId &&
                                                          b.BranchId == obj.BranchId &&
                                                          b.Status != "Cancelled" &&
                                                          b.Status != "Completed" &&
                                                          b.IsDeleted == false &&
                                                          b.BookingDate.Date == CurrentDate.Date &&
                                                          b.BookingTime <= CurrentDate.TimeOfDay &&
                                                          (b.BookingTime.Add(TimeSpan.FromMinutes(b.Duration)) >= CurrentDate.TimeOfDay)
                                                    orderby b.BookingDate descending
                                                    select b).FirstOrDefault();
                                if (activeBooking != null)
                                {
                                    activeBookingId = activeBooking.BookingId;
                                }
                            }

                            ClsKotMaster oKot = new ClsKotMaster()
                            {
                                KotNo = kotNo,
                                TableId = obj.TableId,
                                SalesId = oClsSales.SalesId,
                                BookingId = activeBookingId,
                                OrderType = "DineIn",
                                OrderStatus = "Pending",
                                OrderTime = CurrentDate,
                                ExpectedTime = null,
                                ReadyTime = null,
                                ServedTime = null,
                                WaiterId = 0,
                                GuestCount = 0,
                                CustomerId = obj.CustomerId,
                                SpecialInstructions = obj.Notes,
                                BranchId = obj.BranchId,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                ModifiedBy = obj.AddedBy,
                                Printed = false
                            };

                            oConnectionContext.DbClsKotMaster.Add(oKot);
                            oConnectionContext.SaveChanges();
                            createdKotId = oKot.KotId;

                            // Increase KOT counter
                            if (KotPrefixUserMapId > 0)
                            {
                                string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + KotPrefixUserMapId;
                                oConnectionContext.Database.ExecuteSqlCommand(qq);
                            }

                            // Create KOT Details from Sales Details
                            if (obj.SalesDetails != null && obj.SalesDetails.Count > 0)
                            {
                                foreach (var salesDetail in obj.SalesDetails.Where(sd => !sd.IsComboItems))
                                {
                                    var item = oConnectionContext.DbClsItem.Where(a => a.ItemId == salesDetail.ItemId).FirstOrDefault();
                                    if (item != null)
                                    {
                                        // Get kitchen station for item category
                                        long kitchenStationId = 0;
                                        if (item.CategoryId > 0)
                                        {
                                            var stationMap = oConnectionContext.DbClsKitchenStationCategoryMap
                                                .Where(a => a.CategoryId == item.CategoryId && a.IsActive)
                                                .Select(a => a.KitchenStationId)
                                                .FirstOrDefault();
                                            if (stationMap > 0)
                                            {
                                                kitchenStationId = stationMap;
                                            }
                                        }

                                        ClsKotDetails oKotDetail = new ClsKotDetails()
                                        {
                                            KotId = oKot.KotId,
                                            ItemId = salesDetail.ItemId,
                                            ItemDetailsId = salesDetail.ItemDetailsId,
                                            Quantity = salesDetail.Quantity,
                                            UnitId = item.UnitId,
                                            CookingInstructions = salesDetail.OtherInfo,
                                            ItemStatus = "Pending",
                                            KitchenStationId = kitchenStationId,
                                            EstimatedTime = 0,
                                            StartedCookingAt = null,
                                            ReadyAt = null,
                                            ServedAt = null,
                                            Priority = 0,
                                            CompanyId = obj.CompanyId,
                                            IsActive = true,
                                            IsDeleted = false,
                                            AddedBy = obj.AddedBy,
                                            AddedOn = CurrentDate,
                                            ModifiedBy = obj.AddedBy
                                        };

                                        oConnectionContext.DbClsKotDetails.Add(oKotDetail);
                                    }
                                }
                                oConnectionContext.SaveChanges();
                            }

                            // Update booking if exists
                            if (activeBookingId !=0)
                            {
                                var booking = oConnectionContext.DbClsTableBooking.Where(a => a.BookingId == activeBookingId && a.IsDeleted == false).FirstOrDefault();
                                if (booking != null)
                                {
                                    booking.SalesId = oClsSales.SalesId;
                                    // Note: Relationship maintained via BookingId in tblKotMaster, no need to update booking.KotId
                                    booking.ModifiedBy = obj.AddedBy;
                                    booking.ModifiedOn = CurrentDate;
                                    oConnectionContext.SaveChanges();
                                }
                            }

                            // Emit Socket.IO event for KOT creation
                            try
                            {
                                SocketIoHelper.EmitKotCreated(oKot.KotId, obj.CompanyId, obj.BranchId);
                            }
                            catch { }
                        }
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = (obj.Status != null && obj.Status.ToLower() == "hold") ? "Sales kept on hold successfully" : "Sales created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            SalesId = oClsSales.SalesId,
                            InvoiceId = oClsSales.InvoiceId,
                            KotId = createdKotId
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceBill }).FirstOrDefault(),
                        PosSetting = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceFinal }).FirstOrDefault(),
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDelete(ClsSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int SalesReturnCount = oConnectionContext.DbClsSalesReturn.Where(a => a.CompanyId == obj.CompanyId && a.SalesId == obj.SalesId && a.IsDeleted == false && a.IsCancelled == false && a.IsCancelled == false).Count();
                if (SalesReturnCount > 0)
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

                obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();
                var details = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId && a.IsDeleted == false).Select(a => new
                {
                    a.SalesDetailsId,
                    a.SalesId,
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    //a.QuantityRemaining,
                    //a.PriceAddedFor
                }).ToList();

                //foreach (var item in details)
                //{
                //    if (item.Quantity != item.QuantityRemaining)
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

                ClsSales oClsSales = new ClsSales()
                {
                    SalesId = obj.SalesId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSales.Attach(oClsSales);
                oConnectionContext.Entry(oClsSales).Property(x => x.SalesId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                    //           Where(a => a.SalesDetailsId == item.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();

                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == item.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    string ll = "delete from \"tblSalesDeductionId\" where \"SalesDetailsId\"=" + item.SalesDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(ll);

                    if (_StockDeductionIds != null)
                    {
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);
                        if (_StockDeductionIds != null)
                        {
                            foreach (var res in _StockDeductionIds)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(res.Quantity, item.ItemId, item.PriceAddedFor);
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
                            }
                            ;
                        }
                    }
                }

                var paymentDetails = oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == obj.CompanyId && a.SalesId == obj.SalesId &&
                 (a.Type.ToLower() == "sales payment") && a.IsDeleted == false && a.IsCancelled == false).Select(a => new
                 {
                     a.CustomerPaymentId,
                     a.SalesId,
                     a.Type,
                     a.Amount,
                     PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault(),
                     CustomerId = oConnectionContext.DbClsSales.Where(b => b.SalesId == obj.SalesId).Select(b => b.CustomerId).FirstOrDefault()
                 }).ToList();

                foreach (var item in paymentDetails)
                {
                    ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                    {
                        CustomerPaymentId = item.CustomerPaymentId,
                        IsDeleted = true,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
                    oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    if (item.PaymentType == "Advance")
                    {
                        //var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.CustomerPaymentId == item.CustomerPaymentId).Select(a => new
                        //{
                        //    a.CustomerPaymentDeductionId,
                        //    a.DeductedFromId,
                        //    a.Amount
                        //}).ToList();

                        var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == item.CustomerPaymentId).Select(a => new
                        {
                            a.ParentId,
                            a.Amount
                        }).ToList();

                        foreach (var inner in CustomerPaymentDeductionIds)
                        {
                            string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"CustomerPaymentId\"=" + inner.ParentId;
                            oConnectionContext.Database.ExecuteSqlCommand(q);

                            //q = "update \"tblCustomerPaymentDeductionId\" set \"IsDeleted\"=True where \"CustomerPaymentDeductionId\"=" + inner.CustomerPaymentDeductionId;
                            //oConnectionContext.Database.ExecuteSqlCommand(q);
                        }
                        string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + item.Amount + " where \"UserId\"=" + item.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                var Sale = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => new
                {
                    a.ReferenceId,
                    a.ReferenceType,
                    a.SalesType
                }).FirstOrDefault();

                if (Sale.ReferenceType != "" && Sale.ReferenceType != null)
                {
                    if (Sale.ReferenceType == "sales quotation")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                                {
                                    string PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                                    string qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + Sale.ReferenceId;
                                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                                }
                            }
                        }
                    }
                    else if (Sale.ReferenceType == "sales order")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                            {
                                if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                                {
                                    string PreviousStatus = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                                    string qq = "update \"tblSalesOrder\" set \"Status\"='" + PreviousStatus + "' where \"SalesOrderId\"=" + Sale.ReferenceId;
                                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                                }
                            }

                            long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                            b.SalesOrderId == Sale.ReferenceId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesOrderRefId
                        && a.ReferenceType == "sales quotation" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales quotation" && a.SalesOrderId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales quotation" && a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                    {
                                        if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                               && a.Status == "Invoiced" && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales quotation" && a.DeliveryChallanId != Sale.ReferenceId).Count() == 0)
                                        {
                                            string PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    SalesOrderRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                            string qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + SalesOrderRefId;
                                            oConnectionContext.Database.ExecuteSqlCommand(qq);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Sale.ReferenceType == "sales proforma")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            string PreviousStatus = oConnectionContext.DbClsSalesProforma.Where(a => a.SalesProformaId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                            string qq = "update \"tblSalesProforma\" set \"Status\"='" + PreviousStatus + "' where \"SalesProformaId\"=" + Sale.ReferenceId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);

                            long SalesProRefId = oConnectionContext.DbClsSalesProforma.Where(b =>
                            b.SalesProformaId == Sale.ReferenceId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesProRefId
                        && a.ReferenceType == "sales order" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == SalesProRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == SalesProRefId &&
                                a.DeliveryChallanId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId ==
                                    SalesProRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesOrder\" set \"Status\"='" + PreviousStatus + "' where \"SalesOrderId\"=" + SalesProRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }

                            long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                          b.SalesOrderId == SalesProRefId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesOrderRefId
                        && a.ReferenceType == "sales quotation" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == SalesProRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == SalesProRefId &&
                               a.SalesOrderId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    SalesOrderRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + SalesOrderRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }
                        }
                    }
                    else if (Sale.ReferenceType == "delivery challan")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            string PreviousStatus = oConnectionContext.DbClsDeliveryChallan.Where(a => a.DeliveryChallanId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                            string qq = "update \"tblDeliveryChallan\" set \"Status\"='" + PreviousStatus + "' where \"DeliveryChallanId\"=" + Sale.ReferenceId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);

                            long DeliveryChallanRefId = oConnectionContext.DbClsDeliveryChallan.Where(b =>
                            b.DeliveryChallanId == Sale.ReferenceId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == DeliveryChallanRefId
                        && a.ReferenceType == "sales order" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == DeliveryChallanRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == DeliveryChallanRefId &&
                                a.DeliveryChallanId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId ==
                                    DeliveryChallanRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesOrder\" set \"Status\"='" + PreviousStatus + "' where \"SalesOrderId\"=" + DeliveryChallanRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }

                            long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                          b.SalesOrderId == DeliveryChallanRefId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesOrderRefId
                        && a.ReferenceType == "sales quotation" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == DeliveryChallanRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == DeliveryChallanRefId &&
                               a.SalesOrderId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    SalesOrderRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + SalesOrderRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = Sale.SalesType.ToLower() == "sales" ? "Sales" : "POS",
                    CompanyId = obj.CompanyId,
                    Description = (Sale.SalesType.ToLower() == "sales" ? "Sales Invoice" : "POS") + " \"" + oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsSales.SalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Reverse reward points for deleted sale
                ProcessRewardPointsOnSaleDelete(obj.SalesId, obj.CompanyId, CurrentDate, obj.AddedBy);

                data = new
                {
                    Status = 1,
                    Message = "Sales deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesCancel(ClsSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int SalesReturnCount = oConnectionContext.DbClsSalesReturn.Where(a => a.CompanyId == obj.CompanyId && a.SalesId == obj.SalesId && a.IsDeleted == false && a.IsCancelled == false && a.IsCancelled == false).Count();
                if (SalesReturnCount > 0)
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

                obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();
                var details = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId && a.IsDeleted == false).Select(a => new
                {
                    a.SalesDetailsId,
                    a.SalesId,
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    //a.QuantityRemaining,
                    //a.PriceAddedFor
                }).ToList();

                //foreach (var item in details)
                //{
                //    if (item.Quantity != item.QuantityRemaining)
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

                ClsSales oClsSales = new ClsSales()
                {
                    SalesId = obj.SalesId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSales.Attach(oClsSales);
                oConnectionContext.Entry(oClsSales).Property(x => x.SalesId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                    //           Where(a => a.SalesDetailsId == item.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();

                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == item.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    string ll = "delete from \"tblSalesDeductionId\" where \"SalesDetailsId\"=" + item.SalesDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(ll);

                    if (_StockDeductionIds != null)
                    {
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);
                        if (_StockDeductionIds != null)
                        {
                            foreach (var res in _StockDeductionIds)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(res.Quantity, item.ItemId, item.PriceAddedFor);
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
                            }
                            ;
                        }
                    }
                }

                var paymentDetails = oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == obj.CompanyId && a.SalesId == obj.SalesId &&
                 (a.Type.ToLower() == "sales payment") && a.IsDeleted == false && a.IsCancelled == false).Select(a => new
                 {
                     a.CustomerPaymentId,
                     a.SalesId,
                     a.Type,
                     a.Amount,
                     PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault(),
                     CustomerId = oConnectionContext.DbClsSales.Where(b => b.SalesId == obj.SalesId).Select(b => b.CustomerId).FirstOrDefault()
                 }).ToList();

                foreach (var item in paymentDetails)
                {
                    ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                    {
                        CustomerPaymentId = item.CustomerPaymentId,
                        IsDeleted = true,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
                    oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    if (item.PaymentType == "Advance")
                    {
                        //var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.CustomerPaymentId == item.CustomerPaymentId).Select(a => new
                        //{
                        //    a.CustomerPaymentDeductionId,
                        //    a.DeductedFromId,
                        //    a.Amount
                        //}).ToList();

                        var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == item.CustomerPaymentId).Select(a => new
                        {
                            a.ParentId,
                            a.Amount
                        }).ToList();

                        foreach (var inner in CustomerPaymentDeductionIds)
                        {
                            string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"CustomerPaymentId\"=" + inner.ParentId;
                            oConnectionContext.Database.ExecuteSqlCommand(q);

                            //q = "update \"tblCustomerPaymentDeductionId\" set \"IsDeleted\"=True where \"CustomerPaymentDeductionId\"=" + inner.CustomerPaymentDeductionId;
                            //oConnectionContext.Database.ExecuteSqlCommand(q);
                        }
                        string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + item.Amount + " where \"UserId\"=" + item.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                var Sale = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => new
                {
                    a.ReferenceId,
                    a.ReferenceType,
                    a.SalesType
                }).FirstOrDefault();

                if (Sale.ReferenceType != "" && Sale.ReferenceType != null)
                {
                    if (Sale.ReferenceType == "sales quotation")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                                {
                                    string PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                                    string qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + Sale.ReferenceId;
                                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                                }
                            }
                        }
                    }
                    else if (Sale.ReferenceType == "sales order")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                            {
                                if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                && a.Status == "Invoiced" && a.ReferenceId == Sale.ReferenceId && a.ReferenceType == Sale.ReferenceType).Count() == 0)
                                {
                                    string PreviousStatus = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                                    string qq = "update \"tblSalesOrder\" set \"Status\"='" + PreviousStatus + "' where \"SalesOrderId\"=" + Sale.ReferenceId;
                                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                                }
                            }

                            long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                            b.SalesOrderId == Sale.ReferenceId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesOrderRefId
                        && a.ReferenceType == "sales quotation" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.Status == "Invoiced" && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales quotation" && a.SalesOrderId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales quotation" && a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                    {
                                        if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                               && a.Status == "Invoiced" && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales quotation" && a.DeliveryChallanId != Sale.ReferenceId).Count() == 0)
                                        {
                                            string PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    SalesOrderRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                            string qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + SalesOrderRefId;
                                            oConnectionContext.Database.ExecuteSqlCommand(qq);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Sale.ReferenceType == "sales proforma")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            string PreviousStatus = oConnectionContext.DbClsSalesProforma.Where(a => a.SalesProformaId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                            string qq = "update \"tblSalesProforma\" set \"Status\"='" + PreviousStatus + "' where \"SalesProformaId\"=" + Sale.ReferenceId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);

                            long SalesProRefId = oConnectionContext.DbClsSalesProforma.Where(b =>
                            b.SalesProformaId == Sale.ReferenceId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesProRefId
                        && a.ReferenceType == "sales order" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == SalesProRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == SalesProRefId &&
                                a.DeliveryChallanId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId ==
                                    SalesProRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesOrder\" set \"Status\"='" + PreviousStatus + "' where \"SalesOrderId\"=" + SalesProRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }

                            long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                          b.SalesOrderId == SalesProRefId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesOrderRefId
                        && a.ReferenceType == "sales quotation" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == SalesProRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == SalesProRefId &&
                               a.SalesOrderId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    SalesOrderRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + SalesOrderRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }
                        }
                    }
                    else if (Sale.ReferenceType == "delivery challan")
                    {
                        if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == Sale.ReferenceId
                        && a.ReferenceType == Sale.ReferenceType && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                        {
                            string PreviousStatus = oConnectionContext.DbClsDeliveryChallan.Where(a => a.DeliveryChallanId ==
                                    Sale.ReferenceId).Select(a => a.PreviousStatus).FirstOrDefault();

                            string qq = "update \"tblDeliveryChallan\" set \"Status\"='" + PreviousStatus + "' where \"DeliveryChallanId\"=" + Sale.ReferenceId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);

                            long DeliveryChallanRefId = oConnectionContext.DbClsDeliveryChallan.Where(b =>
                            b.DeliveryChallanId == Sale.ReferenceId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == DeliveryChallanRefId
                        && a.ReferenceType == "sales order" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == DeliveryChallanRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                                && a.Status == "Invoiced" && a.ReferenceType == "sales order" && a.ReferenceId == DeliveryChallanRefId &&
                                a.DeliveryChallanId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesOrder.Where(a => a.SalesOrderId ==
                                    DeliveryChallanRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesOrder\" set \"Status\"='" + PreviousStatus + "' where \"SalesOrderId\"=" + DeliveryChallanRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }

                            long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                          b.SalesOrderId == DeliveryChallanRefId).Select(b => b.ReferenceId).FirstOrDefault();

                            if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.ReferenceId == SalesOrderRefId
                        && a.ReferenceType == "sales quotation" && a.IsDeleted == false && a.IsCancelled == false && a.SalesId != obj.SalesId).Count() == 0)
                            {
                                if (oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == DeliveryChallanRefId &&
                               a.SalesProformaId != Sale.ReferenceId).Count() == 0)
                                {
                                    if (oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.Status == "Invoiced" && a.ReferenceType == "sales quotation" && a.ReferenceId == DeliveryChallanRefId &&
                               a.SalesOrderId != Sale.ReferenceId).Count() == 0)
                                    {
                                        PreviousStatus = oConnectionContext.DbClsSalesQuotation.Where(a => a.SalesQuotationId ==
                                    SalesOrderRefId).Select(a => a.PreviousStatus).FirstOrDefault();

                                        qq = "update \"tblSalesQuotation\" set \"Status\"='" + PreviousStatus + "' where \"SalesQuotationId\"=" + SalesOrderRefId;
                                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                                    }
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = Sale.SalesType.ToLower() == "sales" ? "Sales" : "POS",
                    CompanyId = obj.CompanyId,
                    Description = (Sale.SalesType.ToLower() == "sales" ? "Sales Invoice" : "POS") + " \"" + oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" cancelled",
                    Id = oClsSales.SalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (Sale.SalesType == "Sales")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (Sale.SalesType == "Pos")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Pos", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (Sale.SalesType == "Debit Note")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Debit Note", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (Sale.SalesType == "Bill Of Supply")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Bill Of Supply", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Sales cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSales(ClsSalesVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            if(obj.Status == "Draft")
            {
                obj.Status = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.Status).FirstOrDefault();
            }
            
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

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
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

                if (obj.SalesDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesDate" });
                    isError = true;
                }

                if (obj.DueDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDueDate" });
                    isError = true;
                }

                if (obj.SalesType == "Debit Note")
                {
                    if (obj.SalesDebitNoteReasonId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesDebitNoteReason" });
                        isError = true;
                    }

                    if (obj.ParentId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesInvoice" });
                        isError = true;
                    }
                }

                //if (obj.Status == "" || obj.Status == null)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                //    isError = true;
                //}

                if (obj.SalesDetails == null || obj.SalesDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.Payment != null)
                {
                    if (obj.Payment.Amount != 0 || obj.Payment.PaymentDate != DateTime.MinValue || obj.Payment.PaymentTypeId != 0)
                    {
                        if (obj.Payment.Amount == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPAmount" });
                            isError = true;
                        }
                        if (obj.Payment.PaymentDate == DateTime.MinValue)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
                            isError = true;
                        }
                        if (obj.Payment.PaymentTypeId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                            isError = true;
                        }
                        if (obj.Payment.PaymentTypeId != 0)
                        {
                            string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.Payment.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                            if (_paymentType.ToLower() == "advance")
                            {
                                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault() < obj.Payment.Amount)
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Not enough advance balance",
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

                if (obj.SalesDetails != null)
                {
                    foreach (var item in obj.SalesDetails)
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
                PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                decimal PayableAmount = 0;

                //if (obj.PaymentType != null && obj.PaymentType.ToLower() == "multiple")
                //{
                if (obj.Payments != null)
                {
                    foreach (var item in obj.Payments)
                    {
                        string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == item.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                        if (item.Amount == 0)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "Add amount first",
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                        else
                        {
                            PayableAmount = PayableAmount + item.Amount;

                            if (_paymentType.ToLower() == "advance")
                            {
                                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault() < item.Amount)
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Not enough advance balance",
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
                //}

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
                        //obj.Status = "Due";
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

                if (obj.Payments != null)
                {
                    if (PayableAmount == 0)
                    {
                        //obj.PaymentStatus = "Due";
                        due = obj.GrandTotal;
                    }
                    else
                    {
                        if (obj.GrandTotal <= PayableAmount)
                        {
                            obj.Status = "Paid";
                        }
                        else if (obj.GrandTotal > PayableAmount)
                        {
                            obj.Status = "Partially Paid";
                            due = obj.GrandTotal - PayableAmount;
                        }
                    }
                }

                // check credit limit
                //if (obj.Status.ToLower() == "credit" || obj.Payment == null)
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId && a.SalesId != obj.SalesId
                                            ).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsSales
                                             join b in oConnectionContext.DbClsCustomerPayment
                                         on a.SalesId equals b.SalesId
                                             where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId
                                         && a.SalesId != obj.SalesId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if ((TotalSalesDue + due) > creditLimit)
                        {
                            data = new
                            {
                                Status = 4,
                                //Message = "Maximum credit limit exceeded " + oConnectionContext.DbClsCurrency.Where(b =>
                                // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                //     b.CurrencySymbol).FirstOrDefault() + creditLimit,
                                Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalSalesDue)),
                                Data = new
                                {
                                    User = new
                                    {
                                        CreditLimit = creditLimit,
                                        TotalSalesDue = TotalSalesDue,
                                        TotalSales = due,
                                        UserId = obj.CustomerId
                                    }
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                    }
                }
                // check credit limit

                List<ClsSalesDetailsVm> _SalesDetails = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (Sales.SalesDetailsId != 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.SalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = item.SalesDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1, IsDeleted = Sales.IsDeleted });
                                }
                                _SalesDetails.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails.Add(Sales);
                            }
                        }
                        else
                        {
                            _SalesDetails.Add(Sales);
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails;

                //Release stock
                if (obj.SalesDetails != null)
                {
                    foreach (var StockAdjustment in obj.SalesDetails)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            if (StockAdjustment.SalesDetailsId != 0)
                            {
                                //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                                //    Where(a => a.SalesDetailsId == StockAdjustment.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault() ?? "[]";
                                //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == StockAdjustment.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                if (_StockDeductionIds != null)
                                {
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
                                    }
                                    ;
                                }


                            }
                        }
                        if (StockAdjustment.IsDeleted == true)
                        {
                            string query = "update \"tblSalesDetails\" set \"IsDeleted\"=True where \"SalesDetailsId\"=" + StockAdjustment.SalesDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                    }
                }
                //Release stock

                obj.SalesDetails.RemoveAll(r => r.IsComboItems == true);
                obj.SalesDetails.RemoveAll(r => r.IsDeleted == true);

                List<ClsSalesDetailsVm> _SalesDetails1 = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (Sales.SalesDetailsId == 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oCommonController.CreateToken();
                                var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == Sales.ItemId).Select(a => new
                                {
                                    ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ComboItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails1.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = Sales.SalesDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails1.Add(Sales);
                            }
                        }
                        else
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == Sales.ItemId && b.ComboItemDetailsId == a.ItemDetailsId).Select(b => b.Quantity).FirstOrDefault(),
                                    a.SalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails1.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = item.SalesDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails1.Add(Sales);
                            }
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails1;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                if (EnableLotNo == true)
                {
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            if (Sales.ProductType != "Combo")
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (Sales.IsComboItems == true)
                                    {
                                        //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                        decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                        //if (remainingQty < convertedStock)
                                        //{
                                        //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                        //    isError = true;
                                        //}
                                        decimal convertedStock = 0;
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                                    else
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
                                            if (Sales.SalesDetailsId != 0)
                                            {
                                                remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                            }
                                        }

                                        decimal convertedStock = 0;
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.LotType == inner.LotType && Sales.LotId == inner.LotId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                //if (remainingQty < convertedStock)
                                //{
                                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                //    isError = true;
                                //}
                                decimal convertedStock = 0;
                                foreach (var inner in obj.SalesDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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

                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).FirstOrDefault();

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                if (obj.DueDate == DateTime.MinValue)
                {
                    obj.DueDate = obj.SalesDate;
                }

                ClsSales oClsSales = new ClsSales()
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
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.DueDate,
                    SalesDate = obj.SalesDate.AddHours(5).AddMinutes(30),
                    SalesId = obj.SalesId,
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
                    //Status = obj.Status,
                    PaymentType = obj.PaymentType,
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
                    UserGroupId = UserGroupId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxExemptionId = obj.TaxExemptionId,
                    ParentId = obj.ParentId,
                    GstPayment = obj.GstPayment,
                    IsReverseCharge = obj.IsReverseCharge,
                    PayTaxForExport = obj.PayTaxForExport,
                    TaxCollectedFromCustomer = obj.TaxCollectedFromCustomer,
                    SalesDebitNoteReasonId = obj.SalesDebitNoteReasonId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId,
                    Terms = obj.Terms,
                    RedeemPoints = obj.RedeemPoints,
                    PointsDiscount = obj.PointsDiscount,
                    PointsEarned = 0 // Will be updated after processing reward points
                };

                string pic1 = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/Sales/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Sales/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSales.AttachDocument = filepathPass;
                }
                else
                {
                    oClsSales.AttachDocument = pic1;
                }

                pic1 = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.ShippingDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/Sales/ShippingDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionShippingDocument;

                    string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Sales/ShippingDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ShippingDocument.Replace(obj.ShippingDocument.Substring(0, obj.ShippingDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSales.ShippingDocument = filepathPass;
                }
                else
                {
                    oClsSales.ShippingDocument = pic1;
                }

                oConnectionContext.DbClsSales.Attach(oClsSales);
                oConnectionContext.Entry(oClsSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.CompanyId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.DeliveredTo).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.DueDate).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SalesDate).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SalesId).IsModified = true;
                //oConnectionContext.Entry(oClsSales).Property(x => x.InvoiceNo).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ShippingAddress).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ShippingDetails).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ShippingStatus).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ShippingDocument).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.OnlinePaymentSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ChangeReturn).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TotalPaying).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Balance).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.HoldReason).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PaymentType).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.NetAmount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.RedeemPoints).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PointsDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.RoundOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TaxAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PlaceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ParentId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.GstPayment).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.IsReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SalesDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PayTaxForExport).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.TaxCollectedFromCustomer).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Terms).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.PointsEarned).IsModified = true;
                
                // Reverse old reward points transactions before updating
                ProcessRewardPointsOnSaleDelete(obj.SalesId, obj.CompanyId, CurrentDate, obj.AddedBy);
                
                oConnectionContext.SaveChanges();

                // Validate and Process Reward Points - Redeem first (before calculating earned points)
                if (obj.RedeemPoints > 0)
                {
                    // Get customer's available points after restoration
                    var customerAfterRestore = oConnectionContext.DbClsUser
                        .Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId && !a.IsDeleted)
                        .Select(a => new { a.AvailableRewardPoints })
                        .FirstOrDefault();

                    if (customerAfterRestore != null && customerAfterRestore.AvailableRewardPoints < obj.RedeemPoints)
                    {
                        errors.Add(new ClsError { Message = "Insufficient reward points. Customer has only " + customerAfterRestore.AvailableRewardPoints + " points available, but trying to redeem " + obj.RedeemPoints + " points.", Id = "txtRedeemPoints" });
                        isError = true;
                    }
                    else
                    {
                        bool redemptionSuccess = ProcessRewardPointsRedeemed(obj.SalesId, obj.CustomerId, obj.CompanyId, obj.RedeemPoints, obj.GrandTotal, CurrentDate, obj.AddedBy);
                        if (!redemptionSuccess)
                        {
                            errors.Add(new ClsError { Message = "Failed to process reward points redemption. Insufficient points available.", Id = "txtRedeemPoints" });
                            isError = true;
                        }
                    }
                }

                // Check for errors after reward points validation
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

                // Process Reward Points - Earn (only if status is Final, not Hold/Draft)
                if (obj.Status != null && obj.Status.ToLower() != "hold" && obj.Status.ToLower() != "draft")
                {
                    decimal pointsEarned = ProcessRewardPointsEarned(obj.SalesId, obj.CustomerId, obj.CompanyId, obj.GrandTotal, obj.RedeemPoints, CurrentDate, obj.AddedBy);
                    // Update the sale record with points earned
                    oClsSales.PointsEarned = pointsEarned;
                    oConnectionContext.Entry(oClsSales).Property(x => x.PointsEarned).IsModified = true;
                    oConnectionContext.SaveChanges();
                }

                if (obj.SalesAdditionalCharges != null)
                {
                    foreach (var item in obj.SalesAdditionalCharges)
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

                        long SalesAdditionalChargesId = 0;
                        if (item.SalesAdditionalChargesId == 0)
                        {
                            ClsSalesAdditionalCharges oClsSalesAdditionalCharges = new ClsSalesAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesId = oClsSales.SalesId,
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
                            oConnectionContext.DbClsSalesAdditionalCharges.Add(oClsSalesAdditionalCharges);
                            oConnectionContext.SaveChanges();

                            SalesAdditionalChargesId = oClsSalesAdditionalCharges.SalesAdditionalChargesId;
                        }
                        else
                        {
                            ClsSalesAdditionalCharges oClsSalesAdditionalCharges = new ClsSalesAdditionalCharges()
                            {
                                SalesAdditionalChargesId = item.SalesAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                SalesId = oClsSales.SalesId,
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
                            oConnectionContext.DbClsSalesAdditionalCharges.Attach(oClsSalesAdditionalCharges);
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.SalesId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSalesAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();

                            SalesAdditionalChargesId = oClsSalesAdditionalCharges.SalesAdditionalChargesId;
                        }

                        string query = "delete from \"tblSalesAdditionalTaxJournal\" where \"SalesId\"=" + oClsSales.SalesId + " and \"SalesAdditionalChargesId\"=" + SalesAdditionalChargesId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        foreach (var taxJournal in taxList)
                        {
                            ClsSalesAdditionalTaxJournal oClsSalesAdditionalTaxJournal = new ClsSalesAdditionalTaxJournal()
                            {
                                SalesId = oClsSales.SalesId,
                                SalesAdditionalChargesId = SalesAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                SalesTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsSalesAdditionalTaxJournal.Add(oClsSalesAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (obj.SalesType == "Bill Of Supply")
                        {
                            Sales.TaxId = oConnectionContext.DbClsTax.Where(b => b.Tax == "Non-Taxable").Select(b => b.TaxId).FirstOrDefault();
                            Sales.TaxExemptionId = oConnectionContext.DbClsTaxExemption.Where(b => b.CanDelete == false).Select(b => b.TaxExemptionId).FirstOrDefault();
                        }

                        long SalesAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.SalesAccountId).FirstOrDefault();
                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == Sales.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                        long PurchaseAccountId = 0;
                        long InventoryAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.InventoryAccountId).FirstOrDefault();
                        if (InventoryAccountId != 0)
                        {
                            PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                        }

                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        DateTime? WarrantyExpiryDate = null;
                        if (Sales.WarrantyId != 0)
                        {
                            var warranty = oConnectionContext.DbClsWarranty.Where(a => a.WarrantyId == Sales.WarrantyId).Select(a => new
                            {
                                a.Duration,
                                a.DurationNo
                            }).FirstOrDefault();

                            if (warranty.Duration == "Days")
                            {
                                WarrantyExpiryDate = obj.SalesDate.AddDays(warranty.DurationNo);
                            }
                            else if (warranty.Duration == "Months")
                            {
                                WarrantyExpiryDate = obj.SalesDate.AddMonths(Convert.ToInt32(warranty.DurationNo));
                            }
                            else if (warranty.Duration == "Years")
                            {
                                WarrantyExpiryDate = obj.SalesDate.AddYears(Convert.ToInt32(warranty.DurationNo));
                            }
                        }

                        //                        Sales.SalesDetailsId = oConnectionContext.DbClsSalesDetails.Where(a => a.CompanyId == obj.CompanyId
                        //&& a.IsDeleted == false && a.SalesId == obj.SalesId && a.ItemId == Sales.ItemId
                        //&& a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.SalesDetailsId).FirstOrDefault();

                        if (Sales.SalesDetailsId == 0)
                        {
                            decimal convertedStock = 0, freeConvertedStock = 0;
                            if (Sales.ProductType != "Combo")
                            {
                                convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                                freeConvertedStock = oCommonController.StockConversion(Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        if (Sales.LotId == 0)
                                        {
                                            Sales.StockDeductionIds = oCommonController.deductStock(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.ItemId, Sales.PriceAddedFor);
                                        }
                                        else
                                        {
                                            Sales.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.LotId, Sales.LotType);
                                        }
                                    }
                                }

                                if (Sales.LotType == "stocktransfer")
                                {
                                    Sales.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotId).FirstOrDefault();
                                    Sales.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotType).FirstOrDefault();
                                }
                                else
                                {
                                    Sales.LotIdForLotNoChecking = Sales.LotId;
                                    Sales.LotTypeForLotNoChecking = Sales.LotType;
                                }
                            }

                            string AccountType = "";

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == Sales.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            decimal AmountExcTax = Sales.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Sales.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == Sales.TaxId
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

                            ClsSalesDetails oClsSalesDetails = new ClsSalesDetails()
                            {
                                DiscountType = Sales.DiscountType,
                                OtherInfo = Sales.OtherInfo,
                                PriceIncTax = Sales.PriceIncTax,
                                ItemId = Sales.ItemId,
                                ItemDetailsId = Sales.ItemDetailsId,
                                SalesId = oClsSales.SalesId,
                                TaxId = Sales.TaxId,
                                Discount = Sales.Discount,
                                Quantity = Sales.Quantity,
                                UnitCost = Sales.UnitCost,
                                IsActive = Sales.IsActive,
                                IsDeleted = Sales.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                //StockDeductionIds = Sales.StockDeductionIds,
                                //QuantityRemaining = Sales.IsComboItems == true ? (Sales.Quantity + Sales.FreeQuantity) : (convertedStock + freeConvertedStock),
                                QuantityRemaining = Sales.ProductType == "Combo" ? (Sales.Quantity + Sales.FreeQuantity) : (convertedStock + freeConvertedStock),
                                WarrantyId = Sales.WarrantyId,
                                DefaultUnitCost = DefaultUnitCost,
                                DefaultAmount = Sales.Quantity * DefaultUnitCost,
                                PriceAddedFor = Sales.PriceAddedFor,
                                LotId = Sales.LotId,
                                LotType = Sales.LotType,
                                FreeQuantity = Sales.FreeQuantity,
                                //FreeQuantityPriceAddedFor = Sales.FreeQuantityPriceAddedFor,
                                AmountExcTax = Sales.AmountExcTax,
                                TaxAmount = Sales.TaxAmount,
                                PriceExcTax = Sales.PriceExcTax,
                                AmountIncTax = Sales.AmountIncTax,
                                UnitAddedFor = Sales.UnitAddedFor,
                                LotIdForLotNoChecking = Sales.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = Sales.LotTypeForLotNoChecking,
                                ComboId = Sales.ComboId,
                                IsComboItems = Sales.IsComboItems,
                                QuantitySold = convertedStock + freeConvertedStock,
                                ComboPerUnitQuantity = Sales.ComboPerUnitQuantity,
                                AccountId = SalesAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                PurchaseAccountId = PurchaseAccountId,
                                InventoryAccountId = InventoryAccountId,
                                WarrantyExpiryDate = WarrantyExpiryDate,
                                ExtraDiscount = Sales.ExtraDiscount,
                                TaxExemptionId = Sales.TaxExemptionId,
                                ItemCodeId = Sales.ItemCodeId,
                                TotalTaxAmount = Sales.TotalTaxAmount,
                                IsCombo = Sales.ProductType == "Combo" ? true : false,
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsSalesDetails.Add(oClsSalesDetails);
                            oConnectionContext.SaveChanges();

                            //string ll = "delete from tblSalesDeductionId where SalesDetailsId=" + oClsSalesDetails.SalesDetailsId;
                            //oConnectionContext.Database.ExecuteSqlCommand(ll);

                            if (Sales.StockDeductionIds != null)
                            {
                                foreach (var l in Sales.StockDeductionIds)
                                {
                                    ClsSalesDeductionId oClsSalesDeductionId = new ClsSalesDeductionId()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        Id = l.Id,
                                        Type = l.Type,
                                        Quantity = l.Quantity,
                                        SalesDetailsId = oClsSalesDetails.SalesDetailsId,
                                        SalesId = oClsSales.SalesId,
                                    };
                                    oConnectionContext.DbClsSalesDeductionId.Add(oClsSalesDeductionId);
                                    oConnectionContext.SaveChanges();
                                }
                            }

                            foreach (var taxJournal in taxList)
                            {
                                ClsSalesTaxJournal oClsSalesTaxJournal = new ClsSalesTaxJournal()
                                {
                                    SalesId = oClsSales.SalesId,
                                    SalesDetailsId = oClsSalesDetails.SalesDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    SalesTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsSalesTaxJournal.Add(oClsSalesTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                        }
                        else
                        {
                            decimal QuantityReturned = 0;
                            decimal convertedStock = 0, freeConvertedStock = 0;

                            if (Sales.ProductType != "Combo")
                            {
                                convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                                freeConvertedStock = oCommonController.StockConversion(Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (obj.Status.ToLower() != "draft")
                                    {
                                        if (Sales.LotId == 0)
                                        {
                                            Sales.StockDeductionIds = oCommonController.deductStock(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.ItemId, Sales.PriceAddedFor);
                                        }
                                        else
                                        {
                                            Sales.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.LotId, Sales.LotType);
                                        }
                                    }

                                    QuantityReturned = oCommonController.StockConversion((from a in oConnectionContext.DbClsSalesReturn
                                                                                          join b in oConnectionContext.DbClsSalesReturnDetails
                                                                                             on a.SalesReturnId equals b.SalesReturnId
                                                                                          where a.SalesId == obj.SalesId && b.ItemId == Sales.ItemId &&
                                                                                          b.ItemDetailsId == Sales.ItemDetailsId
                                                                                          select b.Quantity).FirstOrDefault(), Sales.ItemId, Sales.PriceAddedFor);
                                }
                            }
                            else
                            {
                                QuantityReturned = (from a in oConnectionContext.DbClsSalesReturn
                                                    join b in oConnectionContext.DbClsSalesReturnDetails
                                                       on a.SalesReturnId equals b.SalesReturnId
                                                    where a.SalesId == obj.SalesId && b.ItemId == Sales.ItemId &&
                                                    b.ItemDetailsId == Sales.ItemDetailsId
                                                    select b.Quantity).FirstOrDefault();
                            }

                            if (Sales.LotType == "stocktransfer")
                            {
                                Sales.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotId).FirstOrDefault();
                                Sales.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotType).FirstOrDefault();
                            }
                            else
                            {
                                Sales.LotIdForLotNoChecking = Sales.LotId;
                                Sales.LotTypeForLotNoChecking = Sales.LotType;
                            }

                            string AccountType = "";

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == Sales.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            decimal AmountExcTax = Sales.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Sales.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == Sales.TaxId
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

                            ClsSalesDetails oClsSalesDetails = new ClsSalesDetails()
                            {
                                SalesDetailsId = Sales.SalesDetailsId,
                                DiscountType = Sales.DiscountType,
                                OtherInfo = Sales.OtherInfo,
                                PriceIncTax = Sales.PriceIncTax,
                                ItemId = Sales.ItemId,
                                ItemDetailsId = Sales.ItemDetailsId,
                                SalesId = oClsSales.SalesId,
                                TaxId = Sales.TaxId,
                                Discount = Sales.Discount,
                                Quantity = Sales.Quantity,
                                UnitCost = Sales.UnitCost,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                //StockDeductionIds = Sales.StockDeductionIds,
                                QuantityRemaining = Sales.ProductType == "Combo" ? (Sales.Quantity + Sales.FreeQuantity) - QuantityReturned : (convertedStock + freeConvertedStock) - QuantityReturned,
                                //QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityReturned,
                                WarrantyId = Sales.WarrantyId,
                                DefaultUnitCost = DefaultUnitCost,
                                DefaultAmount = Sales.Quantity * DefaultUnitCost,
                                PriceAddedFor = Sales.PriceAddedFor,
                                LotId = Sales.LotId,
                                LotType = Sales.LotType,
                                FreeQuantity = Sales.FreeQuantity,
                                //FreeQuantityPriceAddedFor = Sales.FreeQuantityPriceAddedFor,
                                AmountExcTax = Sales.AmountExcTax,
                                TaxAmount = Sales.TaxAmount,
                                PriceExcTax = Sales.PriceExcTax,
                                AmountIncTax = Sales.AmountIncTax,
                                UnitAddedFor = Sales.UnitAddedFor,
                                LotIdForLotNoChecking = Sales.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = Sales.LotTypeForLotNoChecking,
                                ComboId = Sales.ComboId,
                                IsComboItems = Sales.IsComboItems,
                                QuantitySold = convertedStock + freeConvertedStock,
                                ComboPerUnitQuantity = Sales.ComboPerUnitQuantity,
                                AccountId = SalesAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                PurchaseAccountId = PurchaseAccountId,
                                InventoryAccountId = InventoryAccountId,
                                WarrantyExpiryDate = WarrantyExpiryDate,
                                ExtraDiscount = Sales.ExtraDiscount,
                                TaxExemptionId = Sales.TaxExemptionId,
                                ItemCodeId = Sales.ItemCodeId,
                                TotalTaxAmount = Sales.TotalTaxAmount,
                                IsCombo = Sales.ProductType == "Combo" ? true : false,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsSalesDetails.Attach(oClsSalesDetails);
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DiscountType).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.OtherInfo).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PriceIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.SalesId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.Discount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ModifiedOn).IsModified = true;
                            //oConnectionContext.Entry(oClsSalesDetails).Property(x => x.StockDeductionIds).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.WarrantyId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DefaultUnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DefaultAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotType).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.FreeQuantity).IsModified = true;
                            //oConnectionContext.Entry(oClsSalesDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PriceExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ComboId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.IsComboItems).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.QuantitySold).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ComboPerUnitQuantity).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DiscountAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TaxAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PurchaseAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.InventoryAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.WarrantyExpiryDate).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ExtraDiscount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsSalesDetails).Property(x => x.IsCombo).IsModified = true;
                            oConnectionContext.SaveChanges();

                            if (Sales.StockDeductionIds != null)
                            {
                                string ll = "delete from \"tblSalesDeductionId\" where \"SalesDetailsId\"=" + Sales.SalesDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(ll);

                                foreach (var l in Sales.StockDeductionIds)
                                {
                                    ClsSalesDeductionId oClsSalesDeductionId = new ClsSalesDeductionId()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        Id = l.Id,
                                        Type = l.Type,
                                        Quantity = l.Quantity,
                                        SalesDetailsId = Sales.SalesDetailsId,
                                        SalesId = oClsSales.SalesId,
                                    };
                                    oConnectionContext.DbClsSalesDeductionId.Add(oClsSalesDeductionId);
                                    oConnectionContext.SaveChanges();
                                }
                            }

                            string query = "delete from \"tblSalesTaxJournal\" where \"SalesId\"=" + oClsSales.SalesId + " and \"SalesDetailsId\"=" + oClsSalesDetails.SalesDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            foreach (var taxJournal in taxList)
                            {
                                ClsSalesTaxJournal oClsSalesTaxJournal = new ClsSalesTaxJournal()
                                {
                                    SalesId = oClsSales.SalesId,
                                    SalesDetailsId = oClsSalesDetails.SalesDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    SalesTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsSalesTaxJournal.Add(oClsSalesTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                        }
                    }
                }

                if (obj.ChangeReturn != 0)
                {
                    //long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    long PaymentPrefixUserMapId = 0;
                    var paymentPrefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                 join b in oConnectionContext.DbClsPrefixUserMap
                                                  on a.PrefixMasterId equals b.PrefixMasterId
                                                 where a.IsActive == true && a.IsDeleted == false &&
                                                 b.CompanyId == obj.CompanyId && b.IsActive == true
                                                 && b.IsDeleted == false && a.PrefixType == "Payment"
                                                 && b.PrefixId == PrefixId
                                                 select new
                                                 {
                                                     b.PrefixUserMapId,
                                                     b.Prefix,
                                                     b.NoOfDigits,
                                                     b.Counter
                                                 }).FirstOrDefault();
                    PaymentPrefixUserMapId = paymentPrefixSettings.PrefixUserMapId;
                    string ReferenceNo = paymentPrefixSettings.Prefix + paymentPrefixSettings.Counter.ToString().PadLeft(paymentPrefixSettings.NoOfDigits, '0');

                    //long ChangeReturnAccountId = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.ChangeReturnAccountId).FirstOrDefault();

                    ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        Notes = "",
                        Amount = obj.ChangeReturn,
                        PaymentDate = obj.SalesDate.AddHours(5).AddMinutes(30),//CurrentDate,
                        PaymentTypeId = obj.PaymentTypeId,
                        SalesId = oClsSales.SalesId,
                        AttachDocument = "",
                        Type = "Change Return",
                        BranchId = obj.BranchId,
                        AccountId = obj.AccountId,
                        ReferenceNo = ReferenceNo,
                        IsDebit = 1,
                        //OnlinePaymentSettingsId = 0,
                        ReferenceId = oCommonController.CreateToken(),
                        IsDirectPayment = true,
                        JournalAccountId = AccountId,
                        PrefixId = PrefixId
                    };

                    oConnectionContext.DbClsCustomerPayment.Add(oClsPayment);
                    oConnectionContext.SaveChanges();
                    //increase counter
                    string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PaymentPrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(qq);
                    //increase counter
                }

                if (obj.Payment != null)
                {
                    if (obj.Payment.Amount != 0)
                    {
                        string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.Payment.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();

                        List<ClsCustomerPaymentVm> oAdvanceBalances = new List<ClsCustomerPaymentVm>();
                        if (_paymentType == "Advance")
                        {
                            //decimal AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault();
                            //if (AdvanceBalance < obj.Amount)
                            //{
                            //    errors.Add(new ClsError { Message = "Insuffcient Advance Balance", Id = "divAmount" });
                            //    isError = true;
                            //}
                            //else
                            //{
                            var availableAdvances = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerId == obj.CustomerId && a.IsDeleted == false && a.IsCancelled == false &&
                            a.IsActive == true && a.Type.ToLower() == "customer payment" && a.AmountRemaining > 0).Select(a => new
                            {
                                a.ParentId,
                                a.CustomerPaymentId,
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

                                    string _query1 = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"CustomerPaymentId\"=" + item.CustomerPaymentId;
                                    oConnectionContext.Database.ExecuteSqlCommand(_query1);

                                    amountRemaininToDeduct = amountRemaininToDeduct - amount;

                                    oAdvanceBalances.Add(new ClsCustomerPaymentVm { SalesId = obj.SalesId, CustomerPaymentId = item.CustomerPaymentId, Amount = amount, ParentId = item.ParentId });
                                }
                            }

                            string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Payment.Amount + " where \"UserId\"=" + obj.CustomerId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                            //}
                        }

                        long PaymentPrefixUserMapId = 0;
                        if (obj.Payment.ReferenceNo == "" || obj.Payment.ReferenceNo == null)
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
                            PaymentPrefixUserMapId = prefixSettings.PrefixUserMapId;
                            obj.Payment.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                        }

                        long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                        && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                        if (obj.Payment.AccountId == 0)
                        {
                            obj.Payment.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                        && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault();
                        }

                        if (obj.Payment.PaymentDate == DateTime.MinValue)
                        {
                            obj.Payment.PaymentDate = CurrentDate;
                        }

                        if (oAdvanceBalances != null && oAdvanceBalances.Count > 0)
                        {
                            obj.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                       && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                            foreach (var l in oAdvanceBalances)
                            {
                                ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = obj.Payment.Notes,
                                    Amount = l.Amount,
                                    PaymentDate = obj.Payment.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.Payment.PaymentTypeId,
                                    SalesId = oClsSales.SalesId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = obj.SalesType + " Payment",
                                    BranchId = obj.BranchId,
                                    AccountId = obj.Payment.AccountId,
                                    ReferenceNo = obj.Payment.ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ReferenceId = oCommonController.CreateToken(),
                                    JournalAccountId = JournalAccountId,
                                    CustomerId = obj.CustomerId,
                                    IsDirectPayment = true,
                                    ParentId = l.ParentId,
                                    PrefixId = PrefixId
                                };
                                oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                                oConnectionContext.SaveChanges();

                                //ClsCustomerPaymentDeductionId _oClsCustomerPaymentDeductionId = new ClsCustomerPaymentDeductionId()
                                //{
                                //    AddedBy = obj.AddedBy,
                                //    AddedOn = CurrentDate,
                                //    CompanyId = obj.CompanyId,
                                //    DeductedFromId = l.CustomerPaymentId,
                                //    Amount = l.Amount,
                                //    SalesId = l.SalesId,
                                //    CustomerPaymentId = oClsPayment1.CustomerPaymentId,
                                //    CustomerId = obj.CustomerId
                                //};
                                //oConnectionContext.DbClsCustomerPaymentDeductionId.Add(_oClsCustomerPaymentDeductionId);
                                //oConnectionContext.SaveChanges();
                            }
                        }
                        else
                        {
                            ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                IsActive = obj.IsActive,
                                IsDeleted = obj.IsDeleted,
                                Notes = obj.Payment.Notes,
                                Amount = obj.Payment.Amount,
                                PaymentDate = obj.Payment.PaymentDate.AddHours(5).AddMinutes(30),
                                PaymentTypeId = obj.Payment.PaymentTypeId,
                                SalesId = oClsSales.SalesId,
                                AttachDocument = obj.AttachDocument,
                                Type = obj.SalesType + " Payment",
                                BranchId = obj.BranchId,
                                AccountId = obj.Payment.AccountId,
                                ReferenceNo = obj.Payment.ReferenceNo,
                                IsDebit = 2,
                                //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                ReferenceId = oCommonController.CreateToken(),
                                JournalAccountId = JournalAccountId,
                                CustomerId = obj.CustomerId,
                                IsDirectPayment = true,
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

                                oClsPayment1.AttachDocument = filepathPass;
                            }

                            oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                            oConnectionContext.SaveChanges();
                        }

                        //increase counter
                        string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PaymentPrefixUserMapId;
                        oConnectionContext.Database.ExecuteSqlCommand(qq);
                        //increase counter

                        //ClsActivityLogVm oClsActivityLogVm1 = new ClsActivityLogVm()
                        //{
                        //    AddedBy = obj.AddedBy,
                        //    Browser = obj.Browser,
                        //    Category = PrefixType + " Payment",
                        //    CompanyId = obj.CompanyId,
                        //    Description = "added payment of " + obj.Payment.Amount + " for " + obj.InvoiceNo,
                        //    Id = oClsPayment.CustomerPaymentId,
                        //    IpAddress = obj.IpAddress,
                        //    Platform = obj.Platform,
                        //    Type = "Insert"
                        //};
                        //oCommonController.InsertActivityLog(oClsActivityLogVm1, CurrentDate);
                    }
                }

                if (obj.Payments != null)
                {
                    foreach (var item in obj.Payments)
                    {
                        if (item.Amount != 0)
                        {
                            string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == item.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();

                            List<ClsCustomerPaymentVm> oAdvanceBalances = new List<ClsCustomerPaymentVm>();
                            if (_paymentType == "Advance")
                            {
                                var availableAdvances = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerId == obj.CustomerId && a.IsDeleted == false && a.IsCancelled == false &&
                                a.IsActive == true && a.Type.ToLower() == "customer payment" && a.AmountRemaining > 0).Select(a => new
                                {
                                    a.ParentId,
                                    a.CustomerPaymentId,
                                    a.AmountRemaining
                                }).ToList();

                                decimal amountRemaininToDeduct = item.Amount;
                                foreach (var item1 in availableAdvances)
                                {
                                    if (amountRemaininToDeduct != 0)
                                    {
                                        decimal availableAmount = item1.AmountRemaining;
                                        decimal amount = 0;
                                        if (availableAmount >= amountRemaininToDeduct)
                                        {
                                            amount = amountRemaininToDeduct;
                                        }
                                        else if (availableAmount < amountRemaininToDeduct)
                                        {
                                            amount = availableAmount;
                                        }

                                        string _query1 = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"CustomerPaymentId\"=" + item1.CustomerPaymentId;
                                        oConnectionContext.Database.ExecuteSqlCommand(_query1);

                                        amountRemaininToDeduct = amountRemaininToDeduct - amount;

                                        oAdvanceBalances.Add(new ClsCustomerPaymentVm { SalesId = obj.SalesId, CustomerPaymentId = item1.CustomerPaymentId, Amount = amount, ParentId = item1.ParentId });
                                    }
                                }

                                string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + item.Amount + " where \"UserId\"=" + obj.CustomerId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                            }

                            long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                            && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                            if (item.AccountId == 0)
                            {
                                item.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                            && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();
                            }

                            if (item.PaymentDate == DateTime.MinValue)
                            {
                                item.PaymentDate = CurrentDate;
                            }

                            if (oAdvanceBalances != null && oAdvanceBalances.Count > 0)
                            {
                                item.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                           && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                                foreach (var l in oAdvanceBalances)
                                {
                                    ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        Notes = item.Notes,
                                        Amount = l.Amount,
                                        PaymentDate = item.PaymentDate.AddHours(5).AddMinutes(30),
                                        PaymentTypeId = item.PaymentTypeId,
                                        CustomerId = obj.CustomerId,
                                        SalesId = oClsSales.SalesId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = "Sales Payment",
                                        BranchId = obj.BranchId,
                                        AccountId = item.AccountId,
                                        //ReferenceNo = item.ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = l.CustomerPaymentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = JournalAccountId,
                                        PaymentLinkId = item.PaymentLinkId,
                                        PlaceOfSupplyId = obj.PlaceOfSupplyId,
                                        TaxId = 0,
                                        TaxAccountId = 0,
                                        AmountExcTax = l.Amount,
                                        TaxAmount = 0,
                                        IsBusinessRegistered = userDet.IsBusinessRegistered,
                                        GstTreatment = userDet.GstTreatment,
                                        BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                                        BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                                        BusinessLegalName = userDet.BusinessLegalName,
                                        BusinessTradeName = userDet.BusinessTradeName,
                                        PanNo = userDet.PanNo
                                    };
                                    oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                                    oConnectionContext.SaveChanges();
                                }
                            }
                            else
                            {
                                long PaymentPrefixUserMapId = 0;
                                if (item.ReferenceNo == "" || item.ReferenceNo == null)
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
                                    PaymentPrefixUserMapId = prefixSettings.PrefixUserMapId;
                                    item.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                                }

                                ClsCustomerPayment oClsPayment1 = new ClsCustomerPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = item.Notes,
                                    Amount = item.Amount,
                                    PaymentDate = item.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = item.PaymentTypeId,
                                    SalesId = oClsSales.SalesId,
                                    Type = obj.SalesType + " Payment",
                                    BranchId = obj.BranchId,
                                    AccountId = item.AccountId,
                                    ReferenceNo = item.ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ReferenceId = oCommonController.CreateToken(),
                                    JournalAccountId = JournalAccountId,
                                    CustomerId = obj.CustomerId,
                                    IsDirectPayment = true,
                                    TaxId = 0,
                                    TaxAccountId = 0,
                                    AmountExcTax = item.Amount,
                                    TaxAmount = 0,
                                    PrefixId = PrefixId,
                                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                                };

                                if (item.AttachDocument != "" && item.AttachDocument != null)
                                {
                                    string filepathPass = "";

                                    filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + item.FileExtensionAttachDocument;

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

                                    oClsPayment1.AttachDocument = filepathPass;
                                }

                                oConnectionContext.DbClsCustomerPayment.Add(oClsPayment1);
                                oConnectionContext.SaveChanges();

                                //increase counter
                                string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PaymentPrefixUserMapId;
                                oConnectionContext.Database.ExecuteSqlCommand(qq);
                                //increase counter
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.SalesType.ToLower() == "sales" ? "Sales" : "POS",
                    CompanyId = obj.CompanyId,
                    Description = (obj.SalesType.ToLower() == "sales" ? "Sales Invoice" : "POS") + " \"" + oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" updated",
                    Id = oClsSales.SalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (obj.SalesType == "Sales")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Pos")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Pos", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Debit Note")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Debit Note", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Bill Of Supply")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Bill Of Supply", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Sales updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            SalesId = oClsSales.SalesId,
                            InvoiceId = oConnectionContext.DbClsSales.Where(a => a.SalesId == oClsSales.SalesId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceBill }).FirstOrDefault(),
                        PosSetting = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceFinal }).FirstOrDefault(),
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDetailsDelete(ClsSalesDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.SalesId != 0)
                {
                    obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesId == obj.SalesId && a.IsDeleted == false).Select(a => new
                    {
                        a.SalesDetailsId,
                        a.SalesId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        //a.QuantityRemaining
                    }).ToList();

                    //foreach (var item in details)
                    //{
                    //    if (item.Quantity != item.QuantityRemaining)
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

                    string query = "update \"tblSalesDetails\" set \"IsDeleted\"=True where \"SalesId\"=" + obj.SalesId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in details)
                    {
                        //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                        //           Where(a => a.SalesDetailsId == item.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                        //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                        List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == item.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                        if (_StockDeductionIds != null)
                        {
                            foreach (var res in _StockDeductionIds)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(res.Quantity, item.ItemId, item.PriceAddedFor);
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
                            }
                            ;
                        }

                    }
                }
                else
                {
                    obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == oConnectionContext.DbClsSalesDetails.Where(b => b.SalesDetailsId == obj.SalesDetailsId).Select(b => b.SalesId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();
                    var details = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == obj.SalesDetailsId).Select(a => new
                    {
                        a.SalesDetailsId,
                        a.SalesId,
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                    }).FirstOrDefault();

                    string query = "update \"tblSalesDetails\" set \"IsDeleted\"=True where \"SalesDetailsId\"=" + obj.SalesDetailsId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                    //               Where(a => a.SalesDetailsId == details.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault();
                    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                    List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == details.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                    if (_StockDeductionIds != null)
                    {
                        foreach (var res in _StockDeductionIds)
                        {
                            //decimal convertedStock = oCommonController.StockConversion(res.Quantity, item.ItemId, item.PriceAddedFor);
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
                        }
                        ;
                    }


                    //query = "update tblItemBranchMap set Quantity=Quantity,0)+" + details.Quantity + " where BranchId=" + BranchId + " and ItemId=" + details.ItemId + " and ItemDetailsId=" + details.ItemDetailsId;
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

        public async Task<IHttpActionResult> HoldList(ClsSalesVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            if (obj.Search == "" || obj.Search == null)
            {
                var det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false &&
                a.Status.ToLower() == "draft").Select(a => new
                {
                    a.HoldReason,
                    a.TotalPaying,
                    a.Balance,
                    a.ChangeReturn,
                    a.CustomerId,
                    CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
                    CustomerMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.MobileNo).FirstOrDefault(),
                    a.Status,
                    //a.PayTerm,
                    //a.PayTermNo,
                    a.AttachDocument,
                    SalesId = a.SalesId,
                    a.GrandTotal,
                    a.TaxId,
                    a.TotalDiscount,
                    a.TotalQuantity,
                    a.Discount,
                    a.DiscountType,
                    a.Notes,
                    a.SalesDate,
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
                }).OrderByDescending(a => a.SalesId).Skip(skip).Take(obj.PageSize).ToList();
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Sales = det,
                        TotalCount = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() == "draft").Count(),
                        ActiveCount = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true && a.Status.ToLower() == "draft").Count(),
                        InactiveCount = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == false && a.Status.ToLower() == "draft").Count(),
                        PageSize = obj.PageSize
                    }
                };
            }
            else
            {

            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Invoice(ClsSalesVm obj)
        {
            var det = oConnectionContext.DbClsSales.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.InvoiceId == obj.InvoiceId).Select(a => new
            {
                a.PointsDiscount,
                a.PointsEarned,
                a.RedeemPoints,
                a.Terms,
                a.SalesType,
                a.IsWriteOff,
                a.WriteOffAmount,
                a.IsCancelled,
                a.OnlinePaymentSettingsId,
                a.SalesId,
                a.BranchId,
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
                a.SalesDate,
                a.Subtotal,
                a.Discount,
                a.DiscountType,
                a.TotalDiscount,
                a.GrandTotal,
                a.Status,
                a.TaxAmount,
                //Status = a.Status,
                a.RoundOff,
                SpecialDiscount = a.SpecialDiscount,
                a.NetAmount,
                a.DueDate,
                //Due = obj.SalesType == null ? (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? a.GrandTotal :
                //  a.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()) :
                //  (oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? a.GrandTotal :
                //  a.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.Tax).FirstOrDefault(),
                a.TotalQuantity,
                a.CompanyId,
                Payments = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesId == a.SalesId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
                {
                    b.PaymentDate,
                    ReferenceNo = b.ParentId == 0 ? b.ReferenceNo : oConnectionContext.DbClsCustomerPayment.Where(c => c.CustomerPaymentId == b.ParentId).Select(c => c.ReferenceNo).FirstOrDefault(),
                    b.Amount,
                    b.PaymentTypeId,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                }),
                SalesDetails = (from b in oConnectionContext.DbClsSalesDetails
                                join c in oConnectionContext.DbClsItemDetails
                                on b.ItemDetailsId equals c.ItemDetailsId
                                join d in oConnectionContext.DbClsItem
                                on c.ItemId equals d.ItemId
                                where b.SalesId == a.SalesId && b.IsDeleted == false
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
                                    b.SalesDetailsId,
                                    b.PriceIncTax,
                                    b.OtherInfo,
                                    b.AmountIncTax,
                                    b.Discount,
                                    b.SalesId,
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
                                    ComboItems = (from bb in oConnectionContext.DbClsSalesDetails
                                                  join cc in oConnectionContext.DbClsItemDetails
                                                  on bb.ItemDetailsId equals cc.ItemDetailsId
                                                  join dd in oConnectionContext.DbClsItem
                                                  on cc.ItemId equals dd.ItemId
                                                  where bb.SalesId == a.SalesId && bb.ComboId == b.ComboId && bb.IsDeleted == false
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
                SalesAdditionalCharges = oConnectionContext.DbClsSalesAdditionalCharges.Where(b => b.SalesId == a.SalesId
&& b.IsDeleted == false && b.IsActive == true).Select(b => new ClsSalesAdditionalChargesVm
{
    SalesAdditionalChargesId = b.SalesAdditionalChargesId,
    Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
    AdditionalChargeId = b.AdditionalChargeId,
    SalesId = b.SalesId,
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
                                })).Concat(oConnectionContext.DbClsSalesAdditionalCharges.Where(a => a.SalesId == det.SalesId
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
                a.CountryId,
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
            }).FirstOrDefault();

            var OnlinePaymentSetting = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == det.CompanyId &&
            a.IsDefault == true && a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.OnlinePaymentSettingsId,
                a.RazorpayKey,
                a.RazorpayCurrencyId,
                a.OnlinePaymentService,
                a.PaypalClientId,
                a.PaypalCurrencyId,
                PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.PaypalCurrencyId).Select(c => c.CurrencyCode).FirstOrDefault(),
                RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.RazorpayCurrencyId).Select(c => c.CurrencyCode).FirstOrDefault(),
            }).FirstOrDefault();

            // Get catalogue for this branch that is marked to show in invoices
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
                    OnlinePaymentSetting = OnlinePaymentSetting,
                    Catalogue = catalogue
                }
            };
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> ReturnInvoice(ClsSalesVm obj)
        //{
        //    var det = oConnectionContext.DbClsSalesReturn.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false &&
        //    a.SalesId == obj.SalesId).Select(a => new
        //    {
        //        a.Notes,
        //        BusinessLogo = oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CompanyId).Select(b => b.BusinessLogo).FirstOrDefault(),
        //        BusinessName = oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CompanyId).Select(b => b.BusinessName).FirstOrDefault(),
        //        TaxNo = oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CompanyId).Select(b => b.TaxNo).FirstOrDefault(),
        //        Address = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Address).FirstOrDefault(),
        //        Email = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Email).FirstOrDefault(),
        //        BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
        //        Mobile = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Mobile).FirstOrDefault(),
        //        CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
        //        CustomerAddress = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Address).FirstOrDefault(),
        //        CustomerMobile = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.MobileNo).FirstOrDefault(),
        //        CustomerEmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.EmailId).FirstOrDefault(),
        //        CustomerAltAddress = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.AltAddress).FirstOrDefault(),
        //        a.InvoiceNo,
        //        a.SalesDate,
        //        a.Subtotal,
        //        a.Discount,
        //        a.DiscountType,
        //        a.TotalDiscount,
        //        a.GrandTotal,
        //        a.OtherCharges,
        //        a.Status,
        //        Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.Tax).FirstOrDefault(),
        //        a.ShippingCharge,
        //        a.PackagingCharge,
        //        a.TotalQuantity,
        //        Payments = oConnectionContext.DbClsPayment.Where(b => b.Id == a.SalesId && b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
        //        {
        //            b.Amount,
        //            b.PaymentTypeId,
        //            PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
        //        }),
        //        SalesDetails = (from b in oConnectionContext.DbClsSalesDetails
        //                        join c in oConnectionContext.DbClsItemDetails
        //                        on b.ItemDetailsId equals c.ItemDetailsId
        //                        join d in oConnectionContext.DbClsItem
        //                        on c.ItemId equals d.ItemId
        //                        where b.SalesId == a.SalesId && b.IsDeleted == false && b.IsCancelled == false
        //                        select new
        //                        {
        //                            b.DiscountType,
        //                            b.SalesDetailsId,
        //                            b.PriceIncTax,
        //                            b.OtherInfo,
        //                            b.Amount,
        //                            b.Discount,
        //                            b.SalesId,
        //                            b.Quantity,
        //                            b.TaxId,
        //                            b.UnitCost,
        //                            d.ItemId,
        //                            d.ProductType,
        //                            c.ItemDetailsId,
        //                            d.ItemName,
        //                            c.SKU,
        //                            c.VariationDetailsId,
        //                            VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
        //                            UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
        //                            c.SalesExcTax,
        //                            c.SalesIncTax,
        //                            c.TotalCost,
        //                            Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
        //                            TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
        //                            d.TaxType,
        //                            d.ItemCode
        //                        }).ToList(),
        //    }).FirstOrDefault();
        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            Sale = det,
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> SalesReport(ClsSalesVm obj)
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

            var det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.Status != "Draft"
            && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Select(a => new
            {
                BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                SalesId = a.SalesId,
                a.GrandTotal,
                a.Notes,
                a.SalesDate,
                a.SalesType,
                a.InvoiceNo,
                a.Subtotal,
                a.CustomerId,
                Customer = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                Paid = obj.SalesType == null ? (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? 0 :
                    oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()) :
                    (oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType.ToLower() && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? 0 :
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType.ToLower() && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                Status = a.Status,
                Due = obj.SalesType == null ? (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? a.GrandTotal :
                    a.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()) :
                    (oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Count() == 0 ? a.GrandTotal :
                    a.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == obj.SalesType && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
                TotalQuantity = a.TotalQuantity//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
            }).ToList();


            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.SalesDate.Date >= obj.FromDate && a.SalesDate.Date <= obj.ToDate).ToList();
            }

            if (obj.Status != null && obj.Status != "")
            {
                det = det.Where(a => a.Status.ToLower() == obj.Status.ToLower()).Select(a => a).ToList();
            }

            if (obj.SalesType != null && obj.SalesType != "")
            {
                det = det.Where(a => a.SalesType.ToLower() == obj.SalesType.ToLower()).Select(a => a).ToList();
            }
            if (obj.InvoiceNo != null && obj.InvoiceNo != "")
            {
                det = det.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower()).Select(a => a).ToList();
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

        public async Task<IHttpActionResult> SalesDetailsReport(ClsSalesVm obj)
        {
            var det = (from b in oConnectionContext.DbClsSalesDetails
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.SalesId == obj.SalesId && b.IsDeleted == false
                       && b.IsComboItems == false
                       select new
                       {
                           b.QuantityRemaining,
                           b.SalesDetailsId,
                           b.AmountIncTax,
                           b.Discount,
                           b.Quantity,
                           b.PriceIncTax,
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
                    SalesDetails = det.OrderByDescending(a => a.SalesDetailsId).ToList(),
                    TotalCount = det.Count(),

                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesByUser(ClsSalesVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            obj.CashRegisterId = oConnectionContext.DbClsCashRegister.Where(a => a.AddedBy == obj.AddedBy && a.Status == 1).Select(a => a.CashRegisterId).FirstOrDefault();

            var det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && a.AddedBy == obj.AddedBy && a.SalesType.ToLower() == obj.SalesType.ToLower()
            && a.CashRegisterId == obj.CashRegisterId).Select(a => new
            {
                a.Status,
                a.SalesDate,
                a.InvoiceId,
                SalesId = a.SalesId,
                a.GrandTotal,
                a.InvoiceNo,
                CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
                IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
            }).OrderByDescending(a => a.SalesId).ToList();

            if (obj.Status != "" && obj.Status != null)
            {
                det = det.Where(a => a.Status == obj.Status).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesId).Skip(skip).Take(obj.PageSize).ToList(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesStatus(ClsSalesVm obj)
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

                string status = obj.Status;

                bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();
                obj = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId).Select(a => new ClsSalesVm
                {
                    Browser = obj.Browser,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Status = status,
                    TotalTaxAmount = a.TotalTaxAmount,
                    SalesType = a.SalesType,
                    InvoiceId = a.InvoiceId,
                    RoundOff = a.RoundOff,
                    SpecialDiscount = a.SpecialDiscount,
                    NetAmount = a.NetAmount,
                    ExchangeRate = a.ExchangeRate,
                    BranchId = a.BranchId,
                    PaymentType = a.PaymentType == null ? "" : a.PaymentType,
                    HoldReason = a.HoldReason,
                    TotalPaying = a.TotalPaying,
                    Balance = a.Balance,
                    ChangeReturn = a.ChangeReturn,
                    CustomerId = a.CustomerId,
                    SellingPriceGroupId = a.SellingPriceGroupId,
                    //Status=status,
                    //PayTerm = a.PayTerm,
                    //PayTermNo = a.PayTermNo,
                    AttachDocument = a.AttachDocument,
                    SalesId = a.SalesId,
                    GrandTotal = a.GrandTotal,
                    TaxId = a.TaxId,
                    TotalDiscount = a.TotalDiscount,
                    TotalQuantity = a.TotalQuantity,
                    Discount = a.Discount,
                    DiscountType = a.DiscountType,
                    Notes = a.Notes,
                    SalesDate = a.SalesDate,
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
                    SalesDetails = (from b in oConnectionContext.DbClsSalesDetails
                                    join c in oConnectionContext.DbClsItemBranchMap
                                    on b.ItemDetailsId equals c.ItemDetailsId
                                    join d in oConnectionContext.DbClsItemDetails
                                     on b.ItemDetailsId equals d.ItemDetailsId
                                    join e in oConnectionContext.DbClsItem
                                    on d.ItemId equals e.ItemId
                                    where b.SalesId == a.SalesId && b.IsDeleted == false && c.BranchId == a.BranchId
                                    && b.IsComboItems == false
                                    select new ClsSalesDetailsVm
                                    {
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
                                        SalesDetailsId = b.SalesDetailsId,
                                        OtherInfo = b.OtherInfo,
                                        Discount = b.Discount,
                                        SalesId = b.SalesId,
                                        Quantity = b.Quantity,
                                        TaxId = b.TaxId,
                                        UnitCost = b.UnitCost,
                                        ItemId = d.ItemId,
                                        ProductType = e.ProductType,
                                        ItemDetailsId = d.ItemDetailsId,
                                        ItemName = e.ItemName,
                                        SKU = d.SKU == null ? e.SkuCode : d.SKU,
                                        //d.VariationDetailsId,
                                        VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == d.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                        UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == e.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                        SalesExcTax = d.SalesExcTax,
                                        //SalesExcTax = b.LotType == "purchase" ?
                                        //oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                        //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                        //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                        //: d.SalesExcTax,
                                        SalesIncTax = b.LotTypeForLotNoChecking == "purchase" ?
                                        oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                        : b.LotTypeForLotNoChecking == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                        //: b.LotType == "stockadjustment" ? oConnectionContext.DbClsStockAdjustmentDetails.Where(f => f.StockAdjustmentDetailsId == b.LotId).Select(f => f.SalesIncTax).FirstOrDefault()
                                        : d.SalesIncTax,
                                        //d.TotalCost,
                                        Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                        TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                        TaxType = e.TaxType,
                                        //e.ItemCode,
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
                }).FirstOrDefault();

                // check credit limit

                decimal due = obj.GrandTotal;
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsSales
                                             join b in oConnectionContext.DbClsCustomerPayment
                                         on a.SalesId equals b.SalesId
                                             where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if (obj.Status != "Draft")
                        {
                            if ((TotalSalesDue + due) > creditLimit)
                            {
                                data = new
                                {
                                    Status = 4,
                                    //Message = "Only " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + (creditLimit - TotalSalesDue) + " credit is available out of " + oConnectionContext.DbClsCurrency.Where(b =>
                                    // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b =>
                                    //      b.CurrencySymbol).FirstOrDefault() + creditLimit,
                                    Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalSalesDue)),
                                    Data = new
                                    {
                                        User = new
                                        {
                                            CreditLimit = creditLimit,
                                            TotalSalesDue = TotalSalesDue,
                                            TotalSales = due,
                                            UserId = obj.CustomerId
                                        }
                                    }
                                };
                                return await Task.FromResult(Ok(data));
                            }
                        }
                    }
                }
                // check credit limit

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

                List<ClsSalesDetailsVm> _SalesDetails = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (Sales.SalesDetailsId != 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.SalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = item.SalesDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1, IsDeleted = Sales.IsDeleted });
                                }
                                _SalesDetails.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails.Add(Sales);
                            }
                        }
                        else
                        {
                            _SalesDetails.Add(Sales);
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails;

                //Release stock
                if (obj.SalesDetails != null)
                {
                    foreach (var StockAdjustment in obj.SalesDetails)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            if (StockAdjustment.SalesDetailsId != 0)
                            {
                                //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                                //    Where(a => a.SalesDetailsId == StockAdjustment.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault() ?? "[]";
                                //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == StockAdjustment.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                if (_StockDeductionIds != null)
                                {
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
                                    }
                                    ;
                                }


                            }
                        }
                        if (StockAdjustment.IsDeleted == true)
                        {
                            string query = "update \"tblSalesDetails\" set \"IsDeleted\"=True where \"SalesDetailsId\"=" + StockAdjustment.SalesDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                    }
                }
                //Release stock

                obj.SalesDetails.RemoveAll(r => r.IsComboItems == true);
                obj.SalesDetails.RemoveAll(r => r.IsDeleted == true);

                List<ClsSalesDetailsVm> _SalesDetails1 = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (Sales.SalesDetailsId == 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oCommonController.CreateToken();
                                var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == Sales.ItemId).Select(a => new
                                {
                                    ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ComboItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails1.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = Sales.SalesDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails1.Add(Sales);
                            }
                        }
                        else
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == Sales.ItemId && b.ComboItemDetailsId == a.ItemDetailsId).Select(b => b.Quantity).FirstOrDefault(),
                                    a.SalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails1.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = item.SalesDetailsId, IsActive = true, ComboPerUnitQuantity = item.Quantity, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails1.Add(Sales);
                            }
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails1;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                if (EnableLotNo == true)
                {
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            if (Sales.ProductType != "Combo")
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (Sales.IsComboItems == true)
                                    {
                                        //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                        decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                        //if (remainingQty < convertedStock)
                                        //{
                                        //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                        //    isError = true;
                                        //}
                                        decimal convertedStock = 0;
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                                    else
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
                                            if (Sales.SalesDetailsId != 0)
                                            {
                                                remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                            }
                                        }

                                        decimal convertedStock = 0;
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.LotType == inner.LotType && Sales.LotId == inner.LotId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                            }
                        }
                        if (isError == true)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = errors[0].Message,
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
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                //decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                //if (remainingQty < convertedStock)
                                //{
                                //    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                //    isError = true;
                                //}
                                decimal convertedStock = 0;
                                foreach (var inner in obj.SalesDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (Sales.ItemId == inner.ItemId && Sales.ItemDetailsId == inner.ItemDetailsId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                                Status = 0,
                                Message = errors[0].Message,
                                Errors = errors,
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                    }
                }

                ClsSales oClsSales = new ClsSales()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    SalesId = obj.SalesId,
                    Status = obj.Status,
                };

                oConnectionContext.DbClsSales.Attach(oClsSales);
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.SalesId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        var DefaultUnitCost = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.SalesExcTax).FirstOrDefault();

                        decimal QuantityReturned = 0;
                        decimal convertedStock = 0, freeConvertedStock = 0;

                        if (Sales.ProductType != "Combo")
                        {
                            convertedStock = oCommonController.StockConversion(Sales.Quantity, Sales.ItemId, Sales.PriceAddedFor);
                            freeConvertedStock = oCommonController.StockConversion(Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                if (obj.Status.ToLower() != "draft")
                                {
                                    if (Sales.LotId == 0)
                                    {
                                        Sales.StockDeductionIds = oCommonController.deductStock(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.ItemId, Sales.PriceAddedFor);
                                    }
                                    else
                                    {
                                        Sales.StockDeductionIds = oCommonController.deductStockLot(obj.BranchId, Sales.ItemDetailsId, (convertedStock + freeConvertedStock), Sales.LotId, Sales.LotType);
                                    }
                                }

                                QuantityReturned = oCommonController.StockConversion((from a in oConnectionContext.DbClsSalesReturn
                                                                                      join b in oConnectionContext.DbClsSalesReturnDetails
                                                                                         on a.SalesReturnId equals b.SalesReturnId
                                                                                      where a.SalesId == obj.SalesId && b.ItemId == Sales.ItemId &&
                                                                                      b.ItemDetailsId == Sales.ItemDetailsId
                                                                                      select b.Quantity).FirstOrDefault(), Sales.ItemId, Sales.PriceAddedFor);
                            }
                        }
                        else
                        {
                            QuantityReturned = (from a in oConnectionContext.DbClsSalesReturn
                                                join b in oConnectionContext.DbClsSalesReturnDetails
                                                   on a.SalesReturnId equals b.SalesReturnId
                                                where a.SalesId == obj.SalesId && b.ItemId == Sales.ItemId &&
                                                b.ItemDetailsId == Sales.ItemDetailsId
                                                select b.Quantity).FirstOrDefault();
                        }

                        if (Sales.LotType == "stocktransfer")
                        {
                            Sales.LotIdForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotId).FirstOrDefault();
                            Sales.LotTypeForLotNoChecking = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == Sales.LotId).Select(a => a.LotType).FirstOrDefault();
                        }
                        else
                        {
                            Sales.LotIdForLotNoChecking = Sales.LotId;
                            Sales.LotTypeForLotNoChecking = Sales.LotType;
                        }

                        ClsSalesDetails oClsSalesDetails = new ClsSalesDetails()
                        {
                            SalesDetailsId = Sales.SalesDetailsId,
                            DiscountType = Sales.DiscountType,
                            OtherInfo = Sales.OtherInfo,
                            PriceIncTax = Sales.PriceIncTax,
                            ItemId = Sales.ItemId,
                            ItemDetailsId = Sales.ItemDetailsId,
                            SalesId = oClsSales.SalesId,
                            TaxId = Sales.TaxId,
                            Discount = Sales.Discount,
                            Quantity = Sales.Quantity,
                            UnitCost = Sales.UnitCost,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            //StockDeductionIds = Sales.StockDeductionIds,
                            QuantityRemaining = Sales.ProductType == "Combo" ? (Sales.Quantity + Sales.FreeQuantity) - QuantityReturned : (convertedStock + freeConvertedStock) - QuantityReturned,
                            //QuantityRemaining = (convertedStock + freeConvertedStock) - QuantityReturned,
                            WarrantyId = Sales.WarrantyId,
                            DefaultUnitCost = DefaultUnitCost,
                            DefaultAmount = Sales.Quantity * DefaultUnitCost,
                            PriceAddedFor = Sales.PriceAddedFor,
                            LotId = Sales.LotId,
                            LotType = Sales.LotType,
                            FreeQuantity = Sales.FreeQuantity,
                            //FreeQuantityPriceAddedFor = Sales.FreeQuantityPriceAddedFor,
                            AmountExcTax = Sales.AmountExcTax,
                            TaxAmount = Sales.TaxAmount,
                            PriceExcTax = Sales.PriceExcTax,
                            AmountIncTax = Sales.AmountIncTax,
                            UnitAddedFor = Sales.UnitAddedFor,
                            LotIdForLotNoChecking = Sales.LotIdForLotNoChecking,
                            LotTypeForLotNoChecking = Sales.LotTypeForLotNoChecking,
                            ComboId = Sales.ComboId,
                            IsComboItems = Sales.IsComboItems,
                            QuantitySold = convertedStock + freeConvertedStock,
                            ComboPerUnitQuantity = Sales.ComboPerUnitQuantity,
                            ExtraDiscount = Sales.ExtraDiscount,
                            IsCombo = Sales.ProductType == "Combo" ? true : false,
                        };
                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsSalesDetails.Attach(oClsSalesDetails);
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DiscountType).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.OtherInfo).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PriceIncTax).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ItemId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ItemDetailsId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.SalesId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TaxId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.Discount).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.Quantity).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.UnitCost).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ModifiedOn).IsModified = true;
                        //oConnectionContext.Entry(oClsSalesDetails).Property(x => x.StockDeductionIds).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.QuantityRemaining).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.WarrantyId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DefaultUnitCost).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.DefaultAmount).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PriceAddedFor).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotType).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.FreeQuantity).IsModified = true;
                        //oConnectionContext.Entry(oClsSalesDetails).Property(x => x.FreeQuantityPriceAddedFor).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.AmountExcTax).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.TaxAmount).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.PriceExcTax).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.AmountIncTax).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.UnitAddedFor).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotIdForLotNoChecking).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.LotTypeForLotNoChecking).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ComboId).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.IsComboItems).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.QuantitySold).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ComboPerUnitQuantity).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.WarrantyExpiryDate).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.ExtraDiscount).IsModified = true;
                        oConnectionContext.Entry(oClsSalesDetails).Property(x => x.IsCombo).IsModified = true;
                        oConnectionContext.SaveChanges();

                        if (Sales.StockDeductionIds != null)
                        {
                            string ll = "delete from \"tblSalesDeductionId\" where \"SalesDetailsId\"=" + Sales.SalesDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(ll);

                            foreach (var l in Sales.StockDeductionIds)
                            {
                                ClsSalesDeductionId oClsSalesDeductionId = new ClsSalesDeductionId()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    Id = l.Id,
                                    Type = l.Type,
                                    Quantity = l.Quantity,
                                    SalesDetailsId = Sales.SalesDetailsId,
                                    SalesId = oClsSales.SalesId,
                                };
                                oConnectionContext.DbClsSalesDeductionId.Add(oClsSalesDeductionId);
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.SalesType,
                    CompanyId = obj.CompanyId,
                    Description = (obj.SalesType == "Sales" ? "Sales Invoive" : "POS") + " \"" + oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" status changed to \"Sent\"",
                    Id = oClsSales.SalesId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (obj.SalesType == "Sales")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Pos")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Pos", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Debit Note")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Debit Note", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else if (obj.SalesType == "Bill Of Supply")
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Bill Of Supply", obj.CompanyId, oClsSales.SalesId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Sales Invoice status changed successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Sale = new
                        {
                            SalesId = oClsSales.SalesId,
                            InvoiceId = oConnectionContext.DbClsSales.Where(a => a.SalesId == oClsSales.SalesId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        SaleSetting = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceBill }).FirstOrDefault(),
                        PosSetting = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.InvoiceType, a.AutoPrintInvoiceFinal }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ValidateStock(ClsSalesVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "Branch Name is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.CustomerId == 0)
                {
                    errors.Add(new ClsError { Message = "Customer Name is required", Id = "divCustomer" });
                    isError = true;
                }

                if (obj.SalesDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "Sales Date is required", Id = "divSalesDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "Status is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.SalesDetails == null)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
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

                decimal PayableAmount = 0;
                if (obj.Payments != null)
                {
                    foreach (var item in obj.Payments)
                    {
                        string _paymentType = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == item.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                        if (item.Amount == 0)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "Add amount first",
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                        else
                        {
                            PayableAmount = PayableAmount + item.Amount;
                            if (_paymentType.ToLower() == "advance")
                            {
                                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + item.Amount + " where \"UserId\"=" + obj.CustomerId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.AdvanceBalance).FirstOrDefault() < item.Amount)
                                {
                                    data = new
                                    {
                                        Status = 0,
                                        Message = "Not enough advance balance",
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
                //}

                decimal due = 0;
                if (obj.Payments != null)
                {
                    if (PayableAmount == 0)
                    {
                        obj.Status = "Due";
                        PayableAmount = obj.GrandTotal;
                    }
                    else
                    {
                        if (obj.GrandTotal == PayableAmount)
                        {
                            obj.Status = "Paid";
                        }
                        else if (obj.GrandTotal > PayableAmount)
                        {
                            obj.Status = "Partially Paid";
                            due = obj.GrandTotal - PayableAmount;
                        }
                    }
                }

                // check credit limit
                //if (obj.Status.ToLower() == "credit" || obj.Payment == null)
                if (due > 0)
                {
                    var creditLimit = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.CreditLimit).FirstOrDefault();
                    if (creditLimit != 0)
                    {
                        decimal TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId &&
                                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId && a.SalesId != obj.SalesId
                                            ).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum() -
                                            (from a in oConnectionContext.DbClsSales
                                             join b in oConnectionContext.DbClsCustomerPayment
                                         on a.SalesId equals b.SalesId
                                             where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                         && a.BranchId == obj.BranchId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false &&
                                         b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.CustomerId == obj.CustomerId
                                         && a.SalesId != obj.SalesId
                                             select b.Amount).DefaultIfEmpty().Sum();

                        if ((TotalSalesDue + due) > creditLimit)
                        {
                            data = new
                            {
                                Status = 4,
                                //Message = "Maximum credit limit exceeded " + oConnectionContext.DbClsCurrency.Where(b =>
                                // b.CurrencyId == oConnectionContext.DbClsUser.Where(z => z.UserId == obj.CustomerId).Select(z => z.CurrencyId).FirstOrDefault()).Select(b =>
                                //      b.CurrencySymbol).FirstOrDefault() + creditLimit,
                                Message = "Credit Limit exceeded by " + (due - (creditLimit - TotalSalesDue)),
                                Data = new
                                {
                                    User = new
                                    {
                                        CreditLimit = creditLimit,
                                        TotalSalesDue = TotalSalesDue,
                                        TotalSales = due,
                                        UserId = obj.CustomerId
                                    }
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                    }
                }
                // check credit limit

                List<ClsSalesDetailsVm> _SalesDetails = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (Sales.SalesDetailsId != 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.SalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = item.SalesDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails.Add(Sales);
                            }
                        }
                        else
                        {
                            _SalesDetails.Add(Sales);
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails;

                //Release stock
                if (obj.SalesDetails != null)
                {
                    foreach (var StockAdjustment in obj.SalesDetails)
                    {
                        bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == StockAdjustment.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                        if (IsManageStock == true)
                        {
                            if (StockAdjustment.SalesDetailsId != 0)
                            {
                                //string StockDeductionIds = oConnectionContext.DbClsSalesDetails.
                                //    Where(a => a.SalesDetailsId == StockAdjustment.SalesDetailsId).Select(a => a.StockDeductionIds).FirstOrDefault() ?? "[]";
                                //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(StockDeductionIds);

                                List<ClsStockDeductionIds> _StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(a => a.SalesDetailsId
                        == StockAdjustment.SalesDetailsId).Select(a => new ClsStockDeductionIds { Id = a.Id, Quantity = a.Quantity, Type = a.Type }).ToList();

                                if (_StockDeductionIds != null)
                                {
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
                                    }
                                    ;
                                }
                            }
                        }
                    }
                }
                //Release stock

                obj.SalesDetails.RemoveAll(r => r.IsComboItems == true);

                List<ClsSalesDetailsVm> _SalesDetails1 = new List<ClsSalesDetailsVm>();
                if (obj.SalesDetails != null)
                {
                    foreach (var Sales in obj.SalesDetails)
                    {
                        if (Sales.SalesDetailsId == 0)
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oCommonController.CreateToken();
                                var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == Sales.ItemId).Select(a => new
                                {
                                    ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ComboItemDetailsId,
                                    Quantity = a.Quantity,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails1.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = Sales.SalesDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails1.Add(Sales);
                            }
                        }
                        else
                        {
                            Sales.ProductType = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.ProductType).FirstOrDefault();
                            if (Sales.ProductType.ToLower() == "combo")
                            {
                                Sales.ComboId = oConnectionContext.DbClsSalesDetails.Where(a => a.SalesDetailsId == Sales.SalesDetailsId).Select(a => a.ComboId).FirstOrDefault();
                                var combo = oConnectionContext.DbClsSalesDetails.Where(a => a.ComboId == Sales.ComboId && a.IsComboItems == true).Select(a => new
                                {
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ComboItemDetailsId = a.ItemDetailsId,
                                    Quantity = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == Sales.ItemId && b.ComboItemDetailsId == a.ItemDetailsId).Select(b => b.Quantity).FirstOrDefault(),
                                    a.SalesDetailsId,
                                    a.PriceAddedFor
                                }).ToList();

                                foreach (var item in combo)
                                {
                                    _SalesDetails1.Add(new ClsSalesDetailsVm { ItemId = item.ItemId, ItemDetailsId = item.ComboItemDetailsId, Quantity = item.Quantity * Sales.Quantity, Under = Sales.ItemDetailsId, IsComboItems = true, ComboId = Sales.ComboId, DivId = Sales.DivId, SalesDetailsId = item.SalesDetailsId, IsActive = true, PriceAddedFor = item.PriceAddedFor, UnitAddedFor = 1 });
                                }
                                _SalesDetails1.Add(Sales);
                            }
                            else
                            {
                                _SalesDetails1.Add(Sales);
                            }
                        }
                    }
                }

                obj.SalesDetails = _SalesDetails1;

                var EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();

                if (EnableLotNo == true)
                {
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            if (Sales.ProductType != "Combo")
                            {
                                bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                if (IsManageStock == true)
                                {
                                    if (Sales.IsComboItems == true)
                                    {
                                        decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                        decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                        if (remainingQty < convertedStock)
                                        {
                                            errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantity" + Sales.DivId });
                                            isError = true;
                                        }
                                    }
                                    else
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
                                        foreach (var inner in obj.SalesDetails)
                                        {
                                            bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                            if (IsManageStock_Inner == true)
                                            {
                                                if (Sales.LotType == inner.LotType && Sales.LotId == inner.LotId)
                                                {
                                                    convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
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
                    if (obj.SalesDetails != null)
                    {
                        foreach (var Sales in obj.SalesDetails)
                        {
                            bool IsManageStock = oConnectionContext.DbClsItem.Where(a => a.ItemId == Sales.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                            if (IsManageStock == true)
                            {
                                decimal convertedStock = oCommonController.StockConversion(Sales.Quantity + Sales.FreeQuantity, Sales.ItemId, Sales.PriceAddedFor);
                                decimal remainingQty = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId && a.ItemId == Sales.ItemId && a.ItemDetailsId == Sales.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();
                                if (remainingQty < convertedStock)
                                {
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

                data = new
                {
                    Status = 1,
                    Message = "Stock available",
                    Data = new
                    {

                    }
                };

                //dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesJournal(ClsSalesVm obj)
        {
            var taxList = (from q in oConnectionContext.DbClsSalesTaxJournal
                           join a in oConnectionContext.DbClsSalesDetails
                           on q.SalesDetailsId equals a.SalesDetailsId
                           join b in oConnectionContext.DbClsSales
                        on a.SalesId equals b.SalesId
                           join c in oConnectionContext.DbClsTax
                           on q.TaxId equals c.TaxId
                           where q.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //&& a.TaxAmount != 0
                           && c.TaxTypeId != 0
                           select new
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = 0,
                               Credit = q.TaxAmount,
                               AccountId = q.AccountId
                           }).Concat(from q in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                     join a in oConnectionContext.DbClsSalesAdditionalCharges
                                     on q.SalesAdditionalChargesId equals a.SalesAdditionalChargesId
                                     join b in oConnectionContext.DbClsSales
                                  on a.SalesId equals b.SalesId
                                     join c in oConnectionContext.DbClsTax
                                     on q.TaxId equals c.TaxId
                                     where q.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                     //&& a.TaxAmount != 0
                                     && c.TaxTypeId != 0
                                     && a.AmountExcTax > 0
                                     select new
                                     {
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = 0,
                                         Credit = q.TaxAmount,
                                         AccountId = q.AccountId
                                     }).ToList();

            var journal = (from a in oConnectionContext.DbClsSales
                               //   join b in oConnectionContext.DbClsSalesDetails
                               //on a.SalesId equals b.SalesId
                           where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               //Account Receivable
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = a.GrandTotal,
                               Credit = 0
                           }).Concat(from a in oConnectionContext.DbClsSalesDetails
                                     join b in oConnectionContext.DbClsSales
                                  on a.SalesId equals b.SalesId
                                     where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                    && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsComboItems == false
                                     group a by a.AccountId into stdGroup
                                     select new ClsBankPaymentVm
                                     {
                                         // sales account
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = 0,
                                         Credit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                     }).Concat(from a in oConnectionContext.DbClsSales
                                               where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                               select new ClsBankPaymentVm
                                               {
                                                   // Round off charge
                                                   AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.RoundOffAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                   Debit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                                   Credit = a.RoundOff > 0 ? a.RoundOff : 0,
                                               }).Concat(from a in oConnectionContext.DbClsSales
                                                         where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                         select new ClsBankPaymentVm
                                                         {
                                                             // special discount 
                                                             AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.SpecialDiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                             Debit = a.SpecialDiscount,
                                                             Credit = 0,
                                                         }).Concat(from a in oConnectionContext.DbClsSales
                                                                   where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                   select new ClsBankPaymentVm
                                                                   {
                                                                       // discount 
                                                                       AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.DiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                       Debit = a.TotalDiscount,
                                                                       Credit = 0,
                                                                   }).Concat(from a in oConnectionContext.DbClsSales
                                                                             where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                             && a.IsWriteOff == true
                                                                             select new ClsBankPaymentVm
                                                                             {
                                                                                 // Write Off account 
                                                                                 AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.WriteOffAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                                 Debit = a.GrandTotal,
                                                                                 Credit = 0,
                                                                             }).Concat(from a in oConnectionContext.DbClsSales
                                                                                       where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                                       && a.IsWriteOff == true
                                                                                       select new ClsBankPaymentVm
                                                                                       {
                                                                                           // Write Off journal account 
                                                                                           AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.WriteOffJournalAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                                           Debit = 0,
                                                                                           Credit = a.GrandTotal,
                                                                                       }).Concat(from a in oConnectionContext.DbClsSalesAdditionalCharges
                                                                                                 where a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId
                                                                                                 && a.IsDeleted == false && a.IsActive == true
                                                                                                 select new ClsBankPaymentVm
                                                                                                 {
                                                                                                     // Write Off journal account 
                                                                                                     AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                                                     Debit = 0,
                                                                                                     Credit = a.AmountExcTax,
                                                                                                 }).ToList();



            var sale = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId
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
                                             IsTaxAccount = true,
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

        //public async Task<IHttpActionResult> SalesDue(ClsSalesVm obj)
        //{
        //    decimal AdvanceBalance = 0, Due = 0;

        //        long UserId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.CustomerId).FirstOrDefault();
        //        AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == UserId).Select(a => a.AdvanceBalance).FirstOrDefault();
        //        Due = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
        //               (from a in oConnectionContext.DbClsSales
        //                join b in oConnectionContext.DbClsCustomerPayment
        //    on a.SalesId equals b.SalesId
        //                where a.SalesId == obj.SalesId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
        //                select b.Amount).DefaultIfEmpty().Sum();

        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            User = new
        //            {
        //                AdvanceBalance = AdvanceBalance,
        //                Due = Due,
        //            }
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> WarrantyExpiryReport(ClsSalesDetailsVm obj)
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

            List<ClsSalesDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from b in oConnectionContext.DbClsSalesDetails
                       join a in oConnectionContext.DbClsSales
                       on b.SalesId equals a.SalesId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false && a.Status != "Draft" &&
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && a.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                && a.SalesId != (oConnectionContext.DbClsSalesReturn.Where(p => p.IsDeleted == false && p.IsCancelled == false).Select(p => p.SalesId).FirstOrDefault())
                && b.WarrantyId != 0
                       select new ClsSalesDetailsVm
                       {
                           BranchId = d.BrandId,
                           ItemName = d.ItemName,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.EmailId).FirstOrDefault(),
                           InvoiceNo = a.InvoiceNo,
                           SalesDate = a.SalesDate,
                           CustomerId = a.CustomerId,
                           ItemId = d.ItemId,
                           SalesId = a.SalesId,
                           Duration = oConnectionContext.DbClsWarranty.Where(r => r.WarrantyId == b.WarrantyId).Select(r => r.Duration).FirstOrDefault(),
                           DurationNo = oConnectionContext.DbClsWarranty.Where(r => r.WarrantyId == b.WarrantyId).Select(r => r.DurationNo).FirstOrDefault(),
                           WarrantyExpiryDate = b.WarrantyExpiryDate.Value,
                           //WarrantyExpiryTimeLeft = Convert.ToInt32((b.WarrantyExpiryDate.Value - DateTime.Now).TotalDays)
                       }).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsSalesDetails
                       join a in oConnectionContext.DbClsSales
                       on b.SalesId equals a.SalesId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false && a.Status != "Draft" &&
                       a.BranchId == obj.BranchId
                       && a.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                && a.SalesId != (oConnectionContext.DbClsSalesReturn.Where(p => p.IsDeleted == false && p.IsCancelled == false).Select(p => p.SalesId).FirstOrDefault())
                && b.WarrantyId != 0
                       select new ClsSalesDetailsVm
                       {
                           BranchId = d.BrandId,
                           ItemName = d.ItemName,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.EmailId).FirstOrDefault(),
                           InvoiceNo = a.InvoiceNo,
                           SalesDate = a.SalesDate,
                           CustomerId = a.CustomerId,
                           ItemId = d.ItemId,
                           SalesId = a.SalesId,
                           Duration = oConnectionContext.DbClsWarranty.Where(r => r.WarrantyId == b.WarrantyId).Select(r => r.Duration).FirstOrDefault(),
                           DurationNo = oConnectionContext.DbClsWarranty.Where(r => r.WarrantyId == b.WarrantyId).Select(r => r.DurationNo).FirstOrDefault(),
                           WarrantyExpiryDate = b.WarrantyExpiryDate.Value,
                           //WarrantyExpiryTimeLeft = Convert.ToInt32((b.WarrantyExpiryDate.Value - DateTime.Now).TotalDays)
                       }).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.InvoiceNo != null && obj.InvoiceNo != "")
            {
                det = det.Where(a => a.InvoiceNo == obj.InvoiceNo).Select(a => a).ToList();
            }

            //if (obj.ItemDetailsId != 0)
            //{
            //    det = det.Where(a => a.ItemDetailsId == obj.ItemDetailsId).Select(a => a).ToList();
            //}

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDetails = det,
                    TotalCount = det.Count(),
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesInvoicesByReference(ClsPurchaseVm obj)
        {
            List<ClsSalesVm> det = new List<ClsSalesVm>();
            List<ClsSalesVm> det1 = new List<ClsSalesVm>();
            List<ClsSalesVm> det2 = new List<ClsSalesVm>();
            List<ClsSalesVm> det3 = new List<ClsSalesVm>();

            if (obj.ReferenceType == "sales quotation")
            {
                det1 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == obj.ReferenceId && a.ReferenceType == obj.ReferenceType).Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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

                long SalesOrderRefId = oConnectionContext.DbClsSalesOrder.Where(b =>
                           b.ReferenceId == obj.ReferenceId && b.ReferenceType == obj.ReferenceType).Select(b => b.SalesOrderId).FirstOrDefault();

                det2 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == SalesOrderRefId && a.ReferenceType == "sales order").Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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

                long SalesProformaRefId = oConnectionContext.DbClsSalesProforma.Where(b =>
                           b.ReferenceId == obj.ReferenceId && b.ReferenceType == obj.ReferenceType).Select(b => b.SalesProformaId).FirstOrDefault();

                det3 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == SalesProformaRefId && a.ReferenceType == "sales proforma").Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
               //               Reference = a.ReferenceType == "purchase quotation" ? oConnectionContext.DbClsPurchaseQuotation.Where(c => c.PurchaseQuotationId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
               //                    a.ReferenceType == "purchase order" ? oConnectionContext.DbClsPurchaseOrder.Where(c => c.PurchaseOrderId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
               //                    a.ReferenceType == "purchase" ? oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.ReferenceId).Select(c => c.ReferenceNo).FirstOrDefault() :
               //                    a.ReferenceType == "sales quotation" ? oConnectionContext.DbClsSalesQuotation.Where(c => c.SalesQuotationId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
               //                    a.ReferenceType == "sales order" ? oConnectionContext.DbClsSalesOrder.Where(c => c.SalesOrderId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
               //                    a.ReferenceType == "sales proforma" ? oConnectionContext.DbClsSalesProforma.Where(c => c.SalesProformaId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
               //                    a.ReferenceType == "sales" ? oConnectionContext.DbClsSales.Where(c => c.SalesId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault() :
               //                    oConnectionContext.DbClsDeliveryChallan.Where(c => c.DeliveryChallanId == a.ReferenceId).Select(c => c.InvoiceNo).FirstOrDefault(),
               ReferenceType = a.ReferenceType
           }).ToList();

                det = det1.Concat(det2).Concat(det3).ToList();
            }
            else if (obj.ReferenceType == "sales order")
            {
                det1 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == obj.ReferenceId && a.ReferenceType == obj.ReferenceType).Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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

                long SalesProformaRefId = oConnectionContext.DbClsSalesProforma.Where(b =>
                           b.ReferenceId == obj.ReferenceId && b.ReferenceType == obj.ReferenceType).Select(b => b.SalesProformaId).FirstOrDefault();

                det2 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == SalesProformaRefId && a.ReferenceType == "sales proforma").Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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

                long DeliveryChallanRefId = oConnectionContext.DbClsDeliveryChallan.Where(b =>
                           b.ReferenceId == obj.ReferenceId && b.ReferenceType == obj.ReferenceType).Select(b => b.DeliveryChallanId).FirstOrDefault();

                det3 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == DeliveryChallanRefId && a.ReferenceType == "delivery challan").Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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

                det = det1.Concat(det2).Concat(det3).ToList();
            }
            else if (obj.ReferenceType == "sales proforma")
            {
                det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == obj.ReferenceId && a.ReferenceType == obj.ReferenceType).Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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
            else if (obj.ReferenceType == "delivery challan")
            {
                det = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
           && a.ReferenceId == obj.ReferenceId && a.ReferenceType == obj.ReferenceType).Select(a => new ClsSalesVm
           {
               TotalTaxAmount = a.TotalTaxAmount,
               TotalDiscount = a.TotalDiscount,
               InvoiceId = a.InvoiceId,
               BranchId = a.BranchId,
               Status = a.Status,
               InvoiceUrl = oCommonController.webUrl,//+ "/sales/invoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
               BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
               SalesId = a.SalesId,
               GrandTotal = a.GrandTotal,
               Notes = a.Notes,
               SalesDate = a.SalesDate,
               SalesType = a.SalesType,
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
               Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
               //Status = a.Status,
               Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
               IsSalesReturn = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsCancelled == false && c.IsCancelled == false).Count() == 0 ? false : true,
               TotalQuantity = a.TotalQuantity,//oConnectionContext.DbClsSalesDetails.Where(c=>c.SalesId==a.SalesId && c.IsDeleted==false).Count()
               PaidQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
               FreeQuantity = oConnectionContext.DbClsSalesDetails.Where(c => c.SalesId == a.SalesId && c.IsDeleted == false && c.IsComboItems == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
               //               ReturnDue = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesId == a.SalesId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false).Select(c => c.GrandTotal).DefaultIfEmpty().FirstOrDefault() -
               //(from c in oConnectionContext.DbClsSalesReturn
               // join e in oConnectionContext.DbClsCustomerRefund on c.SalesReturnId equals e.SalesReturnId
               // where c.SalesId == a.SalesId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false
               // select e.Amount).DefaultIfEmpty().Sum(),
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
                    Sales = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesInvoices(ClsSalesVm obj)
        {
            List<ClsSalesVm> det;
            if (obj.SalesType == "Debit Note")
            {
                det = (from b in oConnectionContext.DbClsSales
                       where b.IsDeleted == false && b.IsCancelled == false
                       && b.BranchId == obj.BranchId
                       && b.CustomerId == obj.CustomerId
                       && b.SalesType != "Debit Note"
                       && b.Status.ToLower() != "draft"
                       select new ClsSalesVm
                       {
                           SalesId = b.SalesId,
                           InvoiceNo = b.InvoiceNo
                       }).Distinct().OrderByDescending(a => a.SalesId).ToList();
            }
            else
            {
                if (obj.SalesType == "Credit Note Add")
                {
                    det = (from b in oConnectionContext.DbClsSales
                           join c in oConnectionContext.DbClsSalesDetails
                           on b.SalesId equals c.SalesId
                           where b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                           //&& c.QuantityRemaining > 0
                           && b.BranchId == obj.BranchId
                           && b.CustomerId == obj.CustomerId
                           && b.Status.ToLower() != "draft"
                           select new ClsSalesVm
                           {
                               SalesId = b.SalesId,
                               InvoiceNo = b.InvoiceNo
                           }).Distinct().OrderByDescending(a => a.SalesId).ToList();
                }
                else
                {
                    det = (from b in oConnectionContext.DbClsSales
                           join c in oConnectionContext.DbClsSalesDetails
                           on b.SalesId equals c.SalesId
                           where b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                           && b.BranchId == obj.BranchId
                           && b.CustomerId == obj.CustomerId
                           && b.Status.ToLower() != "draft"
                           select new ClsSalesVm
                           {
                               SalesId = b.SalesId,
                               InvoiceNo = b.InvoiceNo
                           }).Distinct().OrderByDescending(a => a.SalesId).ToList();
                }
            }

            var user = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => new
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
                    Sales = det,
                    User = user
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDetails(ClsSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();

            var det = (from bb in oConnectionContext.DbClsSales
                       where bb.SalesId == obj.SalesId && bb.CompanyId == obj.CompanyId && bb.IsActive == true && bb.IsDeleted == false && bb.IsCancelled == false
                       select new
                       {
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
                           bb.Discount,
                           bb.DiscountType,
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
                                               b.ExtraDiscount,
                                               b.PriceExcTax,
                                               AmountExcTax = 0,
                                               d.IsManageStock,
                                               FreeQuantity = 0,
                                               PurchaseReturnUnitCost = b.UnitCost,
                                               b.QuantityRemaining,
                                               SalesReturnUnitCost = b.PriceIncTax,
                                               SalesReturnAmount = 0,
                                               b.DiscountType,
                                               b.SalesDetailsId,
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
                       }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesReturn = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateWriteOff(ClsSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.WriteOffAmount = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).FirstOrDefault() -
                    (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                obj.WriteOffAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Bad Debt").Select(a => a.AccountId).FirstOrDefault();

                obj.WriteOffJournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                ClsSales oClsSales = new ClsSales()
                {
                    SalesId = obj.SalesId,
                    IsWriteOff = true,
                    WriteOffDate = obj.WriteOffDate,
                    WriteOffReason = obj.WriteOffReason,
                    WriteOffAccountId = obj.WriteOffAccountId,
                    WriteOffAmount = obj.WriteOffAmount,
                    WriteOffJournalAccountId = obj.WriteOffJournalAccountId,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Status = "Paid"
                };

                oConnectionContext.DbClsSales.Attach(oClsSales);
                oConnectionContext.Entry(oClsSales).Property(x => x.IsWriteOff).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffDate).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffReason).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffAmount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffJournalAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Write Off added successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CancelWriteOff(ClsSalesVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int count = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment")
                && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).Count();
                if (count == 0)
                {
                    obj.Status = "Due";
                }
                else
                {
                    obj.Status = "Partially Paid";
                }

                //query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                //oConnectionContext.Database.ExecuteSqlCommand(query);

                #region check OverDue Payment
                var sale = (from a in oConnectionContext.DbClsSales
                            where a.SalesId == obj.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                            && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                            //&& b.PayTermNo != 0
                            select new
                            {
                                a.SalesId,
                                a.SalesDate,
                                a.CustomerId,
                                a.DueDate
                                //b.PayTerm,
                                //b.PayTermNo
                            }).FirstOrDefault();

                if (sale != null)
                {
                    DateTime expDate = DateTime.Now;

                    //if (sale.DueDate < DateTime.Now)
                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                    {
                        obj.Status = "Overdue";
                    }
                }
                #endregion


                ClsSales oClsSales = new ClsSales()
                {
                    SalesId = obj.SalesId,
                    IsWriteOff = false,
                    WriteOffDate = null,
                    WriteOffReason = "",
                    WriteOffAccountId = 0,
                    WriteOffJournalAccountId = 0,
                    WriteOffAmount = 0,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Status = obj.Status
                };

                oConnectionContext.DbClsSales.Attach(oClsSales);
                oConnectionContext.Entry(oClsSales).Property(x => x.IsWriteOff).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffDate).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffReason).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffJournalAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.WriteOffAmount).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSales).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Write Off cancelled successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UnpaidSalesInvoices(ClsSales obj)
        {
            //var det = (from b in oConnectionContext.DbClsSales
            //           where b.Status != "Draft" && b.IsDeleted == false && b.IsCancelled == false
            //           && b.CustomerId == obj.CustomerId
            //           select new
            //           {
            //               b.SalesType,
            //               b.SalesId,
            //               b.InvoiceNo,
            //               b.SalesDate,
            //               b.GrandTotal,
            //               Due = b.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(bb => (bb.Type.ToLower() == "sales" || bb.Type.ToLower() == "pos")
            //               && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.SalesId == b.SalesId).Select(bb => bb.Amount).DefaultIfEmpty().Sum()
            //           }).Distinct().OrderByDescending(a => a.SalesId).ToList();

            var Dues = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                 && a.CustomerId == obj.CustomerId && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                 ).OrderBy(a => a.SalesId).Select(a => new ClsSalesVm
                 {
                     SalesDate = a.SalesDate,
                     InvoiceNo = a.InvoiceNo,
                     GrandTotal = a.GrandTotal,
                     Type = "Sales Payment",
                     BranchId = a.BranchId,
                     SalesId = a.SalesId,
                     CustomerId = a.CustomerId,
                     //Due = a.GrandTotal - oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                     Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                 }).ToList();

            if ((oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.OpeningBalance).FirstOrDefault() -
            oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == "customer opening balance payment" &&
            b.IsDeleted == false && b.IsCancelled == false && b.CustomerId == obj.CustomerId).Select(b => b.Amount).DefaultIfEmpty().Sum()) > 0)
            {
                Dues = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => new ClsSalesVm
                {
                    SalesDate = a.AddedOn,
                    InvoiceNo = "",
                    GrandTotal = a.OpeningBalance,
                    Type = "Customer Opening Balance Payment",
                    BranchId = 0,
                    CustomerId = a.UserId,
                    SalesId = 0,
                    Due = a.OpeningBalance - oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == "customer opening balance payment" && b.IsDeleted == false && b.IsCancelled == false && b.CustomerId == obj.CustomerId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                }).ToList().Union(Dues).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = Dues,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        #region Reward Points Processing Methods

        /// <summary>
        /// Process reward points redeemed on a sale
        /// </summary>
        private bool ProcessRewardPointsRedeemed(long salesId, long customerId, long companyId, decimal redeemPoints, decimal orderAmount, DateTime currentDate, long addedBy)
        {
            var settings = oConnectionContext.DbClsRewardPointSettings
                .Where(a => a.CompanyId == companyId && a.EnableRewardPoint && !a.IsDeleted)
                .FirstOrDefault();

            if (settings == null || redeemPoints <= 0) return true;

            // Calculate discount amount
            decimal discountAmount = redeemPoints * settings.RedeemAmountPerUnitPoint;

            // Get customer record (reward points are now stored in tblUser)
            var customer = oConnectionContext.DbClsUser
                .Where(a => a.UserId == customerId && a.CompanyId == companyId && !a.IsDeleted)
                .FirstOrDefault();

            if (customer == null) return false;

            // Validate available points before redeeming
            if (customer.AvailableRewardPoints < redeemPoints)
            {
                return false; // Insufficient points
            }

            // Update customer reward points balance
            customer.AvailableRewardPoints -= redeemPoints;
            customer.TotalRewardPointsRedeemed += redeemPoints;
            customer.LastRewardPointsRedeemedDate = currentDate;
            customer.ModifiedBy = addedBy;
            customer.ModifiedOn = currentDate;

            // Create transaction record
            var transaction = new ClsRewardPointTransaction
            {
                CustomerId = customerId,
                SalesId = salesId,
                TransactionType = "Redeem",
                Points = redeemPoints,
                OrderAmount = orderAmount,
                TransactionDate = currentDate,
                CompanyId = companyId,
                Notes = $"Points redeemed for invoice",
                IsActive = true,
                IsDeleted = false,
                AddedBy = addedBy,
                AddedOn = currentDate
            };
            oConnectionContext.DbClsRewardPointTransaction.Add(transaction);
            oConnectionContext.SaveChanges();

            return true;
        }

        /// <summary>
        /// Process reward points earned on a sale
        /// </summary>
        private decimal ProcessRewardPointsEarned(long salesId, long customerId, long companyId, decimal orderAmount, decimal redeemPoints, DateTime currentDate, long addedBy)
        {
            var settings = oConnectionContext.DbClsRewardPointSettings
                .Where(a => a.CompanyId == companyId && a.EnableRewardPoint && !a.IsDeleted)
                .FirstOrDefault();

            if (settings == null) return 0;

            // Calculate order amount for points (before points discount)
            decimal orderAmountForPoints = orderAmount + (redeemPoints * settings.RedeemAmountPerUnitPoint);

            // Check minimum order total
            if (settings.MinOrderTotalToEarnReward > 0 && orderAmountForPoints < settings.MinOrderTotalToEarnReward)
            {
                return 0;
            }

            // Get applicable tier or use default rate
            decimal amountSpentForUnitPoint = settings.AmountSpentForUnitPoint;
            var applicableTier = oConnectionContext.DbClsRewardPointTier
                .Where(a => a.CompanyId == companyId && 
                           a.IsActive && 
                           !a.IsDeleted &&
                           a.MinAmount <= orderAmountForPoints &&
                           (a.MaxAmount == null || a.MaxAmount > orderAmountForPoints))
                .OrderBy(a => a.Priority)
                .FirstOrDefault();

            if (applicableTier != null)
            {
                amountSpentForUnitPoint = applicableTier.AmountSpentForUnitPoint;
            }

            // Calculate points earned
            decimal pointsEarned = 0;
            if (amountSpentForUnitPoint > 0)
            {
                pointsEarned = Math.Floor(orderAmountForPoints / amountSpentForUnitPoint);
            }

            // Apply maximum points per order limit
            if (settings.MaxPointsPerOrder > 0 && pointsEarned > settings.MaxPointsPerOrder)
            {
                pointsEarned = settings.MaxPointsPerOrder;
            }

            if (pointsEarned <= 0) return 0;

            // Get customer record (reward points are now stored in tblUser)
            var customer = oConnectionContext.DbClsUser
                .Where(a => a.UserId == customerId && a.CompanyId == companyId && !a.IsDeleted)
                .FirstOrDefault();

            if (customer == null) return 0;

            // Calculate expiry date
            DateTime? expiryDate = CalculateExpiryDate(companyId, currentDate, settings.ExpiryPeriod, settings.ExpiryPeriodType);

            // Update customer reward points balance
            customer.AvailableRewardPoints += pointsEarned;
            customer.TotalRewardPointsEarned += pointsEarned;
            customer.LastRewardPointsEarnedDate = currentDate;
            customer.ModifiedBy = addedBy;
            customer.ModifiedOn = currentDate;

            // Create transaction record
            var transaction = new ClsRewardPointTransaction
            {
                CustomerId = customerId,
                SalesId = salesId,
                TransactionType = "Earn",
                Points = pointsEarned,
                OrderAmount = orderAmountForPoints,
                TransactionDate = currentDate,
                ExpiryDate = expiryDate,
                IsExpired = false,
                CompanyId = companyId,
                Notes = $"Points earned on invoice",
                IsActive = true,
                IsDeleted = false,
                AddedBy = addedBy,
                AddedOn = currentDate
            };
            oConnectionContext.DbClsRewardPointTransaction.Add(transaction);
            oConnectionContext.SaveChanges();

            return pointsEarned;
        }

        /// <summary>
        /// Calculate expiry date for reward points
        /// </summary>
        private DateTime? CalculateExpiryDate(long companyId, DateTime currentDate, int expiryPeriod, int expiryPeriodType)
        {
            if (expiryPeriod <= 0) return null;

            DateTime expiryDate = currentDate;
            switch (expiryPeriodType)
            {
                case 1: // Days
                    expiryDate = currentDate.AddDays(expiryPeriod);
                    break;
                case 2: // Weeks
                    expiryDate = currentDate.AddDays(expiryPeriod * 7);
                    break;
                case 3: // Months
                    expiryDate = currentDate.AddMonths(expiryPeriod);
                    break;
                case 4: // Years
                    expiryDate = currentDate.AddYears(expiryPeriod);
                    break;
            }

            return expiryDate;
        }

        /// <summary>
        /// Reverse reward points when a sale is deleted
        /// </summary>
        private void ProcessRewardPointsOnSaleDelete(long salesId, long companyId, DateTime currentDate, long modifiedBy)
        {
            // Get all transactions for this sale
            var transactions = oConnectionContext.DbClsRewardPointTransaction
                .Where(a => a.SalesId == salesId && a.CompanyId == companyId && !a.IsDeleted)
                .ToList();

            foreach (var txn in transactions)
            {
                var customer = oConnectionContext.DbClsUser
                    .Where(a => a.UserId == txn.CustomerId && a.CompanyId == companyId && !a.IsDeleted)
                    .FirstOrDefault();

                if (customer == null) continue;

                if (txn.TransactionType == "Earn")
                {
                    // Reverse earned points
                    customer.AvailableRewardPoints -= txn.Points;
                    if (customer.AvailableRewardPoints < 0) customer.AvailableRewardPoints = 0;
                    customer.TotalRewardPointsEarned -= txn.Points;
                    if (customer.TotalRewardPointsEarned < 0) customer.TotalRewardPointsEarned = 0;
                }
                else if (txn.TransactionType == "Redeem")
                {
                    // Restore redeemed points
                    customer.AvailableRewardPoints += txn.Points;
                    customer.TotalRewardPointsRedeemed -= txn.Points;
                    if (customer.TotalRewardPointsRedeemed < 0) customer.TotalRewardPointsRedeemed = 0;
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
