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

public sealed class ReportRequestedConsumer : BackgroundService
{
    private readonly IConsumer<long, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<ReportRequestedConsumer> _logger;

    public ReportRequestedConsumer(
        IConsumer<long, byte[]> consumer,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<ReportRequestedConsumer> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ReportRequestedTopic))
            throw new InvalidOperationException("Kafka ReportRequestedTopic is not configured.");

        _consumer.Subscribe(_options.ReportRequestedTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromMilliseconds(_options.PollTimeoutMs));
                if (result?.Message?.Value is null)
                    continue;

                var message = ReportRequestedValue.Parser.ParseFrom(result.Message.Value);
                var evt = new ReportRequestedEvent(
                    RequestId: message.RequestId,
                    ProductId: message.ProductId,
                    CheckoutId: message.CheckoutId,
                    From: message.From.ToDateTime(),
                    To: message.To.ToDateTime(),
                    CreatedAt: message.CreatedAt.ToDateTime());

                using var scope = _scopeFactory.CreateScope();
                var ingestionService =
                    scope.ServiceProvider.GetRequiredService<IReportRequestIngestionService>();
                var processingService =
                    scope.ServiceProvider.GetRequiredService<IReportProcessingService>();

                var createdId = await ingestionService.IngestAsync(evt, stoppingToken);
                await processingService.ProcessAsync(createdId, stoppingToken);
                _consumer.Commit(result);

                _logger.LogInformation(
                    "Ingested and processed report request from bus. IncomingRequestId={IncomingRequestId}, CreatedRequestId={CreatedRequestId}",
                    evt.RequestId,
                    createdId);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error.");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while ingesting report request from Kafka.");
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
