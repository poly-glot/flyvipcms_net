using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Features.Account;

[Authorize]
public sealed class AccountController(AppDbContext db, IPasswordHasher<AppUser> hasher) : Controller
{
    [HttpGet("/account/password")]
    public IActionResult Password() => View(new PasswordViewModel());

    [HttpPost("/account/password")]
    public async Task<IActionResult> Password(PasswordViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var userId = User.Id();
        var user = await db.Users.SingleAsync(u => u.Id == userId);

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, form.CurrentPassword!) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("current_password", "Current password is incorrect.");
            return View(form);
        }

        if (PasswordStrength.Weakness(form.NewPassword!, user.Username, user.Email) is { } weakness)
        {
            ModelState.AddModelError("new_password", weakness);
            return View(form);
        }

        user.PasswordHash = hasher.HashPassword(user, form.NewPassword!);
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        TempData[FlashKey.Message] = "Password updated.";

        return RedirectToAction(nameof(Password));
    }
}
