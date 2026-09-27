using Contracts;
using MassTransit;
using OrderService.DAL.Db;
using OrderService.DAL.Models.Orders;

namespace OrderService.BLL;

public class OrderManager
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IOrderRepository repository;
    private readonly IPublishEndpoint endpoint;

    public OrderManager(IUnitOfWork unitOfWork, IOrderRepository repository, IPublishEndpoint endpoint)
    {
        this.unitOfWork = unitOfWork;
        this.repository = repository;
        this.endpoint = endpoint;
    }

    public async Task<Order> CreateOrderAsync(CreateNewOrderDTO dto, CancellationToken ct)
    {
        Order order = new Order(dto.TotalPrice, dto.BuyerId);

        repository.Add(order);
        await unitOfWork.SaveChangesAsync(ct);

        await endpoint.Publish(new OrderCreated
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt
        });

        return order;
    }
}