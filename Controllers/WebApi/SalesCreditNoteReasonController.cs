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
    public class SalesCreditNoteReasonController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllSalesCreditNoteReasons(ClsSalesCreditNoteReasonVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                SalesCreditNoteReasonId = a.SalesCreditNoteReasonId,
                SalesCreditNoteReason = a.SalesCreditNoteReason,
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
                det = det.Where(a => a.SalesCreditNoteReason.ToLower().Contains(obj.Search.ToLower()) ||
                (a.Description != null && a.Description.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesCreditNoteReasons = det.OrderByDescending(a => a.SalesCreditNoteReasonId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesCreditNoteReason(ClsSalesCreditNoteReason obj)
        {
            var det = oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.SalesCreditNoteReasonId == obj.SalesCreditNoteReasonId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                SalesCreditNoteReasonId = a.SalesCreditNoteReasonId,
                SalesCreditNoteReason = a.SalesCreditNoteReason,
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
                    SalesCreditNoteReason = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertSalesCreditNoteReason(ClsSalesCreditNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SalesCreditNoteReason == null || obj.SalesCreditNoteReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesCreditNoteReason" });
                    isError = true;
                }

                if (obj.SalesCreditNoteReason != null && obj.SalesCreditNoteReason != "")
                {
                    if (oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.SalesCreditNoteReason.ToLower() == obj.SalesCreditNoteReason.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Credit Note Reason exists", Id = "divSalesCreditNoteReason" });
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

                ClsSalesCreditNoteReason oSalesCreditNoteReason = new ClsSalesCreditNoteReason()
                {
                    SalesCreditNoteReason = obj.SalesCreditNoteReason,
                    Description = obj.Description,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsSalesCreditNoteReason.Add(oSalesCreditNoteReason);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Credit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Credit Note Reason \"" + obj.SalesCreditNoteReason + "\" created",
                    Id = oSalesCreditNoteReason.SalesCreditNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Credit Note Reason created successfully",
                    Data = new
                    {
                        SalesCreditNoteReason = oSalesCreditNoteReason
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateSalesCreditNoteReason(ClsSalesCreditNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SalesCreditNoteReason == null || obj.SalesCreditNoteReason == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSalesCreditNoteReason" });
                    isError = true;
                }

                if (obj.SalesCreditNoteReason != null && obj.SalesCreditNoteReason != "")
                {
                    if (oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.SalesCreditNoteReason.ToLower() == obj.SalesCreditNoteReason.ToLower() && a.SalesCreditNoteReasonId != obj.SalesCreditNoteReasonId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Sales Credit Note Reason Name exists", Id = "divSalesCreditNoteReason" });
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

                ClsSalesCreditNoteReason oSalesCreditNoteReason = new ClsSalesCreditNoteReason()
                {
                    SalesCreditNoteReasonId = obj.SalesCreditNoteReasonId,
                    SalesCreditNoteReason = obj.SalesCreditNoteReason,
                    Description = obj.Description,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesCreditNoteReason.Attach(oSalesCreditNoteReason);
                oConnectionContext.Entry(oSalesCreditNoteReason).Property(x => x.SalesCreditNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oSalesCreditNoteReason).Property(x => x.SalesCreditNoteReason).IsModified = true;
                oConnectionContext.Entry(oSalesCreditNoteReason).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oSalesCreditNoteReason).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oSalesCreditNoteReason).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Credit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Credit Note Reason \"" + obj.SalesCreditNoteReason + "\" updated",
                    Id = oSalesCreditNoteReason.SalesCreditNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Credit Note Reason updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesCreditNoteReasonActiveInactive(ClsSalesCreditNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsSalesCreditNoteReason oClsRole = new ClsSalesCreditNoteReason()
                {
                    SalesCreditNoteReasonId = obj.SalesCreditNoteReasonId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesCreditNoteReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SalesCreditNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Credit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Credit Note Reason \"" + oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.SalesCreditNoteReasonId == obj.SalesCreditNoteReasonId).Select(a => a.SalesCreditNoteReason).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.SalesCreditNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Credit Note Reason " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesCreditNoteReasonDelete(ClsSalesCreditNoteReasonVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.SalesCreditNoteReasonId == obj.SalesCreditNoteReasonId && a.IsDeleted == false && a.IsCancelled == false).Count();
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
                ClsSalesCreditNoteReason oClsRole = new ClsSalesCreditNoteReason()
                {
                    SalesCreditNoteReasonId = obj.SalesCreditNoteReasonId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsSalesCreditNoteReason.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.SalesCreditNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Sales Credit Note Reason",
                    CompanyId = obj.CompanyId,
                    Description = "Sales Credit Note Reason \"" + oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.SalesCreditNoteReasonId == obj.SalesCreditNoteReasonId).Select(a => a.SalesCreditNoteReason).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.SalesCreditNoteReasonId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sales Credit Note Reason deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveSalesCreditNoteReasons(ClsSalesCreditNoteReasonVm obj)
        {
            var det = oConnectionContext.DbClsSalesCreditNoteReason.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                SalesCreditNoteReasonId = a.SalesCreditNoteReasonId,
                SalesCreditNoteReason = a.SalesCreditNoteReason,
                a.Description
            }).OrderBy(a => a.SalesCreditNoteReason).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesCreditNoteReasons = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
