using ConversionReportService.Application.Models.Results;

namespace ConversionReportService.Application.Abstractions.Messaging;

public interface IReportRequestPublisher
{
    Task PublishReportAsync(long requestId, CancellationToken cancellationToken);
}