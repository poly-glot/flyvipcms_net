namespace FlyVip.Web.Infrastructure;

public sealed record EmptyState(string Title, string Text, string Icon, string? ActionUrl = null, string? ActionLabel = null);
