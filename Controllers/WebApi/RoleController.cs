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
    public class RoleController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllRoles(ClsRoleVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsRole.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.RoleCode,
                RoleId = a.RoleId,
                RoleName = a.RoleName,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ModifiedBy).Select(b => b.Username).FirstOrDefault(),
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.RoleName.Contains(obj.Search)).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Roles = det.OrderByDescending(a => a.RoleId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Role(ClsRole obj)
        {
            var det = oConnectionContext.DbClsRole.Where(a => a.RoleId == obj.RoleId && a.CompanyId == obj.CompanyId).Select(a => a).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Role = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertRole(ClsRoleVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.RoleName == null || obj.RoleName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRoleName" });
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

                //if (obj.RoleCode != "" && obj.RoleCode != null)
                //{
                //    if (oConnectionContext.DbClsRole.Where(a => a.RoleCode == obj.RoleCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        //data = new
                //        //{
                //        //    Status = 0,
                //        //    Message = "Duplicate Role Code exists",
                //        //    Data = new
                //        //    {
                //        //    }
                //        //}; return await Task.FromResult(Ok(data));
                //    }
                //}

                if (oConnectionContext.DbClsRole.Where(a => a.RoleName.ToLower() == obj.RoleName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Role Name exists", Id = "divRoleName" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Duplicate Role Name exists",
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

                if (obj.RoleCode == "" || obj.RoleCode == null)
                {
                    Random rdn = new Random();
                    obj.RoleCode = obj.RoleName.Split(' ')[0] + rdn.Next(111111, 999999);
                }

                ClsRole oClsRole = new ClsRole()
                {
                    RoleCode = obj.RoleCode,
                    RoleName = obj.RoleName,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsRole.Add(oClsRole);
                oConnectionContext.SaveChanges();

                if (obj.MenuPermissions != null)
                {
                    foreach (var item in obj.MenuPermissions)
                    {
                        ClsMenuPermission oClsMenuPermission = new ClsMenuPermission()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            IsActive = true,
                            IsDeleted = false,
                            RoleId = oClsRole.RoleId,
                            MenuId = item.MenuId,
                            IsAdd = item.IsAdd,
                            IsDelete = item.IsDelete,
                            IsEdit = item.IsEdit,
                            IsExport = item.IsExport,
                            IsView = item.IsView
                        };
                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsMenuPermission.Add(oClsMenuPermission);
                        oConnectionContext.SaveChanges();
                    };
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "User Role",
                    CompanyId = obj.CompanyId,
                    Description = "User Role: \"" + obj.RoleName + "\" created",
                    Id = oClsRole.RoleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Role created successfully",
                    Data = new
                    {
                        Role = oClsRole
                    }
                };
                dbContextTransaction.Complete();
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateRole(ClsRoleVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.RoleName == null || obj.RoleName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divRoleName" });
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
                if (oConnectionContext.DbClsRole.Where(a => a.RoleName.ToLower() == obj.RoleName.ToLower() && a.CompanyId == obj.CompanyId && a.RoleId != obj.RoleId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Role Name exists", Id = "divRoleName" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Duplicate Role Name exists",
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

                ClsRole oClsRole = new ClsRole()
                {
                    RoleId = obj.RoleId,
                    RoleName = obj.RoleName,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsRole.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.RoleId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.RoleName).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.MenuPermissions != null)
                {
                    foreach (var item in obj.MenuPermissions)
                    {
                        if (item.MenuPermissionId == 0)
                        {
                            ClsMenuPermission oClsMenuPermission = new ClsMenuPermission()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                IsActive = true,
                                IsDeleted = false,
                                RoleId = obj.RoleId,
                                MenuId = item.MenuId,
                                IsAdd = item.IsAdd,
                                IsDelete = item.IsDelete,
                                IsEdit = item.IsEdit,
                                IsExport = item.IsExport,
                                IsView = item.IsView
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsMenuPermission.Add(oClsMenuPermission);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsMenuPermission oClsMenuPermission = new ClsMenuPermission()
                            {
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                RoleId = obj.RoleId,
                                MenuId = item.MenuId,
                                MenuPermissionId = item.MenuPermissionId,
                                IsAdd = item.IsAdd,
                                IsDelete = item.IsDelete,
                                IsEdit = item.IsEdit,
                                IsExport = item.IsExport,
                                IsView = item.IsView
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsMenuPermission.Attach(oClsMenuPermission);
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.RoleId).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.MenuId).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.MenuPermissionId).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.IsAdd).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.IsEdit).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.IsDelete).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.IsView).IsModified = true;
                            oConnectionContext.Entry(oClsMenuPermission).Property(x => x.IsExport).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "User Role",
                    CompanyId = obj.CompanyId,
                    Description = "User Role \"" + obj.RoleName + "\" updated",
                    Id = oClsRole.RoleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Role updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RoleActiveInactive(ClsRoleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //if(obj.IsActive == false)
                //{
                //    if (oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserRoleId == obj.RoleId && a.IsDeleted == false && a.IsCancelled == false).Count() != 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Cannot deactivate as it is already in use",
                //            Data = new
                //            {
                //            }
                //        };
                //        return await Task.FromResult(Ok(data));
                //    }
                //}

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsRole oClsRole = new ClsRole()
                {
                    RoleId = obj.RoleId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsRole.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.RoleId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "User Role",
                    CompanyId = obj.CompanyId,
                    Description = "User Role \"" + oConnectionContext.DbClsRole.Where(a => a.RoleId == obj.RoleId).Select(a => a.RoleName).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsRole.RoleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Role " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RoleDelete(ClsRoleVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int UserCount = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserRoleId == obj.RoleId && a.IsDeleted == false).Count();
                if (UserCount != 0)
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
                ClsRole oClsRole = new ClsRole()
                {
                    RoleId = obj.RoleId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsRole.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.RoleId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "User Role",
                    CompanyId = obj.CompanyId,
                    Description = "User Role \"" + oConnectionContext.DbClsRole.Where(a => a.RoleId == obj.RoleId).Select(a => a.RoleName).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.RoleId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Role deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveRoles(ClsRoleVm obj)
        {
            var det = oConnectionContext.DbClsRole.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                RoleId = a.RoleId,
                RoleName = a.RoleName,
            }).OrderBy(a => a.RoleName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Roles = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
