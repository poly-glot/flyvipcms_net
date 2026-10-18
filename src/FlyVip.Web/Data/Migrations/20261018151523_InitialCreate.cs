using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlyVip.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AircraftTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftTypes", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    CallSign = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    AirportType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    CustomsContact = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    FboContact = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    HotelContact = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    TravelAllowance = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TakeOffCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    LandingCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    StayCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    AirportCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    HandlingCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Comments = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactTypes",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Suffix = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactTypes", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Iso2 = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FriendlyName = table.Column<string>(type: "longtext", nullable: true),
                    Xml = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MembershipPlans",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Code = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false),
                    Name = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    OwnerPrefix = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    AllowsSubmembers = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlans", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pilots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    LastName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    City = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    CellPhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    WorkPhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    HomePhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    EmailPrimary = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    EmailSecondary = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    ContractType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    BasicSalary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    HourlySalary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ExtraBonus = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TrainingIncluded = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pilots", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Role", "`Role` IN ('admin', 'member', 'submember')");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Aircrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AircraftTypeId = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ManufacturedOn = table.Column<DateTime>(type: "date", nullable: true),
                    PassengerCapacity = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AirframeHours = table.Column<decimal>(type: "decimal(10,1)", precision: 10, scale: 1, nullable: false),
                    EngineHoursLeft = table.Column<decimal>(type: "decimal(10,1)", precision: 10, scale: 1, nullable: false),
                    EngineHoursRight = table.Column<decimal>(type: "decimal(10,1)", precision: 10, scale: 1, nullable: false),
                    VariableCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FixedCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxAltitude = table.Column<int>(type: "int", nullable: true),
                    GallonsPerHour = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Certification = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    InsuranceCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aircrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aircrafts_AircraftTypes_AircraftTypeId",
                        column: x => x.AircraftTypeId,
                        principalTable: "AircraftTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CountryZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryZones_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PilotDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PilotId = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    DocType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    DocNumber = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    IssuedOn = table.Column<DateTime>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotDocuments_Pilots_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Pilots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    ParentUserId = table.Column<int>(type: "int", nullable: true),
                    ContactTypeId = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    MemberCode = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    MembershipName = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    JoiningFee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    YearlyFee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    JoiningFeePaidAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    NextDueOn = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.CheckConstraint("CK_Members_Status", "`Status` IN ('inactive', 'active', 'banned')");
                    table.ForeignKey(
                        name: "FK_Members_ContactTypes_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalTable: "ContactTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Members_MembershipPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Members_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Members_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    PaidOn = table.Column<DateTime>(type: "date", nullable: false),
                    Remark = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_Type", "`Type` IN ('joining', 'yearly', 'quarterly', 'bonus', 'points')");
                    table.ForeignKey(
                        name: "FK_Payments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PointsTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsTransfers_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PointsTransfers_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PointsTransfers_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AircraftRepairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    RepairedOn = table.Column<DateTime>(type: "date", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    RepairShop = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PerformedHours = table.Column<decimal>(type: "decimal(8,1)", precision: 8, scale: 1, nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiryHours = table.Column<decimal>(type: "decimal(8,1)", precision: 8, scale: 1, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftRepairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AircraftRepairs_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AirRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FromAirportId = table.Column<int>(type: "int", nullable: false),
                    ToAirportId = table.Column<int>(type: "int", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: true),
                    Distance = table.Column<decimal>(type: "decimal(10,1)", precision: 10, scale: 1, nullable: true),
                    FlightTime = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Ete = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    ActualEte = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AirRoutes_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AirRoutes_Airports_FromAirportId",
                        column: x => x.FromAirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AirRoutes_Airports_ToAirportId",
                        column: x => x.ToAirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PilotCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PilotId = table.Column<int>(type: "int", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: true),
                    Certificate = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    CertType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true),
                    AgencySchool = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    IssuedOn = table.Column<DateTime>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "date", nullable: true),
                    DueOn = table.Column<DateTime>(type: "date", nullable: true),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Comments = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotCertifications_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PilotCertifications_Pilots_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Pilots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MembershipTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    StartsOn = table.Column<DateTime>(type: "date", nullable: false),
                    EndsOn = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipTerms_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PilotFlights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PilotId = table.Column<int>(type: "int", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: true),
                    FlightDate = table.Column<DateTime>(type: "date", nullable: false),
                    Identification = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true),
                    CallSign = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    DayHours = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    NightHours = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    IfrHours = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    Landings = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    Comments = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotFlights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotFlights_AirRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "AirRoutes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PilotFlights_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PilotFlights_Pilots_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Pilots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Company = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    StreetAddress = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Cellphone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    OfficePhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    HomePhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    SecondaryPhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    IdNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    IdDueOn = table.Column<DateTime>(type: "date", nullable: true),
                    PassportNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    PassportIssuedOn = table.Column<DateTime>(type: "date", nullable: true),
                    PassportDueOn = table.Column<DateTime>(type: "date", nullable: true),
                    FavouriteRouteId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Profiles_AirRoutes_FavouriteRouteId",
                        column: x => x.FavouriteRouteId,
                        principalTable: "AirRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Profiles_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Profiles_CountryZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "CountryZones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MembershipQuarters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TermId = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    Quarter = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    StartsOn = table.Column<DateTime>(type: "date", nullable: false),
                    EndsOn = table.Column<DateTime>(type: "date", nullable: false),
                    PointsLoaded = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipQuarters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipQuarters_MembershipTerms_TermId",
                        column: x => x.TermId,
                        principalTable: "MembershipTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipQuarters_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ParentReservationId = table.Column<int>(type: "int", nullable: true),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    PilotFlightId = table.Column<int>(type: "int", nullable: true),
                    FlightDate = table.Column<DateTime>(type: "date", nullable: false),
                    Kind = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    Passengers = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.CheckConstraint("CK_Reservations_Kind", "`Kind` IN ('complete', 'partial')");
                    table.CheckConstraint("CK_Reservations_Status", "`Status` IN ('pending', 'confirmed', 'cancelled')");
                    table.ForeignKey(
                        name: "FK_Reservations_AirRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "AirRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_PilotFlights_PilotFlightId",
                        column: x => x.PilotFlightId,
                        principalTable: "PilotFlights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reservations_Reservations_ParentReservationId",
                        column: x => x.ParentReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PointsLedger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Kind = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsLedger", x => x.Id);
                    table.CheckConstraint("CK_PointsLedger_Kind", "`Kind` IN ('payment', 'adjustment', 'transfer_in', 'transfer_out', 'booking', 'refund')");
                    table.ForeignKey(
                        name: "FK_PointsLedger_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PointsLedger_PointsTransfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "PointsTransfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PointsLedger_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PointsLedger_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReservationActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationActivities_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReservationActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReservationSeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    AircraftId = table.Column<int>(type: "int", nullable: false),
                    FlightDate = table.Column<DateTime>(type: "date", nullable: false),
                    SeatNumber = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationSeats_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ContactTypes",
                columns: new[] { "Id", "Name", "Suffix" },
                values: new object[,]
                {
                    { (byte)1, "Family member", "M" },
                    { (byte)2, "Partner", "A" },
                    { (byte)3, "Child", "X" },
                    { (byte)4, "Guest", "X" },
                    { (byte)5, "Other", "X" }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Active", "Iso2", "Name" },
                values: new object[,]
                {
                    { 1, true, "VE", "Venezuela" },
                    { 2, true, "US", "United States" },
                    { 3, true, "GB", "United Kingdom" },
                    { 4, true, "CO", "Colombia" },
                    { 5, true, "PA", "Panama" }
                });

            migrationBuilder.InsertData(
                table: "MembershipPlans",
                columns: new[] { "Id", "AllowsSubmembers", "Code", "Name", "OwnerPrefix" },
                values: new object[,]
                {
                    { (byte)1, false, "P", "FLYVip Personal +", "PO" },
                    { (byte)2, true, "F", "FLYVip Family +", "FO" },
                    { (byte)3, true, "C", "FLYVip Corporate +", "CO" }
                });

            migrationBuilder.InsertData(
                table: "CountryZones",
                columns: new[] { "Id", "Active", "CountryId", "Name" },
                values: new object[,]
                {
                    { 1, true, 1, "Distrito Capital" },
                    { 2, true, 1, "Miranda" },
                    { 3, true, 1, "Nueva Esparta" },
                    { 4, true, 1, "Zulia" },
                    { 5, true, 1, "Carabobo" },
                    { 6, true, 1, "Anzoategui" },
                    { 7, true, 2, "Florida" },
                    { 8, true, 2, "New York" },
                    { 9, true, 2, "Texas" },
                    { 10, true, 3, "England" },
                    { 11, true, 3, "Scotland" },
                    { 12, true, 4, "Bogota" },
                    { 13, true, 4, "Antioquia" },
                    { 14, true, 5, "Panama" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftRepairs_AircraftId",
                table: "AircraftRepairs",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_AircraftTypeId",
                table: "Aircrafts",
                column: "AircraftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AirRoutes_AircraftId",
                table: "AirRoutes",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_AirRoutes_FromAirportId",
                table: "AirRoutes",
                column: "FromAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_AirRoutes_ToAirportId",
                table: "AirRoutes",
                column: "ToAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Iso2",
                table: "Countries",
                column: "Iso2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryZones_CountryId",
                table: "CountryZones",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_ContactTypeId",
                table: "Members",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberCode",
                table: "Members",
                column: "MemberCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ParentUserId",
                table: "Members",
                column: "ParentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PlanId",
                table: "Members",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId",
                table: "Members",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_Code",
                table: "MembershipPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipQuarters_PaymentId",
                table: "MembershipQuarters",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipQuarters_TermId_Quarter",
                table: "MembershipQuarters",
                columns: new[] { "TermId", "Quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipTerms_MemberId",
                table: "MembershipTerms",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedById",
                table: "Payments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PilotCertifications_AircraftId",
                table: "PilotCertifications",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_PilotCertifications_PilotId",
                table: "PilotCertifications",
                column: "PilotId");

            migrationBuilder.CreateIndex(
                name: "IX_PilotDocuments_PilotId",
                table: "PilotDocuments",
                column: "PilotId");

            migrationBuilder.CreateIndex(
                name: "IX_PilotFlights_AircraftId_FlightDate",
                table: "PilotFlights",
                columns: new[] { "AircraftId", "FlightDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PilotFlights_PilotId",
                table: "PilotFlights",
                column: "PilotId");

            migrationBuilder.CreateIndex(
                name: "IX_PilotFlights_RouteId",
                table: "PilotFlights",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsLedger_PaymentId",
                table: "PointsLedger",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsLedger_ReservationId",
                table: "PointsLedger",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsLedger_TransferId",
                table: "PointsLedger",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsLedger_UserId_Id",
                table: "PointsLedger",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransfers_CreatedById",
                table: "PointsTransfers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransfers_ReceiverId",
                table: "PointsTransfers",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransfers_SenderId",
                table: "PointsTransfers",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_CountryId",
                table: "Profiles",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_FavouriteRouteId",
                table: "Profiles",
                column: "FavouriteRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ZoneId",
                table: "Profiles",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationActivities_ReservationId",
                table: "ReservationActivities",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationActivities_UserId",
                table: "ReservationActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AircraftId_FlightDate",
                table: "Reservations",
                columns: new[] { "AircraftId", "FlightDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ParentReservationId",
                table: "Reservations",
                column: "ParentReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PilotFlightId",
                table: "Reservations",
                column: "PilotFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RouteId",
                table: "Reservations",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeats_AircraftId_FlightDate_SeatNumber",
                table: "ReservationSeats",
                columns: new[] { "AircraftId", "FlightDate", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeats_ReservationId",
                table: "ReservationSeats",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftRepairs");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "MembershipQuarters");

            migrationBuilder.DropTable(
                name: "PilotCertifications");

            migrationBuilder.DropTable(
                name: "PilotDocuments");

            migrationBuilder.DropTable(
                name: "PointsLedger");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "ReservationActivities");

            migrationBuilder.DropTable(
                name: "ReservationSeats");

            migrationBuilder.DropTable(
                name: "MembershipTerms");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PointsTransfers");

            migrationBuilder.DropTable(
                name: "CountryZones");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "PilotFlights");

            migrationBuilder.DropTable(
                name: "ContactTypes");

            migrationBuilder.DropTable(
                name: "MembershipPlans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AirRoutes");

            migrationBuilder.DropTable(
                name: "Pilots");

            migrationBuilder.DropTable(
                name: "Aircrafts");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "AircraftTypes");
        }
    }
}
