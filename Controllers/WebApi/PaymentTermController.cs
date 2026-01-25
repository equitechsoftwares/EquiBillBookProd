using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class PaymentTermController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllPaymentTerms(ClsPaymentTermVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);


            var det = oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsDueUponReceipt == false).Select(a => new
            {
                a.IsDueUponReceipt,
                PaymentTermId = a.PaymentTermId,
                PaymentTerm = a.PaymentTerm,
                Days = a.Days,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.PaymentTerm.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentTerms = det.OrderByDescending(a => a.PaymentTermId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentTerm(ClsPaymentTerm obj)
        {
            var det = oConnectionContext.DbClsPaymentTerm.Where(a => a.PaymentTermId == obj.PaymentTermId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                PaymentTermId = a.PaymentTermId,
                PaymentTerm = a.PaymentTerm,
                Days = a.Days,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.IsDueUponReceipt
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentTerm = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPaymentTerm(ClsPaymentTermVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PaymentTerm == null || obj.PaymentTerm == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentTerm" });
                    isError = true;
                }

                if (obj.Days == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDays" });
                    isError = true;
                }

                if (oConnectionContext.DbClsPaymentTerm.Where(a => a.PaymentTerm.ToLower() == obj.PaymentTerm.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Payment Term Name exists", Id = "divPaymentTerm" });
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

                ClsPaymentTerm oPaymentTerm = new ClsPaymentTerm()
                {
                    PaymentTerm = obj.PaymentTerm,
                    Days = obj.Days,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsDueUponReceipt = false
                };
                oConnectionContext.DbClsPaymentTerm.Add(oPaymentTerm);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Terms",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Term \"" + obj.PaymentTerm + "\" created",
                    Id = oPaymentTerm.PaymentTermId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Term created successfully",
                    Data = new
                    {
                        PaymentTerm = oPaymentTerm
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePaymentTerm(ClsPaymentTermVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PaymentTerm == null || obj.PaymentTerm == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentTerm" });
                    isError = true;
                }

                //if (obj.Days == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divDays" });
                //    isError = true;
                //}

                if (oConnectionContext.DbClsPaymentTerm.Where(a => a.PaymentTerm.ToLower() == obj.PaymentTerm.ToLower() && a.PaymentTermId != obj.PaymentTermId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Payment Term Name exists", Id = "divPaymentTerm" });
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

                ClsPaymentTerm oPaymentTerm = new ClsPaymentTerm()
                {
                    PaymentTermId = obj.PaymentTermId,
                    PaymentTerm = obj.PaymentTerm,
                    //Days = obj.Days,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPaymentTerm.Attach(oPaymentTerm);
                oConnectionContext.Entry(oPaymentTerm).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oPaymentTerm).Property(x => x.PaymentTerm).IsModified = true;
                //oConnectionContext.Entry(oPaymentTerm).Property(x => x.Days).IsModified = true;
                oConnectionContext.Entry(oPaymentTerm).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oPaymentTerm).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Terms",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Term \"" + obj.PaymentTerm + "\" updated",
                    Id = oPaymentTerm.PaymentTermId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Term updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentTermActiveInactive(ClsPaymentTermVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPaymentTerm oClsRole = new ClsPaymentTerm()
                {
                    PaymentTermId = obj.PaymentTermId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPaymentTerm.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Terms",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Term \"" + oConnectionContext.DbClsPaymentTerm.Where(a => a.PaymentTermId == obj.PaymentTermId).Select(a => a.PaymentTerm).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.PaymentTermId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Term " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentTermDelete(ClsPaymentTermVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int UserCount = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.PaymentTermId == obj.PaymentTermId).Count();

                int SalesCount = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.PaymentTermId == obj.PaymentTermId).Count();

                int PurchaseCount = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.PaymentTermId == obj.PaymentTermId).Count();

                if (UserCount > 0 || SalesCount > 0 || PurchaseCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPaymentTerm oClsRole = new ClsPaymentTerm()
                {
                    PaymentTermId = obj.PaymentTermId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPaymentTerm.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Terms",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Term \"" + oConnectionContext.DbClsPaymentTerm.Where(a => a.PaymentTermId == obj.PaymentTermId).Select(a => a.PaymentTerm).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.PaymentTermId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Term deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActivePaymentTerms(ClsPaymentTermVm obj)
        {
            var det = oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.IsActive == true).Select(a => new ClsPaymentTermVm
                 {
                     PaymentTermId = a.PaymentTermId,
                     PaymentTerm = a.PaymentTerm,
                     IsDueUponReceipt = a.IsDueUponReceipt
                 }).OrderBy(a=>a.PaymentTerm).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentTerms = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CalculateDueDate(ClsPaymentTermVm obj)
        {
            int Days = oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.IsActive == true && a.PaymentTermId == obj.PaymentTermId).Select(a => a.Days).FirstOrDefault();

            DateTime DueDate = obj.InvoiceDate.AddDays(Days);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentTerm = new
                    {
                        DueDate = DueDate
                    },
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
