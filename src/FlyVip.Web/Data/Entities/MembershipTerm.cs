namespace FlyVip.Web.Data.Entities;

public sealed class MembershipTerm
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public Member Member { get; set; } = null!;

    public DateOnly StartsOn { get; set; }

    public DateOnly EndsOn { get; set; }

    public DateTime? CreatedAt { get; set; }

    public List<MembershipQuarter> Quarters { get; } = [];
}
