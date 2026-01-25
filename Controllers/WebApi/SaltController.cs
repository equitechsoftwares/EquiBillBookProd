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
    public class SaltController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSalts(ClsSaltVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSalt.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                SaltId = a.SaltId,
                SaltName = a.SaltName,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                Indication = a.Indication,
                Dosage = a.Dosage,
                SideEffects = a.SideEffects,
                SpecialPrecautions = a.SpecialPrecautions,
                DrugInteractions = a.DrugInteractions,
                Notes = a.Notes,
                TBItem = a.TBItem,
                IsNarcotic = a.IsNarcotic,
                IsScheduleH = a.IsScheduleH,
                IsScheduleH1 = a.IsScheduleH1,
                IsDiscontinued = a.IsDiscontinued,
                IsProhibited = a.IsProhibited,

            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SaltName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Salts = det.OrderByDescending(a => a.SaltId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Salt(ClsSalt obj)
        {
            if(obj.SaltId == 0 && obj.SaltName != "" && obj.SaltName != null)
            {
                obj.SaltId = oConnectionContext.DbClsSalt.Where(a=>a.SaltName.ToLower() == obj.SaltName.ToLower()).Select(a=>a.SaltId).FirstOrDefault();
            }

            var det = oConnectionContext.DbClsSalt.Where(a => a.SaltId == obj.SaltId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SaltId = a.SaltId,
                SaltName = a.SaltName,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                Indication = a.Indication,
                Dosage = a.Dosage,
                SideEffects = a.SideEffects,
                SpecialPrecautions = a.SpecialPrecautions,
                DrugInteractions = a.DrugInteractions,
                Notes = a.Notes,
                TBItem = a.TBItem,
                IsNarcotic = a.IsNarcotic,
                IsScheduleH = a.IsScheduleH,
                IsScheduleH1 = a.IsScheduleH1,
                IsDiscontinued = a.IsDiscontinued,
                IsProhibited = a.IsProhibited,

            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Salt = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSalt(ClsSaltVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                long PrefixUserMapId = 0;
                if (obj.SaltName == null || obj.SaltName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSaltName" });
                    isError = true;
                }

                if (obj.SaltName != null && obj.SaltName != "")
                {
                    if (oConnectionContext.DbClsSalt.Where(a => a.SaltName.ToLower() == obj.SaltName.ToLower()
                    && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Salt Name exists", Id = "divSaltName" });
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

                ClsSalt oSalt = new ClsSalt()
                {
                    SaltName = obj.SaltName,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Indication = obj.Indication,
                    Dosage = obj.Dosage,
                    SideEffects = obj.SideEffects,
                    SpecialPrecautions = obj.SpecialPrecautions,
                    DrugInteractions = obj.DrugInteractions,
                    Notes = obj.Notes,
                    TBItem = obj.TBItem,
                    IsNarcotic = obj.IsNarcotic,
                    IsScheduleH = obj.IsScheduleH,
                    IsScheduleH1 = obj.IsScheduleH1,
                    IsDiscontinued = obj.IsDiscontinued,
                    IsProhibited = obj.IsProhibited,

                };
                oConnectionContext.DbClsSalt.Add(oSalt);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Salt",
                    CompanyId = obj.CompanyId,
                    Description = "Salt \"" + obj.SaltName + "\" created",
                    Id = oSalt.SaltId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Salt created successfully",
                    Data = new
                    {
                        Salt = oSalt
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalt(ClsSaltVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaltName == null || obj.SaltName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSaltName" });
                    isError = true;
                }

                if (obj.SaltName != null && obj.SaltName != "")
                {
                    if (oConnectionContext.DbClsSalt.Where(a => a.SaltName.ToLower() == obj.SaltName.ToLower()
                    && a.SaltId != obj.SaltId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Salt Name exists", Id = "divSaltName" });
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

                ClsSalt oSalt = new ClsSalt()
                {
                    SaltId = obj.SaltId,
                    SaltName = obj.SaltName,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Indication = obj.Indication,
                    Dosage = obj.Dosage,
                    SideEffects = obj.SideEffects,
                    SpecialPrecautions = obj.SpecialPrecautions,
                    DrugInteractions = obj.DrugInteractions,
                    Notes = obj.Notes,
                    TBItem = obj.TBItem,
                    IsNarcotic = obj.IsNarcotic,
                    IsScheduleH = obj.IsScheduleH,
                    IsScheduleH1 = obj.IsScheduleH1,
                    IsDiscontinued = obj.IsDiscontinued,
                    IsProhibited = obj.IsProhibited,

                };
                oConnectionContext.DbClsSalt.Attach(oSalt);
                oConnectionContext.Entry(oSalt).Property(x => x.SaltId).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.SaltName).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.Indication).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.Dosage).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.SideEffects).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.SpecialPrecautions).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.DrugInteractions).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.TBItem).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.IsNarcotic).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.IsScheduleH).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.IsScheduleH1).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.IsDiscontinued).IsModified = true;
                oConnectionContext.Entry(oSalt).Property(x => x.IsProhibited).IsModified = true;

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Salt",
                    CompanyId = obj.CompanyId,
                    Description = "Salt \"" + obj.SaltName + "\" updated",
                    Id = oSalt.SaltId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Salt updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SaltActiveInactive(ClsSaltVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSalt oClsRole = new ClsSalt()
                {
                    SaltId = obj.SaltId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalt.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SaltId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Salt",
                    CompanyId = obj.CompanyId,
                    Description = "Salt \"" + oConnectionContext.DbClsSalt.Where(a => a.SaltId == obj.SaltId).Select(a => a.SaltName).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.SaltId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Salt " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SaltDelete(ClsSaltVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.SaltId == obj.SaltId && a.IsDeleted == false).Count();
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
                ClsSalt oClsRole = new ClsSalt()
                {
                    SaltId = obj.SaltId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalt.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SaltId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Salt",
                    CompanyId = obj.CompanyId,
                    Description = "Salt \"" + oConnectionContext.DbClsSalt.Where(a => a.SaltId == obj.SaltId).Select(a => a.SaltName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.SaltId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Salt deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSalts(ClsSaltVm obj)
        {
            var det = oConnectionContext.DbClsSalt.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                SaltId = a.SaltId,
                SaltName = a.SaltName,
            }).OrderBy(a => a.SaltName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Salts = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SaltAutocomplete(ClsSaltVm obj)
        {
            var det = oConnectionContext.DbClsSalt.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.SaltName.ToLower().Contains(obj.Search.ToLower())).Select(a => a.SaltName).ToList().Take(10);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SaltsArray = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
