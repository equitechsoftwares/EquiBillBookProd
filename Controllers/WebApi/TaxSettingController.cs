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
    public class TaxSettingController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllTaxSettings(ClsTaxSettingVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                BusinessRegistrationName = oConnectionContext.DbClsBusinessRegistrationName.Where(b=>b.BusinessRegistrationNameId == a.BusinessRegistrationNameId).Select(b=>b.Name).FirstOrDefault(),
                TaxSettingId = a.TaxSettingId,
                a.BusinessRegistrationNameId,
                a.BusinessRegistrationNo,
                a.BusinessLegalName,
                a.BusinessTradeName,
                a.RegisteredOn,
                //a.IsCompositionScheme,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.CompositionSchemeTaxId,
                a.AllowSalesReverseCharge
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.BusinessRegistrationNo.ToLower().Contains(obj.Search.ToLower()) 
                || a.BusinessLegalName.ToLower().Contains(obj.Search.ToLower())
                || a.BusinessTradeName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxSettings = det.OrderByDescending(a => a.TaxSettingId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxSetting(ClsTaxSetting obj)
        {

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = oConnectionContext.DbClsTaxSetting.Where(a => a.TaxSettingId == obj.TaxSettingId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.TaxSettingId,
                a.BusinessRegistrationNameId,
                a.BusinessRegistrationNo,
                a.BusinessLegalName,
                a.BusinessTradeName,
                a.RegisteredOn,
                //a.IsCompositionScheme,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.CompositionSchemeTaxId,
                a.AllowSalesReverseCharge
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxSetting = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTaxSetting(ClsTaxSettingVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                int TotalTaxSettingUsed = oConnectionContext.DbClsTaxSetting.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count();
                int TotalTaxSetting = oCommonController.fetchPlanQuantity(obj.CompanyId, "Tax Setting");
                if (TotalTaxSettingUsed >= TotalTaxSetting)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "GSTIN quota already used. Please upgrade addons from My Plan Menu",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.BusinessRegistrationNameId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationName" });
                    isError = true;
                }

                if (obj.BusinessRegistrationNo == null || obj.BusinessRegistrationNo == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                    isError = true;
                }

                if (obj.BusinessRegistrationNo != null && obj.BusinessRegistrationNo != "")
                {
                    if (oConnectionContext.DbClsTaxSetting.Where(a => a.BusinessRegistrationNo.ToLower() == obj.BusinessRegistrationNo.ToLower()
                    && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Business Registration No exists", Id = "divReason" });
                        isError = true;
                    }
                }

                if (obj.IsCompositionScheme == true)
                {
                    if (obj.CompositionSchemeTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divCompositionSchemeTax" });
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

                ClsTaxSetting oTaxSetting = new ClsTaxSetting()
                {
                    BusinessRegistrationNameId= obj.BusinessRegistrationNameId,
                    BusinessRegistrationNo= obj.BusinessRegistrationNo,
                    BusinessLegalName= obj.BusinessLegalName,
                    BusinessTradeName = obj.BusinessTradeName,
                    RegisteredOn = obj.RegisteredOn,
                    //IsCompositionScheme = obj.IsCompositionScheme,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    CompositionSchemeTaxId = obj.CompositionSchemeTaxId,
                    AllowSalesReverseCharge = obj.AllowSalesReverseCharge
                };
                oConnectionContext.DbClsTaxSetting.Add(oTaxSetting);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Setting",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Setting \"" + obj.BusinessRegistrationNo + "\" created",
                    Id = oTaxSetting.TaxSettingId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Setting created successfully",
                    Data = new
                    {
                        TaxSetting = oTaxSetting
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateTaxSetting(ClsTaxSettingVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.BusinessRegistrationNameId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationName" });
                    isError = true;
                }

                if (obj.BusinessRegistrationNo == null || obj.BusinessRegistrationNo == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                    isError = true;
                }

                if (obj.BusinessRegistrationNo != null && obj.BusinessRegistrationNo != "")
                {
                    if (oConnectionContext.DbClsTaxSetting.Where(a => a.TaxSettingId != obj.TaxSettingId && 
                    a.BusinessRegistrationNo.ToLower() == obj.BusinessRegistrationNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Business Registration No exists", Id = "divReason" });
                        isError = true;
                    }
                }

                if (obj.IsCompositionScheme == true)
                {
                    if (obj.CompositionSchemeTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divCompositionSchemeTax" });
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

                ClsTaxSetting oTaxSetting = new ClsTaxSetting()
                {
                    TaxSettingId = obj.TaxSettingId,
                    BusinessRegistrationNameId = obj.BusinessRegistrationNameId,
                    BusinessRegistrationNo = obj.BusinessRegistrationNo,
                    BusinessLegalName = obj.BusinessLegalName,
                    BusinessTradeName = obj.BusinessTradeName,
                    RegisteredOn = obj.RegisteredOn,
                    //IsCompositionScheme = obj.IsCompositionScheme,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    CompositionSchemeTaxId = obj.CompositionSchemeTaxId,
                    AllowSalesReverseCharge = obj.AllowSalesReverseCharge
                };
                oConnectionContext.DbClsTaxSetting.Attach(oTaxSetting);
                //oConnectionContext.Entry(oTaxSetting).Property(x => x.BusinessRegistrationNameId).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.BusinessRegistrationNo).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.BusinessLegalName).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.BusinessTradeName).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.RegisteredOn).IsModified = true;
                //oConnectionContext.Entry(oTaxSetting).Property(x => x.IsCompositionScheme).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.CompositionSchemeTaxId).IsModified = true;
                oConnectionContext.Entry(oTaxSetting).Property(x => x.AllowSalesReverseCharge).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Setting",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Setting \"" + obj.BusinessRegistrationNo + "\" updated",
                    Id = oTaxSetting.TaxSettingId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Setting updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxSettingActiveInactive(ClsTaxSettingVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsTaxSetting oClsRole = new ClsTaxSetting()
                {
                    TaxSettingId = obj.TaxSettingId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTaxSetting.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TaxSettingId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Setting",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Setting \"" + oConnectionContext.DbClsTaxSetting.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.TaxSettingId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Setting " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxSettingDelete(ClsTaxSettingVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.TaxSettingId == obj.TaxSettingId && a.IsDeleted == false).Count();
                if (BranchCount > 0)
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
                ClsTaxSetting oClsRole = new ClsTaxSetting()
                {
                    TaxSettingId = obj.TaxSettingId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTaxSetting.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TaxSettingId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Setting",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Setting \"" + oConnectionContext.DbClsTaxSetting.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.TaxSettingId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Setting deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveTaxSettings(ClsTaxSettingVm obj)
        {
            var det = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                TaxSettingId = a.TaxSettingId,
                a.BusinessRegistrationNo,
                a.BusinessRegistrationNameId
            }).OrderBy(a=>a.BusinessRegistrationNo).ToList();

            if (obj.BusinessRegistrationNameId != 0)
            {
                det.Where(a => a.BusinessRegistrationNameId == obj.BusinessRegistrationNameId).ToList();
            }

            if (obj.BusinessRegistrationNo != "" && obj.BusinessRegistrationNo != null)
            {
                det.Where(a => a.BusinessRegistrationNo.ToLower() == obj.BusinessRegistrationNo.ToLower()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
