using FlyVip.Web.Data;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Portal.Features.Dashboard;

public sealed class PortalDashboardQueries(AppDbContext db, ReservationQueries reservations, PointsService points, TimeProvider clock)
{
    private const int UpcomingLimit = 3;

    private const int ActivityLimit = 10;

    public async Task<PortalDashboard> Load(PortalMember member)
    {
        var accountId = member.AccountUserId;

        var family = await db.Members
            .Where(child => child.ParentUserId == accountId && child.User.Profile != null && child.ContactType != null)
            .Select(child => new FamilyMember(child.User.Profile!.FirstName, child.User.Profile.LastName, child.MemberCode, child.ContactType!.Name))
            .ToListAsync();

        var recent = await db.PointsLedger
            .Where(entry => entry.UserId == accountId)
            .OrderByDescending(entry => entry.Id)
            .Take(BalanceTrend.Points)
            .Select(entry => entry.BalanceAfter)
            .ToListAsync();

        var upcoming = await reservations.Upcoming(accountId, clock.Today(), UpcomingLimit);

        return new PortalDashboard(
            member,
            await points.Balance(accountId),
            family,
            BalanceTrend.Of(recent),
            [.. upcoming.Select(listing => FlightCardModel.Of(listing))],
            await reservations.RecentActivity(accountId, ActivityLimit));
    }
}
