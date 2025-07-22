using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Forms.Core;
using YeahWeb.Donation.Services;
using YeahWeb.PaymentsIntegration;

namespace YeahWeb.Donation;

public class DonationPageController : RenderController
{
    private readonly ICultureDictionaryFactory _dictionaryFactory;
    private readonly ILogger<DonationPageController> _logger;
    private readonly IDonationTransRepository _donationTransRepo;
    private readonly VNPayService _vnPayService;
    private readonly VNPayOptions _vnpayOptions;

    public DonationPageController(ILogger<DonationPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ICultureDictionaryFactory  dictionaryFactory
        , IDonationTransRepository donationTransRepo, VNPayService vnPayService, IOptions<VNPayOptions> vnpayOptions)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _logger = logger;
        _dictionaryFactory = dictionaryFactory;
        _donationTransRepo = donationTransRepo;
        _vnPayService = vnPayService;
        _vnpayOptions = vnpayOptions.Value;
    }


    public override IActionResult Index()
    {
        //var redirectUrl = HttpContext.Items[Constants.ItemKeys.RedirectAfterFormSubmitUrl]?.ToString();
        ////TODO: need verifying the redirectUrl is desired paygate endpoint???
        //if (!string.IsNullOrEmpty(redirectUrl))
        //    return Redirect(redirectUrl);

        // "currentCulture" to populate for formLocale field
        Dictionary<string, string> additionalData = new() { { "currentCulture", CurrentPage.GetCultureFromDomains() } };

        if (Request.Query.IsNullOrEmpty())
        {            
            ViewData["additionalData"] = additionalData;
            return base.Index(); // View("~/Views/DonationPage.cshtml");
        }

        var validCheck = _vnPayService.CheckResponse(Request.Query, out var payResp);
        var paySuccess = validCheck && payResp.Success/*.ResponseCode == "00" & payResp.TransactionStatus == "00"*/;
        var translationItems = _dictionaryFactory.CreateDictionary();

        TempData["TransactionSuccess"] = paySuccess;
        if (paySuccess)
        {
            var thankyouMsg = string.Format(translationItems["thankyouMessage_Format"], payResp.Amount);
            additionalData.Add("donationThankyouMessage", thankyouMsg);
            TempData["UmbracoFormSubmittedFromCurrentPage"] = Guid.Parse("79be098b-b737-4971-97c5-79ddf4db8fc4");

            //TempData["TransactionMessage"] = string.Format(translationItems["thankyouMessage_Format"], payResp.Amount);
        }
        else
        {
            TempData["TransactionMessage"] = translationItems["unsuccessMessage"];
        }

        ViewData["additionalData"] = additionalData;
        return base.Index(); // View("~/Views/DonationPage.cshtml");
    }
}
