using Contracts;
using MassTransit;
using PaymentService.Application;

namespace PaymentService.API;

class OrderCreatedConsumer(PaymentManager paymentManager) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        OrderCreated msg = context.Message;

        await paymentManager.CreatePayment(new CreatePaymentCommand
        {
            OrderId = msg.Id,
            BuyerId = msg.BuyerId,
            Amount = msg.TotalPrice
        }, context.CancellationToken);
    }
}