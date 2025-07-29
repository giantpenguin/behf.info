using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core;

namespace YeahWeb.Forms;

/// <summary>
/// A way providing addition data through the FieldViewModel.AdditionalSettings a helpee field,
/// Use (helpeeField's alias + "Helper") as a convention to wire and access AdditionalSettings from the target helpee field (template)
/// </summary>
public class HelperField : FieldType, IFieldHelper
{
    public HelperField()
    {
        Id = new Guid("B7C7C78B-0858-4386-B6F5-43B251312133");
        Alias = "helperField";

        Name = "HelperField";
        Description = "Provide addtion data to helpee field";

        FieldTypeViewName = "FieldType.HelperField.cshtml";
        Icon = "icon-firewire";
        HideLabel = true;
        // this field type is not rendered
        RenderInputType = Umbraco.Forms.Core.Enums.RenderInputType.Custom;
    }

    public override bool StoresData => false;

    [Setting("Form text", Description = "Text to use with .form-text", View = "TextField")]
    public string FormText { get; set; } = string.Empty;

    [Setting("Custom Css", Description = "Addition css to class attribute", View = "TextField")]
    public string CustomCss { get; set; } = string.Empty;
}