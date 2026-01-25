using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblJournalPayment")]
    public class ClsJournalPayment
    {
        [Key]
        public long JournalPaymentId { get; set; }
        public long JournalId { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit{ get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long AccountId { get; set; }
        public long ExpenseFor { get; set; }
    }

    public class ClsJournalPaymentVm
    {
        public long JournalPaymentId { get; set; }
        public long JournalId { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long AccountId { get; set; }
        public long ExpenseFor { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}