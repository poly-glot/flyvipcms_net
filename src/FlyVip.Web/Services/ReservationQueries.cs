using System.Linq.Expressions;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public sealed record ReservationListing(
    int Id,
    int UserId,
    int? ParentReservationId,
    DateOnly FlightDate,
    ReservationStatus Status,
    ReservationKind Kind,
    byte Passengers,
    decimal Amount,
    string AircraftName,
    byte Capacity,
    string FromName,
    string ToName,
    string? MemberCode,
    string? FirstName,
    string? LastName)
{
    public bool IsCancelled => Status == ReservationStatus.Cancelled;

    public bool IsPending => Status == ReservationStatus.Pending;

    public bool IsComplete => Kind == ReservationKind.Complete;

    public int SeatsHeld => IsComplete ? Capacity : Passengers;

    public string FullName => FirstName + " " + LastName;

    public bool IsUpcoming(DateOnly today) => !IsCancelled && FlightDate >= today;
}

public sealed class ReservationQueries(AppDbContext db)
{
    public static Expression<Func<Reservation, bool>> IsUpcoming(DateOnly today) =>
        reservation => reservation.Status != ReservationStatus.Cancelled && reservation.FlightDate >= today;

    public Task<List<ReservationListing>> ForAccount(int userId) =>
        Project(db.Reservations.IgnoreQueryFilters().Where(reservation => reservation.UserId == userId).OrderByDescending(reservation => reservation.FlightDate), false)
            .ToListAsync();

    public Task<List<ReservationListing>> Upcoming(int userId, DateOnly today, int take) =>
        Project(db.Reservations.IgnoreQueryFilters().Where(reservation => reservation.UserId == userId).Where(IsUpcoming(today)).OrderBy(reservation => reservation.FlightDate).Take(take), false)
            .ToListAsync();

    public Task<List<ReservationListing>> UpcomingForClub(DateOnly today, int take) =>
        Project(MemberReservations().Where(reservation => reservation.ParentReservationId == null).Where(IsUpcoming(today)).OrderBy(reservation => reservation.FlightDate).Take(take), true)
            .ToListAsync();

    public Task<List<ReservationListing>> All() =>
        Project(MemberReservations().OrderByDescending(reservation => reservation.FlightDate).ThenByDescending(reservation => reservation.Id), true)
            .ToListAsync();

    public Task<List<ReservationListing>> SeatRequestsOn(IReadOnlyCollection<int> ownerReservationIds) =>
        Project(MemberReservations().Where(reservation => ownerReservationIds.Contains(reservation.ParentReservationId ?? 0) && reservation.Status == ReservationStatus.Pending), true)
            .ToListAsync();

    public Task<ReservationListing?> Find(int id) =>
        Project(MemberReservations().Where(reservation => reservation.Id == id), true).FirstOrDefaultAsync();

    public Task<List<ReservationListing>> Joiners(int id) =>
        Project(MemberReservations().Where(reservation => reservation.ParentReservationId == id), true).ToListAsync();

    public Task<List<byte>> Seats(int id) =>
        db.ReservationSeats.Where(seat => seat.ReservationId == id).OrderBy(seat => seat.SeatNumber).Select(seat => seat.SeatNumber).ToListAsync();

    public Task<List<int>> TakenSeats(int aircraftId, DateOnly flightDate) =>
        db.ReservationSeats
            .Where(seat => seat.AircraftId == aircraftId && seat.FlightDate == flightDate)
            .OrderBy(seat => seat.SeatNumber)
            .Select(seat => (int)seat.SeatNumber)
            .ToListAsync();

    public Task<List<ActivityLine>> ActivityOf(int reservationId) =>
        db.ReservationActivities
            .Where(item => item.ReservationId == reservationId)
            .OrderByDescending(item => item.Id)
            .Select(item => new ActivityLine(item.Description, item.CreatedAt))
            .ToListAsync();

    public Task<List<ActivityLine>> RecentActivity(int userId, int take) =>
        db.ReservationActivities
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.Id)
            .Take(take)
            .Select(item => new ActivityLine(item.Description, item.CreatedAt))
            .ToListAsync();

    private IQueryable<Reservation> MemberReservations() =>
        db.Reservations.IgnoreQueryFilters().Where(reservation => reservation.User.Member != null);

    private static IQueryable<ReservationListing> Project(IQueryable<Reservation> reservations, bool withMember) =>
        reservations.Select(r => new ReservationListing(
            r.Id,
            r.UserId,
            r.ParentReservationId,
            r.FlightDate,
            r.Status,
            r.Kind,
            r.Passengers,
            r.Amount,
            r.Aircraft.Name,
            r.Aircraft.PassengerCapacity,
            r.Route.FromAirport.Name,
            r.Route.ToAirport.Name,
            withMember ? r.User.Member!.MemberCode : null,
            withMember ? r.User.Profile!.FirstName : null,
            withMember ? r.User.Profile!.LastName : null));
}
