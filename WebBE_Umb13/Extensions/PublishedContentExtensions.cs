using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;
using static Umbraco.Cms.Core.PropertyEditors.ValueConverters.ColorPickerValueConverter;

namespace YeahWeb.Extensions;

public static class PublishedContentExtensions
{
    private static readonly string ValidDocIdPattern = @"^(-?\d+|[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}|umb://document/[0-9a-f]{32})$";
    private static readonly Regex regexValidDocId = new(ValidDocIdPattern);

    public static bool IsValidDocId(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return regexValidDocId.IsMatch(input);
    }

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

    public static IPublishedContent? GetDocument(this UmbracoHelper umbHelper, string? pageId)
    {
        return IsValidDocId(pageId) ? umbHelper.Content(pageId) : null;
    }

    public static string? GetProgramTitle(this UmbracoHelper umbHelper, string? pageId)
    {
        if (!IsValidDocId(pageId) || GetDocument(umbHelper, pageId) is not IProgramInfo progInfo)
            return string.Empty;

        return progInfo.ProgramTitle;        
    }


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

    /// <summary>
    /// Return a string likes "background-color: rgba(var(--bs-blue-rgb-300),.375);"
    /// </summary>
    public static string GetBgrRGBAForStyle(this IPublishedElement? currentContent, string propBgrColor = "backgroundColor", string propColorShade = "backgroundColorShade", string propColorOpa = "backgroundColorOpacity")
    {
        var colorLabel = GetBgrPickedColor(currentContent, propBgrColor)?.Label;
        if (string.IsNullOrEmpty(colorLabel))
            return "";

        var colorShade = GetBgrColorShade(currentContent, propColorShade);
        var colorOpa = GetBgrColorOpaFormat(currentContent, propColorOpa);
        return $"background-color: rgba(var({colorLabel}-{colorShade}),{colorOpa});";
    }

    public static PickedColor? GetBgrPickedColor(this IPublishedElement? currentContent, string propName = "backgroundColor")
    {
        return currentContent?.Value<PickedColor>(propName);
    }

    public static int GetBgrColorShade(this IPublishedElement? currentContent, string propName = "backgroundColorShade", int defVal = 500)
    {
        return currentContent?.Value<int>(propName) ?? defVal;
    }

    public static double GetBgrColorOpa(this IPublishedElement? currentContent, string propName = "backgroundColorOpacity", double defVal = 1)
    {
        return currentContent?.Value<double>(propName) ?? defVal;
    }

    public static string GetBgrColorOpaFormat(this IPublishedElement? currentContent, string propName = "backgroundColorOpacity", double defVal = 1)
    {
        var bgrOpa = currentContent.GetBgrColorOpa(propName, defVal);
        return bgrOpa.ToStringFormat();
    }

    public static string GetButtonText(this UmbracoHelper umbHelper, IPublishedElement? currentContent, string translationKey = "buttonReadMore", string fallback = "Read More")
    {
        var readMore = currentContent is IItemSettings itmSettings ? itmSettings.TextReadMore : "";

        return string.IsNullOrEmpty(readMore) ? umbHelper.GetDictionaryValueOrDefault(translationKey, fallback) : readMore;
    }

    public const string FormAdditionalDataKey = "formAdditionalData";
    /// <summary>
    /// Get specific formAdditionalData for Forms from <seealso cref="ViewContext"/>
    /// </summary>
    public static Dictionary<string, string> AddFormAdditionalData(this ViewContext viewContext, string key, string val)
    {
        return AddFormAdditionalData(viewContext.ViewData, key, val);
    }

    public static Dictionary<string, string> AddFormAdditionalData(this ViewDataDictionary viewData, string key, string val)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(key));

        var additionalData = viewData.GetValue(FormAdditionalDataKey) as Dictionary<string, string> ?? [];
        if (!string.IsNullOrWhiteSpace(val))
        {
            additionalData.TryAdd(key, val);
        }

        viewData[FormAdditionalDataKey] = additionalData;
        return additionalData;
    }
}
