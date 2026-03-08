using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Dtos;
using ConversionReportService.Application.Models.Statuses;

namespace ConversionReportService.Application.ReportServices;

public class ReportService : IReportService
{
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
            throw new KeyNotFoundException("Report not found");

        var result = await _repository.GetResultAsync(requestId, ct);

        var response = new ReportResponseDto
        {
            RequestId = requestId,
            Status = request.Status.ToString(),
            ConversionRatio = result?.ConversionRatio,
            PaymentsCount = result?.PaymentsCount
        };

        if (request.Status is ReportStatus.Completed or ReportStatus.Failed)
        {
            await _cache.SetAsync(
                requestId,
                response,
                TimeSpan.FromMinutes(5),
                ct);
        }

        return response;
    }
}
