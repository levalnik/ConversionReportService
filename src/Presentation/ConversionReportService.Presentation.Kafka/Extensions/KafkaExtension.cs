using ConversionReportService.Application.Abstractions.Messaging;
using ConversionReportService.Presentation.Kafka.Options;
using ConversionReportService.Presentation.Kafka.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionReportService.Presentation.Kafka.Extensions;

public static class KafkaExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        services.AddScoped<IReportRequestPublisher, ReportRequestPublisher>();
        
        return services;
    }
}