using System.ComponentModel.DataAnnotations;

namespace FlyVip.Web.Data.Entities;

public sealed class CountryZone
{
    public int Id { get; set; }

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    [MaxLength(80)]
    public string Name { get; set; } = "";

    public bool Active { get; set; } = true;
}
