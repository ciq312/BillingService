using OrderService.DAL.Db;
using OrderService.DAL.Models.Orders;

namespace OrderService.BLL;

public class OrderManager
{
    private readonly AppDbContext dbContext;

    public OrderManager(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Order> MakeNewOrder(MakeNewOrderDTO dto)
    {
        Order order = new Order(dto.totalPrice, dto.BuyerId);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        return order;
    }
}