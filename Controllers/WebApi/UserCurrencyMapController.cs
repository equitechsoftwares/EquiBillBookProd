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
    public class UserCurrencyMapController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllCurrencys(ClsCurrencyVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            var det = (from a in oConnectionContext.DbClsUserCurrencyMap
                       join b in oConnectionContext.DbClsCurrency
on a.CurrencyId equals b.CurrencyId
                       where a.CompanyId == obj.CompanyId 
                       //&& a.IsActive == true
                       && a.IsDeleted == false &&
                       b.IsActive == true && 
                       b.IsDeleted == false
                       select new
                       {
                           MainCurrency = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(d => d.IsMain == true && d.CompanyId == obj.CompanyId).Select(d => d.CurrencyId).FirstOrDefault()).Select(c => c.CurrencyCode).FirstOrDefault(),
                           a.IsMain,
                           a.UserCurrencyMapId,
                           a.CurrencyId,
                           b.CurrencyCode,
                           b.CurrencyName,
                           b.CurrencySymbol,
                           a.ExchangeRate,
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
                det = det.Where(a => a.CurrencyName.ToLower().Contains(obj.Search.ToLower()) ||
                a.CurrencyCode.ToLower().Contains(obj.Search.ToLower()) || a.CurrencySymbol.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Currencys = det.OrderByDescending(a => a.UserCurrencyMapId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Currency(ClsUserCurrencyMapVm obj)
        {
            var det = (from a in oConnectionContext.DbClsUserCurrencyMap
                       join b in oConnectionContext.DbClsCurrency
on a.CurrencyId equals b.CurrencyId
                       where a.UserCurrencyMapId == obj.UserCurrencyMapId && a.CompanyId == obj.CompanyId
                       select new
                       {
                           MainCurrency = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(d => d.IsMain == true && d.CompanyId == obj.CompanyId).Select(d => d.CurrencyId).FirstOrDefault()).Select(c => c.CurrencyCode).FirstOrDefault(),
                           a.IsMain,
                           a.IsActive,
                           a.UserCurrencyMapId,
                           a.CurrencyId,
                           b.CurrencyCode,
                           b.CurrencyName,
                           b.CurrencySymbol,
                           a.ExchangeRate
                       }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Currency = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCurrency(ClsUserCurrencyMapVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.CurrencyId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCurrency" });
                    isError = true;
                }

                if (obj.ExchangeRate == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divExchangeRate" });
                    isError = true;
                }

                if (obj.CurrencyId != 0)
                {
                    if (oConnectionContext.DbClsUserCurrencyMap.Where(a => a.CurrencyId == obj.CurrencyId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Currency exists", Id = "divCurrency" });
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

                ClsUserCurrencyMap oCurrency = new ClsUserCurrencyMap()
                {
                    CurrencyId = obj.CurrencyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    ExchangeRate = obj.ExchangeRate,
                    IsMain = false
                };
                oConnectionContext.DbClsUserCurrencyMap.Add(oCurrency);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Currency",
                    CompanyId = obj.CompanyId,
                    Description = "added " + oConnectionContext.DbClsCurrency.Where(a=>a.CurrencyId == obj.CurrencyId).Select(a=>a.CurrencyName).FirstOrDefault(),
                    Id = oCurrency.UserCurrencyMapId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Currency created successfully",
                    Data = new
                    {
                        Currency = (from a in oConnectionContext.DbClsUserCurrencyMap
                                    join b in oConnectionContext.DbClsCurrency
                                    on a.CurrencyId equals b.CurrencyId
                                    where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false &&
                                    b.IsActive == true && b.IsDeleted == false && a.UserCurrencyMapId == oCurrency.UserCurrencyMapId
                                    select new
                                    {
                                        a.IsMain,
                                        a.UserCurrencyMapId,
                                        a.CurrencyId,
                                        b.CurrencyCode,
                                        b.CurrencyName,
                                        b.CurrencySymbol,
                                        a.ExchangeRate
                                    }).FirstOrDefault()
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCurrency(ClsUserCurrencyMapVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.ExchangeRate == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divExchangeRate" });
                    isError = true;
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

                ClsUserCurrencyMap oCurrency = new ClsUserCurrencyMap()
                {
                    UserCurrencyMapId = obj.UserCurrencyMapId,
                    ExchangeRate = obj.ExchangeRate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUserCurrencyMap.Attach(oCurrency);
                oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oCurrency).Property(x => x.ExchangeRate).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Currency",
                    CompanyId = obj.CompanyId,
                    Description = "modified " + oConnectionContext.DbClsCurrency.Where(a => a.CurrencyId == obj.CurrencyId).Select(a => a.CurrencyName).FirstOrDefault(),
                    Id = oCurrency.UserCurrencyMapId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Exchange rates updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CurrencyActiveInactive(ClsUserCurrencyMapVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsUserCurrencyMap oCurrency = new ClsUserCurrencyMap()
                {
                    UserCurrencyMapId = obj.UserCurrencyMapId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUserCurrencyMap.Attach(oCurrency);
                oConnectionContext.Entry(oCurrency).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Currency",
                    CompanyId = obj.CompanyId,
                    Description = (obj.IsActive == true ? "activated " : "deactivated ") + oConnectionContext.DbClsCurrency.Where(a => a.CurrencyId == obj.CurrencyId).Select(a => a.CurrencyName).FirstOrDefault(),
                    Id = oCurrency.UserCurrencyMapId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Currency " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CurrencyDelete(ClsUserCurrencyMapVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsUserCurrencyMap oCurrency = new ClsUserCurrencyMap()
                {
                    UserCurrencyMapId = obj.UserCurrencyMapId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUserCurrencyMap.Attach(oCurrency);
                oConnectionContext.Entry(oCurrency).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Currency",
                    CompanyId = obj.CompanyId,
                    Description = "deleted " + oConnectionContext.DbClsCurrency.Where(a => a.CurrencyId == obj.CurrencyId).Select(a => a.CurrencyName).FirstOrDefault(),
                    Id = oCurrency.UserCurrencyMapId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Currency deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveCurrencys(ClsCurrencyVm obj)
        {
            var det = (from a in oConnectionContext.DbClsUserCurrencyMap
                       join b in oConnectionContext.DbClsCurrency
on a.CurrencyId equals b.CurrencyId
                       where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false &&
                       b.IsActive == true && b.IsDeleted == false
                       select new
                       {
                           a.IsMain,
                           a.UserCurrencyMapId,
                           a.CurrencyId,
                           b.CurrencyCode,
                           b.CurrencyName,
                           b.CurrencySymbol,
                           a.ExchangeRate
                       }).OrderBy(a => a.CurrencyName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Currencys = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> MainCurrency(ClsCurrencyVm obj)
        {
            var det = (from a in oConnectionContext.DbClsUserCurrencyMap
                       join b in oConnectionContext.DbClsCurrency
on a.CurrencyId equals b.CurrencyId
                       where a.CompanyId == obj.CompanyId && a.IsMain == true
                       select new
                       {
                           b.CurrencyCode,
                       }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Currency = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
