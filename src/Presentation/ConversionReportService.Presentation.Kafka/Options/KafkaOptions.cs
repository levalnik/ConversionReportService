namespace ConversionReportService.Presentation.Kafka.Options;

public class KafkaOptions
{
    public string BootstrapServers { get; init; } = string.Empty;

    public string ReportRequestedTopic { get; init; } = string.Empty;

    public string ViewEventsTopic { get; init; } = string.Empty;

    public string PaymentEventsTopic { get; init; } = string.Empty;

    public string ConsumerGroupId { get; init; } = "conversion-report-service";

    public int PollTimeoutMs { get; init; } = 500;
    
    public int BatchSize { get; init; } = 1000;
}
