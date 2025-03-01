//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using BeautySky.Models;
//using BeautySky.Library;
//using BeautySky.Models.Vnpay;
//using BeautySky.Services.Vnpay;

//namespace BeautySky.Controllers
//{
//    [Route("api/payments")]
//    [ApiController]
//    public class PaymentsController : ControllerBase
//    {
//        private readonly IVnPayService _vnPayService;

//        public PaymentsController(IVnPayService vnPayService)
//        {
//            _vnPayService = vnPayService;
//        }

//        [HttpPost("create-payment-url")]
//        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
//        {
//            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
//            return Ok(new { paymentUrl = url });
//        }

//        [HttpGet("callback")]
//        public IActionResult PaymentCallback()
//        {
//            var response = _vnPayService.PaymentExecute(Request.Query);
//            return Ok(response);
//        }
//    }
//}