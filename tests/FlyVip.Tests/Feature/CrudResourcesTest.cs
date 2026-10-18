using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;

namespace FlyVip.Tests.Feature;

public sealed class CrudResourcesTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Theory]
    [InlineData("aircraft-types")]
    [InlineData("aircrafts")]
    [InlineData("airports")]
    [InlineData("flight-schedule")]
    [InlineData("maintenance")]
    [InlineData("pilot-certifications")]
    [InlineData("pilot-documents")]
    [InlineData("pilots")]
    [InlineData("routes")]
    public async Task EveryResourceFieldBindsToItsForm(string slug)
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var index = await admin.GetAsync($"/admin/{slug}", TestContext.Current.CancellationToken);
        using var blank = await admin.GetAsync($"/admin/{slug}/new", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, index.StatusCode);
        Assert.Equal(HttpStatusCode.OK, blank.StatusCode);
    }
}
