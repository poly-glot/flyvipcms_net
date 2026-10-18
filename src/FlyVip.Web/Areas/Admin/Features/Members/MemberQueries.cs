using FlyVip.Web.Data;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Admin.Features.Members;

public sealed class MemberQueries(AppDbContext db, ReservationQueries reservations, PointsService points, MemberService members, TimeProvider clock)
{
    private const int LedgerPreview = 25;

    public async Task<MembersIndex> Index()
    {
        var today = clock.Today();

        var rows = await db.Members
            .Where(member => member.PlanId != null)
            .OrderByDescending(member => member.Id)
            .Select(member => new MemberRow(
                member.UserId,
                member.User.Profile!.FirstName,
                member.User.Profile.LastName,
                member.MemberCode,
                member.User.Email,
                member.Plan!.Name,
                member.Status,
                member.NextDueOn,
                db.PointsLedger.Where(entry => entry.UserId == member.UserId).OrderByDescending(entry => entry.Id).Select(entry => (decimal?)entry.BalanceAfter).FirstOrDefault(),
                member.NextDueOn < today))
            .ToListAsync();

        return new MembersIndex(rows);
    }

    public Task<MemberDetail?> Detail(int userId) =>
        db.Members
            .Where(member => member.UserId == userId)
            .Select(member => new MemberDetail(
                member.UserId,
                member.MemberCode,
                member.PlanId != null,
                member.NextDueOn,
                member.Status,
                member.User.Username,
                member.User.Email,
                member.Plan!.Name,
                member.User.Profile!.FirstName,
                member.User.Profile.LastName,
                member.User.Profile.Cellphone,
                member.User.Profile.Company,
                member.User.Profile.PassportNumber,
                member.User.Profile.PassportDueOn))
            .FirstOrDefaultAsync();

    public async Task<MemberPageViewModel> Page(MemberDetail detail)
    {
        var userId = detail.UserId;

        var children = await db.Members
            .Where(member => member.ParentUserId == userId)
            .Select(member => new SubMemberRow(member.UserId, member.User.Profile!.FirstName, member.User.Profile.LastName, member.MemberCode, member.User.Username, member.ContactType!.Name))
            .ToListAsync();

        var payments = await db.Payments
            .Where(payment => payment.UserId == userId)
            .OrderByDescending(payment => payment.Id)
            .Select(payment => new PaymentLine(payment.Id, payment.UserId, payment.PaidOn, payment.Type, payment.Remark, payment.Description, payment.Amount, null, null, null))
            .ToListAsync();

        var ledger = await db.PointsLedger
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.Id)
            .Take(LedgerPreview)
            .Select(entry => new LedgerLine(entry.Amount, entry.BalanceAfter, entry.Description, entry.CreatedAt, null, null, null))
            .ToListAsync();

        var flights = await reservations.ForAccount(userId);

        return new MemberPageViewModel
        {
            Detail = detail,
            Children = children,
            Payments = payments,
            Ledger = ledger,
            Series = BalanceTrend.Of(ledger.Select(line => line.BalanceAfter), LedgerPreview),
            Flights = FlightList.Of(flights, clock.Today(), listing => $"/admin/reservations/{listing.Id}"),
            Balance = await points.Balance(userId),
            ContactTypes = await members.ContactTypes(),
        };
    }
}
