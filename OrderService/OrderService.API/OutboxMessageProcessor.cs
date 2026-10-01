using MassTransit;
using OrderService.DAL.Db;

namespace OrderService.BLL;

public class OutboxMessageProcessor(IOutboxMessagePublisher outboxMessagePublisher, IPublishEndpoint endpoint) : BackgroundService
{
    private const int TickIntervalMs = 200;
    private const int RowsToTake = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TickIntervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var messages = await outboxMessagePublisher.GetFirstMessages(RowsToTake);

            await endpoint.PublishBatch(messages);
        }
        
    }
}