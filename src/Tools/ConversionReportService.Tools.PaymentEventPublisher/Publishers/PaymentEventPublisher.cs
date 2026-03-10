using Confluent.Kafka;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using Google.Protobuf;
using ConversionReportService.Tools.PaymentEventPublisher.Settings;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace ConversionReportService.Tools.PaymentEventPublisher;

public static class PaymentEventPublisher
{
    public static async Task RunAsync(PublisherSettings settings)
    {
        var bootstrapServers = RequireString(settings.BootstrapServers, "PaymentEventPublisher:BootstrapServers");
        var topic = RequireString(settings.Topic, "PaymentEventPublisher:Topic");
        var productId = RequireLong(settings.ProductId, "PaymentEventPublisher:ProductId");
        var checkoutId = RequireLong(settings.CheckoutId, "PaymentEventPublisher:CheckoutId");
        var status = RequireString(settings.Status, "PaymentEventPublisher:Status");
        var occurredAt = (settings.OccurredAt ?? DateTime.UtcNow).ToUniversalTime();

        var value = new PaymentEventValue
        {
            ProductId = productId,
            CheckoutId = checkoutId,
            Status = status,
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
