using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using EquiBillBook.Models;

namespace EquiBillBook.Controllers.WebApi
{
    public class ItemCodeMasterController : ApiController
    {
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        dynamic data;

        [HttpPost]
        public async Task<IHttpActionResult> ImportItemCodeMaster(ClsItemCodeMasterVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.ItemCodeMasterImports == null || obj.ItemCodeMasterImports.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No data",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
            
            try
            {
                List<ClsItemCodeMaster> importedItems = new List<ClsItemCodeMaster>();

                foreach (var itemCode in obj.ItemCodeMasterImports)
                {
                    ClsItemCodeMaster newItemCode = new ClsItemCodeMaster
                    {
                        ItemCodeType = itemCode.ItemCodeType,
                        Code = itemCode.Code,
                        Description = itemCode.Description,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate
                    };

                    oConnectionContext.DbClsItemCodeMaster.Add(newItemCode);
                    oConnectionContext.SaveChanges();
                    importedItems.Add(newItemCode);
                }

                // Log the import activity
                // Note: You may want to add logging functionality here similar to other controllers

                data = new
                {
                    Status = 1,
                    Message = "Item codes imported successfully",
                    Data = new
                    {
                        ImportedCount = importedItems.Count,
                        ImportedItems = importedItems
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error occurred during import: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        public async Task<IHttpActionResult> ItemCodeAutocomplete(ClsItemCodeMasterVm obj)
        {
            var det = (from c in oConnectionContext.DbClsItemCodeMaster
                   where c.IsActive == true && c.IsDeleted == false && c.ItemCodeType == obj.ItemCodeType &&
                   (c.Code.ToLower().Contains(obj.Search.ToLower()) || c.Description.ToLower().Contains(obj.Search.ToLower()))
                   select c.Code + " ~ " + c.Description).Take(10);
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemsArray = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
