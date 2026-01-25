using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
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
    public class SupportTicketDetailsController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        EmailController oEmailController = new EmailController();

        public async Task<IHttpActionResult> InsertSupportTicketDetails(ClsSupportTicketDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Message == null || obj.Message == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMessage" });
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

                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                ClsSupportTicketDetails oClsSupportTicketDetails = new ClsSupportTicketDetails()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    FromUserId = obj.AddedBy,
                    IsActive = true,
                    IsDeleted = false,
                    Message = obj.Message,
                    SupportTicketId = obj.SupportTicketId,
                    ToUserId = obj.Under,
                    IsFromCompany = false
                };
                oConnectionContext.DbClsSupportTicketDetails.Add(oClsSupportTicketDetails);
                oConnectionContext.SaveChanges();

                ClsSupportTicket oClsSupportTicket = new ClsSupportTicket()
                {
                    SupportTicketId = obj.SupportTicketId,
                    IsActive = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };

                oConnectionContext.DbClsSupportTicket.Attach(oClsSupportTicket);
                oConnectionContext.Entry(oClsSupportTicket).Property(x => x.SupportTicketId).IsModified = true;
                oConnectionContext.Entry(oClsSupportTicket).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsSupportTicket).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSupportTicket).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Support Ticket",
                    CompanyId = obj.CompanyId,
                    Description = "Support Ticket \"" + oConnectionContext.DbClsSupportTicket.Where(a => a.SupportTicketId == obj.SupportTicketId).Select(a => a.SupportTicketNo).FirstOrDefault() + "\" replied",
                    Id = oClsSupportTicketDetails.SupportTicketDetailsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Send confirmation email to customer
                var ticketInfo = oConnectionContext.DbClsSupportTicket.Where(a => a.SupportTicketId == obj.SupportTicketId).Select(a => new { a.SupportTicketNo, a.Subject, a.CompanyId }).FirstOrDefault();
                var customerInfo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => new { a.Name, a.EmailId }).FirstOrDefault();                
                oEmailController.SupportTicketConfirmation(customerInfo.EmailId, "Reply Sent: " + ticketInfo.SupportTicketNo, ticketInfo.SupportTicketNo, ticketInfo.Subject, customerInfo.Name, "Your reply has been successfully sent to our support team.", obj.Message, obj.Domain);

                // Send notification email to admin
                long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
                {
                    a.CompanyId,
                    BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
                }).FirstOrDefault();
                oEmailController.SupportTicketReply(BusinessSetting.BusinessEmail, "New Reply on Support Ticket: " + ticketInfo.SupportTicketNo, ticketInfo.SupportTicketNo, ticketInfo.Subject, customerInfo.Name, obj.Message, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Message created successfully",
                    Data = new
                    {
                        //SupportTicket = oClsSupportTicket
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSupportTicketDetailsAdmin(ClsSupportTicketDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Message == null || obj.Message == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMessage" });
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

                obj.ToUserId = oConnectionContext.DbClsSupportTicket.Where(a => a.SupportTicketId == obj.SupportTicketId).Select(a => a.CompanyId).FirstOrDefault();

                ClsSupportTicketDetails oClsSupportTicketDetails = new ClsSupportTicketDetails()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.Under,
                    FromUserId = obj.AddedBy,
                    IsActive = true,
                    IsDeleted = false,
                    Message = obj.Message,
                    SupportTicketId = obj.SupportTicketId,
                    ToUserId = obj.ToUserId,
                    IsFromCompany = true
                };
                oConnectionContext.DbClsSupportTicketDetails.Add(oClsSupportTicketDetails);
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    Category = "Support Ticket",
                //    CompanyId = obj.CompanyId,
                //    Description = "replied to ticket ",
                //    Id = obj.SupportTicketId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Insert"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Send email notification to customer for admin reply
                var ticketInfo = oConnectionContext.DbClsSupportTicket.Where(a => a.SupportTicketId == obj.SupportTicketId).Select(a => new { a.SupportTicketNo, a.Subject, a.CompanyId }).FirstOrDefault();
                var customerInfo = oConnectionContext.DbClsUser.Where(a => a.UserId == ticketInfo.CompanyId && a.IsDeleted == false).Select(a => new { a.Name, a.EmailId }).FirstOrDefault();
                var adminInfo = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.BusinessName }).FirstOrDefault();
                oEmailController.SupportTicketReply(customerInfo.EmailId, "New Reply on Support Ticket: " + ticketInfo.SupportTicketNo, ticketInfo.SupportTicketNo, ticketInfo.Subject, adminInfo.BusinessName, obj.Message, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Message created successfully",
                    Data = new
                    {
                        //SupportTicket = oClsSupportTicket
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
