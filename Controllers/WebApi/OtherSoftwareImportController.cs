using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class OtherSoftwareImportController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllOtherSoftwareImports(ClsOtherSoftwareImportVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsOtherSoftwareImport.Where(a =>a.IsDeleted == false).Select(a => new
            {
                a.Remarks,
                a.OtherSoftwareImportId,
                a.Type,
                a.Status,
                a.UploadPath,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                Name = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.Name).FirstOrDefault(),
                EmailId = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.EmailId).FirstOrDefault(),
                MobileNo = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.MobileNo).FirstOrDefault(),
                a.ImportNo
            }).ToList();

            if(obj.UserId != 0)
            {
                det = det.Where(a => a.CompanyId == obj.UserId).ToList();
            }

            if (obj.Type != "" && obj.Type != null)
            {
                det = det.Where(a => a.Type.ToLower() == obj.Type.ToLower()).ToList();
            }

            if (obj.Name != "" && obj.Name != null)
            {
                det = det.Where(a => a.Name.ToLower() == obj.Name.ToLower()).ToList();
            }

            if (obj.MobileNo != "" && obj.MobileNo!= null)
            {
                det = det.Where(a => a.MobileNo.ToLower() == obj.MobileNo.ToLower()).ToList();
            }

            if (obj.EmailId != "" && obj.EmailId!= null)
            {
                det = det.Where(a => a.EmailId.ToLower() == obj.EmailId.ToLower()).ToList();
            }

            if (obj.ImportNo != "" && obj.ImportNo != null)
            {
                det = det.Where(a => a.ImportNo == obj.ImportNo).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OtherSoftwareImports = det.OrderByDescending(a => a.OtherSoftwareImportId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PendingCount = det.Where(a => a.Status == 1).Count(),
                    UploadedCount = det.Where(a => a.Status== 2).Count(),
                    RejectedCount = det.Where(a => a.Status== 3).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertOtherSoftwareImport(ClsOtherSoftwareImportVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.UploadPath == null || obj.UploadPath == "")
                {
                    errors.Add(new ClsError { Message = "Upload Excel first", Id = "divOtherSoftwareExcel" });
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

                obj.ImportNo= DateTime.Now.ToFileTime().ToString();

                ClsOtherSoftwareImport oOtherSoftwareImport = new ClsOtherSoftwareImport()
                {
                    Type = obj.Type,
                    Status= 1,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    ImportNo = obj.ImportNo
                };

                if (obj.UploadPath != "" && obj.UploadPath != null && !obj.UploadPath.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Excel/OtherSoftwareImport/" + obj.Type + "/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtension;

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Excel/OtherSoftwareImport/" + obj.Type);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string base64 = obj.UploadPath.Replace(obj.UploadPath.Substring(0, obj.UploadPath.IndexOf(',') + 1), "");
                    File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oOtherSoftwareImport.UploadPath = filepathPass;
                }

                oConnectionContext.DbClsOtherSoftwareImport.Add(oOtherSoftwareImport);
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    OtherSoftwareImport = "OtherSoftwareImport",
                //    CompanyId = obj.CompanyId,
                //    Description = "added " + obj.OtherSoftwareImport,
                //    Id = oOtherSoftwareImport.OtherSoftwareImportId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Insert"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Import request created successfully",
                    Data = new
                    {
                        OtherSoftwareImport = oOtherSoftwareImport
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> OtherSoftwareImportDelete(ClsOtherSoftwareImportVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsOtherSoftwareImport oClsRole = new ClsOtherSoftwareImport()
                {
                    OtherSoftwareImportId = obj.OtherSoftwareImportId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsOtherSoftwareImport.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.OtherSoftwareImportId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    OtherSoftwareImport = "OtherSoftwareImport",
                //    CompanyId = obj.CompanyId,
                //    Description = "deleted  " + oConnectionContext.DbClsOtherSoftwareImport.Where(a => a.OtherSoftwareImportId == obj.OtherSoftwareImportId).Select(a => a.OtherSoftwareImport).FirstOrDefault(),
                //    Id = oClsRole.OtherSoftwareImportId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Delete"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Import request deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateStatus(ClsOtherSoftwareImportVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsOtherSoftwareImport oClsRole = new ClsOtherSoftwareImport()
                {
                    OtherSoftwareImportId = obj.OtherSoftwareImportId,
                    Status = obj.Status,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsOtherSoftwareImport.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.OtherSoftwareImportId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    OtherSoftwareImport = "OtherSoftwareImport",
                //    CompanyId = obj.CompanyId,
                //    Description = "deleted  " + oConnectionContext.DbClsOtherSoftwareImport.Where(a => a.OtherSoftwareImportId == obj.OtherSoftwareImportId).Select(a => a.OtherSoftwareImport).FirstOrDefault(),
                //    Id = oClsRole.OtherSoftwareImportId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Delete"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Status updated successfully",
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
