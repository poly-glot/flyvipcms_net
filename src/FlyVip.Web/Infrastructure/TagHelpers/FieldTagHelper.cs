using System.Globalization;
using FlyVip.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Web.Infrastructure;

[HtmlTargetElement("field", Attributes = "asp-for", TagStructure = TagStructure.WithoutEndTag)]
public sealed class FieldTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;

    public string Label { get; set; } = "";

    public string Type { get; set; } = "text";

    public string Hint { get; set; } = "";

    [HtmlAttributeName("class")]
    public string FieldClass { get; set; } = "";

    public IEnumerable<KeyValuePair<string, string>> Items { get; set; } = [];

    public string? Blank { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public static string FormName(ModelExpression expression, string? name) =>
        name ?? expression.Metadata.BinderModelName ?? expression.Name;

    public static string Current(ViewContext viewContext, string name, object? model) =>
        viewContext.ViewData.ModelState[name]?.AttemptedValue ?? Text(model);

    public static TagHelperAttributeList PassThrough(TagHelperAttributeList attributes) =>
    [
        .. attributes.Select(attribute => Convert.ToString(attribute.Value, CultureInfo.InvariantCulture) == attribute.Name
            ? new TagHelperAttribute(attribute.Name)
            : attribute),
    ];

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var name = FormName(For, null);
        var current = Current(ViewContext, name, For.Model);
        var extra = PassThrough(output.Attributes);

        output.Attributes.Clear();
        output.TagMode = TagMode.StartTagAndEndTag;

        if (Type == "checkbox")
        {
            Switch(output, name, current == "1", extra);
            return;
        }

        var id = "f_" + name;
        var error = ValidationErrors.For(ViewContext.ViewData, name);
        Describe(extra, Hint != "" ? id + "_hint" : "", error != "" ? id + "_error" : "");

        output.TagName = "div";
        output.Attributes.SetAttribute("class", ("field " + FieldClass + (error != "" ? " field--invalid" : "")).Trim());

        var label = HtmlElement.Create("label", new TagHelperAttribute("for", id));
        label.Content.Append(Label);
        output.Content.AppendHtml(label).AppendHtml(Control(id, name, current, extra));

        if (Hint != "")
        {
            var hint = HtmlElement.Create("p", new TagHelperAttribute("class", "field__hint"), new TagHelperAttribute("id", id + "_hint"));
            hint.Content.Append(Hint);
            output.Content.AppendHtml(hint);
        }

        if (error != "")
        {
            var message = HtmlElement.Create("p", new TagHelperAttribute("class", "field__error"), new TagHelperAttribute("id", id + "_error"));
            message.Content.AppendHtml(IconTagHelper.Svg("alert", "icon icon--sm")).Append(error);
            output.Content.AppendHtml(message);
        }
    }

    private static void Describe(TagHelperAttributeList extra, string hintId, string errorId)
    {
        var described = string.Join(' ', new[] { hintId, errorId }.Where(id => id != ""));

        if (described != "" && !extra.ContainsName("aria-describedby"))
        {
            extra.Add("aria-describedby", described);
        }

        if (errorId != "" && !extra.ContainsName("aria-invalid"))
        {
            extra.Add("aria-invalid", "true");
        }
    }

    private TagHelperOutput Control(string id, string name, string current, TagHelperAttributeList extra)
    {
        TagHelperAttributeList attributes = [new("id", id)];

        if (Type is not ("select" or "textarea"))
        {
            attributes.Add("type", Type);
        }

        attributes.Add("name", name);

        if (Type == "textarea")
        {
            attributes.Add("rows", "3");
        }
        else if (Type is not ("select" or "password"))
        {
            attributes.Add("value", current);
        }

        foreach (var attribute in extra)
        {
            attributes.Add(attribute);
        }

        return Type switch
        {
            "select" => Select(attributes, current),
            "textarea" => TextArea(attributes, current),
            _ => HtmlElement.Void("input", attributes),
        };
    }

    private TagHelperOutput Select(TagHelperAttributeList attributes, string current)
    {
        var select = HtmlElement.Create("select", attributes);

        if (Blank is not null)
        {
            select.Content.AppendHtml(Option("", Blank, false));
        }

        foreach (var (value, text) in Items)
        {
            select.Content.AppendHtml(Option(value, text, value == current));
        }

        return select;
    }

    private static TagHelperOutput TextArea(TagHelperAttributeList attributes, string current)
    {
        var textArea = HtmlElement.Create("textarea", attributes);
        textArea.Content.Append(current);

        return textArea;
    }

    private static TagHelperOutput Option(string value, string text, bool selected)
    {
        var option = HtmlElement.Create("option", new TagHelperAttribute("value", value));

        if (selected)
        {
            option.Attributes.Add(new TagHelperAttribute("selected"));
        }

        option.Content.Append(text);

        return option;
    }

    private void Switch(TagHelperOutput output, string name, bool isChecked, TagHelperAttributeList extra)
    {
        var input = HtmlElement.Void("input", new TagHelperAttribute("type", "checkbox"), new TagHelperAttribute("role", "switch"), new TagHelperAttribute("name", name), new TagHelperAttribute("value", "1"));

        if (isChecked)
        {
            input.Attributes.Add(new TagHelperAttribute("checked"));
        }

        foreach (var attribute in extra)
        {
            input.Attributes.Add(attribute);
        }

        var text = HtmlElement.Create("span", new TagHelperAttribute("class", "switch__text"));
        text.Content.Append(Label);

        output.TagName = "label";
        output.Attributes.SetAttribute("class", "switch");
        output.Content
            .AppendHtml(input)
            .AppendHtml(HtmlElement.Create("span", new TagHelperAttribute("class", "switch__track"), new TagHelperAttribute("aria-hidden", "true")))
            .AppendHtml(text);
    }

    public static string Text(object? value) => value switch
    {
        null => "",
        string text => text,
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        bool flag => flag ? "1" : "0",
        Enum choice => EnumText.Of(choice),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };
}
