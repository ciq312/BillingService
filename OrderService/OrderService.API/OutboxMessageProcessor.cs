using MassTransit;
using MassTransit.DependencyInjection;
using OrderService.DAL.Db;

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
                IOutboxMessagePublisher outboxMessagePublisher = scope.ServiceProvider.GetRequiredService<IOutboxMessagePublisher>();
                IPublishEndpoint endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                
                var messages = await outboxMessagePublisher.GetFirstMessages(RowsToTake);

                await endpoint.PublishBatch(messages);    
            }
            
        }
        
    }
}