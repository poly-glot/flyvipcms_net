using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data.Entities;

public sealed class ReservationActivity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public AppUser User { get; set; } = null!;

    public int? ReservationId { get; set; }

    [DeleteBehavior(DeleteBehavior.SetNull)]
    public Reservation? Reservation { get; set; }

    [MaxLength(255)]
    public string Description { get; set; } = "";

    public DateTime? CreatedAt { get; set; }
}
