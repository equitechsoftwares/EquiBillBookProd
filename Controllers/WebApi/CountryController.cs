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
    public class CountryController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllCountrys(ClsCountryVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.CurrencyName,
                a.CurrencyCode,
                a.CurrencySymbol,
                a.DialingCode,
                Currencys = oConnectionContext.DbClsCountryCurrencyMap.Where(b => b.CountryId == a.CountryId).Select(b => new
                {
                    b.CurrencyId,
                    CurrencyName = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == b.CurrencyId).Select(c => c.CurrencyName).FirstOrDefault(),
                    CurrencyCode = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == b.CurrencyId).Select(c => c.CurrencyCode).FirstOrDefault(),
                    CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == b.CurrencyId).Select(c => c.CurrencySymbol).FirstOrDefault(),
                }),
                TimeZones = oConnectionContext.DbClsCountryTimeZoneMap.Where(b => b.CountryId == a.CountryId).Select(b => new
                {
                    b.TimeZoneId,
                    DisplayName = oConnectionContext.DbClsTimeZone.Where(c => c.TimeZoneId == b.TimeZoneId).Select(c => c.DisplayName).FirstOrDefault(),
                    Utc = oConnectionContext.DbClsTimeZone.Where(c => c.TimeZoneId == b.TimeZoneId).Select(c => c.SupportsDaylightSavingTime == false ? c.o1String : c.o2String).FirstOrDefault(),
                }),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Country.Contains(obj.Search) || a.CountryCode.Contains(obj.Search)).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Countrys = det.OrderBy(a => a.Country).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Country(ClsCountry obj)
        {
            var det = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.CurrencyCode,
                a.CurrencyName,
                a.CurrencySymbol,
                a.DialingCode,
                Currencys = oConnectionContext.DbClsCurrency.Where(c => c.IsActive == true && c.IsDeleted == false).Select(c => new
                {
                    c.CurrencyId,
                    c.CurrencyName,
                    c.CurrencyCode,
                    c.CurrencySymbol,
                    CountryCurrencyMapId = oConnectionContext.DbClsCountryCurrencyMap.Where(b => b.CountryId == obj.CountryId && b.CurrencyId == c.CurrencyId).Select(b => b.CountryCurrencyMapId).FirstOrDefault()
                }),
                TimeZones = oConnectionContext.DbClsTimeZone.Where(c => c.IsActive == true && c.IsDeleted == false).Select(c => new
                {
                    c.TimeZoneId,
                    c.DisplayName,
                    Utc = c.SupportsDaylightSavingTime == false ? c.o1String : c.o2String,
                    CountryTimeZoneMapId = oConnectionContext.DbClsCountryTimeZoneMap.Where(b => b.CountryId == obj.CountryId && b.TimeZoneId == c.TimeZoneId).Select(b => b.CountryTimeZoneMapId).FirstOrDefault()
                }),
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Country = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCountry(ClsCountryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;
                if (obj.Country == null || obj.Country == "")
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

                if (obj.CountryCode != "" && obj.CountryCode != null)
                {
                    if (oConnectionContext.DbClsCountry.Where(a => a.CountryCode == obj.CountryCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Duplicate Country Code exists",
                            Data = new
                            {
                            }
                        }; return await Task.FromResult(Ok(data));
                    }
                }
                else
                {
                    //long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "country"
                                          //&& b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.CountryCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                if (oConnectionContext.DbClsCountry.Where(a => a.Country == obj.Country && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Duplicate Country Name exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }
                ClsCountry oCountry = new ClsCountry()
                {
                    Country = obj.Country,
                    CountryCode = obj.CountryCode,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    CurrencyCode = obj.CurrencyCode,
                    CurrencyName = obj.CurrencyName,
                    CurrencySymbol = obj.CurrencySymbol,
                };
                oConnectionContext.DbClsCountry.Add(oCountry);
                oConnectionContext.SaveChanges();

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Country",
                    CompanyId = obj.CompanyId,
                    Description = "added " + obj.Country,
                    Id = oCountry.CountryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Country created successfully",
                    Data = new
                    {
                        Country = oCountry
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCountry(ClsCountryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Country == null || obj.Country == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCountry" });
                    isError = true;
                }

                if (obj.Country != "" && obj.Country != null)
                {
                    if (oConnectionContext.DbClsCountry.Where(a => a.Country.ToLower() == obj.Country.ToLower() && a.CountryId != obj.CountryId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Country Name exists", Id = "divCountry" });
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

                ClsCountry oCountry = new ClsCountry()
                {
                    CountryId = obj.CountryId,
                    //CountryCode = obj.CountryCode,
                    Country = obj.Country,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    //CurrencyCode = obj.CurrencyCode,
                    //CurrencySymbol = obj.CurrencySymbol,
                    //CurrencyName = obj.CurrencyName,
                };
                oConnectionContext.DbClsCountry.Attach(oCountry);
                oConnectionContext.Entry(oCountry).Property(x => x.Country).IsModified = true;
                //oConnectionContext.Entry(oCountry).Property(x => x.CountryCode).IsModified = true;
                oConnectionContext.Entry(oCountry).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCountry).Property(x => x.ModifiedOn).IsModified = true;
                //oConnectionContext.Entry(oCountry).Property(x => x.CurrencyCode).IsModified = true;
                //oConnectionContext.Entry(oCountry).Property(x => x.CurrencyName).IsModified = true;
                //oConnectionContext.Entry(oCountry).Property(x => x.CurrencySymbol).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.CountryCurrencyMaps != null)
                {
                    string query = "update \"tblCountryCurrencyMap\" set \"IsActive\"=False where \"CountryId\"=" + obj.CountryId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in obj.CountryCurrencyMaps)
                    {
                        if (item.CountryCurrencyMapId == 0)
                        {
                            ClsCountryCurrencyMap oClsCountryCurrencyMap = new ClsCountryCurrencyMap()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                IsActive = true,
                                IsDeleted = false,
                                CountryId = obj.CountryId,
                                CurrencyId = item.CurrencyId,
                            };
                            ConnectionContext ocon = new ConnectionContext();
                            ocon.DbClsCountryCurrencyMap.Add(oClsCountryCurrencyMap);
                            ocon.SaveChanges();
                        }
                        else
                        {
                            ClsCountryCurrencyMap oClsCountryCurrencyMap = new ClsCountryCurrencyMap()
                            {
                                CountryCurrencyMapId = item.CountryCurrencyMapId,
                                IsActive = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                //CountryId = item.CountryId,
                                //CurrencyId = item.CurrencyId,
                            };

                            ConnectionContext ocon = new ConnectionContext();
                            ocon.DbClsCountryCurrencyMap.Attach(oClsCountryCurrencyMap);
                            ocon.Entry(oClsCountryCurrencyMap).Property(x => x.ModifiedBy).IsModified = true;
                            ocon.Entry(oClsCountryCurrencyMap).Property(x => x.ModifiedOn).IsModified = true;
                            ocon.Entry(oClsCountryCurrencyMap).Property(x => x.IsActive).IsModified = true;
                            //ocon.Entry(oClsCountryCurrencyMap).Property(x => x.CountryId).IsModified = true;
                            //ocon.Entry(oClsCountryCurrencyMap).Property(x => x.CurrencyId).IsModified = true;
                            ocon.SaveChanges();
                        }
                    }
                }

                if (obj.CountryTimeZoneMaps != null)
                {
                    string query = "update \"tblCountryTimeZoneMap\" set \"IsActive\"=False where \"CountryId\"=" + obj.CountryId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in obj.CountryTimeZoneMaps)
                    {
                        if (item.CountryTimeZoneMapId == 0)
                        {
                            ClsCountryTimeZoneMap oClsCountryTimeZoneMap = new ClsCountryTimeZoneMap()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                IsActive = true,
                                IsDeleted = false,
                                CountryId = obj.CountryId,
                                TimeZoneId = item.TimeZoneId,
                            };
                            ConnectionContext ocon = new ConnectionContext();
                            ocon.DbClsCountryTimeZoneMap.Add(oClsCountryTimeZoneMap);
                            ocon.SaveChanges();
                        }
                        else
                        {
                            ClsCountryTimeZoneMap oClsCountryTimeZoneMap = new ClsCountryTimeZoneMap()
                            {
                                CountryTimeZoneMapId = item.CountryTimeZoneMapId,
                                IsActive = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                //CountryId = item.CountryId,
                                //CurrencyId = item.CurrencyId,
                            };

                            ConnectionContext ocon = new ConnectionContext();
                            ocon.DbClsCountryTimeZoneMap.Attach(oClsCountryTimeZoneMap);
                            ocon.Entry(oClsCountryTimeZoneMap).Property(x => x.ModifiedBy).IsModified = true;
                            ocon.Entry(oClsCountryTimeZoneMap).Property(x => x.ModifiedOn).IsModified = true;
                            ocon.Entry(oClsCountryTimeZoneMap).Property(x => x.IsActive).IsModified = true;
                            //ocon.Entry(oClsCountryCurrencyMap).Property(x => x.CountryId).IsModified = true;
                            //ocon.Entry(oClsCountryCurrencyMap).Property(x => x.CurrencyId).IsModified = true;
                            ocon.SaveChanges();
                        }
                    }
                }


                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Country",
                    CompanyId = obj.CompanyId,
                    Description = "modified " + obj.Country,
                    Id = oCountry.CountryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Country updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CountryActiveInactive(ClsCountryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsCountry oClsRole = new ClsCountry()
                {
                    CountryId = obj.CountryId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCountry.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Country",
                    CompanyId = obj.CompanyId,
                    Description = (obj.IsActive == true ? "activated " : "deactivated ") + obj.Country,
                    Id = oClsRole.CountryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Country " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CountryDelete(ClsCountryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsCountry oClsRole = new ClsCountry()
                {
                    CountryId = obj.CountryId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCountry.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Country",
                    CompanyId = obj.CompanyId,
                    Description = "deleted  " + oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.Country).FirstOrDefault(),
                    Id = oClsRole.CountryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Country deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //[AllowAnonymous]
        public async Task<IHttpActionResult> ActiveCountrys(ClsCountryVm obj)
        {
            var det = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
                a.CurrencyCode,
                a.CurrencyName,
                a.CurrencySymbol,
                a.DialingCode
            }).OrderBy(a => a.Country).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Countrys = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllCountrysMapped(ClsUserCountryMapVm obj)
        {
            //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsCountry
                       where a.IsActive == true && a.IsDeleted == false
                       select new
                       {
                           a.Country,
                           a.CurrencyCode,
                           a.CountryId,
                           IsSelected = oConnectionContext.DbClsUserCountryMap.Where(b => b.CompanyId == obj.CompanyId && b.CountryId == a.CountryId
                           && b.IsActive == true && b.IsDeleted == false).Count() == 0 ? false : true,
                           PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(b => b.CompanyId == obj.CompanyId && b.CountryId == a.CountryId
                           && b.IsActive == true && b.IsDeleted == false).Select(b => b.PriceHikePercentage).DefaultIfEmpty().FirstOrDefault(),
                           UserCountryMapId = oConnectionContext.DbClsUserCountryMap.Where(b => b.CompanyId == obj.CompanyId && b.CountryId == a.CountryId
                           && b.IsDeleted == false).Select(b => b.UserCountryMapId).DefaultIfEmpty().FirstOrDefault(),
                           IsMain = oConnectionContext.DbClsUserCountryMap.Where(b => b.CompanyId == obj.CompanyId && b.CountryId == a.CountryId
                           && b.IsDeleted == false).Select(b => b.IsMain).DefaultIfEmpty().FirstOrDefault(),
                       }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Countrys = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCountryMapped(ClsCountryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
               
                if (obj.UserCountryMaps != null)
                {
                    foreach (var item in obj.UserCountryMaps)
                    {
                        if (item.PriceHikePercentage < 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPriceHikePercentage"+item.DivId });
                            isError = true;
                        }
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

                if (obj.UserCountryMaps != null)
                {
                    string query = "update \"tblUserCountryMap\" set \"IsActive\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in obj.UserCountryMaps)
                    {
                        if (item.UserCountryMapId == 0)
                        {
                            ClsUserCountryMap oClsUserCountryMap = new ClsUserCountryMap()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                IsActive = true,
                                IsDeleted = false,
                                CountryId = item.CountryId,
                                CompanyId = obj.CompanyId,
                                PriceHikePercentage = item.PriceHikePercentage
                            };
                            ConnectionContext ocon = new ConnectionContext();
                            ocon.DbClsUserCountryMap.Add(oClsUserCountryMap);
                            ocon.SaveChanges();
                        }
                        else
                        {
                            ClsUserCountryMap oClsUserCountryMap = new ClsUserCountryMap()
                            {
                                UserCountryMapId = item.UserCountryMapId,
                                IsActive = true,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                PriceHikePercentage = item.PriceHikePercentage
                            };

                            ConnectionContext ocon = new ConnectionContext();
                            ocon.DbClsUserCountryMap.Attach(oClsUserCountryMap);
                            ocon.Entry(oClsUserCountryMap).Property(x => x.ModifiedBy).IsModified = true;
                            ocon.Entry(oClsUserCountryMap).Property(x => x.ModifiedOn).IsModified = true;
                            ocon.Entry(oClsUserCountryMap).Property(x => x.IsActive).IsModified = true;
                            ocon.Entry(oClsUserCountryMap).Property(x => x.PriceHikePercentage).IsModified = true;
                            ocon.SaveChanges();
                        }
                    }
                }

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    Category = "Country",
                //    CompanyId = obj.CompanyId,
                //    Description = "modified " + obj.Country,
                //    Id = oCountry.CountryId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Update"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Country updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveCountrysMapped(ClsUserCountryMapVm obj)
        {
            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsCountry
                       join b in oConnectionContext.DbClsUserCountryMap
                       on a.CountryId equals b.CountryId
                       where a.IsActive == true && a.IsDeleted == false && b.CompanyId == obj.Under
                       && b.IsActive == true && b.IsDeleted == false
                       select new
                       {
                           b.IsMain,
                           CountryId = a.CountryId,
                           a.CountryCode,
                           Country = a.Country,
                           a.CurrencyCode,
                           a.CurrencyName,
                           a.CurrencySymbol,
                           a.DialingCode
                       }).OrderBy(a => a.Country).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Countrys = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> MainCountry(ClsCountryVm obj)
        {
            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            var det = (from a in oConnectionContext.DbClsUserCountryMap
                       join b in oConnectionContext.DbClsCountry
on a.CountryId equals b.CountryId
                       where a.CompanyId == obj.Under && a.IsMain == true
                       select new
                       {
                           b.CountryId,
                           CurrencySymbol = (from a in oConnectionContext.DbClsUserCurrencyMap
                                join b in oConnectionContext.DbClsCurrency
         on a.CurrencyId equals b.CurrencyId
                                where a.CompanyId == obj.Under && a.IsMain == true
                                select b.CurrencySymbol).FirstOrDefault()
                       }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Country = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
