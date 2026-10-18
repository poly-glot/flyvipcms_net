using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("crud-field", Attributes = "definition", TagStructure = TagStructure.WithoutEndTag)]
public sealed class CrudFieldTagHelper : TagHelper
{
    public CrudField Definition { get; set; } = null!;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var property = ViewContext.ViewData.ModelExplorer.Properties.Single(candidate => candidate.Metadata.BinderModelName == Definition.Name);

        if (Definition.Type == CrudInputType.Number)
        {
            output.Attributes.SetAttribute("step", "any");
        }

        new FieldTagHelper
        {
            For = new ModelExpression(Definition.Name, property),
            Label = Definition.Label,
            Type = Definition.InputType,
            Items = Definition.Options ?? [],
            Blank = Definition.Type == CrudInputType.Select ? "—" : null,
            FieldClass = Definition.Type == CrudInputType.Textarea ? "field--full" : "",
            ViewContext = ViewContext,
        }.Process(context, output);
    }
}
