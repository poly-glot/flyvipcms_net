using FlyVip.Web.Infrastructure;
using FlyVip.Web.Models;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Portal.Features.Profile;

[Route("portal/profile")]
public sealed class ProfileController(PortalMemberQueries portal, MemberService members) : PortalController(portal)
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (await CurrentMember() is not { } member)
        {
            return NotFound();
        }

        var form = new ProfileViewModel();
        form.Load(await members.ProfileOf(member.UserId));

        return await Page(member, form);
    }

    [HttpPost("")]
    public async Task<IActionResult> Index(ProfileViewModel form)
    {
        if (await CurrentMember() is not { } member)
        {
            return NotFound();
        }

        if (!member.IsPrimary)
        {
            TempData[FlashKey.Error] = "Only the primary member can edit the profile.";

            return Redirect("/portal");
        }

        if (!ModelState.IsValid)
        {
            return await Page(member, form);
        }

        await members.UpdateOwnProfile(member.UserId, form.ToProfile());

        TempData[FlashKey.Message] = "Profile updated.";

        return Redirect("/portal/profile");
    }

    private async Task<IActionResult> Page(PortalMember member, ProfileViewModel form)
    {
        form.UseChoices(await members.ProfileOptions());

        return View(new PortalProfile(form, !member.IsPrimary));
    }
}
