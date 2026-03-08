using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Results;
using ConversionReportService.Application.Models.Statuses;
using Npgsql;

namespace ConversionReportService.Application.ReportServices;

public sealed class ReportProcessingService : IReportProcessingService
{
    private readonly IReportRepository _repository;
    private readonly NpgsqlDataSource _dataSource;

    public ReportProcessingService(
        IReportRepository repository,
        NpgsqlDataSource dataSource)
    {
        _repository = repository;
        _dataSource = dataSource;
    }

    public async Task ProcessAsync(long requestId, CancellationToken cancellationToken)
    {
        var request = await _repository.GetRequestAsync(requestId, cancellationToken);
        if (request == null)
            throw new KeyNotFoundException($"Report request {requestId} not found.");

        var (views, payments) = await _repository.GetMetricsAsync(
            request.ProductId,
            request.CheckoutId,
            request.Period.From,
            request.Period.To,
            cancellationToken);

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var tran = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            await _repository.UpdateStatusAsync(
                requestId,
                ReportStatus.Processing,
                conn,
                tran,
                cancellationToken);

            var result = new ReportResult(requestId, views, payments);

            await _repository.SaveResultAsync(result, conn, tran, cancellationToken);

            await _repository.UpdateStatusAsync(
                requestId,
                ReportStatus.Completed,
                conn,
                tran,
                cancellationToken);

            await tran.CommitAsync(cancellationToken);
        }
        catch
        {
            await tran.RollbackAsync(cancellationToken);

            await using var failConn = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var failTran = await failConn.BeginTransactionAsync(cancellationToken);

            await _repository.UpdateStatusAsync(
                requestId,
                ReportStatus.Failed,
                failConn,
                failTran,
                cancellationToken);

            await failTran.CommitAsync(cancellationToken);
            throw;
        }
    }
}
