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

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class JournalController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllJournals(ClsJournalVm obj)
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

            List<ClsJournalVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsJournal.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                //&& a.BranchId == obj.BranchId 
                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                ).Select(a => new ClsJournalVm
                {
                    BranchId = a.BranchId,
                    Date = a.Date,
                    IsCashBased = a.IsCashBased,
                    CurrencyId = a.CurrencyId,
                    JournalId = a.JournalId,
                    ReferenceNo = a.ReferenceNo,
                    Notes = a.Notes,
                    AttachDocument = a.AttachDocument,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    TotalAmount = oConnectionContext.DbClsJournalPayment.Where(b => b.JournalId == a.JournalId && b.IsDeleted == false).Select(b=>
                    b.Credit).DefaultIfEmpty().Sum(),
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    CompanyId = a.CompanyId,
                    AddedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ModifiedBy).Select(b => b.Username).FirstOrDefault(),
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsJournal.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.BranchId == obj.BranchId
                               && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                               ).Select(a => new ClsJournalVm
                               {
                                   BranchId = a.BranchId,
                                   Date = a.Date,
                                   IsCashBased = a.IsCashBased,
                                   CurrencyId = a.CurrencyId,
                                   JournalId = a.JournalId,
                                   ReferenceNo = a.ReferenceNo,
                                   Notes = a.Notes,
                                   AttachDocument = a.AttachDocument,
                                   IsActive = a.IsActive,
                                   IsDeleted = a.IsDeleted,
                                   TotalAmount = oConnectionContext.DbClsJournalPayment.Where(b => b.JournalId == a.JournalId && b.IsDeleted == false).Select(b =>
                    b.Credit).DefaultIfEmpty().Sum(),
                                   AddedBy = a.AddedBy,
                                   AddedOn = a.AddedOn,
                                   ModifiedBy = a.ModifiedBy,
                                   ModifiedOn = a.ModifiedOn,
                                   CompanyId = a.CompanyId,
                                   AddedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Username).FirstOrDefault(),
                                   ModifiedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ModifiedBy).Select(b => b.Username).FirstOrDefault(),
                               }).ToList();
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
                    Journals = det.OrderByDescending(a => a.JournalId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Journal(ClsJournal obj)
        {
            var det = oConnectionContext.DbClsJournal.Where(a => a.JournalId == obj.JournalId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                BranchId = a.BranchId,
                Date = a.Date,
                IsCashBased = a.IsCashBased,
                CurrencyId = a.CurrencyId,
                JournalId = a.JournalId,
                ReferenceNo = a.ReferenceNo,
                Notes = a.Notes,
                AttachDocument = a.AttachDocument,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ModifiedBy).Select(b => b.Username).FirstOrDefault(),
                JournalPayments = oConnectionContext.DbClsJournalPayment.Where(b => b.JournalId == a.JournalId && b.IsDeleted == false).Select(b => new
                {
                    b.AccountId,
                    b.Credit,
                    b.Debit,
                    b.JournalPaymentId,
                    b.Notes,
                    b.ExpenseFor
                })
            }).FirstOrDefault();

            //var JournalSubCategories = oConnectionContext.DbClsJournalSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            //&& a.IsActive == true && a.JournalCategoryId == det.JournalCategoryId).Select(a => new
            //{
            //    JournalSubCategoryId = a.JournalSubCategoryId,
            //    a.JournalSubCategoryCode,
            //    JournalSubCategory = a.JournalSubCategory,
            //}).OrderBy(a => a.JournalSubCategory).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Journal = det,
                    //JournalSubCategories = JournalSubCategories
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertJournal(ClsJournalVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsJournal.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Refrence No exists", Id = "divReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.JournalPayments == null)
                {
                    errors.Add(new ClsError { Message = "Please add Debits & Credits", Id = "divJournalPayment" });
                    isError = true;
                }

                if (obj.JournalPayments != null)
                {
                    if (obj.JournalPayments.Select(a => a.Debit).DefaultIfEmpty().Sum() == 0 && obj.JournalPayments.Select(a => a.Credit).DefaultIfEmpty().Sum() == 0)
                    {
                        errors.Add(new ClsError { Message = "Please add Debits & Credits", Id = "divJournalPayment" });
                        isError = true;
                    }

                    if (obj.JournalPayments.Select(a => a.Debit).DefaultIfEmpty().Sum() != obj.JournalPayments.Select(a => a.Credit).DefaultIfEmpty().Sum())
                    {
                        errors.Add(new ClsError { Message = "Debits & Credits should be equal", Id = "divJournalPayment" });
                        isError = true;
                    }
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsJournal.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
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
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "journal"
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

                ClsJournal oJournal = new ClsJournal()
                {
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    ReferenceNo = obj.ReferenceNo,
                    Notes = obj.Notes,
                    IsCashBased = obj.IsCashBased,
                    CurrencyId = obj.CurrencyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BranchId = obj.BranchId,
                    ReferenceId = oCommonController.CreateToken(),
                    PrefixId = PrefixId
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Journal/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Journal/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oJournal.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsJournal.Add(oJournal);
                oConnectionContext.SaveChanges();

                if (obj.JournalPayments != null)
                {
                    foreach (var item in obj.JournalPayments)
                    {
                        if (item.Credit != 0 || item.Debit != 0)
                        {
                            ClsJournalPayment oClsJournalPayment = new ClsJournalPayment()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                IsActive = obj.IsActive,
                                IsDeleted = obj.IsDeleted,
                                JournalId = oJournal.JournalId,
                                Notes = item.Notes,
                                Credit = item.Credit,
                                Debit = item.Debit,
                                AccountId = item.AccountId,
                                ExpenseFor = item.ExpenseFor,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsJournalPayment.Add(oClsJournalPayment);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Journal",
                    CompanyId = obj.CompanyId,
                    Description = "Journal \"" + obj.ReferenceNo+"\" created",
                    Id = oJournal.JournalId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Journal created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateJournal(ClsJournalVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.JournalPayments == null)
                {
                    errors.Add(new ClsError { Message = "Please add Debits & Credits", Id = "divJournalPayment" });
                    isError = true;
                }

                if (obj.JournalPayments != null)
                {
                    if (obj.JournalPayments.Select(a => a.Debit).DefaultIfEmpty().Sum() == 0 && obj.JournalPayments.Select(a => a.Credit).DefaultIfEmpty().Sum() == 0)
                    {
                        errors.Add(new ClsError { Message = "Please add Debits & Credits", Id = "divJournalPayment" });
                        isError = true;
                    }

                    if (obj.JournalPayments.Select(a => a.Debit).DefaultIfEmpty().Sum() != obj.JournalPayments.Select(a => a.Credit).DefaultIfEmpty().Sum())
                    {
                        errors.Add(new ClsError { Message = "Debits & Credits should be equal", Id = "divJournalPayment" });
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

                ClsJournal oJournal = new ClsJournal()
                {
                    JournalId = obj.JournalId,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    ReferenceNo = obj.ReferenceNo,
                    Notes = obj.Notes,
                    IsCashBased = obj.IsCashBased,
                    CurrencyId = obj.CurrencyId,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = CurrentDate,
                    BranchId = obj.BranchId,
                };

                string pic1 = oConnectionContext.DbClsJournal.Where(a => a.JournalId == obj.JournalId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/Journal/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Journal/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oJournal.AttachDocument = filepathPass;
                }
                else
                {
                    oJournal.AttachDocument = pic1;
                }

                oConnectionContext.DbClsJournal.Attach(oJournal);
                oConnectionContext.Entry(oJournal).Property(x => x.JournalId).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.Date).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.ReferenceNo).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.IsCashBased).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oJournal).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.JournalPayments != null)
                {
                    foreach (var item in obj.JournalPayments)
                    {
                        if (item.IsDeleted == true)
                        {
                            string query = "update \"tblJournalPayment\" set \"IsDeleted\"=True where \"JournalPaymentId\"=" + item.JournalPaymentId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        else
                        {
                            if (item.Credit != 0 || item.Debit != 0)
                            {
                                if (item.JournalPaymentId == 0)
                                {
                                    ClsJournalPayment oClsJournalPayment = new ClsJournalPayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        JournalId = oJournal.JournalId,
                                        Notes = item.Notes,
                                        Credit = item.Credit,
                                        Debit = item.Debit,
                                        AccountId = item.AccountId,
                                        ExpenseFor = item.ExpenseFor,
                                    };
                                    //ConnectionContext ocon = new ConnectionContext();
                                    oConnectionContext.DbClsJournalPayment.Add(oClsJournalPayment);
                                    oConnectionContext.SaveChanges();
                                }
                                else
                                {
                                    ClsJournalPayment oClsJournalPayment = new ClsJournalPayment()
                                    {
                                        JournalPaymentId = item.JournalPaymentId,
                                        ModifiedBy = obj.ModifiedBy,
                                        ModifiedOn = CurrentDate,
                                        Notes = item.Notes,
                                        Credit = item.Credit,
                                        Debit = item.Debit,
                                        AccountId = item.AccountId,
                                        ExpenseFor = item.ExpenseFor,
                                    };
                                    //ConnectionContext ocon = new ConnectionContext();
                                    oConnectionContext.DbClsJournalPayment.Attach(oClsJournalPayment);
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.ModifiedBy).IsModified = true;
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.ModifiedOn).IsModified = true;
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.Notes).IsModified = true;
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.Credit).IsModified = true;
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.Debit).IsModified = true;
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.AccountId).IsModified = true;
                                    oConnectionContext.Entry(oClsJournalPayment).Property(x => x.ExpenseFor).IsModified = true;
                                    oConnectionContext.SaveChanges();
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Journal",
                    CompanyId = obj.CompanyId,
                    Description = "Journal \"" + obj.ReferenceNo+"\" updated",
                    Id = oJournal.JournalId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Journal updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> JournalDelete(ClsJournalVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsJournal oClsRole = new ClsJournal()
                {
                    JournalId = obj.JournalId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsJournal.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.JournalId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Journal",
                    CompanyId = obj.CompanyId,
                    Description = "Journal \"" + oConnectionContext.DbClsJournal.Where(a => a.JournalId == obj.JournalId).Select(a => a.ReferenceNo).FirstOrDefault()+"\" deleted",
                    Id = oClsRole.JournalId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Journal deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        public async Task<IHttpActionResult> JournalPaymentDelete(ClsJournalPaymentVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.JournalId != 0)
                {
                    string query = "update \"tblJournalPayment\" set \"IsDeleted\"=True where \"JournalId\"=" + obj.JournalId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblJournalPayment\" set \"IsDeleted\"=True where \"JournalPaymentId\"=" + obj.JournalPaymentId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                data = new
                {
                    Status = 1,
                    Message = "Deleted successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [AllowAnonymous]
        public async Task<IHttpActionResult> JournalReport(ClsJournalVm obj)
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

            List<ClsContraVm> Contra = new List<ClsContraVm>();
            List<ClsContraVm> Expense = new List<ClsContraVm>();

            List<ClsSalesVm> FinalJournal = new List<ClsSalesVm>();

            if (obj.BranchId == 0)
            {
                #region Contra
                Contra = oConnectionContext.DbClsContra.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                ).Select(a => new ClsContraVm
                {
                    Type = a.Type,
                    ReferenceNo = a.ReferenceNo,
                    BranchId = a.BranchId,
                    PaymentDate = a.PaymentDate,
                    FromAccountId = a.FromAccountId,
                    ToAccountId = a.ToAccountId,
                    ContraId = a.ContraId,
                    Amount = a.Amount
                }).ToList();

                foreach (var res in Contra)
                {
                    FinalJournal.Add((from x in Contra
                                      where x.ContraId == res.ContraId
                                      select new ClsSalesVm
                                      {
                                          Type = x.Type,
                                          InvoiceNo = x.ReferenceNo,
                                          SalesDate = x.PaymentDate,
                                          Payments = (from a in Contra
                                                      where a.ContraId == res.ContraId
                                                      select new ClsCustomerPaymentVm
                                                      {
                                                          AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.FromAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                          Debit = 0,
                                                          Credit = a.Amount
                                                      }).Concat(from a in Contra
                                                                where a.ContraId == res.ContraId
                                                                select new ClsCustomerPaymentVm
                                                                {
                                                                    AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.ToAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                    Debit = a.Amount,
                                                                    Credit = 0
                                                                }).ToList()
                                      }).FirstOrDefault());
                }
                #endregion



            }
            else
            {

            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Journals = FinalJournal.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = FinalJournal.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportJournal(ClsJournalVm obj)
        {
            //using (TransactionScope dbContextTransaction = new TransactionScope())
            //{
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.JournalImports == null || obj.JournalImports.Count == 0)
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

            // Check for duplicate ReferenceNo values across different GroupNames
            if (obj.JournalImports != null)
            {
                var reportedRows = new HashSet<int>();
                
                for (int i = 0; i < obj.JournalImports.Count; i++)
                {
                    var item = obj.JournalImports[i];
                    
                    if (!string.IsNullOrEmpty(item.ReferenceNo))
                    {
                        for (int j = i + 1; j < obj.JournalImports.Count; j++)
                        {
                            var other = obj.JournalImports[j];
                            
                            if (!string.IsNullOrEmpty(other.ReferenceNo))
                            {
                                if (item.ReferenceNo.ToLower() == other.ReferenceNo.ToLower() && 
                                    item.GroupName != other.GroupName)
                                {
                                    int rowNumber = j + 1;
                                    if (!reportedRows.Contains(rowNumber))
                                    {
                                        errors.Add(new ClsError { Message = "Duplicate ReferenceNo '" + item.ReferenceNo + "' exists across different groups in row no " + rowNumber, Id = "" });
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
            List<ClsJournalImport> journalGroupsValidate = new List<ClsJournalImport>();
            bool canVariationChecked = true;
            if (obj.JournalImports != null)
            {
                foreach (var item in obj.JournalImports)
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
                        if (oConnectionContext.DbClsJournal.Where(a => a.ReferenceNo == item.ReferenceNo && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Refrence No in row no" + count + " is already taken", Id = "divReferenceNo" });
                            isError = true;
                        }
                    }

                    if (item.Account == "" || item.Account == null)
                    {
                        errors.Add(new ClsError { Message = "Account is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.Account != "" && item.Account != null)
                    {
                        if ((from a in oConnectionContext.DbClsAccount
                             where a.IsDeleted == false && a.AccountName.ToLower() == item.Account.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.AccountId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid Account in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.Contact != "" && item.Contact != null)
                    {
                        var _contact = item.Contact.Split('|');
                        if (_contact.Length < 2)
                        {
                            errors.Add(new ClsError { Message = "Invalid Contact in row no " + count, Id = "" });
                            isError = true;
                        }
                        else
                        {
                            string _mobileNo = _contact[0].Trim();
                            string _userType = _contact[1].Trim();
                            if ((from a in oConnectionContext.DbClsUser
                                 where a.IsDeleted == false && a.MobileNo.ToLower() == _mobileNo.ToLower()
                                 && a.CompanyId == obj.CompanyId && (a.UserType.ToLower() == _userType.ToLower())
                                 select a.UserId).Count() == 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid Contact in row no " + count, Id = "" });
                                isError = true;
                            }
                        }                        
                    }

                    if (item.Debit == 0 && item.Credit == 0)
                    {
                        errors.Add(new ClsError { Message = "Debit / Credit is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.GroupName == "" || item.GroupName == null)
                    {
                        errors.Add(new ClsError { Message = "GroupName is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.GroupName != "" && item.GroupName != null)
                    {
                        if (journalGroupsValidate.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Count() == 0)
                        {
                            canVariationChecked = true;
                        }
                        else
                        {
                            canVariationChecked = false;
                        }

                        if (canVariationChecked == true)
                        {
                            if (obj.JournalImports.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Select(a => a.Debit).DefaultIfEmpty().Sum() !=
                                obj.JournalImports.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Select(a => a.Credit).DefaultIfEmpty().Sum())
                            {
                                errors.Add(new ClsError { Message = "Debits & Credits should be equal for GroupName " + item.GroupName + " in row no " + count, Id = "" });
                                isError = true;
                            }

                            journalGroupsValidate.Add(new ClsJournalImport { GroupName = item.GroupName });
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

            List<ClsJournalImport> journalGroups = new List<ClsJournalImport>();
            foreach (var item in obj.JournalImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    long BranchId = 0, PrefixUserMapId = 0, AccountId = 0, ExpenseFor = 0;
                    string ReferenceNo = "";

                    long JournalId = 0;
                    bool canVariationInsert = true;

                    if (item.BranchName == "" || item.BranchName == null)
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BranchId).FirstOrDefault();
                    }
                    else
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.Branch == item.BranchName && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.BranchId).FirstOrDefault();
                    }

                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.AccountName == item.Account).Select(a => a.AccountId).FirstOrDefault();

                    string _mobileNo = "";
                    string _userType = "";

                    if(item.Contact != null && item.Contact != "")
                    {
                        var _contact = item.Contact.Split('|');
                        _mobileNo = _contact[0].Trim();
                        _userType = _contact[1].Trim();
                        ExpenseFor = oConnectionContext.DbClsUser.Where(a => a.IsDeleted == false && a.MobileNo.ToLower() == _mobileNo.ToLower()
                                 && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == _userType.ToLower()).Select(a => a.UserId).FirstOrDefault();
                    }
                    

                    if (journalGroups.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Count() == 0)
                    {
                        canVariationInsert = true;
                    }
                    else
                    {
                        JournalId = journalGroups.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Select(a => a.JournalId).FirstOrDefault();
                        canVariationInsert = false;
                    }

                    if (canVariationInsert == true)
                    {
                        if (item.ReferenceNo == "" || item.ReferenceNo == null)
                        {
                            long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == BranchId).Select(a => a.PrefixId).FirstOrDefault();
                            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                  join b in oConnectionContext.DbClsPrefixUserMap
                                                   on a.PrefixMasterId equals b.PrefixMasterId
                                                  where a.IsActive == true && a.IsDeleted == false &&
                                                  b.CompanyId == obj.CompanyId && b.IsActive == true
                                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "journal"
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

                            //increase counter
                            string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);
                            //increase counter
                        }
                        else
                        {
                            ReferenceNo = item.ReferenceNo;
                        }

                        if (item.Date == DateTime.MinValue)
                        {
                            item.Date = CurrentDate;
                        }

                        ClsJournal oJournal = new ClsJournal()
                        {
                            Date = item.Date.AddHours(5).AddMinutes(30),
                            ReferenceNo = ReferenceNo,
                            Notes = item.Notes,
                            IsCashBased = obj.IsCashBased,
                            CurrencyId = obj.CurrencyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            BranchId = BranchId,
                            ReferenceId = oCommonController.CreateToken(),
                            BatchNo = obj.BatchNo
                        };
                        oConnectionContext.DbClsJournal.Add(oJournal);
                        oConnectionContext.SaveChanges();

                        JournalId = oJournal.JournalId;

                        if (item.GroupName != "" && item.GroupName != null)
                        {
                            journalGroups.Add(new ClsJournalImport { GroupName = item.GroupName, JournalId = JournalId });
                        }
                    }

                    ClsJournalPayment oClsJournalPayment = new ClsJournalPayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        JournalId = JournalId,
                        Notes = item.Description,
                        Credit = item.Credit,
                        Debit = item.Debit,
                        AccountId = AccountId,
                        ExpenseFor = ExpenseFor,
                    };
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.DbClsJournalPayment.Add(oClsJournalPayment);
                    oConnectionContext.SaveChanges();

                    ////increase counter
                    //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                    //oConnectionContext.Database.ExecuteSqlCommand(q);
                    ////increase counter

                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Journal",
                CompanyId = obj.CompanyId,
                Description = "Journal imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Journal imported successfully",
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> JournalCountByBatch(ClsJournalVm obj)
        {
            long TotalCount = oConnectionContext.DbClsJournal.Where(a => a.BatchNo == obj.BatchNo).Count();

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
