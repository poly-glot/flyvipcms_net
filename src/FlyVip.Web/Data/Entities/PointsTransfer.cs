using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class PointsTransfer
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser Sender { get; set; } = null!;

    public int ReceiverId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser Receiver { get; set; } = null!;

    public decimal Amount { get; set; }

    public int CreatedById { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser CreatedBy { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
