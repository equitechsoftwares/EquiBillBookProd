using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesTaxJournal")]
    public class ClsSalesTaxJournal
    {
        [Key]
        public long SalesTaxJournalId { get; set; }
        public long SalesId { get; set; }
        public long SalesDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesTaxJournalType { get; set; }
    }

    public class ClsSalesTaxJournalVm
    {
        public long SalesTaxJournalId { get; set; }
        public long SalesId { get; set; }
        public long SalesDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SalesTaxJournalType { get; set; }
    }
}