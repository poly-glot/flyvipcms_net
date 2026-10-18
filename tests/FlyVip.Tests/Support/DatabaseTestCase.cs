using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlyVip.Tests.Support;

public abstract class DatabaseTestCase(DatabaseFixture database) : IAsyncLifetime
{
    public const string Password = "Str0ng-Passw0rd-123";

    private static int sequence;

    protected AppDbContext Db { get; } = DatabaseFixture.NewContext();

    protected IPasswordHasher<AppUser> Hasher => database.App.Services.GetRequiredService<IPasswordHasher<AppUser>>();

    protected PointsService Points => new(Db);

    protected PaymentService Payments => new(Db, Points);

    protected MemberService Members => new(Db, Hasher);

    protected ReservationService Reservations => new(Db, Points);

    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    protected static DateOnly Tomorrow => Today.AddDays(1);

    public async ValueTask InitializeAsync() => await database.ResetAsync();

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected HttpClient Client(bool handleCookies = true) => database.App.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = handleCookies });

    protected async Task<AppUser> Login(UserRole role)
    {
        var n = Interlocked.Increment(ref sequence);
        var name = EnumText.Of(role) + n;
        var user = new AppUser { Username = name, Email = $"{name}@example.test", Role = role, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };

        user.PasswordHash = Hasher.HashPassword(user, Password);
        Db.Users.Add(user);
        await Db.SaveChangesAsync();

        return user;
    }

    protected async Task<HttpClient> ActingAs(AppUser user)
    {
        var client = Client();
        using var response = await client.PostAsync("/login", Form(("email", user.Email), ("password", Password)));

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);

        return client;
    }

    protected static FormUrlEncodedContent Form(params (string Key, string Value)[] fields) =>
        new(fields.Select(field => KeyValuePair.Create(field.Key, field.Value)));

    protected static string Text(int value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    protected async Task<int> Admin() => (await Login(UserRole.Admin)).Id;

    protected async Task<HttpClient> ActingAsMember(int userId) => await ActingAs(await Db.Users.SingleAsync(u => u.Id == userId));

    protected async Task<int> Member(int planId = 1, decimal yearlyFee = 12000, bool activate = true, bool fund = true)
    {
        var n = Interlocked.Increment(ref sequence);
        var userId = await Members.Create(new Account($"member{n}", $"member{n}@example.test", Password), new Profile { FirstName = "Test", LastName = $"Member{n}" }, planId, 5000, yearlyFee);

        if (activate)
        {
            await Payments.Record(userId, PaymentType.Joining, Today, "test", await Admin());
        }

        if (activate && fund)
        {
            await Payments.Record(userId, PaymentType.Yearly, Today, "test", await Admin());
        }

        return userId;
    }

    protected async Task<(int Aircraft, int Route)> Fleet(decimal routeCost = 400, byte capacity = 6)
    {
        var now = DateTime.Now;
        var aircraft = new Aircraft { Name = "Test jet", AircraftType = new() { Name = "Jet", CreatedAt = now, UpdatedAt = now }, PassengerCapacity = capacity, TotalCost = 250, CreatedAt = now, UpdatedAt = now };
        var route = new AirRoute
        {
            FromAirport = new() { Name = "A", CreatedAt = now, UpdatedAt = now },
            ToAirport = new() { Name = "B", CreatedAt = now, UpdatedAt = now },
            Aircraft = aircraft,
            Cost = routeCost,
            CreatedAt = now,
            UpdatedAt = now,
        };

        Db.AirRoutes.Add(route);
        await Db.SaveChangesAsync();

        return (aircraft.Id, route.Id);
    }
}
