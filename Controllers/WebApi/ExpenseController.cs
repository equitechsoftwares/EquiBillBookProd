using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Util;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class ExpenseController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllExpenses(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                //&& a.BranchId == obj.BranchId 
                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                ).Select(a => new ClsExpenseVm
                {
                    IsReverseCharge = a.IsReverseCharge,
                    Type = a.Type,
                    Subtotal = a.Subtotal,
                    TaxAmount = a.TaxAmount,
                    TotalQuantity = a.TotalQuantity,
                    GrandTotal = a.GrandTotal,
                    AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
                    AccountId = a.AccountId,
                    BranchId = a.BranchId,
                    ExpenseId = a.ExpenseId,
                    CustomerId = a.CustomerId,
                    SupplierId = a.SupplierId,
                    ExpenseCode = a.ExpenseCode,
                    Date = a.Date,
                    CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
                    SupplierName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.Name).FirstOrDefault(),
                    ReferenceNo = a.ReferenceNo,
                    //Amount = a.Amount,
                    Notes = a.Notes,
                    AttachDocument = a.AttachDocument,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    CompanyId = a.CompanyId,
                    AddedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ModifiedBy).Select(b => b.Username).FirstOrDefault(),
                    //Amount = oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.ExpenseId == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    //    Paid = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Count() == 0 ? 0 :
                    //oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                    //    PaymentStatus = a.PaymentStatus,
                    //    Due = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Count() == 0 ? a.Amount :
                    //a.Amount - oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.BranchId == obj.BranchId
                               && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                               ).Select(a => new ClsExpenseVm
                               {
                                   IsReverseCharge = a.IsReverseCharge,
                                   Type = a.Type,
                                   Subtotal = a.Subtotal,
                                   TaxAmount = a.TaxAmount,
                                   TotalQuantity = a.TotalQuantity,
                                   GrandTotal = a.GrandTotal,
                                   AccountName = oConnectionContext.DbClsAccount.Where(b => b.AccountId == a.AccountId).Select(b => b.AccountName).FirstOrDefault(),
                                   AccountId = a.AccountId,
                                   BranchId = a.BranchId,
                                   ExpenseId = a.ExpenseId,
                                   CustomerId = a.CustomerId,
                                   SupplierId = a.SupplierId,
                                   ExpenseCode = a.ExpenseCode,
                                   Date = a.Date,
                                   CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
                                   SupplierName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.Name).FirstOrDefault(),
                                   ReferenceNo = a.ReferenceNo,
                                   //Amount = a.Amount,
                                   Notes = a.Notes,
                                   AttachDocument = a.AttachDocument,
                                   IsActive = a.IsActive,
                                   IsDeleted = a.IsDeleted,
                                   AddedBy = a.AddedBy,
                                   AddedOn = a.AddedOn,
                                   ModifiedBy = a.ModifiedBy,
                                   ModifiedOn = a.ModifiedOn,
                                   CompanyId = a.CompanyId,
                                   AddedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Username).FirstOrDefault(),
                                   ModifiedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ModifiedBy).Select(b => b.Username).FirstOrDefault(),
                                   //Amount = oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.ExpenseId == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                                   //    Paid = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Count() == 0 ? 0 :
                                   //oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                                   //    PaymentStatus = a.PaymentStatus,
                                   //    Due = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Count() == 0 ? a.Amount :
                                   //a.Amount - oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                               }).ToList();
            }

            //if (obj.ExpenseCategoryId != 0)
            //{
            //    det = det.Where(a => a.ExpenseCategoryId == obj.ExpenseCategoryId).Select(a => a).ToList();
            //}

            if (obj.AccountId != 0)
            {
                det = det.Where(a => a.AccountId == obj.AccountId).Select(a => a).ToList();
            }

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.UserId == obj.UserId).Select(a => a).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
            {
                det = det.Where(a => a.ReferenceNo == obj.ReferenceNo).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Expense(ClsExpense obj)
        {
            var det = oConnectionContext.DbClsExpense.Where(a => a.ExpenseId == obj.ExpenseId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.TaxableAmount,
                a.BusinessRegistrationNameId,
                a.BusinessRegistrationNo,
                a.IsReverseCharge,
                a.VehicleId,
                a.MileageRate,
                a.UnitId,
                a.Type,
                a.GstTreatment,
                a.SourceOfSupplyId,
                a.DestinationOfSupplyId,
                a.CalculateMileageUsing,
                a.OdometerStartReading,
                a.OdometerEndReading,
                a.Distance,
                a.IsBillable,
                a.MarkupBy,
                Subtotal = a.Subtotal,
                TaxAmount = a.TaxAmount,
                TotalQuantity = a.TotalQuantity,
                GrandTotal = a.GrandTotal,
                a.BranchId,
                a.ExpenseId,
                a.CustomerId,
                a.SupplierId,
                a.ExpenseCode,
                a.Date,
                a.ReferenceNo,
                //a.Amount,
                a.Notes,
                a.AttachDocument,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.UserId,
                a.AccountId,
                //Amount = oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && b.ExpenseId == a.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                ExpensePayments = oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId && b.IsDeleted == false).Select(b => new
                {
                    b.AmountExcTax,
                    b.ExpensePaymentId,
                    b.AccountId,
                    b.Amount,
                    b.Notes,
                    b.TaxId,
                    Tax = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.Tax).FirstOrDefault(),
                    b.ITCType,
                    b.TaxExemptionId,
                    b.ItemType,
                    b.ItemCodeId
                })
            }).FirstOrDefault();

            var AllTaxs = oConnectionContext.DbClsExpensePayment.Where(a => a.ExpenseId == det.ExpenseId && a.IsDeleted == false).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                a.AmountExcTax
            }).ToList();

            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
            foreach (var item in AllTaxs)
            {
                bool CanDelete = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.CanDelete).FirstOrDefault();
                if (CanDelete == true)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                        });
                    }
                }
            }

            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                     (k, c) => new
                     {
                         //TaxId = k,
                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                     }
                    ).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expense = det,
                    Taxs = finalTaxs,
                    //ExpenseSubCategories = ExpenseSubCategories
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertExpense(ClsExpenseVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2).Select(a => a.TransactionId).FirstOrDefault();

                var Transaction = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => new ClsTransactionVm
                {
                    StartDate = a.StartDate,
                    ExpiryDate = a.ExpiryDate,
                }).FirstOrDefault();

                int TotalPurchaseBillUsed = oConnectionContext.DbClsPurchase.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();

                int TotalExpenseUsed = oConnectionContext.DbClsExpense.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();

                int TotalOrder = oCommonController.fetchPlanQuantity(obj.CompanyId, "Bill");
                if ((TotalPurchaseBillUsed + TotalExpenseUsed) >= TotalOrder)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Purchase Bill/ Expense quota already used. Please upgrade addons from My Plan Menu",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.GstTreatment == null || obj.GstTreatment == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divGstTreatment" });
                    isError = true;
                }

                if (obj.CountryId == 2 && obj.Type != "Mileage")
                {
                    if (obj.GstTreatment != null && obj.GstTreatment != "")
                    {
                        if (obj.GstTreatment == "Taxable Supply (Registered)" || obj.GstTreatment == "Composition Taxable Supply" ||
    obj.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || obj.GstTreatment == "Deemed Export" || obj.GstTreatment == "Supply by SEZ Developer")
                        {
                            if (obj.BusinessRegistrationNo == "" || obj.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                                isError = true;
                            }
                        }

                        if (obj.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }

                        if (obj.DestinationOfSupplyId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDestinationOfSupply" });
                            isError = true;
                        }
                    }
                }

                if (obj.AccountId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAccount" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.ExpensePayments == null || obj.ExpensePayments.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Please add expense", Id = "divExpensePayment" });
                    isError = true;
                }

                if (obj.ExpensePayments != null)
                {
                    foreach (var item in obj.ExpensePayments)
                    {
                        if(item.IsDeleted == false)
                        {
                            if (item.Amount == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divAmountExcTax" + item.DivId });
                                isError = true;
                            }
                        }                        
                    }
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsExpense.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Refrence No exists", Id = "divReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.Type == "Mileage")
                {
                    if (obj.MileageRate == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMileageRate" });
                        isError = true;
                    }
                    if (obj.CalculateMileageUsing == "Distance Travelled")
                    {
                        if (obj.Distance == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDistance" });
                            isError = true;
                        }
                    }
                    else
                    {
                        if (obj.OdometerStartReading == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divOdometerStartReading" });
                            isError = true;
                        }
                        if (obj.OdometerEndReading == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divOdometerEndReading" });
                            isError = true;
                        }
                    }
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

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    // Hybrid approach: Check Supplier/Customer PrefixId first, then fall back to Branch PrefixId
                    long userPrefixId = 0;
                    if (obj.SupplierId != 0)
                    {
                        userPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => a.PrefixId).FirstOrDefault();
                    }
                    else if (obj.CustomerId != 0)
                    {
                        userPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => a.PrefixId).FirstOrDefault();
                    }
                    
                    if (userPrefixId != 0)
                    {
                        // Use Supplier/Customer's PrefixId if set
                        PrefixId = userPrefixId;
                    }
                    else
                    {
                        // Fall back to Branch PrefixId
                        PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    }
                    
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == obj.Type.ToLower()
                                          && b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.SourceOfSupplyId,
                }).FirstOrDefault();

                string BusinessLegalName = "", BusinessTradeName = "", PanNo = "";

                if (userDet != null)
                {
                    BusinessLegalName = userDet.BusinessLegalName;
                    BusinessTradeName = userDet.BusinessTradeName;
                    PanNo = userDet.PanNo;
                    obj.BusinessRegistrationNameId = userDet.BusinessRegistrationNameId;
                    if (obj.Type == "Mileage")
                    {
                        obj.BusinessRegistrationNo = userDet.BusinessRegistrationNo;
                        obj.SourceOfSupplyId = userDet.SourceOfSupplyId;
                        obj.DestinationOfSupplyId = userDet.SourceOfSupplyId;
                    }
                }

                ClsExpense oExpense = new ClsExpense()
                {
                    Subtotal = obj.Subtotal,
                    TaxAmount = obj.GrandTotal - obj.Subtotal,
                    TotalQuantity = obj.TotalQuantity,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.Subtotal : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    ExpenseCode = obj.ExpenseCode,
                    CustomerId = obj.CustomerId,
                    SupplierId = obj.SupplierId,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    ReferenceNo = obj.ReferenceNo,
                    //Amount = obj.Amount,
                    Notes = obj.Notes,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BranchId = obj.BranchId,
                    UserId = obj.UserId,
                    AccountId = obj.AccountId,
                    ReferenceId = oCommonController.CreateToken(),
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    CalculateMileageUsing = obj.CalculateMileageUsing,
                    OdometerStartReading = obj.OdometerStartReading,
                    OdometerEndReading = obj.OdometerEndReading,
                    Distance = obj.Distance,
                    MileageRate = obj.MileageRate,
                    IsBillable = obj.IsBillable,
                    MarkupBy = obj.MarkupBy,
                    Type = obj.Type,
                    VehicleId = obj.VehicleId,
                    UnitId = obj.UnitId,
                    //IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = obj.GstTreatment,
                    BusinessRegistrationNameId = obj.BusinessRegistrationNameId,
                    BusinessRegistrationNo = obj.BusinessRegistrationNo,
                    BusinessLegalName = BusinessLegalName,
                    BusinessTradeName = BusinessTradeName,
                    PanNo = PanNo,
                    IsReverseCharge = obj.IsReverseCharge,
                    PrefixId = PrefixId
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/Expense/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Expense/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oExpense.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsExpense.Add(oExpense);
                oConnectionContext.SaveChanges();

                if (obj.ExpensePayments != null)
                {
                    foreach (var item in obj.ExpensePayments)
                    {
                        if (item.Amount != 0)
                        {
                            string AccountType = "";

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            //decimal AmountExcTax = obj.IsReverseCharge == 1 ? item.Amount : item.AmountExcTax;
                            decimal AmountExcTax = item.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == item.TaxId
                                           select new
                                           {
                                               TaxId = a.SubTaxId,
                                               Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                               TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                           }).ToList();

                            foreach (var tax in taxs)
                            {
                                oClsTaxVm.Add(new ClsTaxVm
                                {
                                    TaxId = tax.TaxId,
                                    Tax = tax.Tax,
                                    TaxPercent = tax.TaxPercent,
                                    TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                                });
                            }

                            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                     (k, c) => new
                                     {
                                         TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                     }
                                    ).ToList();

                            List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                            if (obj.CountryId == 2)
                            {
                                if (item.ITCType == "Eligible For ITC")
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Reverse Charge Tax Input but not due";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                }
                                else
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Tax Paid Expense";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        AccountType = "Tax Paid Expense";
                                    }
                                }
                            }
                            else
                            {
                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                {
                                    TaxType = "Normal",
                                    TaxId = a.TaxId,
                                    TaxPercent = a.TaxPercent,
                                    TaxAmount = a.TaxAmount,
                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                }).ToList();
                            }

                            //long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.ExpenseAccountId).FirstOrDefault();
                            ClsExpensePayment oClsExpensePayment = new ClsExpensePayment()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                IsActive = obj.IsActive,
                                IsDeleted = obj.IsDeleted,
                                ExpenseId = oExpense.ExpenseId,
                                Notes = item.Notes,
                                Amount = item.Amount,
                                AccountId = item.AccountId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                TaxAmount = item.TaxAmount,
                                //TaxAccountId = TaxAccountId,
                                ITCType = item.ITCType,
                                TaxExemptionId = item.TaxExemptionId,
                                ItemType = item.ItemType,
                                ItemCodeId = item.ItemCodeId
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsExpensePayment.Add(oClsExpensePayment);
                            oConnectionContext.SaveChanges();

                            if (AccountType != "")
                            {
                                var AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false 
                                && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                ClsExpenseTaxJournal oClsExpenseTaxJournal = new ClsExpenseTaxJournal()
                                {
                                    ExpenseId = oExpense.ExpenseId,
                                    ExpensePaymentId = oClsExpensePayment.ExpensePaymentId,
                                    TaxId = item.TaxId,
                                    TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : item.TaxAmount,
                                    AccountId = AccountId,
                                    ExpenseTaxJournalType = "Normal"
                                };
                                oConnectionContext.DbClsExpenseTaxJournal.Add(oClsExpenseTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            foreach (var taxJournal in taxList)
                            {
                                ClsExpenseTaxJournal oClsExpenseTaxJournal = new ClsExpenseTaxJournal()
                                {
                                    ExpenseId = oExpense.ExpenseId,
                                    ExpensePaymentId = oClsExpensePayment.ExpensePaymentId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    ExpenseTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsExpenseTaxJournal.Add(oClsExpenseTaxJournal);
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.Type,
                    CompanyId = obj.CompanyId,
                    Description = obj.Type+" \"" + obj.ReferenceNo + "\" created",
                    Id = oExpense.ExpenseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = obj.Type +" created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateExpense(ClsExpenseVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.GstTreatment == null || obj.GstTreatment == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divGstTreatment" });
                    isError = true;
                }

                if (obj.CountryId == 2 && obj.Type != "Mileage")
                {
                    if (obj.GstTreatment != null && obj.GstTreatment != "")
                    {
                        if (obj.GstTreatment == "Taxable Supply (Registered)" || obj.GstTreatment == "Composition Taxable Supply" ||
    obj.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || obj.GstTreatment == "Deemed Export" || obj.GstTreatment == "Supply by SEZ Developer")
                        {
                            if (obj.BusinessRegistrationNo == "" || obj.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                                isError = true;
                            }
                        }

                        if (obj.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }

                        if (obj.DestinationOfSupplyId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDestinationOfSupply" });
                            isError = true;
                        }
                    }
                }

                if (obj.AccountId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAccount" });
                    isError = true;
                }

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.ExpensePayments == null || obj.ExpensePayments.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Please add expense", Id = "divExpensePayment" });
                    isError = true;
                }

                if (obj.ExpensePayments != null)
                {
                    foreach (var item in obj.ExpensePayments)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Amount == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divAmountExcTax" + item.DivId });
                                isError = true;
                            }
                        }                            
                    }
                }

                if (obj.Type == "Mileage")
                {
                    if (obj.MileageRate == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMileageRate" });
                        isError = true;
                    }
                    if (obj.CalculateMileageUsing == "Distance Travelled")
                    {
                        if (obj.Distance == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDistance" });
                            isError = true;
                        }
                    }
                    else
                    {
                        if (obj.OdometerStartReading == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divOdometerStartReading" });
                            isError = true;
                        }
                        if (obj.OdometerEndReading == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divOdometerEndReading" });
                            isError = true;
                        }
                    }
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

                //decimal Paid = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "expense payment" && b.IsDeleted == false && b.IsCancelled == false && b.Id == obj.ExpenseId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                //if (Paid == obj.Amount)
                //{
                //    obj.PaymentStatus = "Paid";
                //}
                //else if (Paid > obj.Amount)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "More amount is already paid",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}
                //else
                //{
                //    obj.PaymentStatus = "Partially Paid";
                //}

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.SourceOfSupplyId
                }).FirstOrDefault();

                string BusinessLegalName = "", BusinessTradeName = "", PanNo = "";

                if (userDet != null)
                {
                    BusinessLegalName = userDet.BusinessLegalName;
                    BusinessTradeName = userDet.BusinessTradeName;
                    PanNo = userDet.PanNo;
                    obj.BusinessRegistrationNameId = userDet.BusinessRegistrationNameId;
                    if (obj.Type == "Mileage")
                    {
                        obj.BusinessRegistrationNo = userDet.BusinessRegistrationNo;
                        obj.SourceOfSupplyId = userDet.SourceOfSupplyId;
                        obj.DestinationOfSupplyId = userDet.SourceOfSupplyId;
                    }
                }

                ClsExpense oExpense = new ClsExpense()
                {
                    ExpenseId = obj.ExpenseId,
                    Subtotal = obj.Subtotal,
                    TaxAmount = obj.GrandTotal - obj.Subtotal,
                    TotalQuantity = obj.TotalQuantity,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.Subtotal : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    CustomerId = obj.CustomerId,
                    SupplierId = obj.SupplierId,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    ReferenceNo = obj.ReferenceNo,
                    //Amount = obj.Amount,
                    Notes = obj.Notes,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    BranchId = obj.BranchId,
                    UserId = obj.UserId,
                    AccountId = obj.AccountId,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    CalculateMileageUsing = obj.CalculateMileageUsing,
                    OdometerStartReading = obj.OdometerStartReading,
                    OdometerEndReading = obj.OdometerEndReading,
                    Distance = obj.Distance,
                    MileageRate = obj.MileageRate,
                    IsBillable = obj.IsBillable,
                    MarkupBy = obj.MarkupBy,
                    //Type = obj.Type,
                    VehicleId = obj.VehicleId,
                    UnitId = obj.UnitId,
                    //IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = obj.GstTreatment,
                    BusinessRegistrationNameId = obj.BusinessRegistrationNameId,
                    BusinessRegistrationNo = obj.BusinessRegistrationNo,
                    BusinessLegalName = BusinessLegalName,
                    BusinessTradeName = BusinessTradeName,
                    PanNo = PanNo,
                    IsReverseCharge = obj.IsReverseCharge,
                };

                string pic1 = oConnectionContext.DbClsExpense.Where(a => a.ExpenseId == obj.ExpenseId).Select(a => a.AttachDocument).FirstOrDefault();
                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/Expense/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/Expense/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oExpense.AttachDocument = filepathPass;
                }
                else
                {
                    oExpense.AttachDocument = pic1;
                }

                oConnectionContext.DbClsExpense.Attach(oExpense);
                oConnectionContext.Entry(oExpense).Property(x => x.ExpenseId).IsModified = true;
                //oConnectionContext.Entry(oExpense).Property(x => x.ExpenseCode).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.SupplierId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.Date).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.ReferenceNo).IsModified = true;
                //oConnectionContext.Entry(oExpense).Property(x => x.Amount).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.UserId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.GstTreatment).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.SourceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.DestinationOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.CalculateMileageUsing).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.OdometerStartReading).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.OdometerEndReading).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.Distance).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.MileageRate).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.IsBillable).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.MarkupBy).IsModified = true;
                //oConnectionContext.Entry(oExpense).Property(x => x.Type).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.VehicleId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.UnitId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.GstTreatment).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.BusinessRegistrationNameId).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.BusinessRegistrationNo).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.IsReverseCharge).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.BusinessLegalName).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.BusinessTradeName).IsModified = true;
                oConnectionContext.Entry(oExpense).Property(x => x.PanNo).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.ExpensePayments != null)
                {
                    foreach (var item in obj.ExpensePayments)
                    {
                        if (item.IsDeleted == true)
                        {
                            string query = "update \"tblExpensePayment\" set \"IsDeleted\"=True where \"ExpensePaymentId\"=" + item.ExpensePaymentId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                        else
                        {
                            if (item.Amount != 0)
                            {
                                //long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.ExpenseAccountId).FirstOrDefault();
                                if (item.ExpensePaymentId == 0)
                                {
                                    string AccountType = "";

                                    var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                                    List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                                    //decimal AmountExcTax = obj.IsReverseCharge == 1 ? item.Amount : item.AmountExcTax;
                                    decimal AmountExcTax = item.AmountExcTax;
                                    var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                                    {
                                        a.TaxId,
                                        a.Tax,
                                        a.TaxPercent,
                                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                                   where a.TaxId == item.TaxId
                                                   select new
                                                   {
                                                       TaxId = a.SubTaxId,
                                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                                   }).ToList();

                                    foreach (var tax in taxs)
                                    {
                                        oClsTaxVm.Add(new ClsTaxVm
                                        {
                                            TaxId = tax.TaxId,
                                            Tax = tax.Tax,
                                            TaxPercent = tax.TaxPercent,
                                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                                        });
                                    }

                                    var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                             (k, c) => new
                                             {
                                                 TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                                 Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                                 TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                                 TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                             }
                                            ).ToList();

                                    List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                                    if (obj.CountryId == 2)
                                    {
                                        if (item.ITCType == "Eligible For ITC")
                                        {
                                            if (obj.IsReverseCharge == 1)
                                            {
                                                AccountType = "Reverse Charge Tax Input but not due";

                                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                                {
                                                    TaxType = "Reverse Charge",
                                                    TaxId = a.TaxId,
                                                    TaxPercent = a.TaxPercent,
                                                    TaxAmount = a.TaxAmount,
                                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                                }).ToList();
                                            }
                                            else
                                            {
                                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                                {
                                                    TaxType = "Normal",
                                                    TaxId = a.TaxId,
                                                    TaxPercent = a.TaxPercent,
                                                    TaxAmount = a.TaxAmount,
                                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                                }).ToList();
                                            }
                                        }
                                        else
                                        {
                                            if (obj.IsReverseCharge == 1)
                                            {
                                                AccountType = "Tax Paid Expense";

                                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                                {
                                                    TaxType = "Reverse Charge",
                                                    TaxId = a.TaxId,
                                                    TaxPercent = a.TaxPercent,
                                                    TaxAmount = a.TaxAmount,
                                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                                }).ToList();
                                            }
                                            else
                                            {
                                                AccountType = "Tax Paid Expense";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                        }).ToList();
                                    }

                                    ClsExpensePayment oClsExpensePayment = new ClsExpensePayment()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        CompanyId = obj.CompanyId,
                                        IsActive = obj.IsActive,
                                        IsDeleted = obj.IsDeleted,
                                        ExpenseId = oExpense.ExpenseId,
                                        Notes = item.Notes,
                                        Amount = item.Amount,
                                        AccountId = item.AccountId,
                                        TaxId = item.TaxId,
                                        AmountExcTax = item.AmountExcTax,
                                        TaxAmount = item.TaxAmount,
                                        //TaxAccountId = TaxAccountId,
                                        ITCType = item.ITCType,
                                        TaxExemptionId = item.TaxExemptionId,
                                        ItemType = item.ItemType,
                                        ItemCodeId = item.ItemCodeId
                                    };
                                    //ConnectionContext ocon = new ConnectionContext();
                                    oConnectionContext.DbClsExpensePayment.Add(oClsExpensePayment);
                                    oConnectionContext.SaveChanges();

                                    if (AccountType != "")
                                    {
                                        var AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                        && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                        ClsExpenseTaxJournal oClsExpenseTaxJournal = new ClsExpenseTaxJournal()
                                        {
                                            ExpenseId = oExpense.ExpenseId,
                                            ExpensePaymentId = oClsExpensePayment.ExpensePaymentId,
                                            TaxId = item.TaxId,
                                            TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : item.TaxAmount,
                                            AccountId = AccountId,
                                            ExpenseTaxJournalType = "Normal"
                                        };
                                        oConnectionContext.DbClsExpenseTaxJournal.Add(oClsExpenseTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }

                                    foreach (var taxJournal in taxList)
                                    {
                                        ClsExpenseTaxJournal oClsExpenseTaxJournal = new ClsExpenseTaxJournal()
                                        {
                                            ExpenseId = oExpense.ExpenseId,
                                            ExpensePaymentId = oClsExpensePayment.ExpensePaymentId,
                                            TaxId = taxJournal.TaxId,
                                            TaxAmount = taxJournal.TaxAmount,
                                            AccountId = taxJournal.AccountId,
                                            ExpenseTaxJournalType = taxJournal.TaxType
                                        };
                                        oConnectionContext.DbClsExpenseTaxJournal.Add(oClsExpenseTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }
                                }
                                else
                                {
                                    string AccountType = "";

                                    var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                                    List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                                    //decimal AmountExcTax = obj.IsReverseCharge == 1 ? item.Amount : item.AmountExcTax;
                                    decimal AmountExcTax = item.AmountExcTax;
                                    var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                                    {
                                        a.TaxId,
                                        a.Tax,
                                        a.TaxPercent,
                                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                                   where a.TaxId == item.TaxId
                                                   select new
                                                   {
                                                       TaxId = a.SubTaxId,
                                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                                   }).ToList();

                                    foreach (var tax in taxs)
                                    {
                                        oClsTaxVm.Add(new ClsTaxVm
                                        {
                                            TaxId = tax.TaxId,
                                            Tax = tax.Tax,
                                            TaxPercent = tax.TaxPercent,
                                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                                        });
                                    }

                                    var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                             (k, c) => new
                                             {
                                                 TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                                 Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                                 TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                                 TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                             }
                                            ).ToList();

                                    List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                                    if (obj.CountryId == 2)
                                    {
                                        if (item.ITCType == "Eligible For ITC")
                                        {
                                            if (obj.IsReverseCharge == 1)
                                            {
                                                AccountType = "Reverse Charge Tax Input but not due";

                                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                                {
                                                    TaxType = "Reverse Charge",
                                                    TaxId = a.TaxId,
                                                    TaxPercent = a.TaxPercent,
                                                    TaxAmount = a.TaxAmount,
                                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                                }).ToList();
                                            }
                                            else
                                            {
                                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                                {
                                                    TaxType = "Normal",
                                                    TaxId = a.TaxId,
                                                    TaxPercent = a.TaxPercent,
                                                    TaxAmount = a.TaxAmount,
                                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                                }).ToList();
                                            }
                                        }
                                        else
                                        {
                                            if (obj.IsReverseCharge == 1)
                                            {
                                                AccountType = "Tax Paid Expense";

                                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                                {
                                                    TaxType = "Reverse Charge",
                                                    TaxId = a.TaxId,
                                                    TaxPercent = a.TaxPercent,
                                                    TaxAmount = a.TaxAmount,
                                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                                }).ToList();
                                            }
                                            else
                                            {
                                                AccountType = "Tax Paid Expense";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
                                        }).ToList();
                                    }

                                    ClsExpensePayment oClsExpensePayment = new ClsExpensePayment()
                                    {
                                        ExpensePaymentId = item.ExpensePaymentId,
                                        ModifiedBy = obj.ModifiedBy,
                                        ModifiedOn = CurrentDate,
                                        Notes = item.Notes,
                                        Amount = item.Amount,
                                        AccountId = item.AccountId,
                                        TaxId = item.TaxId,
                                        AmountExcTax = item.AmountExcTax,
                                        TaxAmount = item.TaxAmount,
                                        //TaxAccountId = TaxAccountId,
                                        ITCType = item.ITCType,
                                        TaxExemptionId = item.TaxExemptionId,
                                        ItemType = item.ItemType,
                                        ItemCodeId = item.ItemCodeId
                                    };
                                    oConnectionContext.DbClsExpensePayment.Attach(oClsExpensePayment);
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.ModifiedBy).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.ModifiedOn).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.Notes).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.Amount).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.AccountId).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.TaxId).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.AmountExcTax).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.TaxAmount).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.TaxAccountId).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.ITCType).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.TaxExemptionId).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.ItemType).IsModified = true;
                                    oConnectionContext.Entry(oClsExpensePayment).Property(x => x.ItemCodeId).IsModified = true;
                                    oConnectionContext.SaveChanges();

                                    string query = "delete from \"tblExpenseTaxJournal\" where \"ExpenseId\"=" + oExpense.ExpenseId + " and \"ExpensePaymentId\"=" + oClsExpensePayment.ExpensePaymentId;
                                    oConnectionContext.Database.ExecuteSqlCommand(query);

                                    if (AccountType != "")
                                    {
                                        var AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                        && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                        ClsExpenseTaxJournal oClsExpenseTaxJournal = new ClsExpenseTaxJournal()
                                        {
                                            ExpenseId = oExpense.ExpenseId,
                                            ExpensePaymentId = oClsExpensePayment.ExpensePaymentId,
                                            TaxId = item.TaxId,
                                            TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : item.TaxAmount,
                                            AccountId = AccountId,
                                            ExpenseTaxJournalType = "Normal"
                                        };
                                        oConnectionContext.DbClsExpenseTaxJournal.Add(oClsExpenseTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }

                                    foreach (var taxJournal in taxList)
                                    {
                                        ClsExpenseTaxJournal oClsExpenseTaxJournal = new ClsExpenseTaxJournal()
                                        {
                                            ExpenseId = oExpense.ExpenseId,
                                            ExpensePaymentId = oClsExpensePayment.ExpensePaymentId,
                                            TaxId = taxJournal.TaxId,
                                            TaxAmount = taxJournal.TaxAmount,
                                            AccountId = taxJournal.AccountId,
                                            ExpenseTaxJournalType = taxJournal.TaxType
                                        };
                                        oConnectionContext.DbClsExpenseTaxJournal.Add(oClsExpenseTaxJournal);
                                        oConnectionContext.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.Type,
                    CompanyId = obj.CompanyId,
                    Description = obj.Type +" \"" + obj.ReferenceNo + "\" updated",
                    Id = oExpense.ExpenseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = obj.Type +" updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseActiveInactive(ClsExpenseVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsExpense oClsRole = new ClsExpense()
                {
                    ExpenseId = obj.ExpenseId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsExpense.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ExpenseId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Expense",
                    CompanyId = obj.CompanyId,
                    Description = "Expense \"" + oConnectionContext.DbClsExpense.Where(a => a.ExpenseId == obj.ExpenseId).Select(a => a.ReferenceNo).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.ExpenseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Expense " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseDelete(ClsExpenseVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsExpense oClsRole = new ClsExpense()
                {
                    ExpenseId = obj.ExpenseId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsExpense.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ExpenseId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Expense",
                    CompanyId = obj.CompanyId,
                    Description = "Expense \"" + oConnectionContext.DbClsExpense.Where(a => a.ExpenseId == obj.ExpenseId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.ExpenseId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Expense deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpensePaymentDelete(ClsExpensePaymentVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.ExpenseId != 0)
                {
                    string query = "update \"tblExpensePayment\" set \"IsDeleted\"=True where \"ExpenseId\"=" + obj.ExpenseId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblExpensePayment\" set \"IsDeleted\"=True where \"ExpensePaymentId\"=" + obj.ExpensePaymentId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

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

        public async Task<IHttpActionResult> ExpenseJournal(ClsExpenseVm obj)
        {
            //var AllTaxs = oConnectionContext.DbClsExpensePayment.Where(a => a.ExpenseId == obj.ExpenseId
            //&& a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
            //    a.TaxId,
            //    a.AmountExcTax
            //}).ToList();

            //List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
            //foreach (var item in AllTaxs)
            //{
            //    decimal AmountExcTax = item.AmountExcTax;
            //    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
            //    {
            //        a.TaxId,
            //        a.Tax,
            //        a.TaxPercent,
            //    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
            //                   where a.TaxId == item.TaxId
            //                   select new
            //                   {
            //                       TaxId = a.SubTaxId,
            //                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
            //                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
            //                   }).ToList();

            //    foreach (var tax in taxs)
            //    {
            //        oClsTaxVm.Add(new ClsTaxVm
            //        {
            //            TaxId = tax.TaxId,
            //            Tax = tax.Tax,
            //            TaxPercent = tax.TaxPercent,
            //            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
            //        });
            //    }
            //}

            //var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
            //         (k, c) => new
            //         {
            //             TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
            //             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
            //             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
            //             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
            //         }
            //        ).ToList();

            //var taxList = finalTaxs.Select(a => new ClsBankPaymentVm
            //{
            //    AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId ==
            //    oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
            //    ).Select(c => c.AccountName).FirstOrDefault(),
            //    Debit = a.TaxAmount,
            //    Credit = 0,
            //    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.ExpenseAccountId).FirstOrDefault()
            //}).ToList();

            var taxList = (from q in oConnectionContext.DbClsExpenseTaxJournal
                           join a in oConnectionContext.DbClsExpensePayment
                           on q.ExpensePaymentId equals a.ExpensePaymentId
                           join b in oConnectionContext.DbClsExpense
                        on a.ExpenseId equals b.ExpenseId
                           where q.ExpenseId == obj.ExpenseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && b.IsDeleted == false && b.IsActive == true
                 //&& a.TaxAmount != 0
                           select new
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = (q.ExpenseTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                               Credit = (q.ExpenseTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                               AccountId = q.AccountId
                           }).ToList();

            var expenseAccount = (from a in oConnectionContext.DbClsExpensePayment
                                  join b in oConnectionContext.DbClsExpense
                               on a.ExpenseId equals b.ExpenseId
                                  where a.ExpenseId == obj.ExpenseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                  //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                  select new ClsBankPaymentVm
                                  {
                                      AccountId = a.AccountId,
                                      Debit = a.AmountExcTax,//b.IsReverseCharge == 1 ? a.Amount : a.AmountExcTax,
                                      Credit = 0
                                  }).ToList();

            var journal = (from a in oConnectionContext.DbClsExpense
                               //   join b in oConnectionContext.DbClsExpensePayment
                               //on a.ExpenseId equals b.ExpenseId
                           where a.ExpenseId == obj.ExpenseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false  && a.IsActive == true
                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = 0,
                               Credit =a.IsReverseCharge == 1 ? oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == obj.ExpenseId &&
                               c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum() :
                               oConnectionContext.DbClsExpensePayment.Where(c => c.ExpenseId == obj.ExpenseId &&
                               c.IsDeleted == false && c.IsActive == true).Select(c => c.Amount).DefaultIfEmpty().Sum(),
                           }).Take(1)
                           //.Concat(from a in oConnectionContext.DbClsExpensePayment
                           //                      //   join b in oConnectionContext.DbClsExpense
                           //                      //on a.ExpenseId equals b.ExpenseId
                           //                  where a.ExpenseId == obj.ExpenseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           //                  //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //                  group a by a.AccountId into stdGroup
                           //                  //orderby stdGroup.Key descending
                           //                  select new ClsBankPaymentVm
                           //                  {
                           //                      AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                           //                      Debit = stdGroup.Select(x => x.AmountExcTax).DefaultIfEmpty().Sum(),
                           //                      Credit = 0
                           //                  })
                           //.Concat(from a in oConnectionContext.DbClsExpensePayment
                           //                                //   join b in oConnectionContext.DbClsExpense
                           //                                //on a.ExpenseId equals b.ExpenseId
                           //                            where a.ExpenseId == obj.ExpenseId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           //                            //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //                            group a by a.TaxAccountId into stdGroup
                           //                            //orderby stdGroup.Key descending
                           //                            select new ClsBankPaymentVm
                           //                            {
                           //                                // tax 
                           //                                AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                           //                                Debit = stdGroup.Select(x => x.TaxAmount).DefaultIfEmpty().Sum(),
                           //                                Credit = 0,
                           //                            })
                           .ToList();

            journal = journal.Concat(from a in expenseAccount
                                     group a by a.AccountId into stdGroup
                                     select new ClsBankPaymentVm
                                     {
                                         // tax 
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = stdGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                         Credit = stdGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                     }).Concat(from a in taxList
                                               select new ClsBankPaymentVm
                                               {
                                                   // tax 
                                                   AccountName = a.AccountName,
                                                   Debit = a.Debit,
                                                   Credit = a.Credit,
                                                   IsTaxAccount = true
                                               }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = journal
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportExpense(ClsExpenseVm obj)
        {
            //using (TransactionScope dbContextTransaction = new TransactionScope())
            //{
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.ExpenseImports == null || obj.ExpenseImports.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No data",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            int count = 1;
            if (obj.ExpenseImports != null)
            {
                foreach (var item in obj.ExpenseImports)
                {
                    int innerCount = 1;

                    foreach (var inner in obj.ExpenseImports)
                    {
                        if (item.ReferenceNo != "" && item.ReferenceNo != null)
                        {
                            if (item.ReferenceNo.ToLower() == inner.ReferenceNo.ToLower() && item.GroupName != inner.GroupName && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate ReferenceNo exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        innerCount++;
                    }
                    count++;
                }
            }

            count = 1;
            if (obj.ExpenseImports != null)
            {
                foreach (var item in obj.ExpenseImports)
                {
                    if (item.BranchName != "" && item.BranchName != null)
                    {
                        if ((from a in oConnectionContext.DbClsBranch
                             where a.IsDeleted == false && a.Branch.ToLower() == item.BranchName.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.BranchId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid BranchName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.User != "" && item.User != null)
                    {
                        if ((from a in oConnectionContext.DbClsUser
                             where a.IsDeleted == false && a.Username.ToLower() == item.User.ToLower()
                             && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "user"
                             select a.UserId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid User in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.CustomerUserName != "" && item.CustomerUserName != null)
                    {
                        if ((from a in oConnectionContext.DbClsUser
                             where a.IsDeleted == false && a.Username.ToLower() == item.CustomerUserName.ToLower()
                             && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer"
                             select a.UserId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid Customer Username in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.SupplierUserName != "" && item.SupplierUserName != null)
                    {
                        if ((from a in oConnectionContext.DbClsUser
                             where a.IsDeleted == false && a.Username.ToLower() == item.SupplierUserName.ToLower()
                             && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier"
                             select a.UserId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid Supplier Username in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.PaidThrough == "" || item.PaidThrough == null)
                    {
                        errors.Add(new ClsError { Message = "PaidThrough is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.PaidThrough != "" && item.PaidThrough != null)
                    {
                        if ((from a in oConnectionContext.DbClsAccount
                             where a.IsDeleted == false && a.AccountName.ToLower() == item.PaidThrough.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.AccountId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid PaidThrough in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.ReferenceNo != "" && item.ReferenceNo != null)
                    {
                        if (oConnectionContext.DbClsContra.Where(a => a.ReferenceNo == item.ReferenceNo && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "RefrenceNo in row no" + count + " is already taken", Id = "divReferenceNo" });
                            isError = true;
                        }
                    }

                    if (item.ExpenseAccount == "" || item.ExpenseAccount == null)
                    {
                        errors.Add(new ClsError { Message = "ExpenseAccount is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.ExpenseAccount != "" && item.ExpenseAccount != null)
                    {
                        if ((from a in oConnectionContext.DbClsAccount
                             where a.IsDeleted == false && a.AccountName.ToLower() == item.ExpenseAccount.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.AccountId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid ExpenseAccount in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.Amount == 0)
                    {
                        errors.Add(new ClsError { Message = "Amount is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.GroupName == "" || item.GroupName == null)
                    {
                        errors.Add(new ClsError { Message = "GroupName is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.Tax != "" && item.Tax != null)
                    {
                        if ((from a in oConnectionContext.DbClsTax
                             where a.IsDeleted == false && a.Tax.ToLower() == item.Tax.ToLower()
                             && a.CompanyId == obj.CompanyId
                             select a.TaxId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid Tax in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    count++;
                }
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

            List<ClsExpenseImport> expenseGroups = new List<ClsExpenseImport>();
            foreach (var item in obj.ExpenseImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    long BranchId = 0, PrefixUserMapId = 0, PaidThrough = 0, ExpenseAccount = 0, CustomerId = 0, SupplierId = 0, UserId;
                    string ReferenceNo = "";
                    decimal Subtotal = 0, GrandTotal = 0, TotalQuantity = 0, AmountExcTax = 0;

                    long ExpenseId = 0;
                    bool canVariationInsert = true;

                    if (item.BranchName == "" || item.BranchName == null)
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.BranchId).FirstOrDefault();
                    }
                    else
                    {
                        BranchId = oConnectionContext.DbClsBranch.Where(a => a.Branch == item.BranchName && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.BranchId).FirstOrDefault();
                    }

                    PaidThrough = oConnectionContext.DbClsAccount.Where(a => a.AccountName == item.PaidThrough).Select(a => a.AccountId).FirstOrDefault();

                    ExpenseAccount = oConnectionContext.DbClsAccount.Where(a => a.AccountName == item.ExpenseAccount).Select(a => a.AccountId).FirstOrDefault();

                    UserId = oConnectionContext.DbClsUser.Where(a => a.IsDeleted == false && a.Username.ToLower() == item.User.ToLower()
                             && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "user").Select(a => a.UserId).FirstOrDefault();

                    CustomerId = oConnectionContext.DbClsUser.Where(a => a.IsDeleted == false && a.Username.ToLower() == item.CustomerUserName.ToLower()
                             && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer").Select(a => a.UserId).FirstOrDefault();

                    SupplierId = oConnectionContext.DbClsUser.Where(a => a.IsDeleted == false && a.Username.ToLower() == item.SupplierUserName.ToLower()
                             && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier").Select(a => a.UserId).FirstOrDefault();

                    if (expenseGroups.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Count() == 0)
                    {
                        canVariationInsert = true;
                    }
                    else
                    {
                        ExpenseId = expenseGroups.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Select(a => a.ExpenseId).FirstOrDefault();
                        canVariationInsert = false;
                    }

                    if (canVariationInsert == true)
                    {
                        if (item.ReferenceNo == "" || item.ReferenceNo == null)
                        {
                            long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                  join b in oConnectionContext.DbClsPrefixUserMap
                                                   on a.PrefixMasterId equals b.PrefixMasterId
                                                  where a.IsActive == true && a.IsDeleted == false &&
                                                  b.CompanyId == obj.CompanyId && b.IsActive == true
                                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "expense"
                                                  && b.PrefixId == PrefixId
                                                  select new
                                                  {
                                                      b.PrefixUserMapId,
                                                      b.Prefix,
                                                      b.NoOfDigits,
                                                      b.Counter
                                                  }).FirstOrDefault();
                            PrefixUserMapId = prefixSettings.PrefixUserMapId;
                            ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                            //increase counter
                            string qq = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                            oConnectionContext.Database.ExecuteSqlCommand(qq);
                            //increase counter
                        }

                        var summary = obj.ExpenseImports.Where(a => a.GroupName.ToLower() == item.GroupName.ToLower()).Select(a => new
                        {
                            a.Amount,
                            AmountExcTax = (100 * (item.Amount / (100 + oConnectionContext.DbClsTax.Where(b => b.Tax.ToLower() == a.Tax.ToLower()).Select(b => b.TaxPercent).FirstOrDefault())))
                        }).ToList();

                        GrandTotal = summary.Select(a => a.Amount).DefaultIfEmpty().Sum();
                        Subtotal = summary.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();

                        ClsExpense oExpense = new ClsExpense()
                        {
                            Subtotal = Subtotal,
                            TaxAmount = GrandTotal - Subtotal,
                            TotalQuantity = summary.Count,
                            GrandTotal = GrandTotal,
                            ExpenseCode = obj.ExpenseCode,
                            CustomerId = CustomerId,
                            SupplierId = SupplierId,
                            Date = item.Date.AddHours(5).AddMinutes(30),
                            ReferenceNo = ReferenceNo,
                            Notes = obj.Notes,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            BranchId = BranchId,
                            UserId = UserId,
                            AccountId = PaidThrough,
                            ReferenceId = oCommonController.CreateToken(),
                            BatchNo = obj.BatchNo
                        };
                        oConnectionContext.DbClsExpense.Add(oExpense);
                        oConnectionContext.SaveChanges();

                        ExpenseId = oExpense.ExpenseId;

                        if (item.GroupName != "" && item.GroupName != null)
                        {
                            expenseGroups.Add(new ClsExpenseImport { GroupName = item.GroupName, ExpenseId = ExpenseId });
                        }
                    }

                    var TaxDetails = (from a in oConnectionContext.DbClsTax
                                      where a.IsDeleted == false && a.Tax.ToLower() == item.Tax.ToLower()
                                      && a.CompanyId == obj.CompanyId
                                      select new
                                      {
                                          a.TaxId,
                                          a.TaxPercent
                                      }).FirstOrDefault();

                    long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == TaxDetails.TaxId).Select(a => a.ExpenseAccountId).FirstOrDefault();
                    AmountExcTax = (100 * (item.Amount / (100 + TaxDetails.TaxPercent)));

                    ClsExpensePayment oClsExpensePayment = new ClsExpensePayment()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        ExpenseId = ExpenseId,
                        Notes = item.Notes,
                        Amount = item.Amount,
                        AccountId = ExpenseAccount,
                        TaxId = TaxDetails.TaxId,
                        AmountExcTax = AmountExcTax,
                        TaxAmount = item.Amount - AmountExcTax,
                        TaxAccountId = TaxAccountId
                    };
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.DbClsExpensePayment.Add(oClsExpensePayment);
                    oConnectionContext.SaveChanges();

                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Expense",
                CompanyId = obj.CompanyId,
                Description = "Expenses imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
            data = new
            {
                Status = 1,
                Message = "Expense imported successfully",
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseCountByBatch(ClsExpenseVm obj)
        {
            long TotalCount = oConnectionContext.DbClsExpense.Where(a => a.BatchNo == obj.BatchNo).Count();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TotalCount = TotalCount
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseDetails(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                ).ToList().Select(a => new ClsExpenseVm
                {
                    Date = a.Date,
                    ReferenceNo = a.ReferenceNo,
                    CustomerId = a.CustomerId,
                    SupplierId = a.SupplierId,
                    UserName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.UserId).Select(b => b.Name).FirstOrDefault(),
                    CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
                    SupplierName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.Name).FirstOrDefault(),
                    BranchId = a.BranchId,
                    UserId = a.UserId,
                    Subtotal = a.Subtotal,
                    TaxAmount = a.TaxAmount,
                    TotalQuantity = a.TotalQuantity,
                    GrandTotal = a.GrandTotal,
                    Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                    oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                    AccountId = a.AccountId,
                    ExpenseId = a.ExpenseId,
                }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                               && a.BranchId == obj.BranchId
                               && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                               DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                               ).ToList().Select(a => new ClsExpenseVm
                               {
                                   Date = a.Date,
                                   ReferenceNo = a.ReferenceNo,
                                   CustomerId = a.CustomerId,
                                   SupplierId = a.SupplierId,
                                   UserName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.UserId).Select(b => b.Name).FirstOrDefault(),
                                   CustomerName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.CustomerId).Select(b => b.Name).FirstOrDefault(),
                                   SupplierName = oConnectionContext.DbClsUser.Where(b => b.UserId == a.SupplierId).Select(b => b.Name).FirstOrDefault(),
                                   BranchId = a.BranchId,
                                   UserId = a.UserId,
                                   Subtotal = a.Subtotal,
                                   TaxAmount = a.TaxAmount,
                                   TotalQuantity = a.TotalQuantity,
                                   GrandTotal = a.GrandTotal,
                                   Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                                   oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                                   AccountId = a.AccountId,
                                   ExpenseId = a.ExpenseId,
                               }).ToList();
            }

            if (obj.AccountName != "" && obj.AccountName != null)
            {
                det = det.Where(a => a.Categorys.ToLower().Contains(obj.AccountName.ToLower())).Select(a => a).ToList();
            }

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.UserId == obj.UserId).Select(a => a).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
            {
                det = det.Where(a => a.ReferenceNo == obj.ReferenceNo).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseSummaryByCategory(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det1;
            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det1 = (from b in oConnectionContext.DbClsExpensePayment
                        join a in oConnectionContext.DbClsExpense
                       on b.ExpenseId equals a.ExpenseId
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = b.Amount,
                            AmountExcTax = b.AmountExcTax,
                            TaxAmount = b.TaxAmount,
                            AccountId = b.AccountId
                        }).ToList();
            }
            else
            {
                det1 = (from b in oConnectionContext.DbClsExpensePayment
                        join a in oConnectionContext.DbClsExpense
                       on b.ExpenseId equals a.ExpenseId
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                  && a.BranchId == obj.BranchId
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = b.Amount,
                            AmountExcTax = b.AmountExcTax,
                            TaxAmount = b.TaxAmount,
                            AccountId = b.AccountId
                        }).ToList();
            }

            det = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).ToList().Select(a =>
                    new ClsExpenseVm
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName,
                        Amount = det1.Where(b => b.AccountId == a.AccountId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                        AmountExcTax = det1.Where(b => b.AccountId == a.AccountId).Select(b => b.AmountExcTax).DefaultIfEmpty().Sum(),
                    }).OrderBy(a => a.AccountId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.AccountId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseDetailsByCategory(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsExpense
                       join b in oConnectionContext.DbClsExpensePayment
                      on a.ExpenseId equals b.ExpenseId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && b.AccountId == obj.AccountId
                       select new ClsExpenseVm
                       {
                           CustomerId = a.CustomerId,
                           SupplierId = a.SupplierId,
                           UserId = a.UserId,
                           ExpenseId = a.ExpenseId,
                           Date = a.Date,
                           ReferenceNo = a.ReferenceNo,
                           UserName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.UserId).Select(bb => bb.Name).FirstOrDefault(),
                           CustomerName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.CustomerId).Select(bb => bb.Name).FirstOrDefault(),
                           SupplierName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.SupplierId).Select(bb => bb.Name).FirstOrDefault(),
                           Amount = oConnectionContext.DbClsExpensePayment.Where(bb => bb.ExpenseId == a.ExpenseId && bb.AccountId == obj.AccountId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           AmountExcTax = oConnectionContext.DbClsExpensePayment.Where(bb => bb.ExpenseId == a.ExpenseId && bb.AccountId == obj.AccountId).Select(bb => bb.AmountExcTax).DefaultIfEmpty().Sum(),
                       }).GroupBy(x => x.ExpenseId)
       .Select(g => g.FirstOrDefault()).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsExpense
                       join b in oConnectionContext.DbClsExpensePayment
                      on a.ExpenseId equals b.ExpenseId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && b.AccountId == obj.AccountId
                       select new ClsExpenseVm
                       {
                           CustomerId = a.CustomerId,
                           SupplierId = a.SupplierId,
                           UserId = a.UserId,
                           ExpenseId = a.ExpenseId,
                           Date = a.Date,
                           ReferenceNo = a.ReferenceNo,
                           UserName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.UserId).Select(bb => bb.Name).FirstOrDefault(),
                           CustomerName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.CustomerId).Select(bb => bb.Name).FirstOrDefault(),
                           SupplierName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.SupplierId).Select(bb => bb.Name).FirstOrDefault(),
                           Amount = oConnectionContext.DbClsExpensePayment.Where(bb => bb.ExpenseId == a.ExpenseId && bb.AccountId == obj.AccountId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           AmountExcTax = oConnectionContext.DbClsExpensePayment.Where(bb => bb.ExpenseId == a.ExpenseId && bb.AccountId == obj.AccountId).Select(bb => bb.AmountExcTax).DefaultIfEmpty().Sum(),
                       }).GroupBy(x => x.ExpenseId)
       .Select(g => g.FirstOrDefault()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseSummaryByCustomer(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det1;
            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det1 = (from a in oConnectionContext.DbClsExpense
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = a.GrandTotal,
                            AmountExcTax = a.Subtotal,
                            TaxAmount = a.TaxAmount,
                            CustomerId = a.CustomerId
                        }).ToList();
            }
            else
            {
                det1 = (from a in oConnectionContext.DbClsExpense
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                  && a.BranchId == obj.BranchId
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = a.GrandTotal,
                            AmountExcTax = a.Subtotal,
                            TaxAmount = a.TaxAmount,
                            CustomerId = a.CustomerId
                        }).ToList();
            }

            det = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.UserType.ToLower() == "customer").ToList().Select(a =>
                    new ClsExpenseVm
                    {
                        CustomerId = a.UserId,
                        CustomerName = a.Name,
                        ExpenseCount = det1.Where(b => b.CustomerId == a.UserId).Select(b => b.Amount).Count(),
                        Amount = det1.Where(b => b.CustomerId == a.UserId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                        AmountExcTax = det1.Where(b => b.CustomerId == a.UserId).Select(b => b.AmountExcTax).DefaultIfEmpty().Sum(),
                    }).OrderBy(a => a.AccountId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.CustomerId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseDetailsByCustomer(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && a.CustomerId == obj.CustomerId).ToList().Select(a => new ClsExpenseVm
                 {
                     ExpenseId = a.ExpenseId,
                     Date = a.Date,
                     ReferenceNo = a.ReferenceNo,
                     Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                             oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                     Amount = a.Subtotal,
                     AmountExcTax = a.GrandTotal
                 }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.BranchId == obj.BranchId
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && a.CustomerId == obj.CustomerId).ToList().Select(a => new ClsExpenseVm
                 {
                     ExpenseId = a.ExpenseId,
                     Date = a.Date,
                     ReferenceNo = a.ReferenceNo,
                     Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                             oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                     Amount = a.Subtotal,
                     AmountExcTax = a.GrandTotal
                 }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseSummaryBySupplier(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det1;
            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det1 = (from a in oConnectionContext.DbClsExpense
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = a.GrandTotal,
                            AmountExcTax = a.Subtotal,
                            TaxAmount = a.TaxAmount,
                            SupplierId = a.SupplierId
                        }).ToList();
            }
            else
            {
                det1 = (from a in oConnectionContext.DbClsExpense
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.BranchId == obj.BranchId
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = a.GrandTotal,
                            AmountExcTax = a.Subtotal,
                            TaxAmount = a.TaxAmount,
                            SupplierId = a.SupplierId
                        }).ToList();
            }

            det = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.UserType.ToLower() == "supplier").ToList().Select(a =>
                    new ClsExpenseVm
                    {
                        SupplierId = a.UserId,
                        SupplierName = a.Name,
                        ExpenseCount = det1.Where(b => b.SupplierId == a.UserId).Select(b => b.Amount).Count(),
                        Amount = det1.Where(b => b.SupplierId == a.UserId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                        AmountExcTax = det1.Where(b => b.SupplierId == a.UserId).Select(b => b.AmountExcTax).DefaultIfEmpty().Sum(),
                    }).OrderBy(a => a.AccountId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.SupplierId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseDetailsBySupplier(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && a.SupplierId == obj.SupplierId).ToList().Select(a => new ClsExpenseVm
                 {
                     ExpenseId = a.ExpenseId,
                     Date = a.Date,
                     ReferenceNo = a.ReferenceNo,
                     Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                             oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                     Amount = a.Subtotal,
                     AmountExcTax = a.GrandTotal
                 }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.BranchId == obj.BranchId
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && a.SupplierId == obj.SupplierId).ToList().Select(a => new ClsExpenseVm
                 {
                     ExpenseId = a.ExpenseId,
                     Date = a.Date,
                     ReferenceNo = a.ReferenceNo,
                     Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                             oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                     Amount = a.Subtotal,
                     AmountExcTax = a.GrandTotal
                 }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseSummaryByUser(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det1;
            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det1 = (from a in oConnectionContext.DbClsExpense
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = a.GrandTotal,
                            AmountExcTax = a.Subtotal,
                            TaxAmount = a.TaxAmount,
                            UserId = a.UserId
                        }).ToList();
            }
            else
            {
                det1 = (from a in oConnectionContext.DbClsExpense
                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                  && a.BranchId == obj.BranchId
                  && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                        select new ClsExpenseVm
                        {
                            Amount = a.GrandTotal,
                            AmountExcTax = a.Subtotal,
                            TaxAmount = a.TaxAmount,
                            UserId = a.UserId
                        }).ToList();
            }

            det = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.UserType.ToLower() == "user").ToList().Select(a =>
                    new ClsExpenseVm
                    {
                        UserId = a.UserId,
                        UserName = a.Name,
                        ExpenseCount = det1.Where(b => b.UserId == a.UserId).Select(b => b.Amount).Count(),
                        Amount = det1.Where(b => b.UserId == a.UserId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                        AmountExcTax = det1.Where(b => b.UserId == a.UserId).Select(b => b.AmountExcTax).DefaultIfEmpty().Sum(),
                    }).OrderBy(a => a.AccountId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ExpenseDetailsByUser(ClsExpenseVm obj)
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

            List<ClsExpenseVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && a.UserId == obj.UserId).ToList().Select(a => new ClsExpenseVm
                 {
                     ExpenseId = a.ExpenseId,
                     Date = a.Date,
                     ReferenceNo = a.ReferenceNo,
                     Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                             oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                     Amount = a.Subtotal,
                     AmountExcTax = a.GrandTotal
                 }).ToList();
            }
            else
            {
                det = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.BranchId == obj.BranchId
                 && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                 DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                 && a.UserId == obj.UserId).ToList().Select(a => new ClsExpenseVm
                 {
                     ExpenseId = a.ExpenseId,
                     Date = a.Date,
                     ReferenceNo = a.ReferenceNo,
                     Categorys = string.Join(",", oConnectionContext.DbClsExpensePayment.Where(b => b.ExpenseId == a.ExpenseId).Select(b =>
                             oConnectionContext.DbClsAccount.Where(bb => bb.AccountId == b.AccountId).Select(bb => bb.AccountName).FirstOrDefault()).Distinct().ToList()),
                     Amount = a.Subtotal,
                     AmountExcTax = a.GrandTotal
                 }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Expenses = det.Where(a => a.AmountExcTax != 0).OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Where(a => a.AmountExcTax != 0).Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }


    }
}
