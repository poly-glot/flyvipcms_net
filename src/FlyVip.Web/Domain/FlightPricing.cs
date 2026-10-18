namespace FlyVip.Web.Domain;

public static class FlightPricing
{
    public static decimal SeatPrice(decimal routeCost, decimal aircraftTotalCost) => routeCost + aircraftTotalCost;

    public static decimal Charge(decimal seatPrice, int seats) => seatPrice * seats;
}
