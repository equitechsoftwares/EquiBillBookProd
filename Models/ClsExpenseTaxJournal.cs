using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblExpenseTaxJournal")]
    public class ClsExpenseTaxJournal
    {
        [Key]
        public long ExpenseTaxJournalId { get; set; }
        public long ExpenseId { get; set; }
        public long ExpensePaymentId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string ExpenseTaxJournalType { get; set; }
    }

    public class ClsExpenseTaxJournalVm
    {
        public long ExpenseTaxJournalId { get; set; }
        public long ExpenseId { get; set; }
        public long ExpensePaymentId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string ExpenseTaxJournalType { get; set; }
    }

}