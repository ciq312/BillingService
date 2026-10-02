using System.Text.Json;
using Contracts;
using MassTransit;
using OrderService.DAL.Db;
using OrderService.DAL.Db.Outbox;
using OrderService.DAL.Models.Outbox;

namespace OrderService.BLL;

public class OutboxMessageProcessor(IServiceScopeFactory serviceProviderFactory) : BackgroundService
{
    private const int TickIntervalMs = 200;
    private const int RowsToTake = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TickIntervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using (var scope = serviceProviderFactory.CreateAsyncScope()) {
                IOutboxMessageRepository outboxMessageRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                IPublishEndpoint endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var outboxMessages = await outboxMessageRepository.GetFirstMessages(RowsToTake, stoppingToken);

                var messages = outboxMessages.Select(m => JsonSerializer.Deserialize<OrderCreated>(m.Payload)!);

                await endpoint.PublishBatch(messages);    

                foreach (var outboxMessage in outboxMessages)
                {
                    outboxMessage.ProcessedAt = DateTime.UtcNow;
                    outboxMessage.Status = OutboxStatus.Processed;
                }
                
                await unitOfWork.SaveChangesAsync(ct: default);
            }
            
        }
        
    }
}