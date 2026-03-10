namespace ConversionReportService.Application.Models.Events;

public sealed class PaymentEvent
{
    public long ProductId { get; }

    public long CheckoutId { get; }

    public string Status { get; }

    public DateTime OccurredAt { get; }

    public PaymentEvent(long productId, long checkoutId, string status, DateTime occurredAt)
    {
        ProductId = productId;
        CheckoutId = checkoutId;
        Status = status;
        OccurredAt = occurredAt;
    }
}
