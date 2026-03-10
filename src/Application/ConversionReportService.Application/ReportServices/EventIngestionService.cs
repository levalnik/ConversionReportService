using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Events;

namespace ConversionReportService.Application.ReportServices;

public sealed class EventIngestionService : IEventIngestionService
{
    private readonly IEventRepository _repository;

    public EventIngestionService(IEventRepository repository)
    {
        _repository = repository;
    }

    public Task AddViewEventAsync(
        ViewEvent viewEvent,
        CancellationToken cancellationToken)
    {
        return _repository.AddViewEventAsync(
            viewEvent.ProductId,
            viewEvent.CheckoutId,
            viewEvent.OccurredAt,
            cancellationToken);
    }

    public Task AddPaymentEventAsync(
        PaymentEvent paymentEvent,
        CancellationToken cancellationToken)
    {
        return _repository.AddPaymentEventAsync(
            paymentEvent.ProductId,
            paymentEvent.CheckoutId,
            paymentEvent.Status,
            paymentEvent.OccurredAt,
            cancellationToken);
    }
}
