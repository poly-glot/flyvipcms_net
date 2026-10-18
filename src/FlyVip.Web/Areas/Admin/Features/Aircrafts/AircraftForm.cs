using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Aircrafts;

public sealed class AircraftForm : ICrudForm<Aircraft>
{
    [FromForm(Name = "name")]
    [Display(Name = "Name")]
    [Required]
    [MaxCharacters(100)]
    public string? Name { get; set; }

    [FromForm(Name = "aircraft_type_id")]
    [Display(Name = "Type")]
    [Required]
    [PositiveWholeNumber]
    public string? AircraftTypeId { get; set; }

    [FromForm(Name = "model")]
    [Display(Name = "Model")]
    [MaxCharacters(100)]
    public string? Model { get; set; }

    [FromForm(Name = "manufactured_on")]
    [Display(Name = "Manufactured")]
    public DateOnly? ManufacturedOn { get; set; }

    [FromForm(Name = "passenger_capacity")]
    [Display(Name = "Passenger capacity")]
    [Required]
    [PositiveWholeNumber]
    [AtMost(40)]
    public string? PassengerCapacity { get; set; }

    [FromForm(Name = "airframe_hours")]
    [Display(Name = "Airframe hours")]
    [DecimalNumber]
    public string? AirframeHours { get; set; }

    [FromForm(Name = "engine_hours_left")]
    [Display(Name = "Engine hours (left)")]
    [DecimalNumber]
    public string? EngineHoursLeft { get; set; }

    [FromForm(Name = "engine_hours_right")]
    [Display(Name = "Engine hours (right)")]
    [DecimalNumber]
    public string? EngineHoursRight { get; set; }

    [FromForm(Name = "variable_cost")]
    [Display(Name = "Variable cost")]
    [DecimalNumber]
    public string? VariableCost { get; set; }

    [FromForm(Name = "fixed_cost")]
    [Display(Name = "Fixed cost")]
    [DecimalNumber]
    public string? FixedCost { get; set; }

    [FromForm(Name = "total_cost")]
    [Display(Name = "Cost per seat")]
    [Required]
    [DecimalNumber]
    [AtLeast(0)]
    public string? TotalCost { get; set; }

    [FromForm(Name = "max_altitude")]
    [Display(Name = "Max altitude (ft)")]
    [WholeNumber]
    public string? MaxAltitude { get; set; }

    [FromForm(Name = "gallons_per_hour")]
    [Display(Name = "Gallons per hour")]
    [DecimalNumber]
    public string? GallonsPerHour { get; set; }

    [FromForm(Name = "certification")]
    [Display(Name = "Certification")]
    [MaxCharacters(100)]
    public string? Certification { get; set; }

    [FromForm(Name = "insurance_cost")]
    [Display(Name = "Insurance cost")]
    [DecimalNumber]
    public string? InsuranceCost { get; set; }

    public void Load(Aircraft entity)
    {
        Name = entity.Name;
        AircraftTypeId = FormValues.Text(entity.AircraftTypeId);
        Model = entity.Model;
        ManufacturedOn = entity.ManufacturedOn;
        PassengerCapacity = FormValues.Text(entity.PassengerCapacity);
        AirframeHours = FormValues.Text(entity.AirframeHours);
        EngineHoursLeft = FormValues.Text(entity.EngineHoursLeft);
        EngineHoursRight = FormValues.Text(entity.EngineHoursRight);
        VariableCost = FormValues.Text(entity.VariableCost);
        FixedCost = FormValues.Text(entity.FixedCost);
        TotalCost = FormValues.Text(entity.TotalCost);
        MaxAltitude = FormValues.Text(entity.MaxAltitude);
        GallonsPerHour = FormValues.Text(entity.GallonsPerHour);
        Certification = entity.Certification;
        InsuranceCost = FormValues.Text(entity.InsuranceCost);
    }

    public void Apply(Aircraft entity)
    {
        entity.Name = Name!;
        entity.AircraftTypeId = AircraftTypeId.RequiredId();
        entity.Model = Model;
        entity.ManufacturedOn = ManufacturedOn;
        entity.PassengerCapacity = PassengerCapacity.ByteOr(entity.PassengerCapacity);
        entity.AirframeHours = AirframeHours.DecimalOr(entity.AirframeHours);
        entity.EngineHoursLeft = EngineHoursLeft.DecimalOr(entity.EngineHoursLeft);
        entity.EngineHoursRight = EngineHoursRight.DecimalOr(entity.EngineHoursRight);
        entity.VariableCost = VariableCost.DecimalOr(entity.VariableCost);
        entity.FixedCost = FixedCost.DecimalOr(entity.FixedCost);
        entity.TotalCost = TotalCost.DecimalOr(entity.TotalCost);
        entity.MaxAltitude = MaxAltitude.IntOr(entity.MaxAltitude);
        entity.GallonsPerHour = GallonsPerHour.DecimalOr(entity.GallonsPerHour);
        entity.Certification = Certification;
        entity.InsuranceCost = InsuranceCost.DecimalOr(entity.InsuranceCost);
    }
}
