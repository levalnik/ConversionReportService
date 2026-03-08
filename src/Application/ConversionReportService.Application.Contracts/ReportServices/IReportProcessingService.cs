namespace ConversionReportService.Application.Contracts.ReportServices;

public interface IReportProcessingService
{
    Task ProcessAsync(long requestId, CancellationToken cancellationToken);
}
