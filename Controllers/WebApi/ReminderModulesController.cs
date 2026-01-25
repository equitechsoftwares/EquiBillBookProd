using EquiBillBook.Controllers.WebApi.Common;
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
    public class ReminderModulesController : ApiController
    {
        CommonController oCommonController = new CommonController();
        RemindersController oRemindersController = new RemindersController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> ReminderModules(ClsReminderModulesVm obj)
        {
            var det = oConnectionContext.DbClsReminderModules.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.ReminderModulesId,
                a.Sequence,
                a.Title,
                a.Name,
                ReminderModulesSettings = oConnectionContext.DbClsReminderModulesSettings.Where(c => c.IsDeleted == false &&
                            c.ReminderModulesId == a.ReminderModulesId && c.CompanyId == obj.CompanyId).Select(c => new
                            {
                                c.ReminderModulesSettingsId,
                                c.Name,
                                c.Description,
                                c.ReminderType,
                                IsActive = c.IsActive,
                                IsDeleted = c.IsDeleted,
                                AddedBy = c.AddedBy,
                                AddedOn = c.AddedOn,
                                ModifiedBy = c.ModifiedBy,
                                ModifiedOn = c.ModifiedOn,
                                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == c.AddedBy).Select(z => z.Username).FirstOrDefault(),
                                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == c.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                            }).ToList()
            }).OrderBy(a => a.Sequence).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ReminderModules = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ReminderModulesSetting(ClsReminderModulesSettingsVm obj)
        {
            var det = oConnectionContext.DbClsReminderModulesSettings.Where(a => a.ReminderModulesSettingsId == obj.ReminderModulesSettingsId
            && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ReminderModulesId,
                a.ReminderType,
                a.Name,
                a.Description,
                a.AutoSendEmail,
                a.EmailSubject,
                a.CC,
                a.BCC,
                a.EmailBody,
                a.AutoSendSms,
                a.SmsBody,
                a.AutoSendWhatsapp,
                a.WhatsappBody,
                a.ReminderTo,
                a.ReminderInDays,
                a.ReminderBeforeAfter,
                a.TotalDue,
                ReminderExceptionContactsArray = oConnectionContext.DbClsReminderExceptionContacts.Where(b => b.CompanyId == a.CompanyId
                && b.IsActive == true && b.IsDeleted == false && b.ReminderModulesSettingsId == a.ReminderModulesSettingsId).Select(b => b.UserId),
            }).FirstOrDefault();

            var ReminderModule = oConnectionContext.DbClsReminderModules.Where(a => a.ReminderModulesId == det.ReminderModulesId && a.IsActive == true
            && a.IsDeleted == false).Select(a => new
            {
                a.ReminderModulesId,
                a.Title,
                a.Name,
                a.AvailableTags
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ReminderModulesSetting = det,
                    ReminderModule = ReminderModule
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchAvailableTags(ClsReminderModulesVm obj)
        {
            var det = oConnectionContext.DbClsReminderModules.Where(a => a.ReminderModulesId == obj.ReminderModulesId && a.IsActive == true
            && a.IsDeleted == false).Select(a => new
            {
                a.ReminderModulesId,
                a.Title,
                a.Name,
                a.AvailableTags
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ReminderModule = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ReminderTemplateInsert(ClsReminderModulesSettingsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.Name == null || obj.Name == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                    isError = true;
                }

                if (obj.Name != null && obj.Name != "")
                {
                    if (oConnectionContext.DbClsReminderModulesSettings.Where(a => a.Name.ToLower() == obj.Name.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Name exists", Id = "divName" });
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

                ClsReminderModulesSettings oClsReminderModulesSettings = new ClsReminderModulesSettings()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyId = obj.CompanyId,
                    ReminderModulesId = obj.ReminderModulesId,
                    ReminderType = obj.ReminderType,
                    Name = obj.Name,
                    Description = obj.Description,
                    ReminderBeforeAfter = obj.ReminderBeforeAfter,
                    ReminderInDays = obj.ReminderInDays,
                    ReminderTo = obj.ReminderTo,
                    TotalDue = obj.TotalDue,
                    AutoSendEmail = obj.AutoSendEmail,
                    AutoSendSms = obj.AutoSendSms,
                    AutoSendWhatsapp = obj.AutoSendWhatsapp,
                    BCC = obj.BCC,
                    CC = obj.CC,
                    EmailBody = obj.EmailBody,
                    EmailSubject = obj.EmailSubject,
                    SmsBody = obj.SmsBody,
                    WhatsappBody = obj.WhatsappBody
                };
                oConnectionContext.DbClsReminderModulesSettings.Add(oClsReminderModulesSettings);
                oConnectionContext.SaveChanges();

                if (obj.ReminderExceptionContactsArray != null)
                {
                    foreach (var item in obj.ReminderExceptionContactsArray)
                    {
                        if (item != "")
                        {
                            ClsReminderExceptionContacts oClsReminderExceptionContacts = new ClsReminderExceptionContacts()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                ReminderModulesSettingsId = oClsReminderModulesSettings.ReminderModulesSettingsId,
                                UserId = Convert.ToInt64(item)
                            };
                            oConnectionContext.DbClsReminderExceptionContacts.Add(oClsReminderExceptionContacts);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Reminders",
                    CompanyId = obj.CompanyId,
                    Description = "Reminder \"" + obj.Name + "\" created",
                    Id = oClsReminderModulesSettings.ReminderModulesSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                dbContextTransaction.Complete();
                data = new
                {
                    Status = 1,
                    Message = "Reminder created successfully.",
                    Data = new
                    {

                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        public async Task<IHttpActionResult> ReminderTemplateUpdate(ClsReminderModulesSettingsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.Name == null || obj.Name == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                    isError = true;
                }

                if (obj.Name != null && obj.Name != "")
                {
                    if (oConnectionContext.DbClsReminderModulesSettings.Where(a => a.Name.ToLower() == obj.Name.ToLower() && a.CompanyId == obj.CompanyId
                    && a.IsDeleted == false && a.ReminderModulesSettingsId != obj.ReminderModulesSettingsId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Name exists", Id = "divName" });
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
                ClsReminderModulesSettings oClsReminderModulesSettings = new ClsReminderModulesSettings()
                {
                    ReminderModulesSettingsId = obj.ReminderModulesSettingsId,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    //ReminderModulesId = obj.ReminderModulesId,
                    //ReminderType = obj.ReminderType,
                    Name = obj.Name,
                    Description = obj.Description,
                    ReminderBeforeAfter = obj.ReminderBeforeAfter,
                    ReminderInDays = obj.ReminderInDays,
                    ReminderTo = obj.ReminderTo,
                    TotalDue = obj.TotalDue,
                    AutoSendEmail = obj.AutoSendEmail,
                    AutoSendSms = obj.AutoSendSms,
                    AutoSendWhatsapp = obj.AutoSendWhatsapp,
                    BCC = obj.BCC,
                    CC = obj.CC,
                    EmailBody = obj.EmailBody,
                    EmailSubject = obj.EmailSubject,
                    SmsBody = obj.SmsBody,
                    WhatsappBody = obj.WhatsappBody
                };

                oConnectionContext.DbClsReminderModulesSettings.Attach(oClsReminderModulesSettings);
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ModifiedOn).IsModified = true;
                //oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ReminderModulesId).IsModified = true;
                //oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ReminderType).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.Name).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ReminderBeforeAfter).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ReminderInDays).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.ReminderTo).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.TotalDue).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.AutoSendEmail).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.AutoSendSms).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.AutoSendWhatsapp).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.BCC).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.CC).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.EmailBody).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.EmailSubject).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.SmsBody).IsModified = true;
                oConnectionContext.Entry(oClsReminderModulesSettings).Property(x => x.WhatsappBody).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "update \"tblReminderExceptionContacts\" set \"IsActive\"= False where \"ReminderModulesSettingsId\"=" + obj.ReminderModulesSettingsId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                if (obj.ReminderExceptionContactsArray != null)
                {
                    foreach (var item in obj.ReminderExceptionContactsArray)
                    {
                        if (item != "")
                        {
                            long UserId = Convert.ToInt64(item);

                            long ReminderExceptionContactsId = oConnectionContext.DbClsReminderExceptionContacts.Where(a => a.ReminderModulesSettingsId == obj.ReminderModulesSettingsId &&
                            a.UserId == UserId && a.IsDeleted == false).Select(a => a.ReminderExceptionContactsId).FirstOrDefault();
                            if (ReminderExceptionContactsId == 0)
                            {
                                ClsReminderExceptionContacts oClsReminderExceptionContacts = new ClsReminderExceptionContacts()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ReminderModulesSettingsId = obj.ReminderModulesSettingsId,
                                    UserId = Convert.ToInt64(item)
                                };
                                oConnectionContext.DbClsReminderExceptionContacts.Add(oClsReminderExceptionContacts);
                                oConnectionContext.SaveChanges();
                            }
                            else
                            {
                                ClsReminderExceptionContacts oClsReminderExceptionContacts = new ClsReminderExceptionContacts()
                                {
                                    ReminderExceptionContactsId = ReminderExceptionContactsId,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ReminderModulesSettingsId = obj.ReminderModulesSettingsId,
                                    UserId = Convert.ToInt64(item)
                                };
                                oConnectionContext.DbClsReminderExceptionContacts.Attach(oClsReminderExceptionContacts);
                                oConnectionContext.Entry(oClsReminderExceptionContacts).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsReminderExceptionContacts).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.Entry(oClsReminderExceptionContacts).Property(x => x.IsActive).IsModified = true;
                                oConnectionContext.Entry(oClsReminderExceptionContacts).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(oClsReminderExceptionContacts).Property(x => x.ReminderModulesSettingsId).IsModified = true;
                                oConnectionContext.Entry(oClsReminderExceptionContacts).Property(x => x.UserId).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Reminders",
                    CompanyId = obj.CompanyId,
                    Description = "Reminder \"" + obj.Name + "\" updated",
                    Id = obj.ReminderModulesSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                dbContextTransaction.Complete();
                data = new
                {
                    Status = 1,
                    Message = "Reminder updated successfully.",
                    Data = new
                    {

                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        public async Task<IHttpActionResult> ReminderActiveInactive(ClsReminderModulesSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsReminderModulesSettings oClsRole = new ClsReminderModulesSettings()
                {
                    ReminderModulesSettingsId = obj.ReminderModulesSettingsId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsReminderModulesSettings.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ReminderModulesSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Reminders",
                    CompanyId = obj.CompanyId,
                    Description = "Reminder \"" + oConnectionContext.DbClsReminderModulesSettings.Where(a =>
                    a.ReminderModulesSettingsId == obj.ReminderModulesSettingsId).Select(a => a.Name).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsRole.ReminderModulesSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Reminder " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ReminderDelete(ClsReminderModulesSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsReminderModulesSettings oClsRole = new ClsReminderModulesSettings()
                {
                    ReminderModulesSettingsId = obj.ReminderModulesSettingsId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsReminderModulesSettings.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ReminderModulesSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Reminders",
                    CompanyId = obj.CompanyId,
                    Description = "Reminder \"" + oConnectionContext.DbClsReminderModulesSettings.Where(a =>
                    a.ReminderModulesSettingsId == obj.ReminderModulesSettingsId).Select(a => a.Name).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.ReminderModulesSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Reminder deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchReminderModule(ClsReminderModulesSettingsVm obj)
        {
            var det = oConnectionContext.DbClsReminderModulesSettings.Where(a => a.ReminderType == "Manual" && a.ReminderBeforeAfter == obj.ReminderBeforeAfter
            && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ReminderModulesId,
                a.ReminderType,
                a.Name,
                a.Description,
                a.AutoSendEmail,
                a.EmailSubject,
                a.CC,
                a.BCC,
                a.EmailBody,
                a.AutoSendSms,
                a.SmsBody,
                a.AutoSendWhatsapp,
                a.WhatsappBody,
                a.ReminderTo,
                a.ReminderInDays,
                a.ReminderBeforeAfter,
                a.TotalDue,
                ReminderExceptionContactsArray = oConnectionContext.DbClsReminderExceptionContacts.Where(b => b.CompanyId == a.CompanyId
                && b.IsActive == true && b.IsDeleted == false && b.ReminderModulesSettingsId == a.ReminderModulesSettingsId).Select(b => b.UserId),
            }).FirstOrDefault();

            var ReminderModule = oConnectionContext.DbClsReminderModules.Where(a => a.ReminderModulesId == det.ReminderModulesId && a.IsActive == true
            && a.IsDeleted == false).Select(a => new
            {
                a.ReminderModulesId,
                a.Title,
                a.Name,
                a.AvailableTags
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ReminderModulesSetting = det,
                    ReminderModule = ReminderModule
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SendReminders(ClsReminderModulesSettingsVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

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

            obj.ReminderTo = "Customer";
            arr = oRemindersController.SaleReminder(obj, obj.CompanyId, obj.Id, obj.CC, obj.BCC, SmsSettingsId, EmailSettingsId, WhatsappSettingsId, obj.Name, obj.AddedBy, CurrentDate, obj.Domain, obj.ReminderTo);

            data = new
            {
                Status = 1,
                Message = "Reminder sent successfully",
                WhatsappUrl = arr[2],
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
