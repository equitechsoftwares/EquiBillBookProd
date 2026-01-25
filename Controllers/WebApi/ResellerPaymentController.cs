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
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class ResellerPaymentController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;
        public async Task<IHttpActionResult> ResellerRenew(ClsResellerPaymentVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                obj.ReferenceNo = DateTime.Now.ToFileTime().ToString();

                ClsResellerPayment oClsResellerPayment = new ClsResellerPayment()
                {
                    PaymentDate = CurrentDate,
                    IsActive = true,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    Amount = obj.Amount,
                    PayableAmount = obj.PayableAmount,
                    Notes = obj.Notes,
                    ReferenceNo = obj.ReferenceNo,
                    CompanyId = obj.UserId,
                    IsDebit = obj.IsDebit,
                    Month = CurrentDate.Month,
                    Year = CurrentDate.Year,
                    Type = 1
                };
                oConnectionContext.DbClsResellerPayment.Add(oClsResellerPayment);
                oConnectionContext.SaveChanges();

                var expDate = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.ExpiryDate).FirstOrDefault().AddYears(1);

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    ExpiryDate = expDate
                };
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.ExpiryDate).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Renewed successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ResellerPaymentInsert(ClsResellerPaymentVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                obj.ReferenceNo = DateTime.Now.ToFileTime().ToString();

                string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.UserType).FirstOrDefault();
                if(UserType.ToLower() == "reseller")
                {
                    obj.IsDebit = true;
                }
                else
                {
                    obj.IsDebit = false;
                }
                ClsResellerPayment oClsResellerPayment = new ClsResellerPayment()
                {
                    PaymentDate = CurrentDate,
                    IsActive = true,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    Amount = obj.Amount,
                    PayableAmount = obj.Amount,
                    Notes = obj.Notes,
                    ReferenceNo = obj.ReferenceNo,
                    CompanyId = obj.UserId,
                    IsDebit = obj.IsDebit,
                    Month = obj.Month,
                    Year = obj.Year,
                    Type = 2
                };
                oConnectionContext.DbClsResellerPayment.Add(oClsResellerPayment);
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Paid successfully",
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
