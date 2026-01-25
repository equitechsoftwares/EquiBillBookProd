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
    public class FeedbackController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllFeedbacksAdmin(ClsFeedbackVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.UserType).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsFeedback
                       join b in oConnectionContext.DbClsUser
on a.CompanyId equals b.UserId
                       where b.Under == obj.Under && a.IsDeleted == false
                       select new
                       {
                           ResellerId = b.ResellerId,
                           FeedbackId = a.FeedbackId,
                           a.Feedback,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           CompanyId = a.CompanyId,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.EmailId).FirstOrDefault(),
                       }).ToList();

            if (UserType.ToLower() == "reseller")
            {
                det = det.Where(a => a.ResellerId == obj.AddedBy).Select(a => a).ToList();
            }

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Feedback.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Feedbacks = det.OrderByDescending(a => a.FeedbackId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertFeedback(ClsFeedbackVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Feedback == null || obj.Feedback == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divFeedback" });
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

                ClsFeedback oFeedback = new ClsFeedback()
                {
                    Feedback = obj.Feedback,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsFeedback.Add(oFeedback);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Feedback",
                    CompanyId = obj.CompanyId,
                    Description = "Feedback sent",
                    Id = oFeedback.FeedbackId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Thanks for your valuable feedback/ suggestion",
                    Data = new
                    {
                        Feedback = oFeedback
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
