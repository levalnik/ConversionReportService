using Confluent.Kafka;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using ConversionReportService.Presentation.Kafka.Consumers;
using ConversionReportService.Presentation.Kafka.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConversionReportService.Presentation.Kafka.Extensions;

public static class KafkaExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection("Kafka"))
            .ValidateOnStart();

        services.AddSingleton<IConsumer<long, byte[]>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = options.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            return new ConsumerBuilder<long, byte[]>(config)
                .Build();
        });

        services.AddHostedService<ReportRequestedConsumer>();
        
        return services;
    }
}
