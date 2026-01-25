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
    public class PrefixController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllPrefixMasters(ClsPrefixMasterVm obj)
        {
            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2
            //&& a.IsActive == true
            ).Select(a => a.TransactionId).FirstOrDefault();

            var PlanAddons = (from aa in oConnectionContext.DbClsTransaction
                              join bb in oConnectionContext.DbClsTransactionDetails
  on aa.TransactionId equals bb.TransactionId
                              where aa.TransactionId == TransactionId && aa.Status == 2
                              select new { bb.Type }).Union(from aa in oConnectionContext.DbClsTransaction
                                                            join bb in oConnectionContext.DbClsTransactionDetails
                                on aa.TransactionId equals bb.TransactionId
                                                            where aa.ParentTransactionId == TransactionId && aa.Status == 2
                                                            select new { bb.Type }).ToList();

            //var det = oConnectionContext.DbClsPrefixMaster.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
            //).Where(a => PlanAddons.Select(x => x.Type.ToLower()).Contains(a.MenuType.ToLower())
            ////&& a.IsActive == true
            //).Select(a => new
            //{
            //    a.Sequence,
            //    a.PrefixTitle,
            //    a.PrefixMasterId,
            //    a.PrefixType,
            //    PrefixUserMapId = oConnectionContext.DbClsPrefixUserMap.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true &&
            //    b.PrefixMasterId == a.PrefixMasterId && b.PrefixId == obj.PrefixId).Select(b => b.PrefixUserMapId).FirstOrDefault(),
            //    Prefix = oConnectionContext.DbClsPrefixUserMap.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true &&
            //    b.PrefixMasterId == a.PrefixMasterId && b.PrefixId == obj.PrefixId).Select(b => b.Prefix).FirstOrDefault(),
            //    NoOfDigits = oConnectionContext.DbClsPrefixUserMap.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true &&
            //    b.PrefixMasterId == a.PrefixMasterId && b.PrefixId == obj.PrefixId).Select(b => b.NoOfDigits).FirstOrDefault(),
            //}).OrderBy(a => a.Sequence);

            var planAddonTypes = PlanAddons.Select(x => x.Type.ToLower()).ToList();

            var det = oConnectionContext.DbClsPrefixMaster
                .Where(a => a.IsDeleted == false && a.IsActive == true
                            && planAddonTypes.Contains(a.MenuType.ToLower()))  // 'planAddonTypes' in memory
                .Select(a => new
                {
                    a.Sequence,
                    a.PrefixTitle,
                    a.PrefixMasterId,
                    a.PrefixType,
                    PrefixUserMapId = oConnectionContext.DbClsPrefixUserMap
                        .Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                                    && b.PrefixMasterId == a.PrefixMasterId
                                    && b.PrefixId == obj.PrefixId)
                        .Select(b => b.PrefixUserMapId).FirstOrDefault(),
                    Prefix = oConnectionContext.DbClsPrefixUserMap
                        .Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                                    && b.PrefixMasterId == a.PrefixMasterId
                                    && b.PrefixId == obj.PrefixId)
                        .Select(b => b.Prefix).FirstOrDefault(),
                    NoOfDigits = oConnectionContext.DbClsPrefixUserMap
                        .Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                                    && b.PrefixMasterId == a.PrefixMasterId
                                    && b.PrefixId == obj.PrefixId)
                        .Select(b => b.NoOfDigits).FirstOrDefault(),
                })
                .OrderBy(a => a.Sequence)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PrefixMasters = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllPrefixs(ClsPrefixVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsPrefix.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.PrefixName,
                PrefixId = a.PrefixId,
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
                det = det.Where(a => a.PrefixName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Prefixs = det.OrderByDescending(a => a.PrefixId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Prefix(ClsPrefix obj)
        {
            var det = oConnectionContext.DbClsPrefix.Where(a => a.PrefixId == obj.PrefixId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.PrefixName,
                PrefixId = a.PrefixId,
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
                    Prefix = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPrefix(ClsPrefixVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PrefixName == null || obj.PrefixName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPrefixName" });
                    isError = true;
                }

                if (obj.PrefixName != null && obj.PrefixName != "")
                {
                    if (oConnectionContext.DbClsPrefix.Where(a => a.PrefixName.ToLower() == obj.PrefixName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Prefix Name exists", Id = "divPrefixName" });
                        isError = true;
                    }
                }

                foreach (var item in obj.PrefixUserMaps)
                {
                    if(item.Prefix == "" ||item.Prefix == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "div"+item.Id });
                        isError = true;
                    }

                    if(item.NoOfDigits == 0 )
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "div"+item.Id });
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

                ClsPrefix oPrefix = new ClsPrefix()
                {
                    PrefixName = obj.PrefixName,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsPrefix.Add(oPrefix);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.PrefixUserMaps)
                {
                    ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        IsActive = true,
                        IsDeleted = false,
                        CompanyId = obj.CompanyId,
                        Counter = 1,
                        PrefixMasterId = item.PrefixMasterId,
                        NoOfDigits = item.NoOfDigits,
                        Prefix = item.Prefix,
                        PrefixId = oPrefix.PrefixId,
                    };
                    oConnectionContext.DbClsPrefixUserMap.Add(oClsPrefixUserMap);
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Prefix",
                    CompanyId = obj.CompanyId,
                    Description = "Prefix \"" + obj.PrefixName + "\" created",
                    Id = oPrefix.PrefixId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Prefix created successfully",
                    Data = new
                    {
                        Prefix = oPrefix
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePrefix(ClsPrefixVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PrefixName == null || obj.PrefixName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPrefixName" });
                    isError = true;
                }

                if (obj.PrefixName != null && obj.PrefixName != "")
                {
                    if (oConnectionContext.DbClsPrefix.Where(a => a.PrefixName.ToLower() == obj.PrefixName.ToLower() && a.PrefixId != obj.PrefixId
                    && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Prefix Name exists", Id = "divPrefixName" });
                        isError = true;
                    }
                }

                foreach (var item in obj.PrefixUserMaps)
                {
                    if (item.Prefix == "" || item.Prefix == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "div" + item.Id });
                        isError = true;
                    }

                    if (item.NoOfDigits == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "div" + item.Id });
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

                ClsPrefix oPrefix = new ClsPrefix()
                {
                    PrefixId = obj.PrefixId,
                    PrefixName = obj.PrefixName,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPrefix.Attach(oPrefix);
                oConnectionContext.Entry(oPrefix).Property(x => x.PrefixName).IsModified = true;
                oConnectionContext.Entry(oPrefix).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oPrefix).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                foreach (var item in obj.PrefixUserMaps)
                {
                    if (item.PrefixUserMapId == 0)
                    {
                        ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            IsActive = true,
                            IsDeleted = false,
                            CompanyId = obj.CompanyId,
                            Counter = 1,
                            PrefixMasterId = item.PrefixMasterId,
                            NoOfDigits = item.NoOfDigits,
                            Prefix = item.Prefix,
                            PrefixId = obj.PrefixId
                        };
                        oConnectionContext.DbClsPrefixUserMap.Add(oClsPrefixUserMap);
                        oConnectionContext.SaveChanges();
                    }
                    else
                    {
                        ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                        {
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            PrefixUserMapId = item.PrefixUserMapId,
                            NoOfDigits = item.NoOfDigits,
                            Prefix = item.Prefix,
                        };

                        oConnectionContext.DbClsPrefixUserMap.Attach(oClsPrefixUserMap);
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.NoOfDigits).IsModified = true;
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.Prefix).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Prefix",
                    CompanyId = obj.CompanyId,
                    Description = "Prefix \"" + obj.PrefixName + "\" updated",
                    Id = oPrefix.PrefixId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Prefix updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PrefixActiveInactive(ClsPrefixVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPrefix oClsRole = new ClsPrefix()
                {
                    PrefixId = obj.PrefixId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPrefix.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PrefixId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Prefix",
                    CompanyId = obj.CompanyId,
                    Description = "Prefix \"" + oConnectionContext.DbClsPrefix.Where(a => a.PrefixId == obj.PrefixId).Select(a => a.PrefixName).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.PrefixId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Prefix " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PrefixDelete(ClsPrefixVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int ItemCodeCount = oConnectionContext.DbClsItemCode.Where(a => a.CompanyId == obj.CompanyId && a.PrefixId == obj.PrefixId && a.IsDeleted == false && a.IsCancelled == false).Count();
                //int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.PrefixId == obj.PrefixId && a.IsDeleted == false && a.IsCancelled == false).Count();
                //if (ItemCodeCount > 0 || ItemCount > 0)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "Cannot delete as it is already in use",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPrefix oClsRole = new ClsPrefix()
                {
                    PrefixId = obj.PrefixId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPrefix.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PrefixId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Prefix",
                    CompanyId = obj.CompanyId,
                    Description = "Prefix \"" + oConnectionContext.DbClsPrefix.Where(a => a.PrefixId == obj.PrefixId).Select(a => a.PrefixName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.PrefixId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Prefix deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActivePrefixs(ClsPrefixVm obj)
        {
            var det = oConnectionContext.DbClsPrefix.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                PrefixId = a.PrefixId,
                a.PrefixName,
            }).OrderBy(a => a.PrefixName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Prefixs = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePrefix1(ClsPrefixMasterVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int c = 0;
                foreach (var item in obj.PrefixUserMaps)
                {
                    if (item.PrefixUserMapId == 0)
                    {
                        ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            IsActive = true,
                            IsDeleted = false,
                            CompanyId = obj.CompanyId,
                            Counter = 1,
                            PrefixMasterId = item.PrefixMasterId,
                            NoOfDigits = item.NoOfDigits,
                            Prefix = item.Prefix,
                        };
                        oConnectionContext.DbClsPrefixUserMap.Add(oClsPrefixUserMap);
                        oConnectionContext.SaveChanges();
                    }
                    else
                    {
                        ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                        {
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            PrefixUserMapId = item.PrefixUserMapId,
                            NoOfDigits = item.NoOfDigits,
                            Prefix = item.Prefix,
                        };

                        oConnectionContext.DbClsPrefixUserMap.Attach(oClsPrefixUserMap);
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.NoOfDigits).IsModified = true;
                        oConnectionContext.Entry(oClsPrefixUserMap).Property(x => x.Prefix).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                    c++;
                }
                //);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Prefixes\" updated",
                    Id = 0,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Prefixes updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
