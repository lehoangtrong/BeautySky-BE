using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;

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

        // GET: api/Orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .FirstOrDefaultAsync(o => o.OrderId == id);

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

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    UserId = userId != null ? int.Parse(userId) : request.UserID,
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
