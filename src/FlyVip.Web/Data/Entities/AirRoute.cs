using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class AirRoute : ISoftDeletable
{
    public int Id { get; set; }

    public int FromAirportId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Airport FromAirport { get; set; } = null!;

    public int ToAirportId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Airport ToAirport { get; set; } = null!;

    public int? AircraftId { get; set; }

    public Aircraft? Aircraft { get; set; }

    [Precision(10, 1)]
    public decimal? Distance { get; set; }

    [MaxLength(20)]
    public string? FlightTime { get; set; }

    [MaxLength(20)]
    public string? Ete { get; set; }

    [MaxLength(20)]
    public string? ActualEte { get; set; }

    public decimal Cost { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
