using System.Runtime.CompilerServices;

namespace OrderService.DAL.Models.Orders;

public class Order : Entity<Guid>
{
    public decimal TotalPrice { get; private set; }
    public Guid BuyerId { get; private set; }
    public OrderState State { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Order(decimal totalPrice, Guid buyerId)
    {
        Id = Guid.CreateVersion7();
        TotalPrice = totalPrice;
        BuyerId = buyerId;
        State = OrderState.Pending;
    }

    private Order()
    {

    }

    public void MarkPaid()
    {
        TransitionTo(OrderState.Paid);
    }

    private bool CanTransitionTo(OrderState newState)
    {
        return (State, newState) switch
        {
            (OrderState.Pending, OrderState.Canceled) => true,
            (OrderState.Pending, OrderState.Paid) => true,
            _ => false
        };
    }

    private void TransitionTo(OrderState newState)
    {
        if (CanTransitionTo(newState))
            State = newState;
        else throw new InvalidOperationException();
    }

}