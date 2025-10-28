namespace YeahWeb.Extensions;

public static class Globals
{

    public static class Constants
    {
        public const int ThumbnailWidth = 500;
        public static readonly DateTime SiteMinDateTime = new(2020,1,1,0,0,0);

        public const string ReadMoreText = "Read More";
        public const string ReadMoreTransKey = "buttonReadMore";
        public const string EnrollNowText = "Enroll Now";
        public const string EnrollNowTransKey = "buttonEnrollNow";
        public const string RegisterText = "Register";
        public const string RegisterTransKey = "buttonRegister";

    }

    /// <summary>
    /// This contains some properties' name for easier accessing related Document/Block types
    /// </summary>
    public static class ConstProps
    {
        public const string ContentLayoutSource = "contentLayout";    // IPageContentLayout.ContentLayout property (BlockGrid editor)
        
        //ProgramInfo
        public const string LocationsTimelines = "locationsTimelines";

        // addition fields to Examine
        public const string EventEarliestTime = "eventEarliestTime";
        public const string EventLatestTime = "eventLatestTime";

        //ItemLocationTimeline
        public const string TimeStart = "timeStart";
    }
}
