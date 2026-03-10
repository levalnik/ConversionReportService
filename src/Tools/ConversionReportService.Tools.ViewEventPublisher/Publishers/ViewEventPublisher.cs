using Confluent.Kafka;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using Google.Protobuf;
using ConversionReportService.Tools.ViewEventPublisher.Settings;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace ConversionReportService.Tools.ViewEventPublisher;

public static class ViewEventPublisher
{
    public static async Task RunAsync(PublisherSettings settings)
    {
        var bootstrapServers = RequireString(settings.BootstrapServers, "ViewEventPublisher:BootstrapServers");
        var topic = RequireString(settings.Topic, "ViewEventPublisher:Topic");
        var productId = RequireLong(settings.ProductId, "ViewEventPublisher:ProductId");
        var checkoutId = RequireLong(settings.CheckoutId, "ViewEventPublisher:CheckoutId");
        var occurredAt = (settings.OccurredAt ?? DateTime.UtcNow).ToUniversalTime();

        var value = new ViewEventValue
        {
            ProductId = productId,
            CheckoutId = checkoutId,
            OccurredAt = ProtoTimestamp.FromDateTime(occurredAt)
        };

        var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
        using var producer = new ProducerBuilder<long, byte[]>(producerConfig).Build();

        var result = await producer.ProduceAsync(topic, new Message<long, byte[]>
        {
            Key = productId,
            Value = value.ToByteArray()
        });

        Console.WriteLine($"Published: topic={result.Topic}, partition={result.Partition}, offset={result.Offset}");
    }

    private static string RequireString(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Missing required setting '{key}'.");

        return value;
    }

    private static long RequireLong(long? value, string key)
    {
        if (!value.HasValue)
            throw new ArgumentException($"Missing required setting '{key}'.");

        return value.Value;
    }
}
