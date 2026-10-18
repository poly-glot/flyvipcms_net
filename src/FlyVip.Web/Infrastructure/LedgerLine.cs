using FlyVip.Web.Data.Entities;

namespace FlyVip.Web.Infrastructure;

public sealed record LedgerLine(
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTime? CreatedAt,
    string? FirstName,
    string? LastName,
    string? MemberCode)
{
    public bool IsCredit => Amount >= 0;

    public string FullName => FirstName + " " + LastName;

    public static LedgerLine Of(PointsLedgerEntry entry) =>
        new(entry.Amount, entry.BalanceAfter, entry.Description, entry.CreatedAt, null, null, null);
}
