using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(AircraftId), nameof(FlightDate), nameof(SeatNumber), IsUnique = true)]
public sealed class ReservationSeat
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public int AircraftId { get; set; }

    public DateOnly FlightDate { get; set; }

    public byte SeatNumber { get; set; }
}
