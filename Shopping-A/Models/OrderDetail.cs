using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_A.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }
        public string OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PricePerUnit { get; set; }
    }
}
