using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.PilotCertifications;

public sealed class PilotCertificationForm : ICrudForm<PilotCertification>
{
    [FromForm(Name = "pilot_id")]
    [Display(Name = "Pilot")]
    [Required]
    [PositiveWholeNumber]
    public string? PilotId { get; set; }

    [FromForm(Name = "certificate")]
    [Display(Name = "Certificate")]
    [Required]
    [MaxCharacters(120)]
    public string? Certificate { get; set; }

    [FromForm(Name = "aircraft_id")]
    [Display(Name = "Aircraft")]
    [PositiveWholeNumber]
    public string? AircraftId { get; set; }

    [FromForm(Name = "cert_type")]
    [Display(Name = "Type")]
    [MaxCharacters(60)]
    public string? CertType { get; set; }

    [FromForm(Name = "agency_school")]
    [Display(Name = "Agency / school")]
    [MaxCharacters(120)]
    public string? AgencySchool { get; set; }

    [FromForm(Name = "issued_on")]
    [Display(Name = "Issued")]
    public DateOnly? IssuedOn { get; set; }

    [FromForm(Name = "expires_on")]
    [Display(Name = "Expires")]
    public DateOnly? ExpiresOn { get; set; }

    [FromForm(Name = "due_on")]
    [Display(Name = "Renewal due")]
    public DateOnly? DueOn { get; set; }

    [FromForm(Name = "active")]
    [Display(Name = "Active")]
    public string? Active { get; set; }

    [FromForm(Name = "comments")]
    [Display(Name = "Comments")]
    public string? Comments { get; set; }

    public void Load(PilotCertification entity)
    {
        PilotId = FormValues.Text(entity.PilotId);
        Certificate = entity.Certificate;
        AircraftId = FormValues.Text(entity.AircraftId);
        CertType = entity.CertType;
        AgencySchool = entity.AgencySchool;
        IssuedOn = entity.IssuedOn;
        ExpiresOn = entity.ExpiresOn;
        DueOn = entity.DueOn;
        Active = entity.Active ? "1" : "0";
        Comments = entity.Comments;
    }

    public void Apply(PilotCertification entity)
    {
        entity.PilotId = PilotId.RequiredId();
        entity.Certificate = Certificate!;
        entity.AircraftId = AircraftId.OptionalId();
        entity.CertType = CertType;
        entity.AgencySchool = AgencySchool;
        entity.IssuedOn = IssuedOn;
        entity.ExpiresOn = ExpiresOn;
        entity.DueOn = DueOn;
        entity.Active = Active == "1";
        entity.Comments = Comments;
    }
}
