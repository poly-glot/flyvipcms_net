using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(TermId), nameof(Quarter), IsUnique = true)]
public sealed class MembershipQuarter
{
    public int Id { get; set; }

    public int TermId { get; set; }

    public MembershipTerm Term { get; set; } = null!;

    public int? PaymentId { get; set; }

    public Payment? Payment { get; set; }

    public byte Quarter { get; set; }

    public DateOnly StartsOn { get; set; }

    public DateOnly EndsOn { get; set; }

    public decimal PointsLoaded { get; set; }
}
