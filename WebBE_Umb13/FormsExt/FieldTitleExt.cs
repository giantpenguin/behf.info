using Umbraco.Forms.Core.Providers.FieldTypes;
using Umbraco.Forms.Core.Attributes;

namespace YeahWeb.FormsExt;

public class FieldTextExt : Text
{
    public FieldTextExt()
    {
        // new name
        Name = "Headline Text ext";
        Description = "Title and description's extension";

        // different view for this fieldtype
        FieldTypeViewName = "FieldType.TextExt.cshtml";
    }

    [Setting("Css Classes", Description = "Css assign to class attribute", View = "TextField")]
    public string CssClasses { get; set; } = string.Empty;

    [Setting("Css Styles", Description = "Style assign to style attribute", View = "TextField")]
    public string CssStyles { get; set; } = string.Empty;
}