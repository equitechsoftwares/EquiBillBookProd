using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPwaSettings")]
    public class ClsPwaSettings
    {
        [Key]
        public long PwaSettingsId { get; set; }
        public long DomainId { get; set; }
        public string PwaName { get; set; }
        public string PwaShortName { get; set; }
        public string BackgroundColor { get; set; }
        public string ThemeColor { get; set; }
        public string Image_48_48 { get; set; }
        public string Image_72_72 { get; set; }
        public string Image_96_96 { get; set; }
        public string Image_128_128 { get; set; }
        public string Image_144_144 { get; set; }
        public string Image_152_152 { get; set; }
        public string Image_192_192 { get; set; }
        public string Image_284_284 { get; set; }
        public string Image_512_512 { get; set; }
        public string Manifest { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsPwaSettingsVm
    {
        public long PwaSettingsId { get; set; }
        public long DomainId { get; set; }
        public string PwaName { get; set; }
        public string PwaShortName { get; set; }
        public string BackgroundColor { get; set; }
        public string ThemeColor { get; set; }
        public string Image_48_48 { get; set; }
        public string Image_72_72 { get; set; }
        public string Image_96_96 { get; set; }
        public string Image_128_128 { get; set; }
        public string Image_144_144 { get; set; }
        public string Image_152_152 { get; set; }
        public string Image_192_192 { get; set; }
        public string Image_284_284 { get; set; }
        public string Image_512_512 { get; set; }
        public string Manifest { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string FileExtensionImage_48_48 { get; set; }
        public string FileExtensionImage_72_72 { get; set; }
        public string FileExtensionImage_96_96 { get; set; }
        public string FileExtensionImage_128_128 { get; set; }
        public string FileExtensionImage_144_144 { get; set; }
        public string FileExtensionImage_152_152 { get; set; }
        public string FileExtensionImage_192_192 { get; set; }
        public string FileExtensionImage_284_284 { get; set; }
        public string FileExtensionImage_512_512 { get; set; }
        public string Domain { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
    }

}