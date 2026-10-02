using PaymentService.Domain;

namespace PaymentService.Application;

public interface IPaymentRepository
{
    Task<Payment?> GetPaymentByIdAsync(Guid id);
    Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken ct);
    void AddPayment(Payment payment);
}