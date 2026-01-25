using EquiBillBook.Controllers.WebApi.Common;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Twilio.Rest;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class NotificationModulesController : ApiController
    {
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> ActiveNotificationModules(ClsNotificationModulesVm obj)
        {
            var det = oConnectionContext.DbClsNotificationModules.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.NotificationModulesId,
                a.Sequence,
                a.Title,
                a.Name,
                NotificationModulesDetails = oConnectionContext.DbClsNotificationModulesDetails.Where(b => b.IsActive == true && b.IsDeleted == false &&
                b.NotificationModulesId == a.NotificationModulesId)
                .Select(b => new
                {
                    b.Name,
                    b.Title,
                    b.NotificationModulesId,
                    b.NotificationModulesDetailsId,
                    b.Sequence,
                    b.AvailableTags,
                    NotificationTemplates = oConnectionContext.DbClsNotificationTemplates.Where(bb => bb.IsActive == true && bb.IsDeleted == false
                    && bb.NotificationModulesDetailsId == b.NotificationModulesDetailsId).Select(bb => new
                    {
                        bb.Name,
                        bb.Title,
                        bb.Sequence,
                        bb.NotificationModulesId,
                        bb.NotificationModulesDetailsId,
                        bb.NotificationTemplatesId,
                        NotificationModulesSetting = oConnectionContext.DbClsNotificationModulesSettings.Where(c => c.IsActive == true && c.IsDeleted == false &&
                           c.NotificationModulesId == b.NotificationModulesId && c.NotificationModulesDetailsId == b.NotificationModulesDetailsId
                           && c.NotificationTemplatesId == bb.NotificationTemplatesId
                           && c.CompanyId == obj.CompanyId).Select(c => new
                           {
                               c.AutoSendEmail,
                               c.EmailSubject,
                               c.CC,
                               c.BCC,
                               c.EmailBody,
                               c.AutoSendSms,
                               c.SmsBody,
                               c.AutoSendWhatsapp,
                               c.WhatsappBody
                           }).FirstOrDefault()
                    }).OrderBy(bb=>bb.Sequence)

                }).OrderBy(b => b.Sequence),
            }).OrderBy(a => a.Sequence).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    NotificationModules = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SendTestEmail(ClsEmailSettingsVm obj)
        {
            string TestEmailId = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId
            && a.EmailSettingsId == obj.EmailSettingsId).Select(a => a.TestEmailId).FirstOrDefault();

            EmailController oEmailController = new EmailController();

            string[] arr = oEmailController.Test(obj.CompanyId, TestEmailId, "Email Setup", oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessLogo).FirstOrDefault(), obj.Domain, obj.EmailSettingsId);

            if (arr[0] == "0")
            {
                data = new
                {
                    Status = 0,
                    Message = arr[1],
                    Data = new
                    {

                    }
                };
            }
            else
            {
                data = new
                {
                    Status = 1,
                    Message = "Email sent successfully to " + TestEmailId,
                    Data = new
                    {

                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SendTestSms(ClsSmsSettingsVm obj)
        {
            string TestMobileNo = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId
            && a.SmsSettingsId == obj.SmsSettingsId).Select(a => a.TestMobileNo).FirstOrDefault();

            SmsController oSmsController = new SmsController();

            CommonController oCommonController = new CommonController();
            string valid = string.Empty;
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault(),
                a.WebsiteUrl
            }).FirstOrDefault();

            string[] arr = oSmsController.SmsFunction(obj.CompanyId, TestMobileNo, "Congratulations, your sms setup is ready. Now you can send sms to your customers. Thanks, Team "+ BusinessSetting.ParentBusinessName, 0, DateTime.Now, 0, "", obj.Domain, obj.SmsSettingsId);

            if (arr[0] == "0")
            {
                data = new
                {
                    Status = 0,
                    Message = arr[1],
                    Data = new
                    {

                    }
                };
            }
            else
            {
                data = new
                {
                    Status = 1,
                    Message = "Sms sent successfully to " + TestMobileNo,
                    Data = new
                    {

                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SendTestWhatsapp(ClsWhatsappSettingsVm obj)
        {
            string TestMobileNo = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId
            && a.WhatsappSettingsId == obj.WhatsappSettingsId).Select(a => a.TestMobileNo).FirstOrDefault();

            WhatsappController oWhatsappController = new WhatsappController();

            CommonController oCommonController = new CommonController();
            string valid = string.Empty;
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault(),
                a.WebsiteUrl
            }).FirstOrDefault();

            string[] arr = oWhatsappController.WhatsappFunction(obj.CompanyId, TestMobileNo, "Congratulations, your whatsapp setup is ready. Now you can send whatsapp to your customers. Thanks, Team "+ BusinessSetting.ParentBusinessName, obj.WhatsappSettingsId);

            if (arr[0] == "0")
            {
                data = new
                {
                    Status = 0,
                    Message = arr[1],
                    Data = new
                    {

                    }
                };
            }
            else
            {
                data = new
                {
                    Status = 1,
                    Message = "Whatsapp sent successfully to " + TestMobileNo,
                    WhatsappUrl = arr[2],
                    Data = new
                    {

                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchNotificationModule(ClsNotificationModulesVm obj)
        {
            string Status = oCommonController.fetchStatus(obj.CompanyId, obj.Name, obj.Id);

            var NotificationModulesDetail = oConnectionContext.DbClsNotificationModulesDetails.Where(b => b.IsActive == true && b.IsDeleted == false &&
                b.Name.ToLower() == obj.Name.ToLower())
                .Select(b => new
                {
                    b.Name,
                    b.Title,
                    b.NotificationModulesId,
                    b.NotificationModulesDetailsId,
                    b.Sequence,
                    b.AvailableTags,
                    NotificationTemplates = oConnectionContext.DbClsNotificationTemplates.Where(bb => bb.IsActive == true && bb.IsDeleted == false
                    && bb.NotificationModulesDetailsId == b.NotificationModulesDetailsId && bb.Name == Status).Select(bb => new
                    {
                        bb.Name,
                        bb.Title,
                        bb.Sequence,
                        bb.NotificationModulesId,
                        bb.NotificationModulesDetailsId,
                        bb.NotificationTemplatesId,
                        NotificationModulesSetting = oConnectionContext.DbClsNotificationModulesSettings.Where(c => c.IsActive == true && c.IsDeleted == false &&
                           c.NotificationModulesId == b.NotificationModulesId && c.NotificationModulesDetailsId == b.NotificationModulesDetailsId
                           && c.NotificationTemplatesId == bb.NotificationTemplatesId
                           && c.CompanyId == obj.CompanyId).Select(c => new
                           {
                               c.AutoSendEmail,
                               c.EmailSubject,
                               c.CC,
                               c.BCC,
                               c.EmailBody,
                               c.AutoSendSms,
                               c.SmsBody,
                               c.AutoSendWhatsapp,
                               c.WhatsappBody
                           }).FirstOrDefault()
                    }).OrderBy(bb=>bb.Sequence)
                }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    NotificationModulesDetail = NotificationModulesDetail,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SendNotifications(ClsNotificationModulesSettingsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            //ClsNotificationModulesSettingsVm NotificationModulesSetting = new ClsNotificationModulesSettingsVm()
            //{
            //    AutoSendEmail = obj.AutoSendEmail,
            //    AutoSendSms = obj.AutoSendSms,
            //    AutoSendWhatsapp = obj.AutoSendWhatsapp,
            //    EmailSubject = obj.EmailSubject,
            //    CC = obj.CC,
            //    BCC = obj.BCC,
            //    EmailBody = obj.EmailBody == null ? "" : obj.EmailBody,
            //    SmsBody = obj.SmsBody == null ? "" : obj.SmsBody,
            //    WhatsappBody = obj.WhatsappBody == null ? "" : obj.WhatsappBody,
            //};

            long SmsSettingsId = 0, EmailSettingsId = 0, WhatsappSettingsId = 0;
            if (obj.AutoSendSms == true)
            {
                SmsSettingsId = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.SmsSettingsId).FirstOrDefault();
            }

            if (obj.AutoSendEmail == true)
            {
                EmailSettingsId = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.EmailSettingsId).FirstOrDefault();
            }

            if (obj.AutoSendWhatsapp == true)
            {
                WhatsappSettingsId = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDefault == true && a.IsDeleted == false && a.IsActive == true).Select(a => a.WhatsappSettingsId).FirstOrDefault();
            }

            string[] arr = { "", "", "" };
            if (obj.Name == "Sales Quotation")
            {
                arr = oNotificationTemplatesController.SalesQuotation(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Sales Order")
            {
                arr = oNotificationTemplatesController.SalesOrder(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Sales Proforma")
            {
                arr = oNotificationTemplatesController.SalesProforma(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Delivery Challan")
            {
                arr = oNotificationTemplatesController.DeliveryChallan(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Sales Invoice")
            {
                arr = oNotificationTemplatesController.SalesInvoice(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Sales Debit Note")
            {
                arr = oNotificationTemplatesController.SalesDebitNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Bill Of Supply")
            {
                arr = oNotificationTemplatesController.BillOfSupply(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Pos")
            {
                arr = oNotificationTemplatesController.Pos(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Sales Credit Note")
            {
                arr = oNotificationTemplatesController.SalesCreditNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Payment Link")
            {
                arr = oNotificationTemplatesController.PaymentLink(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Sales Invoice Payment")
            {
                arr = oNotificationTemplatesController.SalesInvoicePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Customer Advance Payment")
            {
                arr = oNotificationTemplatesController.CustomerAdvancePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Refund From Credit Note")
            {
                arr = oNotificationTemplatesController.RefundFromCreditNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Refund From Customer Advance Payment")
            {
                arr = oNotificationTemplatesController.RefundFromCustomerAdvancePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Credits Applied From Credit Note")
            {
                arr = oNotificationTemplatesController.CreditsAppliedFromCreditNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Credits Applied From Customer Advance Payment")
            {
                arr = oNotificationTemplatesController.CreditsAppliedFromCustomerAdvancePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Purchase Quotation")
            {
                arr = oNotificationTemplatesController.PurchaseQuotation(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Purchase Order")
            {
                arr = oNotificationTemplatesController.PurchaseOrder(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Purchase Bill")
            {
                arr = oNotificationTemplatesController.PurchaseBill(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Purchase Debit Note")
            {
                arr = oNotificationTemplatesController.PurchaseDebitNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Purchase Bill Payment")
            {
                arr = oNotificationTemplatesController.PurchaseBillPayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Supplier Advance Payment")
            {
                arr = oNotificationTemplatesController.SupplierAdvancePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Refund From Debit Note")
            {
                arr = oNotificationTemplatesController.RefundFromDebitNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Refund From Supplier Advance Payment")
            {
                arr = oNotificationTemplatesController.RefundFromSupplierAdvancePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Credits Applied From Debit Note")
            {
                arr = oNotificationTemplatesController.CreditsAppliedFromDebitNote(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }
            else if (obj.Name == "Credits Applied From Supplier Advance Payment")
            {
                arr = oNotificationTemplatesController.CreditsAppliedFromSupplierAdvancePayment(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain);
            }

            data = new
            {
                Status = 1,
                Message = "Notification sent successfully",
                WhatsappUrl = arr[2],
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }

    }
}