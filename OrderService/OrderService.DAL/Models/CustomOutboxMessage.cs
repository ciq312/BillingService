namespace OrderService.DAL.Models.Outbox;

public class CustomOutboxMessage
{
    public Guid MessageId {get; set;}
    public required string Type {get; set;}
    public required string Payload {get; set;} 
    public OutboxStatus Status {get; set;}
    public string? ErrorMessage {get; set;}
    public int AttemptCount {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime? NextAttemptAt {get; set;}
    public DateTime? ProcessedAt {get; set;} 
    public DateTime? ParkedAt {get; set;}
}