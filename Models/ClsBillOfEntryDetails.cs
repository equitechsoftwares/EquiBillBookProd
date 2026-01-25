using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblBillOfEntryDetails")]
    public class ClsBillOfEntryDetails
    {
        [Key]
        public long BillOfEntryDetailsId { get; set; }
        public long BillOfEntryId { get; set; }
        public long PurchaseId { get; set; }
        public long PurchaseDetailsId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal AssessableValue { get; set; }
        public decimal CustomDuty { get; set; }
        public decimal AmountExcTax { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountIncTax { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsBillOfEntryDetailsVm
    {
        public long BillOfEntryDetailsId { get; set; }
        public long BillOfEntryId { get; set; }
        public long PurchaseId { get; set; }
        public long PurchaseDetailsId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal AssessableValue { get; set; }
        public decimal CustomDuty { get; set; }
        public decimal AmountExcTax { get; set; }
        public long TaxId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountIncTax { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ItemName { get; set; }
        public string VariationName { get; set; }
        public string SKU { get; set; }
        public long DivId { get; set; }
    }
}