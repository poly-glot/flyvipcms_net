using System.Globalization;
using System.Security.Claims;

namespace FlyVip.Web.Infrastructure;

public static class CurrentUser
{
    public static int Id(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("No signed-in user."), CultureInfo.InvariantCulture);
}
