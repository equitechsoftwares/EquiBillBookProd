using DocumentFormat.OpenXml.ExtendedProperties;
using EquiBillBook.Controllers.WebApi.Common;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Xml;
using Twilio.Rest;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    public class CommonController : ApiController
    {
        public string baseUrl = WebConfigurationManager.AppSettings["baseUrl"];
        public string placeholderImage = WebConfigurationManager.AppSettings["placeholderImage"];
        public string businessplaceholderImage = WebConfigurationManager.AppSettings["businessplaceholderImage"];
        public string serverErrorMsg = WebConfigurationManager.AppSettings["serverErrorMsg"];
        public string adminMail = WebConfigurationManager.AppSettings["adminMail"];
        public string smtpUser = WebConfigurationManager.AppSettings["smtpUser"];
        public string expiryDays = WebConfigurationManager.AppSettings["expiryDays"];
        public string commPer = WebConfigurationManager.AppSettings["commPer"];
        public string webUrl = WebConfigurationManager.AppSettings["webUrl"];
        public string smsApiKey = "VpjKiwElNM2zkFDLoWPHx5td86IO9rhmyeJ7nqvAZCUYGSg13Xx2tDidqmRaKTOCLM7E0w918jBXV6vh";
        public decimal DollarRate = 81;

        ConnectionContext oConnectionContext = new ConnectionContext();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public string Sha256Encryption(string randomString)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }

        public string CreateToken()
        {
            Random rdn = new Random();
            string randomKey = Convert.ToString(rdn.Next(111111, 999999));
            string dd = DateTime.Now.ToString("ddMMyyyyhhmmsst");
            string token = Sha256Encryption(dd + randomKey);
            return token;
        }

        // Helper method to extract data from IHttpActionResult
        public async Task<ClsResponse> ExtractResponseFromActionResult(System.Web.Http.IHttpActionResult result)
        {
            try
            {
                // Use reflection to access the Content property
                var resultType = result.GetType();
                var contentProperty = resultType.GetProperty("Content");

                if (contentProperty != null)
                {
                    var content = contentProperty.GetValue(result);
                    if (content != null)
                    {
                        return serializer.Deserialize<ClsResponse>(serializer.Serialize(content));
                    }
                }

                // Fallback: Try to execute the result and read the response
                var request = new System.Net.Http.HttpRequestMessage();
                var config = new System.Web.Http.HttpConfiguration();
                var controllerContext = new System.Web.Http.Controllers.HttpControllerContext(
                    config,
                    new System.Web.Http.Routing.HttpRouteData(new System.Web.Http.Routing.HttpRoute()),
                    request);

                var responseMessage = await result.ExecuteAsync(System.Threading.CancellationToken.None);
                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                return serializer.Deserialize<ClsResponse>(responseContent);
            }
            catch (Exception ex)
            {
                return new ClsResponse { Status = 0, Message = "Failed to extract response: " + ex.Message };
            }
        }

        //[HttpGet]
        //public DateTime DateTimeTest(int hour,int minute)
        //{
        //    DateTime currentTime = DateTime.UtcNow;
        //    DateTime updatedTime = currentTime.AddHours(hour).AddMinutes(minute);
        //    return updatedTime;
        //}

        [HttpGet]
        public DateTime CurrentDate(long CompanyId)
        {
            var TimeZone = (from b in oConnectionContext.DbClsBusinessSettings
                            join a in oConnectionContext.DbClsTimeZone
                            on b.TimeZoneId equals a.TimeZoneId
                            where b.CompanyId == CompanyId
                            select new
                            {
                                a.SupportsDaylightSavingTime,
                                a.o1String,
                                a.o2String,
                                b.EnableDaylightSavingTime
                            }).FirstOrDefault();

            //var TimeZone = oConnectionContext.DbClsTimeZone.Where(a => a.TimeZoneId ==
            // oConnectionContext.DbClsBusinessSettings.Where(aa => aa.CompanyId == CompanyId).Select(aa => aa.TimeZoneId).FirstOrDefault()
            // ).Select(a => new
            // {
            //     a.SupportsDaylightSavingTime,
            //     a.o1String,
            //     a.o2String
            // }).FirstOrDefault();

            int hour = 0, minute = 0;
            string[] val;

            if (TimeZone != null)
            {
                if (TimeZone.EnableDaylightSavingTime == true)
                {
                    if (TimeZone.SupportsDaylightSavingTime == false)
                    {
                        val = TimeZone.o1String.Split(':');
                    }
                    else
                    {
                        val = TimeZone.o2String.Split(':');
                    }
                }
                else
                {
                    val = TimeZone.o1String.Split(':');
                }

                if (val[0].Contains('+'))
                {
                    hour = Convert.ToInt32(val[0]);
                    minute = Convert.ToInt32(val[1]);
                }
                else
                {
                    hour = Convert.ToInt32(val[0]);
                    minute = -Convert.ToInt32(val[1]);
                }
            }

            DateTime currentTime = DateTime.UtcNow;
            DateTime updatedTime = currentTime.AddHours(hour).AddMinutes(minute);
            return updatedTime;
        }

        public class timezones
        {
            public string o1String { get; set; }
            public string o2String { get; set; }
            public string DisplayName { get; set; }
            public string StandardName { get; set; }
            public string DaylightName { get; set; }
            public bool SupportsDaylightSavingTime { get; set; }
        }

        [HttpGet]
        public dynamic AlTimeZones()
        {
            string q = "";
            List<timezones> t = new List<timezones>();
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            var date1 = new DateTime(2015, 1, 15);
            var date2 = new DateTime(2015, 7, 15);

            Console.WriteLine(String.Format("{0,-62} {1,-32} {2,-32} {3,-15} {4,-20} {5,-20}", "Display Name", "Standard Name", "Daylight Name", "Supports DST", "UTC Standard Offset", "UTC Daylight Offset"));
            Console.WriteLine(String.Format("{0}", "".PadRight(186, '-')));

            foreach (var timezone in timezones)
            {
                var o1 = timezone.GetUtcOffset(date1);
                var o2 = timezone.GetUtcOffset(date2);
                var o1String = " 00:00";
                var o2String = " 00:00";

                if (o1 < TimeSpan.Zero)
                    o1String = o1.ToString(@"\-hh\:mm");
                if (o1 > TimeSpan.Zero)
                    o1String = o1.ToString(@"\+hh\:mm");
                if (o2 < TimeSpan.Zero)
                    o2String = o2.ToString(@"\-hh\:mm");
                if (o2 > TimeSpan.Zero)
                    o2String = o2.ToString(@"\+hh\:mm");

                q = q + "<br/>" + (String.Format("{0,-62} {1,-32} {2,-32} {3,-15} {4,-20} {5,-20}",
                                                timezone.DisplayName,
                                                timezone.StandardName,
                                                timezone.DaylightName,
                                                timezone.SupportsDaylightSavingTime ? "Yes" : "No",
                                                o1String,
                                                o2String));


                t.Add(new timezones
                {
                    DisplayName = timezone.DisplayName,
                    StandardName = timezone.StandardName,
                    DaylightName = timezone.DaylightName,
                    SupportsDaylightSavingTime = timezone.SupportsDaylightSavingTime,
                    o1String = o1String,
                    o2String = o2String,
                });

                //ClsTimeZone oClsTimeZone = new ClsTimeZone()
                //{
                //    DisplayName = timezone.DisplayName.Split(')')[1].Trim(),
                //    StandardName = timezone.StandardName,
                //    DaylightName = timezone.DaylightName,
                //    SupportsDaylightSavingTime = timezone.SupportsDaylightSavingTime,
                //    o1String = o1String,
                //    o2String = o2String,
                //    AddedBy = 1,
                //    AddedOn = CurrentDate(),
                //    CompanyId = 1,
                //    IsActive = true
                //};
                //oConnectionContext.DbClsTimeZone.Add(oClsTimeZone);
                //oConnectionContext.SaveChanges();
            }

            return Json(t);
        }

        public bool EmailValidationCheck(string emailid)
        {
            string email = emailid;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
                return true;
            else
                return false;
        }

        public bool MobileValidationCheck(string mobileNo)
        {
            string motif = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

            if (mobileNo != null) return Regex.IsMatch(mobileNo, motif);
            else return false;
        }

        public bool IsValidDomainName(string domain)
        {
            bool isDomainExist = false;
            System.Net.IPHostEntry host;
            try
            {
                host = System.Net.Dns.GetHostEntry(domain);
                if (host != null)
                {
                    isDomainExist = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "No such host is known")
                {
                    isDomainExist = false;
                }
            }

            return isDomainExist;
        }

        //[HttpGet]
        //public float GetExchangeRate(string from, string to, float amount = 1)
        //{
        //    // If currency's are empty abort
        //    if (from == null || to == null)
        //        return 0;

        //    // Convert Euro to Euro
        //    if (from.ToLower() == "eur" && to.ToLower() == "eur")
        //        return amount;

        //    try
        //    {
        //        // First Get the exchange rate of both currencies in euro
        //        float toRate = GetCurrencyRateInEuro(to);
        //        float fromRate = GetCurrencyRateInEuro(from);

        //        // Convert Between Euro to Other Currency
        //        if (from.ToLower() == "eur")
        //        {
        //            return (amount * toRate);
        //        }
        //        else if (to.ToLower() == "eur")
        //        {
        //            return (amount / fromRate);
        //        }
        //        else
        //        {
        //            // Calculate non EURO exchange rates From A to B
        //            return (amount * toRate) / fromRate;
        //        }
        //    }
        //    catch { return 0; }
        //}

        //public float GetCurrencyRateInEuro(string currency)
        //{
        //    if (currency.ToLower() == "")
        //        throw new ArgumentException("Invalid Argument! currency parameter cannot be empty!");
        //    if (currency.ToLower() == "eur")
        //        throw new ArgumentException("Invalid Argument! Cannot get exchange rate from EURO to EURO");

        //    try
        //    {
        //        // Create with currency parameter, a valid RSS url to ECB euro exchange rate feed
        //        string rssUrl = string.Concat("http://www.ecb.int/rss/fxref-", currency.ToLower() + ".html");

        //        // Create & Load New Xml Document
        //        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        //        doc.Load(rssUrl);

        //        // Create XmlNamespaceManager for handling XML namespaces.
        //        System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
        //        nsmgr.AddNamespace("rdf", "http://purl.org/rss/1.0/");
        //        nsmgr.AddNamespace("cb", "http://www.cbwiki.net/wiki/index.php/Specification_1.1");

        //        // Get list of daily currency exchange rate between selected "currency" and the EURO
        //        System.Xml.XmlNodeList nodeList = doc.SelectNodes("//rdf:item", nsmgr);

        //        // Loop Through all XMLNODES with daily exchange rates
        //        foreach (System.Xml.XmlNode node in nodeList)
        //        {
        //            // Create a CultureInfo, this is because EU and USA use different sepperators in float (, or .)
        //            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        //            ci.NumberFormat.CurrencyDecimalSeparator = ".";

        //            try
        //            {
        //                // Get currency exchange rate with EURO from XMLNODE
        //                float exchangeRate = float.Parse(
        //                    node.SelectSingleNode("//cb:statistics//cb:exchangeRate//cb:value", nsmgr).InnerText,
        //                    NumberStyles.Any,
        //                    ci);

        //                return exchangeRate;
        //            }
        //            catch { }
        //        }

        //        // currency not parsed!! 
        //        // return default value
        //        return 0;
        //    }
        //    catch
        //    {
        //        // currency not parsed!! 
        //        // return default value
        //        return 0;
        //    }
        //}

        public string ImageToBase64(string Path)
        {
            using (Image image = Image.FromFile(System.Web.Hosting.HostingEnvironment.MapPath(Path)))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        public long InsertLoginDetails(long AddedBy, string UserType, bool IsLoggedOut, string DeviceId, string DeviceType, string PushID, string Latitude, string Longitude, string IpAddress, string Token, DateTime CurrentDate, string Browser)
        {
            ClsLoginDetails oClsLoginDetails = new ClsLoginDetails()
            {
                AddedBy = AddedBy,
                AddedOn = CurrentDate,
                ModifiedOn = CurrentDate,
                UserType = UserType,
                IsLoggedOut = false,
                DeviceId = DeviceId,
                DeviceType = DeviceType,
                PushID = PushID,
                Latitude = Latitude,
                Longitude = Longitude,
                IpAddress = IpAddress,
                Token = Token,
                Browser = Browser
            };
            oConnectionContext.DbClsLoginDetails.Add(oClsLoginDetails);
            oConnectionContext.SaveChanges();
            return oClsLoginDetails.LoginDetailsId;
        }

        //public string FetchAppVersion(string type)
        //{
        //    if (type == "android")
        //    {
        //        return oConnectionContext.DbClsCounter.Select(a => a.AndroidAppVersion).FirstOrDefault();
        //    }
        //    else if (type == "ios")
        //    {
        //        return oConnectionContext.DbClsCounter.Select(a => a.IosAppVersion).FirstOrDefault();
        //    }
        //    else
        //    {
        //        return "0";
        //    }
        //}

        public async Task<dynamic> PostMethod(string requestData, string url, string userType, string Token, string id)
        {
            using (var stringContent = new StringContent(requestData, System.Text.Encoding.UTF8, "application/json"))

            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromMinutes(30);

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(
                                System.Text.Encoding.ASCII.GetBytes(
                                    string.Format("{0}:{1}", userType + "_" + id, Token))));

                    var response = await client.PostAsync(url, stringContent);

                    var result = await response.Content.ReadAsStringAsync();

                    return result;
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        public async Task<dynamic> GetMethod(string requestData, string url)
        {
            string sd = DateTime.Now.ToString();
            using (var stringContent = new StringContent(requestData, System.Text.Encoding.UTF8, "application/json"))

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(
                                System.Text.Encoding.ASCII.GetBytes(
                                    string.Format("{0}:{1}", "andy", "password"))));

                    var response = await client.GetAsync(url);
                    var result = await response.Content.ReadAsStringAsync();
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    var id = await response.Content.ReadAsStringAsync();

                    //}
                    return result;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }

        }

        public void InsertActivityLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            if (obj.Category == "Register" || obj.Category == "Login" || obj.Category == "Logout" || obj.Category == "User Role" || obj.Category == "Users")
            {
                InsertUserLog(obj, CurrentDate);
            }
            else if (obj.Category == "Categories" || obj.Category == "Sub Categories" || obj.Category == "Sub Sub Categories" || obj.Category == "Units"
                || obj.Category == "Secondary Units" || obj.Category == "Tertiary Units" || obj.Category == "Quaternary Units" || obj.Category == "Variation"
                || obj.Category == "Warranties" || obj.Category == "Brand" || obj.Category == "Items" || obj.Category == "Opening Stock"
                 || obj.Category == "Selling Price Group" || obj.Category == "Print Label" || obj.Category == "Stock Adjust" || obj.Category == "Stock Transfer")
            {
                InsertItemLog(obj, CurrentDate);
            }
            else if (obj.Category == "Suppliers" || obj.Category == "Purchase Quotation" || obj.Category == "Purchase Order" || obj.Category == "Purchase"
            || obj.Category == "Purchase Return" || obj.Category == "Supplier Payment" || obj.Category == "Supplier Refund" || obj.Category == "Expense")
            {
                InsertPurchaseLog(obj, CurrentDate);
            }
            else if (obj.Category == "Customer Group" || obj.Category == "Customers" || obj.Category == "Sales Quotation" || obj.Category == "Sales Order"
            || obj.Category == "Delivery Challan" || obj.Category == "Sales Proforma" || obj.Category == "Sales" || obj.Category == "POS" ||
            obj.Category == "Sales Return" || obj.Category == "Payment Link" || obj.Category == "Customer Payment" || obj.Category == "Customer Refund")
            {
                InsertSaleLog(obj, CurrentDate);
            }
            else if (obj.Category == "State" || obj.Category == "City" || obj.Category == "Branch")
            {
                InsertPlaceLog(obj, CurrentDate);
            }
            else if (obj.Category == "Banks" || obj.Category == "Contra")
            {
                InsertBankLog(obj, CurrentDate);
            }
            else if (obj.Category == "Chart of Accounts" || obj.Category == "Journal")
            {
                InsertAccountLog(obj, CurrentDate);
            }
            else if (obj.Category == "Tax Setting" || obj.Category == "Tax Exemption" || obj.Category == "Currency" || obj.Category == "Tax List" || obj.Category == "Tax Group" ||
            obj.Category == "Item Code" || obj.Category == "Payment Methods" || obj.Category == "Payment Terms" || obj.Category == "Business Settings"
            || obj.Category == "Invoice Templates" || obj.Category == "Notification Templates" || obj.Category == "Reminders" || obj.Category == "Domain"
            || obj.Category == "Profile Update" || obj.Category == "Change Password" || obj.Category == "Change Login Email" || obj.Category == "Password Reset")
            {
                InsertSettingLog(obj, CurrentDate);
            }
            else if (obj.Category == "Subscription Plan" || obj.Category == "Feedback" || obj.Category == "Support Ticket")
            {
                InsertOtherLog(obj, CurrentDate);
            }
        }
        //}

        public void InsertUserLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsUserLog oClsUserLog = new ClsUserLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsUserLog.Add(oClsUserLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertItemLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsItemLog oClsItemLog = new ClsItemLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsItemLog.Add(oClsItemLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertPurchaseLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsPurchaseLog oClsPurchaseLog = new ClsPurchaseLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsPurchaseLog.Add(oClsPurchaseLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertSaleLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsSaleLog oClsSaleLog = new ClsSaleLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsSaleLog.Add(oClsSaleLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertPlaceLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsPlaceLog oClsPlaceLog = new ClsPlaceLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsPlaceLog.Add(oClsPlaceLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertBankLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsBankLog oClsBankLog = new ClsBankLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsBankLog.Add(oClsBankLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertAccountLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsAccountLog oClsAccountLog = new ClsAccountLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsAccountLog.Add(oClsAccountLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertSettingLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsSettingLog oClsSettingLog = new ClsSettingLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsSettingLog.Add(oClsSettingLog);
            oConnectionContext.SaveChanges();
        }

        public void InsertOtherLog(ClsActivityLogVm obj, DateTime CurrentDate)
        {
            ClsOtherLog oClsOtherLog = new ClsOtherLog()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Browser = obj.Browser,
                Category = obj.Category,
                CompanyId = obj.CompanyId,
                Description = obj.Description,
                Id = obj.Id,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = obj.Type,
            };
            oConnectionContext.DbClsOtherLog.Add(oClsOtherLog);
            oConnectionContext.SaveChanges();
        }

        public dynamic GetStockList(long BranchId, long ItemDetailsId)
        {
            var availableStocks = (from a in oConnectionContext.DbClsPurchase
                                   join b in oConnectionContext.DbClsPurchaseDetails
                                    on a.PurchaseId equals b.PurchaseId
                                   where a.BranchId == BranchId
                                   && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                   select new
                                   {
                                       QuantityRemaining = b.QuantityRemaining,
                                       Id = b.PurchaseDetailsId,
                                       Type = "Purchase",
                                       b.AddedOn
                                   }).Union(
                            from a in oConnectionContext.DbClsOpeningStock
                                //join b in oConnectionContext.DbClsOpeningStockDetails
                                //    on a.OpeningStockId equals b.OpeningStockId
                            where a.BranchId == BranchId && a.ItemDetailsId == ItemDetailsId && a.QuantityRemaining > 0
                            select new
                            {
                                QuantityRemaining = a.QuantityRemaining,
                                Id = a.OpeningStockId,
                                Type = "Opening Stock",
                                a.AddedOn
                            }).Union(from a in oConnectionContext.DbClsStockAdjustment
                                     join b in oConnectionContext.DbClsStockAdjustmentDetails
on a.StockAdjustmentId equals b.StockAdjustmentId
                                     where a.BranchId == BranchId && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                     select new
                                     {
                                         QuantityRemaining = b.QuantityRemaining,
                                         Id = b.StockAdjustmentDetailsId,
                                         Type = "Stock Adjustment",
                                         b.AddedOn
                                     }).OrderByDescending(a => DbFunctions.TruncateTime(a.AddedOn));

            return availableStocks;
        }

        public decimal checkStockAvailability(long BranchId, long ItemDetailsId)
        {
            decimal purchaseStocksQty = (from a in oConnectionContext.DbClsPurchase
                                         join b in oConnectionContext.DbClsPurchaseDetails
                                          on a.PurchaseId equals b.PurchaseId
                                         where a.BranchId == BranchId
                                         && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                         select b.QuantityRemaining).Count() == 0 ? 0 : (from a in oConnectionContext.DbClsPurchase
                                                                                         join b in oConnectionContext.DbClsPurchaseDetails
                                                                                          on a.PurchaseId equals b.PurchaseId
                                                                                         where a.BranchId == BranchId
                                                                                         && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                                                                         select b.QuantityRemaining).DefaultIfEmpty().Sum();

            decimal openingStocksQty = (from a in oConnectionContext.DbClsOpeningStock
                                            //join b in oConnectionContext.DbClsOpeningStockDetails
                                            //on a.OpeningStockId equals b.OpeningStockId
                                        where a.BranchId == BranchId && a.ItemDetailsId == ItemDetailsId && a.QuantityRemaining > 0
                                        select a.QuantityRemaining).Count() == 0 ? 0 :
                                    (
                                    from a in oConnectionContext.DbClsOpeningStock
                                        //join b in oConnectionContext.DbClsOpeningStockDetails
                                        //on a.OpeningStockId equals b.OpeningStockId
                                    where a.BranchId == BranchId && a.ItemDetailsId == ItemDetailsId && a.QuantityRemaining > 0
                                    select a.QuantityRemaining).DefaultIfEmpty().Sum();

            decimal stockAdjustmentQty = (from a in oConnectionContext.DbClsStockAdjustment
                                          join b in oConnectionContext.DbClsStockAdjustmentDetails
                                           on a.StockAdjustmentId equals b.StockAdjustmentId
                                          where a.BranchId == BranchId && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                          select b.QuantityRemaining).Count() == 0 ? 0 :
                                        (from a in oConnectionContext.DbClsStockAdjustment
                                         join b in oConnectionContext.DbClsStockAdjustmentDetails
                                         on a.StockAdjustmentId equals b.StockAdjustmentId
                                         where a.BranchId == BranchId && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                         select b.QuantityRemaining).DefaultIfEmpty().Sum();
            return purchaseStocksQty + openingStocksQty + stockAdjustmentQty;
        }

        public decimal StockConversion(decimal Quantity, long ItemId, int PriceAddedFor)
        {
            var ItemDetails = oConnectionContext.DbClsItem.Where(a => a.ItemId == ItemId).Select(a => new
            {
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
            }).FirstOrDefault();

            decimal convertedStock = 0;

            if (ItemDetails.TToQValue != 0)
            {
                if (PriceAddedFor == 4)
                {
                    convertedStock = Quantity;
                }
                else if (PriceAddedFor == 3)
                {
                    convertedStock = Quantity * ItemDetails.TToQValue;
                }
                else if (PriceAddedFor == 2)
                {
                    convertedStock = Quantity * (1 * ItemDetails.SToTValue * ItemDetails.TToQValue);
                }
                else if (PriceAddedFor == 1)
                {
                    convertedStock = Quantity * (1 * ItemDetails.UToSValue * ItemDetails.SToTValue * ItemDetails.TToQValue);
                }
            }
            else if (ItemDetails.SToTValue != 0)
            {
                if (PriceAddedFor == 4)
                {
                    convertedStock = Quantity;
                }
                else if (PriceAddedFor == 3)
                {
                    convertedStock = Quantity * (1 * ItemDetails.SToTValue);
                }
                else if (PriceAddedFor == 2)
                {
                    convertedStock = Quantity * (1 * ItemDetails.UToSValue * ItemDetails.SToTValue);
                }
            }
            else if (ItemDetails.UToSValue != 0)
            {
                if (PriceAddedFor == 4)
                {
                    convertedStock = Quantity;
                }
                else if (PriceAddedFor == 3)
                {
                    convertedStock = Quantity * ItemDetails.UToSValue;
                }
            }
            else
            {
                convertedStock = Quantity;
            }

            //if (PriceAddedFor == 4)
            //{
            //    convertedStock = Quantity;
            //}
            //else if (PriceAddedFor == 3)
            //{
            //    convertedStock = Quantity * ItemDetails.TToQValue;
            //}
            //else if (PriceAddedFor == 2)
            //{
            //    convertedStock = Quantity * (1 * ItemDetails.SToTValue * ItemDetails.TToQValue);

            //}
            //else if (PriceAddedFor == 1)
            //{
            //    convertedStock = Quantity * (1 * ItemDetails.UToSValue * ItemDetails.SToTValue * ItemDetails.TToQValue);
            //}

            return convertedStock;
        }

        public List<ClsStockDeductionIds> deductStock(long BranchId, long ItemDetailsId, decimal Quantity, long ItemId, int PriceAddedFor)
        {
            var ItemDetails = oConnectionContext.DbClsItem.Where(a => a.ItemId == ItemId).Select(a => new
            {
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                a.CompanyId
            }).FirstOrDefault();

            var StockAccountingMethod = oConnectionContext.DbClsItemSettings.Where(b => b.CompanyId == ItemDetails.CompanyId).Select(b => b.StockAccountingMethod).FirstOrDefault();

            var purchaseStocksQty = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
                                      on a.PurchaseId equals b.PurchaseId
                                     where a.BranchId == BranchId
                                     && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                     && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && b.IsActive == true && b.IsDeleted == false
                                     select new
                                     {
                                         b.PriceAddedFor,
                                         b.AddedOn,
                                         Type = "purchase",
                                         QuantityRemaining = b.QuantityRemaining,
                                         Id = b.PurchaseDetailsId
                                     }).ToList();

            var openingStocksQty = (from a in oConnectionContext.DbClsOpeningStock
                                    where a.BranchId == BranchId && a.ItemDetailsId == ItemDetailsId && a.QuantityRemaining > 0
                                         && a.IsActive == true && a.IsDeleted == false && a.IsActive == true && a.IsDeleted == false
                                    select new
                                    {
                                        a.PriceAddedFor,
                                        a.AddedOn,
                                        Type = "openingstock",
                                        QuantityRemaining = a.QuantityRemaining,
                                        Id = a.OpeningStockId
                                    }).ToList();

            var stockReceivedQty = (from a in oConnectionContext.DbClsStockTransfer
                                    join b in oConnectionContext.DbClsStockTransferDetails
                                    on a.StockTransferId equals b.StockTransferId
                                    where a.ToBranchId == BranchId && b.ItemDetailsId == ItemDetailsId && b.QuantityRemaining > 0
                                           && a.IsActive == true && a.IsDeleted == false && b.IsActive == true && b.IsDeleted == false
                                    select new
                                    {
                                        b.PriceAddedFor,
                                        a.AddedOn,
                                        Type = "stocktransfer",
                                        QuantityRemaining = b.QuantityRemaining,
                                        Id = b.StockTransferDetailsId
                                    }).ToList();

            dynamic stockList = null;

            if (StockAccountingMethod == 1)
            {
                stockList = purchaseStocksQty.Union(openingStocksQty).Union(stockReceivedQty).OrderBy(a => a.AddedOn);
            }
            else
            {
                stockList = purchaseStocksQty.Union(openingStocksQty).Union(stockReceivedQty).OrderByDescending(a => a.AddedOn);
            }

            List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();
            decimal qtyRemaininToDeduct = Quantity;
            foreach (var stock in stockList)
            {
                if (qtyRemaininToDeduct != 0)
                {
                    decimal availableQty = stock.QuantityRemaining;
                    decimal qty = 0;
                    string query = "";

                    if (availableQty >= qtyRemaininToDeduct)
                    {
                        qty = qtyRemaininToDeduct;
                    }
                    else if (availableQty < qtyRemaininToDeduct)
                    {
                        qty = availableQty;
                    }

                    if (stock.Type == "purchase")
                    {
                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + qty + ",\"QuantitySold\"=\"QuantitySold\"+" + qty + " where \"PurchaseDetailsId\"=" + stock.Id;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                    else if (stock.Type == "openingstock")
                    {
                        query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + qty + ",\"QuantitySold\"=\"QuantitySold\"+" + qty + " where \"OpeningStockId\"=" + stock.Id;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                    else
                    {
                        query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + qty + ",\"QuantitySold\"=\"QuantitySold\"+" + qty + " where \"StockTransferDetailsId\"=" + stock.Id;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }

                    oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = stock.Id, Type = stock.Type, Quantity = qty });

                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + qty + ") where \"BranchId\"=" + BranchId + " and \"ItemDetailsId\"=" + ItemDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    qtyRemaininToDeduct = qtyRemaininToDeduct - qty;
                }
            }

            //serializer.MaxJsonLength = 2147483644;
            //string _json = serializer.Serialize(oClsStockDeductionIds);

            //return _json;

            return oClsStockDeductionIds;
        }

        public List<ClsStockDeductionIds> deductStockLot(long BranchId, long ItemDetailsId, decimal Quantity, long Id, string Type)
        {
            string query = "";
            List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();

            if (Type == "purchase")
            {
                query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + Quantity + ",\"QuantitySold\"=\"QuantitySold\"+" + Quantity + " where \"PurchaseDetailsId\"=" + Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);
            }
            else if (Type == "openingstock")
            {
                query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + Quantity + ",\"QuantitySold\"=\"QuantitySold\"+" + Quantity + " where \"OpeningStockId\"=" + Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);
            }
            else
            {
                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + Quantity + ",\"QuantitySold\"=\"QuantitySold\"+" + Quantity + " where \"StockTransferDetailsId\"=" + Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);
            }

            oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = Id, Type = Type, Quantity = Quantity });

            query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"-(" + Quantity + ") where \"BranchId\"=" + BranchId + " and \"ItemDetailsId\"=" + ItemDetailsId;
            oConnectionContext.Database.ExecuteSqlCommand(query);

            //serializer.MaxJsonLength = 2147483644;
            //string _json = serializer.Serialize(oClsStockDeductionIds);

            //return _json;

            return oClsStockDeductionIds;
        }

        public List<ClsStockDeductionIds> addStock(long BranchId, long ItemDetailsId, decimal Quantity, long ItemId, int PriceAddedFor)
        {
            var ItemDetails = oConnectionContext.DbClsItem.Where(a => a.ItemId == ItemId).Select(a => new
            {
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                a.CompanyId
            }).FirstOrDefault();

            var StockAccountingMethod = oConnectionContext.DbClsItemSettings.Where(b => b.CompanyId == ItemDetails.CompanyId).Select(b => b.StockAccountingMethod).FirstOrDefault();

            var purchaseStocksQty = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
                                      on a.PurchaseId equals b.PurchaseId
                                     where a.BranchId == BranchId
                                     && b.ItemDetailsId == ItemDetailsId //&& b.QuantityRemaining > 0
                                     && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && b.IsActive == true && b.IsDeleted == false
                                     select new
                                     {
                                         b.PriceAddedFor,
                                         b.AddedOn,
                                         Type = "purchase",
                                         QuantityRemaining = b.QuantityRemaining,
                                         Id = b.PurchaseDetailsId
                                     }).ToList();

            var openingStocksQty = (from a in oConnectionContext.DbClsOpeningStock
                                    where a.BranchId == BranchId && a.ItemDetailsId == ItemDetailsId //&& a.QuantityRemaining > 0
                                         && a.IsActive == true && a.IsDeleted == false && a.IsActive == true && a.IsDeleted == false
                                    select new
                                    {
                                        a.PriceAddedFor,
                                        a.AddedOn,
                                        Type = "openingstock",
                                        QuantityRemaining = a.QuantityRemaining,
                                        Id = a.OpeningStockId
                                    }).ToList();

            var stockReceivedQty = (from a in oConnectionContext.DbClsStockTransfer
                                    join b in oConnectionContext.DbClsStockTransferDetails
                                    on a.StockTransferId equals b.StockTransferId
                                    where a.ToBranchId == BranchId && b.ItemDetailsId == ItemDetailsId //&& b.QuantityRemaining > 0
                                           && a.IsActive == true && a.IsDeleted == false && b.IsActive == true && b.IsDeleted == false
                                    select new
                                    {
                                        b.PriceAddedFor,
                                        a.AddedOn,
                                        Type = "stocktransfer",
                                        QuantityRemaining = b.QuantityRemaining,
                                        Id = b.StockTransferDetailsId
                                    }).ToList();

            dynamic stockList = null;

            if (StockAccountingMethod == 1)
            {
                stockList = purchaseStocksQty.Union(openingStocksQty).Union(stockReceivedQty)
                    .OrderBy(a => a.AddedOn);
            }
            else
            {
                stockList = purchaseStocksQty.Union(openingStocksQty).Union(stockReceivedQty)
                    .OrderByDescending(a => a.AddedOn);
            }

            List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();
            decimal qtyRemaininToDeduct = Quantity;
            string query = "";
            foreach (var stock in stockList)
            {
                if (qtyRemaininToDeduct != 0)
                {
                    if (stock.Type == "purchase")
                    {
                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + Quantity + " where \"PurchaseDetailsId\"=" + stock.Id;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                    else if (stock.Type == "openingstock")
                    {
                        query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + Quantity + " where \"OpeningStockId\"=" + stock.Id;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                    else
                    {
                        query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + Quantity + " where \"StockTransferDetailsId\"=" + stock.Id;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }

                    oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = stock.Id, Type = stock.Type, Quantity = Quantity });

                    query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + Quantity + ") where \"BranchId\"=" + BranchId + " and \"ItemDetailsId\"=" + ItemDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    qtyRemaininToDeduct = 0;
                }
            }

            //serializer.MaxJsonLength = 2147483644;
            //string _json = serializer.Serialize(oClsStockDeductionIds);

            //return _json;
            return oClsStockDeductionIds;
        }

        public List<ClsStockDeductionIds> addStockLot(long BranchId, long ItemDetailsId, decimal Quantity, long Id, string Type)
        {
            string query = "";
            List<ClsStockDeductionIds> oClsStockDeductionIds = new List<ClsStockDeductionIds>();

            if (Type == "purchase")
            {
                query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + Quantity + " where \"PurchaseDetailsId\"=" + Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);
            }
            else if (Type == "openingstock")
            {
                query = "update \"tblOpeningStock\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + Quantity + " where \"OpeningStockId\"=" + Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);
            }
            else
            {
                query = "update \"tblStockTransferDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + Quantity + " where \"StockTransferDetailsId\"=" + Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);
            }

            oClsStockDeductionIds.Add(new ClsStockDeductionIds { Id = Id, Type = Type, Quantity = Quantity });

            query = "update \"tblItemBranchMap\" set \"Quantity\" = \"Quantity\"+(" + Quantity + ") where \"BranchId\"=" + BranchId + " and \"ItemDetailsId\"=" + ItemDetailsId;
            oConnectionContext.Database.ExecuteSqlCommand(query);

            //serializer.MaxJsonLength = 2147483644;
            //string _json = serializer.Serialize(oClsStockDeductionIds);

            //return _json;
            return oClsStockDeductionIds;
        }

        public int fetchPlanQuantity(long CompanyId, string Type)
        {
            int BaseQuantity = 0; int AddonQuantity = 0; int ExtraAddonQuantity = 0;
            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == CompanyId && a.StartDate != null && a.Status == 2).Select(a => a.TransactionId).FirstOrDefault();
            if (Type == "User")
            {
                BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseUser).FirstOrDefault();
            }
            else if (Type == "Branch")
            {
                BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseBranch).FirstOrDefault();
            }
            //else if (Type == "Item")
            //{
            //    BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseItem).FirstOrDefault();
            //}
            else if (Type == "Order")
            {
                BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseOrder).FirstOrDefault();
            }
            else if (Type == "Domain")
            {
                BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseDomain).FirstOrDefault();
            }
            else if (Type == "Bill")
            {
                BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseBill).FirstOrDefault();
            }
            else if (Type == "Tax Setting")
            {
                BaseQuantity = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => a.BaseTaxSetting).FirstOrDefault();
            }

            //AddonQuantity = oConnectionContext.DbClsTransactionDetails.Where(a => a.TransactionId == a.TransactionId && a.Type == Type).Select(a => a.Quantity).FirstOrDefault();

            AddonQuantity = (from a in oConnectionContext.DbClsTransaction
                             join b in oConnectionContext.DbClsTransactionDetails
                               on a.TransactionId equals b.TransactionId
                             where a.TransactionId == TransactionId && a.Status == 2
                             && b.Type == Type
                             select b.Quantity).DefaultIfEmpty().Sum();


            ExtraAddonQuantity = (from a in oConnectionContext.DbClsTransaction
                                  join b in oConnectionContext.DbClsTransactionDetails
on a.TransactionId equals b.TransactionId
                                  where a.ParentTransactionId == TransactionId && a.Status == 2
                                  && b.Type == Type
                                  select b.Quantity).DefaultIfEmpty().Sum();

            return BaseQuantity + AddonQuantity + ExtraAddonQuantity;
        }

        [HttpGet]
        public void ExpirePlans()
        {
            //var Users = oConnectionContext.DbClsUser.Where(a => a.IsCompany == true).Select(a => new { a.CompanyId }).ToList();

            var Users = oConnectionContext.DbClsTransaction.Where(a => a.StartDate != null && a.Status == 2 && a.IsActive == true
            && a.ParentTransactionId == 0).AsEnumerable()
                .Where(a => a.ExpiryDate.Value.Date < DateTime.Now.Date).Select(a => new { a.CompanyId, a.StartDate, a.ExpiryDate, a.TransactionId }).
                ToList();

            foreach (var item in Users)
            {
                DateTime Date = CurrentDate(item.CompanyId);

                int currentBranchQuantity = 0, currentUserQuantity = 0, currentItemQuantity = 0;//, currentOrderQuantity = 0;

                if (item.ExpiryDate.Value.Date < Date.Date)
                {
                    currentUserQuantity = oConnectionContext.DbClsUser.Where(a => a.CompanyId == item.CompanyId && a.IsDeleted == false && a.IsActive == true && a.UserType == "User").Count();

                    currentBranchQuantity = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == item.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();

                    currentItemQuantity = oConnectionContext.DbClsItem.Where(a => a.CompanyId == item.CompanyId && a.IsActive == true && a.IsDeleted == false).Count();

                    string query = "update \"tblTransaction\" set \"IsActive\"=False where \"TransactionId\"=" + item.TransactionId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    var NextPlan = oConnectionContext.DbClsTransaction.OrderBy(a => a.TransactionId).Where(a => a.CompanyId == item.CompanyId &&
                    a.StartDate == null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();

                    if (NextPlan != null)
                    {
                        ClsTransaction oClsTransaction = new ClsTransaction()
                        {
                            TransactionId = NextPlan.TransactionId,
                            StartDate = Date,
                            ExpiryDate = DateTime.Now.AddMonths(oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == NextPlan.TransactionId).Select(b => b.Quantity).FirstOrDefault()),
                            //ModifiedBy = obj.AddedBy,
                            ModifiedOn = Date,
                            Status = 2,
                            IsActive = true
                        };
                        oConnectionContext.DbClsTransaction.Attach(oClsTransaction);
                        oConnectionContext.Entry(oClsTransaction).Property(x => x.StartDate).IsModified = true;
                        oConnectionContext.Entry(oClsTransaction).Property(x => x.ExpiryDate).IsModified = true;
                        //oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsTransaction).Property(x => x.Status).IsModified = true;
                        oConnectionContext.Entry(oClsTransaction).Property(x => x.IsActive).IsModified = true;
                        oConnectionContext.SaveChanges();

                        int nextBranchQuantity = 0, nextUserQuantity = 0, nextItemQuantity = 0;//, nextOrderQuantity = 0;
                        nextUserQuantity = fetchPlanQuantity(item.CompanyId, "User");

                        nextBranchQuantity = fetchPlanQuantity(item.CompanyId, "Branch");

                        nextItemQuantity = fetchPlanQuantity(item.CompanyId, "Item");

                        int reducedQuantity = 0;
                        if (currentUserQuantity > nextUserQuantity)
                        {
                            reducedQuantity = currentUserQuantity - nextUserQuantity;
                            query = "update \"tblUser\" set \"IsActive\"=True where \"UserId\" in (select top " + reducedQuantity + " \"UserId\" from \"tblUser\" where \"IsCompany\" == 0 and \"CompanyId\"=" + item.CompanyId + " order by \"UserId\" desc)";
                        }
                        if (currentBranchQuantity > nextBranchQuantity)
                        {
                            reducedQuantity = currentBranchQuantity - nextBranchQuantity;
                            query = "update \"tblBranch\" set \"IsActive\"=True where \"BranchId\" in (select top " + reducedQuantity + " \"BranchId\" from \"tblBranch\" where \"CompanyId\"=" + item.CompanyId + " order by \"BranchId\" desc)";
                        }
                        if (currentItemQuantity > nextItemQuantity)
                        {
                            reducedQuantity = currentItemQuantity - nextItemQuantity;
                            query = "update \"tblItem\" set \"IsActive\"=True where \"ItemId\" in (select top " + reducedQuantity + " \"ItemId\" from \"tblItem\" where \"CompanyId\"=" + item.CompanyId + " order by \"ItemId\" desc)";
                        }
                    }
                }
            }
        }

        [HttpGet]
        public void CheckPaymentOverDues()
        {
            var purchase = (from a in oConnectionContext.DbClsPurchase
                            where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                            select new
                            {
                                a.CompanyId,
                                a.PurchaseId,
                                a.PurchaseDate,
                                a.SupplierId,
                                a.DueDate
                            }).ToList();

            var purchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                  where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                  select new
                                  {
                                      a.CompanyId,
                                      a.PurchaseReturnId,
                                      a.Date,
                                      a.SupplierId,
                                      a.DueDate,
                                      IsPurchaseDeleted = oConnectionContext.DbClsPurchase.Where(c => c.PurchaseId == a.PurchaseId
                                      && c.IsDeleted == true).Count() == 1 ? true : false
                                  }).ToList();


            var sales = (from a in oConnectionContext.DbClsSales
                         where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                         select new
                         {
                             a.CompanyId,
                             a.SalesId,
                             a.SalesDate,
                             a.CustomerId,
                             a.DueDate,
                         }).ToList();

            var salesReturn = (from a in oConnectionContext.DbClsSalesReturn
                               join c in oConnectionContext.DbClsSales
                               on a.SalesId equals c.SalesId
                               where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                               && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                               select new
                               {
                                   a.CompanyId,
                                   a.SalesReturnId,
                                   a.Date,
                                   c.CustomerId,
                                   a.DueDate,
                               }).ToList();

            foreach (var a in purchase)
            {
                if (a.DueDate < CurrentDate(a.CompanyId))
                {
                    string query = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + a.PurchaseId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }

            foreach (var a in purchaseReturn)
            {
                if (a.DueDate < CurrentDate(a.CompanyId))
                {
                    string query = "update \"tblPurchaseReturn\" set \"Status\"='Overdue' where \"PurchaseReturnId\"=" + a.PurchaseReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }

            foreach (var a in sales)
            {
                if (a.DueDate < CurrentDate(a.CompanyId))
                {
                    string query = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + a.SalesId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }

            foreach (var a in salesReturn)
            {
                if (a.DueDate < CurrentDate(a.CompanyId))
                {
                    string query = "update \"tblSalesReturn\" set \"Status\"='Overdue' where \"SalesReturnId\"=" + a.SalesReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }
        }

        [HttpGet]
        public void ExpirePaymentLinks()
        {
            var PaymentLinks = oConnectionContext.DbClsPaymentLink.Where(a => a.IsActive == true && a.IsDeleted == false
            && a.Status == "Generated").Select(a => new
            {
                a.CompanyId,
                a.PaymentLinkId,
                a.LinkExpirationDate,
            }).ToList();

            foreach (var a in PaymentLinks)
            {
                if (a.LinkExpirationDate < CurrentDate(a.CompanyId))
                {
                    string query = "update \"tblPaymentLink\" set \"Status\"='Expired' where \"PaymentLinkId\"=" + a.PaymentLinkId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }
        }

        [HttpGet]
        public void ProcessExpiredRewardPoints()
        {
            var currentDate = DateTime.Now;
            RewardPointsNotificationHelper notificationHelper = new RewardPointsNotificationHelper();
            
            // Get all companies with reward points enabled
            var companiesWithRewardPoints = oConnectionContext.DbClsRewardPointSettings
                .Where(a => !a.IsDeleted && a.EnableRewardPoint && a.ExpiryPeriod > 0)
                .Select(a => new { a.CompanyId, a.ExpiryPeriod, a.ExpiryPeriodType })
                .ToList();

            foreach (var company in companiesWithRewardPoints)
            {
                var companyCurrentDate = CurrentDate(company.CompanyId);
                
                // Get domain for notifications
                string domain = oConnectionContext.DbClsDomain
                    .Where(a => a.CompanyId == company.CompanyId && !a.IsDeleted && a.IsActive)
                    .Select(a => a.Domain)
                    .FirstOrDefault() ?? "";

                // Get all non-expired earned transactions that have passed expiry date
                var expiredTransactions = oConnectionContext.DbClsRewardPointTransaction
                    .Where(a => !a.IsDeleted 
                        && !a.IsExpired 
                        && a.TransactionType == "Earn" 
                        && a.Points > 0
                        && a.ExpiryDate != null
                        && a.ExpiryDate <= companyCurrentDate
                        && a.CompanyId == company.CompanyId)
                    .ToList();

                foreach (var transaction in expiredTransactions)
                {
                    // Mark as expired
                    transaction.IsExpired = true;
                    transaction.ModifiedOn = companyCurrentDate;

                    // Update customer balance (reward points are now stored in tblUser)
                    var customer = oConnectionContext.DbClsUser
                        .Where(a => a.UserId == transaction.CustomerId 
                            && a.CompanyId == transaction.CompanyId 
                            && !a.IsDeleted)
                        .FirstOrDefault();

                    if (customer != null)
                    {
                        customer.ExpiredRewardPoints += transaction.Points;
                        customer.AvailableRewardPoints -= transaction.Points;
                        if (customer.AvailableRewardPoints < 0)
                            customer.AvailableRewardPoints = 0;
                        customer.ModifiedOn = companyCurrentDate;
                    }

                    // Create expiry transaction record
                    var expiryTransaction = new ClsRewardPointTransaction
                    {
                        CustomerId = transaction.CustomerId,
                        SalesId = transaction.SalesId,
                        TransactionType = "Expire",
                        Points = transaction.Points,
                        OrderAmount = 0,
                        TransactionDate = companyCurrentDate,
                        CompanyId = transaction.CompanyId,
                        Notes = $"Points expired from transaction dated {transaction.TransactionDate:dd-MM-yyyy}",
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = company.CompanyId, // System
                        AddedOn = companyCurrentDate
                    };
                    oConnectionContext.DbClsRewardPointTransaction.Add(expiryTransaction);
                }
            }

            oConnectionContext.SaveChanges();
        }

        /// <summary>
        /// Send notifications for points expiring soon (within next 7 days)
        /// This should be called daily via scheduled task
        /// </summary>
        [HttpGet]
        public void NotifyExpiringRewardPoints()
        {
            var currentDate = DateTime.Now;
            RewardPointsNotificationHelper notificationHelper = new RewardPointsNotificationHelper();
            
            // Get all companies with reward points enabled and expiry configured
            var companiesWithRewardPoints = oConnectionContext.DbClsRewardPointSettings
                .Where(a => !a.IsDeleted && a.EnableRewardPoint && a.ExpiryPeriod > 0)
                .Select(a => new { a.CompanyId })
                .ToList();

            foreach (var company in companiesWithRewardPoints)
            {
                var companyCurrentDate = CurrentDate(company.CompanyId);
                DateTime expiryThreshold = companyCurrentDate.AddDays(7); // Notify if expiring within 7 days
                
                // Get domain for notifications
                string domain = oConnectionContext.DbClsDomain
                    .Where(a => a.CompanyId == company.CompanyId && !a.IsDeleted && a.IsActive)
                    .Select(a => a.Domain)
                    .FirstOrDefault() ?? "";

                // Get transactions expiring soon (within 7 days) that haven't been notified
                // Group by customer to send one notification per customer
                var expiringPointsByCustomer = oConnectionContext.DbClsRewardPointTransaction
                    .Where(a => !a.IsDeleted 
                        && !a.IsExpired 
                        && a.TransactionType == "Earn" 
                        && a.Points > 0
                        && a.ExpiryDate != null
                        && a.ExpiryDate > companyCurrentDate
                        && a.ExpiryDate <= expiryThreshold
                        && a.CompanyId == company.CompanyId)
                    .GroupBy(a => a.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        TotalPoints = g.Sum(a => a.Points),
                        ExpiryDate = g.Min(a => a.ExpiryDate)
                    })
                    .ToList();

                foreach (var customerPoints in expiringPointsByCustomer)
                {
                    // Check if we've already notified this customer recently (within last 24 hours)
                    // This prevents spam - you can adjust the logic as needed
                    var lastNotification = oConnectionContext.DbClsRewardPointTransaction
                        .Where(a => a.CustomerId == customerPoints.CustomerId
                            && a.CompanyId == company.CompanyId
                            && a.TransactionType == "Expire"
                            && a.Notes != null
                            && a.Notes.Contains("Expiring soon notification")
                            && a.TransactionDate >= companyCurrentDate.AddDays(-1))
                        .Any();

                    if (!lastNotification && customerPoints.ExpiryDate.HasValue)
                    {
                        notificationHelper.SendPointsExpiringNotification(
                            customerPoints.CustomerId,
                            company.CompanyId,
                            customerPoints.TotalPoints,
                            customerPoints.ExpiryDate.Value,
                            companyCurrentDate,
                            domain
                        );
                    }
                }
            }
        }

        [HttpGet]
        public void CreateRecurringSalesInvoices(long RecurringSalesId)
        {
            List<ClsRecurringSalesVm> RecurringSales;

            if (RecurringSalesId != 0)
            {
                RecurringSales = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == RecurringSalesId).Select(a => new ClsRecurringSalesVm
                {
                    CompanyId = a.CompanyId,
                    RecurringSalesId = a.RecurringSalesId,
                    Status = a.Status,
                    IsNeverExpires = a.IsNeverExpires,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    RepeatEvery = a.RepeatEvery,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    LastInvoiceDate = a.LastInvoiceDate,
                    NextInvoiceDate = a.NextInvoiceDate
                }).ToList();
            }
            else
            {
                // Fetch all active recurring sales with status "Active" (not "Generated")
                RecurringSales = oConnectionContext.DbClsRecurringSales.Where(a => a.IsActive == true && a.IsDeleted == false
                && a.Status == "Active").Select(a => new ClsRecurringSalesVm
                {
                    CompanyId = a.CompanyId,
                    RecurringSalesId = a.RecurringSalesId,
                    Status = a.Status,
                    IsNeverExpires = a.IsNeverExpires,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    RepeatEvery = a.RepeatEvery,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    LastInvoiceDate = a.LastInvoiceDate,
                    NextInvoiceDate = a.NextInvoiceDate
                }).ToList();
            }

            foreach (var recurring in RecurringSales)
            {
                try
                {
                    if (oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a =>
                    a.CompanyId == recurring.CompanyId && a.StartDate != null && a.Status == 2
                    && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault() == 0)
                    {
                        continue;
                    }

                    var currentDate = CurrentDate(recurring.CompanyId);                    

                    // Use the stored NextInvoiceDate from database instead of recalculating
                    // If NextInvoiceDate is not set (MinValue or default), use StartDate as fallback
                    DateTime nextInvoiceDate = (recurring.NextInvoiceDate == DateTime.MinValue || recurring.NextInvoiceDate == default(DateTime))
                        ? recurring.StartDate
                        : recurring.NextInvoiceDate;

                    // Create ALL missed invoices using a while loop
                    while (nextInvoiceDate <= currentDate)
                    {
                        // Check if the recurring sales has expired (if not set to never expire)
                        if (!recurring.IsNeverExpires && nextInvoiceDate > recurring.EndDate)
                        {
                            // Update status to Expired
                            var recSales = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == recurring.RecurringSalesId).FirstOrDefault();
                            if (recSales != null)
                            {
                                recSales.Status = "Expired";
                                recSales.ModifiedOn = currentDate;
                                recSales.NextInvoiceDate = nextInvoiceDate;
                                oConnectionContext.SaveChanges();
                            }
                            break; // Exit the while loop for expired recurring sales
                        }

                        // Fetch complete recurring sales data with all details
                        var recurringSalesData = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == recurring.RecurringSalesId).FirstOrDefault();
                        if (recurringSalesData == null)
                            break;

                        // Fetch recurring sales details
                        var recurringSalesDetails = oConnectionContext.DbClsRecurringSalesDetails
                            .Where(a => a.RecurringSalesId == recurring.RecurringSalesId && a.IsActive == true && a.IsDeleted == false)
                            .Select(b => new ClsSalesDetailsVm
                            {
                                ItemId = b.ItemId,
                                ItemDetailsId = b.ItemDetailsId,
                                Quantity = b.Quantity,
                                TaxId = b.TaxId,
                                DiscountType = b.DiscountType,
                                Discount = b.Discount,
                                UnitCost = b.UnitCost,
                                PriceIncTax = b.PriceIncTax,
                                OtherInfo = b.OtherInfo,
                                CompanyId = b.CompanyId,
                                IsActive = b.IsActive,
                                IsDeleted = b.IsDeleted,
                                AddedBy = b.AddedBy,
                                QuantityRemaining = b.QuantityRemaining,
                                WarrantyId = b.WarrantyId,
                                DefaultUnitCost = b.DefaultUnitCost,
                                DefaultAmount = b.DefaultAmount,
                                PriceAddedFor = b.PriceAddedFor,
                                LotId = b.LotId,
                                LotType = b.LotType,
                                FreeQuantity = b.FreeQuantity,
                                AmountExcTax = b.AmountExcTax,
                                TaxAmount = b.TaxAmount,
                                PriceExcTax = b.PriceExcTax,
                                AmountIncTax = b.AmountIncTax,
                                Under = b.Under,
                                UnitAddedFor = b.UnitAddedFor,
                                LotIdForLotNoChecking = b.LotIdForLotNoChecking,
                                LotTypeForLotNoChecking = b.LotTypeForLotNoChecking,
                                ComboId = b.ComboId,
                                IsComboItems = b.IsComboItems,
                                QuantitySold = b.QuantitySold,
                                ComboPerUnitQuantity = b.ComboPerUnitQuantity,
                                AccountId = b.AccountId,
                                DiscountAccountId = b.DiscountAccountId,
                                TaxAccountId = b.TaxAccountId,
                                PurchaseAccountId = b.PurchaseAccountId,
                                InventoryAccountId = b.InventoryAccountId,
                                WarrantyExpiryDate = b.WarrantyExpiryDate.HasValue ? b.WarrantyExpiryDate.Value : DateTime.MinValue,
                                ExtraDiscount = b.ExtraDiscount,
                                ItemCodeId = b.ItemCodeId,
                                TaxExemptionId = b.TaxExemptionId,
                                TotalTaxAmount = b.TotalTaxAmount,
                                IsCombo = b.IsCombo
                            }).ToList();

                        // Fetch recurring sales additional charges with charge name
                        var recurringSalesAdditionalCharges = (from b in oConnectionContext.DbClsRecurringSalesAdditionalCharges
                                                               join c in oConnectionContext.DbClsAdditionalCharge on b.AdditionalChargeId equals c.AdditionalChargeId
                                                               where b.RecurringSalesId == recurring.RecurringSalesId && b.IsActive == true && b.IsDeleted == false
                                                               select new ClsSalesAdditionalChargesVm
                                                               {
                                                                   AdditionalChargeId = b.AdditionalChargeId,
                                                                   Name = c.Name,
                                                                   TaxId = b.TaxId,
                                                                   CompanyId = b.CompanyId,
                                                                   IsActive = b.IsActive,
                                                                   IsDeleted = b.IsDeleted,
                                                                   AddedBy = b.AddedBy,
                                                                   AmountExcTax = b.AmountExcTax,
                                                                   TaxAmount = b.TaxAmount,
                                                                   AmountIncTax = b.AmountIncTax,
                                                                   AccountId = b.AccountId,
                                                                   ItemCodeId = b.ItemCodeId,
                                                                   TaxExemptionId = b.TaxExemptionId
                                                               }).ToList();

                        // Create ClsSalesVm object from recurring sales data
                        // Use nextInvoiceDate as the sales date (the actual scheduled date, not current date)

                        // Calculate DueDate based on PaymentTermId
                        DateTime dueDate = nextInvoiceDate;
                        if (recurringSalesData.PaymentTermId > 0)
                        {
                            int paymentTermDays = oConnectionContext.DbClsPaymentTerm
                                .Where(a => a.CompanyId == recurringSalesData.CompanyId
                                    && a.IsDeleted == false
                                    && a.IsActive == true
                                    && a.PaymentTermId == recurringSalesData.PaymentTermId)
                                .Select(a => a.Days)
                                .FirstOrDefault();

                            dueDate = nextInvoiceDate.AddDays(paymentTermDays);
                        }                        

                        var salesObj = new ClsSalesVm
                        {
                            RecurringSalesId = recurringSalesData.RecurringSalesId,
                            DueDate = dueDate,
                            CompanyId = recurringSalesData.CompanyId,
                            BranchId = recurringSalesData.BranchId,
                            CustomerId = recurringSalesData.CustomerId,
                            SellingPriceGroupId = recurringSalesData.SellingPriceGroupId,
                            SalesDate = nextInvoiceDate, // Use the scheduled date, not current date
                            Status = "Draft",
                            AttachDocument = recurringSalesData.AttachDocument,
                            Subtotal = recurringSalesData.Subtotal,
                            TaxId = recurringSalesData.TaxId,
                            TaxAmount = recurringSalesData.TaxAmount,
                            TotalQuantity = recurringSalesData.TotalQuantity,
                            Discount = recurringSalesData.Discount,
                            DiscountType = recurringSalesData.DiscountType,
                            TotalDiscount = recurringSalesData.TotalDiscount,
                            GrandTotal = recurringSalesData.GrandTotal,
                            Notes = recurringSalesData.Notes,
                            SalesType = recurringSalesData.RecurringSalesType,
                            TotalPaying = recurringSalesData.TotalPaying,
                            Balance = recurringSalesData.Balance,
                            ChangeReturn = recurringSalesData.ChangeReturn,
                            PaymentType = recurringSalesData.PaymentType,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = recurringSalesData.AddedBy,
                            ExchangeRate = recurringSalesData.ExchangeRate,
                            RoundOff = recurringSalesData.RoundOff,
                            NetAmount = recurringSalesData.NetAmount,
                            AccountId = recurringSalesData.AccountId,
                            DiscountAccountId = recurringSalesData.DiscountAccountId,
                            RoundOffAccountId = recurringSalesData.RoundOffAccountId,
                            TaxAccountId = recurringSalesData.TaxAccountId,
                            TotalTaxAmount = recurringSalesData.TotalTaxAmount,
                            UserGroupId = recurringSalesData.UserGroupId,
                            PaymentTermId = recurringSalesData.PaymentTermId,
                            PlaceOfSupplyId = recurringSalesData.PlaceOfSupplyId,
                            TaxExemptionId = recurringSalesData.TaxExemptionId,
                            IsBusinessRegistered = recurringSalesData.IsBusinessRegistered,
                            GstTreatment = recurringSalesData.GstTreatment,
                            BusinessRegistrationNameId = recurringSalesData.BusinessRegistrationNameId,
                            BusinessRegistrationNo = recurringSalesData.BusinessRegistrationNo,
                            BusinessLegalName = recurringSalesData.BusinessLegalName,
                            BusinessTradeName = recurringSalesData.BusinessTradeName,
                            PanNo = recurringSalesData.PanNo,
                            IsReverseCharge = recurringSalesData.IsReverseCharge,
                            IsCancelled = false,
                            GstPayment = recurringSalesData.GstPayment,
                            PrefixId = recurringSalesData.PrefixId,
                            NetAmountReverseCharge = recurringSalesData.NetAmountReverseCharge,
                            RoundOffReverseCharge = recurringSalesData.RoundOffReverseCharge,
                            GrandTotalReverseCharge = recurringSalesData.GrandTotalReverseCharge,
                            TaxableAmount = recurringSalesData.TaxableAmount,
                            PayTaxForExport = recurringSalesData.PayTaxForExport,
                            TaxCollectedFromCustomer = recurringSalesData.TaxCollectedFromCustomer,
                            SpecialDiscount = recurringSalesData.SpecialDiscount,
                            SpecialDiscountAccountId = recurringSalesData.SpecialDiscountAccountId,
                            Terms = recurringSalesData.Terms,
                            SalesDetails = recurringSalesDetails,
                            SalesAdditionalCharges = recurringSalesAdditionalCharges,
                            ReferenceId = recurring.RecurringSalesId,
                            ReferenceType = "Recurring Sales",
                            IpAddress = "System",
                            Browser = "System",
                            Platform = "System",
                            //Domain = Domain
                        };

                        // Call SalesController.InsertSales to create the actual invoice
                        SalesController oSalesController = new SalesController();
                        var result = oSalesController.InsertSales(salesObj).Result;

                        // Calculate the next invoice date after this one
                        DateTime calculatedNextInvoiceDate;
                        if (recurring.RepeatEvery == "Day")
                        {
                            calculatedNextInvoiceDate = nextInvoiceDate.AddDays(recurring.RepeatEveryNumber);
                        }
                        else if (recurring.RepeatEvery == "Week")
                        {
                            calculatedNextInvoiceDate = nextInvoiceDate.AddDays(7 * recurring.RepeatEveryNumber);
                        }
                        else if (recurring.RepeatEvery == "Month")
                        {
                            calculatedNextInvoiceDate = nextInvoiceDate.AddMonths(recurring.RepeatEveryNumber);
                        }
                        else if (recurring.RepeatEvery == "Year")
                        {
                            calculatedNextInvoiceDate = nextInvoiceDate.AddYears(recurring.RepeatEveryNumber);
                        }
                        else
                        {
                            // Invalid RepeatEvery value, break out of while loop
                            break;
                        }

                        // Update LastInvoiceDate and NextInvoiceDate in database
                        var updateRecSales = oConnectionContext.DbClsRecurringSales.Where(a => a.RecurringSalesId == recurring.RecurringSalesId).FirstOrDefault();
                        if (updateRecSales != null)
                        {
                            updateRecSales.LastInvoiceDate = nextInvoiceDate;
                            updateRecSales.NextInvoiceDate = calculatedNextInvoiceDate;
                            updateRecSales.ModifiedOn = currentDate;

                            // Check if the next scheduled invoice would be after the end date (if not set to never expire)
                            if (!recurring.IsNeverExpires && calculatedNextInvoiceDate > recurring.EndDate)
                            {
                                updateRecSales.Status = "Completed";
                                oConnectionContext.SaveChanges();
                                break; // This was the last invoice
                            }
                            else
                            {
                                updateRecSales.Status = "Active";
                                oConnectionContext.SaveChanges();
                            }
                        }

                        // Move to the next scheduled date for the while loop
                        nextInvoiceDate = calculatedNextInvoiceDate;
                    }
                }
                catch (Exception)
                {
                    // Log the error but continue processing other recurring sales
                    // In a production environment, you might want to log this to a file or database
                    continue;
                }
            }
        }
        
        [HttpGet]
        public void CreateRecurringTableBookings(long RecurringBookingId)
        {
            List<ClsRecurringBookingVm> RecurringBookings;

            if (RecurringBookingId != 0)
            {
                RecurringBookings = oConnectionContext.DbClsRecurringBooking.Where(a => a.RecurringBookingId == RecurringBookingId).Select(a => new ClsRecurringBookingVm
                {
                    CompanyId = a.CompanyId,
                    RecurringBookingId = a.RecurringBookingId,
                    BookingId = a.BookingId,
                    CustomerId = a.CustomerId,
                    BookingTime = a.BookingTime,
                    Duration = a.Duration,
                    NoOfGuests = a.NoOfGuests,
                    BranchId = a.BranchId,
                    SpecialRequest = a.SpecialRequest,
                    RecurrenceType = a.RecurrenceType,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    RepeatEvery = a.RepeatEvery,
                    DayOfMonth = a.DayOfMonth,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsNeverExpires = a.IsNeverExpires,
                    IsActive = a.IsActive,
                    AddedBy = a.AddedBy
                }).ToList();
            }
            else
            {
                // Fetch all active recurring bookings with IsActive = true
                RecurringBookings = oConnectionContext.DbClsRecurringBooking.Where(a => a.IsActive == true).Select(a => new ClsRecurringBookingVm
                {
                    CompanyId = a.CompanyId,
                    RecurringBookingId = a.RecurringBookingId,
                    BookingId = a.BookingId,
                    CustomerId = a.CustomerId,
                    BookingTime = a.BookingTime,
                    Duration = a.Duration,
                    NoOfGuests = a.NoOfGuests,
                    BranchId = a.BranchId,
                    SpecialRequest = a.SpecialRequest,
                    RecurrenceType = a.RecurrenceType,
                    RepeatEveryNumber = a.RepeatEveryNumber,
                    RepeatEvery = a.RepeatEvery,
                    DayOfMonth = a.DayOfMonth,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsNeverExpires = a.IsNeverExpires,
                    IsActive = a.IsActive,
                    AddedBy = a.AddedBy
                }).ToList();
            }

            foreach (var recurring in RecurringBookings)
            {
                try
                {
                    var currentDate = CurrentDate(recurring.CompanyId);

                    // Check if we should generate bookings for this recurring pattern
                    if (!recurring.IsNeverExpires && recurring.EndDate < currentDate.Date)
                    {
                        // Recurring pattern has ended, mark as inactive
                        var recBooking = oConnectionContext.DbClsRecurringBooking.Where(a => a.RecurringBookingId == recurring.RecurringBookingId).FirstOrDefault();
                        if (recBooking != null)
                        {
                            recBooking.IsActive = false;
                            oConnectionContext.SaveChanges();
                        }
                        continue;
                    }

                    // Start from StartDate to catch up on any missed bookings
                    // The duplicate check will prevent creating bookings that already exist
                    DateTime nextBookingDate = recurring.StartDate;

                    // Create bookings up to current date (catch up on missed bookings)
                    // Also generate upcoming bookings up to 30 days ahead
                    var maxDate = currentDate.Date.AddDays(30);
                    while (nextBookingDate <= maxDate && 
                           (!recurring.IsNeverExpires ? nextBookingDate <= recurring.EndDate : true))
                    {
                        // Check if the recurring booking has expired (if not set to never expire)
                        if (!recurring.IsNeverExpires && nextBookingDate > recurring.EndDate)
                        {
                            // Update status to inactive
                            var recBooking = oConnectionContext.DbClsRecurringBooking.Where(a => a.RecurringBookingId == recurring.RecurringBookingId).FirstOrDefault();
                            if (recBooking != null)
                            {
                                recBooking.IsActive = false;
                                oConnectionContext.SaveChanges();
                            }
                            break; // Exit the while loop for expired recurring bookings
                        }

                        // Fetch complete recurring booking data with all details
                        var recurringBookingData = oConnectionContext.DbClsRecurringBooking.Where(a => a.RecurringBookingId == recurring.RecurringBookingId).FirstOrDefault();
                        if (recurringBookingData == null)
                            break;

                        // Get the template booking or use stored details
                        ClsTableBooking templateBooking = null;
                        List<long> storedTableIds = new List<long>();

                        if (recurringBookingData.BookingId > 0)
                        {
                            templateBooking = oConnectionContext.DbClsTableBooking
                                .Where(b => b.BookingId == recurringBookingData.BookingId && b.IsDeleted == false)
                                .FirstOrDefault();

                            if (templateBooking == null)
                            {
                                break; // Template booking not found, skip this recurring booking
                            }
                        }
                        else
                        {
                            // Get table IDs from junction table
                            storedTableIds = oConnectionContext.DbClsRecurringBookingTable
                                .Where(rt => rt.RecurringBookingId == recurringBookingData.RecurringBookingId)
                                .Select(rt => rt.TableId)
                                .ToList();
                        }

                        // Get days of week from junction table for Weekly recurrence type
                        List<int> daysOfWeek = null;
                        if (recurringBookingData.RecurrenceType == "Weekly")
                        {
                            daysOfWeek = oConnectionContext.DbClsRecurringBookingDay
                                .Where(rd => rd.RecurringBookingId == recurringBookingData.RecurringBookingId)
                                .OrderBy(rd => rd.DisplayOrder)
                                .ThenBy(rd => rd.DayOfWeek)
                                .Select(rd => rd.DayOfWeek)
                                .ToList();

                            // If no days in table, use start date's day of week
                            if (!daysOfWeek.Any())
                            {
                                daysOfWeek = new List<int> { (int)recurringBookingData.StartDate.DayOfWeek };
                            }
                        }

                        // Check if this date matches the recurrence pattern
                        bool shouldCreateBooking = false;
                        if (recurringBookingData.RecurrenceType == "Daily")
                        {
                            // For daily, check if the date is within the interval
                            var daysSinceStart = (nextBookingDate - recurringBookingData.StartDate).Days;
                            if (daysSinceStart >= 0 && daysSinceStart % recurringBookingData.RepeatEveryNumber == 0)
                            {
                                shouldCreateBooking = true;
                            }
                        }
                        else if (recurringBookingData.RecurrenceType == "Weekly")
                        {
                            // For weekly, check if it's one of the specified days and within the interval
                            if (daysOfWeek != null && daysOfWeek.Contains((int)nextBookingDate.DayOfWeek))
                            {
                                var weeksSinceStart = (int)((nextBookingDate - recurringBookingData.StartDate).TotalDays / 7);
                                if (weeksSinceStart % recurringBookingData.RepeatEveryNumber == 0)
                                {
                                    shouldCreateBooking = true;
                                }
                            }
                        }
                        else if (recurringBookingData.RecurrenceType == "Monthly")
                        {
                            // For monthly, check if it's the correct day of month and within the interval
                            var targetDayOfMonth = recurringBookingData.DayOfMonth ?? recurringBookingData.StartDate.Day;
                            if (nextBookingDate.Day == targetDayOfMonth || 
                                (targetDayOfMonth > DateTime.DaysInMonth(nextBookingDate.Year, nextBookingDate.Month) && 
                                 nextBookingDate.Day == DateTime.DaysInMonth(nextBookingDate.Year, nextBookingDate.Month)))
                            {
                                var monthsSinceStart = (nextBookingDate.Year - recurringBookingData.StartDate.Year) * 12 + 
                                                       (nextBookingDate.Month - recurringBookingData.StartDate.Month);
                                if (monthsSinceStart >= 0 && monthsSinceStart % recurringBookingData.RepeatEveryNumber == 0)
                                {
                                    shouldCreateBooking = true;
                                }
                            }
                        }
                        else if (recurringBookingData.RecurrenceType == "Yearly")
                        {
                            // For yearly, check if it's the same month and day (or last day of month for leap year edge cases)
                            var targetDayOfMonth = recurringBookingData.DayOfMonth ?? recurringBookingData.StartDate.Day;
                            var targetMonth = recurringBookingData.StartDate.Month;
                            
                            // Check if month and day match (handle cases where day doesn't exist in some years, e.g., Feb 29)
                            bool monthDayMatch = false;
                            if (nextBookingDate.Month == targetMonth)
                            {
                                if (nextBookingDate.Day == targetDayOfMonth)
                                {
                                    monthDayMatch = true;
                                }
                                else if (targetDayOfMonth > DateTime.DaysInMonth(nextBookingDate.Year, nextBookingDate.Month) && 
                                         nextBookingDate.Day == DateTime.DaysInMonth(nextBookingDate.Year, nextBookingDate.Month))
                                {
                                    // Handle case where target day doesn't exist (e.g., Feb 29 in non-leap year)
                                    monthDayMatch = true;
                                }
                            }
                            
                            if (monthDayMatch)
                            {
                                var yearsSinceStart = nextBookingDate.Year - recurringBookingData.StartDate.Year;
                                if (yearsSinceStart >= 0 && yearsSinceStart % recurringBookingData.RepeatEveryNumber == 0)
                                {
                                    shouldCreateBooking = true;
                                }
                            }
                        }

                        if (shouldCreateBooking)
                        {
                            // Get booking time and other details
                            TimeSpan bookingTime = templateBooking != null ? templateBooking.BookingTime : (recurringBookingData.BookingTime ?? TimeSpan.FromHours(19));
                            long customerId = templateBooking != null ? templateBooking.CustomerId : recurringBookingData.CustomerId;
                            int duration = templateBooking != null ? templateBooking.Duration : (recurringBookingData.Duration > 0 ? recurringBookingData.Duration : 120);
                            int noOfGuests = templateBooking != null ? templateBooking.NoOfGuests : recurringBookingData.NoOfGuests;
                            long branchId = templateBooking != null ? templateBooking.BranchId : recurringBookingData.BranchId;
                            string specialRequest = templateBooking != null ? templateBooking.SpecialRequest : recurringBookingData.SpecialRequest;

                            // Calculate deposit amount dynamically based on restaurant settings
                            decimal depositAmount = 0;
                            var restaurantSettings = oConnectionContext.DbClsRestaurantSettings
                                .Where(rs => rs.CompanyId == recurringBookingData.CompanyId && rs.BranchId == branchId && rs.IsDeleted == false)
                                .FirstOrDefault();

                            if (restaurantSettings != null && restaurantSettings.RequireDeposit)
                            {
                                var depositMode = restaurantSettings.DepositMode ?? "Fixed";
                                if (depositMode.Equals("PerGuest", StringComparison.OrdinalIgnoreCase))
                                {
                                    depositAmount = noOfGuests * restaurantSettings.DepositPerGuestAmount;
                                }
                                else
                                {
                                    depositAmount = restaurantSettings.DepositFixedAmount;
                                }
                            }

                            long addedBy = templateBooking != null ? templateBooking.AddedBy : recurringBookingData.AddedBy;

                            // Get table IDs
                            List<long> tableIds = new List<long>();
                            if (templateBooking != null)
                            {
                                var templateTables = oConnectionContext.DbClsTableBookingTable
                                    .Where(bt => bt.BookingId == templateBooking.BookingId)
                                    .Select(bt => bt.TableId)
                                    .ToList();
                                tableIds = templateTables;
                            }
                            else
                            {
                                tableIds = storedTableIds;
                            }

                            if (customerId > 0 && noOfGuests > 0 && tableIds.Count > 0)
                            {
                                // Check if booking already exists for this date/time from this recurring booking
                                var dateStart = nextBookingDate.Date;
                                var dateEnd = dateStart.AddDays(1);
                                var existingBooking = oConnectionContext.DbClsTableBooking
                                    .Where(b => b.RecurringBookingId == recurringBookingData.RecurringBookingId &&
                                           b.BookingDate >= dateStart &&
                                           b.BookingDate < dateEnd &&
                                           b.BookingTime == bookingTime &&
                                           b.IsDeleted == false)
                                    .FirstOrDefault();

                                if (existingBooking == null)
                                {
                                    // Create new booking
                                    var newBooking = new ClsTableBooking
                                    {
                                        BookingNo = GenerateBookingNo(recurringBookingData.CompanyId, branchId),
                                        CustomerId = customerId,
                                        BookingDate = nextBookingDate.Date,
                                        BookingTime = bookingTime,
                                        Duration = duration,
                                        NoOfGuests = noOfGuests,
                                        Status = "Pending",
                                        BookingType = "Recurring",
                                        SpecialRequest = specialRequest,
                                        DepositAmount = depositAmount,
                                        ReminderSent = false,
                                        BranchId = branchId,
                                        CompanyId = recurringBookingData.CompanyId,
                                        RecurringBookingId = recurringBookingData.RecurringBookingId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        AddedBy = addedBy,
                                        AddedOn = currentDate,
                                        ModifiedBy = addedBy
                                    };

                                    oConnectionContext.DbClsTableBooking.Add(newBooking);
                                    oConnectionContext.SaveChanges();

                                    // Add table assignments
                                    int displayOrder = 0;
                                    foreach (var tableId in tableIds)
                                    {
                                        var newBookingTable = new ClsTableBookingTable
                                        {
                                            BookingId = newBooking.BookingId,
                                            TableId = tableId,
                                            IsPrimary = displayOrder == 0,
                                            DisplayOrder = displayOrder++,
                                            AddedOn = currentDate
                                        };
                                        oConnectionContext.DbClsTableBookingTable.Add(newBookingTable);
                                    }

                                    oConnectionContext.SaveChanges();
                                }
                            }
                        }

                        // Calculate the next booking date after this one
                        DateTime calculatedNextBookingDate;
                        if (recurringBookingData.RecurrenceType == "Daily")
                        {
                            calculatedNextBookingDate = nextBookingDate.AddDays(recurringBookingData.RepeatEveryNumber);
                        }
                        else if (recurringBookingData.RecurrenceType == "Weekly")
                        {
                            // For weekly, find the next matching day of week after the interval weeks
                            // First, move forward by the interval weeks
                            var weeksToAdd = recurringBookingData.RepeatEveryNumber;
                            calculatedNextBookingDate = nextBookingDate.AddDays(7 * weeksToAdd);
                            
                            // If we have specific days of week, find the next matching day
                            if (daysOfWeek != null && daysOfWeek.Any())
                            {
                                // Find the next day of week that matches, starting from the calculated date
                                int attempts = 0;
                                while (!daysOfWeek.Contains((int)calculatedNextBookingDate.DayOfWeek) && attempts < 7)
                                {
                                    calculatedNextBookingDate = calculatedNextBookingDate.AddDays(1);
                                    attempts++;
                                }
                                
                                // If we couldn't find a match in the same week, it means we need to go to the next interval
                                if (attempts >= 7)
                                {
                                    // Move to the next week and find the first matching day
                                    calculatedNextBookingDate = nextBookingDate.AddDays(7 * (weeksToAdd + 1));
                                    while (!daysOfWeek.Contains((int)calculatedNextBookingDate.DayOfWeek))
                                    {
                                        calculatedNextBookingDate = calculatedNextBookingDate.AddDays(1);
                                    }
                                }
                            }
                        }
                        else if (recurringBookingData.RecurrenceType == "Monthly")
                        {
                            // For monthly, add the interval months and adjust to the target day of month
                            var targetDayOfMonth = recurringBookingData.DayOfMonth ?? recurringBookingData.StartDate.Day;
                            calculatedNextBookingDate = nextBookingDate.AddMonths(recurringBookingData.RepeatEveryNumber);
                            
                            // Adjust to the target day of month (handle months with fewer days)
                            var maxDayInMonth = DateTime.DaysInMonth(calculatedNextBookingDate.Year, calculatedNextBookingDate.Month);
                            var dayToUse = Math.Min(targetDayOfMonth, maxDayInMonth);
                            calculatedNextBookingDate = new DateTime(calculatedNextBookingDate.Year, calculatedNextBookingDate.Month, dayToUse);
                        }
                        else if (recurringBookingData.RecurrenceType == "Yearly")
                        {
                            // For yearly, add the interval years and adjust to the target month/day
                            var targetDayOfMonth = recurringBookingData.DayOfMonth ?? recurringBookingData.StartDate.Day;
                            var targetMonth = recurringBookingData.StartDate.Month;
                            calculatedNextBookingDate = nextBookingDate.AddYears(recurringBookingData.RepeatEveryNumber);
                            
                            // Adjust to the target month and day (handle leap year edge cases)
                            var maxDayInMonth = DateTime.DaysInMonth(calculatedNextBookingDate.Year, targetMonth);
                            var dayToUse = Math.Min(targetDayOfMonth, maxDayInMonth);
                            calculatedNextBookingDate = new DateTime(calculatedNextBookingDate.Year, targetMonth, dayToUse);
                        }
                        else
                        {
                            // Invalid RecurrenceType value, break out of while loop
                            break;
                        }

                        // Check if the next scheduled booking would be after the end date (if not set to never expire)
                        if (!recurringBookingData.IsNeverExpires && calculatedNextBookingDate > recurringBookingData.EndDate)
                        {
                            // Mark as inactive since this was the last booking
                            var updateRecBooking = oConnectionContext.DbClsRecurringBooking.Where(a => a.RecurringBookingId == recurring.RecurringBookingId).FirstOrDefault();
                            if (updateRecBooking != null)
                            {
                                updateRecBooking.IsActive = false;
                                oConnectionContext.SaveChanges();
                            }
                            break; // This was the last booking
                        }

                        // Move to the next scheduled date for the while loop
                        nextBookingDate = calculatedNextBookingDate;
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing other recurring bookings
                    // In a production environment, you might want to log this to a file or database
                    continue;
                }
            }
        }

        private string GenerateBookingNo(long companyId, long branchId)
        {
            long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == branchId).Select(a => a.PrefixId).FirstOrDefault();
            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                  join b in oConnectionContext.DbClsPrefixUserMap
                                   on a.PrefixMasterId equals b.PrefixMasterId
                                  where a.IsActive == true && a.IsDeleted == false &&
                                  b.CompanyId == companyId && b.IsActive == true
                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "booking"
                                  && b.PrefixId == PrefixId
                                  select new
                                  {
                                      b.PrefixUserMapId,
                                      b.Prefix,
                                      b.NoOfDigits,
                                      b.Counter
                                  }).FirstOrDefault();

            string bookingNo = "";
            long PrefixUserMapId = 0;
            if (prefixSettings != null)
            {
                PrefixUserMapId = prefixSettings.PrefixUserMapId;
                bookingNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                // Update counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
            }
            else
            {
                bookingNo = "BK" + DateTime.Now.ToString("yyyyMMdd") + oConnectionContext.DbClsTableBooking.Where(a => a.CompanyId == companyId).Count().ToString().PadLeft(4, '0');
            }

            return bookingNo;
        }

        public string GetIPAddress(string domain)
        {
            try
            {
                IPAddress[] ip_Addresses = Dns.GetHostAddresses(domain);
                string ips = string.Empty;
                foreach (IPAddress ipAddress in ip_Addresses)
                {
                    ips += ipAddress.ToString();
                }
                return ips;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        //[HttpGet]
        //public string DownloadFiles()
        //{
        //    try
        //    {
        //        NetworkCredential credentials = new NetworkCredential("test", "c7oMy2?2");
        //        string url = "ftp://test.equitechsoftwares.com/old/bbqueen16/assets/images/brands/";
        //        DownloadFtpDirectory(url, credentials, @"C:\Inetpub\vhosts\equitechsoftwares.com\test.equitechsoftwares.com\old\bbqueen16\assets\images\brands\");
        //        return "success";
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }

        //}

        //    void DownloadFtpDirectory(
        //string url, NetworkCredential credentials, string localPath)
        //    {
        //        FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
        //        listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        //        listRequest.Credentials = credentials;

        //        List<string> lines = new List<string>();

        //        using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
        //        using (Stream listStream = listResponse.GetResponseStream())
        //        using (var listReader = new StreamReader(listStream))
        //        {
        //            while (!listReader.EndOfStream)
        //            {
        //                lines.Add(listReader.ReadLine());
        //            }
        //        }

        //        foreach (string line in lines)
        //        {
        //            string[] tokens =
        //                line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
        //            string name = tokens[3];
        //            string permissions = tokens[0];

        //            string localFilePath = Path.Combine(localPath, name);
        //            string fileUrl = url + name;

        //            if (permissions[0] == 'd')
        //            {
        //                if (!Directory.Exists(localFilePath))
        //                {
        //                    Directory.CreateDirectory(localFilePath);
        //                }

        //                DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath);
        //            }
        //            else
        //            {
        //                FtpWebRequest downloadRequest =
        //                    (FtpWebRequest)WebRequest.Create(fileUrl);
        //                downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
        //                downloadRequest.Credentials = credentials;

        //                using (FtpWebResponse downloadResponse =
        //                          (FtpWebResponse)downloadRequest.GetResponse())
        //                using (Stream sourceStream = downloadResponse.GetResponseStream())
        //                using (Stream targetStream = File.Create(localFilePath))
        //                {
        //                    byte[] buffer = new byte[10240];
        //                    int read;
        //                    while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
        //                    {
        //                        targetStream.Write(buffer, 0, read);
        //                    }
        //                }
        //            }
        //        }
        //    }

        public void GenerateThumbnails(int ImageSize, Stream sourcePath, string targetPath)
        {
            //var result = 4 * Math.Ceiling(((double)n / 3)));
            double scaleFactor = 0;

            if (ImageSize > 0 && ImageSize <= 20000) // 0-20 kb
            {
                scaleFactor = 1;
            }
            else if (ImageSize > 20000 && ImageSize <= 50000)  // 20-50 kb
            {
                scaleFactor = 0.8;
            }
            else if (ImageSize > 50000 && ImageSize <= 100000)  // 50-100 kb
            {
                scaleFactor = 0.5;
            }
            else if (ImageSize > 100000 && ImageSize <= 200000)  // 100-200kb
            {
                scaleFactor = 0.4;
            }
            //else if (ImageSize > 100000 && ImageSize <= 500000)  // 100-500kb
            //{
            //    scaleFactor = 0.3;
            //}
            else if (ImageSize > 200000 && ImageSize <= 1000000) //100-1mb
            {
                scaleFactor = 0.3;
            }
            else
            {
                scaleFactor = 0.2;
            }

            //width = 800; height = 500; scaleFactor = 1;
            using (var image = Image.FromStream(sourcePath))
            {
                if (targetPath.Split('.')[1] == "png")
                {
                    scaleFactor = scaleFactor - 0.1;
                }
                if ((image.Width >= 3000 || image.Height >= 3000) && ImageSize <= 1000000)
                {
                    scaleFactor = scaleFactor - 0.1;
                }
                //if ((image.Width >= 4000 || image.Height >= 4000) && ImageSize <= 1000000)
                //{
                //    scaleFactor = scaleFactor - 0.1;
                //}
                //var newWidth = (int)(width * scaleFactor);
                //var newHeight = (int)(height * scaleFactor);
                var newWidth = (int)(image.Width * scaleFactor);
                var newHeight = (int)(image.Height * scaleFactor);
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
                thumbnailImg.Save(targetPath, image.RawFormat);
            }
        }

        [HttpPost]
        public IHttpActionResult FetchAppVersion()
        {
            var data = new
            {
                AndroidAppVersion = "1.0.1"
            };
            return Json(data);
        }

        //public List<ClsBankPaymentVm> BankTransactions(ClsBankPaymentVm obj)
        //{
        //    List<ClsBankPaymentVm> Contras;
        //    List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

        //    if (obj.BranchId == 0)
        //    {
        //        #region Contras
        //        Contras = (from a in oConnectionContext.DbClsContra
        //                   where
        //                   //a.FromAccountId == obj.AccountId && 
        //                   a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
        //                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //     && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
        //         DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
        //         && a.Amount != 0
        //                   select new ClsBankPaymentVm
        //                   {
        //                       Id = a.ContraId,
        //                       AccountId = a.FromAccountId,
        //                       Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
        //                       AddedOn = a.PaymentDate,
        //                       Type = a.Type,
        //                       ReferenceNo = a.ReferenceNo,
        //                       Debit = a.Amount,
        //                       Credit = 0
        //                   }).Concat(from a in oConnectionContext.DbClsContra
        //                             where
        //                             //a.ToAccountId == obj.AccountId && 
        //                             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
        //                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //               && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
        //                   DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
        //                   && a.Amount != 0
        //                             select new ClsBankPaymentVm
        //                             {
        //                                 Id = a.ContraId,
        //                                 AccountId = a.ToAccountId,
        //                                 Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
        //                                 AddedOn = a.PaymentDate,
        //                                 Type = a.Type,
        //                                 ReferenceNo = a.ReferenceNo,
        //                                 Debit = 0,
        //                                 Credit = a.Amount
        //                             }).ToList();
        //        #endregion

        //        Ledger = Contras.ToList();
        //    }
        //    else
        //    {
        //        #region Contras
        //        Contras = (from a in oConnectionContext.DbClsContra
        //                   where
        //                   //a.FromAccountId == obj.AccountId && 
        //                   a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
        //                  && a.BranchId == obj.BranchId
        //     && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
        //         DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
        //         && a.Amount != 0
        //                   select new ClsBankPaymentVm
        //                   {
        //                       Id = a.ContraId,
        //                       AccountId = a.FromAccountId,
        //                       Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
        //                       AddedOn = a.PaymentDate,
        //                       Type = a.Type,
        //                       ReferenceNo = a.ReferenceNo,
        //                       Debit = a.Amount,
        //                       Credit = 0
        //                   }).Concat(from a in oConnectionContext.DbClsContra
        //                             where
        //                             //a.ToAccountId == obj.AccountId && 
        //                             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
        //                             && a.BranchId == obj.BranchId
        //               && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
        //                   DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
        //                   && a.Amount != 0
        //                             select new ClsBankPaymentVm
        //                             {
        //                                 Id = a.ContraId,
        //                                 AccountId = a.ToAccountId,
        //                                 Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
        //                                 AddedOn = a.PaymentDate,
        //                                 Type = a.Type,
        //                                 ReferenceNo = a.ReferenceNo,
        //                                 Debit = 0,
        //                                 Credit = a.Amount
        //                             }).ToList();
        //        #endregion

        //        Ledger = Contras.ToList();
        //    }
        //    return Ledger;
        //}

        public List<ClsBankPaymentVm> AccountTransactions(ClsBankPaymentVm obj)
        {
            List<ClsBankPaymentVm> Expenses;
            //List<ClsBankPaymentVm> Incomes;
            List<ClsBankPaymentVm> Journals;
            List<ClsBankPaymentVm> Contras;
            List<ClsBankPaymentVm> Sales;
            List<ClsBankPaymentVm> SalesReturn;
            List<ClsBankPaymentVm> Purchase;
            List<ClsBankPaymentVm> PurchaseReturn;
            List<ClsBankPaymentVm> SalesPayments;
            List<ClsBankPaymentVm> SalesReturnPayments;
            List<ClsBankPaymentVm> PurchasePayments;
            List<ClsBankPaymentVm> PurchaseReturnPayments;
            List<ClsBankPaymentVm> StockAdjustment;
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            //var Type = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.AccountId == obj.AccountId).Select(a => a.Type).FirstOrDefault();
            if (obj.BranchId == 0)
            {
                #region Expenses

                var ExpenseTaxList = (from q in oConnectionContext.DbClsExpenseTaxJournal
                                      join a in oConnectionContext.DbClsExpensePayment
                                      on q.ExpensePaymentId equals a.ExpensePaymentId
                                      join b in oConnectionContext.DbClsExpense
                                   on a.ExpenseId equals b.ExpenseId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && b.IsDeleted == false && b.IsActive == true
                                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                               && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                   DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      //&& a.TaxAmount != 0
                                      select new
                                      {
                                          Id = b.ExpenseId,
                                          AccountId = q.AccountId,
                                          Notes = "",
                                          AddedOn = b.Date,
                                          Type = "Expense",
                                          ReferenceNo = b.ReferenceNo,
                                          Debit = (q.ExpenseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                          Credit = (q.ExpenseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                      }).ToList();

                var expenseAccount = (from a in oConnectionContext.DbClsExpensePayment
                                      join b in oConnectionContext.DbClsExpense
                                   on a.ExpenseId equals b.ExpenseId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && b.IsDeleted == false && b.IsActive == true
                                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      //&& a.TaxAmount != 0
                                      select new ClsBankPaymentVm
                                      {
                                          Id = b.ExpenseId,
                                          AccountId = a.AccountId,
                                          Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                          AddedOn = b.Date,
                                          Type = "Expense",
                                          ReferenceNo = b.ReferenceNo,
                                          Debit = b.IsReverseCharge == 1 ? a.Amount : a.AmountExcTax,
                                          Credit = 0
                                      }).ToList();

                Expenses =
                            //      (from a in oConnectionContext.DbClsExpensePayment
                            //              join b in oConnectionContext.DbClsExpense
                            //           on a.ExpenseId equals b.ExpenseId
                            //              where
                            //              //a.AccountId == obj.AccountId && 
                            //              b.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                            //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                            //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                            //&& DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                            //    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                            //    && a.AmountExcTax != 0
                            //              select new ClsBankPaymentVm
                            //              {
                            //                  Id = b.ExpenseId,
                            //                  AccountId = a.AccountId,
                            //                  Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                            //                  AddedOn = b.Date,
                            //                  Type = "Expense",
                            //                  ReferenceNo = b.ReferenceNo,
                            //                  Debit = a.AmountExcTax,
                            //                  Credit = 0
                            //              }).Concat
                            (from a in oConnectionContext.DbClsExpense
                                 //   join b in oConnectionContext.DbClsExpensePayment
                                 //on a.ExpenseId equals b.ExpenseId
                             where
                             //a.AccountId == obj.AccountId && 
                             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                             //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
               && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                   DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                   && a.GrandTotal != 0
                             select new ClsBankPaymentVm
                             {
                                 Id = a.ExpenseId,
                                 AccountId = a.AccountId,
                                 Notes = "",
                                 //Notes = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == a.ExpenseId &&
                                 //c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                                 AddedOn = a.Date,
                                 Type = "Expense",
                                 ReferenceNo = a.ReferenceNo,
                                 Debit = 0,
                                 Credit = a.GrandTotal
                                 //Credit = oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == a.ExpenseId &&
                                 //c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => c.Amount).DefaultIfEmpty().FirstOrDefault(),
                             })//.Where(a => a.Credit != 0)
                            .ToList().Select(a => new ClsBankPaymentVm
                            {
                                Id = a.Id,
                                AccountId = a.AccountId,
                                Notes = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == a.Id &&
                           c.IsDeleted == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                                AddedOn = a.AddedOn,
                                Type = a.Type,
                                ReferenceNo = a.ReferenceNo,
                                Debit = a.Debit,
                                Credit = a.Credit
                            })
                      //      .Concat(from a in oConnectionContext.DbClsExpensePayment
                      //              join b in oConnectionContext.DbClsExpense
                      //           on a.ExpenseId equals b.ExpenseId
                      //              where
                      //              //a.TaxAccountId == obj.AccountId && 
                      //              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                      //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                      //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                      //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                      //&& DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      //    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                      //    && a.TaxAmount != 0
                      //              select new ClsBankPaymentVm
                      //              {
                      //                  // tax 
                      //                  Id = b.ExpenseId,
                      //                  AccountId = a.TaxAccountId,
                      //                  Notes = "",
                      //                  AddedOn = b.Date,
                      //                  Type = "Expense",
                      //                  ReferenceNo = b.ReferenceNo,
                      //                  Debit = a.TaxAmount,
                      //                  Credit = 0,
                      //              })
                      .ToList();

                //Expenses = Expenses.Concat(from a in ExpenseTaxList
                //                           select new ClsBankPaymentVm
                //                           {
                //                               // tax 
                //                               Id = a.Id,
                //                               AccountId = a.AccountId,
                //                               Notes = "",
                //                               AddedOn = a.AddedOn,
                //                               Type = "Expense",
                //                               ReferenceNo = a.ReferenceNo,
                //                               Debit = a.Debit,
                //                               Credit = 0,
                //                           }).ToList();

                Expenses = Expenses.Concat(from a in expenseAccount
                                           group a by new { a.AccountId, a.Id } into stdGroup
                                           select new ClsBankPaymentVm
                                           {
                                               Id = stdGroup.Key.Id,
                                               AccountId = stdGroup.Key.AccountId,
                                               Notes = stdGroup.Select(x => x.Notes).DefaultIfEmpty().FirstOrDefault(),
                                               AddedOn = stdGroup.Select(x => x.AddedOn).DefaultIfEmpty().FirstOrDefault(),
                                               Type = "Expense",
                                               ReferenceNo = stdGroup.Select(x => x.ReferenceNo).DefaultIfEmpty().FirstOrDefault(),
                                               Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                               Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                           }).Concat(from a in ExpenseTaxList
                                                     select new ClsBankPaymentVm
                                                     {
                                                         // tax 
                                                         Id = a.Id,
                                                         AccountId = a.AccountId,
                                                         Notes = a.Notes,
                                                         AddedOn = a.AddedOn,
                                                         Type = "Expense",
                                                         ReferenceNo = a.ReferenceNo,
                                                         Debit = a.Debit,
                                                         Credit = a.Credit,
                                                     }).ToList();

                #endregion

                #region Incomes
                //  Incomes = (from a in oConnectionContext.DbClsIncomePayment
                //             join b in oConnectionContext.DbClsIncome
                //          on a.IncomeId equals b.IncomeId
                //             where
                //             //a.AccountId == obj.AccountId && 
                //             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //&& DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                //    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                //             select new ClsBankPaymentVm
                //             {
                //                 AccountId = a.AccountId,
                //                 Id = 0,
                //                 Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                //                 AddedOn = b.Date,
                //                 Type = "Income",
                //                 ReferenceNo = b.ReferenceNo,
                //                 Debit = 0,
                //                 Credit = a.Amount,
                //             }).Concat(from a in oConnectionContext.DbClsIncome
                //                       join b in oConnectionContext.DbClsIncomePayment
                //                    on a.IncomeId equals b.IncomeId
                //                       where
                //                       //a.AccountId == obj.AccountId && 
                //                       a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                //                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                //                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //             l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
                //         && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                //             DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                //                       select new ClsBankPaymentVm
                //                       {
                //                           AccountId = a.AccountId,
                //                           Id = a.IncomeId,
                //                           Notes = "",
                //                           //Notes = string.Join(",", oConnectionContext.DbClsIncomePayment.Where(c => c.IncomeId == a.IncomeId &&
                //                           //c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                //                           AddedOn = a.Date,
                //                           Type = "Income",
                //                           ReferenceNo = a.ReferenceNo,
                //                           Debit = oConnectionContext.DbClsIncomePayment.Where(c => c.IncomeId == a.IncomeId &&
                //                           c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => c.Amount).DefaultIfEmpty().FirstOrDefault(),
                //                           Credit = 0
                //                       }).AsEnumerable().Select(a => new ClsBankPaymentVm
                //                       {
                //                           AccountId = a.AccountId,
                //                           Id = 0,
                //                           Notes = string.Join(",", oConnectionContext.DbClsIncomePayment.Where(c => c.IncomeId == a.Id &&
                //                           c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                //                           AddedOn = a.AddedOn,
                //                           Type = a.Type,
                //                           ReferenceNo = a.ReferenceNo,
                //                           Debit = a.Debit,
                //                           Credit = a.Credit
                //                       }).Concat(from a in oConnectionContext.DbClsIncomePayment
                //                                 join b in oConnectionContext.DbClsIncome
                //                              on a.IncomeId equals b.IncomeId
                //                                 where
                //                                 //a.TaxAccountId == obj.AccountId && 
                //                                 a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                //                                 && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                //                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //                   && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                //                       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                //                                 select new ClsBankPaymentVm
                //                                 {
                //                                     // tax 
                //                                     AccountId = a.TaxAccountId,
                //                                     Notes = "",
                //                                     AddedOn = b.Date,
                //                                     Type = "Income",
                //                                     ReferenceNo = b.ReferenceNo,
                //                                     Debit = a.TaxAmount,
                //                                     Credit = 0,
                //                                 }).ToList();
                #endregion

                #region Contras
                Contras = (from a in oConnectionContext.DbClsContra
                           where
                           //a.FromAccountId == obj.AccountId && 
                           a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
             && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                 && a.Amount != 0
                           select new ClsBankPaymentVm
                           {
                               Id = a.ContraId,
                               AccountId = a.FromAccountId,
                               Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                               AddedOn = a.PaymentDate,
                               Type = a.Type,
                               ReferenceNo = a.ReferenceNo,
                               Debit = 0,
                               Credit = a.Amount
                           }).Concat(from a in oConnectionContext.DbClsContra
                                     where
                                     //a.ToAccountId == obj.AccountId && 
                                     a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                           && a.Amount != 0
                                     select new ClsBankPaymentVm
                                     {
                                         Id = a.ContraId,
                                         AccountId = a.ToAccountId,
                                         Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         AddedOn = a.PaymentDate,
                                         Type = a.Type,
                                         ReferenceNo = a.ReferenceNo,
                                         Debit = a.Amount,
                                         Credit = 0
                                     }).ToList();
                #endregion

                #region Journals
                Journals = (from a in oConnectionContext.DbClsJournalPayment
                            join b in oConnectionContext.DbClsJournal
                         on a.JournalId equals b.JournalId
                            where
                            //a.AccountId == obj.AccountId && 
                            a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                            && b.IsDeleted == false && b.IsActive == true
                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                            select new ClsBankPaymentVm
                            {
                                Id = a.JournalId,
                                AccountId = a.AccountId,
                                Notes = b.Notes,
                                AddedOn = b.Date,
                                Type = "Journal",
                                ReferenceNo = b.ReferenceNo,
                                Debit = a.Debit,
                                Credit = a.Credit
                            }).ToList();
                #endregion

                #region Sales

                var SalesTaxList = (from q in oConnectionContext.DbClsSalesTaxJournal
                                    join a in oConnectionContext.DbClsSalesDetails
                                    on q.SalesDetailsId equals a.SalesDetailsId
                                    join b in oConnectionContext.DbClsSales
                                 on a.SalesId equals b.SalesId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                    && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                        //&& a.TaxAmount != 0 
                                        && b.Status != "Draft"
                                    select new
                                    {
                                        Id = b.SalesId,
                                        CustomerId = b.CustomerId,
                                        AccountId = q.AccountId,
                                        Notes = "",
                                        AddedOn = b.SalesDate,
                                        Type = "Sales",
                                        ReferenceNo = b.InvoiceNo,
                                        Debit = 0,
                                        Credit = a.TaxAmount,
                                    }).Concat(from q in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                              join b in oConnectionContext.DbClsSales
                                           on q.SalesId equals b.SalesId
                                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                              && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                                  DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                                  //&& a.TaxAmount != 0 
                                                  && b.Status != "Draft"
                                              select new
                                              {
                                                  Id = b.SalesId,
                                                  CustomerId = b.CustomerId,
                                                  AccountId = q.AccountId,
                                                  Notes = "",
                                                  AddedOn = b.SalesDate,
                                                  Type = "Sales",
                                                  ReferenceNo = b.InvoiceNo,
                                                  Debit = 0,
                                                  Credit = b.TaxAmount,
                                              }).ToList();

                Sales = (from a in oConnectionContext.DbClsSales
                         where
                         //a.AccountId == obj.AccountId && 
                         a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                         //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
           && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
               && a.GrandTotal != 0 && a.Status != "Draft"
                         select new ClsBankPaymentVm
                         {
                             //Account Receivable
                             Id = a.SalesId,
                             CustomerId = a.CustomerId,
                             AccountId = a.AccountId,
                             Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                             AddedOn = a.SalesDate,
                             Type = "Sales",
                             ReferenceNo = a.InvoiceNo,
                             Debit = a.GrandTotal,
                             Credit = 0
                         }).Concat(from a in oConnectionContext.DbClsSalesDetails
                                   join b in oConnectionContext.DbClsSales
                                on a.SalesId equals b.SalesId
                                   where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                  && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                      && a.UnitCost != 0 && b.Status != "Draft"
                                   group a by new { a.AccountId, a.SalesId } into stdGroup
                                   select new ClsBankPaymentVm
                                   {
                                       // sales account
                                       Id = stdGroup.Key.SalesId,
                                       CustomerId = oConnectionContext.DbClsSales.Where(x => x.SalesId == stdGroup.Key.SalesId).Select(x => x.CustomerId).FirstOrDefault(),
                                       AccountId = stdGroup.Key.AccountId,
                                       Notes = "",
                                       AddedOn = oConnectionContext.DbClsSales.Where(x => x.SalesId == stdGroup.Key.SalesId).Select(x => x.SalesDate).FirstOrDefault(),
                                       Type = "Sales",
                                       ReferenceNo = oConnectionContext.DbClsSales.Where(x => x.SalesId == stdGroup.Key.SalesId).Select(x => x.InvoiceNo).FirstOrDefault(),
                                       Debit = 0,
                                       Credit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum()
                                   })
                         .Concat(from b in oConnectionContext.DbClsSalesAdditionalCharges
                                 join a in oConnectionContext.DbClsSales
                                 on b.SalesId equals a.SalesId
                                 where
                                 a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                 && b.IsDeleted == false && b.IsActive == true
                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                   && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                       && b.AmountExcTax != 0 && a.Status != "Draft"
                                 select new ClsBankPaymentVm
                                 {
                                     // Additional charge
                                     Id = a.SalesId,
                                     CustomerId = a.CustomerId,
                                     AccountId = b.AccountId,
                                     Notes = "",
                                     AddedOn = a.SalesDate,
                                     Type = "Sales",
                                     ReferenceNo = a.InvoiceNo,
                                     Debit = 0,
                                     Credit = b.AmountExcTax,
                                 }).Concat(from a in oConnectionContext.DbClsSales
                                           where
                                           //a.RoundOffAccountId == obj.AccountId && 
                                           a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                             && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                                 && a.RoundOff != 0 && a.Status != "Draft"
                                           select new ClsBankPaymentVm
                                           {
                                               // Round off charge
                                               Id = a.SalesId,
                                               CustomerId = a.CustomerId,
                                               AccountId = a.RoundOffAccountId,
                                               Notes = "",
                                               AddedOn = a.SalesDate,
                                               Type = "Sales",
                                               ReferenceNo = a.InvoiceNo,
                                               Debit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                               Credit = a.RoundOff > 0 ? a.RoundOff : 0,
                                           }).Concat(from a in oConnectionContext.DbClsSales
                                                     where
                                                     //a.DiscountAccountId == obj.AccountId && 
                                                     a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                       && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                                           && a.Discount != 0 && a.Status != "Draft"
                                                     select new ClsBankPaymentVm
                                                     {
                                                         // discount 
                                                         Id = a.SalesId,
                                                         CustomerId = a.CustomerId,
                                                         AccountId = a.DiscountAccountId,
                                                         Notes = "",
                                                         AddedOn = a.SalesDate,
                                                         Type = "Sales",
                                                         ReferenceNo = a.InvoiceNo,
                                                         Debit = a.TotalDiscount,
                                                         Credit = 0,
                                                     }).Concat(from a in oConnectionContext.DbClsSales
                                                               where
                                                               //a.DiscountAccountId == obj.AccountId && 
                                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                 && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                                                     DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                                                     && a.Discount != 0 && a.Status != "Draft"
                                                               select new ClsBankPaymentVm
                                                               {
                                                                   // special discount 
                                                                   Id = a.SalesId,
                                                                   CustomerId = a.CustomerId,
                                                                   AccountId = a.SpecialDiscountAccountId,
                                                                   Notes = "",
                                                                   AddedOn = a.SalesDate,
                                                                   Type = "Sales",
                                                                   ReferenceNo = a.InvoiceNo,
                                                                   Debit = a.SpecialDiscount,
                                                                   Credit = 0,
                                                               }).ToList();

                Sales = Sales.Concat(from a in SalesTaxList
                                     select new ClsBankPaymentVm
                                     {
                                         // tax 
                                         Id = a.Id,
                                         CustomerId = a.CustomerId,
                                         AccountId = a.AccountId,
                                         Notes = "",
                                         AddedOn = a.AddedOn,
                                         Type = "Sales",
                                         ReferenceNo = a.ReferenceNo,
                                         Debit = a.Debit,
                                         Credit = a.Credit,
                                     }).ToList();

                #endregion

                #region Sales Return

                var AllSalesReturnTaxs = (from a in oConnectionContext.DbClsSalesReturn
                                          join p in oConnectionContext.DbClsSales
                                          on a.SalesId equals p.SalesId
                                          where a.CompanyId == obj.CompanyId
                                          && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                          //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                          && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
           && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
           DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                          //&& a.TaxAmount != 0
                                          select new
                                          {
                                              IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                              a.TaxId,
                                              AmountExcTax = a.Subtotal - a.TotalDiscount,
                                              a.SalesReturnId,
                                              a.Date,
                                              a.InvoiceNo,
                                              a.CustomerId
                                          }).Concat(from a in oConnectionContext.DbClsSalesReturnDetails
                                                    join b in oConnectionContext.DbClsSalesReturn
                                                 on a.SalesReturnId equals b.SalesReturnId
                                                    join p in oConnectionContext.DbClsSales
                                                    on b.SalesId equals p.SalesId
                                                    where a.CompanyId == obj.CompanyId
                                         && a.IsDeleted == false && a.IsActive == true
                                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                    //&& a.TaxAmount != 0
                                                    select new
                                                    {
                                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                                        a.TaxId,
                                                        AmountExcTax = a.AmountExcTax,
                                                        b.SalesReturnId,
                                                        b.Date,
                                                        b.InvoiceNo,
                                                        b.CustomerId
                                                    }).ToList();

                List<ClsTaxVm> oClsSalesReturnTaxVm = new List<ClsTaxVm>();
                foreach (var item in AllSalesReturnTaxs)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesReturnTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax,
                            Id = item.SalesReturnId,
                            AddedOn = item.Date,
                            ReferenceNo = item.InvoiceNo,
                            CustomerId = item.CustomerId
                        });
                    }
                }

                var finalSalesReturnTaxs = oClsSalesReturnTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new
                         {
                             TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum(),
                             Id = c.Select(cs => cs.Id).FirstOrDefault(),
                             Date = c.Select(cs => cs.AddedOn).FirstOrDefault(),
                             ReferenceNo = c.Select(cs => cs.ReferenceNo).FirstOrDefault(),
                             CustomerId = c.Select(cs => cs.CustomerId).FirstOrDefault(),
                         }
                        ).ToList();

                var SalesReturnTaxList = finalSalesReturnTaxs.Select(a => new ClsBankPaymentVm
                {
                    Id = a.Id,
                    CustomerId = a.CustomerId,
                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = a.Date,
                    Type = "Sales Return",
                    ReferenceNo = a.ReferenceNo,
                    Debit = a.TaxAmount,
                    Credit = 0,
                }).ToList();

                SalesReturn = (from a in oConnectionContext.DbClsSalesReturn
                                   //   join b in oConnectionContext.DbClsSalesReturnDetails
                                   //on a.SalesReturnId equals b.SalesReturnId
                               join p in oConnectionContext.DbClsSales
                                  on a.SalesId equals p.SalesId
                               where
                               //a.AccountId == obj.AccountId && 
                               a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                               //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                               && p.IsActive == true && p.IsDeleted == false && p.IsCancelled == false
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                 && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                     && a.GrandTotal != 0
                               select new ClsBankPaymentVm
                               {
                                   //Account Receivable
                                   Id = a.SalesReturnId,
                                   CustomerId = p.CustomerId,
                                   AccountId = a.AccountId,
                                   Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == p.CustomerId).Select(c => c.Name).FirstOrDefault(),
                                   AddedOn = a.Date,
                                   Type = "Sales Return",
                                   ReferenceNo = a.InvoiceNo,
                                   Debit = 0,
                                   Credit = a.GrandTotal
                               }).Concat(from a in oConnectionContext.DbClsSalesReturnDetails
                                         join b in oConnectionContext.DbClsSalesReturn
                                      on a.SalesReturnId equals b.SalesReturnId
                                         join p in oConnectionContext.DbClsSales
                                                      on b.SalesId equals p.SalesId
                                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                                        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                            && a.UnitCost != 0
                                         group a by new { a.AccountId, b.SalesReturnId } into stdGroup
                                         select new ClsBankPaymentVm
                                         {
                                             // sales account
                                             Id = stdGroup.Key.SalesReturnId,
                                             CustomerId = oConnectionContext.DbClsSalesReturn.Where(x => x.SalesReturnId == stdGroup.Key.SalesReturnId).Select(x => x.CustomerId).FirstOrDefault(),
                                             AccountId = stdGroup.Key.AccountId,
                                             Notes = "",
                                             AddedOn = oConnectionContext.DbClsSalesReturn.Where(x => x.IsDeleted == false && x.IsCancelled == false && x.SalesReturnId == stdGroup.Key.SalesReturnId).Select(x => x.Date).FirstOrDefault(),
                                             Type = "Sales Return",
                                             ReferenceNo = oConnectionContext.DbClsSalesReturn.Where(x => x.IsDeleted == false && x.IsCancelled == false && x.SalesReturnId == stdGroup.Key.SalesReturnId).Select(x => x.InvoiceNo).FirstOrDefault(),
                                             Debit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                             Credit = 0
                                         })
                               .Concat(from b in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                       join a in oConnectionContext.DbClsSalesReturn
                                       on b.SalesReturnId equals a.SalesReturnId
                                       join p in oConnectionContext.DbClsSales
                                       on a.SalesId equals p.SalesId
                                       where
                                       //a.OtherChargesAccountId == obj.AccountId && 
                                       a.CompanyId == obj.CompanyId
                                       && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                       && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                       && b.IsDeleted == false && b.IsActive == true
                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                             l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                         && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                             DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                             && b.AmountExcTax != 0
                                       select new ClsBankPaymentVm
                                       {
                                           // other charge
                                           Id = a.SalesReturnId,
                                           CustomerId = p.CustomerId,
                                           AccountId = b.AccountId,
                                           Notes = "",
                                           AddedOn = a.Date,
                                           Type = "Sales Return",
                                           ReferenceNo = a.InvoiceNo,
                                           Debit = b.AmountExcTax,
                                           Credit = 0,
                                       }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                 join p in oConnectionContext.DbClsSales
                                        on a.SalesId equals p.SalesId
                                                 where
                                                 //a.RoundOffAccountId == obj.AccountId && 
                                                 a.CompanyId == obj.CompanyId
                                                 && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                 && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                                   && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                       DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                       && a.RoundOff != 0
                                                 select new ClsBankPaymentVm
                                                 {
                                                     // Round off charge
                                                     Id = a.SalesReturnId,
                                                     CustomerId = p.CustomerId,
                                                     AccountId = a.RoundOffAccountId,
                                                     Notes = "",
                                                     AddedOn = a.Date,
                                                     Type = "Sales Return",
                                                     ReferenceNo = a.InvoiceNo,
                                                     Debit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                     Credit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                                 }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                           join p in oConnectionContext.DbClsSales
                                                           on a.SalesId equals p.SalesId
                                                           where
                                                           //a.DiscountAccountId == obj.AccountId && 
                                                           a.CompanyId == obj.CompanyId
                                                           && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                           && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                                             && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                 && a.Discount != 0
                                                           select new ClsBankPaymentVm
                                                           {
                                                               // discount 
                                                               Id = a.SalesReturnId,
                                                               CustomerId = p.CustomerId,
                                                               AccountId = a.DiscountAccountId,
                                                               Notes = "",
                                                               AddedOn = a.Date,
                                                               Type = "Sales Return",
                                                               ReferenceNo = a.InvoiceNo,
                                                               Debit = 0,
                                                               Credit = a.TotalDiscount,
                                                           }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                                     join p in oConnectionContext.DbClsSales
                                                                     on a.SalesId equals p.SalesId
                                                                     where
                                                                     //a.DiscountAccountId == obj.AccountId && 
                                                                     a.CompanyId == obj.CompanyId
                                                                     && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                     && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                                                       && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                           DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                           && a.Discount != 0
                                                                     select new ClsBankPaymentVm
                                                                     {
                                                                         // special discount 
                                                                         Id = a.SalesReturnId,
                                                                         CustomerId = p.CustomerId,
                                                                         AccountId = a.SpecialDiscountAccountId,
                                                                         Notes = "",
                                                                         AddedOn = a.Date,
                                                                         Type = "Sales Return",
                                                                         ReferenceNo = a.InvoiceNo,
                                                                         Debit = 0,
                                                                         Credit = a.SpecialDiscount,
                                                                     }).ToList();

                SalesReturn = SalesReturn.Concat(from a in SalesReturnTaxList
                                                 select new ClsBankPaymentVm
                                                 {
                                                     // tax 
                                                     Id = a.Id,
                                                     CustomerId = a.CustomerId,
                                                     AccountId = a.AccountId,
                                                     Notes = "",
                                                     AddedOn = a.AddedOn,
                                                     Type = "Sales Return",
                                                     ReferenceNo = a.ReferenceNo,
                                                     Debit = a.Debit,
                                                     Credit = 0
                                                 }).ToList();

                #endregion

                #region Purchase

                var PurchaseTaxList = (from q in oConnectionContext.DbClsPurchaseTaxJournal
                                       join a in oConnectionContext.DbClsPurchaseDetails
                                       on q.PurchaseDetailsId equals a.PurchaseDetailsId
                                       join b in oConnectionContext.DbClsPurchase
                                    on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                                      //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                       && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                           //&& a.TaxAmount != 0 
                                           && b.Status != "Draft"
                                       select new
                                       {
                                           Id = b.PurchaseId,
                                           SupplierId = b.SupplierId,
                                           AccountId = q.AccountId,
                                           Notes = "",
                                           AddedOn = b.PurchaseDate,
                                           Type = "Purchase",
                                           ReferenceNo = b.ReferenceNo,
                                           Debit = (q.PurchaseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                           Credit = (q.PurchaseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                       }).Concat(from q in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                 join a in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                 on q.PurchaseAdditionalChargesId equals a.PurchaseAdditionalChargesId
                                                 join b in oConnectionContext.DbClsPurchase
                                              on a.PurchaseId equals b.PurchaseId
                                                 join c in oConnectionContext.DbClsTax
                                                   on q.TaxId equals c.TaxId
                                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                                 && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                                     //&& a.TaxAmount != 0 
                                                     && b.Status != "Draft"
                                                 select new
                                                 {
                                                     Id = b.PurchaseId,
                                                     SupplierId = b.SupplierId,
                                                     AccountId = q.AccountId,
                                                     Notes = "",
                                                     AddedOn = b.PurchaseDate,
                                                     Type = "Purchase",
                                                     ReferenceNo = b.ReferenceNo,
                                                     Debit = (q.PurchaseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                                     Credit = (q.PurchaseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                                 }).ToList();

                var purchaseAccount = (from a in oConnectionContext.DbClsPurchaseDetails
                                       join b in oConnectionContext.DbClsPurchase
                                    on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                                       //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                            DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                            //&& a.TaxAmount != 0 
                                            && b.Status != "Draft"
                                       select new ClsBankPaymentVm
                                       {
                                           // Purchase account
                                           Id = b.PurchaseId,
                                           SupplierId = b.SupplierId,
                                           AccountId = a.AccountId,
                                           Notes = "",
                                           AddedOn = b.PurchaseDate,
                                           Type = "Purchase",
                                           ReferenceNo = b.ReferenceNo,
                                           Debit = a.UnitCost * a.Quantity, //b.IsReverseCharge == 1 ? a.Amount : a.AmountExcTax,
                                           Credit = 0,
                                       }).ToList();

                Purchase = (from a in oConnectionContext.DbClsPurchase
                                //      join b in oConnectionContext.DbClsPurchaseDetails
                                //on a.PurchaseId equals b.PurchaseId
                            where
                            //a.AccountId == obj.AccountId && 
                            a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                         //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
           && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
               && a.GrandTotal != 0 && a.Status != "Draft"
                            select new ClsBankPaymentVm
                            {
                                //Account Payable
                                Id = a.PurchaseId,
                                SupplierId = a.SupplierId,
                                AccountId = a.AccountId,
                                Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                                AddedOn = a.PurchaseDate,
                                Type = "Purchase",
                                ReferenceNo = a.ReferenceNo,
                                Debit = 0,
                                Credit = a.GrandTotal
                            })
                            .Concat(from b in oConnectionContext.DbClsPurchaseAdditionalCharges
                                    join a in oConnectionContext.DbClsPurchase
                                    on b.PurchaseId equals a.PurchaseId
                                    where
                                    //a.ShippingChargesAccountId == obj.AccountId && 
                                    a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                    && b.IsDeleted == false && b.IsActive == true
                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                      && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                          DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                          && b.AmountExcTax != 0 && a.Status != "Draft"
                                    select new ClsBankPaymentVm
                                    {
                                        // shipping charge
                                        Id = a.PurchaseId,
                                        SupplierId = a.SupplierId,
                                        AccountId = b.AccountId,
                                        Notes = "",
                                        AddedOn = a.PurchaseDate,
                                        Type = "Purchase",
                                        ReferenceNo = a.ReferenceNo,
                                        Debit = b.AmountExcTax,
                                        Credit = 0,
                                    }).Concat(from a in oConnectionContext.DbClsPurchase
                                              where
                                              //a.RoundOffAccountId == obj.AccountId && 
                                              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                                    DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                                    && a.RoundOff != 0 && a.Status != "Draft"
                                              select new ClsBankPaymentVm
                                              {
                                                  // Round off charge
                                                  Id = a.PurchaseId,
                                                  SupplierId = a.SupplierId,
                                                  AccountId = a.RoundOffAccountId,
                                                  Notes = "",
                                                  AddedOn = a.PurchaseDate,
                                                  Type = "Purchase",
                                                  ReferenceNo = a.ReferenceNo,
                                                  Debit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                  Credit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0
                                              }).Concat(from a in oConnectionContext.DbClsPurchase
                                                        where
                                                        //a.DiscountAccountId == obj.AccountId && 
                                                        a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                          && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                                              DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                                              && a.Discount != 0 && a.Status != "Draft"
                                                        select new ClsBankPaymentVm
                                                        {
                                                            // discount 
                                                            Id = a.PurchaseId,
                                                            SupplierId = a.SupplierId,
                                                            AccountId = a.DiscountAccountId,
                                                            Notes = "",
                                                            AddedOn = a.PurchaseDate,
                                                            Type = "Purchase",
                                                            ReferenceNo = a.ReferenceNo,
                                                            Debit = 0,
                                                            Credit = a.TotalDiscount,
                                                        }).Concat(from a in oConnectionContext.DbClsPurchase
                                                                  where
                                                                  //a.DiscountAccountId == obj.AccountId && 
                                                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                    && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                                                        DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                                                        && a.Discount != 0 && a.Status != "Draft"
                                                                  select new ClsBankPaymentVm
                                                                  {
                                                                      // special discount 
                                                                      Id = a.PurchaseId,
                                                                      SupplierId = a.SupplierId,
                                                                      AccountId = a.SpecialDiscountAccountId,
                                                                      Notes = "",
                                                                      AddedOn = a.PurchaseDate,
                                                                      Type = "Purchase",
                                                                      ReferenceNo = a.ReferenceNo,
                                                                      Debit = 0,
                                                                      Credit = a.SpecialDiscount,
                                                                  }).ToList();

                Purchase = Purchase.Concat(from a in purchaseAccount
                                           group a by new { a.AccountId, a.Id } into stdGroup
                                           //orderby stdgroup.key descending
                                           select new ClsBankPaymentVm
                                           {
                                               Id = stdGroup.Key.Id,
                                               SupplierId = stdGroup.Select(x => x.SupplierId).DefaultIfEmpty().FirstOrDefault(),
                                               AccountId = stdGroup.Key.AccountId,
                                               Notes = stdGroup.Select(x => x.Notes).DefaultIfEmpty().FirstOrDefault(),
                                               AddedOn = stdGroup.Select(x => x.AddedOn).DefaultIfEmpty().FirstOrDefault(),
                                               Type = "Purchase",
                                               ReferenceNo = stdGroup.Select(x => x.ReferenceNo).DefaultIfEmpty().FirstOrDefault(),
                                               Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                               Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                           }).Concat(from a in PurchaseTaxList
                                                     select new ClsBankPaymentVm
                                                     {
                                                         // tax 
                                                         Id = a.Id,
                                                         SupplierId = a.SupplierId,
                                                         AccountId = a.AccountId,
                                                         Notes = "",
                                                         AddedOn = a.AddedOn,
                                                         Type = "Purchase",
                                                         ReferenceNo = a.ReferenceNo,
                                                         Debit = a.Debit,
                                                         Credit = a.Credit
                                                     }).ToList();

                #endregion

                #region Purchase Return

                var AllPurchaseReturnTaxs = (from a in oConnectionContext.DbClsPurchaseReturn
                                                 //   join b in oConnectionContext.DbClsPurchaseReturnDetails
                                                 //on a.PurchaseReturnId equals b.PurchaseReturnId
                                             where a.CompanyId == obj.CompanyId
                                             && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                             //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                        && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                             //&& a.TaxAmount != 0
                                             select new
                                             {
                                                 IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                                 a.TaxId,
                                                 AmountExcTax = a.Subtotal - a.TotalDiscount,
                                                 a.PurchaseReturnId,
                                                 a.Date,
                                                 a.InvoiceNo,
                                                 a.SupplierId
                                             }).Concat(from a in oConnectionContext.DbClsPurchaseReturnDetails
                                                       join b in oConnectionContext.DbClsPurchaseReturn
                                                    on a.PurchaseReturnId equals b.PurchaseReturnId
                                                       where a.CompanyId == obj.CompanyId
                                                       && a.IsDeleted == false && a.IsActive == true
                                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                       //&& a.TaxAmount != 0
                                                       select new
                                                       {
                                                           IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                                           a.TaxId,
                                                           AmountExcTax = a.AmountExcTax,
                                                           b.PurchaseReturnId,
                                                           b.Date,
                                                           b.InvoiceNo,
                                                           b.SupplierId
                                                       }).ToList();

                List<ClsTaxVm> oClsPurchaseReturnTaxVm = new List<ClsTaxVm>();
                foreach (var item in AllPurchaseReturnTaxs)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsPurchaseReturnTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax,
                            Id = item.PurchaseReturnId,
                            AddedOn = item.Date,
                            ReferenceNo = item.InvoiceNo,
                            SupplierId = item.SupplierId
                        });
                    }
                }

                var finalPurchaseReturnTaxs = oClsPurchaseReturnTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new
                         {
                             TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum(),
                             Id = c.Select(cs => cs.Id).FirstOrDefault(),
                             Date = c.Select(cs => cs.AddedOn).FirstOrDefault(),
                             ReferenceNo = c.Select(cs => cs.ReferenceNo).FirstOrDefault(),
                             SupplierId = c.Select(cs => cs.SupplierId).FirstOrDefault(),
                         }
                        ).ToList();

                var PurchaseReturnTaxList = finalPurchaseReturnTaxs.Select(a => new ClsBankPaymentVm
                {
                    Id = a.Id,
                    SupplierId = a.SupplierId,
                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = a.Date,
                    Type = "Purchase Return",
                    ReferenceNo = a.ReferenceNo,
                    Debit = 0,
                    Credit = a.TaxAmount,
                }).ToList();

                PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                  where
                                  //a.AccountId == obj.AccountId && 
                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                  //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                    && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                        && a.GrandTotal != 0
                                  select new ClsBankPaymentVm
                                  {
                                      //Account Payable
                                      Id = a.PurchaseReturnId,
                                      SupplierId = a.SupplierId,
                                      AccountId = a.AccountId,
                                      Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                                      AddedOn = a.Date,
                                      Type = "Purchase Return",
                                      ReferenceNo = a.InvoiceNo,
                                      Debit = a.GrandTotal,
                                      Credit = 0
                                  }).Concat
                                  (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                   join b in oConnectionContext.DbClsPurchaseReturn
                                on a.PurchaseReturnId equals b.PurchaseReturnId
                                   where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                   && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      && a.UnitCost != 0
                                   group a by new { a.AccountId, a.PurchaseReturnId } into stdGroup
                                   select new ClsBankPaymentVm
                                   {
                                       // Purchase account
                                       Id = stdGroup.Key.PurchaseReturnId,
                                       SupplierId = oConnectionContext.DbClsPurchaseReturn.Where(x => x.PurchaseReturnId == stdGroup.Key.PurchaseReturnId).Select(x => x.SupplierId).FirstOrDefault(),
                                       AccountId = stdGroup.Key.AccountId,
                                       Notes = "",
                                       AddedOn = oConnectionContext.DbClsPurchaseReturn.Where(x => x.PurchaseReturnId == stdGroup.Key.PurchaseReturnId).Select(x => x.Date).FirstOrDefault(),
                                       Type = "Purchase Return",
                                       ReferenceNo = oConnectionContext.DbClsPurchaseReturn.Where(x => x.PurchaseReturnId == stdGroup.Key.PurchaseReturnId).Select(x => x.InvoiceNo).FirstOrDefault(),
                                       Debit = 0,
                                       Credit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                   })
                                  .Concat(from b in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                          join a in oConnectionContext.DbClsPurchaseReturn
                                          on b.PurchaseReturnId equals a.PurchaseReturnId
                                          where
                                          //a.OtherChargesAccountId == obj.AccountId && 
                                          a.CompanyId == obj.CompanyId
                                          && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                          && b.IsDeleted == false && b.IsActive == true
                                          && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                            && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                && b.AmountExcTax != 0
                                          select new ClsBankPaymentVm
                                          {
                                              // other charge
                                              Id = a.PurchaseReturnId,
                                              SupplierId = a.SupplierId,
                                              AccountId = b.AccountId,
                                              Notes = "",
                                              AddedOn = a.Date,
                                              Type = "Purchase Return",
                                              ReferenceNo = a.InvoiceNo,
                                              Debit = 0,
                                              Credit = b.AmountExcTax,
                                          }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                    where
                                                    //a.RoundOffAccountId == obj.AccountId && 
                                                    a.CompanyId == obj.CompanyId
                                                    && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                      && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                          DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                          && a.RoundOff != 0
                                                    select new ClsBankPaymentVm
                                                    {
                                                        // Round off charge
                                                        Id = a.PurchaseReturnId,
                                                        SupplierId = a.SupplierId,
                                                        AccountId = a.RoundOffAccountId,
                                                        Notes = "",
                                                        AddedOn = a.Date,
                                                        Type = "Purchase Return",
                                                        ReferenceNo = a.InvoiceNo,
                                                        Debit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                                        Credit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                    }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                              where
                                                              //a.DiscountAccountId == obj.AccountId && 
                                                              a.CompanyId == obj.CompanyId
                                                              && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                    DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                    && a.Discount != 0
                                                              select new ClsBankPaymentVm
                                                              {
                                                                  // discount 
                                                                  Id = a.PurchaseReturnId,
                                                                  SupplierId = a.SupplierId,
                                                                  AccountId = a.DiscountAccountId,
                                                                  Notes = "",
                                                                  AddedOn = a.Date,
                                                                  Type = "Purchase Return",
                                                                  ReferenceNo = a.InvoiceNo,
                                                                  Debit = a.TotalDiscount,
                                                                  Credit = 0,
                                                              }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                                        where
                                                                        //a.DiscountAccountId == obj.AccountId && 
                                                                        a.CompanyId == obj.CompanyId
                                                                        && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                          && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                              DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                              && a.Discount != 0
                                                                        select new ClsBankPaymentVm
                                                                        {
                                                                            // special discount 
                                                                            Id = a.PurchaseReturnId,
                                                                            SupplierId = a.SupplierId,
                                                                            AccountId = a.SpecialDiscountAccountId,
                                                                            Notes = "",
                                                                            AddedOn = a.Date,
                                                                            Type = "Purchase Return",
                                                                            ReferenceNo = a.InvoiceNo,
                                                                            Debit = a.SpecialDiscount,
                                                                            Credit = 0,
                                                                        }).ToList();

                PurchaseReturn = PurchaseReturn.Concat(from a in PurchaseReturnTaxList
                                                       select new ClsBankPaymentVm
                                                       {
                                                           // tax 
                                                           Id = a.Id,
                                                           SupplierId = a.SupplierId,
                                                           AccountId = a.AccountId,
                                                           Notes = "",
                                                           AddedOn = a.AddedOn,
                                                           Type = "Purchase Return",
                                                           ReferenceNo = a.ReferenceNo,
                                                           Debit = 0,
                                                           Credit = a.Credit
                                                       }).ToList();

                #endregion

                #region Customer Payment
                SalesPayments = (from a in oConnectionContext.DbClsCustomerPayment
                                     //join b in oConnectionContext.DbClsSales
                                     //on a.SalesId equals b.SalesId
                                 where a.ParentId == 0 &&
                                 //a.AccountId == obj.AccountId && 
                                 a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                 //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                 //(a.Type.ToLower() == "sales payment")
                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                   && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                       && a.Amount != 0
                                 select new ClsBankPaymentVm
                                 {
                                     Id = a.CustomerPaymentId,
                                     CustomerId = a.CustomerId,
                                     AccountId = a.AccountId,
                                     Notes = "",
                                     AddedOn = a.PaymentDate,
                                     Type = a.Type,//"Sales Payment",
                                     ReferenceNo = a.ReferenceNo,
                                     Debit = a.Amount,
                                     Credit = 0
                                 }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                               //join b in oConnectionContext.DbClsSales
                                               //on a.SalesId equals b.SalesId
                                           where a.ParentId == 0 &&
                                           //a.JournalAccountId == obj.AccountId && 
                                           a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                           //(a.Type.ToLower() == "sales payment")
                                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                             && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                 && a.Amount != 0
                                           select new ClsBankPaymentVm
                                           {
                                               Id = a.CustomerPaymentId,
                                               CustomerId = a.CustomerId,
                                               AccountId = a.JournalAccountId,
                                               Notes = "",
                                               AddedOn = a.PaymentDate,
                                               Type = a.Type,//"Sales Payment",
                                               ReferenceNo = a.ReferenceNo,
                                               Debit = 0,
                                               Credit = a.Amount
                                           }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                                         //join b in oConnectionContext.DbClsSales
                                                         //on a.SalesId equals b.SalesId
                                                     where a.ParentId != 0 && a.AccountId != 0 &&
                                                     //a.AccountId == obj.AccountId && 
                                                     a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                     //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                     //(a.Type.ToLower() == "sales payment")
                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                           && a.Amount != 0
                                                     select new ClsBankPaymentVm
                                                     {
                                                         Id = a.ParentId,
                                                         CustomerId = a.CustomerId,
                                                         AccountId = a.AccountId,
                                                         Notes = "",
                                                         AddedOn = a.PaymentDate,
                                                         Type = a.Type,//"Sales Payment",
                                                         ReferenceNo = oConnectionContext.DbClsCustomerPayment.Where(x => x.CustomerPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                         Debit = a.Amount,
                                                         Credit = 0
                                                     }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                                                   //join b in oConnectionContext.DbClsSales
                                                                   //on a.SalesId equals b.SalesId
                                                               where a.ParentId != 0 && a.AccountId != 0 &&
                                                               //a.JournalAccountId == obj.AccountId && 
                                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                        //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                                        //(a.Type.ToLower() == "sales payment")
                                                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                 && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                                     DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                                     && a.Amount != 0
                                                               select new ClsBankPaymentVm
                                                               {
                                                                   Id = a.ParentId,
                                                                   CustomerId = a.CustomerId,
                                                                   AccountId = a.JournalAccountId,
                                                                   Notes = "",
                                                                   AddedOn = a.PaymentDate,
                                                                   Type = a.Type,//"Sales Payment",
                                                                   ReferenceNo = oConnectionContext.DbClsCustomerPayment.Where(x => x.CustomerPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                                   Debit = 0,
                                                                   Credit = a.Amount
                                                               }).ToList();
                #endregion

                #region Supplier Payment
                PurchasePayments = (from a in oConnectionContext.DbClsSupplierPayment
                                        //   join b in oConnectionContext.DbClsPurchase
                                        //on a.PurchaseId equals b.PurchaseId
                                    where a.ParentId == 0 &&
                                    //a.JournalAccountId == obj.AccountId && 
                                    a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                 //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                 //a.Type.ToLower() == "purchase payment"
                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                   && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                       && a.Amount != 0
                                    select new ClsBankPaymentVm
                                    {
                                        Id = a.SupplierPaymentId,
                                        SupplierId = a.SupplierId,
                                        AccountId = a.AccountId,
                                        Notes = "",
                                        AddedOn = a.PaymentDate,
                                        Type = a.Type,//"Purchase Payment",
                                        ReferenceNo = a.ReferenceNo,
                                        Debit = 0,
                                        Credit = a.Amount
                                    }).Concat(from a in oConnectionContext.DbClsSupplierPayment
                                                  //join b in oConnectionContext.DbClsPurchase
                                                  //on a.PurchaseId equals b.PurchaseId
                                              where a.ParentId == 0 &&
                                              //a.AccountId == obj.AccountId && 
                                              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                              //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                              //a.Type.ToLower() == "purchase payment"
                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                    DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                    && a.Amount != 0
                                              select new ClsBankPaymentVm
                                              {
                                                  Id = a.SupplierPaymentId,
                                                  SupplierId = a.SupplierId,
                                                  AccountId = a.JournalAccountId,
                                                  Notes = "",
                                                  AddedOn = a.PaymentDate,
                                                  Type = a.Type,//"Purchase Payment",
                                                  ReferenceNo = a.ReferenceNo,
                                                  Debit = a.Amount,
                                                  Credit = 0
                                              }).Concat(from a in oConnectionContext.DbClsSupplierPayment
                                                            //   join b in oConnectionContext.DbClsPurchase
                                                            //on a.PurchaseId equals b.PurchaseId
                                                        where a.ParentId != 0 && a.AccountId != 0 &&
                                                        //a.JournalAccountId == obj.AccountId && 
                                                        a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                     //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                     //a.Type.ToLower() == "purchase payment"
                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                           && a.Amount != 0
                                                        select new ClsBankPaymentVm
                                                        {
                                                            Id = a.ParentId,
                                                            SupplierId = a.SupplierId,
                                                            AccountId = a.AccountId,
                                                            Notes = "",
                                                            AddedOn = a.PaymentDate,
                                                            Type = a.Type,//"Purchase Payment",
                                                            ReferenceNo = oConnectionContext.DbClsSupplierPayment.Where(x => x.SupplierPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                            Debit = 0,
                                                            Credit = a.Amount
                                                        }).Concat(from a in oConnectionContext.DbClsSupplierPayment
                                                                      //join b in oConnectionContext.DbClsPurchase
                                                                      //on a.PurchaseId equals b.PurchaseId
                                                                  where a.ParentId != 0 && a.AccountId != 0 &&
                                                                  //a.AccountId == obj.AccountId && 
                                                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                                           //a.Type.ToLower() == "purchase payment"
                                                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                    && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                                        DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                                        && a.Amount != 0
                                                                  select new ClsBankPaymentVm
                                                                  {
                                                                      Id = a.ParentId,
                                                                      SupplierId = a.SupplierId,
                                                                      AccountId = a.JournalAccountId,
                                                                      Notes = "",
                                                                      AddedOn = a.PaymentDate,
                                                                      Type = a.Type,//"Purchase Payment",
                                                                      ReferenceNo = oConnectionContext.DbClsSupplierPayment.Where(x => x.SupplierPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                                      Debit = a.Amount,
                                                                      Credit = 0
                                                                  }).ToList();
                #endregion                

                #region Stock Adjustment

                var allStockAdjust = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                                      join b in oConnectionContext.DbClsStockAdjustment
                                   on a.StockAdjustmentId equals b.StockAdjustmentId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && b.IsDeleted == false && b.IsActive == true
                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                  && DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                      && a.Amount != 0
                                      select new ClsBankPaymentVm
                                      {
                                          //AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                          Id = a.StockAdjustmentId,
                                          AccountId = a.AccountId,
                                          Notes = "",
                                          AddedOn = b.AdjustmentDate,
                                          Type = "Stock Adjustment",
                                          ReferenceNo = b.ReferenceNo,
                                          Debit = b.AdjustmentType == "debit" ? 0 : a.UnitCost * a.Quantity,
                                          Credit = b.AdjustmentType == "credit" ? 0 : a.UnitCost * a.Quantity
                                      }).ToList();

                StockAdjustment = (from a in oConnectionContext.DbClsStockAdjustment
                                       //   join b in oConnectionContext.DbClsStockAdjustmentDetails
                                       //on a.StockAdjustmentId equals b.StockAdjustmentId
                                   where
                                   //a.AccountId == obj.AccountId && 
                                   a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                   //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                     && DbFunctions.TruncateTime(a.AdjustmentDate) >= obj.FromDate &&
                         DbFunctions.TruncateTime(a.AdjustmentDate) <= obj.ToDate
                         && a.TotalAmount != 0
                                   select new ClsBankPaymentVm
                                   {
                                       Id = a.StockAdjustmentId,
                                       AccountId = a.AccountId,
                                       Notes = "",
                                       AddedOn = a.AdjustmentDate,
                                       Type = "Stock Adjustment",
                                       ReferenceNo = a.ReferenceNo,
                                       Debit = a.AdjustmentType == "debit" ? a.TotalAmount : 0,
                                       Credit = a.AdjustmentType == "credit" ? a.TotalAmount : 0
                                   })
                                   //    .Concat(from a in oConnectionContext.DbClsStockAdjustmentDetails
                                   //              join b in oConnectionContext.DbClsStockAdjustment
                                   //           on a.StockAdjustmentId equals b.StockAdjustmentId
                                   //              where
                                   //              //b.AccountId == obj.AccountId && 
                                   //              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                   //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                                   //&& DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                   //    DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                   //    && a.Amount != 0
                                   //              select new ClsBankPaymentVm
                                   //              {
                                   //                  Id = a.StockAdjustmentId,
                                   //                  AccountId = b.AccountId,
                                   //                  Notes = "",
                                   //                  AddedOn = b.AdjustmentDate,
                                   //                  Type = "Stock Adjustment",
                                   //                  ReferenceNo = b.ReferenceNo,
                                   //                  Debit = b.AdjustmentType == "debit" ? 0 : a.Amount,
                                   //                  Credit = b.AdjustmentType == "credit" ? 0 : a.Amount
                                   //              })
                                   .ToList();

                StockAdjustment = StockAdjustment.Concat(from a in allStockAdjust
                                                         group a by new { a.AccountId, a.Id } into stdGroup
                                                         select new ClsBankPaymentVm
                                                         {
                                                             // sales account
                                                             Id = stdGroup.Key.Id,
                                                             AccountId = stdGroup.Key.AccountId,
                                                             Notes = "",
                                                             AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                                             Type = "Stock Adjustment",
                                                             ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                                             Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                                             Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                                         }).ToList();

                #endregion

                Ledger = Expenses//.Concat(Incomes)
                    .Concat(Contras).Concat(Journals).Concat(Sales).Concat(SalesReturn).Concat(Purchase).Concat(PurchaseReturn)
                    .Concat(SalesPayments)
                    //.Concat(SalesReturnPayments)
                    .Concat(PurchasePayments)
                    //.Concat(PurchaseReturnPayments)
                    .Concat(StockAdjustment)
                    .ToList();
            }
            else
            {
                #region Expenses
                var ExpenseTaxList = (from q in oConnectionContext.DbClsExpenseTaxJournal
                                      join a in oConnectionContext.DbClsExpensePayment
                                      on q.ExpensePaymentId equals a.ExpensePaymentId
                                      join b in oConnectionContext.DbClsExpense
                                   on a.ExpenseId equals b.ExpenseId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && b.IsDeleted == false && b.IsActive == true
                                      && b.BranchId == obj.BranchId
                               && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                   DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      //&& a.TaxAmount != 0
                                      select new
                                      {
                                          Id = b.ExpenseId,
                                          AccountId = q.AccountId,
                                          Notes = "",
                                          AddedOn = b.Date,
                                          Type = "Expense",
                                          ReferenceNo = b.ReferenceNo,
                                          Debit = (q.ExpenseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                          Credit = (q.ExpenseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                      }).ToList();

                var expenseAccount = (from a in oConnectionContext.DbClsExpensePayment
                                      join b in oConnectionContext.DbClsExpense
                                   on a.ExpenseId equals b.ExpenseId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && b.IsDeleted == false && b.IsActive == true
                                      && b.BranchId == obj.BranchId
                        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      //&& a.TaxAmount != 0
                                      select new ClsBankPaymentVm
                                      {
                                          Id = b.ExpenseId,
                                          AccountId = a.AccountId,
                                          Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                          AddedOn = b.Date,
                                          Type = "Expense",
                                          ReferenceNo = b.ReferenceNo,
                                          Debit = b.IsReverseCharge == 1 ? a.Amount : a.AmountExcTax,
                                          Credit = 0
                                      }).ToList();

                Expenses =
                            //      (from a in oConnectionContext.DbClsExpensePayment
                            //              join b in oConnectionContext.DbClsExpense
                            //           on a.ExpenseId equals b.ExpenseId
                            //              where
                            //              //a.AccountId == obj.AccountId && 
                            //              b.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                            //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                            //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                            //&& DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                            //    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                            //    && a.AmountExcTax != 0
                            //              select new ClsBankPaymentVm
                            //              {
                            //                  Id = b.ExpenseId,
                            //                  AccountId = a.AccountId,
                            //                  Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                            //                  AddedOn = b.Date,
                            //                  Type = "Expense",
                            //                  ReferenceNo = b.ReferenceNo,
                            //                  Debit = a.AmountExcTax,
                            //                  Credit = 0
                            //              }).Concat
                            (from a in oConnectionContext.DbClsExpense
                                 //   join b in oConnectionContext.DbClsExpensePayment
                                 //on a.ExpenseId equals b.ExpenseId
                             where
                             //a.AccountId == obj.AccountId && 
                             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                             //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                             && a.BranchId == obj.BranchId
               && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                   DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                   && a.GrandTotal != 0
                             select new ClsBankPaymentVm
                             {
                                 Id = a.ExpenseId,
                                 AccountId = a.AccountId,
                                 Notes = "",
                                 //Notes = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == a.ExpenseId &&
                                 //c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                                 AddedOn = a.Date,
                                 Type = "Expense",
                                 ReferenceNo = a.ReferenceNo,
                                 Debit = 0,
                                 Credit = a.GrandTotal
                                 //Credit = oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == a.ExpenseId &&
                                 //c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => c.Amount).DefaultIfEmpty().FirstOrDefault(),
                             })//.Where(a => a.Credit != 0)
                            .ToList().Select(a => new ClsBankPaymentVm
                            {
                                Id = a.Id,
                                AccountId = a.AccountId,
                                Notes = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == a.Id &&
                           c.IsDeleted == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                                AddedOn = a.AddedOn,
                                Type = a.Type,
                                ReferenceNo = a.ReferenceNo,
                                Debit = a.Debit,
                                Credit = a.Credit
                            })
                      //      .Concat(from a in oConnectionContext.DbClsExpensePayment
                      //              join b in oConnectionContext.DbClsExpense
                      //           on a.ExpenseId equals b.ExpenseId
                      //              where
                      //              //a.TaxAccountId == obj.AccountId && 
                      //              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                      //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                      //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                      //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                      //&& DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      //    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                      //    && a.TaxAmount != 0
                      //              select new ClsBankPaymentVm
                      //              {
                      //                  // tax 
                      //                  Id = b.ExpenseId,
                      //                  AccountId = a.TaxAccountId,
                      //                  Notes = "",
                      //                  AddedOn = b.Date,
                      //                  Type = "Expense",
                      //                  ReferenceNo = b.ReferenceNo,
                      //                  Debit = a.TaxAmount,
                      //                  Credit = 0,
                      //              })
                      .ToList();

                //Expenses = Expenses.Concat(from a in ExpenseTaxList
                //                           select new ClsBankPaymentVm
                //                           {
                //                               // tax 
                //                               Id = a.Id,
                //                               AccountId = a.AccountId,
                //                               Notes = "",
                //                               AddedOn = a.AddedOn,
                //                               Type = "Expense",
                //                               ReferenceNo = a.ReferenceNo,
                //                               Debit = a.Debit,
                //                               Credit = 0,
                //                           }).ToList();

                Expenses = Expenses.Concat(from a in expenseAccount
                                           group a by new { a.AccountId, a.Id } into stdGroup
                                           select new ClsBankPaymentVm
                                           {
                                               Id = stdGroup.Key.Id,
                                               AccountId = stdGroup.Key.AccountId,
                                               Notes = stdGroup.Select(x => x.Notes).DefaultIfEmpty().FirstOrDefault(),
                                               AddedOn = stdGroup.Select(x => x.AddedOn).DefaultIfEmpty().FirstOrDefault(),
                                               Type = "Expense",
                                               ReferenceNo = stdGroup.Select(x => x.ReferenceNo).DefaultIfEmpty().FirstOrDefault(),
                                               Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                               Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                           }).Concat(from a in ExpenseTaxList
                                                     select new ClsBankPaymentVm
                                                     {
                                                         // tax 
                                                         Id = a.Id,
                                                         AccountId = a.AccountId,
                                                         Notes = a.Notes,
                                                         AddedOn = a.AddedOn,
                                                         Type = "Expense",
                                                         ReferenceNo = a.ReferenceNo,
                                                         Debit = a.Debit,
                                                         Credit = a.Credit,
                                                     }).ToList();

                #endregion

                #region Incomes
                //  Incomes = (from a in oConnectionContext.DbClsIncomePayment
                //             join b in oConnectionContext.DbClsIncome
                //          on a.IncomeId equals b.IncomeId
                //             where
                //             //a.AccountId == obj.AccountId && 
                //             a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //&& DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                //    DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                //             select new ClsBankPaymentVm
                //             {
                //                 AccountId = a.AccountId,
                //                 Id = 0,
                //                 Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                //                 AddedOn = b.Date,
                //                 Type = "Income",
                //                 ReferenceNo = b.ReferenceNo,
                //                 Debit = 0,
                //                 Credit = a.Amount,
                //             }).Concat(from a in oConnectionContext.DbClsIncome
                //                       join b in oConnectionContext.DbClsIncomePayment
                //                    on a.IncomeId equals b.IncomeId
                //                       where
                //                       //a.AccountId == obj.AccountId && 
                //                       a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                //                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                //                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //             l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
                //         && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                //             DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                //                       select new ClsBankPaymentVm
                //                       {
                //                           AccountId = a.AccountId,
                //                           Id = a.IncomeId,
                //                           Notes = "",
                //                           //Notes = string.Join(",", oConnectionContext.DbClsIncomePayment.Where(c => c.IncomeId == a.IncomeId &&
                //                           //c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                //                           AddedOn = a.Date,
                //                           Type = "Income",
                //                           ReferenceNo = a.ReferenceNo,
                //                           Debit = oConnectionContext.DbClsIncomePayment.Where(c => c.IncomeId == a.IncomeId &&
                //                           c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => c.Amount).DefaultIfEmpty().FirstOrDefault(),
                //                           Credit = 0
                //                       }).AsEnumerable().Select(a => new ClsBankPaymentVm
                //                       {
                //                           AccountId = a.AccountId,
                //                           Id = 0,
                //                           Notes = string.Join(",", oConnectionContext.DbClsIncomePayment.Where(c => c.IncomeId == a.Id &&
                //                           c.IsDeleted == false && c.IsCancelled == false && c.IsActive == true).Select(c => oConnectionContext.DbClsAccount.Where(d => d.AccountId == c.AccountId).Select(d => d.AccountName).FirstOrDefault())),
                //                           AddedOn = a.AddedOn,
                //                           Type = a.Type,
                //                           ReferenceNo = a.ReferenceNo,
                //                           Debit = a.Debit,
                //                           Credit = a.Credit
                //                       }).Concat(from a in oConnectionContext.DbClsIncomePayment
                //                                 join b in oConnectionContext.DbClsIncome
                //                              on a.IncomeId equals b.IncomeId
                //                                 where
                //                                 //a.TaxAccountId == obj.AccountId && 
                //                                 a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                //                                 && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                //                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //                   && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                //                       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                //                                 select new ClsBankPaymentVm
                //                                 {
                //                                     // tax 
                //                                     AccountId = a.TaxAccountId,
                //                                     Notes = "",
                //                                     AddedOn = b.Date,
                //                                     Type = "Income",
                //                                     ReferenceNo = b.ReferenceNo,
                //                                     Debit = a.TaxAmount,
                //                                     Credit = 0,
                //                                 }).ToList();
                #endregion

                #region Contras
                Contras = (from a in oConnectionContext.DbClsContra
                           where
                           //a.FromAccountId == obj.AccountId && 
                           a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && a.BranchId == obj.BranchId
             && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                 && a.Amount != 0
                           select new ClsBankPaymentVm
                           {
                               Id = a.ContraId,
                               AccountId = a.FromAccountId,
                               Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                               AddedOn = a.PaymentDate,
                               Type = a.Type,
                               ReferenceNo = a.ReferenceNo,
                               Debit = 0,
                               Credit = a.Amount
                           }).Concat(from a in oConnectionContext.DbClsContra
                                     where
                                     //a.ToAccountId == obj.AccountId && 
                                     a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                    && a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                           && a.Amount != 0
                                     select new ClsBankPaymentVm
                                     {
                                         Id = a.ContraId,
                                         AccountId = a.ToAccountId,
                                         Notes = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         AddedOn = a.PaymentDate,
                                         Type = a.Type,
                                         ReferenceNo = a.ReferenceNo,
                                         Debit = a.Amount,
                                         Credit = 0
                                     }).ToList();
                #endregion

                #region Journals
                Journals = (from a in oConnectionContext.DbClsJournalPayment
                            join b in oConnectionContext.DbClsJournal
                         on a.JournalId equals b.JournalId
                            where
                            //a.AccountId == obj.AccountId && 
                            a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                            && b.IsDeleted == false && b.IsActive == true
                           && b.BranchId == obj.BranchId
              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                            select new ClsBankPaymentVm
                            {
                                Id = a.JournalId,
                                AccountId = a.AccountId,
                                Notes = b.Notes,
                                AddedOn = b.Date,
                                Type = "Journal",
                                ReferenceNo = b.ReferenceNo,
                                Debit = a.Debit,
                                Credit = a.Credit
                            }).ToList();
                #endregion

                #region Sales

                var SalesTaxList = (from q in oConnectionContext.DbClsSalesTaxJournal
                                    join a in oConnectionContext.DbClsSalesDetails
                                    on q.SalesDetailsId equals a.SalesDetailsId
                                    join b in oConnectionContext.DbClsSales
                                 on a.SalesId equals b.SalesId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                                   && b.BranchId == obj.BranchId
                                    && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                        //&& a.TaxAmount != 0 
                                        && b.Status != "Draft"
                                    select new
                                    {
                                        Id = b.SalesId,
                                        CustomerId = b.CustomerId,
                                        AccountId = q.AccountId,
                                        Notes = "",
                                        AddedOn = b.SalesDate,
                                        Type = "Sales",
                                        ReferenceNo = b.InvoiceNo,
                                        Debit = 0,
                                        Credit = a.TaxAmount,
                                    }).Concat(from q in oConnectionContext.DbClsSalesTaxJournal
                                              join b in oConnectionContext.DbClsSales
                                           on q.SalesId equals b.SalesId
                                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                                                                             && b.BranchId == obj.BranchId
                                              && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                                  DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                                  //&& a.TaxAmount != 0 
                                                  && b.Status != "Draft"
                                              select new
                                              {
                                                  Id = b.SalesId,
                                                  CustomerId = b.CustomerId,
                                                  AccountId = q.AccountId,
                                                  Notes = "",
                                                  AddedOn = b.SalesDate,
                                                  Type = "Sales",
                                                  ReferenceNo = b.InvoiceNo,
                                                  Debit = 0,
                                                  Credit = b.TaxAmount,
                                              }).ToList();

                Sales = (from a in oConnectionContext.DbClsSales
                             //   join b in oConnectionContext.DbClsSalesDetails
                             //on a.SalesId equals b.SalesId
                         where
                         //a.AccountId == obj.AccountId && 
                         a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                         //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                         && a.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
               && a.GrandTotal != 0 && a.Status != "Draft"
                         select new ClsBankPaymentVm
                         {
                             //Account Receivable
                             Id = a.SalesId,
                             CustomerId = a.CustomerId,
                             AccountId = a.AccountId,
                             Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                             AddedOn = a.SalesDate,
                             Type = "Sales",
                             ReferenceNo = a.InvoiceNo,
                             Debit = a.GrandTotal,
                             Credit = 0
                         }).Concat(from a in oConnectionContext.DbClsSalesDetails
                                   join b in oConnectionContext.DbClsSales
                                on a.SalesId equals b.SalesId
                                   where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                  && b.BranchId == obj.BranchId
                                  && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                      && a.UnitCost != 0 && b.Status != "Draft"
                                   group a by new { a.AccountId, a.SalesId } into stdGroup
                                   select new ClsBankPaymentVm
                                   {
                                       // sales account
                                       Id = stdGroup.Key.SalesId,
                                       CustomerId = oConnectionContext.DbClsSales.Where(x => x.SalesId == stdGroup.Key.SalesId).Select(x => x.CustomerId).FirstOrDefault(),
                                       AccountId = stdGroup.Key.AccountId,
                                       Notes = "",
                                       AddedOn = oConnectionContext.DbClsSales.Where(x => x.SalesId == stdGroup.Key.SalesId).Select(x => x.SalesDate).FirstOrDefault(),
                                       Type = "Sales",
                                       ReferenceNo = oConnectionContext.DbClsSales.Where(x => x.SalesId == stdGroup.Key.SalesId).Select(x => x.InvoiceNo).FirstOrDefault(),
                                       Debit = 0,
                                       Credit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum()
                                   })
                         .Concat(from b in oConnectionContext.DbClsSalesAdditionalCharges
                                 join a in oConnectionContext.DbClsSales
                                 on b.SalesId equals a.SalesId
                                 where
                                 //a.ShippingChargesAccountId == obj.AccountId && 
                                 a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                 && b.IsDeleted == false && b.IsActive == true
                                 && a.BranchId == obj.BranchId
                   && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                       && b.AmountExcTax != 0 && a.Status != "Draft"
                                 select new ClsBankPaymentVm
                                 {
                                     // shipping charge
                                     Id = a.SalesId,
                                     CustomerId = a.CustomerId,
                                     AccountId = b.AccountId,
                                     Notes = "",
                                     AddedOn = a.SalesDate,
                                     Type = "Sales",
                                     ReferenceNo = a.InvoiceNo,
                                     Debit = 0,
                                     Credit = b.AmountExcTax,
                                 }).Concat(from a in oConnectionContext.DbClsSales
                                           where
                                           //a.RoundOffAccountId == obj.AccountId && 
                                           a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                           && a.BranchId == obj.BranchId
                             && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                                 && a.RoundOff != 0 && a.Status != "Draft"
                                           select new ClsBankPaymentVm
                                           {
                                               // Round off charge
                                               Id = a.SalesId,
                                               CustomerId = a.CustomerId,
                                               AccountId = a.RoundOffAccountId,
                                               Notes = "",
                                               AddedOn = a.SalesDate,
                                               Type = "Sales",
                                               ReferenceNo = a.InvoiceNo,
                                               Debit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                               Credit = a.RoundOff > 0 ? a.RoundOff : 0,
                                           }).Concat(from a in oConnectionContext.DbClsSales
                                                     where
                                                     //a.DiscountAccountId == obj.AccountId && 
                                                     a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                     && a.BranchId == obj.BranchId
                                       && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                                           && a.Discount != 0 && a.Status != "Draft"
                                                     select new ClsBankPaymentVm
                                                     {
                                                         // discount 
                                                         Id = a.SalesId,
                                                         CustomerId = a.CustomerId,
                                                         AccountId = a.DiscountAccountId,
                                                         Notes = "",
                                                         AddedOn = a.SalesDate,
                                                         Type = "Sales",
                                                         ReferenceNo = a.InvoiceNo,
                                                         Debit = a.TotalDiscount,
                                                         Credit = 0,
                                                     }).Concat(from a in oConnectionContext.DbClsSales
                                                               where
                                                               //a.DiscountAccountId == obj.AccountId && 
                                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                               && a.BranchId == obj.BranchId
                                                 && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                                                     DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                                                     && a.Discount != 0 && a.Status != "Draft"
                                                               select new ClsBankPaymentVm
                                                               {
                                                                   // special discount 
                                                                   Id = a.SalesId,
                                                                   CustomerId = a.CustomerId,
                                                                   AccountId = a.SpecialDiscountAccountId,
                                                                   Notes = "",
                                                                   AddedOn = a.SalesDate,
                                                                   Type = "Sales",
                                                                   ReferenceNo = a.InvoiceNo,
                                                                   Debit = a.SpecialDiscount,
                                                                   Credit = 0,
                                                               }).ToList();

                Sales = Sales.Concat(from a in SalesTaxList
                                     select new ClsBankPaymentVm
                                     {
                                         // tax 
                                         Id = a.Id,
                                         CustomerId = a.CustomerId,
                                         AccountId = a.AccountId,
                                         Notes = "",
                                         AddedOn = a.AddedOn,
                                         Type = "Sales",
                                         ReferenceNo = a.ReferenceNo,
                                         Debit = a.Debit,
                                         Credit = a.Credit,
                                     }).ToList();

                #endregion

                #region Sales Return

                var AllSalesReturnTaxs = (from a in oConnectionContext.DbClsSalesReturn
                                          join p in oConnectionContext.DbClsSales
                                          on a.SalesId equals p.SalesId
                                          where a.CompanyId == obj.CompanyId
                                          && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                          //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                          && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                           && p.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
           DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                          //&& a.TaxAmount != 0
                                          select new
                                          {
                                              IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                              a.TaxId,
                                              AmountExcTax = a.Subtotal - a.TotalDiscount,
                                              a.SalesReturnId,
                                              a.Date,
                                              a.InvoiceNo,
                                              a.CustomerId
                                          }).Concat(from a in oConnectionContext.DbClsSalesReturnDetails
                                                    join b in oConnectionContext.DbClsSalesReturn
                                                 on a.SalesReturnId equals b.SalesReturnId
                                                    join p in oConnectionContext.DbClsSales
                                                    on b.SalesId equals p.SalesId
                                                    where a.CompanyId == obj.CompanyId
                                         && a.IsDeleted == false && a.IsActive == true
                                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                         && p.BranchId == obj.BranchId
                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                    //&& a.TaxAmount != 0
                                                    select new
                                                    {
                                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                                        a.TaxId,
                                                        AmountExcTax = a.AmountExcTax,
                                                        b.SalesReturnId,
                                                        b.Date,
                                                        b.InvoiceNo,
                                                        b.CustomerId
                                                    }).ToList();

                List<ClsTaxVm> oClsSalesReturnTaxVm = new List<ClsTaxVm>();
                foreach (var item in AllSalesReturnTaxs)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesReturnTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax,
                            Id = item.SalesReturnId,
                            AddedOn = item.Date,
                            ReferenceNo = item.InvoiceNo,
                            CustomerId = item.CustomerId
                        });
                    }
                }

                var finalSalesReturnTaxs = oClsSalesReturnTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new
                         {
                             TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum(),
                             Id = c.Select(cs => cs.Id).FirstOrDefault(),
                             Date = c.Select(cs => cs.AddedOn).FirstOrDefault(),
                             ReferenceNo = c.Select(cs => cs.ReferenceNo).FirstOrDefault(),
                             CustomerId = c.Select(cs => cs.CustomerId).FirstOrDefault(),
                         }
                        ).ToList();

                var SalesReturnTaxList = finalSalesReturnTaxs.Select(a => new ClsBankPaymentVm
                {
                    Id = a.Id,
                    CustomerId = a.CustomerId,
                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = a.Date,
                    Type = "Sales Return",
                    ReferenceNo = a.ReferenceNo,
                    Debit = a.TaxAmount,
                    Credit = 0,
                }).ToList();

                SalesReturn = (from a in oConnectionContext.DbClsSalesReturn
                               join p in oConnectionContext.DbClsSales
                                  on a.SalesId equals p.SalesId
                               where
                               //a.AccountId == obj.AccountId && 
                               a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                               //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                               && p.IsActive == true && p.IsDeleted == false && p.IsCancelled == false
                               && p.BranchId == obj.BranchId
                 && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                     && a.GrandTotal != 0
                               select new ClsBankPaymentVm
                               {
                                   //Account Receivable
                                   Id = a.SalesReturnId,
                                   CustomerId = p.CustomerId,
                                   AccountId = a.AccountId,
                                   Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == p.CustomerId).Select(c => c.Name).FirstOrDefault(),
                                   AddedOn = a.Date,
                                   Type = "Sales Return",
                                   ReferenceNo = a.InvoiceNo,
                                   Debit = 0,
                                   Credit = a.GrandTotal
                               }).Concat(from a in oConnectionContext.DbClsSalesReturnDetails
                                         join b in oConnectionContext.DbClsSalesReturn
                                      on a.SalesReturnId equals b.SalesReturnId
                                         join p in oConnectionContext.DbClsSales
                                                      on b.SalesId equals p.SalesId
                                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                         && p.BranchId == obj.BranchId
                                        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                            && a.UnitCost != 0
                                         group a by new { a.AccountId, b.SalesReturnId } into stdGroup
                                         select new ClsBankPaymentVm
                                         {
                                             // sales account
                                             Id = stdGroup.Key.SalesReturnId,
                                             CustomerId = oConnectionContext.DbClsSalesReturn.Where(x => x.SalesReturnId == stdGroup.Key.SalesReturnId).Select(x => x.CustomerId).FirstOrDefault(),
                                             AccountId = stdGroup.Key.AccountId,
                                             Notes = "",
                                             AddedOn = oConnectionContext.DbClsSalesReturn.Where(x => x.IsDeleted == false && x.IsCancelled == false && x.SalesReturnId == stdGroup.Key.SalesReturnId).Select(x => x.Date).FirstOrDefault(),
                                             Type = "Sales Return",
                                             ReferenceNo = oConnectionContext.DbClsSalesReturn.Where(x => x.IsDeleted == false && x.IsCancelled == false && x.SalesReturnId == stdGroup.Key.SalesReturnId).Select(x => x.InvoiceNo).FirstOrDefault(),
                                             Debit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                             Credit = 0
                                         })
                               .Concat(from b in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                       join a in oConnectionContext.DbClsSalesReturn
                                       on b.SalesReturnId equals a.SalesReturnId
                                       join p in oConnectionContext.DbClsSales
                                       on a.SalesId equals p.SalesId
                                       where
                                       //a.OtherChargesAccountId == obj.AccountId && 
                                       a.CompanyId == obj.CompanyId
                                       && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                       && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                       && b.IsDeleted == false && b.IsActive == true
                                       && p.BranchId == obj.BranchId
                         && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                             DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                             && b.AmountExcTax != 0
                                       select new ClsBankPaymentVm
                                       {
                                           // other charge
                                           Id = a.SalesReturnId,
                                           CustomerId = p.CustomerId,
                                           AccountId = b.AccountId,
                                           Notes = "",
                                           AddedOn = a.Date,
                                           Type = "Sales Return",
                                           ReferenceNo = a.InvoiceNo,
                                           Debit = b.AmountExcTax,
                                           Credit = 0,
                                       }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                 join p in oConnectionContext.DbClsSales
                                        on a.SalesId equals p.SalesId
                                                 where
                                                 //a.RoundOffAccountId == obj.AccountId && 
                                                 a.CompanyId == obj.CompanyId
                                                 && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                 && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                 && p.BranchId == obj.BranchId
                                   && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                       DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                       && a.RoundOff != 0
                                                 select new ClsBankPaymentVm
                                                 {
                                                     // Round off charge
                                                     Id = a.SalesReturnId,
                                                     CustomerId = p.CustomerId,
                                                     AccountId = a.RoundOffAccountId,
                                                     Notes = "",
                                                     AddedOn = a.Date,
                                                     Type = "Sales Return",
                                                     ReferenceNo = a.InvoiceNo,
                                                     Debit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                     Credit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                                 }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                           join p in oConnectionContext.DbClsSales
                                                           on a.SalesId equals p.SalesId
                                                           where
                                                           //a.DiscountAccountId == obj.AccountId && 
                                                           a.CompanyId == obj.CompanyId
                                                           && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                           && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                           && p.BranchId == obj.BranchId
                                             && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                 && a.Discount != 0
                                                           select new ClsBankPaymentVm
                                                           {
                                                               // discount 
                                                               Id = a.SalesReturnId,
                                                               CustomerId = p.CustomerId,
                                                               AccountId = a.DiscountAccountId,
                                                               Notes = "",
                                                               AddedOn = a.Date,
                                                               Type = "Sales Return",
                                                               ReferenceNo = a.InvoiceNo,
                                                               Debit = 0,
                                                               Credit = a.TotalDiscount,
                                                           }).Concat(from a in oConnectionContext.DbClsSalesReturn
                                                                     join p in oConnectionContext.DbClsSales
                                                                     on a.SalesId equals p.SalesId
                                                                     where
                                                                     //a.DiscountAccountId == obj.AccountId && 
                                                                     a.CompanyId == obj.CompanyId
                                                                     && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                     && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                                                     && p.BranchId == obj.BranchId
                                                       && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                           DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                           && a.Discount != 0
                                                                     select new ClsBankPaymentVm
                                                                     {
                                                                         // special discount 
                                                                         Id = a.SalesReturnId,
                                                                         CustomerId = p.CustomerId,
                                                                         AccountId = a.SpecialDiscountAccountId,
                                                                         Notes = "",
                                                                         AddedOn = a.Date,
                                                                         Type = "Sales Return",
                                                                         ReferenceNo = a.InvoiceNo,
                                                                         Debit = 0,
                                                                         Credit = a.SpecialDiscount,
                                                                     }).ToList();

                SalesReturn = SalesReturn.Concat(from a in SalesReturnTaxList
                                                 select new ClsBankPaymentVm
                                                 {
                                                     // tax 
                                                     Id = a.Id,
                                                     CustomerId = a.CustomerId,
                                                     AccountId = a.AccountId,
                                                     Notes = "",
                                                     AddedOn = a.AddedOn,
                                                     Type = "Sales Return",
                                                     ReferenceNo = a.ReferenceNo,
                                                     Debit = a.Debit,
                                                     Credit = 0
                                                 }).ToList();

                #endregion

                #region Purchase

                var PurchaseTaxList = (from q in oConnectionContext.DbClsPurchaseTaxJournal
                                       join a in oConnectionContext.DbClsPurchaseDetails
                                       on q.PurchaseDetailsId equals a.PurchaseDetailsId
                                       join b in oConnectionContext.DbClsPurchase
                                    on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                                      //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                                      && b.BranchId == obj.BranchId
                                       && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                           //&& a.TaxAmount != 0 
                                           && b.Status != "Draft"
                                       select new
                                       {
                                           Id = b.PurchaseId,
                                           SupplierId = b.SupplierId,
                                           AccountId = q.AccountId,
                                           Notes = "",
                                           AddedOn = b.PurchaseDate,
                                           Type = "Purchase",
                                           ReferenceNo = b.ReferenceNo,
                                           Debit = (q.PurchaseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                           Credit = (q.PurchaseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                       }).Concat(from q in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                 join a in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                 on q.PurchaseAdditionalChargesId equals a.PurchaseAdditionalChargesId
                                                 join b in oConnectionContext.DbClsPurchase
                                              on a.PurchaseId equals b.PurchaseId
                                                 //join c in oConnectionContext.DbClsTax
                                                 //  on q.TaxId equals c.TaxId
                                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                                                                                //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                                                && b.BranchId == obj.BranchId
                                                 && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                                     //&& a.TaxAmount != 0 
                                                     && b.Status != "Draft"
                                                 select new
                                                 {
                                                     Id = b.PurchaseId,
                                                     SupplierId = b.SupplierId,
                                                     AccountId = q.AccountId,
                                                     Notes = "",
                                                     AddedOn = b.PurchaseDate,
                                                     Type = "Purchase",
                                                     ReferenceNo = b.ReferenceNo,
                                                     Debit = (q.PurchaseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                                     Credit = (q.PurchaseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                                 }).ToList();

                var purchaseAccount = (from a in oConnectionContext.DbClsPurchaseDetails
                                       join b in oConnectionContext.DbClsPurchase
                                    on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                                       //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                                       && b.BranchId == obj.BranchId
                                        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                            DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                            //&& a.TaxAmount != 0 
                                            && b.Status != "Draft"
                                       select new ClsBankPaymentVm
                                       {
                                           // Purchase account
                                           Id = b.PurchaseId,
                                           SupplierId = b.SupplierId,
                                           AccountId = a.AccountId,
                                           Notes = "",
                                           AddedOn = b.PurchaseDate,
                                           Type = "Purchase",
                                           ReferenceNo = b.ReferenceNo,
                                           Debit = b.IsReverseCharge == 1 ? a.AmountIncTax : a.AmountExcTax,
                                           Credit = 0,
                                       }).ToList();

                Purchase = (from a in oConnectionContext.DbClsPurchase
                            where
                            //a.AccountId == obj.AccountId && 
                            a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                         //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                         && a.BranchId == obj.BranchId
           && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
               && a.GrandTotal != 0 && a.Status != "Draft"
                            select new ClsBankPaymentVm
                            {
                                //Account Payable
                                Id = a.PurchaseId,
                                SupplierId = a.SupplierId,
                                AccountId = a.AccountId,
                                Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                                AddedOn = a.PurchaseDate,
                                Type = "Purchase",
                                ReferenceNo = a.ReferenceNo,
                                Debit = 0,
                                Credit = a.GrandTotal
                            })
                            .Concat(from b in oConnectionContext.DbClsPurchaseAdditionalCharges
                                    join a in oConnectionContext.DbClsPurchase
                                    on b.PurchaseId equals a.PurchaseId
                                    where
                                    //a.ShippingChargesAccountId == obj.AccountId && 
                                    a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                    && b.IsDeleted == false && b.IsActive == true
                                    && a.BranchId == obj.BranchId
                      && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                          DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                          && b.AmountExcTax != 0 && a.Status != "Draft"
                                    select new ClsBankPaymentVm
                                    {
                                        // additional charges
                                        Id = a.PurchaseId,
                                        SupplierId = a.SupplierId,
                                        AccountId = b.AccountId,
                                        Notes = "",
                                        AddedOn = a.PurchaseDate,
                                        Type = "Purchase",
                                        ReferenceNo = a.ReferenceNo,
                                        Debit = b.AmountExcTax,
                                        Credit = 0,
                                    }).Concat(from a in oConnectionContext.DbClsPurchase
                                              where
                                              //a.RoundOffAccountId == obj.AccountId && 
                                              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                              && a.BranchId == obj.BranchId
                                && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                                    DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                                    && a.RoundOff != 0 && a.Status != "Draft"
                                              select new ClsBankPaymentVm
                                              {
                                                  // Round off charge
                                                  Id = a.PurchaseId,
                                                  SupplierId = a.SupplierId,
                                                  AccountId = a.RoundOffAccountId,
                                                  Notes = "",
                                                  AddedOn = a.PurchaseDate,
                                                  Type = "Purchase",
                                                  ReferenceNo = a.ReferenceNo,
                                                  Debit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                  Credit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0
                                              }).Concat(from a in oConnectionContext.DbClsPurchase
                                                        where
                                                        //a.DiscountAccountId == obj.AccountId && 
                                                        a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                        && a.BranchId == obj.BranchId
                                          && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                                              DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                                              && a.Discount != 0 && a.Status != "Draft"
                                                        select new ClsBankPaymentVm
                                                        {
                                                            // discount 
                                                            Id = a.PurchaseId,
                                                            SupplierId = a.SupplierId,
                                                            AccountId = a.DiscountAccountId,
                                                            Notes = "",
                                                            AddedOn = a.PurchaseDate,
                                                            Type = "Purchase",
                                                            ReferenceNo = a.ReferenceNo,
                                                            Debit = 0,
                                                            Credit = a.TotalDiscount,
                                                        }).Concat(from a in oConnectionContext.DbClsPurchase
                                                                  where
                                                                  //a.DiscountAccountId == obj.AccountId && 
                                                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                  && a.BranchId == obj.BranchId
                                                    && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                                                        DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
                                                        && a.Discount != 0 && a.Status != "Draft"
                                                                  select new ClsBankPaymentVm
                                                                  {
                                                                      // special discount 
                                                                      Id = a.PurchaseId,
                                                                      SupplierId = a.SupplierId,
                                                                      AccountId = a.SpecialDiscountAccountId,
                                                                      Notes = "",
                                                                      AddedOn = a.PurchaseDate,
                                                                      Type = "Purchase",
                                                                      ReferenceNo = a.ReferenceNo,
                                                                      Debit = 0,
                                                                      Credit = a.SpecialDiscount,
                                                                  }).ToList();

                Purchase = Purchase.Concat(from a in purchaseAccount
                                           group a by new { a.AccountId, a.Id } into stdGroup
                                           //orderby stdgroup.key descending
                                           select new ClsBankPaymentVm
                                           {
                                               Id = stdGroup.Key.Id,
                                               SupplierId = stdGroup.Select(x => x.SupplierId).DefaultIfEmpty().FirstOrDefault(),
                                               AccountId = stdGroup.Key.AccountId,
                                               Notes = stdGroup.Select(x => x.Notes).DefaultIfEmpty().FirstOrDefault(),
                                               AddedOn = stdGroup.Select(x => x.AddedOn).DefaultIfEmpty().FirstOrDefault(),
                                               Type = "Purchase",
                                               ReferenceNo = stdGroup.Select(x => x.ReferenceNo).DefaultIfEmpty().FirstOrDefault(),
                                               Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                               Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                           }).Concat(from a in PurchaseTaxList
                                                     select new ClsBankPaymentVm
                                                     {
                                                         // tax 
                                                         Id = a.Id,
                                                         SupplierId = a.SupplierId,
                                                         AccountId = a.AccountId,
                                                         Notes = "",
                                                         AddedOn = a.AddedOn,
                                                         Type = "Purchase",
                                                         ReferenceNo = a.ReferenceNo,
                                                         Debit = a.Debit,
                                                         Credit = a.Credit
                                                     }).ToList();

                #endregion

                #region Purchase Return

                var AllPurchaseReturnTaxs = (from a in oConnectionContext.DbClsPurchaseReturn
                                             where a.CompanyId == obj.CompanyId
                                             && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                             //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                            && a.BranchId == obj.BranchId
                        && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                             //&& a.TaxAmount != 0
                                             select new
                                             {
                                                 IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                                 a.TaxId,
                                                 AmountExcTax = a.Subtotal - a.TotalDiscount,
                                                 a.PurchaseReturnId,
                                                 a.Date,
                                                 a.InvoiceNo,
                                                 a.SupplierId
                                             }).Concat(from a in oConnectionContext.DbClsPurchaseReturnDetails
                                                       join b in oConnectionContext.DbClsPurchaseReturn
                                                    on a.PurchaseReturnId equals b.PurchaseReturnId
                                                       where a.CompanyId == obj.CompanyId
                                                       && a.IsDeleted == false && a.IsActive == true
                                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                     && b.BranchId == obj.BranchId
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                       //&& a.TaxAmount != 0
                                                       select new
                                                       {
                                                           IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                                           a.TaxId,
                                                           AmountExcTax = a.AmountExcTax,
                                                           b.PurchaseReturnId,
                                                           b.Date,
                                                           b.InvoiceNo,
                                                           b.SupplierId
                                                       }).ToList();

                List<ClsTaxVm> oClsPurchaseReturnTaxVm = new List<ClsTaxVm>();
                foreach (var item in AllPurchaseReturnTaxs)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsPurchaseReturnTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax,
                            Id = item.PurchaseReturnId,
                            AddedOn = item.Date,
                            ReferenceNo = item.InvoiceNo,
                            SupplierId = item.SupplierId
                        });
                    }
                }

                var finalPurchaseReturnTaxs = oClsPurchaseReturnTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new
                         {
                             TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum(),
                             Id = c.Select(cs => cs.Id).FirstOrDefault(),
                             Date = c.Select(cs => cs.AddedOn).FirstOrDefault(),
                             ReferenceNo = c.Select(cs => cs.ReferenceNo).FirstOrDefault(),
                             SupplierId = c.Select(cs => cs.SupplierId).FirstOrDefault(),
                         }
                        ).ToList();

                var PurchaseReturnTaxList = finalPurchaseReturnTaxs.Select(a => new ClsBankPaymentVm
                {
                    Id = a.Id,
                    SupplierId = a.SupplierId,
                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = a.Date,
                    Type = "Purchase Return",
                    ReferenceNo = a.ReferenceNo,
                    Debit = 0,
                    Credit = a.TaxAmount,
                }).ToList();

                PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                  where
                                  //a.AccountId == obj.AccountId && 
                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                  //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                 && a.BranchId == obj.BranchId
                    && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                        && a.GrandTotal != 0
                                  select new ClsBankPaymentVm
                                  {
                                      //Account Payable
                                      Id = a.PurchaseReturnId,
                                      SupplierId = a.SupplierId,
                                      AccountId = a.AccountId,
                                      Notes = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                                      AddedOn = a.Date,
                                      Type = "Purchase Return",
                                      ReferenceNo = a.InvoiceNo,
                                      Debit = a.GrandTotal,
                                      Credit = 0
                                  }).Concat
                                  (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                   join b in oConnectionContext.DbClsPurchaseReturn
                                on a.PurchaseReturnId equals b.PurchaseReturnId
                                   where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                   && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   && b.BranchId == obj.BranchId
                                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      && a.UnitCost != 0
                                   group a by new { a.AccountId, a.PurchaseReturnId } into stdGroup
                                   select new ClsBankPaymentVm
                                   {
                                       // Purchase account
                                       Id = stdGroup.Key.PurchaseReturnId,
                                       SupplierId = oConnectionContext.DbClsPurchaseReturn.Where(x => x.PurchaseReturnId == stdGroup.Key.PurchaseReturnId).Select(x => x.SupplierId).FirstOrDefault(),
                                       AccountId = stdGroup.Key.AccountId,
                                       Notes = "",
                                       AddedOn = oConnectionContext.DbClsPurchaseReturn.Where(x => x.PurchaseReturnId == stdGroup.Key.PurchaseReturnId).Select(x => x.Date).FirstOrDefault(),
                                       Type = "Purchase Return",
                                       ReferenceNo = oConnectionContext.DbClsPurchaseReturn.Where(x => x.PurchaseReturnId == stdGroup.Key.PurchaseReturnId).Select(x => x.InvoiceNo).FirstOrDefault(),
                                       Debit = 0,
                                       Credit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                   })
                                  .Concat(from b in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                          join a in oConnectionContext.DbClsPurchaseReturn
                                          on b.PurchaseReturnId equals a.PurchaseReturnId
                                          where
                                          //a.OtherChargesAccountId == obj.AccountId && 
                                          a.CompanyId == obj.CompanyId
                                          && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                          && b.IsDeleted == false && b.IsActive == true
                                         && a.BranchId == obj.BranchId
                            && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                && b.AmountExcTax != 0
                                          select new ClsBankPaymentVm
                                          {
                                              // other charge
                                              Id = a.PurchaseReturnId,
                                              SupplierId = a.SupplierId,
                                              AccountId = b.AccountId,
                                              Notes = "",
                                              AddedOn = a.Date,
                                              Type = "Purchase Return",
                                              ReferenceNo = a.InvoiceNo,
                                              Debit = 0,
                                              Credit = b.AmountExcTax,
                                          }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                    where
                                                    //a.RoundOffAccountId == obj.AccountId && 
                                                    a.CompanyId == obj.CompanyId
                                                    && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                    && a.BranchId == obj.BranchId
                                      && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                          DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                          && a.RoundOff != 0
                                                    select new ClsBankPaymentVm
                                                    {
                                                        // Round off charge
                                                        Id = a.PurchaseReturnId,
                                                        SupplierId = a.SupplierId,
                                                        AccountId = a.RoundOffAccountId,
                                                        Notes = "",
                                                        AddedOn = a.Date,
                                                        Type = "Purchase Return",
                                                        ReferenceNo = a.InvoiceNo,
                                                        Debit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                                        Credit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                    }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                              where
                                                              //a.DiscountAccountId == obj.AccountId && 
                                                              a.CompanyId == obj.CompanyId
                                                              && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                             && a.BranchId == obj.BranchId
                                                && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                    DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                    && a.Discount != 0
                                                              select new ClsBankPaymentVm
                                                              {
                                                                  // discount 
                                                                  Id = a.PurchaseReturnId,
                                                                  SupplierId = a.SupplierId,
                                                                  AccountId = a.DiscountAccountId,
                                                                  Notes = "",
                                                                  AddedOn = a.Date,
                                                                  Type = "Purchase Return",
                                                                  ReferenceNo = a.InvoiceNo,
                                                                  Debit = a.TotalDiscount,
                                                                  Credit = 0,
                                                              }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                                        where
                                                                        //a.DiscountAccountId == obj.AccountId && 
                                                                        a.CompanyId == obj.CompanyId
                                                                        && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                       && a.BranchId == obj.BranchId
                                                          && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                                              DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                                              && a.Discount != 0
                                                                        select new ClsBankPaymentVm
                                                                        {
                                                                            // special discount 
                                                                            Id = a.PurchaseReturnId,
                                                                            SupplierId = a.SupplierId,
                                                                            AccountId = a.SpecialDiscountAccountId,
                                                                            Notes = "",
                                                                            AddedOn = a.Date,
                                                                            Type = "Purchase Return",
                                                                            ReferenceNo = a.InvoiceNo,
                                                                            Debit = a.SpecialDiscount,
                                                                            Credit = 0,
                                                                        }).ToList();

                PurchaseReturn = PurchaseReturn.Concat(from a in PurchaseReturnTaxList
                                                       select new ClsBankPaymentVm
                                                       {
                                                           // tax 
                                                           Id = a.Id,
                                                           SupplierId = a.SupplierId,
                                                           AccountId = a.AccountId,
                                                           Notes = "",
                                                           AddedOn = a.AddedOn,
                                                           Type = "Purchase Return",
                                                           ReferenceNo = a.ReferenceNo,
                                                           Debit = 0,
                                                           Credit = a.Credit
                                                       }).ToList();

                #endregion

                #region Customer Payment
                SalesPayments = (from a in oConnectionContext.DbClsCustomerPayment
                                     //join b in oConnectionContext.DbClsSales
                                     //on a.SalesId equals b.SalesId
                                 where a.ParentId == 0 &&
                                 //a.AccountId == obj.AccountId && 
                                 a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                 //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                 //(a.Type.ToLower() == "sales payment")
                                && a.BranchId == obj.BranchId
                   && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                       && a.Amount != 0
                                 select new ClsBankPaymentVm
                                 {
                                     Id = a.CustomerPaymentId,
                                     CustomerId = a.CustomerId,
                                     AccountId = a.AccountId,
                                     Notes = "",
                                     AddedOn = a.PaymentDate,
                                     Type = a.Type,//"Sales Payment",
                                     ReferenceNo = a.ReferenceNo,
                                     Debit = a.Amount,
                                     Credit = 0
                                 }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                               //join b in oConnectionContext.DbClsSales
                                               //on a.SalesId equals b.SalesId
                                           where a.ParentId == 0 &&
                                           //a.JournalAccountId == obj.AccountId && 
                                           a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                           //(a.Type.ToLower() == "sales payment")
                                                   && a.BranchId == obj.BranchId
                             && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                 && a.Amount != 0
                                           select new ClsBankPaymentVm
                                           {
                                               Id = a.CustomerPaymentId,
                                               CustomerId = a.CustomerId,
                                               AccountId = a.JournalAccountId,
                                               Notes = "",
                                               AddedOn = a.PaymentDate,
                                               Type = a.Type,//"Sales Payment",
                                               ReferenceNo = a.ReferenceNo,
                                               Debit = 0,
                                               Credit = a.Amount
                                           }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                                         //join b in oConnectionContext.DbClsSales
                                                         //on a.SalesId equals b.SalesId
                                                     where a.ParentId != 0 && a.AccountId != 0 &&
                                                     //a.AccountId == obj.AccountId && 
                                                     a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                     //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                     //(a.Type.ToLower() == "sales payment")
                                                     && a.BranchId == obj.BranchId
                                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                           && a.Amount != 0
                                                     select new ClsBankPaymentVm
                                                     {
                                                         Id = a.ParentId,
                                                         CustomerId = a.CustomerId,
                                                         AccountId = a.AccountId,
                                                         Notes = "",
                                                         AddedOn = a.PaymentDate,
                                                         Type = a.Type,//"Sales Payment",
                                                         ReferenceNo = oConnectionContext.DbClsCustomerPayment.Where(x => x.CustomerPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                         Debit = a.Amount,
                                                         Credit = 0
                                                     }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                                                   //join b in oConnectionContext.DbClsSales
                                                                   //on a.SalesId equals b.SalesId
                                                               where a.ParentId != 0 && a.AccountId != 0 &&
                                                               //a.JournalAccountId == obj.AccountId && 
                                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                       //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                                       //(a.Type.ToLower() == "sales payment")
                                                                       && a.BranchId == obj.BranchId
                                                 && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                                     DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                                     && a.Amount != 0
                                                               select new ClsBankPaymentVm
                                                               {
                                                                   Id = a.ParentId,
                                                                   CustomerId = a.CustomerId,
                                                                   AccountId = a.JournalAccountId,
                                                                   Notes = "",
                                                                   AddedOn = a.PaymentDate,
                                                                   Type = a.Type,//"Sales Payment",
                                                                   ReferenceNo = oConnectionContext.DbClsCustomerPayment.Where(x => x.CustomerPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                                   Debit = 0,
                                                                   Credit = a.Amount
                                                               }).ToList();
                #endregion

                #region Supplier Payment
                PurchasePayments = (from a in oConnectionContext.DbClsSupplierPayment
                                        //   join b in oConnectionContext.DbClsPurchase
                                        //on a.PurchaseId equals b.PurchaseId
                                    where a.ParentId == 0 &&
                                    //a.JournalAccountId == obj.AccountId && 
                                    a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                 //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                 //a.Type.ToLower() == "purchase payment"
                                 && a.BranchId == obj.BranchId
                   && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                       && a.Amount != 0
                                    select new ClsBankPaymentVm
                                    {
                                        Id = a.SupplierPaymentId,
                                        SupplierId = a.SupplierId,
                                        AccountId = a.AccountId,
                                        Notes = "",
                                        AddedOn = a.PaymentDate,
                                        Type = a.Type,//"Purchase Payment",
                                        ReferenceNo = a.ReferenceNo,
                                        Debit = 0,
                                        Credit = a.Amount
                                    }).Concat(from a in oConnectionContext.DbClsSupplierPayment
                                                  //join b in oConnectionContext.DbClsPurchase
                                                  //on a.PurchaseId equals b.PurchaseId
                                              where a.ParentId == 0 &&
                                              //a.AccountId == obj.AccountId && 
                                              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                              //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                              //a.Type.ToLower() == "purchase payment"
                                                       && a.BranchId == obj.BranchId
                                && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                    DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                    && a.Amount != 0
                                              select new ClsBankPaymentVm
                                              {
                                                  Id = a.SupplierPaymentId,
                                                  SupplierId = a.SupplierId,
                                                  AccountId = a.JournalAccountId,
                                                  Notes = "",
                                                  AddedOn = a.PaymentDate,
                                                  Type = a.Type,//"Purchase Payment",
                                                  ReferenceNo = a.ReferenceNo,
                                                  Debit = a.Amount,
                                                  Credit = 0
                                              }).Concat(from a in oConnectionContext.DbClsSupplierPayment
                                                            //   join b in oConnectionContext.DbClsPurchase
                                                            //on a.PurchaseId equals b.PurchaseId
                                                        where a.ParentId != 0 && a.AccountId != 0 &&
                                                        //a.JournalAccountId == obj.AccountId && 
                                                        a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                    //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                    //a.Type.ToLower() == "purchase payment"
                                                    && a.BranchId == obj.BranchId
                                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                           && a.Amount != 0
                                                        select new ClsBankPaymentVm
                                                        {
                                                            Id = a.ParentId,
                                                            SupplierId = a.SupplierId,
                                                            AccountId = a.AccountId,
                                                            Notes = "",
                                                            AddedOn = a.PaymentDate,
                                                            Type = a.Type,//"Purchase Payment",
                                                            ReferenceNo = oConnectionContext.DbClsSupplierPayment.Where(x => x.SupplierPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                            Debit = 0,
                                                            Credit = a.Amount
                                                        }).Concat(from a in oConnectionContext.DbClsSupplierPayment
                                                                      //join b in oConnectionContext.DbClsPurchase
                                                                      //on a.PurchaseId equals b.PurchaseId
                                                                  where a.ParentId != 0 && a.AccountId != 0 &&
                                                                  //a.AccountId == obj.AccountId && 
                                                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                          //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true &&
                                                                          //a.Type.ToLower() == "purchase payment"
                                                                          && a.BranchId == obj.BranchId
                                                    && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                                        DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                                        && a.Amount != 0
                                                                  select new ClsBankPaymentVm
                                                                  {
                                                                      Id = a.ParentId,
                                                                      SupplierId = a.SupplierId,
                                                                      AccountId = a.JournalAccountId,
                                                                      Notes = "",
                                                                      AddedOn = a.PaymentDate,
                                                                      Type = a.Type,//"Purchase Payment",
                                                                      ReferenceNo = oConnectionContext.DbClsSupplierPayment.Where(x => x.SupplierPaymentId == a.ParentId).Select(x => x.ReferenceNo).FirstOrDefault(),
                                                                      Debit = a.Amount,
                                                                      Credit = 0
                                                                  }).ToList();
                #endregion

                #region Stock Adjustment

                var allStockAdjust = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                                      join b in oConnectionContext.DbClsStockAdjustment
                                   on a.StockAdjustmentId equals b.StockAdjustmentId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && b.IsDeleted == false && b.IsActive == true
                                       && b.BranchId == obj.BranchId
                                  && DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                      && a.Amount != 0
                                      select new ClsBankPaymentVm
                                      {
                                          //AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                          Id = a.StockAdjustmentId,
                                          AccountId = a.AccountId,
                                          Notes = "",
                                          AddedOn = b.AdjustmentDate,
                                          Type = "Stock Adjustment",
                                          ReferenceNo = b.ReferenceNo,
                                          Debit = b.AdjustmentType == "debit" ? 0 : a.UnitCost * a.Quantity,
                                          Credit = b.AdjustmentType == "credit" ? 0 : a.UnitCost * a.Quantity
                                      }).ToList();

                StockAdjustment = (from a in oConnectionContext.DbClsStockAdjustment
                                       //   join b in oConnectionContext.DbClsStockAdjustmentDetails
                                       //on a.StockAdjustmentId equals b.StockAdjustmentId
                                   where
                                   //a.AccountId == obj.AccountId && 
                                   a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                   //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   && a.BranchId == obj.BranchId
                     && DbFunctions.TruncateTime(a.AdjustmentDate) >= obj.FromDate &&
                         DbFunctions.TruncateTime(a.AdjustmentDate) <= obj.ToDate
                         && a.TotalAmount != 0
                                   select new ClsBankPaymentVm
                                   {
                                       Id = a.StockAdjustmentId,
                                       AccountId = a.AccountId,
                                       Notes = "",
                                       AddedOn = a.AdjustmentDate,
                                       Type = "Stock Adjustment",
                                       ReferenceNo = a.ReferenceNo,
                                       Debit = a.AdjustmentType == "debit" ? a.TotalAmount : 0,
                                       Credit = a.AdjustmentType == "credit" ? a.TotalAmount : 0
                                   })
                                   //    .Concat(from a in oConnectionContext.DbClsStockAdjustmentDetails
                                   //              join b in oConnectionContext.DbClsStockAdjustment
                                   //           on a.StockAdjustmentId equals b.StockAdjustmentId
                                   //              where
                                   //              //b.AccountId == obj.AccountId && 
                                   //              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                   //              && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                   //              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                                   //&& DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                   //    DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                   //    && a.Amount != 0
                                   //              select new ClsBankPaymentVm
                                   //              {
                                   //                  Id = a.StockAdjustmentId,
                                   //                  AccountId = b.AccountId,
                                   //                  Notes = "",
                                   //                  AddedOn = b.AdjustmentDate,
                                   //                  Type = "Stock Adjustment",
                                   //                  ReferenceNo = b.ReferenceNo,
                                   //                  Debit = b.AdjustmentType == "debit" ? 0 : a.Amount,
                                   //                  Credit = b.AdjustmentType == "credit" ? 0 : a.Amount
                                   //              })
                                   .ToList();

                StockAdjustment = StockAdjustment.Concat(from a in allStockAdjust
                                                         group a by new { a.AccountId, a.Id } into stdGroup
                                                         select new ClsBankPaymentVm
                                                         {
                                                             // sales account
                                                             Id = stdGroup.Key.Id,
                                                             AccountId = stdGroup.Key.AccountId,
                                                             Notes = "",
                                                             AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                                             Type = "Stock Adjustment",
                                                             ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                                             Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                                             Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                                         }).ToList();

                #endregion

                Ledger = Expenses//.Concat(Incomes)
                    .Concat(Contras).Concat(Journals).Concat(Sales).Concat(SalesReturn).Concat(Purchase).Concat(PurchaseReturn)
                    .Concat(SalesPayments)
                    //.Concat(SalesReturnPayments)
                    .Concat(PurchasePayments)
                    //.Concat(PurchaseReturnPayments)
                    .Concat(StockAdjustment)
                    .ToList();
            }
            return Ledger;
        }

        public List<ClsBankPaymentVm> OpeningBalance(ClsBankPaymentVm obj)
        {
            //obj.FromDate = obj.PreviousFromDate;
            //obj.ToDate = obj.PreviousToDate;

            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> CustomerOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> SupplierOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> InventoryAsset = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalanceAdjustment = new List<ClsBankPaymentVm>();

            decimal TotalOpeningBalanceDebit = 0, TotalOpeningBalanceCredit = 0;

            if (obj.BranchId == 0)
            {
                #region Opening Balance
                //if (Type != "Accounts Receivable" && Type != "Accounts Payable" && Type != "Inventory Asset")
                //{
                OpeningBalance = (from a in oConnectionContext.DbClsAccountOpeningBalance
                                  join b in oConnectionContext.DbClsAccountSettings
                               on a.CompanyId equals b.CompanyId
                                  where
                                  //a.AccountId == obj.AccountId && 
                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsActive == true
                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                    && DbFunctions.TruncateTime(b.MigrationDate) >= obj.FromDate &&
                        DbFunctions.TruncateTime(b.MigrationDate) <= obj.ToDate
                                  select new ClsBankPaymentVm
                                  {
                                      AccountId = a.AccountId,
                                      AccountSubTypeId = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountSubTypeId).FirstOrDefault(),
                                      Notes = "",
                                      AddedOn = b.MigrationDate,
                                      Type = "Opening Balance",
                                      ReferenceNo = "",
                                      Debit = a.Debit,
                                      Credit = a.Credit,
                                  }).ToList();
                //}
                #endregion

                #region Customer Opening Balance
                //if (Type == "Accounts Receivable")
                //{
                //CustomerOpeningBalance.Add(new ClsBankPaymentVm
                //{
                //    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault(),
                //    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                //    Notes = "",
                //    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //    && a.IsActive == true).Select(a => a.MigrationDate.Value).FirstOrDefault(),
                //    Type = "Opening Balance",
                //    ReferenceNo = "",
                //    Debit = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "customer"
                //    && a.CompanyId == obj.CompanyId
                //     && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                //        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => a.OpeningBalance).DefaultIfEmpty().Sum(),
                //    Credit = 0,
                //});

                long AccountsReceivabletId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();
                long AccountsReceivableSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountSubTypeId).FirstOrDefault();

                CustomerOpeningBalance = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "customer"
                    && a.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => new ClsBankPaymentVm
                        {
                            CustomerId = a.UserId,
                            AccountId = AccountsReceivabletId,
                            AccountSubTypeId = AccountsReceivableSubTypeId,
                            Notes = "",
                            AddedOn = a.JoiningDate,
                            Type = "Opening Balance",
                            ReferenceNo = "",
                            Debit = a.OpeningBalance,
                            Credit = 0,
                        }).ToList();
                //}
                #endregion

                #region Supplier Opening Balance
                //if (Type == "Accounts Payable")
                //{
                //SupplierOpeningBalance.Add(new ClsBankPaymentVm
                //{
                //    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault(),
                //    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                //    Notes = "",
                //    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //    && a.IsActive == true).Select(a => a.MigrationDate.Value).FirstOrDefault(),
                //    Type = "Opening Balance",
                //    ReferenceNo = "",
                //    Debit = 0,
                //    Credit = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "supplier"
                //    && a.CompanyId == obj.CompanyId
                //     && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                //        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => a.OpeningBalance).DefaultIfEmpty().Sum(),
                //});

                long AccountsPayableId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();
                long AccountsPayableSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountSubTypeId).FirstOrDefault();

                SupplierOpeningBalance = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "supplier"
                    && a.CompanyId == obj.CompanyId
                     && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => new ClsBankPaymentVm
                        {
                            SupplierId = a.UserId,
                            AccountId = AccountsPayableId,
                            AccountSubTypeId = AccountsPayableSubTypeId,
                            Notes = "",
                            AddedOn = a.JoiningDate,
                            Type = "Opening Balance",
                            ReferenceNo = "",
                            Debit = 0,
                            Credit = a.OpeningBalance,
                        }).ToList();
                //}
                #endregion

                #region Inventory Asset
                //if (Type == "Inventory Asset")
                //{
                InventoryAsset.Add(new ClsBankPaymentVm
                {
                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Inventory Asset").Select(a => a.AccountId).FirstOrDefault(),
                    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Inventory Asset").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                    Type = "Opening Balance",
                    ReferenceNo = "",
                    Debit = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && a.IsActive == true && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                    && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.Date) <= obj.ToDate).Select(a => a.SubTotal).DefaultIfEmpty().Sum(),
                    Credit = 0,
                });
                //}
                #endregion

                #region Opening Balance Adjustments
                //if (Type == "Opening Balance Adjustments")
                //{
                foreach (var item in OpeningBalance)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                foreach (var item in CustomerOpeningBalance)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                foreach (var item in SupplierOpeningBalance)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                foreach (var item in InventoryAsset)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                if (TotalOpeningBalanceDebit < TotalOpeningBalanceCredit)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceCredit - TotalOpeningBalanceDebit;
                    TotalOpeningBalanceCredit = 0;
                }
                else if (TotalOpeningBalanceCredit < TotalOpeningBalanceDebit)
                {
                    TotalOpeningBalanceCredit = TotalOpeningBalanceDebit - TotalOpeningBalanceCredit;
                    TotalOpeningBalanceDebit = 0;
                }
                else
                {
                    TotalOpeningBalanceDebit = 0;
                    TotalOpeningBalanceCredit = 0;
                }

                OpeningBalanceAdjustment.Add(new ClsBankPaymentVm
                {
                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault(),
                    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Opening Balance Adjustments").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                    Type = "Opening Balance",
                    ReferenceNo = "",
                    Debit = TotalOpeningBalanceDebit,
                    Credit = TotalOpeningBalanceCredit,
                });
                //}
                #endregion
            }
            else
            {
                #region Opening Balance
                //if (Type != "Accounts Receivable" && Type != "Accounts Payable" && Type != "Inventory Asset")
                //{
                OpeningBalance = (from a in oConnectionContext.DbClsAccountOpeningBalance
                                  join b in oConnectionContext.DbClsAccountSettings
                               on a.CompanyId equals b.CompanyId
                                  where
                                  //a.AccountId == obj.AccountId && 
                                  a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsActive == true
                                  && a.BranchId == obj.BranchId
                    && DbFunctions.TruncateTime(b.MigrationDate) >= obj.FromDate &&
                        DbFunctions.TruncateTime(b.MigrationDate) <= obj.ToDate
                                  select new ClsBankPaymentVm
                                  {
                                      AccountSubTypeId = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountSubTypeId).FirstOrDefault(),
                                      Notes = "",
                                      AddedOn = b.MigrationDate,
                                      Type = "Opening Balance",
                                      ReferenceNo = "",
                                      Debit = a.Debit,
                                      Credit = a.Credit,
                                  }).ToList();
                //}
                #endregion

                #region Customer Opening Balance
                //if (Type == "Accounts Receivable")
                //{
                //CustomerOpeningBalance.Add(new ClsBankPaymentVm
                //{
                //    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault(),
                //    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                //    Notes = "",
                //    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //    && a.IsActive == true).Select(a => a.MigrationDate.Value).FirstOrDefault(),
                //    Type = "Opening Balance",
                //    ReferenceNo = "",
                //    Debit = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "customer"
                //    && a.CompanyId == obj.CompanyId
                //     && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                //        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => a.OpeningBalance).DefaultIfEmpty().Sum(),
                //    Credit = 0,
                //});

                long AccountsReceivabletId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();
                long AccountsReceivableSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Receivable").Select(a => a.AccountSubTypeId).FirstOrDefault();

                CustomerOpeningBalance = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "customer"
                    && a.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => new ClsBankPaymentVm
                        {
                            CustomerId = a.UserId,
                            AccountId = AccountsReceivabletId,
                            AccountSubTypeId = AccountsReceivableSubTypeId,
                            Notes = "",
                            AddedOn = a.JoiningDate,
                            Type = "Opening Balance",
                            ReferenceNo = "",
                            Debit = a.OpeningBalance,
                            Credit = 0,
                        }).ToList();

                //}
                #endregion

                #region Supplier Opening Balance
                //if (Type == "Accounts Payable")
                //{
                //SupplierOpeningBalance.Add(new ClsBankPaymentVm
                //{
                //    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault(),
                //    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                //    Notes = "",
                //    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //    && a.IsActive == true).Select(a => a.MigrationDate.Value).FirstOrDefault(),
                //    Type = "Opening Balance",
                //    ReferenceNo = "",
                //    Debit = 0,
                //    Credit = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "supplier"
                //    && a.CompanyId == obj.CompanyId
                //     && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                //        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => a.OpeningBalance).DefaultIfEmpty().Sum(),
                //});

                long AccountsPayableId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();
                long AccountsPayableSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Accounts Payable").Select(a => a.AccountSubTypeId).FirstOrDefault();

                SupplierOpeningBalance = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "supplier"
                    && a.CompanyId == obj.CompanyId
                     && DbFunctions.TruncateTime(a.JoiningDate) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.JoiningDate) <= obj.ToDate).Select(a => new ClsBankPaymentVm
                        {
                            SupplierId = a.UserId,
                            AccountId = AccountsPayableId,
                            AccountSubTypeId = AccountsPayableSubTypeId,
                            Notes = "",
                            AddedOn = a.JoiningDate,
                            Type = "Opening Balance",
                            ReferenceNo = "",
                            Debit = 0,
                            Credit = a.OpeningBalance,
                        }).ToList();

                //}
                #endregion

                #region Inventory Asset
                //if (Type == "Inventory Asset")
                //{
                InventoryAsset.Add(new ClsBankPaymentVm
                {
                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Inventory Asset").Select(a => a.AccountId).FirstOrDefault(),
                    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Inventory Asset").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                    Type = "Opening Balance",
                    ReferenceNo = "",
                    Debit = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && a.IsActive == true && a.BranchId == obj.BranchId
                    && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.Date) <= obj.ToDate).Select(a => a.SubTotal).DefaultIfEmpty().Sum(),
                    Credit = 0,
                });
                //}
                #endregion

                #region Opening Balance Adjustments
                //if (Type == "Opening Balance Adjustments")
                //{
                foreach (var item in OpeningBalance)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                foreach (var item in CustomerOpeningBalance)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                foreach (var item in SupplierOpeningBalance)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                foreach (var item in InventoryAsset)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                    TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
                }

                if (TotalOpeningBalanceDebit < TotalOpeningBalanceCredit)
                {
                    TotalOpeningBalanceDebit = TotalOpeningBalanceCredit - TotalOpeningBalanceDebit;
                    TotalOpeningBalanceCredit = 0;
                }
                else if (TotalOpeningBalanceCredit < TotalOpeningBalanceDebit)
                {
                    TotalOpeningBalanceCredit = TotalOpeningBalanceDebit - TotalOpeningBalanceCredit;
                    TotalOpeningBalanceDebit = 0;
                }
                else
                {
                    TotalOpeningBalanceDebit = 0;
                    TotalOpeningBalanceCredit = 0;
                }

                OpeningBalanceAdjustment.Add(new ClsBankPaymentVm
                {
                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault(),
                    AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.Type == "Opening Balance Adjustments").Select(a => a.AccountSubTypeId).FirstOrDefault(),
                    Notes = "",
                    AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                    && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                    Type = "Opening Balance",
                    ReferenceNo = "",
                    Debit = TotalOpeningBalanceDebit,
                    Credit = TotalOpeningBalanceCredit,
                });
                //}
                #endregion
            }

            return OpeningBalance.Union(CustomerOpeningBalance).Union(SupplierOpeningBalance).Union(InventoryAsset).Union(OpeningBalanceAdjustment).ToList();
        }

        public List<ClsPlanAddonsVm> PlanAddons(ClsMenuVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
                a.Name,
                a.ProfilePic
            }).FirstOrDefault();

            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2 && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault();

            List<ClsPlanAddonsVm> PlanAddons = (from aa in oConnectionContext.DbClsTransaction
                                                join bb in oConnectionContext.DbClsTransactionDetails
                          on aa.TransactionId equals bb.TransactionId
                                                where aa.TransactionId == TransactionId && aa.Status == 2
                                                select new ClsPlanAddonsVm { Type = bb.Type }).Union(from aa in oConnectionContext.DbClsTransaction
                                                                                                     join bb in oConnectionContext.DbClsTransactionDetails
                                                                         on aa.TransactionId equals bb.TransactionId
                                                                                                     where aa.ParentTransactionId == TransactionId && aa.Status == 2
                                                                                                     select new ClsPlanAddonsVm { Type = bb.Type }).ToList();
            return PlanAddons;
        }

        public List<ClsBankPaymentVm> TaxTransactions(ClsBankPaymentVm obj)
        {
            List<ClsBankPaymentVm> Sales;
            List<ClsBankPaymentVm> SalesReturn;
            List<ClsBankPaymentVm> SalesPayments;
            List<ClsBankPaymentVm> Purchase;
            List<ClsBankPaymentVm> PurchaseReturn;
            List<ClsBankPaymentVm> Expenses;
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                #region Expenses
                var expenseTaxList = (from a in oConnectionContext.DbClsExpensePayment
                                      join b in oConnectionContext.DbClsExpense
                                          on a.ExpenseId equals b.ExpenseId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                             && b.IsDeleted == false && b.IsActive == true
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      //&& a.TaxAmount != 0
                                      select new ClsBankPaymentVm
                                      {
                                          IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                          Id = a.ExpenseId,
                                          TaxId = a.TaxId,
                                          AddedOn = b.Date,
                                          Type = "Expense",
                                          ReferenceNo = b.ReferenceNo,
                                          TransactionAmount = b.IsReverseCharge == 1 ? a.AmountExcTax : (a.AmountExcTax * -1),
                                          TaxAmount = b.IsReverseCharge == 1 ? (a.Amount - a.AmountExcTax) : ((a.Amount - a.AmountExcTax) * -1)
                                      }).ToList();

                List<ClsBankPaymentVm> oClsExpenseTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in expenseTaxList)
                {
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsExpenseTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                Expenses = (from a in oClsExpenseTaxVm
                            group a by new { a.TaxId, a.Id } into stdGroup
                            //orderby stdgroup.key descending
                            select new ClsBankPaymentVm
                            {
                                Id = stdGroup.Key.Id,
                                TaxId = stdGroup.Key.TaxId,
                                AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                Type = "Expense",
                                ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                            }).ToList();

                #endregion

                #region Sales

                var salesTaxList = (from a in oConnectionContext.DbClsSalesDetails
                                    join b in oConnectionContext.DbClsSales
                                        on a.SalesId equals b.SalesId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                    //&& a.TaxAmount != 0 
                    && b.Status != "Draft"
                                    select new ClsBankPaymentVm
                                    {
                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                        Id = a.SalesId,
                                        TaxId = a.TaxId,
                                        AddedOn = b.SalesDate,
                                        Type = "Sales",
                                        ReferenceNo = b.InvoiceNo,
                                        TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                        TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax),
                                    }).ToList();

                var salesAdditionalTaxList = (from a in oConnectionContext.DbClsSalesAdditionalCharges
                                              join b in oConnectionContext.DbClsSales
                                                  on a.SalesId equals b.SalesId
                                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                              //&& a.TaxAmount != 0 
                              && b.Status != "Draft"
                                              select new ClsBankPaymentVm
                                              {
                                                  IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                  Id = a.SalesId,
                                                  TaxId = a.TaxId,
                                                  AddedOn = b.SalesDate,
                                                  Type = "Sales",
                                                  ReferenceNo = b.InvoiceNo,
                                                  TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                                  TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax),
                                              }).ToList();

                salesTaxList.AddRange(salesAdditionalTaxList);

                List<ClsBankPaymentVm> oClsSalesTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in salesTaxList)
                {
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                Sales = (from a in oClsSalesTaxVm
                         group a by new { a.TaxId, a.Id } into stdGroup
                         //orderby stdgroup.key descending
                         select new ClsBankPaymentVm
                         {
                             Id = stdGroup.Key.Id,
                             TaxId = stdGroup.Key.TaxId,
                             AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                             Type = "Sales",
                             ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                             TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                             TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                         }).ToList();

                #endregion

                #region Sales Return

                var salesReturnTaxList = (from a in oConnectionContext.DbClsSalesReturnDetails
                                          join b in oConnectionContext.DbClsSalesReturn
                                       on a.SalesReturnId equals b.SalesReturnId
                                          join p in oConnectionContext.DbClsSales
                                          on b.SalesId equals p.SalesId
                                          where a.CompanyId == obj.CompanyId
                               && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                               && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                 && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                          //&& a.TaxAmount != 0
                                          select new ClsBankPaymentVm
                                          {
                                              IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                              Id = b.SalesReturnId,
                                              TaxId = a.TaxId,
                                              AddedOn = b.Date,
                                              Type = "Sales Return",
                                              ReferenceNo = b.InvoiceNo,
                                              TransactionAmount = (a.AmountExcTax) * -1,
                                              TaxAmount = (a.AmountIncTax - a.AmountExcTax) * -1
                                              //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                              //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                          }).ToList();

                var salesReturnAdditionalTaxList = (from a in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                                    join b in oConnectionContext.DbClsSalesReturn
                                       on a.SalesReturnId equals b.SalesReturnId
                                                    join p in oConnectionContext.DbClsSales
                                                    on b.SalesId equals p.SalesId
                                                    where a.CompanyId == obj.CompanyId
                                         && a.IsDeleted == false && a.IsActive == true
                                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                    //&& a.TaxAmount != 0
                                                    select new ClsBankPaymentVm
                                                    {
                                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                        Id = b.SalesReturnId,
                                                        TaxId = a.TaxId,
                                                        AddedOn = b.Date,
                                                        Type = "Sales Return",
                                                        ReferenceNo = b.InvoiceNo,
                                                        TransactionAmount = (a.AmountExcTax) * -1,
                                                        TaxAmount = (a.AmountIncTax - a.AmountExcTax) * -1
                                                        //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                                        //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                                    }).ToList();

                salesReturnTaxList.AddRange(salesReturnAdditionalTaxList);

                List<ClsBankPaymentVm> oClsSalesReturnTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in salesReturnTaxList)
                {
                    //decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesReturnTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                SalesReturn = (from a in oClsSalesReturnTaxVm
                               group a by new { a.TaxId, a.Id } into stdGroup
                               //orderby stdgroup.key descending
                               select new ClsBankPaymentVm
                               {
                                   Id = stdGroup.Key.Id,
                                   TaxId = stdGroup.Key.TaxId,
                                   AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                   Type = "Sales Return",
                                   ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                   TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                   TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                               }).ToList();

                #endregion

                #region Sales Payments

                var salesPaymentTaxList = (from a in oConnectionContext.DbClsCustomerPayment
                                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                           //&& a.TaxAmount != 0 
                           && a.ParentId == 0
                                           select new ClsBankPaymentVm
                                           {
                                               IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                               Id = a.CustomerPaymentId,
                                               TaxId = a.TaxId,
                                               AddedOn = a.PaymentDate,
                                               Type = a.Type,
                                               ReferenceNo = a.ReferenceNo,
                                               TransactionAmount = a.AmountExcTax,
                                               TaxAmount = a.Amount - a.AmountExcTax,
                                           }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                 && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                     //&& a.TaxAmount != 0 
                                     && a.ParentId != 0
                                                     select new ClsBankPaymentVm
                                                     {
                                                         IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                         Id = a.CustomerPaymentId,
                                                         TaxId = a.TaxId,
                                                         AddedOn = a.PaymentDate,
                                                         Type = a.Type,
                                                         ReferenceNo = a.Type == "Customer Refund" ? a.ReferenceNo : oConnectionContext.DbClsSales.Where(bb => bb.SalesId == a.SalesId).Select(bb => bb.InvoiceNo).FirstOrDefault(),
                                                         TransactionAmount = a.AmountExcTax * -1,
                                                         TaxAmount = (a.Amount - a.AmountExcTax) * -1,
                                                     }).ToList();

                List<ClsBankPaymentVm> oClsSalesPaymentTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in salesPaymentTaxList)
                {
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesPaymentTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                SalesPayments = (from a in oClsSalesPaymentTaxVm
                                 group a by new { a.TaxId, a.Id } into stdGroup
                                 //orderby stdgroup.key descending
                                 select new ClsBankPaymentVm
                                 {
                                     Id = stdGroup.Key.Id,
                                     TaxId = stdGroup.Key.TaxId,
                                     AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                     Type = stdGroup.Select(x => x.Type).FirstOrDefault(),
                                     ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                     TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                     TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                                 }).ToList();

                #endregion

                #region Purchase

                var purchaseTaxList = (from a in oConnectionContext.DbClsPurchaseDetails
                                       join b in oConnectionContext.DbClsPurchase
                                    on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                       //&& a.TaxAmount != 0 
                       && b.Status != "Draft"
                                       select new ClsBankPaymentVm
                                       {
                                           IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                           Id = b.PurchaseId,
                                           TaxId = a.TaxId,
                                           AddedOn = b.PurchaseDate,
                                           Type = "Purchase",
                                           ReferenceNo = b.ReferenceNo,
                                           TransactionAmount = b.IsReverseCharge == 1 ? a.AmountExcTax : (a.AmountExcTax * -1),
                                           TaxAmount = b.IsReverseCharge == 1 ? (a.AmountIncTax - a.AmountExcTax) : ((a.AmountIncTax - a.AmountExcTax) * -1)
                                           //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                           //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                       }).ToList();

                var purchaseAdditionalTaxList = (from a in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                 join b in oConnectionContext.DbClsPurchase
                                              on a.PurchaseId equals b.PurchaseId
                                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                 && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                             && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                 //&& a.TaxAmount != 0 
                                 && b.Status != "Draft"
                                                 select new ClsBankPaymentVm
                                                 {
                                                     IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                     Id = b.PurchaseId,
                                                     TaxId = a.TaxId,
                                                     AddedOn = b.PurchaseDate,
                                                     Type = "Purchase",
                                                     ReferenceNo = b.ReferenceNo,
                                                     TransactionAmount = b.IsReverseCharge == 1 ? a.AmountExcTax : (a.AmountExcTax * -1),
                                                     TaxAmount = b.IsReverseCharge == 1 ? (a.AmountIncTax - a.AmountExcTax) : ((a.AmountIncTax - a.AmountExcTax) * -1)
                                                     //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                                     //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                                 }).ToList();

                purchaseTaxList.AddRange(purchaseAdditionalTaxList);

                List<ClsBankPaymentVm> oClsPurchaseTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in purchaseTaxList)
                {
                    //decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsPurchaseTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                Purchase = (from a in oClsPurchaseTaxVm
                            group a by new { a.TaxId, a.Id } into stdGroup
                            //orderby stdgroup.key descending
                            select new ClsBankPaymentVm
                            {
                                Id = stdGroup.Key.Id,
                                TaxId = stdGroup.Key.TaxId,
                                AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                Type = "Purchase",
                                ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                            }).ToList();

                #endregion

                #region Purchase Return

                var purchaseReturnTaxList = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                             join b in oConnectionContext.DbClsPurchaseReturn
                                          on a.PurchaseReturnId equals b.PurchaseReturnId
                                             where a.CompanyId == obj.CompanyId
                                             && a.IsDeleted == false && a.IsActive == true
                                             && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                             //&& a.TaxAmount != 0
                                             select new ClsBankPaymentVm
                                             {
                                                 IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                 Id = b.PurchaseReturnId,
                                                 TaxId = a.TaxId,
                                                 AddedOn = b.Date,
                                                 Type = "Purchase Return",
                                                 ReferenceNo = b.InvoiceNo,
                                                 TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                                 TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax)
                                                 //TransactionAmount = b.Subtotal - b.TotalDiscount,
                                                 //TaxAmount = a.TaxAmount * a.Quantity
                                             }).ToList();

                var purchaseReturnAdditionalTaxList = (from a in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                                       join b in oConnectionContext.DbClsPurchaseReturn
                                                    on a.PurchaseReturnId equals b.PurchaseReturnId
                                                       where a.CompanyId == obj.CompanyId
                                                       && a.IsDeleted == false && a.IsActive == true
                                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                       //&& a.TaxAmount != 0
                                                       select new ClsBankPaymentVm
                                                       {
                                                           IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                           Id = b.PurchaseReturnId,
                                                           TaxId = a.TaxId,
                                                           AddedOn = b.Date,
                                                           Type = "Purchase Return",
                                                           ReferenceNo = b.InvoiceNo,
                                                           TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                                           TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax)
                                                           //TransactionAmount = b.Subtotal - b.TotalDiscount,
                                                           //TaxAmount = a.TaxAmount * a.Quantity
                                                       }).ToList();

                purchaseReturnTaxList.AddRange(purchaseReturnAdditionalTaxList);

                List<ClsBankPaymentVm> oClsPurchaseReturnTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in purchaseReturnTaxList)
                {
                    //decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsPurchaseReturnTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                PurchaseReturn = (from a in oClsPurchaseReturnTaxVm
                                  group a by new { a.TaxId, a.Id } into stdGroup
                                  //orderby stdgroup.key descending
                                  select new ClsBankPaymentVm
                                  {
                                      Id = stdGroup.Key.Id,
                                      TaxId = stdGroup.Key.TaxId,
                                      AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                      Type = "Purchase Return",
                                      ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                      TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                      TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum(),
                                  }).ToList();

                #endregion

                Ledger = Expenses.Concat(Sales).Concat(SalesReturn).Concat(SalesPayments).Concat(Purchase).Concat(PurchaseReturn).ToList();

            }
            else
            {
                #region Expenses
                var expenseTaxList = (from a in oConnectionContext.DbClsExpensePayment
                                      join b in oConnectionContext.DbClsExpense
                                          on a.ExpenseId equals b.ExpenseId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                             && b.IsDeleted == false && b.IsActive == true
                                             && b.BranchId == obj.BranchId
                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                      //&& a.TaxAmount != 0
                                      select new ClsBankPaymentVm
                                      {
                                          IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                          Id = a.ExpenseId,
                                          TaxId = a.TaxId,
                                          AddedOn = b.Date,
                                          Type = "Expense",
                                          ReferenceNo = b.ReferenceNo,
                                          TransactionAmount = b.IsReverseCharge == 1 ? a.AmountExcTax : (a.AmountExcTax * -1),
                                          TaxAmount = b.IsReverseCharge == 1 ? (a.Amount - a.AmountExcTax) : ((a.Amount - a.AmountExcTax) * -1)
                                      }).ToList();

                List<ClsBankPaymentVm> oClsExpenseTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in expenseTaxList)
                {
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsExpenseTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                Expenses = (from a in oClsExpenseTaxVm
                            group a by new { a.TaxId, a.Id } into stdGroup
                            //orderby stdgroup.key descending
                            select new ClsBankPaymentVm
                            {
                                Id = stdGroup.Key.Id,
                                TaxId = stdGroup.Key.TaxId,
                                AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                Type = "Expense",
                                ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                            }).ToList();

                #endregion

                #region Sales

                var salesTaxList = (from a in oConnectionContext.DbClsSalesDetails
                                    join b in oConnectionContext.DbClsSales
                                        on a.SalesId equals b.SalesId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                           && b.BranchId == obj.BranchId
                && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                    //&& a.TaxAmount != 0 
                    && b.Status != "Draft"
                                    select new ClsBankPaymentVm
                                    {
                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                        Id = a.SalesId,
                                        TaxId = a.TaxId,
                                        AddedOn = b.SalesDate,
                                        Type = "Sales",
                                        ReferenceNo = b.InvoiceNo,
                                        TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                        TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax),
                                    }).ToList();

                var salesAdditionalTaxList = (from a in oConnectionContext.DbClsSalesAdditionalCharges
                                              join b in oConnectionContext.DbClsSales
                                                  on a.SalesId equals b.SalesId
                                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                     && b.BranchId == obj.BranchId
                          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                              //&& a.TaxAmount != 0 
                              && b.Status != "Draft"
                                              select new ClsBankPaymentVm
                                              {
                                                  IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                  Id = a.SalesId,
                                                  TaxId = a.TaxId,
                                                  AddedOn = b.SalesDate,
                                                  Type = "Sales",
                                                  ReferenceNo = b.InvoiceNo,
                                                  TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                                  TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax),
                                              }).ToList();

                salesTaxList.AddRange(salesAdditionalTaxList);

                List<ClsBankPaymentVm> oClsSalesTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in salesTaxList)
                {
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                Sales = (from a in oClsSalesTaxVm
                         group a by new { a.TaxId, a.Id } into stdGroup
                         //orderby stdgroup.key descending
                         select new ClsBankPaymentVm
                         {
                             Id = stdGroup.Key.Id,
                             TaxId = stdGroup.Key.TaxId,
                             AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                             Type = "Sales",
                             ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                             TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                             TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                         }).ToList();

                #endregion

                #region Sales Return

                var salesReturnTaxList = (from a in oConnectionContext.DbClsSalesReturnDetails
                                          join b in oConnectionContext.DbClsSalesReturn
                                       on a.SalesReturnId equals b.SalesReturnId
                                          join p in oConnectionContext.DbClsSales
                                          on b.SalesId equals p.SalesId
                                          where a.CompanyId == obj.CompanyId
                               && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                               && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                               && b.BranchId == obj.BranchId
                 && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                          //&& a.TaxAmount != 0
                                          select new ClsBankPaymentVm
                                          {
                                              IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                              Id = b.SalesReturnId,
                                              TaxId = a.TaxId,
                                              AddedOn = b.Date,
                                              Type = "Sales Return",
                                              ReferenceNo = b.InvoiceNo,
                                              TransactionAmount = (a.AmountExcTax) * -1,
                                              TaxAmount = (a.AmountIncTax - a.AmountExcTax) * -1
                                              //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                              //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                          }).ToList();

                var salesReturnAdditionalTaxList = (from a in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                                    join b in oConnectionContext.DbClsSalesReturn
                                                 on a.SalesReturnId equals b.SalesReturnId
                                                    join p in oConnectionContext.DbClsSales
                                                    on b.SalesId equals p.SalesId
                                                    where a.CompanyId == obj.CompanyId
                                         && a.IsDeleted == false && a.IsActive == true
                                         && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                         && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                                         && b.BranchId == obj.BranchId
                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                    //&& a.TaxAmount != 0
                                                    select new ClsBankPaymentVm
                                                    {
                                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                        Id = b.SalesReturnId,
                                                        TaxId = a.TaxId,
                                                        AddedOn = b.Date,
                                                        Type = "Sales Return",
                                                        ReferenceNo = b.InvoiceNo,
                                                        TransactionAmount = (a.AmountExcTax) * -1,
                                                        TaxAmount = (a.AmountIncTax - a.AmountExcTax) * -1
                                                        //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                                        //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                                    }).ToList();

                salesReturnTaxList.AddRange(salesReturnAdditionalTaxList);

                List<ClsBankPaymentVm> oClsSalesReturnTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in salesReturnTaxList)
                {
                    //decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesReturnTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                SalesReturn = (from a in oClsSalesReturnTaxVm
                               group a by new { a.TaxId, a.Id } into stdGroup
                               //orderby stdgroup.key descending
                               select new ClsBankPaymentVm
                               {
                                   Id = stdGroup.Key.Id,
                                   TaxId = stdGroup.Key.TaxId,
                                   AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                   Type = "Sales Return",
                                   ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                   TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                   TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                               }).ToList();

                #endregion

                #region Sales Payments

                var salesPaymentTaxList = (from a in oConnectionContext.DbClsCustomerPayment
                                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                  && a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                           DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                           //&& a.TaxAmount != 0 
                           && a.ParentId == 0
                                           select new ClsBankPaymentVm
                                           {
                                               IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                               Id = a.CustomerPaymentId,
                                               TaxId = a.TaxId,
                                               AddedOn = a.PaymentDate,
                                               Type = a.Type,
                                               ReferenceNo = a.ReferenceNo,
                                               TransactionAmount = a.AmountExcTax,
                                               TaxAmount = a.Amount - a.AmountExcTax,
                                           }).Concat(from a in oConnectionContext.DbClsCustomerPayment
                                                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                            && a.BranchId == obj.BranchId
                                 && DbFunctions.TruncateTime(a.PaymentDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(a.PaymentDate) <= obj.ToDate
                                     //&& a.TaxAmount != 0 
                                     && a.ParentId != 0
                                                     select new ClsBankPaymentVm
                                                     {
                                                         IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                         Id = a.CustomerPaymentId,
                                                         TaxId = a.TaxId,
                                                         AddedOn = a.PaymentDate,
                                                         Type = a.Type,
                                                         ReferenceNo = a.Type == "Customer Refund" ? a.ReferenceNo : oConnectionContext.DbClsSales.Where(bb => bb.SalesId == a.SalesId).Select(bb => bb.InvoiceNo).FirstOrDefault(),
                                                         TransactionAmount = a.AmountExcTax * -1,
                                                         TaxAmount = (a.Amount - a.AmountExcTax) * -1,
                                                     }).ToList();

                List<ClsBankPaymentVm> oClsSalesPaymentTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in salesPaymentTaxList)
                {
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsSalesPaymentTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                SalesPayments = (from a in oClsSalesPaymentTaxVm
                                 group a by new { a.TaxId, a.Id } into stdGroup
                                 //orderby stdgroup.key descending
                                 select new ClsBankPaymentVm
                                 {
                                     Id = stdGroup.Key.Id,
                                     TaxId = stdGroup.Key.TaxId,
                                     AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                     Type = stdGroup.Select(x => x.Type).FirstOrDefault(),
                                     ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                     TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                     TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                                 }).ToList();

                #endregion

                #region Purchase

                var purchaseTaxList = (from a in oConnectionContext.DbClsPurchaseDetails
                                       join b in oConnectionContext.DbClsPurchase
                                    on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                        && b.BranchId == obj.BranchId
                   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                       //&& a.TaxAmount != 0 
                       && b.Status != "Draft"
                                       select new ClsBankPaymentVm
                                       {
                                           IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                           Id = b.PurchaseId,
                                           TaxId = a.TaxId,
                                           AddedOn = b.PurchaseDate,
                                           Type = "Purchase",
                                           ReferenceNo = b.ReferenceNo,
                                           TransactionAmount = b.IsReverseCharge == 1 ? a.AmountExcTax : (a.AmountExcTax * -1),
                                           TaxAmount = b.IsReverseCharge == 1 ? (a.AmountIncTax - a.AmountExcTax) : ((a.AmountIncTax - a.AmountExcTax) * -1)
                                           //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                           //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                       }).ToList();

                var purchaseAdditionalTaxList = (from a in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                 join b in oConnectionContext.DbClsPurchase
                                              on a.PurchaseId equals b.PurchaseId
                                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                                 && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                  && b.BranchId == obj.BranchId
                             && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                 //&& a.TaxAmount != 0 
                                 && b.Status != "Draft"
                                                 select new ClsBankPaymentVm
                                                 {
                                                     IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                     Id = b.PurchaseId,
                                                     TaxId = a.TaxId,
                                                     AddedOn = b.PurchaseDate,
                                                     Type = "Purchase",
                                                     ReferenceNo = b.ReferenceNo,
                                                     TransactionAmount = b.IsReverseCharge == 1 ? a.AmountExcTax : (a.AmountExcTax * -1),
                                                     TaxAmount = b.IsReverseCharge == 1 ? (a.AmountIncTax - a.AmountExcTax) : ((a.AmountIncTax - a.AmountExcTax) * -1)
                                                     //TransactionAmount = (b.Subtotal - b.TotalDiscount) * -1,
                                                     //TaxAmount = (a.TaxAmount * a.Quantity) * -1
                                                 }).ToList();

                purchaseTaxList.AddRange(purchaseAdditionalTaxList);

                List<ClsBankPaymentVm> oClsPurchaseTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in purchaseTaxList)
                {
                    //decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsPurchaseTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                Purchase = (from a in oClsPurchaseTaxVm
                            group a by new { a.TaxId, a.Id } into stdGroup
                            //orderby stdgroup.key descending
                            select new ClsBankPaymentVm
                            {
                                Id = stdGroup.Key.Id,
                                TaxId = stdGroup.Key.TaxId,
                                AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                Type = "Purchase",
                                ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum()
                            }).ToList();

                #endregion

                #region Purchase Return

                var purchaseReturnTaxList = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                             join b in oConnectionContext.DbClsPurchaseReturn
                                          on a.PurchaseReturnId equals b.PurchaseReturnId
                                             where a.CompanyId == obj.CompanyId
                                             && a.IsDeleted == false && a.IsActive == true
                                             && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                             && b.BranchId == obj.BranchId
                                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                             //&& a.TaxAmount != 0
                                             select new ClsBankPaymentVm
                                             {
                                                 IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                 Id = b.PurchaseReturnId,
                                                 TaxId = a.TaxId,
                                                 AddedOn = b.Date,
                                                 Type = "Purchase Return",
                                                 ReferenceNo = b.InvoiceNo,
                                                 TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                                 TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax)
                                                 //TransactionAmount = b.Subtotal - b.TotalDiscount,
                                                 //TaxAmount = a.TaxAmount * a.Quantity
                                             }).ToList();

                var purchaseReturnAdditionalTaxList = (from a in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                                       join b in oConnectionContext.DbClsPurchaseReturn
                                                    on a.PurchaseReturnId equals b.PurchaseReturnId
                                                       where a.CompanyId == obj.CompanyId
                                                       && a.IsDeleted == false && a.IsActive == true
                                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                                       && b.BranchId == obj.BranchId
                                            && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                       //&& a.TaxAmount != 0
                                                       select new ClsBankPaymentVm
                                                       {
                                                           IsTaxGroup = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == a.TaxId).Select(bb => bb.IsTaxGroup).FirstOrDefault(),
                                                           Id = b.PurchaseReturnId,
                                                           TaxId = a.TaxId,
                                                           AddedOn = b.Date,
                                                           Type = "Purchase Return",
                                                           ReferenceNo = b.InvoiceNo,
                                                           TransactionAmount = b.IsReverseCharge == 1 ? (a.AmountExcTax * -1) : a.AmountExcTax,
                                                           TaxAmount = b.IsReverseCharge == 1 ? ((a.AmountIncTax - a.AmountExcTax) * -1) : (a.AmountIncTax - a.AmountExcTax)
                                                           //TransactionAmount = b.Subtotal - b.TotalDiscount,
                                                           //TaxAmount = a.TaxAmount * a.Quantity
                                                       }).ToList();

                purchaseReturnTaxList.AddRange(purchaseReturnAdditionalTaxList);

                List<ClsBankPaymentVm> oClsPurchaseReturnTaxVm = new List<ClsBankPaymentVm>();
                foreach (var item in purchaseReturnTaxList)
                {
                    //decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsPurchaseReturnTaxVm.Add(new ClsBankPaymentVm
                        {
                            Id = item.Id,
                            TaxId = tax.TaxId,
                            AddedOn = item.AddedOn,
                            Type = item.Type,
                            ReferenceNo = item.ReferenceNo,
                            TransactionAmount = item.TransactionAmount,
                            TaxAmount = (tax.TaxPercent / 100) * item.TransactionAmount,
                            TaxPercent = tax.TaxPercent,
                        });
                    }
                }

                PurchaseReturn = (from a in oClsPurchaseReturnTaxVm
                                  group a by new { a.TaxId, a.Id } into stdGroup
                                  //orderby stdgroup.key descending
                                  select new ClsBankPaymentVm
                                  {
                                      Id = stdGroup.Key.Id,
                                      TaxId = stdGroup.Key.TaxId,
                                      AddedOn = stdGroup.Select(x => x.AddedOn).FirstOrDefault(),
                                      Type = "Purchase Return",
                                      ReferenceNo = stdGroup.Select(x => x.ReferenceNo).FirstOrDefault(),
                                      TransactionAmount = stdGroup.Select(x => x.TransactionAmount).Sum(),
                                      TaxAmount = stdGroup.Select(x => x.TaxAmount).Sum(),
                                  }).ToList();

                #endregion

                Ledger = Expenses.Concat(Sales).Concat(SalesReturn).Concat(SalesPayments).Concat(Purchase).Concat(PurchaseReturn).ToList();
            }


            return Ledger;
        }

        public List<ClsStockDetails> StockOpeningBalance(ClsStockDetails obj)
        {
            List<ClsStockDetails> OpeningStock;
            List<ClsStockDetails> OpeningStockExpired;

            if (obj.BranchId == 0)
            {
                #region Opening Stock
                OpeningStock = (from a in oConnectionContext.DbClsOpeningStock
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                               && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                   DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                  && a.Quantity > 0
                                select new ClsStockDetails
                                {
                                    Type = "Opening Stock",
                                    Date = a.Date,
                                    ReferenceNo = "",
                                    //Quantity = a.Quantity,
                                    Quantity = a.QuantityPurchased,
                                    Credit = a.QuantityPurchased,
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    //PriceAddedFor = a.PriceAddedFor,
                                    PriceAddedFor = 4,
                                    AmountIncTax = a.SubTotal
                                }).ToList();

                #endregion

                #region Opening Stock Expired
                OpeningStockExpired = (from a in oConnectionContext.DbClsOpeningStock
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                      && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                          DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                         && a.IsStopSelling == true && a.QuantityRemaining > 0
                                       select new ClsStockDetails
                                       {
                                           Type = "Opening Stock Expired",
                                           Date = a.ExpiryDate.Value,
                                           ReferenceNo = "",
                                           //Quantity = a.QuantityRemaining,
                                           Quantity = a.QuantityRemaining,
                                           Credit = a.QuantityRemaining,
                                           ItemId = a.ItemId,
                                           ItemDetailsId = a.ItemDetailsId,
                                           //PriceAddedFor = a.PriceAddedFor,
                                           PriceAddedFor = 4,
                                           AmountIncTax = a.SubTotal
                                       }).ToList();

                #endregion
            }
            else
            {
                #region Opening Stock
                OpeningStock = (from a in oConnectionContext.DbClsOpeningStock
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && a.BranchId == obj.BranchId
                               && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                   DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                  && a.Quantity > 0
                                select new ClsStockDetails
                                {
                                    Type = "Opening Stock",
                                    Date = a.Date,
                                    ReferenceNo = "",
                                    //Quantity = a.Quantity,
                                    Quantity = a.QuantityPurchased,
                                    Credit = a.QuantityPurchased,
                                    ItemId = a.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    //PriceAddedFor = a.PriceAddedFor,
                                    PriceAddedFor = 4,
                                    AmountIncTax = a.SubTotal
                                }).ToList();

                #endregion

                #region Opening Stock Expired
                OpeningStockExpired = (from a in oConnectionContext.DbClsOpeningStock
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                      && a.BranchId == obj.BranchId
                                      && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                                          DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                         && a.IsStopSelling == true && a.QuantityRemaining > 0
                                       select new ClsStockDetails
                                       {
                                           Type = "Opening Stock Expired",
                                           Date = a.ExpiryDate.Value,
                                           ReferenceNo = "",
                                           //Quantity = a.QuantityRemaining,
                                           Quantity = a.QuantityRemaining,
                                           Credit = a.QuantityRemaining,
                                           ItemId = a.ItemId,
                                           ItemDetailsId = a.ItemDetailsId,
                                           //PriceAddedFor = a.PriceAddedFor,
                                           PriceAddedFor = 4,
                                           AmountIncTax = a.SubTotal
                                       }).ToList();

                #endregion
            }

            return OpeningStock.Union(OpeningStockExpired).ToList();
        }

        public List<ClsStockDetails> StockTransactions(ClsStockDetails obj)
        {
            List<ClsStockDetails> Sales;
            List<ClsStockDetails> SalesCombo;
            List<ClsStockDetails> SalesReturn;
            List<ClsStockDetails> SalesReturnCombo;
            List<ClsStockDetails> Purchase;
            List<ClsStockDetails> PurchaseExpiry;
            List<ClsStockDetails> PurchaseReturn;
            List<ClsStockDetails> PurchaseReturnNoParent;
            List<ClsStockDetails> StockAdjustmentCredit;
            List<ClsStockDetails> StockAdjustmentDebit;
            List<ClsStockDetails> StockTransfer;
            List<ClsStockDetails> StockReceived;
            List<ClsStockDetails> Ledger = new List<ClsStockDetails>();

            //var Type = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.AccountId == obj.AccountId).Select(a => a.Type).FirstOrDefault();
            if (obj.BranchId == 0)
            {
                #region Sales
                Sales = (from a in oConnectionContext.DbClsSalesDetails
                         join b in oConnectionContext.DbClsSales
                      on a.SalesId equals b.SalesId
                         join c in oConnectionContext.DbClsItem
                         on a.ItemId equals c.ItemId
                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                        && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                            && a.UnitCost != 0 && b.Status != "Draft" && a.IsComboItems == false
                            && c.IsManageStock == true
                         select new ClsStockDetails
                         {
                             DetailsDate = a.AddedOn,
                             Type = "Sales",
                             Date = b.SalesDate,
                             ReferenceNo = b.InvoiceNo,
                             //Quantity = a.Quantity + a.FreeQuantity,
                             ItemId = a.ItemId,
                             ItemDetailsId = a.ItemDetailsId,
                             //PriceAddedFor = a.PriceAddedFor,
                             PriceAddedFor = 4,
                             AmountIncTax = a.AmountIncTax,
                             Quantity = a.QuantitySold,
                             Debit = a.QuantitySold,
                         }).ToList();

                #endregion

                #region Sales Combo
                //SalesCombo = (from a in oConnectionContext.DbClsSalesDetails
                //              join b in oConnectionContext.DbClsSales
                //           on a.SalesId equals b.SalesId
                //              join c in oConnectionContext.DbClsItem
                //              on a.ItemId equals c.ItemId
                //              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                //        && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                //        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                //        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                //            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                //            //&& a.UnitCost != 0
                //            && b.Status != "Draft" && a.IsComboItems == true
                //            && c.IsManageStock == true
                //              select new ClsStockDetails
                //              {
                //                  DetailsDate = a.AddedOn,
                //                  Type = "Sales Combo",
                //                  Date = b.SalesDate,
                //                  ReferenceNo = b.InvoiceNo,
                //                  //Quantity = a.Quantity + a.FreeQuantity,
                //                  ItemId = a.ItemId,
                //                  ItemDetailsId = a.ItemDetailsId,
                //                  //PriceAddedFor = a.PriceAddedFor,
                //                  PriceAddedFor = 4,
                //                  AmountIncTax = a.AmountIncTax,
                //                  Quantity = a.QuantitySold,
                //                  Debit = a.QuantitySold
                //              }).ToList();

                SalesCombo = (from a in oConnectionContext.DbClsSalesDetails
                              join b in oConnectionContext.DbClsSales
                           on a.SalesId equals b.SalesId
                              join c in oConnectionContext.DbClsItem
                              on a.ItemId equals c.ItemId
                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                        && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                            //&& a.UnitCost != 0
                            && b.Status != "Draft" && a.IsComboItems == true
                            && c.IsManageStock == true
                              select new
                              {
                                  a.AddedOn,
                                  b.SalesDate,
                                  b.InvoiceNo,
                                  a.ItemId,
                                  a.ItemDetailsId,
                                  a.AmountIncTax,
                                  a.QuantitySold
                              })
                  .AsEnumerable()
                  .Select(x => new ClsStockDetails
                  {
                      DetailsDate = x.AddedOn.AddMilliseconds(2),
                      Type = "Sales Combo",
                      Date = x.SalesDate,
                      ReferenceNo = x.InvoiceNo,
                      ItemId = x.ItemId,
                      ItemDetailsId = x.ItemDetailsId,
                      PriceAddedFor = 4,
                      AmountIncTax = x.AmountIncTax,
                      Quantity = x.QuantitySold,
                      Debit = x.QuantitySold
                  }).ToList();


                #endregion

                #region Sales Return
                SalesReturn = (from a in oConnectionContext.DbClsSalesReturnDetails
                               join b in oConnectionContext.DbClsSalesReturn
                            on a.SalesReturnId equals b.SalesReturnId
                               join p in oConnectionContext.DbClsSales
                                            on b.SalesId equals p.SalesId
                               join c in oConnectionContext.DbClsItem
         on a.ItemId equals c.ItemId
                               where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                               && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                  && a.UnitCost != 0 && b.Status.ToLower() != "draft"
                                 && a.IsComboItems == false && c.IsManageStock == true
                               select new ClsStockDetails
                               {
                                   DetailsDate = a.AddedOn,
                                   Type = "Sales Return",
                                   Date = b.Date,
                                   ReferenceNo = b.InvoiceNo,
                                   //Quantity = a.Quantity + a.FreeQuantity,
                                   ItemId = a.ItemId,
                                   ItemDetailsId = a.ItemDetailsId,
                                   //PriceAddedFor = a.PriceAddedFor,
                                   PriceAddedFor = 4,
                                   AmountIncTax = a.AmountIncTax,
                                   Quantity = a.QuantityReturned,
                                   Credit = a.QuantityReturned
                               }).ToList();

                #endregion

                #region Sales Return Combo
                SalesReturnCombo = (from a in oConnectionContext.DbClsSalesReturnDetails
                                    join b in oConnectionContext.DbClsSalesReturn
                                 on a.SalesReturnId equals b.SalesReturnId
                                    join p in oConnectionContext.DbClsSales
                                                 on b.SalesId equals p.SalesId
                                    join c in oConnectionContext.DbClsItem
              on a.ItemId equals c.ItemId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                               && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
                              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                  //&& a.UnitCost != 0 
                                  && b.Status.ToLower() != "draft"
                                 && a.IsComboItems == true && c.IsManageStock == true
                                    select new
                                    {
                                        a.AddedOn,
                                        b.Date,
                                        b.InvoiceNo,
                                        a.ItemId,
                                        a.ItemDetailsId,
                                        a.AmountIncTax,
                                        a.QuantityReturned
                                    })
                  .AsEnumerable()
                  .Select(x => new ClsStockDetails
                  {
                      DetailsDate = x.AddedOn.AddMilliseconds(2),
                      Type = "Sales Return Combo",
                      Date = x.Date,
                      ReferenceNo = x.InvoiceNo,
                      ItemId = x.ItemId,
                      ItemDetailsId = x.ItemDetailsId,
                      PriceAddedFor = 4,
                      AmountIncTax = x.AmountIncTax,
                      Quantity = x.QuantityReturned,
                      Credit = x.QuantityReturned
                  }).ToList();

                #endregion

                #region Purchase
                Purchase = (from a in oConnectionContext.DbClsPurchaseDetails
                            join b in oConnectionContext.DbClsPurchase
                         on a.PurchaseId equals b.PurchaseId
                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                            && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                           && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                               && a.UnitCost != 0 && b.Status != "Draft"
                            select new ClsStockDetails
                            {
                                DetailsDate = a.AddedOn,
                                Type = "Purchase",
                                Date = b.PurchaseDate,
                                ReferenceNo = b.ReferenceNo,
                                //Quantity = a.Quantity + a.FreeQuantity,
                                ItemId = a.ItemId,
                                ItemDetailsId = a.ItemDetailsId,
                                //PriceAddedFor = a.PriceAddedFor,
                                PriceAddedFor = 4,
                                AmountIncTax = a.AmountIncTax,
                                Quantity = a.QuantityPurchased,
                                Credit = a.QuantityPurchased,
                            }).ToList();
                #endregion

                #region Purchase Expiry
                PurchaseExpiry = (from a in oConnectionContext.DbClsPurchaseDetails
                                  join b in oConnectionContext.DbClsPurchase
                               on a.PurchaseId equals b.PurchaseId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                 && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                     && a.UnitCost != 0 && b.Status != "Draft" && a.IsStopSelling == true
                                  select new ClsStockDetails
                                  {
                                      DetailsDate = a.AddedOn,
                                      Type = "Purchase Expired",
                                      Date = a.ExpiryDate.Value,
                                      ReferenceNo = b.ReferenceNo,
                                      //Quantity = a.Quantity + a.FreeQuantity,
                                      ItemId = a.ItemId,
                                      ItemDetailsId = a.ItemDetailsId,
                                      //PriceAddedFor = a.PriceAddedFor,
                                      PriceAddedFor = 4,
                                      AmountIncTax = a.AmountIncTax,
                                      Quantity = a.QuantityRemaining,
                                      Debit = a.QuantityRemaining
                                  }).ToList();
                #endregion

                #region Purchase Return
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                  join b in oConnectionContext.DbClsPurchaseReturn
                               on a.PurchaseReturnId equals b.PurchaseReturnId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                 && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                     && a.UnitCost != 0 && b.Status.ToLower() != "draft"
                                  select new ClsStockDetails
                                  {
                                      DetailsDate = a.AddedOn,
                                      Type = "Purchase Return",
                                      Date = b.Date,
                                      ReferenceNo = b.InvoiceNo,
                                      //Quantity = a.Quantity + a.FreeQuantity,
                                      ItemId = a.ItemId,
                                      ItemDetailsId = a.ItemDetailsId,
                                      //PriceAddedFor = a.PriceAddedFor,
                                      PriceAddedFor = 4,
                                      AmountIncTax = a.AmountIncTax,
                                      Quantity = a.QuantityReturned,
                                      Debit = a.QuantityReturned
                                  }).ToList();

                #endregion

                #region Stock Adjustment Credit
                StockAdjustmentCredit = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                                         join b in oConnectionContext.DbClsStockAdjustment
                            on a.StockAdjustmentId equals b.StockAdjustmentId
                                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                              && b.IsDeleted == false && b.IsActive == true
                              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                              && DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                  DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                  && b.AdjustmentType.ToLower() == "credit"
                                         select new ClsStockDetails
                                         {
                                             DetailsDate = a.AddedOn,
                                             Type = "Stock Adjustment Credit",
                                             Date = b.AdjustmentDate,
                                             ReferenceNo = b.ReferenceNo,
                                             //Quantity = a.Quantity,
                                             ItemId = a.ItemId,
                                             ItemDetailsId = a.ItemDetailsId,
                                             //PriceAddedFor = a.PriceAddedFor,
                                             PriceAddedFor = 4,
                                             AmountIncTax = a.Amount,
                                             Quantity = a.QuantityAdjusted,
                                             Credit = a.QuantityAdjusted
                                         }).ToList();

                #endregion

                #region Stock Adjustment Debit
                StockAdjustmentDebit = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                                        join b in oConnectionContext.DbClsStockAdjustment
                           on a.StockAdjustmentId equals b.StockAdjustmentId
                                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                             && b.IsDeleted == false && b.IsActive == true
                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                             && DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                 && b.AdjustmentType.ToLower() == "debit"
                                        select new ClsStockDetails
                                        {
                                            DetailsDate = a.AddedOn,
                                            Type = "Stock Adjustment Debit",
                                            Date = b.AdjustmentDate,
                                            ReferenceNo = b.ReferenceNo,
                                            //Quantity = a.Quantity,
                                            ItemId = a.ItemId,
                                            ItemDetailsId = a.ItemDetailsId,
                                            //PriceAddedFor = a.PriceAddedFor,
                                            PriceAddedFor = 4,
                                            AmountIncTax = a.Amount,
                                            Quantity = a.QuantityAdjusted,
                                            Debit = a.QuantityAdjusted
                                        }).ToList();

                #endregion

                #region Stock Transfer
                StockTransfer = (from a in oConnectionContext.DbClsStockTransferDetails
                                 join b in oConnectionContext.DbClsStockTransfer
                    on a.StockTransferId equals b.StockTransferId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                      && b.IsDeleted == false && b.IsActive == true
                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.FromBranchId)
                      && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                          && b.Status == 3
                                 select new ClsStockDetails
                                 {
                                     DetailsDate = a.AddedOn,
                                     Type = "Stock Transfer",
                                     Date = b.Date,
                                     ReferenceNo = b.ReferenceNo,
                                     //Quantity = a.Quantity,
                                     ItemId = a.ItemId,
                                     ItemDetailsId = a.ItemDetailsId,
                                     //PriceAddedFor = a.PriceAddedFor,
                                     PriceAddedFor = 4,
                                     AmountIncTax = a.Amount,
                                     Quantity = a.QuantityTransferred,
                                     Debit = a.QuantityTransferred
                                 }).ToList();

                #endregion

                #region Stock Received
                StockReceived = (from a in oConnectionContext.DbClsStockTransferDetails
                                 join b in oConnectionContext.DbClsStockTransfer
                    on a.StockTransferId equals b.StockTransferId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                      && b.IsDeleted == false && b.IsActive == true
                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.ToBranchId)
                      && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                          && b.Status == 3
                                 select new ClsStockDetails
                                 {
                                     DetailsDate = a.AddedOn,
                                     Type = "Stock Received",
                                     Date = b.Date,
                                     ReferenceNo = b.ReferenceNo,
                                     //Quantity = a.Quantity,
                                     ItemId = a.ItemId,
                                     ItemDetailsId = a.ItemDetailsId,
                                     //PriceAddedFor = a.PriceAddedFor,
                                     PriceAddedFor = 4,
                                     AmountIncTax = a.Amount,
                                     Quantity = a.QuantityTransferred,
                                     Credit = a.QuantityTransferred
                                 }).ToList();

                #endregion

                Ledger = Sales.Concat(SalesCombo).Concat(SalesReturn).Concat(SalesReturnCombo).Concat(Purchase)
                    .Concat(PurchaseExpiry).Concat(PurchaseReturn).Concat(StockAdjustmentCredit)
                    .Concat(StockAdjustmentDebit).Concat(StockTransfer).Concat(StockReceived).ToList();
            }
            else
            {
                #region Sales
                Sales = (from a in oConnectionContext.DbClsSalesDetails
                         join b in oConnectionContext.DbClsSales
                      on a.SalesId equals b.SalesId
                         join c in oConnectionContext.DbClsItem
                         on a.ItemId equals c.ItemId
                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                        && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                        && b.BranchId == obj.BranchId
                        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                            && a.UnitCost != 0 && b.Status != "Draft" && a.IsComboItems == false
                            && c.IsManageStock == true
                         select new ClsStockDetails
                         {
                             DetailsDate = a.AddedOn,
                             Type = "Sales",
                             Date = b.SalesDate,
                             ReferenceNo = b.InvoiceNo,
                             //Quantity = a.Quantity + a.FreeQuantity,
                             ItemId = a.ItemId,
                             ItemDetailsId = a.ItemDetailsId,
                             //PriceAddedFor = a.PriceAddedFor,
                             PriceAddedFor = 4,
                             AmountIncTax = a.AmountIncTax,
                             Quantity = a.QuantitySold,
                             Debit = a.QuantitySold,
                         }).ToList();

                #endregion

                #region Sales Combo
                SalesCombo = (from a in oConnectionContext.DbClsSalesDetails
                              join b in oConnectionContext.DbClsSales
                           on a.SalesId equals b.SalesId
                              join c in oConnectionContext.DbClsItem
                              on a.ItemId equals c.ItemId
                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                        && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                        && b.BranchId == obj.BranchId
                        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                            //&& a.UnitCost != 0
                            && b.Status != "Draft" && a.IsComboItems == true
                            && c.IsManageStock == true
                              select new
                              {
                                  a.AddedOn,
                                  b.SalesDate,
                                  b.InvoiceNo,
                                  a.ItemId,
                                  a.ItemDetailsId,
                                  a.AmountIncTax,
                                  a.QuantitySold
                              })
                  .AsEnumerable()
                  .Select(x => new ClsStockDetails
                  {
                      DetailsDate = x.AddedOn.AddMilliseconds(2),
                      Type = "Sales Combo",
                      Date = x.SalesDate,
                      ReferenceNo = x.InvoiceNo,
                      ItemId = x.ItemId,
                      ItemDetailsId = x.ItemDetailsId,
                      PriceAddedFor = 4,
                      AmountIncTax = x.AmountIncTax,
                      Quantity = x.QuantitySold,
                      Debit = x.QuantitySold
                  }).ToList();

                #endregion

                #region Sales Return
                SalesReturn = (from a in oConnectionContext.DbClsSalesReturnDetails
                               join b in oConnectionContext.DbClsSalesReturn
                            on a.SalesReturnId equals b.SalesReturnId
                               join p in oConnectionContext.DbClsSales
                                            on b.SalesId equals p.SalesId
                               join c in oConnectionContext.DbClsItem
         on a.ItemId equals c.ItemId
                               where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                               && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                              && p.BranchId == obj.BranchId
                              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                  && a.UnitCost != 0 && b.Status.ToLower() != "draft"
                                 && a.IsComboItems == false && c.IsManageStock == true
                               select new ClsStockDetails
                               {
                                   DetailsDate = a.AddedOn,
                                   Type = "Sales Return",
                                   Date = b.Date,
                                   ReferenceNo = b.InvoiceNo,
                                   //Quantity = a.Quantity + a.FreeQuantity,
                                   ItemId = a.ItemId,
                                   ItemDetailsId = a.ItemDetailsId,
                                   //PriceAddedFor = a.PriceAddedFor,
                                   PriceAddedFor = 4,
                                   AmountIncTax = a.AmountIncTax,
                                   Quantity = a.QuantityReturned,
                                   Credit = a.QuantityReturned
                               }).ToList();

                #endregion

                #region Sales Return Combo
                SalesReturnCombo = (from a in oConnectionContext.DbClsSalesReturnDetails
                                    join b in oConnectionContext.DbClsSalesReturn
                                 on a.SalesReturnId equals b.SalesReturnId
                                    join p in oConnectionContext.DbClsSales
                                                 on b.SalesId equals p.SalesId
                                    join c in oConnectionContext.DbClsItem
              on a.ItemId equals c.ItemId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                               && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                               && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                              && p.BranchId == obj.BranchId
                              && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                  DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                  //&& a.UnitCost != 0 
                                  && b.Status.ToLower() != "draft"
                                 && a.IsComboItems == true && c.IsManageStock == true
                                    select new
                                    {
                                        a.AddedOn,
                                        b.Date,
                                        b.InvoiceNo,
                                        a.ItemId,
                                        a.ItemDetailsId,
                                        a.AmountIncTax,
                                        a.QuantityReturned
                                    })
                  .AsEnumerable()
                  .Select(x => new ClsStockDetails
                  {
                      DetailsDate = x.AddedOn.AddMilliseconds(2),
                      Type = "Sales Return Combo",
                      Date = x.Date,
                      ReferenceNo = x.InvoiceNo,
                      ItemId = x.ItemId,
                      ItemDetailsId = x.ItemDetailsId,
                      PriceAddedFor = 4,
                      AmountIncTax = x.AmountIncTax,
                      Quantity = x.QuantityReturned,
                      Credit = x.QuantityReturned
                  }).ToList();

                #endregion

                #region Purchase
                Purchase = (from a in oConnectionContext.DbClsPurchaseDetails
                            join b in oConnectionContext.DbClsPurchase
                         on a.PurchaseId equals b.PurchaseId
                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                            && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                             && b.BranchId == obj.BranchId
                           && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                               && a.UnitCost != 0 && b.Status != "Draft"
                            select new ClsStockDetails
                            {
                                DetailsDate = a.AddedOn,
                                Type = "Purchase",
                                Date = b.PurchaseDate,
                                ReferenceNo = b.ReferenceNo,
                                //Quantity = a.Quantity + a.FreeQuantity,
                                ItemId = a.ItemId,
                                ItemDetailsId = a.ItemDetailsId,
                                //PriceAddedFor = a.PriceAddedFor,
                                PriceAddedFor = 4,
                                AmountIncTax = a.AmountIncTax,
                                Quantity = a.QuantityPurchased,
                                Credit = a.QuantityPurchased,
                            }).ToList();
                #endregion

                #region Purchase Expiry
                PurchaseExpiry = (from a in oConnectionContext.DbClsPurchaseDetails
                                  join b in oConnectionContext.DbClsPurchase
                               on a.PurchaseId equals b.PurchaseId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                                   && b.BranchId == obj.BranchId
                                 && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                     && a.UnitCost != 0 && b.Status != "Draft" && a.IsStopSelling == true
                                  select new ClsStockDetails
                                  {
                                      DetailsDate = a.AddedOn,
                                      Type = "Purchase Expired",
                                      Date = a.ExpiryDate.Value,
                                      ReferenceNo = b.ReferenceNo,
                                      //Quantity = a.Quantity + a.FreeQuantity,
                                      ItemId = a.ItemId,
                                      ItemDetailsId = a.ItemDetailsId,
                                      //PriceAddedFor = a.PriceAddedFor,
                                      PriceAddedFor = 4,
                                      AmountIncTax = a.AmountIncTax,
                                      Quantity = a.QuantityRemaining,
                                      Debit = a.QuantityRemaining
                                  }).ToList();
                #endregion

                #region Purchase Return
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                  join b in oConnectionContext.DbClsPurchaseReturn
                               on a.PurchaseReturnId equals b.PurchaseReturnId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.IsCancelled == false
                                 && b.BranchId == obj.BranchId
                                 && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                     && a.UnitCost != 0 && b.Status.ToLower() != "draft"
                                  select new ClsStockDetails
                                  {
                                      DetailsDate = a.AddedOn,
                                      Type = "Purchase Return",
                                      Date = b.Date,
                                      ReferenceNo = b.InvoiceNo,
                                      //Quantity = a.Quantity + a.FreeQuantity,
                                      ItemId = a.ItemId,
                                      ItemDetailsId = a.ItemDetailsId,
                                      //PriceAddedFor = a.PriceAddedFor,
                                      PriceAddedFor = 4,
                                      AmountIncTax = a.AmountIncTax,
                                      Quantity = a.QuantityReturned,
                                      Debit = a.QuantityReturned
                                  }).ToList();

                #endregion

                #region Stock Adjustment Credit
                StockAdjustmentCredit = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                                         join b in oConnectionContext.DbClsStockAdjustment
                            on a.StockAdjustmentId equals b.StockAdjustmentId
                                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                              && b.IsDeleted == false && b.IsActive == true
                              && b.BranchId == obj.BranchId
                              && DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                  DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                  && b.AdjustmentType.ToLower() == "credit"
                                         select new ClsStockDetails
                                         {
                                             DetailsDate = a.AddedOn,
                                             Type = "Stock Adjustment Credit",
                                             Date = b.AdjustmentDate,
                                             ReferenceNo = b.ReferenceNo,
                                             //Quantity = a.Quantity,
                                             ItemId = a.ItemId,
                                             ItemDetailsId = a.ItemDetailsId,
                                             //PriceAddedFor = a.PriceAddedFor,
                                             PriceAddedFor = 4,
                                             AmountIncTax = a.Amount,
                                             Quantity = a.QuantityAdjusted,
                                             Credit = a.QuantityAdjusted
                                         }).ToList();

                #endregion

                #region Stock Adjustment Debit
                StockAdjustmentDebit = (from a in oConnectionContext.DbClsStockAdjustmentDetails
                                        join b in oConnectionContext.DbClsStockAdjustment
                           on a.StockAdjustmentId equals b.StockAdjustmentId
                                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                             && b.IsDeleted == false && b.IsActive == true
                             && b.BranchId == obj.BranchId
                             && DbFunctions.TruncateTime(b.AdjustmentDate) >= obj.FromDate &&
                                 DbFunctions.TruncateTime(b.AdjustmentDate) <= obj.ToDate
                                 && b.AdjustmentType.ToLower() == "debit"
                                        select new ClsStockDetails
                                        {
                                            DetailsDate = a.AddedOn,
                                            Type = "Stock Adjustment Debit",
                                            Date = b.AdjustmentDate,
                                            ReferenceNo = b.ReferenceNo,
                                            //Quantity = a.Quantity,
                                            ItemId = a.ItemId,
                                            ItemDetailsId = a.ItemDetailsId,
                                            //PriceAddedFor = a.PriceAddedFor,
                                            PriceAddedFor = 4,
                                            AmountIncTax = a.Amount,
                                            Quantity = a.QuantityAdjusted,
                                            Debit = a.QuantityAdjusted
                                        }).ToList();

                #endregion

                #region Stock Transfer
                StockTransfer = (from a in oConnectionContext.DbClsStockTransferDetails
                                 join b in oConnectionContext.DbClsStockTransfer
                    on a.StockTransferId equals b.StockTransferId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                      && b.IsDeleted == false && b.IsActive == true
                     && b.FromBranchId == obj.BranchId
                      && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                          && b.Status == 3
                                 select new ClsStockDetails
                                 {
                                     DetailsDate = a.AddedOn,
                                     Type = "Stock Transfer",
                                     Date = b.Date,
                                     ReferenceNo = b.ReferenceNo,
                                     //Quantity = a.Quantity,
                                     ItemId = a.ItemId,
                                     ItemDetailsId = a.ItemDetailsId,
                                     //PriceAddedFor = a.PriceAddedFor,
                                     PriceAddedFor = 4,
                                     AmountIncTax = a.Amount,
                                     Quantity = a.QuantityTransferred,
                                     Debit = a.QuantityTransferred
                                 }).ToList();

                #endregion

                #region Stock Received
                StockReceived = (from a in oConnectionContext.DbClsStockTransferDetails
                                 join b in oConnectionContext.DbClsStockTransfer
                    on a.StockTransferId equals b.StockTransferId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                      && b.IsDeleted == false && b.IsActive == true
                     && b.ToBranchId == obj.BranchId
                      && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                          && b.Status == 3
                                 select new ClsStockDetails
                                 {
                                     DetailsDate = a.AddedOn,
                                     Type = "Stock Received",
                                     Date = b.Date,
                                     ReferenceNo = b.ReferenceNo,
                                     //Quantity = a.Quantity,
                                     ItemId = a.ItemId,
                                     ItemDetailsId = a.ItemDetailsId,
                                     //PriceAddedFor = a.PriceAddedFor,
                                     PriceAddedFor = 4,
                                     AmountIncTax = a.Amount,
                                     Quantity = a.QuantityTransferred,
                                     Credit = a.QuantityTransferred
                                 }).ToList();

                #endregion

                Ledger = Sales.Concat(SalesCombo).Concat(SalesReturn).Concat(SalesReturnCombo).Concat(Purchase)
                    .Concat(PurchaseExpiry).Concat(PurchaseReturn).Concat(StockAdjustmentCredit)
                    .Concat(StockAdjustmentDebit).Concat(StockTransfer).Concat(StockReceived).ToList();
            }
            return Ledger;
        }

        public void CreditNoteCreditsApply(ClsCustomerPaymentVm obj)
        {
            #region delete previous credit
            var CustomerPayment = oConnectionContext.DbClsCustomerPayment.Where(a => a.SalesReturnId == obj.SalesReturnId &&
            a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            {
                a.Type,
                a.Amount,
                a.CustomerId,
                a.SalesId,
                a.AmountRemaining,
                a.CustomerPaymentId
            }).FirstOrDefault();

            if (CustomerPayment != null)
            {
                obj.CustomerPaymentId = CustomerPayment.CustomerPaymentId;
                if (CustomerPayment.Type.ToLower() == "customer payment")
                {
                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + CustomerPayment.AmountRemaining + " where \"UserId\"=" + CustomerPayment.CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                }

                ClsCustomerPayment _oClsCustomerPayment = new ClsCustomerPayment()
                {
                    CustomerPaymentId = obj.CustomerPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate(obj.CompanyId),
                };
                oConnectionContext.DbClsCustomerPayment.Attach(_oClsCustomerPayment);
                oConnectionContext.Entry(_oClsCustomerPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(_oClsCustomerPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(_oClsCustomerPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                decimal UsedAmount = 0;
                //string paymentIds = oConnectionContext.DbClsCustomerPayment.
                //                    Where(a => a.CustomerPaymentId == obj.CustomerPaymentId).Select(a => a.PaymentIds).FirstOrDefault() ?? "[]";
                //List<ClsCustomerPaymentIds> _paymentIds = serializer.Deserialize<List<ClsCustomerPaymentIds>>(paymentIds);

                List<ClsCustomerPaymentIds> _paymentIds = oConnectionContext.DbClsCustomerPayment.Where(a =>
                a.ParentId == obj.CustomerPaymentId).Select(a => new ClsCustomerPaymentIds
                {
                    CustomerPaymentId = a.CustomerPaymentId,
                    Type = a.Type,
                }).ToList();

                if (_paymentIds != null)
                {
                    foreach (var item in _paymentIds)
                    {
                        if (item.Type.ToLower() == "customer opening balance payment")
                        {
                            ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                            {
                                CustomerPaymentId = item.CustomerPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate(obj.CompanyId),
                            };
                            oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                            {
                                CustomerPaymentId = item.CustomerPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate(obj.CompanyId),
                            };
                            oConnectionContext.DbClsCustomerPayment.Attach(oClsCustomerPayment1);
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsCustomerPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }

                        UsedAmount = UsedAmount + item.Amount;

                        string query = "", PaymentStatus = "";

                        if (item.Type.ToLower() == "sales payment")
                        {
                            int count = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == item.Type && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).Count();
                            if (count == 0)
                            {
                                PaymentStatus = "Due";
                            }
                            else
                            {
                                PaymentStatus = "Partially Paid";
                            }

                            query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            #region check OverDue Payment
                            var sale = (from a in oConnectionContext.DbClsSales
                                        where a.SalesId == item.SalesId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                        && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                        //&& b.PayTermNo != 0
                                        select new
                                        {
                                            a.SalesId,
                                            a.SalesDate,
                                            a.CustomerId,
                                            a.DueDate
                                            //b.PayTerm,
                                            //b.PayTermNo
                                        }).FirstOrDefault();

                            if (sale != null)
                            {
                                DateTime expDate = DateTime.Now;

                                //if (sale.DueDate < DateTime.Now)
                                if ((DateTime.Now - sale.DueDate).Days >= 1)
                                {
                                    string query1 = "update \"tblSales\" set \"Status\"='Overdue' where \"SalesId\"=" + item.SalesId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion

            #region Add Credit

            #endregion

            long CustomerPaymentId = 0;
            long PrefixUserMapId = 0;

            obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId &&
                a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();

            List<ClsCustomerPaymentIds> oClsCustomerPaymentIds = new List<ClsCustomerPaymentIds>();
            decimal RemainingAmount = obj.Amount;

            if (obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count() > 0)
            {
                //if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                //{
                //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixMasterId equals b.PrefixMasterId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Payment"
                //                          && b.PrefixId == PrefixId
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
       && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate(obj.CompanyId),
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    CustomerId = obj.CustomerId,
                    AttachDocument = obj.AttachDocument,
                    Type = obj.Type,
                    BranchId = obj.BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 1,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = CreateToken(),
                    //PaymentIds = _json
                    JournalAccountId = JournalAccountId,
                    PaymentLinkId = obj.PaymentLinkId,
                    TaxId = obj.TaxId,
                    TaxAccountId = 0,
                    AmountExcTax = obj.AmountExcTax,
                    TaxAmount = obj.TaxAmount,
                    AmountRemaining = obj.Amount - obj.CustomerPaymentIds.Sum(a => a.Amount),
                    AmountUsed = obj.CustomerPaymentIds.Sum(a => a.Amount),
                    SalesReturnId = obj.SalesReturnId,
                    SalesId = obj.SalesId
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Payment/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsCustomerPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment);
                oConnectionContext.SaveChanges();

                CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId;

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                foreach (var item in obj.CustomerPaymentIds)
                {
                    long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
      && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                    if (RemainingAmount > 0)
                    {
                        item.Due = oConnectionContext.DbClsSales.Where(a => a.SalesId == item.SalesId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                   (oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                   oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                   && b.SalesId == item.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                        decimal _amount = 0;
                        if (item.Due != 0)
                        {
                            if (item.Due >= RemainingAmount)
                            {
                                _amount = RemainingAmount;
                            }
                            else
                            {
                                _amount = item.Due;
                            }

                            if (item.Type.ToLower() == "customer opening balance payment")
                            {
                                ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate(obj.CompanyId),
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = "",
                                    Amount = _amount,
                                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.PaymentTypeId,
                                    CustomerId = obj.CustomerId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = item.Type,
                                    BranchId = obj.BranchId,
                                    AccountId = JournalAccountId,
                                    //ReferenceNo = ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ParentId = oClsCustomerPayment.CustomerPaymentId,
                                    ReferenceId = CreateToken(),
                                    JournalAccountId = AccountId,
                                    PaymentLinkId = obj.PaymentLinkId,
                                    TaxId = obj.TaxId,
                                    TaxAccountId = 0,
                                    AmountExcTax = _amount,
                                    TaxAmount = 0,
                                };
                                oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment1);
                                oConnectionContext.SaveChanges();

                                oClsCustomerPaymentIds.Add(new ClsCustomerPaymentIds { CustomerPaymentId = oClsCustomerPayment1.CustomerPaymentId, CustomerId = item.CustomerId, SalesId = item.SalesId, Type = item.Type, Amount = _amount });
                            }
                            else
                            {
                                ClsCustomerPayment oClsCustomerPayment1 = new ClsCustomerPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate(obj.CompanyId),
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = "",
                                    Amount = _amount,
                                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.PaymentTypeId,
                                    CustomerId = obj.CustomerId,
                                    SalesId = item.SalesId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = item.Type,
                                    BranchId = obj.BranchId,
                                    AccountId = JournalAccountId,
                                    //ReferenceNo = ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ParentId = oClsCustomerPayment.CustomerPaymentId,
                                    ReferenceId = CreateToken(),
                                    JournalAccountId = AccountId,
                                    PaymentLinkId = obj.PaymentLinkId,
                                    TaxId = obj.TaxId,
                                    TaxAccountId = 0,
                                    AmountExcTax = _amount,
                                    TaxAmount = 0,
                                };
                                oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment1);
                                oConnectionContext.SaveChanges();

                                oClsCustomerPaymentIds.Add(new ClsCustomerPaymentIds { CustomerPaymentId = oClsCustomerPayment1.CustomerPaymentId, CustomerId = item.CustomerId, SalesId = item.SalesId, Type = item.Type, Amount = _amount });
                            }

                            RemainingAmount = RemainingAmount - _amount;

                            if (item.Type == "Sales Payment")
                            {
                                string PaymentStatus = "";
                                decimal GrandTotal = oConnectionContext.DbClsSales.Where(a => a.SalesId == item.SalesId).Select(a => a.GrandTotal).FirstOrDefault();

                                decimal previousPayments = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") &&
                               b.IsDeleted == false && b.IsCancelled == false && b.SalesId == item.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                                if (GrandTotal == (previousPayments))
                                {
                                    PaymentStatus = "Paid";
                                }
                                else if (GrandTotal > (previousPayments))
                                {
                                    PaymentStatus = "Partially Paid";
                                }

                                string query = "update \"tblSales\" set \"Status\"='" + PaymentStatus + "' where \"SalesId\"=" + item.SalesId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }

                //serializer.MaxJsonLength = 2147483644;
                //string _json = serializer.Serialize(oClsCustomerPaymentIds);

                //string r = "update \"tblCustomerPayment\" set \"PaymentIds\"='" + _json + "' where \"CustomerPaymentId\"=" + oClsCustomerPayment.CustomerPaymentId;
                //oConnectionContext.Database.ExecuteSqlCommand(r);

                string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + RemainingAmount + " where \"UserId\"=" + obj.CustomerId;
                oConnectionContext.Database.ExecuteSqlCommand(query1);

                string r = "update \"tblCustomerPayment\" set \"AmountRemaining\"=" + RemainingAmount + " where \"CustomerPaymentId\"=" + oClsCustomerPayment.CustomerPaymentId;
                oConnectionContext.Database.ExecuteSqlCommand(r);
            }
            else
            {
                //Customer Advance Payment
                //if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                //{
                //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixMasterId equals b.PrefixMasterId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Payment"
                //                          && b.PrefixId == PrefixId
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Type == "Deferred Income").Select(a => a.AccountId).FirstOrDefault();

                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.SalesAccountId).FirstOrDefault();

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                ClsCustomerPayment oClsCustomerPayment = new ClsCustomerPayment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate(obj.CompanyId),
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    CustomerId = obj.CustomerId,
                    AttachDocument = obj.AttachDocument,
                    //Type = obj.Type == "Customer Direct Advance Payment" ? "Customer Direct Advance Payment" : "Customer Advance Payment",
                    Type = obj.Type,
                    BranchId = obj.BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 1,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = CreateToken(),
                    //PaymentIds = _json
                    JournalAccountId = JournalAccountId,
                    AmountRemaining = obj.AmountExcTax,
                    PaymentLinkId = obj.PaymentLinkId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    TaxId = obj.TaxId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    TaxAccountId = TaxAccountId,
                    AmountExcTax = obj.AmountExcTax,
                    TaxAmount = obj.TaxAmount,
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Payment/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsCustomerPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsCustomerPayment.Add(oClsCustomerPayment);
                oConnectionContext.SaveChanges();

                CustomerPaymentId = oClsCustomerPayment.CustomerPaymentId;

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + obj.Amount + " where \"UserId\"=" + obj.CustomerId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter
            }

            if (obj.PaymentLinkId != 0)
            {
                string r = "update \"tblPaymentLink\" set \"Status\"= 'Paid' where \"PaymentLinkId\"=" + obj.PaymentLinkId;
                oConnectionContext.Database.ExecuteSqlCommand(r);
            }

            if (obj.SalesReturnId != 0)
            {
                CustomerPaymentId = oConnectionContext.DbClsCustomerPayment.Where(b => b.SalesReturnId == obj.SalesReturnId && b.IsDeleted == false && b.IsCancelled == false
                       && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.CustomerPaymentId).FirstOrDefault();

                decimal AmountRemaining = oConnectionContext.DbClsCustomerPayment.Where(a => a.CustomerPaymentId
                == CustomerPaymentId).Select(a => a.AmountRemaining).FirstOrDefault();

                if (AmountRemaining <= 0)
                {
                    string query = "update \"tblSalesReturn\" set \"Status\"='Closed' where \"SalesReturnId\"=" + obj.SalesReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblSalesReturn\" set \"Status\"='Open' where \"SalesReturnId\"=" + obj.SalesReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }
        }

        public void DebitNoteCreditsApply(ClsSupplierPaymentVm obj)
        {
            #region delete previous credit
            var SupplierPayment = oConnectionContext.DbClsSupplierPayment.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId &&
            a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            {
                a.Type,
                a.Amount,
                a.SupplierId,
                a.PurchaseId,
                a.AmountRemaining,
                a.SupplierPaymentId
            }).FirstOrDefault();

            if (SupplierPayment != null)
            {
                obj.SupplierPaymentId = SupplierPayment.SupplierPaymentId;
                if (SupplierPayment.Type.ToLower() == "supplier payment")
                {
                    string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + SupplierPayment.AmountRemaining + " where \"UserId\"=" + SupplierPayment.SupplierId;
                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                }

                ClsSupplierPayment _oClsSupplierPayment = new ClsSupplierPayment()
                {
                    SupplierPaymentId = obj.SupplierPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate(obj.CompanyId),
                };
                oConnectionContext.DbClsSupplierPayment.Attach(_oClsSupplierPayment);
                oConnectionContext.Entry(_oClsSupplierPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(_oClsSupplierPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(_oClsSupplierPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                decimal UsedAmount = 0;
                //string paymentIds = oConnectionContext.DbClsSupplierPayment.
                //                    Where(a => a.SupplierPaymentId == obj.SupplierPaymentId).Select(a => a.PaymentIds).FirstOrDefault() ?? "[]";
                //List<ClsSupplierPaymentIds> _paymentIds = serializer.Deserialize<List<ClsSupplierPaymentIds>>(paymentIds);

                List<ClsSupplierPaymentIds> _paymentIds = oConnectionContext.DbClsSupplierPayment.Where(a =>
               a.ParentId == obj.SupplierPaymentId).Select(a => new ClsSupplierPaymentIds
               {
                   SupplierPaymentId = a.SupplierPaymentId,
                   Type = a.Type,
               }).ToList();

                if (_paymentIds != null)
                {
                    foreach (var item in _paymentIds)
                    {
                        if (item.Type.ToLower() == "supplier opening balance payment")
                        {
                            ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                            {
                                SupplierPaymentId = item.SupplierPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate(obj.CompanyId),
                            };
                            oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                            {
                                SupplierPaymentId = item.SupplierPaymentId,
                                IsDeleted = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate(obj.CompanyId),
                            };
                            oConnectionContext.DbClsSupplierPayment.Attach(oClsSupplierPayment1);
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSupplierPayment1).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }

                        UsedAmount = UsedAmount + item.Amount;

                        string query = "", PaymentStatus = "";

                        if (item.Type.ToLower() == "purchase payment")
                        {
                            int count = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == item.Type && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).Count();
                            if (count == 0)
                            {
                                PaymentStatus = "Due";
                            }
                            else
                            {
                                PaymentStatus = "Partially Paid";
                            }

                            query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + item.PurchaseId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            #region check OverDue Payment
                            var sale = (from a in oConnectionContext.DbClsPurchase
                                        where a.PurchaseId == item.PurchaseId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                        && a.Status.ToLower() != "draft" && a.Status.ToLower() != "paid"
                                        //&& b.PayTermNo != 0
                                        select new
                                        {
                                            a.PurchaseId,
                                            a.PurchaseDate,
                                            a.SupplierId,
                                            a.DueDate
                                            //b.PayTerm,
                                            //b.PayTermNo
                                        }).FirstOrDefault();

                            if (sale != null)
                            {
                                DateTime expDate = DateTime.Now;

                                //if (sale.DueDate < DateTime.Now)
                                if ((DateTime.Now - sale.DueDate).Days >= 1)
                                {
                                    string query1 = "update \"tblPurchase\" set \"Status\"='Overdue' where \"PurchaseId\"=" + item.PurchaseId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query1);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion

            #region Add Credit

            #endregion
            long SupplierPaymentId = 0;
            long PrefixUserMapId = 0;

            obj.PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId &&
                a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();

            List<ClsSupplierPaymentIds> oClsSupplierPaymentIds = new List<ClsSupplierPaymentIds>();
            decimal RemainingAmount = obj.Amount;

            if (obj.SupplierPaymentIds != null && obj.SupplierPaymentIds.Count() > 0)
            {
                //if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                //{
                //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixMasterId equals b.PrefixMasterId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Payment"
                //                          && b.PrefixId == PrefixId
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
       && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate(obj.CompanyId),
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    SupplierId = obj.SupplierId,
                    AttachDocument = obj.AttachDocument,
                    Type = obj.Type,
                    BranchId = obj.BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 1,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = CreateToken(),
                    //PaymentIds = _json
                    JournalAccountId = JournalAccountId,
                    TaxId = obj.TaxId,
                    TaxAccountId = 0,
                    AmountExcTax = obj.AmountExcTax,
                    TaxAmount = obj.TaxAmount,
                    AmountRemaining = obj.Amount - obj.SupplierPaymentIds.Sum(a => a.Amount),
                    AmountUsed = obj.SupplierPaymentIds.Sum(a => a.Amount),
                    PurchaseReturnId = obj.PurchaseReturnId,
                    PurchaseId = obj.PurchaseId,
                    IsReverseCharge = obj.IsReverseCharge
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Payment/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupplierPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment);
                oConnectionContext.SaveChanges();

                SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId;

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                foreach (var item in obj.SupplierPaymentIds)
                {
                    long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
      && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                    if (RemainingAmount > 0)
                    {
                        item.Due = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == item.PurchaseId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                  (oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false
                  && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                  oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                  && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                        decimal _amount = 0;
                        if (item.Due != 0)
                        {
                            if (item.Due >= RemainingAmount)
                            {
                                _amount = RemainingAmount;
                            }
                            else
                            {
                                _amount = item.Due;
                            }

                            if (item.Type.ToLower() == "supplier opening balance payment")
                            {
                                ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate(obj.CompanyId),
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = "",
                                    Amount = _amount,
                                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.PaymentTypeId,
                                    SupplierId = obj.SupplierId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = item.Type,
                                    BranchId = obj.BranchId,
                                    AccountId = JournalAccountId,
                                    //ReferenceNo = ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ParentId = oClsSupplierPayment.SupplierPaymentId,
                                    ReferenceId = CreateToken(),
                                    JournalAccountId = AccountId,
                                    TaxId = obj.TaxId,
                                    TaxAccountId = 0,
                                    AmountExcTax = _amount,
                                    TaxAmount = 0,
                                };
                                oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment1);
                                oConnectionContext.SaveChanges();

                                oClsSupplierPaymentIds.Add(new ClsSupplierPaymentIds { SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId, SupplierId = item.SupplierId, PurchaseId = item.PurchaseId, Type = item.Type, Amount = _amount });
                            }
                            else
                            {
                                ClsSupplierPayment oClsSupplierPayment1 = new ClsSupplierPayment()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate(obj.CompanyId),
                                    CompanyId = obj.CompanyId,
                                    IsActive = obj.IsActive,
                                    IsDeleted = obj.IsDeleted,
                                    Notes = "",
                                    Amount = _amount,
                                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.PaymentTypeId,
                                    SupplierId = obj.SupplierId,
                                    PurchaseId = item.PurchaseId,
                                    AttachDocument = obj.AttachDocument,
                                    Type = item.Type,
                                    BranchId = obj.BranchId,
                                    AccountId = JournalAccountId,
                                    //ReferenceNo = ReferenceNo,
                                    IsDebit = 2,
                                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                                    ParentId = oClsSupplierPayment.SupplierPaymentId,
                                    ReferenceId = CreateToken(),
                                    JournalAccountId = AccountId,
                                    TaxId = obj.TaxId,
                                    TaxAccountId = 0,
                                    AmountExcTax = _amount,
                                    TaxAmount = 0,
                                };
                                oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment1);
                                oConnectionContext.SaveChanges();

                                oClsSupplierPaymentIds.Add(new ClsSupplierPaymentIds { SupplierPaymentId = oClsSupplierPayment1.SupplierPaymentId, SupplierId = item.SupplierId, PurchaseId = item.PurchaseId, Type = item.Type, Amount = _amount });
                            }

                            RemainingAmount = RemainingAmount - _amount;

                            if (item.Type == "Purchase Payment")
                            {
                                string PaymentStatus = "";
                                decimal GrandTotal = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == item.PurchaseId).Select(a => a.GrandTotal).FirstOrDefault();

                                decimal previousPayments = oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") &&
                               b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == item.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                                if (GrandTotal == (previousPayments))
                                {
                                    PaymentStatus = "Paid";
                                }
                                else if (GrandTotal > (previousPayments))
                                {
                                    PaymentStatus = "Partially Paid";
                                }

                                string query = "update \"tblPurchase\" set \"Status\"='" + PaymentStatus + "' where \"PurchaseId\"=" + item.PurchaseId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }

                //serializer.MaxJsonLength = 2147483644;
                //string _json = serializer.Serialize(oClsSupplierPaymentIds);

                //string r = "update \"tblSupplierPayment\" set \"PaymentIds\"='" + _json + "' where \"SupplierPaymentId\"=" + oClsSupplierPayment.SupplierPaymentId;
                //oConnectionContext.Database.ExecuteSqlCommand(r);

                string query1 = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + RemainingAmount + " where \"UserId\"=" + obj.SupplierId;
                oConnectionContext.Database.ExecuteSqlCommand(query1);

                string r = "update \"tblSupplierPayment\" set \"AmountRemaining\"=" + RemainingAmount + " where \"SupplierPaymentId\"=" + oClsSupplierPayment.SupplierPaymentId;
                oConnectionContext.Database.ExecuteSqlCommand(r);
            }
            else
            {
                //Supplier Advance Payment
                //if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                //{
                //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixMasterId equals b.PrefixMasterId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Payment"
                //                          && b.PrefixId == PrefixId
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Type == "Prepaid Expenses").Select(a => a.AccountId).FirstOrDefault();

                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                ClsSupplierPayment oClsSupplierPayment = new ClsSupplierPayment()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate(obj.CompanyId),
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    SupplierId = obj.SupplierId,
                    AttachDocument = obj.AttachDocument,
                    //Type = obj.Type == "Supplier Direct Advance Payment" ? "Supplier Direct Advance Payment" : "Supplier Advance Payment",
                    Type = obj.Type,
                    BranchId = obj.BranchId,
                    AccountId = obj.AccountId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 1,
                    //OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    ReferenceId = CreateToken(),
                    //PaymentIds = _json
                    JournalAccountId = JournalAccountId,
                    AmountRemaining = obj.AmountExcTax,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    TaxId = obj.TaxId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    TaxAccountId = TaxAccountId,
                    AmountExcTax = obj.AmountExcTax,
                    TaxAmount = obj.TaxAmount,
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Payment/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Payment/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsSupplierPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsSupplierPayment.Add(oClsSupplierPayment);
                oConnectionContext.SaveChanges();

                SupplierPaymentId = oClsSupplierPayment.SupplierPaymentId;

                string query = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + obj.Amount + " where \"UserId\"=" + obj.SupplierId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter
            }

            if (obj.PurchaseReturnId != 0)
            {
                SupplierPaymentId = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseReturnId == obj.PurchaseReturnId && b.IsDeleted == false && b.IsCancelled == false
                       && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.SupplierPaymentId).FirstOrDefault();

                decimal AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(a => a.SupplierPaymentId
                == SupplierPaymentId).Select(a => a.AmountRemaining).FirstOrDefault();

                if (AmountRemaining <= 0)
                {
                    string query = "update \"tblPurchaseReturn\" set \"Status\"='Closed' where \"PurchaseReturnId\"=" + obj.PurchaseReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblPurchaseReturn\" set \"Status\"='Open' where \"PurchaseReturnId\"=" + obj.PurchaseReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
            }

        }

        public List<ClsSalesVm> Gstr4Sec4A(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var PurchaseBill = (from c in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                                on c.PurchaseId equals b.PurchaseId
                                where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
           || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
           || b.GstTreatment == "Tax Deductor")
                    && b.IsReverseCharge == 2 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                select new
                                {
                                    SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                    BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                    BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                    InvoiceNo = b.ReferenceNo,
                                    SalesDate = b.PurchaseDate,
                                    StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                    GrandTotal = b.GrandTotal,
                                    SalesType = "Purchase Bill",
                                    DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                    AmountExcTax = c.AmountExcTax,
                                    c.TaxId,
                                    TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                    TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                select new ClsTaxTypeVm
                                                {
                                                    TaxTypeId = x.TaxTypeId,
                                                    TaxType = x.TaxType,
                                                    TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                 join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                 on y.TaxId equals z.TaxId
                                                                 where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                 && z.PurchaseId == c.PurchaseId && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                 && z.PurchaseTaxJournalType == "Normal"
                                                                 select z.TaxAmount
                                                                ).DefaultIfEmpty().Sum()
                                                }).ToList()
                                }).ToList();

            var PurchaseBillAdditional = (from c in oConnectionContext.DbClsPurchaseAdditionalCharges
                                          join b in oConnectionContext.DbClsPurchase
                                          on c.PurchaseId equals b.PurchaseId
                                          where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                          && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                          l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                          && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
                     || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
                     || b.GstTreatment == "Tax Deductor")
                              && b.IsReverseCharge == 2 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                          select new
                                          {
                                              SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                              BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                              BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                              InvoiceNo = b.ReferenceNo,
                                              SalesDate = b.PurchaseDate,
                                              StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                              GrandTotal = b.GrandTotal,
                                              SalesType = "Purchase Bill",
                                              DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                              AmountExcTax = c.AmountExcTax,
                                              c.TaxId,
                                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                          select new ClsTaxTypeVm
                                                          {
                                                              TaxTypeId = x.TaxTypeId,
                                                              TaxType = x.TaxType,
                                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                           join z in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                                           on y.TaxId equals z.TaxId
                                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                           && z.PurchaseId == c.PurchaseId && z.PurchaseAdditionalChargesId == c.PurchaseAdditionalChargesId
                                                                           && z.PurchaseTaxJournalType == "Normal"
                                                                           select z.TaxAmount
                                                                          ).DefaultIfEmpty().Sum()
                                                          }).ToList()
                                          }).ToList();

            PurchaseBill.AddRange(PurchaseBillAdditional);

            var groupedPurchaseBill = PurchaseBill
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();


            var Expenses = (from c in oConnectionContext.DbClsExpensePayment
                            join b in oConnectionContext.DbClsExpense
                            on c.ExpenseId equals b.ExpenseId
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
            && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
       || b.GstTreatment == "Tax Deductor")
                && b.IsReverseCharge == 2 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                            select new
                            {
                                SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                InvoiceNo = b.ReferenceNo,
                                SalesDate = b.Date,
                                StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                GrandTotal = b.GrandTotal,
                                SalesType = "Expense",
                                DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                AmountExcTax = c.AmountExcTax,
                                c.TaxId,
                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                            select new ClsTaxTypeVm
                                            {
                                                TaxTypeId = x.TaxTypeId,
                                                TaxType = x.TaxType,
                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                             on y.TaxId equals z.TaxId
                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                             && z.ExpenseId == c.ExpenseId && z.ExpensePaymentId == c.ExpensePaymentId
                                                             && z.ExpenseTaxJournalType == "Normal"
                                                             select z.TaxAmount
                                                            ).DefaultIfEmpty().Sum()
                                            }).ToList()
                            }).ToList();

            var groupedExpenses = Expenses
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var DebitNotes = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                              join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                              join e in oConnectionContext.DbClsPurchase
                              on b.PurchaseId equals e.PurchaseId
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.Date) <= obj.ToDate
              && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
         || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
         || b.GstTreatment == "Tax Deductor")
                  && b.IsReverseCharge == 2 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                              select new
                              {
                                  SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                  BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                  BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                  InvoiceNo = b.InvoiceNo,
                                  SalesDate = b.Date,
                                  StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                  GrandTotal = b.GrandTotal,
                                  SalesType = "Debit Note",
                                  DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                  AmountExcTax = c.AmountExcTax,
                                  c.TaxId,
                                  TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                  TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                              select new ClsTaxTypeVm
                                              {
                                                  TaxTypeId = x.TaxTypeId,
                                                  TaxType = x.TaxType,
                                                  TaxAmount = (from y in oConnectionContext.DbClsTax
                                                               join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                               on y.TaxId equals z.TaxId
                                                               where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                               && z.PurchaseReturnId == c.PurchaseReturnId && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                               && z.PurchaseReturnTaxJournalType == "Normal"
                                                               select z.TaxAmount
                                                              ).DefaultIfEmpty().Sum()
                                              }).ToList()
                              }).ToList();

            var DebitNotesAdditional = (from c in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                        join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                                        join e in oConnectionContext.DbClsPurchase
                                        on b.PurchaseId equals e.PurchaseId
                                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
                   || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
                   || b.GstTreatment == "Tax Deductor")
                            && b.IsReverseCharge == 2 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                        select new
                                        {
                                            SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                            BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                            InvoiceNo = b.InvoiceNo,
                                            SalesDate = b.Date,
                                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                            GrandTotal = b.GrandTotal,
                                            SalesType = "Debit Note",
                                            DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                            AmountExcTax = c.AmountExcTax,
                                            c.TaxId,
                                            TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                        select new ClsTaxTypeVm
                                                        {
                                                            TaxTypeId = x.TaxTypeId,
                                                            TaxType = x.TaxType,
                                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                         join z in oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal
                                                                         on y.TaxId equals z.TaxId
                                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                         && z.PurchaseReturnId == c.PurchaseReturnId
                                                                         && z.PurchaseReturnAdditionalChargesId == c.PurchaseReturnAdditionalChargesId
                                                                         && z.PurchaseReturnTaxJournalType == "Normal"
                                                                         select z.TaxAmount
                                                                        ).DefaultIfEmpty().Sum()
                                                        }).ToList()
                                        }).ToList();

            DebitNotes.AddRange(DebitNotesAdditional);

            var groupedDebitNotes = DebitNotes
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var det = groupedPurchaseBill
                .Union(groupedExpenses)
                .Union(groupedDebitNotes)
                .ToList();

            return det;
        }

        public List<ClsSalesVm> Gstr4Sec4B(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var PurchaseBill = (from c in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                                on c.PurchaseId equals b.PurchaseId
                                where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
           || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
           || b.GstTreatment == "Tax Deductor")
                    && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                select new
                                {
                                    SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                    BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                    BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                    InvoiceNo = b.ReferenceNo,
                                    SalesDate = b.PurchaseDate,
                                    StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                    GrandTotal = b.GrandTotal,
                                    SalesType = "Purchase Bill",
                                    DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                    AmountExcTax = c.AmountExcTax,
                                    c.TaxId,
                                    TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                    TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                select new ClsTaxTypeVm
                                                {
                                                    TaxTypeId = x.TaxTypeId,
                                                    TaxType = x.TaxType,
                                                    TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                 join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                 on y.TaxId equals z.TaxId
                                                                 where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                 && z.PurchaseId == c.PurchaseId && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                 && z.PurchaseTaxJournalType == "Reverse Charge"
                                                                 select z.TaxAmount
                                                                ).DefaultIfEmpty().Sum()
                                                }).ToList()
                                }).ToList();

            var PurchaseBillAdditional = (from c in oConnectionContext.DbClsPurchaseAdditionalCharges
                                          join b in oConnectionContext.DbClsPurchase
                                on c.PurchaseId equals b.PurchaseId
                                          where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                          && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                          l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                          && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
                     || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
                     || b.GstTreatment == "Tax Deductor")
                              && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                          select new
                                          {
                                              SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                              BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                              BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                              InvoiceNo = b.ReferenceNo,
                                              SalesDate = b.PurchaseDate,
                                              StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                              GrandTotal = b.GrandTotal,
                                              SalesType = "Purchase Bill",
                                              DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                              AmountExcTax = c.AmountExcTax,
                                              c.TaxId,
                                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                          select new ClsTaxTypeVm
                                                          {
                                                              TaxTypeId = x.TaxTypeId,
                                                              TaxType = x.TaxType,
                                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                           join z in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                                           on y.TaxId equals z.TaxId
                                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                           && z.PurchaseId == c.PurchaseId && z.PurchaseAdditionalChargesId == c.PurchaseAdditionalChargesId
                                                                           && z.PurchaseTaxJournalType == "Reverse Charge"
                                                                           select z.TaxAmount
                                                                          ).DefaultIfEmpty().Sum()
                                                          }).ToList()
                                          }).ToList();

            PurchaseBill.AddRange(PurchaseBillAdditional);

            var groupedPurchaseBill = PurchaseBill
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();


            var Expenses = (from c in oConnectionContext.DbClsExpensePayment
                            join b in oConnectionContext.DbClsExpense
                            on c.ExpenseId equals b.ExpenseId
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
            && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
       || b.GstTreatment == "Tax Deductor")
                && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                            select new
                            {
                                SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                InvoiceNo = b.ReferenceNo,
                                SalesDate = b.Date,
                                StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                GrandTotal = b.GrandTotal,
                                SalesType = "Expense",
                                DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                AmountExcTax = c.AmountExcTax,
                                c.TaxId,
                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                            select new ClsTaxTypeVm
                                            {
                                                TaxTypeId = x.TaxTypeId,
                                                TaxType = x.TaxType,
                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                             on y.TaxId equals z.TaxId
                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                             && z.ExpenseId == c.ExpenseId && z.ExpensePaymentId == c.ExpensePaymentId
                                                             && z.ExpenseTaxJournalType == "Reverse Charge"
                                                             select z.TaxAmount
                                                            ).DefaultIfEmpty().Sum()
                                            }).ToList()
                            }).ToList();

            var groupedExpenses = Expenses
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var DebitNotes = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                              join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                              join e in oConnectionContext.DbClsPurchase
                              on b.PurchaseId equals e.PurchaseId
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.Date) <= obj.ToDate
              && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
         || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
         || b.GstTreatment == "Tax Deductor")
                  && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                              select new
                              {
                                  SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                  BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                  BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                  InvoiceNo = b.InvoiceNo,
                                  SalesDate = b.Date,
                                  StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                  GrandTotal = b.GrandTotal,
                                  SalesType = "Debit Note",
                                  DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                  AmountExcTax = c.AmountExcTax,
                                  c.TaxId,
                                  TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                  TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                              select new ClsTaxTypeVm
                                              {
                                                  TaxTypeId = x.TaxTypeId,
                                                  TaxType = x.TaxType,
                                                  TaxAmount = (from y in oConnectionContext.DbClsTax
                                                               join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                               on y.TaxId equals z.TaxId
                                                               where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                               && z.PurchaseReturnId == c.PurchaseReturnId && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                               && z.PurchaseReturnTaxJournalType == "Reverse Charge"
                                                               select z.TaxAmount
                                                              ).DefaultIfEmpty().Sum()
                                              }).ToList()
                              }).ToList();

            var DebitNotesAdditional = (from c in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                        join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                                        join e in oConnectionContext.DbClsPurchase
                                        on b.PurchaseId equals e.PurchaseId
                                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
                   || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
                   || b.GstTreatment == "Tax Deductor")
                            && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                        select new
                                        {
                                            SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                            BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                            InvoiceNo = b.InvoiceNo,
                                            SalesDate = b.Date,
                                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                            GrandTotal = b.GrandTotal,
                                            SalesType = "Debit Note",
                                            DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                            AmountExcTax = c.AmountExcTax,
                                            c.TaxId,
                                            TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                        select new ClsTaxTypeVm
                                                        {
                                                            TaxTypeId = x.TaxTypeId,
                                                            TaxType = x.TaxType,
                                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                         join z in oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal
                                                                         on y.TaxId equals z.TaxId
                                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                         && z.PurchaseReturnId == c.PurchaseReturnId
                                                                         && z.PurchaseReturnAdditionalChargesId == c.PurchaseReturnAdditionalChargesId
                                                                         && z.PurchaseReturnTaxJournalType == "Reverse Charge"
                                                                         select z.TaxAmount
                                                                        ).DefaultIfEmpty().Sum()
                                                        }).ToList()
                                        }).ToList();

            DebitNotes.AddRange(DebitNotesAdditional);

            var groupedDebitNotes = DebitNotes
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var AdvancePayments = (from b in oConnectionContext.DbClsSupplierPayment
                                   where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                   && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                   l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                  && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                  && b.Type == "Supplier Payment" && b.ParentId == 0 && b.IsDeleted == false && b.PurchaseReturnId == 0
                   && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
            || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
            || b.GstTreatment == "Tax Deductor")
                         && b.IsReverseCharge == 1 && (b.TaxId != ExemptedId || b.TaxId != NonGstId)
                                   select new
                                   {
                                       SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                       BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                       BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                       InvoiceNo = b.ReferenceNo,
                                       SalesDate = b.PaymentDate,
                                       StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                       GrandTotal = b.Amount - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                    ).Select(c => c.Amount).DefaultIfEmpty().Sum(),
                                       SalesType = "Supplier Payment",
                                       DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                       AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                    ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                                       TaxId = b.TaxId,
                                       TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                   select new ClsTaxTypeVm
                                                   {
                                                       TaxTypeId = x.TaxTypeId,
                                                       TaxType = x.TaxType,
                                                       TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                    join z in oConnectionContext.DbClsSupplierPaymentTaxJournal
                                                                    on y.TaxId equals z.TaxId
                                                                    where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                    && z.SupplierPaymentId == b.SupplierPaymentId
                                                                    && z.SupplierPaymentTaxJournalType == "Reverse Charge"
                                                                    select z.TaxAmount
                                                                   ).DefaultIfEmpty().Sum()
                                                   }).ToList()
                                   }).ToList();

            var groupedAdvancePayments = AdvancePayments
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var det = groupedPurchaseBill
                .Union(groupedExpenses)
                .Union(groupedDebitNotes)
                .Union(groupedAdvancePayments)
                .ToList();

            return det;
        }

        public List<ClsSalesVm> Gstr4Sec4C(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var PurchaseBill = (from c in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                                on c.PurchaseId equals b.PurchaseId
                                where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                //&& b.IsReverseCharge == 2 
                && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                select new
                                {
                                    SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                    PanNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.PanNo).FirstOrDefault(),
                                    BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                    IsReverseCharge = b.IsReverseCharge,
                                    InvoiceNo = b.ReferenceNo,
                                    SalesDate = b.PurchaseDate,
                                    StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                    GrandTotal = b.GrandTotal,
                                    SalesType = "Purchase Bill",
                                    DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                    AmountExcTax = c.AmountExcTax,
                                    c.TaxId,
                                    TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                    TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                select new ClsTaxTypeVm
                                                {
                                                    TaxTypeId = x.TaxTypeId,
                                                    TaxType = x.TaxType,
                                                    TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                 join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                 on y.TaxId equals z.TaxId
                                                                 where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                 && z.PurchaseId == c.PurchaseId && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                 && z.PurchaseTaxJournalType == "Normal"
                                                                 select z.TaxAmount
                                                                ).DefaultIfEmpty().Sum()
                                                }).ToList()
                                }).ToList();

            var PurchaseBillAdditional = (from c in oConnectionContext.DbClsPurchaseAdditionalCharges
                                          join b in oConnectionContext.DbClsPurchase
                                          on c.PurchaseId equals b.PurchaseId
                                          where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                          && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                          l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                      && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                          DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                          && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                          //&& b.IsReverseCharge == 2 
                          && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                          select new
                                          {
                                              SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                              PanNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.PanNo).FirstOrDefault(),
                                              BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                              IsReverseCharge = b.IsReverseCharge,
                                              InvoiceNo = b.ReferenceNo,
                                              SalesDate = b.PurchaseDate,
                                              StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                              GrandTotal = b.GrandTotal,
                                              SalesType = "Purchase Bill",
                                              DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                              AmountExcTax = c.AmountExcTax,
                                              c.TaxId,
                                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                          select new ClsTaxTypeVm
                                                          {
                                                              TaxTypeId = x.TaxTypeId,
                                                              TaxType = x.TaxType,
                                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                           join z in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                                           on y.TaxId equals z.TaxId
                                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                           && z.PurchaseId == c.PurchaseId && z.PurchaseAdditionalChargesId == c.PurchaseAdditionalChargesId
                                                                           && z.PurchaseTaxJournalType == "Normal"
                                                                           select z.TaxAmount
                                                                          ).DefaultIfEmpty().Sum()
                                                          }).ToList()
                                          }).ToList();

            PurchaseBill.AddRange(PurchaseBillAdditional);

            var groupedPurchaseBill = PurchaseBill
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                PanNo = g.Select(b => b.PanNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                IsReverseCharge = g.Select(b => b.IsReverseCharge).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();


            var Expenses = (from c in oConnectionContext.DbClsExpensePayment
                            join b in oConnectionContext.DbClsExpense
                            on c.ExpenseId equals b.ExpenseId
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
            && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
            //&& b.IsReverseCharge == 2 
            && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                            select new
                            {
                                SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                PanNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.PanNo).FirstOrDefault(),
                                BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                IsReverseCharge = b.IsReverseCharge,
                                InvoiceNo = b.ReferenceNo,
                                SalesDate = b.Date,
                                StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                GrandTotal = b.GrandTotal,
                                SalesType = "Expense",
                                DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                AmountExcTax = c.AmountExcTax,
                                c.TaxId,
                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                            select new ClsTaxTypeVm
                                            {
                                                TaxTypeId = x.TaxTypeId,
                                                TaxType = x.TaxType,
                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                             on y.TaxId equals z.TaxId
                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                             && z.ExpenseId == c.ExpenseId && z.ExpensePaymentId == c.ExpensePaymentId
                                                             && z.ExpenseTaxJournalType == "Normal"
                                                             select z.TaxAmount
                                                            ).DefaultIfEmpty().Sum()
                                            }).ToList()
                            }).ToList();

            var groupedExpenses = Expenses
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                PanNo = g.Select(b => b.PanNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                IsReverseCharge = g.Select(b => b.IsReverseCharge).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var DebitNotes = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                              join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                              join e in oConnectionContext.DbClsPurchase
                              on b.PurchaseId equals e.PurchaseId
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.Date) <= obj.ToDate
              && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
              //&& b.IsReverseCharge == 2 
              && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                              select new
                              {
                                  SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                  PanNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.PanNo).FirstOrDefault(),
                                  BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                  IsReverseCharge = b.IsReverseCharge,
                                  InvoiceNo = b.InvoiceNo,
                                  SalesDate = b.Date,
                                  StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                  GrandTotal = b.GrandTotal,
                                  SalesType = "Debit Note",
                                  DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                  AmountExcTax = c.AmountExcTax,
                                  c.TaxId,
                                  TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                  TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                              select new ClsTaxTypeVm
                                              {
                                                  TaxTypeId = x.TaxTypeId,
                                                  TaxType = x.TaxType,
                                                  TaxAmount = (from y in oConnectionContext.DbClsTax
                                                               join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                               on y.TaxId equals z.TaxId
                                                               where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                               && z.PurchaseReturnId == c.PurchaseReturnId && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                               && z.PurchaseReturnTaxJournalType == "Normal"
                                                               select z.TaxAmount
                                                              ).DefaultIfEmpty().Sum()
                                              }).ToList()
                              }).ToList();

            var DebitNotesAdditional = (from c in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                        join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                                        join e in oConnectionContext.DbClsPurchase
                                        on b.PurchaseId equals e.PurchaseId
                                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                        && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                        //&& b.IsReverseCharge == 2 
                        && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                                        select new
                                        {
                                            SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                            PanNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.PanNo).FirstOrDefault(),
                                            BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                            IsReverseCharge = b.IsReverseCharge,
                                            InvoiceNo = b.InvoiceNo,
                                            SalesDate = b.Date,
                                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                            GrandTotal = b.GrandTotal,
                                            SalesType = "Debit Note",
                                            DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                            AmountExcTax = c.AmountExcTax,
                                            c.TaxId,
                                            TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                        select new ClsTaxTypeVm
                                                        {
                                                            TaxTypeId = x.TaxTypeId,
                                                            TaxType = x.TaxType,
                                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                         join z in oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal
                                                                         on y.TaxId equals z.TaxId
                                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                         && z.PurchaseReturnId == c.PurchaseReturnId
                                                                         && z.PurchaseReturnAdditionalChargesId == c.PurchaseReturnAdditionalChargesId
                                                                         && z.PurchaseReturnTaxJournalType == "Normal"
                                                                         select z.TaxAmount
                                                                        ).DefaultIfEmpty().Sum()
                                                        }).ToList()
                                        }).ToList();

            DebitNotes.AddRange(DebitNotesAdditional);

            var groupedDebitNotes = DebitNotes
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                PanNo = g.Select(b => b.PanNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                IsReverseCharge = g.Select(b => b.IsReverseCharge).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var AdvancePayments = (from b in oConnectionContext.DbClsSupplierPayment
                                   where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                   && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                   l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                  && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                  && b.Type == "Supplier Payment" && b.ParentId == 0 && b.IsDeleted == false && b.PurchaseReturnId == 0
                  && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                         //&& b.IsReverseCharge == 2
                         && (b.TaxId != ExemptedId || b.TaxId != NonGstId)
                                   select new
                                   {
                                       SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                       BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                       BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                       InvoiceNo = b.ReferenceNo,
                                       SalesDate = b.PaymentDate,
                                       StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                       GrandTotal = b.Amount - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.Amount).DefaultIfEmpty().Sum(),
                                       SalesType = "Supplier Payment",
                                       DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                       AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                                       TaxId = b.TaxId,
                                       TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                   select new ClsTaxTypeVm
                                                   {
                                                       TaxTypeId = x.TaxTypeId,
                                                       TaxType = x.TaxType,
                                                       TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                    join z in oConnectionContext.DbClsSupplierPaymentTaxJournal
                                                                    on y.TaxId equals z.TaxId
                                                                    where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                    && z.SupplierPaymentId == b.SupplierPaymentId
                                                                    && z.SupplierPaymentTaxJournalType == "Reverse Charge"
                                                                    select z.TaxAmount
                                                                   ).DefaultIfEmpty().Sum()
                                                   }).ToList()
                                   }).ToList();

            var groupedAdvancePayments = AdvancePayments
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var det = groupedPurchaseBill
                .Union(groupedExpenses)
                .Union(groupedDebitNotes)
                .ToList();

            return det;
        }

        public List<ClsSalesVm> Gstr4Sec4D(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var PurchaseBill = (from c in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                                on c.PurchaseId equals b.PurchaseId
                                join f in oConnectionContext.DbClsItem
                                on c.ItemId equals f.ItemId
                                where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
                    && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                   && f.ItemType.ToLower() == "service"
                                select new
                                {
                                    InvoiceNo = b.ReferenceNo,
                                    SalesDate = b.PurchaseDate,
                                    StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                    GrandTotal = b.GrandTotal,
                                    SalesType = "Purchase Bill",
                                    DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                    AmountExcTax = c.AmountExcTax,
                                    c.TaxId,
                                    TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                    TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                select new ClsTaxTypeVm
                                                {
                                                    TaxTypeId = x.TaxTypeId,
                                                    TaxType = x.TaxType,
                                                    TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                 join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                 on y.TaxId equals z.TaxId
                                                                 where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                 && z.PurchaseId == c.PurchaseId && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                 && z.PurchaseTaxJournalType == "Reverse Charge"
                                                                 select z.TaxAmount
                                                                ).DefaultIfEmpty().Sum()
                                                }).ToList()
                                }).ToList();

            var groupedPurchaseBill = PurchaseBill
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();


            var Expenses = (from c in oConnectionContext.DbClsExpensePayment
                            join b in oConnectionContext.DbClsExpense
                            on c.ExpenseId equals b.ExpenseId
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.Date) <= obj.ToDate
            && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
                && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                && c.ItemType.ToLower() == "service"
                            select new
                            {
                                BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                InvoiceNo = b.ReferenceNo,
                                SalesDate = b.Date,
                                StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                GrandTotal = b.GrandTotal,
                                SalesType = "Expense",
                                DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                AmountExcTax = c.AmountExcTax,
                                c.TaxId,
                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                            select new ClsTaxTypeVm
                                            {
                                                TaxTypeId = x.TaxTypeId,
                                                TaxType = x.TaxType,
                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                             on y.TaxId equals z.TaxId
                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                             && z.ExpenseId == c.ExpenseId && z.ExpensePaymentId == c.ExpensePaymentId
                                                             && z.ExpenseTaxJournalType == "Reverse Charge"
                                                             select z.TaxAmount
                                                            ).DefaultIfEmpty().Sum()
                                            }).ToList()
                            }).ToList();

            var groupedExpenses = Expenses
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var DebitNotes = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                              join b in oConnectionContext.DbClsPurchaseReturn
                              on c.PurchaseReturnId equals b.PurchaseReturnId
                              join e in oConnectionContext.DbClsPurchase
                              on b.PurchaseId equals e.PurchaseId
                              join f in oConnectionContext.DbClsItem
                                on c.ItemId equals f.ItemId
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.Date) <= obj.ToDate
              && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
                  && b.IsReverseCharge == 1 && (c.TaxId != ExemptedId || c.TaxId != NonGstId)
                  && f.ItemType.ToLower() == "service"
                              select new
                              {
                                  BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                  InvoiceNo = b.InvoiceNo,
                                  SalesDate = b.Date,
                                  StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                  GrandTotal = b.GrandTotal,
                                  SalesType = "Debit Note",
                                  DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                  AmountExcTax = c.AmountExcTax,
                                  c.TaxId,
                                  TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                  TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                              select new ClsTaxTypeVm
                                              {
                                                  TaxTypeId = x.TaxTypeId,
                                                  TaxType = x.TaxType,
                                                  TaxAmount = (from y in oConnectionContext.DbClsTax
                                                               join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                               on y.TaxId equals z.TaxId
                                                               where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                               && z.PurchaseReturnId == c.PurchaseReturnId && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                               && z.PurchaseReturnTaxJournalType == "Reverse Charge"
                                                               select z.TaxAmount
                                                              ).DefaultIfEmpty().Sum()
                                              }).ToList()
                              }).ToList();

            var groupedDebitNotes = DebitNotes
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var AdvancePayments = (from b in oConnectionContext.DbClsSupplierPayment
                                   where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                   && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                   l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                  && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                  && b.Type == "Supplier Payment" && b.ParentId == 0 && b.PurchaseReturnId == 0
                  && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
                                    && b.IsReverseCharge == 1 && (b.TaxId != ExemptedId || b.TaxId != NonGstId)
                                   //&& f.ItemType.ToLower() == "service"
                                   select new
                                   {
                                       SupplyType = b.SourceOfSupplyId == b.DestinationOfSupplyId ? "Intra-State" : "Inter-State",
                                       BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                       BusinessTradeName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessTradeName).FirstOrDefault(),
                                       InvoiceNo = b.ReferenceNo,
                                       SalesDate = b.PaymentDate,
                                       StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                       GrandTotal = b.Amount - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.Amount).DefaultIfEmpty().Sum(),
                                       SalesType = "Supplier Payment",
                                       DestinationOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                       AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                                       TaxId = b.TaxId,
                                       TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                   select new ClsTaxTypeVm
                                                   {
                                                       TaxTypeId = x.TaxTypeId,
                                                       TaxType = x.TaxType,
                                                       TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                    join z in oConnectionContext.DbClsSupplierPaymentTaxJournal
                                                                    on y.TaxId equals z.TaxId
                                                                    where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                    && z.SupplierPaymentId == b.SupplierPaymentId
                                                                    && z.SupplierPaymentTaxJournalType == "Reverse Charge"
                                                                    select z.TaxAmount
                                                                   ).DefaultIfEmpty().Sum()
                                                   }).ToList()
                                   }).ToList();

            var groupedAdvancePayments = AdvancePayments
            .GroupBy(s => new { s.InvoiceNo, s.TaxPercent })
            .Select(g => new ClsSalesVm
            {
                SupplyType = g.Select(b => b.SupplyType).FirstOrDefault(),
                BusinessRegistrationNo = g.Select(b => b.BusinessRegistrationNo).FirstOrDefault(),
                BusinessTradeName = g.Select(b => b.BusinessTradeName).FirstOrDefault(),
                InvoiceNo = g.Select(b => b.InvoiceNo).FirstOrDefault(),
                SalesDate = g.Select(b => b.SalesDate).FirstOrDefault(),
                StateCode = g.Select(b => b.StateCode).FirstOrDefault(),
                GrandTotal = g.Select(b => b.GrandTotal).FirstOrDefault(),
                SalesType = g.Select(b => b.SalesType).FirstOrDefault(),
                DestinationOfSupply = g.Select(b => b.DestinationOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxId = g.Select(b => b.TaxId).FirstOrDefault(),
                TaxPercent = g.Select(b => b.TaxPercent).FirstOrDefault(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsTaxTypeVm
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var det = groupedPurchaseBill
                .Union(groupedExpenses)
                .Union(groupedDebitNotes)
                .ToList();

            return det;
        }

        public List<ClsSalesVm> HsnWiseOutwardSupplies(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = (from a in oConnectionContext.DbClsSalesDetails
                       join b in oConnectionContext.DbClsSales
                       on a.SalesId equals b.SalesId
                       join e in oConnectionContext.DbClsItem
                       on a.ItemId equals e.ItemId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
       && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft"
       //&& b.TotalTaxAmount > 0
       && e.ItemCodeId != 0
                       select new ClsSalesVm
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           Code = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == e.ItemCodeId).Select(d => d.Code).FirstOrDefault(),
                           Description = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == e.ItemCodeId).Select(d => d.Description).FirstOrDefault(),
                           ItemId = a.ItemId,
                           //ItemName = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.ItemName).FirstOrDefault(),
                           //Unit = a.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                           //         : a.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                           //         : a.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                           //         : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                           Quantity = a.QuantitySold,
                           TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == a.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                           AmountExcTax = a.AmountExcTax,
                           GrandTotal = b.GrandTotal,
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsTaxTypeVm
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsSalesTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == b.SalesId
                                                        select z.TaxAmount
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()
                       }).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                PrimaryShortUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitShortName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                SecondaryShortUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitShortName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                TertiaryShortUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitShortName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
                QuaternaryShortUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitShortName).FirstOrDefault(),
            }).ToList();

            foreach (var item in det)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    PrimaryShortUnit = a.PrimaryShortUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    SecondaryShortUnit = a.SecondaryShortUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    TertiaryShortUnit = a.TertiaryShortUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                    QuaternaryShortUnit = a.QuaternaryShortUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.Quantity;

                if (obj.PriceAddedFor == 0) { obj.PriceAddedFor = 1; }

                //TotalCurrentStock Stock
                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    //TotalCurrentStock = TotalCurrentStock;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        //TotalCurrentStock = TotalCurrentStock;
                    }
                }
                else if (conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        //TotalCurrentStock = TotalCurrentStock;
                    }
                }
                else
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                item.Quantity = TotalCurrentStock;
                item.UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit;
                item.UnitShortName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryShortUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryShortUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryShortUnit : conversionRates.QuaternaryShortUnit;
            }

            return det;
        }

        public List<ClsSalesVm> HsnWiseInwardSupplies(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = (from a in oConnectionContext.DbClsPurchaseDetails
                       join b in oConnectionContext.DbClsPurchase
                       on a.PurchaseId equals b.PurchaseId
                       join e in oConnectionContext.DbClsItem
                       on a.ItemId equals e.ItemId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
       && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft"
       //&& b.TotalTaxAmount > 0 
       && e.ItemCodeId != 0
                       select new ClsSalesVm
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.PurchaseId,
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           Code = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == a.ItemCodeId).Select(d => d.Code).FirstOrDefault(),
                           ItemName = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.ItemName).FirstOrDefault(),
                           Unit = a.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                                    : a.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                                    : a.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                           Quantity = a.Quantity,
                           TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == a.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                           AmountExcTax = a.AmountExcTax,
                           GrandTotal = b.GrandTotal,
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsTaxTypeVm
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                        select z.TaxAmount
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()
                       }).ToList();

            return det;
        }

        public string fetchStatus(long CompanyId, string Type, long Id)
        {
            string Status = "";

            if (Type == "Sales Quotation")
            {
                Status = oConnectionContext.DbClsSalesQuotation.Where(a => a.CompanyId == CompanyId && a.SalesQuotationId == Id).Select(a => a.Status).FirstOrDefault();
            }
            else if (Type == "Sales Order")
            {
                Status = oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == CompanyId && a.SalesOrderId == Id).Select(a => a.Status).FirstOrDefault();
            }
            else if (Type == "Sales Proforma")
            {
                Status = oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == CompanyId && a.SalesProformaId == Id).Select(a => a.Status).FirstOrDefault();
            }
            else if (Type == "Delivery Challan")
            {
                if (oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == CompanyId && a.DeliveryChallanId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == CompanyId && a.DeliveryChallanId == Id).Select(a => a.Status).FirstOrDefault();
                }
            }
            else if (Type == "Sales Invoice")
            {
                if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Sent";
                }
            }
            else if (Type == "Sales Debit Note")
            {
                if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Sent";
                }
            }
            else if (Type == "Bill Of Supply")
            {
                if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Sent";
                }
            }
            else if (Type == "Pos")
            {
                if (oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsSales.Where(a => a.CompanyId == CompanyId && a.SalesId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Sent";
                }
            }
            else if (Type == "Sales Credit Note")
            {
                if (oConnectionContext.DbClsSalesReturn.Where(a => a.CompanyId == CompanyId && a.SalesReturnId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsSalesReturn.Where(a => a.CompanyId == CompanyId && a.SalesReturnId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Sent";
                }
            }
            else if (Type == "Payment Link")
            {
                Status = "Payment Link";
            }
            else if (Type == "Sales Invoice Payment")
            {
                if (oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == CompanyId && a.CustomerPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Payment";
                }
            }
            else if (Type == "Customer Advance Payment")
            {
                if (oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == CompanyId && a.CustomerPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Payment";
                }
            }
            else if (Type == "Refund From Credit Note")
            {
                if (oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == CompanyId && a.CustomerPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Refund";
                }
            }
            else if (Type == "Refund From Customer Advance Payment")
            {
                if (oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == CompanyId && a.CustomerPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Refund";
                }
            }
            else if (Type == "Credits Applied From Credit Note")
            {
                Status = "Credits Applied";
            }
            else if (Type == "Credits Applied From Customer Advance Payment")
            {
                Status = "Credits Applied";
            }
            else if (Type == "Purchase Quotation")
            {
                Status = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.CompanyId == CompanyId && a.PurchaseQuotationId == Id).Select(a => a.Status).FirstOrDefault();
            }
            else if (Type == "Purchase Order")
            {
                Status = oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == CompanyId && a.PurchaseOrderId == Id).Select(a => a.Status).FirstOrDefault();
            }
            else if (Type == "Purchase Bill")
            {
                if (oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == CompanyId && a.PurchaseId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == CompanyId && a.PurchaseId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Received";
                }
            }
            else if (Type == "Purchase Debit Note")
            {
                if (oConnectionContext.DbClsPurchaseReturn.Where(a => a.CompanyId == CompanyId && a.PurchaseReturnId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    //Status = oConnectionContext.DbClsPurchaseReturn.Where(a => a.CompanyId == CompanyId && a.PurchaseReturnId == Id).Select(a => a.Status).FirstOrDefault();
                    Status = "Issued";
                }
            }
            else if (Type == "Purchase Bill Payment")
            {
                if (oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == CompanyId && a.SupplierPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Payment";
                }
            }
            else if (Type == "Supplier Advance Payment")
            {
                if (oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == CompanyId && a.SupplierPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Payment";
                }
            }
            else if (Type == "Refund From Debit Note")
            {
                if (oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == CompanyId && a.SupplierPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Refund";
                }
            }
            else if (Type == "Refund From Supplier Advance Payment")
            {
                if (oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == CompanyId && a.SupplierPaymentId == Id).Select(a => a.IsCancelled).FirstOrDefault() == true)
                {
                    Status = "Cancelled";
                }
                else
                {
                    Status = "Refund";
                }
            }
            else if (Type == "Credits Applied From Debit Note")
            {
                Status = "Credits Applied";
            }
            else if (Type == "Credits Applied From Supplier Advance Payment")
            {
                Status = "Credits Applied";
            }
            return Status;
        }

        //#region Insert States
        //public class demo
        //{
        //    public List<countries> countries { get; set; }
        //    public List<states> states { get; set; }
        //}

        //public class countries
        //{
        //    public long id { get; set; }
        //    public string sortname { get; set; }
        //    public string name { get; set; }
        //    public long phoneCode { get; set; }
        //}

        //public class states
        //{
        //    public long id { get; set; }
        //    public string name { get; set; }
        //    public long country_id { get; set; }
        //}

        //public dynamic GetMappedCountries(demo obj)
        //{
        //    List<countries> det = new List<countries>();

        //    foreach (var item in obj.countries)
        //    {
        //        long countryId = oConnectionContext.DbClsCountry.Where(a => a.CountryCode == item.sortname).Select(a => a.CountryId).DefaultIfEmpty().FirstOrDefault();
        //        if (countryId >0)
        //        {
        //            string query = "update tblcountry set country_id=" + item.id + " where CountryId=" + countryId;
        //            oConnectionContext.Database.ExecuteSqlCommand(query);
        //            det.Add(item);
        //        }
        //    }

        //    string q1 = "update tblcountry set country_id=62 where CountryId=153";
        //    oConnectionContext.Database.ExecuteSqlCommand(q1);

        //    return Json(det);
        //}

        //public dynamic insertStates(demo obj)
        //{
        //    List<states> det = new List<states>();

        //    foreach (var item in obj.states)
        //    {
        //        long countryId = oConnectionContext.DbClsCountry.Where(a => a.country_id == item.country_id).Select(a => a.CountryId).DefaultIfEmpty().FirstOrDefault();
        //        if (countryId > 0)
        //        {
        //            ClsState oClsState = new ClsState()
        //            {
        //                CountryId = countryId,
        //                State = item.name,
        //                IsActive= true,
        //                IsDeleted = false,
        //                AddedBy = 1,
        //                AddedOn = CurrentDate(1)
        //            };
        //            oConnectionContext.DbClsState.Add(oClsState);
        //            oConnectionContext.SaveChanges();

        //            det.Add(item);
        //        }
        //    }

        //    return Json(det);
        //}

        //#endregion

    }
}
