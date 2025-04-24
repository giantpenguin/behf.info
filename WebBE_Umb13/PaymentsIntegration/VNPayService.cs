using Microsoft.Extensions.Options;
using System.Net;

namespace YeahWeb.PaymentsIntegration;

public class VNPayService/* : IVNPayService*/
{
    private const string DateTimeFormat = "yyyyMMddHHmmss";
    private readonly VNPayOptions _config;
    private readonly ILogger<VNPayService> _logger;

    public VNPayService(IOptions<VNPayOptions> config, ILogger<VNPayService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public string CreatePaymentUrl(VNPayPayRequest request, string remoteIP)
    {
        var reqTime = DateTime.Now;
        var vnpayLib = new VNPayLib();
        vnpayLib.AddRequestData("vnp_Version", _config.Version);
        vnpayLib.AddRequestData("vnp_TmnCode", _config.TmnCode);
        vnpayLib.AddRequestData("vnp_CurrCode", _config.CurrencyCode);
        vnpayLib.AddRequestData("vnp_ReturnUrl", _config.ReturnUrl);
        vnpayLib.AddRequestData("vnp_Command", "pay");
        vnpayLib.AddRequestData("vnp_Amount", (request.Amount * 100).ToString()); // Decimal elimination - https://sandbox.vnpayment.vn/apis/docs/thanh-toan-pay/pay.html
        vnpayLib.AddRequestData("vnp_Locale", request.Locale);
        vnpayLib.AddRequestData("vnp_OrderInfo", request.OrderDescription);
        vnpayLib.AddRequestData("vnp_OrderType", request.OrderType);
        vnpayLib.AddRequestData("vnp_TxnRef", request.OrderId);
        vnpayLib.AddRequestData("vnp_IpAddr", remoteIP);
        vnpayLib.AddRequestData("vnp_CreateDate", reqTime.ToString(DateTimeFormat));
        vnpayLib.AddRequestData("vnp_ExpireDate", reqTime.AddSeconds(_config.ExpireOffset).ToString(DateTimeFormat));

        if (!string.IsNullOrEmpty(request.BankCode))
        {
            vnpayLib.AddRequestData("vnp_BankCode", request.BankCode);
        }

        var paymentUrl = vnpayLib.CreateRequestUrl(_config.PayEndpoint, _config.HashSecret);
        return paymentUrl;
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
        var vnpayTranId = vnpayLib.GetResponseDataItem("vnp_TransactionNo");
        var vnpResponseCode = vnpayLib.GetResponseDataItem("vnp_ResponseCode");

        return new VNPayResponse
        {
            OrderId = orderId,
            PaymentId = vnpayTranId,
            TransactionId = vnpayTranId,
            ResponseCode = vnpResponseCode,
            BankCode = vnpayLib.GetResponseDataItem("vnp_BankCode"),
            Amount = decimal.Parse(vnpayLib.GetResponseDataItem("vnp_Amount")) / 100,
            CardType = vnpayLib.GetResponseDataItem("vnp_CardType"),
            PayDate = DateTime.ParseExact(vnpayLib.GetResponseDataItem("vnp_PayDate"), DateTimeFormat, null),
            OrderInfo = vnpayLib.GetResponseDataItem("vnp_OrderInfo")
        };
    }
}