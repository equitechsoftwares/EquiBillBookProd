using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblLoginDetails")]
    public class ClsLoginDetails
    {
        [Key]
        public long LoginDetailsId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string UserType { get; set; }
        public bool IsLoggedOut { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string PushID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string IpAddress { get; set; }
        public string Token { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Browser { get; set; }
    }

    public class ClsLoginDetailsVM
    {
        public long LoginDetailsId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string UserType { get; set; }
        public bool IsLoggedOut { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string PushID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string IpAddress { get; set; }
        public string Token { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long VendorId { get; set; }
        public long ClientId { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public string RoleName { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Browser { get; set; }
    }
}