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
    public class TertiaryUnitController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllTertiaryUnits(ClsTertiaryUnitVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsTertiaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.UnitId,
                UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                a.SecondaryUnitId,
                SecondaryUnitName = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnitId = a.TertiaryUnitId,
                a.TertiaryUnitCode,
                TertiaryUnitName = a.TertiaryUnitName,
                a.TertiaryUnitAllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.TertiaryUnitShortName
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.UnitName.ToLower().Contains(obj.Search.ToLower())
                || a.SecondaryUnitName.ToLower().Contains(obj.Search.ToLower()) || a.TertiaryUnitShortName.ToLower().Contains(obj.Search.ToLower()) ||
                a.TertiaryUnitName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TertiaryUnits = det.OrderByDescending(a => a.TertiaryUnitId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TertiaryUnit(ClsTertiaryUnit obj)
        {
            var det = oConnectionContext.DbClsTertiaryUnit.Where(a => a.TertiaryUnitId == obj.TertiaryUnitId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                TertiaryUnitId = a.TertiaryUnitId,
                a.TertiaryUnitCode,
                TertiaryUnitName = a.TertiaryUnitName,
                a.TertiaryUnitAllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.TertiaryUnitShortName,
                a.UnitId,
                a.SecondaryUnitId
            }).FirstOrDefault();

            var Units = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                UnitId = a.UnitId,
                a.UnitCode,
                UnitName = a.UnitName,
            }).OrderBy(a => a.UnitName).ToList();

            var SecondaryUnits = oConnectionContext.DbClsSecondaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.UnitId == det.UnitId && a.IsActive == true).Select(a => new
            {
                SecondaryUnitId = a.SecondaryUnitId,
                a.SecondaryUnitCode,
                SecondaryUnitName = a.SecondaryUnitName,
            }).OrderBy(a => a.SecondaryUnitName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TertiaryUnit = det,
                    Units = Units,
                    SecondaryUnits = SecondaryUnits
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTertiaryUnit(ClsTertiaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.TertiaryUnitName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTertiaryUnitName" });
                    isError = true;
                }

                if (obj.TertiaryUnitShortName == null || obj.TertiaryUnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTertiaryUnitShortName" });
                    isError = true;
                }

                if (obj.SecondaryUnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnit" });
                    isError = true;
                }

                if (obj.UnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                    isError = true;
                }

                //if (obj.TertiaryUnitCode != "" && obj.TertiaryUnitCode != null)
                //{
                //    if (oConnectionContext.DbClsTertiaryUnit.Where(a => a.TertiaryUnitCode == obj.TertiaryUnitCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Tertiary Unit code exists",
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
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "TertiaryUnit"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.TertiaryUnitCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                if (obj.TertiaryUnitName != null && obj.TertiaryUnitName != "")
                {
                    if (oConnectionContext.DbClsTertiaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitName.ToLower() == obj.TertiaryUnitName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Tertiary Unit Name exists", Id = "divTertiaryUnitName" });
                        isError = true;
                    }
                }

                if (obj.TertiaryUnitShortName != null && obj.TertiaryUnitShortName != "")
                {
                    if (oConnectionContext.DbClsTertiaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitShortName.ToLower() == obj.TertiaryUnitShortName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Tertiary Unit Short Name exists", Id = "divTertiaryUnitShortName" });
                        isError = true;
                        data = new
                        {
                            Status = 0,
                            Message = "Duplicate Tertiary Unit Short Name exists",
                            Data = new
                            {
                            }
                        }; return await Task.FromResult(Ok(data));
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

                ClsTertiaryUnit oTertiaryUnit = new ClsTertiaryUnit()
                {
                    TertiaryUnitName = obj.TertiaryUnitName,
                    TertiaryUnitShortName = obj.TertiaryUnitShortName,
                    TertiaryUnitCode = obj.TertiaryUnitCode,
                    TertiaryUnitAllowDecimal = obj.TertiaryUnitAllowDecimal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    UnitId = obj.UnitId,
                    SecondaryUnitId = obj.SecondaryUnitId
                };
                oConnectionContext.DbClsTertiaryUnit.Add(oTertiaryUnit);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tertiary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Tertiary Unit \"" + obj.TertiaryUnitName + "\" created",
                    Id = oTertiaryUnit.TertiaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tertiary Unit created successfully",
                    Data = new
                    {
                        TertiaryUnit = oTertiaryUnit
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateTertiaryUnit(ClsTertiaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.TertiaryUnitName == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTertiaryUnitName" });
                    isError = true;
                }

                if (obj.TertiaryUnitShortName == null || obj.TertiaryUnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTertiaryUnitShortName" });
                    isError = true;
                }

                if (obj.SecondaryUnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnit" });
                    isError = true;
                }

                if (obj.UnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                    isError = true;
                }

                if (obj.TertiaryUnitName != "" && obj.TertiaryUnitName != null)
                {
                    if (oConnectionContext.DbClsTertiaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitName.ToLower() == obj.TertiaryUnitName.ToLower() && a.TertiaryUnitId != obj.TertiaryUnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Tertiary Unit Name exists", Id = "divTertiaryUnitName" });
                        isError = true;
                    }
                }

                if (obj.TertiaryUnitShortName != null && obj.TertiaryUnitShortName != "")
                {
                    if (oConnectionContext.DbClsTertiaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitShortName.ToLower() == obj.TertiaryUnitShortName.ToLower() && a.TertiaryUnitId != obj.TertiaryUnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Tertiary Unit Short Name exists", Id = "divTertiaryUnitShortName" });
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

                ClsTertiaryUnit oTertiaryUnit = new ClsTertiaryUnit()
                {
                    TertiaryUnitId = obj.TertiaryUnitId,
                    TertiaryUnitCode = obj.TertiaryUnitCode,
                    TertiaryUnitName = obj.TertiaryUnitName,
                    TertiaryUnitShortName = obj.TertiaryUnitShortName,
                    TertiaryUnitAllowDecimal = obj.TertiaryUnitAllowDecimal,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    UnitId = obj.UnitId,
                    SecondaryUnitId = obj.SecondaryUnitId
                };
                oConnectionContext.DbClsTertiaryUnit.Attach(oTertiaryUnit);
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.TertiaryUnitId).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.TertiaryUnitName).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.TertiaryUnitAllowDecimal).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.TertiaryUnitShortName).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.TertiaryUnitCode).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oTertiaryUnit).Property(x => x.SecondaryUnitId).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tertiary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Tertiary Unit \"" + obj.TertiaryUnitName + "\" updated",
                    Id = oTertiaryUnit.TertiaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tertiary Unit updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TertiaryUnitActiveInactive(ClsTertiaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsTertiaryUnit oClsRole = new ClsTertiaryUnit()
                {
                    TertiaryUnitId = obj.TertiaryUnitId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTertiaryUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TertiaryUnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tertiary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Tertiary Unit \"" + oConnectionContext.DbClsTertiaryUnit.Where(a => a.TertiaryUnitId == obj.TertiaryUnitId).Select(a => a.TertiaryUnitName).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.TertiaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tertiary Unit " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TertiaryUnitDelete(ClsTertiaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int QuaternaryUnitCount = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.TertiaryUnitId == obj.TertiaryUnitId && a.IsDeleted == false).Count();
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.TertiaryUnitId == obj.TertiaryUnitId && a.IsDeleted == false).Count();
                if (QuaternaryUnitCount > 0 || ItemCount > 0)
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
                ClsTertiaryUnit oClsRole = new ClsTertiaryUnit()
                {
                    TertiaryUnitId = obj.TertiaryUnitId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTertiaryUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TertiaryUnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tertiary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Tertiary Unit \"" + oConnectionContext.DbClsTertiaryUnit.Where(a => a.TertiaryUnitId == obj.TertiaryUnitId).Select(a => a.TertiaryUnitName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.TertiaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tertiary Unit deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveTertiaryUnits(ClsTertiaryUnitVm obj)
        {
            var det = oConnectionContext.DbClsTertiaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.SecondaryUnitId == obj.SecondaryUnitId).Select(a => new
            {
                TertiaryUnitId = a.TertiaryUnitId,
                a.TertiaryUnitCode,
                TertiaryUnitName = a.TertiaryUnitName,
                a.TertiaryUnitShortName
            }).OrderBy(a => a.TertiaryUnitName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TertiaryUnits = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
