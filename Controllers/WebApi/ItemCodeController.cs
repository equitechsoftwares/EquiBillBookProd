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
    public class ItemCodeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllItemCodes(ClsItemCodeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsItemCode.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.TaxPreferenceId,
                a.TaxExemptionId,
                IntraStateTax = oConnectionContext.DbClsTax.Where(b=>b.TaxId == a.IntraStateTaxId).Select(b=>b.Tax).FirstOrDefault(),
                InterStateTax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.InterStateTaxId).Select(b => b.Tax).FirstOrDefault(),
                a.Description,
                a.InterStateTaxId,
                a.IntraStateTaxId,
                a.ItemCodeType,
                ItemCodeId = a.ItemCodeId,
                Code = a.Code,
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

            if (obj.ItemCodeType != "" && obj.ItemCodeType != null)
            {
                det = det.Where(a => a.ItemCodeType == obj.ItemCodeType).ToList();
            }

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Code.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemCodes = det.OrderByDescending(a => a.ItemCodeId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemCode(ClsItemCode obj)
        {
            var det = oConnectionContext.DbClsItemCode.Where(a => a.ItemCodeId == obj.ItemCodeId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                TaxPreference = oConnectionContext.DbClsTax.Where(b=>b.TaxId == a.TaxPreferenceId && b.CompanyId == obj.CompanyId).Select(b=>b.Tax).FirstOrDefault(),
                a.TaxPreferenceId,
                a.TaxExemptionId,
                IntraStateTaxPercentage = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.IntraStateTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                InterStateTaxPercentage = oConnectionContext.DbClsTax.Where(b=>b.TaxId == a.InterStateTaxId).Select(b=>b.TaxPercent).FirstOrDefault(),                
                a.Description,
                a.InterStateTaxId,
                a.IntraStateTaxId,
                a.ItemCodeType,
                ItemCodeId = a.ItemCodeId,
                Code = a.Code,
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
                    ItemCode = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertItemCode(ClsItemCodeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Code == null || obj.Code == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCode" });
                    isError = true;
                }

                if (oConnectionContext.DbClsItemCode.Where(a => a.Code.ToLower() == obj.Code.ToLower() && a.CompanyId == obj.CompanyId
                && a.IsDeleted == false && a.ItemCodeType == obj.ItemCodeType).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate " +obj.ItemCodeType.ToUpper()+" Code exists", Id = "divCode" });
                    isError = true;
                }

                if(obj.TaxPreference == "Taxable")
                {
                    if (obj.IntraStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divIntraStateTax" });
                        isError = true;
                    }

                    if (obj.InterStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divInterStateTax" });
                        isError = true;
                    }
                }
                else if(obj.TaxPreference == "Non-Taxable")
                {
                    if(obj.TaxExemptionId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTaxExemption" });
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

                ClsItemCode oItemCode = new ClsItemCode()
                {
                    TaxExemptionId = obj.TaxExemptionId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    Code = obj.Code,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Description= obj.Description,
                    IntraStateTaxId = obj.IntraStateTaxId,
                    InterStateTaxId = obj.InterStateTaxId,
                    ItemCodeType = obj.ItemCodeType,
                };
                oConnectionContext.DbClsItemCode.Add(oItemCode);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Item Code",
                    CompanyId = obj.CompanyId,
                    Description = obj.ItemCodeType.ToUpper() + " Code \"" + obj.Code + "\" created",
                    Id = oItemCode.ItemCodeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = obj.ItemCodeType.ToUpper()+" Code created successfully",
                    Data = new
                    {
                        ItemCode = oItemCode
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateItemCode(ClsItemCodeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Code == null || obj.Code == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCode" });
                    isError = true;
                }

                if (oConnectionContext.DbClsItemCode.Where(a => a.Code.ToLower() == obj.Code.ToLower() && a.ItemCodeId != obj.ItemCodeId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.ItemCodeType == obj.ItemCodeType).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate " + obj.ItemCodeType.ToUpper() + " Code exists", Id = "divCode" });
                    isError = true;
                }

                if (obj.TaxPreference == "Taxable")
                {
                    if (obj.IntraStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divIntraStateTax" });
                        isError = true;
                    }

                    if (obj.InterStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divInterStateTax" });
                        isError = true;
                    }
                }
                else if (obj.TaxPreference == "Non-Taxable")
                {
                    if (obj.TaxExemptionId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTaxExemption" });
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

                ClsItemCode oItemCode = new ClsItemCode()
                {
                    ItemCodeId = obj.ItemCodeId,
                    TaxExemptionId = obj.TaxExemptionId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    Code = obj.Code,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Description = obj.Description,
                    IntraStateTaxId = obj.IntraStateTaxId,
                    InterStateTaxId = obj.InterStateTaxId,
                    //Type = obj.Type,
                };
                oConnectionContext.DbClsItemCode.Attach(oItemCode);
                oConnectionContext.Entry(oItemCode).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.TaxPreferenceId).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.Code).IsModified = true;
                //oConnectionContext.Entry(oItemCode).Property(x => x.Days).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.IntraStateTaxId).IsModified = true;
                oConnectionContext.Entry(oItemCode).Property(x => x.InterStateTaxId).IsModified = true;
                //oConnectionContext.Entry(oItemCode).Property(x => x.Type).IsModified = true;
                oConnectionContext.SaveChanges();

                if(obj.IsUpdate == true)
                {
                    string query = "update \"tblItem\" set \"TaxExemptionId\"=" + obj.TaxExemptionId + ",\"TaxPreferenceId\"='" + obj.TaxPreferenceId + "',\"TaxId\"=" + obj.IntraStateTaxId + ",\"InterStateTaxId\"=" + obj.InterStateTaxId + " where \"ItemCodeId\"=" + obj.ItemCodeId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Item Code",
                    CompanyId = obj.CompanyId,
                    Description = obj.ItemCodeType.ToUpper() + " Code \"" + obj.Code + "\" updated",
                    Id = oItemCode.ItemCodeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = obj.ItemCodeType.ToUpper() + " Code updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemCodeActiveInactive(ClsItemCodeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsItemCode oClsRole = new ClsItemCode()
                {
                    ItemCodeId = obj.ItemCodeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsItemCode.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ItemCodeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var ItemCode = oConnectionContext.DbClsItemCode.Where(a => a.ItemCodeId == obj.ItemCodeId).Select(a => new { a.Code, a.ItemCodeType }).FirstOrDefault();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Item Code",
                    CompanyId = obj.CompanyId,
                    Description = ItemCode.ItemCodeType.ToUpper() + " Code \"" + ItemCode.Code + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.ItemCodeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = ItemCode.ItemCodeType.ToUpper() +" Code " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemCodeDelete(ClsItemCodeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.ItemCodeId == obj.ItemCodeId).Count();

                if (ItemCount > 0)
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
                ClsItemCode oClsRole = new ClsItemCode()
                {
                    ItemCodeId = obj.ItemCodeId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsItemCode.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ItemCodeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var ItemCode = oConnectionContext.DbClsItemCode.Where(a => a.ItemCodeId == obj.ItemCodeId).Select(a => new { a.Code, a.ItemCodeType }).FirstOrDefault();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Item Code",
                    CompanyId = obj.CompanyId,
                    Description = ItemCode.ItemCodeType.ToUpper() + " Code \"" + ItemCode.Code + "\" deleted",
                    Id = oClsRole.ItemCodeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = ItemCode.ItemCodeType.ToUpper() + " Code deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveItemCodes(ClsItemCodeVm obj)
        {
            var det = oConnectionContext.DbClsItemCode.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.IsActive == true).Select(a => new ClsItemCodeVm
                 {
                     ItemCodeType = a.ItemCodeType,
                     ItemCodeId = a.ItemCodeId,
                     Code = a.Code,
                     Description = a.Description,
                 }).OrderBy(a=>a.Code).ToList();

            if(obj.ItemCodeType != "" && obj.ItemCodeType != null)
            {
                det = det.Where(a=>a.ItemCodeType.ToLower() == obj.ItemCodeType.ToLower()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemCodes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
