using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Results;
using ConversionReportService.Application.Models.Statuses;
using Npgsql;

namespace ConversionReportService.Application.Abstractions.Repositories;

public interface IReportRepository
{
    Task<long> CreateRequestAsync(
        ReportRequest request,
        NpgsqlConnection conn,
        NpgsqlTransaction tran,
        CancellationToken cancellationToken);

    Task SaveResultAsync(
        ReportResult result,
        NpgsqlConnection conn,
        NpgsqlTransaction tran,
        CancellationToken cancellationToken);

    Task UpdateStatusAsync(
        long requestId,
        ReportStatus status,
        NpgsqlConnection conn,
        NpgsqlTransaction tran,
        CancellationToken cancellationToken);

    Task<ReportRequest?> GetRequestAsync(long id, CancellationToken cancellationToken);

    Task<ReportResult?> GetResultAsync(long requestId, CancellationToken cancellationToken);
}