using FlyVip.Web.Infrastructure;

namespace FlyVip.Web.Areas.Portal.Features.Dashboard;

public sealed record FamilyMember(string FirstName, string LastName, string MemberCode, string ContactType)
{
    public string FullName => FirstName + " " + LastName;
}

public sealed record PortalDashboard(
    PortalMember Member,
    decimal Balance,
    IReadOnlyList<FamilyMember> Family,
    IReadOnlyList<decimal> Series,
    IReadOnlyList<FlightCardModel> Upcoming,
    IReadOnlyList<ActivityLine> Activity);
