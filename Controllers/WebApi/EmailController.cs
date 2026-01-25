using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using Twilio.TwiML.Messaging;
using Vonage.Meetings.Common;

namespace EquiBillBook.Controllers.WebApi
{
    public class EmailController : ApiController
    {
        public string[] EmailFunction(long CompanyId, string To, string Subject, string Body, string CC, string Bcc, long AddedBy, DateTime CurrentDate, long Id, string Type, string Domain,long EmailSettingsId)
        {
            ConnectionContext oConnectionContext = new ConnectionContext();
            try
            {
                dynamic EmailSetting = null;
                bool EnableDefaultEmailBranding = false;

                long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                if (CompanyId == 0)
                {
                    EmailSetting = new
                    {
                        SmtpServer = WebConfigurationManager.AppSettings["smtpServer"],
                        SmtpUser = WebConfigurationManager.AppSettings["smtpUser"],
                        SmtpPass = WebConfigurationManager.AppSettings["smtpPass"],
                        SmtpPort = WebConfigurationManager.AppSettings["smtpPort"],
                        EnableSsl = WebConfigurationManager.AppSettings["enableSsl"],
                        FromName = "Equitech Softwares"
                    };
                }
                else
                {
                    EnableDefaultEmailBranding = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == CompanyId).Select(a => a.EnableDefaultEmailBranding).FirstOrDefault();

                    if (EnableDefaultEmailBranding == false)
                    {
                        if(EmailSettingsId == 0)
                        {
                            EmailSetting = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == CompanyId && a.IsDefault == true).Select(a => new
                            {
                                a.SmtpServer,
                                a.SmtpUser,
                                a.SmtpPass,
                                a.SmtpPort,
                                a.EnableSsl,
                                a.FromName
                            }).FirstOrDefault();
                        }
                        else
                        {
                            EmailSetting = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == CompanyId && a.EmailSettingsId == EmailSettingsId).Select(a => new
                            {
                                a.SmtpServer,
                                a.SmtpUser,
                                a.SmtpPass,
                                a.SmtpPort,
                                a.EnableSsl,
                                a.FromName
                            }).FirstOrDefault();
                        }
                    }
                    else
                    {
                        EmailSetting = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == Under && a.IsDefault == true).Select(a => new
                        {
                            a.SmtpServer,
                            a.SmtpUser,
                            a.SmtpPass,
                            a.SmtpPort,
                            a.EnableSsl,
                            a.FromName
                        }).FirstOrDefault();
                        //EmailSetting = new
                        //{
                        //    SmtpServer = WebConfigurationManager.AppSettings["smtpServer"],
                        //    SmtpUser = WebConfigurationManager.AppSettings["smtpUser"],
                        //    SmtpPass = WebConfigurationManager.AppSettings["smtpPass"],
                        //    SmtpPort = WebConfigurationManager.AppSettings["smtpPort"],
                        //    EnableSsl = WebConfigurationManager.AppSettings["enableSsl"],
                        //    FromName = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == CompanyId).Select(a => a.BusinessName).FirstOrDefault()
                        //};
                    }
                }

                MailMessage msg = new MailMessage();

                msg.From = new MailAddress(EmailSetting.SmtpUser, EmailSetting.FromName);
                msg.To.Add(new MailAddress(To));

                if (CC != "" && CC != null)
                {
                    string[] CCArray = CC.Split(',');
                    foreach (var item in CCArray)
                    {
                        msg.CC.Add(new MailAddress(item));
                    }
                }

                if (Bcc != "" && Bcc != null)
                {
                    string[] BccArray = Bcc.Split(',');
                    foreach (var item in BccArray)
                    {
                        msg.Bcc.Add(new MailAddress(item));
                    }
                }

                msg.Subject = Subject;
                msg.Body = Body.ToString();
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.SubjectEncoding = System.Text.Encoding.Default;
                msg.IsBodyHtml = true;

                SmtpClient mySmtpClient = new SmtpClient();
                System.Net.NetworkCredential myCredential = new System.Net.NetworkCredential(EmailSetting.SmtpUser, EmailSetting.SmtpPass);
                mySmtpClient.Host = EmailSetting.SmtpServer;
                mySmtpClient.Port = Convert.ToInt32(EmailSetting.SmtpPort);
                mySmtpClient.EnableSsl = Convert.ToBoolean(EmailSetting.EnableSsl);
                mySmtpClient.UseDefaultCredentials = false;
                mySmtpClient.Credentials = myCredential;

                mySmtpClient.Send(msg);
                msg.Dispose();

                if (EnableDefaultEmailBranding == true)
                {
                    ClsEmailUsed oClsEmailUsed = new ClsEmailUsed()
                    {
                        AddedBy = AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = CompanyId,
                        Id = Id,
                        Type = Type,
                    };
                    oConnectionContext.DbClsEmailUsed.Add(oClsEmailUsed);
                    oConnectionContext.SaveChanges();
                }

                string[] arr = { "1", "" };
                return arr;
            }
            catch (Exception ex)
            {
                string[] arr = { "0", ex.Message };
                return arr;
            }
        }

        public string[] Test(long CompanyId,string To, string Subject, string BusinessLogo, string Domain, long EmailSettingsId)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault(),
                a.WebsiteUrl
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/Test.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);
            myString = myString.Replace("--websiteUrl--", BusinessSetting.WebsiteUrl);

            return EmailFunction(CompanyId, To, Subject, myString,"","",0,DateTime.Now,0,"",Domain, EmailSettingsId);
        }

        public string[] ForgotPassword(string To, string Subject, string ResetUrl, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/ForgotPassword.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--email--", To);
            myString = myString.Replace("--resetUrl--", ResetUrl);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain,0);
        }

        public string[] RegisterOtp(string To, string Subject, string Name, string Otp, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/RegisterOtp.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--name--", Name);
            myString = myString.Replace("--otp--", Otp);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain,0);
        }

        public string[] Welcome(string To, string Subject, string Name, string Url, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/Welcome.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--name--", Name);
            myString = myString.Replace("--url--", Url);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain,0);
        }

        public string[] WelcomeReseller(string To, string Subject, string Name, string Url, string Domain, string JoiningLink)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/WelcomeReseller.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--name--", Name);
            myString = myString.Replace("--url--", Url + "/adminlogin");
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);
            myString = myString.Replace("--joiningLink--", JoiningLink);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain,0);
        }

        public string[] ChangeLoginEmailOtp(string To, string Subject, string Name, string Otp, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/ChangeLoginEmailOtp.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--name--", Name);
            myString = myString.Replace("--otp--", Otp);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain,0);
        }

        public string[] NotificationTemplate(long CompanyId,string To, string Subject, string Body, string CC, string Bcc, long AddedBy, DateTime CurrentDate, long Id, string Type, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();

            //string Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == recurring.CompanyId &&
            //                a.IsDeleted == false && a.IsActive == true).Select(a => a.Domain).FirstOrDefault();

            if (Domain == null || Domain == "")
            {
                long _Under = oConnectionContext.DbClsUser.Where(a => a.UserId == CompanyId).Select(a =>
                a.Under).FirstOrDefault();

                Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == _Under && a.IsDeleted == false &&
                a.IsActive == true).Select(a => a.Domain).FirstOrDefault();
            }

            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/NotificationTemplate.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--body--", Body);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(CompanyId == 0 ? BusinessSetting.CompanyId : CompanyId, To, Subject, myString, CC, Bcc, AddedBy, CurrentDate, Id, Type, Domain,0);
        }

        internal string[] WelcomeWhitelabelReseller(string To, string Subject, string Name, string Url, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/WelcomeWhitelabelReseller.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--name--", Name);
            myString = myString.Replace("--url--", Url + "/adminlogin");
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }

        public string[] SupportTicketNew(string To, string Subject, string TicketNo, string SubjectText, string CustomerName, string Message, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/SupportTicketNew.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--ticketNo--", TicketNo);
            myString = myString.Replace("--subject--", SubjectText);
            myString = myString.Replace("--customerName--", CustomerName);
            myString = myString.Replace("--message--", Message);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }

        public string[] SupportTicketReply(string To, string Subject, string TicketNo, string SubjectText, string FromName, string Message, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/SupportTicketReply.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--ticketNo--", TicketNo);
            myString = myString.Replace("--subject--", SubjectText);
            myString = myString.Replace("--fromName--", FromName);
            myString = myString.Replace("--message--", Message);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }

        public string[] SupportTicketClosed(string To, string Subject, string TicketNo, string SubjectText, string CustomerName, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/SupportTicketClosed.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--ticketNo--", TicketNo);
            myString = myString.Replace("--subject--", SubjectText);
            myString = myString.Replace("--customerName--", CustomerName);
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }

        public string[] SupportTicketConfirmation(string To, string Subject, string TicketNo, string SubjectText, string CustomerName, string ConfirmationMessage, string Message, string Domain)
        {
            CommonController oCommonController = new CommonController();
            string valid = string.Empty;

            ConnectionContext oConnectionContext = new ConnectionContext();
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/SupportTicketConfirmation.html"));
            string readFile = reader.ReadToEnd();
            string myString = "";
            myString = readFile;
            myString = myString.Replace("--logo--", "https://equibillbook.com" + BusinessSetting.BusinessLogo);
            myString = myString.Replace("--ticketNo--", TicketNo);
            myString = myString.Replace("--subject--", SubjectText);
            myString = myString.Replace("--customerName--", CustomerName);
            myString = myString.Replace("--confirmationMessage--", ConfirmationMessage);
            
            string messageSection = "";
            if (!string.IsNullOrEmpty(Message))
            {
                messageSection = "<br /><br /><strong>Your Message:</strong><br />" + Message;
            }
            myString = myString.Replace("--messageSection--", messageSection);
            
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName);
            myString = myString.Replace("--businessEmail--", BusinessSetting.BusinessEmail);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }

        public string[] PublicBookingConfirmation(string To, string Subject, string BookingNo, string CustomerName, DateTime BookingDate, TimeSpan BookingTime, int Duration, int NoOfGuests, string TableNo, string TableName, string SpecialRequest, string CancellationToken, string BusinessName, string ParentBusinessName, string BusinessLogo, string BranchName, string BranchAddress, string BranchMobile, string BranchEmail, string Domain = "")
        {
            CommonController oCommonController = new CommonController();
            ConnectionContext oConnectionContext = new ConnectionContext();
            
            long Under = 0;
            if (string.IsNullOrEmpty(Domain))
            {
                // Try to get from business name or use default
                Under = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.BusinessName == BusinessName || a.ParentBusinessName == ParentBusinessName)
                    .Select(a => a.CompanyId)
                    .FirstOrDefault();
            }
            else
            {
                Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            if (BusinessSetting == null)
            {
                BusinessSetting = new
                {
                    CompanyId = Under,
                    BusinessLogo = BusinessLogo,
                    BusinessName = BusinessName,
                    ParentBusinessName = ParentBusinessName,
                    BusinessEmail = BranchEmail
                };
            }

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Content/Mailers/PublicBookingConfirmation.html"));
            string readFile = reader.ReadToEnd();
            string myString = readFile;

            myString = myString.Replace("--logo--", "https://equibillbook.com" + (BusinessSetting.BusinessLogo ?? BusinessLogo));
            myString = myString.Replace("--customerName--", CustomerName);
            myString = myString.Replace("--bookingNo--", BookingNo);
            myString = myString.Replace("--bookingDate--", BookingDate.ToString("dddd, MMMM dd, yyyy"));
            myString = myString.Replace("--bookingTime--", BookingTime.ToString(@"hh\:mm"));
            myString = myString.Replace("--duration--", Duration.ToString());
            myString = myString.Replace("--noOfGuests--", NoOfGuests.ToString());
            myString = myString.Replace("--tableNo--", TableNo);
            // Format table name(s) - if multiple tables, names are already comma-separated
            string formattedTableName = "";
            if (!string.IsNullOrEmpty(TableName))
            {
                // If TableNo contains commas, it's multiple tables - format names accordingly
                if (TableNo.Contains(","))
                {
                    formattedTableName = "(" + TableName + ")";
                }
                else
                {
                    formattedTableName = "- " + TableName;
                }
            }
            myString = myString.Replace("--tableName--", formattedTableName);
            myString = myString.Replace("--branchName--", BranchName);
            myString = myString.Replace("--branchAddress--", BranchAddress ?? "");
            myString = myString.Replace("--branchMobile--", BranchMobile ?? "");
            myString = myString.Replace("--branchEmail--", BranchEmail ?? "");
            myString = myString.Replace("--parentBusinessName--", BusinessSetting.ParentBusinessName ?? ParentBusinessName);
            myString = myString.Replace("--businessName--", BusinessSetting.BusinessName ?? BusinessName);

            // Status (assuming confirmed for now, can be passed as parameter if needed)
            myString = myString.Replace("--status--", "confirmed");
            myString = myString.Replace("--statusClass--", "status-confirmed");
            myString = myString.Replace("--pendingMessage--", "");

            // Special request section
            string specialRequestSection = "";
            if (!string.IsNullOrEmpty(SpecialRequest))
            {
                specialRequestSection = "<div class=\"detail-row\"><span class=\"detail-label\">Special Request:</span><span class=\"detail-value\">" + SpecialRequest + "</span></div>";
            }
            myString = myString.Replace("--specialRequestSection--", specialRequestSection);

            // Cancellation link
            string cancellationLink = "";
            if (!string.IsNullOrEmpty(CancellationToken))
            {
                string cancelUrl = "https://equibillbook.com/publicbooking/bookingcancellation?bookingNo=" + BookingNo + "&token=" + CancellationToken;
                cancellationLink = "<a href=\"" + cancelUrl + "\" class=\"cancel-link\" style=\"color: white !important; text-decoration: none;\">Cancel Booking</a>";
            }
            myString = myString.Replace("--cancellationLink--", cancellationLink);

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }

        public string[] PublicBookingCancellation(string To, string Subject, string BookingNo, string CustomerName, DateTime BookingDate, TimeSpan BookingTime, string BusinessName, string ParentBusinessName, string BusinessLogo, string Domain = "")
        {
            CommonController oCommonController = new CommonController();
            ConnectionContext oConnectionContext = new ConnectionContext();
            
            long Under = 0;
            if (string.IsNullOrEmpty(Domain))
            {
                Under = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.BusinessName == BusinessName || a.ParentBusinessName == ParentBusinessName)
                    .Select(a => a.CompanyId)
                    .FirstOrDefault();
            }
            else
            {
                Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == Under).Select(a => new
            {
                a.CompanyId,
                a.BusinessLogo,
                a.BusinessName,
                a.ParentBusinessName,
                BusinessEmail = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == Under).Select(b => b.Email).FirstOrDefault()
            }).FirstOrDefault();

            if (BusinessSetting == null)
            {
                BusinessSetting = new
                {
                    CompanyId = Under,
                    BusinessLogo = BusinessLogo,
                    BusinessName = BusinessName,
                    ParentBusinessName = ParentBusinessName,
                    BusinessEmail = ""
                };
            }

            string myString = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #dc3545; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f8f9fa; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Booking Cancelled</h2>
        </div>
        <div class='content'>
            <p>Hello <strong>" + CustomerName + @"</strong>,</p>
            <p>Your table booking has been cancelled.</p>
            <p><strong>Booking Number:</strong> " + BookingNo + @"</p>
            <p><strong>Date:</strong> " + BookingDate.ToString("dddd, MMMM dd, yyyy") + @"</p>
            <p><strong>Time:</strong> " + BookingTime.ToString(@"hh\:mm") + @"</p>
            <p>We hope to serve you in the future.</p>
            <p>Thanks,<br />Team " + (BusinessSetting.ParentBusinessName ?? ParentBusinessName) + @"</p>
        </div>
    </div>
</body>
</html>";

            return EmailFunction(BusinessSetting.CompanyId, To, Subject, myString, "", "", 0, DateTime.Now, 0, "", Domain, 0);
        }
    }
}
