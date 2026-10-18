using FlyVip.Web.Infrastructure;

namespace FlyVip.Web.Areas.Admin.Features.Dashboard;

public sealed record StatTile(string Label, int Count, string Icon);

public sealed record OverdueMember(int UserId, string MemberCode, DateOnly NextDueOn, string FirstName, string LastName, int DaysOverdue)
{
    public string FullName => FirstName + " " + LastName;
}

public sealed record AdminDashboard(
    IReadOnlyList<StatTile> Tiles,
    IReadOnlyList<OverdueMember> Overdue,
    IReadOnlyList<FlightCardModel> Upcoming,
    DateOnly Today);
