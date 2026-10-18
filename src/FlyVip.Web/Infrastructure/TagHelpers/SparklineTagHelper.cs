using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("sparkline", Attributes = "values", TagStructure = TagStructure.WithoutEndTag)]
public sealed class SparklineTagHelper : TagHelper
{
    private const decimal Inset = 3;

    public IReadOnlyList<decimal> Values { get; set; } = [];

    public int Width { get; set; } = 160;

    public int Height { get; set; } = 40;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Values.Count < 2)
        {
            output.SuppressOutput();
            return;
        }

        var low = Values.Min();
        var span = Math.Max(Values.Max() - low, 0.01m);
        var step = (decimal)Width / (Values.Count - 1);
        var line = string.Join(' ', Values.Select((value, index) =>
            $"{OneDecimal(index * step)},{OneDecimal(Inset + ((Height - (2 * Inset)) * (1 - ((value - low) / span))))}"));

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "sparkline");
        output.Attributes.SetAttribute("viewBox", Invariant($"0 0 {Width} {Height}"));
        output.Attributes.SetAttribute("width", Invariant($"{Width}"));
        output.Attributes.SetAttribute("height", Invariant($"{Height}"));
        output.Attributes.SetAttribute("role", "img");
        output.Attributes.SetAttribute("aria-label", "Balance trend");
        output.Content
            .AppendHtml("<polygon class=\"sparkline__area\" points=\"").Append(Invariant($"0,{Height} {line} {Width},{Height}")).AppendHtml("\"/>")
            .AppendHtml("<polyline class=\"sparkline__line\" points=\"").Append(line).AppendHtml("\"/>");
    }

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    private static string OneDecimal(decimal value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0", CultureInfo.InvariantCulture);
}
