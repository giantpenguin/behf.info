using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Web;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;
using YeahWeb.Donation.Services;
using YeahWeb.Extensions;
using YeahWeb.PaymentsIntegration;

namespace YeahWeb.Donation;

public class DonationHandleSurfaceController : SurfaceController
{
    private readonly IDonationRepository _donationStoreService;
    private readonly VNPayService _vnPayService;
    private readonly ILogger<DonationHandleSurfaceController> _logger;

    public DonationHandleSurfaceController(ILogger<DonationHandleSurfaceController> logger, IDonationRepository donationStoreService, VNPayService vnPayService,
        IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _donationStoreService = donationStoreService;
        _vnPayService = vnPayService;
        _logger = logger;
    }

    [Obsolete]
    [HttpPost("process")]
    //[ValidateUmbracoFormRouteString]
    //[ValidateAntiForgeryToken]
    public IActionResult ProcessDonation(DonationViewModel model)
    {
        throw new NotImplementedException();
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var request = new DonationRequest
            {
                Amount = model.MoneyAmount,
                DonorName = model.DonatorName,
                DonorEmail = model.DonatorEmail,
                Message = model.Message,
                PayGate = model.PayGate,
                Locale = model.Locale
            };

        try
        {
            var transId = _donationStoreService.StoreTransaction(request);

            var paymentUrl = _vnPayService.CreatePaymentUrl(new VNPayPayRequest
            {
                Name = model.DonatorName,
                Email = model.DonatorEmail,
                OrderId = transId,
                Amount = (decimal)model.MoneyAmount,
                OrderDescription = model.Message,
                Locale = model.Locale,
                OrderType = "donation",
                BankCode = model.BankCode,
            }, HttpContext.GetRemoteIPString());

            return Redirect(paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing donation");
            ModelState.AddModelError("", "An error occurred while processing your donation.");
            return CurrentUmbracoPage();
        }
    }

    [HttpGet]
    public IActionResult PayGateReturn(/*[FromQuery] IQueryCollection collections*/)
    {
        //HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
        try
        {
            var response = _vnPayService.ProcessPaymentResponse(HttpContext.Request.Query);

            // Update transaction status
            _donationStoreService.UpdateTransactionStatus(response.OrderId, response.PayDate,
                response.Success ? "Completed" : "Failed",
                JsonSerializer.Serialize(response));

            // Redirect to appropriate page
            if (response.Success)
            {
                return RedirectToCurrentUmbracoPage();
                //return RedirectToUmbracoPage(GetSuccessPageId());
            }
            else
            {
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToCurrentUmbracoPage(new QueryString($"?transactionId={response.TransactionId}"));
                //return RedirectToUmbracoPage(GetFailurePageId());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay return");
            return RedirectToCurrentUmbracoPage();
            //return RedirectToUmbracoPage(GetErrorPageId());
        }
    }
}
