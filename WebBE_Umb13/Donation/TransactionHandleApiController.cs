using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.PublishedModels;
using YeahWeb.Donation.Services;
using YeahWeb.PaymentsIntegration;

namespace YeahWeb.Donation;

[ApiController]
[Route("/api/transactionhandle")]
public class TransactionHandleApiController : ControllerBase
{
    private readonly ILogger<TransactionHandleApiController> _logger;
    private readonly IDonationTransRepository _donatTransRepo;
    private readonly VNPayService _vnPayService;

    public TransactionHandleApiController(ILogger<TransactionHandleApiController> logger, VNPayService vnPayService, IDonationTransRepository donatTransRepo) 
    {
        _donatTransRepo = donatTransRepo;
        _vnPayService = vnPayService;
        _logger = logger;
    }

    [HttpGet("vnpayipn")]  
    public IActionResult VNPayIPN()
    {
        if (Request.Query.IsNullOrEmpty())
        {
            return GetContentResult("99","Empty PayGate response");
        }

        var rspCode = CheckResponse(Request.Query, out string payGateTransNo, out var transContent);
        if (transContent == null)
        {
            return GetContentResult(rspCode);
        }

        try
        {
            var payStatus = rspCode == "00" ? "Success" : "Fail";
            transContent.SetValue(nameof(DonationTransaction.PayingStatus), payStatus);
            transContent.SetValue(nameof(DonationTransaction.PayGateResponse), Request.QueryString.ToString());
            transContent.SetValue(nameof(DonationTransaction.PayGateRecordId), payGateTransNo);
            _donatTransRepo.UpdateTransaction(transContent);
        }
        catch (Exception ex)
        {
            rspCode = "99";
            _logger.LogError(ex, "Error processing VNPay response");
        }

        return GetContentResult(rspCode);
    }

    [HttpGet("vnpayerror")]
    public IActionResult VNPayError([FromQuery] string code = "99", [FromQuery] string message = "Invalid PayGate response")
    {
        return GetContentResult(code, message);
    }


    private string CheckResponse(IQueryCollection respData, out string payGateTransNo, out IContent? transContent)
    {
        _logger.LogInformation("VNPay response: {0}", respData);

        transContent = default;
        var validResp = _vnPayService.CheckResponse(respData, out var payResp);
        payGateTransNo = payResp.TransactionNo;

        if (!validResp)
        {
            _logger.LogWarning("Invalid checksum");
            return "97";
        }

        var orgTransId = payResp.OrderId;
        transContent = _donatTransRepo.GetTransaction(orgTransId);

        if (transContent is null)
        {
            _logger.LogWarning("Transaction NotFound: {orgTransId}.", orgTransId);
            return "01";
        }

        if (transContent.GetValue<string>(nameof(DonationTransaction.PayingStatus)) != "Pending")
        {
            _logger.LogWarning("Transaction {orgTransId} PayingStatus has been updated.", orgTransId);
            return "02";
        }

        if (transContent.GetValue<decimal>(nameof(DonationTransaction.Amount)) != payResp.Amount)
        {
            _logger.LogWarning("Invalid amount: {payResp.Amount}, transaction {orgTransId}.", payResp.Amount, orgTransId);
            return "04";
        }

        return payResp.Success ? "00" : payResp.ResponseCode != "00" ? payResp.ResponseCode : payResp.TransactionStatus;
    }

    private ContentResult GetContentResult(string code, string? customMsg = null)
    { 
        var msg = customMsg ?? GetRspMessage(code);
        return Content("{" + $"\"RspCode\":\"{code}\",\"Message\":\"{msg}\"" + "}");
    }

    private static string GetRspMessage(string rspCode) => RspCodeMessages.GetValue(rspCode, "Error while processing");

    private static readonly Dictionary<string, string> RspCodeMessages = new() {
        {"00", "Confirm Success" },
        {"01", "Order not found" },
        {"02", "Order already confirmed" },
        {"04", "Invalid amount" },
        {"97", "Invalid signature" },
        {"99", "Error while processing" }
        //{"00", "" },{"00", "" },
    };

    [HttpPost]
    [Route("newrequest")]    
    //[ValidateAntiForgeryToken]
    public IActionResult NewRequest(/*[FromForm]DonationViewModel model*/)
    {

        var formId = Request.Form["formId"];
        return Redirect("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        return Ok(formId);
        //    var request = new DonationRequest
        //    {
        //        Amount = model.MoneyAmount,
        //        DonorName = model.DonatorName,
        //        DonorEmail = model.DonatorEmail,
        //        Message = model.Message,
        //        PayGate = model.PayGate,
        //        Locale = model.Locale
        //    };

        //try
        //{
        //    var transId = _donationStoreService.CreateDonationTransaction(request);

        //    var paymentUrl = _vnPayService.CreatePaymentUrl(new VNPayPayRequest
        //    {
        //        Name = model.DonatorName,
        //        Email = model.DonatorEmail,
        //        OrderId = transId,
        //        Amount = (decimal)model.MoneyAmount,
        //        OrderDescription = model.Message,
        //        //BankCode = model.BankCode,
        //        Locale = model.Locale,
        //        OrderType = "donation",
        //    }, HttpContext.GetRemoteIPString());

        //    return Redirect(paymentUrl);

        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Error processing donation");
        //    ModelState.AddModelError("", "An error occurred while processing your donation.");
        //    return BadRequest();
        //}
    }
}
