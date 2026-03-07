using ConversionReportService.Application.Models.Events;
using ConversionReportService.Application.Models.Results;

namespace ConversionReportService.Application.Abstractions.Messaging;

public interface IReportRequestPublisher
{
    Task PublishReportAsync(ReportRequestedEvent requestId, CancellationToken cancellationToken);
}