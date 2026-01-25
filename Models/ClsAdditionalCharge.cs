using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblAdditionalCharge")]
    public class ClsAdditionalCharge
    {
        [Key]
        public long AdditionalChargeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long ItemCodeId { get; set; }
        public long TaxPreferenceId { get; set; }
        public long TaxExemptionId { get; set; }
        public long IntraStateTaxId { get; set; }
        public long InterStateTaxId { get; set; }
        public string Description { get; set; }
        public long PurchaseAccountId { get; set; }
        public long SalesAccountId { get; set; }
        public string ShortCutKey { get; set; }
    }

    public class ClsAdditionalChargeVm
    {
        public long AdditionalChargeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long ItemCodeId { get; set; }
        public long TaxPreferenceId { get; set; }
        public long TaxExemptionId { get; set; }
        public long IntraStateTaxId { get; set; }
        public long InterStateTaxId { get; set; }
        public string Description { get; set; }
        public long PurchaseAccountId { get; set; }
        public long SalesAccountId { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public string Domain { get; set; }
        public string TaxPreference { get; set; }
        public long CustomerId { get; set; }
        public long SupplierId { get; set; }
        public string GstTreatment { get; set; }
        public long BranchId { get; set; }
        public bool IsBillOfSupply { get; set; }
        public string ShortCutKey { get; set; }
    }
}