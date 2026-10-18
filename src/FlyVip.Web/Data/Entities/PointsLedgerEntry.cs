using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

[Index(nameof(UserId), nameof(Id))]
public sealed class PointsLedgerEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser User { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public LedgerKind Kind { get; set; }

    [MaxLength(255)]
    public string Description { get; set; } = "";

    public int? PaymentId { get; set; }

    public Payment? Payment { get; set; }

    public int? TransferId { get; set; }

    public PointsTransfer? Transfer { get; set; }

    public int? ReservationId { get; set; }

    public Reservation? Reservation { get; set; }

    public DateTime? CreatedAt { get; set; }
}
