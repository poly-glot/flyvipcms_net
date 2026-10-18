using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlyVip.Web.Data;

public sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
    date => date.ToDateTime(TimeOnly.MinValue),
    stamp => DateOnly.FromDateTime(stamp));
