using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public sealed record Account(string Username, string Email, string Password);

public sealed record MemberOption(int UserId, string MemberCode, string FirstName, string LastName, decimal Balance);

public sealed record ProfileChoices(
    IReadOnlyList<KeyValuePair<string, string>> Countries,
    IReadOnlyList<KeyValuePair<string, string>> Zones,
    IReadOnlyList<KeyValuePair<string, string>> Routes);

public sealed class MemberService(AppDbContext db, IPasswordHasher<AppUser> hasher)
{
    public Task<int> Create(Account account, Profile profile, int planId, decimal joiningFee, decimal yearlyFee) => Atomic.Run(db, async () =>
    {
        var plan = await db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == planId) ?? throw new DomainRuleException("Unknown membership plan.");
        var user = await Enrol(account, UserRole.Member, profile);

        return await Join(user, new Member
        {
            PlanId = plan.Id,
            MemberCode = Code(plan.OwnerPrefix, user.Id),
            MembershipName = Code(plan.Code, user.Id),
            JoiningFee = joiningFee,
            YearlyFee = yearlyFee,
            Status = MemberStatus.Inactive,
        });
    });

    public Task<int> AddSubMember(int parentUserId, Account account, Profile profile, int contactTypeId) => Atomic.Run(db, async () =>
    {
        var plan = await db.Members.Where(m => m.UserId == parentUserId).Select(m => m.Plan).FirstOrDefaultAsync()
            ?? throw new DomainRuleException("Sub-members can only be attached to a primary member.");

        if (!plan.AllowsSubmembers)
        {
            throw new DomainRuleException($"The {plan.Name} plan does not allow sub-members.");
        }

        var contactType = await db.ContactTypes.FirstOrDefaultAsync(t => t.Id == contactTypeId) ?? throw new DomainRuleException("Unknown contact type.");
        var user = await Enrol(account, UserRole.Submember, profile);

        return await Join(user, new Member
        {
            ParentUserId = parentUserId,
            ContactTypeId = contactType.Id,
            MemberCode = Code(plan.Code + contactType.Suffix, user.Id),
            MembershipName = Code(plan.Code, user.Id),
            Status = MemberStatus.Inactive,
        });
    });

    public async Task Update(int userId, Account account, Profile profile)
    {
        var user = await db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId) ?? throw new DomainRuleException("Member not found.");
        var now = DateTime.Now;

        user.Username = string.IsNullOrEmpty(account.Username) ? user.Username : account.Username;
        user.Email = string.IsNullOrEmpty(account.Email) ? user.Email : account.Email;
        user.PasswordHash = string.IsNullOrEmpty(account.Password) ? user.PasswordHash : hasher.HashPassword(user, account.Password);
        user.UpdatedAt = now;

        if (user.Profile is { } existing)
        {
            profile.UserId = userId;
            profile.CreatedAt = existing.CreatedAt;
            profile.UpdatedAt = now;
            db.Entry(existing).CurrentValues.SetValues(profile);
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateOwnProfile(int userId, Profile posted)
    {
        var existing = await db.Profiles.SingleAsync(p => p.UserId == userId);

        posted.UserId = userId;
        posted.FirstName = existing.FirstName;
        posted.LastName = existing.LastName;
        posted.CreatedAt = existing.CreatedAt;
        posted.UpdatedAt = DateTime.Now;
        db.Entry(existing).CurrentValues.SetValues(posted);
        await db.SaveChangesAsync();
    }

    public async Task SetStatus(int userId, MemberStatus status)
    {
        var user = await db.Users.IgnoreQueryFilters().Include(u => u.Member).FirstOrDefaultAsync(u => u.Id == userId) ?? throw new DomainRuleException("Member not found.");

        if (user.Member is not { } member)
        {
            return;
        }

        member.Status = status;
        member.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task Remove(int userId)
    {
        var now = DateTime.Now;
        var family = await db.Members.IgnoreQueryFilters()
            .Where(m => m.UserId == userId || (m.ParentUserId == userId && m.DeletedAt == null))
            .ToListAsync();

        foreach (var member in family)
        {
            member.DeletedAt = now;
        }

        await db.SaveChangesAsync();
    }

    public Task<List<MemberOption>> PrimaryMembers(bool activeOnly = false, bool withBalances = false)
    {
        var primaries = db.Members
            .Where(m => m.PlanId != null && (!activeOnly || m.Status == MemberStatus.Active))
            .OrderBy(m => m.User.Profile!.LastName);

        return withBalances
            ? primaries
                .Select(m => new MemberOption(
                    m.UserId,
                    m.MemberCode,
                    m.User.Profile!.FirstName,
                    m.User.Profile.LastName,
                    db.PointsLedger.Where(entry => entry.UserId == m.UserId).OrderByDescending(entry => entry.Id).Select(entry => (decimal?)entry.BalanceAfter).FirstOrDefault() ?? 0m))
                .ToListAsync()
            : primaries
                .Select(m => new MemberOption(m.UserId, m.MemberCode, m.User.Profile!.FirstName, m.User.Profile.LastName, 0m))
                .ToListAsync();
    }

    public Task<bool> UsernameTaken(string? username, int? exceptUserId) =>
        db.Users.AnyAsync(user => user.Username == username && user.Id != exceptUserId);

    public Task<bool> EmailTaken(string? email, int? exceptUserId) =>
        db.Users.AnyAsync(user => user.Email == email && user.Id != exceptUserId);

    public Task<Profile> ProfileOf(int userId) => db.Profiles.AsNoTracking().SingleAsync(profile => profile.UserId == userId);

    public async Task<IReadOnlyList<KeyValuePair<string, string>>> Plans() =>
        [.. (await db.MembershipPlans.OrderBy(plan => plan.Id).ToListAsync()).Select(plan => Menus.Option(plan.Id, plan.Name))];

    public async Task<IReadOnlyList<KeyValuePair<string, string>>> ContactTypes() =>
        [.. (await db.ContactTypes.OrderBy(type => type.Id).ToListAsync()).Select(type => Menus.Option(type.Id, type.Name))];

    public async Task<ProfileChoices> ProfileOptions() => new(
        await db.Countries.Where(c => c.Active).OrderBy(c => c.Name).Select(c => Menus.Option(c.Id, c.Name)).ToListAsync(),
        await db.CountryZones.Where(z => z.Active).OrderBy(z => z.Name).Select(z => Menus.Option(z.Id, z.Name)).ToListAsync(),
        await db.RouteMenu());

    private static string Code(string prefix, int userId) => $"{prefix}-{userId:D3}";

    private async Task<AppUser> Enrol(Account account, UserRole role, Profile profile)
    {
        var now = DateTime.Now;
        var user = new AppUser { Username = account.Username, Email = account.Email, Role = role, CreatedAt = now, UpdatedAt = now, Profile = profile };

        profile.CreatedAt = profile.UpdatedAt = now;
        user.PasswordHash = hasher.HashPassword(user, account.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    private async Task<int> Join(AppUser user, Member member)
    {
        member.CreatedAt = member.UpdatedAt = DateTime.Now;
        user.Member = member;
        await db.SaveChangesAsync();

        return user.Id;
    }
}
