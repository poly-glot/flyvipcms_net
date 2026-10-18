using System.Globalization;

namespace FlyVip.Web.Infrastructure;

public static class FormValues
{
    public static string? Text(IFormattable? value) => value?.ToString(null, CultureInfo.InvariantCulture);

    public static decimal ParseDecimal(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);

    public static int RequiredId(this string? text) => int.Parse(text!, CultureInfo.InvariantCulture);

    public static int? OptionalId(this string? text) => text is null ? null : int.Parse(text, CultureInfo.InvariantCulture);

    public static decimal DecimalOr(this string? text, decimal current) => text is null ? current : ParseDecimal(text);

    public static decimal? DecimalOr(this string? text, decimal? current) => text is null ? current : ParseDecimal(text);

    public static int IntOr(this string? text, int current) => text is null ? current : int.Parse(text, CultureInfo.InvariantCulture);

    public static int? IntOr(this string? text, int? current) => text is null ? current : int.Parse(text, CultureInfo.InvariantCulture);

    public static byte ByteOr(this string? text, byte current) => text is null ? current : byte.Parse(text, CultureInfo.InvariantCulture);
}
