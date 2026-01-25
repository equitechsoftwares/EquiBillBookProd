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
    public class UnitController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllUnits(ClsUnitVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsPredefined == false).Select(a => new
            {
                UnitId = a.UnitId,
                a.UnitCode,
                UnitName = a.UnitName,
                a.AllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.UnitShortName
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.UnitName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Units = det.OrderByDescending(a => a.UnitId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Unit(ClsUnit obj)
        {
            var det = oConnectionContext.DbClsUnit.Where(a => a.UnitId == obj.UnitId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                UnitId = a.UnitId,
                a.UnitCode,
                UnitName = a.UnitName,
                a.AllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.UnitShortName
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Unit = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertUnit(ClsUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.UnitName == null || obj.UnitName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnitName" });
                    isError = true;
                }

                if (obj.UnitShortName == null || obj.UnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnitShortName" });
                    isError = true;
                }

                //if (obj.UnitCode != "" && obj.UnitCode != null)
                //{
                //    if (oConnectionContext.DbClsUnit.Where(a => a.UnitCode == obj.UnitCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Unit Code exists",
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
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Unit"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.UnitCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                if (obj.UnitName != null && obj.UnitName != "")
                {
                    if (oConnectionContext.DbClsUnit.Where(a => a.UnitName.ToLower() == obj.UnitName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Unit Name exists", Id = "divUnitName" });
                        isError = true;
                    }
                }

                if (obj.UnitShortName != null && obj.UnitShortName != "")
                {
                    if (oConnectionContext.DbClsUnit.Where(a => a.UnitShortName.ToLower() == obj.UnitShortName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Unit Short Name exists", Id = "divUnitShortName" });
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

                ClsUnit oUnit = new ClsUnit()
                {
                    UnitName = obj.UnitName,
                    UnitShortName = obj.UnitShortName,
                    UnitCode = obj.UnitCode,
                    AllowDecimal = obj.AllowDecimal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsPredefined = false
                };
                oConnectionContext.DbClsUnit.Add(oUnit);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Units",
                    CompanyId = obj.CompanyId,
                    Description = "Unit \"" + obj.UnitName+"\" created",
                    Id = oUnit.UnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Unit created successfully",
                    Data = new
                    {
                        Unit = oUnit
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateUnit(ClsUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.UnitName == null || obj.UnitName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnitName" });
                    isError = true;
                }

                if (obj.UnitShortName == null || obj.UnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnitShortName" });
                    isError = true;
                }

                //if (obj.UnitCode != "" && obj.UnitCode != null)
                //{
                //    if (oConnectionContext.DbClsUnit.Where(a => a.UnitCode == obj.UnitCode && a.UnitId != obj.UnitId && a.CompanyId == obj.CompanyId).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Unit code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}

                if (obj.UnitName != "" && obj.UnitName != null)
                {
                    if (oConnectionContext.DbClsUnit.Where(a => a.UnitName.ToLower() == obj.UnitName.ToLower() && a.UnitId != obj.UnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Unit Name exists", Id = "divUnitName" });
                        isError = true;
                    }
                }

                if (obj.UnitShortName != "" && obj.UnitShortName != null)
                {
                    if (oConnectionContext.DbClsUnit.Where(a => a.UnitShortName.ToLower() == obj.UnitShortName.ToLower() && a.UnitId != obj.UnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Unit Short Name exists", Id = "divUnitShortName" });
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

                ClsUnit oUnit = new ClsUnit()
                {
                    UnitId = obj.UnitId,
                    UnitCode = obj.UnitCode,
                    UnitName = obj.UnitName,
                    UnitShortName = obj.UnitShortName,
                    AllowDecimal = obj.AllowDecimal,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUnit.Attach(oUnit);
                oConnectionContext.Entry(oUnit).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oUnit).Property(x => x.UnitName).IsModified = true;
                oConnectionContext.Entry(oUnit).Property(x => x.AllowDecimal).IsModified = true;
                oConnectionContext.Entry(oUnit).Property(x => x.UnitShortName).IsModified = true;
                oConnectionContext.Entry(oUnit).Property(x => x.UnitCode).IsModified = true;
                oConnectionContext.Entry(oUnit).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oUnit).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Units",
                    CompanyId = obj.CompanyId,
                    Description = "Unit \"" + obj.UnitName+"\" updated",
                    Id = oUnit.UnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Unit updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UnitActiveInactive(ClsUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsUnit oClsRole = new ClsUnit()
                {
                    UnitId = obj.UnitId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Units",
                    CompanyId = obj.CompanyId,
                    Description = "Unit \""+oConnectionContext.DbClsUnit.Where(a => a.UnitId == obj.UnitId).Select(a => a.UnitName).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsRole.UnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Unit " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UnitDelete(ClsUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int SecondaryUnitCount = oConnectionContext.DbClsSecondaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.UnitId == obj.UnitId && a.IsDeleted == false).Count();
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.UnitId == obj.UnitId && a.IsDeleted == false).Count();
                if (SecondaryUnitCount > 0 || ItemCount > 0)
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
                ClsUnit oClsRole = new ClsUnit()
                {
                    UnitId = obj.UnitId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Units",
                    CompanyId = obj.CompanyId,
                    Description = "Unit \"" + oConnectionContext.DbClsUnit.Where(a => a.UnitId == obj.UnitId).Select(a => a.UnitName).FirstOrDefault()+"\" deleted",
                    Id = oClsRole.UnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Unit deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveUnits(ClsUnitVm obj)
        {
            List<ClsUnitVm> det= null;
            if(obj.UnitType == "all")
            {
                det = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                && a.IsActive == true).Select(a => new ClsUnitVm
            {
                UnitId = a.UnitId,
                UnitCode=a.UnitCode,
                UnitName = a.UnitName,
                UnitShortName = a.UnitShortName,
                IsPredefined = a.IsPredefined
            }).OrderBy(a => a.UnitName).ToList();
            }
            else if(obj.UnitType == "predefined")
            {
                det = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.IsPredefined == true).Select(a => new ClsUnitVm
            {
                UnitId = a.UnitId,
                UnitCode=a.UnitCode,
                UnitName = a.UnitName,
                UnitShortName=a.UnitShortName,
                IsPredefined = a.IsPredefined
            }).OrderBy(a => a.UnitName).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.IsPredefined == false).Select(a => new ClsUnitVm
            {
                UnitId = a.UnitId,
                UnitCode=a.UnitCode,
                UnitName = a.UnitName,
                UnitShortName = a.UnitShortName,
                IsPredefined = a.IsPredefined
            }).OrderBy(a => a.UnitName).ToList();
            }
            
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Units = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
