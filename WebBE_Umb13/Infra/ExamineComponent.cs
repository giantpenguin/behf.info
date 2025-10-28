using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Options;
using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.PublishedModels;
using YeahWeb.Extensions;

namespace YeahWeb.Infra;

public class ExamineComponent(ILogger<ExamineComponent> logger, IExamineManager examineManager, IContentService contentService) : IComponent
{
    private readonly ILogger<ExamineComponent> _logger = logger;
    private readonly IExamineManager _examineManager = examineManager;
    private readonly IContentService _contentService = contentService;

    public void Initialize()
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out IIndex index))
        {
            throw new InvalidOperationException(
                $"No index {Constants.UmbracoIndexes.ExternalIndexName} found.");
        }

        if (index is not BaseIndexProvider indexProvider)
            throw new InvalidOperationException("Not a BaseIndexProvider");

        indexProvider.TransformingIndexValues += IndexProviderTransformingIndexValues;

    }

    private void IndexProviderTransformingIndexValues(object? sender, IndexingItemEventArgs e)
    {
        var valset = e.ValueSet;

        if (int.TryParse(valset.Id, out var nodeId))
        {
            switch (valset.ItemType)
            {
                // try adding LocationsTimelines to index in a searchable format for usable in query service
                case PageEventInfo.ModelTypeAlias:
                    var content = _contentService.GetById(nodeId);

                    var defLanguage = CultureInfo.DefaultThreadCurrentCulture.TwoLetterISOLanguageName;
                    var jsonTimelines = content?.GetValue<string>(Globals.ConstProps.LocationsTimelines, defLanguage);
                    if (string.IsNullOrEmpty(jsonTimelines))
                    {
                        _logger.LogWarning("No value from {propName} of content Id {nodeId}.", Globals.ConstProps.LocationsTimelines, nodeId);
                        break;
                    }

                    BlockEditorData blockData = BlockEditorData.Empty;
                    var converter = new BlockListEditorDataConverter();
                    try
                    {
                        blockData = converter.Deserialize(jsonTimelines);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserialize {propName} of content Id {nodeId}.", Globals.ConstProps.LocationsTimelines, nodeId);
                    }

                    var timelines = blockData?.BlockValue.ContentData.Select(i => {
                        var attempt = i.RawPropertyValues[Globals.ConstProps.TimeStart].TryConvertTo<DateTime?>();
                        return attempt.Success && attempt.Result != null ? attempt.Result.Value : DateTime.MinValue;
                    }).Order().ToList();

                    if (timelines?.Count > 0)
                    {
                        var values = valset.Values.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value);
                        // Add new key to the dictionary
                        values.TryAdd(Globals.ConstProps.EventEarliestTime, [timelines.FirstOrDefault()]);
                        values.TryAdd(Globals.ConstProps.EventLatestTime, [ timelines.LastOrDefault()]);

                        e.SetValues(values);
                    }

                break;
            }
        }
    }

    public void Terminate()
    {
        // do not do anything
    }

}


public class ConfigureExternalIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    public void Configure(string? name, LuceneDirectoryIndexOptions options)
    {
        if (!string.IsNullOrEmpty(name) && name.Equals(Constants.UmbracoIndexes.ExternalIndexName))
        {
            options.Validator = new ContentValueSetValidator(true);

            options.FieldDefinitions.AddOrUpdate(new FieldDefinition(Globals.ConstProps.EventEarliestTime, FieldDefinitionTypes.DateTime));
            options.FieldDefinitions.AddOrUpdate(new FieldDefinition(Globals.ConstProps.EventLatestTime, FieldDefinitionTypes.DateTime));
        }
    }

    public void Configure(LuceneDirectoryIndexOptions options)
    {
        // Part of the interface, but does not need to be implemented for this.
        throw new NotImplementedException();
    }
}