using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
    public class SellingPriceGroupController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSellingPriceGroups(ClsSellingPriceGroupVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSellingPriceGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.Description,
                a.SellingPriceGroupId,
                SellingPriceGroup = a.SellingPriceGroup,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Name).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Name).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SellingPriceGroup.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SellingPriceGroups = det.OrderByDescending(a => a.SellingPriceGroupId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SellingPriceGroup(ClsSellingPriceGroup obj)
        {
            var det = oConnectionContext.DbClsSellingPriceGroup.Where(a => a.SellingPriceGroupId == obj.SellingPriceGroupId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                Branchs = oConnectionContext.DbClsSellingPriceBranchMap.Where(b => b.SellingPriceGroupId == a.SellingPriceGroupId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => b.BranchId).ToList(),
                a.Description,
                a.SellingPriceGroupId,
                SellingPriceGroup = a.SellingPriceGroup,
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
                    SellingPriceGroup = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSellingPriceGroup(ClsSellingPriceGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SellingPriceGroup == null || obj.SellingPriceGroup == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSellingPriceGroup" });
                    isError = true;
                }

                if (obj.SellingPriceGroup != null && obj.SellingPriceGroup != "")
                {
                    if (oConnectionContext.DbClsSellingPriceGroup.Where(a => a.SellingPriceGroup.ToLower() == obj.SellingPriceGroup.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Selling Price Group Name exists", Id = "divSellingPriceGroup" });
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

                ClsSellingPriceGroup oSellingPriceGroup = new ClsSellingPriceGroup()
                {
                    Description = obj.Description,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    SellingPriceGroup = obj.SellingPriceGroup,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsSellingPriceGroup.Add(oSellingPriceGroup);
                oConnectionContext.SaveChanges();

                ////Map with Branch
                if (obj.Branchs != null)
                {
                    foreach (var item in obj.Branchs)
                    {
                        long BranchId = Convert.ToInt64(item);

                        ClsSellingPriceBranchMap oClsSellingPriceBranchMap = new ClsSellingPriceBranchMap()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            BranchId = BranchId,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            ModifiedBy = obj.AddedBy,
                            SellingPriceGroupId = oSellingPriceGroup.SellingPriceGroupId
                        };
                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsSellingPriceBranchMap.Add(oClsSellingPriceBranchMap);
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Selling Price Group",
                    CompanyId = obj.CompanyId,
                    Description = "Selling Price Group \"" + obj.SellingPriceGroup + "\" created",
                    Id = oSellingPriceGroup.SellingPriceGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Selling Price Group created successfully",
                    Data = new
                    {
                        SellingPriceGroup = oSellingPriceGroup
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSellingPriceGroup(ClsSellingPriceGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.SellingPriceGroup == null || obj.SellingPriceGroup == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSellingPriceGroup" });
                    isError = true;
                }

                if (obj.SellingPriceGroup != null && obj.SellingPriceGroup != "")
                {
                    if (oConnectionContext.DbClsSellingPriceGroup.Where(a => a.SellingPriceGroup.ToLower() == obj.SellingPriceGroup.ToLower() && a.SellingPriceGroupId != obj.SellingPriceGroupId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Selling Group Name exists", Id = "divSellingPriceGroup" });
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

                ClsSellingPriceGroup oSellingPriceGroup = new ClsSellingPriceGroup()
                {
                    Description = obj.Description,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    SellingPriceGroup = obj.SellingPriceGroup,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSellingPriceGroup.Attach(oSellingPriceGroup);
                oConnectionContext.Entry(oSellingPriceGroup).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oSellingPriceGroup).Property(x => x.SellingPriceGroup).IsModified = true;
                oConnectionContext.Entry(oSellingPriceGroup).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSellingPriceGroup).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oSellingPriceGroup).Property(x => x.Description).IsModified = true;

                oConnectionContext.SaveChanges();

                string query = "update \"tblSellingPriceBranchMap\" set \"IsActive\"= False where \"SellingPriceGroupId\"=" + obj.SellingPriceGroupId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                //Map with Branch
                if (obj.Branchs != null)
                {
                    foreach (var item in obj.Branchs)
                    {
                        long BranchId = Convert.ToInt64(item);
                        long SellingPriceBranchMapId = oConnectionContext.DbClsSellingPriceBranchMap.Where(a => a.SellingPriceGroupId == oSellingPriceGroup.SellingPriceGroupId &&
                        a.BranchId == BranchId && a.IsDeleted == false).Select(a => a.SellingPriceBranchMapId).FirstOrDefault();
                        if (SellingPriceBranchMapId == 0)
                        {
                            ClsSellingPriceBranchMap oClsSellingPriceBranchMap = new ClsSellingPriceBranchMap()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                BranchId = BranchId,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                ModifiedBy = obj.AddedBy,
                                SellingPriceGroupId = oSellingPriceGroup.SellingPriceGroupId
                            };
                            oConnectionContext.DbClsSellingPriceBranchMap.Add(oClsSellingPriceBranchMap);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsSellingPriceBranchMap oClsSellingPriceBranchMap = new ClsSellingPriceBranchMap()
                            {
                                SellingPriceBranchMapId = SellingPriceBranchMapId,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                BranchId = BranchId,
                                IsActive = true,
                                IsDeleted = false,
                                SellingPriceGroupId = oSellingPriceGroup.SellingPriceGroupId
                            };
                            oConnectionContext.DbClsSellingPriceBranchMap.Attach(oClsSellingPriceBranchMap);
                            oConnectionContext.Entry(oClsSellingPriceBranchMap).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsSellingPriceBranchMap).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsSellingPriceBranchMap).Property(x => x.BranchId).IsModified = true;
                            oConnectionContext.Entry(oClsSellingPriceBranchMap).Property(x => x.IsActive).IsModified = true;
                            oConnectionContext.Entry(oClsSellingPriceBranchMap).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsSellingPriceBranchMap).Property(x => x.SellingPriceGroupId).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Selling Price Group",
                    CompanyId = obj.CompanyId,
                    Description = "Selling Price Group \"" + obj.SellingPriceGroup +"\" updated",
                    Id = oSellingPriceGroup.SellingPriceGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Selling Price Group updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SellingPriceGroupActiveInactive(ClsSellingPriceGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSellingPriceGroup oClsRole = new ClsSellingPriceGroup()
                {
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSellingPriceGroup.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Selling Price Group",
                    CompanyId = obj.CompanyId,
                    Description = "Selling Price Group \""+ oConnectionContext.DbClsSellingPriceGroup.Where(a => a.SellingPriceGroupId == obj.SellingPriceGroupId).Select(a => a.SellingPriceGroup).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.SellingPriceGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Selling Price Group " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SellingPriceGroupDelete(ClsSellingPriceGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemSellingPriceGroupMapCount = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(a => a.CompanyId == obj.CompanyId && a.SellingPriceGroupId == obj.SellingPriceGroupId && a.IsDeleted == false).Count();
                if (ItemSellingPriceGroupMapCount > 0)
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
                ClsSellingPriceGroup oClsRole = new ClsSellingPriceGroup()
                {
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSellingPriceGroup.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Selling Price Group",
                    CompanyId = obj.CompanyId,
                    Description = "Selling Price Group \"" + oConnectionContext.DbClsSellingPriceGroup.Where(a => a.SellingPriceGroupId == obj.SellingPriceGroupId).Select(a => a.SellingPriceGroup).FirstOrDefault()+"\" deleted",
                    Id = oClsRole.SellingPriceGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Selling Price Group deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSellingPriceGroups(ClsSellingPriceGroupVm obj)
        {
            List<ClsSellingPriceGroupVm> det;

            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsSellingPriceGroup
                           join b in oConnectionContext.DbClsSellingPriceBranchMap
                           on a.SellingPriceGroupId equals b.SellingPriceGroupId
                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                            //&& b.BranchId == obj.BranchId
                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                           && b.IsActive == true && b.IsDeleted == false
                           select new ClsSellingPriceGroupVm
                           {
                               SellingPriceGroupId = a.SellingPriceGroupId,
                               SellingPriceGroup = a.SellingPriceGroup,
                           }).OrderBy(a=>a.SellingPriceGroup).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsSellingPriceGroup
                           join b in oConnectionContext.DbClsSellingPriceBranchMap
                           on a.SellingPriceGroupId equals b.SellingPriceGroupId
                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                             && b.BranchId == obj.BranchId
                           && b.IsActive == true && b.IsDeleted == false
                           select new ClsSellingPriceGroupVm
                           {
                               SellingPriceGroupId = a.SellingPriceGroupId,
                               SellingPriceGroup = a.SellingPriceGroup,
                           }).OrderBy(a=>a.SellingPriceGroup).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SellingPriceGroups = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SellingPriceGroupReport(ClsUserVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsUserVm> det;
            if (obj.BranchId == 0)
            {
                det = (from e in oConnectionContext.DbClsSellingPriceGroup
                           //join a in oConnectionContext.DbClsUser
                           //on e.UserGroupId equals a.UserGroupId
                           //join b in oConnectionContext.DbClsUserBranchMap
                           //on a.UserId equals b.UserId
                       where e.CompanyId == obj.CompanyId
                       //                       && a.UserType.ToLower() == obj.UserType.ToLower() &&
                       //                       a.IsDeleted == false && a.IsCancelled == false &&
                       //                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                       select new ClsUserVm
                       {
                           SellingPriceGroupId = e.SellingPriceGroupId,
                           SellingPriceGroup = e.SellingPriceGroup,
                           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.SellingPriceGroupId == e.SellingPriceGroupId
                           && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false 
                                             //&& c.BranchId == obj.BranchId 
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
&& c.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                               join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                               where d.SellingPriceGroupId == e.SellingPriceGroupId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && d.CompanyId == obj.CompanyId && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                        //&& d.BranchId == obj.BranchId 
                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                       && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSales > 0).ToList();
            }
            else
            {
                det = (from e in oConnectionContext.DbClsSellingPriceGroup
                           //join a in oConnectionContext.DbClsUser
                           //on e.UserGroupId equals a.UserGroupId
                           //join b in oConnectionContext.DbClsUserBranchMap
                           //on a.UserId equals b.UserId
                       where e.CompanyId == obj.CompanyId
                       //&& a.UserType.ToLower() == obj.UserType.ToLower() &&
                       //a.IsDeleted == false && a.IsCancelled == false && b.BranchId == obj.BranchId
                       select new ClsUserVm
                       {
                           SellingPriceGroupId = e.SellingPriceGroupId,
                           SellingPriceGroup = e.SellingPriceGroup,
                           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.SellingPriceGroupId == e.SellingPriceGroupId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                             && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                               join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                               where d.SellingPriceGroupId == e.SellingPriceGroupId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && d.CompanyId == obj.CompanyId && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                       && d.BranchId == obj.BranchId && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSales > 0).ToList();
            }

            if (obj.SellingPriceGroupId != 0)
            {
                det = det.Where(a => a.SellingPriceGroupId== obj.SellingPriceGroupId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.TotalSales).Take(obj.PageSize),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
