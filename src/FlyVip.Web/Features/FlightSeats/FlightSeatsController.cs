using FlyVip.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Features.FlightSeats;

[Authorize]
public sealed class FlightSeatsController(ReservationQueries queries) : Controller
{
    [HttpGet("/flights/taken-seats")]
    public async Task<IActionResult> Taken([FromQuery(Name = "aircraft_id")] int? aircraftId, [FromQuery(Name = "flight_date")] DateOnly? flightDate)
    {
        if (aircraftId is not > 0 || flightDate is not { } date)
        {
            return UnprocessableEntity(new { taken = Array.Empty<int>() });
        }

        return Json(new { taken = await queries.TakenSeats(aircraftId.Value, date) });
    }
}
