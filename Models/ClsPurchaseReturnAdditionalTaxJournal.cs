using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseReturnAdditionalTaxJournal")]
    public class ClsPurchaseReturnAdditionalTaxJournal
    {
        [Key]
        public long PurchaseReturnAdditionalTaxJournalId { get; set; }
        public long PurchaseReturnId { get; set; }
        public long PurchaseReturnAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseReturnTaxJournalType { get; set; }
    }

    public class ClsPurchaseReturnAdditionalTaxJournalVm
    {
        public long PurchaseReturnAdditionalTaxJournalId { get; set; }
        public long PurchaseReturnId { get; set; }
        public long PurchaseReturnAdditionalChargesId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseReturnTaxJournalType { get; set; }
    }
}