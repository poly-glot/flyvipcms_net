using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.Pilots;

[CrudResource("pilots")]
public sealed class PilotsController(AppDbContext db) : CrudController<Pilot, PilotForm>(db)
{
    protected override string Title => "Pilot";

    protected override string RowsView => "_CrudRowsPilots.cshtml";

    protected override CrudLayout Layout() => new(
        [
            new(nameof(PilotForm.FirstName), Text),
            new(nameof(PilotForm.LastName), Text),
            new(nameof(PilotForm.City), Text),
            new(nameof(PilotForm.CellPhone), Text),
            new(nameof(PilotForm.EmailPrimary), Email),
            new(nameof(PilotForm.ContractType), Text),
        ],
        [
            new(nameof(PilotForm.BirthDate), Date),
            new(nameof(PilotForm.Address), Text),
            new(nameof(PilotForm.Country), Text),
            new(nameof(PilotForm.WorkPhone), Text),
            new(nameof(PilotForm.HomePhone), Text),
            new(nameof(PilotForm.EmailSecondary), Email),
            new(nameof(PilotForm.BasicSalary), Number),
            new(nameof(PilotForm.HourlySalary), Number),
            new(nameof(PilotForm.ExtraBonus), Number),
            new(nameof(PilotForm.TrainingIncluded), Text),
            new(nameof(PilotForm.Notes), Textarea),
        ]);
}
