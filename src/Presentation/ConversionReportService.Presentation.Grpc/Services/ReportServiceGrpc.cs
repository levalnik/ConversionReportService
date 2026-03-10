using Conversionreportservice;
using ConversionReportService.Application.Contracts.ReportServices;
using Grpc.Core;

namespace ConversionReportService.Presentation.Grpc.Services;

public class ReportServiceGrpc : ReportGrpcService.ReportGrpcServiceBase
{
    private readonly IReportService _reportService;

    public ReportServiceGrpc(IReportService reportService)
    {
        _reportService = reportService;
    }

    public override async Task<GetReportResponse> GetReport(GetReportRequest request, ServerCallContext context)
    {
        var report = await _reportService.GetReportAsync(request.RequestId, context.CancellationToken);

        return new GetReportResponse
        {
            RequestId = report.RequestId,
            Status = report.Status,
            ConversionRatio = report.ConversionRatio ?? 0,
            PaymentsCount = report.PaymentsCount ?? 0
        };
    }
}