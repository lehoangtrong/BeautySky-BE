using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.Models.Vnpay;
using BeautySky.Services.Vnpay;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ProjectSwpContext context, IVnPayService vnPayService, ILogger<PaymentsController> logger)
        {
            _context = context;
            _vnPayService = vnPayService;
            _logger = logger;
        }

        [HttpPost("ProcessAndConfirmPayment/{orderId}")]
        public async Task<ActionResult<Payment>> ProcessAndConfirmPayment(int orderId)
        {
            _logger.LogInformation($"Processing payment for Order ID: {orderId}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found.");
                    return NotFound("Order not found.");
                }

                if (order.PaymentId != null)
                {
                    _logger.LogWarning($"Order {orderId} already has a payment.");
                    return BadRequest("Order already has a payment.");
                }

                if (order.UserId == null)
                {
                    _logger.LogWarning($"Order {orderId} has no associated User.");
                    return BadRequest("Order has no associated User.");
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
                _context.Entry(payment).GetDatabaseValues(); // Load PaymentId

                order.PaymentId = payment.PaymentId;
                order.Status = "Completed";

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation($"Payment {payment.PaymentId} processed successfully.");

                return Created($"api/Payments/{payment.PaymentId}", payment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error processing payment for Order ID {orderId}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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
        

  

            //[HttpPost]
            //public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
            //{
            //    var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            //    return Redirect(url);
            //}
            //[HttpGet]
            //public IActionResult PaymentCallbackVnpay()
            //{
            //    var response = _vnPayService.PaymentExecute(Request.Query);

            //    return Json(response);
            //}

            //private IActionResult Json(PaymentResponseModel response)
            //{
            //    throw new NotImplementedException();
            //}
        }
    }

