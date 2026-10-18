using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.PilotDocuments;

[CrudResource("pilot-documents")]
public sealed class PilotDocumentsController(AppDbContext db) : CrudController<PilotDocument, PilotDocumentForm>(db)
{
    protected override string Title => "Pilot document";

    protected override async Task<CrudOptions> Options() => new()
    {
        [nameof(PilotDocumentForm.PilotId)] = await Db.PilotMenu(),
    };

    protected override CrudLayout Layout() => new(
        [
            new(nameof(PilotDocumentForm.PilotId), Select),
            new(nameof(PilotDocumentForm.DocType), Text),
            new(nameof(PilotDocumentForm.DocNumber), Text),
            new(nameof(PilotDocumentForm.Country), Text),
            new(nameof(PilotDocumentForm.IssuedOn), Date),
            new(nameof(PilotDocumentForm.ExpiresOn), Date),
        ]);
}
