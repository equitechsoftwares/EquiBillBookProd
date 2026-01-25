using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblItemCode")]
    public class ClsItemCode
    {
        [Key]
        public long ItemCodeId { get; set; }
        public string ItemCodeType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public long IntraStateTaxId { get; set; }
        public long InterStateTaxId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long TaxPreferenceId { get; set; }
        public long TaxExemptionId { get; set; }
    }

    public class ClsItemCodeVm
    {
        public long ItemCodeId { get; set; }
        public string ItemCodeType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public long IntraStateTaxId { get; set; }
        public long InterStateTaxId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public string Domain { get; set; }
        public string IntraStateTax { get; set; }
        public string InterStateTax { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public bool IsUpdate { get; set; }
        public decimal IntraStateTaxPercentage { get; set; }
        public decimal InterStateTaxPercentage { get; set; }
        public long TaxPreferenceId { get; set; }
        public long TaxExemptionId { get; set; }
        public string TaxExemptionType { get; set; }
        public string TaxPreference { get; set; }
    }

}