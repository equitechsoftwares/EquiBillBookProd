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
using System.Web.Util;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class AccountOpeningBalanceController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> OpeningBalance(ClsAccountOpeningBalanceVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //  oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //    && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            //}).FirstOrDefault();

            // Step 1: Retrieve the user information (without nested collections)
            var user = oConnectionContext.DbClsUser
                .Where(a => a.UserId == obj.AddedBy)
                .Select(a => new { a.UserId, a.IsCompany })
                .FirstOrDefault();

            if (user != null)
            {
                // Step 2: Fetch the branch data based on IsCompany flag
                var branchIds = user.IsCompany
                    ? oConnectionContext.DbClsBranch
                        .Where(b => b.CompanyId == obj.CompanyId && b.IsActive && !b.IsDeleted)
                        .Select(b => new { b.BranchId, b.Branch })
                        .ToList() // Materialize the collection outside the main query
                    : oConnectionContext.DbClsUserBranchMap
                        .Where(ub => ub.UserId == user.UserId && ub.IsActive && !ub.IsDeleted)
                        .Join(oConnectionContext.DbClsBranch,
                              ub => ub.BranchId,
                              br => br.BranchId,
                              (ub, br) => new { br.BranchId, br.Branch })
                        .ToList(); // Materialize the collection outside the main query

                var userDetails = new
                {
                    BranchIds = branchIds
                };

                if (obj.BranchId == 0)
                {
                    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
                }
            }

            var det = oConnectionContext.DbClsAccountType.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                a.AccountType,
                Accounts = oConnectionContext.DbClsAccount.Where(aa => aa.AccountTypeId == a.AccountTypeId && aa.CompanyId == obj.CompanyId &&
                aa.IsDeleted == false && aa.IsActive == true).Select(aa => new
                {
                    aa.DisplayAs,
                    aa.AccountId,
                    aa.AccountName,
                    aa.AccountNumber,
                    Debit = oConnectionContext.DbClsAccountOpeningBalance.Where(b => b.BranchId == obj.BranchId && b.AccountId == aa.AccountId).Select(b => b.Debit).FirstOrDefault(),
                    Credit = oConnectionContext.DbClsAccountOpeningBalance.Where(b => b.BranchId == obj.BranchId && b.AccountId == aa.AccountId).Select(b => b.Credit).FirstOrDefault(),
                    AccountOpeningBalanceId = oConnectionContext.DbClsAccountOpeningBalance.Where(b => b.BranchId == obj.BranchId && b.AccountId == aa.AccountId).Select(b => b.AccountOpeningBalanceId).FirstOrDefault(),
                    aa.Type
                }).OrderBy(aa => aa.AccountId)
            }).ToList();

            List<ClsBankPaymentVm> OpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> CustomerOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> SupplierOpeningBalance = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> InventoryAsset = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> OpeningBalanceAdjustment = new List<ClsBankPaymentVm>();
            List<ClsBankPaymentVm> Ledger = new List<ClsBankPaymentVm>();

            decimal TotalOpeningBalanceDebit = 0, TotalOpeningBalanceCredit = 0;

            long OpeningBalanceAdjustmentsId = oConnectionContext.DbClsAccount.Where(aa => aa.CompanyId == obj.CompanyId &&
            aa.IsDeleted == false && aa.IsActive == true && aa.Type == "Opening Balance Adjustments").Select(aa => aa.AccountId).FirstOrDefault();

            #region Opening Balance
            OpeningBalance = (from a in oConnectionContext.DbClsAccountOpeningBalance
                              join b in oConnectionContext.DbClsAccountSettings
                           on a.CompanyId equals b.CompanyId
                              where
                              //a.AccountId == obj.AccountId && 
                              a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                              && b.IsDeleted == false && b.IsActive == true
                              && a.BranchId == obj.BranchId
                              && a.AccountId != OpeningBalanceAdjustmentsId
                              select new ClsBankPaymentVm
                              {
                                  AccountId = a.AccountId,
                                  Notes = "",
                                  AddedOn = b.MigrationDate,
                                  Type = "Opening Balance",
                                  ReferenceNo = "",
                                  Debit = a.Debit,
                                  Credit = a.Credit,
                              }).ToList();
            #endregion

            #region Customer Opening Balance
            CustomerOpeningBalance.Add(new ClsBankPaymentVm
            {
                AccountId = obj.AccountId,
                Notes = "",
                AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                Type = "Accounts Receivable",
                ReferenceNo = "",
                Debit = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "customer"
                && a.CompanyId == obj.CompanyId).Select(a => a.OpeningBalance).DefaultIfEmpty().Sum(),
                Credit = 0,
            });
            #endregion

            #region Supplier Opening Balance
            SupplierOpeningBalance.Add(new ClsBankPaymentVm
            {
                AccountId = obj.AccountId,
                Notes = "",
                AddedOn =  oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                Type = "Accounts Payable",
                ReferenceNo = "",
                Debit = 0,
                Credit = oConnectionContext.DbClsUser.Where(a => a.UserType.ToLower() == "supplier"
                && a.CompanyId == obj.CompanyId).Select(a => a.OpeningBalance).DefaultIfEmpty().Sum(),
            });
            #endregion

            #region Inventory Asset
            InventoryAsset.Add(new ClsBankPaymentVm
            {
                AccountId = obj.AccountId,
                Notes = "",
                AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                Type = "Inventory Asset",
                ReferenceNo = "",
                Debit = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true && a.BranchId == obj.BranchId).Select(a => a.SubTotal).DefaultIfEmpty().Sum(),
                Credit = 0,
            });
            #endregion

            #region Opening Balance Adjustments
            foreach (var item in OpeningBalance)
            {
                TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
            }

            foreach (var item in CustomerOpeningBalance)
            {
                TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
            }

            foreach (var item in SupplierOpeningBalance)
            {
                TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
            }

            foreach (var item in InventoryAsset)
            {
                TotalOpeningBalanceDebit = TotalOpeningBalanceDebit + item.Debit;
                TotalOpeningBalanceCredit = TotalOpeningBalanceCredit + item.Credit;
            }

            if (TotalOpeningBalanceDebit < TotalOpeningBalanceCredit)
            {
                TotalOpeningBalanceDebit = TotalOpeningBalanceCredit - TotalOpeningBalanceDebit;
                TotalOpeningBalanceCredit = 0;
            }
            else if (TotalOpeningBalanceCredit < TotalOpeningBalanceDebit)
            {
                TotalOpeningBalanceCredit = TotalOpeningBalanceDebit - TotalOpeningBalanceCredit;
                TotalOpeningBalanceDebit = 0;
            }
            else
            {
                TotalOpeningBalanceDebit = 0;
                TotalOpeningBalanceCredit = 0;
            }

            OpeningBalanceAdjustment.Add(new ClsBankPaymentVm
            {
                AccountId = obj.AccountId,
                Notes = "",
                AddedOn = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true).Select(a => a.MigrationDate).FirstOrDefault(),
                Type = "Opening Balance Adjustments",
                ReferenceNo = "",
                Debit = TotalOpeningBalanceDebit,
                Credit = TotalOpeningBalanceCredit,
            });
            #endregion

            Ledger = CustomerOpeningBalance.Union(SupplierOpeningBalance).Union(InventoryAsset).Union(OpeningBalanceAdjustment).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AccountTypes = det,
                    BankPayments = Ledger
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateOpeningBalance(ClsAccountTypeVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.MigrationDate == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMigrationDate" });
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

                var AccountSettingsId = oConnectionContext.DbClsAccountSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.AccountSettingsId).FirstOrDefault();

                ClsAccountSettings oAccount = new ClsAccountSettings()
                {
                    AccountSettingsId = AccountSettingsId,
                    MigrationDate = obj.MigrationDate.Value.AddHours(5).AddMinutes(30) ,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsAccountSettings.Attach(oAccount);
                oConnectionContext.Entry(oAccount).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oAccount).Property(x => x.MigrationDate).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.AccountOpeningBalances != null)
                {
                    foreach (var item in obj.AccountOpeningBalances)
                    {
                        var Type = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.AccountId == item.AccountId).Select(a => a.Type).FirstOrDefault();

                        if (Type != "Accounts Receivable" && Type != "Accounts Payable" && Type != "Inventory Asset")
                        {
                            if (item.AccountOpeningBalanceId == 0)
                            {
                                ClsAccountOpeningBalance oClsAccount = new ClsAccountOpeningBalance()
                                {
                                    AccountId = item.AccountId,
                                    BranchId = item.BranchId,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Debit = item.Debit,
                                    Credit = item.Credit,
                                };
                                oConnectionContext.DbClsAccountOpeningBalance.Add(oClsAccount);
                                oConnectionContext.SaveChanges();
                            }
                            else
                            {
                                ClsAccountOpeningBalance oClsAccount = new ClsAccountOpeningBalance()
                                {
                                    AccountOpeningBalanceId = item.AccountOpeningBalanceId,
                                    //AccountId = item.AccountId,
                                    //BranchId = item.BranchId,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    Debit = item.Debit,
                                    Credit = item.Credit,
                                };
                                oConnectionContext.DbClsAccountOpeningBalance.Attach(oClsAccount);
                                oConnectionContext.Entry(oClsAccount).Property(x => x.Debit).IsModified = true;
                                oConnectionContext.Entry(oClsAccount).Property(x => x.Credit).IsModified = true;
                                oConnectionContext.Entry(oClsAccount).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsAccount).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }

                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Opening Balances\" updated",
                    Id = 0,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Opening Balances updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> DeleteOpeningBalance(ClsAccountOpeningBalanceVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                string query = "update \"tblAccountOpeningBalance\" set \"Debit\"=0,\"Credit\"=0 where \"CompanyId\"=" + obj.CompanyId + " and \"BranchId\"=" + obj.BranchId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Opening Balances\" deleted",
                    Id = 0,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Account Opening Balance deleted successfully",
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