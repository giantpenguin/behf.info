using Microsoft.Extensions.Options;
using System.Net;

namespace YeahWeb.PaymentsIntegration;

public class VNPayService/* : IVNPayService*/
{
    public static DateTime FallbackPayDate => new (1900, 1, 1, 0, 0, 1);
    private readonly VNPayOptions _config;
    private readonly ILogger<VNPayService> _logger;

    public VNPayService(IOptions<VNPayOptions> config, ILogger<VNPayService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public string CreatePaymentUrl(VNPayPayRequest request, string remoteIP)
    {
        var vnpayLib = new VNPayLib();
        vnpayLib.AddRequestData("vnp_Version", _config.Version);
        vnpayLib.AddRequestData("vnp_TmnCode", _config.TmnCode);
        vnpayLib.AddRequestData("vnp_CurrCode", _config.CurrencyCode);
        vnpayLib.AddRequestData("vnp_Command", "pay");
        vnpayLib.AddRequestData("vnp_Amount", (request.Amount * 100).ToString()); // Decimal elimination - https://sandbox.vnpayment.vn/apis/docs/thanh-toan-pay/pay.html
        vnpayLib.AddRequestData("vnp_Locale", request.Locale);
        vnpayLib.AddRequestData("vnp_OrderInfo", request.OrderDescription);
        vnpayLib.AddRequestData("vnp_OrderType", request.OrderType);
        vnpayLib.AddRequestData("vnp_TxnRef", request.OrderId);
        vnpayLib.AddRequestData("vnp_IpAddr", remoteIP);

        //NOTE: GMT+7 & yyyyMMddHHmmss format is a must for VNPay v2.1.0
        var reqTime = DateTime.Now;
        vnpayLib.AddRequestData("vnp_CreateDate", reqTime.ToString(_config.DateTimeFormat));
        vnpayLib.AddRequestData("vnp_ExpireDate", reqTime.AddSeconds(_config.ExpireOffset).ToString(_config.DateTimeFormat));

        if (!string.IsNullOrEmpty(request.BankCode))
        {
            vnpayLib.AddRequestData("vnp_BankCode", request.BankCode);
        }

        vnpayLib.AddRequestData("vnp_ReturnUrl", request.ReturnUrl ?? _config.ReturnUrl);

        var paymentUrl = vnpayLib.CreateRequestUrl(_config.PayEndpoint, _config.HashSecret);
        return paymentUrl;
    }

    public bool CheckResponse(IQueryCollection collections, out VNPayResponse objResp)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(collections));

        var vnpayLib = new VNPayLib();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpayLib.AddResponseData(key, value.ToString());
            }
        }

        var vnpSecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
        var checkSignature = vnpayLib.ValidateSignature(vnpSecureHash, _config.HashSecret);

        var orderId = vnpayLib.GetResponseDataItem("vnp_TxnRef");
        var vnpResponseCode = vnpayLib.GetResponseDataItem("vnp_ResponseCode");
        var vnpTransStatus = vnpayLib.GetResponseDataItem("vnp_TransactionStatus");
        var vnpayTranNo = vnpayLib.GetResponseDataItem("vnp_TransactionNo");

        var isValidPayDate = DateTime.TryParseExact(vnpayLib.GetResponseDataItem("vnp_PayDate"), _config.DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out var payDate);
        var isValidAmount = decimal.TryParse(vnpayLib.GetResponseDataItem("vnp_Amount"), out var payAmount);

        objResp = new VNPayResponse
        {
            OrderId = orderId,
            //PaymentId = vnpayTranId,
            TransactionNo = vnpayTranNo,
            TransactionStatus = vnpTransStatus,
            ResponseCode = vnpResponseCode,
            BankCode = vnpayLib.GetResponseDataItem("vnp_BankCode"),
            CardType = vnpayLib.GetResponseDataItem("vnp_CardType"),
            OrderInfo = vnpayLib.GetResponseDataItem("vnp_OrderInfo"),
            Amount = isValidAmount ? payAmount / 100 : -1,
            PayDate = isValidPayDate ? payDate : FallbackPayDate
        };

        return checkSignature;
    }

    public bool IsValidChecksum(IQueryCollection collections)
    {
        var vnpayLib = new VNPayLib();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpayLib.AddResponseData(key, value.ToString());
            }
        }

        var vnpSecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
        return vnpayLib.ValidateSignature(vnpSecureHash, _config.HashSecret);
    }

    public VNPayResponse ProcessPaymentResponse(IQueryCollection collections)
    {
        var vnpayLib = new VNPayLib();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpayLib.AddResponseData(key, value.ToString());
            }
        }

        var vnpSecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
        var checkSignature = vnpayLib.ValidateSignature(vnpSecureHash, _config.HashSecret);
        if (!checkSignature)
        {
            throw new Exception("Invalid signature in payment response");
        }

        var orderId = vnpayLib.GetResponseDataItem("vnp_TxnRef");
        var vnpayTranNo = vnpayLib.GetResponseDataItem("vnp_TransactionNo");
        var vnpResponseCode = vnpayLib.GetResponseDataItem("vnp_ResponseCode");

        return new VNPayResponse
        {
            OrderId = orderId,
            //PaymentId = vnpayTranId,
            TransactionNo = vnpayTranNo,
            TransactionStatus = vnpayLib.GetResponseDataItem("vnp_TransactionStatus"),
            ResponseCode = vnpResponseCode,
            BankCode = vnpayLib.GetResponseDataItem("vnp_BankCode"),
            Amount = decimal.Parse(vnpayLib.GetResponseDataItem("vnp_Amount")) / 100,
            CardType = vnpayLib.GetResponseDataItem("vnp_CardType"),
            PayDate = DateTime.ParseExact(vnpayLib.GetResponseDataItem("vnp_PayDate"), _config.DateTimeFormat, null),
            OrderInfo = vnpayLib.GetResponseDataItem("vnp_OrderInfo")
        };
    }
}