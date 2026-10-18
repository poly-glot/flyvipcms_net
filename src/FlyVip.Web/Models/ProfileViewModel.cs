using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlyVip.Web.Models;

public class ProfileViewModel
{
    [FromForm(Name = "company")]
    [Display(Name = "company")]
    [MaxCharacters(150)]
    public string? Company { get; set; }

    [FromForm(Name = "street_address")]
    [Display(Name = "street_address")]
    [MaxCharacters(255)]
    public string? StreetAddress { get; set; }

    [FromForm(Name = "country_id")]
    [Display(Name = "country_id")]
    [PositiveWholeNumber]
    public string? CountryId { get; set; }

    [FromForm(Name = "zone_id")]
    [Display(Name = "zone_id")]
    [PositiveWholeNumber]
    public string? ZoneId { get; set; }

    [FromForm(Name = "city")]
    [Display(Name = "city")]
    [MaxCharacters(100)]
    public string? City { get; set; }

    [FromForm(Name = "zip_code")]
    [Display(Name = "zip_code")]
    [MaxCharacters(20)]
    public string? ZipCode { get; set; }

    [FromForm(Name = "cellphone")]
    [Display(Name = "cellphone")]
    [MaxCharacters(30)]
    public string? Cellphone { get; set; }

    [FromForm(Name = "office_phone")]
    [Display(Name = "office_phone")]
    [MaxCharacters(30)]
    public string? OfficePhone { get; set; }

    [FromForm(Name = "home_phone")]
    [Display(Name = "home_phone")]
    [MaxCharacters(30)]
    public string? HomePhone { get; set; }

    [FromForm(Name = "secondary_phone")]
    [Display(Name = "secondary_phone")]
    [MaxCharacters(30)]
    public string? SecondaryPhone { get; set; }

    [FromForm(Name = "id_number")]
    [Display(Name = "id_number")]
    [MaxCharacters(50)]
    public string? IdNumber { get; set; }

    [FromForm(Name = "id_due_on")]
    [Display(Name = "id_due_on")]
    public DateOnly? IdDueOn { get; set; }

    [FromForm(Name = "passport_number")]
    [Display(Name = "passport_number")]
    [MaxCharacters(50)]
    public string? PassportNumber { get; set; }

    [FromForm(Name = "passport_issued_on")]
    [Display(Name = "passport_issued_on")]
    public DateOnly? PassportIssuedOn { get; set; }

    [FromForm(Name = "passport_due_on")]
    [Display(Name = "passport_due_on")]
    public DateOnly? PassportDueOn { get; set; }

    [FromForm(Name = "favourite_route_id")]
    [Display(Name = "favourite_route_id")]
    [PositiveWholeNumber]
    public string? FavouriteRouteId { get; set; }

    [FromForm(Name = "heard_about_us")]
    [Display(Name = "heard_about_us")]
    [MaxCharacters(255)]
    public string? HeardAboutUs { get; set; }

    [BindNever]
    public ProfileChoices Choices { get; private set; } = new([], [], []);

    public void UseChoices(ProfileChoices choices) => Choices = choices;

    public virtual void Load(Profile profile)
    {
        Company = profile.Company;
        StreetAddress = profile.StreetAddress;
        CountryId = FormValues.Text(profile.CountryId);
        ZoneId = FormValues.Text(profile.ZoneId);
        City = profile.City;
        ZipCode = profile.ZipCode;
        Cellphone = profile.Cellphone;
        OfficePhone = profile.OfficePhone;
        HomePhone = profile.HomePhone;
        SecondaryPhone = profile.SecondaryPhone;
        IdNumber = profile.IdNumber;
        IdDueOn = profile.IdDueOn;
        PassportNumber = profile.PassportNumber;
        PassportIssuedOn = profile.PassportIssuedOn;
        PassportDueOn = profile.PassportDueOn;
        FavouriteRouteId = FormValues.Text(profile.FavouriteRouteId);
        HeardAboutUs = profile.HeardAboutUs;
    }

    public virtual Profile ToProfile() => new()
    {
        Company = Company,
        StreetAddress = StreetAddress,
        CountryId = CountryId.OptionalId(),
        ZoneId = ZoneId.OptionalId(),
        City = City,
        ZipCode = ZipCode,
        Cellphone = Cellphone,
        OfficePhone = OfficePhone,
        HomePhone = HomePhone,
        SecondaryPhone = SecondaryPhone,
        IdNumber = IdNumber,
        IdDueOn = IdDueOn,
        PassportNumber = PassportNumber,
        PassportIssuedOn = PassportIssuedOn,
        PassportDueOn = PassportDueOn,
        FavouriteRouteId = FavouriteRouteId.OptionalId(),
        HeardAboutUs = HeardAboutUs,
    };
}

public class NamedProfileViewModel : ProfileViewModel
{
    [FromForm(Name = "first_name")]
    [Display(Name = "first_name")]
    [Required]
    [MaxCharacters(100)]
    public string? FirstName { get; set; }

    [FromForm(Name = "last_name")]
    [Display(Name = "last_name")]
    [Required]
    [MaxCharacters(100)]
    public string? LastName { get; set; }

    public override void Load(Profile profile)
    {
        base.Load(profile);
        FirstName = profile.FirstName;
        LastName = profile.LastName;
    }

    public override Profile ToProfile()
    {
        var profile = base.ToProfile();
        profile.FirstName = FirstName ?? "";
        profile.LastName = LastName ?? "";

        return profile;
    }
}
