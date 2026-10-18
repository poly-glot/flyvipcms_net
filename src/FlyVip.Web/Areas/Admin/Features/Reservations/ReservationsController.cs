using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Reservations;

[Area("Admin")]
[Authorize(Policy = "admin")]
[Route("admin/reservations")]
public sealed class ReservationsController(ReservationQueries queries, ReservationService reservations, MemberService members, BookingQueries booking, TimeProvider clock) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(FlightList.Of(await queries.All(), clock.Today(), listing => $"/admin/reservations/{listing.Id}"));

    [HttpGet("new")]
    public Task<IActionResult> New() => Form(new AdminBookingViewModel());

    [HttpPost("")]
    public async Task<IActionResult> Create(AdminBookingViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return await Form(form);
        }

        try
        {
            var id = await form.Book(reservations, PositiveWholeNumbersAttribute.Natural(form.UserId));
            TempData[FlashKey.Message] = "Reservation created.";

            return Redirect($"/admin/reservations/{id}");
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("reservation", error.Message);

            return await Form(form);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        if (await queries.Find(id) is not { } listing)
        {
            return NotFound();
        }

        return View(new ReservationDetail(listing, await queries.Seats(id), await queries.Joiners(id), await queries.ActivityOf(id)));
    }

    [HttpPost("{id:int}/confirm")]
    public Task<IActionResult> Confirm(int id) =>
        Run(id, "Seat request confirmed.", () => reservations.Confirm(id, User.Id(), true));

    [HttpPost("{id:int}/cancel")]
    public Task<IActionResult> Cancel(int id) =>
        Run(id, "Reservation cancelled and points refunded.", () => reservations.Cancel(id, User.Id(), true));

    private async Task<IActionResult> Form(AdminBookingViewModel form)
    {
        form.Members = (await members.PrimaryMembers(activeOnly: true, withBalances: true)).MenuWithBalances();
        form.Choices = await booking.Choices();

        return View(nameof(Form), form);
    }

    private async Task<IActionResult> Run(int id, string success, Func<Task> action)
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

        return Redirect($"/admin/reservations/{id}");
    }
}
