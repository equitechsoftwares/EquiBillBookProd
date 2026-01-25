using EquiBillBook.Filters;
using EquiBillBook.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Helpers;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class PwaSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllPwaSettings(ClsPwaSettingsVm obj)
        {
            var det = (from b in oConnectionContext.DbClsDomain
                       join a in oConnectionContext.DbClsPwaSettings
                       on b.DomainId equals a.DomainId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                       && a.IsDeleted == false && a.IsActive == true
                       select new
                       {
                           Domain = b.Domain,
                           PwaSettingsId = a.PwaSettingsId,
                           a.PwaName,
                           a.PwaShortName,
                           a.BackgroundColor,
                           a.ThemeColor,
                           a.Image_48_48,
                           a.Image_72_72,
                           a.Image_96_96,
                           a.Image_128_128,
                           a.Image_144_144,
                           a.Image_152_152,
                           a.Image_192_192,
                           a.Image_284_284,
                           a.Image_512_512,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           a.AddedOn,
                           a.ModifiedOn,
                       }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PwaSettings = det.OrderByDescending(a => a.PwaSettingsId).ToList(),
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PwaSetting(ClsPwaSettingsVm obj)
        {
            var det = oConnectionContext.DbClsPwaSettings.Where(a => a.PwaSettingsId == obj.PwaSettingsId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.PwaSettingsId,
                a.PwaName,
                a.PwaShortName,
                a.BackgroundColor,
                a.ThemeColor,
                a.Image_48_48,
                a.Image_72_72,
                a.Image_96_96,
                a.Image_128_128,
                a.Image_144_144,
                a.Image_152_152,
                a.Image_192_192,
                a.Image_284_284,
                a.Image_512_512
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PwaSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPwaSettings(ClsPwaSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PwaName == "" || obj.PwaName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPwaName" });
                    isError = true;
                }

                if (obj.PwaShortName == "" || obj.PwaShortName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPwaShortName" });
                    isError = true;
                }

                if (oConnectionContext.DbClsPwaSettings.Where(a => a.PwaName.ToLower() == obj.PwaName.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Pwa Name", Id = "divPwaName" });
                    isError = true;
                }

                if (oConnectionContext.DbClsPwaSettings.Where(a => a.PwaShortName.ToLower() == obj.PwaShortName.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Pwa Short Name", Id = "divPwaShortName" });
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

                // Get DomainId from the domain string (obj.Domain) or find it based on CompanyId
                long DomainId = 0;
                if (!string.IsNullOrEmpty(obj.Domain))
                {
                    DomainId = oConnectionContext.DbClsDomain.Where(a => a.Domain.ToLower() == obj.Domain.ToLower() && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Select(a => a.DomainId).FirstOrDefault();
                }
                
                if (DomainId == 0)
                {
                    // Fallback: get first active domain for the company
                    DomainId = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Select(a => a.DomainId).FirstOrDefault();
                }

                ClsPwaSettings oClsPwaSettings = new ClsPwaSettings()
                {
                    PwaSettingsId = obj.PwaSettingsId,
                    DomainId = DomainId,
                    CompanyId = obj.CompanyId,
                    PwaName = obj.PwaName,
                    PwaShortName = obj.PwaShortName,
                    BackgroundColor = obj.BackgroundColor,
                    ThemeColor = obj.ThemeColor,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false
                };

                if (obj.Image_48_48 != "" && obj.Image_48_48 != null && !obj.Image_48_48.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_48_48/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_48_48;

                    string base64 = obj.Image_48_48.Replace(obj.Image_48_48.Substring(0, obj.Image_48_48.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_48_48");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_48_48 = filepathPass;
                }

                if (obj.Image_72_72 != "" && obj.Image_72_72 != null && !obj.Image_72_72.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_72_72/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_72_72;

                    string base64 = obj.Image_72_72.Replace(obj.Image_72_72.Substring(0, obj.Image_72_72.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_72_72");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_72_72 = filepathPass;
                }

                if (obj.Image_96_96 != "" && obj.Image_96_96 != null && !obj.Image_96_96.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_96_96/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_96_96;

                    string base64 = obj.Image_96_96.Replace(obj.Image_96_96.Substring(0, obj.Image_96_96.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_96_96");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_96_96 = filepathPass;
                }

                if (obj.Image_128_128 != "" && obj.Image_128_128 != null && !obj.Image_128_128.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_128_128/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_128_128;

                    string base64 = obj.Image_128_128.Replace(obj.Image_128_128.Substring(0, obj.Image_128_128.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_128_128");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_128_128 = filepathPass;
                }

                if (obj.Image_144_144 != "" && obj.Image_144_144 != null && !obj.Image_144_144.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_144_144/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_144_144;

                    string base64 = obj.Image_144_144.Replace(obj.Image_144_144.Substring(0, obj.Image_144_144.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_144_144");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_144_144 = filepathPass;
                }

                if (obj.Image_152_152 != "" && obj.Image_152_152 != null && !obj.Image_152_152.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_152_152/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_152_152;

                    string base64 = obj.Image_152_152.Replace(obj.Image_152_152.Substring(0, obj.Image_152_152.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_152_152");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_152_152 = filepathPass;
                }

                if (obj.Image_192_192 != "" && obj.Image_192_192 != null && !obj.Image_192_192.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_192_192/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_192_192;

                    string base64 = obj.Image_192_192.Replace(obj.Image_192_192.Substring(0, obj.Image_192_192.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_192_192");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_192_192 = filepathPass;
                }

                if (obj.Image_284_284 != "" && obj.Image_284_284 != null && !obj.Image_284_284.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_284_284/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_284_284;

                    string base64 = obj.Image_284_284.Replace(obj.Image_284_284.Substring(0, obj.Image_284_284.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_284_284");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_284_284 = filepathPass;
                }

                if (obj.Image_512_512 != "" && obj.Image_512_512 != null && !obj.Image_512_512.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Image_512_512/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_512_512;

                    string base64 = obj.Image_512_512.Replace(obj.Image_512_512.Substring(0, obj.Image_512_512.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_512_512");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_512_512 = filepathPass;
                }

                // create manifest

                string Domain = oConnectionContext.DbClsDomain.Where(a => a.DomainId == DomainId && a.IsActive == true && a.IsDeleted == false).Select(a => a.Domain).FirstOrDefault();

                if (string.IsNullOrEmpty(Domain))
                {
                    // Fallback: get first active domain for the company
                    Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Select(a => a.Domain).FirstOrDefault();
                }

                string filepathPass1 = "/ExternalContents/Manifest/" + Domain + ".json";

                if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass1))))
                {
                    System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass1));
                }

                var folder1 = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Manifest");
                if (!Directory.Exists(folder1))
                {
                    Directory.CreateDirectory(folder1);
                }

                var jsonArray = new JArray();

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_48_48,
                    sizes = "48x48",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_72_72,
                    sizes = "72x72",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_96_96,
                    sizes = "96x96",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_128_128,
                    sizes = "128x128",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_144_144,
                    sizes = "144x144",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_152_152,
                    sizes = "152x152",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_192_192,
                    sizes = "192x192",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_284_284,
                    sizes = "284x284",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_512_512,
                    sizes = "512x512",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                var responseData = new
                {
                    name = obj.PwaName,
                    short_name = obj.PwaShortName,
                    start_url = "/dashboard",
                    display = "standalone",
                    background_color = "#fdfdfd",
                    theme_color = "#db4938",
                    orientation = "portrait-primary",
                    icons = jsonArray
                };

                string jsonData = JsonConvert.SerializeObject(responseData, Formatting.None);
                File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass1), jsonData);
                // create manifest

                oConnectionContext.DbClsPwaSettings.Add(oClsPwaSettings);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Pwa Settings\" created",
                    Id = oClsPwaSettings.PwaSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Pwa Settings created successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PwaSettingsUpdate(ClsPwaSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PwaName == "" || obj.PwaName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPwaName" });
                    isError = true;
                }

                if (obj.PwaShortName == "" || obj.PwaShortName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPwaShortName" });
                    isError = true;
                }

                if (oConnectionContext.DbClsPwaSettings.Where(a => a.PwaName.ToLower() == obj.PwaName.ToLower() && a.IsDeleted == false
                && a.CompanyId == obj.CompanyId && a.PwaSettingsId != obj.PwaSettingsId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Pwa Name", Id = "divPwaName" });
                    isError = true;
                }

                if (oConnectionContext.DbClsPwaSettings.Where(a => a.PwaShortName.ToLower() == obj.PwaShortName.ToLower() && a.IsDeleted == false
                && a.CompanyId == obj.CompanyId && a.PwaSettingsId != obj.PwaSettingsId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Pwa Short Name", Id = "divPwaShortName" });
                    isError = true;
                }

                var PwaSetting = oConnectionContext.DbClsPwaSettings.Where(a => a.PwaSettingsId == obj.PwaSettingsId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    Domain= oConnectionContext.DbClsDomain.Where(b=>b.DomainId == a.DomainId).Select(b=>b.Domain).FirstOrDefault(),
                    a.DomainId,
                    a.Image_48_48,
                    a.Image_72_72,
                    a.Image_96_96,
                    a.Image_128_128,
                    a.Image_144_144,
                    a.Image_152_152,
                    a.Image_192_192,
                    a.Image_284_284,
                    a.Image_512_512
                }).FirstOrDefault();

                if ((obj.Image_48_48 == "" || obj.Image_48_48 == null) && (PwaSetting.Image_48_48 == "" || PwaSetting.Image_48_48 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_48_48" });
                    isError = true;
                }

                if ((obj.Image_72_72 == "" || obj.Image_72_72 == null) && (PwaSetting.Image_72_72 == "" || PwaSetting.Image_72_72 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_72_72" });
                    isError = true;
                }

                if ((obj.Image_96_96 == "" || obj.Image_96_96 == null) && (PwaSetting.Image_96_96 == "" || PwaSetting.Image_96_96 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_96_96" });
                    isError = true;
                }

                if ((obj.Image_128_128 == "" || obj.Image_128_128 == null) && (PwaSetting.Image_128_128 == "" || PwaSetting.Image_128_128 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_128_128" });
                    isError = true;
                }

                if ((obj.Image_144_144 == "" || obj.Image_144_144 == null) && (PwaSetting.Image_144_144 == "" || PwaSetting.Image_144_144 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_144_144" });
                    isError = true;
                }

                if ((obj.Image_152_152 == "" || obj.Image_152_152 == null) && (PwaSetting.Image_152_152 == "" || PwaSetting.Image_152_152 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_152_152" });
                    isError = true;
                }

                if ((obj.Image_192_192 == "" || obj.Image_192_192 == null) && (PwaSetting.Image_192_192 == "" || PwaSetting.Image_192_192 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_192_192" });
                    isError = true;
                }

                if ((obj.Image_284_284 == "" || obj.Image_284_284 == null) && (PwaSetting.Image_284_284 == "" || PwaSetting.Image_284_284 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_284_284" });
                    isError = true;
                }

                if ((obj.Image_512_512 == "" || obj.Image_512_512 == null) && (PwaSetting.Image_512_512 == "" || PwaSetting.Image_512_512 == null))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divImage_512_512" });
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

                ClsPwaSettings oClsPwaSettings = new ClsPwaSettings()
                {
                    PwaSettingsId = obj.PwaSettingsId,
                    PwaName = obj.PwaName,
                    PwaShortName = obj.PwaShortName,
                    BackgroundColor = obj.BackgroundColor,
                    ThemeColor = obj.ThemeColor,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = CurrentDate
                };

                //string Image_48_48 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_48_48).FirstOrDefault();
                if (obj.Image_48_48 != "" && obj.Image_48_48 != null && !obj.Image_48_48.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_48_48 != "" && PwaSetting.Image_48_48 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_48_48))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_48_48));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_48_48/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_48_48;

                    string base64 = obj.Image_48_48.Replace(obj.Image_48_48.Substring(0, obj.Image_48_48.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_48_48");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_48_48 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_48_48 = PwaSetting.Image_48_48;
                }

                //string Image_72_72 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_72_72).FirstOrDefault();
                if (obj.Image_72_72 != "" && obj.Image_72_72 != null && !obj.Image_72_72.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_72_72 != "" && PwaSetting.Image_72_72 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_72_72))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_72_72));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_72_72/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_72_72;

                    string base64 = obj.Image_72_72.Replace(obj.Image_72_72.Substring(0, obj.Image_72_72.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_72_72");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_72_72 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_72_72 = PwaSetting.Image_72_72;
                }

                //string Image_96_96 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_96_96).FirstOrDefault();
                if (obj.Image_96_96 != "" && obj.Image_96_96 != null && !obj.Image_96_96.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_96_96 != "" && PwaSetting.Image_96_96 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_96_96))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_96_96));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_96_96/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_96_96;

                    string base64 = obj.Image_96_96.Replace(obj.Image_96_96.Substring(0, obj.Image_96_96.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_96_96");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_96_96 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_96_96 = PwaSetting.Image_96_96;
                }

                //string Image_128_128 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_128_128).FirstOrDefault();
                if (obj.Image_128_128 != "" && obj.Image_128_128 != null && !obj.Image_128_128.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_128_128 != "" && PwaSetting.Image_128_128 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_128_128))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_128_128));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_128_128/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_128_128;

                    string base64 = obj.Image_128_128.Replace(obj.Image_128_128.Substring(0, obj.Image_128_128.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_128_128");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_128_128 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_128_128 = PwaSetting.Image_128_128;
                }

                //string Image_144_144 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_144_144).FirstOrDefault();
                if (obj.Image_144_144 != "" && obj.Image_144_144 != null && !obj.Image_144_144.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_144_144 != "" && PwaSetting.Image_144_144 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_144_144))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_144_144));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_144_144/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_144_144;

                    string base64 = obj.Image_144_144.Replace(obj.Image_144_144.Substring(0, obj.Image_144_144.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_144_144");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_144_144 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_144_144 = PwaSetting.Image_144_144;
                }

                //string Image_152_152 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_152_152).FirstOrDefault();
                if (obj.Image_152_152 != "" && obj.Image_152_152 != null && !obj.Image_152_152.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_152_152 != "" && PwaSetting.Image_152_152 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_152_152))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_152_152));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_152_152/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_152_152;

                    string base64 = obj.Image_152_152.Replace(obj.Image_152_152.Substring(0, obj.Image_152_152.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_152_152");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_152_152 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_152_152 = PwaSetting.Image_152_152;
                }

                //string Image_192_192 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_192_192).FirstOrDefault();
                if (obj.Image_192_192 != "" && obj.Image_192_192 != null && !obj.Image_192_192.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_192_192 != "" && PwaSetting.Image_192_192 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_192_192))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_192_192));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_192_192/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_192_192;

                    string base64 = obj.Image_192_192.Replace(obj.Image_192_192.Substring(0, obj.Image_192_192.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_192_192");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_192_192 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_192_192 = PwaSetting.Image_192_192;
                }

                //string Image_284_284 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_284_284).FirstOrDefault();
                if (obj.Image_284_284 != "" && obj.Image_284_284 != null && !obj.Image_284_284.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_284_284 != "" && PwaSetting.Image_284_284 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_284_284))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_284_284));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_284_284/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_284_284;

                    string base64 = obj.Image_284_284.Replace(obj.Image_284_284.Substring(0, obj.Image_284_284.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_284_284");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_284_284 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_284_284 = PwaSetting.Image_284_284;
                }

                //string Image_512_512 = oConnectionContext.DbClsPwaSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Image_512_512).FirstOrDefault();
                if (obj.Image_512_512 != "" && obj.Image_512_512 != null && !obj.Image_512_512.Contains("http"))
                {
                    string filepathPass = "";

                    if (PwaSetting.Image_512_512 != "" && PwaSetting.Image_512_512 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_512_512))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(PwaSetting.Image_512_512));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Image_512_512/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionImage_512_512;

                    string base64 = obj.Image_512_512.Replace(obj.Image_512_512.Substring(0, obj.Image_512_512.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);

                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Image_512_512");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oClsPwaSettings.Image_512_512 = filepathPass;
                }
                else
                {
                    oClsPwaSettings.Image_512_512 = PwaSetting.Image_512_512;
                }

                // create manifest

                //string Domain = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false).Select(a => a.Domain).FirstOrDefault();

                string filepathPass1 = "/ExternalContents/Manifest/" + PwaSetting.Domain + ".json";

                if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass1))))
                {
                    System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass1));
                }

                var folder1 = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Manifest");
                if (!Directory.Exists(folder1))
                {
                    Directory.CreateDirectory(folder1);
                }

                var jsonArray = new JArray();

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_48_48,
                    sizes = "48x48",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_72_72,
                    sizes = "72x72",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_96_96,
                    sizes = "96x96",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_128_128,
                    sizes = "128x128",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_144_144,
                    sizes = "144x144",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_152_152,
                    sizes = "152x152",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_192_192,
                    sizes = "192x192",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_284_284,
                    sizes = "284x284",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                jsonArray.Add(JToken.FromObject(new
                {
                    src = oClsPwaSettings.Image_512_512,
                    sizes = "512x512",
                    type = "image/png",
                    purpose = "maskable any"
                }));

                var responseData = new
                {
                    name = obj.PwaName,
                    short_name = obj.PwaShortName,
                    start_url = "/dashboard",
                    display = "standalone",
                    background_color = "#fdfdfd",
                    theme_color = "#db4938",
                    orientation = "portrait-primary",
                    icons = jsonArray
                };

                string jsonData = JsonConvert.SerializeObject(responseData, Formatting.None);
                File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass1), jsonData);
                oClsPwaSettings.Manifest = filepathPass1;
                // create manifest

                oConnectionContext.DbClsPwaSettings.Attach(oClsPwaSettings);
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.PwaName).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.PwaShortName).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.BackgroundColor).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.ThemeColor).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_48_48).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_72_72).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_96_96).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_128_128).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_144_144).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_144_144).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_152_152).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_192_192).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_284_284).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Image_512_512).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.Manifest).IsModified = true;

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Pwa Settings\" updated",
                    Id = oClsPwaSettings.PwaSettingsId,
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
                    Message = "Pwa Settings updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PwaSettingsDelete(ClsPwaSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPwaSettings oClsPwaSettings = new ClsPwaSettings()
                {
                    PwaSettingsId = obj.PwaSettingsId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPwaSettings.Attach(oClsPwaSettings);
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.PwaSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPwaSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Pwa Settings\" deleted",
                    Id = oClsPwaSettings.PwaSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Pwa Setting deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActivePwaSettings(ClsPwaSettingsVm obj)
        {
            var det = oConnectionContext.DbClsPwaSettings.Where(a => a.IsDeleted == false && a.IsActive == true && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.PwaName,
                a.PwaShortName,
                a.PwaSettingsId
            }).OrderBy(a => a.PwaName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PwaSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
