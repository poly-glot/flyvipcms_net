using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Features.Auth;

public sealed class LoginViewModel
{
    [FromForm(Name = "email")]
    [Display(Name = "Email Address")]
    [Required]
    [MaxCharacters(254)]
    [EmailFormat]
    public string? Email { get; init; }

    [FromForm(Name = "password")]
    [Display(Name = "Password")]
    [Required]
    [MaxBytes(72)]
    public string? Password { get; init; }
}
