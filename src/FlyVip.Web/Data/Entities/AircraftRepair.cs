using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class AircraftRepair : ISoftDeletable
{
    public int Id { get; set; }

    public int AircraftId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Aircraft Aircraft { get; set; } = null!;

    public DateOnly RepairedOn { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = "";

    public string Description { get; set; } = "";

    [MaxLength(100)]
    public string RepairShop { get; set; } = "";

    [Precision(8, 1)]
    public decimal PerformedHours { get; set; }

    public DateOnly ExpiresOn { get; set; }

    [Precision(8, 1)]
    public decimal ExpiryHours { get; set; }

    public decimal Cost { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
