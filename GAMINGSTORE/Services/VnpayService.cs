using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GAMINGSTORE.Services
{
    public class VnpayService : IVnpayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnpayService> _logger;

        public VnpayService(IConfiguration configuration, ILogger<VnpayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string returnUrl)
        {
            try
            {
                var vnpayUrl = _configuration["Vnpay:Url"];
                var tmnCode = _configuration["Vnpay:TmnCode"];
                var hashSecret = _configuration["Vnpay:HashSecret"];

                if (string.IsNullOrEmpty(vnpayUrl) || string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
                {
                    _logger.LogError("VNPay configuration is missing");
                    throw new InvalidOperationException("VNPay configuration is not set");
                }

                var vnp_Url = vnpayUrl;
                var vnp_TmnCode = tmnCode;
                var vnp_HashSecret = hashSecret;
                var vnp_Amount = (long)(amount * 100); // VNPay expects amount in cents
                var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var vnp_ExpireDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");
                var vnp_IpAddr = GetClientIpAddress();
                var vnp_Locale = "vn";
                var vnp_OrderInfo = orderInfo;
                var vnp_OrderType = "other";
                var vnp_ReturnUrl = returnUrl;
                var vnp_TxnRef = orderId.ToString() + DateTime.Now.Ticks.ToString().Substring(0, 6);

                var vnp_Params = new SortedDictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", vnp_TmnCode },
                    { "vnp_Amount", vnp_Amount.ToString() },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_TxnRef", vnp_TxnRef },
                    { "vnp_OrderInfo", vnp_OrderInfo },
                    { "vnp_OrderType", vnp_OrderType },
                    { "vnp_Locale", vnp_Locale },
                    { "vnp_ReturnUrl", vnp_ReturnUrl },
                    { "vnp_IpAddr", vnp_IpAddr },
                    { "vnp_CreateDate", vnp_CreateDate },
                    { "vnp_ExpireDate", vnp_ExpireDate }
                };

                var queryStringBuilder = new StringBuilder();
                foreach (var kvp in vnp_Params)
                {
                    if (!string.IsNullOrEmpty(kvp.Value))
                    {
                        queryStringBuilder.Append(Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value) + "&");
                    }
                }

                var queryString = queryStringBuilder.ToString();
                if (queryString.EndsWith("&"))
                {
                    queryString = queryString.Remove(queryString.Length - 1);
                }

                var vnp_SecureHash = HmacSHA512(vnp_HashSecret, queryString);
                var paymentUrl = vnp_Url + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;

                _logger.LogInformation($"VNPay payment URL created for order {orderId}");
                return paymentUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating VNPay payment URL: {ex.Message}");
                throw;
            }
        }

        public (bool success, string message, int? orderId) ProcessPaymentReturn(IQueryCollection queryCollection)
        {
            try
            {
                var hashSecret = _configuration["Vnpay:HashSecret"];
                if (string.IsNullOrEmpty(hashSecret))
                {
                    return (false, "VNPay configuration is missing", null);
                }

                var vnp_SecureHash = queryCollection["vnp_SecureHash"].ToString();
                var vnp_ResponseCode = queryCollection["vnp_ResponseCode"].ToString();
                var vnp_TransactionStatus = queryCollection["vnp_TransactionStatus"].ToString();
                var vnp_TxnRef = queryCollection["vnp_TxnRef"].ToString();

                // Extract order ID from TxnRef (format: orderId + timestamp)
                var orderIdStr = vnp_TxnRef.Substring(0, vnp_TxnRef.Length - 6);
                if (!int.TryParse(orderIdStr, out int orderId))
                {
                    return (false, "Invalid order ID", null);
                }

                // Verify secure hash
                var queryParams = new SortedDictionary<string, string>();
                foreach (var key in queryCollection.Keys)
                {
                    if (key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                    {
                        queryParams.Add(key, queryCollection[key].ToString());
                    }
                }

                var queryString = string.Join("&", queryParams.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value)));
                var calculatedHash = HmacSHA512(hashSecret, queryString);

                if (calculatedHash != vnp_SecureHash)
                {
                    _logger.LogWarning($"Invalid secure hash for order {orderId}");
                    return (false, "Invalid secure hash", orderId);
                }

                // Check response code
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    _logger.LogInformation($"VNPay payment successful for order {orderId}");
                    return (true, "Thanh toán thành công", orderId);
                }
                else if (vnp_ResponseCode == "07")
                {
                    return (false, "Trừ tiền thất bại", orderId);
                }
                else if (vnp_ResponseCode == "09")
                {
                    return (false, "Giao dịch không tồn tại", orderId);
                }
                else if (vnp_ResponseCode == "10")
                {
                    return (false, "Khách hàng hủy giao dịch", orderId);
                }
                else if (vnp_ResponseCode == "11")
                {
                    return (false, "Đã hết hạn thanh toán", orderId);
                }
                else if (vnp_ResponseCode == "12")
                {
                    return (false, "Thẻ/Tài khoản bị khóa", orderId);
                }
                else
                {
                    return (false, $"Thanh toán thất bại: {vnp_ResponseCode}", orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing VNPay return: {ex.Message}");
                return (false, "Lỗi xử lý thanh toán", null);
            }
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
                foreach (byte x in hashBytes)
                {
                    hash.Append(x.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        private string GetClientIpAddress()
        {
            // This will be set from the controller context
            return "127.0.0.1";
        }
    }
}
