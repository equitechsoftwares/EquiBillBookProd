using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblOnlinePaymentSettings")]
    public class ClsOnlinePaymentSettings
    {
        [Key]
        public long OnlinePaymentSettingsId { get; set; }
        public int OnlinePaymentService { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string RazorpayKey { get; set; }
        public long RazorpayCurrencyId { get; set; }
        public string PaypalClientId { get; set; }
        public long PaypalCurrencyId { get; set; }
        public bool IsDefault { get; set; }
        public string SaveAs { get; set; }
    }

    public class ClsOnlinePaymentSettingsVm
    {
        public long OnlinePaymentSettingsId { get; set; }
        public int OnlinePaymentService { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string RazorpayKey { get; set; }
        public long RazorpayCurrencyId { get; set; }
        public string PaypalClientId { get; set; }
        public long PaypalCurrencyId { get; set; }
        public string PaypalCurrencyCode { get; set; }
        public string RazorpayCurrencyCode { get; set; }
        public bool IsDefault { get; set; }
        public string SaveAs { get; set; }
        public long Under { get; set; }
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