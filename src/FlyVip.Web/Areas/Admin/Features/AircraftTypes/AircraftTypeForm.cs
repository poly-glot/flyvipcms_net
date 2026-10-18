using System.ComponentModel.DataAnnotations;
using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.AircraftTypes;

public sealed class AircraftTypeForm : ICrudForm<AircraftType>
{
    [FromForm(Name = "name")]
    [Display(Name = "Name")]
    [Required]
    [MaxCharacters(100)]
    public string? Name { get; set; }

    public void Load(AircraftType entity)
    {
        Name = entity.Name;
    }

    public void Apply(AircraftType entity)
    {
        entity.Name = Name!;
    }
}
