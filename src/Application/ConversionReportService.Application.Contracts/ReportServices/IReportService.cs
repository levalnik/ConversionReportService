using ConversionReportService.Application.Models.Dtos;

namespace ConversionReportService.Application.Contracts.ReportServices;

public interface IReportService
{
    Task<ReportResponseDto> GetReportAsync(long requestId, CancellationToken cancellationToken);
}
