using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblOtherSoftwareImport")]
    public class ClsOtherSoftwareImport
    {
        [Key]
        public long OtherSoftwareImportId { get; set; }
        public string Type { get; set; }
        public string UploadPath { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string ImportNo { get; set; }
    }

    public class ClsOtherSoftwareImportVm
    {
        public long OtherSoftwareImportId { get; set; }
        public string Type { get; set; }
        public string UploadPath { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string FileExtension{ get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public string AddedByCode { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string ImportNo { get; set; }
    }

}