using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts;
using MassTransit;
using OrderService.DAL.Db;
using OrderService.DAL.Models;
using OrderService.DAL.Models.Orders;

namespace OrderService.BLL;

public class OrderManager
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IOrderRepository repository;
    private readonly IOutboxMessagePublisher messagePublisher;

    public OrderManager(IUnitOfWork unitOfWork, IOrderRepository repository, IOutboxMessagePublisher messagePublisher)
    {
        this.unitOfWork = unitOfWork;
        this.repository = repository;
        this.messagePublisher = messagePublisher;
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

        messagePublisher.Publish(new CustomOutboxMessage
        {
            CreatedAt = DateTime.UtcNow,
            MessageId = Guid.NewGuid(),
            AttemptCount = 1,
            Type = messageTypeName.MessageTypeName,
            Status = "pending",
            Payload = JsonSerializer.Serialize(message)
        }
        );
        
        await unitOfWork.SaveChangesAsync(ct);

        return order;
    }
}