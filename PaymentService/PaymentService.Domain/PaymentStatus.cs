namespace PaymentService.Domain;

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Succeded = 2,
    Failed = 3,
    Canceled = 4,
}