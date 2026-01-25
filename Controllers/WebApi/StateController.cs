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
    public class StateController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllStates(ClsStateVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsState.Where(a => 
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false).Select(a => new
            {
                StateId = a.StateId,
                a.CountryId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                a.StateCode,
                State = a.State,
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
                det = det.Where(a => a.State.ToLower().Contains(obj.Search.ToLower()) || a.StateCode.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    States = det.OrderByDescending(a => a.StateId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> State(ClsState obj)
        {
            var det = oConnectionContext.DbClsState.Where(a => a.StateId == obj.StateId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                a.CountryId,
                State = a.State,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).FirstOrDefault();

            var Countrys = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
            }).OrderBy(a => a.Country).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    State = det,
                    Countrys = Countrys
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertState(ClsStateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                long PrefixUserMapId = 0;
                //obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                if (obj.CountryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCountry" });
                    isError = true;
                }

                if (obj.State == null || obj.State == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }

                //if (obj.StateCode != "" && obj.StateCode != null)
                //{
                //    if (oConnectionContext.DbClsState.Where(a => a.StateCode.ToLower() == obj.StateCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate State Code exists", Id = "divStateCode" });
                //        isError = true;
                //    }
                //}

                if (obj.State != null && obj.State == "" && obj.CountryId != 0)
                {
                    if (oConnectionContext.DbClsState.Where(a => a.State.ToLower() == obj.State.ToLower() && a.CountryId == obj.CountryId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate State Name exists", Id = "divState" });
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

                //if (obj.StateCode == "" || obj.StateCode == null)
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "State"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.StateCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsState oState = new ClsState()
                {
                    State = obj.State,
                    StateCode = obj.StateCode,
                    CountryId = obj.CountryId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsState.Add(oState);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "State",
                    CompanyId = obj.CompanyId,
                    Description = "State \"" + obj.State +"\" created",
                    Id = oState.StateId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "State created successfully",
                    Data = new
                    {
                        State = oState
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateState(ClsStateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                //obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                if (obj.CountryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCountry" });
                    isError = true;
                }

                if (obj.State == null || obj.State == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }

                if (obj.State != null && obj.State == "" && obj.CountryId != 0)
                {
                    if (oConnectionContext.DbClsState.Where(a => a.State.ToLower() == obj.State.ToLower() && a.CountryId == obj.CountryId && a.StateId != obj.StateId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate State Name exists", Id = "divState" });
                        isError = true;
                    }
                }

                //if (obj.StateCode != "" && obj.StateCode != null)
                //{
                //    if (oConnectionContext.DbClsState.Where(a => a.StateCode == obj.StateCode && a.StateId != obj.StateId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Duplicate State Code exists",
                //            Data = new
                //            {
                //            }
                //        }; return await Task.FromResult(Ok(data));
                //    }
                //}

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

                ClsState oState = new ClsState()
                {
                    StateId = obj.StateId,
                    //StateCode = obj.StateCode,
                    CountryId = obj.CountryId,
                    State = obj.State,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsState.Attach(oState);
                oConnectionContext.Entry(oState).Property(x => x.StateId).IsModified = true;
                oConnectionContext.Entry(oState).Property(x => x.State).IsModified = true;
                //oConnectionContext.Entry(oState).Property(x => x.StateCode).IsModified = true;
                oConnectionContext.Entry(oState).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oState).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oState).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "State",
                    CompanyId = obj.CompanyId,
                    Description = "State \"" + oConnectionContext.DbClsState.Where(a => a.StateId == obj.StateId).Select(a => a.State).FirstOrDefault()+"\" updated",
                    Id = oState.StateId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "State updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StateActiveInactive(ClsStateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsState oClsRole = new ClsState()
                {
                    StateId = obj.StateId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsState.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.StateId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "State",
                    CompanyId = obj.CompanyId,
                    Description = "State \"" + oConnectionContext.DbClsState.Where(a => a.StateId == obj.StateId).Select(a => a.State).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.StateId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "State " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StateDelete(ClsStateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int UserCount = (from a in oConnectionContext.DbClsUser
                                          join b in oConnectionContext.DbClsAddress
                                          on a.UserId equals b.UserId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.StateId == obj.StateId
                                          select a.UserId).Count();

                int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.StateId == obj.StateId && a.IsDeleted == false).Count();
                
                if (UserCount > 0 || BranchCount > 0)
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
                ClsState oClsRole = new ClsState()
                {
                    StateId = obj.StateId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsState.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.StateId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "State",
                    CompanyId = obj.CompanyId,
                    Description = "State \"" + oConnectionContext.DbClsState.Where(a => a.StateId == obj.StateId).Select(a => a.State).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.StateId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "State deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveStates(ClsStateVm obj)
        {
            if(obj.ShowAllStates == false)
            {
                if (obj.CountryId == 0)
                {
                    obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                }

                var det = oConnectionContext.DbClsState.Where(a =>
                //a.CompanyId == obj.CompanyId && 
                a.IsDeleted == false && a.IsActive == true
                && a.CountryId == obj.CountryId
                ).Select(a => new
                {
                    a.CountryId,
                    StateId = a.StateId,
                    a.StateCode,
                    State = a.State,
                }).OrderBy(a => a.State).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        States = det,
                    }
                };
            }
            else
            {
                var det = oConnectionContext.DbClsState.Where(a =>
                //a.CompanyId == obj.CompanyId && 
                a.IsDeleted == false && a.IsActive == true
                ).Select(a => new
                {
                    a.CountryId,
                    StateId = a.StateId,
                    a.StateCode,
                    State = a.State,
                }).OrderBy(a => a.State).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        States = det,
                    }
                };
            }
           
            return await Task.FromResult(Ok(data));
        }

    }
}
