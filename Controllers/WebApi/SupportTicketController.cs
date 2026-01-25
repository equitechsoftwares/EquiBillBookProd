using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
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
    public class SupportTicketController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        EmailController oEmailController = new EmailController();

        public async Task<IHttpActionResult> AllSupportTickets(ClsSupportTicketVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSupportTicket.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                SupportTicketId = a.SupportTicketId,
                a.HelpTopic,
                a.Subject,
                a.SupportTicketNo,
                a.Attachment1,
                a.Attachment2,
                a.Attachment3,
                a.Attachment4,
                a.Attachment5,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SupportTicketNo.ToLower().Contains(obj.Search.ToLower()) || a.Subject.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupportTickets = det.OrderByDescending(a => a.SupportTicketId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupportTicket(ClsSupportTicket obj)
        {
            var det = oConnectionContext.DbClsSupportTicket.Where(a => a.SupportTicketId == obj.SupportTicketId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SupportTicketId = a.SupportTicketId,
                a.SupportTicketNo,
                a.HelpTopic,
                a.Subject,
                a.Attachment1,
                a.Attachment2,
                a.Attachment3,
                a.Attachment4,
                a.Attachment5,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                SupportTicketDetails = oConnectionContext.DbClsSupportTicketDetails.Where(b => b.SupportTicketId == a.SupportTicketId).Select(b => new
                {
                    b.Message,
                    b.AddedOn,
                    b.IsFromCompany,
                    b.FromUserId,
                    b.ToUserId,
                    From = oConnectionContext.DbClsUser.Where(c => c.UserId == b.FromUserId).Select(c => c.Name).FirstOrDefault(),
                    To = oConnectionContext.DbClsUser.Where(c => c.UserId == b.ToUserId).Select(c => c.Name).FirstOrDefault(),
                }).ToList()
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupportTicket = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSupportTicket(ClsSupportTicketVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.HelpTopic == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divHelpTopic" });
                    isError = true;
                }

                if (obj.Subject == null || obj.Subject == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubject" });
                    isError = true;
                }

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

                obj.SupportTicketNo = Convert.ToString(DateTime.Now.ToFileTime());

                ClsSupportTicket oClsSupportTicket = new ClsSupportTicket()
                {
                    SupportTicketNo = obj.SupportTicketNo,
                    HelpTopic = obj.HelpTopic,
                    Subject = obj.Subject,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };

                if (obj.Attachment1 != "" && obj.Attachment1 != null && !obj.Attachment1.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SupportTicket/1" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.Attachment1Extension;

                    string base64 = obj.Attachment1.Replace(obj.Attachment1.Substring(0, obj.Attachment1.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SupportTicket/1");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.Attachment1.Replace(obj.Attachment1.Substring(0, obj.Attachment1.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupportTicket.Attachment1 = filepathPass;
                }

                if (obj.Attachment2 != "" && obj.Attachment2 != null && !obj.Attachment2.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SupportTicket/2" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.Attachment2Extension;

                    string base64 = obj.Attachment2.Replace(obj.Attachment2.Substring(0, obj.Attachment2.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SupportTicket/2");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.Attachment2.Replace(obj.Attachment2.Substring(0, obj.Attachment2.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupportTicket.Attachment2 = filepathPass;
                }

                if (obj.Attachment3 != "" && obj.Attachment3 != null && !obj.Attachment3.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SupportTicket/3" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.Attachment3Extension;

                    string base64 = obj.Attachment3.Replace(obj.Attachment3.Substring(0, obj.Attachment3.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SupportTicket/3");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.Attachment3.Replace(obj.Attachment3.Substring(0, obj.Attachment3.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupportTicket.Attachment3 = filepathPass;
                }

                if (obj.Attachment4 != "" && obj.Attachment4 != null && !obj.Attachment4.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SupportTicket/4" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.Attachment4Extension;

                    string base64 = obj.Attachment4.Replace(obj.Attachment4.Substring(0, obj.Attachment4.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SupportTicket/4");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.Attachment4.Replace(obj.Attachment4.Substring(0, obj.Attachment4.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupportTicket.Attachment4 = filepathPass;
                }

                if (obj.Attachment5 != "" && obj.Attachment5 != null && !obj.Attachment5.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/SupportTicket/5" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.Attachment5Extension;

                    string base64 = obj.Attachment5.Replace(obj.Attachment5.Substring(0, obj.Attachment5.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/SupportTicket/5");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.Attachment5.Replace(obj.Attachment5.Substring(0, obj.Attachment5.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupportTicket.Attachment5 = filepathPass;
                }

                oConnectionContext.DbClsSupportTicket.Add(oClsSupportTicket);
                oConnectionContext.SaveChanges();

                ClsSupportTicketDetails oClsSupportTicketDetails = new ClsSupportTicketDetails()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    FromUserId = obj.AddedBy,
                    IsActive = true,
                    IsDeleted = false,
                    Message = obj.Message,
                    SupportTicketId = oClsSupportTicket.SupportTicketId,
                    ToUserId = 1,
                    IsFromCompany = false
                };
                oConnectionContext.DbClsSupportTicketDetails.Add(oClsSupportTicketDetails);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Support Ticket",
                    CompanyId = obj.CompanyId,
                    Description = "Support Ticket \"" + obj.SupportTicketNo+"\" created",
                    Id = oClsSupportTicket.SupportTicketId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Send confirmation email to customer
                var customerInfo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => new { a.Name, a.EmailId }).FirstOrDefault();
                oEmailController.SupportTicketConfirmation(customerInfo.EmailId, "Support Ticket Created: " + obj.SupportTicketNo, obj.SupportTicketNo, obj.Subject, customerInfo.Name, "Your support ticket has been successfully created. We have received your request and our team will review it shortly.", obj.Message, obj.Domain);

                // Send notification email to admin
                long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
                {
                    a.CompanyId,
                    BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
                }).FirstOrDefault();
                oEmailController.SupportTicketNew(BusinessSetting.BusinessEmail, "New Support Ticket: " + obj.SupportTicketNo, obj.SupportTicketNo, obj.Subject, customerInfo.Name, obj.Message, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Ticket created successfully",
                    Data = new
                    {
                        SupportTicket = oClsSupportTicket
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllSupportTicketsAdmin(ClsSupportTicketVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.UserType).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsSupportTicket
                       join b in oConnectionContext.DbClsUser
on a.CompanyId equals b.UserId
                       where b.Under == obj.Under && a.IsDeleted == false
                       select new
                       {
                           ResellerId = b.ResellerId,
                           b.Under,
                           SupportTicketId = a.SupportTicketId,
                           a.HelpTopic,
                           a.Subject,
                           a.SupportTicketNo,
                           a.Attachment1,
                           a.Attachment2,
                           a.Attachment3,
                           a.Attachment4,
                           a.Attachment5,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           CompanyId = a.CompanyId,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.EmailId).FirstOrDefault(),
                       }).ToList();

            if (UserType.ToLower() == "reseller")
            {
                det = det.Where(a => a.ResellerId == obj.AddedBy).Select(a => a).ToList();
            }

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SupportTicketNo.ToLower().Contains(obj.Search.ToLower()) || a.Subject.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupportTickets = det.OrderByDescending(a => a.SupportTicketId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupportTicketAdmin(ClsSupportTicketVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

            var det = (from a in oConnectionContext.DbClsSupportTicket
                       join b in oConnectionContext.DbClsUser
on a.CompanyId equals b.UserId
                       where b.Under == obj.Under && 
                       a.IsDeleted == false && a.SupportTicketId == obj.SupportTicketId
                       select new
                       {
                           SupportTicketId = a.SupportTicketId,
                           a.SupportTicketNo,
                           a.HelpTopic,
                           a.Subject,
                           a.Attachment1,
                           a.Attachment2,
                           a.Attachment3,
                           a.Attachment4,
                           a.Attachment5,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           CompanyId = a.CompanyId,
                           SupportTicketDetails = oConnectionContext.DbClsSupportTicketDetails.Where(b => b.SupportTicketId == a.SupportTicketId).Select(b => new
                           {
                               b.Message,
                               b.AddedOn,
                               b.IsFromCompany,
                               b.FromUserId,
                               b.ToUserId,
                               From = oConnectionContext.DbClsUser.Where(c => c.UserId == b.FromUserId).Select(c => c.Name).FirstOrDefault(),
                               To = oConnectionContext.DbClsUser.Where(c => c.UserId == b.ToUserId).Select(c => c.Name).FirstOrDefault(),
                           }).ToList()
                       }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SupportTicket = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CloseSupportTicketAdmin(ClsSupportTicketVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SupportTicketId == 0)
                {
                    errors.Add(new ClsError { Message = "Support Ticket ID is required", Id = "divSupportTicketId" });
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

                if (obj.Under == 0)
                {
                    obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                }

                var ticketExists = (from a in oConnectionContext.DbClsSupportTicket
                           join b in oConnectionContext.DbClsUser
    on a.CompanyId equals b.UserId
                           where b.Under == obj.Under &&
                           a.IsDeleted == false && a.SupportTicketId == obj.SupportTicketId
                           select new { 
                           a.IsActive,
                           a.SupportTicketNo
                           }).FirstOrDefault();

                if (ticketExists == null)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "Support Ticket not found",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (ticketExists.IsActive == false)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "Support Ticket is already closed",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsSupportTicket oClsSupportTicket = new ClsSupportTicket()
                {
                    SupportTicketId = obj.SupportTicketId,
                    IsActive = false,
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
                    CompanyId = obj.Under,
                    Description = "Support Ticket \"" + ticketExists.SupportTicketNo + "\" closed",
                    Id = obj.SupportTicketId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                // Send email notification to customer for ticket closed                
                var ticketInfo = oConnectionContext.DbClsSupportTicket.Where(a => a.SupportTicketId == obj.SupportTicketId).Select(a => new { a.CompanyId, a.Subject }).FirstOrDefault();
                var customerInfo = oConnectionContext.DbClsUser.Where(a => a.UserId == ticketInfo.CompanyId && a.IsDeleted == false).Select(a => new { a.Name, a.EmailId }).FirstOrDefault();
                oEmailController.SupportTicketClosed(customerInfo.EmailId, "Support Ticket Closed: " + ticketExists.SupportTicketNo, ticketExists.SupportTicketNo, ticketInfo.Subject, customerInfo.Name, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Support Ticket closed successfully",
                    Data = new
                    {
                        SupportTicket = oClsSupportTicket
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
