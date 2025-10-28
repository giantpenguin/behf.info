using Examine;
using Examine.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.PublishedModels;
using YeahWeb.Extensions;
using YeahWeb.Models;

namespace YeahWeb.Services;

public class ContentQueryService(ILogger<ContentQueryService> logger, IExamineManager examineManager
    , IPublishedContentQuery publishedContentQuery, IPageViewModelBuildService pageViewModelService)
{
    private readonly IExamineManager _examineManager = examineManager;
    private readonly IPublishedContentQuery _publishedContentQuery = publishedContentQuery;
    private readonly IPageViewModelBuildService _pageViewModelService = pageViewModelService;

    public IEnumerable<ProgramInfoItemViewModel> GetAllEvents(int pageNum = 1, int pageSize = 20)
    {
        IList<SortableField>? sortFields = [ new SortableField(Globals.ConstProps.EventLatestTime) ];
        var contentEvents = QueryContents(PageEventInfo.ModelTypeAlias, sortFields: sortFields, pageNum: pageNum, pageSize: pageSize);

        return contentEvents.Any() ? [.. contentEvents.Select(_pageViewModelService.BuildProgramInfoViewModel)] : [];
    }

    public IEnumerable<ProgramInfoItemViewModel> GetUpcomingEvents()
    {
        var contentEvents = GetUpcomingContents(PageEventInfo.ModelTypeAlias);
        return contentEvents.Any() ? [.. contentEvents.Select(_pageViewModelService.BuildProgramInfoViewModel)] : [];
    }

    public IEnumerable<IPublishedContent> GetUpcomingContents(string typeAlias)
    {
        var indexName = Constants.UmbracoIndexes.ExternalIndexName;
        if (!_examineManager.TryGetIndex(indexName, out IIndex index))
        {
            throw new InvalidOperationException($"No index '{indexName}' found.");
        }

        var query = index.Searcher.CreateQuery(IndexTypes.Content);
        var queryExecutor = query.NodeTypeAlias(typeAlias)
            .And(sub => sub.RangeQuery<DateTime>([Globals.ConstProps.EventLatestTime], DateTime.Today, null, minInclusive: true, maxInclusive: false));

        List<IPublishedContent> publishedContents = [];
        var result = _publishedContentQuery.Search(queryExecutor);
        publishedContents.AddRange(
            result.Where(i => i.Content.IsPublished()).Select(i => i.Content));
        //foreach (var item in result)
        //{
        //    var content = item.Content;
        //    if (!content.IsPublished())
        //        continue;

        //    publishedContents.Add(content);
        //}

        return publishedContents;
    }

    public IEnumerable<IPublishedContent> QueryContents(string typeAlias, string? term = null, IEnumerable<SortableField>? sortFields = null, int pageNum = 1, int pageSize = 20)
    {
        var indexName = Constants.UmbracoIndexes.ExternalIndexName;
        if (!_examineManager.TryGetIndex(indexName, out IIndex index))
        {
            throw new InvalidOperationException($"No index '{indexName}' found.");
        }

        int skip = (pageNum - 1) * pageSize;
        int take = pageSize;

        var query = index.Searcher.CreateQuery(IndexTypes.Content);
        IOrdering queryExecutor = query.NodeTypeAlias(typeAlias);
        if (sortFields?.Any() == true)
        {
            queryExecutor = queryExecutor.OrderBy(sortFields.ToArray());
        }

        List<IPublishedContent> publishedContents = [];
        var result = _publishedContentQuery.Search(queryExecutor, skip, take, out var totalCount);
        publishedContents.AddRange(
           result.Where(i => i.Content.IsPublished()).Select(i => i.Content));

        return publishedContents;
    }
}
