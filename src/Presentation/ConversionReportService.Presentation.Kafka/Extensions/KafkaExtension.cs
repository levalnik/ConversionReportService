using ConversionReportService.Presentation.Kafka.Consumers;
using ConversionReportService.Presentation.Kafka.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionReportService.Presentation.Kafka.Extensions;

public static class KafkaExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection("Kafka"))
            .ValidateOnStart();

        services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();

        services.AddHostedService<ReportRequestedEventConsumer>();
        services.AddHostedService<ViewEventConsumer>();
        services.AddHostedService<PaymentEventConsumer>();

        return services;
    }
}
