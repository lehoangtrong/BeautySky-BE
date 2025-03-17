using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using Microsoft.AspNetCore.Authorization;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public OrdersController(ProjectSwpContext context)
        {
            _context = context;
        }




        [HttpGet("orders/myOrders")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Invalid token or missing userId claim.");
            }

            var userId = int.Parse(userIdClaim);

            var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
            if (!orders.Any()) return NotFound("No orders found for this user");

            return Ok(orders);
        }



        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderProducts)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.UserId,
                    o.PromotionId,
                    o.DiscountAmount,
                    o.TotalAmount,
                    o.FinalAmount,
                    o.Status,
                    Products = o.OrderProducts.Select(op => new
                    {
                        op.ProductId,
                        ProductName = _context.Products.Where(p => p.ProductId == op.ProductId).Select(p => p.ProductName).FirstOrDefault(),
                        op.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .Where(o => o.OrderId == id)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.UserId,
                    o.PromotionId,
                    o.DiscountAmount, 
                    o.TotalAmount,
                    o.FinalAmount,
                    o.Status,
                    Products = o.OrderProducts.Select(op => new
                    {
                        op.ProductId,
                        ProductName = _context.Products.Where(p => p.ProductId == op.ProductId).Select(p => p.ProductName).FirstOrDefault(),
                        op.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }
            return order;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal? discountAmount = null;
                if (request.PromotionID.HasValue)
                {
                    var promotion = await _context.Promotions.FindAsync(request.PromotionID);
                    if (promotion != null)
                    {
                        discountAmount = promotion.DiscountPercentage / 100 * request.OrderProducts.Sum(p => p.Quantity * _context.Products.Find(p.ProductID).Price);
                    }
                }

                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token or missing userId claim.");
                }

                int userId = int.Parse(userIdClaim);

                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    UserId = userId,
                    PromotionId = request.PromotionID,
                    DiscountAmount = discountAmount,
                    Status = "Pending"
                };

                var orderProducts = new List<OrderProduct>();
                decimal totalAmount = 0;

                foreach (var productRequest in request.OrderProducts)
                {
                    var product = await _context.Products.FindAsync(productRequest.ProductID);
                    if (product == null)
                    {
                        return NotFound($"Product with ID {productRequest.ProductID} not found.");
                    }

                    var unitPrice = product.Price;
                    var totalPrice = productRequest.Quantity * unitPrice;

                    orderProducts.Add(new OrderProduct
                    {
                        ProductId = productRequest.ProductID,
                        Quantity = productRequest.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice
                    });

                    totalAmount += totalPrice;
                }

                order.TotalAmount = totalAmount;
                order.FinalAmount = totalAmount - (discountAmount ?? 0);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var orderProduct in orderProducts)
                {
                    orderProduct.OrderId = order.OrderId;
                }

                _context.OrderProducts.AddRange(orderProducts);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create order");
            }
        }
        // PUT: api/Orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderRequest request)
        {
            var order = await _context.Orders.Include(o => o.OrderProducts).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal? discountAmount = null;
                if (request.PromotionID.HasValue)
                {
                    var promotion = await _context.Promotions.FindAsync(request.PromotionID);
                    if (promotion != null)
                    {
                        discountAmount = promotion.DiscountPercentage / 100 * request.OrderProducts.Sum(p => p.Quantity * _context.Products.Find(p.ProductID).Price);
                    }
                }

                order.PromotionId = request.PromotionID;
                order.DiscountAmount = discountAmount;

                _context.OrderProducts.RemoveRange(order.OrderProducts);

                var orderProducts = new List<OrderProduct>();
                decimal totalAmount = 0;

                foreach (var productRequest in request.OrderProducts)
                {
                    var product = await _context.Products.FindAsync(productRequest.ProductID);
                    if (product == null)
                    {
                        return NotFound($"Product with ID {productRequest.ProductID} not found.");
                    }

                    var unitPrice = product.Price;
                    var totalPrice = productRequest.Quantity * unitPrice;

                    orderProducts.Add(new OrderProduct
                    {
                        OrderId = order.OrderId,
                        ProductId = productRequest.ProductID,
                        Quantity = productRequest.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice
                    });

                    totalAmount += totalPrice;
                }

                order.TotalAmount = totalAmount;
                order.FinalAmount = totalAmount - (discountAmount ?? 0);

                _context.OrderProducts.AddRange(orderProducts);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update order");
            }
        }

        // DELETE: api/Orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderProducts).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.OrderProducts.RemoveRange(order.OrderProducts);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class OrderRequest
    {
        public int? UserID { get; set; }
        public int? PromotionID { get; set; }
        public List<OrderProductRequest> OrderProducts { get; set; }
    }

    public class OrderProductRequest
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}
