using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesDeductionId")]
    public class ClsSalesDeductionId
    {
        [Key]
        public long SalesDeductionId { get; set; }
        public long SalesId { get; set; }
        public long SalesDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsSalesDeductionIdVm
    {
        public long SalesDeductionId { get; set; }
        public long SalesId { get; set; }
        public long SalesDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

}