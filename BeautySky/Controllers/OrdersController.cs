using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders.Include(o => o.OrderProducts).ToListAsync();
            return Ok(orders);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest("ID đơn hàng không khớp");
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(o => o.OrderId == id))
                {
                    return NotFound("Đơn hàng không tồn tại");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderProducts).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound("Đơn hàng không tồn tại");
            }

            _context.OrderProducts.RemoveRange(order.OrderProducts);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("order-products")]
        public async Task<IActionResult> CreateOrder(int userID, int? promotionID, List<OrderProductRequest> products)
        {
            if (products == null || !products.Any())
            {
                return BadRequest("Danh sách sản phẩm trống");
            }

            var totalAmount = 0m;
            var orderProducts = new List<OrderProduct>();

            foreach (var item in products)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductID);
                if (product == null)
                {
                    return NotFound($"Sản phẩm với ID {item.ProductID} không tồn tại");
                }

                var itemTotal = product.Price * item.Quantity;
                totalAmount += itemTotal;

                orderProducts.Add(new OrderProduct
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = itemTotal
                });
            }

            decimal discountAmount = 0m;
            if (promotionID.HasValue)
            {
                var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == promotionID);
                if (promotion != null)
                {
                    discountAmount = totalAmount * (promotion.DiscountPercentage / 100);
                }
            }

            var finalAmount = totalAmount - discountAmount;

            var order = new Order
            {
                UserId = userID,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                PromotionId = promotionID,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var orderProduct in orderProducts)
            {
                orderProduct.OrderId = order.OrderId;
                _context.OrderProducts.Add(orderProduct);
            }

            await _context.SaveChangesAsync();

            return Ok(new { order.OrderId, order.Status, totalAmount, discountAmount, finalAmount });
        }
    }

    public class OrderProductRequest
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}