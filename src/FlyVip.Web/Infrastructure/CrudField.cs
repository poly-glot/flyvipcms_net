namespace FlyVip.Web.Infrastructure;

public enum CrudInputType
{
    Checkbox,
    Date,
    Email,
    Number,
    Select,
    Text,
    Textarea,
}

public sealed record CrudField(
    string Name,
    CrudInputType Type,
    IReadOnlyList<KeyValuePair<string, string>>? Options = null)
{
    public string Label { get; init; } = "";

    public string InputType => Type.ToString().ToLowerInvariant();
}
