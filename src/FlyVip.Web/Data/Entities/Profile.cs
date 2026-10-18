using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class Profile
{
    [Key]
    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    [MaxLength(100)]
    public string FirstName { get; set; } = "";

    [MaxLength(100)]
    public string LastName { get; set; } = "";

    [MaxLength(150)]
    public string? Company { get; set; }

    [MaxLength(255)]
    public string? StreetAddress { get; set; }

    public int? CountryId { get; set; }

    public Country? Country { get; set; }

    public int? ZoneId { get; set; }

    public CountryZone? Zone { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? ZipCode { get; set; }

    [MaxLength(30)]
    public string? Cellphone { get; set; }

    [MaxLength(30)]
    public string? OfficePhone { get; set; }

    [MaxLength(30)]
    public string? HomePhone { get; set; }

    [MaxLength(30)]
    public string? SecondaryPhone { get; set; }

    [MaxLength(50)]
    public string? IdNumber { get; set; }

    public DateOnly? IdDueOn { get; set; }

    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    public DateOnly? PassportIssuedOn { get; set; }

    public DateOnly? PassportDueOn { get; set; }

    public int? FavouriteRouteId { get; set; }

    [DeleteBehavior(DeleteBehavior.SetNull)]
    public AirRoute? FavouriteRoute { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
