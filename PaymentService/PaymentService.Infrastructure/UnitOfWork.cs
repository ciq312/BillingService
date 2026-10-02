using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaymentService.Application;

namespace PaymentService.Infrastructure;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext context = context;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new DuplicateException($"Unique constraint '{pg.ConstraintName}' violated", e);
        }
    }
}
