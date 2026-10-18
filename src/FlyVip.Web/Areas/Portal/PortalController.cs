using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyVip.Web.Areas.Portal;

[Area("Portal")]
[Authorize(Policy = "portal")]
public abstract class PortalController(PortalMemberQueries members) : Controller
{
    protected Task<PortalMember?> CurrentMember() => members.Current(User.Id());
}
