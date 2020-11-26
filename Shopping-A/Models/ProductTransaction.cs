using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_A.Models
{
    public class ProductTransaction
    {
        [Key]
        public int ProductionTransactionID { get; set; }
        public int ProductionID { get; set; }
        public string Action { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
