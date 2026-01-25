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
    public class CurrencyController : ApiController
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
            if (obj.Search == "" || obj.Search == null)
            {
                var det = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
                {
                    CurrencyId = a.CurrencyId,
                    CurrencyName = a.CurrencyName,
                    a.CurrencySymbol,
                    a.CurrencyCode,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    CompanyId = a.CompanyId,
                }).OrderByDescending(a => a.CurrencyId).Skip(skip).Take(obj.PageSize).ToList();
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Currencys = det,
                        TotalCount = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count(),
                        ActiveCount = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count(),
                        InactiveCount = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == false).Count(),
                        PageSize = obj.PageSize
                    }
                };
            }
            else
            {
                var det = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && (a.CurrencyName.Contains(obj.Search) ||
                a.CurrencyCode.Contains(obj.Search))).Select(a => new
                {
                    CurrencyId = a.CurrencyId,
                    CurrencyName = a.CurrencyName,
                    a.CurrencySymbol,
                    a.CurrencyCode,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    CompanyId = a.CompanyId,
                }).OrderByDescending(a => a.CurrencyId).Skip(skip).Take(obj.PageSize).ToList();
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Currencys = det,
                        TotalCount = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && (a.CurrencyName.Contains(obj.Search) ||
                a.CurrencyCode.Contains(obj.Search))).Count(),
                        ActiveCount = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && (a.CurrencyName.Contains(obj.Search) ||
                a.CurrencyCode.Contains(obj.Search)) && a.IsActive == true).Count(),
                        InactiveCount = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && (a.CurrencyName.Contains(obj.Search) ||
                a.CurrencyCode.Contains(obj.Search)) && a.IsActive == false).Count(),
                        PageSize = obj.PageSize
                    }
                };
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Currency(ClsCurrency obj)
        {
            var det = oConnectionContext.DbClsCurrency.Where(a => a.CurrencyId == obj.CurrencyId).Select(a => new
            {
                CurrencyId = a.CurrencyId,
                a.CurrencyCode,
                CurrencyName = a.CurrencyName,
                a.CurrencySymbol,
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
                    Currency = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCurrency(ClsCurrencyVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            if (obj.CurrencyName == null || obj.CurrencyName == "" || obj.CurrencySymbol == null || obj.CurrencySymbol == "")
            {
                data = new
                {
                    Status = 0,
                    Message = "Fields marked with * is mandatory",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            if (obj.CurrencyCode != "" && obj.CurrencyCode != null)
            {
                if (oConnectionContext.DbClsCurrency.Where(a => a.CurrencyCode == obj.CurrencyCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Duplicate Currency Code exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }
            }

            if (oConnectionContext.DbClsCurrency.Where(a => a.CurrencyName == obj.CurrencyName && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Duplicate Currency Name exists",
                    Data = new
                    {
                    }
                }; return await Task.FromResult(Ok(data));
            }
            ClsCurrency oCurrency = new ClsCurrency()
            {
                CurrencyName = obj.CurrencyName,
                CurrencyCode = obj.CurrencyCode,
                CurrencySymbol = obj.CurrencySymbol,
                IsActive = obj.IsActive,
                IsDeleted = obj.IsDeleted,
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                CompanyId = obj.CompanyId,
            };
            oConnectionContext.DbClsCurrency.Add(oCurrency);
            oConnectionContext.SaveChanges();
            data = new
            {
                Status = 1,
                Message = "Currency created successfully",
                Data = new
                {
                    Currency = oCurrency
                }
            };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCurrency(ClsCurrency obj)
        {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            if (obj.CurrencyName == null || obj.CurrencyName == "" || obj.CurrencySymbol == null || obj.CurrencySymbol == "")
            {
                data = new
                {
                    Status = 0,
                    Message = "Fields marked with * is mandatory",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            if (obj.CurrencyCode != "" && obj.CurrencyCode != null)
            {
                if (oConnectionContext.DbClsCurrency.Where(a => a.CurrencyCode == obj.CurrencyCode && a.CurrencyId != obj.CurrencyId && a.CompanyId == obj.CompanyId).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Duplicate Currency Code exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }
            }

            if (oConnectionContext.DbClsCurrency.Where(a => a.CurrencyName == obj.CurrencyName && a.CurrencyId != obj.CurrencyId && a.CompanyId == obj.CompanyId).Count() > 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Duplicate Currency Name exists",
                    Data = new
                    {
                    }
                }; return await Task.FromResult(Ok(data));
            }
            ClsCurrency oCurrency = new ClsCurrency()
            {
                CurrencyId = obj.CurrencyId,
                CurrencyCode = obj.CurrencyCode,
                CurrencyName = obj.CurrencyName,
                CurrencySymbol = obj.CurrencySymbol,
                ModifiedBy = obj.AddedBy,
                ModifiedOn = CurrentDate,
            };
            oConnectionContext.DbClsCurrency.Attach(oCurrency);
            oConnectionContext.Entry(oCurrency).Property(x => x.CurrencyId).IsModified = true;
            oConnectionContext.Entry(oCurrency).Property(x => x.CurrencySymbol).IsModified = true;
            oConnectionContext.Entry(oCurrency).Property(x => x.CurrencyName).IsModified = true;
            oConnectionContext.Entry(oCurrency).Property(x => x.CurrencyCode).IsModified = true;
            oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedBy).IsModified = true;
            oConnectionContext.Entry(oCurrency).Property(x => x.ModifiedOn).IsModified = true;
            oConnectionContext.SaveChanges(); data = new
            {
                Status = 1,
                Message = "Currency updated successfully",
                Data = new
                {
                }
            };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CurrencyActiveInactive(ClsCurrency obj)
        {
                    using (TransactionScope dbContextTransaction = new TransactionScope())
                    {
                        var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            ClsCurrency oClsRole = new ClsCurrency()
            {
                CurrencyId = obj.CurrencyId,
                IsActive = obj.IsActive,
                ModifiedBy = obj.AddedBy,
                ModifiedOn = CurrentDate,
            };
            oConnectionContext.DbClsCurrency.Attach(oClsRole);
            oConnectionContext.Entry(oClsRole).Property(x => x.CurrencyId).IsModified = true;
            oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
            oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
            oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
            oConnectionContext.SaveChanges(); data = new
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

        public async Task<IHttpActionResult> CurrencyDelete(ClsCurrency obj)
        {
                        using (TransactionScope dbContextTransaction = new TransactionScope())
                        {
                            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            ClsCurrency oClsRole = new ClsCurrency()
            {
                CurrencyId = obj.CurrencyId,
                IsDeleted = true,
                ModifiedBy = obj.AddedBy,
                ModifiedOn = CurrentDate,
            };
            oConnectionContext.DbClsCurrency.Attach(oClsRole);
            oConnectionContext.Entry(oClsRole).Property(x => x.CurrencyId).IsModified = true;
            oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
            oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
            oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
            oConnectionContext.SaveChanges(); data = new
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

        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveCurrencys(ClsCurrencyVm obj)
        {
            var det = oConnectionContext.DbClsCurrency.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CurrencyId = a.CurrencyId,
                a.CurrencyCode,
                CurrencyName = a.CurrencyName,
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
    }
}
