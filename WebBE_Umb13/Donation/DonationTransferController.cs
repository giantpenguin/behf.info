using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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

public class DonationTransferController : SurfaceController
{
    private readonly IDonationStoreService _donationStoreService;
    private readonly VNPayService _vnPayService;
    private readonly ILogger<DonationTransferController> _logger;

    public DonationTransferController(IDonationStoreService donationStoreService, VNPayService vnPayService,ILogger<DonationTransferController> logger,
        IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _donationStoreService = donationStoreService;
        _vnPayService = vnPayService;
        _logger = logger;
    }

    [HttpPost]
    [ValidateUmbracoFormRouteString]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessDonation(DonationViewModel model)
    {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var request = new DonationRequest
            {
                Amount = model.Amount,
                DonorName = model.DonorName,
                DonorEmail = model.DonorEmail,
                Message = model.Message,
                PayGate = model.PayGate
            };

        try
        {
            var transId = _donationStoreService.CreateDonationTransaction(request);

            var paymentUrl = _vnPayService.CreatePaymentUrl(new VNPayPayRequest
            {
                OrderId = transId,
                Amount = model.Amount,
                //Name = model.DonorName,
                //Email = model.DonorEmail,
                OrderDescription = model.Message,
                BankCode = model.BankCode,
                Locale = model.Locale,
                OrderType = "other-donation",
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
    public IActionResult PayGateReturn([FromQuery] IQueryCollection collections)
    {
        try
        {
            var response = _vnPayService.ProcessPaymentResponse(collections);

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
