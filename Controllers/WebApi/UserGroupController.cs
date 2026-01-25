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
    public class UserGroupController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllUserGroups(ClsUserGroupVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsUserGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.Description,
                a.PriceCalculationType,
                a.SellingPriceGroupId,
                a.CalculationPercentage,
                UserGroupId = a.UserGroupId,
                UserGroup = a.UserGroup,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                SellingPriceGroup = oConnectionContext.DbClsSellingPriceGroup.Where(b => b.SellingPriceGroupId == a.SellingPriceGroupId).Select(b => b.SellingPriceGroup).FirstOrDefault()
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.UserGroup.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserGroups = det.OrderByDescending(a => a.UserGroupId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserGroup(ClsUserGroup obj)
        {
            var det = oConnectionContext.DbClsUserGroup.Where(a => a.UserGroupId == obj.UserGroupId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.Description,
                a.PriceCalculationType,
                a.SellingPriceGroupId,
                a.CalculationPercentage,
                UserGroupId = a.UserGroupId,
                UserGroup = a.UserGroup,
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
                    UserGroup = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertUserGroup(ClsUserGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.UserGroup == null || obj.UserGroup == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUserGroup" });
                    isError = true;
                }

                if (obj.UserGroup != "" && obj.UserGroup != null)
                {
                    if (oConnectionContext.DbClsUserGroup.Where(a => a.UserGroup.ToLower() == obj.UserGroup.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Customer Group Name exists", Id = "divUserGroup" });
                        isError = true;
                    }
                }

                if (obj.PriceCalculationType == 1)
                {
                    if (obj.CalculationPercentage == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divCalculationPercentage" });
                        isError = true;
                    }
                }
                else if (obj.PriceCalculationType == 2)
                {
                    if (obj.SellingPriceGroupId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSellingPriceGroup" });
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

                ClsUserGroup oUserGroup = new ClsUserGroup()
                {
                    Description = obj.Description,
                    PriceCalculationType = obj.PriceCalculationType,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    CalculationPercentage = obj.CalculationPercentage,
                    UserGroup = obj.UserGroup,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsUserGroup.Add(oUserGroup);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Group",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Group \"" + obj.UserGroup + "\" created",
                    Id = oUserGroup.UserGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Customer Group created successfully",
                    Data = new
                    {
                        UserGroup = oUserGroup
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateUserGroup(ClsUserGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.UserGroup == null || obj.UserGroup == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUserGroup" });
                    isError = true;
                }

                if (obj.UserGroup != "" && obj.UserGroup != null)
                {
                    if (oConnectionContext.DbClsUserGroup.Where(a => a.UserGroup.ToLower() == obj.UserGroup.ToLower() && a.UserGroupId != obj.UserGroupId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Customer Group Name exists", Id = "divUserGroup" });
                        isError = true;
                    }
                }

                if (obj.PriceCalculationType == 1)
                {
                    if (obj.CalculationPercentage == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divCalculationPercentage" });
                        isError = true;
                    }
                }
                else if (obj.PriceCalculationType == 2)
                {
                    if (obj.SellingPriceGroupId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSellingPriceGroup" });
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

                ClsUserGroup oUserGroup = new ClsUserGroup()
                {
                    Description = obj.Description,
                    UserGroupId = obj.UserGroupId,
                    PriceCalculationType = obj.PriceCalculationType,
                    SellingPriceGroupId = obj.SellingPriceGroupId,
                    CalculationPercentage = obj.CalculationPercentage,
                    UserGroup = obj.UserGroup,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUserGroup.Attach(oUserGroup);
                oConnectionContext.Entry(oUserGroup).Property(x => x.UserGroupId).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.UserGroup).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.PriceCalculationType).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.SellingPriceGroupId).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.CalculationPercentage).IsModified = true;
                oConnectionContext.Entry(oUserGroup).Property(x => x.Description).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Group",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Group \"" + obj.UserGroup + "\" updated",
                    Id = oUserGroup.UserGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Customer Group updated successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserGroupActiveInactive(ClsUserGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsUserGroup oClsRole = new ClsUserGroup()
                {
                    UserGroupId = obj.UserGroupId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUserGroup.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.UserGroupId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Group",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Group \"" + oConnectionContext.DbClsUserGroup.Where(a => a.UserGroupId == obj.UserGroupId).Select(a => a.UserGroup).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.UserGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Customer Group " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserGroupDelete(ClsUserGroupVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int UserCount = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserGroupId == obj.UserGroupId && a.IsDeleted == false).Count();
                if (UserCount > 0)
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
                ClsUserGroup oClsRole = new ClsUserGroup()
                {
                    UserGroupId = obj.UserGroupId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUserGroup.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.UserGroupId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Customer Group",
                    CompanyId = obj.CompanyId,
                    Description = "Customer Group \"" + oConnectionContext.DbClsUserGroup.Where(a => a.UserGroupId == obj.UserGroupId).Select(a => a.UserGroup).FirstOrDefault()+"\" deleted",
                    Id = oClsRole.UserGroupId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Customer Group deleted successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveUserGroups(ClsUserGroupVm obj)
        {
            var det = oConnectionContext.DbClsUserGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                UserGroupId = a.UserGroupId,
                UserGroup = a.UserGroup,
            }).OrderBy(a => a.UserGroup).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserGroups = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerGroupReport(ClsUserVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //    oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //      && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            //}).FirstOrDefault();

            //if (obj.BranchId == 0)
            //{
            //    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            //}

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
                det = (from e in oConnectionContext.DbClsUserGroup
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
                           UserGroupId = e.UserGroupId,
                           UserGroup = e.UserGroup,
                           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.UserGroupId == e.UserGroupId
                           && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.SellingPriceGroupId == 0
                                                            //&& c.BranchId == obj.BranchId 
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
&& c.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                               join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                               where d.UserGroupId == e.UserGroupId && d.SellingPriceGroupId == 0 && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && d.CompanyId == obj.CompanyId && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                        //&& d.BranchId == obj.BranchId 
                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                       && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                           //                                       TotalSalesPaid = (from c in oConnectionContext.DbClsSales
                           //                                                         join d in oConnectionContext.DbClsPayment on c.SalesId equals d.Id
                           //                                                         where
                           //d.Type.ToLower() == "sales payment" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                           //                                                         select c.GrandTotal).DefaultIfEmpty().Sum(),
                           //                                       TotalSalesDue =
                           //                        oConnectionContext.DbClsSales.Where(c => c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                           //                        && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                           //                        (from c in oConnectionContext.DbClsSales
                           //                         join d in oConnectionContext.DbClsPayment on c.SalesId equals d.Id
                           //                         where
                           //d.Type.ToLower() == "sales payment" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                           //                         select c.GrandTotal).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSales > 0).ToList();
            }
            else
            {
                det = (from e in oConnectionContext.DbClsUserGroup
                       //join a in oConnectionContext.DbClsUser
                       //on e.UserGroupId equals a.UserGroupId
                       //join b in oConnectionContext.DbClsUserBranchMap
                       //on a.UserId equals b.UserId
                       where e.CompanyId == obj.CompanyId 
                       //&& a.UserType.ToLower() == obj.UserType.ToLower() &&
                       //a.IsDeleted == false && a.IsCancelled == false && b.BranchId == obj.BranchId
                       select new ClsUserVm
                       {
                           UserGroupId = e.UserGroupId,
                           UserGroup = e.UserGroup,
                           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.UserGroupId == e.UserGroupId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                            && c.SellingPriceGroupId == 0 && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                               join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                               where d.UserGroupId == e.UserGroupId && d.SellingPriceGroupId == 0 && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && d.CompanyId == obj.CompanyId && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                       && d.BranchId == obj.BranchId && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                           //                                       TotalSalesPaid = (from c in oConnectionContext.DbClsSales
                           //                                                         join d in oConnectionContext.DbClsPayment on c.SalesId equals d.Id
                           //                                                         where
                           //d.Type.ToLower() == "sales payment" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                           //                                                         select c.GrandTotal).DefaultIfEmpty().Sum(),
                           //                                       TotalSalesDue =
                           //                        oConnectionContext.DbClsSales.Where(c => c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                           //                        && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                           //                        (from c in oConnectionContext.DbClsSales
                           //                         join d in oConnectionContext.DbClsPayment on c.SalesId equals d.Id
                           //                         where
                           //d.Type.ToLower() == "sales payment" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                           //                         select c.GrandTotal).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSales > 0).ToList();
            }

            if (obj.UserGroupId != 0)
            {
                det = det.Where(a => a.UserGroupId == obj.UserGroupId).Select(a => a).ToList();
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
