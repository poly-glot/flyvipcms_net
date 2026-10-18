using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.AirRoutes;

public sealed class AirRouteForm : ICrudForm<AirRoute>
{
    [FromForm(Name = "from_airport_id")]
    [Display(Name = "From")]
    [Required]
    [PositiveWholeNumber]
    public string? FromAirportId { get; set; }

    [FromForm(Name = "to_airport_id")]
    [Display(Name = "To")]
    [Required]
    [PositiveWholeNumber]
    [Differs(nameof(FromAirportId))]
    public string? ToAirportId { get; set; }

    [FromForm(Name = "aircraft_id")]
    [Display(Name = "Aircraft")]
    [PositiveWholeNumber]
    public string? AircraftId { get; set; }

    [FromForm(Name = "distance")]
    [Display(Name = "Distance (nm)")]
    [DecimalNumber]
    public string? Distance { get; set; }

    [FromForm(Name = "flight_time")]
    [Display(Name = "Flight time")]
    [MaxCharacters(20)]
    public string? FlightTime { get; set; }

    [FromForm(Name = "ete")]
    [Display(Name = "ETE")]
    [MaxCharacters(20)]
    public string? Ete { get; set; }

    [FromForm(Name = "actual_ete")]
    [Display(Name = "Actual ETE")]
    [MaxCharacters(20)]
    public string? ActualEte { get; set; }

    [FromForm(Name = "cost")]
    [Display(Name = "Cost per seat")]
    [Required]
    [DecimalNumber]
    [AtLeast(0)]
    public string? Cost { get; set; }

    public void Load(AirRoute entity)
    {
        FromAirportId = FormValues.Text(entity.FromAirportId);
        ToAirportId = FormValues.Text(entity.ToAirportId);
        AircraftId = FormValues.Text(entity.AircraftId);
        Distance = FormValues.Text(entity.Distance);
        FlightTime = entity.FlightTime;
        Ete = entity.Ete;
        ActualEte = entity.ActualEte;
        Cost = FormValues.Text(entity.Cost);
    }

    public void Apply(AirRoute entity)
    {
        entity.FromAirportId = FromAirportId.RequiredId();
        entity.ToAirportId = ToAirportId.RequiredId();
        entity.AircraftId = AircraftId.OptionalId();
        entity.Distance = Distance.DecimalOr(entity.Distance);
        entity.FlightTime = FlightTime;
        entity.Ete = Ete;
        entity.ActualEte = ActualEte;
        entity.Cost = Cost.DecimalOr(entity.Cost);
    }
}
