using System.Globalization;
using System.Security.Claims;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Features.Auth;

public sealed class AuthController(AppDbContext db, IPasswordHasher<AppUser> hasher) : Controller
{
    public const string BeforeLoginUrlKey = "beforeLoginUrl";
    public const int BeforeLoginUrlMaxLength = 512;

    [HttpGet("/login")]
    public IActionResult Login() => User.Identity?.IsAuthenticated == true ? LocalRedirect(LoginRedirect()) : View(new LoginViewModel());

    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == form.Email);
        var refusal = await Refusal(user, form.Password!);

        if (user is null || refusal is not null)
        {
            TempData[FlashKey.Error] = refusal;
            return View(form);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Principal(user));

        return LocalRedirect(LoginRedirect());
    }

    [HttpPost("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData[FlashKey.Message] = "You have successfully logged out.";

        return Redirect("/login");
    }

    private string LoginRedirect() => TempData[BeforeLoginUrlKey] as string ?? "/";

    private async Task<string?> Refusal(AppUser? user, string password)
    {
        if (user is null)
        {
            return "Unable to log you in. Please check your credentials.";
        }

        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return "Unable to log you in. Please check your password.";
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
            await db.SaveChangesAsync();
        }

        var member = await db.Members.IgnoreQueryFilters()
            .Where(m => m.UserId == user.Id)
            .Select(m => new { m.Status, m.DeletedAt })
            .SingleOrDefaultAsync();

        return member switch
        {
            { DeletedAt: not null } => "Member removed",
            { Status: MemberStatus.Banned } => "Banned by admin",
            _ => null,
        };
    }

    private static ClaimsPrincipal Principal(AppUser user) => new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, EnumText.Of(user.Role)),
        ],
        CookieAuthenticationDefaults.AuthenticationScheme));
}
