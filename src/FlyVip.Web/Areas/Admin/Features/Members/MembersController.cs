using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Members;

[Area("Admin")]
[Authorize(Policy = "admin")]
[Route("admin/members")]
public sealed class MembersController(MemberQueries queries, MemberService members) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await queries.Index());

    [HttpGet("new")]
    public Task<IActionResult> New() => Form(new NewMemberViewModel { JoiningFee = "5000", YearlyFee = "12000" });

    [HttpPost("")]
    public async Task<IActionResult> Create(NewMemberViewModel form)
    {
        await RejectTaken(form, null);

        if (!ModelState.IsValid)
        {
            return await Form(form);
        }

        try
        {
            var userId = await form.Create(members);

            return Back($"/admin/members/{userId}", "Member created. Record the joining fee to activate the membership.");
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("member", error.Message);

            return await Form(form);
        }
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> Show(int userId) => await queries.Detail(userId) is { } detail ? View(nameof(Show), await queries.Page(detail)) : NotFound();

    [HttpGet("{userId:int}/edit")]
    public async Task<IActionResult> Edit(int userId) => await queries.Detail(userId) is { } detail ? await Form(MemberEditViewModel.Of(detail, await members.ProfileOf(userId)), detail) : NotFound();

    [HttpPost("{userId:int}")]
    public async Task<IActionResult> Update(int userId, MemberEditViewModel form)
    {
        if (await queries.Detail(userId) is not { } detail)
        {
            return NotFound();
        }

        await RejectTaken(form, userId);

        if (!ModelState.IsValid)
        {
            return await Form(form, detail);
        }

        try
        {
            await form.Save(members, userId);
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("member", error.Message);

            return await Form(form, detail);
        }

        return Back($"/admin/members/{userId}", "Member updated.");
    }

    [HttpPost("{userId:int}/status")]
    public async Task<IActionResult> Status(
        int userId,
        [FromForm(Name = "status")]
        [Required]
        [RegularExpression("inactive|active|banned", ErrorMessage = "The {0} field must be one of: inactive,active,banned.")]
        string? status)
    {
        if (await queries.Detail(userId) is not { } detail)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(Show), await queries.Page(detail));
        }

        await members.SetStatus(userId, Enum.Parse<MemberStatus>(status!, true));

        return Back($"/admin/members/{userId}", "Membership status updated.");
    }

    [HttpPost("{userId:int}/sub-members")]
    public async Task<IActionResult> AddSubMember(int userId, SubMemberViewModel form)
    {
        if (await queries.Detail(userId) is not { } detail)
        {
            return NotFound();
        }

        await RejectTaken(form, null);

        if (!ModelState.IsValid)
        {
            return View(nameof(Show), await queries.Page(detail));
        }

        try
        {
            await form.AddTo(members, userId);
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("member", error.Message);

            return View(nameof(Show), await queries.Page(detail));
        }

        return Back($"/admin/members/{userId}", "Sub-member added.");
    }

    [HttpPost("{userId:int}/delete")]
    public async Task<IActionResult> Delete(int userId)
    {
        if (await queries.Detail(userId) is null)
        {
            return NotFound();
        }

        await members.Remove(userId);

        return Back("/admin/members", "Member and sub-members removed.");
    }

    private async Task RejectTaken(MemberAccountViewModel form, int? userId)
    {
        if (await members.UsernameTaken(form.Username, userId))
        {
            ModelState.AddModelError("username", "The username field must contain a unique value.");
        }

        if (await members.EmailTaken(form.Email, userId))
        {
            ModelState.AddModelError("email", "The email field must contain a unique value.");
        }
    }

    private async Task<IActionResult> Form(NewMemberViewModel form)
    {
        form.UseChoices(await members.ProfileOptions());
        form.UsePlans(await members.Plans());

        return View(nameof(Form), form);
    }

    private async Task<IActionResult> Form(MemberEditViewModel form, MemberDetail detail)
    {
        form.UseChoices(await members.ProfileOptions());
        form.Identify(detail.UserId, detail.MemberCode);

        return View(nameof(Form), form);
    }

    private RedirectResult Back(string to, string message)
    {
        TempData[FlashKey.Message] = message;

        return Redirect(to);
    }
}
