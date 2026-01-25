using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSupplierPaymentTaxJournal")]
    public class ClsSupplierPaymentTaxJournal
    {
        [Key]
        public long SupplierPaymentTaxJournalId { get; set; }
        public long SupplierPaymentId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SupplierPaymentTaxJournalType { get; set; }
    }

    public class ClsSupplierPaymentTaxJournalVm
    {
        public long SupplierPaymentTaxJournalId { get; set; }
        public long SupplierPaymentId { get; set; }
        public long AccountId { get; set; }
        public long CompanyId { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public string SupplierPaymentTaxJournalType { get; set; }
    }

}