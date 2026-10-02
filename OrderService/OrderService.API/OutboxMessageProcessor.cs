using System.Text.Json;
using MassTransit;
using OrderService.DAL.Db;
using OrderService.DAL.Db.Outbox;
using OrderService.DAL.Models.Outbox;

namespace OrderService.API;

public class OutboxMessageProcessor(IServiceScopeFactory serviceProviderFactory, MessageTypeRegistry registry, ILogger<OutboxMessageProcessor> logger) : BackgroundService
{
    private const int TickIntervalMs = 200;
    private const int RowsToTake = 10;
    private const int MaxRetryAttempts = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TickIntervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using (var scope = serviceProviderFactory.CreateAsyncScope()) {
                IOutboxMessageRepository outboxMessageRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                IPublishEndpoint endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    var outboxMessages = await outboxMessageRepository.GetFirstMessages(RowsToTake, stoppingToken);

                foreach (var outboxMessage in outboxMessages)
                {
                    try
                    {
                        var type = registry.GetMsgTypeNames()[outboxMessage.Type];
                        var message = JsonSerializer.Deserialize(outboxMessage.Payload, type)!;

                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        await endpoint.Publish(message, type, ctx => ctx.MessageId = outboxMessage.MessageId, stoppingToken);

                        outboxMessage.ProcessedAt = DateTime.UtcNow;
                        outboxMessage.Status = OutboxStatus.Processed;
                    }
                    catch (Exception e) 
                    {
                        outboxMessage.AttemptCount++;
                        if (outboxMessage.AttemptCount < MaxRetryAttempts)
                        {
                            outboxMessage.ErrorMessage = e.Message;
                            outboxMessage.NextAttemptAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, outboxMessage.AttemptCount));
                        }
                        else
                        {
                            logger.LogError(e, "Outbox message {MessageId} ({Type}) parked after {Attempts} attempts",
                                outboxMessage.MessageId, outboxMessage.Type, outboxMessage.AttemptCount);                            
                            outboxMessage.Status = OutboxStatus.Parked;
                            outboxMessage.ErrorMessage = e.Message;
                            outboxMessage.ParkedAt = DateTime.UtcNow;
                        }
                    }
                }
                
                await unitOfWork.SaveChangesAsync(ct: default);    
                }

                catch (Exception e)
                {
                    logger.LogError(e, "Error occurred while processing outbox messages");
                }
                
            }
            
        }
        
    }
}