using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCouponAddon")]
    public class ClsCouponAddon
    {
        [Key]
        public long CouponAddonId { get; set; }
        public long CouponId { get; set; }
        public long PlanAddonsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class ClsCouponAddonVm
    {
        public long CouponAddonId { get; set; }
        public long CouponId { get; set; }
        public long PlanAddonsId { get; set; }
        public string AddonTitle { get; set; }
        public string Title { get { return AddonTitle; } set { AddonTitle = value; } }
        public string AddonType { get; set; }
        public bool IsSelected { get; set; }
    }
}

