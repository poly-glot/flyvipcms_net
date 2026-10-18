using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public sealed class PaymentService(AppDbContext db, PointsService points)
{
    public static readonly IReadOnlyList<PaymentType> FreeAmountTypes = [PaymentType.Bonus, PaymentType.Points];

    public async Task<IReadOnlyList<PaymentType>> AllowedTypes(int userId) => await AllowedTypes(await PrimaryMember(userId));

    public async Task<decimal?> ExpectedAmount(int userId, PaymentType type) => ExpectedAmount(await PrimaryMember(userId), type, 1);

    public Task<int> Record(int userId, PaymentType type, DateOnly paidOn, string remark, int adminId, decimal? freeAmount = null, int quarters = 1, DateOnly? termStart = null) =>
        Atomic.Run(db, async () =>
        {
            var member = await PrimaryMember(userId);

            if (!(await AllowedTypes(member)).Contains(type))
            {
                throw new DomainRuleException($"A {EnumText.Of(type)} payment is not allowed for this member right now.");
            }

            var amount = ExpectedAmount(member, type, quarters) ?? PointsService.Normalise(freeAmount ?? 0m);

            if (amount <= 0)
            {
                throw new DomainRuleException("Payment amount must be greater than zero.");
            }

            var paymentId = await InsertPayment(userId, type, amount, paidOn, remark, adminId);

            await (type switch
            {
                PaymentType.Joining => ApplyJoining(member, paidOn),
                PaymentType.Yearly => ApplyYearly(member, paymentId, paidOn, termStart),
                PaymentType.Quarterly => ApplyQuarterly(member, paymentId, paidOn, quarters),
                PaymentType.Bonus => points.Credit(userId, amount, LedgerKind.Payment, "Bonus points", paymentId),
                PaymentType.Points => points.Credit(userId, amount, LedgerKind.Payment, "Points purchased", paymentId),
                _ => throw new DomainRuleException($"Unknown payment type {EnumText.Of(type)}."),
            });

            return paymentId;
        });

    public static DateOnly[] TermStartOptions(DateOnly today)
    {
        var current = QuarterCalendar.QuarterStart(today);

        return [current, current.AddMonths(3)];
    }

    private static decimal QuarterShare(Member member) => Math.Round(member.YearlyFee / 4, 2, MidpointRounding.ToZero);

    private static decimal? ExpectedAmount(Member member, PaymentType type, int quarters) => type switch
    {
        PaymentType.Joining => member.JoiningFee,
        PaymentType.Yearly => member.YearlyFee,
        PaymentType.Quarterly => QuarterShare(member) * quarters,
        _ => null,
    };

    private async Task<IReadOnlyList<PaymentType>> AllowedTypes(Member member)
    {
        if (member.JoiningFeePaidAt is null)
        {
            return [PaymentType.Joining];
        }

        List<PaymentType> types = [PaymentType.Quarterly, PaymentType.Points];

        if (await CanStartYearlyTerm(member.Id))
        {
            types.Insert(0, PaymentType.Yearly);
        }

        if (member.Status == MemberStatus.Active)
        {
            types.Add(PaymentType.Bonus);
        }

        return types;
    }

    private async Task ApplyJoining(Member member, DateOnly paidOn)
    {
        member.JoiningFeePaidAt = paidOn.ToDateTime(TimeOnly.MinValue);
        member.Status = MemberStatus.Active;
        member.NextDueOn = QuarterCalendar.FirstDueAfterJoining(paidOn);
        member.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
    }

    private async Task ApplyYearly(Member member, int paymentId, DateOnly paidOn, DateOnly? termStart)
    {
        var start = await NextTermStart(member.Id, paidOn, termStart);

        var term = NewTerm(member.Id, start);

        term.Quarters.AddRange(Enumerable.Range(1, 4).Select(quarter => Quarter(quarter, start, paymentId, quarter == 1 ? member.YearlyFee : 0m)));
        await db.SaveChangesAsync();

        await points.Credit(member.UserId, member.YearlyFee, LedgerKind.Payment, $"Yearly fee {start:yyyy-MM-dd}", paymentId);
        await SetNextDue(member, QuarterCalendar.TermEnd(start));
    }

    private async Task<DateOnly> NextTermStart(int memberId, DateOnly paidOn, DateOnly? requested)
    {
        if (await LatestTerm(memberId) is { } last)
        {
            return last.EndsOn.AddDays(1);
        }

        var options = TermStartOptions(paidOn);
        var start = requested ?? options[0];

        return options.Contains(start) ? start : throw new DomainRuleException("Choose the current or the next quarter as the term start.");
    }

    private async Task ApplyQuarterly(Member member, int paymentId, DateOnly paidOn, int quarters)
    {
        if (quarters is < 1 or > 4)
        {
            throw new DomainRuleException("Pay between one and four quarters at a time.");
        }

        var perQuarter = QuarterShare(member);
        var lastEnd = paidOn;

        for (var i = 0; i < quarters; i++)
        {
            var (term, quarter) = await NextOpenQuarter(member.Id, paidOn);
            var paidQuarter = Quarter(quarter, term.StartsOn, paymentId, perQuarter);

            term.Quarters.Add(paidQuarter);
            await db.SaveChangesAsync();
            lastEnd = paidQuarter.EndsOn;
        }

        await points.Credit(member.UserId, perQuarter * quarters, LedgerKind.Payment, $"Quarterly fee x{quarters}", paymentId);
        await SetNextDue(member, lastEnd);
    }

    private async Task<(MembershipTerm Term, int Quarter)> NextOpenQuarter(int memberId, DateOnly paidOn)
    {
        if (await LatestTerm(memberId) is not { } term)
        {
            return (NewTerm(memberId, QuarterCalendar.NextQuarterStart(paidOn)), 1);
        }

        var paid = await PaidQuarters(term.Id);

        return paid < 4 ? (term, paid + 1) : (NewTerm(memberId, term.EndsOn.AddDays(1)), 1);
    }

    private async Task<bool> CanStartYearlyTerm(int memberId) =>
        await LatestTerm(memberId) is not { } term || await PaidQuarters(term.Id) >= 4;

    private Task<MembershipTerm?> LatestTerm(int memberId) =>
        db.MembershipTerms.Where(t => t.MemberId == memberId).OrderByDescending(t => t.Id).FirstOrDefaultAsync();

    private Task<int> PaidQuarters(int termId) => db.MembershipQuarters.CountAsync(q => q.TermId == termId);

    private async Task<int> InsertPayment(int userId, PaymentType type, decimal amount, DateOnly paidOn, string remark, int adminId)
    {
        var now = DateTime.Now;
        var payment = new Payment
        {
            UserId = userId,
            Type = type,
            Description = $"{type} payment",
            Amount = amount,
            PaidOn = paidOn,
            Remark = remark,
            CreatedById = adminId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        return payment.Id;
    }

    private MembershipTerm NewTerm(int memberId, DateOnly start) =>
        db.MembershipTerms.Add(new MembershipTerm { MemberId = memberId, StartsOn = start, EndsOn = QuarterCalendar.TermEnd(start), CreatedAt = DateTime.Now }).Entity;

    private static MembershipQuarter Quarter(int quarter, DateOnly termStart, int paymentId, decimal pointsLoaded)
    {
        var start = QuarterCalendar.QuarterStartInTerm(termStart, quarter);

        return new MembershipQuarter { PaymentId = paymentId, Quarter = (byte)quarter, StartsOn = start, EndsOn = QuarterCalendar.QuarterEnd(start), PointsLoaded = pointsLoaded };
    }

    private async Task SetNextDue(Member member, DateOnly periodEnd)
    {
        member.NextDueOn = QuarterCalendar.DueAfter(periodEnd);
        member.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
    }

    private async Task<Member> PrimaryMember(int userId) =>
        await db.Members.FirstOrDefaultAsync(m => m.UserId == userId && m.PlanId != null)
            ?? throw new DomainRuleException("Payments can only be recorded for primary members.");
}
