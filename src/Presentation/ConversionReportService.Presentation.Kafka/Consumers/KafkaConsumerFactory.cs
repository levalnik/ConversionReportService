using Confluent.Kafka;
using ConversionReportService.Presentation.Kafka.Options;
using Microsoft.Extensions.Options;

namespace ConversionReportService.Presentation.Kafka.Consumers;

public sealed class KafkaConsumerFactory : IKafkaConsumerFactory
{
    private readonly KafkaOptions _options;

    public KafkaConsumerFactory(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
    }

    public IConsumer<long, byte[]> Create()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        return new ConsumerBuilder<long, byte[]>(config).Build();
    }
}
