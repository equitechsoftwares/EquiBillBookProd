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
    public class TaxExemptionController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllTaxExemptions(ClsTaxExemptionVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsTaxExemption.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.CanDelete,
                a.TaxExemptionType,
                TaxExemptionId = a.TaxExemptionId,
                Reason = a.Reason,
                a.Description,
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
                det = det.Where(a => a.Reason.ToLower().Contains(obj.Search.ToLower()) || a.Description.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxExemptions = det.OrderByDescending(a => a.TaxExemptionId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxExemption(ClsTaxExemption obj)
        {
            var det = oConnectionContext.DbClsTaxExemption.Where(a => a.TaxExemptionId == obj.TaxExemptionId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CanDelete,
                a.TaxExemptionType,
                TaxExemptionId = a.TaxExemptionId,
                a.Reason,
                a.Description,
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
                    TaxExemption = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTaxExemption(ClsTaxExemptionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Reason == null || obj.Reason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divReason" });
                    isError = true;
                }

                if (obj.Reason != null && obj.Reason != "")
                {
                    if (oConnectionContext.DbClsTaxExemption.Where(a => a.Reason.ToLower() == obj.Reason.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Reason exists", Id = "divReason" });
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

                ClsTaxExemption oTaxExemption = new ClsTaxExemption()
                {
                    TaxExemptionType = obj.TaxExemptionType,
                    Reason = obj.Reason,
                    Description = obj.Description,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    CanDelete=true
                };
                oConnectionContext.DbClsTaxExemption.Add(oTaxExemption);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Exemption",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Exemption \"" + obj.Reason + "\" created",
                    Id = oTaxExemption.TaxExemptionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Exemption created successfully",
                    Data = new
                    {
                        TaxExemption = oTaxExemption
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateTaxExemption(ClsTaxExemptionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Reason == null || obj.Reason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divReason" });
                    isError = true;
                }

                if (obj.Reason != null && obj.Reason != "")
                {
                    if (oConnectionContext.DbClsTaxExemption.Where(a => a.Reason.ToLower() == obj.Reason.ToLower() && a.TaxExemptionId != obj.TaxExemptionId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Reason exists", Id = "divReason" });
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

                ClsTaxExemption oTaxExemption = new ClsTaxExemption()
                {
                    TaxExemptionId = obj.TaxExemptionId,
                    TaxExemptionType = obj.TaxExemptionType,
                    Reason = obj.Reason,
                    Description = obj.Description,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTaxExemption.Attach(oTaxExemption);
                oConnectionContext.Entry(oTaxExemption).Property(x => x.TaxExemptionType).IsModified = true;
                oConnectionContext.Entry(oTaxExemption).Property(x => x.Reason).IsModified = true;
                oConnectionContext.Entry(oTaxExemption).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oTaxExemption).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTaxExemption).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Exemption",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Exemption \"" + obj.Reason + "\" updated",
                    Id = oTaxExemption.TaxExemptionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Exemption updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxExemptionActiveInactive(ClsTaxExemptionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsTaxExemption oClsRole = new ClsTaxExemption()
                {
                    TaxExemptionId = obj.TaxExemptionId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTaxExemption.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Exemption",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Exemption \"" + oConnectionContext.DbClsTaxExemption.Where(a => a.TaxExemptionId == obj.TaxExemptionId).Select(a => a.Reason).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.TaxExemptionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Exemption " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxExemptionDelete(ClsTaxExemptionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCodeCount = oConnectionContext.DbClsItemCode.Where(a => a.CompanyId == obj.CompanyId && a.TaxExemptionId == obj.TaxExemptionId && a.IsDeleted == false).Count();
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.TaxExemptionId == obj.TaxExemptionId && a.IsDeleted == false).Count();
                if (ItemCodeCount > 0 || ItemCount > 0)
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
                ClsTaxExemption oClsRole = new ClsTaxExemption()
                {
                    TaxExemptionId = obj.TaxExemptionId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTaxExemption.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Exemption",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Exemption \"" + oConnectionContext.DbClsTaxExemption.Where(a => a.TaxExemptionId == obj.TaxExemptionId).Select(a => a.Reason).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.TaxExemptionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Exemption deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveTaxExemptions(ClsTaxExemptionVm obj)
        {
            var det = oConnectionContext.DbClsTaxExemption.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                TaxExemptionId = a.TaxExemptionId,
                a.Reason,
                a.TaxExemptionType,
                a.Description,
            }).OrderBy(a => a.Reason).ToList();

            if(obj.TaxExemptionType != "" && obj.TaxExemptionType != null)
            {
                det = det.Where(a => a.TaxExemptionType.ToLower() == obj.TaxExemptionType.ToLower()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxExemptions = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
