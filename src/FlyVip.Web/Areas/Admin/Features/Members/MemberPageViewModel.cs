using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;

namespace FlyVip.Web.Areas.Admin.Features.Members;

public sealed record MemberRow(
    int UserId,
    string FirstName,
    string LastName,
    string MemberCode,
    string Email,
    string PlanName,
    MemberStatus Status,
    DateOnly? NextDueOn,
    decimal? Balance,
    bool IsOverdue)
{
    public string FullName => FirstName + " " + LastName;
}

public sealed record MembersIndex(IReadOnlyList<MemberRow> Rows)
{
    public IReadOnlyDictionary<MemberStatus, int> StatusCounts { get; } = Rows.CountBy(row => row.Status).ToDictionary();
}

public sealed record MemberDetail(
    int UserId,
    string MemberCode,
    bool IsPrimary,
    DateOnly? NextDueOn,
    MemberStatus Status,
    string Username,
    string Email,
    string? PlanName,
    string FirstName,
    string LastName,
    string? Cellphone,
    string? Company,
    string? PassportNumber,
    DateOnly? PassportDueOn)
{
    public string FullName => FirstName + " " + LastName;
}

public sealed record SubMemberRow(int UserId, string FirstName, string LastName, string MemberCode, string Username, string ContactType)
{
    public string FullName => FirstName + " " + LastName;
}

public sealed class MemberPageViewModel : SubMemberViewModel
{
    public static readonly IReadOnlyList<KeyValuePair<string, string>> StatusChoices = [new("inactive", "Inactive"), new("active", "Active"), new("banned", "Banned")];

    public required MemberDetail Detail { get; init; }

    public required IReadOnlyList<SubMemberRow> Children { get; init; }

    public required IReadOnlyList<PaymentLine> Payments { get; init; }

    public required IReadOnlyList<LedgerLine> Ledger { get; init; }

    public required IReadOnlyList<decimal> Series { get; init; }

    public required FlightList Flights { get; init; }

    public required decimal Balance { get; init; }

    public required IReadOnlyList<KeyValuePair<string, string>> ContactTypes { get; init; }

    public string CurrentStatus => EnumText.Of(Detail.Status);
}
