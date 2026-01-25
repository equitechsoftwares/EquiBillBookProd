using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSupportTicket")]
    public class ClsSupportTicket
    {
        [Key]
        public long SupportTicketId { get; set; }
        public string SupportTicketNo { get; set; }
        public int HelpTopic { get; set; }
        public string Subject { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Attachment1 { get; set; }
        public string Attachment2 { get; set; }
        public string Attachment3 { get; set; }
        public string Attachment4 { get; set; }
        public string Attachment5 { get; set; }
    }

    public class ClsSupportTicketVm
    {
        public long SupportTicketId { get; set; }
        public string SupportTicketNo { get; set; }
        public int HelpTopic { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Attachment1 { get; set; }
        public string Attachment2 { get; set; }
        public string Attachment3 { get; set; }
        public string Attachment4 { get; set; }
        public string Attachment5 { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public string Browser { get; set; }
        public string SupportTicket { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Attachment1Extension { get; set; }
        public string Attachment2Extension { get; set; }
        public string Attachment3Extension { get; set; }
        public string Attachment4Extension { get; set; }
        public string Attachment5Extension { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public List<ClsSupportTicketDetailsVm> SupportTicketDetails { get; set; }
        public long Under { get; set; }
        public string Domain { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
    }

}