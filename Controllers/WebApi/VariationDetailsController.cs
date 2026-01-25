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
    public class VariationDetailsController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> VariationDetailsDelete(ClsVariationDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                {
                    VariationDetailsId = obj.VariationDetailsId,
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
                data = new
                {
                    Status = 1,
                    Message = "Deleted successfully",
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
