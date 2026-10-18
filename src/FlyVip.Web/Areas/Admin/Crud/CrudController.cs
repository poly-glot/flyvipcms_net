using System.Reflection;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Admin.Crud;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CrudResourceAttribute(string slug) : RouteAttribute($"admin/{slug}")
{
    public string Slug { get; } = slug;
}

[Area("Admin")]
[Authorize(Policy = "admin")]
public abstract class CrudController<TEntity, TForm>(AppDbContext db) : Controller
    where TEntity : class, ISoftDeletable, new()
    where TForm : class, ICrudForm<TEntity>, new()
{
    private const string IndexView = "~/Areas/Admin/Crud/Index.cshtml";
    private const string FormView = "~/Areas/Admin/Crud/Form.cshtml";

    protected AppDbContext Db { get; } = db;

    protected abstract string Title { get; }

    protected virtual string RowsView => "_CrudRowsTable.cshtml";

    private string Slug => GetType().GetCustomAttribute<CrudResourceAttribute>()?.Slug ?? "";

    private string Home => $"/admin/{Slug}";

    protected abstract CrudLayout Layout();

    protected virtual Task<CrudOptions> Options() => Task.FromResult(new CrudOptions());

    protected virtual TForm Blank() => new();

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var rows = await Db.Set<TEntity>().AsNoTracking().OrderByDescending(entity => entity.Id).ToListAsync();
        var page = await Page();
        var properties = FormProperties();

        return View(IndexView, new CrudIndex(page, [.. rows.Select(entity => Row(entity, properties))], RowsView));
    }

    [HttpGet("new")]
    public async Task<IActionResult> New() => Form(Blank(), await Page(Home));

    [HttpPost("")]
    public async Task<IActionResult> Create(TForm form)
    {
        if (!ModelState.IsValid)
        {
            return Form(form, await Page(Home));
        }

        var entity = new TEntity();

        Db.Add(entity);
        form.Apply(entity);
        entity.CreatedAt = entity.UpdatedAt = DateTime.Now;
        await Db.SaveChangesAsync();

        return Back("created");
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        if (await Db.Set<TEntity>().FindAsync(id) is not { } entity)
        {
            return NotFound();
        }

        var form = new TForm();
        form.Load(entity);

        return Form(form, await Page($"{Home}/{id}", editing: true));
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> Update(int id, TForm form)
    {
        if (await Db.Set<TEntity>().FindAsync(id) is not { } entity)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Form(form, await Page($"{Home}/{id}", editing: true));
        }

        form.Apply(entity);
        entity.UpdatedAt = DateTime.Now;
        await Db.SaveChangesAsync();

        return Back("updated");
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await Db.Set<TEntity>().FindAsync(id) is not { } entity)
        {
            return NotFound();
        }

        entity.DeletedAt = entity.UpdatedAt = DateTime.Now;
        await Db.SaveChangesAsync();

        return Back("deleted");
    }

    private ViewResult Form(TForm form, CrudPage page)
    {
        ViewData[CrudPage.ViewDataKey] = page;

        return View(FormView, form);
    }

    private RedirectResult Back(string outcome)
    {
        TempData[FlashKey.Message] = $"{Title} {outcome}.";

        return Redirect(Home);
    }

    private async Task<CrudPage> Page(string action = "", bool editing = false)
    {
        var properties = FormProperties().ToDictionary(property => property.Name!);
        var options = await Options();
        var layout = Layout();

        CrudField Resolve(CrudField field) => field with
        {
            Label = properties[field.Name].GetDisplayName(),
            Name = properties[field.Name].BinderModelName!,
            Options = options.GetValueOrDefault(field.Name),
        };

        return new CrudPage(Slug, Title, [.. layout.Essentials.Select(Resolve)], [.. layout.Details.Select(Resolve)], action, editing);
    }

    private IReadOnlyList<ModelMetadata> FormProperties() => [.. MetadataProvider.GetMetadataForType(typeof(TForm)).Properties];

    private static CrudRow Row(TEntity entity, IReadOnlyList<ModelMetadata> properties)
    {
        var form = new TForm();
        form.Load(entity);

        var values = properties.ToDictionary(property => property.BinderModelName!, property => property.PropertyGetter!(form) is { } value ? FieldTagHelper.Text(value) : null);

        return new CrudRow(entity.Id, values);
    }
}
