using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.PilotDocuments;

public sealed class PilotDocumentForm : ICrudForm<PilotDocument>
{
    [FromForm(Name = "pilot_id")]
    [Display(Name = "Pilot")]
    [Required]
    [PositiveWholeNumber]
    public string? PilotId { get; set; }

    [FromForm(Name = "doc_type")]
    [Display(Name = "Type")]
    [Required]
    [MaxCharacters(40)]
    public string? DocType { get; set; }

    [FromForm(Name = "doc_number")]
    [Display(Name = "Number")]
    [Required]
    [MaxCharacters(60)]
    public string? DocNumber { get; set; }

    [FromForm(Name = "country")]
    [Display(Name = "Country")]
    [MaxCharacters(80)]
    public string? Country { get; set; }

    [FromForm(Name = "issued_on")]
    [Display(Name = "Issued")]
    public DateOnly? IssuedOn { get; set; }

    [FromForm(Name = "expires_on")]
    [Display(Name = "Expires")]
    public DateOnly? ExpiresOn { get; set; }

    public void Load(PilotDocument entity)
    {
        PilotId = FormValues.Text(entity.PilotId);
        DocType = entity.DocType;
        DocNumber = entity.DocNumber;
        Country = entity.Country;
        IssuedOn = entity.IssuedOn;
        ExpiresOn = entity.ExpiresOn;
    }

    public void Apply(PilotDocument entity)
    {
        entity.PilotId = PilotId.RequiredId();
        entity.DocType = DocType!;
        entity.DocNumber = DocNumber!;
        entity.Country = Country;
        entity.IssuedOn = IssuedOn;
        entity.ExpiresOn = ExpiresOn;
    }
}
