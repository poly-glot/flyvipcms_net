using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Portal.Features.Dashboard;

[Route("portal")]
public sealed class DashboardController(PortalMemberQueries members, PortalDashboardQueries queries) : PortalController(members)
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => await CurrentMember() is { } member ? View(await queries.Load(member)) : NotFound();
}
