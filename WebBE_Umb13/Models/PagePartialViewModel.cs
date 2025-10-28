using Microsoft.AspNetCore.Html;

namespace YeahWeb.Models
{
    public class PagePartialViewModel
    {        
        public string Title { get; set; }

        public string? Description { get; set; } = string.Empty;

        public HtmlString Excerpt { get; set; } = HtmlString.Empty;

        public string? PageUrl { get; set; }

        public string? ContentType { get; set; } //e.g., Workshop, Community Talk, Course

        public string? ThumbnailUrl { get; set; }
    }

    public class ProgramInfoItemViewModel : PagePartialViewModel
    {
        public ProgramInfoItemViewModel() { }

        public ProgramInfoItemViewModel(PagePartialViewModel vwModel) 
        {
            Title = vwModel.Title;
            Description = vwModel.Description;
            Excerpt = vwModel.Excerpt;
            PageUrl = vwModel.PageUrl;
            ContentType = vwModel.ContentType;
            ThumbnailUrl = vwModel.ThumbnailUrl;
        }

        public IList<LocationTimelineModel> Schedule { get; set; } = [];
    }

    public class LocationTimelineModel
    {
        public string Location { get; set; }

        public string? LocationDetail { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Note { get; set; }
    }
}
