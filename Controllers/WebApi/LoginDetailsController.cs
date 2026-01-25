using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class LoginDetailsController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> LoginDetailsReport(ClsUserVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
                BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
              && b.IsDeleted == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
               oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            }).FirstOrDefault();

            if (obj.BranchId == 0)
            {
                obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            }

            var Users = (from a in oConnectionContext.DbClsUser
                         join b in oConnectionContext.DbClsUserBranchMap
                            on a.UserId equals b.UserId
                         where a.IsDeleted == false && a.CompanyId == obj.CompanyId
                            && b.BranchId == obj.BranchId && a.UserType.ToLower() == "user"
                         select new
                         {
                             a.Name,
                             a.UserId,
                             a.MobileNo,
                             a.Username
                         });

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            var det1 = (from a in oConnectionContext.DbClsUser
                        join b in oConnectionContext.DbClsUserBranchMap
                        on a.UserId equals b.UserId
                        join c in oConnectionContext.DbClsLoginDetails
                        on a.UserId equals c.AddedBy
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.BranchId == obj.BranchId
                        select new
                        {
                            a.UserId,
                            c.ModifiedOn,
                            a.Username,
                            c.LoginDetailsId,
                            a.Name,
                            a.MobileNo,
                            a.EmailId,
                            c.AddedOn,
                            a.UserRoleId,
                            RoleName = oConnectionContext.DbClsRole.Where(d => d.RoleId == a.UserRoleId).Select(d => d.RoleName).FirstOrDefault(),
                            a.UserType,
                            c.IsLoggedOut
                        }).ToList();

            if (userDetails.IsCompany == true)
            {
                det1 = det1.Concat((from a in oConnectionContext.DbClsUser
                                    join c in oConnectionContext.DbClsLoginDetails
                                    on a.UserId equals c.AddedBy
                                    where a.UserId == obj.CompanyId && a.IsDeleted == false
                                    && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate && DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                    select new
                                    {
                                        a.UserId,
                                        c.ModifiedOn,
                                        a.Username,
                                        c.LoginDetailsId,
                                        a.Name,
                                        a.MobileNo,
                                        a.EmailId,
                                        c.AddedOn,
                                        a.UserRoleId,
                                        RoleName = "Company",
                                        a.UserType,
                                        c.IsLoggedOut
                                    })).ToList();

               Users =  Users.Concat(oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => new
                {
                    a.Name,
                    a.UserId,
                    a.MobileNo,
                    a.Username
                }));
            }

            var det = det1.Where(a => a.IsLoggedOut == true).Select(a => new
            {
                a.UserId,
                a.UserRoleId,
                AddedOn = Convert.ToDateTime(a.ModifiedOn),
                a.Username,
                a.LoginDetailsId,
                a.Name,
                a.MobileNo,
                a.EmailId,
                RoleName = a.RoleName,
                a.UserType,
                IsLoggedOut = true
            }).Concat(det1.Select(a => new
            {
                a.UserId,
                a.UserRoleId,
                AddedOn = a.AddedOn,
                a.Username,
                a.LoginDetailsId,
                a.Name,
                a.MobileNo,
                a.EmailId,
                RoleName = a.RoleName,
                a.UserType,
                IsLoggedOut = false
            })).ToList();

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.UserId == obj.UserId).ToList();
            }

            if (obj.UserRoleId != 0)
            {
                det = det.Where(a => a.UserRoleId == obj.UserRoleId).ToList();
            }

            if (obj.Type != "" && obj.Type != null)
            {
                bool IsLoggedOut = Convert.ToBoolean(obj.Type);
                det = det.Where(a => a.IsLoggedOut == IsLoggedOut).ToList();
            }

            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.AddedOn.Date >= obj.FromDate && a.AddedOn.Date <= obj.ToDate).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    LoginDetails = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    Branchs = userDetails.BranchIds,
                    Users = Users,
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
