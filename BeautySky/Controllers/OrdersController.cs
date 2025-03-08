using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BeautySky.Models;
using Azure.Core;
using BeautySky.Models.Vnpay;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var orders = await _context.Orders
                .Include(o => o.OrderProducts)
                .Select(o => new
                {
                    o.OrderId,
                    o.UserId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.PromotionId,
                    o.DiscountAmount,
                    o.FinalAmount,
                    o.Status,
                    OrderProducts = o.OrderProducts.Select(op => new
                    {
                        op.ProductId,
                        op.Quantity,
                        op.UnitPrice,
                        op.TotalPrice
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }
        [HttpGet("in-cart")]
        [Authorize]
        public async Task<IActionResult> GetUserInCartOrder()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Không thể xác định UserId từ token");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("UserId không hợp lệ");
            }

            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .Where(o => o.UserId == userId && o.Status == "In Cart")
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    OrderProducts = o.OrderProducts.Select(op => new
                    {
                        op.ProductId,
                        op.Quantity,
                        op.UnitPrice,
                        op.TotalPrice
                    })
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng nào đang In Cart");
            }

            return Ok(order);
        }
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart(int userID, List<OrderProductRequest> products)
        {
            if (products == null || !products.Any())
            {
                return BadRequest("Danh sách sản phẩm trống");
            }

            decimal totalAmount = 0m;
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

            var order = new Order
            {
                UserId = userID,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "In Cart"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var orderProduct in orderProducts)
            {
                orderProduct.OrderId = order.OrderId;
                _context.OrderProducts.Add(orderProduct);
            }

            await _context.SaveChangesAsync();

            return Ok(new { order.OrderId, order.Status, totalAmount });
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(int orderId, int? promotionID)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Đơn hàng với ID {orderId} không tồn tại");
            }
            if (order.UserId != 1)
            {
                return Forbid("Người dùng không có quyền sử dụng mã khuyến mãi cho đơn hàng này");
            }

            decimal discountAmount = 0m;
            decimal finalAmount = order.TotalAmount ?? 0m;

            if (promotionID.HasValue)
            {
                var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == promotionID);
                if (promotion != null)
                {
                    discountAmount = finalAmount * (promotion.DiscountPercentage / 100);
                    finalAmount -= discountAmount;
                }
            }

            order.PromotionId = promotionID;
            order.DiscountAmount = discountAmount;
            order.FinalAmount = finalAmount;
            order.Status = "Pending";

            await _context.SaveChangesAsync();

            return Ok(new { order.OrderId, order.Status, order.TotalAmount, discountAmount, order.FinalAmount });
        }
        [HttpGet("Pending-orders")]
        public async Task<IActionResult> GetPaidOrders()
        {
            var paidOrders = await _context.Orders
                .Where(o => o.Status == "Pending")
                .ToListAsync();

            return Ok(paidOrders);
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

    }

    public class OrderProductRequest
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}

