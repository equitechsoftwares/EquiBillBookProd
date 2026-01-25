using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblExceptionLogger")]
    public class ClsExceptionLogger
    {
        [Key]
        public long ExceptionLoggerId { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public string Uri { get; set; }
        public string RequestJson { get; set; }
        public DateTime AddedOn { get; set; }
        public string InnerException { get; set; }
    }
}