using FlyVip.Web.Data;
using FlyVip.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Admin.Features.Points;

public sealed class PointsQueries(AppDbContext db)
{
    public const int HistoryLimit = 300;

    public Task<List<LedgerLine>> History() =>
        db.PointsLedger
            .IgnoreQueryFilters()
            .OrderByDescending(entry => entry.Id)
            .Select(entry => new LedgerLine(
                entry.Amount,
                entry.BalanceAfter,
                entry.Description,
                entry.CreatedAt,
                entry.User.Profile!.FirstName,
                entry.User.Profile.LastName,
                entry.User.Member!.MemberCode))
            .Take(HistoryLimit)
            .ToListAsync();
}
