using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSecondaryUnit")]
    public class ClsSecondaryUnit
    {
        [Key]
        public long SecondaryUnitId { get; set; }
        public long UnitId { get; set; }
        public string SecondaryUnitCode { get; set; }
        public string SecondaryUnitName { get; set; }
        public string SecondaryUnitShortName { get; set; }
        public bool SecondaryUnitAllowDecimal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsSecondaryUnitVm
    {
        public long SecondaryUnitId { get; set; }
        public long UnitId { get; set; }
        public string SecondaryUnitCode { get; set; }
        public string SecondaryUnitName { get; set; }
        public string SecondaryUnitShortName { get; set; }
        public bool SecondaryUnitAllowDecimal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Title { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Search { get; set; }
        public string UnitName { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string Domain { get; set; }
    }

}