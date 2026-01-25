using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblExpense")]
    public class ClsExpense
    {
        [Key]
        public long ExpenseId { get; set; }
        public string ExpenseCode { get; set; }
        public long UserId { get; set; }
        public long CustomerId { get; set; }
        public long SupplierId { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        //public decimal Amount { get; set; }
        public string Notes { get; set; }
        public string AttachDocument { get; set; }
        public long BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long AccountId { get; set; }
        public string ReferenceId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal GrandTotal { get; set; }
        public string BatchNo { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public string CalculateMileageUsing { get; set; }
        public decimal OdometerStartReading { get; set; }
        public decimal OdometerEndReading { get; set; }
        public decimal Distance { get; set; }
        public decimal MileageRate { get; set; }
        public bool IsBillable { get; set; }
        public decimal MarkupBy { get; set; }
        public string Type { get; set; }
        public long VehicleId { get; set; }
        public long UnitId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public int IsReverseCharge { get; set; }
        public long PrefixId { get; set; }
        public decimal TaxableAmount { get; set; }
    }
    public class ClsExpenseVm
    {
        public long ExpenseId { get; set; }
        public string ExpenseCode { get; set; }
        public long UserId { get; set; }
        public long CustomerId { get; set; }
        public long SupplierId { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        //public decimal Amount { get; set; }
        public string Notes { get; set; }
        public string AttachDocument { get; set; }
        public long BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long AccountId { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string FileExtensionAttachDocument { get; set; }
        public string ExpenseCategory { get; set; }
        public string Title { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string PaymentStatus { get; set; }
        public string ExpenseSubCategory { get; set; }
        public long TaxId { get; set; }
        public string UserType { get; set; }
        //public ClsAccountsPaymentVm Payment { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public string Name { get; set; }
        public IList<ClsExpenseImport> ExpenseImports { get; set; }
        public string TaxNo { get; set; }
        public List<ClsTaxVm> Taxs { get; set; }
        public decimal AmountExcTax { get; set; }
        public string PaymentType { get; set; }
        public string ExpenseForName { get; set; }
        public string Domain { get; set; }
        public string Tax { get; set; }
        public string ExpenseForMobileNo { get; set; }
        public string AccountName { get; set; }
        public List<ClsExpensePaymentVm> ExpensePayments { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal GrandTotal { get; set; }
        public string BatchNo { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public string Categorys { get; set; }
        public long ExpenseCount { get; set; }
        public string UserName { get; set; }
        public long SourceOfSupplyId { get; set; }
        public long DestinationOfSupplyId { get; set; }
        public string CalculateMileageUsing { get; set; }
        public decimal OdometerStartReading { get; set; }
        public decimal OdometerEndReading { get; set; }
        public decimal Distance { get; set; }
        public decimal MileageRate { get; set; }
        public bool IsBillable { get; set; }
        public decimal MarkupBy { get; set; }
        public string Type { get; set; }
        public long VehicleId { get; set; }
        public long UnitId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public int IsReverseCharge { get; set; }
        public long CountryId { get; set; }
        public long PrefixId { get; set; }
        public decimal TaxableAmount { get; set; }
    }

    public class ClsExpenseImport
    {
        public long ExpenseId { get; set; }
        public string BranchName { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public string CustomerUserName { get; set; }
        public string SupplierUserName { get; set; }
        public string PaidThrough { get; set; }
        public string ReferenceNo { get; set; }
        public string ExpenseAccount { get; set; }
        public string Notes { get; set; }
        public string Tax { get; set; }
        public decimal Amount { get; set; }
        public string GroupName { get; set; }        
    }

}