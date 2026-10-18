using System.ComponentModel.DataAnnotations;

namespace FlyVip.Web.Data.Entities;

public sealed class Airport : ISoftDeletable
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(20)]
    public string? CallSign { get; set; }

    [MaxLength(40)]
    public string? AirportType { get; set; }

    [MaxLength(120)]
    public string? CustomsContact { get; set; }

    [MaxLength(120)]
    public string? FboContact { get; set; }

    [MaxLength(120)]
    public string? HotelContact { get; set; }

    public decimal TravelAllowance { get; set; }

    public decimal TakeOffCost { get; set; }

    public decimal LandingCost { get; set; }

    public decimal StayCost { get; set; }

    public decimal AirportCost { get; set; }

    public decimal HandlingCost { get; set; }

    public decimal TotalCost { get; set; }

    public string? Comments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
