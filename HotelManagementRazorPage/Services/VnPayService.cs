using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(Booking booking, HttpContext httpContext, decimal amount)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks;
            var pay = new VnPayLibrary();
            var urlCallBack = _config["VnPay:ReturnUrl"];

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", (_config["VnPay:TmnCode"] ?? "").Trim());
            pay.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(httpContext)); // Get real IP instead of 127.0.0.1
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Booking_Room_{booking.Id}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", (urlCallBack ?? "").Trim());
            pay.AddRequestData("vnp_TxnRef", $"{booking.Id}_{tick}"); 

            var paymentUrl =
                pay.CreateRequestUrl((_config["VnPay:BaseUrl"] ?? "").Trim(), (_config["VnPay:HashSecret"] ?? "").Trim());

            // DEBUG: In URL ra console để kiểm tra
            Console.WriteLine("=== VNPAY DEBUG ===");
            Console.WriteLine("TmnCode: " + (_config["VnPay:TmnCode"] ?? "").Trim());
            Console.WriteLine("Payment URL: " + paymentUrl);
            Console.WriteLine("===================");

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, (_config["VnPay:HashSecret"] ?? "").Trim());

            return response;
        }
    }

    // Helper classes for VNPay (Usually put in a separate file, but included here for completeness)
    public class VnPayLibrary
    {
        public class CompareOrdinal : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var Compare = string.CompareOrdinal(x, y);
                return Compare;
            }
        }

        private SortedList<string, string> _requestData = new SortedList<string, string>(new CompareOrdinal());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new CompareOrdinal());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            // Both hashData and queryUrl use the same encoding (URLEncode on value only, raw key)
            // This matches the official VNPAY Java/C# sample
            StringBuilder hashData = new StringBuilder();
            StringBuilder queryData = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    if (hashData.Length > 0)
                    {
                        hashData.Append('&');
                        queryData.Append('&');
                    }
                    // hashData: key=urlEncode(value)   <-- per official VNPAY Java sample
                    hashData.Append(kv.Key);
                    hashData.Append('=');
                    hashData.Append(WebUtility.UrlEncode(kv.Value));

                    // queryString is the same format
                    queryData.Append(kv.Key);
                    queryData.Append('=');
                    queryData.Append(WebUtility.UrlEncode(kv.Value));
                }
            }

            string queryUrl = queryData.ToString();
            string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, hashData.ToString());
            string paymentUrl = baseUrl + "?" + queryUrl + "&vnp_SecureHash=" + vnp_SecureHash;
            return paymentUrl;
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string vnp_HashSecret)
        {
            var vnPay = new VnPayLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value.ToString());
                }
            }
            
            // Extract BookingId from TxnRef (Format: BookingId_Ticks)
            string txnRef = vnPay.GetResponseData("vnp_TxnRef");
            string orderId = txnRef;
            if (txnRef.Contains("_"))
            {
                orderId = txnRef.Split('_')[0];
            }

            long vnp_Amount = Convert.ToInt64(vnPay.GetResponseData("vnp_Amount")) / 100;
            string vnp_ResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionId = vnPay.GetResponseData("vnp_TransactionNo");
            string vnp_SecureHash = collection.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            string orderInfo = vnPay.GetResponseData("vnp_OrderInfo");

            bool checkSignature = vnPay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            if (!checkSignature)
            {
                   return new PaymentResponseModel { Success = false, VnPayResponseCode = "Invalid Signature" };
            }

            return new PaymentResponseModel
            {
                Success = vnp_ResponseCode == "00", // 00 means success
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId, 
                TransactionId = vnp_TransactionId,
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
        
        public string GetResponseData(string key)
        {
            string retValue;
            if (_responseData.TryGetValue(key, out retValue))
            {
                return retValue;
            }
            else
            {
                return string.Empty;
            }
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rspRaw = GetResponseData();
            string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetResponseData()
        {
            StringBuilder data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }
            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    if (data.Length > 0) data.Append('&');
                    // Same format as createRequestUrl: key=urlEncode(value)
                    data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value));
                }
            }
            return data.ToString();
        }
    }

    public static class Utils
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
        
        public static string GetIpAddress(HttpContext context)
        {
             var ipAddress = string.Empty;
             try
             {
                 var remoteIpAddress = context.Connection.RemoteIpAddress;
                 if (remoteIpAddress != null)
                 {
                     if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                     {
                         remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
                             .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                     }
          
                     if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
          
                     return string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress;
                 }
             }
             catch (Exception)
             {
                 return "127.0.0.1";
             }

             return "127.0.0.1";
        }
    }
}
