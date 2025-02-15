using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Linq;
using BeautySky.Models;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayPaymentController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ProjectSwpContext _context;

        public VNPayPaymentController(IConfiguration config, ProjectSwpContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("create-payment")]
        public IActionResult CreatePayment(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound("Đơn hàng không tồn tại");
            }

            decimal amount = order.FinalAmount ?? 0;

            string vnp_TmnCode = _config["VNPayConfig:TmnCode"];
            string vnp_HashSecret = _config["VNPayConfig:HashSecret"];
            string vnp_Url = _config["VNPayConfig:VnpUrl"];
            string vnp_ReturnUrl = _config["VNPayConfig:ReturnUrl"];

            var vnpayData = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Amount", ((int)(amount * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan don hang {orderId}" },
                { "vnp_OrderType", "billpayment" },
                { "vnp_ReturnUrl", vnp_ReturnUrl },
                { "vnp_TxnRef", orderId.ToString() }
            };

            string queryString = string.Join("&", vnpayData.Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));
            string hashData = string.Join("&", vnpayData.Select(kv => $"{kv.Key}={kv.Value}"));

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData));
                string signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                queryString += $"&vnp_SecureHash={signature}";
            }

            string paymentUrl = $"{vnp_Url}?{queryString}";

            return Ok(new { PaymentUrl = paymentUrl });
        }

        [HttpGet("vnpay-return")]
        public IActionResult VNPayReturn([FromQuery] Dictionary<string, string> vnpayResponse)
        {
            string vnp_HashSecret = _config["VNPayConfig:HashSecret"];

            string receivedHash = vnpayResponse["vnp_SecureHash"];
            vnpayResponse.Remove("vnp_SecureHash");

            string hashData = string.Join("&", vnpayResponse.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData));
                string expectedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                if (receivedHash == expectedHash)
                {
                    if (vnpayResponse["vnp_ResponseCode"] == "00")
                    {
                        return Ok("Thanh toán thành công");
                    }
                    return BadRequest("Thanh toán thất bại");
                }
            }
            return BadRequest("Chữ ký không hợp lệ");
        }
    }
}
