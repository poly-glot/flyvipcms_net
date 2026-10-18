namespace FlyVip.Web.Infrastructure;

public static class QueryRegistration
{
    private const string Suffix = "Queries";

    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        var queries = typeof(QueryRegistration).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith(Suffix, StringComparison.Ordinal));

        foreach (var query in queries)
        {
            services.AddScoped(query);
        }

        return services;
    }
}
