using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.PilotFlights;

public sealed class PilotFlightForm : ICrudForm<PilotFlight>
{
    [FromForm(Name = "flight_date")]
    [Display(Name = "Date")]
    [Required]
    public DateOnly? FlightDate { get; set; }

    [FromForm(Name = "pilot_id")]
    [Display(Name = "Pilot")]
    [Required]
    [PositiveWholeNumber]
    public string? PilotId { get; set; }

    [FromForm(Name = "aircraft_id")]
    [Display(Name = "Aircraft")]
    [Required]
    [PositiveWholeNumber]
    public string? AircraftId { get; set; }

    [FromForm(Name = "route_id")]
    [Display(Name = "Route")]
    [PositiveWholeNumber]
    public string? RouteId { get; set; }

    [FromForm(Name = "identification")]
    [Display(Name = "Flight identification")]
    [MaxCharacters(60)]
    public string? Identification { get; set; }

    [FromForm(Name = "call_sign")]
    [Display(Name = "Call sign")]
    [MaxCharacters(30)]
    public string? CallSign { get; set; }

    [FromForm(Name = "day_hours")]
    [Display(Name = "Day hours")]
    [DecimalNumber]
    public string? DayHours { get; set; }

    [FromForm(Name = "night_hours")]
    [Display(Name = "Night hours")]
    [DecimalNumber]
    public string? NightHours { get; set; }

    [FromForm(Name = "ifr_hours")]
    [Display(Name = "IFR hours")]
    [DecimalNumber]
    public string? IfrHours { get; set; }

    [FromForm(Name = "landings")]
    [Display(Name = "Landings")]
    [WholeNumber]
    public string? Landings { get; set; }

    [FromForm(Name = "total_hours")]
    [Display(Name = "Total hours")]
    [DecimalNumber]
    public string? TotalHours { get; set; }

    [FromForm(Name = "comments")]
    [Display(Name = "Comments")]
    public string? Comments { get; set; }

    public void Load(PilotFlight entity)
    {
        FlightDate = entity.FlightDate;
        PilotId = FormValues.Text(entity.PilotId);
        AircraftId = FormValues.Text(entity.AircraftId);
        RouteId = FormValues.Text(entity.RouteId);
        Identification = entity.Identification;
        CallSign = entity.CallSign;
        DayHours = FormValues.Text(entity.DayHours);
        NightHours = FormValues.Text(entity.NightHours);
        IfrHours = FormValues.Text(entity.IfrHours);
        Landings = FormValues.Text(entity.Landings);
        TotalHours = FormValues.Text(entity.TotalHours);
        Comments = entity.Comments;
    }

    public void Apply(PilotFlight entity)
    {
        entity.FlightDate = FlightDate!.Value;
        entity.PilotId = PilotId.RequiredId();
        entity.AircraftId = AircraftId.RequiredId();
        entity.RouteId = RouteId.OptionalId();
        entity.Identification = Identification;
        entity.CallSign = CallSign;
        entity.DayHours = DayHours.DecimalOr(entity.DayHours);
        entity.NightHours = NightHours.DecimalOr(entity.NightHours);
        entity.IfrHours = IfrHours.DecimalOr(entity.IfrHours);
        entity.Landings = Landings.IntOr(entity.Landings);
        entity.TotalHours = TotalHours.DecimalOr(entity.TotalHours);
        entity.Comments = Comments;
    }
}
