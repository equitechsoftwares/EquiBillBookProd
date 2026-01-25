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
    public class SupplierPaymentController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllSupplierPayments(ClsSupplierPaymentVm obj)
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

            var det = oConnectionContext.DbClsSupplierPayment.Where(b => b.CompanyId == obj.CompanyId
            //&& b.Type.ToLower() == obj.Type.ToLower() 
            && b.ParentId == 0 && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == 0
            && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate).Select(b => new
            {
                b.BranchId,
                b.AmountRemaining,
                b.IsDirectPayment,
                SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == b.SupplierId).Select(c => c.Name).FirstOrDefault(),
                SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == b.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                b.SupplierId,
                InvoiceUrl = oCommonController.webUrl,
                b.ParentId,
                b.ReferenceNo,
                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.SupplierPaymentId,
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
                b.IsReverseCharge,
                b.IsAdvance
            }).ToList();

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupplierPayments = det.OrderByDescending(a => a.SupplierPaymentId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierPayment(ClsSupplierPaymentVm obj)
        {
            var det = oConnectionContext.DbClsSupplierPayment.Where(b => b.SupplierPaymentId == obj.SupplierPaymentId
           && b.CompanyId == obj.CompanyId).Select(b => new
           {
               b.IsAdvance,
               b.IsReverseCharge,
               b.SourceOfSupplyId,
               b.DestinationOfSupplyId,
               b.TaxId,
               b.PurchaseId,
               b.PurchaseReturnId,
               b.SupplierId,
               b.AmountRemaining,
               b.IsDirectPayment,
               b.AccountId,
               b.ParentId,
               b.ReferenceNo,
               PurchaseReturnReferenceNo = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseReturnId == b.PurchaseReturnId).Select(c => c.InvoiceNo).FirstOrDefault(),
               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
               b.SupplierPaymentId,
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
               SupplierPaymentIds = oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId == b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false).Select(c => new
               {
                   c.ParentId,
                   c.IsCancelled,
                   c.ReferenceNo,
                   c.SupplierPaymentId,
                   c.PaymentDate,
                   PaymentType = oConnectionContext.DbClsPaymentType.Where(d => d.PaymentTypeId == c.PaymentTypeId).Select(d => d.PaymentType).FirstOrDefault(),
                   c.Amount,
                   c.PurchaseId,
                   PurchaseDate = oConnectionContext.DbClsPurchase.Where(d => d.PurchaseId == c.PurchaseId).Select(d => d.PurchaseDate).FirstOrDefault(),
                   c.Type,
                   InvoiceNo = oConnectionContext.DbClsPurchase.Where(d => d.PurchaseId == c.PurchaseId).Select(d => d.ReferenceNo).FirstOrDefault(),
                   GrandTotal = oConnectionContext.DbClsPurchase.Where(d => d.PurchaseId == c.PurchaseId).Select(d => d.GrandTotal).FirstOrDefault(),
                   OpeningBalance = oConnectionContext.DbClsUser.Where(d => d.UserId == c.SupplierId).Select(d => d.OpeningBalance).FirstOrDefault()
               }).Union(
                   oConnectionContext.DbClsSupplierPayment.Where(c => c.SupplierPaymentId == b.SupplierPaymentId && c.IsDirectPayment == true).Select(c => new
                   {
                       c.ParentId,
                       c.IsCancelled,
                       c.ReferenceNo,
                       c.SupplierPaymentId,
                       c.PaymentDate,
                       PaymentType = oConnectionContext.DbClsPaymentType.Where(d => d.PaymentTypeId == c.PaymentTypeId).Select(d => d.PaymentType).FirstOrDefault(),
                       c.Amount,
                       c.PurchaseId,
                       PurchaseDate = oConnectionContext.DbClsPurchase.Where(d => d.PurchaseId == c.PurchaseId).Select(d => d.PurchaseDate).FirstOrDefault(),
                       c.Type,
                       InvoiceNo = oConnectionContext.DbClsPurchase.Where(d => d.PurchaseId == c.PurchaseId).Select(d => d.ReferenceNo).FirstOrDefault(),
                       GrandTotal = oConnectionContext.DbClsPurchase.Where(d => d.PurchaseId == c.PurchaseId).Select(d => d.GrandTotal).FirstOrDefault(),
                       OpeningBalance = oConnectionContext.DbClsUser.Where(d => d.UserId == c.SupplierId).Select(d => d.OpeningBalance).FirstOrDefault()
                   }))
           }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupplierPayment = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> InsertSupplierPayment(ClsSupplierPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.SupplierId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
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

                if (obj.SupplierPaymentIds != null)
                {
                    obj.SupplierPaymentIds = obj.SupplierPaymentIds.Where(a => a.Amount > 0).ToList();
                    foreach (var item in obj.SupplierPaymentIds)
                    {
                        if (item.Due < item.Amount)
                        {
                            errors.Add(new ClsError { Message = "Amount received cannot be more than due", Id = "divAmount" + item.PurchaseId });
                            isError = true;
                        }
                    }

                    if (obj.Amount != 0)
                    {
                        if (obj.Amount < obj.SupplierPaymentIds.Select(a => a.Amount).DefaultIfEmpty().Sum())
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

                long SupplierPaymentId = 0;

                List<ClsSupplierPaymentIds> oClsSupplierPaymentIds = new List<ClsSupplierPaymentIds>();
                decimal RemainingAmount = obj.Amount;

                if (obj.SupplierPaymentIds != null && obj.SupplierPaymentIds.Count() > 0)
                {
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
           && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                    var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                    {
                        a.IsBusinessRegistered,
                        a.GstTreatment,
                        a.BusinessRegistrationNameId,
                        a.BusinessRegistrationNo,
                        a.BusinessLegalName,
                        a.BusinessTradeName,
                        a.PanNo,
                        a.SourceOfSupplyId,
                    }).FirstOrDefault();

                    ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
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
                        SupplierId = obj.SupplierId,
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
                        TaxId = obj.TaxId,
                        TaxAccountId = 0,
                        AmountExcTax = obj.AmountExcTax,
                        TaxAmount = obj.TaxAmount,
                        AmountRemaining = obj.Amount - obj.SupplierPaymentIds.Sum(a => a.Amount),
                        AmountUsed = obj.SupplierPaymentIds.Sum(a => a.Amount),
                        IsReverseCharge = obj.IsReverseCharge,
                        PrefixId = PrefixId,
                        SourceOfSupplyId = userDet.SourceOfSupplyId,
                        DestinationOfSupplyId = obj.DestinationOfSupplyId,
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

                        oClsSupplierPayment.AttachDocument = filepathPass;
                    }
                    oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment);
                    oConnectionContext.SaveChanges();

                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + (obj.Amount - obj.SupplierPaymentIds.Sum(a => a.Amount)) + " where \"UserId\"=" + obj.SupplierId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);

                    SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId;

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    foreach (var item in obj.SupplierPaymentIds)
                    {
                        long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
          && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                        if (RemainingAmount > 0)
                        {
                            if (item.Due != 0)
                            {
                                decimal _amount = 0;
                                _amount = item.Amount;

                                if (item.Type.ToLower() == "supplier opening balance payment")
                                {
                                    ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
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
                                        SupplierId = obj.SupplierId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = item.Type,
                                        BranchId = obj.BranchId,
                                        AccountId = JournalAccountId,
                                        //ReferenceNo = ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = oClsSupplierPayment.SupplierPaymentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = AccountId,
                                        TaxId = obj.TaxId,
                                        TaxAccountId = 0,
                                        AmountExcTax = _amount,
                                        TaxAmount = 0,
                                        IsReverseCharge = obj.IsReverseCharge
                                    };
                                    oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment1);
                                    oConnectionContext.SaveChanges();

                                    oClsSupplierPaymentIds.Add(new ClsSupplierPaymentIds { SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId, SupplierId = item.SupplierId, PurchaseId = item.PurchaseId, Type = item.Type, Amount = _amount });
                                }
                                else
                                {
                                    ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
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
                                        SupplierId = obj.SupplierId,
                                        PurchaseId = item.PurchaseId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = item.Type,
                                        BranchId = obj.BranchId,
                                        AccountId = JournalAccountId,
                                        //ReferenceNo = ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = oClsSupplierPayment.SupplierPaymentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = AccountId,
                                        TaxId = obj.TaxId,
                                        TaxAccountId = 0,
                                        AmountExcTax = _amount,
                                        TaxAmount = 0,
                                        IsReverseCharge = obj.IsReverseCharge
                                    };
                                    oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment1);
                                    oConnectionContext.SaveChanges();

                                    oClsSupplierPaymentIds.Add(new ClsSupplierPaymentIds { SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId, SupplierId = item.SupplierId, PurchaseId = item.PurchaseId, Type = item.Type, Amount = _amount });
                                }

                                RemainingAmount = RemainingAmount - _amount;

                                if (item.Type == "Purchase Payment")
                                {
                                    string PaymentStatus = "";
                                    decimal GrandTotal = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == item.PurchaseId).Select(a => a.GrandTotal).FirstOrDefault();

                                    decimal previousPayments = oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") &&
                                   b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                                    if (GrandTotal == (previousPayments))
                                    {
                                        PaymentStatus = "Paid";
                                    }
                                    else if (GrandTotal > (previousPayments))
                                    {
                                        PaymentStatus = "Partially Paid";
                                    }

                                    string query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + item.PurchaseId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);
                                }
                            }
                        }
                    }

                    serializer.MaxJsonLength = 2147483644;
                    string _json = serializer.Serialize(oClsSupplierPaymentIds);

                    //string r = "update \"tblSupplierPayment\" set \"PaymentIds\"='" + _json + "' where \"SupplierPaymentId\"=" + oClsSupplierPayment.SupplierPaymentId;
                    //oConnectionContext.Database.ExecuteSqlCommand(r);
                    //}

                    string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsSupplierPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    //Supplier Advance Payment
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
                && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                    long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                    var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                    {
                        a.IsBusinessRegistered,
                        a.GstTreatment,
                        a.BusinessRegistrationNameId,
                        a.BusinessRegistrationNo,
                        a.BusinessLegalName,
                        a.BusinessTradeName,
                        a.PanNo,
                        CountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.AddedBy).Select(b => b.CountryId).FirstOrDefault(),
                    }).FirstOrDefault();

                    //tax journal

                    string AccountType = "";

                    var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == obj.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                    List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                    //decimal AmountExcTax = obj.IsReverseCharge == 1 ? obj.Amount : obj.AmountExcTax;
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

                    if (userDet.CountryId == 2)
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
                            taxList = finalTaxs.Select(a => new ClsTaxVm
                            {
                                TaxType = "Normal",
                                TaxId = a.TaxId,
                                TaxPercent = a.TaxPercent,
                                TaxAmount = a.TaxAmount,
                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                            }).ToList();
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
                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                        }).ToList();
                    }

                    //tax journal

                    ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
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
                        SupplierId = obj.SupplierId,
                        AttachDocument = obj.AttachDocument,
                        //Type = obj.Type == "Supplier Direct Advance Payment" ? "Supplier Direct Advance Payment" : "Supplier Advance Payment",
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
                        SourceOfSupplyId = obj.SourceOfSupplyId,
                        DestinationOfSupplyId = obj.DestinationOfSupplyId,
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
                        IsReverseCharge = obj.IsReverseCharge,
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

                        oClsSupplierPayment.AttachDocument = filepathPass;
                    }
                    oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment);
                    oConnectionContext.SaveChanges();

                    SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId;

                    string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + obj.Amount + " where \"UserId\"=" + obj.SupplierId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    if (AccountType != "")
                    {
                        var AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                        && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                        ClsSupplierPaymentTaxJournal oClsSupplierPaymentTaxJournal = new ClsSupplierPaymentTaxJournal()
                        {
                            SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId,
                            TaxId = obj.TaxId,
                            TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : obj.TaxAmount,
                            AccountId = AccountId,
                            SupplierPaymentTaxJournalType = "Normal"
                        };
                        oConnectionContext.DbClsSupplierPaymentTaxJournal.Add(oClsSupplierPaymentTaxJournal);
                        oConnectionContext.SaveChanges();
                    }

                    foreach (var taxJournal in taxList)
                    {
                        ClsSupplierPaymentTaxJournal oClsSupplierPaymentTaxJournal = new ClsSupplierPaymentTaxJournal()
                        {
                            SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId,
                            TaxId = taxJournal.TaxId,
                            TaxAmount = taxJournal.TaxAmount,
                            AccountId = taxJournal.AccountId,
                            SupplierPaymentTaxJournalType = taxJournal.TaxType
                        };
                        oConnectionContext.DbClsSupplierPaymentTaxJournal.Add(oClsSupplierPaymentTaxJournal);
                        oConnectionContext.SaveChanges();
                    }

                    string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsSupplierPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Supplier Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Supplier Payment \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
                    Id = SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Supplier Payment created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierPaymentDelete(ClsSupplierPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var SupplierPayment = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => new
                {
                    a.Type,
                    a.Amount,
                    a.SupplierId,
                    a.PurchaseId,
                    a.AmountRemaining
                }).FirstOrDefault();

                if(SupplierPayment!= null)
                {
                    if (SupplierPayment.Type.ToLower() == "supplier payment")
                    {
                        //if (oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == obj.SupplierPaymentId).Count() == 1)
                        //{
                        //    data = new
                        //    {
                        //        Status = 0,
                        //        Message = "Cannot Delete.. Some of the amount is already used in Purchase Payment",
                        //        Data = new
                        //        {
                        //        }
                        //    };
                        //    return await Task.FromResult(Ok(data));
                        //}
                        //if (oConnectionContext.DbClsSupplierPayment.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.ParentId == obj.SupplierPaymentId).Count() == 1)
                        //{
                        //    data = new
                        //    {
                        //        Status = 0,
                        //        Message = "Cannot Delete.. Some of the amount is already used in Purchase Payment",
                        //        Data = new
                        //        {
                        //        }
                        //    };
                        //    return await Task.FromResult(Ok(data));
                        //}
                        //else
                        if (oConnectionContext.DbClsSupplierPayment.Where(a => a.ParentId == obj.SupplierPaymentId
                        && a.IsDeleted == false && a.IsCancelled == false && a.Type.ToLower() == "supplier refund").Select(a => a.SupplierPaymentId).Count() > 0)
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
                            string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + SupplierPayment.AmountRemaining + " where \"UserId\"=" + SupplierPayment.SupplierId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                        }
                    }

                    ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
                    {
                        SupplierPaymentId = obj.SupplierPaymentId,
                        IsDeleted = true,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment);
                    oConnectionContext.Entry(oClsSupplierPayment).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(oClsSupplierPayment).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsSupplierPayment).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    decimal UsedAmount = 0;
                    //string paymentIds = oConnectionContext.DbClsSupplierPayment.
                    //                    Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.PaymentIds).FirstOrDefault() ?? "[]";
                    //List<ClsSupplierPaymentIds> _paymentIds = serializer.Deserialize<List<ClsSupplierPaymentIds>>(paymentIds);

                    List<ClsSupplierPaymentIds> _paymentIds = oConnectionContext.DbClsSupplierPayment.Where(a =>
                    a.ParentId == obj.SupplierPaymentId && a.Type != "Supplier Refund").Select(a => new ClsSupplierPaymentIds
                    {
                        SupplierPaymentId = a.SupplierPaymentId,
                        Type = a.Type,
                        PurchaseId = a.PurchaseId
                    }).ToList();

                    if (_paymentIds != null)
                    {
                        foreach (var item in _paymentIds)
                        {
                            //if(item.Type.ToLower() == "supplier refund")
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
                            if (item.Type.ToLower() == "supplier opening balance payment")
                            {
                                ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                                {
                                    SupplierPaymentId = item.SupplierPaymentId,
                                    IsDeleted = true,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                };
                                oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                                oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                            //else if (item.Type.ToLower() == "supplier advance payment")
                            //{
                            //    if (oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == item.SupplierPaymentId).Count() == 1)
                            //    {
                            //        data = new
                            //        {
                            //            Status = 0,
                            //            Message = "Cannot Delete.. Some of the amount is already used in Purchase Payment",
                            //            Data = new
                            //            {
                            //            }
                            //        };
                            //        return await Task.FromResult(Ok(data));
                            //    }
                            //    ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                            //    {
                            //        SupplierPaymentId = item.SupplierPaymentId,
                            //        IsDeleted = true,
                            //        ModifiedBy = obj.AddedBy,
                            //        ModifiedOn = CurrentDate,
                            //    };
                            //    oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                            //    oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                            //    oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            //    oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            //    oConnectionContext.SaveChanges();

                            //    string query1 = "update tblUser set AdvanceBalance=AdvanceBalance,0)-" + item.Amount + " where Userid=" + item.SupplierId;
                            //    oConnectionContext.Database.ExecuteSqlCommand(query1);
                            //}
                            else
                            {
                                ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                                {
                                    SupplierPaymentId = item.SupplierPaymentId,
                                    IsDeleted = true,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                };
                                oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                                oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }

                            UsedAmount = UsedAmount + item.Amount;

                            string query = "", PaymentStatus = "";

                            if (item.Type.ToLower() == "purchase payment")
                            {
                                int count = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == item.Type && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).Count();
                                if (count == 0)
                                {
                                    PaymentStatus = "Due";
                                }
                                else
                                {
                                    PaymentStatus = "Partially Paid";
                                }

                                query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + item.PurchaseId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                #region check OverDue Payment
                                var sale = (from a in oConnectionContext.DbClsPurchase
                                                //                         join b in oConnectionContext.DbClsUser
                                                //on a.SupplierId equals b.UserId
                                            where a.PurchaseId == item.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                            && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                            //&& b.PayTermNo != 0
                                            select new
                                            {
                                                a.PurchaseId,
                                                a.PurchaseDate,
                                                a.SupplierId,
                                                a.DueDate
                                                //b.PayTerm,
                                                //b.PayTermNo
                                            }).FirstOrDefault();

                                if (sale != null)
                                {
                                    DateTime expDate = DateTime.Now;

                                    //if (sale.PayTerm == 1)
                                    //{
                                    //    expDate = sale.PurchaseDate.AddDays(sale.PayTermNo);
                                    //}
                                    //else if (sale.PayTerm == 2)
                                    //{
                                    //    expDate = sale.PurchaseDate.AddMonths(sale.PayTermNo);
                                    //}
                                    //else if (sale.PayTerm == 3)
                                    //{
                                    //    expDate = sale.PurchaseDate.AddYears(sale.PayTermNo);
                                    //}

                                    //if (sale.DueDate < DateTime.Now)
                                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                                    {
                                        string query1 = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + item.PurchaseId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                                    }
                                }
                                #endregion
                            }
                        }
                    }

                    // var paymentDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a =>
                    //new
                    //{
                    //    a.SupplierId,
                    //    a.PurchaseId,
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
                        Category = "Supplier Payment",
                        CompanyId = obj.CompanyId,
                        Description = "Supplier Payment \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                        Id = oClsSupplierPayment.SupplierPaymentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Delete"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }                

                data = new
                {
                    Status = 1,
                    Message = "Supplier Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierPaymentCancel(ClsSupplierPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var SupplierPayment = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => new
                {
                    a.Type,
                    a.Amount,
                    a.SupplierId,
                    a.PurchaseId,
                    a.AmountRemaining,
                    a.IsAdvance
                }).FirstOrDefault();

                //if (SupplierPayment.Type.ToLower() == "supplier direct advance payment" || SupplierPayment.Type.ToLower() == "supplier advance payment")
                if (SupplierPayment.Type.ToLower() == "supplier payment")
                {
                    //if (oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == obj.SupplierPaymentId).Count() == 1)
                    //{
                    //    data = new
                    //    {
                    //        Status = 0,
                    //        Message = "Cannot Delete.. Some of the amount is already used in Purchase Payment",
                    //        Data = new
                    //        {
                    //        }
                    //    };
                    //    return await Task.FromResult(Ok(data));
                    //}
                    if (oConnectionContext.DbClsSupplierPayment.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.ParentId == obj.SupplierPaymentId).Count() == 1)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Cannot Delete.. Some of the amount is already used in Purchase Payment",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    else if (oConnectionContext.DbClsSupplierPayment.Where(a => a.ParentId == obj.SupplierPaymentId
                    && a.IsDeleted == false && a.IsCancelled == false && a.Type.ToLower() == "supplier refund").Select(a => a.SupplierPaymentId).Count() > 0)
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
                        string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + SupplierPayment.AmountRemaining + " where \"UserId\"=" + SupplierPayment.SupplierId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
                {
                    SupplierPaymentId = obj.SupplierPaymentId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment);
                oConnectionContext.Entry(oClsSupplierPayment).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsSupplierPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSupplierPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                decimal UsedAmount = 0;
                string paymentIds = oConnectionContext.DbClsSupplierPayment.
                                    Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.PaymentIds).FirstOrDefault() ?? "[]";
                List<ClsSupplierPaymentIds> _paymentIds = serializer.Deserialize<List<ClsSupplierPaymentIds>>(paymentIds);

                if (_paymentIds != null)
                {
                    foreach (var item in _paymentIds)
                    {
                        //if(item.Type.ToLower() == "supplier refund")
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
                        if (item.Type.ToLower() == "supplier opening balance payment")
                        {
                            ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                            {
                                SupplierPaymentId = item.SupplierPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                        //else if (item.Type.ToLower() == "supplier advance payment")
                        //{
                        //    if (oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.DeductedFromId == item.SupplierPaymentId).Count() == 1)
                        //    {
                        //        data = new
                        //        {
                        //            Status = 0,
                        //            Message = "Cannot Delete.. Some of the amount is already used in Purchase Payment",
                        //            Data = new
                        //            {
                        //            }
                        //        };
                        //        return await Task.FromResult(Ok(data));
                        //    }
                        //    ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                        //    {
                        //        SupplierPaymentId = item.SupplierPaymentId,
                        //        IsDeleted = true,
                        //        ModifiedBy = obj.AddedBy,
                        //        ModifiedOn = CurrentDate,
                        //    };
                        //    oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                        //    oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                        //    oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                        //    oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                        //    oConnectionContext.SaveChanges();

                        //    string query1 = "update tblUser set AdvanceBalance=AdvanceBalance,0)-" + item.Amount + " where Userid=" + item.SupplierId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query1);
                        //}
                        else
                        {
                            ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                            {
                                SupplierPaymentId = item.SupplierPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }

                        UsedAmount = UsedAmount + item.Amount;

                        string query = "", PaymentStatus = "";

                        if (item.Type.ToLower() == "purchase payment")
                        {
                            int count = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == item.Type && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).Count();
                            if (count == 0)
                            {
                                PaymentStatus = "Due";
                            }
                            else
                            {
                                PaymentStatus = "Partially Paid";
                            }

                            query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + item.PurchaseId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            #region check OverDue Payment
                            var sale = (from a in oConnectionContext.DbClsPurchase
                                            //                         join b in oConnectionContext.DbClsUser
                                            //on a.SupplierId equals b.UserId
                                        where a.PurchaseId == item.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                        && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                        //&& b.PayTermNo != 0
                                        select new
                                        {
                                            a.PurchaseId,
                                            a.PurchaseDate,
                                            a.SupplierId,
                                            a.DueDate
                                            //b.PayTerm,
                                            //b.PayTermNo
                                        }).FirstOrDefault();

                            if (sale != null)
                            {
                                DateTime expDate = DateTime.Now;

                                //if (sale.PayTerm == 1)
                                //{
                                //    expDate = sale.PurchaseDate.AddDays(sale.PayTermNo);
                                //}
                                //else if (sale.PayTerm == 2)
                                //{
                                //    expDate = sale.PurchaseDate.AddMonths(sale.PayTermNo);
                                //}
                                //else if (sale.PayTerm == 3)
                                //{
                                //    expDate = sale.PurchaseDate.AddYears(sale.PayTermNo);
                                //}

                                //if (sale.DueDate < DateTime.Now)
                                if ((DateTime.Now - sale.DueDate).Days >= 1)
                                {
                                    string query1 = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + item.PurchaseId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                }
                            }
                            #endregion
                        }
                    }
                }

                // var paymentDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a =>
                //new
                //{
                //    a.SupplierId,
                //    a.PurchaseId,
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
                    Category = "Supplier Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Supplier Payment \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" cancelled",
                    Id = oClsSupplierPayment.SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (SupplierPayment.IsAdvance == false)
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsSupplierPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsSupplierPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Supplier Payment cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> InsertPurchasePayment(ClsSupplierPaymentVm obj)
        {
            if (obj.CompanyId == 0)
            {
                obj.CompanyId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.CompanyId).FirstOrDefault();
                obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId
                && a.IsPaymentGateway == true).Select(a => a.PaymentTypeId).FirstOrDefault();
            }

            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                string PaymentStatus = "";
                decimal GrandTotal = 0;
                decimal previousPayments = 0;
                int IsDebit = 0;

                if (obj.PaymentType == "Advance")
                {
                    if (obj.SupplierPaymentIds != null && obj.SupplierPaymentIds.Count > 0)
                    {
                        obj.SupplierPaymentIds = obj.SupplierPaymentIds.Where(a => a.Amount > 0).DefaultIfEmpty().ToList();
                        
                        // Validate PaymentDate for credit application
                        if (obj.PaymentDate == DateTime.MinValue)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divCreditsAppliedDate" });
                            isError = true;
                        }
                        
                        foreach (var item in obj.SupplierPaymentIds)
                        {
                            if (item.AmountRemaining < item.Amount)
                            {
                                errors.Add(new ClsError { Message = "Amount received cannot be more than Amount Remaining", Id = "divAmount" + item.SupplierPaymentId });
                                isError = true;
                            }
                        }

                        obj.Amount = obj.SupplierPaymentIds.Select(a => a.Amount).DefaultIfEmpty().Sum();
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

                List<ClsSupplierPaymentVm> oAdvanceBalances = new List<ClsSupplierPaymentVm>();

                obj.SupplierId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.SupplierId).FirstOrDefault();
                if (obj.PaymentType == "" || obj.PaymentType == null)
                {
                    obj.PaymentType = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.PaymentTypeId == obj.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault();
                }
                else
                {
                    obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();
                }

                //if (obj.PaymentType == "Advance")
                //{
                //    if (obj.Type.ToLower() == "purchase payment")
                //    {
                //        decimal AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.AdvanceBalance).FirstOrDefault();
                //        if (AdvanceBalance < obj.Amount)
                //        {
                //            errors.Add(new ClsError { Message = "Insuffcient Advance Balance", Id = "divAmount" });
                //            isError = true;
                //        }
                //    }
                //}

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

                GrandTotal = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.GrandTotal).FirstOrDefault();
                obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.BranchId).FirstOrDefault();
                IsDebit = 2;

                previousPayments = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == obj.Type && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum();
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

                long SupplierPaymentId = 0;

                if (obj.PaymentType == "Advance")
                {
                    obj.AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                    //decimal amountRemaininToDeduct = obj.Amount;
                    foreach (var item in obj.SupplierPaymentIds)
                    {
                        //var PaymentIds = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => a.PaymentIds).FirstOrDefault();

                        //List<ClsSupplierPaymentIds> oClsSupplierPaymentIds = new List<ClsSupplierPaymentIds>();

                        //if (PaymentIds != null)
                        //{
                            //oClsSupplierPaymentIds = serializer.Deserialize<List<ClsSupplierPaymentIds>>(PaymentIds);
                        //}

                        //decimal availableAmount = item.AmountRemaining;
                        decimal amount = item.Amount;
                        //if (availableAmount >= amountRemaininToDeduct)
                        //{
                        //    amount = amountRemaininToDeduct;
                        //}
                        //else if (availableAmount < amountRemaininToDeduct)
                        //{
                        //    amount = availableAmount;
                        //}
                        ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
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
                            SupplierId = obj.SupplierId,
                            PurchaseId = obj.PurchaseId,
                            AttachDocument = obj.AttachDocument,
                            Type = obj.Type,
                            BranchId = obj.BranchId,
                            AccountId = obj.AccountId,
                            //ReferenceNo = ReferenceNo,
                            IsDebit = IsDebit,
                            //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                            ParentId = item.SupplierPaymentId,
                            ReferenceId = oCommonController.CreateToken(),
                            JournalAccountId = JournalAccountId,
                            SourceOfSupplyId = obj.SourceOfSupplyId,
                            DestinationOfSupplyId = obj.DestinationOfSupplyId,
                            TaxId = 0,
                            TaxAccountId = 0,
                            AmountExcTax = amount,
                            TaxAmount = 0,
                            IsReverseCharge = obj.IsReverseCharge
                        };
                        oConnectionContext.DbClsSupplierPayment.Add(oClsPayment);
                        oConnectionContext.SaveChanges();

                        //oClsSupplierPaymentIds.Add(new ClsSupplierPaymentIds { SupplierPaymentId = oClsPayment.SupplierPaymentId, SupplierId = obj.SupplierId, PurchaseId = obj.PurchaseId, Type = obj.Type, Amount = amount });

                        //serializer.MaxJsonLength = 2147483644;
                        //string _json = serializer.Serialize(oClsSupplierPaymentIds);

                        //string _query1 = "update \"tblSupplierPayment\" set \"PaymentIds\"='" + _json + "',\"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"SupplierPaymentId\"=" + item.SupplierPaymentId;
                        //oConnectionContext.Database.ExecuteSqlCommand(_query1);

                        string _query1 = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + amount + ",\"AmountUsed\"=\"AmountUsed\"+" + amount + " where \"SupplierPaymentId\"=" + item.SupplierPaymentId;
                        oConnectionContext.Database.ExecuteSqlCommand(_query1);

                        //amountRemaininToDeduct = amountRemaininToDeduct - amount;

                        //oAdvanceBalances.Add(new ClsSupplierPaymentVm { PurchaseId = obj.PurchaseId, SupplierPaymentId = item.SupplierPaymentId, Amount = amount, ParentId = item.ParentId });

                        long PurchaseReturnId = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => a.PurchaseReturnId).FirstOrDefault();
                        if (PurchaseReturnId != 0)
                        {
                            decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                            == item.SupplierPaymentId).Select(a => a.AmountRemaining).FirstOrDefault();

                            if (AmountRemaining <= 0)
                            {
                                string query11 = "update \"tblPurchaseReturn\" set \"Status\"='Closed' where \"PurchaseReturnId\"=" + PurchaseReturnId;
                                oConnectionContext.Database.ExecuteSqlCommand(query11);
                            }
                        }
                    }

                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Amount + " where \"UserId\"=" + obj.SupplierId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);

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

                    ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
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
                        PurchaseId = obj.PurchaseId,
                        AttachDocument = obj.AttachDocument,
                        Type = obj.Type,
                        BranchId = obj.BranchId,
                        AccountId = obj.AccountId,
                        ReferenceNo = obj.ReferenceNo,
                        IsDebit = IsDebit,
                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                        ReferenceId = oCommonController.CreateToken(),
                        JournalAccountId = JournalAccountId,
                        SupplierId = obj.SupplierId,
                        IsDirectPayment = true,
                        TaxId = 0,
                        TaxAccountId = 0,
                        AmountExcTax = obj.Amount,
                        TaxAmount = 0,
                        IsReverseCharge = obj.IsReverseCharge,
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

                    SupplierPaymentId = oClsPayment.SupplierPaymentId;

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                string query = "";
                query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + obj.PurchaseId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                #region check OverDue Payment
                if (PaymentStatus != "Paid")
                {
                    //if (obj.Type.ToLower() == "purchase payment")
                    //{
                    var sale = (from a in oConnectionContext.DbClsPurchase
                                    //                         join b in oConnectionContext.DbClsUser
                                    //on a.SupplierId equals b.UserId
                                where a.PurchaseId == obj.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                //&& b.PayTermNo != 0
                                select new
                                {
                                    a.PurchaseId,
                                    a.PurchaseDate,
                                    a.SupplierId,
                                    a.DueDate,
                                    //b.PayTerm,
                                    //b.PayTermNo
                                }).FirstOrDefault();

                    if (sale != null)
                    {
                        //DateTime expDate = DateTime.Now;

                        //if (sale.PayTerm == 1)
                        //{
                        //    expDate = sale.PurchaseDate.AddDays(sale.PayTermNo);
                        //}
                        //else if (sale.PayTerm == 2)
                        //{
                        //    expDate = sale.PurchaseDate.AddMonths(sale.PayTermNo);
                        //}
                        //else if (sale.PayTerm == 3)
                        //{
                        //    expDate = sale.PurchaseDate.AddYears(sale.PayTermNo);
                        //}

                        //if (sale.DueDate < DateTime.Now)
                        if ((DateTime.Now - sale.DueDate).Days >= 1)
                        {
                            string query1 = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + sale.PurchaseId;
                            oConnectionContext.Database.ExecuteSqlCommand(query1);
                        }
                    }
                    //}
                    //  else if (obj.Type.ToLower() == "supplier refund")
                    //  {
                    //      var PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                    //                         join c in oConnectionContext.DbClsPurchase
                    //                         on a.PurchaseId equals c.PurchaseId
                    //                         join b in oConnectionContext.DbClsUser
                    //on c.SupplierId equals b.UserId
                    //                         where a.PurchaseReturnId == obj.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                    //             && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && a.PaymentStatus.ToLower() != "paid"
                    //             && b.PayTermNo != 0
                    //                         select new
                    //                         {
                    //                             a.PurchaseReturnId,
                    //                             a.Date,
                    //                             c.SupplierId,
                    //                             b.PayTerm,
                    //                             b.PayTermNo
                    //                         }).FirstOrDefault();

                    //      if (PurchaseReturn != null)
                    //      {
                    //          DateTime expDate = DateTime.Now;

                    //          if (PurchaseReturn.PayTerm == 1)
                    //          {
                    //              expDate = PurchaseReturn.Date.AddDays(PurchaseReturn.PayTermNo);
                    //          }
                    //          else if (PurchaseReturn.PayTerm == 2)
                    //          {
                    //              expDate = PurchaseReturn.Date.AddMonths(PurchaseReturn.PayTermNo);
                    //          }
                    //          else if (PurchaseReturn.PayTerm == 3)
                    //          {
                    //              expDate = PurchaseReturn.Date.AddYears(PurchaseReturn.PayTermNo);
                    //          }

                    //          if (expDate > DateTime.Now)
                    //          {
                    //              string query1 = "update tblPurchaseReturn set Status='Overdue' where PurchaseReturnId=" + PurchaseReturn.PurchaseReturnId;
                    //              oConnectionContext.Database.ExecuteSqlCommand(query1);
                    //          }
                    //      }
                    //  }
                }
                #endregion


                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Supplier Payment",
                        CompanyId = obj.CompanyId,
                        Description = "Purchase Payment \"" + obj.ReferenceNo + "\" created",
                        Id = SupplierPaymentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }

                data = new
                {
                    Status = 1,
                    Message = "Purchase Payment created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchasePayments(ClsSupplierPaymentVm obj)
        {
            decimal AdvanceBalance = 0, Due = 0;

            long UserId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.SupplierId).FirstOrDefault();
            AdvanceBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == UserId).Select(a => a.AdvanceBalance).FirstOrDefault();

            Due = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                   (from a in oConnectionContext.DbClsPurchase
                    join b in oConnectionContext.DbClsSupplierPayment
        on a.PurchaseId equals b.PurchaseId
                    where a.PurchaseId == obj.PurchaseId && (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false
                    select b.Amount).DefaultIfEmpty().Sum();


            var det = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseId == obj.PurchaseId
            //&& b.Type.ToLower() == obj.Type.ToLower() 
            && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
            {
                b.Type,
                InvoiceUrl = oCommonController.webUrl,
                b.ParentId,
                ParentReferenceNo = oConnectionContext.DbClsSupplierPayment.Where(bb => bb.SupplierPaymentId == b.ParentId).Select(bb => bb.ReferenceNo).FirstOrDefault(),
                b.ReferenceNo,
                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.SupplierPaymentId,
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
                PurchaseReturnId = oConnectionContext.DbClsSupplierPayment.Where(bb => bb.SupplierPaymentId == b.ParentId).Select(bb => bb.PurchaseReturnId).FirstOrDefault(),
                PurchaseReturnInvoiceNo = oConnectionContext.DbClsPurchaseReturn.Where(c => c.PurchaseReturnId ==
                oConnectionContext.DbClsSupplierPayment.Where(bb => bb.SupplierPaymentId == b.ParentId).Select(bb => bb.PurchaseReturnId).FirstOrDefault()).Select(c => c.InvoiceNo).FirstOrDefault(),
                b.IsReverseCharge
            }).OrderByDescending(b => b.SupplierPaymentId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupplierPayments = det,
                    User = new
                    {
                        AdvanceBalance = AdvanceBalance,
                        Due = Due,
                    }
                }
            };
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> PurchasePayment(ClsSupplierPaymentVm obj)
        //{
        //    var det = oConnectionContext.DbClsSupplierPayment.Where(b => b.SupplierPaymentId == obj.SupplierPaymentId && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
        //    {
        //        b.AccountId,
        //        b.ParentId,
        //        b.ReferenceNo,
        //        AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
        //        b.SupplierPaymentId,
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
        //            SupplierPayment = det,
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> PurchasePaymentDelete(ClsSupplierPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                {
                    SupplierPaymentId = obj.SupplierPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var paymentDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a =>
                new
                {
                    a.ParentId,
                    a.PurchaseId,
                    a.Type,
                    a.Amount,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault()
                }).FirstOrDefault();
                string PaymentStatus = ""; int count = 0;

                count = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == paymentDetails.Type && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == paymentDetails.PurchaseId).Select(b => b.Amount).Count();
                if (count == 0)
                {
                    PaymentStatus = "Due";
                }
                else
                {
                    PaymentStatus = "Partially Paid";
                }

                //string query = "";
                //if (paymentDetails.Type.ToLower() == "purchase payment")
                //{
                string query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + paymentDetails.PurchaseId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                //if (paymentDetails.PaymentType == "Advance")
                if (paymentDetails.ParentId != 0)
                {
                    //var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPaymentDeductionId.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => new
                    //{
                    //    a.SupplierPaymentDeductionId,
                    //    a.DeductedFromId,
                    //    a.Amount
                    //}).ToList();

                    var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => new
                    {
                        ParentPurchaseReturnId = oConnectionContext.DbClsSupplierPayment.Where(b => b.SupplierPaymentId == a.ParentId).Select(b => b.PurchaseReturnId).FirstOrDefault(),
                        a.ParentId,
                        a.Amount
                    }).ToList();

                    foreach (var inner in SupplierPaymentDeductionIds)
                    {
                        string q = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"SupplierPaymentId\"=" + inner.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(q);

                        //q = "update \"tblSupplierPaymentDeductionId\" set \"IsDeleted\"=True where \"SupplierPaymentDeductionId\"=" + inner.SupplierPaymentDeductionId;
                        //oConnectionContext.Database.ExecuteSqlCommand(q);

                        if (inner.ParentPurchaseReturnId != 0)
                        {
                            decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                            == inner.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                            if (AmountRemaining > 0)
                            {
                                string query1 = "update \"tblPurchaseReturn\" set \"Status\"='Open' where \"PurchaseReturnId\"=" + inner.ParentPurchaseReturnId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                            }
                        }
                    }

                    long SupplierId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == paymentDetails.PurchaseId).Select(a => a.SupplierId).FirstOrDefault();
                    string query2 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + paymentDetails.Amount + " where \"UserId\"=" + SupplierId;
                    oConnectionContext.Database.ExecuteSqlCommand(query2);
                }

                #region check OverDue Payment
                var sale = (from a in oConnectionContext.DbClsPurchase
                                //                         join b in oConnectionContext.DbClsUser
                                //on a.SupplierId equals b.UserId
                            where a.PurchaseId == paymentDetails.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                            //&& b.PayTermNo != 0
                            select new
                            {
                                a.PurchaseId,
                                a.PurchaseDate,
                                a.SupplierId,
                                a.DueDate,
                                //b.PayTerm,
                                //b.PayTermNo
                            }).FirstOrDefault();

                if (sale != null)
                {
                    //DateTime expDate = DateTime.Now;

                    //if (sale.PayTerm == 1)
                    //{
                    //    expDate = sale.PurchaseDate.AddDays(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 2)
                    //{
                    //    expDate = sale.PurchaseDate.AddMonths(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 3)
                    //{
                    //    expDate = sale.PurchaseDate.AddYears(sale.PayTermNo);
                    //}

                    //if (sale.DueDate < DateTime.Now)
                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                    {
                        string query1 = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + sale.PurchaseId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }
                #endregion

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Supplier Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Payment \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchasePaymentCancel(ClsSupplierPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                {
                    SupplierPaymentId = obj.SupplierPaymentId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var paymentDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a =>
                new
                {
                    a.IsAdvance,
                    a.PurchaseId,
                    a.Type,
                    a.Amount,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(b => b.PaymentTypeId == a.PaymentTypeId).Select(b => b.PaymentType).FirstOrDefault()
                }).FirstOrDefault();
                string PaymentStatus = ""; int count = 0;

                count = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == paymentDetails.Type && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == paymentDetails.PurchaseId).Select(b => b.Amount).Count();
                if (count == 0)
                {
                    PaymentStatus = "Due";
                }
                else
                {
                    PaymentStatus = "Partially Paid";
                }

                string query = "";
                //if (paymentDetails.Type.ToLower() == "purchase payment")
                //{
                query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + paymentDetails.PurchaseId;
                if (paymentDetails.PaymentType == "Advance")
                {
                    var SupplierPaymentDeductionIds = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => new
                    {
                        a.ParentId,
                        a.Amount
                    }).ToList();

                    foreach (var inner in SupplierPaymentDeductionIds)
                    {
                        string q = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + inner.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + inner.Amount + " where \"SupplierPaymentId\"=" + inner.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(q);

                        //q = "update \"tblSupplierPaymentDeductionId\" set \"IsDeleted\"=True where \"SupplierPaymentDeductionId\"=" + inner.SupplierPaymentDeductionId;
                        //oConnectionContext.Database.ExecuteSqlCommand(q);
                    }

                    long SupplierId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == paymentDetails.PurchaseId).Select(a => a.SupplierId).FirstOrDefault();
                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + paymentDetails.Amount + " where \"UserId\"=" + SupplierId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                }
                else
                {
                    if (paymentDetails.IsAdvance == false)
                    {
                        string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                    }
                    else
                    {
                        string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Bill Payment", obj.CompanyId, oClsPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                    }
                }
                oConnectionContext.Database.ExecuteSqlCommand(query);

                #region check OverDue Payment
                var sale = (from a in oConnectionContext.DbClsPurchase
                                //                         join b in oConnectionContext.DbClsUser
                                //on a.SupplierId equals b.UserId
                            where a.PurchaseId == paymentDetails.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                            //&& b.PayTermNo != 0
                            select new
                            {
                                a.PurchaseId,
                                a.PurchaseDate,
                                a.SupplierId,
                                a.DueDate,
                                //b.PayTerm,
                                //b.PayTermNo
                            }).FirstOrDefault();

                if (sale != null)
                {
                    //DateTime expDate = DateTime.Now;

                    //if (sale.PayTerm == 1)
                    //{
                    //    expDate = sale.PurchaseDate.AddDays(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 2)
                    //{
                    //    expDate = sale.PurchaseDate.AddMonths(sale.PayTermNo);
                    //}
                    //else if (sale.PayTerm == 3)
                    //{
                    //    expDate = sale.PurchaseDate.AddYears(sale.PayTermNo);
                    //}

                    //if (sale.DueDate < DateTime.Now)
                    if ((DateTime.Now - sale.DueDate).Days >= 1)
                    {
                        string query1 = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + sale.PurchaseId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }
                #endregion

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Supplier Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Payment \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" cancelled",
                    Id = oClsPayment.SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                

                data = new
                {
                    Status = 1,
                    Message = "Purchase Payment cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchasePaymentReport(ClsSupplierPaymentVm obj)
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

            List<ClsSupplierPaymentVm> det;
            if (obj.BranchId == 0)
            {
                det = (from b in oConnectionContext.DbClsSupplierPayment
                       join a in oConnectionContext.DbClsPurchase on b.PurchaseId equals a.PurchaseId
                       where b.CompanyId == obj.CompanyId && a.Status != "Draft"
                       && (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false
                       && a.IsDeleted == false && a.IsCancelled == false &&
                       //&& a.BranchId == obj.BranchId
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                       select new ClsSupplierPaymentVm
                       {
                           IsReverseCharge = b.IsReverseCharge,
                           BranchId = a.BranchId,
                           PurchaseReferenceNo = a.ReferenceNo,
                           SupplierId = a.SupplierId,
                           SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                           SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                           Type = b.Type,
                           PurchaseId = b.PurchaseId,
                           SupplierPaymentId = b.SupplierPaymentId,
                           //PurchaseId = a.PurchaseId,
                           PaymentDate = b.PaymentDate,
                           Notes = b.Notes,
                           ReferenceNo = b.ReferenceNo,
                           GrandTotal = a.GrandTotal,
                           AddedOn = b.AddedOn,
                           Amount = b.Amount,
                           AttachDocument = b.AttachDocument,
                           PaymentTypeId = b.PaymentTypeId,
                           TotalQuantity = a.TotalQuantity,
                           //       Due = oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "purchase payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "purchase payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.PurchaseId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                       }).OrderByDescending(b => b.SupplierPaymentId).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsSupplierPayment
                       join a in oConnectionContext.DbClsPurchase on b.PurchaseId equals a.PurchaseId
                       where b.CompanyId == obj.CompanyId && a.Status != "Draft"
                       && (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false
                       && a.IsDeleted == false && a.IsCancelled == false
                       && a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                       select new ClsSupplierPaymentVm
                       {
                           IsReverseCharge = b.IsReverseCharge,
                           BranchId = a.BranchId,
                           PurchaseReferenceNo = a.ReferenceNo,
                           SupplierId = a.SupplierId,
                           SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                           SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                           Type = b.Type,
                           PurchaseId = b.PurchaseId,
                           SupplierPaymentId = b.SupplierPaymentId,
                           //PurchaseId = a.PurchaseId,
                           PaymentDate = b.PaymentDate,
                           Notes = b.Notes,
                           ReferenceNo = b.ReferenceNo,
                           GrandTotal = a.GrandTotal,
                           AddedOn = b.AddedOn,
                           Amount = b.Amount,
                           AttachDocument = b.AttachDocument,
                           PaymentTypeId = b.PaymentTypeId,
                           TotalQuantity = a.TotalQuantity,
                           //       Due = oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "purchase payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsPayment.Where(bb => bb.Type.ToLower() == "purchase payment" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.Id == a.PurchaseId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                       }).OrderByDescending(b => b.SupplierPaymentId).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            //if (obj.UserGroupId != 0)
            //{
            //    det = det.Where(a => a.UserGroupId == obj.UserGroupId).Select(a => a).ToList();
            //}

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
                    SupplierPayments = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
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
        public async Task<IHttpActionResult> SupplierPaymentReceipt(ClsSupplierPaymentVm obj)
        {
            var payDetails = oConnectionContext.DbClsSupplierPayment.Where(a => a.ReferenceId == obj.ReferenceId && a.IsActive == true &&
    a.IsDeleted == false && a.IsCancelled == false).Select(a => new { a.SupplierPaymentId, a.SupplierId, a.BranchId, a.CompanyId }).FirstOrDefault();

            var User = oConnectionContext.DbClsUser.Where(c => c.UserId == payDetails.SupplierId).Select(c => new
            {
                c.Name,
                c.MobileNo,
                c.EmailId,
                TaxNo = c.BusinessRegistrationNo,
                Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == payDetails.SupplierId).Select(b => new
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

            var Payments = (oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == payDetails.SupplierPaymentId &&
                a.CompanyId == payDetails.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsSupplierPaymentVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    PaymentDate = a.PaymentDate,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == a.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                    ParentId = a.ParentId,
                    Amount = a.Amount,
                    SupplierPaymentId = a.SupplierPaymentId,
                    SupplierId = a.SupplierId,
                    PurchaseId = a.PurchaseId,
                    ReferenceNo = a.ReferenceNo,
                    Type = a.Type,
                    InvoiceNo = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.ReferenceNo).FirstOrDefault(),
                    PurchaseDate = a.Type == "Supplier Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.SupplierId).Select(b => b.JoiningDate).FirstOrDefault() :
                    oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.PurchaseDate).FirstOrDefault(),
                    GrandTotal = a.Type == "Supplier Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.SupplierId).Select(b => b.OpeningBalance).FirstOrDefault() :
                    oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.GrandTotal).FirstOrDefault(),
                    Due = a.Type == "Supplier Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.SupplierId).Select(b => b.OpeningBalance).FirstOrDefault() -
                    oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier opening balance payment"
                    && b.IsDeleted == false && b.IsCancelled == false && b.SupplierId == payDetails.SupplierId).Select(b => b.Amount).DefaultIfEmpty().Sum() :
                    oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.GrandTotal).FirstOrDefault() -
                    oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment")
                    && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                })).Concat(oConnectionContext.DbClsSupplierPayment.Where(a => a.ParentId == payDetails.SupplierPaymentId
            && a.AccountId != 0 &&
                a.CompanyId == payDetails.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsSupplierPaymentVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    PaymentDate = a.PaymentDate,
                    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == a.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault(),
                    ParentId = a.ParentId,
                    Amount = a.Amount,
                    SupplierPaymentId = a.SupplierPaymentId,
                    SupplierId = a.SupplierId,
                    PurchaseId = a.PurchaseId,
                    ReferenceNo = a.ReferenceNo,
                    Type = a.Type,
                    InvoiceNo = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.ReferenceNo).FirstOrDefault(),
                    PurchaseDate = a.Type == "Supplier Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.SupplierId).Select(b => b.JoiningDate).FirstOrDefault() :
                    oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.PurchaseDate).FirstOrDefault(),
                    GrandTotal = a.Type == "Supplier Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.SupplierId).Select(b => b.OpeningBalance).FirstOrDefault() :
                    oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.GrandTotal).FirstOrDefault(),
                    Due = a.Type == "Supplier Opening Balance Payment" ? oConnectionContext.DbClsUser.Where(b => b.UserId == payDetails.SupplierId).Select(b => b.OpeningBalance).FirstOrDefault() -
                    oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier opening balance payment"
                    && b.IsDeleted == false && b.IsCancelled == false && b.SupplierId == payDetails.SupplierId).Select(b => b.Amount).DefaultIfEmpty().Sum() :
                    oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.GrandTotal).FirstOrDefault() -
                    oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment")
                    && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
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
                    SupplierPayments = Payments,
                    BusinessSetting = BusinessSetting,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierPaymentJournal(ClsSupplierPaymentVm obj)
        {
            var payments = (oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId &&
             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsSupplierPaymentVm
             {
                 AmountExcTax = a.AmountExcTax,
                 TaxId = a.TaxId,
                 AccountId = a.AccountId,
                 JournalAccountId = a.JournalAccountId,
                 Amount = a.Amount,
                 SupplierPaymentId = a.SupplierPaymentId,
                 SupplierId = a.SupplierId,
                 PurchaseId = a.PurchaseId,
                 Type = a.Type,
                 IsReverseCharge = a.IsReverseCharge
             })).Concat(oConnectionContext.DbClsSupplierPayment.Where(a => a.ParentId == obj.SupplierPaymentId
            && a.AccountId != 0 &&
             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new ClsSupplierPaymentVm
             {
                 AmountExcTax = a.AmountExcTax,
                 TaxId = a.TaxId,
                 AccountId = a.AccountId,
                 JournalAccountId = a.JournalAccountId,
                 Amount = a.Amount,
                 SupplierPaymentId = a.SupplierPaymentId,
                 SupplierId = a.SupplierId,
                 PurchaseId = a.PurchaseId,
                 Type = a.Type,
                 IsReverseCharge = a.IsReverseCharge
             })).ToList();

            List<ClsPurchaseVm> Purchases = new List<ClsPurchaseVm>();

            foreach (var item in payments)
            {
                //var AllTaxs = payments.Where(a => a.SupplierPaymentId == item.SupplierPaymentId).Select(a => new
                //{
                //    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                //    a.TaxId,
                //    a.AmountExcTax
                //}).ToList();

                //List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                //foreach (var itemTax in AllTaxs)
                //{
                //    decimal AmountExcTax = itemTax.AmountExcTax;
                //    var taxs = itemTax.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == itemTax.TaxId).Select(a => new
                //    {
                //        a.TaxId,
                //        a.Tax,
                //        a.TaxPercent,
                //    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                //                   where a.TaxId == itemTax.TaxId
                //                   select new
                //                   {
                //                       TaxId = a.SubTaxId,
                //                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                //                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                //                   }).ToList();

                //    foreach (var tax in taxs)
                //    {
                //        oClsTaxVm.Add(new ClsTaxVm
                //        {
                //            TaxId = tax.TaxId,
                //            Tax = tax.Tax,
                //            TaxPercent = tax.TaxPercent,
                //            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                //        });
                //    }
                //}

                //var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                //         (k, c) => new
                //         {
                //             TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                //             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                //             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                //             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                //         }
                //        ).ToList();

                //var taxList = finalTaxs.Select(a => new ClsBankPaymentVm
                //{
                //    AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId ==
                //    oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                //    ).Select(c => c.AccountName).FirstOrDefault(),
                //    Debit = a.TaxAmount,
                //    Credit = a.TaxAmount,
                //    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                //}).ToList();

                var taxList = (from q in oConnectionContext.DbClsSupplierPaymentTaxJournal
                               join a in oConnectionContext.DbClsSupplierPayment
                               on q.SupplierPaymentId equals a.SupplierPaymentId
                               where q.SupplierPaymentId == item.SupplierPaymentId
                               && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                     //&& a.TaxAmount != 0
                               select new
                               {
                                   AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                   //Debit = (q.SupplierPaymentTaxJournalType == "Normal") ? q.TaxAmount : (a.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                   //Credit = (q.SupplierPaymentTaxJournalType == "Normal") ? 0 : (a.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                   Debit = (q.SupplierPaymentTaxJournalType == "Normal") ? q.TaxAmount : (a.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                   Credit = (q.SupplierPaymentTaxJournalType == "Normal") ? 0 : (a.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                   AccountId = q.AccountId
                               }).ToList();


                Purchases.Add(new ClsPurchaseVm
                {
                    Type = (item.Type.ToLower() == "purchase payment") ? item.Type +
                    " (Invoice No: " + oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == item.PurchaseId).Select(a => a.ReferenceNo).FirstOrDefault() + ")" :
                    item.Type,
                    Payments = (from a in payments.Where(b => b.SupplierPaymentId == item.SupplierPaymentId)
                                select new ClsSupplierPaymentVm
                                {
                                    AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                    Debit = item.Type.ToLower() == "supplier refund" ? a.Amount : 0,
                                    Credit = item.Type.ToLower() == "supplier refund" ? 0 : (item.IsReverseCharge == 1 ? a.AmountExcTax : a.Amount)
                                }).Concat(from a in payments.Where(b => b.SupplierPaymentId == item.SupplierPaymentId)
                                          select new ClsSupplierPaymentVm
                                          {
                                              AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.JournalAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                              Debit = item.Type.ToLower() == "supplier refund" ? 0 : (item.IsReverseCharge == 1 ? a.AmountExcTax : a.Amount),
                                              Credit = item.Type.ToLower() == "supplier refund" ? a.Amount : 0
                                          }).Concat(from a in taxList
                                                    select new ClsSupplierPaymentVm
                                                    {
                                                        // tax 
                                                        AccountName = a.AccountName,
                                                        //Debit = item.Type.ToLower() == "supplier refund" ? 0 : a.Debit,
                                                        //Credit = item.Type.ToLower() == "supplier refund" ? a.Credit : 0,
                                                        Debit = a.Debit,
                                                        Credit = a.Credit,
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
                    Purchases = Purchases
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DueSummary(ClsSupplierPaymentVm obj)
        {
            decimal AdvanceBalance = 0, Due = 0, OpeningBalance = 0, OpeningBalanceDue = 0, TotalPurchase = 0, TotalPurchasePaid = 0, TotalPurchaseDue = 0;

            OpeningBalance = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.OpeningBalance).FirstOrDefault();

            OpeningBalanceDue = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.OpeningBalance).FirstOrDefault() -
              oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier opening balance payment" &&
              b.IsDeleted == false && b.IsCancelled == false && b.SupplierId == obj.SupplierId).Select(b => b.Amount).DefaultIfEmpty().Sum();

            TotalPurchase = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
             && a.Status != "Draft" && a.SupplierId == obj.SupplierId && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId
             && l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
             ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();

            TotalPurchasePaid = (from c in oConnectionContext.DbClsPurchase
                                 join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                 where c.Status != "Draft" &&
        (d.Type.ToLower() == "purchase payment") && c.SupplierId == obj.SupplierId
        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
        && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
        && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                 select d.Amount).DefaultIfEmpty().Sum();

            TotalPurchaseDue = TotalPurchase - TotalPurchasePaid;

            var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => new
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
                TotalPurchase = TotalPurchase,
                TotalPurchasePaid = TotalPurchasePaid,
                TotalPurchaseDue = TotalPurchaseDue
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
                    //    TotalPurchase = TotalPurchase,
                    //    TotalPurchasePaid = TotalPurchasePaid,
                    //    TotalPurchaseDue = TotalPurchaseDue
                    //}
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UnusedAdvanceBalance(ClsSupplierPaymentVm obj)
        {
            var det = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierId == obj.SupplierId && a.IsDeleted == false && a.IsCancelled == false &&
                            a.IsActive == true && a.Type.ToLower() == "supplier payment" && a.AmountRemaining > 0).Select(a => new ClsSupplierPaymentVm
                            {
                                SupplierId = a.SupplierId,
                                AccountId = a.AccountId,
                                Amount = a.Amount,
                                ReferenceNo = a.PurchaseReturnId == 0 ? a.ReferenceNo : oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.PurchaseReturnId).Select(b => b.InvoiceNo).FirstOrDefault(),
                                PaymentDate = a.PaymentDate,
                                ParentId = a.ParentId,
                                ParentReferenceNo = oConnectionContext.DbClsSupplierPayment.Where(b => b.SupplierPaymentId == a.ParentId).Select(b => b.ReferenceNo).FirstOrDefault(),
                                SupplierPaymentId = a.SupplierPaymentId,
                                AmountRemaining = a.AmountRemaining,
                                IsReverseCharge = a.IsReverseCharge,
                            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupplierPayments = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierRefunds(ClsSupplierPaymentVm obj)
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

            var det = oConnectionContext.DbClsSupplierPayment.Where(b => b.CompanyId == obj.CompanyId
            && b.Type.ToLower() == "supplier refund"
            && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
            {
                b.PurchaseId,
                b.PurchaseReturnId,
                b.IsDirectPayment,
                SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == b.SupplierId).Select(c => c.Name).FirstOrDefault(),
                b.SupplierId,
                InvoiceUrl = oCommonController.webUrl,
                b.ParentId,
                b.ReferenceNo,
                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.SupplierPaymentId,
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
                b.IsReverseCharge
            }).ToList();

            if (obj.ParentId != 0)
            {
                det = det.Where(a => a.ParentId == obj.ParentId).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).ToList();
            }

            if (obj.PurchaseReturnId != 0)
            {
                det = det.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupplierPayments = det.OrderByDescending(a => a.SupplierPaymentId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSupplierRefund(ClsSupplierPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var userDet = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.ParentId
               && a.CompanyId == obj.CompanyId).Select(a => new
               {
                   a.BranchId,
                   a.SupplierId,
                   a.IsBusinessRegistered,
                   a.GstTreatment,
                   a.BusinessRegistrationNameId,
                   a.BusinessRegistrationNo,
                   a.BusinessLegalName,
                   a.BusinessTradeName,
                   a.PanNo,
                   a.TaxId,
                   a.PurchaseReturnId,
                   a.AmountRemaining,
                   a.IsReverseCharge,
                   //CountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.AddedBy).Select(b => b.CountryId).FirstOrDefault(),
               }).FirstOrDefault();

                //if (obj.SupplierId == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
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
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
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
                obj.SupplierId = userDet.SupplierId;

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
            && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                long TaxAccountId = 0;

                List<ClsTaxVm> taxList = new List<ClsTaxVm>();
                string AccountType = "";

                if (userDet.TaxId != 0)
                {
                    obj.TaxId = userDet.TaxId;
                    TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == userDet.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();
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
                    //decimal AmountExcTax = obj.IsReverseCharge == 1 ? obj.Amount : obj.AmountExcTax;
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
                            TaxAmount = (tax.TaxPercent / 100) * obj.Amount
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

                    //if (userDet.CountryId == 2)
                    //{
                    if (userDet.IsReverseCharge == 1)
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
                        taxList = finalTaxs.Select(a => new ClsTaxVm
                        {
                            TaxType = "Normal",
                            TaxId = a.TaxId,
                            TaxPercent = a.TaxPercent,
                            TaxAmount = a.TaxAmount,
                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                        }).ToList();
                    }
                    //}
                    //else
                    //{
                    //    taxList = finalTaxs.Select(a => new ClsTaxVm
                    //    {
                    //        TaxType = "Normal",
                    //        TaxId = a.TaxId,
                    //        TaxPercent = a.TaxPercent,
                    //        TaxAmount = a.TaxAmount,
                    //        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                    //    }).ToList();
                    //}

                    //tax journal
                }
                else
                {
                    obj.AmountExcTax = obj.Amount;
                    obj.TaxAmount = 0;
                }

                ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
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
                    SupplierId = obj.SupplierId,
                    AttachDocument = obj.AttachDocument,
                    Type = "Supplier Refund",
                    BranchId = obj.BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 1,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = oCommonController.CreateToken(),
                    //PaymentIds = _json
                    JournalAccountId = JournalAccountId,
                    AmountRemaining = obj.AmountExcTax,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
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
                    IsReverseCharge = userDet.IsReverseCharge,
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

                    oClsSupplierPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment);
                oConnectionContext.SaveChanges();

                long SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId;

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + obj.Amount + " where \"UserId\"=" + obj.SupplierId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                query = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + obj.Amount + " where \"SupplierPaymentId\"=" + obj.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                if (AccountType != "")
                {
                    var AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                    && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                    ClsSupplierPaymentTaxJournal oClsSupplierPaymentTaxJournal = new ClsSupplierPaymentTaxJournal()
                    {
                        SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId,
                        TaxId = obj.TaxId,
                        TaxAmount = userDet.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : obj.TaxAmount,
                        AccountId = AccountId,
                        SupplierPaymentTaxJournalType = "Normal"
                    };
                    oConnectionContext.DbClsSupplierPaymentTaxJournal.Add(oClsSupplierPaymentTaxJournal);
                    oConnectionContext.SaveChanges();
                }

                foreach (var taxJournal in taxList)
                {
                    ClsSupplierPaymentTaxJournal oClsSupplierPaymentTaxJournal = new ClsSupplierPaymentTaxJournal()
                    {
                        SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId,
                        TaxId = taxJournal.TaxId,
                        TaxAmount = taxJournal.TaxAmount,
                        AccountId = taxJournal.AccountId,
                        SupplierPaymentTaxJournalType = taxJournal.TaxType
                    };
                    oConnectionContext.DbClsSupplierPaymentTaxJournal.Add(oClsSupplierPaymentTaxJournal);
                    oConnectionContext.SaveChanges();
                }

                if (userDet.PurchaseReturnId != 0)
                {
                    decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                    == obj.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                    if (AmountRemaining <= 0)
                    {
                        query = "update \"tblPurchaseReturn\" set \"Status\"='Closed' where \"PurchaseReturnId\"=" + userDet.PurchaseReturnId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Supplier Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Supplier Refund \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
                    Id = SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (userDet.PurchaseReturnId != 0)
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Debit Note", obj.CompanyId, oClsSupplierPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Supplier Advance Payment", obj.CompanyId, oClsSupplierPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Supplier Refund created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RefundDelete(ClsSupplierPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                {
                    SupplierPaymentId = obj.SupplierPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var userDet = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId
                && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    ParentPurchaseReturnId = oConnectionContext.DbClsSupplierPayment.Where(b => b.SupplierPaymentId == a.ParentId).Select(b => b.PurchaseReturnId).FirstOrDefault(),
                    a.SupplierId,
                    a.Amount,
                    a.ParentId
                }).FirstOrDefault();

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + userDet.Amount + " where \"UserId\"=" + userDet.SupplierId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                string q = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + userDet.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + userDet.Amount + " where \"SupplierPaymentId\"=" + userDet.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(q);

                if (userDet.ParentPurchaseReturnId != 0)
                {
                    decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                    == userDet.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                    if (AmountRemaining > 0)
                    {
                        query = "update \"tblPurchaseReturn\" set \"Status\"='Open' where \"PurchaseReturnId\"=" + userDet.ParentPurchaseReturnId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Supplier Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Supplier Refund \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RefundCancel(ClsSupplierPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
                {
                    SupplierPaymentId = obj.SupplierPaymentId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var userDet = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId
                && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    ParentPurchaseReturnId = oConnectionContext.DbClsSupplierPayment.Where(b => b.SupplierPaymentId == a.ParentId).Select(b => b.PurchaseReturnId).FirstOrDefault(),
                    a.SupplierId,
                    a.Amount,
                    a.ParentId
                }).FirstOrDefault();

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + userDet.Amount + " where \"UserId\"=" + userDet.SupplierId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                string q = "update \"tblSupplierPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + userDet.Amount + ",\"AmountUsed\"=\"AmountUsed\"-" + userDet.Amount + " where \"SupplierPaymentId\"=" + userDet.ParentId;
                oConnectionContext.Database.ExecuteSqlCommand(q);

                if (userDet.ParentPurchaseReturnId != 0)
                {
                    decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                    == userDet.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                    if (AmountRemaining > 0)
                    {
                        query = "update \"tblPurchaseReturn\" set \"Status\"='Open' where \"PurchaseReturnId\"=" + userDet.ParentPurchaseReturnId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Supplier Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Supplier Refund \"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.SupplierPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                if (userDet.ParentPurchaseReturnId != 0)
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Debit Note", obj.CompanyId, oClsPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }
                else
                {
                    string[] arr = oNotificationTemplatesController.SendNotifications("Refund From Supplier Advance Payment", obj.CompanyId, oClsPayment.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Refund cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ApplyCreditsToInvoices(ClsSupplierPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //List<ClsSupplierPaymentIds> oClsSupplierPaymentIds = new List<ClsSupplierPaymentIds>();
                if (obj.SupplierPaymentIds == null)
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

                var SupplierPayment = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.ParentId).Select(a => new
                {
                    a.BranchId,
                    a.SupplierId,
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
                    a.PurchaseReturnId,
                    a.IsReverseCharge,
                    a.SourceOfSupplyId,
                    a.DestinationOfSupplyId
                }).FirstOrDefault();

                if (obj.SupplierPaymentIds != null)
                {
                    if (obj.SupplierPaymentIds.Sum(a => a.Amount) > SupplierPayment.AmountRemaining)
                    {
                        errors.Add(new ClsError { Message = "Total Credits to Apply cannot be more than Balance", Id = "divCredits" });
                        isError = true;
                    }

                    var Dues = obj.SupplierPaymentIds.Select(a => new
                    {
                        DivId = a.DivId,
                        Type = a.Type,
                        BranchId = SupplierPayment.BranchId,
                        PurchaseId = a.PurchaseId,
                        SupplierId = SupplierPayment.SupplierId,
                        Amount = a.Amount,
                        Due = a.Type == "Purchase Payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.GrandTotal).FirstOrDefault() -
                    oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false &&
                    b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum() :
                    oConnectionContext.DbClsUser.Where(d => d.UserId == SupplierPayment.SupplierId).Select(d => d.OpeningBalance).FirstOrDefault() 
                    - (from d in oConnectionContext.DbClsSupplierPayment                                                                                                                                                      
                       where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == SupplierPayment.SupplierId                                                                                                                                   
                       && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false                                                                                                                                                      
                       select d.Amount).DefaultIfEmpty().Sum(),
                        IsReverseCharge = a.IsReverseCharge
                    }).ToList();

                    foreach (var item in Dues)
                    {
                        if (item.Amount > item.Due)
                        {
                            errors.Add(new ClsError { Message = "Credits to Apply cannot be more than Due", Id = "divAmount" + item.DivId });
                            isError = true;
                        }

                        if (item.Amount != 0)
                        {
                            if (SupplierPayment.IsReverseCharge != item.IsReverseCharge)
                            {
                                errors.Add(new ClsError { Message = "Reverse Charge Tax Type should be same for Bill and Vendor Payment", Id = "divAmount" + item.PurchaseId });
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

                    obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();
                    decimal RemainingAmount = SupplierPayment.AmountRemaining;

                    if (Dues != null && Dues.Count() > 0)
                    {
                        long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

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
                                    string AccountType = "";

                                    if (SupplierPayment.TaxId != 0)
                                    {
                                        obj.TaxId = SupplierPayment.TaxId;
                                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == SupplierPayment.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                                        //obj.AmountExcTax = (100 * (_amount) / (100 +
                                        //    oConnectionContext.DbClsTax.Where(a => a.TaxId == SupplierPayment.TaxId).Select(a => a.TaxPercent).FirstOrDefault()));
                                        //obj.TaxAmount = (_amount - obj.AmountExcTax);
                                        //obj.Amount = item.Amount;

                                        obj.AmountExcTax = item.Amount;
                                        obj.Amount = item.Amount + ((oConnectionContext.DbClsTax.Where(a => a.TaxId ==
                                        SupplierPayment.TaxId).Select(a => a.TaxPercent).FirstOrDefault()/100)* item.Amount);
                                        obj.TaxAmount = (obj.Amount - obj.AmountExcTax);
                                        

                                        //tax journal

                                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == obj.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                                        //decimal AmountExcTax = obj.IsReverseCharge == 1 ? obj.Amount : obj.AmountExcTax;
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
                                                TaxAmount = (tax.TaxPercent / 100) * obj.Amount
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

                                        //if (userDet.CountryId == 2)
                                        //{
                                        if (SupplierPayment.IsReverseCharge == 1)
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
                                            taxList = finalTaxs.Select(a => new ClsTaxVm
                                            {
                                                TaxType = "Normal",
                                                TaxId = a.TaxId,
                                                TaxPercent = a.TaxPercent,
                                                TaxAmount = a.TaxAmount,
                                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                            }).ToList();
                                        }
                                        //}
                                        //else
                                        //{
                                        //    taxList = finalTaxs.Select(a => new ClsTaxVm
                                        //    {
                                        //        TaxType = "Normal",
                                        //        TaxId = a.TaxId,
                                        //        TaxPercent = a.TaxPercent,
                                        //        TaxAmount = a.TaxAmount,
                                        //        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                        //    }).ToList();
                                        //}

                                        //tax journal
                                    }
                                    else
                                    {
                                        obj.AmountExcTax = item.Amount;
                                        obj.TaxAmount = 0;
                                    }

                                    ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
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
                                        SupplierId = SupplierPayment.SupplierId,
                                        PurchaseId = item.PurchaseId,
                                        AttachDocument = obj.AttachDocument,
                                        Type = item.Type,
                                        BranchId = SupplierPayment.BranchId,
                                        AccountId = JournalAccountId,
                                        //ReferenceNo = ReferenceNo,
                                        IsDebit = 2,
                                        //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                        ParentId = obj.ParentId,
                                        ReferenceId = oCommonController.CreateToken(),
                                        JournalAccountId = AccountId,
                                        SourceOfSupplyId = SupplierPayment.SourceOfSupplyId,
                                        DestinationOfSupplyId = SupplierPayment.DestinationOfSupplyId,
                                        TaxId = SupplierPayment.TaxId,
                                        IsBusinessRegistered = SupplierPayment.IsBusinessRegistered,
                                        GstTreatment = SupplierPayment.GstTreatment,
                                        BusinessRegistrationNameId = SupplierPayment.BusinessRegistrationNameId,
                                        BusinessRegistrationNo = SupplierPayment.BusinessRegistrationNo,
                                        BusinessLegalName = SupplierPayment.BusinessLegalName,
                                        BusinessTradeName = SupplierPayment.BusinessTradeName,
                                        PanNo = SupplierPayment.PanNo,
                                        TaxAccountId = TaxAccountId,
                                        AmountExcTax = obj.AmountExcTax,
                                        TaxAmount = obj.TaxAmount,
                                        IsReverseCharge = SupplierPayment.IsReverseCharge
                                    };
                                    oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment1);
                                    oConnectionContext.SaveChanges();

                                    //oClsSupplierPaymentIds.Add(new ClsSupplierPaymentIds { SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId, SupplierId = item.SupplierId, PurchaseId = item.PurchaseId, Type = item.Type, Amount = _amount });

                                    RemainingAmount = RemainingAmount - item.Amount;

                                    if (item.Type.ToLower() == "purchase payment")
                                    {
                                        string PaymentStatus = "";
                                        decimal GrandTotal = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == item.PurchaseId).Select(a => a.GrandTotal).FirstOrDefault();

                                        decimal previousPayments = oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") &&
                                       b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                                        if (GrandTotal == (previousPayments))
                                        {
                                            PaymentStatus = "Paid";
                                        }
                                        else if (GrandTotal > (previousPayments))
                                        {
                                            PaymentStatus = "Partially Paid";
                                        }

                                        string query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + item.PurchaseId;
                                        oConnectionContext.Database.ExecuteSqlCommand(query);
                                    }

                                    if (AccountType != "")
                                    {
                                        var AccountId1 = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                                        && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                        ClsSupplierPaymentTaxJournal oClsSupplierPaymentTaxJournal = new ClsSupplierPaymentTaxJournal()
                                        {
                                            SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId,
                                            TaxId = obj.TaxId,
                                            TaxAmount = SupplierPayment.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : obj.TaxAmount,
                                            AccountId = AccountId1,
                                            SupplierPaymentTaxJournalType = "Normal"
                                        };
                                        oConnectionContext.DbClsSupplierPaymentTaxJournal.Add(oClsSupplierPaymentTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }

                                    foreach (var taxJournal in taxList)
                                    {
                                        ClsSupplierPaymentTaxJournal oClsSupplierPaymentTaxJournal = new ClsSupplierPaymentTaxJournal()
                                        {
                                            SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId,
                                            TaxId = taxJournal.TaxId,
                                            TaxAmount = taxJournal.TaxAmount,
                                            AccountId = taxJournal.AccountId,
                                            SupplierPaymentTaxJournalType = taxJournal.TaxType
                                        };
                                        oConnectionContext.DbClsSupplierPaymentTaxJournal.Add(oClsSupplierPaymentTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }

                                    if (SupplierPayment.PurchaseReturnId != 0)
                                    {
                                        string[] arr = oNotificationTemplatesController.SendNotifications("Credits Applied From Debit Note", obj.CompanyId, oClsSupplierPayment1.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                                    }
                                    else
                                    {
                                        string[] arr = oNotificationTemplatesController.SendNotifications("Credits Applied From Supplier Advance Payment", obj.CompanyId, oClsSupplierPayment1.SupplierPaymentId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);
                                    }

                                }
                            }
                        }

                        //serializer.MaxJsonLength = 2147483644;
                        //string _json = serializer.Serialize(oClsSupplierPaymentIds);

                        string r = "update \"tblSupplierPayment\" set \"AmountRemaining\"=" + RemainingAmount + ",\"AmountUsed\"=\"Amount\"-" + RemainingAmount + " where \"SupplierPaymentId\"=" + obj.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(r);

                        r = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + Dues.Select(a => a.Amount).DefaultIfEmpty().Sum() + " where \"UserId\"=" + SupplierPayment.SupplierId;
                        oConnectionContext.Database.ExecuteSqlCommand(r);
                    }

                    if (SupplierPayment.PurchaseReturnId != 0)
                    {
                        decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                        == obj.ParentId).Select(a => a.AmountRemaining).FirstOrDefault();

                        if (AmountRemaining <= 0)
                        {
                            string query = "update \"tblPurchaseReturn\" set \"Status\"='Closed' where \"PurchaseReturnId\"=" + SupplierPayment.PurchaseReturnId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                    }

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Supplier Payment",
                        CompanyId = obj.CompanyId,
                        Description = "Credits Applied To Invoices\"" + oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId == obj.ParentId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" created",
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
