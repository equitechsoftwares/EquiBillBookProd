using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblStockTransferDeductionId")]
    public class ClsStockTransferDeductionId
    {
        [Key]
        public long StockTransferDeductionId { get; set; }
        public long StockTransferId { get; set; }
        public long StockTransferDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsStockTransferDeductionIdVm
    {
        public long StockTransferDeductionId { get; set; }
        public long StockTransferId { get; set; }
        public long StockTransferDetailsId { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long CompanyId { get; set; }
    }

}