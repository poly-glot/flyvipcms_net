using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(Code), IsUnique = true)]
public sealed class MembershipPlan
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte Id { get; set; }

    [MaxLength(1)]
    public string Code { get; set; } = "";

    [MaxLength(40)]
    public string Name { get; set; } = "";

    [MaxLength(2)]
    public string OwnerPrefix { get; set; } = "";

    public bool AllowsSubmembers { get; set; }
}
