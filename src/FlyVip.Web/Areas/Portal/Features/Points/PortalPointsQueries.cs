using FlyVip.Web.Data;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Portal.Features.Points;

public sealed class PortalPointsQueries(AppDbContext db, PointsService points)
{
    private const int HistoryLimit = 100;

    public async Task<PortalPoints> Load(int accountId)
    {
        var entries = await db.PointsLedger
            .Where(entry => entry.UserId == accountId)
            .OrderByDescending(entry => entry.Id)
            .Take(HistoryLimit)
            .Select(entry => new LedgerLine(entry.Amount, entry.BalanceAfter, entry.Description, entry.CreatedAt, null, null, null))
            .ToListAsync();

        return new PortalPoints(await points.Balance(accountId), entries, BalanceTrend.Of(entries.Select(line => line.BalanceAfter)));
    }
}
