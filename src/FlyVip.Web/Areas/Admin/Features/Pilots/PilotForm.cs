using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Pilots;

public sealed class PilotForm : ICrudForm<Pilot>
{
    [FromForm(Name = "first_name")]
    [Display(Name = "First name")]
    [Required]
    [MaxCharacters(80)]
    public string? FirstName { get; set; }

    [FromForm(Name = "last_name")]
    [Display(Name = "Last name")]
    [Required]
    [MaxCharacters(80)]
    public string? LastName { get; set; }

    [FromForm(Name = "birth_date")]
    [Display(Name = "Date of birth")]
    public DateOnly? BirthDate { get; set; }

    [FromForm(Name = "city")]
    [Display(Name = "City")]
    [MaxCharacters(80)]
    public string? City { get; set; }

    [FromForm(Name = "address")]
    [Display(Name = "Address")]
    [MaxCharacters(255)]
    public string? Address { get; set; }

    [FromForm(Name = "country")]
    [Display(Name = "Country")]
    [MaxCharacters(80)]
    public string? Country { get; set; }

    [FromForm(Name = "cell_phone")]
    [Display(Name = "Cell phone")]
    [MaxCharacters(30)]
    public string? CellPhone { get; set; }

    [FromForm(Name = "work_phone")]
    [Display(Name = "Work phone")]
    [MaxCharacters(30)]
    public string? WorkPhone { get; set; }

    [FromForm(Name = "home_phone")]
    [Display(Name = "Home phone")]
    [MaxCharacters(30)]
    public string? HomePhone { get; set; }

    [FromForm(Name = "email_primary")]
    [Display(Name = "Email")]
    [EmailFormat]
    [MaxCharacters(120)]
    public string? EmailPrimary { get; set; }

    [FromForm(Name = "email_secondary")]
    [Display(Name = "Secondary email")]
    [EmailFormat]
    [MaxCharacters(120)]
    public string? EmailSecondary { get; set; }

    [FromForm(Name = "contract_type")]
    [Display(Name = "Contract type")]
    [MaxCharacters(40)]
    public string? ContractType { get; set; }

    [FromForm(Name = "basic_salary")]
    [Display(Name = "Basic salary")]
    [DecimalNumber]
    public string? BasicSalary { get; set; }

    [FromForm(Name = "hourly_salary")]
    [Display(Name = "Hourly salary")]
    [DecimalNumber]
    public string? HourlySalary { get; set; }

    [FromForm(Name = "extra_bonus")]
    [Display(Name = "Extra bonus")]
    [DecimalNumber]
    public string? ExtraBonus { get; set; }

    [FromForm(Name = "training_included")]
    [Display(Name = "Training included")]
    [MaxCharacters(120)]
    public string? TrainingIncluded { get; set; }

    [FromForm(Name = "notes")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public void Load(Pilot entity)
    {
        FirstName = entity.FirstName;
        LastName = entity.LastName;
        BirthDate = entity.BirthDate;
        City = entity.City;
        Address = entity.Address;
        Country = entity.Country;
        CellPhone = entity.CellPhone;
        WorkPhone = entity.WorkPhone;
        HomePhone = entity.HomePhone;
        EmailPrimary = entity.EmailPrimary;
        EmailSecondary = entity.EmailSecondary;
        ContractType = entity.ContractType;
        BasicSalary = FormValues.Text(entity.BasicSalary);
        HourlySalary = FormValues.Text(entity.HourlySalary);
        ExtraBonus = FormValues.Text(entity.ExtraBonus);
        TrainingIncluded = entity.TrainingIncluded;
        Notes = entity.Notes;
    }

    public void Apply(Pilot entity)
    {
        entity.FirstName = FirstName!;
        entity.LastName = LastName!;
        entity.BirthDate = BirthDate;
        entity.City = City;
        entity.Address = Address;
        entity.Country = Country;
        entity.CellPhone = CellPhone;
        entity.WorkPhone = WorkPhone;
        entity.HomePhone = HomePhone;
        entity.EmailPrimary = EmailPrimary;
        entity.EmailSecondary = EmailSecondary;
        entity.ContractType = ContractType;
        entity.BasicSalary = BasicSalary.DecimalOr(entity.BasicSalary);
        entity.HourlySalary = HourlySalary.DecimalOr(entity.HourlySalary);
        entity.ExtraBonus = ExtraBonus.DecimalOr(entity.ExtraBonus);
        entity.TrainingIncluded = TrainingIncluded;
        entity.Notes = Notes;
    }
}
