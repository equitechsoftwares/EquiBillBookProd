using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseTaxJournal")]
    public class ClsPurchaseTaxJournal
    {
        [Key]
        public long PurchaseTaxJournalId { get; set; }
        public long PurchaseId { get; set; }
        public long PurchaseDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseTaxJournalType { get; set; }
    }

    public class ClsPurchaseTaxJournalVm
    {
        public long PurchaseTaxJournalId { get; set; }
        public long PurchaseId { get; set; }
        public long PurchaseDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseTaxJournalType { get; set; }
    }
}