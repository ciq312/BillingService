using PaymentService.Domain;

namespace PaymentService.Application;

public class PaymentManager(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
{
    public async Task<Payment> CreatePayment(CreatePaymentCommand command, CancellationToken ct)
    {
        var existing = await paymentRepository.GetPaymentByOrderIdAsync(command.OrderId, ct);
        if (existing is not null)
            return existing;  

        var payment = new Payment(command.Amount, command.OrderId, command.BuyerId);

        paymentRepository.AddPayment(payment);

        await unitOfWork.SaveChangesAsync(ct);
        
        return payment;
    }
}

public class CreatePaymentCommand
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public Guid BuyerId { get; set; }
}