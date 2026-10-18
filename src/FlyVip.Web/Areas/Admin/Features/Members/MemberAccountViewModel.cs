using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Models;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Areas.Admin.Features.Members;

public abstract class MemberAccountViewModel : NamedProfileViewModel
{
    protected const int AfterProfile = 10_001;

    protected const int AfterAccount = 10_002;

    [FromForm(Name = "username")]
    [Display(Name = "username", Order = AfterProfile)]
    [Required]
    [RegularExpression(@"(?i)\A[A-Z0-9 ~!#$%\&\*\-_+=|:.]+\z", ErrorMessage = "The {0} field may contain only alphanumeric characters, spaces, and  ~ ! # $ % & * - _ + = | : . characters.")]
    [MinCharacters(3)]
    [MaxCharacters(30)]
    public string? Username { get; set; }

    [FromForm(Name = "email")]
    [Display(Name = "email", Order = AfterProfile)]
    [Required]
    [EmailFormat]
    public string? Email { get; set; }

    [FromForm(Name = "password")]
    [Display(Name = "password", Order = AfterAccount)]
    public abstract string? Password { get; set; }

    protected Account Account() => new(Username ?? "", Email ?? "", Password ?? "");
}

public sealed class NewMemberViewModel : MemberAccountViewModel
{
    [Required]
    [MinCharacters(8)]
    [MaxBytes(72)]
    public override string? Password { get; set; }

    [FromForm(Name = "plan_id")]
    [Display(Name = "plan_id", Order = AfterAccount)]
    [Required]
    [PositiveWholeNumber]
    public string? PlanId { get; set; }

    [FromForm(Name = "joining_fee")]
    [Display(Name = "joining_fee", Order = AfterAccount)]
    [Required]
    [DecimalNumber]
    [AtLeast(0)]
    public string? JoiningFee { get; set; }

    [FromForm(Name = "yearly_fee")]
    [Display(Name = "yearly_fee", Order = AfterAccount)]
    [Required]
    [DecimalNumber]
    [GreaterThan(0)]
    public string? YearlyFee { get; set; }

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> Plans { get; private set; } = [];

    public void UsePlans(IReadOnlyList<KeyValuePair<string, string>> plans) => Plans = plans;

    public Task<int> Create(MemberService members) => members.Create(
        Account(),
        ToProfile(),
        PositiveWholeNumbersAttribute.Natural(PlanId),
        FormValues.ParseDecimal(JoiningFee!),
        FormValues.ParseDecimal(YearlyFee!));
}

public sealed class MemberEditViewModel : MemberAccountViewModel
{
    [MinCharacters(8)]
    [MaxBytes(72)]
    public override string? Password { get; set; }

    [BindNever]
    public int UserId { get; private set; }

    [BindNever]
    public string? MemberCode { get; private set; }

    public static MemberEditViewModel Of(MemberDetail detail, Profile profile)
    {
        var form = new MemberEditViewModel { Username = detail.Username, Email = detail.Email };
        form.Load(profile);

        return form;
    }

    public void Identify(int userId, string memberCode)
    {
        UserId = userId;
        MemberCode = memberCode;
    }

    public Task Save(MemberService members, int userId) => members.Update(userId, Account(), ToProfile());
}

public class SubMemberViewModel : MemberAccountViewModel
{
    [Required]
    [MinCharacters(8)]
    [MaxBytes(72)]
    public override string? Password { get; set; }

    [FromForm(Name = "contact_type_id")]
    [Display(Name = "contact_type_id", Order = AfterAccount)]
    [Required]
    [PositiveWholeNumber]
    public string? ContactTypeId { get; set; }

    public Task<int> AddTo(MemberService members, int parentUserId) =>
        members.AddSubMember(parentUserId, Account(), ToProfile(), PositiveWholeNumbersAttribute.Natural(ContactTypeId));
}
