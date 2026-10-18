using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(AircraftId), nameof(FlightDate))]
public sealed class Reservation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser User { get; set; } = null!;

    public int? ParentReservationId { get; set; }

    public Reservation? ParentReservation { get; set; }

    public int AircraftId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Aircraft Aircraft { get; set; } = null!;

    public int RouteId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AirRoute Route { get; set; } = null!;

    public int? PilotFlightId { get; set; }

    [DeleteBehavior(DeleteBehavior.SetNull)]
    public PilotFlight? PilotFlight { get; set; }

    public DateOnly FlightDate { get; set; }

    public ReservationKind Kind { get; set; }

    public byte Passengers { get; set; }

    public decimal Amount { get; set; }

    public ReservationStatus Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ReservationSeat> Seats { get; } = [];
}
