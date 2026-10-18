using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Feature;

public sealed class AccountTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task PasswordChangeRequiresLogin()
    {
        using var client = Client();

        using var response = await client.GetAsync("/account/password", TestContext.Current.CancellationToken);

        Assert.Equal("/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task UserChangesPasswordWithCorrectCurrentPassword()
    {
        var user = await Login(UserRole.Member);
        using var client = await ActingAs(user);

        using var response = await client.PostAsync("/account/password", Form(
            ("current_password", Password),
            ("new_password", "An0ther-Str0ng-Pass-456"),
            ("confirm_password", "An0ther-Str0ng-Pass-456")), TestContext.Current.CancellationToken);

        Assert.True(await Verifies(user.Id, "An0ther-Str0ng-Pass-456"));
    }

    [Fact]
    public async Task WrongCurrentPasswordChangesNothing()
    {
        var user = await Login(UserRole.Admin);
        using var client = await ActingAs(user);

        using var response = await client.PostAsync("/account/password", Form(
            ("current_password", "not-my-password"),
            ("new_password", "An0ther-Str0ng-Pass-456"),
            ("confirm_password", "An0ther-Str0ng-Pass-456")), TestContext.Current.CancellationToken);

        Assert.Contains("Current password is incorrect.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
        Assert.True(await Verifies(user.Id, Password));
    }

    private async Task<bool> Verifies(int userId, string password)
    {
        var user = await Db.Users.AsNoTracking().SingleAsync(u => u.Id == userId);

        return Hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
    }
}
