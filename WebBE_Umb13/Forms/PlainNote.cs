using Umbraco.Forms.Core.Providers.FieldTypes;
using Umbraco.Forms.Core.Attributes;

namespace YeahWeb.Forms;

public class PlainNote : Text
{
    public PlainNote()
    {
        //NOTE: reuse the Text type so that not define the Id Guid
        //Id = new Guid("C1FAC598-8F37-46EB-99C2-3F703B393D59");
        Alias = "plainNote";

        // new name
        Name = "PlainNote";
        Description = "Simple short note";

        HideLabel = true;
        RenderInputType = Umbraco.Forms.Core.Enums.RenderInputType.Custom;

        FieldTypeViewName = "PlainNote.cshtml";
        EditView = "PlainNote";
    }

    [Setting("Custom Css", Description = "Custom Css assign to class attribute", View = "TextField")]
    public string CustomCss { get; set; } = string.Empty;

    public override bool StoresData => false;
}