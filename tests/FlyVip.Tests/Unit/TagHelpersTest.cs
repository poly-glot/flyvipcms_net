using System.Text.Encodings.Web;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FlyVip.Tests.Unit;

public sealed class TagHelpersTest
{
    private static readonly EmptyModelMetadataProvider Metadata = new();

    [Fact]
    public void DisplayHelpersRenderTheSourceMarkup()
    {
        Assert.Equal("<svg class=\"icon icon--sm\" aria-hidden=\"true\" focusable=\"false\"><use href=\"#i-x\"/></svg>", Render(new IconTagHelper { Name = "x", CssClass = "icon icon--sm" }));
        Assert.Equal("<span class=\"avatar \" aria-hidden=\"true\">AP</span>", Render(new AvatarTagHelper { First = "ana", Last = "personal" }));
        Assert.Equal(
            "<span class=\"pips\" role=\"img\" aria-label=\"1 of 2 seats\"><i class=\"pip pip--filled\"></i><i class=\"pip\"></i></span>",
            Render(new PipsTagHelper { Filled = 1, Capacity = 2 }));
        Assert.Equal(
            "<svg class=\"sparkline\" viewBox=\"0 0 160 40\" width=\"160\" height=\"40\" role=\"img\" aria-label=\"Balance trend\"><polygon class=\"sparkline__area\" points=\"0,40 0.0,37.0 160.0,3.0 160,40\"/><polyline class=\"sparkline__line\" points=\"0.0,37.0 160.0,3.0\"/></svg>",
            Render(new SparklineTagHelper { Values = [0m, 10m] }));
        Assert.Equal("", Render(new SparklineTagHelper { Values = [1m] }));
    }

    [Fact]
    public void FormControlsKeepTheirFormNamesValuesAndErrors()
    {
        var view = new ViewContext { ViewData = new ViewDataDictionary(Metadata, new ModelStateDictionary()) };
        view.ViewData.ModelState.SetModelValue("plan_id", "2", "2");
        view.ViewData.ModelState.AddModelError("plan_id", "The plan_id field is required.");
        KeyValuePair<string, string>[] plans = [new("1", "Personal"), new("2", "Family")];

        Assert.Equal(
            "<div class=\"field  field--invalid\"><label for=\"f_plan_id\">Plan</label><select id=\"f_plan_id\" name=\"plan_id\" required aria-describedby=\"f_plan_id_error\" aria-invalid=\"true\"><option value=\"\">—</option><option value=\"1\">Personal</option><option value=\"2\" selected>Family</option></select><p class=\"field__error\" id=\"f_plan_id_error\"><svg class=\"icon icon--sm\" aria-hidden=\"true\" focusable=\"false\"><use href=\"#i-alert\"/></svg>The plan_id field is required.</p></div>",
            Render(new FieldTagHelper { For = For("plan_id", 1), Label = "Plan", Type = "select", Items = plans, Blank = "—", ViewContext = view }, new TagHelperAttribute("required")));
        Assert.Equal(
            "<div class=\"field field--full\"><label for=\"f_notes\">Notes</label><textarea id=\"f_notes\" name=\"notes\" rows=\"3\" aria-describedby=\"f_notes_hint\">a &amp; b</textarea><p class=\"field__hint\" id=\"f_notes_hint\">Optional.</p></div>",
            Render(new FieldTagHelper { For = For("notes", "a & b"), Label = "Notes", Type = "textarea", Hint = "Optional.", FieldClass = "field--full", ViewContext = view }));
        Assert.Equal(
            "<label class=\"switch\"><input type=\"checkbox\" role=\"switch\" name=\"active\" value=\"1\" checked><span class=\"switch__track\" aria-hidden=\"true\"></span><span class=\"switch__text\">Active</span></label>",
            Render(new FieldTagHelper { For = For("active", true), Label = "Active", Type = "checkbox", ViewContext = view }));
        Assert.Equal(
            "<div class=\"field\"><label for=\"f_flight_date\">Flight date</label><input id=\"f_flight_date\" type=\"date\" name=\"flight_date\" value=\"2026-04-05\" required></div>",
            Render(new FieldTagHelper { For = For("flight_date", new DateOnly(2026, 4, 5)), Label = "Flight date", Type = "date", ViewContext = view }, new TagHelperAttribute("required", "required")));
        Assert.Equal(
            "<fieldset class=\"segmented\"><legend>Kind</legend><div class=\"segmented__track\"><label class=\"segmented__option\"><input type=\"radio\" name=\"kind\" value=\"complete\"><span>Complete</span></label><label class=\"segmented__option\"><input type=\"radio\" name=\"kind\" value=\"partial\" checked><span>Partial</span></label></div></fieldset>",
            Render(new SegmentedTagHelper { For = For("Kind", "partial"), Name = "kind", Legend = "Kind", Items = [new("complete", "Complete"), new("partial", "Partial")], ViewContext = view }));
    }

    private static ModelExpression For<T>(string name, T value) => new(name, Metadata.GetModelExplorerForType(typeof(T), value));

    private static string Render(TagHelper helper, params TagHelperAttribute[] attributes)
    {
        var context = new TagHelperContext([.. attributes], new Dictionary<object, object>(), "test");
        var output = new TagHelperOutput("test", [.. attributes], (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        helper.Process(context, output);

        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All));

        return writer.ToString();
    }
}
