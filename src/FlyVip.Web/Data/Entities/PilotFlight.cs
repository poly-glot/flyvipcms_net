using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(AircraftId), nameof(FlightDate))]
public sealed class PilotFlight : ISoftDeletable
{
    public int Id { get; set; }

    public int PilotId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Pilot Pilot { get; set; } = null!;

    public int AircraftId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Aircraft Aircraft { get; set; } = null!;

    public int? RouteId { get; set; }

    public AirRoute? Route { get; set; }

    public DateOnly FlightDate { get; set; }

    [MaxLength(60)]
    public string? Identification { get; set; }

    [MaxLength(30)]
    public string? CallSign { get; set; }

    [Precision(6, 1)]
    public decimal DayHours { get; set; }

    [Precision(6, 1)]
    public decimal NightHours { get; set; }

    [Precision(6, 1)]
    public decimal IfrHours { get; set; }

    public int Landings { get; set; }

    [Precision(6, 1)]
    public decimal TotalHours { get; set; }

    public string? Comments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
