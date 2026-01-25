using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesAdditionalTaxJournal")]
    public class ClsSalesAdditionalTaxJournal
    {
        [Key]
        public long SalesAdditionalTaxJournalId { get; set; }
        public long SalesId { get; set; }
        public long SalesAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesTaxJournalType { get; set; }
    }

    public class ClsSalesAdditionalTaxJournalVm
    {
        public long SalesAdditionalTaxJournalId { get; set; }
        public long SalesId { get; set; }
        public long SalesAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesTaxJournalType { get; set; }
    }
}