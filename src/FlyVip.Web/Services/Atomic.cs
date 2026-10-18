using System.Data;
using FlyVip.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public static class Atomic
{
    public static async Task<T> Run<T>(AppDbContext db, Func<Task<T>> work)
    {
        if (db.Database.CurrentTransaction is not null)
        {
            return await work();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var result = await work();
            await transaction.CommitAsync();

            return result;
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
    }

    public static Task Run(AppDbContext db, Func<Task> work) => Run(db, async () =>
    {
        await work();

        return true;
    });
}
