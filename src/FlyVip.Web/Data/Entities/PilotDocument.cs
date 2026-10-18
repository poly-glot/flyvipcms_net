using System.ComponentModel.DataAnnotations;

namespace FlyVip.Web.Data.Entities;

public sealed class PilotDocument : ISoftDeletable
{
    public int Id { get; set; }

    public int PilotId { get; set; }

    public Pilot Pilot { get; set; } = null!;

    [MaxLength(80)]
    public string? Country { get; set; }

    [MaxLength(40)]
    public string DocType { get; set; } = "";

    [MaxLength(60)]
    public string DocNumber { get; set; } = "";

    public DateOnly? IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
