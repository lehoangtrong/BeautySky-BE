using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.Library;
using System.Text.Json;
using BeautySky.Models.Vnpay;
using BeautySky.Services.Vnpay;

namespace BeautySky.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly ProjectSwpContext _context;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentsController(IVnPayService vnPayService,
            ProjectSwpContext context,
            ILogger<PaymentsController> logger,
            IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            try
            {
                var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
                if (url != null)
                    return Ok(url);
                else return BadRequest(url);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("callback")]
        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Ok(response);
        }
    }
}