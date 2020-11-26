using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_A.Data;
using Shopping_A.Enumeration;
using Shopping_A.Models;

namespace Shopping_A.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTransactionsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ProductTransactionsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/ProductTransactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductTransaction>>> GetProductTransactions()
        {
            return await _context.ProductTransactions.ToListAsync();
        }

        [HttpGet("searchProductTransaction")]
        public async Task<ActionResult<IEnumerable<ProductTransaction>>> GetProductTransactions(string productName, DateTime? dateFrom, DateTime? dateTo)
        {
            var productTransactions = await (from t in _context.ProductTransactions
                                             join p in _context.Products on t.ProductionID equals p.ProductID
                                             where (p.ProductName.Contains(productName) || string.IsNullOrEmpty(productName)) &&
                                             ((dateFrom.HasValue && dateTo.HasValue && t.TransactionDate >= dateFrom && t.TransactionDate <= dateTo) ||
                                              (!dateFrom.HasValue && dateTo.HasValue && t.TransactionDate <= dateTo.Value) ||
                                              (dateFrom.HasValue && !dateTo.HasValue && t.TransactionDate >= dateFrom.Value) ||
                                              (!dateFrom.HasValue && !dateTo.HasValue))
                                             select t
                                        ).ToListAsync();
            return productTransactions;

        }

        // GET: api/ProductTransactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductTransaction>> GetProductTransaction(int id)
        {
            var productTransaction = await _context.ProductTransactions.FindAsync(id);

            if (productTransaction == null)
            {
                return NotFound();
            }

            return productTransaction;
        }

        // PUT: api/ProductTransactions/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductTransaction(int id, ProductTransaction productTransaction)
        {
            if (id != productTransaction.ProductionTransactionID)
            {
                return BadRequest();
            }

            _context.Entry(productTransaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductTransactions
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<ProductTransaction>> PostProductTransaction(ProductTransaction productTransaction)
        {
            if (productTransaction.ProductionID == 0 || string.IsNullOrEmpty(productTransaction.Action))
                return BadRequest();
            var productToUpdate = _context.Products.Where(x => x.ProductID == productTransaction.ProductionID).FirstOrDefault();
            if (productToUpdate == null)
                return BadRequest("Product not found.");
            if (productTransaction.Action == Enum.GetName(typeof(ProductTransactionAction), ProductTransactionAction.REMOVE)
               || productTransaction.Action == Enum.GetName(typeof(ProductTransactionAction), ProductTransactionAction.SALE))
            {
                if (productToUpdate.Quantity - productTransaction.Quantity < 0)
                {
                    return BadRequest($"No enough product. Product : {productToUpdate.ProductName} remaining {productToUpdate.Quantity}");
                }
                productToUpdate.Quantity -= productTransaction.Quantity;
            }
            else
            {
                productToUpdate.Quantity += productTransaction.Quantity;
            }
            _context.Entry(productToUpdate).State = EntityState.Modified;
            _context.ProductTransactions.Add(productTransaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductTransaction", new { id = productTransaction.ProductionTransactionID }, productTransaction);
        }

        // DELETE: api/ProductTransactions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ProductTransaction>> DeleteProductTransaction(int id)
        {
            var productTransaction = await _context.ProductTransactions.FindAsync(id);
            if (productTransaction == null)
            {
                return NotFound();
            }

            _context.ProductTransactions.Remove(productTransaction);
            await _context.SaveChangesAsync();

            return productTransaction;
        }

        private bool ProductTransactionExists(int id)
        {
            return _context.ProductTransactions.Any(e => e.ProductionTransactionID == id);
        }
    }
}
