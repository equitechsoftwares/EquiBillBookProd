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
    public class BankPaymentController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> InsertBankPayment(ClsBankPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
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

                if (obj.FromAccountId == obj.ToAccountId)
                {
                    errors.Add(new ClsError { Message = "From & To Account cannot be same", Id = "divAccount" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsBankPayment.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
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

                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
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

                ClsBankPayment oClsPayment = new ClsBankPayment()
                {
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
                    ReferenceId = oCommonController.CreateToken()
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
                oConnectionContext.DbClsBankPayment.Add(oClsPayment);
                oConnectionContext.SaveChanges();

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.Type + " Payment",
                    CompanyId = obj.CompanyId,
                    Description = "added payment of " + obj.Amount,
                    Id = oClsPayment.BankPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllBankPayments(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsBankPayment.Where(b => b.IsDeleted == false && b.CompanyId == obj.CompanyId
                && b.Type == obj.Type
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate).Select(b => new ClsBankPaymentVm 
                {
                    ReferenceNo = b.ReferenceNo,
                    FromAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    ToAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    BankPaymentId = b.BankPaymentId,
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
                det = oConnectionContext.DbClsBankPayment.Where(b => b.IsDeleted == false && b.CompanyId == obj.CompanyId
                && b.Type == obj.Type && b.BranchId == obj.BranchId
                && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate).Select(b => new ClsBankPaymentVm
                {
                    ReferenceNo = b.ReferenceNo,
                    FromAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    ToAccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                    BankPaymentId = b.BankPaymentId,
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

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = det.OrderByDescending(a => a.BankPaymentId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> BankPayment(ClsBankPaymentVm obj)
        {
            var det = oConnectionContext.DbClsBankPayment.Where(b => b.BankPaymentId == obj.BankPaymentId && b.CompanyId == obj.CompanyId && b.IsDeleted == false).Select(b => new
            {
                b.FromAccountId,
                b.ToAccountId,
                b.ReferenceNo,
                //AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == b.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                b.BankPaymentId,
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
                    BankPayment = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BankPaymentDelete(ClsBankPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsBankPayment oClsPayment = new ClsBankPayment()
                {
                    BankPaymentId = obj.BankPaymentId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBankPayment.Attach(oClsPayment);
                oConnectionContext.Entry(oClsPayment).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var paymentDetails = oConnectionContext.DbClsBankPayment.Where(a => a.BankPaymentId == obj.BankPaymentId).Select(a =>
                new
                {
                    a.FromAccountId,
                    a.Type,
                    a.Amount,
                }).FirstOrDefault();
                

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = paymentDetails.Type + " Payment",
                    CompanyId = obj.CompanyId,
                    Description = "deleted payment of " + paymentDetails.Amount,
                    Id = oClsPayment.BankPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateBankPayment(ClsBankPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
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

                if (obj.FromAccountId == obj.ToAccountId)
                {
                    errors.Add(new ClsError { Message = "From & To Account cannot be same", Id = "divAccount" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsBankPayment.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
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

                ClsBankPayment oClsPayment = new ClsBankPayment()
                {
                    BankPaymentId = obj.PaymentId,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    FromAccountId = obj.FromAccountId,
                    AttachDocument = obj.AttachDocument,
                    Type = obj.Type,
                    BranchId = obj.BranchId,
                    ToAccountId = obj.ToAccountId,
                    ReferenceNo = obj.ReferenceNo,
                    ReferenceId = oCommonController.CreateToken()
                };

                string pic1 = oConnectionContext.DbClsBankPayment.Where(a => a.BankPaymentId == obj.BankPaymentId).Select(a => a.AttachDocument).FirstOrDefault();
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

                oConnectionContext.DbClsBankPayment.Attach(oClsPayment);
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
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.Type,
                    CompanyId = obj.CompanyId,
                    Description = "modified payment of " + obj.Amount,
                    Id = oClsPayment.BankPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = obj.Type+" updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> WithdrawJournal(ClsBankPaymentVm obj)
        {
            var journal = (from a in oConnectionContext.DbClsBankPayment
                            where a.BankPaymentId == obj.BankPaymentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                            && a.Type == "Withdraw"
                            select new ClsBankPaymentVm
                            {
                                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                Debit = 0,
                                Credit = a.Amount
                            }).Union(from a in oConnectionContext.DbClsBankPayment
                                     where a.BankPaymentId == obj.BankPaymentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && a.Type == "Withdraw"
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

        public async Task<IHttpActionResult> DepositJournal(ClsBankPaymentVm obj)
        {
            var journal = (from a in oConnectionContext.DbClsBankPayment
                           where a.BankPaymentId == obj.BankPaymentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && a.Type == "Deposit"
                           select new ClsBankPaymentVm
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = a.Amount,
                               Credit = 0
                           }).Union(from a in oConnectionContext.DbClsBankPayment
                                    where a.BankPaymentId == obj.BankPaymentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                    && a.Type == "Deposit"
                                    select new ClsBankPaymentVm
                                    {
                                        AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                        Debit = 0,
                                        Credit = a.Amount
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

        public async Task<IHttpActionResult> FundTransferJournal(ClsBankPaymentVm obj)
        {
            var journal = (from a in oConnectionContext.DbClsBankPayment
                           where a.BankPaymentId == obj.BankPaymentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && a.Type == "Fund Transfer"
                           select new ClsBankPaymentVm
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = 0,
                               Credit = a.Amount
                           }).Union(from a in oConnectionContext.DbClsBankPayment
                                    where a.BankPaymentId == obj.BankPaymentId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                    && a.Type == "Fund Transfer"
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

    }
}
