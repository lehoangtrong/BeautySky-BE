using BeautySky.Library;
using BeautySky.Models.Vnpay;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BeautySky.Services.Vnpay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
                var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                var pay = new VnPayLibrary();

                // Chuẩn bị số tiền: làm tròn để tránh phần thập phân
                long amountInVnd = (long)Math.Round(model.Amount * 100, 0);

                // Thêm các tham số yêu cầu
                pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
                pay.AddRequestData("vnp_Command", "pay");
                pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
                pay.AddRequestData("vnp_Amount", amountInVnd.ToString());
                pay.AddRequestData("vnp_BankCode", "");
                pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", "VND");
                pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
                pay.AddRequestData("vnp_Locale", "vn");
                pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang: {model.OrderId}");
                pay.AddRequestData("vnp_OrderType", "other");
                pay.AddRequestData("vnp_ReturnUrl", _configuration["Vnpay:PaymentBackReturnUrl"]);
                pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());

                // Tạo URL thanh toán
                var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

                // Log để debug
                _logger.LogInformation("Payment URL created: {PaymentUrl}", paymentUrl);

                return paymentUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo URL thanh toán VNPay");
                throw;
            }
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            return response;
        }
    }

    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}