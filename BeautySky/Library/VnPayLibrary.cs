using BeautySky.Models.Vnpay;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BeautySky.Library
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData;
        private readonly SortedList<string, string> _responseData;

        public VnPayLibrary()
        {
            _requestData = new SortedList<string, string>(new VnPayCompare());
            _responseData = new SortedList<string, string>(new VnPayCompare());
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            try
            {
                if (collection == null || string.IsNullOrEmpty(hashSecret))
                {
                    return new PaymentResponseModel
                    {
                        Success = false,
                        OrderDescription = "Dữ liệu không hợp lệ"
                    };
                }

                // Thêm dữ liệu vào response data
                foreach (var (key, value) in collection)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        AddResponseData(key, value);
                    }
                }

                // Lấy dữ liệu từ response
                var vnpTxnRef = GetResponseData("vnp_TxnRef");
                var vnpTransactionNo = GetResponseData("vnp_TransactionNo");
                var vnpResponseCode = GetResponseData("vnp_ResponseCode");
                var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
                var orderInfo = GetResponseData("vnp_OrderInfo");

                // Kiểm tra dữ liệu
                if (!long.TryParse(vnpTxnRef, out long orderId))
                {
                    return new PaymentResponseModel
                    {
                        Success = false,
                        OrderDescription = "Mã đơn hàng không hợp lệ"
                    };
                }

                if (!long.TryParse(vnpTransactionNo, out long vnPayTranId))
                {
                    return new PaymentResponseModel
                    {
                        Success = false,
                        OrderDescription = "Mã giao dịch không hợp lệ"
                    };
                }

                // Kiểm tra chữ ký
                var checkSignature = ValidateSignature(vnpSecureHash, hashSecret);
                if (!checkSignature)
                {
                    return new PaymentResponseModel
                    {
                        Success = false,
                        OrderDescription = "Chữ ký không hợp lệ"
                    };
                }

                // Trả về kết quả thành công
                return new PaymentResponseModel
                {
                    Success = true,
                    PaymentMethod = "VnPay",
                    OrderDescription = orderInfo,
                    OrderId = orderId.ToString(),
                    PaymentId = vnPayTranId.ToString(),
                    TransactionId = vnPayTranId.ToString(),
                    Token = vnpSecureHash,
                    VnPayResponseCode = vnpResponseCode
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseModel
                {
                    Success = false,
                    OrderDescription = $"Lỗi xử lý: {ex.Message}"
                };
            }
        }

        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var remoteIpAddress = context?.Connection?.RemoteIpAddress;
                if (remoteIpAddress == null)
                {
                    return "127.0.0.1";
                }

                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    var ipv4Address = Dns.GetHostEntry(remoteIpAddress)
                        .AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    return ipv4Address?.ToString() ?? "127.0.0.1";
                }

                return remoteIpAddress.ToString();
            }
            catch (Exception)
            {
                return "127.0.0.1";
            }
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _requestData[key] = value; // Sử dụng indexer thay vì Add để tránh lỗi key trùng
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return !string.IsNullOrEmpty(key) && _responseData.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            try
            {
                // Sắp xếp dữ liệu theo thứ tự alphabet
                var sortedData = new SortedList<string, string>(_requestData, new VnPayCompare());

                var data = new StringBuilder();
                foreach (var (key, value) in sortedData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Encode từng phần của URL
                        data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
                    }
                }

                string queryString = data.ToString();
                queryString = queryString.Substring(0, queryString.Length - 1); // Bỏ dấu & cuối cùng

                // Tạo chuỗi hash
                string signData = queryString;
                string vnpSecureHash = HmacSha512(vnpHashSecret, signData);

                // Tạo URL hoàn chỉnh
                string requestUrl = baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;

                return requestUrl;
            }
            catch (Exception ex)
            {
                // Log lỗi
                return string.Empty;
            }
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            try
            {
                var rspRaw = GetResponseData();
                var myChecksum = HmacSha512(secretKey, rspRaw);

                // So sánh không phân biệt chữ hoa/thường
                return string.Equals(myChecksum, inputHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(key ?? string.Empty);
                var inputBytes = Encoding.UTF8.GetBytes(inputData ?? string.Empty);
                using (var hmac = new HMACSHA512(keyBytes))
                {
                    var hashValue = hmac.ComputeHash(inputBytes);
                    foreach (var theByte in hashValue)
                    {
                        hash.Append(theByte.ToString("x2"));
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();

            // Tạo bản sao và sắp xếp
            var sortedResponse = new SortedList<string, string>(_responseData, new VnPayCompare());

            if (sortedResponse.ContainsKey("vnp_SecureHashType"))
            {
                sortedResponse.Remove("vnp_SecureHashType");
            }
            if (sortedResponse.ContainsKey("vnp_SecureHash"))
            {
                sortedResponse.Remove("vnp_SecureHash");
            }

            foreach (KeyValuePair<string, string> kv in sortedResponse)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            // Xóa dấu & cuối cùng
            if (data.Length > 0)
            {
                data.Length = data.Length - 1;
            }

            return data.ToString();
        }

        internal void AddRequestData(string v, object value)
        {
            throw new NotImplementedException();
        }

        internal object CreateRequestUrl(object value1, object value2)
        {
            throw new NotImplementedException();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        private readonly CompareInfo _compareInfo;

        public VnPayCompare()
        {
            _compareInfo = CompareInfo.GetCompareInfo("en-US");
        }

        public int Compare(string x, string y)
        {
            if (string.Equals(x, y, StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.IsNullOrEmpty(x)) return -1;
            if (string.IsNullOrEmpty(y)) return 1;
            return _compareInfo.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}