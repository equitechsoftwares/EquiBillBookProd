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
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]

    public class BalanceSheetController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> TrialBalanceReport(ClsUserVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
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

            decimal Purchase = 0;
            if (obj.BranchId == 0)
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a =>
             //a.BranchId == obj.BranchId
             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
            DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                  DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) &&
                  a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a =>
            a.BranchId == obj.BranchId &&
            DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                  DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) &&
                  a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }


            decimal SalesReturn = 0;
            if (obj.BranchId == 0)
            {
                SalesReturn = (from a in oConnectionContext.DbClsSales
                               join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                               where
                                 //a.BranchId == obj.BranchId
                                 oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                               DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                               select b.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesReturn = (from a in oConnectionContext.DbClsSales
                               join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                               where
                               a.BranchId == obj.BranchId &&
                               DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false && a.IsCancelled == false
                               select b.GrandTotal).DefaultIfEmpty().Sum();
            }


            decimal PurchaseReturn = 0;
            if (obj.BranchId == 0)
            {
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchase
                                  join b in oConnectionContext.DbClsPurchaseReturn
    on a.PurchaseId equals b.PurchaseId
                                  where
                                    //a.BranchId == obj.BranchId
                                    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                                  DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                  select b.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchase
                                  join b in oConnectionContext.DbClsPurchaseReturn
    on a.PurchaseId equals b.PurchaseId
                                  where
                                  a.BranchId == obj.BranchId &&
                                  DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                  select b.GrandTotal).DefaultIfEmpty().Sum();

            }


            decimal Sales = 0;
            if (obj.BranchId == 0)
            {
                Sales = oConnectionContext.DbClsSales.Where(a =>
             //a.BranchId == obj.BranchId 
             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
            DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                Sales = oConnectionContext.DbClsSales.Where(a =>
          a.BranchId == obj.BranchId &&
          DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }

            List<ClsBalanceSheet> BalanceSheet = new List<ClsBalanceSheet>();
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Purchase", Credit = 0, Debit = Purchase });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Purchase Return", Credit = PurchaseReturn, Debit = 0 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Sales", Credit = 0, Debit = Sales });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Sales Return", Credit = SalesReturn, Debit = 0 });

            if (obj.BranchId == 0)
            {
                //BalanceSheet.AddRange(oConnectionContext.DbClsIncomeCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    Name = a.IncomeCategory,
                //    Credit = oConnectionContext.DbClsIncome.Where(aa =>
                //    //aa.BranchId == obj.BranchId 
                //     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == aa.BranchId)
                //    && aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.IncomeCategoryId == a.IncomeCategoryId).Select(aa => aa.Amount).DefaultIfEmpty().Sum()
                //}).ToList());

                //BalanceSheet.AddRange(oConnectionContext.DbClsExpenseCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    Name = a.ExpenseCategory,
                //    Credit = oConnectionContext.DbClsExpense.Where(aa =>
                //    //aa.BranchId == obj.BranchId 
                //      oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == aa.BranchId)
                //    && aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.ExpenseCategoryId == a.ExpenseCategoryId).Select(aa => aa.Amount).DefaultIfEmpty().Sum()
                //}).ToList());
            }
            else
            {
                //BalanceSheet.AddRange(oConnectionContext.DbClsIncomeCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    Name = a.IncomeCategory,
                //    Credit = oConnectionContext.DbClsIncome.Where(aa => aa.BranchId == obj.BranchId && aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.IncomeCategoryId == a.IncomeCategoryId).Select(aa => aa.Amount).DefaultIfEmpty().Sum()
                //}).ToList());

                //BalanceSheet.AddRange(oConnectionContext.DbClsExpenseCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    Name = a.ExpenseCategory,
                //    Credit = oConnectionContext.DbClsExpense.Where(aa => aa.BranchId == obj.BranchId && aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.ExpenseCategoryId == a.ExpenseCategoryId).Select(aa => aa.Amount).DefaultIfEmpty().Sum()
                //}).ToList());
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    BalanceSheet = BalanceSheet.OrderBy(a => a.Name),
                    //Branchs = userDetails.BranchIds
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ProfitLossReport(ClsItemVm obj)
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

            List<ClsBalanceSheet> BalanceSheet = new List<ClsBalanceSheet>();

            #region Purchase

            decimal Purchase = 0;
            if (obj.BranchId == 0)
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a =>
            //a.BranchId == obj.BranchId 
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                          DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                          DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                          .Select(a => a.Subtotal).DefaultIfEmpty().Sum();
            }
            else
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a =>
            a.BranchId == obj.BranchId &&
                          DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                          DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                          .Select(a => a.Subtotal).DefaultIfEmpty().Sum();
            }

            decimal PurchaseAdditionalCharges = 0;
            if (obj.BranchId == 0)
            {
                PurchaseAdditionalCharges = (from b in oConnectionContext.DbClsPurchaseAdditionalCharges
                                             join a in oConnectionContext.DbClsPurchase
                                             on b.PurchaseId equals a.PurchaseId
                                             where oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                            DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) 
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                             select b.AmountExcTax).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseAdditionalCharges = (from b in oConnectionContext.DbClsPurchaseAdditionalCharges
                                        join a in oConnectionContext.DbClsPurchase
                                        on b.PurchaseId equals a.PurchaseId
                                        where a.BranchId == obj.BranchId &&
                       DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                       DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                       && a.IsDeleted == false && a.IsCancelled == false
                                        && b.IsDeleted == false && b.IsActive == true
                                        select b.AmountExcTax).DefaultIfEmpty().Sum();
            }

            decimal PurchaseDiscount = 0;
            if (obj.BranchId == 0)
            {
                PurchaseDiscount = oConnectionContext.DbClsPurchase.Where(a =>
            //a.BranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                    DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                    DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                    .Select(a => a.Discount).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseDiscount = oConnectionContext.DbClsPurchase.Where(a =>
         a.BranchId == obj.BranchId &&
                 DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                 .Select(a => a.Discount).DefaultIfEmpty().Sum();
            }

            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Purchase (Exc. Tax, Discount & Additional Charges)", Balance = Purchase, Type = "Purchase", OrderNo = 1 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Additional Charges of Purchase (Exc. Tax)", Balance = PurchaseAdditionalCharges, Type = "Purchase", OrderNo = 3 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Discount on Purchase", Balance = PurchaseDiscount, Type = "Purchase", OrderNo = 6 });
            #endregion

            #region Purchase Return

            decimal PurchaseReturn = 0;
            if (obj.BranchId == 0)
            {
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchase
                                  join b in oConnectionContext.DbClsPurchaseReturn
on a.PurchaseId equals b.PurchaseId
                                  where
                                  //a.BranchId == obj.BranchId &&
                                  oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                            DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                  select b.Subtotal).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchase
                                  join b in oConnectionContext.DbClsPurchaseReturn
on a.PurchaseId equals b.PurchaseId
                                  where
                                  a.BranchId == obj.BranchId &&
                            DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                  select b.Subtotal).DefaultIfEmpty().Sum();
            }

            decimal PurchaseReturnAdditionalCharges = 0;
            if (obj.BranchId == 0)
            {
                PurchaseReturnAdditionalCharges = (from b in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                             join a in oConnectionContext.DbClsPurchaseReturn
                                             on b.PurchaseReturnId equals a.PurchaseReturnId
                                             where oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                            DbFunctions.TruncateTime(a.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                             select b.AmountExcTax).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseReturnAdditionalCharges = (from b in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                                   join a in oConnectionContext.DbClsPurchaseReturn
                                             on b.PurchaseReturnId equals a.PurchaseId
                                             where a.BranchId == obj.BranchId &&
                            DbFunctions.TruncateTime(a.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                             select b.AmountExcTax).DefaultIfEmpty().Sum();
            }

            decimal PurchaseReturnDiscount = 0;
            if (obj.BranchId == 0)
            {
                PurchaseReturnDiscount = (from a in oConnectionContext.DbClsPurchase
                                          join b in oConnectionContext.DbClsPurchaseReturn
  on a.PurchaseId equals b.PurchaseId
                                          where
                                          //a.BranchId == obj.BranchId &&
                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                                    DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                                    DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                          select b.Discount).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseReturnDiscount = (from a in oConnectionContext.DbClsPurchase
                                          join b in oConnectionContext.DbClsPurchaseReturn
  on a.PurchaseId equals b.PurchaseId
                                          where
                                          a.BranchId == obj.BranchId &&
                                    DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                                    DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                          select b.Discount).DefaultIfEmpty().Sum();
            }

            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Purchase Return (Exc. Tax, Discount & Additional Charges)", Balance = PurchaseReturn, Type = "Purchase Return", OrderNo = 1 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Additional Charges of Purchase (Exc. Tax)", Balance = PurchaseReturnAdditionalCharges, Type = "Purchase", OrderNo = 3 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Discount on Purchase Return", Balance = PurchaseReturnDiscount, Type = "Purchase Return", OrderNo = 3 });
            #endregion

            #region Sales

            decimal Sales = 0;
            if (obj.BranchId == 0)
            {
                Sales = oConnectionContext.DbClsSales.Where(a =>
            //a.BranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                          DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                          DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                          .Select(a => a.Subtotal).DefaultIfEmpty().Sum();
            }
            else
            {
                Sales = oConnectionContext.DbClsSales.Where(a =>
         a.BranchId == obj.BranchId &&
                       DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                       DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                       .Select(a => a.Subtotal).DefaultIfEmpty().Sum();
            }

            decimal SalesAdditionalCharges = 0;
            if (obj.BranchId == 0)
            {
                SalesAdditionalCharges = (from b in oConnectionContext.DbClsSalesAdditionalCharges
                                          join a in oConnectionContext.DbClsSales
                                             on b.SalesId equals a.SalesId
                                          where oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                            DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                             select b.AmountExcTax).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesAdditionalCharges = (from b in oConnectionContext.DbClsSalesAdditionalCharges
                                          join a in oConnectionContext.DbClsSales
                                             on b.SalesId equals a.SalesId
                                          where a.BranchId == obj.BranchId &&
                            DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                             select b.AmountExcTax).DefaultIfEmpty().Sum();
            }

            decimal SalesDiscount = 0;
            if (obj.BranchId == 0)
            {
                SalesDiscount = oConnectionContext.DbClsSales.Where(a =>
            //a.BranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                    DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                    DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                    .Select(a => a.Discount).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesDiscount = oConnectionContext.DbClsSales.Where(a =>
         a.BranchId == obj.BranchId &&
                 DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false)
                 .Select(a => a.Discount).DefaultIfEmpty().Sum();
            }

            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Sales (Exc. Tax, Discount & Additional Charges)", Balance = Sales, Type = "Sales", OrderNo = 1 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Additional Charges of Sales (Exc. Tax)", Balance = SalesAdditionalCharges, Type = "Sales", OrderNo = 3 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Discount on Sales", Balance = SalesDiscount, Type = "Sales", OrderNo = 6 });
            #endregion

            #region Sales Return

            decimal SalesReturn = 0;
            if (obj.BranchId == 0)
            {
                SalesReturn = (from a in oConnectionContext.DbClsSales
                               join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                               where
                               //a.BranchId == obj.BranchId &&
                               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                         DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                         DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                               select b.Subtotal).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesReturn = (from a in oConnectionContext.DbClsSales
                               join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                               where
                               a.BranchId == obj.BranchId &&
                         DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                         DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                               select b.Subtotal).DefaultIfEmpty().Sum();
            }

            decimal SalesReturnAdditionalCharges = 0;
            if (obj.BranchId == 0)
            {
                SalesReturnAdditionalCharges = (from b in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                                join a in oConnectionContext.DbClsSalesReturn
                                             on b.SalesReturnId equals a.SalesReturnId
                                                where oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                            DbFunctions.TruncateTime(a.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                          select b.AmountExcTax).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesReturnAdditionalCharges = (from b in oConnectionContext.DbClsSalesReturnAdditionalCharges
                                                join a in oConnectionContext.DbClsSalesReturn
                                             on b.SalesReturnId equals a.SalesReturnId
                                                where a.BranchId == obj.BranchId &&
                            DbFunctions.TruncateTime(a.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                            DbFunctions.TruncateTime(a.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                            && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsDeleted == false && b.IsActive == true
                                          select b.AmountExcTax).DefaultIfEmpty().Sum();
            }

            decimal SalesReturnDiscount = 0;
            if (obj.BranchId == 0)
            {
                SalesReturnDiscount = (from a in oConnectionContext.DbClsSales
                                       join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                                       where
                                       //a.BranchId == obj.BranchId &&
                                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                                 DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                                 DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                       select b.Discount).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesReturnDiscount = (from a in oConnectionContext.DbClsSales
                                       join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                                       where
                                       a.BranchId == obj.BranchId &&
                                 DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                                 DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                       select b.Discount).DefaultIfEmpty().Sum();
            }

            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Sales Return (Exc. Tax, Discount & Additional Charges)", Balance = SalesReturn, Type = "Sales Return", OrderNo = 1 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Additional Charges of Sales Return (Exc. Tax)", Balance = SalesReturnAdditionalCharges, Type = "Sales Return", OrderNo = 2 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Discount on Sales Return", Balance = SalesReturnDiscount, Type = "Sales Return", OrderNo = 3 });
            #endregion

            #region Expense
            dynamic expense = null;
        //    if (obj.BranchId == 0)
        //    {
        //        expense = oConnectionContext.DbClsExpense.Where(aa =>
        //    //aa.BranchId == obj.BranchId &&
        //    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == aa.BranchId) &&
        //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
        //    DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
        //    ).Select(aa => aa.Amount).DefaultIfEmpty().Sum();
        //    }
        //    else
        //    {
        //        expense = oConnectionContext.DbClsExpense.Where(aa =>
        // aa.BranchId == obj.BranchId &&
        // aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
        // DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
        // ).Select(aa => aa.Amount).DefaultIfEmpty().Sum();
        //    }
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Expense", Balance = expense, Type = "Expense", OrderNo = 1 });
            #endregion

            #region Income
            dynamic income = null;
        //    if (obj.BranchId == 0)
        //    {
        //        income = oConnectionContext.DbClsIncome.Where(aa =>
        //    //aa.BranchId == obj.BranchId &&
        //    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == aa.BranchId) &&
        //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
        //    DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
        //    ).Select(aa => aa.Amount).DefaultIfEmpty().Sum();
        //    }
        //    else
        //    {
        //         income = oConnectionContext.DbClsIncome.Where(aa =>
        //    aa.BranchId == obj.BranchId &&
        //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
        //    DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
        //    ).Select(aa => aa.Amount).DefaultIfEmpty().Sum();

        //    }
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Income", Balance = income, Type = "Income", OrderNo = 1 });
            #endregion

            #region Stock

            dynamic stockAdjustmentCredit = null;
            if (obj.BranchId == 0)
            {
                stockAdjustmentCredit = oConnectionContext.DbClsStockAdjustment.Where(aa =>
            //aa.BranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.BranchId) &&
            aa.IsDeleted == false &&
            DbFunctions.TruncateTime(aa.AdjustmentDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.AdjustmentDate) <= DbFunctions.TruncateTime(obj.ToDate)
            && aa.AdjustmentType.ToLower() == "credit").Select(aa => aa.TotalAmount).DefaultIfEmpty().Sum();
            }
            else
            {
                stockAdjustmentCredit = oConnectionContext.DbClsStockAdjustment.Where(aa =>
            aa.BranchId == obj.BranchId &&
            aa.IsDeleted == false &&
            DbFunctions.TruncateTime(aa.AdjustmentDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.AdjustmentDate) <= DbFunctions.TruncateTime(obj.ToDate)
            && aa.AdjustmentType.ToLower() == "credit").Select(aa => aa.TotalAmount).DefaultIfEmpty().Sum();
            }

            dynamic stockAdjustmentdebit = null;
            if (obj.BranchId == 0)
            {
                stockAdjustmentdebit = oConnectionContext.DbClsStockAdjustment.Where(aa =>
            //aa.BranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.BranchId) &&
            aa.IsDeleted == false &&
            DbFunctions.TruncateTime(aa.AdjustmentDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.AdjustmentDate) <= DbFunctions.TruncateTime(obj.ToDate)
            && aa.AdjustmentType.ToLower() == "debit").Select(aa => aa.TotalAmount).DefaultIfEmpty().Sum();
            }
            else
            {
                stockAdjustmentdebit = oConnectionContext.DbClsStockAdjustment.Where(aa =>
         aa.BranchId == obj.BranchId &&
         aa.IsDeleted == false &&
         DbFunctions.TruncateTime(aa.AdjustmentDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.AdjustmentDate) <= DbFunctions.TruncateTime(obj.ToDate)
         && aa.AdjustmentType.ToLower() == "debit").Select(aa => aa.TotalAmount).DefaultIfEmpty().Sum();
            }

            dynamic StockRecovered = null;
            if (obj.BranchId == 0)
            {
                StockRecovered = oConnectionContext.DbClsStockAdjustment.Where(aa =>
            //aa.BranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.BranchId) &&
            aa.IsDeleted == false &&
             DbFunctions.TruncateTime(aa.AdjustmentDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.AdjustmentDate) <= DbFunctions.TruncateTime(obj.ToDate)
             && aa.AdjustmentType.ToLower() == "credit").Select(aa => aa.TotalAmountRecovered).DefaultIfEmpty().Sum();
            }
            else
            {
                StockRecovered = oConnectionContext.DbClsStockAdjustment.Where(aa =>
         aa.BranchId == obj.BranchId &&
         aa.IsDeleted == false &&
          DbFunctions.TruncateTime(aa.AdjustmentDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.AdjustmentDate) <= DbFunctions.TruncateTime(obj.ToDate)
          && aa.AdjustmentType.ToLower() == "credit").Select(aa => aa.TotalAmountRecovered).DefaultIfEmpty().Sum();
            }

            dynamic StockTransfer = null;
            if (obj.BranchId == 0)
            {
                StockTransfer = oConnectionContext.DbClsStockTransfer.Where(aa =>
            //aa.FromBranchId == obj.BranchId &&
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.FromBranchId) &&
            aa.IsDeleted == false &&
         DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
         ).Select(aa => aa.TotalAmount).DefaultIfEmpty().Sum();
            }
            else
            {
                StockTransfer = oConnectionContext.DbClsStockTransfer.Where(aa =>
         aa.FromBranchId == obj.BranchId &&
         aa.IsDeleted == false &&
      DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
      ).Select(aa => aa.TotalAmount).DefaultIfEmpty().Sum();
            }

        //    dynamic StockTransferShippingCharge = null;
        //    if (obj.BranchId == 0)
        //    {
        //        StockTransferShippingCharge = oConnectionContext.DbClsStockTransfer.Where(aa =>
        //    //aa.FromBranchId == obj.BranchId &&
        //    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.FromBranchId) &&
        //    aa.IsDeleted == false &&
        //     DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
        //     ).Select(aa => aa.ShippingCharge).DefaultIfEmpty().Sum();
        //    }
        //    else
        //    {
        //        StockTransferShippingCharge = oConnectionContext.DbClsStockTransfer.Where(aa =>
        // aa.FromBranchId == obj.BranchId &&
        // aa.IsDeleted == false &&
        //  DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
        //  ).Select(aa => aa.ShippingCharge).DefaultIfEmpty().Sum();
        //    }            

            BalanceSheet.Add(new ClsBalanceSheet { Name = "Opening Stock (By purchase price)", Balance = 0, Type = "Stock", OrderNo = 1 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Opening Stock (By sales price)", Balance = 0, Type = "Stock", OrderNo = 2 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Closing Stock (By purchase price)", Balance = 0, Type = "Stock", OrderNo = 3 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Closing Stock (By sales price)", Balance = 0, Type = "Stock", OrderNo = 4 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Stock Adjustment (Credit)", Balance = stockAdjustmentCredit, Type = "Stock", OrderNo = 5 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Stock Adjustment (Debit)", Balance = stockAdjustmentdebit, Type = "Stock", OrderNo = 6 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Stock Transfer", Balance = StockTransfer, Type = "Stock", OrderNo = 7 });
            //BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Stock Transfer Shipping Charge", Balance = StockTransferShippingCharge, Type = "Stock", OrderNo = 8 });
            //BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Stock Wastage", Balance = StockWastage, Type = "Stock", OrderNo = 9 });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Total Stock Recovered", Balance = StockRecovered, Type = "Stock", OrderNo = 10 });
            #endregion

            BalanceSheet.Add(new ClsBalanceSheet { Name = "Gross Profit", Balance = Sales - Purchase, Type = "Gross Profit", OrderNo = 1 });
            BalanceSheet.Add(new ClsBalanceSheet
            {
                Name = "Net Profit",
                Balance = (((Sales - Purchase) + SalesAdditionalCharges + StockRecovered + PurchaseDiscount + stockAdjustmentdebit) -
                (stockAdjustmentdebit + expense + PurchaseAdditionalCharges + SalesDiscount)),
                Type = "Gross Profit",
                OrderNo = 1
            });

            List<ClsPurchaseSales> PurchaseSales;
            if (obj.BranchId == 0)
            {
                PurchaseSales = oConnectionContext.DbClsCategory.Where(a => a.IsActive == true && a.IsDeleted == false 
                && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
                {
                    Name = a.Category,
                    TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                     join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                     join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                     where
                                     //b.BranchId == obj.BranchId &&
                                     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                     d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                     && DbFunctions.TruncateTime(b.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                     select b.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalSales = (from b in oConnectionContext.DbClsSales
                                  join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                  join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                  where
                                  //b.BranchId == obj.BranchId &&
                                  oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                  d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                  && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                  select b.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                                          join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                          join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                          where
                                          //b.BranchId == obj.BranchId &&
                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                          d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                          && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                          select b.TotalQuantity).DefaultIfEmpty().Sum(),
                    TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                        join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                        join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                        where
                                        //b.BranchId == obj.BranchId &&
                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                        d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                        && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                        select b.GrandTotal).DefaultIfEmpty().Sum() - 
                                        (from b in oConnectionContext.DbClsPurchase
                                        join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                        join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                        where
                                        //b.BranchId == obj.BranchId &&
                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId) &&
                                        d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                        && DbFunctions.TruncateTime(b.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                        select b.GrandTotal).DefaultIfEmpty().Sum()
                }).ToList();
            }
            else
            {
                PurchaseSales = oConnectionContext.DbClsCategory.Where(a => a.IsActive == true && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new ClsPurchaseSales
                {
                    Name = a.Category,
                    TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                     join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                     join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                     where
                                     b.BranchId == obj.BranchId &&
                                     d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false 
                                     && DbFunctions.TruncateTime(b.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                     select b.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalSales = (from b in oConnectionContext.DbClsSales
                                  join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                  join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                  where
                                  b.BranchId == obj.BranchId &&
                                  d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                  && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                  select b.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                                          join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                          join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                          where
                                          b.BranchId == obj.BranchId &&
                                          d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                          && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                          select b.TotalQuantity).DefaultIfEmpty().Sum(),
                    TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                        join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                        join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                        where
                                        b.BranchId == obj.BranchId &&
                                        d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false 
                                        && DbFunctions.TruncateTime(b.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                        select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                       join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                       join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                                       where
                                                                                       b.BranchId == obj.BranchId &&
                                                                                       d.CategoryId == a.CategoryId && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && c.IsDeleted == false
                                                                                       && DbFunctions.TruncateTime(b.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(b.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate)
                                                                                       select b.GrandTotal).DefaultIfEmpty().Sum()
                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    BalanceSheet = BalanceSheet.OrderBy(a => a.Name),
                    //Branchs = userDetails.BranchIds,
                    PurchaseSales = PurchaseSales
                }
            };

            return await Task.FromResult(Ok(data));

        }

        public async Task<IHttpActionResult> BalanceSheetReport(ClsUserVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
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

            decimal Purchase = 0;
            if (obj.BranchId == 0)
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a =>
            //a.BranchId == obj.BranchId
            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
           DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) &&
                 a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a =>
            a.BranchId == obj.BranchId &&
           DbFunctions.TruncateTime(a.PurchaseDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(a.PurchaseDate) <= DbFunctions.TruncateTime(obj.ToDate) &&
                 a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal SalesReturn = 0;
            if (obj.BranchId == 0)
            {
                SalesReturn = (from a in oConnectionContext.DbClsSales
                               join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                               where
                               //a.BranchId == obj.BranchId
                               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                               &&
                               DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                               select b.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                SalesReturn = (from a in oConnectionContext.DbClsSales
                               join b in oConnectionContext.DbClsSalesReturn
on a.SalesId equals b.SalesId
                               where
                               a.BranchId == obj.BranchId &&
                               DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                               select b.GrandTotal).DefaultIfEmpty().Sum();
            }


            decimal PurchaseReturn = 0;
            if (obj.BranchId == 0)
            {
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchase
                                  join b in oConnectionContext.DbClsPurchaseReturn
    on a.PurchaseId equals b.PurchaseId
                                  where
                                  //a.BranchId == obj.BranchId
                                  oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                                  DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                  select b.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                PurchaseReturn = (from a in oConnectionContext.DbClsPurchase
                                  join b in oConnectionContext.DbClsPurchaseReturn
    on a.PurchaseId equals b.PurchaseId
                                  where
                                  a.BranchId == obj.BranchId &&
                                  DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(obj.FromDate) &&
                 DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false
                                  select b.GrandTotal).DefaultIfEmpty().Sum();
            }


            decimal Sales = 0;
            if (obj.BranchId == 0)
            {
                Sales = oConnectionContext.DbClsSales.Where(a =>
             //a.BranchId == obj.BranchId
             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
            DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                Sales = oConnectionContext.DbClsSales.Where(a =>
        a.BranchId == obj.BranchId &&
        DbFunctions.TruncateTime(a.SalesDate) >= DbFunctions.TruncateTime(obj.FromDate) &&
DbFunctions.TruncateTime(a.SalesDate) <= DbFunctions.TruncateTime(obj.ToDate) && a.IsDeleted == false && a.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }


            List<ClsBalanceSheet> BalanceSheet = new List<ClsBalanceSheet>();
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Purchase", Credit = 0, Debit = Purchase, IsDebit = true });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Purchase Return", Credit = PurchaseReturn, Debit = 0, IsDebit = false });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Sales", Credit = 0, Debit = Sales, IsDebit = true });
            BalanceSheet.Add(new ClsBalanceSheet { Name = "Sales Return", Credit = SalesReturn, Debit = 0, IsDebit = false });

            if (obj.BranchId == 0)
            {
                //BalanceSheet.AddRange(oConnectionContext.DbClsIncomeCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    IsDebit = false,
                //    Name = a.IncomeCategory,
                //    BalanceSheetDetails = oConnectionContext.DbClsIncome.Where(aa =>
                //    //aa.BranchId == obj.BranchId 
                //     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == aa.BranchId) &&
                //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.IncomeCategoryId == a.IncomeCategoryId).Select(aa => new ClsBalanceSheetDetails
                //{
                //    Name = "Income",//aa.IncomeFor,
                //    Credit = aa.Amount
                //}).ToList()
                //}).ToList());

                //BalanceSheet.AddRange(oConnectionContext.DbClsExpenseCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    IsDebit = true,
                //    Name = a.ExpenseCategory,
                //    BalanceSheetDetails = oConnectionContext.DbClsExpense.Where(aa =>
                //    //aa.BranchId == obj.BranchId 
                //     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == aa.BranchId) &&
                //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.ExpenseCategoryId == a.ExpenseCategoryId).Select(aa => new ClsBalanceSheetDetails
                //{
                //    Name = "Expense",//aa.ExpenseFor,
                //    Debit = aa.Amount
                //}).ToList()
                //}).ToList());
            }
            else
            {
                //BalanceSheet.AddRange(oConnectionContext.DbClsIncomeCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    IsDebit = false,
                //    Name = a.IncomeCategory,
                //    BalanceSheetDetails = oConnectionContext.DbClsIncome.Where(aa =>
                //    aa.BranchId == obj.BranchId &&
                //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.IncomeCategoryId == a.IncomeCategoryId).Select(aa => new ClsBalanceSheetDetails
                //{
                //    Name = "Income",//aa.IncomeFor,
                //    Credit = aa.Amount
                //}).ToList()
                //}).ToList());

                //BalanceSheet.AddRange(oConnectionContext.DbClsExpenseCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Select(a => new ClsBalanceSheet
                //{
                //    IsDebit = true,
                //    Name = a.ExpenseCategory,
                //    BalanceSheetDetails = oConnectionContext.DbClsExpense.Where(aa =>
                //    aa.BranchId == obj.BranchId &&
                //    aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false &&
                //DbFunctions.TruncateTime(aa.Date) >= DbFunctions.TruncateTime(obj.FromDate) && DbFunctions.TruncateTime(aa.Date) <= DbFunctions.TruncateTime(obj.ToDate)
                //&& aa.ExpenseCategoryId == a.ExpenseCategoryId).Select(aa => new ClsBalanceSheetDetails
                //{
                //    Name = "Expense",//aa.ExpenseFor,
                //    Debit = aa.Amount
                //}).ToList()
                //}).ToList());
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    BalanceSheet = BalanceSheet.OrderBy(a => a.Name),
                    //Branchs = userDetails.BranchIds,
                }
            };

            return await Task.FromResult(Ok(data));

        }

    }
}
