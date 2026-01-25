using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsDashboard
    {
        public decimal TotalPurchase { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPos { get; set; }
        public decimal TotalPurchaseDue { get; set; }
        public decimal TotalSalesDue { get; set; }
        public decimal TotalPosDue { get; set; }
        public decimal TotalPurchaseReturn { get; set; }
        public decimal TotalPurchaseReturnDue { get; set; }
        public decimal TotalSalesReturn { get; set; }
        public decimal TotalSalesReturnDue { get; set; }
        public decimal TotalWastage { get; set; }
        public decimal TotalStockAdjusted { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalExpenseDue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalUsers { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalExpiredItems { get; set; }
        public decimal TotalStockAlertItems { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalIncomeDue { get; set; }
        public decimal TotalStockTransferred { get; set; }
        public List<ClsItemVm> ExpiredItems { get; set; }
        public List<ClsUserVm> TopCustomers { get; set; }
        public List<ClsItemVm> TopItems { get; set; }
        public List<ClsSalesVm> Sales { get; set; }
        public List<ClsPurchaseVm> Purchases { get; set; }
        public List<ClsSalesMonthWise> SalesMonthWise { get; set; }
        public List<ClsPurchaseMonthWise> PurchaseMonthWise { get; set; }
        public List<ClsItemVm> StockAlertItems { get; set; }
        //public ClsTransactionVm Transaction { get; set; }
    }

    public class ClsSalesMonthWise
    {
        public int DayNo { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class ClsPurchaseMonthWise
    {
        public int DayNo { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPurchase { get; set; }
    }
}