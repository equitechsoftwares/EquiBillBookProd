using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseAdditionalTaxJournal")]
    public class ClsPurchaseAdditionalTaxJournal
    {
        [Key]
        public long PurchaseAdditionalTaxJournalId { get; set; }
        public long PurchaseId { get; set; }
        public long PurchaseAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseTaxJournalType { get; set; }
    }

    public class ClsPurchaseAdditionalTaxJournalVm
    {
        public long PurchaseAdditionalTaxJournalId { get; set; }
        public long PurchaseId { get; set; }
        public long PurchaseAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseTaxJournalType { get; set; }
    }
}