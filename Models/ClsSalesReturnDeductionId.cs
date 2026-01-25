using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSalesReturnDeductionId")]
    public class ClsSalesReturnDeductionId
    {
        [Key]
        public long SalesReturnDeductionId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesReturnDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsSalesReturnDeductionIdVm
    {
        public long SalesReturnDeductionId { get; set; }
        public long SalesReturnId { get; set; }
        public long SalesReturnDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

}