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
    public class SecondaryUnitController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSecondaryUnits(ClsSecondaryUnitVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSecondaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.UnitId,
                UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnitId = a.SecondaryUnitId,
                a.SecondaryUnitCode,
                SecondaryUnitName = a.SecondaryUnitName,
                a.SecondaryUnitAllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.SecondaryUnitShortName
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.UnitName.ToLower().Contains(obj.Search.ToLower()) || a.SecondaryUnitShortName.ToLower().Contains(obj.Search.ToLower()) ||
                a.SecondaryUnitName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SecondaryUnits = det.OrderByDescending(a => a.SecondaryUnitId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SecondaryUnit(ClsSecondaryUnit obj)
        {
            var det = oConnectionContext.DbClsSecondaryUnit.Where(a => a.SecondaryUnitId == obj.SecondaryUnitId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SecondaryUnitId = a.SecondaryUnitId,
                a.SecondaryUnitCode,
                SecondaryUnitName = a.SecondaryUnitName,
                a.SecondaryUnitAllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.SecondaryUnitShortName,
                a.UnitId
            }).FirstOrDefault();

            var Units = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                UnitId = a.UnitId,
                a.UnitCode,
                UnitName = a.UnitName,
            }).OrderBy(a => a.UnitName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SecondaryUnit = det,
                    Units = Units
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSecondaryUnit(ClsSecondaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.SecondaryUnitName == null || obj.SecondaryUnitName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnitName" });
                    isError = true;
                }

                if (obj.SecondaryUnitShortName == null || obj.SecondaryUnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnitShortName" });
                    isError = true;
                }

                if (obj.UnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                    isError = true;
                }

                //if (obj.SecondaryUnitCode != "" && obj.SecondaryUnitCode != null)
                //{
                //    if (oConnectionContext.DbClsSecondaryUnit.Where(a => a.SecondaryUnitCode == obj.SecondaryUnitCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Secondary Unit code exists",
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
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "SecondaryUnit"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.SecondaryUnitCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                if (obj.SecondaryUnitName != null && obj.SecondaryUnitName != "")
                {
                    if (oConnectionContext.DbClsSecondaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitName.ToLower() == obj.SecondaryUnitName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Secondary Unit Name exists", Id = "divSecondaryUnitName" });
                        isError = true;
                    }
                }

                if (obj.SecondaryUnitShortName != null && obj.SecondaryUnitShortName != "")
                {
                    if (oConnectionContext.DbClsSecondaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitShortName.ToLower() == obj.SecondaryUnitShortName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Secondary Unit Short Name exists", Id = "divSecondaryUnitShortName" });
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

                ClsSecondaryUnit oSecondaryUnit = new ClsSecondaryUnit()
                {
                    SecondaryUnitName = obj.SecondaryUnitName,
                    SecondaryUnitShortName = obj.SecondaryUnitShortName,
                    SecondaryUnitCode = obj.SecondaryUnitCode,
                    SecondaryUnitAllowDecimal = obj.SecondaryUnitAllowDecimal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    UnitId = obj.UnitId
                };
                oConnectionContext.DbClsSecondaryUnit.Add(oSecondaryUnit);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Secondary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Secondary Unit \"" + obj.SecondaryUnitName + "\" created",
                    Id = oSecondaryUnit.SecondaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Secondary Unit created successfully",
                    Data = new
                    {
                        SecondaryUnit = oSecondaryUnit
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSecondaryUnit(ClsSecondaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SecondaryUnitName == null || obj.SecondaryUnitName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnitName" });
                    isError = true;
                }

                if (obj.SecondaryUnitShortName == null || obj.SecondaryUnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnitShortName" });
                    isError = true;
                }

                if (obj.UnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                    isError = true;
                }

                if (obj.SecondaryUnitName != null && obj.SecondaryUnitName != "")
                {
                    if (oConnectionContext.DbClsSecondaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitName.ToLower() == obj.SecondaryUnitName.ToLower() && a.SecondaryUnitId != obj.SecondaryUnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Secondary Unit Name exists", Id = "divSecondaryUnitName" });
                        isError = true;
                    }
                }

                if (obj.SecondaryUnitShortName != null && obj.SecondaryUnitShortName != "")
                {
                    if (oConnectionContext.DbClsSecondaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitShortName.ToLower() == obj.SecondaryUnitShortName.ToLower() && a.SecondaryUnitId != obj.SecondaryUnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Secondary Unit Short Name exists", Id = "divSecondaryUnitShortName" });
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

                ClsSecondaryUnit oSecondaryUnit = new ClsSecondaryUnit()
                {
                    SecondaryUnitId = obj.SecondaryUnitId,
                    SecondaryUnitCode = obj.SecondaryUnitCode,
                    SecondaryUnitName = obj.SecondaryUnitName,
                    SecondaryUnitShortName = obj.SecondaryUnitShortName,
                    SecondaryUnitAllowDecimal = obj.SecondaryUnitAllowDecimal,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    UnitId = obj.UnitId
                };
                oConnectionContext.DbClsSecondaryUnit.Attach(oSecondaryUnit);
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.SecondaryUnitId).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.SecondaryUnitName).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.SecondaryUnitAllowDecimal).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.SecondaryUnitShortName).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.SecondaryUnitCode).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oSecondaryUnit).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Secondary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Secondary Unit \"" + obj.SecondaryUnitName + "\" updated",
                    Id = oSecondaryUnit.SecondaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Secondary Unit updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SecondaryUnitActiveInactive(ClsSecondaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSecondaryUnit oClsRole = new ClsSecondaryUnit()
                {
                    SecondaryUnitId = obj.SecondaryUnitId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSecondaryUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SecondaryUnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Secondary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Secondary Unit \"" + oConnectionContext.DbClsSecondaryUnit.Where(a => a.SecondaryUnitId == obj.SecondaryUnitId).Select(a => a.SecondaryUnitName).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated"),
                    Id = oClsRole.SecondaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Secondary Unit " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SecondaryUnitDelete(ClsSecondaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int TertiaryUnitCount = oConnectionContext.DbClsTertiaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.SecondaryUnitId == obj.SecondaryUnitId && a.IsDeleted == false).Count();
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.SecondaryUnitId == obj.SecondaryUnitId && a.IsDeleted == false).Count();
                if (TertiaryUnitCount > 0 || ItemCount > 0)
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
                ClsSecondaryUnit oClsRole = new ClsSecondaryUnit()
                {
                    SecondaryUnitId = obj.SecondaryUnitId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSecondaryUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SecondaryUnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Secondary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Secondary Unit \"" + oConnectionContext.DbClsSecondaryUnit.Where(a => a.SecondaryUnitId == obj.SecondaryUnitId).Select(a => a.SecondaryUnitName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.SecondaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Secondary Unit deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSecondaryUnits(ClsSecondaryUnitVm obj)
        {
            var det = oConnectionContext.DbClsSecondaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.UnitId == obj.UnitId).Select(a => new
            {
                SecondaryUnitId = a.SecondaryUnitId,
                a.SecondaryUnitCode,
                SecondaryUnitName = a.SecondaryUnitName,
                a.SecondaryUnitShortName
            }).OrderBy(a => a.SecondaryUnitName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SecondaryUnits = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
