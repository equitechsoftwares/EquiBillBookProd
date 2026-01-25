using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblUserCurrencyMap")]
    public class ClsUserCurrencyMap
    {
        [Key]
        public long UserCurrencyMapId { get; set; }
        public long CurrencyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsMain { get; set; }
        public decimal ExchangeRate { get; set; }
    }

    public class ClsUserCurrencyMapVm
    {
        public long UserCurrencyMapId { get; set; }
        public long CurrencyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsMain { get; set; }
        public decimal ExchangeRate { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}