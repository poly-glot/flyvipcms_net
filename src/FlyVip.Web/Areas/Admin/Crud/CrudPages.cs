using FlyVip.Web.Infrastructure;

namespace FlyVip.Web.Areas.Admin.Crud;

public sealed class CrudOptions : Dictionary<string, IReadOnlyList<KeyValuePair<string, string>>>;

public sealed record CrudLayout(IReadOnlyList<CrudField> Essentials, IReadOnlyList<CrudField> Details)
{
    public CrudLayout(IReadOnlyList<CrudField> essentials)
        : this(essentials, [])
    {
    }
}

public sealed record CrudPage(string Slug, string Title, IReadOnlyList<CrudField> Essentials, IReadOnlyList<CrudField> Details, string Action = "", bool Editing = false)
{
    public const string ViewDataKey = nameof(CrudPage);
}

public sealed record CrudRow(int Id, IReadOnlyDictionary<string, string?> Values);

public sealed record CrudIndex(CrudPage Page, IReadOnlyList<CrudRow> Rows, string RowsView)
{
    public IReadOnlyList<CrudField> Columns { get; } = [.. Page.Essentials.Where(field => field.Type != CrudInputType.Textarea)];
}
