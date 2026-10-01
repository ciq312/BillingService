using OrderService.DAL.Models;

namespace OrderService.DAL.Db;

public interface IOutboxMessagePublisher
{
    void Publish(CustomOutboxMessage message);

    Task<List<CustomOutboxMessage>> GetFirstMessages(int batchSize);
}