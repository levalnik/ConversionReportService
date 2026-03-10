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

public sealed class ViewEventConsumer : BackgroundService
{
    private readonly IConsumer<long, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<ViewEventConsumer> _logger;

    public ViewEventConsumer(
        IKafkaConsumerFactory consumerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<ViewEventConsumer> logger)
    {
        _consumer = consumerFactory.Create();
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ViewEventsTopic))
            throw new InvalidOperationException("Kafka ViewEventsTopic is not configured.");

        _consumer.Subscribe(_options.ViewEventsTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromMilliseconds(_options.PollTimeoutMs));
                if (result?.Message?.Value is null)
                    continue;

                var message = ViewEventValue.Parser.ParseFrom(result.Message.Value);

                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IEventIngestionService>();

                var evt = new ViewEvent(
                    message.ProductId,
                    message.CheckoutId,
                    message.OccurredAt.ToDateTime());

                await service.AddViewEventAsync(evt, stoppingToken);

                _consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error for view events.");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while ingesting view event from Kafka.");
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
