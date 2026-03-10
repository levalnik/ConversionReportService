namespace ConversionReportService.Tools.ViewEventPublisher.Settings;

public class PublisherSettings
{
    public string? BootstrapServers { get; init; }

    public string? Topic { get; init; }

    public long? ProductId { get; init; }

    public long? CheckoutId { get; init; }

    public DateTime? OccurredAt { get; init; }
}
