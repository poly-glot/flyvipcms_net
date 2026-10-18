using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class Payment : ISoftDeletable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser User { get; set; } = null!;

    public PaymentType Type { get; set; }

    [MaxLength(255)]
    public string Description { get; set; } = "";

    public decimal Amount { get; set; }

    public DateOnly PaidOn { get; set; }

    [MaxLength(255)]
    public string Remark { get; set; } = "";

    public int CreatedById { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser CreatedBy { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
