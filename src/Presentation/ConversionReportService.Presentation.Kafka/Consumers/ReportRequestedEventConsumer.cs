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

public sealed class ReportRequestedEventConsumer : BackgroundService
{
    private readonly IConsumer<long, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<ReportRequestedEventConsumer> _logger;

    public ReportRequestedEventConsumer(
        IKafkaConsumerFactory consumerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<ReportRequestedEventConsumer> logger)
    {
        _consumer = consumerFactory.Create();
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
                var batchSize = Math.Max(1, _options.BatchSize);
                var results = new List<ConsumeResult<long, byte[]>>(batchSize);

                var first = _consumer.Consume(TimeSpan.FromMilliseconds(_options.PollTimeoutMs));
                if (first?.Message?.Value is null)
                    continue;

                results.Add(first);

                for (var i = 1; i < batchSize; i++)
                {
                    var next = _consumer.Consume(TimeSpan.Zero);
                    if (next?.Message?.Value is null)
                        break;

                    results.Add(next);
                }

                using var scope = _scopeFactory.CreateScope();
                var ingestionService =
                    scope.ServiceProvider.GetRequiredService<IReportRequestIngestionService>();
                var processingService =
                    scope.ServiceProvider.GetRequiredService<IReportProcessingService>();

                foreach (var result in results)
                {
                    try
                    {
                        var envelope = ReportRequestedEnvelope.Parser.ParseFrom(result.Message.Value);
                        var events = new List<ReportRequestedEvent>();

                        switch (envelope.PayloadCase)
                        {
                            case ReportRequestedEnvelope.PayloadOneofCase.Single:
                                events.Add(ToEvent(envelope.Single));
                                break;
                            case ReportRequestedEnvelope.PayloadOneofCase.Batch:
                                foreach (var item in envelope.Batch.Requests)
                                    events.Add(ToEvent(item));
                                break;
                            case ReportRequestedEnvelope.PayloadOneofCase.None:
                                _logger.LogWarning("Received Kafka message with empty payload.");
                                _consumer.Commit(result);
                                continue;
                        }

                        foreach (var evt in events)
                        {
                            var createdId = await ingestionService.IngestAsync(evt, stoppingToken);
                            await processingService.ProcessAsync(createdId, stoppingToken);

                            _logger.LogInformation(
                                "Ingested and processed report request from bus. CreatedRequestId={CreatedRequestId}",
                                createdId);
                        }

                        _consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while ingesting report request from Kafka.");
                    }
                }
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

    private static ReportRequestedEvent ToEvent(ReportRequestedValue message)
    {
        return new ReportRequestedEvent(
            RequestId: message.RequestId,
            ProductId: message.ProductId,
            CheckoutId: message.CheckoutId,
            From: message.From.ToDateTime(),
            To: message.To.ToDateTime(),
            CreatedAt: message.CreatedAt.ToDateTime());
    }
}
