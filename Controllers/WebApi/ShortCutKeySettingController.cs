using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class ShortCutKeySettingController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> ShortCutKeySettings(ClsShortCutKeySettingVm obj)
        {
            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2
            //&& a.IsActive == true
            ).Select(a => a.TransactionId).FirstOrDefault();

            var PlanAddons = (from aa in oConnectionContext.DbClsTransaction
                              join bb in oConnectionContext.DbClsTransactionDetails
  on aa.TransactionId equals bb.TransactionId
                              where aa.TransactionId == TransactionId && aa.Status == 2
                              select new { bb.Type }).Union(from aa in oConnectionContext.DbClsTransaction
                                                            join bb in oConnectionContext.DbClsTransactionDetails
                                on aa.TransactionId equals bb.TransactionId
                                                            where aa.ParentTransactionId == TransactionId && aa.Status == 2
                                                            select new { bb.Type }).ToList();

            var det = oConnectionContext.DbClsMenu.Where(a => a.IsDeleted == false &&
            a.IsQuickLink == true
            ).ToList().Where(a => PlanAddons.Select(x => x.Type.ToLower()).Contains(a.MenuType.ToLower())
            ).Select(a => new
            {
                a.Sequence,
                a.MenuId,
                Menu = a.Title,
                Title = oConnectionContext.DbClsShortCutKeySetting.Where(b => b.CompanyId == obj.CompanyId && b.MenuId == a.MenuId && b.IsActive == true).Select(b => b.Title).FirstOrDefault(),
                ShortCutKey = oConnectionContext.DbClsShortCutKeySetting.Where(b => b.CompanyId == obj.CompanyId && b.MenuId == a.MenuId && b.IsActive == true).Select(b => b.ShortCutKey).FirstOrDefault(),
                ShortCutKeySettingId = oConnectionContext.DbClsShortCutKeySetting.Where(b => b.CompanyId == obj.CompanyId && b.MenuId == a.MenuId && b.IsActive == true).Select(b => b.ShortCutKeySettingId).FirstOrDefault(),
            }).OrderBy(a => a.Sequence).Union(oConnectionContext.DbClsShortCutKeySetting.Where(a => a.CompanyId == obj.CompanyId && a.MenuId == 0).Select(a => new
            {
                Sequence = 0,
                a.MenuId,
                Menu = a.Title,
                a.Title,
                a.ShortCutKey,
                a.ShortCutKeySettingId
            })).ToList();

            // var det = oConnectionContext.DbClsMenu.Where(a => a.IsDeleted == false && a.IsCancelled == false &&
            //a.IsQuickLink == true
            //).ToList().Where(a => PlanAddons.Select(x => x.Type.ToLower()).Contains(a.MenuType.ToLower())
            //).Select(a => new
            //{
            //    a.Sequence,
            //    a.MenuId,
            //    Menu = a.Title,
            //    Title = oConnectionContext.DbClsShortCutKeySetting.Where(b => b.CompanyId == obj.CompanyId && b.MenuId == a.MenuId && b.IsActive == true).Select(b => b.Title).FirstOrDefault(),
            //    ShortCutKey = oConnectionContext.DbClsShortCutKeySetting.Where(b => b.CompanyId == obj.CompanyId && b.MenuId == a.MenuId && b.IsActive == true).Select(b => b.ShortCutKey).FirstOrDefault(),
            //    ShortCutKeySettingId = oConnectionContext.DbClsShortCutKeySetting.Where(b => b.CompanyId == obj.CompanyId && b.MenuId == a.MenuId && b.IsActive == true).Select(b => b.ShortCutKeySettingId).FirstOrDefault(),
            //}).OrderBy(a => a.Sequence)
            //.Union(oConnectionContext.DbClsShortCutKeySetting.Where(a => a.CompanyId == obj.CompanyId && a.MenuId == 0).Select(a => new
            //{
            //    Sequence = 0,
            //    a.MenuId,
            //    Menu = a.Title,
            //    a.Title,
            //    a.ShortCutKey,
            //    a.ShortCutKeySettingId
            //})).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ShortCutKeySettings = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ShortCutKeySettingUpdate(ClsBusinessSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.ShortCutKeySettings != null)
                {
                    foreach (var item in obj.ShortCutKeySettings)
                    {
                        foreach (var inner in obj.ShortCutKeySettings)
                        {
                            if (item.Title != "" && item.Title != null)
                            {
                                if (item.Title.ToLower() == inner.Title.ToLower() && item.divId != inner.divId)
                                {
                                    errors.Add(new ClsError { Message = "Duplicate Custom Name exists", Id = "divTitle" + item.divId });
                                    isError = true;
                                }
                            }

                            if (item.ShortCutKey != "" && item.ShortCutKey != null)
                            {
                                if (inner.ShortCutKey != "" && inner.ShortCutKey != null)
                                {
                                    if (item.ShortCutKey.ToLower() == inner.ShortCutKey.ToLower() && item.divId != inner.divId)
                                    {
                                        errors.Add(new ClsError { Message = "Duplicate Shortcut Key exists", Id = "divShortCutKey" + item.divId });
                                        isError = true;
                                    }
                                }
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

                    string query = "update \"tblShortCutKeySetting\" set \"IsActive\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var ShortCutKeySetting in obj.ShortCutKeySettings)
                    {
                        if (ShortCutKeySetting.ShortCutKeySettingId == 0)
                        {
                            ClsShortCutKeySetting oClsShortCutKeySetting = new ClsShortCutKeySetting()
                            {
                                IsActive = true,
                                IsDeleted = false,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                MenuId = ShortCutKeySetting.MenuId,
                                Title = ShortCutKeySetting.Title,
                                ShortCutKey = ShortCutKeySetting.ShortCutKey,
                            };

                            oConnectionContext.DbClsShortCutKeySetting.Add(oClsShortCutKeySetting);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsShortCutKeySetting oClsPurchaseDetails = new ClsShortCutKeySetting()
                            {
                                ShortCutKeySettingId = ShortCutKeySetting.ShortCutKeySettingId,
                                IsActive = true,
                                //IsDeleted = ShortCutKeySetting.IsDeleted,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                MenuId = ShortCutKeySetting.MenuId,
                                Title = ShortCutKeySetting.Title,
                                ShortCutKey = ShortCutKeySetting.ShortCutKey,
                            };
                            oConnectionContext.DbClsShortCutKeySetting.Attach(oClsPurchaseDetails);
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.IsActive).IsModified = true;
                            //oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.MenuId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.Title).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseDetails).Property(x => x.ShortCutKey).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Shortcut Keys\" updated",
                    Id = 0,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Shortcut Keys Information updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveShortCutKeySettings(ClsShortCutKeySettingVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
            }).FirstOrDefault();

            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2 && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault();

            var PlanAddons = (from aa in oConnectionContext.DbClsTransaction
                              join bb in oConnectionContext.DbClsTransactionDetails
  on aa.TransactionId equals bb.TransactionId
                              where aa.TransactionId == TransactionId && aa.Status == 2
                              select new { bb.Type }).Union(from aa in oConnectionContext.DbClsTransaction
                                                            join bb in oConnectionContext.DbClsTransactionDetails
                                on aa.TransactionId equals bb.TransactionId
                                                            where aa.ParentTransactionId == TransactionId && aa.Status == 2
                                                            select new { bb.Type }).ToList();

            var det = (from a in oConnectionContext.DbClsShortCutKeySetting
                       join b in oConnectionContext.DbClsMenu
on a.MenuId equals b.MenuId
                       where b.IsDeleted == false && b.IsQuickLink == true
                       && a.CompanyId == obj.CompanyId && a.IsActive == true
                       && b.QuickLinkParentId == 0 && a.ShortCutKey != "" && a.ShortCutKey != null
                       select new
                       {
                           a.MenuId,
                           IsView = userDetails.IsCompany ? true : oConnectionContext.DbClsMenuPermission.Where(bb => bb.MenuId == a.MenuId && bb.RoleId == userDetails.UserRoleId).Select(bb => bb.IsView).FirstOrDefault(),
                           b.Sequence,
                           b.Url,
                           a.Title,
                           a.ShortCutKey,
                           b.MenuType
                       }).AsEnumerable().Where(a => PlanAddons.Select(x => x.Type).Contains(a.MenuType)).OrderBy(a => a.Sequence)
                       .Union(from a in oConnectionContext.DbClsShortCutKeySetting
                              join b in oConnectionContext.DbClsMenu
       on a.MenuId equals b.MenuId
                              where b.IsDeleted == false && b.IsQuickLink == true
                              && a.CompanyId == obj.CompanyId && a.IsActive == true
                              && b.QuickLinkParentId != 0 && a.ShortCutKey != "" && a.ShortCutKey != null
                              select new
                              {
                                  a.MenuId,
                                  IsView = userDetails.IsCompany ? true : oConnectionContext.DbClsMenuPermission.Where(bb => bb.MenuId == b.QuickLinkParentId && bb.RoleId == userDetails.UserRoleId).Select(bb => bb.IsAdd).FirstOrDefault(),
                                  b.Sequence,
                                  b.Url,
                                  a.Title,
                                  a.ShortCutKey,
                                  b.MenuType
                              }).AsEnumerable().Where(a => PlanAddons.Select(x => x.Type).Contains(a.MenuType)).OrderBy(a => a.Sequence)
                       .Union(oConnectionContext.DbClsShortCutKeySetting.Where(a => a.CompanyId == obj.CompanyId && a.MenuId == 0).Select(a => new
                       {
                           a.MenuId,
                           IsView = true,
                           Sequence = 0,
                           Url = "",
                           a.Title,
                           a.ShortCutKey,
                           MenuType = ""
                       })).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ShortCutKeySettings = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
