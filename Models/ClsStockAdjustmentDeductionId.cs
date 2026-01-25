using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblStockAdjustmentDeductionId")]
    public class ClsStockAdjustmentDeductionId
    {
        [Key]
        public long StockAdjustmentDeductionId { get; set; }
        public long StockAdjustmentId { get; set; }
        public long StockAdjustmentDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsStockAdjustmentDeductionIdVm
    {
        public long StockAdjustmentDeductionId { get; set; }
        public long StockAdjustmentId { get; set; }
        public long StockAdjustmentDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

}