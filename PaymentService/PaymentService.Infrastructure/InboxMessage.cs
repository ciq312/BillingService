namespace PaymentService.Infrastructure;

public class InboxMessage
{
    public Guid MessageId {get; set;}
    public required string Consumer {get; set;}
    public DateTime ProcessedAt {get; set;}
}