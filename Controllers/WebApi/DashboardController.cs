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
    public class DashboardController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> Dashboard(ClsUserVm obj)
        {
            decimal TotalPurchase = 0;
            if (obj.BranchId == 0)
            {
                TotalPurchase = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
              //&& a.BranchId == obj.BranchId 
              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
             && a.Status.ToLower() != "draft").Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalPurchase = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
             && a.BranchId == obj.BranchId && a.Status.ToLower() != "draft").Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal TotalSales = 0;
            if (obj.BranchId == 0)
            {
                TotalSales = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
              //&& a.BranchId == obj.BranchId
              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")
             ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalSales = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
             && a.BranchId == obj.BranchId && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal TotalPos = 0;
            if (obj.BranchId == 0)
            {
                TotalPos = oConnectionContext.DbClsSales.Where(a => (a.Status != "Draft" && a.Status != "Hold") && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
              //&& a.BranchId == obj.BranchId
              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && a.SalesType.ToLower() == "pos"
             ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalPos = oConnectionContext.DbClsSales.Where(a => (a.Status != "Draft" && a.Status != "Hold") && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
             && a.BranchId == obj.BranchId && a.SalesType.ToLower() == "pos").Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal TotalPurchaseDue = 0;
            if (obj.BranchId == 0)
            {
                TotalPurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //&& a.BranchId == obj.BranchId 
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
           && a.Status.ToLower() != "draft").Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
           (from a in oConnectionContext.DbClsPurchase
            join b in oConnectionContext.DbClsSupplierPayment
on a.PurchaseId equals b.PurchaseId
            where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
 //&& a.BranchId == obj.BranchId 
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
&& a.Status.ToLower() != "draft" && b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false &&
b.CompanyId == obj.CompanyId
            select b.Amount).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalPurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
           && a.BranchId == obj.BranchId && a.Status.ToLower() != "draft").Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
           (from a in oConnectionContext.DbClsPurchase
            join b in oConnectionContext.DbClsSupplierPayment
on a.PurchaseId equals b.PurchaseId
            where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
&& a.BranchId == obj.BranchId && a.Status.ToLower() != "draft" && b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false &&
b.CompanyId == obj.CompanyId
            select b.Amount).DefaultIfEmpty().Sum();
            }

            decimal TotalSalesDue = 0;
            if (obj.BranchId == 0)
            {
                TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")
           ).Select(a => a.GrandTotal - a.WriteOffAmount).DefaultIfEmpty().Sum() -
                ((from a in oConnectionContext.DbClsSales
                 join b in oConnectionContext.DbClsCustomerPayment
     on a.SalesId equals b.SalesId
                 where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
     && b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false &&
     b.CompanyId == obj.CompanyId
                 && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")
                  select b.Amount).DefaultIfEmpty().Sum() -
                 (from a in oConnectionContext.DbClsSales
                  join b in oConnectionContext.DbClsCustomerPayment
      on a.SalesId equals b.SalesId
                  where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
      && b.Type.ToLower() == "change return" && b.IsDeleted == false && b.IsCancelled == false &&
      b.CompanyId == obj.CompanyId
                  && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")
                  select b.Amount).DefaultIfEmpty().Sum());
            }
            else
            {
                TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
           && a.BranchId == obj.BranchId && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")).Select(a => a.GrandTotal-a.WriteOffAmount).DefaultIfEmpty().Sum() -
                ((from a in oConnectionContext.DbClsSales
                 join b in oConnectionContext.DbClsCustomerPayment
     on a.SalesId equals b.SalesId
                 where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
     && a.BranchId == obj.BranchId && b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false &&
     b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")
                 select b.Amount).DefaultIfEmpty().Sum() -
                 (from a in oConnectionContext.DbClsSales
                  join b in oConnectionContext.DbClsCustomerPayment
      on a.SalesId equals b.SalesId
                  where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
      && a.BranchId == obj.BranchId && b.Type.ToLower() == "change return" && b.IsDeleted == false && b.IsCancelled == false &&
      b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && (a.SalesType.ToLower() == "sales" || a.SalesType.ToLower() == "bill of supply")
                  select b.Amount).DefaultIfEmpty().Sum());
            }

            decimal TotalPosDue = 0;
            if (obj.BranchId == 0)
            {
                TotalPosDue = oConnectionContext.DbClsSales.Where(a => (a.Status != "Draft" && a.Status != "Hold") && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //&& a.BranchId == obj.BranchId
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && a.SalesType.ToLower() == "pos"
           ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                (from a in oConnectionContext.DbClsSales
                 join b in oConnectionContext.DbClsCustomerPayment
     on a.SalesId equals b.SalesId
                 where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
      //&& a.BranchId == obj.BranchId 
      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
     && b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false &&
     b.CompanyId == obj.CompanyId
     && a.SalesType.ToLower() == "pos"
                 //&& b.BranchId == obj.BranchId
                 select b.Amount).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalPosDue = oConnectionContext.DbClsSales.Where(a => (a.Status != "Draft" && a.Status != "Hold") && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
           && a.BranchId == obj.BranchId && a.SalesType.ToLower() == "pos").Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                (from a in oConnectionContext.DbClsSales
                 join b in oConnectionContext.DbClsCustomerPayment
     on a.SalesId equals b.SalesId
                 where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
     && a.BranchId == obj.BranchId && b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false &&
     b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId && a.SalesType.ToLower() == "pos"
                 select b.Amount).DefaultIfEmpty().Sum();
            }

            decimal TotalPurchaseReturn = 0;
            if (obj.BranchId == 0)
            {
                TotalPurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                           //                           join b in oConnectionContext.DbClsPurchase
                                           //on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false
                                        //&& b.BranchId == obj.BranchId
                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                       select a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalPurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                           //                           join b in oConnectionContext.DbClsPurchase
                                           //on a.PurchaseId equals b.PurchaseId
                                       where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId
                                       select a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal TotalPurchaseReturnDue = 0;
            if (obj.BranchId == 0)
            {
        //        TotalPurchaseReturnDue = (from a in oConnectionContext.DbClsPurchaseReturn
        //                                      //                           join b in oConnectionContext.DbClsPurchase
        //                                      //on a.PurchaseId equals b.PurchaseId
        //                                  where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false
        //                                   //&& b.BranchId == obj.BranchId
        //                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //                                  select a.GrandTotal).DefaultIfEmpty().Sum() -
        //                                      (from a in oConnectionContext.DbClsPurchaseReturn
        //                                           //                           join b in oConnectionContext.DbClsPurchase
        //                                           //on a.PurchaseId equals b.PurchaseId
        //                                       join c in oConnectionContext.DbClsSupplierRefund
        //                                       on a.PurchaseReturnId equals c.PurchaseReturnId
        //                                       where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false
        //                                        //&& b.IsDeleted == false && b.IsCancelled == false
        //                                        //&& b.BranchId == obj.BranchId
        //                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //                                       && c.Type.ToLower() == "supplier refund" && c.IsDeleted == false && c.IsCancelled == false &&
        //   c.CompanyId == obj.CompanyId
        //                                       select c.Amount).DefaultIfEmpty().Sum();
            }
            else
            {
           //     TotalPurchaseReturnDue = (from a in oConnectionContext.DbClsPurchaseReturn
           //                                   //                           join b in oConnectionContext.DbClsPurchase
           //                                   //on a.PurchaseId equals b.PurchaseId
           //                               where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId
           //                               select a.GrandTotal).DefaultIfEmpty().Sum() -
           //                                   (from a in oConnectionContext.DbClsPurchaseReturn
           //                                        //                           join b in oConnectionContext.DbClsPurchase
           //                                        //on a.PurchaseId equals b.PurchaseId
           //                                    join c in oConnectionContext.DbClsSupplierRefund
           //                                    on a.PurchaseReturnId equals c.PurchaseReturnId
           //                                    where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false
           //                                    //&& b.IsDeleted == false && b.IsCancelled == false 
           //                                    && a.BranchId == obj.BranchId
           //                                    && c.Type.ToLower() == "supplier refund" && c.IsDeleted == false && c.IsCancelled == false &&
           //c.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId
           //                                    select c.Amount).DefaultIfEmpty().Sum();
            }

            decimal TotalSalesReturn = 0;
            if (obj.BranchId == 0)
            {
                TotalSalesReturn = (from a in oConnectionContext.DbClsSalesReturn
                                    join b in oConnectionContext.DbClsSales on a.SalesId equals b.SalesId
                                    where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
     //&& b.BranchId == obj.BranchId 
     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false
                                    select a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalSalesReturn = (from a in oConnectionContext.DbClsSalesReturn
                                    join b in oConnectionContext.DbClsSales on a.SalesId equals b.SalesId
                                    where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
    && b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false
                                    select a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal TotalSalesReturnDue = 0;
            if (obj.BranchId == 0)
            {
                //TotalSalesReturnDue =
                //                (from a in oConnectionContext.DbClsSalesReturn
                //                 join b in oConnectionContext.DbClsSales on a.SalesId equals b.SalesId
                //                 where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                // //&& b.BranchId == obj.BranchId
                // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //&& b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false
                //                 select a.GrandTotal).DefaultIfEmpty().Sum() -
                //                  (from a in oConnectionContext.DbClsSalesReturn
                //                   join b in oConnectionContext.DbClsSales
                //on a.SalesId equals b.SalesId
                //                   join c in oConnectionContext.DbClsCustomerRefund
                //                   on a.SalesReturnId equals c.SalesReturnId
                //                   where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false
                //                    //&& b.BranchId == obj.BranchId
                //                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //                   && b.IsDeleted == false && b.IsCancelled == false
                //                   && c.Type.ToLower() == "customer refund" && c.IsDeleted == false && c.IsCancelled == false &&
                //c.CompanyId == obj.CompanyId //&& c.BranchId == obj.BranchId
                //                   select c.Amount).DefaultIfEmpty().Sum();
            }
            else
            {
//                TotalSalesReturnDue =
//                (from a in oConnectionContext.DbClsSalesReturn
//                 join b in oConnectionContext.DbClsSales on a.SalesId equals b.SalesId
//                 where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
//&& b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false
//                 select a.GrandTotal).DefaultIfEmpty().Sum() -
//                  (from a in oConnectionContext.DbClsSalesReturn
//                   join b in oConnectionContext.DbClsSales
//on a.SalesId equals b.SalesId
//                   join c in oConnectionContext.DbClsCustomerRefund
//                   on a.SalesReturnId equals c.SalesReturnId
//                   where a.CompanyId == obj.CompanyId && a.IsActive && a.IsDeleted == false && a.IsCancelled == false && b.BranchId == obj.BranchId
//                   && b.IsDeleted == false && b.IsCancelled == false
//                   && c.Type.ToLower() == "customer refund" && c.IsDeleted == false && c.IsCancelled == false &&
//c.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId
//                   select c.Amount).DefaultIfEmpty().Sum();
            }

            decimal TotalStockAdjusted = 0;
            if (obj.BranchId == 0)
            {
                TotalStockAdjusted = oConnectionContext.DbClsStockAdjustment.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
     //&& a.BranchId == obj.BranchId
     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
    ).Select(a => a.TotalAmount).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalStockAdjusted = oConnectionContext.DbClsStockAdjustment.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
    && a.BranchId == obj.BranchId).Select(a => a.TotalAmount).DefaultIfEmpty().Sum();
            }

            decimal TotalExpense = 0;
            if (obj.BranchId == 0)
            {
                TotalExpense = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
    //&& a.BranchId == obj.BranchId
    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
   ).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalExpense = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.BranchId == obj.BranchId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();
            }

            decimal TotalExpenseDue = 0;
            //            if (obj.BranchId == 0)
            //            {
            //                TotalExpenseDue = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //  //&& a.BranchId == obj.BranchId
            //  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            //                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
            // ).Select(a => a.Amount).DefaultIfEmpty().Sum() -
            //  (from a in oConnectionContext.DbClsExpense
            //   join b in oConnectionContext.DbClsAccountsPayment
            //on a.ExpenseId equals b.Id
            //   where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            // //&& a.BranchId == obj.BranchId 
            // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            //                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
            //&& b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false &&
            //b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId
            //   select b.Amount).DefaultIfEmpty().Sum();
            //            }
            //            else
            //            {
            //                TotalExpenseDue = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //&& a.BranchId == obj.BranchId).Select(a => a.Amount).DefaultIfEmpty().Sum() -
            //(from a in oConnectionContext.DbClsExpense
            // join b in oConnectionContext.DbClsAccountsPayment
            // on a.ExpenseId equals b.Id
            // where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            // && a.BranchId == obj.BranchId && b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false &&
            // b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId
            // select b.Amount).DefaultIfEmpty().Sum();
            //            }

            int TotalCustomers = 0;
            if (obj.BranchId == 0)
            {
                TotalCustomers = (from a in oConnectionContext.DbClsUser
                                      //join b in oConnectionContext.DbClsUserBranchMap
                                      //on a.UserId equals b.UserId
                                  where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
            //&& b.BranchId == obj.BranchId
            //                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
            //&& b.IsActive == true 
            && a.UserId != obj.AddedBy
                                  select a.UserId).Distinct().Count();
            }
            else
            {
                TotalCustomers = (from a in oConnectionContext.DbClsUser
                                      //join b in oConnectionContext.DbClsUserBranchMap
                                      //on a.UserId equals b.UserId
                                  where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                  //b.BranchId == obj.BranchId 
                                  //&& b.IsActive == true 
                                  && a.UserId != obj.AddedBy
                                  select a.UserId).Count();
            }

            int TotalSuppliers = 0;
            if (obj.BranchId == 0)
            {
                TotalSuppliers = (from a in oConnectionContext.DbClsUser
                                      //join b in oConnectionContext.DbClsUserBranchMap
                                      //on a.UserId equals b.UserId
                                  where a.CompanyId == obj.CompanyId && a.UserType == "supplier" && a.IsDeleted == false
                                  //                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                  //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                                  && a.UserId != obj.AddedBy
                                  select a.UserId).Distinct().Count();
            }
            else
            {
                TotalSuppliers = (from a in oConnectionContext.DbClsUser
                                      //join b in oConnectionContext.DbClsUserBranchMap
                                      //on a.UserId equals b.UserId
                                  where a.CompanyId == obj.CompanyId && a.UserType == "supplier" && a.IsDeleted == false
                                  //&& b.BranchId == obj.BranchId
                                  && a.UserId != obj.AddedBy
                                  select a.UserId).Count();
            }

            int TotalUsers = 0;
            if (obj.BranchId == 0)
            {
                TotalUsers = (from a in oConnectionContext.DbClsUser
                              join b in oConnectionContext.DbClsUserBranchMap
                              on a.UserId equals b.UserId
                              where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "user" && a.IsDeleted == false
                               //&& b.BranchId == obj.BranchId
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                              //&& a.UserId != obj.AddedBy
                              select a.UserId).Distinct().Count();
            }
            else
            {
                TotalUsers = (from a in oConnectionContext.DbClsUser
                              join b in oConnectionContext.DbClsUserBranchMap
                              on a.UserId equals b.UserId
                              where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "user" && a.IsDeleted == false
                              && b.BranchId == obj.BranchId
                              //&& a.UserId != obj.AddedBy
                              select a.UserId).Count();
            }

            int TotalItems = 0;
            if (obj.BranchId == 0)
            {
                //TotalItems = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false &&
                //(oConnectionContext.DbClsItemBranchMap.Where(z => z.BranchId == obj.BranchId).Any(z => z.ItemId == a.ItemId))).Count();

                TotalItems = (from a in oConnectionContext.DbClsItem
                              join b in oConnectionContext.DbClsItemBranchMap
on a.ItemId equals b.ItemId
                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               //&& b.BranchId == obj.BranchId
                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                              select a.ItemId).Distinct().Count();
            }
            else
            {
                TotalItems = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                (oConnectionContext.DbClsItemBranchMap.Where(z => z.BranchId == obj.BranchId).Any(z => z.ItemId == a.ItemId))).Count();
            }

            //var Sales = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Select(a => new
            //{
            //    a.TotalPaying,
            //    a.Balance,
            //    a.ChangeReturn,
            //    a.CustomerId,
            //    Customer = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
            //    CustomerMobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.MobileNo).FirstOrDefault(),
            //    a.Status,
            //    a.PayTerm,
            //    a.PayTermNo,
            //    a.AttachDocument,
            //    SalesId = a.SalesId,
            //    a.GrandTotal,
            //    a.TaxId,
            //    a.TotalDiscount,
            //    a.TotalQuantity,
            //    a.Discount,
            //    a.DiscountType,
            //    a.Notes,
            //    a.OtherCharges,
            //    a.SalesDate,
            //    a.ShippingAddress,
            //    a.ShippingCharge,
            //    a.ShippingDetails,
            //    a.ShippingDocument,
            //    a.ShippingStatus,
            //    a.DeliveredTo,
            //    a.InvoiceNo,
            //    a.Subtotal,
            //    CompanyId = a.CompanyId,
            //    IsActive = a.IsActive,
            //    IsDeleted = a.IsDeleted,
            //    AddedBy = a.AddedBy,
            //    AddedOn = a.AddedOn,
            //    ModifiedBy = a.ModifiedBy,
            //    ModifiedOn = a.ModifiedOn,
            //    Paid = oConnectionContext.DbClsPayment.Where(b => b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
            //    a.PaymentStatus,
            //    Due = oConnectionContext.DbClsPayment.Where(b => b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.SalesId).Count() == 0 ? a.GrandTotal :
            //        a.GrandTotal - oConnectionContext.DbClsPayment.Where(b => b.Type.ToLower() == "sales payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.SalesId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
            //    ReturnDue = 0
            //}).OrderByDescending(a => a.SalesId).Take(obj.PageSize).ToList();

            //var Purchases = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Select(a => new
            //{
            //    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
            //    PurchaseId = a.PurchaseId,
            //    a.GrandTotal,
            //    a.Notes,
            //    a.OtherCharges,
            //    a.PurchaseDate,
            //    a.Status,
            //    a.ReferenceNo,
            //    a.Subtotal,
            //    a.SupplierId,
            //    SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
            //    CompanyId = a.CompanyId,
            //    IsActive = a.IsActive,
            //    IsDeleted = a.IsDeleted,
            //    AddedBy = a.AddedBy,
            //    AddedOn = a.AddedOn,
            //    ModifiedBy = a.ModifiedBy,
            //    ModifiedOn = a.ModifiedOn,
            //    Paid = oConnectionContext.DbClsPayment.Where(b => b.Type == "Purchase" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.PurchaseId).Count() == 0 ? 0 :
            //       oConnectionContext.DbClsPayment.Where(b => b.Type == "Purchase" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
            //    a.PaymentStatus,
            //    Due = oConnectionContext.DbClsPayment.Where(b => b.Type == "Purchase" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.PurchaseId).Count() == 0 ? a.GrandTotal :
            //       a.GrandTotal - oConnectionContext.DbClsPayment.Where(b => b.Type == "Purchase" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum()
            //}).OrderByDescending(a => a.PurchaseId).Take(obj.PageSize).ToList();

            dynamic TopCustomers = null;
            if (obj.BranchId == 0)
            {
                TopCustomers = (from a in oConnectionContext.DbClsUser
                                    //join b in oConnectionContext.DbClsUserBranchMap
                                    //on a.UserId equals b.UserId
                                where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                //&& b.BranchId == obj.BranchId
                                //                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                                select new
                                {
                                    Name = a.Name,
                                    MobileNo = a.MobileNo,
                                    UserId = a.UserId,
                                    TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                 //&& a.BranchId == obj.BranchId
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum()
                                }).Where(a => a.TotalSales > 0).OrderByDescending(o => o.TotalSales).Take(10);
            }
            else
            {
                TopCustomers = (from a in oConnectionContext.DbClsUser
                                    //join b in oConnectionContext.DbClsUserBranchMap
                                    //on a.UserId equals b.UserId
                                where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                //&& b.BranchId == obj.BranchId
                                select new
                                {
                                    Name = a.Name,
                                    MobileNo = a.MobileNo,
                                    UserId = a.UserId,
                                    TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                            && c.BranchId == obj.BranchId
                ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum()
                                }).Where(a => a.TotalSales > 0).OrderByDescending(o => o.TotalSales).Take(10);
            }

            dynamic TopItems = null;
            if (obj.BranchId == 0)
            {
                TopItems = (from a in oConnectionContext.DbClsItemBranchMap
                            join b in oConnectionContext.DbClsItemDetails
                            on a.ItemDetailsId equals b.ItemDetailsId
                            join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                            where
                             //a.BranchId == obj.BranchId 
                             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                            && c.ProductType != "Combo"
                            select new
                            {
                                b.ItemId,
                                a.ItemDetailsId,
                                c.ItemName,
                                SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                TotalSales = (from e in oConnectionContext.DbClsSales
                                              join f in oConnectionContext.DbClsSalesDetails
on e.SalesId equals f.SalesId
                                              where e.Status != "Draft" &&
                                               //e.BranchId == obj.BranchId 
                                               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                              && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
                                              select f.AmountIncTax).DefaultIfEmpty().Sum()
                            }).Union((from a in oConnectionContext.DbClsItem
                                      where a.ProductType == "Combo" &&
   oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId
    //&& b.BranchId == obj.BranchId
    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   )
                                      select new
                                      {
                                          a.ItemId,
                                          ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                          a.ItemName,
                                          SKU = a.SkuCode,
                                          VariationName = "",
                                          TotalSales = (from e in oConnectionContext.DbClsSales
                                                        join f in oConnectionContext.DbClsSalesDetails
  on e.SalesId equals f.SalesId
                                                        where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                          && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                                                        select f.AmountIncTax).DefaultIfEmpty().Sum()
                                      })).Where(a => a.TotalSales > 0).OrderByDescending(a => a.TotalSales).Take(10).ToList();
            }
            else
            {
                TopItems = (from a in oConnectionContext.DbClsItemBranchMap
                            join b in oConnectionContext.DbClsItemDetails
                            on a.ItemDetailsId equals b.ItemDetailsId
                            join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                            where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                            select new
                            {
                                b.ItemId,
                                a.ItemDetailsId,
                                c.ItemName,
                                SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                TotalSales = (from e in oConnectionContext.DbClsSales
                                              join f in oConnectionContext.DbClsSalesDetails
on e.SalesId equals f.SalesId
                                              where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
                                              select f.AmountIncTax).DefaultIfEmpty().Sum()
                            }).Union((from a in oConnectionContext.DbClsItem
                                      where a.ProductType == "Combo" &&
   oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                      select new
                                      {
                                          a.ItemId,
                                          ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                          a.ItemName,
                                          SKU = a.SkuCode,
                                          VariationName = "",
                                          TotalSales = (from e in oConnectionContext.DbClsSales
                                                        join f in oConnectionContext.DbClsSalesDetails
  on e.SalesId equals f.SalesId
                                                        where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                          && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                                                        select f.AmountIncTax).DefaultIfEmpty().Sum()
                                      })).Where(a => a.TotalSales > 0).OrderByDescending(a => a.TotalSales).Take(10).ToList();
            }

            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            var dates = new List<DateTime>();

            // Loop from the first day of the month until we hit the next month, moving forward a day at a time
            for (var date = new DateTime(currentYear, currentMonth, 1); date.Month == currentMonth; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            dynamic SalesMonthWise = null;
            if (obj.BranchId == 0)
            {
                SalesMonthWise = (from a in dates
                                  select new
                                  {
                                      DayNo = a.Day,
                                      Date = a,
                                      TotalSales = oConnectionContext.DbClsSales.Where(b => b.Status != "Draft" && b.CompanyId == obj.CompanyId &&
                                        //b.BranchId == obj.BranchId 
                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                      && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false)
                                      .Where(b => DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(b.SalesDate)).Select(b => b.GrandTotal).DefaultIfEmpty().Sum()
                                  }).ToList();
            }
            else
            {
                SalesMonthWise = (from a in dates
                                  select new
                                  {
                                      DayNo = a.Day,
                                      Date = a,
                                      TotalSales = oConnectionContext.DbClsSales.Where(b => b.Status != "Draft" && b.CompanyId == obj.CompanyId &&
                                      b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false)
                                      .Where(b => DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(b.SalesDate)).Select(b => b.GrandTotal).DefaultIfEmpty().Sum()
                                  }).ToList();
            }

            dynamic PurchaseMonthWise = null;
            if (obj.BranchId == 0)
            {
                PurchaseMonthWise = (from a in dates
                                     select new
                                     {
                                         DayNo = a.Day,
                                         Date = a,
                                         TotalPurchase = oConnectionContext.DbClsPurchase.Where(b => b.Status.ToLower() != "draft" &&
                                         b.CompanyId == obj.CompanyId
                                           //b.BranchId == obj.BranchId 
                                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                          && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false)
                                         .Where(b => DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(b.PurchaseDate)).Select(b => b.GrandTotal).DefaultIfEmpty().Sum()
                                     }).ToList();
            }
            else
            {
                PurchaseMonthWise = (from a in dates
                                     select new
                                     {
                                         DayNo = a.Day,
                                         Date = a,
                                         TotalPurchase = oConnectionContext.DbClsPurchase.Where(b => b.Status.ToLower() != "draft" &&
                                         b.CompanyId == obj.CompanyId &&
                                          b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false)
                                         .Where(b => DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(b.PurchaseDate)).Select(b => b.GrandTotal).DefaultIfEmpty().Sum()
                                     }).ToList();
            }

            dynamic StockAlertItems = null;
            if (obj.BranchId == 0)
            {
                StockAlertItems = (from a in oConnectionContext.DbClsItemBranchMap
                                   join b in oConnectionContext.DbClsItemDetails
                                   on a.ItemDetailsId equals b.ItemDetailsId
                                   join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                   where
                                    //a.BranchId == obj.BranchId 
                                    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                   && c.ProductType != "Combo" && c.IsManageStock == true && a.Quantity <= c.AlertQuantity
                                   && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                                   select new
                                   {
                                       b.ItemId,
                                       a.ItemDetailsId,
                                       c.ItemName,
                                       SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                       a.Quantity
                                   }).Union((from b in oConnectionContext.DbClsItemBranchMap
                                             join a in oConnectionContext.DbClsItem
     on b.ItemId equals a.ItemId
                                             where a.ProductType == "Combo"
                                              //&& b.BranchId == obj.BranchId 
                                              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                             && a.IsManageStock == true && b.Quantity <= a.AlertQuantity
                                             && a.IsDeleted == false && b.IsDeleted == false
                                             select new
                                             {
                                                 a.ItemId,
                                                 ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                 a.ItemName,
                                                 SKU = a.SkuCode,
                                                 VariationName = "",
                                                 b.Quantity
                                             })).OrderByDescending(a => a.Quantity).Take(10).ToList();
            }
            else
            {
                StockAlertItems = (from a in oConnectionContext.DbClsItemBranchMap
                                   join b in oConnectionContext.DbClsItemDetails
                                   on a.ItemDetailsId equals b.ItemDetailsId
                                   join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                   where a.BranchId == obj.BranchId && c.ProductType != "Combo" && c.IsManageStock == true && a.Quantity <= c.AlertQuantity
                                   && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                                   select new
                                   {
                                       b.ItemId,
                                       a.ItemDetailsId,
                                       c.ItemName,
                                       SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                       a.Quantity
                                   }).Union((from b in oConnectionContext.DbClsItemBranchMap
                                             join a in oConnectionContext.DbClsItem
     on b.ItemId equals a.ItemId
                                             where a.ProductType == "Combo" && b.BranchId == obj.BranchId && a.IsManageStock == true && b.Quantity <= a.AlertQuantity
                                             && a.IsDeleted == false && b.IsDeleted == false
                                             select new
                                             {
                                                 a.ItemId,
                                                 ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                 a.ItemName,
                                                 SKU = a.SkuCode,
                                                 VariationName = "",
                                                 b.Quantity
                                             })).OrderByDescending(a => a.Quantity).Take(10).ToList();
            }

            dynamic ExpiredItems = null;
//            if (obj.BranchId == 0)
//            {
//                ExpiredItems = (from a in oConnectionContext.DbClsPurchase
//                                join b in oConnectionContext.DbClsPurchaseDetails
//         on a.PurchaseId equals b.PurchaseId
//                                where
//                                //a.BranchId == obj.BranchId 
//                                oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
//         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
//                                && b.QuantityRemaining > 0
//                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.ExpiryDate != null
//                                && DbFunctions.TruncateTime(b.ExpiryDate.Value.Date) <= obj.ExpiryDate
//                                select new
//                                {
//                                    b.LotNo,
//                                    b.ExpiryDate,
//                                    b.ItemId,
//                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
//                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
//                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
//                                                        oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
//                                                        oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
//                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
//                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
//                                    b.Quantity
//                                })
//                               //                       .Union(from a in oConnectionContext.DbClsStockAdjustment
//                               //                                join b in oConnectionContext.DbClsStockAdjustmentDetails
//                               //on a.StockAdjustmentId equals b.StockAdjustmentId
//                               //                                where a.BranchId == obj.BranchId && b.ExpiryDate != null && b.QuantityRemaining > 0
//                               //                                && a.IsDeleted== false&& b.IsDeleted == false && b.IsCancelled == false
//                               //                               && (DbFunctions.DiffDays(DateTime.Now, b.ExpiryDate) < 30)
//                               //                                select new
//                               //                                {
//                               //                                    b.LotNo,
//                               //                                    b.ExpiryDate,
//                               //                                    b.ItemId,
//                               //                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
//                               //                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
//                               //                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
//                               //                                    oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
//                               //                                    oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
//                               //                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
//                               //                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
//                               //                                    b.Quantity
//                               //                                })
//                               .OrderBy(a => a.Quantity).Take(10).ToList();
//            }
//            else
//            {
//                ExpiredItems = (from a in oConnectionContext.DbClsPurchase
//                                join b in oConnectionContext.DbClsPurchaseDetails
//on a.PurchaseId equals b.PurchaseId
//                                where a.BranchId == obj.BranchId && b.ExpiryDate != null && b.QuantityRemaining > 0
//                                && a.Status.ToLower() != "draft"
//                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false
//                                //&& ((DbFunctions.TruncateTime(b.ExpiryDate) - DbFunctions.TruncateTime(DateTime.Now)).Value.Days>30)
//                                && (DbFunctions.DiffDays(DateTime.Now, b.ExpiryDate) < 30)
//                                select new
//                                {
//                                    b.LotNo,
//                                    b.ExpiryDate,
//                                    b.ItemId,
//                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
//                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
//                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
//                                                        oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
//                                                        oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
//                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
//                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
//                                    b.Quantity
//                                })
//                               //                       .Union(from a in oConnectionContext.DbClsStockAdjustment
//                               //                                join b in oConnectionContext.DbClsStockAdjustmentDetails
//                               //on a.StockAdjustmentId equals b.StockAdjustmentId
//                               //                                where a.BranchId == obj.BranchId && b.ExpiryDate != null && b.QuantityRemaining > 0
//                               //                                && a.IsDeleted== false&& b.IsDeleted == false && b.IsCancelled == false
//                               //                               && (DbFunctions.DiffDays(DateTime.Now, b.ExpiryDate) < 30)
//                               //                                select new
//                               //                                {
//                               //                                    b.LotNo,
//                               //                                    b.ExpiryDate,
//                               //                                    b.ItemId,
//                               //                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
//                               //                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
//                               //                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
//                               //                                    oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
//                               //                                    oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
//                               //                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
//                               //                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
//                               //                                    b.Quantity
//                               //                                })
//                               .OrderBy(a => a.Quantity).Take(10).ToList();
//            }


            decimal TotalExpiredItems = 0;
            if (obj.BranchId == 0)
            {
                TotalExpiredItems = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
              on a.PurchaseId equals b.PurchaseId
                                     where
                                     //a.BranchId == obj.BranchId 
                                     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                     && b.QuantityRemaining > 0
                                     && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.ExpiryDate != null
                                     && DbFunctions.TruncateTime(b.ExpiryDate.Value) <= DbFunctions.TruncateTime(obj.ExpiryDate)
                                     && a.Status.ToLower() != "draft"
                                     select a.TotalQuantity).Count()+(from a in oConnectionContext.DbClsOpeningStock
                                              where
                                              //a.BranchId == obj.BranchId && 
                                              oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                                              a.QuantityRemaining > 0
                                              && a.IsDeleted == false && a.ExpiryDate != null
                                              && DbFunctions.TruncateTime(a.ExpiryDate.Value) <= DbFunctions.TruncateTime(obj.ExpiryDate)
                                                                                       select a.SubTotal).Count();
            }
            else
            {
                TotalExpiredItems = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
              on a.PurchaseId equals b.PurchaseId
                                     where
                                     a.BranchId == obj.BranchId
                                     //&& b.QuantityRemaining > 0
                                     && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.ExpiryDate != null
                                     && DbFunctions.TruncateTime(b.ExpiryDate.Value) <= DbFunctions.TruncateTime(obj.ExpiryDate)
                                     && a.Status.ToLower() != "draft"
                                     select a.TotalQuantity).Count()+(from a in oConnectionContext.DbClsOpeningStock
                                              where
                                              a.BranchId == obj.BranchId &&
                                              a.QuantityRemaining > 0
                                              && a.IsDeleted == false && a.ExpiryDate != null
                                              && DbFunctions.TruncateTime(a.ExpiryDate.Value) <= DbFunctions.TruncateTime(obj.ExpiryDate)
                                                                                       select a.SubTotal).Count();
            }

            decimal TotalStockAlertItems = 0;
            if (obj.BranchId == 0)
            {
                TotalStockAlertItems = (from a in oConnectionContext.DbClsItemBranchMap
                                        join b in oConnectionContext.DbClsItemDetails
                                        on a.ItemDetailsId equals b.ItemDetailsId
                                        join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                        where
                                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && c.ProductType != "Combo" && c.IsManageStock == true && a.Quantity <= c.AlertQuantity
                       && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                                        select a.Quantity).DefaultIfEmpty().Count();
                //                        +((from b in oConnectionContext.DbClsItemBranchMap
                //                                  join a in oConnectionContext.DbClsItem
                //                                  on b.ItemId equals a.ItemId
                //                                  where a.ProductType == "Combo"
                //                                   //&& b.BranchId == obj.BranchId 
                //                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                //                                  && a.IsManageStock == true && b.Quantity <= a.AlertQuantity
                //                                    && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false
                //                                  select b.Quantity)).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalStockAlertItems = (from a in oConnectionContext.DbClsItemBranchMap
                                        join b in oConnectionContext.DbClsItemDetails
                                        on a.ItemDetailsId equals b.ItemDetailsId
                                        join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                        where a.BranchId == obj.BranchId
                                         && c.ProductType != "Combo" && c.IsManageStock == true && a.Quantity <= c.AlertQuantity
                       && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                                        select a.Quantity).DefaultIfEmpty().Count();
                //+ ((from b in oConnectionContext.DbClsItemBranchMap
                //                                              join a in oConnectionContext.DbClsItem
                //                                              on b.ItemId equals a.ItemId
                //                                              where a.ProductType == "Combo" && b.BranchId == obj.BranchId && a.IsManageStock == true && b.Quantity <= a.AlertQuantity
                //                                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false
                //                                              select b.Quantity)).DefaultIfEmpty().Sum();
            }

            decimal TotalIncome = 0;
            //         if (obj.BranchId == 0)
            //         {
            //             TotalIncome = oConnectionContext.DbClsIncome.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            // //&& a.BranchId == obj.BranchId
            // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            //             l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
            //).Select(a => a.Amount).DefaultIfEmpty().Sum();
            //         }
            //         else
            //         {
            //             TotalIncome = oConnectionContext.DbClsIncome.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //&& a.BranchId == obj.BranchId).Select(a => a.Amount).DefaultIfEmpty().Sum();
            //         }

            decimal TotalIncomeDue = 0;
            //            if (obj.BranchId == 0)
            //            {
            //                TotalIncomeDue = oConnectionContext.DbClsIncome.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //   //&& a.BranchId == obj.BranchId
            //   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            //                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
            //  ).Select(a => a.Amount).DefaultIfEmpty().Sum() -
            //  (from a in oConnectionContext.DbClsIncome
            //   join b in oConnectionContext.DbClsAccountsPayment
            //on a.IncomeId equals b.Id
            //   where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            // //&& a.BranchId == obj.BranchId
            // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            //                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
            //&& b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false &&
            //b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId
            //   select b.Amount).DefaultIfEmpty().Sum();
            //            }
            //            else
            //            {
            //                TotalIncomeDue = oConnectionContext.DbClsIncome.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            //&& a.BranchId == obj.BranchId).Select(a => a.Amount).DefaultIfEmpty().Sum() -
            //(from a in oConnectionContext.DbClsIncome
            // join b in oConnectionContext.DbClsAccountsPayment
            // on a.IncomeId equals b.Id
            // where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
            // && a.BranchId == obj.BranchId && b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false &&
            // b.CompanyId == obj.CompanyId && b.BranchId == obj.BranchId
            // select b.Amount).DefaultIfEmpty().Sum();
            //            }

            decimal TotalStockTransferred = 0;
            if (obj.BranchId == 0)
            {
                TotalStockTransferred = oConnectionContext.DbClsStockTransfer.Where(a =>
                //a.FromBranchId == obj.BranchId 
                 oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.FromBranchId)
                && a.IsDeleted == false && a.Status == 3).
                Select(a => a.TotalAmount).DefaultIfEmpty().Sum();
            }
            else
            {
                TotalStockTransferred = oConnectionContext.DbClsStockTransfer.Where(a => a.FromBranchId == obj.BranchId && a.IsDeleted == false && a.Status == 3).
             Select(a => a.TotalAmount).DefaultIfEmpty().Sum();
            }

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).
                Select(a => new
                {
                    IsMainBranchUpdated = oConnectionContext.DbClsBranch.Where(b => b.IsMain == true && b.CompanyId == obj.CompanyId && b.CityId == 0).Count() == 1 ? false : true,
                    BranchId = oConnectionContext.DbClsBranch.Where(b => b.IsMain == true && b.CompanyId == obj.CompanyId).Select(b => b.BranchId).FirstOrDefault(),
                    IsBusinessNameUpdated = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.CompanyId && b.BusinessName == null).Count() == 1 ? false : true,
                    IsFirstCustomerCreated = oConnectionContext.DbClsUser.Where(b => b.UserType == "customer" && b.CompanyId == obj.CompanyId).Count() == 0 ? false : true,
                    IsFirstItemCreated = oConnectionContext.DbClsItem.Where(b => b.CompanyId == obj.CompanyId).Count() == 0 ? false : true,
                    IsFirstSaleCreated = oConnectionContext.DbClsSales.Where(b => b.CompanyId == obj.CompanyId).Count() == 0 ? false : true,
                }).FirstOrDefault();

            var Transaction = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null).Select(a => new ClsTransactionVm
            {
                IsTrial = a.IsTrial,
                TransactionId = a.TransactionId,
                ExpiryDate = a.ExpiryDate,
                DaysLeft = ((int)(a.ExpiryDate.Value.Date - DateTime.Now.Date).TotalDays),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Dashboard = new
                    {
                        TotalPurchase = TotalPurchase,
                        TotalSales = TotalSales,
                        TotalPos = TotalPos,
                        TotalPurchaseDue = TotalPurchaseDue,
                        TotalSalesDue = TotalSalesDue,
                        TotalPosDue = TotalPosDue,
                        TotalPurchaseReturn = TotalPurchaseReturn,
                        TotalSalesReturn = TotalSalesReturn,
                        TotalSalesReturnDue = TotalSalesReturnDue,
                        //TotalWastage = TotalWastage,
                        TotalStockAdjusted = TotalStockAdjusted,
                        TotalExpense = TotalExpense,
                        TotalExpenseDue = TotalExpenseDue,
                        TotalCustomers = TotalCustomers,
                        TotalSuppliers = TotalSuppliers,
                        TotalUsers = TotalUsers,
                        TotalItems = TotalItems,
                        //Sales = Sales,
                        //Purchases = Purchases,
                        TopCustomers = TopCustomers,
                        TopItems = TopItems,
                        SalesMonthWise = SalesMonthWise,
                        PurchaseMonthWise = PurchaseMonthWise,
                        ExpiredItems = ExpiredItems,
                        StockAlertItems = StockAlertItems,
                        TotalExpiredItems = TotalExpiredItems,
                        TotalStockAlertItems = TotalStockAlertItems,
                        TotalIncome = TotalIncome,
                        TotalIncomeDue = TotalIncomeDue,
                        TotalStockTransferred = TotalStockTransferred,
                        TotalPurchaseReturnDue = TotalPurchaseReturnDue
                    },
                    //Branchs = userDetails.BranchIds,
                    BusinessSetting = BusinessSetting,
                    Transaction = Transaction
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TopCustomersGraph(ClsUserVm obj)
        {
            if (obj.Type == "amount")
            {
                if (obj.BranchId == 0)
                {
                    var TopCustomers = (from a in oConnectionContext.DbClsUser
                                            //join b in oConnectionContext.DbClsUserBranchMap
                                            //on a.UserId equals b.UserId
                                        where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                        //&& b.BranchId == obj.BranchId
                                        select new
                                        {
                                            Name = a.Name,
                                            MobileNo = a.MobileNo,
                                            UserId = a.UserId,
                                            TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                         //&& a.BranchId == obj.BranchId
                                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                        ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum()
                                        }).Where(a => a.TotalSales > 0).OrderByDescending(o => o.TotalSales).Take(10);

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopCustomers = TopCustomers,
                            },
                        }
                    };
                }
                else
                {
                    var TopCustomers = (from a in oConnectionContext.DbClsUser
                                            //join b in oConnectionContext.DbClsUserBranchMap
                                            //on a.UserId equals b.UserId
                                        where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                        //&& b.BranchId == obj.BranchId
                                        select new
                                        {
                                            Name = a.Name,
                                            MobileNo = a.MobileNo,
                                            UserId = a.UserId,
                                            TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                         && c.BranchId == obj.BranchId
                                                        ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum()
                                        }).Where(a => a.TotalSales > 0).OrderByDescending(o => o.TotalSales).Take(10);

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopCustomers = TopCustomers,
                            },
                        }
                    };
                }

            }
            else
            {
                if (obj.BranchId == 0)
                {
                    var TopCustomers = (from a in oConnectionContext.DbClsUser
                                            //join b in oConnectionContext.DbClsUserBranchMap
                                            //on a.UserId equals b.UserId
                                        where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                        //&& b.BranchId == obj.BranchId
                                        select new
                                        {
                                            Name = a.Name,
                                            MobileNo = a.MobileNo,
                                            UserId = a.UserId,
                                            TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                     //&& a.BranchId == obj.BranchId
                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                    ).Select(c => c.GrandTotal).DefaultIfEmpty().Count()
                                        }).Where(a => a.TotalSales > 0).OrderByDescending(o => o.TotalSales).Take(10);

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopCustomers = TopCustomers,
                            },
                        }
                    };
                }
                else
                {
                    var TopCustomers = (from a in oConnectionContext.DbClsUser
                                            //join b in oConnectionContext.DbClsUserBranchMap
                                            //on a.UserId equals b.UserId
                                        where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false
                                        //&& b.BranchId == obj.BranchId
                                        select new
                                        {
                                            Name = a.Name,
                                            MobileNo = a.MobileNo,
                                            UserId = a.UserId,
                                            TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                     && c.BranchId == obj.BranchId
                                                    ).Select(c => c.GrandTotal).DefaultIfEmpty().Count()
                                        }).Where(a => a.TotalSales > 0).OrderByDescending(o => o.TotalSales).Take(10);

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopCustomers = TopCustomers,
                            },
                        }
                    };
                }
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TopItemsGraph(ClsUserVm obj)
        {
            if (obj.Type == "amount")
            {
                if (obj.BranchId == 0)
                {
                    var TopItems = (from a in oConnectionContext.DbClsItemBranchMap
                                    join b in oConnectionContext.DbClsItemDetails
                                    on a.ItemDetailsId equals b.ItemDetailsId
                                    join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                    where
                                     //a.BranchId == obj.BranchId 
                                     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                    && c.ProductType != "Combo"
                                    select new
                                    {
                                        b.ItemId,
                                        a.ItemDetailsId,
                                        c.ItemName,
                                        SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                        VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                        TotalSales = (from e in oConnectionContext.DbClsSales
                                                      join f in oConnectionContext.DbClsSalesDetails
        on e.SalesId equals f.SalesId
                                                      where e.Status != "Draft" &&
                                                       //e.BranchId == obj.BranchId 
                                                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                      && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                        && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
                                                      select f.AmountIncTax).DefaultIfEmpty().Sum()
                                    }).Union((from a in oConnectionContext.DbClsItem
                                              where a.ProductType == "Combo" &&
           oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId
            //&& b.BranchId == obj.BranchId
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           )
                                              select new
                                              {
                                                  a.ItemId,
                                                  ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                  a.ItemName,
                                                  SKU = a.SkuCode,
                                                  VariationName = "",
                                                  TotalSales = (from e in oConnectionContext.DbClsSales
                                                                join f in oConnectionContext.DbClsSalesDetails
          on e.SalesId equals f.SalesId
                                                                where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                                  && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                                                                select f.AmountIncTax).DefaultIfEmpty().Sum()
                                              })).Where(a => a.TotalSales > 0).OrderByDescending(a => a.TotalSales).Take(10).ToList();

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopItems = TopItems,
                            },
                        }
                    };
                }
                else
                {
                    var TopItems = (from a in oConnectionContext.DbClsItemBranchMap
                                    join b in oConnectionContext.DbClsItemDetails
                                    on a.ItemDetailsId equals b.ItemDetailsId
                                    join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                    where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                                    select new
                                    {
                                        b.ItemId,
                                        a.ItemDetailsId,
                                        c.ItemName,
                                        SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                        VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                        TotalSales = (from e in oConnectionContext.DbClsSales
                                                      join f in oConnectionContext.DbClsSalesDetails
        on e.SalesId equals f.SalesId
                                                      where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                        && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
                                                      select f.AmountIncTax).DefaultIfEmpty().Sum()
                                    }).Union((from a in oConnectionContext.DbClsItem
                                              where a.ProductType == "Combo" &&
           oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                              select new
                                              {
                                                  a.ItemId,
                                                  ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                  a.ItemName,
                                                  SKU = a.SkuCode,
                                                  VariationName = "",
                                                  TotalSales = (from e in oConnectionContext.DbClsSales
                                                                join f in oConnectionContext.DbClsSalesDetails
          on e.SalesId equals f.SalesId
                                                                where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                                  && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                                                                select f.AmountIncTax).DefaultIfEmpty().Sum()
                                              })).Where(a => a.TotalSales > 0).OrderByDescending(a => a.TotalSales).Take(10).ToList();

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopItems = TopItems,
                            },
                        }
                    };
                }
            }
            else
            {
                if (obj.BranchId == 0)
                {
                    var TopItems = (from a in oConnectionContext.DbClsItemBranchMap
                                    join b in oConnectionContext.DbClsItemDetails
                                    on a.ItemDetailsId equals b.ItemDetailsId
                                    join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                    where
                                     //a.BranchId == obj.BranchId 
                                     oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                    && c.ProductType != "Combo"
                                    select new
                                    {
                                        b.ItemId,
                                        a.ItemDetailsId,
                                        c.ItemName,
                                        SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                        VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                        TotalSales = (from e in oConnectionContext.DbClsSales
                                                      join f in oConnectionContext.DbClsSalesDetails
        on e.SalesId equals f.SalesId
                                                      where e.Status != "Draft" &&
                                                       //e.BranchId == obj.BranchId 
                                                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                                      && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                        && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
                                                      select f.AmountIncTax).Count()
                                    }).Union((from a in oConnectionContext.DbClsItem
                                              where a.ProductType == "Combo" &&
           oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId
            //&& b.BranchId == obj.BranchId
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           )
                                              select new
                                              {
                                                  a.ItemId,
                                                  ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                  a.ItemName,
                                                  SKU = a.SkuCode,
                                                  VariationName = "",
                                                  TotalSales = (from e in oConnectionContext.DbClsSales
                                                                join f in oConnectionContext.DbClsSalesDetails
          on e.SalesId equals f.SalesId
                                                                where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                                  && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                                                                select f.AmountIncTax).Count()
                                              })).Where(a => a.TotalSales > 0).OrderByDescending(a => a.TotalSales).Take(10).ToList();
                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopItems = TopItems,
                            },
                        }
                    };
                }
                else
                {
                    var TopItems = (from a in oConnectionContext.DbClsItemBranchMap
                                    join b in oConnectionContext.DbClsItemDetails
                                    on a.ItemDetailsId equals b.ItemDetailsId
                                    join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                    where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                                    select new
                                    {
                                        b.ItemId,
                                        a.ItemDetailsId,
                                        c.ItemName,
                                        SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                        VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                        TotalSales = (from e in oConnectionContext.DbClsSales
                                                      join f in oConnectionContext.DbClsSalesDetails
        on e.SalesId equals f.SalesId
                                                      where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                        && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
                                                      select f.AmountIncTax).Count()
                                    }).Union((from a in oConnectionContext.DbClsItem
                                              where a.ProductType == "Combo" &&
           oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                              select new
                                              {
                                                  a.ItemId,
                                                  ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                  a.ItemName,
                                                  SKU = a.SkuCode,
                                                  VariationName = "",
                                                  TotalSales = (from e in oConnectionContext.DbClsSales
                                                                join f in oConnectionContext.DbClsSalesDetails
          on e.SalesId equals f.SalesId
                                                                where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                                  && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                                                                select f.AmountIncTax).Count()
                                              })).Where(a => a.TotalSales > 0).OrderByDescending(a => a.TotalSales).Take(10).ToList();
                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            Dashboard = new
                            {
                                TopItems = TopItems,
                            },
                        }
                    };
                }
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesMonthWiseGraph(ClsUserVm obj)
        {
            int currentMonth = obj.Month;
            int currentYear = obj.Year;

            var dates = new List<DateTime>();

            // Loop from the first day of the month until we hit the next month, moving forward a day at a time
            for (var date = new DateTime(currentYear, currentMonth, 1); date.Month == currentMonth; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            if (obj.BranchId == 0)
            {
                var SalesMonthWise = (from a in dates
                                      select new
                                      {
                                          DayNo = a.Day,
                                          Date = a,
                                          TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" &&
                                          c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                          && DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(c.SalesDate)
                                         //&& a.BranchId == obj.BranchId
                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                        ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                                      }).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Dashboard = new
                        {
                            SalesMonthWise = SalesMonthWise,
                        },
                    }
                };
            }
            else
            {
                var SalesMonthWise = (from a in dates
                                      select new
                                      {
                                          DayNo = a.Day,
                                          Date = a,
                                          TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" &&
                                          c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                          && DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(c.SalesDate)
                                         && c.BranchId == obj.BranchId
                                        ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                                      }).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Dashboard = new
                        {
                            SalesMonthWise = SalesMonthWise,
                        },
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseMonthWiseGraph(ClsUserVm obj)
        {
            int currentMonth = obj.Month;
            int currentYear = obj.Year;

            var dates = new List<DateTime>();

            // Loop from the first day of the month until we hit the next month, moving forward a day at a time
            for (var date = new DateTime(currentYear, currentMonth, 1); date.Month == currentMonth; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            if (obj.BranchId == 0)
            {
                var PurchaseMonthWise = (from a in dates
                                         select new
                                         {
                                             DayNo = a.Day,
                                             Date = a,
                                             TotalPurchase = oConnectionContext.DbClsPurchase.Where(c => c.Status != "Draft" &&
                                             c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                             && DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(c.PurchaseDate)
                                            //&& a.BranchId == obj.BranchId
                                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                            ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                                         }).ToList();


                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Dashboard = new
                        {
                            PurchaseMonthWise = PurchaseMonthWise,
                        },
                    }
                };
            }
            else
            {
                var PurchaseMonthWise = (from a in dates
                                         select new
                                         {
                                             DayNo = a.Day,
                                             Date = a,
                                             TotalPurchase = oConnectionContext.DbClsPurchase.Where(c => c.Status != "Draft" &&
                                             c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                             && DbFunctions.TruncateTime(a) == DbFunctions.TruncateTime(c.PurchaseDate)
                                             && c.BranchId == obj.BranchId
                                            ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                                         }).ToList();


                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Dashboard = new
                        {
                            PurchaseMonthWise = PurchaseMonthWise,
                        },
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

    }
}
