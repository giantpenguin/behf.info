using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Web.Common.PublishedModels;
using System.Collections.Concurrent;
using System.Globalization;

namespace YeahWeb.Donation.Services;

public interface IDonationTransRepository
{
    string RootContainerName { get; }
    int RootContainerId { get; }
    string StoreTransaction(DonationRequest request);

    IContent? GetTransaction(string transactionId);

    bool UpdateTransaction(IContent transContent);

    bool UpdateTransactionStatus(string transactionId, DateTime transDate, string status, string gatewayResponse);
    //IContent GetTransactionForUpdate(string transactionId, DateTime transDate);
}

public class DonationTransRepository : IDonationTransRepository
{
    //NOTE: should not Publish these contents???

    private readonly IContentService _contentService;
    //private readonly IQuery<IContent> _contentQuery;
    //private readonly INotificationService _notificationService;
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly ILogger<DonationTransRepository> _logger;
    private readonly ConcurrentDictionary<int, int> dictContainerTree = [];
    private static string TransContainerTypeAlias => DonationTransContainer.ModelTypeAlias;
    private static string DonationTransTypeAlias => DonationTransaction.ModelTypeAlias;
    private const string DATETIMEFORMAT = "yyyyMMddHHmmss";
    private const string TRANSNAMEPREFIX = "Donation-";

    public string RootContainerName => "Donation Transactions";

    public int RootContainerId => GetOrCreateRootContainer();

    public DonationTransRepository(ILogger<DonationTransRepository> logger,
        IContentService contentService, /*IQuery<IContent> contentQuery, INotificationService notificationService,*/
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
    {
        _logger = logger;
        _contentService = contentService;
        //_contentQuery = contentQuery;
        //_notificationService = notificationService;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
    }

    public string StoreTransaction(DonationRequest request)
    {
        //TODO: UTC???
        var transTime = DateTime.Now;
        try
        {
            var containerId = GetTransactionsContainerId(transTime);
            //TODO: include some more data to avoid duplicate
            var transId = $"{TRANSNAMEPREFIX}{transTime.ToString(DATETIMEFORMAT, CultureInfo.InvariantCulture)}";

            var transactionContent = _contentService.Create(transId, containerId, DonationTransTypeAlias);

            transactionContent.SetValue(nameof(DonationTransaction.TransactionId).ToFirstLower(), transId);
            transactionContent.SetValue(nameof(DonationTransaction.InitialTime).ToFirstLower(), transTime);
            transactionContent.SetValue(nameof(DonationTransaction.Status).ToFirstLower(), "Pending");
            transactionContent.SetValue(nameof(DonationTransaction.PayingStatus).ToFirstLower(), "Pending");

            transactionContent.SetValue(nameof(DonationTransaction.Paymentor).ToFirstLower(), request.DonorName);
            transactionContent.SetValue(nameof(DonationTransaction.DonorEmail).ToFirstLower(), request.DonorEmail);
            transactionContent.SetValue(nameof(DonationTransaction.Amount).ToFirstLower(), request.Amount);
            transactionContent.SetValue(nameof(DonationTransaction.Message).ToFirstLower(), request.Message);
            transactionContent.SetValue(nameof(DonationTransaction.PayGateName).ToFirstLower(), request.PayGate);
            transactionContent.SetValue(nameof(DonationTransaction.Locale).ToFirstLower(), request.Locale);
            transactionContent.SetValue(nameof(DonationTransaction.SubmittedRecordId).ToFirstLower(), request.FormRecordId);
            transactionContent.SetValue(nameof(DonationTransaction.SubmitReferrer).ToFirstLower(), request.SubmitReferrer);

            _contentService.Save(transactionContent);
            //TODO: cache (align with PayGate expiration time) TransactionId & ContentId for latter processing???

            return transId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DonationTransaction content");
            throw;
        }
    }

    public bool UpdateTransaction(IContent transContent)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(transContent));

        transContent.SetValue(nameof(DonationTransaction.UpdatedTime).ToFirstLower(), DateTime.Now);

        var result = _contentService.Save(transContent);
        return result.Success;
    }

    public bool UpdateTransactionStatus(string transactionId, DateTime transDate, string status, string gatewayResponse)
    {
        if (TryGetTransactionContent(transactionId, transDate, out var tranContent, true))
        {
            throw new Exception($"Transaction {transactionId} not found");
        }

        tranContent.SetValue(nameof(DonationTransaction.Status).ToFirstLower()/*"status"*/, status);
        tranContent.SetValue(nameof(DonationTransaction.UpdatedTime).ToFirstLower(), DateTime.Now);
        tranContent.SetValue("gatewayResponse", gatewayResponse);

        var result = _contentService.Save(tranContent);
        return result.Success;
    }

    //private IQuery<IContent> QueryContainerContentType => _contentQuery.Where(c => c.ContentType.Alias == TransContainerTypeAlias);
    //private IQuery<IContent> QueryTransactionContents(DateTime date) => _contentQuery.Where(c => /*c.ContentType.Alias == "donationTransaction"*/ c.UpdateDate <= date);
    private int GetTransactionsContainerId(DateTime transDate)
    {
        var rootContainerId = RootContainerId;  // GetOrCreateRootContainer();
        int keyYearMonth = transDate.Year + transDate.Month;

        return dictContainerTree.GetOrAdd(keyYearMonth, (a) =>
        {
            var containerMonth = transDate.Month.ToString("D2"); // likes 02, 12
            var containerYear = transDate.Year.ToString("D4");

            // year folder
            var yearContainer = _contentService.GetPagedChildren(rootContainerId, 0, 100, out _/*, QueryContainerContentType*/)
                .FirstOrDefault(x => x.Name == containerYear);
            if (yearContainer == null)
            {
                try
                {
                    yearContainer = _contentService.Create(containerYear, rootContainerId, TransContainerTypeAlias);
                    _contentService.SaveAndPublish(yearContainer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Year Container");
                    throw;
                }
            }

            // month folder, should 12 months under each year
            var monthContainer = _contentService.GetPagedChildren(yearContainer.Id, 0, 20, out _/*, QueryContainerContentType*/)
                .FirstOrDefault(x => x.Name == containerMonth);
            if (monthContainer == null)
            {
                try
                {
                    monthContainer = _contentService.Create(containerMonth, yearContainer.Id, TransContainerTypeAlias);
                    _contentService.SaveAndPublish(monthContainer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Month Container");
                    throw;
                }
            }

            return monthContainer.Id;
        });
    }

    private int GetOrCreateRootContainer()
    {
        return dictContainerTree.GetOrAdd(-1, (a) =>
        {
            var transRootContainer = _contentService.GetByLevel(1).FirstOrDefault(x => x.ContentType.Alias == TransContainerTypeAlias);
            if (transRootContainer == null)
            {
                try
                {
                    transRootContainer = _contentService.Create(RootContainerName, -1, TransContainerTypeAlias);
                    _contentService.SaveAndPublish(transRootContainer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Root Container");
                    throw;
                }
            }
            return transRootContainer.Id;
        });
    }

    private bool TryGetTransactionContent(string transactionId, DateTime date, out IContent contentTrans, bool tryExtend = false)
    {
        var containerId = GetTransactionsContainerId(date);

        //TODO: fine tune transaction finding???
        // Find transaction by transactionId
        contentTrans = _contentService.GetPagedChildren(containerId, 0, 100, out _/*, QueryTransactionContents(date)*/)
            .FirstOrDefault(x => x.GetValue<string>("transactionId") == transactionId);
        if (contentTrans == null)
        {
            _logger.LogWarning("Transaction {transactionId} in {date:yyyyMMdd} not found", transactionId, date);
            if (tryExtend) // try extend finding in day-before Container
            {
                return TryGetTransactionContent(transactionId, date: date.AddDays(-1), out contentTrans, false);
            }
        }

        return contentTrans != null;
    }

    public IContent? GetTransaction(string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId) || !transactionId.StartsWith(TRANSNAMEPREFIX)
            || !TryGetTransDateTime(transactionId, out var dateTime))
            return null;

        TryGetTransactionContent(transactionId, dateTime, out var contentTrans, true);
        return contentTrans;
    }

    private bool TryGetTransDateTime(string transactionId, out DateTime dateTime)
    {
        var sdt = transactionId.Substring(TRANSNAMEPREFIX.Length);
        return DateTime.TryParseExact(sdt, DATETIMEFORMAT, null, DateTimeStyles.None, out dateTime);
    }
}