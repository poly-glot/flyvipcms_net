using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

public static class HtmlElement
{
    public static TagHelperOutput Create(string tagName, params IEnumerable<TagHelperAttribute> attributes) =>
        new(tagName, [.. attributes], (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = TagMode.StartTagAndEndTag,
        };

    public static TagHelperOutput Void(string tagName, params IEnumerable<TagHelperAttribute> attributes)
    {
        var element = Create(tagName, attributes);
        element.TagMode = TagMode.StartTagOnly;

        return element;
    }
}
