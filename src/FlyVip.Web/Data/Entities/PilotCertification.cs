using System.ComponentModel.DataAnnotations;

namespace FlyVip.Web.Data.Entities;

public sealed class PilotCertification : ISoftDeletable
{
    public int Id { get; set; }

    public int PilotId { get; set; }

    public Pilot Pilot { get; set; } = null!;

    public int? AircraftId { get; set; }

    public Aircraft? Aircraft { get; set; }

    [MaxLength(120)]
    public string Certificate { get; set; } = "";

    [MaxLength(60)]
    public string? CertType { get; set; }

    [MaxLength(120)]
    public string? AgencySchool { get; set; }

    public DateOnly? IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public DateOnly? DueOn { get; set; }

    public bool Active { get; set; } = true;

    public string? Comments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
