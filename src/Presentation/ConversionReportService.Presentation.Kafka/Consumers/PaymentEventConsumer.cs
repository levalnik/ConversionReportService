using Confluent.Kafka;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Events;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using ConversionReportService.Presentation.Kafka.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversionReportService.Presentation.Kafka.Consumers;

public sealed class PaymentEventConsumer : BackgroundService
{
    private readonly IConsumer<long, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<PaymentEventConsumer> _logger;

    public PaymentEventConsumer(
        IKafkaConsumerFactory consumerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<PaymentEventConsumer> logger)
    {
        _consumer = consumerFactory.Create();
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.PaymentEventsTopic))
            throw new InvalidOperationException("Kafka PaymentEventsTopic is not configured.");

        _consumer.Subscribe(_options.PaymentEventsTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromMilliseconds(_options.PollTimeoutMs));
                if (result?.Message?.Value is null)
                    continue;

                var message = PaymentEventValue.Parser.ParseFrom(result.Message.Value);

                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IEventIngestionService>();

                var evt = new PaymentEvent(
                    message.ProductId,
                    message.CheckoutId,
                    message.Status,
                    message.OccurredAt.ToDateTime());

                await service.AddPaymentEventAsync(evt, stoppingToken);

                _consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error for payment events.");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while ingesting payment event from Kafka.");
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
