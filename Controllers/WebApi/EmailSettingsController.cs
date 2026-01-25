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
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class EmailSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllEmailSettings(ClsEmailSettingsVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.SaveAs,
                EmailSettingsId = a.EmailSettingsId,
                SmtpServer = a.SmtpServer,
                EnableSsl = a.EnableSsl,
                SmtpPort = a.SmtpPort,
                SmtpUser = a.SmtpUser,
                SmtpPass = a.SmtpPass,
                FromName = a.FromName,
                TestEmailId = a.TestEmailId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.AddedOn,
                a.ModifiedOn,
                a.IsDefault,
                a.IsActive
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SaveAs.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    EmailSettings = det.OrderByDescending(a => a.EmailSettingsId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> EmailSetting(ClsEmailSettingsVm obj)
        {
            var det = oConnectionContext.DbClsEmailSettings.Where(a => a.EmailSettingsId == obj.EmailSettingsId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsDefault,
                a.SaveAs,
                EmailSettingsId = a.EmailSettingsId,
                SmtpServer = a.SmtpServer,
                EnableSsl = a.EnableSsl,
                SmtpPort = a.SmtpPort,
                SmtpUser = a.SmtpUser,
                SmtpPass = a.SmtpPass,
                FromName = a.FromName,
                TestEmailId = a.TestEmailId
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    EmailSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertEmailSetting(ClsEmailSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divEmailSaveAs" });
                    isError = true;
                }
                if (obj.SmtpServer == null || obj.SmtpServer == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpServer" });
                    isError = true;
                }
                if (obj.SmtpPort == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpPort" });
                    isError = true;
                }
                if (obj.SmtpUser == null || obj.SmtpUser == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpUser" });
                    isError = true;
                }
                if (obj.SmtpPass == null || obj.SmtpPass == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpPass" });
                    isError = true;
                }
                if (obj.FromName == null || obj.FromName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFromName" });
                    isError = true;
                }
                if (obj.TestEmailId == null || obj.TestEmailId == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTestEmailId" });
                    isError = true;
                }

                if (obj.TestEmailId != null && obj.TestEmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.TestEmailId);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divTestEmailId" });
                        isError = true;
                    }
                }

                if (oConnectionContext.DbClsEmailSettings.Where(a => a.SaveAs.ToLower() == obj.SaveAs.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate name", Id = "divEmailSaveAs" });
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

                ClsEmailSettings oClsEmailSettings = new ClsEmailSettings();
                oClsEmailSettings.CompanyId = obj.CompanyId;
                oClsEmailSettings.SmtpServer = obj.SmtpServer;
                oClsEmailSettings.SmtpPort = obj.SmtpPort;
                oClsEmailSettings.SmtpUser = obj.SmtpUser;
                oClsEmailSettings.SmtpPass = obj.SmtpPass;
                oClsEmailSettings.AddedOn = CurrentDate;
                oClsEmailSettings.AddedBy = obj.AddedBy;
                oClsEmailSettings.EnableSsl = obj.EnableSsl;
                oClsEmailSettings.FromName = obj.FromName;
                oClsEmailSettings.TestEmailId = obj.TestEmailId;
                oClsEmailSettings.SaveAs = obj.SaveAs;
                oClsEmailSettings.IsDefault = obj.IsDefault;
                oClsEmailSettings.IsActive = true;
                oClsEmailSettings.IsDeleted = false;
                oConnectionContext.DbClsEmailSettings.Add(oClsEmailSettings);
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblEmailSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"EmailSettingsId\"!=" + oClsEmailSettings.EmailSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Email Settings\" created",
                    Id = oClsEmailSettings.EmailSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Email Setting created successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateEmailSetting(ClsEmailSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divEmailSaveAs" });
                    isError = true;
                }

                if (obj.SmtpServer == null || obj.SmtpServer == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpServer" });
                    isError = true;
                }
                if (obj.SmtpPort == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpPort" });
                    isError = true;
                }
                if (obj.SmtpUser == null || obj.SmtpUser == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpUser" });
                    isError = true;
                }
                if (obj.SmtpPass == null || obj.SmtpPass == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmtpPass" });
                    isError = true;
                }
                if (obj.FromName == null || obj.FromName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFromName" });
                    isError = true;
                }
                if (obj.TestEmailId == null || obj.TestEmailId == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTestEmailId" });
                    isError = true;
                }
                if (obj.TestEmailId != null && obj.TestEmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.TestEmailId);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divTestEmailId" });
                        isError = true;
                    }
                }

                if (oConnectionContext.DbClsEmailSettings.Where(a=>a.SaveAs.ToLower() == obj.SaveAs.ToLower() && a.IsDeleted == false && a.CompanyId== obj.CompanyId && a.EmailSettingsId!= obj.EmailSettingsId).Count()>0)
                {
                    errors.Add(new ClsError { Message = "Duplicate name", Id = "divEmailSaveAs" });
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

                ClsEmailSettings oClsEmailSettings = new ClsEmailSettings();
                oClsEmailSettings.EmailSettingsId = obj.EmailSettingsId;
                oClsEmailSettings.SmtpServer = obj.SmtpServer;
                oClsEmailSettings.SmtpPort = obj.SmtpPort;
                oClsEmailSettings.SmtpUser = obj.SmtpUser;
                oClsEmailSettings.SmtpPass = obj.SmtpPass;
                oClsEmailSettings.ModifiedOn = CurrentDate;
                oClsEmailSettings.ModifiedBy = obj.AddedBy;
                oClsEmailSettings.EnableSsl = obj.EnableSsl;
                oClsEmailSettings.FromName = obj.FromName;
                oClsEmailSettings.TestEmailId = obj.TestEmailId;
                oClsEmailSettings.SaveAs = obj.SaveAs;
                oClsEmailSettings.IsDefault = obj.IsDefault;

                oConnectionContext.DbClsEmailSettings.Attach(oClsEmailSettings);
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.SmtpServer).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.SmtpPort).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.SmtpUser).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.SmtpPass).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.EnableSsl).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.FromName).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.TestEmailId).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.SaveAs).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.IsDefault).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblEmailSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"EmailSettingsId\"!=" + obj.EmailSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Email Settings\" updated",
                    Id = oClsEmailSettings.EmailSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Email Setting updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> EmailSettingActiveInactive(ClsEmailSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsEmailSettings oClsEmailSettings = new ClsEmailSettings()
                {
                    EmailSettingsId = obj.EmailSettingsId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsEmailSettings.Attach(oClsEmailSettings);
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId
               && a.EmailSettingsId == obj.EmailSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long EmailSettingsId = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.EmailSettingsId != obj.EmailSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.EmailSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblEmailSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (EmailSettingsId != 0)
                    {
                        string query1 = "update \"tblEmailSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"EmailSettingsId\"=" + EmailSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"" + oConnectionContext.DbClsEmailSettings.Where(a => a.EmailSettingsId == obj.EmailSettingsId).Select(a => a.SaveAs).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsEmailSettings.EmailSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Email Setting " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> EmailSettingDelete(ClsEmailSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsEmailSettings oClsEmailSettings = new ClsEmailSettings()
                {
                    EmailSettingsId = obj.EmailSettingsId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsEmailSettings.Attach(oClsEmailSettings);
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsEmailSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId
               && a.EmailSettingsId == obj.EmailSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long EmailSettingsId = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.EmailSettingsId != obj.EmailSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.EmailSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblEmailSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (EmailSettingsId != 0)
                    {
                        string query1 = "update \"tblEmailSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"EmailSettingsId\"=" + EmailSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Email Settings\" deleted",
                    Id = oClsEmailSettings.EmailSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Email Setting deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveEmailSettings(ClsEmailSettingsVm obj)
        {
            var det = oConnectionContext.DbClsEmailSettings.Where(a => a.IsDeleted == false && a.IsActive == true && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsDefault,
                a.SaveAs,
                a.EmailSettingsId
            }).OrderBy(a => a.SaveAs).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    EmailSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
