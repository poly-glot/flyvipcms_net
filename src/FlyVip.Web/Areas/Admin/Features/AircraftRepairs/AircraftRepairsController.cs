using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.AircraftRepairs;

[CrudResource("maintenance")]
public sealed class AircraftRepairsController(AppDbContext db) : CrudController<AircraftRepair, AircraftRepairForm>(db)
{
    protected override string Title => "Maintenance record";

    protected override async Task<CrudOptions> Options() => new()
    {
        [nameof(AircraftRepairForm.AircraftId)] = await Db.AircraftMenu(),
    };

    protected override CrudLayout Layout() => new(
        [
            new(nameof(AircraftRepairForm.AircraftId), Select),
            new(nameof(AircraftRepairForm.RepairedOn), Date),
            new(nameof(AircraftRepairForm.Code), Text),
            new(nameof(AircraftRepairForm.Description), Textarea),
            new(nameof(AircraftRepairForm.RepairShop), Text),
            new(nameof(AircraftRepairForm.PerformedHours), Number),
            new(nameof(AircraftRepairForm.ExpiresOn), Date),
            new(nameof(AircraftRepairForm.ExpiryHours), Number),
            new(nameof(AircraftRepairForm.Cost), Number),
        ]);
}
