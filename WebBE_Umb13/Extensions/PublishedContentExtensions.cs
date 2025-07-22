using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace YeahWeb.Extensions;

public static class PublishedContentExtensions
{
    public static Dictionary<string, string> GetBlocksWithGroupNames(this IPublishedContent contentItem, string gridAlias)
    {
        var blocks = new Dictionary<string, string>();

        var contentProperty = contentItem.Properties.FirstOrDefault(x => x.Alias == gridAlias);
        if (contentProperty == null) return blocks;

        var config = contentProperty.PropertyType.DataType.Configuration as BlockGridConfiguration;
        if (config == null || config.BlockGroups == null) return blocks;

        var blockGroupDictionary = config.BlockGroups.ToDictionary(item => item.Key.ToString(), item => item.Name);

        foreach (var item in config.Blocks)
        {
            blocks.Add(item.ContentElementTypeKey.ToString(), blockGroupDictionary.GetValue(item.GroupKey));
        }

        return blocks;
    }

    public static IPublishedContent? GetCurrentPage(this IUmbracoContextAccessor contextAccessor) => contextAccessor.GetRequiredUmbracoContext()!.PublishedRequest!.PublishedContent;
    public static IPublishedContent GetHomepage(this UmbracoHelper umbHelper) => umbHelper.ContentAtRoot().First(c => c.ContentType.Alias == HomePage.ModelTypeAlias);

    public static Dictionary<string, Tuple<string, string, string>> GetLocalizedLangInfos(this UmbracoHelper umbHelper, IPublishedContent? currentPage)
    {
        var homePage = GetHomepage(umbHelper);
        // Item's key is the ISO code of language added (through Settings\Languages)
        var culPairs = homePage.Cultures.Select(i => Tuple.Create(i.Key.ToLower(), CultureInfo.GetCultureInfo(i.Key)));

        Dictionary<string, Tuple<string, string, string>> dictLangInfos = [];
        foreach (var pair in culPairs)
        {
            var flag = umbHelper.GetDictionaryValueOrDefault("languageFlag", pair.Item2, "N/A");
            var langUrl = currentPage?.Url(pair.Item1, UrlMode.Relative) ?? homePage.Url(pair.Item1, UrlMode.Relative);
            dictLangInfos.Add(pair.Item1, Tuple.Create(pair.Item2.NativeName, langUrl, flag));
        }

        return dictLangInfos;
    }
}
