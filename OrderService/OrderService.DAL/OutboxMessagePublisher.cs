using Microsoft.EntityFrameworkCore;
using OrderService.DAL.Db;
using OrderService.DAL.Models;

namespace OrderService.DAL;

public class OutboxMessagePublisher(AppDbContext dbContext) : IOutboxMessagePublisher
{
    public async Task<List<CustomOutboxMessage>> GetFirstMessages(int batchSize)
    {
        return await dbContext.OutboxMessages
            .Where(m => m.Status == "Pending")
            .Where(m => m.ProcessedAt == null)
            .Where(m => m.ParkedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public void Publish(CustomOutboxMessage message)
    {
        dbContext.Add(message);
    }
}