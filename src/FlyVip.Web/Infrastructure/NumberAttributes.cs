using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FlyVip.Web.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class DecimalNumberAttribute() : RegularExpressionAttribute(@"\A[-+]?\d{0,}\.?\d+\z")
{
    public override string FormatErrorMessage(string name) => $"The {name} field must contain a decimal number.";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class WholeNumberAttribute() : RegularExpressionAttribute(@"\A[0-9]+\z")
{
    public override string FormatErrorMessage(string name) => $"The {name} field must only contain digits.";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class PositiveWholeNumberAttribute() : RegularExpressionAttribute(@"\A(?!0\z)[0-9]+\z")
{
    public override string FormatErrorMessage(string name) => $"The {name} field must only contain digits and must be greater than zero.";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AtLeastAttribute(int minimum) : ValidationAttribute("The {0} field must contain a number greater than or equal to {1}.")
{
    public override bool IsValid(object? value) =>
        value is not string { Length: > 0 } text || (Number(text) is { } number && number >= minimum);

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, minimum);

    internal static decimal? Number(string text) =>
        decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : null;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AtMostAttribute(int maximum) : ValidationAttribute("The {0} field must contain a number less than or equal to {1}.")
{
    public override bool IsValid(object? value) =>
        value is not string { Length: > 0 } text || (AtLeastAttribute.Number(text) is { } number && number <= maximum);

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, maximum);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GreaterThanAttribute(int minimum) : ValidationAttribute("The {0} field must contain a number greater than {1}.")
{
    public override bool IsValid(object? value) =>
        value is not string { Length: > 0 } text || (AtLeastAttribute.Number(text) is { } number && number > minimum);

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, minimum);
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class PositiveWholeNumbersAttribute() : ValidationAttribute("The {0} field must only contain digits and must be greater than zero.")
{
    public override bool IsValid(object? value) => value is not IEnumerable<string?> texts || texts.All(text => Natural(text) > 0);

    public static int Natural(string? value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0;
}
