using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblForgotPassword")]
    public class ClsForgotPassword
    {
        [Key]
        public long ForgotPasswordId { get; set; }
        public string EmailId { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsBlocked { get; set; }
        public string Token { get; set; }
    }

    public class ClsForgotPasswordVm
    {
        public long ForgotPasswordId { get; set; }
        public string EmailId { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsBlocked { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string CPassword { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public long CompanyId { get; set; }
        public string UserType { get; set; }
        public string Domain { get; set; }
    }
}