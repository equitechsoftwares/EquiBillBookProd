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
    public class CityController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        [AllowAnonymous]
        public async Task<IHttpActionResult> AllCitys(ClsCityVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                CityId = a.CityId,
                a.CountryId,
                a.StateId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                a.CityCode,
                City = a.City,
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
                det = det.Where(a => a.City.ToLower().Contains(obj.Search.ToLower()) || a.CityCode.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Citys = det.OrderByDescending(a => a.CityId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> City(ClsCity obj)
        {
            var det = oConnectionContext.DbClsCity.Where(a => a.CityId == obj.CityId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                a.CountryId,
                a.StateId,
                City = a.City,
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

            var States = oConnectionContext.DbClsState.Where(a => 
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == det.CountryId
            ).Select(a => new
            {
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
                    City = det,
                    Countrys = Countrys,
                    States = States
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCity(ClsCityVm obj)
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

                if (obj.StateId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }

                if (obj.City == null || obj.City == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCity" });
                    isError = true;
                }

                //if (obj.CityCode != "" && obj.CityCode != null)
                //{
                //    if (oConnectionContext.DbClsCity.Where(a => a.CityCode == obj.CityCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "This field is required", Id = "divCityCode" });
                //        isError = true;
                //    }
                //}

                if (obj.City != null && obj.City == "" && obj.CountryId != 0 && obj.StateId != 0)
                {
                    if (oConnectionContext.DbClsCity.Where(a => a.City.ToLower() == obj.City.ToLower() && a.CountryId == obj.CountryId && a.StateId == obj.StateId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate City Name exists", Id = "divCity" });
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

                //if (obj.CityCode == "" || obj.CityCode == null)
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "City"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.CityCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsCity oCity = new ClsCity()
                {
                    City = obj.City,
                    CityCode = obj.CityCode,
                    CountryId = obj.CountryId,
                    StateId = obj.StateId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsCity.Add(oCity);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "City",
                    CompanyId = obj.CompanyId,
                    Description = "City \"" + obj.City+"\" created",
                    Id = oCity.CityId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "City created successfully",
                    Data = new
                    {
                        City = oCity
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCity(ClsCityVm obj)
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

                if (obj.StateId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }

                if (obj.City == null || obj.City == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCity" });
                    isError = true;
                }

                if (obj.City != null && obj.City == "" && obj.CountryId != 0 && obj.StateId != 0)
                {
                    if (oConnectionContext.DbClsCity.Where(a => a.City.ToLower() == obj.City.ToLower() && a.CityId != obj.CityId && a.CountryId == obj.CountryId
                    && a.StateId == obj.StateId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate City Name exists", Id = "divCity" });
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

                ClsCity oCity = new ClsCity()
                {
                    CityId = obj.CityId,
                    StateId = obj.StateId,
                    //CityCode = obj.CityCode,
                    CountryId = obj.CountryId,
                    City = obj.City,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCity.Attach(oCity);
                oConnectionContext.Entry(oCity).Property(x => x.CityId).IsModified = true;
                oConnectionContext.Entry(oCity).Property(x => x.StateId).IsModified = true;
                oConnectionContext.Entry(oCity).Property(x => x.City).IsModified = true;
                //oConnectionContext.Entry(oCity).Property(x => x.CityCode).IsModified = true;
                oConnectionContext.Entry(oCity).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oCity).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCity).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "City",
                    CompanyId = obj.CompanyId,
                    Description = "City " + oConnectionContext.DbClsCity.Where(a => a.CityId == obj.CityId).Select(a => a.City).FirstOrDefault() + "\" updated",
                    Id = oCity.CityId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "City updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CityActiveInactive(ClsCityVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsCity oClsRole = new ClsCity()
                {
                    CityId = obj.CityId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCity.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.CityId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "City",
                    CompanyId = obj.CompanyId,
                    Description = "City \"" +oConnectionContext.DbClsCity.Where(a => a.CityId == obj.CityId).Select(a => a.City).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.CityId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "City " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CityDelete(ClsCityVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int UserCount = (from a in oConnectionContext.DbClsUser
                                 join b in oConnectionContext.DbClsAddress
                                 on a.UserId equals b.UserId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                  && b.CityId == obj.CityId
                                 select a.UserId).Count();

                int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.CityId == obj.CityId && a.IsDeleted == false).Count();

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
                ClsCity oClsCity = new ClsCity()
                {
                    CityId = obj.CityId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCity.Attach(oClsCity);
                oConnectionContext.Entry(oClsCity).Property(x => x.CityId).IsModified = true;
                oConnectionContext.Entry(oClsCity).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsCity).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsCity).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "City",
                    CompanyId = obj.CompanyId,
                    Description = "City \"" + oConnectionContext.DbClsCity.Where(a => a.CityId == obj.CityId).Select(a => a.City).FirstOrDefault()+"\" deleted",
                    Id = oClsCity.CityId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "City deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveCitys(ClsCityVm obj)
        {
            //var det = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.StateId == obj.StateId).Select(a => new
            //{
            //    CityId = a.CityId,
            //    a.CityCode,
            //    City = a.City,
            //}).OrderBy(a => a.City).ToList();

            if (obj.ShowAllCities == false)
            {
                var det = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.StateId == obj.StateId).Select(a => new
                {
                    a.CountryId,
                    a.StateId,
                    CityId = a.CityId,
                    a.CityCode,
                    City = a.City,
                }).OrderBy(a => a.City).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Citys = det,
                    }
                };
            }
            else
            {
                var det = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
                {
                    a.CountryId,
                    a.StateId,
                    CityId = a.CityId,
                    a.CityCode,
                    City = a.City,
                }).OrderBy(a => a.City).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Citys = det,
                    }
                };
            }
            
            return await Task.FromResult(Ok(data));
        }
    }
}
