using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Application.Abstractions.Messaging;
using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Dtos;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.ValueObjects;
using Npgsql;

namespace ConversionReportService.Application.ReportServices;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IReportRequestPublisher _publisher;
    private readonly IReportCache _cache;
    private readonly NpgsqlDataSource _dataSource;

    public ReportService(
        IReportRepository repository,
        IReportRequestPublisher publisher,
        IReportCache cache,
        NpgsqlDataSource dataSource)
    {
        _repository = repository;
        _publisher = publisher;
        _cache = cache;
        _dataSource = dataSource;
    }

    public async Task<long> CreateReportAsync(CreateReportRequestDto dto, CancellationToken ct)
    {
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(ct);
        await using NpgsqlTransaction tran = await conn.BeginTransactionAsync(ct);

        try
        {
            var request = new ReportRequest(
                dto.ProductId,
                dto.CheckoutId,
                new ReportPeriod(dto.From, dto.To)
            );

            long id = await _repository.CreateRequestAsync(request, conn, tran, ct);

            request.SetId(id);

            await tran.CommitAsync(ct);

            await _publisher.PublishReportAsync(request.Id, ct);

            return request.Id;
        }
        catch
        {
            await tran.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ReportResponseDto> GetReportAsync(long requestId, CancellationToken ct)
    {
        var cached = await _cache.GetAsync<ReportResponseDto>(requestId, ct);

        if (cached != null)
            return cached;

        var request = await _repository.GetRequestAsync(requestId, ct);

        if (request == null)
            throw new Exception("Report not found");

        var result = await _repository.GetResultAsync(requestId, ct);

        var response = new ReportResponseDto
        {
            RequestId = requestId,
            Status = request.Status.ToString(),
            ConversionRatio = result?.ConversionRatio,
            PaymentsCount = result?.PaymentsCount
        };

        await _cache.SetAsync(
            requestId,
            response,
            TimeSpan.FromMinutes(5),
            ct);

        return response;
    }
}