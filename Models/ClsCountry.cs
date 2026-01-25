using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCountry")]
    public class ClsCountry
    {
        [Key]
        public long CountryId { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal ConversionRate { get; set; }
        public string DialingCode { get; set; }
    }
    public class ClsCountryVm
    {
        public long CountryId { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal ConversionRate { get; set; }
        public string DialingCode { get; set; }
        public List<ClsCountryCurrencyMapVm> CountryCurrencyMaps { get; set; }
        public List<ClsCountryTimeZoneMap> CountryTimeZoneMaps { get; set; }
        public List<ClsCurrencyVm> Currencys { get; set; }
        public List<ClsTimeZoneVm> TimeZones { get; set; }
        public List<ClsUserCountryMapVm> UserCountryMaps { get; set; }
        public bool IsSelected { get; set; }
        public long UserCountryMapId { get; set; }
        public bool IsMain { get; set; }
        public long Under { get; set; }
        public string Domain { get; set; }
        public decimal PriceHikePercentage { get; set; }
    }
}