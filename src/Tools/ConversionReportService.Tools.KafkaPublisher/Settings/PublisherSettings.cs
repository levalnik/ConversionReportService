namespace ConversionReportService.Tools.KafkaPublisher.Settings;

public class PublisherSettings
{
    public string? BootstrapServers { get; init; }

    public string? Topic { get; init; }
    
    public long? RequestId { get; init; }
    
    public long? ProductId { get; init; }
    
    public long? CheckoutId { get; init; }
    
    public DateTime? From { get; init; }
    
    public DateTime? To { get; init; } 
    
    public DateTime? CreatedAt { get; init; }
}