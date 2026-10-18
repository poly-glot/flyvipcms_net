using System.Text.Json;

namespace FlyVip.Web.Data;

public static class EnumText
{
    public static string Of<TEnum>(TEnum value)
        where TEnum : struct, Enum => Of((Enum)value);

    public static string Of(Enum value) => JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());
}
