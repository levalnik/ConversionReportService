using ConversionReportService.Presentation.Grpc.Interceptors;
using ConversionReportService.Presentation.Grpc.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionReportService.Presentation.Grpc.Extensions;

public static class PresentationServiceExtension
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddScoped<GrpcExceptionInterceptor>();

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        return services;
    }

    public static IEndpointRouteBuilder MapGrpcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<ReportServiceGrpc>();

        return endpoints;
    }
}