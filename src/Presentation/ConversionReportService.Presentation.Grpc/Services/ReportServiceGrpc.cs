using Conversionreportservice;
using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.Models.Dtos;
using Grpc.Core;

namespace ConversionReportService.Presentation.Grpc.Services;

public class ReportServiceGrpc : ReportGrpcService.ReportGrpcServiceBase
{
    private readonly IReportService _reportService;

    public ReportServiceGrpc(IReportService reportService)
    {
        _reportService = reportService;
    }

    public override async Task<CreateReportResponse> CreateReport(CreateReportRequest request, ServerCallContext context)
    {
        try
        {
            var dto = new CreateReportRequestDto
            {
                ProductId = request.ProductId,
                CheckoutId = request.CheckoutId,
                From = request.From.ToDateTime(),
                To = request.To.ToDateTime()
            };

            long requestId = await _reportService.CreateReportAsync(dto, context.CancellationToken);

            return new CreateReportResponse { RequestId = requestId };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<GetReportResponse> GetReport(GetReportRequest request, ServerCallContext context)
    {
        try
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
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }
}