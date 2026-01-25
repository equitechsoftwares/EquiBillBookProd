using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTaxMaster")]
    public class ClsTaxMaster
    {
        [Key]
        public long TaxMasterId { get; set; }
        public long CountryId { get; set; }
        public string Tax { get; set; }
        public decimal TaxPercent { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool ForTaxGroupOnly { get; set; }
        public bool IsTaxGroup { get; set; }
        public long TaxTypeId { get; set; }
        public bool IsPredefined { get; set; }
        public bool IsCompositionScheme { get; set; }
    }
}