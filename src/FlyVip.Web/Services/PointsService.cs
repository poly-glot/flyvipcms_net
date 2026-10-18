using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public sealed class PointsService(AppDbContext db)
{
    public async Task<decimal> Balance(int userId) =>
        await db.PointsLedger
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.Id)
            .Select(entry => (decimal?)entry.BalanceAfter)
            .FirstOrDefaultAsync() ?? 0m;

    public Task<decimal> Credit(int userId, decimal amount, LedgerKind kind, string description, int? paymentId = null, int? transferId = null, int? reservationId = null) =>
        Post(new PointsLedgerEntry { UserId = userId, Amount = Normalise(amount), Kind = kind, Description = description, PaymentId = paymentId, TransferId = transferId, ReservationId = reservationId }, false);

    public Task<decimal> Debit(int userId, decimal amount, LedgerKind kind, string description, int? paymentId = null, int? transferId = null, int? reservationId = null, bool allowOverdraft = false) =>
        Post(new PointsLedgerEntry { UserId = userId, Amount = -Normalise(amount), Kind = kind, Description = description, PaymentId = paymentId, TransferId = transferId, ReservationId = reservationId }, allowOverdraft);

    public Task<int> Transfer(int senderId, int receiverId, decimal amount, int adminId)
    {
        if (senderId == receiverId)
        {
            throw new DomainRuleException("Sender and receiver must be different members.");
        }

        if (amount <= 0)
        {
            throw new DomainRuleException("Transfer amount must be greater than zero.");
        }

        return Atomic.Run(db, async () =>
        {
            var transfer = new PointsTransfer { SenderId = senderId, ReceiverId = receiverId, Amount = amount, CreatedById = adminId, CreatedAt = DateTime.Now };
            db.PointsTransfers.Add(transfer);
            await db.SaveChangesAsync();

            await Debit(senderId, amount, LedgerKind.TransferOut, $"Points transferred to member #{receiverId} (transfer #{transfer.Id})", transferId: transfer.Id);
            await Credit(receiverId, amount, LedgerKind.TransferIn, $"Points received from member #{senderId} (transfer #{transfer.Id})", transferId: transfer.Id);

            return transfer.Id;
        });
    }

    public static decimal Normalise(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    private Task<decimal> Post(PointsLedgerEntry entry, bool allowOverdraft) => Atomic.Run(db, async () =>
    {
        await db.Database.ExecuteSqlAsync($"SELECT Id FROM Users WHERE Id = {entry.UserId} FOR UPDATE");

        entry.BalanceAfter = await Balance(entry.UserId) + entry.Amount;

        if (!allowOverdraft && entry.BalanceAfter < 0)
        {
            throw new DomainRuleException("Not enough points for this operation.");
        }

        entry.CreatedAt = DateTime.Now;
        db.PointsLedger.Add(entry);
        await db.SaveChangesAsync();

        return entry.BalanceAfter;
    });
}
