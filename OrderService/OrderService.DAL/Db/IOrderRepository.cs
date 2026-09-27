using OrderService.DAL.Models.Orders;

namespace OrderService.DAL.Db;

public interface IOrderRepository
{
    void Add(Order order);
}