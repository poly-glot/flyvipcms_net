using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FlyVip.Web.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MaxCharactersAttribute(int maximum) : ValidationAttribute("The {0} field cannot exceed {1} characters in length.")
{
    public override bool IsValid(object? value) => value is not string text || text.EnumerateRunes().Count() <= maximum;

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, maximum);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MinCharactersAttribute(int minimum) : ValidationAttribute("The {0} field must be at least {1} characters in length.")
{
    public override bool IsValid(object? value) => value is not string text || text.EnumerateRunes().Count() >= minimum;

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, minimum);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MaxBytesAttribute(int maximum) : ValidationAttribute("Password cannot exceed {1} bytes in length.")
{
    public override bool IsValid(object? value) => value is not string text || Encoding.UTF8.GetByteCount(text) <= maximum;

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, maximum);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EmailFormatAttribute() : RegularExpressionAttribute(@"\A[^@\s]+@[^@\s]+\.[^@\s]+\z")
{
    public override string FormatErrorMessage(string name) => $"The {name} field must contain a valid email address.";
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class DiffersAttribute(string otherProperty) : ValidationAttribute("The {0} field must differ from the {1} field.")
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var other = validationContext.ObjectType.GetProperty(otherProperty) ?? throw new InvalidOperationException($"Unknown property '{otherProperty}'.");

        if (value is null || !Equals(value, other.GetValue(validationContext.ObjectInstance)))
        {
            return ValidationResult.Success;
        }

        var otherLabel = other.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? otherProperty;
        var message = string.Format(CultureInfo.InvariantCulture, ErrorMessageString, validationContext.DisplayName, otherLabel);

        return new ValidationResult(message, validationContext.MemberName is { } member ? [member] : null);
    }
}
