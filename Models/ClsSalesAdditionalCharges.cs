using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesAdditionalCharges")]
    public class ClsSalesAdditionalCharges
    {
        [Key]
        public long SalesAdditionalChargesId { get; set; }
        public long SalesId { get; set; }
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
    }

    public class ClsSalesAdditionalChargesVm
    {
        public long SalesAdditionalChargesId { get; set; }
        public long SalesId { get; set; }
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
        public long InterStateTaxId { get; set; }
        public long IntraStateTaxId { get; set; }
    }
} 