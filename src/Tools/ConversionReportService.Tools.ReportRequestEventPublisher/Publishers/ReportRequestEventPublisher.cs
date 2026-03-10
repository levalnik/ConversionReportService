using Confluent.Kafka;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using Google.Protobuf;
using ConversionReportService.Tools.KafkaPublisher.Settings;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace ConversionReportService.Tools.KafkaPublisher;

public static class ReportRequestEventPublisher
{
    public static async Task RunAsync(PublisherSettings settings)
    {
        var bootstrapServers = RequireString(settings.BootstrapServers, "KafkaPublisher:BootstrapServers");
        var topic = RequireString(settings.Topic, "KafkaPublisher:Topic");
        var now = DateTime.UtcNow;
        var envelope = BuildEnvelope(settings, now);
        var key = GetKey(envelope);

        var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
        using var producer = new ProducerBuilder<long, byte[]>(producerConfig).Build();

        var result = await producer.ProduceAsync(topic, new Message<long, byte[]>
        {
            Key = key,
            Value = envelope.ToByteArray()
        });

        Console.WriteLine($"Published: topic={result.Topic}, partition={result.Partition}, offset={result.Offset}, key={key}");
    }

    private static ReportRequestedEnvelope BuildEnvelope(PublisherSettings settings, DateTime now)
    {
        if (settings.Batch is { Count: > 0 })
        {
            var baseId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var batch = new ReportRequestedBatchValue();

            for (var i = 0; i < settings.Batch.Count; i++)
            {
                var item = settings.Batch[i];
                var requestId = item.RequestId ?? (baseId + i);
                var productId = RequireLong(item.ProductId, $"KafkaPublisher:Batch[{i}].ProductId");
                var checkoutId = RequireLong(item.CheckoutId, $"KafkaPublisher:Batch[{i}].CheckoutId");

                var from = (item.From ?? now.AddHours(-1)).ToUniversalTime();
                var to = (item.To ?? now).ToUniversalTime();
                var createdAt = (item.CreatedAt ?? now).ToUniversalTime();

                if (from >= to)
                    throw new ArgumentException($"KafkaPublisher:Batch[{i}].From must be earlier than KafkaPublisher:Batch[{i}].To.");

                batch.Requests.Add(new ReportRequestedValue
                {
                    RequestId = requestId,
                    ProductId = productId,
                    CheckoutId = checkoutId,
                    From = ProtoTimestamp.FromDateTime(from),
                    To = ProtoTimestamp.FromDateTime(to),
                    CreatedAt = ProtoTimestamp.FromDateTime(createdAt)
                });
            }

            return new ReportRequestedEnvelope { Batch = batch };
        }

        var singleRequestId = settings.RequestId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var singleProductId = RequireLong(settings.ProductId, "KafkaPublisher:ProductId");
        var singleCheckoutId = RequireLong(settings.CheckoutId, "KafkaPublisher:CheckoutId");
        var singleFrom = (settings.From ?? now.AddHours(-1)).ToUniversalTime();
        var singleTo = (settings.To ?? now).ToUniversalTime();
        var singleCreatedAt = (settings.CreatedAt ?? now).ToUniversalTime();

        if (singleFrom >= singleTo)
            throw new ArgumentException("KafkaPublisher:From must be earlier than KafkaPublisher:To.");

        return new ReportRequestedEnvelope
        {
            Single = new ReportRequestedValue
            {
                RequestId = singleRequestId,
                ProductId = singleProductId,
                CheckoutId = singleCheckoutId,
                From = ProtoTimestamp.FromDateTime(singleFrom),
                To = ProtoTimestamp.FromDateTime(singleTo),
                CreatedAt = ProtoTimestamp.FromDateTime(singleCreatedAt)
            }
        };
    }

    private static long GetKey(ReportRequestedEnvelope envelope)
    {
        return envelope.PayloadCase switch
        {
            ReportRequestedEnvelope.PayloadOneofCase.Single => envelope.Single.RequestId,
            ReportRequestedEnvelope.PayloadOneofCase.Batch when envelope.Batch.Requests.Count > 0 => envelope.Batch.Requests[0].RequestId,
            _ => 0
        };
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
