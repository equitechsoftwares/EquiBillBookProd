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
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    public class PrintLabelController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        [AllowAnonymous]
        public async Task<IHttpActionResult> PrintLabel(ClsPrintLabelVm obj)
        {
            var det = oConnectionContext.DbClsPrintLabel.Where(a => a.IsDeleted == false && a.PrintLabelId == obj.PrintLabelId).Select(a => new
            {
                a.PrintLabelJson,
                a.CompanyId
            }).FirstOrDefault();

            var User = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PrintLabel = det,
                    User = User
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPrintLabel(ClsPrintLabelVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.ItemDetails == null || obj.ItemDetails.Count == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divtags" });
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

                obj.PrintLabelId = oConnectionContext.DbClsPrintLabel.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.PrintLabelId).FirstOrDefault();


                if (obj.PrintLabelId == 0)
                {
                    ClsPrintLabel oPrintLabel = new ClsPrintLabel()
                    {
                        PrintLabelJson = obj.PrintLabelJson,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                    };
                    oConnectionContext.DbClsPrintLabel.Add(oPrintLabel);
                    oConnectionContext.SaveChanges();

                    obj.PrintLabelId = oPrintLabel.PrintLabelId;
                }
                else
                {
                    ClsPrintLabel oPrintLabel = new ClsPrintLabel()
                    {
                        PrintLabelId = obj.PrintLabelId,
                        PrintLabelJson = obj.PrintLabelJson,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                    };
                    oConnectionContext.DbClsPrintLabel.Attach(oPrintLabel);
                    oConnectionContext.Entry(oPrintLabel).Property(x => x.PrintLabelId).IsModified = true;
                    oConnectionContext.Entry(oPrintLabel).Property(x => x.PrintLabelJson).IsModified = true;
                    oConnectionContext.Entry(oPrintLabel).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oPrintLabel).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Print Label",
                    CompanyId = obj.CompanyId,
                    Description = "Created Barcode",
                    Id = 0,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert",
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Barcode created successfully",
                    Data = new
                    {
                        PrintLabel =new {
                            PrintLabelId = obj.PrintLabelId
                        } 
                        //PrintLabel = oConnectionContext.DbClsPrintLabel.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.PrintLabelId == obj.PrintLabelId).Select(a => new
                        //{
                        //    obj.PrintLabelId,
                        //    a.PrintLabelJson
                        //}).FirstOrDefault()
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }


    }
}
