using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Portal;

public sealed record PortalMember(
    int UserId,
    int? ParentUserId,
    bool IsPrimary,
    string MemberCode,
    MemberStatus Status,
    DateOnly? NextDueOn,
    string FirstName,
    string LastName,
    string? PlanName)
{
    public int AccountUserId => ParentUserId ?? UserId;
}

public sealed class PortalMemberQueries(AppDbContext db)
{
    public Task<PortalMember?> Current(int userId) =>
        db.Members
            .Where(member => member.UserId == userId && member.User.Profile != null)
            .Select(member => new PortalMember(
                member.UserId,
                member.ParentUserId,
                member.PlanId != null,
                member.MemberCode,
                member.Status,
                member.NextDueOn,
                member.User.Profile!.FirstName,
                member.User.Profile.LastName,
                member.Plan!.Name))
            .FirstOrDefaultAsync();
}
