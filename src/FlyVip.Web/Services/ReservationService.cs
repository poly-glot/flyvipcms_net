using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Domain;
using FlyVip.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public sealed class ReservationService(AppDbContext db, PointsService points, TimeProvider? clock = null)
{
    private DateOnly Today => (clock ?? TimeProvider.System).Today();

    public Task<int> Book(int userId, int routeId, int aircraftId, DateOnly flightDate, ReservationKind kind, IEnumerable<int>? seats = null) => Atomic.Run(db, async () =>
    {
        await AssertBookable(userId, flightDate);

        var route = await db.AirRoutes.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == routeId) ?? throw new DomainRuleException("Route not found.");
        var aircraft = await db.Aircrafts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == aircraftId) ?? throw new DomainRuleException("Aircraft not found.");

        if (route.AircraftId is { } operatedBy && operatedBy != aircraftId)
        {
            throw new DomainRuleException("This route is not operated by the selected aircraft.");
        }

        var capacity = aircraft.PassengerCapacity;
        var seatPrice = FlightPricing.SeatPrice(route.Cost, aircraft.TotalCost);
        var owner = await FlightOwner(aircraftId, flightDate);

        if (kind == ReservationKind.Complete)
        {
            return await BookComplete(userId, route, aircraft, flightDate, capacity, seatPrice, owner);
        }

        var chosen = ValidSeats(seats ?? [], capacity);

        return owner is null
            ? await BookPartialOwner(userId, route, aircraft, flightDate, capacity, seatPrice, chosen)
            : await JoinPartial(userId, route, aircraft, flightDate, seatPrice, chosen, owner);
    });

    public Task Confirm(int reservationId, int actorId, bool actorIsAdmin) => Atomic.Run(db, async () =>
    {
        var reservation = await Find(reservationId, "Reservation not found.");

        if (reservation.Status != ReservationStatus.Pending || reservation.ParentReservationId is not { } ownerId)
        {
            throw new DomainRuleException("Only pending seat requests can be confirmed.");
        }

        var owner = await Find(ownerId, "Flight owner not found.");

        if (!actorIsAdmin && owner.UserId != actorId)
        {
            throw new DomainRuleException("Only the flight owner can confirm seat requests.");
        }

        reservation.Status = ReservationStatus.Confirmed;
        reservation.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        await Activity(owner.UserId, reservationId, "You confirmed a seat request on your flight");
        await Activity(reservation.UserId, reservationId, "Your seat request was confirmed");
    });

    public Task Cancel(int reservationId, int actorId, bool actorIsAdmin) => Atomic.Run(db, async () =>
    {
        var reservation = await Find(reservationId, "Reservation not found.");

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new DomainRuleException("This reservation is already cancelled.");
        }

        if (!actorIsAdmin && reservation.UserId != actorId)
        {
            throw new DomainRuleException("You can only cancel your own reservations.");
        }

        if (!actorIsAdmin && reservation.FlightDate < Today)
        {
            throw new DomainRuleException("Past flights cannot be cancelled.");
        }

        await (reservation.ParentReservationId is null ? CancelOwner(reservation) : CancelJoiner(reservation));
    });

    private async Task<int> BookComplete(int userId, AirRoute route, Aircraft aircraft, DateOnly date, int capacity, decimal seatPrice, Reservation? owner)
    {
        if (owner is not null || await db.ReservationSeats.AnyAsync(s => s.AircraftId == aircraft.Id && s.FlightDate == date))
        {
            throw new DomainRuleException("This aircraft already has bookings on that date.");
        }

        var charge = FlightPricing.Charge(seatPrice, capacity);
        var reservation = await Reserve(userId, null, route, aircraft, date, ReservationKind.Complete, [.. Enumerable.Range(1, capacity).Select(seat => (byte)seat)], charge, ReservationStatus.Confirmed);

        await points.Debit(userId, charge, LedgerKind.Booking, $"Complete charter reservation #{reservation.Id}", reservationId: reservation.Id);
        await Activity(userId, reservation.Id, $"You booked a complete charter on {date:yyyy-MM-dd}");

        return reservation.Id;
    }

    private async Task<int> BookPartialOwner(int userId, AirRoute route, Aircraft aircraft, DateOnly date, int capacity, decimal seatPrice, List<byte> seats)
    {
        var charge = FlightPricing.Charge(seatPrice, capacity);
        var reservation = await Reserve(userId, null, route, aircraft, date, ReservationKind.Partial, seats, charge, ReservationStatus.Confirmed);

        await points.Debit(userId, charge, LedgerKind.Booking, $"Shared flight reservation #{reservation.Id} (full aircraft held)", reservationId: reservation.Id);
        await Activity(userId, reservation.Id, $"You opened a shared flight on {date:yyyy-MM-dd}");

        return reservation.Id;
    }

    private async Task<int> JoinPartial(int userId, AirRoute route, Aircraft aircraft, DateOnly date, decimal seatPrice, List<byte> seats, Reservation owner)
    {
        if (owner.Kind != ReservationKind.Partial || owner.RouteId != route.Id)
        {
            throw new DomainRuleException("That aircraft is already booked on this date for a different flight.");
        }

        if (owner.UserId == userId)
        {
            throw new DomainRuleException("You already own this flight.");
        }

        var share = FlightPricing.Charge(seatPrice, seats.Count);
        var reservation = await Reserve(userId, owner.Id, route, aircraft, date, ReservationKind.Partial, seats, share, ReservationStatus.Pending);

        await points.Debit(userId, share, LedgerKind.Booking, $"Seat request #{reservation.Id} on flight #{owner.Id}", reservationId: reservation.Id);
        await points.Credit(owner.UserId, share, LedgerKind.Refund, $"Seat share refunded: member joined flight #{owner.Id}", reservationId: reservation.Id);

        owner.Amount -= share;
        owner.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        await Activity(userId, reservation.Id, $"You requested seats on shared flight #{owner.Id}; waiting for the owner to confirm");
        await Activity(owner.UserId, reservation.Id, "A member requested seats on your flight");

        return reservation.Id;
    }

    private async Task CancelOwner(Reservation owner)
    {
        var children = await db.Reservations.Where(r => r.ParentReservationId == owner.Id && r.Status != ReservationStatus.Cancelled).ToListAsync();

        foreach (var child in children)
        {
            await RefundAndCancel(child, "The flight owner cancelled the shared flight");
        }

        await RefundAndCancel(owner, "You cancelled your flight");
    }

    private async Task CancelJoiner(Reservation joiner)
    {
        var owner = await Find(joiner.ParentReservationId ?? 0, "Flight owner not found.");
        var share = joiner.Amount;

        await points.Debit(owner.UserId, share, LedgerKind.Booking, $"Seat share re-charged: member left flight #{owner.Id}", reservationId: joiner.Id, allowOverdraft: true);

        owner.Amount += share;
        owner.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        await Activity(owner.UserId, joiner.Id, "A member cancelled their seats on your flight");
        await RefundAndCancel(joiner, "You cancelled your seats");
    }

    private async Task RefundAndCancel(Reservation reservation, string activity)
    {
        if (reservation.Amount > 0)
        {
            await points.Credit(reservation.UserId, reservation.Amount, LedgerKind.Refund, $"Refund for reservation #{reservation.Id}", reservationId: reservation.Id);
        }

        await db.ReservationSeats.Where(s => s.ReservationId == reservation.Id).ExecuteDeleteAsync();
        reservation.Status = ReservationStatus.Cancelled;
        reservation.Amount = 0m;
        reservation.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        await Activity(reservation.UserId, reservation.Id, activity);
    }

    private async Task AssertBookable(int userId, DateOnly flightDate)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.UserId == userId && m.PlanId != null)
            ?? throw new DomainRuleException("Only primary members can book flights.");

        if (member.Status != MemberStatus.Active)
        {
            throw new DomainRuleException("Membership is not active.");
        }

        if (flightDate < Today)
        {
            throw new DomainRuleException("Flight date must be today or later.");
        }
    }

    private static List<byte> ValidSeats(IEnumerable<int> seats, int capacity)
    {
        var unique = seats.Distinct().ToList();

        if (unique.Count == 0)
        {
            throw new DomainRuleException("Select at least one seat.");
        }

        foreach (var seat in unique.Where(seat => seat < 1 || seat > capacity))
        {
            throw new DomainRuleException($"Seat {seat} does not exist on this aircraft.");
        }

        return [.. unique.Select(seat => (byte)seat)];
    }

    private Task<Reservation?> FlightOwner(int aircraftId, DateOnly date) =>
        db.Reservations
            .Where(r => r.AircraftId == aircraftId && r.FlightDate == date && r.ParentReservationId == null && r.Status != ReservationStatus.Cancelled)
            .OrderBy(r => r.Id)
            .FirstOrDefaultAsync();

    private async Task<Reservation> Reserve(int userId, int? ownerId, AirRoute route, Aircraft aircraft, DateOnly date, ReservationKind kind, List<byte> seats, decimal amount, ReservationStatus status)
    {
        var taken = await db.ReservationSeats
            .Where(s => s.AircraftId == aircraft.Id && s.FlightDate == date && seats.Contains(s.SeatNumber))
            .OrderBy(s => s.SeatNumber)
            .Select(s => s.SeatNumber)
            .ToListAsync();

        if (taken.Count > 0)
        {
            throw new DomainRuleException($"Seat(s) {string.Join(", ", taken)} already booked.");
        }

        var now = DateTime.Now;
        var reservation = new Reservation
        {
            UserId = userId,
            ParentReservationId = ownerId,
            AircraftId = aircraft.Id,
            RouteId = route.Id,
            PilotFlightId = await db.PilotFlights.Where(f => f.AircraftId == aircraft.Id && f.FlightDate == date).Select(f => (int?)f.Id).FirstOrDefaultAsync(),
            FlightDate = date,
            Kind = kind,
            Passengers = (byte)seats.Count,
            Amount = amount,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now,
        };

        reservation.Seats.AddRange(seats.Select(seat => new ReservationSeat { AircraftId = aircraft.Id, FlightDate = date, SeatNumber = seat }));
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        return reservation;
    }

    private async Task Activity(int userId, int reservationId, string description)
    {
        db.ReservationActivities.Add(new ReservationActivity { UserId = userId, ReservationId = reservationId, Description = description, CreatedAt = DateTime.Now });
        await db.SaveChangesAsync();
    }

    private async Task<Reservation> Find(int id, string message) => await db.Reservations.FindAsync(id) ?? throw new DomainRuleException(message);
}
