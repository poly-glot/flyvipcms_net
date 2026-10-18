using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Models;

public class BookingViewModel
{
    [FromForm(Name = "route_id")]
    [Display(Name = "route_id")]
    [Required]
    [PositiveWholeNumber]
    public string? RouteId { get; init; }

    [FromForm(Name = "aircraft_id")]
    [Display(Name = "aircraft_id")]
    [Required]
    [PositiveWholeNumber]
    public string? AircraftId { get; init; }

    [FromForm(Name = "flight_date")]
    [Display(Name = "flight_date")]
    [Required]
    public DateOnly? FlightDate { get; init; }

    [FromForm(Name = "kind")]
    [Display(Name = "kind")]
    [Required]
    [RegularExpression("complete|partial", ErrorMessage = "The {0} field must be one of: complete,partial.")]
    public string? Kind { get; init; }

    [FromForm(Name = "seats[]")]
    [Display(Name = "seats.*")]
    [PositiveWholeNumbers]
    public List<string?> Seats { get; init; } = [];

    public string KindChoice => Kind ?? "partial";

    [BindNever]
    public BookingChoices Choices { get; set; } = new([], [], "{}", "");

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> Routes => Choices.Routes;

    [BindNever]
    public IReadOnlyList<KeyValuePair<string, string>> Aircrafts => Choices.Aircrafts;

    [BindNever]
    public string Capacities => Choices.Capacities;

    [BindNever]
    public string EarliestFlightDate => Choices.EarliestFlightDate;

    public Task<int> Book(ReservationService reservations, int userId) => reservations.Book(
        userId,
        PositiveWholeNumbersAttribute.Natural(RouteId),
        PositiveWholeNumbersAttribute.Natural(AircraftId),
        FlightDate!.Value,
        Enum.Parse<ReservationKind>(Kind!, true),
        Seats.Select(PositiveWholeNumbersAttribute.Natural));
}
