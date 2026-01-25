using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCountryTaxTypeMap")]
    public class ClsCountryTaxTypeMap
    {
        [Key]
        public long CountryTaxTypeMapId { get; set; }
        public long CountryId { get; set; }
        public long TaxTypeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}