using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTax")]
    public class ClsTax
    {
        [Key]
        public long TaxId { get; set; }
        public string Tax { get; set; }
        public decimal TaxPercent { get; set; }
        //public string SubTaxIds { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool ForTaxGroupOnly { get; set; }
        public bool IsTaxGroup { get; set; }
        public long PurchaseAccountId { get; set; }
        public long SalesAccountId { get; set; }
        public long SupplierPaymentAccountId { get; set; }
        public long CustomerPaymentAccountId { get; set; }
        public long ExpenseAccountId { get; set; }
        public long IncomeAccountId { get; set; }
        public long TaxTypeId { get; set; }
        public bool CanDelete { get; set; }
        public bool IsPredefined { get; set; }
        public bool IsCompositionScheme { get; set; }
    }

    public class ClsTaxVm
    {
        public long TaxId { get; set; }
        public string Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public string SubTaxIds { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string[] SubTaxs { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public bool ForTaxGroupOnly { get; set; }
        public bool IsTaxGroup { get; set; }
        public string TaxType { get; set; }
        public long BranchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TaxAmount { get; set; }
        public long UserId { get; set; }
        public string Domain { get; set; }
        public decimal SubTotal { get; set; }
        public long PurchaseAccountId { get; set; }
        public long SalesAccountId { get; set; }
        public long SupplierPaymentAccountId { get; set; }
        public long CustomerPaymentAccountId { get; set; }
        public long ExpenseAccountId { get; set; }
        public long IncomeAccountId { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxableAmount { get; set; }
        public long TaxTypeId { get; set; }
        public bool CanDelete { get; set; }
        public bool IsPredefined { get; set; }
        public long Id { get; set; }
        public string ReferenceNo { get; set; }
        public long CustomerId { get; set; }
        public long SupplierId { get; set; }
        public long AccountId { get; set; }
        public bool IsCompositionScheme { get; set; }
    }

    public class ClsTaxReport
    {
        public long TotalPurchaseCount { get; set; }
        public List<ClsPurchaseVm> Purchases { get; set; }
        public long TotalSaleCount { get; set; }
        public List<ClsSalesVm> Sales { get; set; }
        public long TotalExpenseCount { get; set; }
        public List<ClsExpenseVm> Expenses { get; set; }
        public long TotalIncomeCount { get; set; }
        public decimal OverallTax { get; set; }
    }

    public class ClsTaxDocuments
    {
        public long PrefixId { get; set; }
        public string NatureOfDocument { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int TotalNumber { get; set; }
        public int TotalCancelled { get; set; }
        public int DocumentNumber { get; set; }
    }
}