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
    public class PurchaseDebitNoteReasonController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllPurchaseDebitNoteReasons(ClsPurchaseDebitNoteReasonVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                PurchaseDebitNoteReasonId = a.PurchaseDebitNoteReasonId,
                PurchaseDebitNoteReason = a.PurchaseDebitNoteReason,
                a.Description,
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
                det = det.Where(a =>a.PurchaseDebitNoteReason.ToLower().Contains(obj.Search.ToLower()) ||
                (a.Description != null && a.Description.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDebitNoteReasons = det.OrderByDescending(a => a.PurchaseDebitNoteReasonId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDebitNoteReason(ClsPurchaseDebitNoteReason obj)
        {
            var det = oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.PurchaseDebitNoteReasonId == obj.PurchaseDebitNoteReasonId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                PurchaseDebitNoteReasonId = a.PurchaseDebitNoteReasonId,
                PurchaseDebitNoteReason = a.PurchaseDebitNoteReason,
                a.Description,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDebitNoteReason = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPurchaseDebitNoteReason(ClsPurchaseDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PurchaseDebitNoteReason == null || obj.PurchaseDebitNoteReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseDebitNoteReason" });
                    isError = true;
                }

                if (obj.PurchaseDebitNoteReason != null && obj.PurchaseDebitNoteReason != "")
                {
                    if (oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.PurchaseDebitNoteReason.ToLower() == obj.PurchaseDebitNoteReason.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Purchase Debit Note Reason Name exists", Id = "divPurchaseDebitNoteReason" });
                        isError = true;
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

                ClsPurchaseDebitNoteReason oPurchaseDebitNoteReason = new ClsPurchaseDebitNoteReason()
                {
                    PurchaseDebitNoteReason = obj.PurchaseDebitNoteReason,
                    Description = obj.Description,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsPurchaseDebitNoteReason.Add(oPurchaseDebitNoteReason);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Debit Note Reason \"" + obj.PurchaseDebitNoteReason + "\" created",
                    Id = oPurchaseDebitNoteReason.PurchaseDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Debit Note Reason created successfully",
                    Data = new
                    {
                        PurchaseDebitNoteReason = oPurchaseDebitNoteReason
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchaseDebitNoteReason(ClsPurchaseDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.PurchaseDebitNoteReason == null || obj.PurchaseDebitNoteReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseDebitNoteReason" });
                    isError = true;
                }

                if (obj.PurchaseDebitNoteReason != null && obj.PurchaseDebitNoteReason != "")
                {
                    if (oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.PurchaseDebitNoteReason.ToLower() == obj.PurchaseDebitNoteReason.ToLower() && a.PurchaseDebitNoteReasonId != obj.PurchaseDebitNoteReasonId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Purchase Debit Note Reason Name exists", Id = "divPurchaseDebitNoteReason" });
                        isError = true;
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

                ClsPurchaseDebitNoteReason oPurchaseDebitNoteReason = new ClsPurchaseDebitNoteReason()
                {
                    PurchaseDebitNoteReasonId = obj.PurchaseDebitNoteReasonId,
                    PurchaseDebitNoteReason = obj.PurchaseDebitNoteReason,
                    Description = obj.Description,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchaseDebitNoteReason.Attach(oPurchaseDebitNoteReason);
                oConnectionContext.Entry(oPurchaseDebitNoteReason).Property(x => x.PurchaseDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oPurchaseDebitNoteReason).Property(x => x.PurchaseDebitNoteReason).IsModified = true;
                oConnectionContext.Entry(oPurchaseDebitNoteReason).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oPurchaseDebitNoteReason).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oPurchaseDebitNoteReason).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Debit Note Reason \"" + obj.PurchaseDebitNoteReason + "\" updated",
                    Id = oPurchaseDebitNoteReason.PurchaseDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Debit Note Reason updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDebitNoteReasonActiveInactive(ClsPurchaseDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPurchaseDebitNoteReason oClsRole = new ClsPurchaseDebitNoteReason()
                {
                    PurchaseDebitNoteReasonId = obj.PurchaseDebitNoteReasonId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchaseDebitNoteReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PurchaseDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Debit Note Reason \"" + oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.PurchaseDebitNoteReasonId == obj.PurchaseDebitNoteReasonId).Select(a => a.PurchaseDebitNoteReason).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.PurchaseDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Debit Note Reason " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDebitNoteReasonDelete(ClsPurchaseDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.PurchaseDebitNoteReasonId == obj.PurchaseDebitNoteReasonId && a.IsDeleted == false && a.IsCancelled == false).Count();
                //if (ItemCount > 0)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "Cannot delete as it is already in use",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPurchaseDebitNoteReason oClsRole = new ClsPurchaseDebitNoteReason()
                {
                    PurchaseDebitNoteReasonId = obj.PurchaseDebitNoteReasonId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchaseDebitNoteReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PurchaseDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Debit Note Reason \"" + oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.PurchaseDebitNoteReasonId == obj.PurchaseDebitNoteReasonId).Select(a => a.PurchaseDebitNoteReason).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.PurchaseDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Debit Note Reason deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActivePurchaseDebitNoteReasons(ClsPurchaseDebitNoteReasonVm obj)
        {
            var det = oConnectionContext.DbClsPurchaseDebitNoteReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                PurchaseDebitNoteReasonId = a.PurchaseDebitNoteReasonId,
                PurchaseDebitNoteReason = a.PurchaseDebitNoteReason,
                a.Description
            }).OrderBy(a => a.PurchaseDebitNoteReason).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDebitNoteReasons = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
