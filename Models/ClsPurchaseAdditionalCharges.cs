using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblPurchaseAdditionalCharges")]
    public class ClsPurchaseAdditionalCharges
    {
        [Key]
        public long PurchaseAdditionalChargesId { get; set; }
        public long PurchaseId { get; set; }
        public long TaxId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountIncTax { get; set; }
        public long AccountId { get; set; }
        public long ItemCodeId { get; set; }
        public long TaxExemptionId { get; set; }
        public long AdditionalChargeId { get; set; }
        public string ITCType { get; set; }
    }

    public class ClsPurchaseAdditionalChargesVm
    {
        public long PurchaseAdditionalChargesId { get; set; }
        public long PurchaseId { get; set; }
        public long TaxId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountIncTax { get; set; }
        public long AccountId { get; set; }
        public long ItemCodeId { get; set; }
        public long TaxExemptionId { get; set; }
        public long AdditionalChargeId { get; set; }
        public string Name { get; set; }
        public string TaxExemptionReason { get; set; }
        public string ITCType { get; set; }
    }
} 