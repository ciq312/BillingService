using System.Runtime.InteropServices.Marshalling;
using Contracts;
using MassTransit;

namespace PaymentService.API;

class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        OrderCreated msg = context.Message;
        Console.WriteLine($"Received message: {msg.Id}, {msg.BuyerId}, {msg.CreatedAt}, {msg.TotalPrice}");
    }
}