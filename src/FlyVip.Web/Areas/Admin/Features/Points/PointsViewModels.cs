using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Areas.Admin.Features.Points;

public abstract class PointsFormViewModel
{
    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> Members { get; private set; } = [];

    public void UseMembers(IReadOnlyList<KeyValuePair<string, string>> members) => Members = members;
}

public sealed class PointsAdjustViewModel : PointsFormViewModel
{
    [FromForm(Name = "user_id")]
    [Display(Name = "user_id")]
    [Required]
    [PositiveWholeNumber]
    public string? UserId { get; init; }

    [FromForm(Name = "amount")]
    [Display(Name = "amount")]
    [Required]
    [DecimalNumber]
    [GreaterThan(0)]
    public string? Amount { get; init; }

    [FromForm(Name = "remark")]
    [Display(Name = "remark")]
    [Required]
    [MaxCharacters(255)]
    public string? Remark { get; init; }

    public Task Credit(PointsService points) =>
        points.Credit(PositiveWholeNumbersAttribute.Natural(UserId), FormValues.ParseDecimal(Amount!), LedgerKind.Adjustment, $"Points added by admin: {Remark}");
}

public sealed class PointsTransferViewModel : PointsFormViewModel
{
    [FromForm(Name = "sender_id")]
    [Display(Name = "sender_id")]
    [Required]
    [PositiveWholeNumber]
    public string? SenderId { get; init; }

    [FromForm(Name = "receiver_id")]
    [Display(Name = "receiver_id")]
    [Required]
    [PositiveWholeNumber]
    [Differs(nameof(SenderId))]
    public string? ReceiverId { get; init; }

    [FromForm(Name = "amount")]
    [Display(Name = "amount")]
    [Required]
    [DecimalNumber]
    [GreaterThan(0)]
    public string? Amount { get; init; }

    public Task Transfer(PointsService points, int adminId) =>
        points.Transfer(PositiveWholeNumbersAttribute.Natural(SenderId), PositiveWholeNumbersAttribute.Natural(ReceiverId), FormValues.ParseDecimal(Amount!), adminId);
}
