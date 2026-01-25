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
    public class WebsiteViewerController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        [AllowAnonymous]
        public async Task<IHttpActionResult> InsertWebsiteViewer(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain).Select(a => a.CompanyId).FirstOrDefault();

            long d = oConnectionContext.DbClsWebsiteViewer.Where(a => a.IpAddress == obj.IpAddress && a.Under== obj.Under).Count();
            if (d == 0)
            {
                ClsWebsiteViewer oClsWebsiteViewer = new ClsWebsiteViewer()
                {
                    IpAddress = obj.IpAddress,
                    AddedOn = oCommonController.CurrentDate(1),
                    Under = obj.Under
                };
                oConnectionContext.DbClsWebsiteViewer.Add(oClsWebsiteViewer);
                oConnectionContext.SaveChanges();
            }

            data = new
            {
                Status = 1,
                Message = "",
                Data = new
                {
                }
            };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
