using FlyVip.Tests.Support;
using FlyVip.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

[assembly: AssemblyFixture(typeof(DatabaseFixture))]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace FlyVip.Tests.Support;

public sealed class DatabaseFixture : IAsyncLifetime
{
    public static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Test")
        ?? "Server=127.0.0.1;Port=13306;Database=flyvip_test;User Id=root;Password=FlyVip!Dev2026";

    private string resetSql = "";

    public WebApplicationFactory<Program> App { get; } = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
        .UseEnvironment("Testing")
        .ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection([
            KeyValuePair.Create<string, string?>("ConnectionStrings:Default", ConnectionString),
            KeyValuePair.Create<string, string?>("DB_HOST", ""),
        ])));

    public static AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>().UseMySQL(ConnectionString).Options);

    public async ValueTask InitializeAsync()
    {
        await using var db = NewContext();
        await db.Database.MigrateAsync();

        var tables = db.GetService<IDesignTimeModel>().Model.GetEntityTypes()
            .Where(entity => !entity.GetSeedData().Any())
            .Select(entity => entity.GetTableName())
            .OfType<string>()
            .ToList();

        resetSql = string.Join('\n', [
            "SET FOREIGN_KEY_CHECKS = 0;",
            .. tables.Select(table => $"TRUNCATE TABLE `{table}`;"),
            "SET FOREIGN_KEY_CHECKS = 1;",
        ]);
    }

    public async Task ResetAsync()
    {
        await using var db = NewContext();
        await db.Database.ExecuteSqlRawAsync(resetSql);
    }

    public async ValueTask DisposeAsync() => await App.DisposeAsync();
}
