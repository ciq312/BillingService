namespace OrderService.DAL.Models;

public class CustomOutboxMessage
{
    public Guid MessageId {get; set;}
    public string Type {get; set;}
    public string Payload {get; set;} 
    public string Status {get; set;}
    public string ErrorMessage {get; set;}
    public int AttemptCount {get; set;}
    public DateTime? CreatedAt {get; set;}
    public DateTime? ProcessedAt {get; set;} 
    public DateTime? ParkedAt {get; set;}
}