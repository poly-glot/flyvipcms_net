using System.Globalization;
using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Feature;

public sealed class AdminFlowsTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task AdminCreatesAndDeletesFleetRecords()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var created = await admin.PostAsync("/admin/aircraft-types", Form(("name", "Heavy jet")), TestContext.Current.CancellationToken);
        var type = await Db.AircraftTypes.SingleAsync(t => t.Name == "Heavy jet", TestContext.Current.CancellationToken);

        using var deleted = await admin.PostAsync($"/admin/aircraft-types/{type.Id}/delete", Form(), TestContext.Current.CancellationToken);
        var removed = await Db.AircraftTypes.IgnoreQueryFilters().AsNoTracking().SingleAsync(t => t.Id == type.Id, TestContext.Current.CancellationToken);

        Assert.Equal("/admin/aircraft-types", created.Headers.Location?.OriginalString);
        Assert.Equal("/admin/aircraft-types", deleted.Headers.Location?.OriginalString);
        Assert.NotNull(removed.DeletedAt);
    }

    [Fact]
    public async Task AirportTotalCostIsComputedServerSide()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        await admin.PostAsync("/admin/airports", Form(
            ("name", "Costly"),
            ("travel_allowance", "10"),
            ("take_off_cost", "20"),
            ("landing_cost", "30"),
            ("stay_cost", "40"),
            ("airport_cost", "50"),
            ("handling_cost", "60"),
            ("total_cost", "1")), TestContext.Current.CancellationToken);

        Assert.Equal(210.00m, (await Db.Airports.SingleAsync(a => a.Name == "Costly", TestContext.Current.CancellationToken)).TotalCost);
    }

    [Fact]
    public async Task RouteRejectsSameAirportOnBothEnds()
    {
        await Fleet();
        var airportId = (await Db.Airports.OrderByDescending(a => a.Id).FirstAsync(TestContext.Current.CancellationToken)).Id.ToString(CultureInfo.InvariantCulture);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var response = await admin.PostAsync("/admin/routes", Form(
            ("from_airport_id", airportId),
            ("to_airport_id", airportId),
            ("cost", "10")), TestContext.Current.CancellationToken);

        Assert.Equal(1, await Db.AirRoutes.CountAsync(TestContext.Current.CancellationToken));
        Assert.Contains("The To field must differ from the From field.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminCreatesMemberThenRecordsJoiningFee()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var created = await admin.PostAsync("/admin/members", Form(
            ("username", "newbie"),
            ("email", "newbie@example.test"),
            ("password", Password),
            ("first_name", "New"),
            ("last_name", "Bie"),
            ("plan_id", "1"),
            ("joining_fee", "5000"),
            ("yearly_fee", "12000")), TestContext.Current.CancellationToken);
        var newbie = await Db.Users.SingleAsync(u => u.Username == "newbie", TestContext.Current.CancellationToken);
        var before = await Db.Members.AsNoTracking().SingleAsync(m => m.UserId == newbie.Id, TestContext.Current.CancellationToken);

        using var paid = await admin.PostAsync("/admin/payments", Form(
            ("user_id", Text(newbie.Id)),
            ("type", "joining"),
            ("paid_on", Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("remark", "cash")), TestContext.Current.CancellationToken);
        var after = await Db.Members.AsNoTracking().SingleAsync(m => m.UserId == newbie.Id, TestContext.Current.CancellationToken);

        Assert.Equal($"/admin/members/{newbie.Id}", created.Headers.Location?.OriginalString);
        Assert.Equal(MemberStatus.Inactive, before.Status);
        Assert.Equal(HttpStatusCode.Redirect, paid.StatusCode);
        Assert.Equal(MemberStatus.Active, after.Status);
    }

    [Fact]
    public async Task MemberCreationRejectsWeakAndDuplicateInput()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));
        var existing = await Login(UserRole.Member);

        using var response = await admin.PostAsync("/admin/members", Form(
            ("username", "dupe"),
            ("email", existing.Email),
            ("password", "short"),
            ("first_name", "D"),
            ("last_name", "Upe"),
            ("plan_id", "1"),
            ("joining_fee", "1"),
            ("yearly_fee", "1")), TestContext.Current.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, await Db.Users.CountAsync(u => u.Username == "dupe", TestContext.Current.CancellationToken));
        Assert.Contains("The email field must contain a unique value.", html, StringComparison.Ordinal);
        Assert.Contains("The password field must be at least 8 characters in length.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminAddsSubMemberChangesStatusAndRemovesTheFamily()
    {
        var parent = await Member(planId: 2);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var added = await admin.PostAsync($"/admin/members/{parent}/sub-members", Form(
            ("first_name", "Kid"),
            ("last_name", "One"),
            ("username", "kid1"),
            ("email", "kid1@example.test"),
            ("password", Password),
            ("contact_type_id", "3")), TestContext.Current.CancellationToken);
        var child = await Db.Members.AsNoTracking().SingleAsync(m => m.ParentUserId == parent, TestContext.Current.CancellationToken);
        var page = await admin.GetStringAsync($"/admin/members/{parent}", TestContext.Current.CancellationToken);

        using var banned = await admin.PostAsync($"/admin/members/{parent}/status", Form(("status", "banned")), TestContext.Current.CancellationToken);
        var status = (await Db.Members.AsNoTracking().SingleAsync(m => m.UserId == parent, TestContext.Current.CancellationToken)).Status;

        using var removed = await admin.PostAsync($"/admin/members/{parent}/delete", Form(), TestContext.Current.CancellationToken);
        using var childPage = await admin.GetAsync($"/admin/members/{child.UserId}", TestContext.Current.CancellationToken);

        Assert.Equal($"/admin/members/{parent}", added.Headers.Location?.OriginalString);
        Assert.Equal($"FX-{child.UserId:D3}", child.MemberCode);
        Assert.Contains("<span class=\"pill pill--accent pill--plain\">Child</span>", page, StringComparison.Ordinal);
        Assert.Equal(MemberStatus.Banned, status);
        Assert.Equal("/admin/members", removed.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.NotFound, childPage.StatusCode);
    }

    [Fact]
    public async Task PaymentFormOffersOnlyTheJoiningFeeUntilItIsPaid()
    {
        var userId = await Member(activate: false);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        var html = await admin.GetStringAsync($"/admin/payments/new?user_id={userId}", TestContext.Current.CancellationToken);
        using var refused = await admin.PostAsync("/admin/payments", Form(
            ("user_id", Text(userId)),
            ("type", "yearly"),
            ("paid_on", Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("remark", "cash")), TestContext.Current.CancellationToken);

        Assert.Contains("<select id=\"f_member\" name=\"user_id\" data-autosubmit>", html, StringComparison.Ordinal);
        Assert.Contains("<input type=\"radio\" name=\"type\" value=\"joining\" checked=\"checked\">", html, StringComparison.Ordinal);
        Assert.Contains("<span class=\"muted num\">5000.00</span>", html, StringComparison.Ordinal);
        Assert.Contains("<div class=\"contents\" data-when=\"type:bonus,points\" hidden>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("value=\"yearly\"", html, StringComparison.Ordinal);
        Assert.Contains("A yearly payment is not allowed for this member right now.", await refused.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
        Assert.Equal(0, await Db.Payments.CountAsync(p => p.Type == PaymentType.Yearly && p.UserId == userId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AdminAddsAndTransfersPoints()
    {
        var sender = await Member();
        var receiver = await Member(activate: false);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var added = await admin.PostAsync("/admin/points/adjust", Form(("user_id", Text(sender)), ("amount", "50"), ("remark", "goodwill")), TestContext.Current.CancellationToken);
        using var moved = await admin.PostAsync("/admin/points/transfer", Form(("sender_id", Text(sender)), ("receiver_id", Text(receiver)), ("amount", "100")), TestContext.Current.CancellationToken);
        using var refused = await admin.PostAsync("/admin/points/transfer", Form(("sender_id", Text(receiver)), ("receiver_id", Text(sender)), ("amount", "1000")), TestContext.Current.CancellationToken);
        var history = await admin.GetStringAsync("/admin/points", TestContext.Current.CancellationToken);

        Assert.Equal("/admin/points", added.Headers.Location?.OriginalString);
        Assert.Equal("/admin/points", moved.Headers.Location?.OriginalString);
        Assert.Contains("Not enough points for this operation.", await refused.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
        Assert.Equal((11950.00m, 100.00m), (await Points.Balance(sender), await Points.Balance(receiver)));
        Assert.Contains("Points added by admin: goodwill", history, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/admin/members")]
    [InlineData("/admin/members/new")]
    [InlineData("/admin/members/{member}")]
    [InlineData("/admin/members/{member}/edit")]
    [InlineData("/admin/payments")]
    [InlineData("/admin/payments?q={payment}")]
    [InlineData("/admin/payments/new")]
    [InlineData("/admin/payments/new?user_id={member}")]
    [InlineData("/admin/payments/{payment}")]
    [InlineData("/admin/points")]
    [InlineData("/admin/points/adjust")]
    [InlineData("/admin/points/transfer")]
    public async Task ClubPagesRenderForAdmin(string template)
    {
        var member = await Member();
        var payment = await Db.Payments.OrderBy(p => p.Id).FirstAsync(TestContext.Current.CancellationToken);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var response = await admin.GetAsync(template.Replace("{member}", Text(member), StringComparison.Ordinal).Replace("{payment}", Text(payment.Id), StringComparison.Ordinal), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("aircraft-types")]
    [InlineData("aircrafts")]
    [InlineData("maintenance")]
    [InlineData("airports")]
    [InlineData("routes")]
    [InlineData("pilots")]
    [InlineData("pilot-documents")]
    [InlineData("pilot-certifications")]
    [InlineData("flight-schedule")]
    public async Task EveryResourceListsAndOffersAForm(string slug)
    {
        await Fleet();
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var index = await admin.GetAsync($"/admin/{slug}", TestContext.Current.CancellationToken);
        using var form = await admin.GetAsync($"/admin/{slug}/new", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, index.StatusCode);
        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.Contains($"<form class=\"form\" method=\"post\" action=\"/admin/{slug}\">", await form.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvalidRecordReRendersWithInlineErrors()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var response = await admin.PostAsync("/admin/aircraft-types", Form(("name", "")), TestContext.Current.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<p class=\"field__error\" id=\"f_name_error\">", html, StringComparison.Ordinal);
        Assert.Contains("The Name field is required.", html, StringComparison.Ordinal);
        Assert.Equal(0, await Db.AircraftTypes.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AdminEditsPilotAndListsIt()
    {
        var pilot = new Pilot { FirstName = "Ana", LastName = "Vega", City = "Caracas", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
        Db.Pilots.Add(pilot);
        await Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        var form = await admin.GetStringAsync($"/admin/pilots/{pilot.Id}/edit", TestContext.Current.CancellationToken);
        using var updated = await admin.PostAsync($"/admin/pilots/{pilot.Id}", Form(("first_name", "Ana"), ("last_name", "Rojas"), ("contract_type", "Captain")), TestContext.Current.CancellationToken);
        var list = await admin.GetStringAsync("/admin/pilots", TestContext.Current.CancellationToken);
        var saved = await Db.Pilots.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);

        Assert.Contains("value=\"Vega\"", form, StringComparison.Ordinal);
        Assert.Equal("/admin/pilots", updated.Headers.Location?.OriginalString);
        Assert.Equal(("Rojas", (string?)null), (saved.LastName, saved.City));
        Assert.Contains("Pilot updated.", list, StringComparison.Ordinal);
        Assert.Contains("<span class=\"pill pill--accent pill--plain\">Captain</span>", list, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeletedRecordIsNotFound()
    {
        var (_, route) = await Fleet();
        using var admin = await ActingAs(await Login(UserRole.Admin));

        await admin.PostAsync($"/admin/routes/{route}/delete", Form(), TestContext.Current.CancellationToken);
        using var edit = await admin.GetAsync($"/admin/routes/{route}/edit", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, edit.StatusCode);
    }

    [Fact]
    public async Task DashboardShowsUpcomingFlightsAndOverdueMembers()
    {
        var (aircraft, route) = await Fleet();
        var userId = await Member(activate: false);
        var member = await Db.Members.SingleAsync(m => m.UserId == userId, TestContext.Current.CancellationToken);
        var profile = await Db.Profiles.SingleAsync(p => p.UserId == userId, TestContext.Current.CancellationToken);
        member.NextDueOn = Today.AddDays(-3);
        Db.Reservations.Add(new Reservation { UserId = userId, AircraftId = aircraft, RouteId = route, FlightDate = Tomorrow, Kind = ReservationKind.Complete, Passengers = 6, Amount = 3900, Status = ReservationStatus.Confirmed });
        await Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        var html = await admin.GetStringAsync("/admin", TestContext.Current.CancellationToken);

        Assert.Contains("<span class=\"flight__place\">A</span>", html, StringComparison.Ordinal);
        Assert.Contains($"{profile.FirstName} {profile.LastName}", html, StringComparison.Ordinal);
        Assert.Contains("<span class=\"pill pill--overdue pill--plain\">3 days</span>", html, StringComparison.Ordinal);
        Assert.Contains("3,900.00 pts", html, StringComparison.Ordinal);
    }
}
