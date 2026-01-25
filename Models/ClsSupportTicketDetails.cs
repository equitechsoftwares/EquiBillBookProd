using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSupportTicketDetails")]
    public class ClsSupportTicketDetails
    {
        [Key]
        public long SupportTicketDetailsId { get; set; }
        public long SupportTicketId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public bool IsFromCompany { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsSupportTicketDetailsVm
    {
        public long SupportTicketDetailsId { get; set; }
        public long SupportTicketId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public bool IsFromCompany { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Domain { get; set; }
        public long Under { get; set; }
    }

}