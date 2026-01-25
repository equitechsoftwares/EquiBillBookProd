using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblOtherLog")]
    public class ClsOtherLog
    {
        [Key]
        public long OtherLogId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public long Id { get; set; }
        public string Description { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsOtherLogVm
    {
        public long OtherLogId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public long Id { get; set; }
        public string Description { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public long CompanyId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long BusinessId { get; set; }
        public long BranchId { get; set; }
        public long UserId { get; set; }
        public string Domain { get; set; }
    }
}