using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblHeader")]
    public class ClsHeader
    {
        [Key]
        public long HeaderId { get; set; }
        public string HeaderName { get; set; }
        public string HeaderType { get; set; }
        public long Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsHeaderVm
    {
        public long HeaderId { get; set; }
        public string HeaderName { get; set; }
        public string HeaderType { get; set; }
        public long Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public List<ClsMenuVm> Menus{ get; set; }
    }

}