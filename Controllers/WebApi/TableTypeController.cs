using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class TableTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        [HttpPost]
        public async Task<IHttpActionResult> GetTableTypes(ClsTableTypeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsTableType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                TableTypeId = a.TableTypeId,
                TableTypeName = a.TableTypeName,
                Description = a.Description,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => a.TableTypeName.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TableTypes = det.OrderByDescending(a => a.TableTypeId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TableType(ClsTableTypeVm obj)
        {
            var det = oConnectionContext.DbClsTableType.Where(a => a.TableTypeId == obj.TableTypeId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                TableTypeId = a.TableTypeId,
                TableTypeName = a.TableTypeName,
                Description = a.Description,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TableType = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTableType(ClsTableTypeVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (string.IsNullOrEmpty(obj.TableTypeName))
                {
                    errors.Add(new ClsError { Message = "Table Type Name is required", Id = "divTableTypeName" });
                    isError = true;
                }

                if (!string.IsNullOrEmpty(obj.TableTypeName))
                {
                    if (oConnectionContext.DbClsTableType.Where(a => a.TableTypeName.ToLower() == obj.TableTypeName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.TableTypeId != obj.TableTypeId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Table Type Name exists", Id = "divTableTypeName" });
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
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsTableType oTableType = new ClsTableType()
                {
                    TableTypeId = obj.TableTypeId,
                    TableTypeName = obj.TableTypeName,
                    Description = obj.Description,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy
                };

                if (obj.TableTypeId == 0)
                {
                    oConnectionContext.DbClsTableType.Add(oTableType);
                }
                else
                {
                    oConnectionContext.DbClsTableType.Attach(oTableType);
                    oConnectionContext.Entry(oTableType).Property(x => x.TableTypeId).IsModified = true;
                    oConnectionContext.Entry(oTableType).Property(x => x.TableTypeName).IsModified = true;
                    oConnectionContext.Entry(oTableType).Property(x => x.Description).IsModified = true;
                    oConnectionContext.Entry(oTableType).Property(x => x.IsActive).IsModified = true;
                    oConnectionContext.Entry(oTableType).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oTableType).Property(x => x.ModifiedOn).IsModified = true;
                    oTableType.ModifiedOn = CurrentDate;
                }

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "TableType",
                    CompanyId = obj.CompanyId,
                    Description = "Table Type \"" + obj.TableTypeName + "\" " + (obj.TableTypeId == 0 ? "created" : "updated"),
                    Id = oTableType.TableTypeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = obj.TableTypeId == 0 ? "Insert" : "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Table Type " + (obj.TableTypeId == 0 ? "created" : "updated") + " successfully",
                    Data = new
                    {
                        TableType = oTableType
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TableTypeActiveInactive(ClsTableTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                ClsTableType oTableType = new ClsTableType()
                {
                    TableTypeId = obj.TableTypeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsTableType.Attach(oTableType);
                oConnectionContext.Entry(oTableType).Property(x => x.TableTypeId).IsModified = true;
                oConnectionContext.Entry(oTableType).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oTableType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTableType).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Table Type " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TableTypeDelete(ClsTableTypeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                // Check if table type is in use
                int tableCount = oConnectionContext.DbClsRestaurantTable.Where(a => a.TableTypeId == obj.TableTypeId && a.IsDeleted == false).Count();

                if (tableCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsTableType oTableType = new ClsTableType()
                {
                    TableTypeId = obj.TableTypeId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate
                };
                oConnectionContext.DbClsTableType.Attach(oTableType);
                oConnectionContext.Entry(oTableType).Property(x => x.TableTypeId).IsModified = true;
                oConnectionContext.Entry(oTableType).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oTableType).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTableType).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Table Type deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveTableTypes(ClsTableTypeVm obj)
        {
            var tableTypes = oConnectionContext.DbClsTableType.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false)
                .Select(a => new
                {
                    TableTypeId = a.TableTypeId,
                    TableTypeName = a.TableTypeName
                }).OrderBy(a => a.TableTypeName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TableTypes = tableTypes
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}


