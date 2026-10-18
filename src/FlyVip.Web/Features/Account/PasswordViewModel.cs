using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Features.Account;

public sealed class PasswordViewModel
{
    [FromForm(Name = "current_password")]
    [Display(Name = "current_password")]
    [Required]
    public string? CurrentPassword { get; init; }

    [FromForm(Name = "new_password")]
    [Display(Name = "new_password")]
    [Required]
    [MinCharacters(8)]
    [MaxBytes(72)]
    [Differs(nameof(CurrentPassword))]
    public string? NewPassword { get; init; }

    [FromForm(Name = "confirm_password")]
    [Display(Name = "confirm_password")]
    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "The {0} field does not match the {1} field.")]
    public string? ConfirmPassword { get; init; }
}
