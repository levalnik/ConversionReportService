namespace ConversionReportService.Application.Models.Events;

public sealed class ViewEvent
{
    public long ProductId { get; }

    public long CheckoutId { get; }

    public DateTime OccurredAt { get; }

    public ViewEvent(long productId, long checkoutId, DateTime occurredAt)
    {
        ProductId = productId;
        CheckoutId = checkoutId;
        OccurredAt = occurredAt;
    }
}
