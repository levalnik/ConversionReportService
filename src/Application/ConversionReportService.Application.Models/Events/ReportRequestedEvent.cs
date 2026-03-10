namespace ConversionReportService.Application.Models.Events;

public sealed record ReportRequestedEvent
(
    long RequestId,

    long ProductId,

    long CheckoutId,

    DateTime From,

    DateTime To,

    DateTime CreatedAt);