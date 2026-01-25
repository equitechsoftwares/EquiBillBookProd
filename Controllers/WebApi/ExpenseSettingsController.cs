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
    public class ExpenseSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> ExpenseSetting(ClsExpenseSettingsVm obj)
        {
            var det = oConnectionContext.DbClsExpenseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                ExpenseSettingsId = a.ExpenseSettingsId,
                a.MileageAccountId,
                a.EnableMileage,
                a.UnitId,
                UnitName = oConnectionContext.DbClsUnit.Where(b=>b.CompanyId == obj.CompanyId && b.UnitId== a.UnitId).Select(b=>b.UnitName).FirstOrDefault(),
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ExpenseSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseSettingsUpdate(ClsExpenseSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long ExpenseSettingsId = oConnectionContext.DbClsExpenseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.ExpenseSettingsId).FirstOrDefault();

                ClsExpenseSettings oClsExpenseSettings = new ClsExpenseSettings()
                {
                    ExpenseSettingsId = ExpenseSettingsId,
                    MileageAccountId = obj.MileageAccountId,
                    EnableMileage = obj.EnableMileage,
                    UnitId = obj.UnitId,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };

                oConnectionContext.DbClsExpenseSettings.Attach(oClsExpenseSettings);
                oConnectionContext.Entry(oClsExpenseSettings).Property(x => x.ExpenseSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsExpenseSettings).Property(x => x.MileageAccountId).IsModified = true;
                oConnectionContext.Entry(oClsExpenseSettings).Property(x => x.EnableMileage).IsModified = true;
                oConnectionContext.Entry(oClsExpenseSettings).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oClsExpenseSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsExpenseSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Expense\" updated",
                    Id = oClsExpenseSettings.ExpenseSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Expense Info updated successfully.",
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
