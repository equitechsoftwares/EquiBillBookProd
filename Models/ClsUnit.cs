using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblUnit")]
    public class ClsUnit
    {
        [Key]
        public long UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public bool AllowDecimal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsPredefined { get; set; }
    }

    public class ClsUnitVm
    {
        public long UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public bool AllowDecimal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public bool IsPredefined { get; set; }
        public string UnitType { get; set; }
    }

}