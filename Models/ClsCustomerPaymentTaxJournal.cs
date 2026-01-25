using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCustomerPaymentTaxJournal")]
    public class ClsCustomerPaymentTaxJournal
    {
        [Key]
        public long CustomerPaymentTaxJournalId { get; set; }
        public long CustomerPaymentId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string CustomerPaymentTaxJournalType { get; set; }
    }

    public class ClsCustomerPaymentTaxJournalVm
    {
        public long CustomerPaymentTaxJournalId { get; set; }
        public long CustomerPaymentId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string CustomerPaymentTaxJournalType { get; set; }
    }

}