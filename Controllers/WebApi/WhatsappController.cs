using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EquiBillBook.Controllers.WebApi
{
    public class WhatsappController : ApiController
    {
        public dynamic WhatsappFunction(long CompanyId, string To, string Message, long WhatsappSettingsId)
        {
            try
            {
                ConnectionContext oConnectionContext = new ConnectionContext();
                dynamic WhatsappSetting = null;
                if (WhatsappSettingsId == 0)
                {
                    WhatsappSetting = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == CompanyId
                    && a.IsDefault == true).Select(a => new
                    {
                        WhatsappService = a.WhatsappService,
                        TwilioAccountSID = a.TwilioAccountSID,
                        TwilioAuthToken = a.TwilioAuthToken,
                        TwilioFrom = a.TwilioFrom,
                        DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId ==
                        oConnectionContext.DbClsBusinessSettings.Where(c => c.CompanyId == CompanyId).Select(c => c.CountryId).FirstOrDefault()).Select(b => b.DialingCode).FirstOrDefault(),
                    }).FirstOrDefault();
                }
                else
                {
                    WhatsappSetting = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == CompanyId
                    && a.WhatsappSettingsId == WhatsappSettingsId).Select(a => new
                    {
                        WhatsappService = a.WhatsappService,
                        TwilioAccountSID = a.TwilioAccountSID,
                        TwilioAuthToken = a.TwilioAuthToken,
                        TwilioFrom = a.TwilioFrom,
                        DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId ==
                        oConnectionContext.DbClsBusinessSettings.Where(c => c.CompanyId == CompanyId).Select(c => c.CountryId).FirstOrDefault()).Select(b => b.DialingCode).FirstOrDefault(),
                    }).FirstOrDefault();
                }

                if (WhatsappSetting == null)
                {
                    string[] arr = { "0", "WhatsApp settings not found", "" };
                    return arr;
                }

                if (WhatsappSetting.WhatsappService == 1)
                {
                    // WhatsApp Desktop - opens web link
                    string encodedMessage = HttpUtility.UrlEncode(Message);
                    string[] arr = { "2", "", "https://api.whatsapp.com/send?phone=" + WhatsappSetting.DialingCode + To + "&text=" + encodedMessage };
                    return arr;
                }
                else if (WhatsappSetting.WhatsappService == 2)
                {
                    // Twilio (paid)
                    string accountSid = WhatsappSetting.TwilioAccountSID;
                    string authToken = WhatsappSetting.TwilioAuthToken;

                    TwilioClient.Init(accountSid, authToken);

                    var message = MessageResource.Create(
                        body: Message,
                        from: new Twilio.Types.PhoneNumber("whatsapp:" + WhatsappSetting.TwilioFrom),
                        to: new Twilio.Types.PhoneNumber("whatsapp:" + WhatsappSetting.DialingCode + To)
                    );

                    string[] arr = { "1", "", "" };
                    return arr;
                }
                else if (WhatsappSetting.WhatsappService == 3)
                {
                    // WhatsApp Web via Node.js service (multi-tenant)
                    return SendViaNodeService(WhatsappSetting.DialingCode + To, Message, CompanyId);
                }
                else
                {
                    string[] arr = { "0", "Invalid WhatsApp Service", "" };
                    return arr;
                }
            }
            catch (Exception ex)
            {
                string[] arr = { "0", ex.Message, "" };
                return arr;
            }
        }

        private dynamic SendViaNodeService(string phoneNumber, string message, long CompanyId)
        {
            try
            {
                // Convert HTML to WhatsApp formatting
                string formattedMessage = ConvertHtmlToWhatsAppFormat(message);

                // Get Node.js service URL from Web.config
                string nodeServiceUrl = System.Configuration.ConfigurationManager.AppSettings["WhatsAppNodeServiceUrl"] 
                    ?? "http://localhost:3000/send-message";

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    // Include CompanyId in the payload for multi-tenant support
                    var payload = new
                    {
                        companyId = CompanyId.ToString(),  // Pass CompanyId for multi-tenant
                        phone = phoneNumber,
                        message = formattedMessage
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = client.PostAsync(nodeServiceUrl, content).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (result.success == true)
                        {
                            string[] arr = { "1", "", "" };
                            return arr;
                        }
                        else
                        {
                            string[] arr = { "0", "Node.js Service Error: " + (result.message ?? responseContent), "" };
                            return arr;
                        }
                    }
                    else
                    {
                        string[] arr = { "0", "Node.js Service Error: " + responseContent, "" };
                        return arr;
                    }
                }
            }
            catch (Exception ex)
            {
                string[] arr = { "0", "Node.js Service Exception: " + ex.Message, "" };
                return arr;
            }
        }

        /// <summary>
        /// Converts HTML tags to WhatsApp formatting
        /// WhatsApp supports: *bold*, _italic_, ~strikethrough~, ```monospace```
        /// </summary>
        private string ConvertHtmlToWhatsAppFormat(string htmlMessage)
        {
            if (string.IsNullOrEmpty(htmlMessage))
                return htmlMessage;

            string result = htmlMessage;

            // Decode HTML entities first
            result = HttpUtility.HtmlDecode(result);

            // Convert line breaks: <br>, <br/>, <br />, <BR>, etc.
            result = Regex.Replace(result, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

            // Convert paragraphs: <p>...</p> to double line breaks
            result = Regex.Replace(result, @"<p[^>]*>", "\n\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</p>", "", RegexOptions.IgnoreCase);

            // Convert bold: <b>...</b> or <strong>...</strong> to *text*
            result = Regex.Replace(result, @"<(b|strong)[^>]*>(.*?)</\1>", "*$2*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Convert italic: <i>...</i> or <em>...</em> to _text_
            result = Regex.Replace(result, @"<(i|em)[^>]*>(.*?)</\1>", "_$2_", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Convert strikethrough: <s>...</s> or <strike>...</strike> or <del>...</del> to ~text~
            result = Regex.Replace(result, @"<(s|strike|del)[^>]*>(.*?)</\1>", "~$2~", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Convert code: <code>...</code> to ```text```
            result = Regex.Replace(result, @"<code[^>]*>(.*?)</code>", "```$1```", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Convert headings: <h1>...</h1>, <h2>...</h2>, etc. to bold with line breaks
            result = Regex.Replace(result, @"<h[1-6][^>]*>(.*?)</h[1-6]>", "\n*$1*\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Convert lists: <li>...</li> to bullet points
            result = Regex.Replace(result, @"<li[^>]*>(.*?)</li>", "• $1\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            result = Regex.Replace(result, @"<(ul|ol)[^>]*>", "\n", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</(ul|ol)>", "\n", RegexOptions.IgnoreCase);

            // Convert divs with line breaks
            result = Regex.Replace(result, @"</div>", "\n", RegexOptions.IgnoreCase);

            // Remove all remaining HTML tags
            result = Regex.Replace(result, @"<[^>]+>", "", RegexOptions.IgnoreCase);

            // Clean up multiple consecutive line breaks (more than 2)
            result = Regex.Replace(result, @"\n{3,}", "\n\n", RegexOptions.Multiline);

            // Clean up multiple spaces (more than 2)
            result = Regex.Replace(result, @" {3,}", " ", RegexOptions.Multiline);

            // Trim whitespace from each line and remove empty lines at start/end
            var lines = result.Split('\n');
            var cleanedLines = new List<string>();
            foreach (var line in lines)
            {
                var trimmed = line.TrimEnd();
                if (!string.IsNullOrWhiteSpace(trimmed) || cleanedLines.Count > 0)
                {
                    cleanedLines.Add(trimmed);
                }
            }
            result = string.Join("\n", cleanedLines).Trim();

            return result;
        }
    }
}
