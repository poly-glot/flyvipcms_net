using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.PilotFlights;

[CrudResource("flight-schedule")]
public sealed class PilotFlightsController(AppDbContext db) : CrudController<PilotFlight, PilotFlightForm>(db)
{
    protected override string Title => "Scheduled flight";

    protected override async Task<CrudOptions> Options() => new()
    {
        [nameof(PilotFlightForm.PilotId)] = await Db.PilotMenu(),
        [nameof(PilotFlightForm.AircraftId)] = await Db.AircraftMenu(),
        [nameof(PilotFlightForm.RouteId)] = await Db.RouteMenu(),
    };

    protected override CrudLayout Layout() => new(
        [
            new(nameof(PilotFlightForm.FlightDate), Date),
            new(nameof(PilotFlightForm.PilotId), Select),
            new(nameof(PilotFlightForm.AircraftId), Select),
            new(nameof(PilotFlightForm.RouteId), Select),
            new(nameof(PilotFlightForm.Identification), Text),
            new(nameof(PilotFlightForm.TotalHours), Number),
        ],
        [
            new(nameof(PilotFlightForm.CallSign), Text),
            new(nameof(PilotFlightForm.DayHours), Number),
            new(nameof(PilotFlightForm.NightHours), Number),
            new(nameof(PilotFlightForm.IfrHours), Number),
            new(nameof(PilotFlightForm.Landings), Number),
            new(nameof(PilotFlightForm.Comments), Textarea),
        ]);
}
