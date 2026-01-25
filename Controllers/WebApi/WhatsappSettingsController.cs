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
    public class WhatsappSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllWhatsappSettings(ClsWhatsappSettingsVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.SaveAs,
                WhatsappSettingsId = a.WhatsappSettingsId,
                WhatsappService = a.WhatsappService,
                TwilioAccountSID = a.TwilioAccountSID,
                TwilioAuthToken = a.TwilioAuthToken,
                TwilioFrom = a.TwilioFrom,
                TestMobileNo = a.TestMobileNo,
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
                    WhatsappSettings = det.OrderByDescending(a => a.WhatsappSettingsId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> WhatsappSetting(ClsWhatsappSettingsVm obj)
        {
            var det = oConnectionContext.DbClsWhatsappSettings.Where(a => a.WhatsappSettingsId == obj.WhatsappSettingsId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                WhatsappSettingsId = a.WhatsappSettingsId,
                WhatsappService = a.WhatsappService,
                TwilioAccountSID = a.TwilioAccountSID,
                TwilioAuthToken = a.TwilioAuthToken,
                TwilioFrom = a.TwilioFrom,
                TestMobileNo = a.TestMobileNo,
                a.IsDefault,
                a.SaveAs
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    WhatsappSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertWhatsappSetting(ClsWhatsappSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWhatsappSaveAs" });
                    isError = true;
                }
                if (obj.WhatsappService == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWhatsappService" });
                    isError = true;
                }
                if (obj.TestMobileNo == "" || obj.TestMobileNo == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWTestMobileNo" });
                    isError = true;
                }

                if (obj.WhatsappService == 2)
                {
                    if (obj.TwilioAccountSID == "" || obj.TwilioAccountSID == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divWTwilioAccountSID" });
                        isError = true;
                    }
                    if (obj.TwilioAuthToken == "" || obj.TwilioAuthToken == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divWTwilioAuthToken" });
                        isError = true;
                    }
                    if (obj.TwilioFrom == "" || obj.TwilioFrom == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divWTwilioFrom" });
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

                ClsWhatsappSettings oClsWhatsappSettings = new ClsWhatsappSettings()
                {
                    WhatsappService = obj.WhatsappService,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    TwilioAccountSID = obj.TwilioAccountSID,
                    TwilioAuthToken = obj.TwilioAuthToken,
                    TwilioFrom = obj.TwilioFrom,
                    TestMobileNo = obj.TestMobileNo,
                    SaveAs = obj.SaveAs,
                    IsDefault = obj.IsDefault,
                    CompanyId = obj.CompanyId,
                    IsActive = true,
                    IsDeleted = false

                };
                oConnectionContext.DbClsWhatsappSettings.Add(oClsWhatsappSettings);
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblWhatsappSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"WhatsappSettingsId\"!=" + oClsWhatsappSettings.WhatsappSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Whatsapp Settings\" created",
                    Id = oClsWhatsappSettings.WhatsappSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Whatsapp Settings created successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateWhatsappSetting(ClsWhatsappSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWhatsappSaveAs" });
                    isError = true;
                }

                if (obj.WhatsappService == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWhatsappService" });
                    isError = true;
                }
                if (obj.TestMobileNo == "" || obj.TestMobileNo == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWTestMobileNo" });
                    isError = true;
                }

                if (obj.WhatsappService == 2)
                {
                    if (obj.TwilioAccountSID == "" || obj.TwilioAccountSID == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divWTwilioAccountSID" });
                        isError = true;
                    }
                    if (obj.TwilioAuthToken == "" || obj.TwilioAuthToken == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divWTwilioAuthToken" });
                        isError = true;
                    }
                    if (obj.TwilioFrom == "" || obj.TwilioFrom == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divWTwilioFrom" });
                        isError = true;
                    }
                }

                if (oConnectionContext.DbClsWhatsappSettings.Where(a => a.SaveAs.ToLower() == obj.SaveAs.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId && a.WhatsappSettingsId != obj.WhatsappSettingsId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate name", Id = "divWhatsappSaveAs" });
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

                ClsWhatsappSettings oClsWhatsappSettings = new ClsWhatsappSettings()
                {
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    WhatsappService = obj.WhatsappService,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    TwilioAccountSID = obj.TwilioAccountSID,
                    TwilioAuthToken = obj.TwilioAuthToken,
                    TwilioFrom = obj.TwilioFrom,
                    TestMobileNo = obj.TestMobileNo,
                    SaveAs = obj.SaveAs,
                    IsDefault = obj.IsDefault
                };

                oConnectionContext.DbClsWhatsappSettings.Attach(oClsWhatsappSettings);
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.WhatsappService).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.TwilioAccountSID).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.TwilioAuthToken).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.TwilioFrom).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.TestMobileNo).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.SaveAs).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.IsDefault).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblWhatsappSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"WhatsappSettingsId\"!=" + obj.WhatsappSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Whatsapp Settings\" updated",
                    Id = oClsWhatsappSettings.WhatsappSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Whatsapp Settings updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        
        public async Task<IHttpActionResult> WhatsappSettingActiveInactive(ClsWhatsappSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsWhatsappSettings oClsWhatsappSettings = new ClsWhatsappSettings()
                {
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsWhatsappSettings.Attach(oClsWhatsappSettings);
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.WhatsappSettingsId == obj.WhatsappSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long WhatsappSettingsId = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.WhatsappSettingsId != obj.WhatsappSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.WhatsappSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblWhatsappSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if(WhatsappSettingsId != 0)
                    {
                        string query1 = "update \"tblWhatsappSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"WhatsappSettingsId\"=" + WhatsappSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }                    
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"" + oConnectionContext.DbClsWhatsappSettings.Where(a => a.WhatsappSettingsId == obj.WhatsappSettingsId).Select(a => a.SaveAs).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsWhatsappSettings.WhatsappSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Whatsapp Setting " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> WhatsappSettingDelete(ClsWhatsappSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsWhatsappSettings oClsWhatsappSettings = new ClsWhatsappSettings()
                {
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsWhatsappSettings.Attach(oClsWhatsappSettings);
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsWhatsappSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.WhatsappSettingsId == obj.WhatsappSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long WhatsappSettingsId = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.WhatsappSettingsId != obj.WhatsappSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.WhatsappSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblWhatsappSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (WhatsappSettingsId != 0)
                    {
                        string query1 = "update \"tblWhatsappSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"WhatsappSettingsId\"=" + WhatsappSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Whatsapp Settings\" deleted",
                    Id = oClsWhatsappSettings.WhatsappSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Whatsapp Setting deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveWhatsappSettings(ClsWhatsappSettingsVm obj)
        {
            var det = oConnectionContext.DbClsWhatsappSettings.Where(a => a.IsDeleted == false && a.IsActive == true && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsDefault,
                a.SaveAs,
                a.WhatsappSettingsId
            }).OrderBy(a => a.SaveAs).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    WhatsappSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
