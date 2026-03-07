namespace ConversionReportService.Presentation.Kafka.Abstractions;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(string topic, TKey key, TValue value, CancellationToken ct);
}