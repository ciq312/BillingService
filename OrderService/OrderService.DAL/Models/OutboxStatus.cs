namespace OrderService.DAL.Models.Outbox;

public enum OutboxStatus
{
    Pending,
    Processed,
    Parked
}