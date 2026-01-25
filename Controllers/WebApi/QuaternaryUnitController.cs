using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class QuaternaryUnitController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllQuaternaryUnits(ClsQuaternaryUnitVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.UnitId,
                UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                a.SecondaryUnitId,
                SecondaryUnitName = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                a.TertiaryUnitId,
                TertiaryUnitName = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnitId = a.QuaternaryUnitId,
                a.QuaternaryUnitCode,
                QuaternaryUnitName = a.QuaternaryUnitName,
                a.QuaternaryUnitAllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.QuaternaryUnitShortName
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.UnitName.ToLower().Contains(obj.Search.ToLower())
                || a.SecondaryUnitName.ToLower().Contains(obj.Search.ToLower()) ||
                a.TertiaryUnitName.ToLower().Contains(obj.Search.ToLower()) ||
                a.QuaternaryUnitShortName.ToLower().Contains(obj.Search.ToLower()) || a.QuaternaryUnitName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    QuaternaryUnits = det.OrderByDescending(a => a.QuaternaryUnitId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> QuaternaryUnit(ClsQuaternaryUnit obj)
        {
            var det = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.QuaternaryUnitId == obj.QuaternaryUnitId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                QuaternaryUnitId = a.QuaternaryUnitId,
                a.QuaternaryUnitCode,
                QuaternaryUnitName = a.QuaternaryUnitName,
                a.QuaternaryUnitAllowDecimal,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.QuaternaryUnitShortName,
                a.UnitId,
                a.SecondaryUnitId,
                a.TertiaryUnitId
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


            var TertiaryUnits = oConnectionContext.DbClsTertiaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.SecondaryUnitId == det.SecondaryUnitId && a.IsActive == true).Select(a => new
            {
                TertiaryUnitId = a.TertiaryUnitId,
                a.TertiaryUnitCode,
                TertiaryUnitName = a.TertiaryUnitName,
            }).OrderBy(a => a.TertiaryUnitName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    QuaternaryUnit = det,
                    Units = Units,
                    SecondaryUnits = SecondaryUnits,
                    TertiaryUnits = TertiaryUnits
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertQuaternaryUnit(ClsQuaternaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.QuaternaryUnitName == null || obj.QuaternaryUnitName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divQuaternaryUnitName" });
                    isError = true;
                }

                if (obj.QuaternaryUnitShortName == null || obj.QuaternaryUnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divQuaternaryUnitShortName" });
                    isError = true;
                }

                if (obj.UnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                    isError = true;
                }

                if (obj.SecondaryUnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnit" });
                    isError = true;
                }

                if (obj.TertiaryUnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTertiaryUnit" });
                    isError = true;
                }

                //if (obj.QuaternaryUnitCode != "" && obj.QuaternaryUnitCode != null)
                //{
                //    if (oConnectionContext.DbClsQuaternaryUnit.Where(a => a.QuaternaryUnitCode == obj.QuaternaryUnitCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate Quaternary Unit code exists",
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
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "QuaternaryUnit"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.QuaternaryUnitCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                if (obj.QuaternaryUnitName != null && obj.QuaternaryUnitName != "")
                {
                    if (oConnectionContext.DbClsQuaternaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitId == obj.TertiaryUnitId && a.QuaternaryUnitName.ToLower() == obj.QuaternaryUnitName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Quaternary Unit Name exists", Id = "divQuaternaryUnitName" });
                        isError = true;
                    }
                }

                if (obj.QuaternaryUnitShortName != null && obj.QuaternaryUnitShortName != "")
                {
                    if (oConnectionContext.DbClsQuaternaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitId == obj.TertiaryUnitId && a.QuaternaryUnitShortName.ToLower() == obj.QuaternaryUnitShortName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Quaternary Unit Short Name exists", Id = "divQuaternaryUnitShortName" });
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

                ClsQuaternaryUnit oQuaternaryUnit = new ClsQuaternaryUnit()
                {
                    QuaternaryUnitName = obj.QuaternaryUnitName,
                    QuaternaryUnitShortName = obj.QuaternaryUnitShortName,
                    QuaternaryUnitCode = obj.QuaternaryUnitCode,
                    QuaternaryUnitAllowDecimal = obj.QuaternaryUnitAllowDecimal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    UnitId = obj.UnitId,
                    SecondaryUnitId = obj.SecondaryUnitId,
                    TertiaryUnitId = obj.TertiaryUnitId
                };
                oConnectionContext.DbClsQuaternaryUnit.Add(oQuaternaryUnit);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Quaternary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Quaternary Unit \"" + obj.QuaternaryUnitName + "\" created",
                    Id = oQuaternaryUnit.QuaternaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Quaternary Unit created successfully",
                    Data = new
                    {
                        QuaternaryUnit = oQuaternaryUnit
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateQuaternaryUnit(ClsQuaternaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.QuaternaryUnitName == null || obj.QuaternaryUnitName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divQuaternaryUnitName" });
                    isError = true;
                }

                if (obj.QuaternaryUnitShortName == null || obj.QuaternaryUnitShortName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divQuaternaryUnitShortName" });
                    isError = true;
                }

                if (obj.UnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                    isError = true;
                }

                if (obj.SecondaryUnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSecondaryUnit" });
                    isError = true;
                }

                if (obj.TertiaryUnitId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTertiaryUnit" });
                    isError = true;
                }

                if (obj.QuaternaryUnitName != null && obj.QuaternaryUnitName != "")
                {
                    if (oConnectionContext.DbClsQuaternaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitId == obj.TertiaryUnitId && a.QuaternaryUnitName.ToLower() == obj.QuaternaryUnitName.ToLower() && a.QuaternaryUnitId != obj.QuaternaryUnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Quaternary Unit Name exists", Id = "divQuaternaryUnitName" });
                        isError = true;
                    }
                }

                if (obj.QuaternaryUnitShortName != null && obj.QuaternaryUnitShortName != "")
                {
                    if (oConnectionContext.DbClsQuaternaryUnit.Where(a => a.UnitId == obj.UnitId && a.SecondaryUnitId == obj.SecondaryUnitId && a.TertiaryUnitId == obj.TertiaryUnitId && a.QuaternaryUnitShortName.ToLower() == obj.QuaternaryUnitShortName.ToLower() && a.QuaternaryUnitId != obj.QuaternaryUnitId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Quaternary Unit Short Name exists", Id = "divQuaternaryUnitShortName" });
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

                ClsQuaternaryUnit oQuaternaryUnit = new ClsQuaternaryUnit()
                {
                    QuaternaryUnitId = obj.QuaternaryUnitId,
                    QuaternaryUnitCode = obj.QuaternaryUnitCode,
                    QuaternaryUnitName = obj.QuaternaryUnitName,
                    QuaternaryUnitShortName = obj.QuaternaryUnitShortName,
                    QuaternaryUnitAllowDecimal = obj.QuaternaryUnitAllowDecimal,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    UnitId = obj.UnitId,
                    SecondaryUnitId = obj.SecondaryUnitId,
                    TertiaryUnitId = obj.TertiaryUnitId
                };
                oConnectionContext.DbClsQuaternaryUnit.Attach(oQuaternaryUnit);
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.QuaternaryUnitId).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.QuaternaryUnitName).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.QuaternaryUnitAllowDecimal).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.QuaternaryUnitShortName).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.QuaternaryUnitCode).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.SecondaryUnitId).IsModified = true;
                oConnectionContext.Entry(oQuaternaryUnit).Property(x => x.TertiaryUnitId).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Quaternary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Quaternary Unit \"" + obj.QuaternaryUnitName + "\" updated",
                    Id = oQuaternaryUnit.QuaternaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Quaternary Unit updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> QuaternaryUnitActiveInactive(ClsQuaternaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsQuaternaryUnit oClsRole = new ClsQuaternaryUnit()
                {
                    QuaternaryUnitId = obj.QuaternaryUnitId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsQuaternaryUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.QuaternaryUnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Quaternary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Quaternary Unit \"" + oConnectionContext.DbClsQuaternaryUnit.Where(a => a.QuaternaryUnitId == obj.QuaternaryUnitId).Select(a => a.QuaternaryUnitName).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.QuaternaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Quaternary Unit " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> QuaternaryUnitDelete(ClsQuaternaryUnitVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.QuaternaryUnitId == obj.QuaternaryUnitId && a.IsDeleted == false).Count();
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
                ClsQuaternaryUnit oClsRole = new ClsQuaternaryUnit()
                {
                    QuaternaryUnitId = obj.QuaternaryUnitId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsQuaternaryUnit.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.QuaternaryUnitId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Quaternary Units",
                    CompanyId = obj.CompanyId,
                    Description = "Quaternary Unit \"" + oConnectionContext.DbClsQuaternaryUnit.Where(a => a.QuaternaryUnitId == obj.QuaternaryUnitId).Select(a => a.QuaternaryUnitName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.QuaternaryUnitId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Quaternary Unit deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveQuaternaryUnits(ClsQuaternaryUnitVm obj)
        {
            var det = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.TertiaryUnitId == obj.TertiaryUnitId).Select(a => new
            {
                QuaternaryUnitId = a.QuaternaryUnitId,
                a.QuaternaryUnitCode,
                QuaternaryUnitName = a.QuaternaryUnitName,
                a.QuaternaryUnitShortName
            }).OrderBy(a => a.QuaternaryUnitName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    QuaternaryUnits = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
