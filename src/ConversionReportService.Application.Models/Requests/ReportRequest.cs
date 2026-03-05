using ConversionReportService.Application.Models.Exceptions;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.Models.ValueObjects;

namespace ConversionReportService.Application.Models.Requests;

public class ReportRequest
{
    public long Id { get; private set; }

    public long ProductId { get; private set; }

    public long CheckoutId { get; private set; }

    public ReportPeriod Period { get; private set; }

    public ReportStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private ReportRequest() { }

    public ReportRequest(
        long id,
        long productId,
        long checkoutId,
        ReportPeriod period)
    {
        Id = id;
        ProductId = productId;
        CheckoutId = checkoutId;
        Period = period;
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status != ReportStatus.Pending)
            throw new DomainException("Report cannot start processing");

        Status = ReportStatus.Processing;
    }

    public void Complete()
    {
        if (Status != ReportStatus.Processing)
            throw new DomainException("Report cannot be completed");

        Status = ReportStatus.Completed;
    }

    public void Fail()
    {
        Status = ReportStatus.Failed;
    }
}