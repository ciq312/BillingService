using OrderService.DAL.Models.Outbox;

namespace OrderService.DAL.Db.Outbox;

public interface IOutboxMessageRepository
{
    void Add(CustomOutboxMessage message);

    void Update(CustomOutboxMessage message);

    Task<List<CustomOutboxMessage>> GetFirstMessages(int batchSize);
}