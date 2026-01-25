using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblContra")]
    public class ClsContra
    {
        [Key]
        public long ContraId { get; set; }
        public long BranchId { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string ReferenceId { get; set; }
        public string Type { get; set; }
        public long PaymentTypeId { get; set; }
        public string BatchNo { get; set; }
        public long PrefixId { get; set; }
    }

    public class ClsContraVm
    {
        public long ContraId { get; set; }
        public long BranchId { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
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
        public string FromAccountName { get; set; }
        public string AccountNumber { get; set; }
        public string ToAccountName { get; set; }
        public string ModifiedByCode { get; set; }
        public string InvoiceUrl { get; set; }
        public string Type { get; set; }
        public long PaymentTypeId { get; set; }
        public List<ClsContraImport> ContraImports { get; set; }
        public string BatchNo { get; set; }
        public long PrefixId { get; set; }
    }

    public class ClsContraImport
    {
        public string BranchName { get; set; }
        public string ReferenceNo { get; set; }
        public string Type { get; set; }
        public string FromAccountName { get; set; }
        public string ToAccountName { get; set; }        
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Notes { get; set; }
        public string PaymentType { get; set; }
    }
}