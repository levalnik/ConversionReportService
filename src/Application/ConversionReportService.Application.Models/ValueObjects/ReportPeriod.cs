using ConversionReportService.Application.Models.Exceptions;

namespace ConversionReportService.Application.Models.ValueObjects;

public class ReportPeriod
{
    public DateTime From { get; }
    public DateTime To { get; }

    public ReportPeriod(DateTime from, DateTime to)
    {
        if (from >= to)
            throw new DomainException("Report period is invalid");

        From = from;
        To = to;
    }
}