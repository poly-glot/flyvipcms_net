using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Admin.Features.Dashboard;

[Area("Admin")]
[Authorize(Policy = "admin")]
public sealed class DashboardController(AdminDashboardQueries queries) : Controller
{
    [HttpGet("/admin")]
    public async Task<IActionResult> Index() => View(await queries.Load());
}
