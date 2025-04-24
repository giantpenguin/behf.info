using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Web.Common.PublishedModels;
using System.Collections.Concurrent;
using System.Globalization;

namespace YeahWeb.Donation.Services;

public interface IDonationStoreService
{
    string CreateDonationTransaction(DonationRequest request);
    bool UpdateTransactionStatus(string transactionId, DateTime transDate, string status, string gatewayResponse);
    //IContent GetTransactionForUpdate(string transactionId, DateTime transDate);
}

public class DonationStoreService : IDonationStoreService
{
    //NOTE: should not Publish these contents???

    private readonly IContentService _contentService;
    //private readonly IQuery<IContent> _contentQuery;
    //private readonly INotificationService _notificationService;
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly ILogger<DonationStoreService> _logger;
    private readonly ConcurrentDictionary<int, int> dictContainerTree = [];
    private static string TransContainerTypeAlias => DonationTransContainer.ModelTypeAlias;
    private static string DonationTransTypeAlias => DonationTransaction.ModelTypeAlias;

    public DonationStoreService(
        IContentService contentService,
        //IQuery<IContent> contentQuery,
        //INotificationService notificationService,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        ILogger<DonationStoreService> logger)
    {
        _contentService = contentService;
        //_contentQuery = contentQuery;
        //_notificationService = notificationService;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _logger = logger;
    }

    public string CreateDonationTransaction(DonationRequest request)
    {
            var transTime = DateTime.Now;
        try
        {
            var containerId = GetTransactionsContainerId(transTime);
            var transId = $"Donation-{transTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}";

            var transactionContent = _contentService.Create(transId, containerId, DonationTransTypeAlias);
            transactionContent.SetValue(nameof(DonationTransaction.TransactionId).ToFirstLower()/*"transactionId"*/, transId);
            transactionContent.SetValue(nameof(DonationTransaction.Amount).ToFirstLower()/*"amount"*/, request.Amount);
            transactionContent.SetValue(nameof(DonationTransaction.Paymentor).ToFirstLower(), request.DonorName);
            transactionContent.SetValue(nameof(DonationTransaction.DonorEmail).ToFirstLower()/*"donorEmail"*/, request.DonorEmail);
            transactionContent.SetValue(nameof(DonationTransaction.Message).ToFirstLower()/*"message"*/, request.Message);
            transactionContent.SetValue(nameof(DonationTransaction.StartTime).ToFirstLower()/*"transactionTime"*/, transTime);
            transactionContent.SetValue(nameof(DonationTransaction.Status).ToFirstLower()/*"status"*/, "Pending");
            transactionContent.SetValue(nameof(DonationTransaction.PaymentGate).ToFirstLower(), request.PayGate);

            _contentService.Save(transactionContent);
            //TODO: cache dictionary of TransactionId & Content Id for latter processing
            return transId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DonationTransaction content");
            throw;
        }
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
        // Get/Create top-level Donation Transactions container
        var rootContainerId = GetOrCreateRootContainer();
        int keyYearMonth = transDate.Year + transDate.Month;

        return dictContainerTree.GetOrAdd(keyYearMonth, (a) =>
        {
            var containerMonth = transDate.Month.ToString("D2"); // likes 02, 12
            var containerYear = transDate.Year.ToString("D4");

            // year folder
            var yearContainered = _contentService.GetPagedChildren(rootContainerId, 0, 100, out _/*, QueryContainerContentType*/)
                .FirstOrDefault(x => x.Name == containerYear);
            if (yearContainered == null)
            {
                try
                {
                    yearContainered = _contentService.Create(containerYear, rootContainerId, TransContainerTypeAlias);
                    _contentService.SaveAndPublish(yearContainered);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Year Container");
                    throw;
                }
            }

            // month folder, should 12 months under each year
            var monthContainered = _contentService.GetPagedChildren(yearContainered.Id, 0, 20, out _/*, QueryContainerContentType*/)
                .FirstOrDefault(x => x.Name == containerMonth);
            if (monthContainered == null)
            {
                try
                {
                    monthContainered = _contentService.Create(containerMonth, yearContainered.Id, TransContainerTypeAlias);
                    _contentService.SaveAndPublish(monthContainered);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Month Container");
                    throw;
                }
            }

            return monthContainered.Id;
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
                    transRootContainer = _contentService.Create("Donation Transactions", -1, TransContainerTypeAlias);
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

        //TODO: fine tune transaction finding
        // Find transaction by transactionId
        contentTrans = _contentService.GetPagedChildren(containerId, 0, 100, out _/*, QueryTransactionContents(date)*/)
            .FirstOrDefault(x => x.GetValue<string>("transactionId") == transactionId);
        if (contentTrans == null)
        {
            _logger.LogWarning("Transaction {transactionId} in {date:yyyyMMdd} not found", transactionId, date);
            if (tryExtend) // try extend finding in a day before Container
            {                
                return TryGetTransactionContent(transactionId, date: date.AddDays(-1), out contentTrans, false);
            }
        }

        return contentTrans != null;
    }
}