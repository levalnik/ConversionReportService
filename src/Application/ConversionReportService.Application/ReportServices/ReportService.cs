using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Dtos;
using ConversionReportService.Application.Models.Exceptions;
using ConversionReportService.Application.Models.Statuses;

namespace ConversionReportService.Application.ReportServices;

public class ReportService : IReportService
{
    private static readonly TimeSpan TerminalCacheTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan InFlightCacheTtl = TimeSpan.FromSeconds(15);

    private readonly IReportRepository _repository;
    private readonly IReportCache _cache;

    public ReportService(
        IReportRepository repository,
        IReportCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ReportResponseDto> GetReportAsync(long requestId, CancellationToken ct)
    {
        var cached = await _cache.GetAsync<ReportResponseDto>(requestId, ct);

        if (cached != null)
            return cached;

        var request = await _repository.GetRequestAsync(requestId, ct);

        if (request == null)
            throw new ReportNotFoundException(requestId);

        var result = await _repository.GetResultAsync(requestId, ct);

        var response = new ReportResponseDto
        {
            RequestId = requestId,
            Status = request.Status.ToString(),
            ConversionRatio = result?.ConversionRatio,
            PaymentsCount = result?.PaymentsCount
        };

        var ttl = request.Status is ReportStatus.Completed or ReportStatus.Failed ? TerminalCacheTtl : InFlightCacheTtl;

        await _cache.SetAsync(requestId, response, ttl, ct);

        return response;
    }
}
