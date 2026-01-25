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
    public class PaymentTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllPaymentTypes(ClsPaymentTypeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);


            var det = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId
            && a.IsAdvance == false && a.IsDeleted == false).Select(a => new
            {
                a.IsOnlyView,
                PaymentTypeId = a.PaymentTypeId,
                PaymentType = a.PaymentType,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                //a.IsPosShown,
                a.ShortCutKey,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.PaymentType.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentTypes = det.OrderByDescending(a => a.PaymentTypeId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentType(ClsPaymentType obj)
        {
            var det = oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.PaymentTypeId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                PaymentTypeId = a.PaymentTypeId,
                PaymentType = a.PaymentType,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                //a.IsPosShown,
                a.ShortCutKey
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentType = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPaymentType(ClsPaymentTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PaymentType == null || obj.PaymentType == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                    isError = true;
                }

                if (oConnectionContext.DbClsPaymentType.Where(a => a.PaymentType.ToLower() == obj.PaymentType.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Payment Method Name exists", Id = "divPaymentType" });
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

                ClsPaymentType oPaymentType = new ClsPaymentType()
                {
                    PaymentType = obj.PaymentType,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    //IsPosShown = obj.IsPosShown,
                    ShortCutKey = obj.ShortCutKey,
                    IsOnlyView = false
                };
                oConnectionContext.DbClsPaymentType.Add(oPaymentType);
                oConnectionContext.SaveChanges();

                if (obj.BranchId != 0)
                {
                    ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap = new ClsBranchPaymentTypeMap()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        PaymentTypeId = oPaymentType.PaymentTypeId,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedBy = obj.AddedBy,
                        BranchId = obj.BranchId,
                        AccountId = 0,
                        IsPosShown = false
                    };
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap);
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Methods",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Method \"" + obj.PaymentType + "\" created",
                    Id = oPaymentType.PaymentTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);


                data = new
                {
                    Status = 1,
                    Message = "Payment Method created successfully",
                    Data = new
                    {
                        PaymentType = oPaymentType
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePaymentType(ClsPaymentTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PaymentType == null || obj.PaymentType == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Fields marked with * is mandatory",
                    //    Data = new
                    //    {
                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (oConnectionContext.DbClsPaymentType.Where(a => a.PaymentType.ToLower() == obj.PaymentType.ToLower() && a.PaymentTypeId != obj.PaymentTypeId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Payment Method Name exists", Id = "divPaymentType" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Duplicate Payment Method name exists",
                    //    Data = new
                    //    {
                    //    }
                    //}; return await Task.FromResult(Ok(data));
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

                ClsPaymentType oPaymentType = new ClsPaymentType()
                {
                    PaymentTypeId = obj.PaymentTypeId,
                    PaymentType = obj.PaymentType,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    //IsPosShown = obj.IsPosShown,
                    ShortCutKey = obj.ShortCutKey
                };
                oConnectionContext.DbClsPaymentType.Attach(oPaymentType);
                oConnectionContext.Entry(oPaymentType).Property(x => x.PaymentTypeId).IsModified = true;
                oConnectionContext.Entry(oPaymentType).Property(x => x.PaymentType).IsModified = true;
                oConnectionContext.Entry(oPaymentType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oPaymentType).Property(x => x.ModifiedOn).IsModified = true;
                //oConnectionContext.Entry(oPaymentType).Property(x => x.IsPosShown).IsModified = true;
                oConnectionContext.Entry(oPaymentType).Property(x => x.ShortCutKey).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Methods",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Method \"" + obj.PaymentType+"\" updated",
                    Id = oPaymentType.PaymentTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

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

        public async Task<IHttpActionResult> PaymentTypeActiveInactive(ClsPaymentTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPaymentType oClsRole = new ClsPaymentType()
                {
                    PaymentTypeId = obj.PaymentTypeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPaymentType.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PaymentTypeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Methods",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Method \"" + oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault()+(obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.PaymentTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Method " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentTypeDelete(ClsPaymentTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int CustomerPaymentCount = oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.PaymentTypeId == obj.PaymentTypeId).Count();

                //int CustomerRefundCount = oConnectionContext.DbClsCustomerRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.PaymentTypeId == obj.PaymentTypeId).Count();

                int SupplierPaymentCount = oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.PaymentTypeId == obj.PaymentTypeId).Count();

                //int SupplierRefundCount = oConnectionContext.DbClsSupplierRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.PaymentTypeId == obj.PaymentTypeId).Count();

                if (CustomerPaymentCount > 0
                    //|| CustomerRefundCount > 0
                    || SupplierPaymentCount > 0
                    //|| SupplierRefundCount > 0
                    )
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
                ClsPaymentType oClsRole = new ClsPaymentType()
                {
                    PaymentTypeId = obj.PaymentTypeId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPaymentType.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PaymentTypeId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Methods",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Method \"" + oConnectionContext.DbClsPaymentType.Where(a => a.PaymentTypeId == obj.PaymentTypeId).Select(a => a.PaymentType).FirstOrDefault()+"\" deleted",
                    Id = oClsRole.PaymentTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Method deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActivePaymentTypes(ClsPaymentTypeVm obj)
        {
            List<ClsPaymentTypeVm> det;
            if (obj.PaymentType == "all")
            {
                det = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.IsActive == true && a.IsAdvance == false).Select(a => new ClsPaymentTypeVm
                {
                    IsAdvance = a.IsAdvance,
                    PaymentTypeId = a.PaymentTypeId,
                    PaymentType = a.PaymentType,
                    //IsPosShown=a.IsPosShown,
                    ShortCutKey = a.ShortCutKey,
                    IsDefault = false
                }).ToList();
            }
            else if (obj.BranchId == 0)
            {
                det = (from c in oConnectionContext.DbClsBranchPaymentTypeMap
                       join b in oConnectionContext.DbClsPaymentType
on c.PaymentTypeId equals b.PaymentTypeId
                       where
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                       && c.IsDeleted == false && c.IsActive == true &&
                        b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                        && b.IsAdvance == false
                       select new ClsPaymentTypeVm
                       {
                           IsAdvance = b.IsAdvance,
                           PaymentTypeId = b.PaymentTypeId,
                           PaymentType = b.PaymentType,
                           //IsPosShown = c.IsPosShown,
                           ShortCutKey = b.ShortCutKey,
                           IsDefault = false
                       }).Distinct().ToList();
            }
            else
            {
                det = (from c in oConnectionContext.DbClsBranchPaymentTypeMap
                       join b in oConnectionContext.DbClsPaymentType
on c.PaymentTypeId equals b.PaymentTypeId
                       where
                       c.BranchId == obj.BranchId
                       && c.IsDeleted == false && c.IsActive == true &&
                        b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                        && b.IsAdvance == false
                       select new ClsPaymentTypeVm
                       {
                           IsAdvance = b.IsAdvance,
                           PaymentTypeId = b.PaymentTypeId,
                           PaymentType = b.PaymentType,
                           ShortCutKey = b.ShortCutKey,
                           IsPosShown = c.IsPosShown,
                           IsDefault = c.IsDefault
                       }).ToList();
            }

            if (obj.IsAdvance == true)
            {
                det = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId
                && a.IsAdvance == true).Select(a => new ClsPaymentTypeVm
                {
                    IsAdvance = a.IsAdvance,
                    PaymentTypeId = a.PaymentTypeId,
                    PaymentType = a.PaymentType,
                    //IsPosShown = a.IsPosShown,
                    ShortCutKey = a.ShortCutKey,
                    IsDefault = false
                }).ToList().Union(det).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentTypes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
