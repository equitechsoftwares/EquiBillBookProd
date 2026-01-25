using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class RewardPointsReportsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        /// <summary>
        /// Customer Points Statement - List all customers with their point balances
        /// </summary>
        public async Task<IHttpActionResult> CustomerPointsStatement(ClsCustomerRewardPointsVm obj)
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

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            // Reward points are now stored directly in tblUser
            var query = from u in oConnectionContext.DbClsUser
                        where u.CompanyId == obj.CompanyId && !u.IsDeleted && u.UserType != null && u.UserType.ToLower() == "customer"
                        select new ClsCustomerRewardPointsVm
                        {
                            CustomerRewardPointsId = u.UserId, // Use UserId as identifier
                            CustomerId = u.UserId,
                            CompanyId = u.CompanyId,
                            TotalPointsEarned = u.TotalRewardPointsEarned,
                            TotalPointsRedeemed = u.TotalRewardPointsRedeemed,
                            AvailablePoints = u.AvailableRewardPoints,
                            ExpiredPoints = u.ExpiredRewardPoints,
                            LastEarnedDate = u.LastRewardPointsEarnedDate,
                            LastRedeemedDate = u.LastRewardPointsRedeemedDate,
                            CustomerName = u.Name
                        };

            if (!string.IsNullOrEmpty(obj.Search))
            {
                query = query.Where(a => a.CustomerName.Contains(obj.Search));
            }

            if (obj.CustomerId > 0)
            {
                query = query.Where(a => a.CustomerId == obj.CustomerId);
            }

            var totalCount = query.Count();
            var results = query.OrderByDescending(a => a.AvailablePoints).Skip(skip).Take(obj.PageSize).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerRewardPoints = results,
                    TotalCount = totalCount,
                    PageSize = obj.PageSize,
                    PageIndex = obj.PageIndex,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate
                }
            };

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Points Transaction History - Detailed log of all point transactions
        /// </summary>
        public async Task<IHttpActionResult> PointsTransactionHistory(ClsRewardPointTransactionVm obj)
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

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var query = from txn in oConnectionContext.DbClsRewardPointTransaction
                        join u in oConnectionContext.DbClsUser on txn.CustomerId equals u.UserId
                        join s in oConnectionContext.DbClsSales on txn.SalesId equals s.SalesId into salesJoin
                        from s in salesJoin.DefaultIfEmpty()
                        where txn.CompanyId == obj.CompanyId && !txn.IsDeleted && !u.IsDeleted
                        && txn.TransactionDate >= obj.FromDate && txn.TransactionDate <= obj.ToDate
                        select new ClsRewardPointTransactionVm
                        {
                            RewardPointTransactionId = txn.RewardPointTransactionId,
                            CustomerId = txn.CustomerId,
                            SalesId = txn.SalesId,
                            SalesReturnId = txn.SalesReturnId,
                            TransactionType = txn.TransactionType,
                            Points = txn.Points,
                            OrderAmount = txn.OrderAmount,
                            TransactionDate = txn.TransactionDate,
                            ExpiryDate = txn.ExpiryDate,
                            IsExpired = txn.IsExpired,
                            CustomerName = u.Name,
                            InvoiceNo = s != null ? s.InvoiceNo : "",
                            Notes = txn.Notes
                        };

            if (!string.IsNullOrEmpty(obj.Search))
            {
                query = query.Where(a => a.CustomerName.Contains(obj.Search) || a.InvoiceNo.Contains(obj.Search));
            }

            if (obj.CustomerId > 0)
            {
                query = query.Where(a => a.CustomerId == obj.CustomerId);
            }

            var totalCount = query.Count();
            var results = query.OrderByDescending(a => a.TransactionDate).Skip(skip).Take(obj.PageSize).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Transactions = results,
                    TotalCount = totalCount,
                    PageSize = obj.PageSize,
                    PageIndex = obj.PageIndex,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate
                }
            };

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Expiring Points Report - Points expiring within specified days
        /// </summary>
        public async Task<IHttpActionResult> ExpiringPointsReport(ClsRewardPointTransactionVm obj)
        {
            var currentDate = oCommonController.CurrentDate(obj.CompanyId);
            int daysUntilExpiry = obj.ExpiryPeriod > 0 ? obj.ExpiryPeriod : 30; // Default 30 days
            DateTime expiryThreshold = currentDate.AddDays(daysUntilExpiry);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var query = from txn in oConnectionContext.DbClsRewardPointTransaction
                        join u in oConnectionContext.DbClsUser on txn.CustomerId equals u.UserId
                        where txn.CompanyId == obj.CompanyId 
                            && !txn.IsDeleted 
                            && !u.IsDeleted
                            && txn.TransactionType == "Earn"
                            && !txn.IsExpired
                            && txn.ExpiryDate != null
                            && txn.ExpiryDate > currentDate
                            && txn.ExpiryDate <= expiryThreshold
                        select new ClsRewardPointTransactionVm
                        {
                            RewardPointTransactionId = txn.RewardPointTransactionId,
                            CustomerId = txn.CustomerId,
                            SalesId = txn.SalesId,
                            TransactionType = txn.TransactionType,
                            Points = txn.Points,
                            TransactionDate = txn.TransactionDate,
                            ExpiryDate = txn.ExpiryDate,
                            CustomerName = u.Name
                        };

            if (!string.IsNullOrEmpty(obj.Search))
            {
                query = query.Where(a => a.CustomerName.Contains(obj.Search));
            }

            var totalCount = query.Count();
            var results = query.OrderBy(a => a.ExpiryDate).Skip(skip).Take(obj.PageSize).ToList();

            // Calculate days until expiry
            foreach (var result in results)
            {
                if (result.ExpiryDate.HasValue)
                {
                    result.Notes = $"Expires in {((result.ExpiryDate.Value.Date - currentDate.Date).Days)} days";
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ExpiringPoints = results,
                    TotalCount = totalCount,
                    PageSize = obj.PageSize,
                    PageIndex = obj.PageIndex
                }
            };

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Points Summary Report - Summary of points earned, redeemed, and reversed by customer
        /// </summary>
        public async Task<IHttpActionResult> PointsSummaryReport(ClsCustomerRewardPointsVm obj)
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

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            var query = from txn in oConnectionContext.DbClsRewardPointTransaction
                        join u in oConnectionContext.DbClsUser on txn.CustomerId equals u.UserId
                        where txn.CompanyId == obj.CompanyId 
                            && !txn.IsDeleted 
                            && !u.IsDeleted
                            && txn.TransactionDate >= obj.FromDate 
                            && txn.TransactionDate <= obj.ToDate
                        group txn by new { txn.CustomerId, u.Name } into g
                        select new ClsPointsSummaryVm
                        {
                            CustomerId = g.Key.CustomerId,
                            CustomerName = g.Key.Name,
                            TotalPointsEarned = g.Where(t => t.TransactionType == "Earn").Sum(t => (decimal?)t.Points) ?? 0,
                            TotalPointsRedeemed = g.Where(t => t.TransactionType == "Redeem").Sum(t => (decimal?)t.Points) ?? 0,
                            ExpiredPoints = g.Where(t => t.TransactionType == "Expire").Sum(t => (decimal?)t.Points) ?? 0,
                            PointsReversed = g.Where(t => t.TransactionType == "Reverse").Sum(t => (decimal?)t.Points) ?? 0
                        };

            if (!string.IsNullOrEmpty(obj.Search))
            {
                query = query.Where(a => a.CustomerName.Contains(obj.Search));
            }

            var results = query.OrderByDescending(a => a.TotalPointsEarned).ToList();

            // Calculate AvailablePoints for each result
            foreach (var result in results)
            {
                result.AvailablePoints = result.TotalPointsEarned - result.TotalPointsRedeemed - result.ExpiredPoints - result.PointsReversed;
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Summary = results,
                    TotalCount = results.Count,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate
                }
            };

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Customer Points Statement Detailed - Detailed statement for a specific customer
        /// </summary>
        public async Task<IHttpActionResult> CustomerPointsStatementDetailed(ClsCustomerRewardPointsVm obj)
        {
            if (obj.CustomerId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Customer ID is required",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

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

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            // Get customer info
            var customer = oConnectionContext.DbClsUser
                .Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId && !a.IsDeleted)
                .Select(a => new ClsCustomerInfoVm
                {
                    Name = a.Name,
                    MobileNo = a.MobileNo,
                    EmailId = a.EmailId
                })
                .FirstOrDefault();

            // Get opening balance (points earned before FromDate)
            var openingBalance = oConnectionContext.DbClsRewardPointTransaction
                .Where(a => a.CustomerId == obj.CustomerId 
                    && a.CompanyId == obj.CompanyId 
                    && !a.IsDeleted
                    && a.TransactionDate < obj.FromDate
                    && (a.TransactionType == "Earn" || a.TransactionType == "Redeem" || a.TransactionType == "Expire" || a.TransactionType == "Reverse"))
                .GroupBy(a => 1)
                .Select(g => new
                {
                    OpeningEarned = g.Where(t => t.TransactionType == "Earn").Sum(t => t.Points),
                    OpeningRedeemed = g.Where(t => t.TransactionType == "Redeem").Sum(t => t.Points),
                    OpeningExpired = g.Where(t => t.TransactionType == "Expire").Sum(t => t.Points),
                    OpeningReversed = g.Where(t => t.TransactionType == "Reverse").Sum(t => t.Points)
                })
                .FirstOrDefault();

            decimal openingBalancePoints = 0;
            if (openingBalance != null)
            {
                openingBalancePoints = openingBalance.OpeningEarned - openingBalance.OpeningRedeemed - openingBalance.OpeningExpired - openingBalance.OpeningReversed;
            }

            // Get transactions in date range
            var transactions = (from txn in oConnectionContext.DbClsRewardPointTransaction
                              join s in oConnectionContext.DbClsSales on txn.SalesId equals s.SalesId into salesJoin
                              from s in salesJoin.DefaultIfEmpty()
                              where txn.CustomerId == obj.CustomerId
                                  && txn.CompanyId == obj.CompanyId
                                  && !txn.IsDeleted
                                  && txn.TransactionDate >= obj.FromDate
                                  && txn.TransactionDate <= obj.ToDate
                              select new ClsRewardPointTransactionVm
                              {
                                  RewardPointTransactionId = txn.RewardPointTransactionId,
                                  TransactionType = txn.TransactionType,
                                  Points = txn.Points,
                                  OrderAmount = txn.OrderAmount,
                                  TransactionDate = txn.TransactionDate,
                                  ExpiryDate = txn.ExpiryDate,
                                  InvoiceNo = s != null ? s.InvoiceNo : "",
                                  Notes = txn.Notes
                              })
                              .OrderBy(a => a.TransactionDate)
                              .ToList();

            // Calculate running balance
            decimal runningBalance = openingBalancePoints;
            foreach (var txn in transactions)
            {
                if (txn.TransactionType == "Earn" || txn.TransactionType == "Reverse")
                {
                    runningBalance += txn.Points;
                }
                else if (txn.TransactionType == "Redeem" || txn.TransactionType == "Expire")
                {
                    runningBalance -= txn.Points;
                }
                txn.Notes = $"Balance: {runningBalance:N0}";
            }

            decimal closingBalance = runningBalance;

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Customer = customer,
                    OpeningBalance = openingBalancePoints,
                    ClosingBalance = closingBalance,
                    Transactions = transactions,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}

