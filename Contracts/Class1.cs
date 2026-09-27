namespace Contracts;

public class OrderCreated
{
    public Guid Id { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid BuyerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
