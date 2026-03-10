using Confluent.Kafka;

namespace ConversionReportService.Presentation.Kafka.Consumers;

public interface IKafkaConsumerFactory
{
    IConsumer<long, byte[]> Create();
}
