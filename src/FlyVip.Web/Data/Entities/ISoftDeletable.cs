namespace FlyVip.Web.Data.Entities;

public interface ISoftDeletable
{
    int Id { get; }

    DateTime? CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }

    DateTime? DeletedAt { get; set; }
}
