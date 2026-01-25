using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblExpensePayment")]
    public class ClsExpensePayment
    {
        [Key]
        public long ExpensePaymentId { get; set; }
        public long ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long AccountId { get; set; }
        public long TaxId { get; set; }
        public long TaxAccountId { get; set; }
        public string ITCType { get; set; }
        public long TaxExemptionId { get; set; }
        public string ItemType { get; set; }
        public long ItemCodeId { get; set; }
    }

    public class ClsExpensePaymentVm
    {
        public long ExpensePaymentId { get; set; }
        public long ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long AccountId { get; set; }
        public long TaxId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string PaymentType { get; set; }
        public string AddedByCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Balance { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public long PaymentId { get; set; }
        public string From_ToAccount { get; set; }
        public string UserType { get; set; }
        public string InvoiceNo { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string ReferenceId { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public long DivId { get; set; }
        public long TaxAccountId { get; set; }
        public string ITCType { get; set; }
        public string Tax { get; set; }
        public long TaxExemptionId { get; set; }
        public string ItemType { get; set; }
        public long ItemCodeId { get; set; }
    }
}