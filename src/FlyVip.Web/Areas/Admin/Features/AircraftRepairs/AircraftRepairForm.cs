using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.AircraftRepairs;

public sealed class AircraftRepairForm : ICrudForm<AircraftRepair>
{
    [FromForm(Name = "aircraft_id")]
    [Display(Name = "Aircraft")]
    [Required]
    [PositiveWholeNumber]
    public string? AircraftId { get; set; }

    [FromForm(Name = "repaired_on")]
    [Display(Name = "Date")]
    [Required]
    public DateOnly? RepairedOn { get; set; }

    [FromForm(Name = "code")]
    [Display(Name = "Code")]
    [Required]
    [MaxCharacters(50)]
    public string? Code { get; set; }

    [FromForm(Name = "description")]
    [Display(Name = "Description")]
    [Required]
    public string? Description { get; set; }

    [FromForm(Name = "repair_shop")]
    [Display(Name = "Repair shop")]
    [Required]
    [MaxCharacters(100)]
    public string? RepairShop { get; set; }

    [FromForm(Name = "performed_hours")]
    [Display(Name = "Performed hours")]
    [Required]
    [DecimalNumber]
    public string? PerformedHours { get; set; }

    [FromForm(Name = "expires_on")]
    [Display(Name = "Expires on")]
    [Required]
    public DateOnly? ExpiresOn { get; set; }

    [FromForm(Name = "expiry_hours")]
    [Display(Name = "Expiry hours")]
    [Required]
    [DecimalNumber]
    public string? ExpiryHours { get; set; }

    [FromForm(Name = "cost")]
    [Display(Name = "Cost")]
    [Required]
    [DecimalNumber]
    public string? Cost { get; set; }

    public void Load(AircraftRepair entity)
    {
        AircraftId = FormValues.Text(entity.AircraftId);
        RepairedOn = entity.RepairedOn;
        Code = entity.Code;
        Description = entity.Description;
        RepairShop = entity.RepairShop;
        PerformedHours = FormValues.Text(entity.PerformedHours);
        ExpiresOn = entity.ExpiresOn;
        ExpiryHours = FormValues.Text(entity.ExpiryHours);
        Cost = FormValues.Text(entity.Cost);
    }

    public void Apply(AircraftRepair entity)
    {
        entity.AircraftId = AircraftId.RequiredId();
        entity.RepairedOn = RepairedOn!.Value;
        entity.Code = Code!;
        entity.Description = Description!;
        entity.RepairShop = RepairShop!;
        entity.PerformedHours = PerformedHours.DecimalOr(entity.PerformedHours);
        entity.ExpiresOn = ExpiresOn!.Value;
        entity.ExpiryHours = ExpiryHours.DecimalOr(entity.ExpiryHours);
        entity.Cost = Cost.DecimalOr(entity.Cost);
    }
}
