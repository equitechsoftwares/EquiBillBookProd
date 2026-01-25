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
    public class EnquiryController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;
        public async Task<IHttpActionResult> InsertEnquiry(ClsEnquiryVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null || obj.EmailId == "" || obj.EmailId == null
                || obj.Subject == "" || obj.Subject == null || obj.Message == "" || obj.Message == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "All fields are required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Mobile No",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Email Id",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                ClsEnquiry oClsEnquiry = new ClsEnquiry()
                {
                    Name = obj.Name,
                    EmailId = obj.EmailId,
                    AddedOn = CurrentDate,
                    AddedBy = obj.AddedBy,
                    IsActive = true,
                    IsDeleted = false,
                    MobileNo = obj.MobileNo,
                    Subject = obj.Subject,
                    Message = obj.Message
                };
                oConnectionContext.DbClsEnquiry.Add(oClsEnquiry);
                oConnectionContext.SaveChanges();

                string myString = "Name: " + obj.Name + "<br />" +
                "Email Id: " + obj.EmailId + "<br />" +
                "Mobile No: " + obj.MobileNo + "<br />" +
                  "Subject: " + obj.Subject + "<br />" +
                "Message: " + obj.Message;

                oEmailController.NotificationTemplate(0, "info@equibillbook.com", "New enquiry from "+ obj.Name, myString,"equitechsoftwares@gmail.com","",0,CurrentDate,0,"",obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Enquiry sent successfully. Our team will get back to you soon",
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
