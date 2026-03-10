namespace ConversionReportService.Application.Contracts.ReportServices;

public interface IEventIngestionService
{
    Task AddViewEventAsync(
        ConversionReportService.Application.Models.Events.ViewEvent viewEvent,
        CancellationToken cancellationToken);

    Task AddPaymentEventAsync(
        ConversionReportService.Application.Models.Events.PaymentEvent paymentEvent,
        CancellationToken cancellationToken);
}
