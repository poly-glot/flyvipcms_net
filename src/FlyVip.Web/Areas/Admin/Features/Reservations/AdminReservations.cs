using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Models;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Areas.Admin.Features.Reservations;

public sealed class AdminBookingViewModel : BookingViewModel
{
    [FromForm(Name = "user_id")]
    [Display(Name = "user_id")]
    [Required]
    [PositiveWholeNumber]
    public string? UserId { get; init; }

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> Members { get; set; } = [];
}

public sealed record ReservationDetail(
    ReservationListing Listing,
    IReadOnlyList<byte> Seats,
    IReadOnlyList<ReservationListing> Joiners,
    IReadOnlyList<ActivityLine> Activity)
{
    public bool CanConfirm => Listing.IsPending;

    public bool CanCancel => !Listing.IsCancelled;

    public FlightCardModel Card => FlightCardModel.Of(Listing, $"/admin/reservations/{Listing.Id}");

    public IReadOnlyList<int> HeldSeats => Listing.IsComplete ? [.. Enumerable.Range(1, Listing.Capacity)] : [.. Seats.Select(seat => (int)seat)];
}
