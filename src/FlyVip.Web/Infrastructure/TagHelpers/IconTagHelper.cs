using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("icon", Attributes = "name", TagStructure = TagStructure.WithoutEndTag)]
public sealed class IconTagHelper : TagHelper
{
    public string Name { get; set; } = "";

    [HtmlAttributeName("class")]
    public string CssClass { get; set; } = "icon";

    public static IHtmlContent Svg(string name, string cssClass)
    {
        var svg = HtmlElement.Create("svg");
        Fill(svg, name, cssClass);

        return svg;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output) => Fill(output, Name, CssClass);

    private static void Fill(TagHelperOutput output, string name, string cssClass)
    {
        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", cssClass);
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute("focusable", "false");
        output.Content.AppendHtml("<use href=\"#i-").Append(name).AppendHtml("\"/>");
    }
}
