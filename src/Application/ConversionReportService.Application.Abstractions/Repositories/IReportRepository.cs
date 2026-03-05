using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Results;
using ConversionReportService.Application.Models.Statuses;

namespace ConversionReportService.Application.Abstractions.Repositories;

public interface IReportRepository
{
    Task SaveRequestAsync(ReportRequest request, CancellationToken cancellationToken);

    Task<ReportRequest?> GetRequestAsync(long id, CancellationToken cancellationToken);

    Task SaveResultAsync(ReportResult result, CancellationToken cancellationToken);

    Task<ReportResult?> GetResultAsync(long requestId, CancellationToken cancellationToken);

    Task UpdateStatusAsync(long requestId, ReportStatus status, CancellationToken cancellationToken);
}