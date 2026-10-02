namespace PaymentService.Domain;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid BuyerId { get; private set; }
    public PaymentStatus Status { get; private set; }

    public Payment(decimal amount, Guid orderId, Guid buyerId)
    {
        Id = Guid.CreateVersion7();
        OrderId = orderId;
        BuyerId = buyerId;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        Status = PaymentStatus.Pending;
    }

    private Payment() {}
    public void MarkProcessing()
    {
        TransitionTo(PaymentStatus.Processing);
    }

    public void MarkSucceded()
    {
        TransitionTo(PaymentStatus.Succeded);
    }

    public void MarkFailed()
    {
        TransitionTo(PaymentStatus.Failed);
    }

    public void MarkCanceled()
    {
        TransitionTo(PaymentStatus.Canceled);
    }

    private bool CanTransitionTo(PaymentStatus newState)
    {
        return (Status, newState) switch
        {
            (PaymentStatus.Pending, PaymentStatus.Processing) => true,
            (PaymentStatus.Processing, PaymentStatus.Succeded) => true,
            (PaymentStatus.Processing, PaymentStatus.Failed) => true,
            (PaymentStatus.Processing, PaymentStatus.Canceled) => true,
            _ => false
        };
    }

    private void TransitionTo(PaymentStatus newState)
    {
        if (CanTransitionTo(newState))
            Status = newState;
        else throw new InvalidOperationException();
    }
}