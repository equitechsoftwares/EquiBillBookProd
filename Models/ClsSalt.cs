using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblSalt")]
    public class ClsSalt
    {
        [Key]
        public long SaltId { get; set; }
        public string SaltName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Indication { get; set; }
        public string Dosage { get; set; }
        public string SideEffects { get; set; }
        public string SpecialPrecautions { get; set; }
        public string DrugInteractions { get; set; }
        public string Notes { get; set; }
        public string TBItem { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsScheduleH { get; set; }
        public bool IsScheduleH1 { get; set; }
        public bool IsDiscontinued { get; set; }
        public bool IsProhibited { get; set; }
    }
    public class ClsSaltVm
    {
        public long SaltId { get; set; }
        public string SaltName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        [AllowHtml]
        public string Indication { get; set; }
        [AllowHtml]
        public string Dosage { get; set; }
        [AllowHtml]
        public string SideEffects { get; set; }
        [AllowHtml]
        public string SpecialPrecautions { get; set; }
        [AllowHtml]
        public string DrugInteractions { get; set; }
        [AllowHtml]
        public string Notes { get; set; }
        public string TBItem { get; set; }
        public bool IsNarcotic { get; set; }
        public bool IsScheduleH { get; set; }
        public bool IsScheduleH1 { get; set; }
        public bool IsDiscontinued { get; set; }
        public bool IsProhibited { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public long BranchId { get; set; }
    }
}