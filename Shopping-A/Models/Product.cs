using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_A.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set;}
        public string ProductName { get; set;}
        public int CategoryID { get; set;}
        public string Unit { get; set;}
        public decimal PricePerUnit { get; set;}
        public int Quantity { get; set;}
    }
}
