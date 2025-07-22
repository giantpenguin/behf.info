using Umbraco.Cms.Core.Composing;
using Umbraco.Forms.Core.Providers.Extensions;

namespace YeahWeb.FormsExt;

public class FormsExtProvidersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.FormsFields().Add<PlainNote>();
    }
}
