using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.AircraftTypes;

[CrudResource("aircraft-types")]
public sealed class AircraftTypesController(AppDbContext db) : CrudController<AircraftType, AircraftTypeForm>(db)
{
    protected override string Title => "Aircraft type";

    protected override CrudLayout Layout() => new(
        [
            new(nameof(AircraftTypeForm.Name), Text),
        ]);
}
