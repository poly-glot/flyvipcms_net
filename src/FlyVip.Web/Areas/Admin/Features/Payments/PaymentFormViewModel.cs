using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Areas.Admin.Features.Payments;

public sealed record PaymentIndexViewModel(IReadOnlyList<PaymentLine> Payments, string Search);

public sealed record PaymentChoices(
    IReadOnlyList<KeyValuePair<string, string>> Members,
    int Selected,
    IReadOnlyList<PaymentType> Allowed,
    IReadOnlyDictionary<PaymentType, decimal> Expected,
    IReadOnlyList<DateOnly> TermStarts);

public sealed record PaymentTypeOption(string Value, string Label, string AmountText);

public sealed class PaymentFormViewModel
{
    [FromForm(Name = "user_id")]
    [Display(Name = "user_id")]
    [Required]
    [PositiveWholeNumber]
    public string? UserId { get; init; }

    [FromForm(Name = "type")]
    [Display(Name = "type")]
    [Required]
    [RegularExpression("joining|yearly|quarterly|bonus|points", ErrorMessage = "The {0} field must be one of: joining,yearly,quarterly,bonus,points.")]
    public string? Type { get; init; }

    [FromForm(Name = "paid_on")]
    [Display(Name = "paid_on")]
    [Required]
    public DateOnly? PaidOn { get; init; }

    [FromForm(Name = "remark")]
    [Display(Name = "remark")]
    [Required]
    [MaxCharacters(255)]
    public string? Remark { get; init; }

    [FromForm(Name = "amount")]
    [Display(Name = "amount")]
    [DecimalNumber]
    [GreaterThan(0)]
    public string? Amount { get; init; }

    [FromForm(Name = "quarters")]
    [Display(Name = "quarters")]
    [RegularExpression("[1-4]", ErrorMessage = "The {0} field must be one of: 1,2,3,4.")]
    public string? Quarters { get; init; }

    [FromForm(Name = "term_start")]
    [Display(Name = "term_start")]
    public DateOnly? TermStart { get; init; }

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> Members { get; private set; } = [];

    [BindNever]
    public int Selected { get; private set; }

    [BindNever]
    public IReadOnlyList<PaymentTypeOption> TypeOptions { get; private set; } = [];

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> QuarterChoices { get; } = [.. Enumerable.Range(1, 4).Select(count => KeyValuePair.Create(count.ToString(CultureInfo.InvariantCulture), $"{count} {UiHelpers.UiPlural(count, "quarter")}"))];

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> TermChoices { get; private set; } = [];

    [BindNever]
    public string CheckedType => Type ?? TypeOptions[0].Value;

    public bool LacksFreeAmount() => PaymentService.FreeAmountTypes.Contains(Kind()) && Amount is null;

    public void UseChoices(PaymentChoices choices)
    {
        Members = choices.Members;
        Selected = choices.Selected;
        TypeOptions = [.. choices.Allowed.Select(type => new PaymentTypeOption(EnumText.Of(type), type.ToString(), AmountText(choices, type)))];
        TermChoices = [.. choices.TermStarts.Select(start => KeyValuePair.Create(start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), UiHelpers.UiDate(start)))];
    }

    public Task<int> Record(PaymentService payments, int adminId) => payments.Record(
        PositiveWholeNumbersAttribute.Natural(UserId),
        Kind(),
        PaidOn!.Value,
        Remark!,
        adminId,
        Amount is null ? null : FormValues.ParseDecimal(Amount),
        Quarters.IntOr(1),
        TermStart);

    private static string AmountText(PaymentChoices choices, PaymentType type) =>
        choices.Expected.TryGetValue(type, out var amount)
            ? amount.ToString("0.00", CultureInfo.InvariantCulture) + (type == PaymentType.Quarterly ? " per quarter" : "")
            : "Free amount";

    private PaymentType Kind() => Enum.Parse<PaymentType>(Type!, true);
}

