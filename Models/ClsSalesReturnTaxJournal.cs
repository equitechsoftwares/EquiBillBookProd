using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesReturnTaxJournal")]
    public class ClsSalesReturnTaxJournal
    {
        [Key]
        public long SalesReturnTaxJournalId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesReturnDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesReturnTaxJournalType { get; set; }
    }

    public class ClsSalesReturnTaxJournalVm
    {
        public long SalesReturnTaxJournalId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesReturnDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesReturnTaxJournalType { get; set; }
    }

}