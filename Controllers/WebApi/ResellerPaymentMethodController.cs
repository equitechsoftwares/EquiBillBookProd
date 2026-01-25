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
    public class ResellerPaymentMethodController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> ResellerPaymentMethod(ClsResellerPaymentMethodVm obj)
        {
            obj.ResellerPaymentMethodId = oConnectionContext.DbClsResellerPaymentMethod.Where(a => a.CompanyId == obj.UserId).Select(a => a.ResellerPaymentMethodId).FirstOrDefault();
            var det = oConnectionContext.DbClsResellerPaymentMethod.Where(a => a.ResellerPaymentMethodId == obj.ResellerPaymentMethodId).Select(a => new
            {
                a.ResellerPaymentMethodId,
                a.PaymentMethod,
                a.AccountName,
                a.AccountNumber,
                a.Notes,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.BankName,
                a.BranchName,
                a.BranchCode,
                a.IFSC,
                a.SwiftBIC,
                a.UpiID,
                a.Gpay,
                a.PhonePe,
                a.Paytm,
                a.Paypal,
                a.BankAddress,
                a.AccountHolder
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ResellerPaymentMethod = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateResellerPaymentMethod(ClsResellerPaymentMethodVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if(obj.PaymentMethod == 1) //Bank Account
                {
                    if (obj.BankName == null || obj.BankName == "")
                    {
                        errors.Add(new ClsError { Message = "Bank Name is required", Id = "divBankName" });
                        isError = true;
                    }

                    if (obj.AccountNumber == null || obj.AccountNumber == "")
                    {
                        errors.Add(new ClsError { Message = "Account Number is required", Id = "divAccountNumber" });
                        isError = true;
                    }

                    //if (obj.AccountNumber != "" && obj.AccountNumber != null)
                    //{
                    //    if (oConnectionContext.DbClsAccount.Where(a => a.AccountNumber.ToLower() == obj.AccountNumber.ToLower() &&
                    // a.AccountId != obj.AccountId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate Account Number exists", Id = "divAccountNumber" });
                    //        isError = true;
                    //    }
                    //}

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
                }
                else if(obj.PaymentMethod ==2) //UPI Id
                {
                    if (obj.UpiID == null || obj.UpiID == "")
                    {
                        errors.Add(new ClsError { Message = "UPI ID is required", Id = "divUpiID" });
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
                }
                else if (obj.PaymentMethod == 3) //Gpay
                {
                    if (obj.Gpay == null || obj.Gpay == "")
                    {
                        errors.Add(new ClsError { Message = "GPay is required", Id = "divGpay" });
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
                }
                else if (obj.PaymentMethod == 4) //PhonePe
                {
                    if (obj.PhonePe == null || obj.PhonePe== "")
                    {
                        errors.Add(new ClsError { Message = "PhonePe is required", Id = "divPhonePe" });
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
                }
                else if (obj.PaymentMethod == 5) //Paytm
                {
                    if (obj.Paytm == null || obj.Paytm == "")
                    {
                        errors.Add(new ClsError { Message = "Paytm is required", Id = "divPaytm" });
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
                }
                else if (obj.PaymentMethod == 6) //Paypal
                {
                    if (obj.Paypal == null || obj.Paypal == "")
                    {
                        errors.Add(new ClsError { Message = "Paypal is required", Id = "divPaypal" });
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
                }
                
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                obj.ResellerPaymentMethodId = oConnectionContext.DbClsResellerPaymentMethod.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.ResellerPaymentMethodId).FirstOrDefault();

                ClsResellerPaymentMethod oClsResellerPaymentMethod= new ClsResellerPaymentMethod()
                {
                    ResellerPaymentMethodId = obj.ResellerPaymentMethodId,
                    PaymentMethod = obj.PaymentMethod,
                    AccountName = obj.AccountName,
                    AccountNumber = obj.AccountNumber,
                    Notes = obj.Notes,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    BankName = obj.BankName,
                    BranchCode = obj.BranchCode,
                    BranchName = obj.BranchName,
                    IFSC = obj.IFSC,
                    SwiftBIC = obj.SwiftBIC,
                    UpiID = obj.UpiID,
                    Gpay = obj.Gpay,
                    PhonePe = obj.PhonePe,
                    Paytm = obj.Paytm,
                    Paypal = obj.Paypal,
                    AccountHolder = obj.AccountHolder,
                    BankAddress = obj.BankAddress,
                };
                oConnectionContext.DbClsResellerPaymentMethod.Attach(oClsResellerPaymentMethod);
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.PaymentMethod).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.AccountName).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.AccountNumber).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.ModifiedOn).IsModified = true;                
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.BankName).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.BranchName).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.BranchCode).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.IFSC).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.SwiftBIC).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.UpiID).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.Gpay).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.PhonePe).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.Paytm).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.Paypal).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.AccountHolder).IsModified = true;
                oConnectionContext.Entry(oClsResellerPaymentMethod).Property(x => x.BankAddress).IsModified = true;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    Category = "Account",
                //    CompanyId = obj.CompanyId,
                //    Description = "modified " + obj.AccountNumber,
                //    Id = oAccount.AccountId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Update"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Method updated successfully",
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
