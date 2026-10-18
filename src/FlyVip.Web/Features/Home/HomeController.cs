using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Features.Home;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return View();
        }

        return Redirect(User.IsInRole("admin") ? "/admin" : "/portal");
    }
}
