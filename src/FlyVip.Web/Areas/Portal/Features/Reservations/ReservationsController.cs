using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Portal.Features.Reservations;

[Route("portal/reservations")]
public sealed class ReservationsController(
    PortalMemberQueries members,
    ReservationQueries queries,
    ReservationService reservations,
    PointsService points,
    BookingQueries booking,
    TimeProvider clock) : PortalController(members)
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (await CurrentMember() is not { } member)
        {
            return NotFound();
        }

        var own = await queries.ForAccount(member.AccountUserId);
        var requests = await queries.SeatRequestsOn([.. own.Select(listing => listing.Id)]);
        var today = clock.Today();

        var flights = FlightList.Of(own, today, actions: listing => CancelActions(listing, member.IsPrimary, today));

        return View(new PortalReservations(flights, requests, member.IsPrimary));
    }

    [HttpGet("new")]
    public Task<IActionResult> New() => Form(new PortalBookingViewModel());

    [HttpPost("")]
    public async Task<IActionResult> Create(PortalBookingViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return await Form(form);
        }

        try
        {
            await form.Book(reservations, User.Id());
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("reservation", error.Message);

            return await Form(form);
        }

        TempData[FlashKey.Message] = "Reservation created.";

        return Redirect("/portal/reservations");
    }

    [HttpPost("{id:int}/confirm")]
    public Task<IActionResult> Confirm(int id) =>
        Run("Seat request confirmed.", () => reservations.Confirm(id, User.Id(), false));

    [HttpPost("{id:int}/cancel")]
    public Task<IActionResult> Cancel(int id) =>
        Run("Reservation cancelled and points refunded.", () => reservations.Cancel(id, User.Id(), false));

    private async Task<IActionResult> Form(PortalBookingViewModel form)
    {
        form.Balance = await points.Balance(User.Id());
        form.Choices = await booking.Choices();

        return View(nameof(Form), form);
    }

    private async Task<IActionResult> Run(string success, Func<Task> action)
    {
        try
        {
            await action();
            TempData[FlashKey.Message] = success;
        }
        catch (DomainRuleException error)
        {
            TempData[FlashKey.Error] = error.Message;
        }

        return Redirect("/portal/reservations");
    }

    private static IReadOnlyList<FlightAction> CancelActions(ReservationListing listing, bool canBook, DateOnly today) =>
        canBook && listing.IsUpcoming(today)
            ? [new("Cancel reservation", $"/portal/reservations/{listing.Id}/cancel", "Cancel this reservation?", "Your points are refunded and the seats are released.")]
            : [];
}
