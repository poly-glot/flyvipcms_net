using FlyVip.Web.Models;

namespace FlyVip.Web.Areas.Portal.Features.Profile;

public sealed record PortalProfile(ProfileViewModel Form, bool ReadOnly);
