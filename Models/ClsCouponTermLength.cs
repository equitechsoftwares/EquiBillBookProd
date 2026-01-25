using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCouponTermLength")]
    public class ClsCouponTermLength
    {
        [Key]
        public long CouponTermLengthId { get; set; }
        public long CouponId { get; set; }
        public long TermLengthId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class ClsCouponTermLengthVm
    {
        public long CouponTermLengthId { get; set; }
        public long CouponId { get; set; }
        public long TermLengthId { get; set; }
        public string TermLengthTitle { get; set; }
        public string Title { get { return TermLengthTitle; } set { TermLengthTitle = value; } }
        public int Months { get; set; }
        public bool IsSelected { get; set; }
    }
}

