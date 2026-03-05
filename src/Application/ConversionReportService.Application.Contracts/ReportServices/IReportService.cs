using ConversionReportService.Application.Models.Dtos;

namespace ConversionReportService.Application.Contracts.ReportServices;

public interface IReportService
{
    Task<long> CreateReportAsync(CreateReportRequestDto dto, CancellationToken cancellationToken);

    Task<ReportResponseDto> GetReportAsync(long requestId, CancellationToken cancellationToken);
}