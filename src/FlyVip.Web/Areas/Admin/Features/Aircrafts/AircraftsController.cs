using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.Aircrafts;

[CrudResource("aircrafts")]
public sealed class AircraftsController(AppDbContext db) : CrudController<Aircraft, AircraftForm>(db)
{
    protected override string Title => "Aircraft";

    protected override async Task<CrudOptions> Options() => new()
    {
        [nameof(AircraftForm.AircraftTypeId)] = await Db.AircraftTypeMenu(),
    };

    protected override CrudLayout Layout() => new(
        [
            new(nameof(AircraftForm.Name), Text),
            new(nameof(AircraftForm.AircraftTypeId), Select),
            new(nameof(AircraftForm.PassengerCapacity), Number),
            new(nameof(AircraftForm.TotalCost), Number),
        ],
        [
            new(nameof(AircraftForm.Model), Text),
            new(nameof(AircraftForm.ManufacturedOn), Date),
            new(nameof(AircraftForm.AirframeHours), Number),
            new(nameof(AircraftForm.EngineHoursLeft), Number),
            new(nameof(AircraftForm.EngineHoursRight), Number),
            new(nameof(AircraftForm.VariableCost), Number),
            new(nameof(AircraftForm.FixedCost), Number),
            new(nameof(AircraftForm.MaxAltitude), Number),
            new(nameof(AircraftForm.GallonsPerHour), Number),
            new(nameof(AircraftForm.Certification), Text),
            new(nameof(AircraftForm.InsuranceCost), Number),
        ]);
}
