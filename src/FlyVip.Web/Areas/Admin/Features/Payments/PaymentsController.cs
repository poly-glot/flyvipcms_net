using System.Globalization;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Payments;

[Area("Admin")]
[Authorize(Policy = "admin")]
[Route("admin/payments")]
public sealed class PaymentsController(PaymentQueries queries, PaymentService payments) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q) => View(await queries.Index((q ?? "").Trim()));

    [HttpGet("new")]
    public Task<IActionResult> New([FromQuery(Name = "user_id")] int? userId) =>
        Form(new PaymentFormViewModel { PaidOn = queries.Today }, userId ?? 0);

    [HttpPost("")]
    public async Task<IActionResult> Create(PaymentFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return await Form(form, Selected(form));
        }

        if (form.LacksFreeAmount())
        {
            ModelState.AddModelError("amount", "Amount is required for this payment type.");

            return await Form(form, Selected(form));
        }

        try
        {
            var id = await form.Record(payments, User.Id());
            TempData[FlashKey.Message] = "Payment recorded.";

            return Redirect($"/admin/payments/{id}");
        }
        catch (DomainRuleException error)
        {
            ModelState.AddModelError("payment", error.Message);

            return await Form(form, Selected(form));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id) =>
        await queries.Find(id) is { } row ? View(row) : NotFound();

    private static int Selected(PaymentFormViewModel form) => int.TryParse(form.UserId, CultureInfo.InvariantCulture, out var userId) ? userId : 0;

    private async Task<IActionResult> Form(PaymentFormViewModel form, int selected)
    {
        form.UseChoices(await queries.FormChoices(selected));

        return View(nameof(Form), form);
    }
}
