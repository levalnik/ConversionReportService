using ConversionReportService.Application.Abstractions.Messaging;
using ConversionReportService.Application.Models.Events;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using ConversionReportService.Presentation.Kafka.Abstractions;
using ConversionReportService.Presentation.Kafka.Options;
using Google.Protobuf.WellKnownTypes;

namespace ConversionReportService.Presentation.Kafka.Publishers;

public sealed class ReportRequestPublisher : IReportRequestPublisher
{
    private readonly IKafkaProducer<long, ReportRequestedValue> _producer;
    private readonly KafkaOptions _options;

    public ReportRequestPublisher(
        IKafkaProducer<long, ReportRequestedValue> producer,
        KafkaOptions options)
    {
        _producer = producer;
        _options = options;
    }

    public Task PublishReportAsync(ReportRequestedEvent evt, CancellationToken ct)
    {
        if (_options.ReportRequestedTopic == null)
            throw new ArgumentNullException(nameof(_options.ReportRequestedTopic));

        var value = new ReportRequestedValue
        {
            RequestId = evt.RequestId,
            ProductId = evt.ProductId,
            CheckoutId = evt.CheckoutId,
            From = evt.From.ToTimestamp(),
            To = evt.To.ToTimestamp(),
            CreatedAt = evt.CreatedAt.ToTimestamp()
        };

        return _producer.ProduceAsync(
            _options.ReportRequestedTopic,
            evt.RequestId,
            value,
            ct);
    }
}