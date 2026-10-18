using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(Iso2), IsUnique = true)]
public sealed class Country
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = "";

    [MaxLength(2)]
    public string Iso2 { get; set; } = "";

    public bool Active { get; set; } = true;
}
