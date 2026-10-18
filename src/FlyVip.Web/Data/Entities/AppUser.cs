using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public sealed class AppUser
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string Username { get; set; } = "";

    [MaxLength(254)]
    public string Email { get; set; } = "";

    [MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    public UserRole Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Profile? Profile { get; set; }

    [InverseProperty(nameof(Entities.Member.User))]
    public Member? Member { get; set; }
}
