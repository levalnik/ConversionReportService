using ConversionReportService.Application.Models.Exceptions;

namespace ConversionReportService.Application.Models.Results;

public class ReportResult
{
    public long RequestId { get; private set; }

    public double ConversionRatio { get; private set; }

    public int PaymentsCount { get; private set; }

    public DateTime GeneratedAt { get; private set; }

    private ReportResult() { }

    public ReportResult(
        long requestId,
        int views,
        int payments)
    {
        if (views < 0) 
            throw new DomainException("Views cannot be negative");

        if (payments < 0)
            throw new DomainException("Payments cannot be negative");

        RequestId = requestId;

        PaymentsCount = payments;

        ConversionRatio = views == 0 ? 0 : (double)payments / views;

        GeneratedAt = DateTime.UtcNow;
    }
}