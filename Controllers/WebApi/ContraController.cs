using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class ContraController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllContras(ClsContraVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsContraVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsContra.Where(b => b.IsDeleted == false && b.CompanyId == obj.CompanyId
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate).Select(b => new ClsContraVm
                {
                    FromAccountId = b.FromAccountId,
                    ToAccountId = b.ToAccountId,
                    PaymentTypeId = b.PaymentTypeId,
                    Type = b.Type,
                    InvoiceUrl = oCommonController.webUrl,
                    ReferenceNo = b.ReferenceNo,
                    FromAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    ToAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    ContraId = b.ContraId,
                    PaymentDate = b.PaymentDate,
                    Notes = b.Notes,
                    Amount = b.Amount,
                    AttachDocument = b.AttachDocument,
                    AddedOn = b.AddedOn,
                    ModifiedOn = b.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    ReferenceId = b.ReferenceId
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsContra.Where(b => b.IsDeleted == false && b.CompanyId == obj.CompanyId
                && b.BranchId == obj.BranchId
                && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate).Select(b => new ClsContraVm
                {
                    FromAccountId = b.FromAccountId,
                    ToAccountId = b.ToAccountId,
                    PaymentTypeId = b.PaymentTypeId,
                    Type = b.Type,
                    InvoiceUrl = oCommonController.webUrl,
                    ReferenceNo = b.ReferenceNo,
                    FromAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    ToAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    ContraId = b.ContraId,
                    PaymentDate = b.PaymentDate,
                    Notes = b.Notes,
                    Amount = b.Amount,
                    AttachDocument = b.AttachDocument,
                    AddedOn = b.AddedOn,
                    ModifiedOn = b.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                    ReferenceId = b.ReferenceId
                }).ToList();
            }

            if (obj.Type != "" && obj.Type != null)
            {
                det = det.Where(a => a.Type.ToLower() == obj.Type.ToLower()).ToList();
            }

            if (obj.FromAccountId != 0)
            {
                det = det.Where(a => a.FromAccountId == obj.FromAccountId).ToList();
            }

            if (obj.ToAccountId != 0)
            {
                det = det.Where(a => a.ToAccountId == obj.ToAccountId).ToList();
            }

            if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
            {
                det = det.Where(a => a.ReferenceNo == obj.ReferenceNo).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Contras = det.OrderByDescending(a => a.ContraId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    //InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertContra(ClsContraVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int IsDebit = 0;

                if (obj.FromAccountId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFromAccount" });
                    isError = true;
                }

                if ( obj.ToAccountId== 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divToAccount" });
                    isError = true;
                }

                if (obj.Amount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                    isError = true;
                }

                if (obj.PaymentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
                    isError = true;
                }

                if (obj.FromAccountId != 0 && obj.ToAccountId != 0)
                {
                    if (obj.FromAccountId == obj.ToAccountId)
                    {
                        errors.Add(new ClsError { Message = "From & To Account cannot be same", Id = "divToAccount" });
                        isError = true;
                    }
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsContra.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Refrence No exists", Id = "divReferenceNo" });
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

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == obj.Type.ToLower()
                                          && b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                ClsContra oClsPayment = new ClsContra()
                {
                    PaymentTypeId = obj.PaymentTypeId,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    FromAccountId = obj.FromAccountId,
                    AttachDocument = obj.AttachDocument,
                    Type = obj.Type,
                    BranchId = obj.BranchId,
                    ToAccountId = obj.ToAccountId,
                    ReferenceNo = obj.ReferenceNo,
                    ReferenceId = oCommonController.CreateToken(),
                    PrefixId = PrefixId
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

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPayment.AttachDocument = filepathPass;
                }
                oConnectionContext.DbClsContra.Add(oClsPayment);
                oConnectionContext.SaveChanges();

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Contra",
                    CompanyId = obj.CompanyId,
                    Description = "Contra (" + obj.Type + ") \"" + obj.ReferenceNo + "\" created",
                    Id = oClsPayment.ContraId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Contra (" + obj.Type +") created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Contra(ClsContraVm obj)
        {
            var det = oConnectionContext.DbClsContra.Where(b => b.ContraId == obj.ContraId && b.CompanyId == obj.CompanyId && b.IsDeleted == false).Select(b => new
            {
                b.PaymentTypeId,
                b.Type,
                b.FromAccountId,
                b.ToAccountId,
                b.ReferenceNo,
                //AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.ContraId,
                b.PaymentDate,
                b.Notes,
                b.Amount,
                b.AttachDocument,
                b.AddedOn,
                b.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                b.BranchId,
                b.ReferenceId
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Contra = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ContraDelete(ClsContraVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsContra oClsPayment = new ClsContra()
                {
                    ContraId = obj.ContraId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsContra.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var paymentDetails = oConnectionContext.DbClsContra.Where(a => a.ContraId == obj.ContraId).Select(a =>
                new
                {
                    a.FromAccountId,
                    a.Amount,
                }).FirstOrDefault();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Contra",
                    CompanyId = obj.CompanyId,
                    Description = "Contra (" + oConnectionContext.DbClsContra.Where(a => a.ContraId == obj.ContraId).Select(a => a.Type).FirstOrDefault() + ") \"" + oConnectionContext.DbClsContra.Where(a => a.ContraId == obj.ContraId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPayment.ContraId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Contra (" + oConnectionContext.DbClsContra.Where(a => a.ContraId == obj.ContraId).Select(a => a.Type).FirstOrDefault() + ") deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateContra(ClsContraVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int IsDebit = 0;

                if (obj.FromAccountId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFromAccount" });
                    isError = true;
                }

                if (obj.ToAccountId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divToAccount" });
                    isError = true;
                }


                if (obj.Amount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                    isError = true;
                }

                if(obj.FromAccountId != 0 && obj.ToAccountId!= 0)
                {
                    if (obj.FromAccountId == obj.ToAccountId)
                    {
                        errors.Add(new ClsError { Message = "From & To Account cannot be same", Id = "divToAccount" });
                        isError = true;
                    }
                }                

                if (obj.PaymentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
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

                ClsContra oClsPayment = new ClsContra()
                {
                    PaymentTypeId = obj.PaymentTypeId,
                    ContraId = obj.ContraId,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    FromAccountId = obj.FromAccountId,
                    AttachDocument = obj.AttachDocument,
                    BranchId = obj.BranchId,
                    ToAccountId = obj.ToAccountId,
                    ReferenceNo = obj.ReferenceNo,
                    ReferenceId = oCommonController.CreateToken(),
                    Type = obj.Type
                };

                string pic1 = oConnectionContext.DbClsContra.Where(a => a.ContraId == obj.ContraId).Select(a => a.AttachDocument).FirstOrDefault();
                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }
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

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPayment.AttachDocument = filepathPass;
                }
                else
                {
                    oClsPayment.AttachDocument = pic1;
                }

                oConnectionContext.DbClsContra.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.Amount).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.PaymentDate).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.FromAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ToAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ReferenceNo).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ReferenceId).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.Type).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.PaymentTypeId).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Contra",
                    CompanyId = obj.CompanyId,
                    Description = "Contra ("+obj.Type+") \"" + oConnectionContext.DbClsContra.Where(a => a.ContraId == obj.ContraId).Select(a => a.ReferenceNo).FirstOrDefault()+"\" updated",
                    Id = oClsPayment.ContraId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Contra (" +obj.Type+ ") updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ContraJournal(ClsContraVm obj)
        {
            var journal = (from a in oConnectionContext.DbClsContra
                           where a.ContraId == obj.ContraId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = 0,
                               Credit = a.Amount
                           }).Concat(from a in oConnectionContext.DbClsContra
                                     where a.ContraId == obj.ContraId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     select new ClsBankPaymentVm
                                     {
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = a.Amount,
                                         Credit = 0
                                     }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = journal
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportContra(ClsContraVm obj)
        {
            //using (TransactionScope dbContextTransaction = new TransactionScope())
            //{
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.ContraImports == null || obj.ContraImports.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No data",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            // Check for duplicate ReferenceNo values within the import list
            if (obj.ContraImports != null)
            {
                var reportedRows = new HashSet<int>();
                
                for (int i = 0; i < obj.ContraImports.Count; i++)
                {
                    var item = obj.ContraImports[i];
                    
                    if (!string.IsNullOrEmpty(item.ReferenceNo))
                    {
                        for (int j = i + 1; j < obj.ContraImports.Count; j++)
                        {
                            var other = obj.ContraImports[j];
                            
                            if (!string.IsNullOrEmpty(other.ReferenceNo))
                            {
                                if (item.ReferenceNo.ToLower() == other.ReferenceNo.ToLower())
                                {
                                    int rowNumber = j + 1;
                                    if (!reportedRows.Contains(rowNumber))
                                    {
                                        errors.Add(new ClsError { Message = "Duplicate ReferenceNo '" + item.ReferenceNo + "' exists in row no " + rowNumber, Id = "" });
                                        isError = true;
                                        reportedRows.Add(rowNumber);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int count = 1;
            if (obj.ContraImports != null)
            {
                foreach (var item in obj.ContraImports)
                {
                    if (item.BranchName != "" && item.BranchName != null)
                    {
                        if ((from a in oConnectionContext.DbClsBranch
                             where a.IsDeleted == false && a.Branch.ToLower() == item.BranchName.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.BranchId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid BranchName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.ReferenceNo != "" && item.ReferenceNo != null)
                    {
                        if (oConnectionContext.DbClsContra.Where(a => a.ReferenceNo == item.ReferenceNo && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Refrence No in row no" + count + " is already taken", Id = "divReferenceNo" });
                            isError = true;
                        }
                    }

                    if (item.Type == "" || item.Type == null)
                    {
                        errors.Add(new ClsError { Message = "Type is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.Type != "" && item.Type != null)
                    {
                        if(item.Type.ToLower() != "fund transfer" && item.Type.ToLower() != "deposit" && item.Type.ToLower() != "withdraw")
                        {
                            errors.Add(new ClsError { Message = "Invalid Type in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.FromAccountName == "" || item.FromAccountName == null)
                    {
                        errors.Add(new ClsError { Message = "From Account is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.FromAccountName != "" && item.FromAccountName != null)
                    {
                        if ((from a in oConnectionContext.DbClsAccount
                             where a.IsDeleted == false && a.AccountName.ToLower() == item.FromAccountName.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.AccountId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid FromAccountName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.ToAccountName == "" || item.ToAccountName == null)
                    {
                        errors.Add(new ClsError { Message = "To Account is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.ToAccountName != "" && item.ToAccountName != null)
                    {
                        if ((from a in oConnectionContext.DbClsAccount
                             where a.IsDeleted == false && a.AccountName.ToLower() == item.ToAccountName.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.AccountId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid ToAccountName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.Amount == 0)
                    {
                        errors.Add(new ClsError { Message = "Amount is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.PaymentType != "" && item.PaymentType != null)
                    {
                        if(oConnectionContext.DbClsPaymentType.Where(a => a.IsDeleted == false && a.PaymentType.ToLower() == item.PaymentType.ToLower()
                             && a.CompanyId == obj.CompanyId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid PaymentType in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    count++;
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

            foreach (var item in obj.ContraImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    long BranchId = 0, PrefixUserMapId = 0, PaymentTypeId = 0, FromAccountId = 0, ToAccountId = 0;
                    string ReferenceNo = "";

                    if (item.BranchName == "" || item.BranchName == null)
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BranchId).FirstOrDefault();
                    }
                    else
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.Branch == item.BranchName && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.BranchId).FirstOrDefault();
                    }

                    if (item.PaymentType != "" && item.PaymentType != null)
                    {
                        PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentType == item.PaymentType).Select(a => a.PaymentTypeId).FirstOrDefault();
                    }

                    FromAccountId = oConnectionContext.DbClsAccount.Where(a => a.AccountName.ToLower() == item.FromAccountName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.AccountId).FirstOrDefault();
                    ToAccountId = oConnectionContext.DbClsAccount.Where(a => a.AccountName.ToLower() == item.ToAccountName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.AccountId).FirstOrDefault();

                    if (item.ReferenceNo == "" || item.ReferenceNo == null)
                    {
                        long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == BranchId).Select(a => a.PrefixId).FirstOrDefault();
                        var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                              join b in oConnectionContext.DbClsPrefixUserMap
                                               on a.PrefixMasterId equals b.PrefixMasterId
                                              where a.IsActive == true && a.IsDeleted == false &&
                                              b.CompanyId == obj.CompanyId && b.IsActive == true
                                              && b.IsDeleted == false && a.PrefixType.ToLower() == item.Type.ToLower()
                                              && b.PrefixId == PrefixId
                                              select new
                                              {
                                                  b.PrefixUserMapId,
                                                  b.Prefix,
                                                  b.NoOfDigits,
                                                  b.Counter
                                              }).FirstOrDefault();
                        PrefixUserMapId = prefixSettings.PrefixUserMapId;
                        ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    }
                    else
                    {
                        ReferenceNo = item.ReferenceNo;
                    }

                        ClsContra oClsPayment = new ClsContra()
                        {
                            PaymentTypeId = PaymentTypeId,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            Notes = item.Notes,
                            Amount = item.Amount,
                            PaymentDate = (item.PaymentDate == DateTime.MinValue ? CurrentDate : item.PaymentDate).AddHours(5).AddMinutes(30),
                            FromAccountId = FromAccountId,
                            AttachDocument = "",
                            Type = item.Type,
                            BranchId = BranchId,
                            ToAccountId = ToAccountId,
                            ReferenceNo = ReferenceNo,
                            ReferenceId = oCommonController.CreateToken(),
                            BatchNo = obj.BatchNo
                        };
                    oConnectionContext.DbClsContra.Add(oClsPayment);
                    oConnectionContext.SaveChanges();

                    //increase counter
                    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(q);
                    //increase counter

                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Contra",
                CompanyId = obj.CompanyId,
                Description = "Contra imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Contra imported successfully",
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }
        
        public async Task<IHttpActionResult> ContraCountByBatch(ClsContraVm obj)
        {
            long TotalCount = oConnectionContext.DbClsContra.Where(a => a.BatchNo == obj.BatchNo).Count();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TotalCount = TotalCount
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
