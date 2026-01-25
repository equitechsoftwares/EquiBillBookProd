using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseReturnTaxJournal")]
    public class ClsPurchaseReturnTaxJournal
    {
        [Key]
        public long PurchaseReturnTaxJournalId { get; set; }
        public long PurchaseReturnId { get; set; }
        public long PurchaseReturnDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseReturnTaxJournalType { get; set; }
    }

    public class ClsPurchaseReturnTaxJournalVm
    {
        public long PurchaseReturnTaxJournalId { get; set; }
        public long PurchaseReturnId { get; set; }
        public long PurchaseReturnDetailsId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string PurchaseReturnTaxJournalType { get; set; }
    }

}