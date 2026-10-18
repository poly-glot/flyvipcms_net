using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data;

public sealed class DemoSeeder(
    AppDbContext db,
    IHostEnvironment environment,
    IPasswordHasher<AppUser> hasher,
    MemberService members,
    PaymentService payments,
    ReservationService reservations)
{
    public static readonly IReadOnlyDictionary<string, (string Email, string Password)> Accounts = new Dictionary<string, (string, string)>
    {
        ["admin"] = ("admin@flyvip.test", "FlyVIP-Admin-2026!"),
        ["personal"] = ("ana.personal@flyvip.test", "FlyVIP-Personal-2026!"),
        ["family"] = ("carlos.family@flyvip.test", "FlyVIP-Family-2026!"),
        ["submember"] = ("lucia.family@flyvip.test", "FlyVIP-Submember-2026!"),
        ["corporate"] = ("acme.corporate@flyvip.test", "FlyVIP-Corporate-2026!"),
        ["prospect"] = ("new.prospect@flyvip.test", "FlyVIP-Prospect-2026!"),
        ["banned"] = ("banned.member@flyvip.test", "FlyVIP-Banned-2026!"),
    };

    public async Task Run()
    {
        if (environment.IsProduction())
        {
            throw new InvalidOperationException("DemoSeeder must never run in production.");
        }

        if (await db.Users.AnyAsync(u => u.Email == Accounts["admin"].Email))
        {
            return;
        }

        var adminId = await CreateAdmin();
        var fleet = await SeedFleet();
        var (personal, family) = await SeedMembers(adminId);

        await SeedBookings(fleet, personal, family);
    }

    private static DateOnly DaysFromToday(int days) => DateOnly.FromDateTime(DateTime.Now.AddDays(days));

    private static Account Account(string username, string key) => new(username, Accounts[key].Email, Accounts[key].Password);

    private static Profile Profile(string first, string last, string city, string? company = null) =>
        new() { FirstName = first, LastName = last, City = city, Cellphone = "+58 412 555 0100", CountryId = 1, Company = company };

    private async Task<int> CreateAdmin()
    {
        var now = DateTime.Now;
        var admin = new AppUser { Username = "admin", Email = Accounts["admin"].Email, Role = UserRole.Admin, CreatedAt = now, UpdatedAt = now };

        admin.PasswordHash = hasher.HashPassword(admin, Accounts["admin"].Password);
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        return admin.Id;
    }

    private async Task<(AirRoute Route, Aircraft Aircraft)> SeedFleet()
    {
        var now = DateTime.Now;
        AircraftType[] types =
        [
            new() { Name = "Light jet", CreatedAt = now, UpdatedAt = now },
            new() { Name = "Turboprop", CreatedAt = now, UpdatedAt = now },
        ];
        Aircraft[] aircrafts =
        [
            new() { Name = "Citation CJ3", AircraftType = types[0], Model = "CJ3", PassengerCapacity = 6, VariableCost = 150, FixedCost = 100, TotalCost = 250, MaxAltitude = 45000, CreatedAt = now, UpdatedAt = now },
            new() { Name = "King Air 350", AircraftType = types[1], Model = "350", PassengerCapacity = 8, VariableCost = 110, FixedCost = 70, TotalCost = 180, MaxAltitude = 35000, CreatedAt = now, UpdatedAt = now },
        ];
        Airport[] airports =
        [
            new() { Name = "Maiquetia (CCS)", CallSign = "SVMI", AirportType = "International", CreatedAt = now, UpdatedAt = now },
            new() { Name = "Porlamar (PMV)", CallSign = "SVMG", AirportType = "International", CreatedAt = now, UpdatedAt = now },
            new() { Name = "Miami (MIA)", CallSign = "KMIA", AirportType = "International", CreatedAt = now, UpdatedAt = now },
            new() { Name = "Maracaibo (MAR)", CallSign = "SVMC", AirportType = "International", CreatedAt = now, UpdatedAt = now },
        ];
        AirRoute[] routes =
        [
            new() { FromAirport = airports[0], ToAirport = airports[1], Aircraft = aircrafts[0], Distance = 280, FlightTime = "1:00", Cost = 400, CreatedAt = now, UpdatedAt = now },
            new() { FromAirport = airports[0], ToAirport = airports[2], Aircraft = aircrafts[0], Distance = 1230, FlightTime = "3:15", Cost = 900, CreatedAt = now, UpdatedAt = now },
            new() { FromAirport = airports[3], ToAirport = airports[0], Aircraft = aircrafts[1], Distance = 360, FlightTime = "1:20", Cost = 350, CreatedAt = now, UpdatedAt = now },
        ];
        Pilot[] pilots =
        [
            new() { FirstName = "Marta", LastName = "Rojas", City = "Caracas", CellPhone = "+58 412 555 0101", EmailPrimary = "marta.rojas@flyvip.test", ContractType = "Full time", HourlySalary = 90, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Diego", LastName = "Salas", City = "Maracaibo", CellPhone = "+58 414 555 0102", EmailPrimary = "diego.salas@flyvip.test", ContractType = "Contractor", HourlySalary = 75, CreatedAt = now, UpdatedAt = now },
        ];

        db.AddRange(types);
        db.AddRange(aircrafts);
        db.AddRange(airports);
        db.AddRange(routes);
        db.AddRange(pilots);
        db.Add(new PilotDocument { Pilot = pilots[0], DocType = "Passport", DocNumber = "P1234567", Country = "Venezuela", ExpiresOn = DateOnly.FromDateTime(now.AddYears(3)), CreatedAt = now, UpdatedAt = now });
        db.Add(new PilotCertification { Pilot = pilots[0], Aircraft = aircrafts[0], Certificate = "Citation CJ3 type rating", CertType = "Type rating", ExpiresOn = DateOnly.FromDateTime(now.AddYears(1)), Active = true, CreatedAt = now, UpdatedAt = now });
        db.AddRange(
            new PilotFlight { Pilot = pilots[0], Aircraft = aircrafts[0], Route = routes[0], FlightDate = DaysFromToday(7), Identification = "FV101", TotalHours = 1, CreatedAt = now, UpdatedAt = now },
            new PilotFlight { Pilot = pilots[1], Aircraft = aircrafts[1], Route = routes[2], FlightDate = DaysFromToday(9), Identification = "FV201", TotalHours = 1.3m, CreatedAt = now, UpdatedAt = now });
        db.Add(new AircraftRepair { Aircraft = aircrafts[0], RepairedOn = DaysFromToday(-30), Code = "A-CHECK", Description = "Scheduled A inspection", RepairShop = "Maiquetia MRO", PerformedHours = 12, ExpiresOn = DaysFromToday(150), ExpiryHours = 200, Cost = 4200, CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        return (routes[0], aircrafts[0]);
    }

    private async Task<(int Personal, int Family)> SeedMembers(int adminId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var personal = await members.Create(Account("ana.personal", "personal"), Profile("Ana", "Personal", "Caracas"), 1, 5000, 12000);
        await payments.Record(personal, PaymentType.Joining, today, "Wire transfer", adminId);
        await payments.Record(personal, PaymentType.Yearly, today, "Wire transfer", adminId);

        var family = await members.Create(Account("carlos.family", "family"), Profile("Carlos", "Family", "Maracaibo"), 2, 7500, 24000);
        await payments.Record(family, PaymentType.Joining, today, "Card", adminId);
        await payments.Record(family, PaymentType.Yearly, today, "Card", adminId);
        await members.AddSubMember(family, Account("lucia.family", "submember"), Profile("Lucia", "Family", "Maracaibo"), 2);

        var corporate = await members.Create(Account("acme.corporate", "corporate"), Profile("Acme", "Corporate", "Valencia", "Acme Holdings"), 3, 10000, 48000);
        await payments.Record(corporate, PaymentType.Joining, today, "Wire transfer", adminId);
        await payments.Record(corporate, PaymentType.Quarterly, today, "Wire transfer", adminId, null, 2);

        await members.Create(Account("new.prospect", "prospect"), Profile("Nora", "Prospect", "Caracas"), 1, 5000, 12000);

        var banned = await members.Create(Account("banned.member", "banned"), Profile("Bruno", "Banned", "Caracas"), 1, 5000, 12000);
        await payments.Record(banned, PaymentType.Joining, today, "Cash", adminId);
        await members.SetStatus(banned, MemberStatus.Banned);

        return (personal, family);
    }

    private async Task SeedBookings((AirRoute Route, Aircraft Aircraft) fleet, int personal, int family)
    {
        var date = DaysFromToday(7);

        await reservations.Book(family, fleet.Route.Id, fleet.Aircraft.Id, date, ReservationKind.Partial, [1, 2]);
        await reservations.Book(personal, fleet.Route.Id, fleet.Aircraft.Id, date, ReservationKind.Partial, [3, 4]);
    }
}
