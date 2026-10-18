using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(MemberCode), IsUnique = true)]
public sealed class Member : ISoftDeletable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public byte? PlanId { get; set; }

    public MembershipPlan? Plan { get; set; }

    public int? ParentUserId { get; set; }

    public AppUser? ParentUser { get; set; }

    public byte? ContactTypeId { get; set; }

    public ContactType? ContactType { get; set; }

    [MaxLength(12)]
    public string MemberCode { get; set; } = "";

    [MaxLength(12)]
    public string MembershipName { get; set; } = "";

    public decimal JoiningFee { get; set; }

    public decimal YearlyFee { get; set; }

    public DateTime? JoiningFeePaidAt { get; set; }

    public MemberStatus Status { get; set; }

    public DateOnly? NextDueOn { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
