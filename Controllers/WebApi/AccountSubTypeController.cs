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
    public class AccountSubTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllAccountSubTypes(ClsAccountSubTypeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            var det = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.Type,
                AccountSubTypeId = a.AccountSubTypeId,
                a.AccountTypeId,
                AccountType = oConnectionContext.DbClsAccountType.Where(b => b.AccountTypeId == a.AccountTypeId).Select(b => b.AccountType).FirstOrDefault(),
                a.AccountSubTypeCode,
                AccountSubType = a.AccountSubType,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.ParentId,
                a.DisplayAs,
                a.CanDelete
            }).ToList();

            if (obj.AccountTypeId != 0)
            {
                det = det.Where(a => a.AccountTypeId == obj.AccountTypeId).ToList();
            }

            if (obj.AccountSubType != "" && obj.AccountSubType != null)
            {
                det = det.Where(a => a.AccountSubType.ToLower().Contains(obj.AccountSubType.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountSubTypes = det.OrderByDescending(a => a.AccountSubTypeId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountSubType(ClsAccountSubType obj)
        {
            var det = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubTypeId == obj.AccountSubTypeId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                AccountSubTypeId = a.AccountSubTypeId,
                a.AccountSubTypeCode,
                a.AccountTypeId,
                AccountSubType = a.AccountSubType,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.ParentId,
                a.DisplayAs
            }).FirstOrDefault();

            //var AccountTypes = oConnectionContext.DbClsAccountType.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    AccountTypeId = a.AccountTypeId,
            //    a.AccountTypeCode,
            //    AccountType = a.AccountType,
            //}).OrderBy(a => a.AccountType).ToList();

            var AccountSubTypes = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && 
            a.IsActive == true && a.AccountTypeId == det.AccountTypeId).Select(a => new
            {
                AccountSubTypeId = a.AccountSubTypeId,
                a.AccountSubTypeCode,
                AccountSubType = a.AccountSubType,
                a.DisplayAs
            }).OrderBy(a => a.AccountSubType).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountSubType = det,
                    AccountSubTypes = AccountSubTypes
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertAccountSubType(ClsAccountSubTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                //long PrefixUserMapId = 0;

                if (obj.AccountSubType == null || obj.AccountSubType == "")
                {
                    errors.Add(new ClsError { Message = "Account Sub Type is required", Id = "divAccountSubType" });
                    isError = true;
                }

                if (obj.AccountTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                    isError = true;
                }

                if (obj.AccountSubType != "" && obj.AccountSubType != null)
                {
                    if (oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType.ToLower() == obj.AccountSubType.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Sub Type exists", Id = "divAccountSubType" });
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
                ClsAccountSubType oAccountSubType = new ClsAccountSubType()
                {
                    AccountSubType = obj.AccountSubType,
                    AccountSubTypeCode = obj.AccountSubTypeCode,
                    AccountTypeId = obj.AccountTypeId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    ParentId = obj.ParentId,
                    DisplayAs = obj.DisplayAs,
                    CanDelete = true
                };
                oConnectionContext.DbClsAccountSubType.Add(oAccountSubType);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Account Type",
                    CompanyId = obj.CompanyId,
                    Description = "added " + obj.AccountSubType,
                    Id = oAccountSubType.AccountSubTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Account Type created successfully",
                    Data = new
                    {
                        AccountSubType = oAccountSubType
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateAccountSubType(ClsAccountSubTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.AccountSubType == null || obj.AccountSubType == "")
                {
                    errors.Add(new ClsError { Message = "Account Sub Type is required", Id = "divAccountSubType" });
                    isError = true;
                }

                if (obj.AccountTypeId == 0)
                {
                    errors.Add(new ClsError { Message = "Account Type is required", Id = "divAccountType" });
                    isError = true;
                }

                if (obj.AccountSubType != "" && obj.AccountSubType != null)
                {
                    if (oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType.ToLower() == obj.AccountSubType.ToLower()
                && a.AccountSubTypeId != obj.AccountSubTypeId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Account Sub Type exists", Id = "divAccountSubType" });
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
                ClsAccountSubType oAccountSubType = new ClsAccountSubType()
                {
                    AccountSubTypeId = obj.AccountSubTypeId,
                    //AccountSubTypeCode = obj.AccountSubTypeCode,
                    AccountTypeId = obj.AccountTypeId,
                    AccountSubType = obj.AccountSubType,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    ParentId = obj.ParentId,
                    DisplayAs = obj.DisplayAs
                };
                oConnectionContext.DbClsAccountSubType.Attach(oAccountSubType);
                oConnectionContext.Entry(oAccountSubType).Property(x => x.AccountSubTypeId).IsModified = true;
                oConnectionContext.Entry(oAccountSubType).Property(x => x.AccountSubType).IsModified = true;
                //oConnectionContext.Entry(oAccountSubType).Property(x => x.AccountSubTypeCode).IsModified = true;
                oConnectionContext.Entry(oAccountSubType).Property(x => x.AccountTypeId).IsModified = true;
                oConnectionContext.Entry(oAccountSubType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAccountSubType).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oAccountSubType).Property(x => x.ParentId).IsModified = true;
                oConnectionContext.Entry(oAccountSubType).Property(x => x.DisplayAs).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Account Type",
                    CompanyId = obj.CompanyId,
                    Description = "modified " + obj.AccountSubType,
                    Id = oAccountSubType.AccountSubTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Account Type updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountSubTypeActiveInactive(ClsAccountSubTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccountSubType oClsRole = new ClsAccountSubType()
                {
                    AccountSubTypeId = obj.AccountSubTypeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccountSubType.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountSubTypeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Account Type",
                    CompanyId = obj.CompanyId,
                    Description = (obj.IsActive == true ? "activated " : "deactivated ") + obj.AccountSubType,
                    Id = oClsRole.AccountSubTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account Sub Type " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AccountSubTypeDelete(ClsAccountSubTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAccountSubType oClsRole = new ClsAccountSubType()
                {
                    AccountSubTypeId = obj.AccountSubTypeId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccountSubType.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.AccountSubTypeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sub Account Type",
                    CompanyId = obj.CompanyId,
                    Description = "deleted  " + oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubTypeId == obj.AccountSubTypeId).Select(a => a.AccountSubType).FirstOrDefault(),
                    Id = oClsRole.AccountSubTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sub Account Type deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        public async Task<IHttpActionResult> ActiveAccountSubTypes(ClsAccountSubTypeVm obj)
        {
            var det = oConnectionContext.DbClsAccountSubType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.AccountTypeId == obj.AccountTypeId).Select(a => new
            {
                a.Type,
                AccountSubTypeId = a.AccountSubTypeId,
                a.AccountSubTypeCode,
                AccountSubType = a.AccountSubType,
                a.DisplayAs
            }).OrderBy(a => a.AccountSubType).ToList();
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
    }
}
