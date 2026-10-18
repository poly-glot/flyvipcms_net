using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Points;

[Area("Admin")]
[Authorize(Policy = "admin")]
[Route("admin/points")]
public sealed class PointsController(PointsQueries queries, PointsService points, MemberService members) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await queries.History());

    [HttpGet("adjust")]
    public Task<IActionResult> Adjust() => Form(new PointsAdjustViewModel(), nameof(Adjust));

    [HttpPost("adjust")]
    public async Task<IActionResult> StoreAdjustment(PointsAdjustViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return await Form(form, nameof(Adjust));
        }

        await form.Credit(points);

        return Back("Points added.");
    }

    [HttpGet("transfer")]
    public Task<IActionResult> Transfer() => Form(new PointsTransferViewModel(), nameof(Transfer));

    [HttpPost("transfer")]
    public async Task<IActionResult> StoreTransfer(PointsTransferViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return await Form(form, nameof(Transfer));
        }

        try
        {
            await form.Transfer(points, User.Id());
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("transfer", error.Message);

            return await Form(form, nameof(Transfer));
        }

        return Back("Points transferred.");
    }

    private async Task<IActionResult> Form(PointsFormViewModel form, string view)
    {
        form.UseMembers((await members.PrimaryMembers(withBalances: true)).MenuWithBalances());

        return View(view, form);
    }

    private RedirectResult Back(string message)
    {
        TempData[FlashKey.Message] = message;

        return Redirect("/admin/points");
    }
}
