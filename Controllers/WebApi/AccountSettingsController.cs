using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class AccountSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;
        public async Task<IHttpActionResult> AccountSetting(ClsAccountSettingsVm obj)
        {
            var det = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.AccountSettingsId,
                a.MigrationDate
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
