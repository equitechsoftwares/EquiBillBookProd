using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblUserCountryMap")]
    public class ClsUserCountryMap
    {
        [Key]
        public long UserCountryMapId { get; set; }
        public long CountryId { get; set; }
        public decimal PriceHikePercentage { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsMain { get; set; }
    }

    public class ClsUserCountryMapVm
    {
        public long UserCountryMapId { get; set; }
        public long CountryId { get; set; }
        public decimal PriceHikePercentage { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsMain { get; set; }
        public string Domain { get; set; }
        public long Under { get; set; }
        public long DivId { get; set; }
    }

}