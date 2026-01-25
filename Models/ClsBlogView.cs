using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblBlogView")]
    public class ClsBlogView
    {
        [Key]
        public long BlogViewId { get; set; }
        public long BlogId { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public DateTime ViewedOn { get; set; }
    }
}

