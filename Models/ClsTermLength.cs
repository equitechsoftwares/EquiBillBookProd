using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTermLength")]
    public class ClsTermLength
    {
        [Key]
        public long TermLengthId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int OrderNo { get; set; }
        public int Months { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long CountryId { get; set; }
        public bool IsChecked { get; set; }
    }

    public class ClsTermLengthVm
    {
        public long TermLengthId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int OrderNo { get; set; }
        public int Months { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long CountryId { get; set; }
        public bool IsChecked { get; set; }
        public long Under { get; set; }
        public string Domain { get; set; }
    }

}