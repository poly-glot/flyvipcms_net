using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Areas.Admin.Features.Payments;

public sealed class PaymentQueries(AppDbContext db, MemberService members, PaymentService payments, TimeProvider clock)
{
    public DateOnly Today => clock.Today();

    public async Task<PaymentIndexViewModel> Index(string search)
    {
        var matching = db.Payments.IgnoreQueryFilters().Where(payment => payment.DeletedAt == null);

        if (search != "")
        {
            matching = matching.Where(payment =>
                payment.User.Member!.MemberCode.Contains(search) ||
                payment.User.Profile!.FirstName.Contains(search) ||
                payment.User.Profile.LastName.Contains(search) ||
                payment.Remark.Contains(search) ||
                (payment.Id + "").Contains(search));
        }

        return new PaymentIndexViewModel(await Project(matching.OrderByDescending(payment => payment.Id)).ToListAsync(), search);
    }

    public Task<PaymentLine?> Find(int id) =>
        Project(db.Payments.IgnoreQueryFilters().Where(payment => payment.Id == id)).FirstOrDefaultAsync();

    public async Task<PaymentChoices> FormChoices(int selected)
    {
        var primaries = await members.PrimaryMembers();
        var payable = primaries.Exists(member => member.UserId == selected);
        var allowed = payable ? await payments.AllowedTypes(selected) : [];
        var expected = new Dictionary<PaymentType, decimal>();

        foreach (var type in allowed)
        {
            if (await payments.ExpectedAmount(selected, type) is { } amount)
            {
                expected[type] = amount;
            }
        }

        return new PaymentChoices(primaries.Menu(), payable ? selected : 0, allowed, expected, PaymentService.TermStartOptions(Today));
    }

    private static IQueryable<PaymentLine> Project(IQueryable<Payment> source) =>
        source.Select(payment => new PaymentLine(
            payment.Id,
            payment.UserId,
            payment.PaidOn,
            payment.Type,
            payment.Remark,
            payment.Description,
            payment.Amount,
            payment.User.Member!.MemberCode,
            payment.User.Profile!.FirstName,
            payment.User.Profile.LastName));
}
