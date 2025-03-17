using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.Service.Vnpay;
using BeautySky.Models.Vnpay;
using System.Diagnostics;
using Azure;
using Microsoft.AspNetCore.Authorization;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly ProjectSwpContext _context;

        public PaymentsController(ProjectSwpContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }


        [HttpPost("create-payment")]
        [Authorize]
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationModel model)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized("userId không tồn tại trong token.");
            }
            int userId = int.Parse(userIdClaim.Value);
            model.UserId = userId;
            Debug.WriteLine($"Debug: orderType = {model.OrderType}, amount = {model.Amount}, userId = {model.UserId}");
            if (model == null || model.Amount <= 0 || string.IsNullOrEmpty(model.OrderType))
            {
                return BadRequest("Invalid payment information");
            }

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Ok(new { paymentUrl = url });
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Nếu thanh toán thành công, chuyển hướng đến trang thành công
            if (response.Success)
            {
                return Redirect("https://localhost:5173/paymentsuccess");
            }

            // Nếu thất bại, chuyển hướng đến trang thất bại
            return Redirect("https://localhost:5173/paymentfailed");
        }   





        // GET: api/Payments/Details/{paymentId}
        [HttpGet("Details/{paymentId}")]
        public async Task<ActionResult<object>> GetPaymentDetails(int paymentId)
        {
            var paymentDetails = await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .Select(p => new
                {
                    p.PaymentId,
                    p.PaymentDate,
                    PaymentType = _context.PaymentTypes
                        .Where(pt => pt.PaymentTypeId == p.PaymentTypeId)
                        .Select(pt => pt.PaymentTypeName)
                        .FirstOrDefault(),
                    PaymentStatus = _context.PaymentStatuses
                        .Where(ps => ps.PaymentStatusId == p.PaymentStatusId)
                        .Select(ps => ps.PaymentStatus1)
                        .FirstOrDefault(),
                    Order = _context.Orders
                        .Where(o => o.PaymentId == p.PaymentId)
                        .Select(o => new
                        {
                            o.OrderId,
                            o.OrderDate,
                            o.FinalAmount,
                            o.Status
                        }).FirstOrDefault(),
                    User = _context.Users
                        .Where(u => u.UserId == p.UserId)
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName,
                            u.FullName,
                            u.Email,
                            u.Phone,
                            u.Address,
                            u.DateCreate,
                            u.IsActive
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (paymentDetails == null)
            {
                return NotFound("Payment details not found.");
            }

            return Ok(paymentDetails);
        }

        // GET: api/Payments/AllDetails
        [HttpGet("AllDetails")]
        public async Task<ActionResult<object>> GetAllPaymentDetails()
        {
            var allPaymentDetails = await _context.Payments
                .Select(p => new
                {
                    p.PaymentId,
                    p.PaymentDate,
                    PaymentType = _context.PaymentTypes
                        .Where(pt => pt.PaymentTypeId == p.PaymentTypeId)
                        .Select(pt => pt.PaymentTypeName)
                        .FirstOrDefault(),
                    PaymentStatus = _context.PaymentStatuses
                        .Where(ps => ps.PaymentStatusId == p.PaymentStatusId)
                        .Select(ps => ps.PaymentStatus1)
                        .FirstOrDefault(),
                    Order = _context.Orders
                        .Where(o => o.PaymentId == p.PaymentId)
                        .Select(o => new
                        {
                            o.OrderId,
                            o.OrderDate,
                            o.FinalAmount,
                            o.Status
                        }).FirstOrDefault(),
                    User = _context.Users
                        .Where(u => u.UserId == p.UserId)
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName,
                            u.FullName,
                            u.Email,
                            u.Phone,
                            u.Address,
                            u.DateCreate,
                            u.IsActive
                        }).FirstOrDefault()
                }).ToListAsync();

            if (!allPaymentDetails.Any())
            {
                return NotFound("No payment details found.");
            }

            return Ok(allPaymentDetails);
        }
        // POST: api/Payments/ProcessAndConfirmPayment/{orderId}
        [HttpPost("ProcessAndConfirmPayment/{orderId}")]
        public async Task<ActionResult<Payment>> ProcessAndConfirmPayment(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var payment = new Payment
            {
                UserId = order.UserId,
                PaymentTypeId = 1, // Default payment type
                PaymentStatusId = 2, // Confirmed status
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            order.PaymentId = payment.PaymentId;
            order.Status = "Paid";

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPayment", new { id = payment.PaymentId }, payment);
        }


        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }

    }
}
