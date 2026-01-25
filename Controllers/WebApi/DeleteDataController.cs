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
    //[ExceptionHandlerAttribute]
    //[IdentityBasicAuthenticationAttribute]
    public class DeleteDataController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        [HttpPost]
        public async Task<IHttpActionResult> DeleteAll(ClsBranch obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                string branch = "update tblBranch set IsDeleted=1 where IsMain=0 and Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(branch);

                string brand = "update tblBrand set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(brand);

                string category = "update tblCategory set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(category);

                string city = "update tblCity set IsDeleted=1  where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(city);

                string country = "update tblCountry set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(country);

                string currency = "update tblCurrency  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(currency);

                string expense = "update tblExpense  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(expense);

                string expenseCategory = "update tblExpenseCategory  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(expenseCategory);

                string income = "update tblIncome  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(income);

                string incomeCategory = "update tblIncomeCategory  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(incomeCategory);

                string item = "update tblItem  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(item);

                string itemBranchNap = "update tblItemBranchMap set IsDeleted=1  where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(itemBranchNap);

                string itemDetails = "update tblItemDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(itemDetails);

                string itemLocation = "update tblItemLocation  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(itemLocation);

                //string menuPermission = "update tblMenuPermission  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                //oConnectionContext.Database.ExecuteSqlCommand(menuPermission);

                string openingStock = "update tblOpeningStock  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(openingStock);

                string opeingStockDetails = "update tblOpeningStockDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(opeingStockDetails);

                string payment = "update tblPayment set IsDeleted=1  where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(payment);

                string paymentType = "update tblPaymentType  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(paymentType);

                string purchase = "update tblPurchase  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(purchase);

                string purchaseDetails = "update tblPurchaseDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(purchaseDetails);

                string purchaseReturn = "update tblPurchaseReturn  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(purchaseReturn);

                string purchaseReturnDetails = "update tblPurchaseReturnDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(purchaseReturnDetails);

                string religion = "update tblReligion  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(religion);

                string role = "update tblRole  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(role);

                string sales = "update tblSales  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(sales);

                string salesDetails = "update tblSalesDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(salesDetails);

                string salesReturn = "update tblSalesReturn  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(salesReturn);

                string salesReturnDetails = "update tblSalesReturnDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(salesReturnDetails);

                string state = "update tblState  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(state);

                string stockAdjustment = "update tblStockAdjustment  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(stockAdjustment);

                string stockAdjustmentDetails = "update tblStockAdjustmentDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(stockAdjustmentDetails);

                string stockTransfer = "update tblStockTransfer set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(stockTransfer);

                string stockTransferDetails = "update tblStockTransferDetails set IsDeleted=1  where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(stockTransferDetails);

                string subCategory = "update tblSubCategory  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(subCategory);

                string subSubCategory = "update tblSubSubCategory  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(subSubCategory);

                string tax = "update tblTax  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(tax);

                string unit = "update tblUnit  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(unit);

                string user = "update tblUser  set IsDeleted=1 where IsCompany=0 and Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(user);

                string userBranchMap = "update tblUserBranchMap set IsDeleted=1  where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(userBranchMap);

                string userGroup = "update  tblUserGroup  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(userGroup);

                string variation = "update tblVariation  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(variation);

                string variationDetails = "update tblVariationDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(variationDetails);

                string warranty = "update tblWarranty  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(warranty);

                string wastage = "update tblWastage  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(wastage);

                string wastageDetails = "update tblWastageDetails  set IsDeleted=1 where Companyid=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(wastageDetails);


                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
