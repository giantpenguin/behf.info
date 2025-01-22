using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace YeahWeb.Infra;

public class CustomComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        PatchLicense.DoPatch();
    }
}