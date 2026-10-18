using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Airports;

public sealed class AirportForm : ICrudForm<Airport>
{
    [FromForm(Name = "name")]
    [Display(Name = "Name")]
    [Required]
    [MaxCharacters(120)]
    public string? Name { get; set; }

    [FromForm(Name = "call_sign")]
    [Display(Name = "Call sign")]
    [MaxCharacters(20)]
    public string? CallSign { get; set; }

    [FromForm(Name = "airport_type")]
    [Display(Name = "Type")]
    [MaxCharacters(40)]
    public string? AirportType { get; set; }

    [FromForm(Name = "customs_contact")]
    [Display(Name = "Customs contact")]
    [MaxCharacters(120)]
    public string? CustomsContact { get; set; }

    [FromForm(Name = "fbo_contact")]
    [Display(Name = "FBO contact")]
    [MaxCharacters(120)]
    public string? FboContact { get; set; }

    [FromForm(Name = "hotel_contact")]
    [Display(Name = "Hotel contact")]
    [MaxCharacters(120)]
    public string? HotelContact { get; set; }

    [FromForm(Name = "travel_allowance")]
    [Display(Name = "Travel allowance")]
    [DecimalNumber]
    public string? TravelAllowance { get; set; }

    [FromForm(Name = "take_off_cost")]
    [Display(Name = "Take-off cost")]
    [DecimalNumber]
    public string? TakeOffCost { get; set; }

    [FromForm(Name = "landing_cost")]
    [Display(Name = "Landing cost")]
    [DecimalNumber]
    public string? LandingCost { get; set; }

    [FromForm(Name = "stay_cost")]
    [Display(Name = "Stay cost")]
    [DecimalNumber]
    public string? StayCost { get; set; }

    [FromForm(Name = "airport_cost")]
    [Display(Name = "Airport cost")]
    [DecimalNumber]
    public string? AirportCost { get; set; }

    [FromForm(Name = "handling_cost")]
    [Display(Name = "Handling cost")]
    [DecimalNumber]
    public string? HandlingCost { get; set; }

    [FromForm(Name = "comments")]
    [Display(Name = "Comments")]
    public string? Comments { get; set; }

    public void Load(Airport entity)
    {
        Name = entity.Name;
        CallSign = entity.CallSign;
        AirportType = entity.AirportType;
        CustomsContact = entity.CustomsContact;
        FboContact = entity.FboContact;
        HotelContact = entity.HotelContact;
        TravelAllowance = FormValues.Text(entity.TravelAllowance);
        TakeOffCost = FormValues.Text(entity.TakeOffCost);
        LandingCost = FormValues.Text(entity.LandingCost);
        StayCost = FormValues.Text(entity.StayCost);
        AirportCost = FormValues.Text(entity.AirportCost);
        HandlingCost = FormValues.Text(entity.HandlingCost);
        Comments = entity.Comments;
    }

    public void Apply(Airport entity)
    {
        entity.Name = Name!;
        entity.CallSign = CallSign;
        entity.AirportType = AirportType;
        entity.CustomsContact = CustomsContact;
        entity.FboContact = FboContact;
        entity.HotelContact = HotelContact;
        entity.TravelAllowance = TravelAllowance.DecimalOr(entity.TravelAllowance);
        entity.TakeOffCost = TakeOffCost.DecimalOr(entity.TakeOffCost);
        entity.LandingCost = LandingCost.DecimalOr(entity.LandingCost);
        entity.StayCost = StayCost.DecimalOr(entity.StayCost);
        entity.AirportCost = AirportCost.DecimalOr(entity.AirportCost);
        entity.HandlingCost = HandlingCost.DecimalOr(entity.HandlingCost);
        entity.Comments = Comments;

        var postedCosts = new[] { TravelAllowance, TakeOffCost, LandingCost, StayCost, AirportCost, HandlingCost }.OfType<string>().ToList();

        if (postedCosts.Count > 0)
        {
            entity.TotalCost = postedCosts.Sum(FormValues.ParseDecimal);
        }
    }
}
