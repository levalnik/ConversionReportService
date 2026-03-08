using ConversionReportService.Application.Models.Events;

namespace ConversionReportService.Application.Contracts.ReportServices;

public interface IReportRequestIngestionService
{
    Task<long> IngestAsync(ReportRequestedEvent evt, CancellationToken cancellationToken);
}
