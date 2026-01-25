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
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class CustomerPaymentController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllCustomerPayments(ClsCustomerPaymentVm obj)
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

            var det = oConnectionContext.DbClsCustomerPayment.Where(b => b.CompanyId == obj.CompanyId
            //&& b.Type.ToLower() == obj.Type.ToLower() 
            && b.ParentId == 0 && b.IsDeleted == false && b.IsCancelled == false && b.SalesReturnId == 0
             && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate).Select(b => new
                {
                    b.IsCancelled,
                    b.BranchId,
                    b.AmountRemaining,
                    b.IsDirectPayment,
                    CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == b.CustomerId).Select(c => c.Name).FirstOrDefault(),
                    CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == b.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                    b.CustomerId,
                    InvoiceUrl = oCommonController.webUrl,
                    b.ParentId,
                    b.ReferenceNo,
                    AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                    b.CustomerPaymentId,
                    b.PaymentDate,
                    b.Notes,
                    b.Amount,
                    b.AmountExcTax,
                    b.AttachDocument,
                    b.PaymentTypeId,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                    b.AddedOn,
                    b.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    b.ReferenceId,
                    b.IsAdvance
                }).ToList();

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det.OrderByDescending(a => a.CustomerPaymentId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerPayment(ClsCustomerPaymentVm obj)
        {
            var det = oConnectionContext.DbClsCustomerPayment.Where(b => b.CustomerPaymentId == obj.CustomerPaymentId
            && b.CompanyId == obj.CompanyId).Select(b => new
            {
                b.BookingId,
                b.IsAdvance,
                b.PlaceOfSupplyId,
                b.TaxId,
                b.SalesId,
                b.SalesReturnId,
                b.CustomerId,
                b.AmountRemaining,
                b.IsDirectPayment,
                b.AccountId,
                b.ParentId,
                b.ReferenceNo,
                SalesReturnReferenceNo = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesReturnId == b.SalesReturnId).Select(c => c.InvoiceNo).FirstOrDefault(),
                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.CustomerPaymentId,
                b.PaymentDate,
                b.Notes,
                b.Amount,
                b.AttachDocument,
                b.PaymentTypeId,
                PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                b.AddedOn,
                b.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                b.BranchId,
                b.ReferenceId,
                CustomerPaymentIds = oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId == b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false).Select(c => new
                {
                    c.ParentId,
                    c.ReferenceNo,
                    c.CustomerPaymentId,
                    c.PaymentDate,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(d => d.PaymentTypeId == c.PaymentTypeId).Select(d => d.PaymentType).FirstOrDefault(),
                    c.Amount,
                    c.SalesId,
                    SalesDate = oConnectionContext.DbClsSales.Where(d => d.SalesId == c.SalesId).Select(d => d.SalesDate).FirstOrDefault(),
                    c.Type,
                    InvoiceNo = oConnectionContext.DbClsSales.Where(d => d.SalesId == c.SalesId).Select(d => d.InvoiceNo).FirstOrDefault(),
                    GrandTotal = oConnectionContext.DbClsSales.Where(d => d.SalesId == c.SalesId).Select(d => d.GrandTotal).FirstOrDefault(),
                    OpeningBalance = oConnectionContext.DbClsUser.Where(d => d.UserId == c.CustomerId).Select(d => d.OpeningBalance).FirstOrDefault(),
                    // Booking specific details for credits applied to table bookings
                    c.BookingId,
                    BookingDate = oConnectionContext.DbClsTableBooking.Where(d => d.BookingId == c.BookingId).Select(d => d.BookingDate).FirstOrDefault(),
                    BookingNo = oConnectionContext.DbClsTableBooking.Where(d => d.BookingId == c.BookingId).Select(d => d.BookingNo).FirstOrDefault(),
                    DepositAmount = oConnectionContext.DbClsTableBooking.Where(d => d.BookingId == c.BookingId).Select(d => d.DepositAmount).FirstOrDefault()
                }).Union(
                    oConnectionContext.DbClsCustomerPayment.Where(c => c.CustomerPaymentId == b.CustomerPaymentId && c.IsDirectPayment == true).Select(c => new
                    {
                        c.ParentId,
                        c.ReferenceNo,
                        c.CustomerPaymentId,
                        c.PaymentDate,
                        PaymentType = oConnectionContext.DbClsPaymentType.Where(d => d.PaymentTypeId == c.PaymentTypeId).Select(d => d.PaymentType).FirstOrDefault(),
                        c.Amount,
                        c.SalesId,
                        SalesDate = oConnectionContext.DbClsSales.Where(d => d.SalesId == c.SalesId).Select(d => d.SalesDate).FirstOrDefault(),
                        c.Type,
                        InvoiceNo = oConnectionContext.DbClsSales.Where(d => d.SalesId == c.SalesId).Select(d => d.InvoiceNo).FirstOrDefault(),
                        GrandTotal = oConnectionContext.DbClsSales.Where(d => d.SalesId == c.SalesId).Select(d => d.GrandTotal).FirstOrDefault(),
                        OpeningBalance = oConnectionContext.DbClsUser.Where(d => d.UserId == c.CustomerId).Select(d => d.OpeningBalance).FirstOrDefault(),
                        // Booking specific details for direct booking payments, if any
                        c.BookingId,
                        BookingDate = oConnectionContext.DbClsTableBooking.Where(d => d.BookingId == c.BookingId).Select(d => d.BookingDate).FirstOrDefault(),
                        BookingNo = oConnectionContext.DbClsTableBooking.Where(d => d.BookingId == c.BookingId).Select(d => d.BookingNo).FirstOrDefault(),
                        DepositAmount = oConnectionContext.DbClsTableBooking.Where(d => d.BookingId == c.BookingId).Select(d => d.DepositAmount).FirstOrDefault()
                    }))
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayment = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> InsertCustomerPayment(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.CompanyId == 0)
            {
                obj.BranchId = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.BranchId).FirstOrDefault();
                obj.CompanyId = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.CompanyId).FirstOrDefault();
                obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId
                && a.IsPaymentGateway == true).Select(a => a.PaymentTypeId).FirstOrDefault();
                obj.CustomerId = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.CustomerId).FirstOrDefault();
                obj.Amount = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.Amount).FirstOrDefault();
                obj.PaymentDate = CurrentDate;
                obj.AmountExcTax = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.Amount).FirstOrDefault();
                obj.PlaceOfSupplyId = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.PlaceOfSupplyId).FirstOrDefault();
                obj.AccountId = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.AccountId).FirstOrDefault();
            }

            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.CustomerId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCustomer" });
                    isError = true;
                }

                if (obj.AmountExcTax == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmountExcTax" });
                    isError = true;
                }

                if (obj.PaymentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
                    isError = true;
                }

                if (obj.PaymentTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                    isError = true;
                }

                if (obj.CustomerPaymentIds != null)
                {
                    obj.CustomerPaymentIds = obj.CustomerPaymentIds.Where(a => a.Amount > 0).ToList();
                    foreach (var item in obj.CustomerPaymentIds)
                    {
                        if (item.Due < item.Amount)
                        {
                            errors.Add(new ClsError { Message = "Amount received cannot be more than due", Id = "divAmount" + item.SalesId });
                            isError = true;
                        }
                    }

                    if (obj.Amount != 0)
                    {
                        if (obj.Amount < obj.CustomerPaymentIds.Select(a => a.Amount).DefaultIfEmpty().Sum())
                        {
                            errors.Add(new ClsError { Message = "The amount entered for individual invoice(s) exceeds the total amount", Id = "divAmount" });
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

                long CustomerPaymentId = 0;

                List<ClsCustomerPaymentIds> oClsCustomerPaymentIds = new List<ClsCustomerPaymentIds>();
                decimal RemainingAmount = obj.Amount;

                if (obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count() > 0)
                {
                    long PrefixId = 0;
                    if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
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
           && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                    var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => new
                    {
                        a.IsBusinessRegistered,
                        a.GstTreatment,
                        a.BusinessRegistrationNameId,
                        a.BusinessRegistrationNo,
                        a.BusinessLegalName,
                        a.BusinessTradeName,
                        a.PanNo,
                        a.PlaceOfSupplyId
                    }).FirstOrDefault();

                    ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        Notes = obj.Notes,
                        Amount = obj.Amount,
                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                        PaymentTypeId = obj.PaymentTypeId,
                        CustomerId = obj.CustomerId,
                        AttachDocument = obj.AttachDocument,
                        Type = obj.Type,
                        BranchId = obj.BranchId,
                        AccountId = obj.AccountId,
                        ReferenceNo = obj.ReferenceNo,
                        IsDebit = 1,
                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                        ReferenceId = oCommonController.CreateToken(),
                        //PaymentIds = _json
                        JournalAccountId = JournalAccountId,
                        PaymentLinkId = obj.PaymentLinkId,
                        TaxId = obj.TaxId,
                        TaxAccountId = 0,
                        AmountExcTax = obj.AmountExcTax,
                        TaxAmount = obj.TaxAmount,
                        AmountRemaining = obj.Amount - obj.CustomerPaymentIds.Sum(a => a.Amount),
                        AmountUsed = obj.CustomerPaymentIds.Sum(a => a.Amount),
                        PrefixId = PrefixId,
                        PlaceOfSupplyId = userDet.PlaceOfSupplyId,
                        IsBusinessRegistered = userDet.IsBusinessRegistered,
                        GstTreatment = userDet.GstTreatment,
                        BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                        BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                        BusinessLegalName = userDet.BusinessLegalName,
                        BusinessTradeName = userDet.BusinessTradeName,
                        PanNo = userDet.PanNo,
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

                        oClsCustomerPayment.AttachDocument = filepathPass;
                    }
                    oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment);
                    oConnectionContext.SaveChanges();

                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + (obj.Amount - obj.CustomerPaymentIds.Sum(a => a.Amount)) + " where \"UserId\"=" + obj.CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);

                    CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId;

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();

                    foreach (var item in obj.CustomerPaymentIds)
                    {
                        long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
          && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                        if (RemainingAmount > 0)
                        {
                            if (item.Due != 0)
                            {
                                decimal _amount = 0;
                                _amount = item.Amount;
                                if (item.Type.ToLower() == "customer opening balance payment")
                                {
                                    ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        Notes = "",
                                        Amount = _amount,
                                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                        PaymentTypeId = obj.PaymentTypeId,
                                        CustomerId = obj.CustomerId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = item.Type,
                                        BranchId = obj.BranchId,
                                        AccountId = JournalAccountId,
                                        //ReferenceNo = ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = oClsCustomerPayment.CustomerPaymentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = AccountId,
                                        PaymentLinkId = obj.PaymentLinkId,
                                        TaxId = obj.TaxId,
                                        TaxAccountId = 0,
                                        AmountExcTax = _amount,
                                        TaxAmount = 0,
                                    };
                                    oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment1);
                                    oConnectionContext.SaveChanges();

                                    oClsCustomerPaymentIds.Add(new ClsCustomerPaymentIds { CustomerPaymentId = oClsCustomerPayment1.CustomerPaymentId, CustomerId = item.CustomerId, SalesId = item.SalesId, Type = item.Type, Amount = _amount });
                                }
                                else
                                {
                                    ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        Notes = "",
                                        Amount = _amount,
                                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                        PaymentTypeId = obj.PaymentTypeId,
                                        CustomerId = obj.CustomerId,
                                        SalesId = item.SalesId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = item.Type,
                                        BranchId = obj.BranchId,
                                        AccountId = JournalAccountId,
                                        //ReferenceNo = ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = oClsCustomerPayment.CustomerPaymentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = AccountId,
                                        PaymentLinkId = obj.PaymentLinkId,
                                        TaxId = obj.TaxId,
                                        TaxAccountId = 0,
                                        AmountExcTax = _amount,
                                        TaxAmount = 0,
                                    };
                                    oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment1);
                                    oConnectionContext.SaveChanges();

                                    oClsCustomerPaymentIds.Add(new ClsCustomerPaymentIds { CustomerPaymentId = oClsCustomerPayment1.CustomerPaymentId, CustomerId = item.CustomerId, SalesId = item.SalesId, Type = item.Type, Amount = _amount });
                                }

                                RemainingAmount = RemainingAmount - _amount;

                                if (item.Type == "Sales Payment")
                                {
                                    string PaymentStatus = "";
                                    decimal GrandTotal = oConnectionContext.DbClsSales.Where(a => a.SalesId == item.SalesId).Select(a => a.GrandTotal).FirstOrDefault();

                                    decimal previousPayments = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") &&
                                   b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                                    if (GrandTotal == (previousPayments))
                                    {
                                        PaymentStatus = "Paid";
                                    }
                                    else if (GrandTotal > (previousPayments))
                                    {
                                        PaymentStatus = "Partially Paid";
                                    }

                                    string query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                            }
                        }
                    }

                    serializer.MaxJsonLength = 2147483644;
                    string _json = serializer.Serialize(oClsCustomerPaymentIds);

                    //string r = "update \"tblCustomerPayment\" set \"PaymentIds\"='" + _json + "' where \"CustomerPaymentId\"=" + oClsCustomerPayment.CustomerPaymentId;
                    //oConnectionContext.Database.ExecuteSqlCommand(r);

                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsCustomerPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    //Customer Advance Payment
                    long PrefixId = 0;
                    if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                    {
                        PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
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
                && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                    long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

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

                    //tax journal
                    var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == obj.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                    List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                    decimal AmountExcTax = obj.AmountExcTax;
                    var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == obj.TaxId
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

                    //tax journal

                    ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        Notes = obj.Notes,
                        Amount = obj.Amount,
                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                        PaymentTypeId = obj.PaymentTypeId,
                        CustomerId = obj.CustomerId,
                        AttachDocument = obj.AttachDocument,
                        //Type = obj.Type == "Customer Direct Advance Payment" ? "Customer Direct Advance Payment" : "Customer Advance Payment",
                        Type = obj.Type,
                        BranchId = obj.BranchId,
                        AccountId = obj.AccountId,
                        ReferenceNo = obj.ReferenceNo,
                        IsDebit = 1,
                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                        ReferenceId = oCommonController.CreateToken(),
                        //PaymentIds = _json
                        JournalAccountId = JournalAccountId,
                        AmountRemaining = obj.Amount,
                        PaymentLinkId = obj.PaymentLinkId,
                        PlaceOfSupplyId = obj.PlaceOfSupplyId,
                        TaxId = obj.TaxId,
                        IsBusinessRegistered = userDet.IsBusinessRegistered,
                        GstTreatment = userDet.GstTreatment,
                        BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                        BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                        BusinessLegalName = userDet.BusinessLegalName,
                        BusinessTradeName = userDet.BusinessTradeName,
                        PanNo = userDet.PanNo,
                        TaxAccountId = TaxAccountId,
                        AmountExcTax = obj.AmountExcTax,
                        TaxAmount = obj.TaxAmount,
                        PrefixId = PrefixId,
                        IsAdvance = true
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

                        oClsCustomerPayment.AttachDocument = filepathPass;
                    }
                    oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment);
                    oConnectionContext.SaveChanges();

                    CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId;

                    string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + obj.Amount + " where \"UserId\"=" + obj.CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    foreach (var taxJournal in taxList)
                    {
                        ClsCustomerPaymentTaxJournal oClsCustomerPaymentTaxJournal = new ClsCustomerPaymentTaxJournal()
                        {
                            CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId,
                            TaxId = taxJournal.TaxId,
                            TaxAmount = taxJournal.TaxAmount,
                            AccountId = taxJournal.AccountId,
                            CustomerPaymentTaxJournalType = taxJournal.TaxType
                        };
                        oConnectionContext.DbClsCustomerPaymentTaxJournal.Add(oClsCustomerPaymentTaxJournal);
                        oConnectionContext.SaveChanges();
                    }

                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsCustomerPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                if (obj.PaymentLinkId != 0)
                {
                    string r = "update \"tblPaymentLink\" set \"Status\"= 'Paid' where \"PaymentLinkId\"=" + obj.PaymentLinkId;
                    oConnectionContext.Database.ExecuteSqlCommand(r);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Payment \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
                    Id = CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Customer Payment created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerPaymentDelete(ClsCustomerPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CustomerPayment = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => new
                {
                    a.Type,
                    a.Amount,
                    a.CustomerId,
                    a.SalesId,
                    a.AmountRemaining
                }).FirstOrDefault();

                if (CustomerPayment != null)
                {
                    if (CustomerPayment.Type.ToLower() == "customer payment")
                    {
                        //if (oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == obj.CustomerPaymentId).Count() == 1)
                        //{
                        //    data = new
                        //    {
                        //        Status = 0,
                        //        Message = "Cannot Delete.. Some of the amount is already used in Sales Payment",
                        //        Data = new
                        //        {
                        //        }
                        //    };
                        //    return await Task.FromResult(Ok(data));
                        //}
                        //if (oConnectionContext.DbClsCustomerPayment.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.ParentId == obj.CustomerPaymentId).Count() >= 1)
                        //{
                        //    data = new
                        //    {
                        //        Status = 0,
                        //        Message = "Cannot Delete.. Some of the amount is already used in Sales Payment",
                        //        Data = new
                        //        {
                        //        }
                        //    };
                        //    return await Task.FromResult(Ok(data));
                        //}
                        //else 
                        if (oConnectionContext.DbClsCustomerPayment.Where(a => a.ParentId == obj.CustomerPaymentId
                        && a.IsDeleted == false && a.IsCancelled == false && a.Type.ToLower() == "customer refund").Select(a => a.CustomerPaymentId).Count() > 0)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "Payment cannot be deleted as one or more refunds have been recorded for the payment",
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                        else
                        {
                            string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + CustomerPayment.AmountRemaining + " where \"UserId\"=" + CustomerPayment.CustomerId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                        }
                    }

                    ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                    {
                        CustomerPaymentId = obj.CustomerPaymentId,
                        IsDeleted = true,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment);
                    oConnectionContext.Entry(oClsCustomerPayment).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(oClsCustomerPayment).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsCustomerPayment).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    decimal UsedAmount = 0;
                    //string paymentIds = oConnectionContext.DbClsCustomerPayment.
                    //                    Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.PaymentIds).FirstOrDefault() ?? "[]";
                    //List<ClsCustomerPaymentIds> _paymentIds = serializer.Deserialize<List<ClsCustomerPaymentIds>>(paymentIds);

                    List<ClsCustomerPaymentIds> _paymentIds = oConnectionContext.DbClsCustomerPayment.Where(a =>
                    a.ParentId == obj.CustomerPaymentId && a.Type != "Customer Refund").Select(a => new ClsCustomerPaymentIds
                    {
                        CustomerPaymentId = a.CustomerPaymentId,
                        Type = a.Type,
                        SalesId = a.SalesId
                    }).ToList();

                    if (_paymentIds != null)
                    {
                        foreach (var item in _paymentIds)
                        {
                            //if(item.Type.ToLower() == "customer refund")
                            //{
                            //    data = new
                            //    {
                            //        Status = 0,
                            //        Message = "Payment cannot be deleted as one or more refunds have been recorded for the payment",
                            //        Data = new
                            //        {
                            //        }
                            //    };
                            //    return await Task.FromResult(Ok(data));
                            //}
                            if (item.Type.ToLower() == "customer opening balance payment")
                            {
                                ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                {
                                    CustomerPaymentId = item.CustomerPaymentId,
                                    IsDeleted = true,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                };
                                oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                                oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                            //else if (item.Type.ToLower() == "customer advance payment")
                            //{
                            //    if (oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == item.CustomerPaymentId).Count() == 1)
                            //    {
                            //        data = new
                            //        {
                            //            Status = 0,
                            //            Message = "Cannot Delete.. Some of the amount is already used in Sales Payment",
                            //            Data = new
                            //            {
                            //            }
                            //        };
                            //        return await Task.FromResult(Ok(data));
                            //    }
                            //    ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                            //    {
                            //        CustomerPaymentId = item.CustomerPaymentId,
                            //        IsDeleted = true,
                            //        ModifiedBy = obj.AddedBy,
                            //        ModifiedOn = CurrentDate,
                            //    };
                            //    oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                            //    oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                            //    oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            //    oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            //    oConnectionContext.SaveChanges();

                            //    string query1 = "update tblUser set AdvanceBalance=AdvanceBalance,0)-" + item.Amount + " where Userid=" + item.CustomerId;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query1);
                            //}
                            else
                            {
                                ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                {
                                    CustomerPaymentId = item.CustomerPaymentId,
                                    IsDeleted = true,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                };
                                oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                                oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }

                            UsedAmount = UsedAmount + item.Amount;

                            string query = "", PaymentStatus = "";

                            if (item.Type.ToLower() == "sales payment")
                            {
                                int count = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == item.Type && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).Count();
                                if (count == 0)
                                {
                                    PaymentStatus = "Due";
                                }
                                else
                                {
                                    PaymentStatus = "Partially Paid";
                                }

                                query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                #region check OverDue Payment
                                var sale = (from a in oConnectionContext.DbClsSales
                                                //                         join b in oConnectionContext.DbClsUser
                                                //on a.CustomerId equals b.UserId
                                            where a.SalesId == item.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
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

                                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                                    {
                                        string query1 = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + item.SalesId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                                    }
                                }
                                #endregion
                            }
                        }
                    }

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Customer Payment",
                        CompanyId = obj.CompanyId,
                        Description = "Customer Payment \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                        Id = oClsCustomerPayment.CustomerPaymentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Delete"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }

                data = new
                {
                    Status = 1,
                    Message = "Customer Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerPaymentCancel(ClsCustomerPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CustomerPayment = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => new
                {
                    a.Type,
                    a.Amount,
                    a.CustomerId,
                    a.SalesId,
                    a.AmountRemaining,
                    a.IsAdvance
                }).FirstOrDefault();

                //if (CustomerPayment.Type.ToLower() == "customer direct advance payment" || CustomerPayment.Type.ToLower() == "customer advance payment")
                if (CustomerPayment.Type.ToLower() == "customer payment")
                {
                    //if (oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == obj.CustomerPaymentId).Count() == 1)
                    //{
                    //    data = new
                    //    {
                    //        Status = 0,
                    //        Message = "Cannot Delete.. Some of the amount is already used in Sales Payment",
                    //        Data = new
                    //        {
                    //        }
                    //    };
                    //    return await Task.FromResult(Ok(data));
                    //}
                    if (oConnectionContext.DbClsCustomerPayment.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.ParentId == obj.CustomerPaymentId).Count() == 1)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Cannot Delete.. Some of the amount is already used in Sales Payment",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    else if (oConnectionContext.DbClsCustomerPayment.Where(a => a.ParentId == obj.CustomerPaymentId
                    && a.IsDeleted == false && a.IsCancelled == false && a.Type.ToLower() == "customer refund").Select(a => a.CustomerPaymentId).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Payment cannot be deleted as one or more refunds have been recorded for the payment",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    else
                    {
                        string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + CustomerPayment.AmountRemaining + " where \"UserId\"=" + CustomerPayment.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                {
                    CustomerPaymentId = obj.CustomerPaymentId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment);
                oConnectionContext.Entry(oClsCustomerPayment).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsCustomerPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsCustomerPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                decimal UsedAmount = 0;
                //string paymentIds = oConnectionContext.DbClsCustomerPayment.
                //                    Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.PaymentIds).FirstOrDefault() ?? "[]";
                //List<ClsCustomerPaymentIds> _paymentIds = serializer.Deserialize<List<ClsCustomerPaymentIds>>(paymentIds);

                List<ClsCustomerPaymentIds> _paymentIds = oConnectionContext.DbClsCustomerPayment.Where(a =>
                a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => new ClsCustomerPaymentIds
                {
                    CustomerPaymentId = a.CustomerPaymentId,
                    Type = a.Type,
                }).ToList();

                if (_paymentIds != null)
                {
                    foreach (var item in _paymentIds)
                    {
                        //if(item.Type.ToLower() == "customer refund")
                        //{
                        //    data = new
                        //    {
                        //        Status = 0,
                        //        Message = "Payment cannot be deleted as one or more refunds have been recorded for the payment",
                        //        Data = new
                        //        {
                        //        }
                        //    };
                        //    return await Task.FromResult(Ok(data));
                        //}
                        if (item.Type.ToLower() == "customer opening balance payment")
                        {
                            ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                            {
                                CustomerPaymentId = item.CustomerPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                        //else if (item.Type.ToLower() == "customer advance payment")
                        //{
                        //    if (oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == item.CustomerPaymentId).Count() == 1)
                        //    {
                        //        data = new
                        //        {
                        //            Status = 0,
                        //            Message = "Cannot Delete.. Some of the amount is already used in Sales Payment",
                        //            Data = new
                        //            {
                        //            }
                        //        };
                        //        return await Task.FromResult(Ok(data));
                        //    }
                        //    ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                        //    {
                        //        CustomerPaymentId = item.CustomerPaymentId,
                        //        IsDeleted = true,
                        //        ModifiedBy = obj.AddedBy,
                        //        ModifiedOn = CurrentDate,
                        //    };
                        //    oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                        //    oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                        //    oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                        //    oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                        //    oConnectionContext.SaveChanges();

                        //    string query1 = "update tblUser set AdvanceBalance=AdvanceBalance,0)-" + item.Amount + " where Userid=" + item.CustomerId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query1);
                        //}
                        else
                        {
                            ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                            {
                                CustomerPaymentId = item.CustomerPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }

                        UsedAmount = UsedAmount + item.Amount;

                        string query = "", PaymentStatus = "";

                        if (item.Type.ToLower() == "sales payment")
                        {
                            int count = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == item.Type && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).Count();
                            if (count == 0)
                            {
                                PaymentStatus = "Due";
                            }
                            else
                            {
                                PaymentStatus = "Partially Paid";
                            }

                            query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            #region check OverDue Payment
                            var sale = (from a in oConnectionContext.DbClsSales
                                            //                         join b in oConnectionContext.DbClsUser
                                            //on a.CustomerId equals b.UserId
                                        where a.SalesId == item.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
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

                                //if (sale.PayTerm == 1)
                                //{
                                //    expDate = sale.SalesDate.AddDays(sale.PayTermNo);
                                //}
                                //else if (sale.PayTerm == 2)
                                //{
                                //    expDate = sale.SalesDate.AddMonths(sale.PayTermNo);
                                //}
                                //else if (sale.PayTerm == 3)
                                //{
                                //    expDate = sale.SalesDate.AddYears(sale.PayTermNo);
                                //}

                                //if (sale.DueDate < DateTime.Now)
                                if ((DateTime.Now - sale.DueDate).Days >= 1)
                                {
                                    string query1 = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + item.SalesId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                }
                            }
                            #endregion
                        }
                    }
                }

                // var paymentDetails = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a =>
                //new
                //{
                //    a.CustomerId,
                //    a.SalesId,
                //    a.Type,
                //    a.Amount,
                //}).FirstOrDefault();

                // decimal RemainingAmount = paymentDetails.Amount - UsedAmount;

                // if (RemainingAmount > 0)
                // {
                //     string query = "update tblUser set AdvanceBalance=AdvanceBalance,0)-" + RemainingAmount + " where Userid=" + paymentDetails.Id;
                //     oConnectionContext.Database.ExecuteSqlCommand(query);
                // }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Payment \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" cancelled",
                    Id = oClsCustomerPayment.CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (CustomerPayment.IsAdvance == false)
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsCustomerPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsCustomerPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Customer Payment cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> InsertSalesPayment(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            if (obj.CompanyId == 0)
            {
                obj.CompanyId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.CompanyId).FirstOrDefault();
                obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId
                && a.IsPaymentGateway == true).Select(a => a.PaymentTypeId).FirstOrDefault();
                obj.PaymentDate = CurrentDate;
                obj.Amount = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum() -
                (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum());
                obj.AccountId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.AccountId).FirstOrDefault();
                obj.Type = "Sales Payment";
            }

            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                string PaymentStatus = "";
                decimal GrandTotal = 0;
                decimal previousPayments = 0;
                int IsDebit = 0;

                if (obj.PaymentType == "Advance")
                {
                    if (obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count > 0)
                    {
                        obj.CustomerPaymentIds = obj.CustomerPaymentIds.Where(a => a.Amount > 0).DefaultIfEmpty().ToList();
                        
                        // Validate PaymentDate for credit application
                        if (obj.PaymentDate == DateTime.MinValue)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divCreditsAppliedDate" });
                            isError = true;
                        }
                        
                        foreach (var item in obj.CustomerPaymentIds)
                        {
                            if (item.AmountRemaining < item.Amount)
                            {
                                errors.Add(new ClsError { Message = "Amount received cannot be more than Amount Remaining", Id = "divAmount" + item.CustomerPaymentId });
                                isError = true;
                            }
                        }

                        obj.Amount = obj.CustomerPaymentIds.Select(a => a.Amount).DefaultIfEmpty().Sum();
                    }
                    else
                    {
                        // For non-credit advance payments, use CurrentDate
                        obj.PaymentDate = CurrentDate;
                    }
                }
                else if (obj.PaymentTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                    isError = true;
                }

                if (obj.PaymentType != "Advance")
                {
                    if (obj.Amount == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                        isError = true;
                    }

                    if (obj.PaymentDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
                        isError = true;
                    }

                    if (obj.AccountId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divAccount" });
                        isError = true;
                    }
                }

                List<ClsCustomerPaymentVm> oAdvanceBalances = new List<ClsCustomerPaymentVm>();

                obj.CustomerId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.CustomerId).FirstOrDefault();
                obj.PlaceOfSupplyId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.PlaceOfSupplyId).FirstOrDefault();
                if (obj.PaymentType == "" || obj.PaymentType == null)
                {
                    obj.PaymentType = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.PaymentTypeId == obj.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                }
                else
                {
                    obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();
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

                GrandTotal = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).FirstOrDefault();
                obj.BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();
                IsDebit = 2;

                previousPayments = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == obj.Type && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == obj.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum();
                if ((previousPayments + obj.Amount) > GrandTotal)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Payment amount cannot be more than Due amount",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (GrandTotal == (previousPayments + obj.Amount))
                {
                    PaymentStatus = "Paid";
                }
                else if (GrandTotal > (previousPayments + obj.Amount))
                {
                    PaymentStatus = "Partially Paid";
                }

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                long CustomerPaymentId = 0;

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

                if (obj.PaymentType == "Advance")
                {
                    obj.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                    foreach (var item in obj.CustomerPaymentIds)
                    {
                        decimal amount = item.Amount;
                        ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            IsActive = obj.IsActive,
                            IsDeleted = obj.IsDeleted,
                            Notes = obj.Notes,
                            Amount = amount,
                            PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                            PaymentTypeId = obj.PaymentTypeId,
                            CustomerId = obj.CustomerId,
                            SalesId = obj.SalesId,
                            AttachDocument = obj.AttachDocument,
                            Type = obj.Type,
                            BranchId = obj.BranchId,
                            AccountId = obj.AccountId,
                            //ReferenceNo = ReferenceNo,
                            IsDebit = IsDebit,
                            //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                            ParentId = item.CustomerPaymentId,
                            ReferenceId = oCommonController.CreateToken(),
                            JournalAccountId = JournalAccountId,
                            PaymentLinkId = obj.PaymentLinkId,
                            PlaceOfSupplyId = obj.PlaceOfSupplyId,
                            TaxId = 0,
                            TaxAccountId = 0,
                            AmountExcTax = amount,
                            TaxAmount = 0,
                            IsBusinessRegistered = userDet.IsBusinessRegistered,
                            GstTreatment = userDet.GstTreatment,
                            BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                            BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                            BusinessLegalName = userDet.BusinessLegalName,
                            BusinessTradeName = userDet.BusinessTradeName,
                            PanNo = userDet.PanNo
                        };
                        oConnectionContext.DbClsCustomerPayment.Add(oClsPayment);
                        oConnectionContext.SaveChanges();

                        string _query1 = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"CustomerPaymentId\"=" + item.CustomerPaymentId;
                        oConnectionContext.Database.ExecuteSqlCommand(_query1);

                        long SalesReturnId = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == item.CustomerPaymentId).Select(a => a.SalesReturnId).FirstOrDefault();
                        if (SalesReturnId != 0)
                        {
                            decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                            == item.CustomerPaymentId).Select(a => a.AmountRemaining).FirstOrDefault();

                            if (AmountRemaining <= 0)
                            {
                                string query11 = "update \"tblSalesReturn\" set \"Status\"='Closed' where \"SalesReturnId\"=" + SalesReturnId;
                                oConnectionContext.Database.ExecuteSqlCommand(query11);
                            }
                        }
                    }

                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Amount + " where \"UserId\"=" + obj.CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                }
                else
                {
                    long PrefixId = 0;
                    if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                    {
                        PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
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

                    ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        Notes = obj.Notes,
                        Amount = obj.Amount,
                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                        PaymentTypeId = obj.PaymentTypeId,
                        SalesId = obj.SalesId,
                        AttachDocument = obj.AttachDocument,
                        Type = obj.Type,
                        BranchId = obj.BranchId,
                        AccountId = obj.AccountId,
                        ReferenceNo = obj.ReferenceNo,
                        IsDebit = IsDebit,
                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                        ReferenceId = oCommonController.CreateToken(),
                        JournalAccountId = JournalAccountId,
                        CustomerId = obj.CustomerId,
                        IsDirectPayment = true,
                        TaxId = 0,
                        TaxAccountId = 0,
                        AmountExcTax = obj.Amount,
                        TaxAmount = 0,
                        PrefixId = PrefixId,
                        PlaceOfSupplyId = obj.PlaceOfSupplyId,
                        IsBusinessRegistered = userDet.IsBusinessRegistered,
                        GstTreatment = userDet.GstTreatment,
                        BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                        BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                        BusinessLegalName = userDet.BusinessLegalName,
                        BusinessTradeName = userDet.BusinessTradeName,
                        PanNo = userDet.PanNo
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

                        oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                        oClsPayment.AttachDocument = filepathPass;
                    }

                    oConnectionContext.DbClsCustomerPayment.Add(oClsPayment);
                    oConnectionContext.SaveChanges();

                    CustomerPaymentId = oClsPayment.CustomerPaymentId;

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                }

                string query = "";
                query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + obj.SalesId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                #region check OverDue Payment
                if (PaymentStatus != "Paid")
                {
                    var sale = (from a in oConnectionContext.DbClsSales
                                where a.SalesId == obj.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                select new
                                {
                                    a.SalesId,
                                    a.SalesDate,
                                    a.CustomerId,
                                    a.DueDate,
                                    //b.PayTerm,
                                    //b.PayTermNo
                                }).FirstOrDefault();

                    if (sale != null)
                    {
                        if ((DateTime.Now - sale.DueDate).Days >= 1)
                        {
                            string query1 = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + sale.SalesId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                        }
                    }
                }
                #endregion

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Customer Payment",
                        CompanyId = obj.CompanyId,
                        Description = "Sales Payment \"" + obj.ReferenceNo + "\" created",
                        Id = CustomerPaymentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }

                data = new
                {
                    Status = 1,
                    Message = "Sales Payment created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesPayments(ClsCustomerPaymentVm obj)
        {
            decimal AdvanceBalance = 0, Due = 0;

            long UserId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.CustomerId).FirstOrDefault();
            AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == UserId).Select(a => a.AdvanceBalance).FirstOrDefault();

            Due = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                   (from a in oConnectionContext.DbClsSales
                    join b in oConnectionContext.DbClsCustomerPayment
        on a.SalesId equals b.SalesId
                    where a.SalesId == obj.SalesId && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
                    select b.Amount).DefaultIfEmpty().Sum();


            var det = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesId == obj.SalesId
            //&& b.Type.ToLower() == obj.Type.ToLower() 
            && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
            {
                b.Type,
                InvoiceUrl = oCommonController.webUrl,
                b.ParentId,
                ParentReferenceNo = oConnectionContext.DbClsCustomerPayment.Where(bb => bb.CustomerPaymentId == b.ParentId).Select(bb => bb.ReferenceNo).FirstOrDefault(),
                b.ReferenceNo,
                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.CustomerPaymentId,
                b.PaymentDate,
                b.Notes,
                b.Amount,
                b.AttachDocument,
                b.PaymentTypeId,
                PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                b.AddedOn,
                b.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                b.ReferenceId,
                SalesReturnId = oConnectionContext.DbClsCustomerPayment.Where(bb => bb.CustomerPaymentId == b.ParentId).Select(bb => bb.SalesReturnId).FirstOrDefault(),
                SalesReturnInvoiceNo = oConnectionContext.DbClsSalesReturn.Where(c => c.SalesReturnId ==
                oConnectionContext.DbClsCustomerPayment.Where(bb => bb.CustomerPaymentId == b.ParentId).Select(bb => bb.SalesReturnId).FirstOrDefault()).Select(c => c.InvoiceNo).FirstOrDefault(),
            }).OrderByDescending(b => b.CustomerPaymentId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det,
                    User = new
                    {
                        AdvanceBalance = AdvanceBalance,
                        Due = Due,
                    }
                }
            };
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> SalesPayment(ClsCustomerPaymentVm obj)
        //{
        //    var det = oConnectionContext.DbClsCustomerPayment.Where(b => b.CustomerPaymentId == obj.CustomerPaymentId && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
        //    {
        //        b.AccountId,
        //        b.ParentId,
        //        b.ReferenceNo,
        //        AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
        //        b.CustomerPaymentId,
        //        b.PaymentDate,
        //        b.Notes,
        //        b.Amount,
        //        b.AttachDocument,
        //        b.PaymentTypeId,
        //        PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
        //        b.AddedOn,
        //        b.ModifiedOn,
        //        AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
        //        ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
        //        b.BranchId,
        //        b.ReferenceId
        //    }).FirstOrDefault();

        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            CustomerPayment = det,
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> SalesPaymentDelete(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                {
                    CustomerPaymentId = obj.CustomerPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var paymentDetails = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a =>
                new
                {
                    a.ParentId,
                    a.SalesId,
                    a.Type,
                    a.Amount,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault()
                }).FirstOrDefault();
                string PaymentStatus = ""; int count = 0;

                count = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == paymentDetails.Type && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == paymentDetails.SalesId).Select(b => b.Amount).Count();
                if (count == 0)
                {
                    PaymentStatus = "Due";
                }
                else
                {
                    PaymentStatus = "Partially Paid";
                }

                //string query = "";
                //if (paymentDetails.Type.ToLower() == "sales payment")
                //{
                string query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + paymentDetails.SalesId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                //if (paymentDetails.PaymentType == "Advance")
                if (paymentDetails.ParentId != 0)
                {
                    //var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => new
                    //{
                    //    a.CustomerPaymentDeductionId,
                    //    a.DeductedFromId,
                    //    a.Amount
                    //}).ToList();

                    var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => new
                    {
                        ParentSalesReturnId = oConnectionContext.DbClsCustomerPayment.Where(b => b.CustomerPaymentId == a.ParentId).Select(b => b.SalesReturnId).FirstOrDefault(),
                        a.ParentId,
                        a.Amount
                    }).ToList();

                    foreach (var inner in CustomerPaymentDeductionIds)
                    {
                        string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"CustomerPaymentId\"=" + inner.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(q);

                        //q = "update \"tblCustomerPaymentDeductionId\" set \"IsDeleted\"=True where \"CustomerPaymentDeductionId\"=" + inner.CustomerPaymentDeductionId;
                        //oConnectionContext.Database.ExecuteSqlCommand(q);

                        if (inner.ParentSalesReturnId != 0)
                        {
                            decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                            == inner.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                            if (AmountRemaining > 0)
                            {
                                string query1 = "update \"tblSalesReturn\" set \"Status\"='Open' where \"SalesReturnId\"=" + inner.ParentSalesReturnId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                            }
                        }
                    }

                    long CustomerId = oConnectionContext.DbClsSales.Where(a => a.SalesId == paymentDetails.SalesId).Select(a => a.CustomerId).FirstOrDefault();
                    string query2 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + paymentDetails.Amount + " where \"UserId\"=" + CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(query2);
                }

                #region check OverDue Payment
                var sale = (from a in oConnectionContext.DbClsSales
                                //                         join b in oConnectionContext.DbClsUser
                                //on a.CustomerId equals b.UserId
                            where a.SalesId == paymentDetails.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                            //&& b.PayTermNo != 0
                            select new
                            {
                                a.SalesId,
                                a.SalesDate,
                                a.CustomerId,
                                a.DueDate,
                                //b.PayTerm,
                                //b.PayTermNo
                            }).FirstOrDefault();

                if (sale != null)
                {
                    //DateTime expDate = DateTime.Now;

                    //if (sale.PayTerm == 1)
                    //{
                    //    expDate = sale.SalesDate.AddDays(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 2)
                    //{
                    //    expDate = sale.SalesDate.AddMonths(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 3)
                    //{
                    //    expDate = sale.SalesDate.AddYears(sale.PayTermNo);
                    //}

                    //if (sale.DueDate < DateTime.Now)
                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                    {
                        string query1 = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + sale.SalesId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }
                #endregion

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Payment \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesPaymentCancel(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                {
                    CustomerPaymentId = obj.CustomerPaymentId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var paymentDetails = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a =>
                new
                {
                    a.IsAdvance,
                    a.SalesId,
                    a.Type,
                    a.Amount,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault()
                }).FirstOrDefault();
                string PaymentStatus = ""; int count = 0;

                count = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == paymentDetails.Type && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == paymentDetails.SalesId).Select(b => b.Amount).Count();
                if (count == 0)
                {
                    PaymentStatus = "Due";
                }
                else
                {
                    PaymentStatus = "Partially Paid";
                }

                string query = "";
                //if (paymentDetails.Type.ToLower() == "sales payment")
                //{
                query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + paymentDetails.SalesId;
                if (paymentDetails.PaymentType == "Advance")
                {
                    //var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPaymentDeductionId.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => new
                    //{
                    //    a.CustomerPaymentDeductionId,
                    //    a.DeductedFromId,
                    //    a.Amount
                    //}).ToList();

                    var CustomerPaymentDeductionIds = oConnectionContext.DbClsCustomerPayment.Where(a => a.ParentId == obj.CustomerPaymentId).Select(a => new
                    {
                        a.CustomerPaymentId,
                        a.Amount
                    }).ToList();

                    foreach (var inner in CustomerPaymentDeductionIds)
                    {
                        string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"CustomerPaymentId\"=" + inner.CustomerPaymentId;
                        oConnectionContext.Database.ExecuteSqlCommand(q);

                        //q = "update \"tblCustomerPaymentDeductionId\" set \"IsDeleted\"=True where \"CustomerPaymentDeductionId\"=" + inner.CustomerPaymentDeductionId;
                        //oConnectionContext.Database.ExecuteSqlCommand(q);
                    }

                    long CustomerId = oConnectionContext.DbClsSales.Where(a => a.SalesId == paymentDetails.SalesId).Select(a => a.CustomerId).FirstOrDefault();
                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + paymentDetails.Amount + " where \"UserId\"=" + CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                }
                else
                {
                    if (paymentDetails.IsAdvance == false)
                    {
                        string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                    }
                    else
                    {
                        string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                    }
                }
                oConnectionContext.Database.ExecuteSqlCommand(query);

                #region check OverDue Payment
                var sale = (from a in oConnectionContext.DbClsSales
                                //                         join b in oConnectionContext.DbClsUser
                                //on a.CustomerId equals b.UserId
                            where a.SalesId == paymentDetails.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                            //&& b.PayTermNo != 0
                            select new
                            {
                                a.SalesId,
                                a.SalesDate,
                                a.CustomerId,
                                a.DueDate,
                                //b.PayTerm,
                                //b.PayTermNo
                            }).FirstOrDefault();

                if (sale != null)
                {
                    //DateTime expDate = DateTime.Now;

                    //if (sale.PayTerm == 1)
                    //{
                    //    expDate = sale.SalesDate.AddDays(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 2)
                    //{
                    //    expDate = sale.SalesDate.AddMonths(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 3)
                    //{
                    //    expDate = sale.SalesDate.AddYears(sale.PayTermNo);
                    //}

                    //if (sale.DueDate < DateTime.Now)
                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                    {
                        string query1 = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + sale.SalesId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }
                #endregion

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Payment \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" cancelled",
                    Id = oClsPayment.CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Payment cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> InsertPaymentOnline(ClsCustomerPaymentVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.CompanyId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.CompanyId).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                string PaymentStatus = "";
                decimal GrandTotal = 0;
                long BranchId = 0;
                int IsDebit = 0;

                if (obj.Type.ToLower() == "sales payment")
                {
                    GrandTotal = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.GrandTotal).FirstOrDefault();
                    BranchId = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.BranchId).FirstOrDefault();
                    IsDebit = 2;
                }

                PaymentStatus = "Paid";

                ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    SalesId = obj.SalesId,
                    AttachDocument = obj.AttachDocument,
                    Type = obj.Type,
                    BranchId = BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = IsDebit,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = oCommonController.CreateToken()
                };
                oConnectionContext.DbClsCustomerPayment.Add(oClsPayment);
                oConnectionContext.SaveChanges();

                string query = "";

                if (obj.Type.ToLower() == "sales payment")
                {
                    query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + obj.SalesId;
                }

                if (query != "")
                {
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Online Sales Payment \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == oClsPayment.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
                    Id = oClsPayment.CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Sales Invoice Payment", obj.CompanyId, oClsPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Payment done successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesPaymentReport(ClsCustomerPaymentVm obj)
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

            List<ClsCustomerPaymentVm> det;
            if (obj.BranchId == 0)
            {
                det = (from b in oConnectionContext.DbClsCustomerPayment
                       join a in oConnectionContext.DbClsSales on b.SalesId equals a.SalesId
                       where b.CompanyId == obj.CompanyId && a.Status != "Draft"
                       && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
                       && a.IsDeleted == false && a.IsCancelled == false &&
                       //&& a.BranchId == obj.BranchId
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                       select new ClsCustomerPaymentVm
                       {
                           BranchId = a.BranchId,
                           SalesReferenceNo = a.InvoiceNo,
                           UserGroupId = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.UserGroupId).FirstOrDefault(),
                           CustomerId = a.CustomerId,
                           CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                           CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                           Type = b.Type,
                           SalesId = b.SalesId,
                           CustomerPaymentId = b.CustomerPaymentId,
                           //SalesId = a.SalesId,
                           PaymentDate = b.PaymentDate,
                           Notes = b.Notes,
                           ReferenceNo = b.ReferenceNo,
                           GrandTotal = a.GrandTotal,
                           AddedOn = b.AddedOn,
                           Amount = b.Amount,
                           AttachDocument = b.AttachDocument,
                           PaymentTypeId = b.PaymentTypeId,
                           TotalQuantity = a.TotalQuantity,
                           //       Due = oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "sales payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.SalesId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "sales payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.SalesId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                       }).OrderByDescending(b => b.CustomerPaymentId).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsCustomerPayment
                       join a in oConnectionContext.DbClsSales on b.SalesId equals a.SalesId
                       where b.CompanyId == obj.CompanyId && a.Status != "Draft"
                       && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
                       && a.IsDeleted == false && a.IsCancelled == false
                       && a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                       select new ClsCustomerPaymentVm
                       {
                           BranchId = a.BranchId,
                           SalesReferenceNo = a.InvoiceNo,
                           UserGroupId = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.UserGroupId).FirstOrDefault(),
                           CustomerId = a.CustomerId,
                           CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                           CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                           Type = b.Type,
                           SalesId = b.SalesId,
                           CustomerPaymentId = b.CustomerPaymentId,
                           //SalesId = a.SalesId,
                           PaymentDate = b.PaymentDate,
                           Notes = b.Notes,
                           ReferenceNo = b.ReferenceNo,
                           GrandTotal = a.GrandTotal,
                           AddedOn = b.AddedOn,
                           Amount = b.Amount,
                           AttachDocument = b.AttachDocument,
                           PaymentTypeId = b.PaymentTypeId,
                           TotalQuantity = a.TotalQuantity,
                           //       Due = oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "sales payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.SalesId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "sales payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.SalesId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                       }).OrderByDescending(b => b.CustomerPaymentId).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.UserGroupId != 0)
            {
                det = det.Where(a => a.UserGroupId == obj.UserGroupId).Select(a => a).ToList();
            }

            if (obj.PaymentTypeId != 0)
            {
                det = det.Where(a => a.PaymentTypeId == obj.PaymentTypeId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
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
        public async Task<IHttpActionResult> CustomerPaymentReceipt(ClsCustomerPaymentVm obj)
        {
            var payDetails = oConnectionContext.DbClsCustomerPayment.Where(a => a.ReferenceId == obj.ReferenceId && a.IsActive == true &&
    a.IsDeleted == false && a.IsCancelled == false).Select(a => new { a.CustomerPaymentId, a.CustomerId, a.BranchId, a.CompanyId, a.BookingId }).FirstOrDefault();

            var User = oConnectionContext.DbClsUser.Where(c => c.UserId == payDetails.CustomerId).Select(c => new
            {
                c.Name,
                c.MobileNo,
                c.EmailId,
                TaxNo = c.BusinessRegistrationNo,
                Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == payDetails.CustomerId).Select(b => new
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
            }).FirstOrDefault();

            var Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == payDetails.BranchId).Select(b => new
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
            }).FirstOrDefault();

            var Payments = (oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == payDetails.CustomerPaymentId &&
                a.CompanyId == payDetails.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsCustomerPaymentVm
                {
                    PaymentDate = a.PaymentDate,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == a.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                    ParentId = a.ParentId,
                    Amount = a.Amount,
                    CustomerPaymentId = a.CustomerPaymentId,
                    CustomerId = a.CustomerId,
                    SalesId = a.SalesId,
                    ReferenceNo = a.ReferenceNo,
                    Type = a.Type,
                    InvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.InvoiceNo).FirstOrDefault(),
                    SalesDate = a.Type == "Customer Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.CustomerId).Select(b => b.JoiningDate).FirstOrDefault() :
                    oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.SalesDate).FirstOrDefault(),
                    GrandTotal = a.Type == "Customer Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.CustomerId).Select(b => b.OpeningBalance).FirstOrDefault() :
                    oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.GrandTotal).FirstOrDefault(),
                    Due = a.Type == "Customer Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.CustomerId).Select(b => b.OpeningBalance).FirstOrDefault() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == "customer opening balance payment"
                    && b.IsDeleted == false && b.IsCancelled == false && b.CustomerId == payDetails.CustomerId).Select(b => b.Amount).DefaultIfEmpty().Sum() :
                    oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.GrandTotal).FirstOrDefault() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment")
                    && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                })).Concat(oConnectionContext.DbClsCustomerPayment.Where(a => a.ParentId == payDetails.CustomerPaymentId
            && a.AccountId != 0 &&
                a.CompanyId == payDetails.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsCustomerPaymentVm
                {
                    PaymentDate = a.PaymentDate,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == a.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                    ParentId = a.ParentId,
                    Amount = a.Amount,
                    CustomerPaymentId = a.CustomerPaymentId,
                    CustomerId = a.CustomerId,
                    SalesId = a.SalesId,
                    ReferenceNo = a.ReferenceNo,
                    Type = a.Type,
                    InvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.InvoiceNo).FirstOrDefault(),
                    SalesDate = a.Type == "Customer Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.CustomerId).Select(b => b.JoiningDate).FirstOrDefault() :
                    oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.SalesDate).FirstOrDefault(),
                    GrandTotal = a.Type == "Customer Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.CustomerId).Select(b => b.OpeningBalance).FirstOrDefault() :
                    oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.GrandTotal).FirstOrDefault(),
                    Due = a.Type == "Customer Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.CustomerId).Select(b => b.OpeningBalance).FirstOrDefault() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == "customer opening balance payment"
                    && b.IsDeleted == false && b.IsCancelled == false && b.CustomerId == payDetails.CustomerId).Select(b => b.Amount).DefaultIfEmpty().Sum() :
                    oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.GrandTotal).FirstOrDefault() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment")
                    && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                })).ToList();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == payDetails.CompanyId).Select(a => new
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
                    User = User,
                    Branch = Branch,
                    CustomerPayments = Payments,
                    BusinessSetting = BusinessSetting,
                    BookingId = payDetails != null ? payDetails.BookingId : 0,
                    CompanyId = payDetails != null ? payDetails.CompanyId : 0,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerPaymentJournal(ClsCustomerPaymentVm obj)
        {
            var payments = (oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId &&
             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsCustomerPaymentVm
             {
                 AmountExcTax = a.AmountExcTax,
                 TaxId = a.TaxId,
                 AccountId = a.AccountId,
                 JournalAccountId = a.JournalAccountId,
                 Amount = a.Amount,
                 CustomerPaymentId = a.CustomerPaymentId,
                 CustomerId = a.CustomerId,
                 SalesId = a.SalesId,
                 Type = a.Type,
                 ParentId = a.ParentId
             })).Concat(oConnectionContext.DbClsCustomerPayment.Where(a => a.ParentId == obj.CustomerPaymentId
            && a.AccountId != 0 &&
             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsCustomerPaymentVm
             {
                 AmountExcTax = a.AmountExcTax,
                 TaxId = a.TaxId,
                 AccountId = a.AccountId,
                 JournalAccountId = a.JournalAccountId,
                 Amount = a.Amount,
                 CustomerPaymentId = a.CustomerPaymentId,
                 CustomerId = a.CustomerId,
                 SalesId = a.SalesId,
                 Type = a.Type,
                 ParentId = a.ParentId
             })).ToList();

            List<ClsSalesVm> sales = new List<ClsSalesVm>();

            foreach (var item in payments)
            {
                var taxList = (from q in oConnectionContext.DbClsCustomerPaymentTaxJournal
                               join a in oConnectionContext.DbClsCustomerPayment
                               on q.CustomerPaymentId equals a.CustomerPaymentId
                               where q.CustomerPaymentId == item.CustomerPaymentId
                               && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                               //&& a.TaxAmount != 0
                               select new
                               {
                                   AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                   Debit = q.TaxAmount,
                                   Credit = q.TaxAmount,
                                   AccountId = q.AccountId,
                                   a.ParentId
                               }).ToList();

                sales.Add(new ClsSalesVm
                {
                    Type = (item.Type.ToLower() == "sales payment") ? item.Type +
                    " (Invoice No: " + oConnectionContext.DbClsSales.Where(a => a.SalesId == item.SalesId).Select(a => a.InvoiceNo).FirstOrDefault() + ")" :
                    item.Type,
                    Payments = (from a in payments.Where(b => b.CustomerPaymentId == item.CustomerPaymentId)
                                select new ClsCustomerPaymentVm
                                {
                                    AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                    Debit = a.Type == "Customer Refund" ? 0 : a.ParentId == 0 ? a.Amount : a.AmountExcTax,
                                    Credit = a.Type == "Customer Refund" ? a.Amount : 0
                                }).Concat(from a in payments.Where(b => b.CustomerPaymentId == item.CustomerPaymentId)
                                          select new ClsCustomerPaymentVm
                                          {
                                              AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.JournalAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                              Debit = a.Type == "Customer Refund" ? a.AmountExcTax : 0,
                                              Credit = a.Type == "Customer Refund" ? 0 : a.ParentId == 0 ? a.AmountExcTax : a.Amount
                                          }).Concat(from a in taxList
                                                    select new ClsCustomerPaymentVm
                                                    {
                                                        // tax 
                                                        AccountName = a.AccountName,
                                                        Debit = a.ParentId == 0 ? 0 : a.Debit,
                                                        Credit = a.ParentId == 0 ? a.Credit : 0,
                                                        IsTaxAccount = true
                                                    }).ToList()
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = sales
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DueSummary(ClsCustomerPaymentVm obj)
        {
            decimal AdvanceBalance = 0, Due = 0, OpeningBalance = 0, OpeningBalanceDue = 0, TotalSales = 0, TotalSalesPaid = 0, TotalSalesDue = 0;

            OpeningBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.OpeningBalance).FirstOrDefault();

            OpeningBalanceDue = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.OpeningBalance).FirstOrDefault() -
              oConnectionContext.DbClsCustomerPayment.Where(b => b.Type.ToLower() == "customer opening balance payment" &&
              b.IsDeleted == false && b.IsCancelled == false && b.CustomerId == obj.CustomerId).Select(b => b.Amount).DefaultIfEmpty().Sum();

            TotalSales = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
             && a.Status != "Draft" && a.CustomerId == obj.CustomerId && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId
             && l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
             ).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum();

            TotalSalesPaid = (from c in oConnectionContext.DbClsSales
                              join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                              where c.Status != "Draft" &&
     (d.Type.ToLower() == "sales payment") && c.CustomerId == obj.CustomerId
     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
     && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
     && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                              select d.Amount).DefaultIfEmpty().Sum();

            TotalSalesDue = TotalSales - TotalSalesPaid;

            var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => new
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
                a.TaxExemptionId,
                Due = Due,
                OpeningBalance = OpeningBalance,
                OpeningBalanceDue = OpeningBalanceDue,
                TotalSales = TotalSales,
                TotalSalesPaid = TotalSalesPaid,
                TotalSalesDue = TotalSalesDue
            }).FirstOrDefault();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId,
                StateId = oConnectionContext.DbClsBranch.Where(aa => aa.BranchId == obj.BranchId).Select(aa => aa.StateId).FirstOrDefault()
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = det,
                    BusinessSetting = BusinessSetting
                    //User = new
                    //{
                    //    AdvanceBalance = AdvanceBalance,
                    //    Due = Due,
                    //    OpeningBalance = OpeningBalance,
                    //    OpeningBalanceDue = OpeningBalanceDue,
                    //    TotalSales = TotalSales,
                    //    TotalSalesPaid = TotalSalesPaid,
                    //    TotalSalesDue = TotalSalesDue
                    //}
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UnusedAdvanceBalance(ClsCustomerPaymentVm obj)
        {
            var det = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerId == obj.CustomerId && a.IsDeleted == false && a.IsCancelled == false &&
                            a.IsActive == true && a.Type.ToLower() == "customer payment" && a.AmountRemaining > 0).Select(a => new ClsCustomerPaymentVm
                            {
                                CustomerId = a.CustomerId,
                                AccountId = a.AccountId,
                                Amount = a.Amount,
                                ReferenceNo = a.SalesReturnId == 0 ? a.ReferenceNo : oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.SalesReturnId).Select(b => b.InvoiceNo).FirstOrDefault(),
                                PaymentDate = a.PaymentDate,
                                ParentId = a.ParentId,
                                ParentReferenceNo = oConnectionContext.DbClsCustomerPayment.Where(b => b.CustomerPaymentId == a.ParentId).Select(b => b.ReferenceNo).FirstOrDefault(),
                                CustomerPaymentId = a.CustomerPaymentId,
                                AmountRemaining = a.AmountRemaining
                            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerRefunds(ClsCustomerPaymentVm obj)
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

            var det = oConnectionContext.DbClsCustomerPayment.Where(b => b.CompanyId == obj.CompanyId
            && b.Type.ToLower() == "customer refund"
            && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
            {
                b.SalesId,
                b.SalesReturnId,
                b.IsDirectPayment,
                CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == b.CustomerId).Select(c => c.Name).FirstOrDefault(),
                b.CustomerId,
                InvoiceUrl = oCommonController.webUrl,
                b.ParentId,
                b.ReferenceNo,
                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.CustomerPaymentId,
                b.PaymentDate,
                b.Notes,
                b.Amount,
                b.AttachDocument,
                b.PaymentTypeId,
                PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                b.AddedOn,
                b.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                b.ReferenceId
            }).ToList();

            if (obj.ParentId != 0)
            {
                det = det.Where(a => a.ParentId == obj.ParentId).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).ToList();
            }

            if (obj.SalesReturnId != 0)
            {
                det = det.Where(a => a.SalesReturnId == obj.SalesReturnId).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det.OrderByDescending(a => a.CustomerPaymentId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCustomerRefund(ClsCustomerPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var userDet = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.ParentId
                && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.BranchId,
                    a.CustomerId,
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo,
                    a.TaxId,
                    a.SalesId,
                    a.SalesReturnId,
                    a.AmountRemaining
                }).FirstOrDefault();

                //if (obj.CustomerId == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divCustomer" });
                //    isError = true;
                //}

                if (obj.Amount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                    isError = true;
                }

                if (obj.Amount > 0)
                {
                    if (obj.Amount > userDet.AmountRemaining)
                    {
                        errors.Add(new ClsError { Message = "Amount cannot be more than unused credits", Id = "divAmount" });
                        isError = true;
                    }
                }

                if (obj.PaymentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRefundDate" });
                    isError = true;
                }

                if (obj.PaymentTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
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

                obj.BranchId = userDet.BranchId;
                obj.CustomerId = userDet.CustomerId;

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
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

                //    long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                //&& a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault();

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                long TaxAccountId = 0;

                List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                if (userDet.TaxId != 0)
                {
                    obj.TaxId = userDet.TaxId;
                    TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == userDet.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();
                    obj.AmountExcTax = (100 * (obj.Amount) / (100 +
                        oConnectionContext.DbClsTax.Where(a => a.TaxId == userDet.TaxId).Select(a => a.TaxPercent).FirstOrDefault()));
                    obj.TaxAmount = (obj.Amount - obj.AmountExcTax);

                    //obj.AmountExcTax = obj.Amount;
                    //obj.Amount = obj.Amount + ((oConnectionContext.DbClsTax.Where(a => a.TaxId ==
                    //userDet.TaxId).Select(a => a.TaxPercent).FirstOrDefault() / 100) * obj.Amount);
                    //obj.TaxAmount = (obj.Amount - obj.AmountExcTax);


                    //tax journal
                    var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == obj.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                    List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                    decimal AmountExcTax = obj.AmountExcTax;
                    var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == obj.TaxId
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

                    taxList = finalTaxs.Select(a => new ClsTaxVm
                    {
                        TaxType = "Normal",
                        TaxId = a.TaxId,
                        TaxPercent = a.TaxPercent,
                        TaxAmount = a.TaxAmount,
                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                    }).ToList();

                    //tax journal
                }
                else
                {
                    obj.AmountExcTax = obj.Amount;
                    obj.TaxAmount = 0;
                }

                ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    CustomerId = obj.CustomerId,
                    AttachDocument = obj.AttachDocument,
                    Type = "Customer Refund",
                    BranchId = obj.BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 1,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = oCommonController.CreateToken(),
                    //PaymentIds = _json
                    JournalAccountId = JournalAccountId,
                    AmountRemaining = obj.AmountExcTax,
                    PaymentLinkId = obj.PaymentLinkId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxId = userDet.TaxId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    TaxAccountId = TaxAccountId,
                    AmountExcTax = obj.AmountExcTax,
                    TaxAmount = obj.TaxAmount,
                    ParentId = obj.ParentId,
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

                    oClsCustomerPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment);
                oConnectionContext.SaveChanges();

                long CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId;

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Amount + " where \"UserId\"=" + obj.CustomerId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                query = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + obj.Amount + " where \"CustomerPaymentId\"=" + obj.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                foreach (var taxJournal in taxList)
                {
                    ClsCustomerPaymentTaxJournal oClsCustomerPaymentTaxJournal = new ClsCustomerPaymentTaxJournal()
                    {
                        CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId,
                        TaxId = taxJournal.TaxId,
                        TaxAmount = taxJournal.TaxAmount,
                        AccountId = taxJournal.AccountId,
                        CustomerPaymentTaxJournalType = taxJournal.TaxType
                    };
                    oConnectionContext.DbClsCustomerPaymentTaxJournal.Add(oClsCustomerPaymentTaxJournal);
                    oConnectionContext.SaveChanges();
                }

                if (userDet.SalesReturnId != 0)
                {
                    decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                    == obj.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                    if (AmountRemaining <= 0)
                    {
                        query = "update \"tblSalesReturn\" set \"Status\"='Closed' where \"SalesReturnId\"=" + userDet.SalesReturnId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Refund \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
                    Id = CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (userDet.SalesReturnId != 0)
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Credit Note", obj.CompanyId, oClsCustomerPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Customer Advance Payment", obj.CompanyId, oClsCustomerPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Customer Refund created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RefundDelete(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                {
                    CustomerPaymentId = obj.CustomerPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var userDet = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId
                && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    ParentSalesReturnId = oConnectionContext.DbClsCustomerPayment.Where(b => b.CustomerPaymentId == a.ParentId).Select(b => b.SalesReturnId).FirstOrDefault(),
                    a.CustomerId,
                    a.Amount,
                    a.ParentId
                }).FirstOrDefault();

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + userDet.Amount + " where \"UserId\"=" + userDet.CustomerId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + userDet.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + userDet.Amount + " where \"CustomerPaymentId\"=" + userDet.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(q);

                if (userDet.ParentSalesReturnId != 0)
                {
                    decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                    == userDet.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                    if (AmountRemaining > 0)
                    {
                        query = "update \"tblSalesReturn\" set \"Status\"='Open' where \"SalesReturnId\"=" + userDet.ParentSalesReturnId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Refund \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RefundCancel(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
                {
                    CustomerPaymentId = obj.CustomerPaymentId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var userDet = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId
                && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    ParentSalesReturnId = oConnectionContext.DbClsCustomerPayment.Where(b => b.CustomerPaymentId == a.ParentId).Select(b => b.SalesReturnId).FirstOrDefault(),
                    a.CustomerId,
                    a.Amount,
                    a.ParentId
                }).FirstOrDefault();

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + userDet.Amount + " where \"UserId\"=" + userDet.CustomerId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + userDet.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + userDet.Amount + " where \"CustomerPaymentId\"=" + userDet.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(q);

                if (userDet.ParentSalesReturnId != 0)
                {
                    decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                    == userDet.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                    if (AmountRemaining > 0)
                    {
                        query = "update \"tblSalesReturn\" set \"Status\"='Open' where \"SalesReturnId\"=" + userDet.ParentSalesReturnId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Refund \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.CustomerPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (userDet.ParentSalesReturnId != 0)
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Credit Note", obj.CompanyId, oClsPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Customer Advance Payment", obj.CompanyId, oClsPayment.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Refund deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> RefundCancel(ClsCustomerPaymentVm obj)
        //{
        //    var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
        //    using (TransactionScope dbContextTransaction = new TransactionScope())
        //    {
        //        ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
        //        {
        //            CustomerPaymentId = obj.CustomerPaymentId,
        //            IsCancelled = true,
        //            ModifiedBy = obj.AddedBy,
        //            ModifiedOn = CurrentDate,
        //        };
        //        oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
        //        oConnectionContext.Entry(oClsPayment).Property(x => x.IsCancelled).IsModified = true;
        //        oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
        //        oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
        //        oConnectionContext.SaveChanges();

        //        var userDet = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId
        //        && a.CompanyId == obj.CompanyId).Select(a => new
        //        {
        //            a.CustomerId,
        //            a.Amount,
        //            a.ParentId
        //        }).FirstOrDefault();

        //        string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + userDet.Amount + " where \"UserId\"=" + userDet.CustomerId;
        //        oConnectionContext.Database.ExecuteSqlCommand(query);

        //        string q = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + userDet.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + userDet.Amount + " where \"CustomerPaymentId\"=" + userDet.ParentId;
        //        oConnectionContext.Database.ExecuteSqlCommand(q);

        //        ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
        //        {
        //            AddedBy = obj.AddedBy,
        //            Browser = obj.Browser,
        //            Category = "Customer Refund",
        //            CompanyId = obj.CompanyId,
        //            Description = "Customer Refund \"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" cancelled",
        //            Id = oClsPayment.CustomerPaymentId,
        //            IpAddress = obj.IpAddress,
        //            Platform = obj.Platform,
        //            Type = "Delete"
        //        };
        //        oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

        //        data = new
        //        {
        //            Status = 1,
        //            Message = "Sales Payment cancelled successfully",
        //            Data = new
        //            {
        //            }
        //        };
        //        dbContextTransaction.Complete();
        //    }
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> ApplyCreditsToInvoices(ClsCustomerPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //List<ClsCustomerPaymentIds> oClsCustomerPaymentIds = new List<ClsCustomerPaymentIds>();
                if (obj.CustomerPaymentIds == null)
                {
                    errors.Add(new ClsError { Message = "Add Credits", Id = "divCredits" });
                    isError = true;
                }
                if (obj.PaymentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
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
                var CustomerPayment = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.ParentId).Select(a => new
                {
                    a.BranchId,
                    a.CustomerId,
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo,
                    a.TaxId,
                    a.AmountRemaining,
                    //a.PaymentIds,
                    a.SalesReturnId,
                    a.PlaceOfSupplyId
                }).FirstOrDefault();

                if (obj.CustomerPaymentIds != null)
                {
                    if (obj.CustomerPaymentIds.Sum(a => a.Amount) > CustomerPayment.AmountRemaining)
                    {
                        errors.Add(new ClsError { Message = "Total Credits to Apply cannot be more than Balance", Id = "divCredits" });
                        isError = true;
                    }

                    var Dues = obj.CustomerPaymentIds.Select(a => new
                    {
                        DivId = a.DivId,
                        Type = a.Type,
                        BranchId = CustomerPayment.BranchId,
                        SalesId = a.SalesId,
                        CustomerId = CustomerPayment.CustomerId,
                        Amount = a.Amount,
                        Due = a.Type == "Sales Payment" ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b =>
                        b.GrandTotal - b.WriteOffAmount).FirstOrDefault() -
                    (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false &&
                    b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                    && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()) :
                    oConnectionContext.DbClsUser.Where(d => d.UserId == CustomerPayment.CustomerId).Select(d => d.OpeningBalance).FirstOrDefault() -
                    (from d in oConnectionContext.DbClsCustomerPayment
                     where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == CustomerPayment.CustomerId
                        && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                     select d.Amount).DefaultIfEmpty().Sum()
                    }).ToList();

                    foreach (var item in Dues)
                    {
                        if (item.Amount > item.Due)
                        {
                            errors.Add(new ClsError { Message = "Credits to Apply cannot be more than Due", Id = "divAmount" + item.DivId });
                            isError = true;
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

                    obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();
                    decimal RemainingAmount = CustomerPayment.AmountRemaining;

                    if (Dues != null && Dues.Count() > 0)
                    {
                        long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                        foreach (var item in Dues)
                        {
                            long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
              && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                            if (RemainingAmount > 0)
                            {
                                if (item.Amount != 0)
                                {
                                    //decimal _amount = item.Amount;

                                    long TaxAccountId = 0;

                                    List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                                    if (CustomerPayment.TaxId != 0)
                                    {
                                        obj.TaxId = CustomerPayment.TaxId;
                                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == CustomerPayment.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();
                                        //obj.AmountExcTax = (100 * (_amount) / (100 +
                                        //    oConnectionContext.DbClsTax.Where(a => a.TaxId == CustomerPayment.TaxId).Select(a => a.TaxPercent).FirstOrDefault()));
                                        //obj.TaxAmount = (_amount - obj.AmountExcTax);

                                        obj.AmountExcTax = item.Amount;
                                        obj.Amount = item.Amount + ((oConnectionContext.DbClsTax.Where(a => a.TaxId ==
                                        CustomerPayment.TaxId).Select(a => a.TaxPercent).FirstOrDefault() / 100) * item.Amount);
                                        obj.TaxAmount = (obj.Amount - obj.AmountExcTax);

                                        //tax journal
                                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == obj.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                                        decimal AmountExcTax = obj.AmountExcTax;
                                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => new
                                        {
                                            a.TaxId,
                                            a.Tax,
                                            a.TaxPercent,
                                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                                       where a.TaxId == obj.TaxId
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

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();

                                        //tax journal
                                    }
                                    else
                                    {
                                        obj.AmountExcTax = item.Amount;
                                        obj.TaxAmount = 0;
                                    }

                                    ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        Notes = "",
                                        Amount = item.Amount,
                                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                        PaymentTypeId = obj.PaymentTypeId,
                                        CustomerId = CustomerPayment.CustomerId,
                                        SalesId = item.SalesId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = item.Type,
                                        BranchId = CustomerPayment.BranchId,
                                        AccountId = JournalAccountId,
                                        //ReferenceNo = ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = obj.ParentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = AccountId,
                                        PaymentLinkId = obj.PaymentLinkId,
                                        PlaceOfSupplyId = CustomerPayment.PlaceOfSupplyId,
                                        TaxId = CustomerPayment.TaxId,
                                        IsBusinessRegistered = CustomerPayment.IsBusinessRegistered,
                                        GstTreatment = CustomerPayment.GstTreatment,
                                        BusinessRegistrationNameId = CustomerPayment.BusinessRegistrationNameId,
                                        BusinessRegistrationNo = CustomerPayment.BusinessRegistrationNo,
                                        BusinessLegalName = CustomerPayment.BusinessLegalName,
                                        BusinessTradeName = CustomerPayment.BusinessTradeName,
                                        PanNo = CustomerPayment.PanNo,
                                        TaxAccountId = TaxAccountId,
                                        AmountExcTax = obj.AmountExcTax,
                                        TaxAmount = obj.TaxAmount,
                                    };
                                    oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment1);
                                    oConnectionContext.SaveChanges();

                                    //oClsCustomerPaymentIds.Add(new ClsCustomerPaymentIds { CustomerPaymentId = oClsCustomerPayment1.CustomerPaymentId, CustomerId = item.CustomerId, SalesId = item.SalesId, Type = item.Type, Amount = _amount });

                                    RemainingAmount = RemainingAmount - item.Amount;

                                    if (item.Type.ToLower() == "sales payment")
                                    {
                                        string PaymentStatus = "";
                                        decimal GrandTotal = oConnectionContext.DbClsSales.Where(a => a.SalesId == item.SalesId).Select(a => a.GrandTotal).FirstOrDefault();

                                        decimal previousPayments = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") &&
                                       b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                                        if (GrandTotal == (previousPayments))
                                        {
                                            PaymentStatus = "Paid";
                                        }
                                        else if (GrandTotal > (previousPayments))
                                        {
                                            PaymentStatus = "Partially Paid";
                                        }

                                        string query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    foreach (var taxJournal in taxList)
                                    {
                                        ClsCustomerPaymentTaxJournal oClsCustomerPaymentTaxJournal = new ClsCustomerPaymentTaxJournal()
                                        {
                                            CustomerPaymentId = oClsCustomerPayment1.CustomerPaymentId,
                                            TaxId = taxJournal.TaxId,
                                            TaxAmount = taxJournal.TaxAmount,
                                            AccountId = taxJournal.AccountId,
                                            CustomerPaymentTaxJournalType = taxJournal.TaxType
                                        };
                                        oConnectionContext.DbClsCustomerPaymentTaxJournal.Add(oClsCustomerPaymentTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }

                                    if (CustomerPayment.SalesReturnId != 0)
                                    {
                                        string[] arr = oNotificationTemplatesController.SendNotifications("Credits Applied From Credit Note", obj.CompanyId, oClsCustomerPayment1.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                                    }
                                    else
                                    {
                                        string[] arr = oNotificationTemplatesController.SendNotifications("Credits Applied From Customer Advance Payment", obj.CompanyId, oClsCustomerPayment1.CustomerPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                                    }
                                }
                            }
                        }

                        string r = "update \"tblCustomerPayment\" set \"AmountRemaining\"=" + RemainingAmount + ",\"AmountUsed\"=\"Amount\"-" + RemainingAmount + " where \"CustomerPaymentId\"=" + obj.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(r);

                        r = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + Dues.Select(a => a.Amount).DefaultIfEmpty().Sum() + " where \"UserId\"=" + CustomerPayment.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(r);
                    }

                    if (CustomerPayment.SalesReturnId != 0)
                    {
                        decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                        == obj.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                        if (AmountRemaining <= 0)
                        {
                            string query = "update \"tblSalesReturn\" set \"Status\"='Closed' where \"SalesReturnId\"=" + CustomerPayment.SalesReturnId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                    }

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Customer Payment",
                        CompanyId = obj.CompanyId,
                        Description = "Credits Applied To Invoices\"" + oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId == obj.ParentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
                        Id = obj.ParentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                }

                data = new
                {
                    Status = 1,
                    Message = "Credits applied to invoice(s) successfully",
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
