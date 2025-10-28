using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Web.Common.PublishedModels;

public partial class PageEventInfo : PublishedContentModel, IPageBase, IPageContentLayout, IPageSettings, IProgramInfo
{
    //public List<DateTime> FlattenStartTimes
    //{
    //    get
    //    {
    //        return LocationTimelines?.Select(i =>
    //        {
    //            var content = i.Content as ItemLocationTimeline;
    //            return content.TimeStart;
    //        }).Order().ToList() ?? [];
    //    }
    //}

    //[ImplementPropertyType("oldestStartTime")]
    //public string OldestStartTime     /*=> FlattenStartTimes.FirstOrDefault().ToIsoString();*/
    //{
    //    get
    //    {
    //     var timelinesModel =  ProgramInfo.GetLocationTimelines(this, _publishedValueFallback);
    //        return timelinesModel.Select(i =>
    //        {
    //            var content = i.Content as ItemLocationTimeline;
    //            return content.TimeStart;
    //        }).Order().FirstOrDefault().ToIsoString();
    //    }
    //}

    //[ImplementPropertyType("latestStartTime")]
    //public DateTime LatestStartTime => FlattenStartTimes.LastOrDefault();

}