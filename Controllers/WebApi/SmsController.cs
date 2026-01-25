
using EquiBillBook.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Vonage;
using Vonage.Request;

namespace EquiBillBook.Controllers.WebApi
{
    public class SmsController : ApiController
    {
        public dynamic SmsFunction(long CompanyId, string To, string Message, long AddedBy, DateTime CurrentDate, long Id, string Type,string Domain,long SmsSettingsId)
        {
            ConnectionContext oConnectionContext = new ConnectionContext();
            try
            {
                ClsSmsSettingsVm SmsSetting = new ClsSmsSettingsVm();

                long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                bool EnableDefaultSmsBranding = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == CompanyId).Select(a => a.EnableDefaultSmsBranding).FirstOrDefault();

                if (EnableDefaultSmsBranding == false)
                {
                    if(SmsSettingsId == 0)
                    {
                        SmsSetting = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == CompanyId && a.IsDefault == true).Select(a => new ClsSmsSettingsVm
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
                            DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId ==
                            oConnectionContext.DbClsBusinessSettings.Where(c => c.CompanyId == CompanyId).Select(c => c.CountryId).FirstOrDefault()).Select(b => b.DialingCode).FirstOrDefault(),
                        }).FirstOrDefault();
                    }
                    else
                    {
                        SmsSetting = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == CompanyId && a.SmsSettingsId == SmsSettingsId).Select(a => new ClsSmsSettingsVm
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
                            DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId ==
                            oConnectionContext.DbClsBusinessSettings.Where(c => c.CompanyId == CompanyId).Select(c => c.CountryId).FirstOrDefault()).Select(b => b.DialingCode).FirstOrDefault(),
                        }).FirstOrDefault();
                    }
                    
                }
                else
                {
                    SmsSetting = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == Under && a.IsDefault == true).Select(a => new ClsSmsSettingsVm
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
                        DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId ==
                        oConnectionContext.DbClsBusinessSettings.Where(c => c.CompanyId == CompanyId).Select(c => c.CountryId).FirstOrDefault()).Select(b => b.DialingCode).FirstOrDefault(),
                    }).FirstOrDefault();
                }

                if (SmsSetting.EnableCountryCode == true)
                {
                    To = SmsSetting.DialingCode + To;
                }

                if (SmsSetting.SmsService == 1) // Twilio
                {
                    string accountSid = SmsSetting.TwilioAccountSID;
                    string authToken = SmsSetting.TwilioAuthToken;

                    TwilioClient.Init(accountSid, authToken);

                    var response = MessageResource.Create(
                        body: Message,
                        from: new Twilio.Types.PhoneNumber(SmsSetting.TwilioFrom),
                        to: new Twilio.Types.PhoneNumber(To)
                    );

                    if (EnableDefaultSmsBranding == true)
                    {
                        ClsSmsUsed oClsSmsUsed = new ClsSmsUsed()
                        {
                            AddedBy = AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = CompanyId,
                            Id = Id,
                            Type = Type,
                        };
                        oConnectionContext.DbClsSmsUsed.Add(oClsSmsUsed);
                        oConnectionContext.SaveChanges();
                    }

                    string[] arr = { "1", "" };
                    return arr;
                }
                else if (SmsSetting.SmsService == 2) // Nexmo
                {
                    var toNumber = To;
                    var vonageBrandName = SmsSetting.NexmoFrom;
                    var vonageApiKey = SmsSetting.NexmoApiKey;
                    var vonageApiSecret = SmsSetting.NexmoApiSecret;

                    var credentials = Credentials.FromApiKeyAndSecret(
                        vonageApiKey,
                        vonageApiSecret
                        );

                    var vonageClient = new VonageClient(credentials);

                    var response = vonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
                    {
                        To = toNumber,
                        From = vonageBrandName,
                        Text = Message
                    });

                    if (EnableDefaultSmsBranding == true)
                    {
                        ClsSmsUsed oClsSmsUsed = new ClsSmsUsed()
                        {
                            AddedBy = AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = CompanyId,
                            Id = Id,
                            Type = Type,
                        };
                        oConnectionContext.DbClsSmsUsed.Add(oClsSmsUsed);
                        oConnectionContext.SaveChanges();
                    }

                    string[] arr = { "1", "" };
                    return arr;
                }
                else // Others
                {
                    if (SmsSetting.RequestMethod == 1)
                    {
                        string url = SmsSetting.Url + "?" + SmsSetting.SendToParameterName + "=" + To + "&" + SmsSetting.MessageParameterName + "=" + Message;

                        if (SmsSetting.Parameter1Key != null && SmsSetting.Parameter1Key != "" && SmsSetting.Parameter1Value != null && SmsSetting.Parameter1Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter1Key + "=" + SmsSetting.Parameter1Value;
                        }
                        if (SmsSetting.Parameter2Key != null && SmsSetting.Parameter2Key != "" && SmsSetting.Parameter2Value != null && SmsSetting.Parameter2Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter2Key + "=" + SmsSetting.Parameter2Value;
                        }
                        if (SmsSetting.Parameter3Key != null && SmsSetting.Parameter3Key != "" && SmsSetting.Parameter3Value != null && SmsSetting.Parameter3Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter3Key + "=" + SmsSetting.Parameter3Value;
                        }
                        if (SmsSetting.Parameter4Key != null && SmsSetting.Parameter4Key != "" && SmsSetting.Parameter4Value != null && SmsSetting.Parameter4Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter4Key + "=" + SmsSetting.Parameter4Value;
                        }
                        if (SmsSetting.Parameter5Key != null && SmsSetting.Parameter5Key != "" && SmsSetting.Parameter5Value != null && SmsSetting.Parameter5Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter5Key + "=" + SmsSetting.Parameter5Value;
                        }
                        if (SmsSetting.Parameter6Key != null && SmsSetting.Parameter6Key != "" && SmsSetting.Parameter6Value != null && SmsSetting.Parameter6Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter6Key + "=" + SmsSetting.Parameter6Value;
                        }
                        if (SmsSetting.Parameter7Key != null && SmsSetting.Parameter7Key != "" && SmsSetting.Parameter7Value != null && SmsSetting.Parameter7Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter7Key + "=" + SmsSetting.Parameter7Value;
                        }
                        if (SmsSetting.Parameter8Key != null && SmsSetting.Parameter8Key != "" && SmsSetting.Parameter8Value != null && SmsSetting.Parameter8Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter8Key + "=" + SmsSetting.Parameter8Value;
                        }
                        if (SmsSetting.Parameter9Key != null && SmsSetting.Parameter9Key != "" && SmsSetting.Parameter9Value != null && SmsSetting.Parameter9Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter9Key + "=" + SmsSetting.Parameter9Value;
                        }
                        if (SmsSetting.Parameter10Key != null && SmsSetting.Parameter10Key != "" && SmsSetting.Parameter10Value != null && SmsSetting.Parameter10Value != "")
                        {
                            url = url + "&" + SmsSetting.Parameter10Key + "=" + SmsSetting.Parameter10Value;
                        }
                        var client = new RestClient(url);
                        var request = new RestRequest("", Method.Get);
                        var response = client.Execute(request);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (EnableDefaultSmsBranding == true)
                            {
                                ClsSmsUsed oClsSmsUsed = new ClsSmsUsed()
                                {
                                    AddedBy = AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = CompanyId,
                                    Id = Id,
                                    Type = Type,
                                };
                                oConnectionContext.DbClsSmsUsed.Add(oClsSmsUsed);
                                oConnectionContext.SaveChanges();
                            }

                            string[] arr = { "1", "" };
                            return arr;
                        }
                        else
                        {
                            string[] arr = { "0", response.Content };
                            return arr;
                        }
                    }
                    else
                    {
                        var client = new RestClient(SmsSetting.Url);
                        var request = new RestRequest("", Method.Post);

                        if (SmsSetting.Header1Key != null && SmsSetting.Header1Key != "" && SmsSetting.Header1Value != null && SmsSetting.Header1Value != "")
                        {
                            request.AddHeader(SmsSetting.Header1Key, SmsSetting.Header1Value);
                        }
                        if (SmsSetting.Header2Key != null && SmsSetting.Header2Key != "" && SmsSetting.Header2Value != null && SmsSetting.Header2Value != "")
                        {
                            request.AddHeader(SmsSetting.Header2Key, SmsSetting.Header2Value);
                        }
                        if (SmsSetting.Header3Key != null && SmsSetting.Header3Key != "" && SmsSetting.Header3Value != null && SmsSetting.Header3Value != "")
                        {
                            request.AddHeader(SmsSetting.Header3Key, SmsSetting.Header3Value);
                        }
                        if (SmsSetting.Header4Key != null && SmsSetting.Header4Key != "" && SmsSetting.Header4Value != null && SmsSetting.Header4Value != "")
                        {
                            request.AddHeader(SmsSetting.Header4Key, SmsSetting.Header4Value);
                        }


                        request.AddParameter(SmsSetting.SendToParameterName, To);
                        request.AddParameter(SmsSetting.MessageParameterName, Message);

                        if (SmsSetting.Parameter1Key != null && SmsSetting.Parameter1Key != "" && SmsSetting.Parameter1Value != null && SmsSetting.Parameter1Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter1Key, SmsSetting.Parameter1Value);
                        }
                        if (SmsSetting.Parameter2Key != null && SmsSetting.Parameter2Key != "" && SmsSetting.Parameter2Value != null && SmsSetting.Parameter2Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter2Key, SmsSetting.Parameter2Value);
                        }
                        if (SmsSetting.Parameter3Key != null && SmsSetting.Parameter3Key != "" && SmsSetting.Parameter3Value != null && SmsSetting.Parameter3Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter3Key, SmsSetting.Parameter3Value);
                        }
                        if (SmsSetting.Parameter4Key != null && SmsSetting.Parameter4Key != "" && SmsSetting.Parameter4Value != null && SmsSetting.Parameter4Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter4Key, SmsSetting.Parameter4Value);
                        }
                        if (SmsSetting.Parameter5Key != null && SmsSetting.Parameter5Key != "" && SmsSetting.Parameter5Value != null && SmsSetting.Parameter5Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter5Key, SmsSetting.Parameter5Value);
                        }
                        if (SmsSetting.Parameter6Key != null && SmsSetting.Parameter6Key != "" && SmsSetting.Parameter6Value != null && SmsSetting.Parameter6Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter6Key, SmsSetting.Parameter6Value);
                        }
                        if (SmsSetting.Parameter7Key != null && SmsSetting.Parameter7Key != "" && SmsSetting.Parameter7Value != null && SmsSetting.Parameter7Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter7Key, SmsSetting.Parameter7Value);
                        }
                        if (SmsSetting.Parameter8Key != null && SmsSetting.Parameter8Key != "" && SmsSetting.Parameter8Value != null && SmsSetting.Parameter8Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter8Key, SmsSetting.Parameter8Value);
                        }
                        if (SmsSetting.Parameter9Key != null && SmsSetting.Parameter9Key != "" && SmsSetting.Parameter9Value != null && SmsSetting.Parameter9Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter9Key, SmsSetting.Parameter9Value);
                        }
                        if (SmsSetting.Parameter10Key != null && SmsSetting.Parameter10Key != "" && SmsSetting.Parameter10Value != null && SmsSetting.Parameter10Value != "")
                        {
                            request.AddParameter(SmsSetting.Parameter10Key, SmsSetting.Parameter10Value);
                        }

                        var response = client.Execute(request);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (EnableDefaultSmsBranding == true)
                            {
                                ClsSmsUsed oClsSmsUsed = new ClsSmsUsed()
                                {
                                    AddedBy = AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = CompanyId,
                                    Id = Id,
                                    Type = Type,
                                };
                                oConnectionContext.DbClsSmsUsed.Add(oClsSmsUsed);
                                oConnectionContext.SaveChanges();
                            }

                            string[] arr = { "1", "" };
                            return arr;
                        }
                        else
                        {
                            string[] arr = { "0", response.Content };
                            return arr;
                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                string[] arr = { "0", ex.Message };
                return arr;
            }
        }
    }
}
