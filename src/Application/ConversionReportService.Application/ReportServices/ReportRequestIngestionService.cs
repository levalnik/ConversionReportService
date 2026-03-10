using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Events;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.ValueObjects;
using Npgsql;

namespace ConversionReportService.Application.ReportServices;

public sealed class ReportRequestIngestionService : IReportRequestIngestionService
{
    private readonly IReportRepository _repository;
    private readonly NpgsqlDataSource _dataSource;

    public ReportRequestIngestionService(
        IReportRepository repository,
        NpgsqlDataSource dataSource)
    {
        _repository = repository;
        _dataSource = dataSource;
    }

    public async Task<long> IngestAsync(ReportRequestedEvent evt, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction tran = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            var request = new ReportRequest(
                evt.ProductId,
                evt.CheckoutId,
                new ReportPeriod(evt.From, evt.To));
            request.SetId(evt.RequestId);

            await _repository.CreateRequestAsync(evt.RequestId, request, conn, tran, cancellationToken);
            await tran.CommitAsync(cancellationToken);

            return request.Id;
        }
        catch
        {
            await tran.RollbackAsync(cancellationToken);
            throw;
        }
    }
}