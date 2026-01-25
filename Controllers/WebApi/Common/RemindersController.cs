using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi.Common
{
    [ExceptionHandler]
    public class RemindersController : ApiController
    {
        public string webUrl = WebConfigurationManager.AppSettings["webUrl"];
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        [HttpGet]
        public void SendReminders()
        {
            string Domain = "";
            long SmsSettingsId = 0, EmailSettingsId = 0, WhatsappSettingsId = 0;

            List<ClsReminderModulesSettingsVm> NotificationModulesSettings = (from b in oConnectionContext.DbClsReminderModulesSettings
                                                                              where b.ReminderType == "Automated" && b.IsActive == true && b.IsDeleted == false
                                                                              select new ClsReminderModulesSettingsVm
                                                                              {
                                                                                  Name = b.Name,
                                                                                  CompanyId = b.CompanyId,
                                                                                  AutoSendEmail = b.AutoSendEmail,
                                                                                  AutoSendSms = b.AutoSendSms,
                                                                                  AutoSendWhatsapp = b.AutoSendWhatsapp,
                                                                                  EmailSubject = b.EmailSubject,
                                                                                  CC = b.CC,
                                                                                  BCC = b.BCC,
                                                                                  EmailBody = b.EmailBody == null ? "" : b.EmailBody,
                                                                                  SmsBody = b.SmsBody == null ? "" : b.SmsBody,
                                                                                  WhatsappBody = b.WhatsappBody == null ? "" : b.WhatsappBody,
                                                                                  ReminderTo = b.ReminderTo,
                                                                                  ReminderInDays = b.ReminderInDays,
                                                                                  ReminderBeforeAfter = b.ReminderBeforeAfter,
                                                                                  TotalDue = b.TotalDue
                                                                              }).ToList();

            foreach (var NotificationModulesSetting in NotificationModulesSettings)
            {
                if (oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == NotificationModulesSetting.CompanyId &&
             a.StartDate != null && a.Status == 2 && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault() == 0)
                {
                    continue;
                }

                if (NotificationModulesSetting.AutoSendSms == true || NotificationModulesSetting.AutoSendEmail == true ||
            NotificationModulesSetting.AutoSendWhatsapp == true)
                {
                    if (NotificationModulesSetting.ReminderType == "Sales Invoice")
                    {
                        DateTime toDay = oCommonController.CurrentDate(NotificationModulesSetting.CompanyId);
                        List<ClsSalesVm> Sales = new List<ClsSalesVm>();

                        if (NotificationModulesSetting.ReminderBeforeAfter == "Before Due Date")
                        {
                            Sales = oConnectionContext.DbClsSales.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId && a.IsActive == true
                            && a.IsDeleted == false && a.IsCancelled == false && (a.Status == "Due" || a.Status == "Partially Paid")).AsEnumerable().Select(a => new ClsSalesVm
                            {
                                DueDays = SqlFunctions.DateDiff("day", a.DueDate, toDay),
                                SalesId = a.SalesId,
                                CustomerId = a.CustomerId,
                                Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                                - oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false &&
                                b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                            }).ToList();

                            if (NotificationModulesSetting.ReminderInDays > 0)
                            {
                                Sales = Sales.Where(a => a.DueDays == NotificationModulesSetting.ReminderInDays).ToList();
                            }

                            if (NotificationModulesSetting.TotalDue > 0)
                            {
                                Sales = Sales.Where(a => a.Due >= NotificationModulesSetting.TotalDue).ToList();
                            }
                        }
                        else
                        {
                            Sales = oConnectionContext.DbClsSales.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId && a.IsActive == true
                            && a.IsDeleted == false && a.IsCancelled == false && a.Status == "Overdue").AsEnumerable().Select(a => new ClsSalesVm
                            {
                                DueDays = SqlFunctions.DateDiff("day", toDay, a.DueDate),
                                SalesId = a.SalesId,
                                CustomerId = a.CustomerId,
                                Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                                - oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false &&
                                b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                            }).ToList();

                            if (NotificationModulesSetting.ReminderInDays > 0)
                            {
                                Sales = Sales.Where(a => a.DueDays == NotificationModulesSetting.ReminderInDays).ToList();
                            }

                            if (NotificationModulesSetting.TotalDue > 0)
                            {
                                Sales = Sales.Where(a => a.Due >= NotificationModulesSetting.TotalDue).ToList();
                            }
                        }

                        if (Sales != null && Sales.Count > 0)
                        {
                            Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId &&
                            a.IsDeleted == false && a.IsActive == true).Select(a => a.Domain).FirstOrDefault();

                            if (Domain == null || Domain == "")
                            {
                                long Under = oConnectionContext.DbClsUser.Where(a => a.UserId == NotificationModulesSetting.CompanyId).Select(a =>
                                a.Under).FirstOrDefault();

                                Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == Under && a.IsDeleted == false &&
                                a.IsActive == true).Select(a => a.Domain).FirstOrDefault();
                            }

                            SmsSettingsId = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId
                            && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.SmsSettingsId).FirstOrDefault();

                            EmailSettingsId = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId
                            && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.EmailSettingsId).FirstOrDefault();

                            WhatsappSettingsId = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId
                            && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.WhatsappSettingsId).FirstOrDefault();

                            foreach (var item in Sales)
                            {
                                var _ = SaleReminder(NotificationModulesSetting, NotificationModulesSetting.CompanyId, item.SalesId, NotificationModulesSetting.CC, NotificationModulesSetting.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, NotificationModulesSetting.Name, NotificationModulesSetting.CompanyId, oCommonController.CurrentDate(NotificationModulesSetting.CompanyId), Domain, NotificationModulesSetting.ReminderTo);
                            }
                        }
                    }
                    else
                    {
                        DateTime toDay = oCommonController.CurrentDate(NotificationModulesSetting.CompanyId);
                        List<ClsPurchaseVm> Purchases = new List<ClsPurchaseVm>();

                        if (NotificationModulesSetting.ReminderBeforeAfter == "Before Due Date")
                        {
                            Purchases = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId && a.IsActive == true
                            && a.IsDeleted == false && a.IsCancelled == false && (a.Status == "Due" || a.Status == "Partially Paid")).Select(a => new ClsPurchaseVm
                            {
                                DueDays = SqlFunctions.DateDiff("day", a.DueDate, toDay),
                                PurchaseId = a.PurchaseId,
                                SupplierId = a.SupplierId,
                                Due = a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment"
                                && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum()
                            }).ToList();

                            if (NotificationModulesSetting.ReminderInDays > 0)
                            {
                                Purchases = Purchases.Where(a => a.DueDays == NotificationModulesSetting.ReminderInDays).ToList();
                            }

                            if (NotificationModulesSetting.TotalDue > 0)
                            {
                                Purchases = Purchases.Where(a => a.Due >= NotificationModulesSetting.TotalDue).ToList();
                            }
                        }
                        else
                        {
                            Purchases = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId && a.IsActive == true
                            && a.IsDeleted == false && a.IsCancelled == false && a.Status == "Overdue").AsEnumerable().Select(a => new ClsPurchaseVm
                            {
                                DueDays = SqlFunctions.DateDiff("day", toDay, a.DueDate),
                                PurchaseId = a.PurchaseId,
                                SupplierId = a.SupplierId,
                                Due = a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment"
                                && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                            }).ToList();

                            if (NotificationModulesSetting.ReminderInDays > 0)
                            {
                                Purchases = Purchases.Where(a => a.DueDays == NotificationModulesSetting.ReminderInDays).ToList();
                            }

                            if (NotificationModulesSetting.TotalDue > 0)
                            {
                                Purchases = Purchases.Where(a => a.Due >= NotificationModulesSetting.TotalDue).ToList();
                            }
                        }

                        if (Purchases != null && Purchases.Count > 0)
                        {
                            Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId &&
                            a.IsDeleted == false && a.IsActive == true).Select(a => a.Domain).FirstOrDefault();

                            if (Domain == null || Domain == "")
                            {
                                long Under = oConnectionContext.DbClsUser.Where(a => a.UserId == NotificationModulesSetting.CompanyId).Select(a =>
                                a.Under).FirstOrDefault();

                                Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == Under && a.IsDeleted == false &&
                                a.IsActive == true).Select(a => a.Domain).FirstOrDefault();
                            }

                            SmsSettingsId = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId
                            && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.SmsSettingsId).FirstOrDefault();

                            EmailSettingsId = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId
                            && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.EmailSettingsId).FirstOrDefault();

                            WhatsappSettingsId = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == NotificationModulesSetting.CompanyId
                            && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.WhatsappSettingsId).FirstOrDefault();

                            foreach (var item in Purchases)
                            {
                                var _ = PurchaseReminder(NotificationModulesSetting, NotificationModulesSetting.CompanyId, item.PurchaseId, NotificationModulesSetting.CC, NotificationModulesSetting.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, NotificationModulesSetting.Name, NotificationModulesSetting.CompanyId, oCommonController.CurrentDate(NotificationModulesSetting.CompanyId), Domain, NotificationModulesSetting.ReminderTo);
                            }
                        }
                    }
                }
            }

            //string[] arr = { "1", "", "" };
            //return arr;
        }

        public dynamic SaleReminder(ClsReminderModulesSettingsVm NotificationModulesSetting, long CompanyId, long Id, string CC, string Bcc, long SmsSettingsId, long EmailSettingsId, long WhatsappSettingsId, string Type, long AddedBy, DateTime CurrentDate, string Domain, string ReminderTo)
        {
            var Sale = oConnectionContext.DbClsSales.Where(a => a.SalesId == Id).Select(a => new
            {
                OverdueDays = DbFunctions.DiffDays(CurrentDate, a.DueDate),
                Paid = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                Due = (a.GrandTotal - a.WriteOffAmount) - (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                    oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum()),
                a.BranchId,
                a.InvoiceNo,
                InvoiceUrl = webUrl + (a.SalesType.ToLower() == "pos" ? "/sales/receipt?InvoiceId=" + a.InvoiceId : "/sales/invoice?InvoiceId=" + a.InvoiceId),
                CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
                CustomerMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.MobileNo).FirstOrDefault(),
                CustomerEmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.EmailId).FirstOrDefault(),
                PlaceOfSupply = oConnectionContext.DbClsState.Where(b => b.StateId == a.PlaceOfSupplyId).Select(b => b.State).FirstOrDefault(),
                a.SalesDate,
                PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(b => b.PaymentTermId == a.PaymentTermId).Select(b => b.PaymentTerm).FirstOrDefault(),
                a.DueDate,
                SellingPriceGroup = oConnectionContext.DbClsSellingPriceGroup.Where(b => b.SellingPriceGroupId == a.SellingPriceGroupId).
                Select(b => b.SellingPriceGroup).FirstOrDefault(),
                a.Notes,
                a.Subtotal,
                a.TotalDiscount,
                a.TotalTaxAmount,
                a.NetAmount,
                a.RoundOff,
                a.SpecialDiscount,
                a.GrandTotal,
                CompanyEmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CompanyId).Select(b => b.EmailId).FirstOrDefault(),
                CompanyMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CompanyId).Select(b => b.MobileNo).FirstOrDefault(),
            }).FirstOrDefault();

            var Branch = oConnectionContext.DbClsBranch.Where(a => a.BranchId == Sale.BranchId).Select(a => new
            {
                BusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == CompanyId).Select(b => b.BusinessName).FirstOrDefault(),
                BusinessLogo = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == CompanyId).Select(b => b.BusinessLogo).FirstOrDefault(),
                a.Branch,
                a.BranchCode,
                a.Address,
                a.Zipcode,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                //ContactPersonName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ContactPersonId).Select(b => b.Name).FirstOrDefault(),
                //ContactPersonMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ContactPersonId).Select(b => b.MobileNo).FirstOrDefault(),
                a.Mobile,
                a.AltMobileNo,
                a.Email,
                BusinessRegistrationNo = oConnectionContext.DbClsTaxSetting.Where(b => b.TaxSettingId == a.TaxSettingId).Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessRegistrationName = oConnectionContext.DbClsBusinessRegistrationName.Where(c => c.BusinessRegistrationNameId ==
                oConnectionContext.DbClsTaxSetting.Where(b => b.TaxSettingId == a.TaxSettingId).Select(b => b.BusinessRegistrationNameId).FirstOrDefault()).Select(c => c.Name).FirstOrDefault()
            }).FirstOrDefault();

            if (SmsSettingsId != 0)
            {
                string formattedBody = NotificationModulesSetting.SmsBody;
                string[] array = NotificationModulesSetting.SmsBody.Split('{');
                List<string> list = new List<string>();

                foreach (var item in array)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list)
                {
                    if (item == "Business Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedBody = formattedBody.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedBody = formattedBody.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Invoice No")
                    {
                        formattedBody = formattedBody.Replace("{Invoice No}", Sale.InvoiceNo);
                    }
                    else if (item == "Invoice Url")
                    {
                        formattedBody = formattedBody.Replace("{Invoice Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Customer Name")
                    {
                        formattedBody = formattedBody.Replace("{Customer Name}", Sale.CustomerName);
                    }
                    else if (item == "Customer Mobile No")
                    {
                        formattedBody = formattedBody.Replace("{Customer Mobile No}", Sale.CustomerMobileNo);
                    }
                    else if (item == "Customer Email")
                    {
                        formattedBody = formattedBody.Replace("{Customer Email}", Sale.CustomerEmailId);
                    }
                    else if (item == "Place Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Place Of Supply}", Sale.PlaceOfSupply);
                    }
                    else if (item == "Invoice Date")
                    {
                        formattedBody = formattedBody.Replace("{Invoice Date}", Sale.SalesDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedBody = formattedBody.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedBody = formattedBody.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Selling Price Group")
                    {
                        formattedBody = formattedBody.Replace("{Selling Price Group}", Sale.SellingPriceGroup);
                    }
                    else if (item == "Notes")
                    {
                        formattedBody = formattedBody.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedBody = formattedBody.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedBody = formattedBody.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedBody = formattedBody.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedBody = formattedBody.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedBody = formattedBody.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedBody = formattedBody.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }

                SmsController oSmsController = new SmsController();
                if (ReminderTo == "Customer" || ReminderTo == "Customer + Me")
                {
                    oSmsController.SmsFunction(CompanyId, Sale.CustomerMobileNo, formattedBody, AddedBy, CurrentDate, Id, Type, Domain, 0);
                }
                if (ReminderTo == "Me" || ReminderTo == "Customer + Me")
                {
                    oSmsController.SmsFunction(CompanyId, Sale.CompanyMobileNo, formattedBody, AddedBy, CurrentDate, Id, Type, Domain, 0);
                }
            }
            if (EmailSettingsId != 0)
            {
                #region Email Subject
                string formattedSubject = "";

                formattedSubject = NotificationModulesSetting.EmailSubject;
                string[] arr1 = NotificationModulesSetting.EmailSubject.Split('{');
                List<string> list1 = new List<string>();

                foreach (var item in arr1)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list1.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list1)
                {
                    if (item == "Business Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Invoice No")
                    {
                        formattedSubject = formattedSubject.Replace("{Invoice No}", Sale.InvoiceNo);
                    }
                    else if (item == "Invoice Url")
                    {
                        formattedSubject = formattedSubject.Replace("{Invoice Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Customer Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Customer Name}", Sale.CustomerName);
                    }
                    else if (item == "Customer Mobile No")
                    {
                        formattedSubject = formattedSubject.Replace("{Customer Mobile No}", Sale.CustomerMobileNo);
                    }
                    else if (item == "Customer Email")
                    {
                        formattedSubject = formattedSubject.Replace("{Customer Email}", Sale.CustomerEmailId);
                    }
                    else if (item == "Place Of Supply")
                    {
                        formattedSubject = formattedSubject.Replace("{Place Of Supply}", Sale.PlaceOfSupply);
                    }
                    else if (item == "Invoice Date")
                    {
                        formattedSubject = formattedSubject.Replace("{Invoice Date}", Sale.SalesDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedSubject = formattedSubject.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedSubject = formattedSubject.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Selling Price Group")
                    {
                        formattedSubject = formattedSubject.Replace("{Selling Price Group}", Sale.SellingPriceGroup);
                    }
                    else if (item == "Notes")
                    {
                        formattedSubject = formattedSubject.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedSubject = formattedSubject.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedSubject = formattedSubject.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedSubject = formattedSubject.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }
                #endregion

                #region EmailBody
                string formattedBody = NotificationModulesSetting.EmailBody;
                string[] array = NotificationModulesSetting.EmailBody.Split('{');
                List<string> list = new List<string>();

                foreach (var item in array)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list)
                {
                    if (item == "Business Logo")
                    {
                        formattedBody = formattedBody.Replace("{Business Logo}", "<img src='" + webUrl + Branch.BusinessLogo + "' />");
                    }
                    else if (item == "Business Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedBody = formattedBody.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedBody = formattedBody.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Invoice No")
                    {
                        formattedBody = formattedBody.Replace("{Invoice No}", Sale.InvoiceNo);
                    }
                    else if (item == "Invoice Url")
                    {
                        formattedBody = formattedBody.Replace("{Invoice Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Customer Name")
                    {
                        formattedBody = formattedBody.Replace("{Customer Name}", Sale.CustomerName);
                    }
                    else if (item == "Customer Mobile No")
                    {
                        formattedBody = formattedBody.Replace("{Customer Mobile No}", Sale.CustomerMobileNo);
                    }
                    else if (item == "Customer Email")
                    {
                        formattedBody = formattedBody.Replace("{Customer Email}", Sale.CustomerEmailId);
                    }
                    else if (item == "Place Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Place Of Supply}", Sale.PlaceOfSupply);
                    }
                    else if (item == "Invoice Date")
                    {
                        formattedBody = formattedBody.Replace("{Invoice Date}", Sale.SalesDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedBody = formattedBody.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedBody = formattedBody.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Selling Price Group")
                    {
                        formattedBody = formattedBody.Replace("{Selling Price Group}", Sale.SellingPriceGroup);
                    }
                    else if (item == "Notes")
                    {
                        formattedBody = formattedBody.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedBody = formattedBody.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedBody = formattedBody.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedBody = formattedBody.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedBody = formattedBody.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedBody = formattedBody.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedBody = formattedBody.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }
                #endregion

                EmailController oEmailController = new EmailController();
                if (ReminderTo == "Customer" || ReminderTo == "Customer + Me")
                {
                    oEmailController.NotificationTemplate(CompanyId, Sale.CustomerEmailId, formattedSubject, formattedBody, CC, Bcc, AddedBy, CurrentDate, Id, Type, Domain);
                }
                if (ReminderTo == "Me" || ReminderTo == "Customer + Me")
                {
                    oEmailController.NotificationTemplate(CompanyId, Sale.CompanyEmailId, formattedSubject, formattedBody, CC, Bcc, AddedBy, CurrentDate, Id, Type, Domain);
                }
            }
            if (WhatsappSettingsId != 0)
            {
                string formattedBody = NotificationModulesSetting.WhatsappBody;
                string[] array = NotificationModulesSetting.WhatsappBody.Split('{');
                List<string> list = new List<string>();

                foreach (var item in array)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list)
                {
                    if (item == "Business Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedBody = formattedBody.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedBody = formattedBody.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Invoice No")
                    {
                        formattedBody = formattedBody.Replace("{Invoice No}", Sale.InvoiceNo);
                    }
                    else if (item == "Invoice Url")
                    {
                        formattedBody = formattedBody.Replace("{Invoice Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Customer Name")
                    {
                        formattedBody = formattedBody.Replace("{Customer Name}", Sale.CustomerName);
                    }
                    else if (item == "Customer Mobile No")
                    {
                        formattedBody = formattedBody.Replace("{Customer Mobile No}", Sale.CustomerMobileNo);
                    }
                    else if (item == "Customer Email")
                    {
                        formattedBody = formattedBody.Replace("{Customer Email}", Sale.CustomerEmailId);
                    }
                    else if (item == "Place Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Place Of Supply}", Sale.PlaceOfSupply);
                    }
                    else if (item == "Invoice Date")
                    {
                        formattedBody = formattedBody.Replace("{Invoice Date}", Sale.SalesDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedBody = formattedBody.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedBody = formattedBody.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Selling Price Group")
                    {
                        formattedBody = formattedBody.Replace("{Selling Price Group}", Sale.SellingPriceGroup);
                    }
                    else if (item == "Notes")
                    {
                        formattedBody = formattedBody.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedBody = formattedBody.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedBody = formattedBody.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedBody = formattedBody.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedBody = formattedBody.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedBody = formattedBody.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedBody = formattedBody.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }
                WhatsappController oWhatsappController = new WhatsappController();

                string[] arr1 = { "1", "", "" };

                if (ReminderTo == "Customer" || ReminderTo == "Customer + Me")
                {
                    arr1 = oWhatsappController.WhatsappFunction(CompanyId, Sale.CustomerMobileNo, formattedBody, 0);
                    if (arr1[0] == "2")
                    {
                        return arr1;
                    }
                }

                if (arr1[0] == "1")
                {
                    if (ReminderTo == "Me" || ReminderTo == "Customer + Me")
                    {
                        return oWhatsappController.WhatsappFunction(CompanyId, Sale.CompanyMobileNo, formattedBody, 0);
                    }
                }
            }
            string[] arr = { "1", "", "" };
            return arr;
        }

        public dynamic PurchaseReminder(ClsReminderModulesSettingsVm NotificationModulesSetting, long CompanyId, long Id, string CC, string Bcc, long SmsSettingsId, long EmailSettingsId, long WhatsappSettingsId, string Type, long AddedBy, DateTime CurrentDate, string Domain, string ReminderTo)
        {
            var Sale = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == Id).Select(a => new
            {
                OverdueDays = DbFunctions.DiffDays(CurrentDate, a.DueDate),
                Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? 0 :
                oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                //PaymentStatus = a.PaymentStatus,
                Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Count() == 0 ? a.GrandTotal :
                a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                a.BranchId,
                SupplierName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.Name).FirstOrDefault(),
                SupplierEmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.EmailId).FirstOrDefault(),
                SupplierMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.MobileNo).FirstOrDefault(),
                SourceOfSupply = oConnectionContext.DbClsState.Where(b => b.StateId == a.SourceOfSupplyId).Select(b => b.State).FirstOrDefault(),
                DestinationOfSupply = oConnectionContext.DbClsState.Where(b => b.StateId == a.DestinationOfSupplyId).Select(b => b.State).FirstOrDefault(),
                a.ReferenceNo,
                InvoiceUrl = webUrl + "/purchase/invoice?InvoiceId=" + a.InvoiceId,
                a.PurchaseDate,
                PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(b => b.PaymentTermId == a.PaymentTermId).Select(b => b.PaymentTerm).FirstOrDefault(),
                a.DueDate,
                a.IsReverseCharge,
                a.Notes,
                a.Subtotal,
                a.TotalDiscount,
                a.TotalTaxAmount,
                a.NetAmount,
                a.RoundOff,
                a.SpecialDiscount,
                a.GrandTotal,
                CompanyEmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CompanyId).Select(b => b.EmailId).FirstOrDefault(),
                CompanyMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CompanyId).Select(b => b.MobileNo).FirstOrDefault(),
            }).FirstOrDefault();

            var Branch = oConnectionContext.DbClsBranch.Where(a => a.BranchId == Sale.BranchId).Select(a => new
            {
                BusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == CompanyId).Select(b => b.BusinessName).FirstOrDefault(),
                BusinessLogo = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == CompanyId).Select(b => b.BusinessLogo).FirstOrDefault(),
                a.Branch,
                a.BranchCode,
                a.Address,
                a.Zipcode,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                //ContactPersonName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ContactPersonId).Select(b => b.Name).FirstOrDefault(),
                //ContactPersonMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ContactPersonId).Select(b => b.MobileNo).FirstOrDefault(),
                a.Mobile,
                a.AltMobileNo,
                a.Email,
                BusinessRegistrationNo = oConnectionContext.DbClsTaxSetting.Where(b => b.TaxSettingId == a.TaxSettingId).Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessRegistrationName = oConnectionContext.DbClsBusinessRegistrationName.Where(c => c.BusinessRegistrationNameId ==
                oConnectionContext.DbClsTaxSetting.Where(b => b.TaxSettingId == a.TaxSettingId).Select(b => b.BusinessRegistrationNameId).FirstOrDefault()).Select(c => c.Name).FirstOrDefault()
            }).FirstOrDefault();

            if (SmsSettingsId != 0)
            {
                string formattedBody = NotificationModulesSetting.SmsBody;
                string[] array = NotificationModulesSetting.SmsBody.Split('{');
                List<string> list = new List<string>();

                foreach (var item in array)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list)
                {
                    if (item == "Business Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedBody = formattedBody.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedBody = formattedBody.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Supplier Name")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Name}", Sale.SupplierName);
                    }
                    else if (item == "Supplier Mobile No")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Mobile No}", Sale.SupplierMobileNo);
                    }
                    else if (item == "Supplier Email")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Email}", Sale.SupplierEmailId);
                    }
                    else if (item == "Source Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Source Of Supply}", Sale.SourceOfSupply);
                    }
                    else if (item == "Destination Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Destination Of Supply}", Sale.DestinationOfSupply);
                    }
                    else if (item == "Bill No")
                    {
                        formattedBody = formattedBody.Replace("{Bill No}", Sale.ReferenceNo.ToString());
                    }
                    else if (item == "Bill Url")
                    {
                        formattedBody = formattedBody.Replace("{Bill Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Bill Date")
                    {
                        formattedBody = formattedBody.Replace("{Bill Date}", Sale.PurchaseDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedBody = formattedBody.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedBody = formattedBody.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Is Reverse Charge")
                    {
                        formattedBody = formattedBody.Replace("{Is Reverse Charge}", Sale.IsReverseCharge.ToString());
                    }
                    else if (item == "Notes")
                    {
                        formattedBody = formattedBody.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedBody = formattedBody.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedBody = formattedBody.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedBody = formattedBody.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedBody = formattedBody.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedBody = formattedBody.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedBody = formattedBody.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }

                SmsController oSmsController = new SmsController();
                if (ReminderTo == "Customer" || ReminderTo == "Customer + Me")
                {
                    oSmsController.SmsFunction(CompanyId, Sale.SupplierMobileNo, formattedBody, AddedBy, CurrentDate, Id, Type, Domain, 0);
                }
                if (ReminderTo == "Me" || ReminderTo == "Customer + Me")
                {
                    oSmsController.SmsFunction(CompanyId, Sale.CompanyMobileNo, formattedBody, AddedBy, CurrentDate, Id, Type, Domain, 0);
                }
            }
            if (EmailSettingsId != 0)
            {
                #region Email Subject
                string formattedSubject = "";

                formattedSubject = NotificationModulesSetting.EmailSubject;
                string[] arr1 = NotificationModulesSetting.EmailSubject.Split('{');
                List<string> list1 = new List<string>();

                foreach (var item in arr1)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list1.Add(item.Split('}')[0]);
                    }
                }


                foreach (var item in list1)
                {
                    if (item == "Business Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedSubject = formattedSubject.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Supplier Name")
                    {
                        formattedSubject = formattedSubject.Replace("{Supplier Name}", Sale.SupplierName);
                    }
                    else if (item == "Supplier Mobile No")
                    {
                        formattedSubject = formattedSubject.Replace("{Supplier Mobile No}", Sale.SupplierMobileNo);
                    }
                    else if (item == "Supplier Email")
                    {
                        formattedSubject = formattedSubject.Replace("{Supplier Email}", Sale.SupplierEmailId);
                    }
                    else if (item == "Source Of Supply")
                    {
                        formattedSubject = formattedSubject.Replace("{Source Of Supply}", Sale.SourceOfSupply);
                    }
                    else if (item == "Destination Of Supply")
                    {
                        formattedSubject = formattedSubject.Replace("{Destination Of Supply}", Sale.DestinationOfSupply);
                    }
                    else if (item == "Bill No")
                    {
                        formattedSubject = formattedSubject.Replace("{Bill No}", Sale.ReferenceNo.ToString());
                    }
                    else if (item == "Bill Url")
                    {
                        formattedSubject = formattedSubject.Replace("{Bill Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Bill Date")
                    {
                        formattedSubject = formattedSubject.Replace("{Bill Date}", Sale.PurchaseDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedSubject = formattedSubject.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedSubject = formattedSubject.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Is Reverse Charge")
                    {
                        formattedSubject = formattedSubject.Replace("{Is Reverse Charge}", Sale.IsReverseCharge.ToString());
                    }
                    else if (item == "Notes")
                    {
                        formattedSubject = formattedSubject.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedSubject = formattedSubject.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedSubject = formattedSubject.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedSubject = formattedSubject.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedSubject = formattedSubject.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }
                #endregion


                #region EmailBody
                string formattedBody = NotificationModulesSetting.EmailBody;
                string[] array = NotificationModulesSetting.EmailBody.Split('{');
                List<string> list = new List<string>();

                foreach (var item in array)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list)
                {
                    if (item == "Business Logo")
                    {
                        formattedBody = formattedBody.Replace("{Business Logo}", "<img src='" + webUrl + Branch.BusinessLogo + "' />");
                    }
                    else if (item == "Business Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedBody = formattedBody.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedBody = formattedBody.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Supplier Name")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Name}", Sale.SupplierName);
                    }
                    else if (item == "Supplier Mobile No")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Mobile No}", Sale.SupplierMobileNo);
                    }
                    else if (item == "Supplier Email")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Email}", Sale.SupplierEmailId);
                    }
                    else if (item == "Source Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Source Of Supply}", Sale.SourceOfSupply);
                    }
                    else if (item == "Destination Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Destination Of Supply}", Sale.DestinationOfSupply);
                    }
                    else if (item == "Bill No")
                    {
                        formattedBody = formattedBody.Replace("{Bill No}", Sale.ReferenceNo.ToString());
                    }
                    else if (item == "Bill Url")
                    {
                        formattedBody = formattedBody.Replace("{Bill Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Bill Date")
                    {
                        formattedBody = formattedBody.Replace("{Bill Date}", Sale.PurchaseDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedBody = formattedBody.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedBody = formattedBody.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Is Reverse Charge")
                    {
                        formattedBody = formattedBody.Replace("{Is Reverse Charge}", Sale.IsReverseCharge.ToString());
                    }
                    else if (item == "Notes")
                    {
                        formattedBody = formattedBody.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedBody = formattedBody.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedBody = formattedBody.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedBody = formattedBody.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedBody = formattedBody.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedBody = formattedBody.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedBody = formattedBody.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }
                #endregion


                EmailController oEmailController = new EmailController();
                if (ReminderTo == "Customer" || ReminderTo == "Customer + Me")
                {
                    oEmailController.NotificationTemplate(CompanyId, Sale.SupplierEmailId, formattedSubject, formattedBody, CC, Bcc, AddedBy, CurrentDate, Id, Type, Domain);
                }
                if (ReminderTo == "Me" || ReminderTo == "Customer + Me")
                {
                    oEmailController.NotificationTemplate(CompanyId, Sale.CompanyEmailId, formattedSubject, formattedBody, CC, Bcc, AddedBy, CurrentDate, Id, Type, Domain);
                }
            }
            if (WhatsappSettingsId != 0)
            {
                string formattedBody = NotificationModulesSetting.WhatsappBody;
                string[] array = NotificationModulesSetting.WhatsappBody.Split('{');
                List<string> list = new List<string>();

                foreach (var item in array)
                {
                    if (item.IndexOf('}') > -1)
                    {
                        list.Add(item.Split('}')[0]);
                    }
                }

                foreach (var item in list)
                {
                    if (item == "Business Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Name}", Branch.BusinessName);
                    }
                    else if (item == "Business Location Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Name}", Branch.Branch);
                    }
                    else if (item == "Business Location Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Contact No}", Branch.Mobile);
                    }
                    else if (item == "Business Location Alternative Contact No")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Alternative Contact No}", Branch.AltMobileNo);
                    }
                    else if (item == "Business Location Email Id")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Email Id}", Branch.Email);
                    }
                    else if (item == "Business Registration Name")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration Name}", Branch.BusinessRegistrationName);
                    }
                    else if (item == "Business Registration No")
                    {
                        formattedBody = formattedBody.Replace("{Business Registration No}", Branch.BusinessRegistrationNo);
                    }
                    else if (item == "Business Location Country")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Country}", Branch.Country);
                    }
                    else if (item == "Business Location State")
                    {
                        formattedBody = formattedBody.Replace("{Business Location State}", Branch.State);
                    }
                    else if (item == "Business Location City")
                    {
                        formattedBody = formattedBody.Replace("{Business Location City}", Branch.City);
                    }
                    else if (item == "Business Location Zip Code")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Zip Code}", Branch.Zipcode);
                    }
                    else if (item == "Business Location Address")
                    {
                        formattedBody = formattedBody.Replace("{Business Location Address}", Branch.Address);
                    }
                    else if (item == "Supplier Name")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Name}", Sale.SupplierName);
                    }
                    else if (item == "Supplier Mobile No")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Mobile No}", Sale.SupplierMobileNo);
                    }
                    else if (item == "Supplier Email")
                    {
                        formattedBody = formattedBody.Replace("{Supplier Email}", Sale.SupplierEmailId);
                    }
                    else if (item == "Source Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Source Of Supply}", Sale.SourceOfSupply);
                    }
                    else if (item == "Destination Of Supply")
                    {
                        formattedBody = formattedBody.Replace("{Destination Of Supply}", Sale.DestinationOfSupply);
                    }
                    else if (item == "Bill No")
                    {
                        formattedBody = formattedBody.Replace("{Bill No}", Sale.ReferenceNo.ToString());
                    }
                    else if (item == "Bill Url")
                    {
                        formattedBody = formattedBody.Replace("{Bill Url}", Sale.InvoiceUrl);
                    }
                    else if (item == "Bill Date")
                    {
                        formattedBody = formattedBody.Replace("{Bill Date}", Sale.PurchaseDate.ToString());
                    }
                    else if (item == "Payment Term")
                    {
                        formattedBody = formattedBody.Replace("{Payment Term}", Sale.PaymentTerm);
                    }
                    else if (item == "Due Date")
                    {
                        formattedBody = formattedBody.Replace("{Due Date}", Sale.DueDate.ToString());
                    }
                    else if (item == "Is Reverse Charge")
                    {
                        formattedBody = formattedBody.Replace("{Is Reverse Charge}", Sale.IsReverseCharge.ToString());
                    }
                    else if (item == "Notes")
                    {
                        formattedBody = formattedBody.Replace("{Notes}", Sale.Notes);
                    }
                    else if (item == "Gross Amount")
                    {
                        formattedBody = formattedBody.Replace("{Gross Amount}", Sale.Subtotal.ToString());
                    }
                    else if (item == "Discount")
                    {
                        formattedBody = formattedBody.Replace("{Discount}", Sale.TotalDiscount.ToString());
                    }
                    else if (item == "Tax Amount")
                    {
                        formattedBody = formattedBody.Replace("{Tax Amount}", Sale.TotalTaxAmount.ToString());
                    }
                    else if (item == "Net Amount")
                    {
                        formattedBody = formattedBody.Replace("{Net Amount}", Sale.NetAmount.ToString());
                    }
                    else if (item == "Round Off")
                    {
                        formattedBody = formattedBody.Replace("{Round Off}", Sale.RoundOff.ToString());
                    }
                    else if (item == "Total Amount")
                    {
                        formattedBody = formattedBody.Replace("{Total Amount}", Sale.GrandTotal.ToString());
                    }
                    else if (item == "Paid Amount")
                    {
                        formattedBody = formattedBody.Replace("{Paid Amount}", Sale.Paid.ToString());
                    }
                    else if (item == "Due Amount")
                    {
                        formattedBody = formattedBody.Replace("{Due Amount}", Sale.Due.ToString());
                    }
                    else if (item == "Overdue Days")
                    {
                        formattedBody = formattedBody.Replace("{Overdue Days}", Sale.OverdueDays.ToString());
                    }
                }

                WhatsappController oWhatsappController = new WhatsappController();

                string[] arr1 = { "1", "", "" };

                if (ReminderTo == "Supplier" || ReminderTo == "Supplier + Me")
                {
                    arr1 = oWhatsappController.WhatsappFunction(CompanyId, Sale.SupplierMobileNo, formattedBody, 0);
                    if (arr1[0] == "2")
                    {
                        return arr1;
                    }
                }

                if (arr1[0] == "1")
                {
                    if (ReminderTo == "Me" || ReminderTo == "Supplier + Me")
                    {
                        return oWhatsappController.WhatsappFunction(CompanyId, Sale.CompanyMobileNo, formattedBody, 0);
                    }
                }
            }
            string[] arr = { "1", "", "" };
            return arr;
        }

    }
}
