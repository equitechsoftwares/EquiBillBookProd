using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblAddress")]
    public class ClsAddress
    {
        [Key]
        public long AddressId { get; set; }
        public string Type { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public long CountryId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string Address { get; set; }
        public long Zipcode { get; set; }
        public string MobileNo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Locality { get; set; }
        public string Landmark { get; set; }
        public string MobileNo2 { get; set; }
    }

    public class ClsAddressVm
    {
        public long AddressId { get; set; }
        public string Type { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public long CountryId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string Address { get; set; }
        public long Zipcode { get; set; }
        public string MobileNo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Locality { get; set; }
        public string Landmark { get; set; }
        public string MobileNo2 { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Domain { get; set; }
    }

}