using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.Airports;

[CrudResource("airports")]
public sealed class AirportsController(AppDbContext db) : CrudController<Airport, AirportForm>(db)
{
    protected override string Title => "Airport";

    protected override CrudLayout Layout() => new(
        [
            new(nameof(AirportForm.Name), Text),
            new(nameof(AirportForm.CallSign), Text),
            new(nameof(AirportForm.AirportType), Text),
        ],
        [
            new(nameof(AirportForm.CustomsContact), Text),
            new(nameof(AirportForm.FboContact), Text),
            new(nameof(AirportForm.HotelContact), Text),
            new(nameof(AirportForm.TravelAllowance), Number),
            new(nameof(AirportForm.TakeOffCost), Number),
            new(nameof(AirportForm.LandingCost), Number),
            new(nameof(AirportForm.StayCost), Number),
            new(nameof(AirportForm.AirportCost), Number),
            new(nameof(AirportForm.HandlingCost), Number),
            new(nameof(AirportForm.Comments), Textarea),
        ]);
}
