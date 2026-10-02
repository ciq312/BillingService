using System.Reflection;
using System.Text.Json;
using Contracts;
using OrderService.DAL.Db;
using OrderService.DAL.Db.Orders;
using OrderService.DAL.Db.Outbox;
using OrderService.DAL.Models.Orders;
using OrderService.DAL.Models.Outbox;

namespace OrderService.BLL;

public class OrderManager
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IOrderRepository repository;
    private readonly IOutboxMessageRepository messageRepository;

    public OrderManager(IUnitOfWork unitOfWork, IOrderRepository repository, IOutboxMessageRepository messageRepository)
    {
        this.unitOfWork = unitOfWork;
        this.repository = repository;
        this.messageRepository = messageRepository;
    }

    public async Task<Order> CreateOrderAsync(CreateNewOrderDTO dto, CancellationToken ct)
    {
        Order order = new Order(dto.TotalPrice, dto.BuyerId);


        var messageTypeName = typeof(OrderCreated).GetCustomAttribute<MessageTypeAttribute>();

        if (messageTypeName == null) throw new InvalidCastException("Can't find attribute");
        
        OrderCreated message = new OrderCreated
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt
        };

        repository.Add(order);
        
        messageRepository.Add(new CustomOutboxMessage
        {
            CreatedAt = DateTime.UtcNow,
            MessageId = Guid.CreateVersion7(),
            AttemptCount = 0,
            Type = messageTypeName.MessageTypeName,
            Status = OutboxStatus.Pending,
            Payload = JsonSerializer.Serialize(message)
        }
        );
        
        await unitOfWork.SaveChangesAsync(ct);

        return order;
    }
}