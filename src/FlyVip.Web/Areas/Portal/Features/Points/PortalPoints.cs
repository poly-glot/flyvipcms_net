using FlyVip.Web.Infrastructure;

namespace FlyVip.Web.Areas.Portal.Features.Points;

public sealed record PortalPoints(decimal Balance, IReadOnlyList<LedgerLine> Entries, IReadOnlyList<decimal> Series);
