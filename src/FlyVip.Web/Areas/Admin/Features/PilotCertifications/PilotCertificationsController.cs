using FlyVip.Web.Areas.Admin.Crud;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using static FlyVip.Web.Infrastructure.CrudInputType;

namespace FlyVip.Web.Areas.Admin.Features.PilotCertifications;

[CrudResource("pilot-certifications")]
public sealed class PilotCertificationsController(AppDbContext db) : CrudController<PilotCertification, PilotCertificationForm>(db)
{
    protected override string Title => "Pilot certification";

    protected override PilotCertificationForm Blank() => new() { Active = "1" };

    protected override async Task<CrudOptions> Options() => new()
    {
        [nameof(PilotCertificationForm.PilotId)] = await Db.PilotMenu(),
        [nameof(PilotCertificationForm.AircraftId)] = await Db.AircraftMenu(),
    };

    protected override CrudLayout Layout() => new(
        [
            new(nameof(PilotCertificationForm.PilotId), Select),
            new(nameof(PilotCertificationForm.Certificate), Text),
            new(nameof(PilotCertificationForm.AircraftId), Select),
            new(nameof(PilotCertificationForm.IssuedOn), Date),
            new(nameof(PilotCertificationForm.ExpiresOn), Date),
            new(nameof(PilotCertificationForm.Active), Checkbox),
        ],
        [
            new(nameof(PilotCertificationForm.CertType), Text),
            new(nameof(PilotCertificationForm.AgencySchool), Text),
            new(nameof(PilotCertificationForm.DueOn), Date),
            new(nameof(PilotCertificationForm.Comments), Textarea),
        ]);
}
