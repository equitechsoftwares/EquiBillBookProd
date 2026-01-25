using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class ActivityLogController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllActivityLogs(ClsActivityLogVm obj)
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

            List<ClsActivityLogVm> det = new List<ClsActivityLogVm>();

            if (obj.Type == "" || obj.Type == null || obj.Type == "Register" || obj.Type == "Login" || obj.Type == "Logout" || obj.Type == "User Role"
                || obj.Type == "Users")
            {
                det = (from c in oConnectionContext.DbClsUserLog
                       where c.CompanyId == obj.CompanyId
                       && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                       select new ClsActivityLogVm
                       {
                           AddedBy = c.AddedBy,
                           CompanyId = c.CompanyId,
                           IpAddress = c.IpAddress,
                           ActivityLogId = c.UserLogId,
                           AddedOn = c.AddedOn,
                           Browser = c.Browser,
                           Description = c.Description,
                           Category = c.Category,
                           Id = c.Id,
                           Platform = c.Platform,
                           Type = c.Type,
                           Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                       }).ToList();
            }

            if (obj.Type == "" || obj.Type == null || obj.Type == "Categories" || obj.Type == "Sub Categories" || obj.Type == "Sub Sub Categories" ||
                obj.Type == "Units" || obj.Type == "Secondary Units" || obj.Type == "Tertiary Units" || obj.Type == "Quaternary Units" ||
                obj.Type == "Variation" || obj.Type == "Warranties" || obj.Type == "Brand" || obj.Type == "Items" || obj.Type == "Opening Stock"
                 || obj.Type == "Selling Price Group" || obj.Type == "Print Label" || obj.Type == "Stock Adjust" || obj.Type == "Stock Transfer")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsItemLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.ItemLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }
            if (obj.Type == "" || obj.Type == null || obj.Type == "Suppliers" || obj.Type == "Purchase Quotation" || obj.Type == "Purchase Order" ||
                obj.Type == "Purchase" || obj.Type == "Purchase Return" || obj.Type == "Supplier Payment" || obj.Type == "Supplier Refund"
                || obj.Type == "Expense")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsPurchaseLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.PurchaseLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }
            if (obj.Type == "" || obj.Type == null || obj.Type == "Customer Group" || obj.Type == "Customers" || obj.Type == "Sales Quotation"
                || obj.Type == "Sales Order" || obj.Type == "Delivery Challan" || obj.Type == "Sales Proforma" || obj.Type == "Sales" || obj.Type == "POS"
                || obj.Type == "Sales Return" || obj.Type == "Customer Payment" || obj.Type == "Customer Refund")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsSaleLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.SaleLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }
            if (obj.Type == "" || obj.Type == null || obj.Type == "State" || obj.Type == "City" || obj.Type == "Branch")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsPlaceLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.PlaceLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }

            if (obj.Type == "" || obj.Type == null || obj.Type == "Banks" || obj.Type == "Contra")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsBankLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.BankLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }

            if (obj.Type == "" || obj.Type == null || obj.Type == "Chart of Accounts" || obj.Type == "Journal")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsAccountLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.AccountLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }
            if (obj.Type == "" || obj.Type == null || obj.Type == "Currency" || obj.Type == "Tax List" || obj.Type == "Tax Group" ||
                    obj.Type == "Payment Methods" || obj.Type == "Payment Terms" || obj.Type == "Business Settings" || obj.Type == "Invoice Templates" ||
                    obj.Type == "Notification Templates" || obj.Type == "Reminders" || obj.Type == "Domain" || obj.Type == "Profile Update" ||
                    obj.Type == "Change Password" || obj.Type == "Change Login Email" || obj.Type == "Password Reset")
            {
                    det = det.Concat(from c in oConnectionContext.DbClsSettingLog
                                     where c.CompanyId == obj.CompanyId
                                     && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                     select new ClsActivityLogVm
                                     {
                                         AddedBy = c.AddedBy,
                                         CompanyId = c.CompanyId,
                                         IpAddress = c.IpAddress,
                                         ActivityLogId = c.SettingLogId,
                                         AddedOn = c.AddedOn,
                                         Browser = c.Browser,
                                         Description = c.Description,
                                         Category = c.Category,
                                         Id = c.Id,
                                         Platform = c.Platform,
                                         Type = c.Type,
                                         Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                     }).ToList();
            }
            if (obj.Type == "" || obj.Type == null || obj.Type == "Subscription Plan" || obj.Type == "Feedback" || obj.Type == "Support Ticket")
            {
                det = det.Concat(from c in oConnectionContext.DbClsOtherLog
                                 where c.CompanyId == obj.CompanyId
                                 && DbFunctions.TruncateTime(c.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(c.AddedOn) <= obj.ToDate
                                 select new ClsActivityLogVm
                                 {
                                     AddedBy = c.AddedBy,
                                     CompanyId = c.CompanyId,
                                     IpAddress = c.IpAddress,
                                     ActivityLogId = c.OtherLogId,
                                     AddedOn = c.AddedOn,
                                     Browser = c.Browser,
                                     Description = c.Description,
                                     Category = c.Category,
                                     Id = c.Id,
                                     Platform = c.Platform,
                                     Type = c.Type,
                                     Name = oConnectionContext.DbClsUser.Where(bb => bb.UserId == c.AddedBy).Select(bb => bb.Username).FirstOrDefault()
                                 }).ToList();
            }

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.AddedBy == obj.UserId).ToList();
            }

            if (obj.Type != "" && obj.Type != null)
            {
                det = det.Where(a => a.Category != null && a.Category.ToLower() == obj.Type.ToLower()).ToList();
            }

            //if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            //{
            //    det = det.Where(a => a.AddedOn.Date >= obj.FromDate.AddHours(5).AddMinutes(30) && a.AddedOn.Date <= obj.ToDate.AddHours(5).AddMinutes(30)).ToList();
            //}

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ActivityLogs = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    //Branchs = userDetails.BranchIds,
                    //Users = Users,
                    TotalCount = det.Count(),
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

    }
}
