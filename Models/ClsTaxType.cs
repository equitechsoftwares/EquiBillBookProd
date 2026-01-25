using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTaxType")]
    public class ClsTaxType
    {
        [Key]
        public long TaxTypeId { get; set; }
        public string TaxType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ClsTaxTypeVm
    {
        public long TaxTypeId { get; set; }
        public string TaxType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long CountryId { get; set; }
        public long CompanyId { get; set; }
        public decimal TaxAmount { get; set; }
    }
}