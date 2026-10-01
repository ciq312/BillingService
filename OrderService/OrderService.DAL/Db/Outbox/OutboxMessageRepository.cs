using Microsoft.EntityFrameworkCore;
using OrderService.DAL.Models.Outbox;

namespace OrderService.DAL.Db.Outbox;

public class OutboxMessageRepository(AppDbContext dbContext) : IOutboxMessageRepository
{
    public async Task<List<CustomOutboxMessage>> GetFirstMessages(int batchSize)
    {
        return await dbContext.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public void Add(CustomOutboxMessage message)
    {
        dbContext.Add(message);
    }

    public void Update(CustomOutboxMessage message)
    {
        dbContext.Update(message);
    }
}