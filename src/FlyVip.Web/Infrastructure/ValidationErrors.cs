using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FlyVip.Web.Infrastructure;

public static class ValidationErrors
{
    public static IReadOnlyList<string> Of(ViewDataDictionary viewData)
    {
        var fieldOrder = viewData.ModelMetadata.Properties.Select(property => property.BinderModelName ?? property.PropertyName ?? "").ToList();
        var keys = fieldOrder.Concat(viewData.ModelState.Keys.Except(fieldOrder));

        return [.. keys.Select(key => viewData.ModelState[key] is { Errors: [var first, ..] } ? first.ErrorMessage : null).OfType<string>()];
    }

    public static string For(ViewDataDictionary viewData, string key) =>
        viewData.ModelState[key] is { Errors: [var first, ..] } ? first.ErrorMessage : "";
}
