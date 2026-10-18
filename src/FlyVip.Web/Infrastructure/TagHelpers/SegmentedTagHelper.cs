using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("segmented", Attributes = "asp-for", TagStructure = TagStructure.WithoutEndTag)]
public sealed class SegmentedTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;

    public string? Name { get; set; }

    public string Legend { get; set; } = "";

    public IEnumerable<KeyValuePair<string, string>> Items { get; set; } = [];

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var name = FieldTagHelper.FormName(For, Name);
        var current = FieldTagHelper.Current(ViewContext, name, For.Model);

        var legend = HtmlElement.Create("legend");
        legend.Content.Append(Legend);

        var track = HtmlElement.Create("div", new TagHelperAttribute("class", "segmented__track"));

        foreach (var (value, text) in Items)
        {
            track.Content.AppendHtml(Option(name, value, text, value == current));
        }

        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "segmented");
        output.Content.AppendHtml(legend).AppendHtml(track);
    }

    private static TagHelperOutput Option(string name, string value, string text, bool isChecked)
    {
        var radio = HtmlElement.Void("input", new TagHelperAttribute("type", "radio"), new TagHelperAttribute("name", name), new TagHelperAttribute("value", value));

        if (isChecked)
        {
            radio.Attributes.Add(new TagHelperAttribute("checked"));
        }

        var caption = HtmlElement.Create("span");
        caption.Content.Append(text);

        var option = HtmlElement.Create("label", new TagHelperAttribute("class", "segmented__option"));
        option.Content.AppendHtml(radio).AppendHtml(caption);

        return option;
    }
}
