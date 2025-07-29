using Umbraco.Forms.Core.Providers.FieldTypes;
using Umbraco.Forms.Core.Attributes;

namespace YeahWeb.Forms;

/// <summary>
/// reuse the Textfield type to have own view template
/// </summary>
public class TextareaExt : Textarea
{
    public TextareaExt()
    {
        Alias = "textareaExt";
        FieldTypeViewName = "TextareaExt.cshtml";
        EditView = "TextareaExt.html";
    }

    [Setting("Form text", Description = "Description text for the input", SupportsPlaceholders = true, DisplayOrder = 100, View = "TextField")]
    public string FormText { get; set; } = string.Empty;

    [Setting("Left input-group-text", Description = "Text for left input-group-text", DisplayOrder = 110, View = "TextField")]
    public string LeftInputGroupText { get; set; } = string.Empty;
}