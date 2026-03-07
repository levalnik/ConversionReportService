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

    private ReportRequest()
    {
        Period = null!;
    }

    private ReportRequest(
        long id,
        long productId,
        long checkoutId,
        ReportPeriod period,
        ReportStatus status,
        DateTime createdAt)
    {
        Id = id;
        ProductId = productId;
        CheckoutId = checkoutId;
        Period = period;
        Status = status;
        CreatedAt = createdAt;
    }

    public ReportRequest(
        long productId,
        long checkoutId,
        ReportPeriod period)
    {
        ProductId = productId;
        CheckoutId = checkoutId;
        Period = period;
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetId(long id)
    {
        Id = id;
    }

    public static ReportRequest FromDatabase(
        long id,
        long productId,
        long checkoutId,
        ReportPeriod period,
        ReportStatus status,
        DateTime createdAt)
    {
        return new ReportRequest(
            id,
            productId,
            checkoutId,
            period,
            status,
            createdAt);
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
