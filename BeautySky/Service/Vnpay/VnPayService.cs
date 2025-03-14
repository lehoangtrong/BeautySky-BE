using BeautySky.Library;
using BeautySky.Models.Vnpay;


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
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);


            var transactionId = $"{model.UserId}{DateTime.Now.Ticks}";
            _logger.LogInformation($"Generated TransactionId: {transactionId}");

            var pay = new VnPayLibrary();
            var returnUrl = _configuration["Vnpay:ReturnUrl"];
            _logger.LogInformation($"Callback URL: {returnUrl}");

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);

            // Log giá trị Amount
            _logger.LogInformation($"Original Amount: {model.Amount}");
            decimal vndAmount = model.Amount * 24500m;
            _logger.LogInformation($"VND Amount: {vndAmount}");
            vndAmount = Math.Round(vndAmount, 0);
            var vnpayAmount = Convert.ToInt64(vndAmount * 100m);
            _logger.LogInformation($"Final VNPay Amount: {vnpayAmount}");

            pay.AddRequestData("vnp_Amount", vnpayAmount.ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.UserId}"); // Lưu thông tin vào OrderInfo
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", transactionId);

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            _logger.LogInformation($"Generated Payment URL: {paymentUrl}");

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_OrderInfo = collections.FirstOrDefault(k => k.Key == "vnp_OrderInfo").Value.ToString();
            var vnp_TransactionNo = collections.FirstOrDefault(k => k.Key == "vnp_TransactionNo").Value.ToString();
            var vnp_ResponseCode = collections.FirstOrDefault(k => k.Key == "vnp_ResponseCode").Value.ToString();
            var vnp_TxnRef = collections.FirstOrDefault(k => k.Key == "vnp_TxnRef").Value.ToString();
            var vnp_SecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();

            _logger.LogInformation($"OrderInfo: {vnp_OrderInfo}");
            _logger.LogInformation($"TxnRef: {vnp_TxnRef}");

            bool validSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["Vnpay:HashSecret"]);
            if (validSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    return new PaymentResponseModel
                    {
                        Success = true,
                        PaymentMethod = "VnPay",
                        OrderDescription = vnp_OrderInfo,
                        OrderId = vnp_TxnRef,
                        TransactionId = vnp_TransactionNo,
                        Token = vnp_TxnRef,
                        VnPayResponseCode = vnp_ResponseCode
                    };
                }
            }

            return new PaymentResponseModel
            {
                Success = false,
                PaymentMethod = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_TxnRef,
                TransactionId = vnp_TransactionNo,
                Token = vnp_TxnRef,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}