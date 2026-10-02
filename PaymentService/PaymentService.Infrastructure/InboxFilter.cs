using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Application;

namespace PaymentService.Infrastructure;

public class InboxFilter<T>(AppDbContext db, ILogger<InboxFilter<T>> logger)
    : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (context.MessageId is not { } messageId)
            throw new InvalidOperationException("Message has no MessageId, cannot deduplicate");

        var consumer = typeof(T).Name;

        if (await db.Set<InboxMessage>().AnyAsync(
                m => m.MessageId == messageId && m.Consumer == consumer, context.CancellationToken))
        {
            logger.LogInformation("Duplicate {MessageId} for {Consumer}, skipping", messageId, consumer);
            return;
        }

        db.Set<InboxMessage>().Add(new InboxMessage
        {
            MessageId = messageId, Consumer = consumer, ProcessedAt = DateTime.UtcNow
        });

        try
        {
            await next.Send(context);
        }
        catch (DuplicateException)
        {
            if (!await IsRecorded(messageId, consumer, context.CancellationToken))
                throw;

            logger.LogInformation("Concurrent duplicate {MessageId}, skipping", messageId);
        }
    }

    private async Task<bool> IsRecorded(Guid id, string consumer, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        return await db.Set<InboxMessage>().AnyAsync(m => m.MessageId == id && m.Consumer == consumer, ct);
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("inbox");
}