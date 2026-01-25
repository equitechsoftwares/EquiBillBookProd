using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
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
    public class AccountTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllAccountTypes(ClsAccountTypeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            var det = oConnectionContext.DbClsAccountType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.AccountTypeId,
                a.AccountType,
                a.AccountTypeCode,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.AccountType.Contains(obj.Search) || a.AccountTypeCode.Contains(obj.Search)).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountTypes = det.OrderByDescending(a => a.AccountType).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountType(ClsAccountType obj)
        {
            var det = oConnectionContext.DbClsAccountType.Where(a => a.AccountTypeId == obj.AccountTypeId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.AccountTypeId,
                a.AccountType,
                a.AccountTypeCode,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountType = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertAccountType(ClsAccountTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.AccountType == null || obj.AccountType == "")
                {
                    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                    isError = true;
                }

                //if (obj.AccountTypeCode != "" && obj.AccountTypeCode != null)
                //{
                //    if (oConnectionContext.DbClsAccountType.Where(a => a.AccountTypeCode == obj.AccountTypeCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Account Type code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}
                //else
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "AccountType"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.AccountTypeCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                if (obj.AccountType != null && obj.AccountType != "")
                {
                    if (oConnectionContext.DbClsAccountType.Where(a => a.AccountType.ToLower() == obj.AccountType.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Type exists", Id = "divAccountType" });
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
                ClsAccountType oAccountType = new ClsAccountType()
                {
                    AccountType = obj.AccountType,
                    AccountTypeCode = obj.AccountTypeCode,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsAccountType.Add(oAccountType);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Account Type",
                    CompanyId = obj.CompanyId,
                    Description = "added " + obj.AccountType,
                    Id = oAccountType.AccountTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account Type created successfully",
                    Data = new
                    {
                        AccountType = oAccountType
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateAccountType(ClsAccountTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.AccountType == null || obj.AccountType == "")
                {
                    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                    isError = true;
                }

                //if (obj.AccountTypeCode != "" && obj.AccountTypeCode != null)
                //{
                //    if (oConnectionContext.DbClsAccountType.Where(a => a.AccountTypeCode == obj.AccountTypeCode && a.AccountTypeId != obj.AccountTypeId && a.CompanyId == obj.CompanyId).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Account Type code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}

                if (oConnectionContext.DbClsAccountType.Where(a => a.AccountType.ToLower() == obj.AccountType.ToLower() && a.AccountTypeId != obj.AccountTypeId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Account Type exists", Id = "divAccountType" });
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

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccountType oAccountType = new ClsAccountType()
                {
                    AccountTypeId = obj.AccountTypeId,
                    //AccountTypeCode = obj.AccountTypeCode,
                    AccountType = obj.AccountType,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccountType.Attach(oAccountType);
                oConnectionContext.Entry(oAccountType).Property(x => x.AccountTypeId).IsModified = true;
                oConnectionContext.Entry(oAccountType).Property(x => x.AccountType).IsModified = true;
                //oConnectionContext.Entry(oAccountType).Property(x => x.AccountTypeCode).IsModified = true;
                oConnectionContext.Entry(oAccountType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAccountType).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Account Type",
                    CompanyId = obj.CompanyId,
                    Description = "modified " + obj.AccountType,
                    Id = oAccountType.AccountTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account Type updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountTypeActiveInactive(ClsAccountTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccountType oClsRole = new ClsAccountType()
                {
                    AccountTypeId = obj.AccountTypeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccountType.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountTypeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Account Type",
                    CompanyId = obj.CompanyId,
                    Description = (obj.IsActive == true ? "activated " : "deactivated ") + obj.AccountType,
                    Id = oClsRole.AccountTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account Type " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountTypeDelete(ClsAccountTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccountType oClsRole = new ClsAccountType()
                {
                    AccountTypeId = obj.AccountTypeId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccountType.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountTypeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Account Type",
                    CompanyId = obj.CompanyId,
                    Description = "deleted  " + oConnectionContext.DbClsAccountType.Where(a => a.AccountTypeId == obj.AccountTypeId).Select(a => a.AccountType).FirstOrDefault(),
                    Id = oClsRole.AccountTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account Type deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAccountTypes(ClsAccountTypeVm obj)
        {
            var det = oConnectionContext.DbClsAccountType.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                AccountTypeId = a.AccountTypeId,
                a.AccountTypeCode,
                AccountType = a.AccountType,
            }).OrderBy(a => a.AccountType).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountTypes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAccountTypesDropdown(ClsAccountTypeVm obj)
        {
            var det = oConnectionContext.DbClsAccountType.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                AccountTypeId = a.AccountTypeId,
                a.AccountTypeCode,
                AccountType = a.AccountType,
                AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(b => b.IsDeleted == false && b.IsActive == true 
                && b.AccountTypeId == a.AccountTypeId && b.CompanyId == obj.CompanyId).Select(b => new
                {
                    b.Type,
                    b.DisplayAs,
                    b.AccountSubTypeId,
                    b.AccountSubType
                }).OrderBy(b=>b.AccountSubType)
            }).OrderBy(a => a.AccountType).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountTypes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
