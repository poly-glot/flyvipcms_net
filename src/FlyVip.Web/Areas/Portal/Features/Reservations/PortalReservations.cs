using FlyVip.Web.Infrastructure;
using FlyVip.Web.Models;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Areas.Portal.Features.Reservations;

public sealed record PortalReservations(FlightList Flights, IReadOnlyList<ReservationListing> Requests, bool CanBook);

public sealed class PortalBookingViewModel : BookingViewModel
{
    [BindNever]
    public decimal Balance { get; set; }
}
