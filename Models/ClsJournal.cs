using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EquiBillBook.Models;

namespace EquiBillBook
{
    [Table("public.tblJournal")]
    public class ClsJournal
    {
        [Key]
        public long JournalId { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }
        public bool IsCashBased { get; set; }
        public long CurrencyId { get; set; }
        public string AttachDocument { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long BranchId { get; set; }
        public string ReferenceId { get; set; }
        public string BatchNo { get; set; }
        public long PrefixId { get; set; }
    }

    public class ClsJournalVm
    {
        public long JournalId { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }
        public bool IsCashBased { get; set; }
        public long CurrencyId { get; set; }
        public string AttachDocument { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public long BranchId { get; set; }
        public string FileExtensionAttachDocument { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public List<ClsJournalPaymentVm> JournalPayments { get; set; }
        public string Domain { get; set; }
        public string ReferenceId { get; set; }
        public List<ClsJournalImport> JournalImports { get; set; }
        public string BatchNo { get; set; }
        public long PrefixId { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ClsJournalImport
    {
        public long JournalId { get; set; }
        public string BranchName { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }
        public string Account { get; set; }
        public string Description { get; set; }
        public string Contact { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public string GroupName { get; set; }
    }

}