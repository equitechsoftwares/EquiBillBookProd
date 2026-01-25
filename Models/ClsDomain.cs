using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblDomain")]
    public class ClsDomain
    {
        [Key]
        public long DomainId { get; set; }
        public string Domain { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsDomainVm
    {
        public long DomainId { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long Under { get; set; }
        public long CountryId { get; set; }
        public string UserType { get; set; }
        public long UserId { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}