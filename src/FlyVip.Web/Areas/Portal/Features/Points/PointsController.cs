using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Portal.Features.Points;

[Route("portal/points")]
public sealed class PointsController(PortalMemberQueries members, PortalPointsQueries queries) : PortalController(members)
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => await CurrentMember() is { } member ? View(await queries.Load(member.AccountUserId)) : NotFound();
}
