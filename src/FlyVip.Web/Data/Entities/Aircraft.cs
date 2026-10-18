using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class Aircraft : ISoftDeletable
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    public int AircraftTypeId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AircraftType AircraftType { get; set; } = null!;

    [MaxLength(100)]
    public string? Model { get; set; }

    public DateOnly? ManufacturedOn { get; set; }

    public byte PassengerCapacity { get; set; } = 1;

    [Precision(10, 1)]
    public decimal AirframeHours { get; set; }

    [Precision(10, 1)]
    public decimal EngineHoursLeft { get; set; }

    [Precision(10, 1)]
    public decimal EngineHoursRight { get; set; }

    public decimal VariableCost { get; set; }

    public decimal FixedCost { get; set; }

    public decimal TotalCost { get; set; }

    public int? MaxAltitude { get; set; }

    [Precision(8, 2)]
    public decimal? GallonsPerHour { get; set; }

    [MaxLength(100)]
    public string? Certification { get; set; }

    public decimal InsuranceCost { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
