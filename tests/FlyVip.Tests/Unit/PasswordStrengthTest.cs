using FlyVip.Web.Infrastructure;

namespace FlyVip.Tests.Unit;

public sealed class PasswordStrengthTest
{
    [Theory]
    [InlineData("An0ther-Str0ng-Pass-456", null)]
    [InlineData("       short ", "Passwords must be at least 8 characters long.")]
    [InlineData("member7-rocks-hard", "Passwords cannot contain re-hashed personal information.")]
    [InlineData("example-jet-2026", "Passwords cannot contain re-hashed personal information.")]
    [InlineData("membxr77", "Password is too similar to the username.")]
    public void JudgesPasswordsLikeShield(string password, string? weakness)
    {
        Assert.Equal(weakness, PasswordStrength.Weakness(password, "member7", "member7@example.test"));
    }
}
