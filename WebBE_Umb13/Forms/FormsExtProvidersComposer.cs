using Umbraco.Cms.Core.Composing;
using Umbraco.Forms.Core.Providers.Extensions;

namespace YeahWeb.Forms;

public class FormsExtProvidersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.FormsFields().Add<TextfieldExt>();
        builder.FormsFields().Add<TextareaExt>();
        builder.FormsFields().Add<PlainNote>();
        //builder.FormsFields().Add<HelperField>();
    }
}
