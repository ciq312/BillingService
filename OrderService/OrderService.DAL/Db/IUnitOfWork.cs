namespace OrderService.DAL.Db;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}