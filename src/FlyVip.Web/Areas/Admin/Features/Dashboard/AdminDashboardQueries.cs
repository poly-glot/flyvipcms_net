using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Admin.Features.Dashboard;

public sealed class AdminDashboardQueries(AppDbContext db, ReservationQueries reservations, TimeProvider clock)
{
    private const int UpcomingLimit = 5;

    public async Task<AdminDashboard> Load()
    {
        var today = clock.Today();

        var tiles = new List<StatTile>
        {
            new("Active members", await db.Members.CountAsync(member => member.PlanId != null && member.Status == MemberStatus.Active), "users"),
            new("Sub-members", await db.Members.CountAsync(member => member.ParentUserId != null), "user"),
            new("Upcoming flights", await db.Reservations.Where(reservation => reservation.ParentReservationId == null).CountAsync(ReservationQueries.IsUpcoming(today)), "plane"),
            new("Pending seat requests", await db.Reservations.CountAsync(reservation => reservation.Status == ReservationStatus.Pending), "clock"),
            new("Aircraft", await db.Aircrafts.CountAsync(), "layers"),
            new("Pilots", await db.Pilots.CountAsync(), "award"),
        };

        var overdue = await db.Members
            .Where(member => member.PlanId != null && member.NextDueOn < today && member.User.Profile != null)
            .OrderBy(member => member.NextDueOn)
            .Select(member => new { member.UserId, member.MemberCode, DueOn = member.NextDueOn ?? today, member.User.Profile!.FirstName, member.User.Profile.LastName })
            .ToListAsync();

        var upcoming = await reservations.UpcomingForClub(today, UpcomingLimit);

        return new AdminDashboard(
            tiles,
            [.. overdue.Select(member => new OverdueMember(member.UserId, member.MemberCode, member.DueOn, member.FirstName, member.LastName, today.DayNumber - member.DueOn.DayNumber))],
            [.. upcoming.Select(listing => FlightCardModel.Of(listing, $"/admin/reservations/{listing.Id}"))],
            today);
    }
}
