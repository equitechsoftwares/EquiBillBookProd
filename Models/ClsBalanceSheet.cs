using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsBalanceSheet
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance { get; set; }
        public string Type { get; set; }
        public int OrderNo { get; set; }
        public bool IsDebit { get; set; }
        public List<ClsBalanceSheetDetails> BalanceSheetDetails { get; set; }
    }

    public class ClsBalanceSheetDetails
    {
        public string Name { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance { get; set; }

    }
}