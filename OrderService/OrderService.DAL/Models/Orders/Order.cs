namespace OrderService.DAL.Models.Orders;

public class Order : Entity<Guid>
{
    public decimal TotalPrice { get; private set; }
    public Guid BuyerId { get; private set; }
    public OrderState State { get; private set; } = OrderState.Pending;

    private Order()
    {

    }

    public Order(decimal totalPrice, Guid buyerId)
    {
        Id = Guid.CreateVersion7();
        TotalPrice = totalPrice;
        BuyerId = buyerId;
    }

    public void MarkPaid()
    {
        TransitionTo(OrderState.Paid);
    }

    private void TransitionTo(OrderState newState)
    {
        if (State == OrderState.Pending)
        {
            if (newState == OrderState.Pending) throw new InvalidOperationException();
            State = newState;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}