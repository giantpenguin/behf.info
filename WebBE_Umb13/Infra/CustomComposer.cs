using Examine.Lucene;
using Microsoft.Extensions.Options;
using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using YeahWeb.Services;

namespace YeahWeb.Infra;

public class CustomComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        PatchLicense.DoPatch();
        builder.Dashboards().Remove<ContentDashboard>();

        builder.Components().Append<DefaultCultureComponent>();

        builder.Services.AddSingleton<IConfigureOptions<LuceneDirectoryIndexOptions>, ConfigureExternalIndexOptions>();
        builder.Services.ConfigureOptions<ConfigureExternalIndexOptions>();
        builder.Components().Append<ExamineComponent>();


        builder.Services.AddSingleton<IPageViewModelBuildService, PageViewModelBuildService>();
        builder.Services.AddScoped<ContentQueryService>();

    }
}

public class DefaultCultureComponent(IRuntimeState runtimeState, IScopeProvider scopeProvider, ILanguageRepository languageRepository) : IComponent
{
    private readonly IRuntimeState _runtimeState = runtimeState;
    private readonly IScopeProvider _scopeProvider = scopeProvider;
    private readonly ILanguageRepository _languageRepository = languageRepository;

    public void Initialize()
    {
        if (_runtimeState.Level < RuntimeLevel.Run)
            return;

        using var scope = _scopeProvider.CreateScope();
        string defaultIsoCode = _languageRepository.GetDefaultIsoCode();
        var defaultCultrue = new CultureInfo(defaultIsoCode);

        CultureInfo.DefaultThreadCurrentCulture = defaultCultrue;
        CultureInfo.DefaultThreadCurrentUICulture = defaultCultrue;
    }

    public void Terminate() { }
}