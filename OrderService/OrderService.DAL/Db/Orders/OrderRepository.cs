using OrderService.DAL.Models.Orders;

namespace OrderService.DAL.Db.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }
}