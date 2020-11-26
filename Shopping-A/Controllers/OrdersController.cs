using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_A.Data;
using Shopping_A.Models;
using Shopping_A.Models.Pagination;

namespace Shopping_A.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public OrdersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(string id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders
        [HttpGet("searchOrder")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(string orderId, int? customerId)
        {
            return await _context.Orders.Where(order =>
            (order.OrderID.Contains(orderId) || string.IsNullOrEmpty(orderId))
            && (order.CustomerID == customerId || !customerId.HasValue || customerId == 0)
            ).ToListAsync();
        }

        // GET: api/Orders
        [HttpGet("searchOrderTransaction")]
        public async Task<ActionResult<PagedResponse<List<Order>>>> SearchOrderTransaction(string productName, string customerName, DateTime? dateFrom, DateTime? dateTo, [FromQuery] PaginationFilter filter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var orderTransactions = await (from o in _context.Orders
                                           join ot in _context.OrderDetails on o.OrderID equals ot.OrderID
                                           join c in _context.Customers on o.CustomerID equals c.CustomerID
                                           join p in _context.Products on ot.ProductID equals p.ProductID
                                           where (string.IsNullOrEmpty(productName) || p.ProductName.Contains(productName)) &&
                                           (string.IsNullOrEmpty(customerName) || (c.FirstName + " " + c.MiddleName + " " + c.LastName).Contains(customerName)) &&
                                            ((dateFrom.HasValue && dateTo.HasValue && o.OrderDate >= dateFrom && o.OrderDate <= dateTo) ||
                                              (!dateFrom.HasValue && dateTo.HasValue && o.OrderDate <= dateTo.Value) ||
                                              (dateFrom.HasValue && !dateTo.HasValue && o.OrderDate >= dateFrom.Value) ||
                                              (!dateFrom.HasValue && !dateTo.HasValue))
                                           select o).Distinct().OrderBy(x => x.OrderID)
                                           .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                           .Take(validFilter.PageSize)
                                           .ToListAsync();
            int totalRecords = await (from o in _context.Orders
                                      join ot in _context.OrderDetails on o.OrderID equals ot.OrderID
                                      join c in _context.Customers on o.CustomerID equals c.CustomerID
                                      join p in _context.Products on ot.ProductID equals p.ProductID
                                      where (string.IsNullOrEmpty(productName) || p.ProductName.Contains(productName)) &&
                                      (string.IsNullOrEmpty(customerName) || (c.FirstName + " " + c.MiddleName + " " + c.LastName).Contains(customerName)) &&
                                       ((dateFrom.HasValue && dateTo.HasValue && o.OrderDate >= dateFrom && o.OrderDate <= dateTo) ||
                                         (!dateFrom.HasValue && dateTo.HasValue && o.OrderDate <= dateTo.Value) ||
                                         (dateFrom.HasValue && !dateTo.HasValue && o.OrderDate >= dateFrom.Value) ||
                                         (!dateFrom.HasValue && !dateTo.HasValue))
                                      select o).Distinct().CountAsync();
            PagedResponse<List<Order>> result = new PagedResponse<List<Order>>(orderTransactions, validFilter.PageNumber, validFilter.PageSize)
            {
                TotalRecords = totalRecords,
                TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(totalRecords / validFilter.PageSize)))
            };
            return result;

        }


        // PUT: api/Orders/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(string id, Order order)
        {
            if (id != order.OrderID)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        // POST: api/Orders
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OrderExists(order.OrderID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOrder", new { id = order.OrderID }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
