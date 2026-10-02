using Microsoft.EntityFrameworkCore;
using PaymentService.Application;
using PaymentService.Domain;

namespace PaymentService.Infrastructure;

public class PaymentRepository(AppDbContext context) : IPaymentRepository
{
    private readonly AppDbContext context = context;
    public void AddPayment(Payment payment)
    {
        context.Payments.Add(payment);
    }

    public Task<Payment?> GetPaymentByIdAsync(Guid id) => context.Payments.FirstOrDefaultAsync(p => p.Id == id);

    public Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken ct) =>
        context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    
}