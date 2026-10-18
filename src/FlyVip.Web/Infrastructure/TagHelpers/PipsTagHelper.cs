using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("pips", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PipsTagHelper : TagHelper
{
    private const int MaxSeats = 40;

    public int Filled { get; set; }

    public int Capacity { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var total = Math.Clamp(Capacity, 1, MaxSeats);

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "pips");
        output.Attributes.SetAttribute("role", "img");
        output.Attributes.SetAttribute("aria-label", string.Create(CultureInfo.InvariantCulture, $"{Filled} of {total} seats"));

        for (var seat = 1; seat <= total; ++seat)
        {
            output.Content.AppendHtml(HtmlElement.Create("i", new TagHelperAttribute("class", seat <= Filled ? "pip pip--filled" : "pip")));
        }
    }
}
