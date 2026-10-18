using System.ComponentModel.DataAnnotations;

namespace FlyVip.Web.Data.Entities;

public sealed class AircraftType : ISoftDeletable
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
