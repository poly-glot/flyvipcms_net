using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Infrastructure;

public sealed class IsoDateModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var posted = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (posted == ValueProviderResult.None || posted.FirstValue is not { Length: > 0 } text)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, posted);

        if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            bindingContext.Result = ModelBindingResult.Success(date);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"The {bindingContext.ModelMetadata.GetDisplayName()} field must contain a valid date.");
        }

        return Task.CompletedTask;
    }
}

public sealed class IsoDateModelBinderProvider : IModelBinderProvider
{
    private static readonly IsoDateModelBinder Binder = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.UnderlyingOrModelType == typeof(DateOnly) ? Binder : null;
}
