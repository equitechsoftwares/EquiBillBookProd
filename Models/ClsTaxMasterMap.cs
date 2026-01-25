using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTaxMasterMap")]
    public class ClsTaxMasterMap
    {
        [Key]
        public long TaxMasterMapId { get; set; }
        public long TaxMasterId { get; set; }
        public long SubTaxMasterId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}