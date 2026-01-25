using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Twilio.Types;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class BusinessSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;
        public async Task<IHttpActionResult> BusinessSetting(ClsBusinessSettingsVm obj)
        {
            string[] emptyArr = new string[0];
            //List<string> emptyArr = new List<string>();
            var det = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new ClsBusinessSettingsVm
            {
                IsBusinessRegistered=a.IsBusinessRegistered,
                BusinessRegistrationType =a.BusinessRegistrationType,
                ResellerTermsUrl=a.ResellerTermsUrl,
                FreeTrialDays=a.FreeTrialDays,
                ParentWebsiteUrl=a.ParentWebsiteUrl,
                ParentBusinessName=a.ParentBusinessName,
                WebsiteUrl=a.WebsiteUrl,
                BusinessName=a.BusinessName,
                CountryId=a.CountryId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                CurrencyName = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyName).FirstOrDefault(),
                //TimeZoneDisplayName = oConnectionContext.DbClsTimeZone.Where(b => b.TimeZoneId == a.TimeZoneId).Select(b => b.DisplayName).FirstOrDefault(),
                SupportsDaylightSavingTime = oConnectionContext.DbClsTimeZone.Where(b => b.TimeZoneId == a.TimeZoneId).Select(b => b.SupportsDaylightSavingTime).FirstOrDefault(),
                BusinessLogo = a.BusinessLogo,
                BusinessIcon = a.BusinessIcon,
                Favicon =a.Favicon,
                StartDate=a.StartDate,
                CurrencySymbolPlacement=a.CurrencySymbolPlacement,
                TimeFormat=a.TimeFormat,
                TimeZoneId=a.TimeZoneId,
                EnableDaylightSavingTime=a.EnableDaylightSavingTime,
                IndustryTypeId=a.IndustryTypeId,
                BusinessTypeIds = a.BusinessTypeIds,
                //BusinessTypes = emptyArr,
                //BusinessTypes = a.BusinessTypeIds == null ? emptyArr : a.BusinessTypeIds.ToString().Split(','),
                FinancialYearStartMonth = a.FinancialYearStartMonth,
                TransactionEditDays = a.TransactionEditDays,
                DateFormat = a.DateFormat,
                //a.Tax1Name,
                //a.Tax1No,
                //a.Tax2Name,
                //a.Tax2No,
                //a.EnableInlineTax,
                CreditLimitForCustomer = a.CreditLimitForCustomer,
                CreditLimitForSupplier = a.CreditLimitForSupplier,
                DatatablePageEntries = a.DatatablePageEntries,
                EnableDarkMode = a.EnableDarkMode,
                ShowHelpText = a.ShowHelpText,
                FixedHeader = a.FixedHeader,
                FixedFooter = a.FixedFooter,
                EnableSound = a.EnableSound,
                EnableDefaultSmsBranding = a.EnableDefaultSmsBranding,
                EnableDefaultEmailBranding = a.EnableDefaultEmailBranding,
                CollapseSidebar = a.CollapseSidebar
                //a.AllowOnlinePayment
            }).FirstOrDefault();

            // Now split BusinessTypeIds in memory after fetching the data
            if (det != null && det.BusinessTypeIds != null)
            {
                det.BusinessTypes = det.BusinessTypeIds.Split(',');
            }
            else
            {
                det.BusinessTypes = emptyArr;
            }

            //var Currencys = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    CurrencyId = a.CurrencyId,
            //    a.CurrencyCode,
            //    a.CurrencySymbol,
            //    a.CurrencyName,
            //}).OrderBy(a => a.CurrencyName).ToList();

            ClsCountryVm oClsCountryVm = new ClsCountryVm()
            {
                CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault(),
            };

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BusinessSetting = det,
                    Country = oClsCountryVm
                    //Currencys = Currencys
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BusinessSettingsUpdate(ClsBusinessSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.BusinessName == "" || obj.BusinessName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessName" });
                    isError = true;
                }

                if (obj.IndustryTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divIndustryType" });
                    isError = true;
                }

                if (obj.BusinessTypes == null || obj.BusinessTypes[0] == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessType" });
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

                obj.BusinessTypeIds = "";
                if (obj.BusinessTypes != null)
                {
                    foreach (var item in obj.BusinessTypes)
                    {
                        obj.BusinessTypeIds = obj.BusinessTypeIds + "," + item;
                    }
                }

                long BusinessSettingsId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessSettingsId).FirstOrDefault();

                ClsBusinessSettings oClsBusinessSettings = new ClsBusinessSettings();
                oClsBusinessSettings.BusinessSettingsId = BusinessSettingsId;
                oClsBusinessSettings.WebsiteUrl = obj.WebsiteUrl;
                oClsBusinessSettings.BusinessName = obj.BusinessName;
                oClsBusinessSettings.StartDate = obj.StartDate != null ? obj.StartDate.Value.AddHours(5).AddMinutes(30) : obj.StartDate;
                //oClsBusinessSettings.CountryId= obj.CountryId;
                oClsBusinessSettings.CurrencySymbolPlacement = obj.CurrencySymbolPlacement;
                oClsBusinessSettings.TimeZoneId = obj.TimeZoneId;
                oClsBusinessSettings.EnableDaylightSavingTime = obj.EnableDaylightSavingTime;
                oClsBusinessSettings.IndustryTypeId = obj.IndustryTypeId;
                oClsBusinessSettings.FinancialYearStartMonth = obj.FinancialYearStartMonth;
                oClsBusinessSettings.TransactionEditDays = obj.TransactionEditDays;
                oClsBusinessSettings.DateFormat = obj.DateFormat;
                oClsBusinessSettings.TimeFormat = obj.TimeFormat;
                oClsBusinessSettings.ModifiedOn = CurrentDate;
                oClsBusinessSettings.ModifiedBy = obj.AddedBy;
                //oClsBusinessSettings.AllowOnlinePayment = obj.AllowOnlinePayment;
                oClsBusinessSettings.BusinessTypeIds = obj.BusinessTypeIds;
                oClsBusinessSettings.EnableDefaultSmsBranding = obj.EnableDefaultSmsBranding;
                oClsBusinessSettings.EnableDefaultEmailBranding = obj.EnableDefaultEmailBranding;
                oClsBusinessSettings.IsBusinessRegistered = obj.IsBusinessRegistered;
                oClsBusinessSettings.BusinessRegistrationType= obj.BusinessRegistrationType;

                string pic1 = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessLogo).FirstOrDefault();
                if (obj.BusinessLogo != "" && obj.BusinessLogo != null && !obj.BusinessLogo.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/BusinessLogo/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtension;

                    string base64 = obj.BusinessLogo.Replace(obj.BusinessLogo.Substring(0, obj.BusinessLogo.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BusinessLogo");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.BusinessLogo.Replace(obj.BusinessLogo.Substring(0, obj.BusinessLogo.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsBusinessSettings.BusinessLogo = filepathPass;
                }
                else
                {
                    oClsBusinessSettings.BusinessLogo = pic1;
                }


                oConnectionContext.DbClsBusinessSettings.Attach(oClsBusinessSettings);
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.WebsiteUrl).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessName).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.StartDate).IsModified = true;
                //oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CurrencySymbolPlacement).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.TimeZoneId).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDaylightSavingTime).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.IndustryTypeId).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.FinancialYearStartMonth).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.TransactionEditDays).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.DateFormat).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.TimeFormat).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessLogo).IsModified = true;
                //oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.AllowOnlinePayment).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessTypeIds).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDefaultSmsBranding).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDefaultEmailBranding).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.IsBusinessRegistered).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessRegistrationType).IsModified = true;

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Business\" updated",
                    Id = oClsBusinessSettings.BusinessSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                //SmsController oSmsController = new SmsController();
                //oSmsController.SmsFunction("", "8013200659");

                data = new
                {
                    Status = 1,
                    Message = "Business Info updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> TaxUpdate(ClsBusinessSettingsVm obj)
        //{
        //    long BusinessSettingsId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessSettingsId).FirstOrDefault();

        //    ClsBusinessSettings oClsBusinessSettings = new ClsBusinessSettings();
        //    oClsBusinessSettings.BusinessSettingsId = BusinessSettingsId;
        //    oClsBusinessSettings.Tax1Name = obj.Tax1Name;
        //    oClsBusinessSettings.Tax1No= obj.Tax1No;
        //    oClsBusinessSettings.Tax2Name = obj.Tax2Name;
        //    oClsBusinessSettings.Tax2No= obj.Tax2No;
        //    oClsBusinessSettings.EnableInlineTax = obj.EnableInlineTax;
        //    oClsBusinessSettings.ModifiedOn = CurrentDate;
        //    oClsBusinessSettings.ModifiedBy = obj.AddedBy;

        //    oConnectionContext.DbClsBusinessSettings.Attach(oClsBusinessSettings);
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.Tax1Name).IsModified = true;
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.Tax1No).IsModified = true;
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.Tax2Name).IsModified = true;
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.Tax2No).IsModified = true;
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableInlineTax).IsModified = true;
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedBy).IsModified = true;
        //    oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedOn).IsModified = true;
        //    oConnectionContext.SaveChanges();

        //    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
        //    {
        //        AddedBy = obj.AddedBy,
        //        Browser = obj.Browser,
        //        Category = "Business Settings - Tax Informations Update",
        //        CompanyId = obj.CompanyId,
        //        Description = "updated business informations",
        //        Id = oClsBusinessSettings.BusinessSettingsId,
        //        IpAddress = obj.IpAddress,
        //        Platform = obj.Platform,
        //        Type = "Business Settings"
        //    };
        //    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

        //    data = new
        //    {
        //        Status = 1,
        //        Message = "Tax Info updated successfully.",
        //        Data = new
        //        {

        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> CreditLimitUpdate(ClsBusinessSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long BusinessSettingsId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessSettingsId).FirstOrDefault();

                ClsBusinessSettings oClsBusinessSettings = new ClsBusinessSettings();
                oClsBusinessSettings.BusinessSettingsId = BusinessSettingsId;
                oClsBusinessSettings.CreditLimitForCustomer = obj.CreditLimitForCustomer;
                oClsBusinessSettings.CreditLimitForSupplier = obj.CreditLimitForSupplier;
                oClsBusinessSettings.ModifiedOn = CurrentDate;
                oClsBusinessSettings.ModifiedBy = obj.AddedBy;

                oConnectionContext.DbClsBusinessSettings.Attach(oClsBusinessSettings);
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CreditLimitForCustomer).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CreditLimitForSupplier).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Credit Limit\" updated",
                    Id = oClsBusinessSettings.BusinessSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Credit Limit updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SystemUpdate(ClsBusinessSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long BusinessSettingsId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessSettingsId).FirstOrDefault();

                ClsBusinessSettings oClsBusinessSettings = new ClsBusinessSettings();
                oClsBusinessSettings.BusinessSettingsId = BusinessSettingsId;
                oClsBusinessSettings.EnableDarkMode = obj.EnableDarkMode;
                oClsBusinessSettings.DatatablePageEntries = obj.DatatablePageEntries;
                oClsBusinessSettings.EnableSound = obj.EnableSound;
                oClsBusinessSettings.ShowHelpText = obj.ShowHelpText;
                oClsBusinessSettings.FixedHeader = obj.FixedHeader;
                oClsBusinessSettings.FixedFooter = obj.FixedFooter;
                oClsBusinessSettings.CollapseSidebar = obj.CollapseSidebar;
                oClsBusinessSettings.ModifiedOn = CurrentDate;
                oClsBusinessSettings.ModifiedBy = obj.AddedBy;

                oConnectionContext.DbClsBusinessSettings.Attach(oClsBusinessSettings);
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDarkMode).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.DatatablePageEntries).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableSound).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ShowHelpText).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.FixedHeader).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.FixedFooter).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CollapseSidebar).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"System\" updated",
                    Id = oClsBusinessSettings.BusinessSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Business Settings"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "System updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CheckTime(ClsBusinessSettingsVm obj)
        {
            var TimeZone = (from a in oConnectionContext.DbClsTimeZone
                            where a.TimeZoneId == obj.TimeZoneId
                            select new
                            {
                                a.SupportsDaylightSavingTime,
                                a.o1String,
                                a.o2String,
                            }).FirstOrDefault();

            int hour = 0, minute = 0;
            string[] val;

            if (TimeZone != null)
            {
                if (obj.EnableDaylightSavingTime == true)
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

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).FirstOrDefault();

            string formattedDate = updatedTime.ToString(BusinessSetting.DateFormat + " " + BusinessSetting.TimeFormat);
            data = new
            {
                Status = 1,
                Message = formattedDate,
                Data = new
                {

                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BusinessSettingsUpdateAdmin(ClsBusinessSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.BusinessName == "" || obj.BusinessName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessName" });
                    isError = true;
                }

                if (obj.WebsiteUrl == "" || obj.WebsiteUrl == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divWebsiteUrl" });
                    isError = true;
                }

                if (obj.FreeTrialDays == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFreeTrialDays" });
                    isError = true;
                }

                if (obj.FreeTrialDays > 30)
                {
                    errors.Add(new ClsError { Message = "Free trial cannot be more than 30 days", Id = "divFreeTrialDays" });
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

                long BusinessSettingsId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessSettingsId).FirstOrDefault();

                ClsBusinessSettings oClsBusinessSettings = new ClsBusinessSettings();
                oClsBusinessSettings.ResellerTermsUrl = obj.ResellerTermsUrl;
                oClsBusinessSettings.BusinessSettingsId = BusinessSettingsId;
                oClsBusinessSettings.WebsiteUrl = obj.WebsiteUrl.TrimEnd('/');
                oClsBusinessSettings.BusinessName = obj.BusinessName;
                oClsBusinessSettings.StartDate = obj.StartDate != null ? obj.StartDate.Value.AddHours(5).AddMinutes(30) : obj.StartDate;
                //oClsBusinessSettings.CountryId= obj.CountryId;
                oClsBusinessSettings.CurrencySymbolPlacement = obj.CurrencySymbolPlacement;
                oClsBusinessSettings.TimeZoneId = obj.TimeZoneId;
                oClsBusinessSettings.EnableDaylightSavingTime = obj.EnableDaylightSavingTime;
                oClsBusinessSettings.IndustryTypeId = obj.IndustryTypeId;
                oClsBusinessSettings.FinancialYearStartMonth = obj.FinancialYearStartMonth;
                oClsBusinessSettings.TransactionEditDays = obj.TransactionEditDays;
                oClsBusinessSettings.DateFormat = obj.DateFormat;
                oClsBusinessSettings.TimeFormat = obj.TimeFormat;
                oClsBusinessSettings.ModifiedOn = CurrentDate;
                oClsBusinessSettings.ModifiedBy = obj.AddedBy;
                //oClsBusinessSettings.AllowOnlinePayment = obj.AllowOnlinePayment;
                oClsBusinessSettings.BusinessTypeIds = obj.BusinessTypeIds;
                oClsBusinessSettings.EnableDefaultSmsBranding = obj.EnableDefaultSmsBranding;
                oClsBusinessSettings.EnableDefaultEmailBranding = obj.EnableDefaultEmailBranding;
                oClsBusinessSettings.ParentBusinessName = obj.ParentBusinessName;
                oClsBusinessSettings.ParentWebsiteUrl = obj.ParentWebsiteUrl;
                oClsBusinessSettings.FreeTrialDays = obj.FreeTrialDays;

                string pic1 = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessLogo).FirstOrDefault();
                if (obj.BusinessLogo != "" && obj.BusinessLogo != null && !obj.BusinessLogo.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/BusinessLogo/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtension;

                    string base64 = obj.BusinessLogo.Replace(obj.BusinessLogo.Substring(0, obj.BusinessLogo.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BusinessLogo");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.BusinessLogo.Replace(obj.BusinessLogo.Substring(0, obj.BusinessLogo.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsBusinessSettings.BusinessLogo = filepathPass;
                }
                else
                {
                    oClsBusinessSettings.BusinessLogo = pic1;
                }

                string pic2 = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Favicon).FirstOrDefault();
                if (obj.Favicon != "" && obj.Favicon != null && !obj.Favicon.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic2 != "" && pic2 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic2))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic2));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Favicon/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionFavicon;

                    string base64 = obj.Favicon.Replace(obj.Favicon.Substring(0, obj.Favicon.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Favicon");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.Favicon.Replace(obj.Favicon.Substring(0, obj.Favicon.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsBusinessSettings.Favicon = filepathPass;
                }
                else
                {
                    oClsBusinessSettings.Favicon = pic2;
                }

                string pic3 = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BusinessIcon).FirstOrDefault();
                if (obj.BusinessIcon != "" && obj.BusinessIcon != null && !obj.BusinessIcon.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic3 != "" && pic3 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic3))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic3));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/BusinessIcon/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionIcon;

                    string base64 = obj.BusinessIcon.Replace(obj.BusinessIcon.Substring(0, obj.BusinessIcon.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BusinessIcon");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.BusinessIcon.Replace(obj.BusinessIcon.Substring(0, obj.BusinessIcon.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsBusinessSettings.BusinessIcon = filepathPass;
                }
                else
                {
                    oClsBusinessSettings.BusinessIcon = pic1;
                }

                oConnectionContext.DbClsBusinessSettings.Attach(oClsBusinessSettings);
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ResellerTermsUrl).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.WebsiteUrl).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessName).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.StartDate).IsModified = true;
                //oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.CurrencySymbolPlacement).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.TimeZoneId).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDaylightSavingTime).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.IndustryTypeId).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.FinancialYearStartMonth).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.TransactionEditDays).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.DateFormat).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.TimeFormat).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessLogo).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessIcon).IsModified = true;
                //oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.AllowOnlinePayment).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.BusinessTypeIds).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDefaultSmsBranding).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.EnableDefaultEmailBranding).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.Favicon).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ParentBusinessName).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.ParentWebsiteUrl).IsModified = true;
                oConnectionContext.Entry(oClsBusinessSettings).Property(x => x.FreeTrialDays).IsModified = true;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    Category = "Business Settings - Business Information Update",
                //    CompanyId = obj.CompanyId,
                //    Description = "updated business informations",
                //    Id = oClsBusinessSettings.BusinessSettingsId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Business Settings"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                //SmsController oSmsController = new SmsController();
                //oSmsController.SmsFunction("", "8013200659");

                data = new
                {
                    Status = 1,
                    Message = "Business Info updated successfully.",
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
