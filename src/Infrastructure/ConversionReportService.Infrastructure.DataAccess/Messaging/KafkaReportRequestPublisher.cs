using System.Text.Json;
using Confluent.Kafka;
using ConversionReportService.Application.Abstractions.Messaging;

namespace ConversionReportService.Infrastructure.DataAccess.Messaging;

public class KafkaReportRequestPublisher : IReportRequestPublisher
{
    private readonly IProducer<string, string> _producer;
    
    public KafkaReportRequestPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishReportAsync(long requestId, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(new
        {
            RequestId = requestId
        });

        await _producer.ProduceAsync(
            "report-requests",
            new Message<string, string>
            {
                Key = requestId.ToString(),
                Value = message
            },
            cancellationToken);
    }
}