using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("avatar", TagStructure = TagStructure.WithoutEndTag)]
public sealed class AvatarTagHelper : TagHelper
{
    public string First { get; set; } = "";

    public string Last { get; set; } = "";

    public string Size { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "avatar " + Size);
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Content.Append(UiHelpers.UiInitials(First, Last));
    }
}
