using OrderService.DAL.Models.Orders;

namespace OrderService.DAL.Db.Orders;

public interface IOrderRepository
{
    void Add(Order order);
}