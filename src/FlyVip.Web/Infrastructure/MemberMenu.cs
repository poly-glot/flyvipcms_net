using System.Globalization;
using FlyVip.Web.Data;
using FlyVip.Web.Services;

namespace FlyVip.Web.Infrastructure;

public static class MemberMenu
{
    public static IReadOnlyList<KeyValuePair<string, string>> Menu(this IEnumerable<MemberOption> members) =>
        [.. members.Select(member => Menus.Option(member.UserId, Label(member)))];

    public static IReadOnlyList<KeyValuePair<string, string>> MenuWithBalances(this IEnumerable<MemberOption> members) =>
        [.. members.Select(member => Menus.Option(member.UserId, $"{Label(member)} - {member.Balance.ToString("0.00", CultureInfo.InvariantCulture)} pts"))];

    private static string Label(MemberOption member) => $"{member.FirstName} {member.LastName} [{member.MemberCode}]";
}
