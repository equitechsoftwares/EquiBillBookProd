using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblUnitMaster")]
    public class ClsUnitMaster
    {
        [Key]
        public long UnitMasterId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public bool AllowDecimal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPredefined { get; set; }
    }

    public class ClsUnitMasterVm
    {
        public long UnitMasterId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public bool AllowDecimal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPredefined { get; set; }
    }

}