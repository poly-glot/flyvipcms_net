using System.Linq.Expressions;
using FlyVip.Web.Data.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyVip.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<ContactType> ContactTypes => Set<ContactType>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<CountryZone> CountryZones => Set<CountryZone>();
    public DbSet<AircraftType> AircraftTypes => Set<AircraftType>();
    public DbSet<Aircraft> Aircrafts => Set<Aircraft>();
    public DbSet<AircraftRepair> AircraftRepairs => Set<AircraftRepair>();
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<AirRoute> AirRoutes => Set<AirRoute>();
    public DbSet<Pilot> Pilots => Set<Pilot>();
    public DbSet<PilotDocument> PilotDocuments => Set<PilotDocument>();
    public DbSet<PilotCertification> PilotCertifications => Set<PilotCertification>();
    public DbSet<PilotFlight> PilotFlights => Set<PilotFlight>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MembershipTerm> MembershipTerms => Set<MembershipTerm>();
    public DbSet<MembershipQuarter> MembershipQuarters => Set<MembershipQuarter>();
    public DbSet<PointsTransfer> PointsTransfers => Set<PointsTransfer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationSeat> ReservationSeats => Set<ReservationSeat>();
    public DbSet<ReservationActivity> ReservationActivities => Set<ReservationActivity>();
    public DbSet<PointsLedgerEntry> PointsLedger => Set<PointsLedgerEntry>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
        configurationBuilder.Properties<DateOnly?>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var amount in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()).Where(IsUnsizedDecimal))
        {
            amount.SetPrecision(12);
            amount.SetScale(2);
        }

        modelBuilder.Entity<AircraftType>().HasQueryFilter(t => t.DeletedAt == null);
        modelBuilder.Entity<Aircraft>().HasQueryFilter(a => a.DeletedAt == null);
        modelBuilder.Entity<AircraftRepair>().HasQueryFilter(r => r.DeletedAt == null);
        modelBuilder.Entity<Airport>().HasQueryFilter(a => a.DeletedAt == null);
        modelBuilder.Entity<AirRoute>().HasQueryFilter(r => r.DeletedAt == null);
        modelBuilder.Entity<Pilot>().HasQueryFilter(p => p.DeletedAt == null);
        modelBuilder.Entity<PilotDocument>().HasQueryFilter(d => d.DeletedAt == null);
        modelBuilder.Entity<PilotCertification>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<PilotFlight>().HasQueryFilter(f => f.DeletedAt == null);
        modelBuilder.Entity<Member>().HasQueryFilter(m => m.DeletedAt == null);
        modelBuilder.Entity<Payment>().HasQueryFilter(p => p.DeletedAt == null);

        LowercaseEnum(modelBuilder.Entity<AppUser>(), u => u.Role);
        LowercaseEnum(modelBuilder.Entity<Member>(), m => m.Status);
        LowercaseEnum(modelBuilder.Entity<Payment>(), p => p.Type);
        LowercaseEnum(modelBuilder.Entity<Reservation>(), r => r.Kind);
        LowercaseEnum(modelBuilder.Entity<Reservation>(), r => r.Status);
        LowercaseEnum(modelBuilder.Entity<PointsLedgerEntry>(), e => e.Kind);

        modelBuilder.Entity<MembershipPlan>().HasData(
            new MembershipPlan { Id = 1, Code = "P", Name = "FLYVip Personal +", OwnerPrefix = "PO", AllowsSubmembers = false },
            new MembershipPlan { Id = 2, Code = "F", Name = "FLYVip Family +", OwnerPrefix = "FO", AllowsSubmembers = true },
            new MembershipPlan { Id = 3, Code = "C", Name = "FLYVip Corporate +", OwnerPrefix = "CO", AllowsSubmembers = true });

        modelBuilder.Entity<ContactType>().HasData(
            new ContactType { Id = 1, Name = "Family member", Suffix = "M" },
            new ContactType { Id = 2, Name = "Partner", Suffix = "A" },
            new ContactType { Id = 3, Name = "Child", Suffix = "X" },
            new ContactType { Id = 4, Name = "Guest", Suffix = "X" },
            new ContactType { Id = 5, Name = "Other", Suffix = "X" });

        modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "Venezuela", Iso2 = "VE" },
            new Country { Id = 2, Name = "United States", Iso2 = "US" },
            new Country { Id = 3, Name = "United Kingdom", Iso2 = "GB" },
            new Country { Id = 4, Name = "Colombia", Iso2 = "CO" },
            new Country { Id = 5, Name = "Panama", Iso2 = "PA" });

        modelBuilder.Entity<CountryZone>().HasData(
            new CountryZone { Id = 1, CountryId = 1, Name = "Distrito Capital" },
            new CountryZone { Id = 2, CountryId = 1, Name = "Miranda" },
            new CountryZone { Id = 3, CountryId = 1, Name = "Nueva Esparta" },
            new CountryZone { Id = 4, CountryId = 1, Name = "Zulia" },
            new CountryZone { Id = 5, CountryId = 1, Name = "Carabobo" },
            new CountryZone { Id = 6, CountryId = 1, Name = "Anzoategui" },
            new CountryZone { Id = 7, CountryId = 2, Name = "Florida" },
            new CountryZone { Id = 8, CountryId = 2, Name = "New York" },
            new CountryZone { Id = 9, CountryId = 2, Name = "Texas" },
            new CountryZone { Id = 10, CountryId = 3, Name = "England" },
            new CountryZone { Id = 11, CountryId = 3, Name = "Scotland" },
            new CountryZone { Id = 12, CountryId = 4, Name = "Bogota" },
            new CountryZone { Id = 13, CountryId = 4, Name = "Antioquia" },
            new CountryZone { Id = 14, CountryId = 5, Name = "Panama" });
    }

    private static bool IsUnsizedDecimal(IMutableProperty property) =>
        (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == typeof(decimal) && property.GetPrecision() is null;

    private static void LowercaseEnum<TEntity, TEnum>(EntityTypeBuilder<TEntity> entity, Expression<Func<TEntity, TEnum>> property)
        where TEntity : class
        where TEnum : struct, Enum
    {
        var column = entity.Property(property);
        var name = column.Metadata.Name;
        var labels = Enum.GetValues<TEnum>().Select(EnumText.Of).ToArray();

        entity.ToTable(table => table.HasCheckConstraint($"CK_{entity.Metadata.GetTableName()}_{name}", $"`{name}` IN ('{string.Join("', '", labels)}')"));
        column
            .HasConversion(value => EnumText.Of(value), text => Enum.Parse<TEnum>(text.Replace("_", "", StringComparison.Ordinal), true))
            .HasMaxLength(labels.Max(label => label.Length));
    }
}
