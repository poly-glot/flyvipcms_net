using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("form")]
public sealed class AntiforgeryFirstTagHelper : TagHelper
{
    public override int Order => 1000;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!output.PostContent.IsModified)
        {
            return;
        }

        output.PostContent.MoveTo(output.PreContent);
    }
}
