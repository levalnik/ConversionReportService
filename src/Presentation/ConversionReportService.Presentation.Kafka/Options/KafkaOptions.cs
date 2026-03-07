namespace ConversionReportService.Presentation.Kafka.Options;

public class KafkaOptions
{
    public string BootstrapServers { get; init; } = string.Empty;
    
    public string ReportRequestedTopic { get;  init; } = string.Empty;

    public int BatchSize { get; init; } = 100;

    public int PollTimeoutMs { get; init; } = 500;
}