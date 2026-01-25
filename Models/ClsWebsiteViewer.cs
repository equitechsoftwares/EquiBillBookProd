using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblWebsiteViewer")]
    public class ClsWebsiteViewer
    {
        [Key]
        public long WebsiteViewerId { get; set; }
        public string IpAddress { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long AddedBy { get; set; }
        public long ModifiedBy { get; set; }
        public bool IsBlocked { get; set; }
        public long Under { get; set; }
    }
}