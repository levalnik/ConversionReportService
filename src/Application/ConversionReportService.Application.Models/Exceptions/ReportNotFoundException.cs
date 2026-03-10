namespace ConversionReportService.Application.Models.Exceptions;

public sealed class ReportNotFoundException : KeyNotFoundException
{
    public ReportNotFoundException(long requestId) : base($"Report request {requestId} not found.") { }
}
