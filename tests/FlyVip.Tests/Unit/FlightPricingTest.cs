using FlyVip.Web.Domain;

namespace FlyVip.Tests.Unit;

public sealed class FlightPricingTest
{
    [Fact]
    public void SeatPriceIsRoutePlusAircraft()
    {
        Assert.Equal(650.00m, FlightPricing.SeatPrice(400.00m, 250.00m));
    }

    [Fact]
    public void ChargeScalesBySeats()
    {
        Assert.Equal(3900.00m, FlightPricing.Charge(650.00m, 6));
        Assert.Equal(1300.00m, FlightPricing.Charge(650.00m, 2));
    }
}
