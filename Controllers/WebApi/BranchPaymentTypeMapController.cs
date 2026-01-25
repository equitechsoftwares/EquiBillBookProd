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
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class BranchPaymentTypeMapController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> FetchAccountMapped(ClsBranchPaymentTypeMapVm obj)
        {
            var det = oConnectionContext.DbClsBranchPaymentTypeMap.Where(a => a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true && a.BranchId== obj.BranchId && a.PaymentTypeId == obj.PaymentTypeId).Select(a => new
            {
                AccountId = a.AccountId,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BranchPaymentTypeMap = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
