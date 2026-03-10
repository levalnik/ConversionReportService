namespace ConversionReportService.Application.Abstractions.Repositories;

public interface IEventRepository
{
    Task AddViewEventAsync(
        long productId,
        long checkoutId,
        DateTime occurredAt,
        CancellationToken cancellationToken);

    Task AddPaymentEventAsync(
        long productId,
        long checkoutId,
        string status,
        DateTime occurredAt,
        CancellationToken cancellationToken);
}
