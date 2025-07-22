using Umbraco.Forms.Core.Providers.FieldTypes;
using Umbraco.Forms.Core.Attributes;

namespace YeahWeb.FormsExt;

public class PlainNote : Text
{
    public PlainNote()
    {
        // new name
        Name = "Plain Note";
        Description = "Simple short note";

        // different view for this fieldtype
        FieldTypeViewName = "FieldType.PlainNote.cshtml";
    }

    [Setting("Css Classes", Description = "Css assign to class attribute", View = "TextField")]
    public string CssClasses { get; set; } = string.Empty;

    [Setting("Css Styles", Description = "Style assign to style attribute", View = "TextField")]
    public string CssStyles { get; set; } = string.Empty;
}