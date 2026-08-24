using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Rivo.API.Binders;

/// <summary>
/// Query-string DateTime values (e.g. "?from=2026-08-15") bind with Kind=Unspecified by default, which
/// Npgsql then rejects when comparing against a "timestamp with time zone" column ("Cannot write
/// DateTime with Kind=Unspecified..." — hit by every Analytics/Finance/Reports/Dashboard endpoint that
/// takes a date range). The app stores everything in UTC, so treat an unspecified value as already UTC
/// rather than silently misapplying the server's local offset.
/// </summary>
public class UtcDateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format.");
            return Task.CompletedTask;
        }

        var utc = parsed.Kind switch
        {
            DateTimeKind.Utc => parsed,
            DateTimeKind.Local => parsed.ToUniversalTime(),
            _ => DateTime.SpecifyKind(parsed, DateTimeKind.Utc),
        };

        bindingContext.Result = ModelBindingResult.Success(utc);
        return Task.CompletedTask;
    }
}

public class UtcDateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
        {
            return new UtcDateTimeModelBinder();
        }
        return null;
    }
}
