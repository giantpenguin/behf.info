using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;
using YeahWeb.Donation.Services;
using YeahWeb.PaymentsIntegration;

namespace YeahWeb.Donation;

public class DonationTransHandleWorkflow : WorkflowType
{
    private const string FieldAliasDonatorName = "donatorName";
    private const string FieldAliasDonatorEmail = "donatorEmail";
    private const string FieldAliasMoneyAmount = "moneyAmount";
    private const string FieldAliasMessage = "donationMessage";
    private const string FieldAliasPayGate = "payGate";
    private const string FieldAliasLocale = "formLocale";

    private readonly ILogger<DonationTransHandleWorkflow> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUmbracoHelperAccessor _umbracoHelperAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IDonationTransRepository _donationStoreService;
    private readonly VNPayService _vnPayService;

    public DonationTransHandleWorkflow(ILogger<DonationTransHandleWorkflow> logger, IHttpContextAccessor httpContextAccessor
        , IUmbracoHelperAccessor umbracoHelperAccessor, IPublishedUrlProvider publishedUrlProvider
        , IDonationTransRepository donationStoreService, VNPayService vnPayService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _umbracoHelperAccessor = umbracoHelperAccessor;
        _publishedUrlProvider = publishedUrlProvider;
        _donationStoreService = donationStoreService;
        _vnPayService = vnPayService;

        Id = new Guid("8f87e3ee-7b81-4f4f-b667-3f08aded35bc");
        Name = "Handle Donation Request";
        Description = "Receive, store Donation Form request";
        Icon = "icon-forms-stackoverflow";
        Group = "Pay_Services";
    }

    [Setting("Use Submit-Page Url", Description = "Whether use the submit page or use the config's ReturnUrl", DisplayOrder = 10, PreValues = "True", View = "Checkbox")]
    public virtual string UseSubmitPage { get; set; }

    [Setting("Store PaymentUrl in context", Description = "Store calculated PaymentUrl in HttpContext.Items[\"FormsRedirectAfterFormSubmitUrl\"]", DisplayOrder = 13, PreValues = "True", View = "Checkbox")]
    public virtual string StorePayUrlInContext { get; set; }

    public override Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
    {
        var ctxRecord = context.Record;
        var submitRecId = ctxRecord.UniqueId;
        _logger.LogDebug($"from IP {ctxRecord.IP}.");

        RecordField? rfldDonatorName = ctxRecord.GetRecordFieldByAlias(FieldAliasDonatorName);
        RecordField? rfldDonatorEmail = ctxRecord.GetRecordFieldByAlias(FieldAliasDonatorEmail);
        RecordField? rfldMoney = ctxRecord.GetRecordFieldByAlias(FieldAliasMoneyAmount);
        RecordField? rfldMessage = ctxRecord.GetRecordFieldByAlias(FieldAliasMessage);
        RecordField? rfldPayGate = ctxRecord.GetRecordFieldByAlias(FieldAliasPayGate);
        RecordField? rfldLocale = ctxRecord.GetRecordFieldByAlias(FieldAliasLocale);

        var donorName = rfldDonatorName?.ValuesAsString();
        var donorEmail = rfldDonatorEmail?.ValuesAsString();
        var message = rfldMessage?.ValuesAsString();
        var payGate = rfldPayGate?.ValuesAsString();
        var locale = rfldLocale?.ValuesAsString();

        // use double to avoid decimal is to minimize storage
        var moneyAmount = rfldMoney != null ? rfldMoney.ValuesAsString().TryConvertTo<double>().Result : 0.0D;

        string submitPageUrl = null;
        if (useSubmitPage && _umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper) && umbracoHelper != null &&
            umbracoHelper.Content(context.Record.UmbracoPageId) is IPublishedContent content)
        {
            submitPageUrl = content.Url(_publishedUrlProvider, null, UrlMode.Absolute);
        }

        var donationReq = new DonationRequest()
        {
            DonorName = donorName,
            DonorEmail = donorEmail,
            Amount = moneyAmount,
            Message = message,
            PayGate = payGate,
            Locale = locale,
            FormRecordId = submitRecId,
            SubmitReferrer = submitPageUrl
        };

        try
        {
            var transId = _donationStoreService.StoreTransaction(donationReq);

            var paymentUrl = _vnPayService.CreatePaymentUrl(new VNPayPayRequest
            {
                OrderId = transId,
                Name = donorName,
                Email = donorEmail,
                Amount = (decimal)moneyAmount,
                OrderDescription = message,
                Locale = locale,
                OrderType = "donation",
                ReturnUrl = submitPageUrl
                //BankCode = model.BankCode,
            }, context.Record.IP);

            if (storePayUrlInContext)
            {
                _httpContextAccessor.HttpContext.Items[Constants.ItemKeys.RedirectAfterFormSubmitUrl] = paymentUrl;
            }
            ctxRecord.State = FormState.Approved;

            return Task.FromResult(WorkflowExecutionStatus.Completed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Submit {submitRecId} transaction save error.", submitRecId);
            return Task.FromResult(WorkflowExecutionStatus.Failed);
        }
    }

    public override List<Exception> ValidateSettings()
    {
        return [];
    }

    private bool useSubmitPage => UseSubmitPage.InvariantEquals(bool.TrueString);

    private bool storePayUrlInContext => StorePayUrlInContext.InvariantEquals(bool.TrueString);
}
