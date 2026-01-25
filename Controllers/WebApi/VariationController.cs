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
    public class VariationController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllVariations(ClsVariationVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            //var det = oConnectionContext.DbClsVariation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).AsEnumerable().Select(a => new
            //{
            //    //a.VariationCode,
            //    VariationId = a.VariationId,
            //    Variation = a.Variation,
            //    IsActive = a.IsActive,
            //    IsDeleted = a.IsDeleted,
            //    AddedBy = a.AddedBy,
            //    AddedOn = a.AddedOn,
            //    ModifiedBy = a.ModifiedBy,
            //    ModifiedOn = a.ModifiedOn,
            //    CompanyId = a.CompanyId,
            //    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
            //    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            //    Values = string.Join(",", oConnectionContext.DbClsVariationDetails.Where(p => p.VariationId == a.VariationId && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true)
            //                 .Select(p => p.VariationDetails))
            //}).ToList();

            var variations = oConnectionContext.DbClsVariation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                //a.VariationCode,
                VariationId = a.VariationId,
                Variation = a.Variation,
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

            var variationIds = variations.Select(v => v.VariationId).ToList();

            var variationDetailsDictionary = oConnectionContext.DbClsVariationDetails
                .Where(p => variationIds.Contains(p.VariationId) && p.IsDeleted == false && p.IsActive == true)
                .GroupBy(p => p.VariationId)
                .ToDictionary(g => g.Key, g => string.Join(",", g.Select(p => p.VariationDetails)));

            var det = variations.Select(v => new
            {
                v.VariationId,
                v.Variation,
                v.IsActive,
                v.IsDeleted,
                v.AddedBy,
                v.AddedOn,
                v.ModifiedBy,
                v.ModifiedOn,
                v.CompanyId,
                AddedByCode = v.AddedByCode,
                ModifiedByCode = v.ModifiedByCode,
                Values = variationDetailsDictionary.ContainsKey(v.VariationId) ? variationDetailsDictionary[v.VariationId] : string.Empty
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Values.ToLower().Contains(obj.Search.ToLower()) || a.Variation.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Variations = det.OrderByDescending(a => a.VariationId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Variation(ClsVariation obj)
        {
            var det = oConnectionContext.DbClsVariation.Where(a => a.VariationId == obj.VariationId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                //a.VariationCode,
                VariationId = a.VariationId,
                Variation = a.Variation,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                VariationDetails = oConnectionContext.DbClsVariationDetails.Where(b => b.VariationId == a.VariationId).Select(b => new
                {
                    b.VariationDetailsId,
                    b.VariationDetails
                }).ToList()
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Variation = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertVariation(ClsVariationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;
                if (obj.Variation == null || obj.Variation == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divVariation" });
                    isError = true;
                }

                //if (obj.VariationCode != "" && obj.VariationCode != null)
                //{
                //    if (oConnectionContext.DbClsVariation.Where(a => a.VariationCode.ToLower() == obj.VariationCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Variation Code exists", Id = "divVariationCode" });
                //        isError = true;
                //    }
                //}

                if (obj.Variation != "" && obj.Variation != null)
                {
                    if (oConnectionContext.DbClsVariation.Where(a => a.Variation.ToLower() == obj.Variation.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Variation Name exists", Id = "divVariation" });
                        isError = true;
                    }
                }

                if (obj.VariationDetails == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divVariationDetails" });
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

                //if (obj.VariationCode == "" || obj.VariationCode == null)
                //{
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixId equals b.PrefixId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Variation"
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.VariationCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsVariation oClsVariation = new ClsVariation()
                {
                    //VariationCode = obj.VariationCode,
                    Variation = obj.Variation,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsVariation.Add(oClsVariation);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.VariationDetails)
                {
                    if (item.VariationDetails != null && item.VariationDetails != "")
                    {
                        ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            VariationDetails = item.VariationDetails,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            VariationId = oClsVariation.VariationId
                        };
                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsVariationDetails.Add(oClsVariationDetails);
                        oConnectionContext.SaveChanges();
                    }
                }

                ////increase counter
                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Variation",
                    CompanyId = obj.CompanyId,
                    Description = "Variation \"" + obj.Variation + "\" created",
                    Id = oClsVariation.VariationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Variation created successfully",
                    Data = new
                    {
                        Variation = oClsVariation
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Updatevariation(ClsVariationVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Variation == null || obj.Variation == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divVariation" });
                    isError = true;
                }

                if (obj.Variation == null || obj.Variation == "")
                {
                    if (oConnectionContext.DbClsVariation.Where(a => a.Variation.ToLower() == obj.Variation.ToLower() && a.VariationId != obj.VariationId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate variation Name exists", Id = "divVariation" });
                        isError = true;
                    }
                }

                if ((obj.VariationDetails == null) || (obj.VariationDetails.Where(a=>a.IsDeleted== false).Count() == 0))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divVariationDetails" });
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

                ClsVariation ovariation = new ClsVariation()
                {
                    VariationId = obj.VariationId,
                    Variation = obj.Variation,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVariation.Attach(ovariation);
                oConnectionContext.Entry(ovariation).Property(x => x.Variation).IsModified = true;
                oConnectionContext.Entry(ovariation).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(ovariation).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                foreach (var item in obj.VariationDetails)
                {
                    if (item.IsDeleted == true)
                    {
                        ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                        {
                            VariationDetailsId = item.VariationDetailsId,
                            IsDeleted = true,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                        };
                        oConnectionContext.DbClsVariationDetails.Attach(oClsVariationDetails);
                        oConnectionContext.Entry(oClsVariationDetails).Property(x => x.VariationId).IsModified = true;
                        oConnectionContext.Entry(oClsVariationDetails).Property(x => x.IsDeleted).IsModified = true;
                        oConnectionContext.Entry(oClsVariationDetails).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsVariationDetails).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                    else
                    {
                        if (item.VariationDetailsId == 0)
                        {
                            ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                VariationDetails = item.VariationDetails,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                VariationId = obj.VariationId
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsVariationDetails.Add(oClsVariationDetails);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            if (item.VariationDetails != null && item.VariationDetails != "")
                            {
                                ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                                {
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    VariationDetails = item.VariationDetails,
                                    VariationDetailsId = item.VariationDetailsId
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsVariationDetails.Attach(oClsVariationDetails);
                                oConnectionContext.Entry(oClsVariationDetails).Property(x => x.VariationDetails).IsModified = true;
                                oConnectionContext.Entry(oClsVariationDetails).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsVariationDetails).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Variation",
                    CompanyId = obj.CompanyId,
                    Description = "Variation \"" + obj.Variation + "\" updated",
                    Id = ovariation.VariationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Variation updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> variationActiveInactive(ClsVariationVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsVariation oClsRole = new ClsVariation()
                {
                    VariationId = obj.VariationId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVariation.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.VariationId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Variation",
                    CompanyId = obj.CompanyId,
                    Description = "Variation \"" + oConnectionContext.DbClsVariation.Where(a => a.VariationId == obj.VariationId).Select(a => a.Variation).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.VariationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Variation " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> variationDelete(ClsVariationVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = (from a in oConnectionContext.DbClsItem
                                 join b in oConnectionContext.DbClsItemDetails
                                 on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                 && b.VariationId == obj.VariationId
                                 select a.ItemId).Count();
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
                ClsVariation oClsRole = new ClsVariation()
                {
                    VariationId = obj.VariationId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsVariation.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.VariationId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Variation",
                    CompanyId = obj.CompanyId,
                    Description = "Variation \"" + oConnectionContext.DbClsVariation.Where(a => a.VariationId == obj.VariationId).Select(a => a.Variation).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.VariationId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "variation deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Activevariations(ClsVariation obj)
        {
            var det = oConnectionContext.DbClsVariation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                VariationId = a.VariationId,
                Variation = a.Variation,
            }).OrderBy(a => a.Variation).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Variations = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveVariationDetails(ClsVariation obj)
        {
            var det = oConnectionContext.DbClsVariationDetails.Where(a => a.VariationId == obj.VariationId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                VariationDetailsId = a.VariationDetailsId,
                a.VariationDetails
            }).OrderBy(a => a.VariationDetails).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    VariationDetails = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
