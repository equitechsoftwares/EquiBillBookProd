using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Security;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class AccountController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllAccounts(ClsAccountVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                Type = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.Type).FirstOrDefault(),
                AccountType = oConnectionContext.DbClsAccountType.Where(b => b.AccountTypeId == a.AccountTypeId).Select(b => b.AccountType).FirstOrDefault(),
                AccountSubType = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.DisplayAs).FirstOrDefault(),
                a.Status,
                AccountId = a.AccountId,
                a.AccountName,
                a.AccountNumber,
                a.AccountSubTypeId,
                a.AccountTypeId,
                a.Notes,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.CanDelete,
                a.ParentId,
                a.DisplayAs,
            }).ToList();

            if (obj.Type == "Bank")
            {
                det = det.Where(a => a.Type == "Bank").ToList();
            }
            else if (obj.Type == "Credit Card")
            {
                det = det.Where(a => a.Type == "Credit Card").ToList();
            }
            else if (obj.Type == "Bank & Credit Card")
            {
                det = det.Where(a => (a.Type == "Bank" || a.Type == "Credit Card")).ToList();
            }
            //else
            //{
            //    det = det.Where(a => (a.Type != "Bank" && a.Type != "Credit Card")).ToList();
            //}

            if (obj.AccountTypeId != 0)
            {
                det = det.Where(a => a.AccountTypeId == obj.AccountTypeId).ToList();
            }

            if (obj.AccountSubTypeId != 0)
            {
                det = det.Where(a => a.AccountSubTypeId == obj.AccountSubTypeId).ToList();
            }

            if (obj.AccountName != "" && obj.AccountName != null)
            {
                det = det.Where(a => a.AccountName != null && a.AccountName.Contains(obj.AccountName)).ToList();
            }

            if (obj.AccountNumber != "" && obj.AccountNumber != null)
            {
                det = det.Where(a => a.AccountNumber != null && a.AccountNumber.Contains(obj.AccountNumber)).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Accounts = det.OrderByDescending(a => a.AccountId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Account(ClsAccount obj)
        {
            var det = oConnectionContext.DbClsAccount.Where(a => a.AccountId == obj.AccountId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                Type = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.Type).FirstOrDefault(),
                AccountType = oConnectionContext.DbClsAccountType.Where(b => b.AccountTypeId == a.AccountTypeId).Select(b => b.AccountType).FirstOrDefault(),
                AccountSubType = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.AccountSubType).FirstOrDefault(),
                AccountId = a.AccountId,
                a.AccountName,
                a.AccountNumber,
                a.AccountSubTypeId,
                a.AccountTypeId,
                a.Notes,
                //a.OpeningBalance,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.BankName,
                a.BranchName,
                a.BranchCode,
                a.ParentId,
                a.DisplayAs,
                AccountDetails = oConnectionContext.DbClsAccountDetails.Where(b => b.AccountId == a.AccountId).Select(b => new
                {
                    b.AccountDetailsId,
                    b.Label,
                    b.Value
                }).ToList()
            }).FirstOrDefault();

            var AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.AccountSubTypeId == det.AccountSubTypeId).Select(a => new
            {
                a.Type,
                a.DisplayAs,
                AccountType = oConnectionContext.DbClsAccountType.Where(b => b.AccountTypeId == a.AccountTypeId).Select(b => b.AccountType).FirstOrDefault(),
                Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountSubTypeId == a.AccountSubTypeId && aa.CompanyId == obj.CompanyId &&
                aa.IsDeleted == false && aa.IsActive == true).Select(aa => new
                {
                    aa.Type,
                    aa.AccountId,
                    aa.AccountName,
                    aa.AccountNumber,
                    aa.DisplayAs
                }).OrderBy(aa => aa.AccountId)
            }).ToList();

            //var AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            //&& a.IsActive == true && a.AccountTypeId == det.AccountTypeId).Select(a => new
            //{
            //    AccountSubTypeId = a.AccountSubTypeId,
            //    a.AccountSubTypeCode,
            //    AccountSubType = a.AccountSubType,
            //}).OrderBy(a => a.AccountSubType).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Account = det,
                    AccountSubTypes = AccountSubTypes,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertAccount(ClsAccountVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var AccountTypeDetail = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId &&
                a.Type == obj.Type).Select(a => new
                {
                    a.AccountTypeId,
                    a.AccountSubTypeId
                }).FirstOrDefault();

                obj.AccountTypeId = AccountTypeDetail.AccountTypeId;
                obj.AccountSubTypeId = AccountTypeDetail.AccountSubTypeId;

                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.AccountName == null || obj.AccountName == "")
                {
                    errors.Add(new ClsError { Message = "Account Name is required", Id = "divAccountName" });
                    isError = true;
                }

                //if (obj.AccountNumber == null || obj.AccountNumber == "")
                //{
                //    errors.Add(new ClsError { Message = "Account Number is required", Id = "divAccountNumber" });
                //    isError = true;
                //}

                //if (obj.AccountTypeId == 0)
                //{
                //    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                //    isError = true;
                //}

                if (obj.AccountName != "" && obj.AccountName != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountName.ToLower() == obj.AccountName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Name exists", Id = "divAccountName" });
                        isError = true;
                    }
                }

                if (obj.AccountNumber != "" && obj.AccountNumber != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountNumber.ToLower() == obj.AccountNumber.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate " + (obj.Type == "Bank" ? "Account Number exists" : "Card Number exists"), Id = "divAccountNumber" });
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

                //long PrefixUserMapId = 0;
                //var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                      join b in oConnectionContext.DbClsPrefixUserMap
                //                       on a.PrefixId equals b.PrefixId
                //                      where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                      b.CompanyId == obj.CompanyId && b.IsActive == true
                //                      && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Payment"
                //                      select new
                //                      {
                //                          b.PrefixUserMapId,
                //                          b.Prefix,
                //                          b.NoOfDigits,
                //                          b.Counter
                //                      }).FirstOrDefault();

                //PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //string ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                ClsAccount oClsAccount = new ClsAccount()
                {
                    AccountName = obj.AccountName,
                    AccountNumber = obj.AccountNumber,
                    AccountSubTypeId = obj.AccountSubTypeId,
                    AccountTypeId = obj.AccountTypeId,
                    Notes = obj.Notes,
                    //OpeningBalance = obj.OpeningBalance,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BankName = obj.BankName,
                    BranchCode = obj.BranchCode,
                    BranchName = obj.BranchName,
                    CurrencyId = obj.CurrencyId,
                    CanDelete = true,
                    ParentId = obj.ParentId,
                    DisplayAs = obj.DisplayAs,
                };
                oConnectionContext.DbClsAccount.Add(oClsAccount);
                oConnectionContext.SaveChanges();

                if (obj.AccountDetails != null)
                {
                    foreach (var item in obj.AccountDetails)
                    {
                        if (item.Label != null && item.Label != "" || item.Value != null && item.Value != "")
                        {
                            ClsAccountDetails oClsAccountDetails = new ClsAccountDetails()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                Label = item.Label,
                                Value = item.Value,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                AccountId = oClsAccount.AccountId
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsAccountDetails.Add(oClsAccountDetails);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Banks",
                    CompanyId = obj.CompanyId,
                    Description = "Bank/ Credit Card \"" + obj.AccountName + "\" created",
                    Id = oClsAccount.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Bank/ Credit Card created successfully",
                    Data = new
                    {
                        Account = oClsAccount
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateAccount(ClsAccountVm obj)
        {
            var AccountTypeDetail = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId &&
                a.Type == obj.Type).Select(a => new
                {
                    a.AccountTypeId,
                    a.AccountSubTypeId
                }).FirstOrDefault();

            obj.AccountTypeId = AccountTypeDetail.AccountTypeId;
            obj.AccountSubTypeId = AccountTypeDetail.AccountSubTypeId;

            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.AccountName == null || obj.AccountName == "")
                {
                    errors.Add(new ClsError { Message = "Account Name is required", Id = "divAccountName" });
                    isError = true;
                }

                //if (obj.AccountNumber == null || obj.AccountNumber == "")
                //{
                //    errors.Add(new ClsError { Message = "Account Number is required", Id = "divAccountNumber" });
                //    isError = true;
                //}

                //if (obj.AccountTypeId == 0)
                //{
                //    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                //    isError = true;
                //}

                if (obj.AccountName != "" && obj.AccountName != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountName.ToLower() == obj.AccountName.ToLower() &&
                 a.AccountId != obj.AccountId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Name exists", Id = "divAccountName" });
                        isError = true;
                    }
                }

                if (obj.AccountNumber != "" && obj.AccountNumber != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountNumber.ToLower() == obj.AccountNumber.ToLower() &&
                 a.AccountId != obj.AccountId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate " + (obj.Type == "Bank" ? "Account Number exists" : "Card Number exists"), Id = "divAccountNumber" });
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

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                ClsAccount oAccount = new ClsAccount()
                {
                    AccountId = obj.AccountId,
                    AccountName = obj.AccountName,
                    AccountNumber = obj.AccountNumber,
                    AccountSubTypeId = obj.AccountSubTypeId,
                    AccountTypeId = obj.AccountTypeId,
                    Notes = obj.Notes,
                    //OpeningBalance = obj.OpeningBalance,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    BankName = obj.BankName,
                    BranchCode = obj.BranchCode,
                    BranchName = obj.BranchName,
                    ParentId = obj.ParentId,
                    DisplayAs = obj.DisplayAs
                };
                oConnectionContext.DbClsAccount.Attach(oAccount);
                oConnectionContext.Entry(oAccount).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountName).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountNumber).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountSubTypeId).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountTypeId).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.Notes).IsModified = true;
                //oConnectionContext.Entry(oAccount).Property(x => x.OpeningBalance).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.BankName).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.BranchName).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.BranchCode).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.ParentId).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.DisplayAs).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.AccountDetails != null)
                {
                    foreach (var item in obj.AccountDetails)
                    {
                        if (item.AccountDetailsId == 0)
                        {
                            if (item.Label != null && item.Label != "" || item.Value != null && item.Value != "")
                            {
                                ClsAccountDetails oClsAccountDetails = new ClsAccountDetails()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    Label = item.Label,
                                    Value = item.Value,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AccountId = obj.AccountId
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsAccountDetails.Add(oClsAccountDetails);
                                oConnectionContext.SaveChanges();
                            }
                        }
                        else
                        {
                            if (item.Label != null && item.Label != "" || item.Value != null && item.Value != "")
                            {
                                ClsAccountDetails oClsAccountDetails = new ClsAccountDetails()
                                {
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    Label = item.Label,
                                    Value = item.Value,
                                    AccountDetailsId = item.AccountDetailsId
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsAccountDetails.Attach(oClsAccountDetails);
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.Label).IsModified = true;
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.Value).IsModified = true;
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Banks",
                    CompanyId = obj.CompanyId,
                    Description = "Bank/ Credit Card \"" + obj.AccountName + "\"updated",
                    Id = oAccount.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Bank/ Credit Card updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertChartOfAccount(ClsAccountVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.AccountName == null || obj.AccountName == "")
                {
                    errors.Add(new ClsError { Message = "Account Name is required", Id = "divAccountName" });
                    isError = true;
                }

                //if (obj.AccountNumber == null || obj.AccountNumber == "")
                //{
                //    errors.Add(new ClsError { Message = "Account Number is required", Id = "divAccountNumber" });
                //    isError = true;
                //}

                //if (obj.AccountTypeId == 0)
                //{
                //    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                //    isError = true;
                //}

                if (obj.AccountSubTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "Account Group is required", Id = "divAccountSubType" });
                    isError = true;
                }

                if (obj.AccountName != "" && obj.AccountName != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountName.ToLower() == obj.AccountName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Name exists", Id = "divAccountName" });
                        isError = true;
                    }
                }

                if (obj.AccountNumber != "" && obj.AccountNumber != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountNumber.ToLower() == obj.AccountNumber.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Number exists", Id = "divAccountNumber" });
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

                //long PrefixUserMapId = 0;
                //var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                      join b in oConnectionContext.DbClsPrefixUserMap
                //                       on a.PrefixId equals b.PrefixId
                //                      where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                      b.CompanyId == obj.CompanyId && b.IsActive == true
                //                      && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Payment"
                //                      select new
                //                      {
                //                          b.PrefixUserMapId,
                //                          b.Prefix,
                //                          b.NoOfDigits,
                //                          b.Counter
                //                      }).FirstOrDefault();

                //PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //string ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                obj.AccountTypeId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubTypeId == obj.AccountSubTypeId).Select(a => a.AccountTypeId).FirstOrDefault();

                ClsAccount oClsAccount = new ClsAccount()
                {
                    AccountName = obj.AccountName,
                    AccountNumber = obj.AccountNumber,
                    AccountSubTypeId = obj.AccountSubTypeId,
                    AccountTypeId = obj.AccountTypeId,
                    Notes = obj.Notes,
                    //OpeningBalance = obj.OpeningBalance,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BankName = obj.BankName,
                    BranchCode = obj.BranchCode,
                    BranchName = obj.BranchName,
                    CanDelete = true,
                    ParentId = obj.ParentId,
                    DisplayAs = obj.DisplayAs
                };
                oConnectionContext.DbClsAccount.Add(oClsAccount);
                oConnectionContext.SaveChanges();

                if (obj.AccountDetails != null)
                {
                    foreach (var item in obj.AccountDetails)
                    {
                        if (item.Label != null && item.Label != "" || item.Value != null && item.Value != "")
                        {
                            ClsAccountDetails oClsAccountDetails = new ClsAccountDetails()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                Label = item.Label,
                                Value = item.Value,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                AccountId = oClsAccount.AccountId
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsAccountDetails.Add(oClsAccountDetails);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Chart of Accounts",
                    CompanyId = obj.CompanyId,
                    Description = "Chart of Accounts \"" + obj.AccountName + "\" created",
                    Id = oClsAccount.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account created successfully",
                    Data = new
                    {
                        Account = oClsAccount
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateChartOfAccount(ClsAccountVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.AccountName == null || obj.AccountName == "")
                {
                    errors.Add(new ClsError { Message = "Account Name is required", Id = "divAccountName" });
                    isError = true;
                }

                //if (obj.AccountNumber == null || obj.AccountNumber == "")
                //{
                //    errors.Add(new ClsError { Message = "Account Number is required", Id = "divAccountNumber" });
                //    isError = true;
                //}

                //if (obj.AccountTypeId == 0)
                //{
                //    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                //    isError = true;
                //}

                if (obj.AccountSubTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "Account Group is required", Id = "divAccountSubType" });
                    isError = true;
                }

                if (obj.AccountName != "" && obj.AccountName != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountName.ToLower() == obj.AccountName.ToLower() &&
                 a.AccountId != obj.AccountId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Name exists", Id = "divAccountName" });
                        isError = true;
                    }
                }

                if (obj.AccountNumber != "" && obj.AccountNumber != null)
                {
                    if (oConnectionContext.DbClsAccount.Where(a => a.AccountNumber.ToLower() == obj.AccountNumber.ToLower() &&
                 a.AccountId != obj.AccountId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Number exists", Id = "divAccountNumber" });
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

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                obj.AccountTypeId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubTypeId == obj.AccountSubTypeId).Select(a => a.AccountTypeId).FirstOrDefault();

                ClsAccount oAccount = new ClsAccount()
                {
                    AccountId = obj.AccountId,
                    AccountName = obj.AccountName,
                    AccountNumber = obj.AccountNumber,
                    AccountSubTypeId = obj.AccountSubTypeId,
                    AccountTypeId = obj.AccountTypeId,
                    Notes = obj.Notes,
                    //OpeningBalance = obj.OpeningBalance,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    BankName = obj.BankName,
                    BranchCode = obj.BranchCode,
                    BranchName = obj.BranchName,
                    ParentId = obj.ParentId,
                    DisplayAs = obj.DisplayAs
                };
                oConnectionContext.DbClsAccount.Attach(oAccount);
                oConnectionContext.Entry(oAccount).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountName).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountNumber).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountSubTypeId).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.AccountTypeId).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.Notes).IsModified = true;
                //oConnectionContext.Entry(oAccount).Property(x => x.OpeningBalance).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.BankName).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.BranchName).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.BranchCode).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.ParentId).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.DisplayAs).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.AccountDetails != null)
                {
                    foreach (var item in obj.AccountDetails)
                    {
                        if (item.AccountDetailsId == 0)
                        {
                            if (item.Label != null && item.Label != "" || item.Value != null && item.Value != "")
                            {
                                ClsAccountDetails oClsAccountDetails = new ClsAccountDetails()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    Label = item.Label,
                                    Value = item.Value,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AccountId = obj.AccountId
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsAccountDetails.Add(oClsAccountDetails);
                                oConnectionContext.SaveChanges();
                            }
                        }
                        else
                        {
                            if (item.Label != null && item.Label != "" || item.Value != null && item.Value != "")
                            {
                                ClsAccountDetails oClsAccountDetails = new ClsAccountDetails()
                                {
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    Label = item.Label,
                                    Value = item.Value,
                                    AccountDetailsId = item.AccountDetailsId
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsAccountDetails.Attach(oClsAccountDetails);
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.Label).IsModified = true;
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.Value).IsModified = true;
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsAccountDetails).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Chart of Accounts",
                    CompanyId = obj.CompanyId,
                    Description = "Chart of Accounts \"" + obj.AccountName + "\" updated",
                    Id = oAccount.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountActiveInactive(ClsAccountVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var det = oConnectionContext.DbClsAccount.Where(a => a.AccountId == obj.AccountId).Select(a => new
                {
                    AccountSubType = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.AccountSubType).FirstOrDefault(),
                }).FirstOrDefault();

                string _menu = "", _title = "";
                if (det.AccountSubType == "Bank" || det.AccountSubType == "Credit Card")
                {
                    _menu = "Banks";
                    _title = "Bank/ Credit Card";
                }
                else
                {
                    _menu = "Chart of Accounts";
                    _title = "Chart of Accounts";
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccount oClsRole = new ClsAccount()
                {
                    AccountId = obj.AccountId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccount.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = _menu,
                    CompanyId = obj.CompanyId,
                    Description = _title + " \"" + oConnectionContext.DbClsAccount.Where(a => a.AccountId == obj.AccountId).Select(a => a.AccountName).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = _title + " " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountDelete(ClsAccountVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var det = oConnectionContext.DbClsAccount.Where(a => a.AccountId == obj.AccountId).Select(a => new
                {
                    AccountSubType = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.AccountSubType).FirstOrDefault(),
                }).FirstOrDefault();

                string _menu = "", _title = "";
                if (det.AccountSubType == "Bank" || det.AccountSubType == "Credit Card")
                {
                    _menu = "Banks";
                    _title = "Bank/ Credit Card";
                }
                else
                {
                    _menu = "Chart of Accounts";
                    _title = "Chart of Accounts";
                }

                ClsAccount oClsRole = new ClsAccount()
                {
                    AccountId = obj.AccountId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccount.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = _menu,
                    CompanyId = obj.CompanyId,
                    Description = _title + " \"" + oConnectionContext.DbClsAccount.Where(a => a.AccountId == obj.AccountId).Select(a => a.AccountName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = _title + " deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountClose(ClsAccountVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = (from a in oConnectionContext.DbClsItem
                                 join b in oConnectionContext.DbClsItemDetails
                                 on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                 && (b.InventoryAccountId == obj.AccountId || b.PurchaseAccountId == obj.AccountId || b.SalesAccountId == obj.AccountId)
                                 select a.ItemId).Count();

                int OpeningStockCount = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && (a.AccountId == obj.AccountId || a.JournalAccountId == obj.AccountId)).Count();

                int StockAdjustmentCount = (from a in oConnectionContext.DbClsStockAdjustment
                                            join b in oConnectionContext.DbClsStockAdjustmentDetails
                                            on a.StockAdjustmentId equals b.StockAdjustmentId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                            && (a.AccountId == obj.AccountId || b.AccountId == obj.AccountId)
                                            select a.StockAdjustmentId).Count();

                int PurchaseCount = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
                                     on a.PurchaseId equals b.PurchaseId
                                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                     && (a.AccountId == obj.AccountId || a.DiscountAccountId == obj.AccountId
                                     || a.RoundOffAccountId == obj.AccountId
                                     || a.TaxAccountId == obj.AccountId || b.AccountId == obj.AccountId || b.DiscountAccountId == obj.AccountId
                                     || b.TaxAccountId == obj.AccountId)
                                     select a.PurchaseId).Count();

                int PurchaseAdditionalCount = (from a in oConnectionContext.DbClsPurchase
                                               join b in oConnectionContext.DbClsPurchaseAdditionalCharges
                                               on a.PurchaseId equals b.PurchaseId
                                               where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                               && b.AccountId == obj.AccountId
                                               select a.PurchaseId).Count();

                int PurchaseReturnCount = (from a in oConnectionContext.DbClsPurchaseReturn
                                           join b in oConnectionContext.DbClsPurchaseReturnDetails
                                           on a.PurchaseReturnId equals b.PurchaseReturnId
                                           join c in oConnectionContext.DbClsPurchase
                                           on a.PurchaseId equals c.PurchaseId
                                           where a.CompanyId == obj.CompanyId 
                                           && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                           && c.IsDeleted == false && c.IsCancelled == false
                                           && (a.AccountId == obj.AccountId || a.DiscountAccountId == obj.AccountId
                                           || a.RoundOffAccountId == obj.AccountId
                                           || a.TaxAccountId == obj.AccountId || b.AccountId == obj.AccountId || b.DiscountAccountId == obj.AccountId
                                           || b.TaxAccountId == obj.AccountId && c.TaxAccountId == obj.AccountId)
                                           select a.PurchaseId).Count();

                int PurchaseReturnAdditionalCount = (from a in oConnectionContext.DbClsPurchaseReturn
                                                     join b in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                                     on a.PurchaseReturnId equals b.PurchaseReturnId
                                                     join c in oConnectionContext.DbClsPurchase
                                           on a.PurchaseId equals c.PurchaseId
                                                     where a.CompanyId == obj.CompanyId 
                                                     && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                                     && c.IsDeleted == false && c.IsCancelled == false
                                                     && b.AccountId == obj.AccountId
                                                     select a.PurchaseId).Count();

                int SupplierPaymentCount = oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && (a.AccountId == obj.AccountId || a.JournalAccountId == obj.AccountId)).Count();

                //int SupplierRefundCount = oConnectionContext.DbClsSupplierRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //&& (a.AccountId == obj.AccountId || a.JournalAccountId == obj.AccountId)).Count();

                int ExpenseCount = (from a in oConnectionContext.DbClsExpense
                                    join b in oConnectionContext.DbClsExpensePayment
                                    on a.ExpenseId equals b.ExpenseId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                    && (a.AccountId == obj.AccountId || b.AccountId == obj.AccountId || b.TaxAccountId == obj.AccountId)
                                    select a.ExpenseId).Count();

                int SalesCount = (from a in oConnectionContext.DbClsSales
                                  join b in oConnectionContext.DbClsSalesDetails
                                  on a.SalesId equals b.SalesId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                  && (a.AccountId == obj.AccountId || a.DiscountAccountId == obj.AccountId
                                  || a.RoundOffAccountId == obj.AccountId
                                  || a.TaxAccountId == obj.AccountId || b.AccountId == obj.AccountId || b.DiscountAccountId == obj.AccountId
                                  || b.InventoryAccountId == obj.AccountId || b.PurchaseAccountId == obj.AccountId || b.TaxAccountId == obj.AccountId)
                                  select a.SalesId).Count();

                int SalesAdditionalCount = (from a in oConnectionContext.DbClsSales
                                            join b in oConnectionContext.DbClsSalesAdditionalCharges
                                            on a.SalesId equals b.SalesId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                            && b.AccountId == obj.AccountId
                                            select a.SalesId).Count();

                int SalesReturnCount = (from a in oConnectionContext.DbClsSalesReturn
                                        join b in oConnectionContext.DbClsSalesReturnDetails
                                        on a.SalesReturnId equals b.SalesReturnId
                                        join c in oConnectionContext.DbClsSales
                                        on a.SalesId equals c.SalesId
                                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                                        && a.IsCancelled == false && b.IsDeleted == false && c.IsCancelled == false
                                        && b.IsDeleted == false && c.IsDeleted == false
                                        && (a.AccountId == obj.AccountId || a.DiscountAccountId == obj.AccountId
                                           || a.RoundOffAccountId == obj.AccountId
                                           || a.TaxAccountId == obj.AccountId || b.AccountId == obj.AccountId || b.DiscountAccountId == obj.AccountId
                                           || c.RoundOffAccountId == obj.AccountId
                                           || b.TaxAccountId == obj.AccountId || c.TaxAccountId == obj.AccountId)
                                        select a.SalesId).Count();

                int SalesReturnAdditionalCount = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                                  join a in oConnectionContext.DbClsSalesReturn
                                                  on c.SalesReturnId equals a.SalesReturnId
                                                  join b in oConnectionContext.DbClsSales
                                                  on a.SalesId equals b.SalesId
                                                  where a.CompanyId == obj.CompanyId
                                                  && c.IsDeleted == false && c.IsActive == true
                                                  && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false
                                                  && a.AccountId == obj.AccountId
                                                  select a.SalesId).Count();

                int CustomerPaymentCount = oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && (a.AccountId == obj.AccountId || a.JournalAccountId == obj.AccountId)).Count();

                //int CustomerRefundCount = oConnectionContext.DbClsCustomerRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //&& (a.AccountId == obj.AccountId || a.JournalAccountId == obj.AccountId)).Count();

                int ContraCount = oConnectionContext.DbClsContra.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && (a.FromAccountId == obj.AccountId || a.ToAccountId == obj.AccountId)).Count();

                int JournalCount = (from a in oConnectionContext.DbClsJournal
                                    join b in oConnectionContext.DbClsJournalPayment
                                    on a.JournalId equals b.JournalId
                                    where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                    && b.AccountId == obj.AccountId
                                    select a.JournalId).Count();

                int TaxCount = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && (a.CustomerPaymentAccountId == obj.AccountId || a.ExpenseAccountId == obj.AccountId
                || a.IncomeAccountId == obj.AccountId || a.PurchaseAccountId == obj.AccountId
                || a.SalesAccountId == obj.AccountId || a.SupplierPaymentAccountId == obj.AccountId)).Count();

                int SaleSettingsCount = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && (a.DiscountAccountId == obj.AccountId || a.RoundOffAccountId == obj.AccountId
                )).Count();

                int PurchaseSettingsCount = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && (a.DiscountAccountId == obj.AccountId || a.RoundOffAccountId == obj.AccountId
                )).Count();

                int AccountOpeningBalanceCount = oConnectionContext.DbClsAccountOpeningBalance.Where(a => a.CompanyId == obj.CompanyId
                && (a.AccountId == obj.AccountId || a.AccountOpeningBalanceId == obj.AccountId)).Count();

                if (ItemCount > 0 || ItemCount > 0 || OpeningStockCount > 0 || StockAdjustmentCount > 0 || PurchaseCount > 0
                    || PurchaseAdditionalCount > 0 || PurchaseReturnCount > 0 || 
                    PurchaseReturnAdditionalCount >0 || SupplierPaymentCount > 0
                    //|| SupplierRefundCount > 0
                    || ExpenseCount > 0
                    || SalesCount > 0 || SalesAdditionalCount > 0 || SalesReturnCount > 0 || 
                    SalesReturnAdditionalCount > 0 || CustomerPaymentCount > 0
                    //|| CustomerRefundCount > 0
                    || ContraCount > 0 || JournalCount > 0 || TaxCount > 0 || SaleSettingsCount > 0 || PurchaseSettingsCount > 0
                    || AccountOpeningBalanceCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccount oClsRole = new ClsAccount()
                {
                    AccountId = obj.AccountId,
                    Status = 2,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccount.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Account",
                    CompanyId = obj.CompanyId,
                    Description = "closed  " + oConnectionContext.DbClsAccount.Where(a => a.AccountId == obj.AccountId).Select(a => a.AccountNumber).FirstOrDefault(),
                    Id = oClsRole.AccountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account closed successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAccounts(ClsAccount obj)
        {
            var det = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                Type = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == a.AccountSubTypeId).Select(b => b.Type).FirstOrDefault(),
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                a.AccountNumber,
                a.DisplayAs
            }).OrderBy(a => a.AccountName).ToList();

            if (obj.Type == "Bank")
            {
                det = det.Where(a => a.Type == "Bank").ToList();
            }
            else if (obj.Type == "Credit Card")
            {
                det = det.Where(a => a.Type == "Credit Card").ToList();
            }
            else if (obj.Type == "Bank & Credit Card")
            {
                det = det.Where(a => (a.Type == "Bank" || a.Type == "Credit Card")).ToList();
            }
            else if (obj.Type == "Chart Of Account")
            {
                det = det.Where(a => (a.Type != "Bank" && a.Type != "Credit Card")).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Accounts = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAccountsDropdown(ClsAccount obj)
        {
            var det = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                a.Type,
                a.DisplayAs,
                AccountType = oConnectionContext.DbClsAccountType.Where(b => b.AccountTypeId == a.AccountTypeId).Select(b => b.AccountType).FirstOrDefault(),
                Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountSubTypeId == a.AccountSubTypeId && aa.CompanyId == obj.CompanyId &&
                aa.IsDeleted == false && aa.IsActive == true).Select(aa => new
                {
                    aa.Type,
                    aa.AccountId,
                    aa.AccountName,
                    aa.AccountNumber,
                    aa.DisplayAs
                }).OrderBy(aa => aa.AccountName)
            }).OrderBy(a => a.AccountType).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountSubTypes = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAccountDetails(ClsAccount obj)
        {
            var det = oConnectionContext.DbClsAccountDetails.Where(a => a.AccountId == obj.AccountId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                AccountDetailsId = a.AccountDetailsId,
                a.Label,
                a.Value
            }).OrderBy(a => a.Label).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountDetails = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveOtherAccounts(ClsAccount obj)
        {
            var det = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId &&
            a.IsDeleted == false && a.IsActive == true && a.AccountId != obj.AccountId).Select(a => new
            {
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                a.AccountNumber
            }).OrderBy(a => a.AccountName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Accounts = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        //        public async Task<IHttpActionResult> PaymentAccountReport(ClsAccountsPaymentVm obj)
        //        {
        //            if (obj.FromDate == DateTime.MinValue)
        //            {
        //                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

        //                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
        //                if (obj.FromDate > DateTime.Now)
        //                {
        //                    obj.FromDate = obj.FromDate.AddYears(-1);
        //                }

        //                obj.ToDate = obj.FromDate.AddMonths(11);

        //                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

        //                obj.ToDate = obj.ToDate.AddDays(days - 1);
        //            }

        //            if (obj.PageSize == 0)
        //            {
        //                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
        //            }

        //            int skip = obj.PageSize * (obj.PageIndex - 1);

        //            List<ClsAccountsPaymentVm> Payments;
        //            if (obj.BranchId == 0)
        //            {
        //                Payments = (from a in oConnectionContext.DbClsSupplierPayment
        //                            where a.CompanyId == obj.CompanyId
        //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                            select new ClsAccountsPaymentVm
        //                            {
        //                                AddedOn = a.AddedOn,
        //                                Amount = a.Amount,
        //                                PaymentId = a.SupplierPaymentId,
        //                                PaymentDate = a.PaymentDate,
        //                                ReferenceNo = a.ReferenceNo,
        //                                InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "customer refund") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                                Type = a.Type,
        //                                CompanyId = a.CompanyId,
        //                                AccountId = a.AccountId,
        //                                AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                            }).Union(from a in oConnectionContext.DbClsCustomerPayment
        //                                     where a.CompanyId == obj.CompanyId
        //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                                     select new ClsAccountsPaymentVm
        //                                     {
        //                                         AddedOn = a.AddedOn,
        //                                         Amount = a.Amount,
        //                                         PaymentId = a.CustomerPaymentId,
        //                                         PaymentDate = a.PaymentDate,
        //                                         ReferenceNo = a.ReferenceNo,
        //                                         InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "customer refund") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                                         Type = a.Type,
        //                                         CompanyId = a.CompanyId,
        //                                         AccountId = a.AccountId,
        //                                         AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                         AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                                     }).Union(from a in oConnectionContext.DbClsAccountsPayment
        //                                              where a.CompanyId == obj.CompanyId
        //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && (a.Type.ToLower() == "income payment" || a.Type.ToLower() == "expense payment")
        //                                              select new ClsAccountsPaymentVm
        //                                              {
        //                                                  AddedOn = a.AddedOn,
        //                                                  Amount = a.Amount,
        //                                                  PaymentId = a.AccountsPaymentId,
        //                                                  PaymentDate = a.PaymentDate,
        //                                                  ReferenceNo = a.ReferenceNo,
        //                                                  InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "customer refund") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                                                  Type = a.Type,
        //                                                  CompanyId = a.CompanyId,
        //                                                  AccountId = a.AccountId,
        //                                                  AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                                  AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                                              }).ToList();
        //            }
        //            else
        //            {
        //                Payments = (from a in oConnectionContext.DbClsSupplierPayment
        //                            where a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                            select new ClsAccountsPaymentVm
        //                            {
        //                                AddedOn = a.AddedOn,
        //                                Amount = a.Amount,
        //                                PaymentId = a.SupplierPaymentId,
        //                                PaymentDate = a.PaymentDate,
        //                                ReferenceNo = a.ReferenceNo,
        //                                InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "customer refund") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                                Type = a.Type,
        //                                CompanyId = a.CompanyId,
        //                                AccountId = a.AccountId,
        //                                AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                            }).Union(from a in oConnectionContext.DbClsCustomerPayment
        //                                     where a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                                     select new ClsAccountsPaymentVm
        //                                     {
        //                                         AddedOn = a.AddedOn,
        //                                         Amount = a.Amount,
        //                                         PaymentId = a.CustomerPaymentId,
        //                                         PaymentDate = a.PaymentDate,
        //                                         ReferenceNo = a.ReferenceNo,
        //                                         InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "customer refund") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                                         Type = a.Type,
        //                                         CompanyId = a.CompanyId,
        //                                         AccountId = a.AccountId,
        //                                         AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                         AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                                     }).Union(from a in oConnectionContext.DbClsAccountsPayment
        //                                              where a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && (a.Type.ToLower() == "income payment" || a.Type.ToLower() == "expense payment")
        //                                              select new ClsAccountsPaymentVm
        //                                              {
        //                                                  AddedOn = a.AddedOn,
        //                                                  Amount = a.Amount,
        //                                                  PaymentId = a.AccountsPaymentId,
        //                                                  PaymentDate = a.PaymentDate,
        //                                                  ReferenceNo = a.ReferenceNo,
        //                                                  InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "customer refund") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                                                  Type = a.Type,
        //                                                  CompanyId = a.CompanyId,
        //                                                  AccountId = a.AccountId,
        //                                                  AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                                  AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                                              }).ToList();
        //            }

        //            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
        //            {
        //                Payments = Payments.Where(a => a.PaymentDate.Date >= obj.FromDate && a.PaymentDate.Date <= obj.ToDate).ToList();
        //            }

        //            if (obj.AccountId != 0)
        //            {
        //                Payments = Payments.Where(a => a.AccountId == obj.AccountId).Select(a => a).ToList();
        //            }

        //            data = new
        //            {
        //                Status = 1,
        //                Message = "found",
        //                Data = new
        //                {
        //                    AccountsPayments = Payments.OrderByDescending(a => a.PaymentDate).Skip(skip).Take(obj.PageSize).ToList(),
        //                    TotalCount = Payments.Count(),
        //                    //Branchs = userDetails.BranchIds,
        //                    FromDate = obj.FromDate,
        //                    ToDate = obj.ToDate,
        //                    PageSize = obj.PageSize
        //                }
        //            };
        //            return await Task.FromResult(Ok(data));

        //        }

        //public async Task<IHttpActionResult> LinkPaymentWithAccount(ClsAccountsPaymentVm obj)
        //{
        //    using (TransactionScope dbContextTransaction = new TransactionScope())
        //    {
        //        bool isError = false;
        //        List<ClsError> errors = new List<ClsError>();

        //        var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

        //        if (obj.AccountId == 0)
        //        {
        //            errors.Add(new ClsError { Message = "This field is required", Id = "divAccount" });
        //            isError = true;
        //        }

        //        if (isError == true)
        //        {
        //            data = new
        //            {
        //                Status = 2,
        //                Message = "",
        //                Errors = errors,
        //                Data = new
        //                {
        //                }
        //            };
        //            return await Task.FromResult(Ok(data));
        //        }

        //        if (obj.Type.ToLower() == "sales payment" ||
        //            obj.Type.ToLower() == "customer refund")
        //        {
        //            ClsCustomerPayment oClsPayment = new ClsCustomerPayment()
        //            {
        //                CustomerPaymentId = obj.PaymentId,
        //                AccountId = obj.AccountId,
        //                ModifiedBy = obj.AddedBy,
        //                ModifiedOn = CurrentDate,
        //            };
        //            oConnectionContext.DbClsCustomerPayment.Attach(oClsPayment);
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.AccountId).IsModified = true;
        //            oConnectionContext.SaveChanges();
        //        }
        //        else if (obj.Type.ToLower() == "purchase payment" || obj.Type.ToLower() == "supplier refund")
        //        {
        //            ClsSupplierPayment oClsPayment = new ClsSupplierPayment()
        //            {
        //                SupplierPaymentId = obj.PaymentId,
        //                AccountId = obj.AccountId,
        //                ModifiedBy = obj.AddedBy,
        //                ModifiedOn = CurrentDate,
        //            };
        //            oConnectionContext.DbClsSupplierPayment.Attach(oClsPayment);
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.AccountId).IsModified = true;
        //            oConnectionContext.SaveChanges();
        //        }
        //        else if (obj.Type.ToLower() == "expense payment" || obj.Type.ToLower() == "income payment")
        //        {
        //            ClsAccountsPayment oClsPayment = new ClsAccountsPayment()
        //            {
        //                AccountsPaymentId = obj.PaymentId,
        //                AccountId = obj.AccountId,
        //                ModifiedBy = obj.AddedBy,
        //                ModifiedOn = CurrentDate,
        //            };
        //            oConnectionContext.DbClsAccountsPayment.Attach(oClsPayment);
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedBy).IsModified = true;
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.ModifiedOn).IsModified = true;
        //            oConnectionContext.Entry(oClsPayment).Property(x => x.AccountId).IsModified = true;
        //            oConnectionContext.SaveChanges();
        //        }


        //        ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
        //        {
        //            AddedBy = obj.AddedBy,
        //            Browser = obj.Browser,
        //            Category = obj.Type,
        //            CompanyId = obj.CompanyId,
        //            Description = "Payment linked with account",
        //            Id = obj.PaymentId,
        //            IpAddress = obj.IpAddress,
        //            Platform = obj.Platform,
        //            Type = "Update"
        //        };
        //        oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

        //        data = new
        //        {
        //            Status = 1,
        //            Message = "Payment linked successfully",
        //            Data = new
        //            {
        //            }
        //        };

        //        dbContextTransaction.Complete();
        //    }
        //    return await Task.FromResult(Ok(data));
        //}

        //        public async Task<IHttpActionResult> CashFlowReport(ClsAccountsPaymentVm obj)
        //        {
        //            if (obj.FromDate == DateTime.MinValue)
        //            {
        //                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

        //                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
        //                if (obj.FromDate > DateTime.Now)
        //                {
        //                    obj.FromDate = obj.FromDate.AddYears(-1);
        //                }

        //                obj.ToDate = obj.FromDate.AddMonths(11);

        //                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

        //                obj.ToDate = obj.ToDate.AddDays(days - 1);
        //            }

        //            List<ClsAccountsPaymentVm> Payments;
        //            if (obj.BranchId == 0)
        //            {
        //                Payments = (from a in oConnectionContext.DbClsSupplierPayment
        //                            where a.CompanyId == obj.CompanyId
        //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                            select new ClsAccountsPaymentVm
        //                            {
        //                                BranchId = a.BranchId,
        //                                Id = a.Id,
        //                                AddedBy = a.AddedBy,
        //                                AccountId = a.AccountId,
        //                                Balance = 0,
        //                                PaymentDate = a.PaymentDate,
        //                                ReferenceNo = a.ReferenceNo,
        //                                Type = a.Type,
        //                                Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                PaymentId = a.SupplierPaymentId,
        //                                IsDebit = a.IsDebit
        //                            }).Union(from a in oConnectionContext.DbClsCustomerPayment
        //                                     where a.CompanyId == obj.CompanyId
        //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                                     select new ClsAccountsPaymentVm
        //                                     {
        //                                         BranchId = a.BranchId,
        //                                         Id = a.Id,
        //                                         AddedBy = a.AddedBy,
        //                                         AccountId = a.AccountId,
        //                                         Balance = 0,
        //                                         PaymentDate = a.PaymentDate,
        //                                         ReferenceNo = a.ReferenceNo,
        //                                         Type = a.Type,
        //                                         Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                         Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                         PaymentId = a.CustomerPaymentId,
        //                                         IsDebit = a.IsDebit
        //                                     }).Union(from a in oConnectionContext.DbClsAccountsPayment
        //                                              where a.CompanyId == obj.CompanyId
        //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0
        //                                              select new ClsAccountsPaymentVm
        //                                              {
        //                                                  BranchId = a.BranchId,
        //                                                  Id = a.Id,
        //                                                  AddedBy = a.AddedBy,
        //                                                  AccountId = a.AccountId,
        //                                                  Balance = 0,
        //                                                  PaymentDate = a.PaymentDate,
        //                                                  ReferenceNo = a.ReferenceNo,
        //                                                  Type = a.Type,
        //                                                  Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                                  Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                                  PaymentId = a.AccountsPaymentId,
        //                                                  IsDebit = a.IsDebit
        //                                              }).ToList();
        //            }
        //            else
        //            {
        //                Payments = (from a in oConnectionContext.DbClsSupplierPayment
        //                            where a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                            select new ClsAccountsPaymentVm
        //                            {
        //                                BranchId = a.BranchId,
        //                                Id = a.Id,
        //                                AddedBy = a.AddedBy,
        //                                AccountId = a.AccountId,
        //                                Balance = 0,
        //                                PaymentDate = a.PaymentDate,
        //                                ReferenceNo = a.ReferenceNo,
        //                                Type = a.Type,
        //                                Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                PaymentId = a.SupplierPaymentId,
        //                                IsDebit = a.IsDebit
        //                            }).Union(from a in oConnectionContext.DbClsCustomerPayment
        //                                     where a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0 && a.ParentId == 0
        //                                     select new ClsAccountsPaymentVm
        //                                     {
        //                                         BranchId = a.BranchId,
        //                                         Id = a.Id,
        //                                         AddedBy = a.AddedBy,
        //                                         AccountId = a.AccountId,
        //                                         Balance = 0,
        //                                         PaymentDate = a.PaymentDate,
        //                                         ReferenceNo = a.ReferenceNo,
        //                                         Type = a.Type,
        //                                         Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                         Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                         PaymentId = a.CustomerPaymentId,
        //                                         IsDebit = a.IsDebit
        //                                     }).Union(from a in oConnectionContext.DbClsAccountsPayment
        //                                              where a.CompanyId == obj.CompanyId && a.BranchId == obj.BranchId
        //&& a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0
        //                                              select new ClsAccountsPaymentVm
        //                                              {
        //                                                  BranchId = a.BranchId,
        //                                                  Id = a.Id,
        //                                                  AddedBy = a.AddedBy,
        //                                                  AccountId = a.AccountId,
        //                                                  Balance = 0,
        //                                                  PaymentDate = a.PaymentDate,
        //                                                  ReferenceNo = a.ReferenceNo,
        //                                                  Type = a.Type,
        //                                                  Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                                  Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                                  PaymentId = a.AccountsPaymentId,
        //                                                  IsDebit = a.IsDebit
        //                                              }).ToList();
        //            }

        //            /// Advance payment
        //            Payments = Payments.Union(from a in oConnectionContext.DbClsUserPayment
        //                                      where a.CompanyId == obj.CompanyId
        //                                      && a.BranchId == 0
        //                                     && a.IsDeleted == false && a.IsCancelled == false && a.Amount > 0
        //                                      //&& (a.Type.ToLower() == "customer payment" || a.Type.ToLower() == "supplier payment")
        //                                      select new ClsAccountsPaymentVm
        //                                      {
        //                                          BranchId = a.BranchId,
        //                                          Id = a.Id,
        //                                          AddedBy = a.AddedBy,
        //                                          AccountId = a.AccountId,
        //                                          Balance = 0,
        //                                          PaymentDate = a.PaymentDate,
        //                                          ReferenceNo = a.ReferenceNo,
        //                                          Type = a.Type,
        //                                          Credit = a.IsDebit == 2 ? a.Amount : 0,
        //                                          Debit = a.IsDebit == 1 ? a.Amount : 0,
        //                                          PaymentId = a.UserPaymentId,
        //                                          IsDebit = a.IsDebit
        //                                      }).ToList();

        //            /// Advance payment

        //            // receiver
        //            if (obj.Type == "account book")
        //            {
        //                Payments = Payments.Union(from a in oConnectionContext.DbClsAccountsPayment
        //                                          where a.CompanyId == obj.CompanyId
        //                                         && a.IsDeleted == false && a.IsCancelled == false
        //                                         && (a.Type.ToLower() == "fund transfer" || a.Type.ToLower() == "deposit"
        //                                         || a.Type.ToLower() == "withdraw" || a.Type.ToLower() == "account opening balance")
        //                                         && a.Amount > 0
        //                                          select new ClsAccountsPaymentVm
        //                                          {
        //                                              BranchId = a.BranchId,
        //                                              Id = a.Id,
        //                                              AddedBy = a.AddedBy,
        //                                              AccountId = a.AccountId,
        //                                              Balance = 0,
        //                                              PaymentDate = a.PaymentDate,
        //                                              ReferenceNo = a.ReferenceNo,
        //                                              Type = a.Type,
        //                                              Credit = a.Amount,
        //                                              Debit = 0,
        //                                              PaymentId = a.AccountsPaymentId,
        //                                              IsDebit = a.IsDebit,
        //                                              From_ToAccount = "From: " + oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.Id).Select(b => b.AccountName).FirstOrDefault(),
        //                                          }).ToList();
        //                // receiver

        //                //sender
        //                Payments = Payments.Union(from a in oConnectionContext.DbClsAccountsPayment
        //                                          where a.CompanyId == obj.CompanyId
        //                                         && a.IsDeleted == false && a.IsCancelled == false
        //                                         && (a.Type.ToLower() == "fund transfer" || a.Type.ToLower() == "deposit" || a.Type.ToLower() == "withdraw")
        //                                         && a.Amount > 0
        //                                          select new ClsAccountsPaymentVm
        //                                          {
        //                                              BranchId = a.BranchId,
        //                                              Id = a.Id,
        //                                              AddedBy = a.AddedBy,
        //                                              AccountId = a.Id,
        //                                              Balance = 0,
        //                                              PaymentDate = a.PaymentDate,
        //                                              ReferenceNo = a.ReferenceNo,
        //                                              Type = a.Type,
        //                                              Credit = 0,
        //                                              Debit = a.Amount,
        //                                              PaymentId = a.AccountsPaymentId,
        //                                              IsDebit = a.IsDebit,
        //                                              From_ToAccount = "To: " + oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                                          }).ToList();
        //                //sender
        //            }

        //            if (obj.AccountId != 0)
        //            {
        //                Payments = Payments.Where(a => a.AccountId == obj.AccountId).Select(a => a).ToList();
        //            }

        //            if (obj.IsDebit != 0)
        //            {
        //                Payments = Payments.Where(a => a.IsDebit == obj.IsDebit).ToList();
        //            }

        //            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
        //            {
        //                Payments = Payments.Where(a => a.PaymentDate.Date >= obj.FromDate && a.PaymentDate.Date <= obj.ToDate).ToList();
        //            }

        //            if (obj.PageSize == 0)
        //            {
        //                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
        //            }

        //            int skip = obj.PageSize * (obj.PageIndex - 1);

        //            decimal balance = 0;

        //            foreach (var item in Payments)
        //            {
        //                balance = balance + (item.Credit - item.Debit);
        //                item.Balance = balance;
        //            }

        //            var det = Payments.Select(a => new
        //            {
        //                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
        //                PaymentDate = a.PaymentDate,
        //                ReferenceNo = a.ReferenceNo,
        //                Type = a.Type,
        //                Credit = a.Credit,
        //                Debit = a.Debit,
        //                Balance = a.Balance,
        //                PaymentId = a.PaymentId,
        //                InvoiceNo = a.Type.ToLower() == "purchase payment" ? oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "supplier refund" ? oConnectionContext.DbClsPurchaseReturn.Where(b => b.PurchaseReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales payment" || a.Type.ToLower() == "change return") ? oConnectionContext.DbClsSales.Where(b => b.SalesId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : (a.Type.ToLower() == "sales return") ? oConnectionContext.DbClsSalesReturn.Where(b => b.SalesReturnId == a.Id).Select(b => b.InvoiceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "income payment" ? oConnectionContext.DbClsIncome.Where(b => b.IncomeId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault()
        //                           : a.Type.ToLower() == "expense payment" ? oConnectionContext.DbClsExpense.Where(b => b.ExpenseId == a.Id).Select(b => b.ReferenceNo).FirstOrDefault() : "",
        //                AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
        //                AccountNumber = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountNumber).FirstOrDefault(),
        //                From_ToAccount = a.From_ToAccount
        //            }).OrderByDescending(a => a.PaymentId).Skip(skip).Take(obj.PageSize).ToList();

        //            data = new
        //            {
        //                Status = 1,
        //                Message = "found",
        //                Data = new
        //                {
        //                    AccountsPayments = det,
        //                    TotalCount = Payments.Count(),
        //                    FromDate = obj.FromDate,
        //                    ToDate = obj.ToDate,
        //                    PageSize = obj.PageSize
        //                }
        //            };

        //            return await Task.FromResult(Ok(data));
        //        }

        //public async Task<IHttpActionResult> UpdateAccountSettings(ClsAccountSettingsVm obj)
        //{
        //    var AccountSettingsId = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.AccountSettingsId).FirstOrDefault();

        //    using (TransactionScope dbContextTransaction = new TransactionScope())
        //    {
        //        var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

        //        ClsAccountSettings oAccount = new ClsAccountSettings()
        //        {
        //            AccountSettingsId = AccountSettingsId,
        //            VendorAdvanceAccountId = obj.VendorAdvanceAccountId,
        //            CustomerAdvanceAccountId = obj.CustomerAdvanceAccountId,
        //            ChangeReturnAccountId = obj.ChangeReturnAccountId,
        //            ModifiedBy = obj.AddedBy,
        //            ModifiedOn = CurrentDate,
        //        };
        //        oConnectionContext.DbClsAccountSettings.Attach(oAccount);
        //        oConnectionContext.Entry(oAccount).Property(x => x.ModifiedBy).IsModified = true;
        //        oConnectionContext.Entry(oAccount).Property(x => x.ModifiedOn).IsModified = true;
        //        oConnectionContext.Entry(oAccount).Property(x => x.VendorAdvanceAccountId).IsModified = true;
        //        oConnectionContext.Entry(oAccount).Property(x => x.CustomerAdvanceAccountId).IsModified = true;
        //        oConnectionContext.Entry(oAccount).Property(x => x.ChangeReturnAccountId).IsModified = true;
        //        oConnectionContext.SaveChanges();

        //        ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
        //        {
        //            AddedBy = obj.AddedBy,
        //            Browser = obj.Browser,
        //            Category = "Business Settings - Account Settings Update",
        //            CompanyId = obj.CompanyId,
        //            Description = "updated account settings",
        //            Id = 0,
        //            IpAddress = obj.IpAddress,
        //            Platform = obj.Platform,
        //            Type = "Business Settings"
        //        };
        //        oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

        //        data = new
        //        {
        //            Status = 1,
        //            Message = "Account settings updated successfully",
        //            Data = new
        //            {
        //            }
        //        };
        //        dbContextTransaction.Complete();
        //    }
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> BankTransactions(ClsBankPaymentVm obj)
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

            //List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            //List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            //List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            //decimal TotalPreviousDebit = 0, TotalPreviousCredit = 0;

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj).ToList();

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            OpeningBalance = OpeningBalance.Where(a => a.AccountId == obj.AccountId).ToList();
            Transactions = Transactions.Where(a => a.AccountId == obj.AccountId).ToList();

            Ledger = OpeningBalance.Concat(Transactions).ToList();

            int count = Ledger.Count();
            Ledger = Ledger.OrderBy(a => a.AddedOn).ThenBy(a => a.Id).Skip(skip).Take(obj.PageSize).ToList();
            Ledger = Ledger.Select(a => new ClsBankPaymentVm
            {
                Id = a.Id,
                AccountId = a.AccountId,
                AddedOn = a.AddedOn,
                Notes = a.Notes,
                Type = a.Type,
                ReferenceNo = a.ReferenceNo,
                Debit = a.Credit,
                Credit = a.Debit,
                AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault()
            }).ToList();

            decimal _balance = 0;
            foreach (var a in Ledger)
            {
                if (a.Debit > 0)
                {
                    _balance = _balance - a.Debit;
                    a.Balance = _balance;
                }
                else
                {
                    _balance = _balance + a.Credit;
                    a.Balance = _balance;
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    //BankPayments = Ledger.OrderBy(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalCount = Ledger.Count(),
                    BankPayments = Ledger.OrderByDescending(a => a.AddedOn).ThenBy(a => a.Id).ToList(),
                    TotalCount = count,
                    PageSize = obj.PageSize
                    //Branchs = userDetails.BranchIds
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountTransactions(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            decimal TotalPreviousDebit = 0, TotalPreviousCredit = 0;

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj).ToList();

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            if (obj.AccountId != 0)
            {
                Transactions = Transactions.Where(a => a.AccountId == obj.AccountId).ToList();

                obj.ToDate = obj.FromDate;
                obj.FromDate = Convert.ToDateTime("01-01-2020");

                PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
                PreviousTransactions = oCommonController.AccountTransactions(obj);
                PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();
                PreviousLedger = PreviousLedger.Where(a => a.AccountId == obj.AccountId).ToList();
                TotalPreviousDebit = PreviousLedger.Select(a => a.Debit).DefaultIfEmpty().Sum();
                TotalPreviousCredit = PreviousLedger.Select(a => a.Credit).DefaultIfEmpty().Sum();
                //TotalOpeningBalance =TotalPreviousDebit  - TotalPreviousCredit;

                Ledger.Add(new ClsBankPaymentVm
                {
                    AccountId = 0,
                    AccountSubTypeId = 0,
                    Notes = "",
                    AddedOn = obj.ToDate,
                    Type = "Opening Balance",
                    ReferenceNo = "",
                    Debit = (TotalPreviousDebit > TotalPreviousCredit) ? (TotalPreviousDebit - TotalPreviousCredit) : 0,
                    Credit = (TotalPreviousCredit > TotalPreviousDebit) ? (TotalPreviousCredit - TotalPreviousDebit) : 0,
                });

                Ledger = Ledger.Concat(Transactions).ToList();
            }
            else
            {
                Ledger = Transactions;
            }

            int count = Ledger.Count();
            Ledger = Ledger.OrderBy(a => a.AddedOn).ThenBy(a => a.Id).Skip(skip).Take(obj.PageSize).ToList();
            Ledger = Ledger.Select(a => new ClsBankPaymentVm
            {
                Id = a.Id,
                AccountId = a.AccountId,
                AddedOn = a.AddedOn,
                Notes = a.Notes,
                Type = a.Type,
                ReferenceNo = a.ReferenceNo,
                Debit = a.Debit,
                Credit = a.Credit,
                AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    //BankPayments = Ledger.OrderBy(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalCount = Ledger.Count(),
                    BankPayments = Ledger.ToList(),
                    TotalCount = count,
                    PageSize = obj.PageSize
                    //Branchs = userDetails.BranchIds
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountTypeSummary(ClsBankPaymentVm obj)
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

            //if (obj.PageSize == 0)
            //{
            //    obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            //}

            //int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            //decimal TotalOpeningBalanceDebit = 0, TotalOpeningBalanceCredit = 0;

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            foreach (var prevtransaction in PreviousTransactions)
            {
                prevtransaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == prevtransaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            foreach (var transaction in Transactions)
            {
                transaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == transaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }

            var det = oConnectionContext.DbClsAccountType.ToList().Select(a => new ClsAccountTypeVm
            {
                AccountType = a.AccountType,
                AccountTypeId = a.AccountTypeId,
                AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(b => b.CompanyId == obj.CompanyId && b.AccountTypeId == a.AccountTypeId).ToList().Select(b => new ClsAccountSubTypeVm
                {
                    AccountSubType = b.AccountSubType,
                    AccountSubTypeId = b.AccountSubTypeId,
                    OpeningBalanceDebit = (PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Debit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Credit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Debit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Credit).DefaultIfEmpty().Sum()) : 0,
                    OpeningBalanceCredit = (PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Credit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Debit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Credit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Debit).DefaultIfEmpty().Sum()) : 0,
                    Debit = Transactions.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Debit).DefaultIfEmpty().Sum(),
                    Credit = Transactions.Where(c => c.AccountSubTypeId == b.AccountSubTypeId).Select(c => c.Credit).DefaultIfEmpty().Sum()
                }).ToList()
            }).OrderBy(a => a.AccountTypeId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    AccountTypes = det
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GeneralLedger(ClsBankPaymentVm obj)
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

            //if (obj.PageSize == 0)
            //{
            //    obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            //}

            //int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            //decimal TotalOpeningBalanceDebit = 0, TotalOpeningBalanceCredit = 0;

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            var det = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).AsEnumerable().Select(a =>
            new ClsAccountVm
            {
                AccountSubTypeId = a.AccountSubTypeId,
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                AccountNumber = a.AccountNumber,
                OpeningBalanceDebit = (PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Debit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Credit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Debit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Credit).DefaultIfEmpty().Sum()) : 0,
                OpeningBalanceCredit = (PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Credit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Debit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Credit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountId == a.AccountId).Select(c => c.Debit).DefaultIfEmpty().Sum()) : 0,
                Debit = Transactions.Where(b => b.AccountId == a.AccountId).Select(b => b.Debit).DefaultIfEmpty().Sum(),
                Credit = Transactions.Where(b => b.AccountId == a.AccountId).Select(b => b.Credit).DefaultIfEmpty().Sum(),
            }).OrderBy(a => a.AccountId).ToList();

            if (obj.AccountSubTypeId != 0)
            {
                det = det.Where(a => a.AccountSubTypeId == obj.AccountSubTypeId).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    Accounts = det
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TrialBalance(ClsBankPaymentVm obj)
        {
            if (obj.ToDate == DateTime.MinValue)
            {
                obj.ToDate = DateTime.Now;
            }

            int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

            obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(obj.ToDate.Year));

            if (obj.ToDate < obj.FromDate)
            {
                //obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(obj.ToDate.AddYears(-1)));
                obj.FromDate = obj.FromDate.AddYears(-1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            //if (obj.PageSize == 0)
            //{
            //    obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            //}

            //int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            Ledger = PreviousLedger.Concat(Transactions).ToList();

            var det = oConnectionContext.DbClsAccountType.ToList().Select(a => new ClsAccountTypeVm
            {
                AccountType = a.AccountType,
                AccountTypeId = a.AccountTypeId,
                Accounts = oConnectionContext.DbClsAccount.Where(b => b.AccountTypeId == a.AccountTypeId).ToList().Select(b => new ClsAccountVm
                {
                    AccountId = b.AccountId,
                    AccountName = b.AccountName,
                    AccountNumber = b.AccountNumber,
                    Debit = Ledger.Where(c => c.AccountId == b.AccountId).Select(c => c.Debit).DefaultIfEmpty().Sum(),
                    Credit = Ledger.Where(c => c.AccountId == b.AccountId).Select(c => c.Credit).DefaultIfEmpty().Sum(),
                }).ToList()
            }).OrderBy(a => a.AccountTypeId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    AccountTypes = det
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SundryDebtors(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            //Ledger = PreviousLedger.Concat(Transactions).ToList();

            long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

            var det = oConnectionContext.DbClsUser.Where(a => a.IsDeleted == false && a.IsActive == true &&
            a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer").AsEnumerable().Select(a => new ClsUserVm
            {
                Name = a.Name,
                MobileNo = a.MobileNo,
                UserId = a.UserId,
                OpeningBalanceDebit = (PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum()) : 0,
                OpeningBalanceCredit = (PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum()) : 0,
                Debit = Transactions.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum(),
                Credit = Transactions.Where(c => c.AccountId == AccountId && c.CustomerId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum(),
            }).OrderBy(a => a.UserId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SundryDebtorDetails(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            decimal TotalPreviousDebit = 0, TotalPreviousCredit = 0;

            long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj).Where(a => a.AccountId == AccountId && a.CustomerId == obj.CustomerId).ToList();

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;
            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();
            PreviousLedger = PreviousLedger.Where(a => a.AccountId == AccountId && a.CustomerId == obj.CustomerId).ToList();
            TotalPreviousDebit = PreviousLedger.Select(a => a.Debit).DefaultIfEmpty().Sum();
            TotalPreviousCredit = PreviousLedger.Select(a => a.Credit).DefaultIfEmpty().Sum();

            Ledger.Add(new ClsBankPaymentVm
            {
                AccountId = 0,
                AccountSubTypeId = 0,
                Notes = "",
                AddedOn = obj.ToDate,
                Type = "Opening Balance",
                ReferenceNo = "",
                Debit = (TotalPreviousDebit > TotalPreviousCredit) ? (TotalPreviousDebit - TotalPreviousCredit) : 0,
                Credit = (TotalPreviousCredit > TotalPreviousDebit) ? (TotalPreviousCredit - TotalPreviousDebit) : 0,
            });

            Ledger = Ledger.Concat(Transactions).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = Ledger.OrderBy(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = Ledger.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SundryCreditors(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            //Ledger = PreviousLedger.Concat(Transactions).ToList();

            long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

            var det = oConnectionContext.DbClsUser.Where(a => a.IsDeleted == false && a.IsActive == true &&
            a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier").AsEnumerable().Select(a => new ClsUserVm
            {
                Name = a.Name,
                MobileNo = a.MobileNo,
                UserId = a.UserId,
                OpeningBalanceDebit = (PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum()) : 0,
                OpeningBalanceCredit = (PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum() >
PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum()) ?
(PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum() -
PreviousLedger.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum()) : 0,
                Debit = Transactions.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Debit).DefaultIfEmpty().Sum(),
                Credit = Transactions.Where(c => c.AccountId == AccountId && c.SupplierId == a.UserId).Select(c => c.Credit).DefaultIfEmpty().Sum(),
            }).OrderBy(a => a.UserId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SundryCreditorDetails(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            decimal TotalPreviousDebit = 0, TotalPreviousCredit = 0;
            long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj).Where(a => a.AccountId == AccountId && a.SupplierId == obj.SupplierId).ToList();

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;
            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();
            PreviousLedger = PreviousLedger.Where(a => a.AccountId == AccountId && a.SupplierId == obj.SupplierId).ToList();
            TotalPreviousDebit = PreviousLedger.Select(a => a.Debit).DefaultIfEmpty().Sum();
            TotalPreviousCredit = PreviousLedger.Select(a => a.Credit).DefaultIfEmpty().Sum();

            Ledger.Add(new ClsBankPaymentVm
            {
                AccountId = 0,
                AccountSubTypeId = 0,
                Notes = "",
                AddedOn = obj.ToDate,
                Type = "Opening Balance",
                ReferenceNo = "",
                Debit = (TotalPreviousDebit > TotalPreviousCredit) ? (TotalPreviousDebit - TotalPreviousCredit) : 0,
                Credit = (TotalPreviousCredit > TotalPreviousDebit) ? (TotalPreviousCredit - TotalPreviousDebit) : 0,
            });

            Ledger = Ledger.Concat(Transactions).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = Ledger.OrderBy(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = Ledger.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ProfitAndLoss(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);
            foreach (var transaction in Transactions)
            {
                transaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == transaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            foreach (var prevtransaction in PreviousTransactions)
            {
                prevtransaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == prevtransaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            Ledger = PreviousLedger.Concat(Transactions).ToList();

            long IncomeId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Income").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long CostofGoodsSoldId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Cost of Goods Sold").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long ExpenseId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Expense").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherIncomeId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Other Income").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherExpenseId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Other Expense").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var accounts = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId &&
            a.IsDeleted == false && a.IsActive == true).AsEnumerable().Select(a =>
             new ClsAccountVm
             {
                 AccountSubTypeId = a.AccountSubTypeId,
                 AccountId = a.AccountId,
                 AccountName = a.AccountName,
                 AccountNumber = a.AccountNumber,
                 Debit = Ledger.Where(b => b.AccountId == a.AccountId).Select(b => b.Debit).DefaultIfEmpty().Sum(),
                 Credit = Ledger.Where(b => b.AccountId == a.AccountId).Select(b => b.Credit).DefaultIfEmpty().Sum(),
             }).OrderBy(a => a.AccountId).ToList();

            List<ClsAccountSubTypeVm> det = new List<ClsAccountSubTypeVm>();

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Operating Income",
                Accounts = accounts.Where(a => a.AccountSubTypeId == IncomeId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Cost of Goods Sold",
                Accounts = accounts.Where(a => a.AccountSubTypeId == CostofGoodsSoldId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Operating Expense",
                Accounts = accounts.Where(a => a.AccountSubTypeId == ExpenseId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Non Operating Income",
                Accounts = accounts.Where(a => a.AccountSubTypeId == OtherIncomeId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Non Operating Expense",
                Accounts = accounts.Where(a => a.AccountSubTypeId == OtherExpenseId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountSubTypes = det.ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BalanceSheet(ClsBankPaymentVm obj)
        {
            if (obj.ToDate == DateTime.MinValue)
            {
                obj.ToDate = DateTime.Now;
            }

            int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

            obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(obj.ToDate.Year));

            if (obj.ToDate < obj.FromDate)
            {
                //obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(obj.ToDate.AddYears(-1)));
                obj.FromDate = obj.FromDate.AddYears(-1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);
            foreach (var transaction in Transactions)
            {
                transaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == transaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            foreach (var prevtransaction in PreviousTransactions)
            {
                prevtransaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == prevtransaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            Ledger = PreviousLedger.Concat(Transactions).ToList();

            #region profit & Loss

            long IncomeId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Income").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long CostofGoodsSoldId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Cost of Goods Sold").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long ExpenseId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Expense").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherIncomeId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Other Income").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherExpenseId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Other Expense").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var accounts = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId &&
            a.IsDeleted == false && a.IsActive == true).AsEnumerable().Select(a =>
             new ClsAccountVm
             {
                 AccountSubTypeId = a.AccountSubTypeId,
                 AccountId = a.AccountId,
                 AccountName = a.AccountName,
                 AccountNumber = a.AccountNumber,
                 Debit = Ledger.Where(b => b.AccountId == a.AccountId).Select(b => b.Debit).DefaultIfEmpty().Sum(),
                 Credit = Ledger.Where(b => b.AccountId == a.AccountId).Select(b => b.Credit).DefaultIfEmpty().Sum(),
             }).OrderBy(a => a.AccountId).ToList();

            decimal OperatingIncome = 0, CostOfGoods = 0, OperatingExpense = 0, NonOperatingIncome = 0,
                NonOperatingExpense = 0, NetProfiLoss = 0;

            OperatingIncome = accounts.Where(a => a.AccountSubTypeId == IncomeId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            CostOfGoods = accounts.Where(a => a.AccountSubTypeId == CostofGoodsSoldId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            OperatingExpense = accounts.Where(a => a.AccountSubTypeId == ExpenseId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            NonOperatingIncome = accounts.Where(a => a.AccountSubTypeId == OtherIncomeId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            NonOperatingExpense = accounts.Where(a => a.AccountSubTypeId == OtherExpenseId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();

            NetProfiLoss = ((OperatingIncome - CostOfGoods) - (OperatingExpense + NonOperatingIncome) - (NonOperatingExpense));
            #endregion

            #region Balance Sheet

            #endregion
            //long AssetId = oConnectionContext.DbClsAccountType.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //    && a.AccountType == "Asset").Select(a => a.AccountTypeId).FirstOrDefault();

            //long LiabilityId = oConnectionContext.DbClsAccountType.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //    && a.AccountType == "Liability").Select(a => a.AccountTypeId).FirstOrDefault();

            //long EquityId = oConnectionContext.DbClsAccountType.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //    && a.AccountType == "Equity").Select(a => a.AccountTypeId).FirstOrDefault();

            long CashId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Cash").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long BankId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Bank").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long AccountsReceivableId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Accounts Receivable").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherCurrentAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Other Current Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long StockId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Stock").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Other Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherCurrentLiabilityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Other Current Liability").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherLiabilityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Other Liability").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long CreditCardId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Credit Card").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long AccountsPayableId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Accounts Payable").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long LongTermLiabilityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Long Term Liability").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long EquityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
             && a.Type == "Equity").Select(a => a.AccountSubTypeId).FirstOrDefault();

            List<ClsAccountTypeVm> det = new List<ClsAccountTypeVm>();

            det.Add(new ClsAccountTypeVm
            {
                AccountType = "Assets",
                AccountSubHeaders = new List<ClsAccountSubHeadersVm>
                {
                  new  ClsAccountSubHeadersVm {
                    AccountSubHeader = "Current Assets",
                    AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == CashId
                    || b.AccountSubTypeId == BankId || b.AccountSubTypeId == AccountsReceivableId ||
                    b.AccountSubTypeId == OtherCurrentAssetId || b.AccountSubTypeId == StockId).ToList().Select(b => new ClsAccountSubTypeVm
                    {
                        Sequence = b.Type == "Cash" ? 1 : b.Type == "Bank" ?2 :b.Type == "Accounts Receivable" ? 3 : b.Type == "Stock" ? 4 :5,
                        Type = b.Type,
                        AccountSubType = b.AccountSubType,
                        AccountSubTypeId = b.AccountSubTypeId,
                        Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountSubTypeId == b.AccountSubTypeId && aa.CompanyId == obj.CompanyId &&
                        aa.IsDeleted == false && aa.IsActive == true).ToList().Select(aa =>
                            new ClsAccountVm
                            {
                                Type = aa.Type,
                                AccountId = aa.AccountId,
                                AccountName = aa.AccountName,
                                AccountNumber = aa.AccountNumber,
                                Debit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Debit).DefaultIfEmpty().Sum(),
                                Credit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Credit).DefaultIfEmpty().Sum(),
                            }).ToList()
                    }).OrderBy(b=>b.Sequence).ToList()
                  },
                   new  ClsAccountSubHeadersVm  {
                AccountSubHeader = "Other Assets",
                AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == OtherAssetId).ToList().Select(b => new ClsAccountSubTypeVm
                {
                    Type = b.Type,
                    AccountSubType = b.AccountSubType,
                    AccountSubTypeId = b.AccountSubTypeId,
                    Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountSubTypeId == b.AccountSubTypeId && aa.CompanyId == obj.CompanyId &&
                    aa.IsDeleted == false && aa.IsActive == true).ToList().Select(aa =>
                        new ClsAccountVm
                        {
                            Type = aa.Type,
                            AccountId = aa.AccountId,
                            AccountName = aa.AccountName,
                            AccountNumber = aa.AccountNumber,
                            Debit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Debit).DefaultIfEmpty().Sum(),
                            Credit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Credit).DefaultIfEmpty().Sum(),
                        }).ToList()
                }).ToList()

            },
                    new  ClsAccountSubHeadersVm  {
                AccountSubHeader = "Fixed Assets",
                AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == FixedAssetId).ToList().Select(b => new ClsAccountSubTypeVm
                {
                    Type = b.Type,
                    AccountSubType = b.AccountSubType,
                    AccountSubTypeId = b.AccountSubTypeId,
                    Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountSubTypeId == b.AccountSubTypeId && aa.CompanyId == obj.CompanyId &&
                    aa.IsDeleted == false && aa.IsActive == true).ToList().Select(aa =>
                        new ClsAccountVm
                        {
                            Type = aa.Type,
                            AccountId = aa.AccountId,
                            AccountName = aa.AccountName,
                            AccountNumber = aa.AccountNumber,
                            Debit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Debit).DefaultIfEmpty().Sum(),
                            Credit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Credit).DefaultIfEmpty().Sum(),
                        }).ToList()
                }).ToList()
                    }
                }
            });

            det.Add(new ClsAccountTypeVm
            {
                AccountType = "Liabilities & Equities",
                AccountSubHeaders = new List<ClsAccountSubHeadersVm>
                {
                  new  ClsAccountSubHeadersVm {
                    AccountSubHeader = "Liabilities",
                    AccountSubTypes = new List<ClsAccountSubTypeVm>
                    {
                        new ClsAccountSubTypeVm
                        {
                            AccountSubType = "Current Liabilities",
                            Accounts = oConnectionContext.DbClsAccount.Where(aa => (aa.AccountSubTypeId == OtherCurrentLiabilityId ||
                            aa.AccountSubTypeId == OtherLiabilityId || aa.AccountSubTypeId == CreditCardId || aa.AccountSubTypeId == AccountsPayableId)
                            && aa.CompanyId == obj.CompanyId &&
                        aa.IsDeleted == false && aa.IsActive == true).ToList().Select(aa =>
                            new ClsAccountVm
                            {
                                AccountId = aa.AccountId,
                                AccountName = aa.AccountName,
                                AccountNumber = aa.AccountNumber,
                                Debit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Debit).DefaultIfEmpty().Sum(),
                                Credit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Credit).DefaultIfEmpty().Sum(),
                            }).ToList()
                        },
                         new ClsAccountSubTypeVm
                        {
                            AccountSubType = "Long Term Liabilities",
                            Accounts = oConnectionContext.DbClsAccount.Where(aa => (aa.AccountSubTypeId == LongTermLiabilityId)
                            && aa.CompanyId == obj.CompanyId &&
                        aa.IsDeleted == false && aa.IsActive == true).ToList().Select(aa =>
                            new ClsAccountVm
                            {
                                Type = aa.Type,
                                AccountId = aa.AccountId,
                                AccountName = aa.AccountName,
                                AccountNumber = aa.AccountNumber,
                                Debit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Debit).DefaultIfEmpty().Sum(),
                                Credit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Credit).DefaultIfEmpty().Sum(),
                            }).ToList()
                        }
                    }
                  },
                   new  ClsAccountSubHeadersVm  {
                AccountSubHeader = "Equities",
                AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(b => b.AccountSubTypeId == EquityId).ToList().Select(b => new ClsAccountSubTypeVm
                {
                    Type = b.Type,
                    AccountSubType = b.AccountSubType,
                    AccountSubTypeId = b.AccountSubTypeId,
                    Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountSubTypeId == b.AccountSubTypeId && aa.CompanyId == obj.CompanyId &&
                    aa.IsDeleted == false && aa.IsActive == true).ToList().Select(aa =>
                        new ClsAccountVm
                        {
                            Type = aa.Type,
                            AccountId = aa.AccountId,
                            AccountName = aa.AccountName,
                            AccountNumber = aa.AccountNumber,
                            Debit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Debit).DefaultIfEmpty().Sum(),
                            Credit = Ledger.Where(bb => bb.AccountId == aa.AccountId).Select(bb => bb.Credit).DefaultIfEmpty().Sum(),
                        }).ToList()
                }).ToList()
                   }
                }
            });

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    NetProfitLoss = NetProfiLoss,
                    AccountTypes = det.ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CashFlow(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousTransactions = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> PreviousLedger = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            OpeningBalance = oCommonController.OpeningBalance(obj);
            Transactions = oCommonController.AccountTransactions(obj);
            foreach (var transaction in Transactions)
            {
                transaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == transaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.OpeningBalance(obj);
            PreviousTransactions = oCommonController.AccountTransactions(obj);
            foreach (var prevtransaction in PreviousTransactions)
            {
                prevtransaction.AccountSubTypeId = oConnectionContext.DbClsAccount.Where(a => a.AccountId == prevtransaction.AccountId).Select(a => a.AccountSubTypeId).FirstOrDefault();
            }
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            Ledger = PreviousLedger.Concat(Transactions).ToList();

            #region profit & Loss

            long IncomeId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Income").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long CostofGoodsSoldId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Cost of Goods Sold").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long ExpenseId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Expense").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherIncomeId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Other Income").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherExpenseId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Other Expense").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var accounts = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId &&
            a.IsDeleted == false && a.IsActive == true).AsEnumerable().Select(a =>
             new ClsAccountVm
             {
                 AccountSubTypeId = a.AccountSubTypeId,
                 AccountId = a.AccountId,
                 AccountName = a.AccountName,
                 AccountNumber = a.AccountNumber,
                 Debit = Ledger.Where(b => b.AccountId == a.AccountId).Select(b => b.Debit).DefaultIfEmpty().Sum(),
                 Credit = Ledger.Where(b => b.AccountId == a.AccountId).Select(b => b.Credit).DefaultIfEmpty().Sum(),
             }).OrderBy(a => a.AccountId).ToList();

            decimal OperatingIncome = 0, CostOfGoods = 0, OperatingExpense = 0, NonOperatingIncome = 0,
                NonOperatingExpense = 0, NetProfiLoss = 0;

            OperatingIncome = accounts.Where(a => a.AccountSubTypeId == IncomeId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            CostOfGoods = accounts.Where(a => a.AccountSubTypeId == CostofGoodsSoldId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            OperatingExpense = accounts.Where(a => a.AccountSubTypeId == ExpenseId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            NonOperatingIncome = accounts.Where(a => a.AccountSubTypeId == OtherIncomeId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();
            NonOperatingExpense = accounts.Where(a => a.AccountSubTypeId == OtherExpenseId).Select(a => a.Debit - a.Credit).DefaultIfEmpty().Sum();

            NetProfiLoss = ((OperatingIncome - CostOfGoods) - (OperatingExpense + NonOperatingIncome) - (NonOperatingExpense));
            #endregion

            long AccountsPayableId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Payable").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long AccountsReceivableId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Accounts Receivable").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherCurrentAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Other Current Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long CreditCardId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Credit Card").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherCurrentLiabilityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Other Current Liability").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long StockId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Stock").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long OtherAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Other Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long LongTermLiabilityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Long Term Liability").Select(a => a.AccountSubTypeId).FirstOrDefault();

            long EquityId = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Equity").Select(a => a.AccountSubTypeId).FirstOrDefault();

            accounts = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId &&
           a.IsDeleted == false && a.IsActive == true).AsEnumerable().Select(a =>
            new ClsAccountVm
            {
                AccountSubTypeId = a.AccountSubTypeId,
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                AccountNumber = a.AccountNumber,
                Debit = Transactions.Where(b => b.AccountId == a.AccountId).Select(b => b.Debit).DefaultIfEmpty().Sum(),
                Credit = Transactions.Where(b => b.AccountId == a.AccountId).Select(b => b.Credit).DefaultIfEmpty().Sum(),
            }).OrderBy(a => a.AccountId).ToList();

            List<ClsAccountSubTypeVm> det = new List<ClsAccountSubTypeVm>();

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Cash Flow from Operating Activities",
                Accounts = accounts.Where(a => a.AccountSubTypeId == AccountsPayableId || a.AccountSubTypeId == AccountsReceivableId
                || a.AccountSubTypeId == OtherCurrentAssetId || a.AccountSubTypeId == CreditCardId ||
                a.AccountSubTypeId == OtherCurrentLiabilityId || a.AccountSubTypeId == StockId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Cash Flow from Investing Activities",
                Accounts = accounts.Where(a => a.AccountSubTypeId == OtherAssetId || a.AccountSubTypeId == FixedAssetId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            det.Add(new ClsAccountSubTypeVm
            {
                AccountSubType = "Cash Flow from Financing Activities",
                Accounts = accounts.Where(a => a.AccountSubTypeId == LongTermLiabilityId || a.AccountSubTypeId == EquityId).Select(a => new ClsAccountVm
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.Debit,
                    Credit = a.Credit
                }).ToList()
            });

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    NetProfitLoss = NetProfiLoss,
                    AccountSubTypes = det.ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
