using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.AirRoutes;

[CrudResource("routes")]
public sealed class AirRoutesController(AppDbContext db) : CrudController<AirRoute, AirRouteForm>(db)
{
    protected override string Title => "Route";

    protected override async Task<CrudOptions> Options()
    {
        var airports = await Db.AirportMenu();

        return new()
        {
            [nameof(AirRouteForm.FromAirportId)] = airports,
            [nameof(AirRouteForm.ToAirportId)] = airports,
            [nameof(AirRouteForm.AircraftId)] = await Db.AircraftMenu(),
        };
    }

    protected override CrudLayout Layout() => new(
        [
            new(nameof(AirRouteForm.FromAirportId), Select),
            new(nameof(AirRouteForm.ToAirportId), Select),
            new(nameof(AirRouteForm.AircraftId), Select),
            new(nameof(AirRouteForm.Distance), Number),
            new(nameof(AirRouteForm.FlightTime), Text),
            new(nameof(AirRouteForm.Cost), Number),
        ],
        [
            new(nameof(AirRouteForm.Ete), Text),
            new(nameof(AirRouteForm.ActualEte), Text),
        ]);
}
