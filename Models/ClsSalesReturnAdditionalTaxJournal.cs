using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesReturnAdditionalTaxJournal")]
    public class ClsSalesReturnAdditionalTaxJournal
    {
        [Key]
        public long SalesReturnAdditionalTaxJournalId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesReturnAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesReturnTaxJournalType { get; set; }
    }

    public class ClsSalesReturnAdditionalTaxJournalVm
    {
        public long SalesReturnAdditionalTaxJournalId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesReturnAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesReturnTaxJournalType { get; set; }
    }
}