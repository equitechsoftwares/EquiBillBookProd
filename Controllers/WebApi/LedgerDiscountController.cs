using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class LedgerDiscountController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public async Task<IHttpActionResult> InsertLedgerDiscount(ClsLedgerDiscountVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            long PrefixUserMapId = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                string PaymentStatus = "";
                decimal GrandTotal = 0;
                decimal previousPayments = 0;
                long BranchId = 0;
                int IsDebit = 0;

                if (obj.Amount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                    isError = true;
                }

                if (obj.PaymentDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentDate" });
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

                ClsLedgerDiscount oClsLedgerDiscount = new ClsLedgerDiscount()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    Amount = obj.Amount,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    Id = obj.Id,
                    Type = obj.Type,
                };

                oConnectionContext.DbClsLedgerDiscount.Add(oClsLedgerDiscount);
                oConnectionContext.SaveChanges();

                string query = "update \"tblUser\" set \"LedgerDiscount\"=\"LedgerDiscount\"+" + obj.Amount + " where \"UserId\"=" + obj.Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.Type + " Payment",
                    CompanyId = obj.CompanyId,
                    Description = "added ledger discount of " + obj.Amount,
                    Id = oClsLedgerDiscount.LedgerDiscountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Ledger discount created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> LedgerDiscounts(ClsLedgerDiscountVm obj)
        {
            var det = oConnectionContext.DbClsLedgerDiscount.Where(b => b.Id == obj.Id && b.Type.ToLower() == obj.Type.ToLower() && b.IsDeleted == false).Select(b => new
            {
                b.LedgerDiscountId,
                b.PaymentDate,
                b.Notes,
                b.Amount,
                b.AddedOn,
                b.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).OrderByDescending(b => b.LedgerDiscountId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    LedgerDiscounts = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> LedgerDiscount(ClsLedgerDiscountVm obj)
        //{
        //    var det = oConnectionContext.DbClsLedgerDiscount.Where(b => b.LedgerDiscountId == obj.LedgerDiscountId && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
        //    {
        //        b.PaymentDate,
        //        b.Notes,
        //        b.Amount,
        //        b.AddedOn,
        //        b.ModifiedOn,
        //        AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.AddedBy).Select(z => z.Username).FirstOrDefault(),
        //        ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == b.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
        //    }).FirstOrDefault();

        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            LedgerDiscount = det,
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> LedgerDiscountDelete(ClsLedgerDiscountVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsLedgerDiscount oClsLedgerDiscount = new ClsLedgerDiscount()
                {
                    LedgerDiscountId = obj.LedgerDiscountId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsLedgerDiscount.Attach(oClsLedgerDiscount);
                oConnectionContext.Entry(oClsLedgerDiscount).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsLedgerDiscount).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsLedgerDiscount).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var LedgerDiscountDetails = oConnectionContext.DbClsLedgerDiscount.Where(a => a.LedgerDiscountId == obj.LedgerDiscountId).Select(a =>
                new
                {
                    a.Id,
                    a.Type,
                    a.Amount,
                }).FirstOrDefault();

                string query = "update \"tblUser\" set \"LedgerDiscount\"=\"LedgerDiscount\"-" + LedgerDiscountDetails.Amount + " where \"UserId\"=" + LedgerDiscountDetails.Id;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = LedgerDiscountDetails.Type + " Payment",
                    CompanyId = obj.CompanyId,
                    Description = "deleted payment of " + LedgerDiscountDetails.Amount,
                    Id = oClsLedgerDiscount.LedgerDiscountId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Ledger Discount deleted successfully",
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
