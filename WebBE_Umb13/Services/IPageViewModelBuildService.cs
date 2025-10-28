using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.PublishedModels;
using YeahWeb.Extensions;
using YeahWeb.Models;

namespace YeahWeb.Services;


//NOTE: this is a simple version handling some document types
//TODO: there should be a specific build service to handle a given document type e.g., PagePartialViewModelBuildService<PageType>
public class PageViewModelBuildService(ILogger<ContentQueryService> logger, IPublishedUrlProvider publishedUrlProvider) : IPageViewModelBuildService
{
    private readonly IPublishedUrlProvider _publishedUrlProvider = publishedUrlProvider;

    public T BuildPartial<T>(IPublishedContent srcContent) where T : PagePartialViewModel
    {
        throw new NotImplementedException();
    }

    public virtual PagePartialViewModel BuildCommonPartial(IPublishedContent srcContent)
    {
        var vwModel = new PagePartialViewModel
        {
            Title = srcContent.Value<string>("title") ?? srcContent.Name,
            Description = srcContent.Value<string>("description")
        };

        vwModel.PageUrl = srcContent.Url(_publishedUrlProvider);

        var imgThumb = srcContent.Value<IPublishedContent>("thumbnailImage", fallback: Fallback.ToDefaultLanguage);
        vwModel.ThumbnailUrl = imgThumb?.GetCropUrl(Globals.Constants.ThumbnailWidth);

        return vwModel;
    }

    public ProgramInfoItemViewModel BuildProgramInfoViewModel(IPublishedContent contentProgram)
    {
        var genModel = BuildCommonPartial(contentProgram);
        ProgramInfoItemViewModel vwModel = new(genModel);

        var progTitle =    contentProgram.Value<string>("programTitle");
        if (!string.IsNullOrWhiteSpace(progTitle))
        {
            vwModel.Title = progTitle;
        }

        if (contentProgram.HasValue("excerpt"))
        {
            vwModel.Excerpt = new HtmlString(contentProgram.Value("excerpt").ToString());
        }

        var loctimeList = contentProgram.Value<BlockListModel>(Globals.ConstProps.LocationsTimelines);
        vwModel.Schedule = loctimeList?.Select(i => BuildLocationTimeline(i.Content as ItemLocationTimeline))
            .WhereNotNull().ToList() ?? [];

        return vwModel;
    }

    private static LocationTimelineModel? BuildLocationTimeline(ItemLocationTimeline? locTimeline)
    {
        if (string.IsNullOrWhiteSpace(locTimeline?.LocationName) || locTimeline?.TimeStart <= Globals.Constants.SiteMinDateTime)
            return null;

        return new LocationTimelineModel()
        {
            Location = locTimeline.LocationName,
            LocationDetail = locTimeline.LocationDetail,
            StartTime = locTimeline.TimeStart,
            EndTime = locTimeline.TimeEnd,
            Note = locTimeline.Note
        };
    }
}


public interface IPageViewModelBuildService
{
    public PagePartialViewModel BuildCommonPartial(IPublishedContent srcContent);

    public ProgramInfoItemViewModel BuildProgramInfoViewModel(IPublishedContent srcContent);

    public T BuildPartial<T>(IPublishedContent srcContent) where T : PagePartialViewModel;
}
