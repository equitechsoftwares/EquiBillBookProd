using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblOtp")]
    public class ClsOtp
    {
        [Key]
        public long OtpId { get; set; }
        public string Otp { get; set; }
        public string EmailId { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class ClsOtpVm
    {
        public long OtpId { get; set; }
        public string Otp { get; set; }
        public string EmailId { get; set; }
        public DateTime AddedOn { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }
}