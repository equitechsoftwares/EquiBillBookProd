using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblBankPayment")]
    public class ClsBankPayment
    {
        [Key]
        public long BankPaymentId { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string Type { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceId { get; set; }
    }

    public class ClsBankPaymentVm
    {
        public long BankPaymentId { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string Type { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceId { get; set; }
        public string Domain { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string FileExtensionAttachDocument { get; set; }
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
        public long AccountId { get; set; }
        public long Id { get; set; }
        public string FromAccountName { get; set; }
        public string ToAccountName { get; set; }
        public string ModifiedByCode { get; set; }
        public long AccountSubTypeId { get; set; }
        public long CustomerId { get; set; }
        public long SupplierId { get; set; }
        public long TaxId { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Tax { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public bool IsTaxGroup { get; set; }
        public decimal TaxPercent { get; set; }
        public string DocumentName { get; set; }
        public int NoOfRecords { get; set; }
        //public decimal TotalInvoiceValue { get; set; }
        //public decimal TotalTaxableValue { get; set; }
        //public long TaxSettingId { get; set; }
        //public List<ClsTaxTypeVm> TaxTypes { get; set; }
        public bool IsTaxAccount { get; set; }
    }
}