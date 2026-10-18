using System.ComponentModel.DataAnnotations;

namespace FlyVip.Web.Data.Entities;

public sealed class Pilot : ISoftDeletable
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string FirstName { get; set; } = "";

    [MaxLength(80)]
    public string LastName { get; set; } = "";

    public DateOnly? BirthDate { get; set; }

    [MaxLength(80)]
    public string? City { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(80)]
    public string? Country { get; set; }

    [MaxLength(30)]
    public string? CellPhone { get; set; }

    [MaxLength(30)]
    public string? WorkPhone { get; set; }

    [MaxLength(30)]
    public string? HomePhone { get; set; }

    [MaxLength(120)]
    public string? EmailPrimary { get; set; }

    [MaxLength(120)]
    public string? EmailSecondary { get; set; }

    [MaxLength(40)]
    public string? ContractType { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal HourlySalary { get; set; }

    public decimal ExtraBonus { get; set; }

    [MaxLength(120)]
    public string? TrainingIncluded { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
