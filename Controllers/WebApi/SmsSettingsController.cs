using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class SmsSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllSmsSettings(ClsSmsSettingsVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted== false).Select(a => new
            {
                SmsSettingsId = a.SmsSettingsId,
                SmsService = a.SmsService,
                RequestMethod = a.RequestMethod,
                Url = a.Url,
                SendToParameterName = a.SendToParameterName,
                MessageParameterName = a.MessageParameterName,
                Header1Key = a.Header1Key,
                Header1Value = a.Header1Value,
                Header2Key = a.Header2Key,
                Header2Value = a.Header2Value,
                Header3Key = a.Header3Key,
                Header3Value = a.Header3Value,
                Header4Key = a.Header4Key,
                Header4Value = a.Header4Value,
                Parameter1Key = a.Parameter1Key,
                Parameter1Value = a.Parameter1Value,
                Parameter2Key = a.Parameter2Key,
                Parameter2Value = a.Parameter2Value,
                Parameter3Key = a.Parameter3Key,
                Parameter3Value = a.Parameter3Value,
                Parameter4Key = a.Parameter4Key,
                Parameter4Value = a.Parameter4Value,
                Parameter5Key = a.Parameter5Key,
                Parameter5Value = a.Parameter5Value,
                Parameter6Key = a.Parameter6Key,
                Parameter6Value = a.Parameter6Value,
                Parameter7Key = a.Parameter7Key,
                Parameter7Value = a.Parameter7Value,
                Parameter8Key = a.Parameter8Key,
                Parameter8Value = a.Parameter8Value,
                Parameter9Key = a.Parameter9Key,
                Parameter9Value = a.Parameter9Value,
                Parameter10Key = a.Parameter10Key,
                Parameter10Value = a.Parameter10Value,
                TwilioAccountSID = a.TwilioAccountSID,
                TwilioAuthToken = a.TwilioAuthToken,
                TwilioFrom = a.TwilioFrom,
                NexmoApiKey = a.NexmoApiKey,
                NexmoApiSecret = a.NexmoApiSecret,
                NexmoFrom = a.NexmoFrom,
                EnableCountryCode = a.EnableCountryCode,
                TestMobileNo = a.TestMobileNo,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.AddedOn,
                a.ModifiedOn,
                a.IsDefault,
                a.SaveAs,
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
                    SmsSettings = det.OrderByDescending(a => a.SmsSettingsId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SmsSetting(ClsSmsSettingsVm obj)
        {
            var det = oConnectionContext.DbClsSmsSettings.Where(a => a.SmsSettingsId == obj.SmsSettingsId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsDefault,
                SmsSettingsId = a.SmsSettingsId,
                SmsService = a.SmsService,
                RequestMethod = a.RequestMethod,
                Url = a.Url,
                SendToParameterName = a.SendToParameterName,
                MessageParameterName = a.MessageParameterName,
                Header1Key = a.Header1Key,
                Header1Value = a.Header1Value,
                Header2Key = a.Header2Key,
                Header2Value = a.Header2Value,
                Header3Key = a.Header3Key,
                Header3Value = a.Header3Value,
                Header4Key = a.Header4Key,
                Header4Value = a.Header4Value,
                Parameter1Key = a.Parameter1Key,
                Parameter1Value = a.Parameter1Value,
                Parameter2Key = a.Parameter2Key,
                Parameter2Value = a.Parameter2Value,
                Parameter3Key = a.Parameter3Key,
                Parameter3Value = a.Parameter3Value,
                Parameter4Key = a.Parameter4Key,
                Parameter4Value = a.Parameter4Value,
                Parameter5Key = a.Parameter5Key,
                Parameter5Value = a.Parameter5Value,
                Parameter6Key = a.Parameter6Key,
                Parameter6Value = a.Parameter6Value,
                Parameter7Key = a.Parameter7Key,
                Parameter7Value = a.Parameter7Value,
                Parameter8Key = a.Parameter8Key,
                Parameter8Value = a.Parameter8Value,
                Parameter9Key = a.Parameter9Key,
                Parameter9Value = a.Parameter9Value,
                Parameter10Key = a.Parameter10Key,
                Parameter10Value = a.Parameter10Value,
                TwilioAccountSID = a.TwilioAccountSID,
                TwilioAuthToken = a.TwilioAuthToken,
                TwilioFrom = a.TwilioFrom,
                NexmoApiKey = a.NexmoApiKey,
                NexmoApiSecret = a.NexmoApiSecret,
                NexmoFrom = a.NexmoFrom,
                EnableCountryCode = a.EnableCountryCode,
                TestMobileNo = a.TestMobileNo,
                a.SaveAs
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SmsSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSmsSetting(ClsSmsSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmsSaveAs" });
                    isError = true;
                }

                if (obj.SmsService == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmsService" });
                    isError = true;
                }
                if (obj.TestMobileNo == "" || obj.TestMobileNo == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTestMobileNo" });
                    isError = true;
                }
                if (obj.TestMobileNo != null && obj.TestMobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.TestMobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divTestMobileNo" });
                        isError = true;
                    }
                }

                if (obj.SmsService == 1)
                {
                    if (obj.TwilioAccountSID == "" || obj.TwilioAccountSID == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTwilioAccountSID" });
                        isError = true;
                    }
                    if (obj.TwilioAuthToken == "" || obj.TwilioAuthToken == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTwilioAuthToken" });
                        isError = true;
                    }
                    if (obj.TwilioFrom == "" || obj.TwilioFrom == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTwilioFrom" });
                        isError = true;
                    }
                }

                if (obj.SmsService == 2)
                {
                    if (obj.NexmoApiKey == "" || obj.NexmoApiKey == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divNexmoApiKey" });
                        isError = true;
                    }
                    if (obj.NexmoApiSecret == "" || obj.NexmoApiSecret == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divNexmoApiSecret" });
                        isError = true;
                    }
                    if (obj.NexmoFrom == "" || obj.NexmoFrom == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divNexmoFrom" });
                        isError = true;
                    }
                }

                if (obj.SmsService == 3)
                {
                    if (obj.Url == "" || obj.Url == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUrl" });
                        isError = true;
                    }
                    if (obj.SendToParameterName == "" || obj.SendToParameterName == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSendToParameterName" });
                        isError = true;
                    }
                    if (obj.MessageParameterName == "" || obj.MessageParameterName == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMessageParameterName" });
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

                ClsSmsSettings oClsSmsSettings = new ClsSmsSettings()
                {
                    SmsService = obj.SmsService,
                    RequestMethod = obj.RequestMethod,
                    Url = obj.Url,
                    SendToParameterName = obj.SendToParameterName,
                    MessageParameterName = obj.MessageParameterName,
                    Header1Key = obj.Header1Key,
                    Header1Value = obj.Header1Value,
                    Header2Key = obj.Header2Key,
                    Header2Value = obj.Header2Value,
                    Header3Key = obj.Header3Key,
                    Header3Value = obj.Header3Value,
                    Header4Key = obj.Header4Key,
                    Header4Value = obj.Header4Value,
                    Parameter1Key = obj.Parameter1Key,
                    Parameter1Value = obj.Parameter1Value,
                    Parameter2Key = obj.Parameter2Key,
                    Parameter2Value = obj.Parameter2Value,
                    Parameter3Key = obj.Parameter3Key,
                    Parameter3Value = obj.Parameter3Value,
                    Parameter4Key = obj.Parameter4Key,
                    Parameter4Value = obj.Parameter4Value,
                    Parameter5Key = obj.Parameter5Key,
                    Parameter5Value = obj.Parameter5Value,
                    Parameter6Key = obj.Parameter6Key,
                    Parameter6Value = obj.Parameter6Value,
                    Parameter7Key = obj.Parameter7Key,
                    Parameter7Value = obj.Parameter7Value,
                    Parameter8Key = obj.Parameter8Key,
                    Parameter8Value = obj.Parameter8Value,
                    Parameter9Key = obj.Parameter9Key,
                    Parameter9Value = obj.Parameter9Value,
                    Parameter10Key = obj.Parameter10Key,
                    Parameter10Value = obj.Parameter10Value,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    TwilioAccountSID = obj.TwilioAccountSID,
                    TwilioAuthToken = obj.TwilioAuthToken,
                    TwilioFrom = obj.TwilioFrom,
                    NexmoApiKey = obj.NexmoApiKey,
                    NexmoApiSecret = obj.NexmoApiSecret,
                    NexmoFrom = obj.NexmoFrom,
                    EnableCountryCode = obj.EnableCountryCode,
                    TestMobileNo = obj.TestMobileNo,
                    SaveAs = obj.SaveAs,
                    CompanyId = obj.CompanyId,
                    IsDefault = obj.IsDefault,
                    IsActive= true,
                    IsDeleted = false
                };

                oConnectionContext.DbClsSmsSettings.Add(oClsSmsSettings);
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblSmsSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"SmsSettingsId\"!=" + oClsSmsSettings.SmsSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Sms Settings\" created",
                    Id = oClsSmsSettings.SmsSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sms settings created successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSmsSetting(ClsSmsSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {

                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmsSaveAs" });
                    isError = true;
                }

                if (obj.SmsService == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSmsService" });
                    isError = true;
                }
                if (obj.TestMobileNo == "" || obj.TestMobileNo == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTestMobileNo" });
                    isError = true;
                }
                if (obj.TestMobileNo != null && obj.TestMobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.TestMobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divTestMobileNo" });
                        isError = true;
                    }
                }
                if (obj.SmsService == 1)
                {
                    if (obj.TwilioAccountSID == "" || obj.TwilioAccountSID == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTwilioAccountSID" });
                        isError = true;
                    }
                    if (obj.TwilioAuthToken == "" || obj.TwilioAuthToken == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTwilioAuthToken" });
                        isError = true;
                    }
                    if (obj.TwilioFrom == "" || obj.TwilioFrom == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTwilioFrom" });
                        isError = true;
                    }
                }

                if (obj.SmsService == 2)
                {
                    if (obj.NexmoApiKey == "" || obj.NexmoApiKey == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divNexmoApiKey" });
                        isError = true;
                    }
                    if (obj.NexmoApiSecret == "" || obj.NexmoApiSecret == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divNexmoApiSecret" });
                        isError = true;
                    }
                    if (obj.NexmoFrom == "" || obj.NexmoFrom == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divNexmoFrom" });
                        isError = true;
                    }
                }

                if (obj.SmsService == 3)
                {
                    if (obj.Url == "" || obj.Url == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUrl" });
                        isError = true;
                    }
                    if (obj.SendToParameterName == "" || obj.SendToParameterName == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSendToParameterName" });
                        isError = true;
                    }
                    if (obj.MessageParameterName == "" || obj.MessageParameterName == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMessageParameterName" });
                        isError = true;
                    }
                }

                if (oConnectionContext.DbClsSmsSettings.Where(a => a.SaveAs.ToLower() == obj.SaveAs.ToLower() && a.IsDeleted == false  && a.CompanyId == obj.CompanyId && a.SmsSettingsId != obj.SmsSettingsId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate name", Id = "divSmsSaveAs" });
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

                ClsSmsSettings oClsSmsSettings = new ClsSmsSettings()
                {
                    SmsSettingsId = obj.SmsSettingsId,
                    SmsService = obj.SmsService,
                    RequestMethod = obj.RequestMethod,
                    Url = obj.Url,
                    SendToParameterName = obj.SendToParameterName,
                    MessageParameterName = obj.MessageParameterName,
                    Header1Key = obj.Header1Key,
                    Header1Value = obj.Header1Value,
                    Header2Key = obj.Header2Key,
                    Header2Value = obj.Header2Value,
                    Header3Key = obj.Header3Key,
                    Header3Value = obj.Header3Value,
                    Header4Key = obj.Header4Key,
                    Header4Value = obj.Header4Value,
                    Parameter1Key = obj.Parameter1Key,
                    Parameter1Value = obj.Parameter1Value,
                    Parameter2Key = obj.Parameter2Key,
                    Parameter2Value = obj.Parameter2Value,
                    Parameter3Key = obj.Parameter3Key,
                    Parameter3Value = obj.Parameter3Value,
                    Parameter4Key = obj.Parameter4Key,
                    Parameter4Value = obj.Parameter4Value,
                    Parameter5Key = obj.Parameter5Key,
                    Parameter5Value = obj.Parameter5Value,
                    Parameter6Key = obj.Parameter6Key,
                    Parameter6Value = obj.Parameter6Value,
                    Parameter7Key = obj.Parameter7Key,
                    Parameter7Value = obj.Parameter7Value,
                    Parameter8Key = obj.Parameter8Key,
                    Parameter8Value = obj.Parameter8Value,
                    Parameter9Key = obj.Parameter9Key,
                    Parameter9Value = obj.Parameter9Value,
                    Parameter10Key = obj.Parameter10Key,
                    Parameter10Value = obj.Parameter10Value,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    TwilioAccountSID = obj.TwilioAccountSID,
                    TwilioAuthToken = obj.TwilioAuthToken,
                    TwilioFrom = obj.TwilioFrom,
                    NexmoApiKey = obj.NexmoApiKey,
                    NexmoApiSecret = obj.NexmoApiSecret,
                    NexmoFrom = obj.NexmoFrom,
                    EnableCountryCode = obj.EnableCountryCode,
                    TestMobileNo = obj.TestMobileNo,
                    SaveAs = obj.SaveAs,
                    IsDefault = obj.IsDefault,
                };

                oConnectionContext.DbClsSmsSettings.Attach(oClsSmsSettings);
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.SmsService).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.RequestMethod).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Url).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.SendToParameterName).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.MessageParameterName).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header1Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header1Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header2Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header2Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header3Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header3Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header4Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Header4Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter1Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter1Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter2Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter2Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter3Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter3Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter4Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter4Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter5Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter5Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter6Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter6Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter7Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter7Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter8Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter8Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter9Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter9Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter10Key).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.Parameter10Value).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.TwilioAccountSID).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.TwilioAuthToken).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.TwilioFrom).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.NexmoApiKey).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.NexmoApiSecret).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.NexmoFrom).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.EnableCountryCode).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.TestMobileNo).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.SaveAs).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.IsDefault).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblSmsSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"SmsSettingsId\"!=" + obj.SmsSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Sms Settings\" updated",
                    Id = oClsSmsSettings.SmsSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sms settings updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SmsSettingActiveInactive(ClsSmsSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSmsSettings oClsSmsSettings = new ClsSmsSettings()
                {
                    SmsSettingsId = obj.SmsSettingsId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSmsSettings.Attach(oClsSmsSettings);
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.SmsSettingsId == obj.SmsSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long SmsSettingsId = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.SmsSettingsId != obj.SmsSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.SmsSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblSmsSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (SmsSettingsId != 0)
                    {
                        string query1 = "update \"tblSmsSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"SmsSettingsId\"=" + SmsSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"" + oConnectionContext.DbClsSmsSettings.Where(a => a.SmsSettingsId == obj.SmsSettingsId).Select(a => a.SaveAs).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsSmsSettings.SmsSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sms Settings " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SmsSettingDelete(ClsSmsSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSmsSettings oClsSmsSettings = new ClsSmsSettings()
                {
                    SmsSettingsId = obj.SmsSettingsId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSmsSettings.Attach(oClsSmsSettings);
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsSmsSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.SmsSettingsId == obj.SmsSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long SmsSettingsId = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.SmsSettingsId != obj.SmsSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.SmsSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblSmsSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (SmsSettingsId != 0)
                    {
                        string query1 = "update \"tblSmsSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"SmsSettingsId\"=" + SmsSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Sms Settings\" deleted",
                    Id = oClsSmsSettings.SmsSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sms Settings deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSmsSettings(ClsSmsSettings obj)
        {
            var det = oConnectionContext.DbClsSmsSettings.Where(a => a.IsDeleted == false && a.IsActive == true && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsDefault,
                a.SaveAs,
                a.SmsSettingsId
            }).OrderBy(a => a.SaveAs).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SmsSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
