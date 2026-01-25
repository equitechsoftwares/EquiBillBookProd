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
    public class SalesDebitNoteReasonController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSalesDebitNoteReasons(ClsSalesDebitNoteReasonVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                SalesDebitNoteReasonId = a.SalesDebitNoteReasonId,
                SalesDebitNoteReason = a.SalesDebitNoteReason,
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
                det = det.Where(a => a.SalesDebitNoteReason.ToLower().Contains(obj.Search.ToLower()) ||
                (a.Description != null && a.Description.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDebitNoteReasons = det.OrderByDescending(a => a.SalesDebitNoteReasonId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDebitNoteReason(ClsSalesDebitNoteReason obj)
        {
            var det = oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.SalesDebitNoteReasonId == obj.SalesDebitNoteReasonId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SalesDebitNoteReasonId = a.SalesDebitNoteReasonId,
                SalesDebitNoteReason = a.SalesDebitNoteReason,
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
                    SalesDebitNoteReason = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSalesDebitNoteReason(ClsSalesDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SalesDebitNoteReason == null || obj.SalesDebitNoteReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesDebitNoteReason" });
                    isError = true;
                }

                if (obj.SalesDebitNoteReason != null && obj.SalesDebitNoteReason != "")
                {
                    if (oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.SalesDebitNoteReason.ToLower() == obj.SalesDebitNoteReason.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Debit Note Reason exists", Id = "divSalesDebitNoteReason" });
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

                ClsSalesDebitNoteReason oSalesDebitNoteReason = new ClsSalesDebitNoteReason()
                {
                    SalesDebitNoteReason = obj.SalesDebitNoteReason,
                    Description = obj.Description,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsSalesDebitNoteReason.Add(oSalesDebitNoteReason);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Debit Note Reason \"" + obj.SalesDebitNoteReason + "\" created",
                    Id = oSalesDebitNoteReason.SalesDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Debit Note Reason created successfully",
                    Data = new
                    {
                        SalesDebitNoteReason = oSalesDebitNoteReason
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesDebitNoteReason(ClsSalesDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SalesDebitNoteReason == null || obj.SalesDebitNoteReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesDebitNoteReason" });
                    isError = true;
                }

                if (obj.SalesDebitNoteReason != null && obj.SalesDebitNoteReason != "")
                {
                    if (oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.SalesDebitNoteReason.ToLower() == obj.SalesDebitNoteReason.ToLower() && a.SalesDebitNoteReasonId != obj.SalesDebitNoteReasonId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Debit Note Reason exists", Id = "divSalesDebitNoteReason" });
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

                ClsSalesDebitNoteReason oSalesDebitNoteReason = new ClsSalesDebitNoteReason()
                {
                    SalesDebitNoteReasonId = obj.SalesDebitNoteReasonId,
                    SalesDebitNoteReason = obj.SalesDebitNoteReason,
                    Description = obj.Description,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesDebitNoteReason.Attach(oSalesDebitNoteReason);
                oConnectionContext.Entry(oSalesDebitNoteReason).Property(x => x.SalesDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oSalesDebitNoteReason).Property(x => x.SalesDebitNoteReason).IsModified = true;
                oConnectionContext.Entry(oSalesDebitNoteReason).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oSalesDebitNoteReason).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSalesDebitNoteReason).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Debit Note Reason \"" + obj.SalesDebitNoteReason + "\" updated",
                    Id = oSalesDebitNoteReason.SalesDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Debit Note Reason updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDebitNoteReasonActiveInactive(ClsSalesDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSalesDebitNoteReason oClsRole = new ClsSalesDebitNoteReason()
                {
                    SalesDebitNoteReasonId = obj.SalesDebitNoteReasonId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesDebitNoteReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SalesDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Debit Note Reason \"" + oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.SalesDebitNoteReasonId == obj.SalesDebitNoteReasonId).Select(a => a.SalesDebitNoteReason).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.SalesDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Debit Note Reason " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDebitNoteReasonDelete(ClsSalesDebitNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.SalesDebitNoteReasonId == obj.SalesDebitNoteReasonId && a.IsDeleted == false && a.IsCancelled == false).Count();
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
                ClsSalesDebitNoteReason oClsRole = new ClsSalesDebitNoteReason()
                {
                    SalesDebitNoteReasonId = obj.SalesDebitNoteReasonId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesDebitNoteReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SalesDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Debit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Debit Note Reason \"" + oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.SalesDebitNoteReasonId == obj.SalesDebitNoteReasonId).Select(a => a.SalesDebitNoteReason).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.SalesDebitNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Debit Note Reason deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSalesDebitNoteReasons(ClsSalesDebitNoteReasonVm obj)
        {
            var det = oConnectionContext.DbClsSalesDebitNoteReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                SalesDebitNoteReasonId = a.SalesDebitNoteReasonId,
                SalesDebitNoteReason = a.SalesDebitNoteReason,
                a.Description
            }).OrderBy(a => a.SalesDebitNoteReason).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDebitNoteReasons = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
